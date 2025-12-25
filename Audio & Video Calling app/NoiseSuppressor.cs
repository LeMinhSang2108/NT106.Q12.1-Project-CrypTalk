using System;
using System.Linq;

public class NoiseSuppressor
{
    private double noiseFloor;
    private double[] noiseProfile;
    private int profileSize = 100;
    private double[] recentSamples;
    private int sampleIndex = 0;
    private const double NOISE_GATE_THRESHOLD = 0.015;
    private const double SMOOTHING_FACTOR = 0.90;

    public bool IsEnabled { get; set; } = false;
    public double NoiseLevel { get; private set; }
    public double SignalLevel { get; private set; }
    public int ProcessedFrames { get; private set; }

    public NoiseSuppressor()
    {
        noiseProfile = new double[profileSize];
        recentSamples = new double[profileSize];
        noiseFloor = 0.01;
    }

    public byte[] ProcessAudio(byte[] input)
    {
        if (!IsEnabled || input == null || input.Length == 0)
            return input;

        byte[] output = new byte[input.Length];
        int sampleCount = input.Length / 2;

        try
        {
            double frameEnergy = 0;

            for (int i = 0; i < sampleCount; i++)
            {
                short sample = BitConverter.ToInt16(input, i * 2);
                double signal = sample / 32768.0;

                frameEnergy += signal * signal;

                recentSamples[sampleIndex] = Math.Abs(signal);
                sampleIndex = (sampleIndex + 1) % profileSize;

                if (ProcessedFrames % 10 == 0)
                {
                    var sorted = recentSamples.OrderBy(x => x).ToArray();
                    noiseFloor = sorted[profileSize / 10];
                }

                double absSignal = Math.Abs(signal);

                double gain = 1.0;

                if (absSignal < noiseFloor + NOISE_GATE_THRESHOLD)
                {
                    double ratio = absSignal / (noiseFloor + NOISE_GATE_THRESHOLD);
                    gain = Math.Pow(ratio, 2);
                }
                else
                {
                    gain = 1.0;
                }

                double cleanSignal = signal * gain;
                cleanSignal = Math.Max(-0.98, Math.Min(0.98, cleanSignal));
                short outputSample = (short)(cleanSignal * 32767);
                byte[] sampleBytes = BitConverter.GetBytes(outputSample);
                output[i * 2] = sampleBytes[0];
                output[i * 2 + 1] = sampleBytes[1];
            }

            frameEnergy = Math.Sqrt(frameEnergy / sampleCount);
            SignalLevel = frameEnergy;
            NoiseLevel = noiseFloor;
            ProcessedFrames++;
        }
        catch
        {
            return input;
        }

        return output;
    }

    public void Reset()
    {
        Array.Clear(noiseProfile, 0, profileSize);
        Array.Clear(recentSamples, 0, profileSize);
        sampleIndex = 0;
        ProcessedFrames = 0;
        noiseFloor = 0.01;
    }

    public string GetStats()
    {
        double snr = SignalLevel > 0 ? 20 * Math.Log10(SignalLevel / (NoiseLevel + 0.0001)) : 0;
        return $"Noise Suppressor: {(IsEnabled ? "ON" : "OFF")}, SNR={snr:F1}dB, Floor={NoiseLevel:F4}";
    }
}