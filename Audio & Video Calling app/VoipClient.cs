using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;

public class VoipClient : IDisposable
{
    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;
    private AdaptiveJitterBuffer jitterBuffer;
    private CancellationTokenSource cancellationToken;

    private WaveInEvent waveIn;
    private WaveOutEvent waveOut;
    private BufferedWaveProvider waveProvider;
    private NetworkSimulator networkSimulator;
    private CallRecorder callRecorder;
    private EchoCanceller echoCanceller;
    private NoiseSuppressor noiseSuppressor;

    private uint sequenceNumber = 0;
    private uint timestamp = 0;

    private byte[] lastSpeakerOutput = new byte[0];
    private object speakerLock = new object();

    public event Action<JitterBufferStats> OnStatsUpdated;
    public event Action<string> OnLog;

    private bool isRunning = false;
    private WaveFormat audioFormat = new WaveFormat(8000, 16, 1);

    public bool IsJitterBufferEnabled
    {
        get => jitterBuffer?.IsEnabled ?? false;
        set { if (jitterBuffer != null) jitterBuffer.IsEnabled = value; }
    }

    public bool IsEchoCancellerEnabled
    {
        get => echoCanceller?.IsEnabled ?? false;
        set { if (echoCanceller != null) echoCanceller.IsEnabled = value; }
    }

    public bool IsNoiseSuppressorEnabled 
    {
        get => noiseSuppressor?.IsEnabled ?? false;
        set { if (noiseSuppressor != null) noiseSuppressor.IsEnabled = value; }
    }

    public bool IsRecording => callRecorder?.IsRecording ?? false;

    public NetworkSimulator NetworkSim => networkSimulator;
    public CallRecorder Recorder => callRecorder;
    public EchoCanceller EchoCanceller => echoCanceller;
    public NoiseSuppressor NoiseSuppressor => noiseSuppressor;

    public VoipClient(int localPort = 0)
    {
        udpClient = new UdpClient(localPort);
        jitterBuffer = new AdaptiveJitterBuffer(minSize: 3, maxSize: 15);
        networkSimulator = new NetworkSimulator();
        callRecorder = new CallRecorder(audioFormat);
        echoCanceller = new EchoCanceller(filterLength: 512, stepSize: 0.3);
        noiseSuppressor = new NoiseSuppressor();
        cancellationToken = new CancellationTokenSource();
    }

    public void StartCall(string remoteIp, int remotePort)
    {
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIp), remotePort);
        isRunning = true;

        StartAudioCapture();
        StartAudioPlayback();
        Task.Run(() => ReceivePackets());
        Task.Run(() => MonitorStats());

        OnLog?.Invoke($"📞 Call started to {remoteIp}:{remotePort}");
    }

    public void StartRecording()
    {
        callRecorder.StartRecording();
        OnLog?.Invoke($"🔴 Recording started: {callRecorder.CurrentSessionId}");
    }

    public string StopRecording()
    {
        string filePath = callRecorder.StopRecording();
        OnLog?.Invoke($"⏹ Recording stopped: {filePath}");
        return filePath;
    }

    private void StartAudioCapture()
    {
        waveIn = new WaveInEvent
        {
            WaveFormat = audioFormat,
            BufferMilliseconds = 20
        };

        waveIn.DataAvailable += (s, e) =>
        {
            if (!isRunning || remoteEndPoint == null) return;

            byte[] audioToSend = new byte[e.BytesRecorded];
            Array.Copy(e.Buffer, audioToSend, e.BytesRecorded);

            if (noiseSuppressor.IsEnabled)
            {
                audioToSend = noiseSuppressor.ProcessAudio(audioToSend);
            }

            if (echoCanceller.IsEnabled)
            {
                lock (speakerLock)
                {
                    if (lastSpeakerOutput.Length > 0)
                    {
                        audioToSend = echoCanceller.ProcessAudio(audioToSend, lastSpeakerOutput);
                    }
                }
            }

            if (callRecorder.IsRecording)
            {
                callRecorder.WriteLocalAudio(audioToSend, 0, audioToSend.Length);
            }

            SendAudioPacket(audioToSend, audioToSend.Length);
        };

        waveIn.StartRecording();
    }

    private void StartAudioPlayback()
    {
        waveProvider = new BufferedWaveProvider(audioFormat)
        {
            BufferDuration = TimeSpan.FromSeconds(2),
            DiscardOnBufferOverflow = true
        };

        waveOut = new WaveOutEvent();
        waveOut.Init(waveProvider);
        waveOut.Play();
    }

    private void SendAudioPacket(byte[] audioData, int length)
    {
        try
        {
            var packet = new RtpPacket
            {
                SequenceNumber = sequenceNumber++,
                Timestamp = timestamp,
                Payload = new byte[length],
                Type = PacketType.Audio
            };

            Array.Copy(audioData, packet.Payload, length);
            timestamp += 160;

            byte[] data = SerializePacket(packet);

            networkSimulator.ProcessPacket(data, (simulatedData) =>
            {
                try
                {
                    udpClient.Send(simulatedData, simulatedData.Length, remoteEndPoint);
                }
                catch { }
            });
        }
        catch (Exception ex)
        {
            OnLog?.Invoke($"❌ Send error: {ex.Message}");
        }
    }

    private async Task ReceivePackets()
    {
        var playbackTimer = new System.Timers.Timer(20);
        playbackTimer.Elapsed += (s, e) => PlayNextPacket();
        playbackTimer.Start();

        while (isRunning)
        {
            try
            {
                var result = await udpClient.ReceiveAsync();
                var packet = DeserializePacket(result.Buffer);

                if (packet != null)
                {
                    jitterBuffer.AddPacket(packet);
                }
            }
            catch (Exception ex)
            {
                if (isRunning)
                    OnLog?.Invoke($"❌ Receive error: {ex.Message}");
            }
        }

        playbackTimer.Stop();
    }

    private void PlayNextPacket()
    {
        if (!isRunning) return;

        RtpPacket packet = null;

        if (!jitterBuffer.IsEnabled)
        {
            packet = jitterBuffer.GetNextPacket();
        }
        else
        {
            packet = jitterBuffer.GetNextPacket();
        }

        if (packet != null && packet.Payload != null)
        {
            lock (speakerLock)
            {
                lastSpeakerOutput = new byte[packet.Payload.Length];
                Array.Copy(packet.Payload, lastSpeakerOutput, packet.Payload.Length);
            }

            if (callRecorder.IsRecording)
            {
                callRecorder.WriteRemoteAudio(packet.Payload, 0, packet.Payload.Length);
            }

            waveProvider?.AddSamples(packet.Payload, 0, packet.Payload.Length);
        }
    }

    private async Task MonitorStats()
    {
        while (isRunning)
        {
            await Task.Delay(1000);

            jitterBuffer.CheckPacketLoss();
            OnStatsUpdated?.Invoke(jitterBuffer.Stats);
        }
    }

    private byte[] SerializePacket(RtpPacket packet)
    {
        byte[] data = new byte[12 + packet.Payload.Length];

        data[0] = 0x80;
        data[1] = (byte)packet.Type;
        data[2] = (byte)(packet.SequenceNumber >> 8);
        data[3] = (byte)(packet.SequenceNumber & 0xFF);
        data[4] = (byte)(packet.Timestamp >> 24);
        data[5] = (byte)(packet.Timestamp >> 16);
        data[6] = (byte)(packet.Timestamp >> 8);
        data[7] = (byte)(packet.Timestamp & 0xFF);

        Array.Copy(packet.Payload, 0, data, 12, packet.Payload.Length);

        return data;
    }

    private RtpPacket DeserializePacket(byte[] data)
    {
        if (data.Length < 12) return null;

        var packet = new RtpPacket
        {
            SequenceNumber = (uint)((data[2] << 8) | data[3]),
            Timestamp = (uint)((data[4] << 24) | (data[5] << 16) | (data[6] << 8) | data[7]),
            Type = (PacketType)data[1],
            Payload = new byte[data.Length - 12],
            ArrivalTime = DateTime.Now
        };

        Array.Copy(data, 12, packet.Payload, 0, packet.Payload.Length);

        return packet;
    }

    public void StopCall()
    {
        isRunning = false;

        waveIn?.StopRecording();
        waveIn?.Dispose();
        waveOut?.Stop();
        waveOut?.Dispose();

        if (callRecorder.IsRecording)
            StopRecording();

        OnLog?.Invoke("📴 Call stopped");
    }

    public void Dispose()
    {
        StopCall();
        udpClient?.Close();
        callRecorder?.Dispose();
        cancellationToken?.Cancel();
    }
}