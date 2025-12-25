using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class AdaptiveJitterBuffer
{
    private SortedDictionary<uint, RtpPacket> buffer;
    private uint expectedSeq;
    private int minBufferSize;
    private int maxBufferSize;
    private int currentBufferSize;

    private Queue<double> jitterHistory;
    private const int JITTER_HISTORY_SIZE = 30;
    private RtpPacket lastReceivedPacket = null;

    private Queue<RtpPacket> recentPackets = new Queue<RtpPacket>(5);
    private bool enablePLC = true;

    private int latePackets = 0;
    private int concealedPackets = 0;
    private Queue<long> arrivalTimeDeltas = new Queue<long>(20);

    public JitterBufferStats Stats { get; private set; }
    public bool IsEnabled { get; set; }
    public int CurrentBufferCount => buffer.Count;
    public int TargetBufferSize => currentBufferSize;
    public bool EnablePacketLossConcealment
    {
        get => enablePLC;
        set => enablePLC = value;
    }

    public AdaptiveJitterBuffer(int minSize = 3, int maxSize = 15)
    {
        buffer = new SortedDictionary<uint, RtpPacket>();
        minBufferSize = minSize;
        maxBufferSize = maxSize;
        currentBufferSize = minSize;
        jitterHistory = new Queue<double>(JITTER_HISTORY_SIZE);
        expectedSeq = 0;
        Stats = new JitterBufferStats();
        IsEnabled = true;
    }

    public void AddPacket(RtpPacket packet)
    {
        Stats.PacketsReceived++;

        if (!IsEnabled)
        {
            lastReceivedPacket = packet;
            Stats.PacketsPlayed++;
            return;
        }

        lock (buffer)
        {
            if (buffer.ContainsKey(packet.SequenceNumber))
            {
                Stats.DuplicatePackets++;
                return;
            }

            if (expectedSeq > 0 && packet.SequenceNumber < expectedSeq)
            {
                latePackets++;
                return;
            }

            buffer[packet.SequenceNumber] = packet;

            if (recentPackets.Count >= 5)
                recentPackets.Dequeue();
            recentPackets.Enqueue(packet);

            CalculateJitter(packet);
            AdaptBufferSize();
        }
    }

    public RtpPacket GetNextPacket()
    {
        if (!IsEnabled)
        {
            RtpPacket packet = lastReceivedPacket;
            lastReceivedPacket = null;
            return packet;
        }

        lock (buffer)
        {
            if (buffer.Count < currentBufferSize)
            {
                if (enablePLC && expectedSeq > 0 &&
                    buffer.Count > 0 &&
                    !buffer.ContainsKey(expectedSeq))
                {
                    CheckForMissingPackets();
                }
                return null;
            }

            if (buffer.Count > 0)
            {
                var first = buffer.First();

                if (expectedSeq > 0 && first.Key > expectedSeq)
                {
                    int gap = (int)(first.Key - expectedSeq);
                    Stats.PacketsLost += gap;

                    if (enablePLC && recentPackets.Count > 0)
                    {
                        var concealedPacket = GenerateConcealmentPacket(expectedSeq);
                        expectedSeq++;
                        concealedPackets++;
                        return concealedPacket;
                    }
                }

                buffer.Remove(first.Key);
                expectedSeq = first.Key + 1;
                Stats.PacketsPlayed++;

                return first.Value;
            }
        }

        return null;
    }

    private RtpPacket GenerateConcealmentPacket(uint sequenceNumber)
    {
        var lastPacket = recentPackets.Last();
        var concealedPayload = new byte[lastPacket.Payload.Length];

        for (int i = 0; i < lastPacket.Payload.Length; i += 2)
        {
            short sample = BitConverter.ToInt16(lastPacket.Payload, i);
            sample = (short)(sample * 0.7);
            byte[] bytes = BitConverter.GetBytes(sample);
            concealedPayload[i] = bytes[0];
            concealedPayload[i + 1] = bytes[1];
        }

        return new RtpPacket
        {
            SequenceNumber = sequenceNumber,
            Timestamp = lastPacket.Timestamp + 160,
            Payload = concealedPayload,
            ArrivalTime = DateTime.Now,
            Type = lastPacket.Type
        };
    }

    private void CheckForMissingPackets()
    {
        if (buffer.Count == 0 || expectedSeq == 0)
            return;

        uint firstSeq = buffer.First().Key;
        if (firstSeq > expectedSeq)
        {
            int gap = (int)(firstSeq - expectedSeq);
            Stats.PacketsLost += gap;
        }
    }

    private void CalculateJitter(RtpPacket packet)
    {
        if (packet.ArrivalTime.Ticks > 0 && Stats.LastArrivalTime.Ticks > 0)
        {
            double interArrivalTime = (packet.ArrivalTime - Stats.LastArrivalTime).TotalMilliseconds;
            double expectedInterval = 20.0;

            double jitter = Math.Abs(interArrivalTime - expectedInterval);

            jitterHistory.Enqueue(jitter);
            if (jitterHistory.Count > JITTER_HISTORY_SIZE)
                jitterHistory.Dequeue();

            Stats.AverageJitter = jitterHistory.Average();
            Stats.CurrentJitter = jitter;

            arrivalTimeDeltas.Enqueue((long)interArrivalTime);
            if (arrivalTimeDeltas.Count > 20)
                arrivalTimeDeltas.Dequeue();
        }

        Stats.LastArrivalTime = packet.ArrivalTime;
    }

    private void AdaptBufferSize()
    {
        if (jitterHistory.Count < 5)
            return;

        double avgJitter = Stats.AverageJitter;
        double jitterVariance = CalculateJitterVariance();

        if (avgJitter > 15.0 && currentBufferSize < maxBufferSize)
        {
            currentBufferSize += 2;
            currentBufferSize = Math.Min(currentBufferSize, maxBufferSize);
            Stats.BufferAdjustments++;
        }
        else if (avgJitter > 8.0 && currentBufferSize < maxBufferSize)
        {
            currentBufferSize++;
            Stats.BufferAdjustments++;
        }
        else if (avgJitter < 2.0 && jitterVariance < 1.0 && currentBufferSize > minBufferSize)
        {
            currentBufferSize--;
            Stats.BufferAdjustments++;
        }

        if (buffer.Count > currentBufferSize * 1.5 && currentBufferSize < maxBufferSize)
        {
            currentBufferSize++;
            Stats.BufferAdjustments++;
        }
    }

    private double CalculateJitterVariance()
    {
        if (jitterHistory.Count < 2)
            return 0;

        double mean = jitterHistory.Average();
        double variance = jitterHistory.Sum(x => Math.Pow(x - mean, 2)) / jitterHistory.Count;
        return variance;
    }

    public void CheckPacketLoss()
    {
        lock (buffer)
        {
            CheckForMissingPackets();
        }
    }

    public void Reset()
    {
        lock (buffer)
        {
            buffer.Clear();
            recentPackets.Clear();
            expectedSeq = 0;
            currentBufferSize = minBufferSize;
            jitterHistory.Clear();
            arrivalTimeDeltas.Clear();
            latePackets = 0;
            concealedPackets = 0;
        }
    }

    public string GetDetailedStats()
    {
        double lossRate = Stats.PacketsReceived > 0
            ? (Stats.PacketsLost * 100.0 / Stats.PacketsReceived)
            : 0;

        double jitterStdDev = jitterHistory.Count > 1
            ? Math.Sqrt(CalculateJitterVariance())
            : 0;

        return $"📊 BUFFER STATS\n" +
               $"Buffer: {buffer.Count}/{currentBufferSize} (min:{minBufferSize}, max:{maxBufferSize})\n" +
               $"Jitter: {Stats.AverageJitter:F1}ms ±{jitterStdDev:F1}ms\n" +
               $"Loss: {lossRate:F2}% ({Stats.PacketsLost}/{Stats.PacketsReceived})\n" +
               $"Played: {Stats.PacketsPlayed} | Duplicates: {Stats.DuplicatePackets}\n" +
               $"Late: {latePackets} | Concealed: {concealedPackets}\n" +
               $"Adjustments: {Stats.BufferAdjustments}";
    }
}

public class RtpPacket
{
    public uint SequenceNumber { get; set; }
    public uint Timestamp { get; set; }
    public byte[] Payload { get; set; }
    public DateTime ArrivalTime { get; set; }
    public PacketType Type { get; set; }

    public RtpPacket()
    {
        ArrivalTime = DateTime.Now;
    }
}

public enum PacketType
{
    Audio,
    Video
}

public class JitterBufferStats
{
    public int PacketsReceived { get; set; }
    public int PacketsPlayed { get; set; }
    public int PacketsLost { get; set; }
    public int DuplicatePackets { get; set; }
    public int BufferAdjustments { get; set; }
    public double CurrentJitter { get; set; }
    public double AverageJitter { get; set; }
    public DateTime LastArrivalTime { get; set; }

    public double PacketLossRate => PacketsReceived > 0
        ? (PacketsLost * 100.0 / PacketsReceived)
        : 0;
}