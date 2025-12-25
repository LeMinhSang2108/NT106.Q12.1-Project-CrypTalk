using System;
using System.IO;
using NAudio.Wave;

public class CallRecorder : IDisposable
{
    private WaveFileWriter localWriter;
    private WaveFileWriter remoteWriter;
    private WaveFileWriter mixedWriter;

    private WaveFormat waveFormat;
    private bool isRecording = false;

    private string recordingFolder;
    private string sessionId;

    public bool IsRecording => isRecording;
    public string CurrentSessionId => sessionId;

    public CallRecorder(WaveFormat format)
    {
        waveFormat = format;
        recordingFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "VoipRecordings"
        );

        if (!Directory.Exists(recordingFolder))
            Directory.CreateDirectory(recordingFolder);
    }

    public void StartRecording()
    {
        if (isRecording)
            return;

        sessionId = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        string localFile = Path.Combine(recordingFolder, $"{sessionId}_local.wav");
        string remoteFile = Path.Combine(recordingFolder, $"{sessionId}_remote.wav");
        string mixedFile = Path.Combine(recordingFolder, $"{sessionId}_mixed.wav");

        localWriter = new WaveFileWriter(localFile, waveFormat);
        remoteWriter = new WaveFileWriter(remoteFile, waveFormat);

        var stereoFormat = new WaveFormat(waveFormat.SampleRate, 16, 2);
        mixedWriter = new WaveFileWriter(mixedFile, stereoFormat);

        isRecording = true;
    }

    public string StopRecording()
    {
        if (!isRecording)
            return null;

        localWriter?.Close();
        remoteWriter?.Close();
        mixedWriter?.Close();

        localWriter?.Dispose();
        remoteWriter?.Dispose();
        mixedWriter?.Dispose();

        localWriter = null;
        remoteWriter = null;
        mixedWriter = null;

        isRecording = false;

        return Path.Combine(recordingFolder, $"{sessionId}_mixed.wav");
    }

    public void WriteLocalAudio(byte[] buffer, int offset, int count)
    {
        if (!isRecording || localWriter == null)
            return;

        try
        {
            localWriter.Write(buffer, offset, count);

            WriteMixedAudio(buffer, offset, count, true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing local audio: {ex.Message}");
        }
    }

    public void WriteRemoteAudio(byte[] buffer, int offset, int count)
    {
        if (!isRecording || remoteWriter == null)
            return;

        try
        {
            remoteWriter.Write(buffer, offset, count);

            WriteMixedAudio(buffer, offset, count, false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing remote audio: {ex.Message}");
        }
    }

    private void WriteMixedAudio(byte[] buffer, int offset, int count, bool isLeft)
    {
        if (mixedWriter == null)
            return;

        byte[] stereoBuffer = new byte[count * 2];

        for (int i = 0; i < count; i += 2)
        {
            if (isLeft)
            {
                stereoBuffer[i * 2] = buffer[offset + i];
                stereoBuffer[i * 2 + 1] = buffer[offset + i + 1];
                stereoBuffer[i * 2 + 2] = 0;
                stereoBuffer[i * 2 + 3] = 0;
            }
            else
            {
                stereoBuffer[i * 2] = 0;
                stereoBuffer[i * 2 + 1] = 0;
                stereoBuffer[i * 2 + 2] = buffer[offset + i];
                stereoBuffer[i * 2 + 3] = buffer[offset + i + 1];
            }
        }

        mixedWriter.Write(stereoBuffer, 0, stereoBuffer.Length);
    }

    public string[] GetRecordings()
    {
        if (!Directory.Exists(recordingFolder))
            return new string[0];

        return Directory.GetFiles(recordingFolder, "*_mixed.wav");
    }

    public void PlayRecording(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        try
        {
            var audioFile = new AudioFileReader(filePath);
            var outputDevice = new WaveOutEvent();
            outputDevice.Init(audioFile);
            outputDevice.Play();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error playing recording: {ex.Message}");
        }
    }

    public void DeleteRecording(string sessionId)
    {
        try
        {
            string[] files = {
                Path.Combine(recordingFolder, $"{sessionId}_local.wav"),
                Path.Combine(recordingFolder, $"{sessionId}_remote.wav"),
                Path.Combine(recordingFolder, $"{sessionId}_mixed.wav")
            };

            foreach (var file in files)
            {
                if (File.Exists(file))
                    File.Delete(file);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting recording: {ex.Message}");
        }
    }

    public RecordingInfo GetRecordingInfo(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        var fileInfo = new FileInfo(filePath);

        using (var reader = new WaveFileReader(filePath))
        {
            return new RecordingInfo
            {
                FileName = Path.GetFileName(filePath),
                FilePath = filePath,
                Duration = reader.TotalTime,
                FileSize = fileInfo.Length,
                SampleRate = reader.WaveFormat.SampleRate,
                Channels = reader.WaveFormat.Channels,
                CreatedDate = fileInfo.CreationTime
            };
        }
    }

    public void Dispose()
    {
        StopRecording();
    }
}

public class RecordingInfo
{
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public TimeSpan Duration { get; set; }
    public long FileSize { get; set; }
    public int SampleRate { get; set; }
    public int Channels { get; set; }
    public DateTime CreatedDate { get; set; }

    public string FormattedDuration =>
        $"{(int)Duration.TotalMinutes:D2}:{Duration.Seconds:D2}";

    public string FormattedFileSize
    {
        get
        {
            if (FileSize < 1024) return $"{FileSize} B";
            if (FileSize < 1024 * 1024) return $"{FileSize / 1024} KB";
            return $"{FileSize / (1024 * 1024)} MB";
        }
    }
}