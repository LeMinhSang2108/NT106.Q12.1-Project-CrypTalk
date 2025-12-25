using System;
using System.Linq;

public class EchoCanceller
{
    private int filterLength;
    private double[] adaptiveFilter;
    private double[] referenceBuffer;
    private double stepSize;
    private double regularization;

    private int maxDelay = 400;
    private int estimatedDelay = 0;
    private double[] delayBuffer;
    private int delayBufferIndex = 0;
    private bool delayEstimated = false;
    private int delayEstimationCounter = 0;

    private Queue<double> micEnergyHistory = new Queue<double>(30);
    private Queue<double> speakerEnergyHistory = new Queue<double>(30);
    private Queue<double> erleHistory = new Queue<double>(20);

    private double[] outputBuffer = new double[10];
    private int outputIndex = 0;
    private double suppressionGain = 1.0;
    private double targetSuppressionGain = 1.0;
    private double averageERLE = 0;
    private int convergenceCounter = 0;

    public bool IsEnabled { get; set; } = false;
    public double EchoSuppressionLevel { get; private set; }
    public int ProcessedSamples { get; private set; }
    public int EstimatedDelayMs => (int)(estimatedDelay / 8.0);

    public EchoCanceller(int filterLength = 512, double stepSize = 0.3)
    {
        this.filterLength = filterLength;
        this.stepSize = stepSize;
        this.regularization = 1e-6;

        adaptiveFilter = new double[this.filterLength];
        referenceBuffer = new double[this.filterLength + maxDelay];
        delayBuffer = new double[maxDelay];
    }

    public byte[] ProcessAudio(byte[] micInput, byte[] speakerOutput)
    {
        if (!IsEnabled || micInput == null || speakerOutput == null)
            return micInput;

        if (speakerOutput.Length == 0)
            return micInput;

        int sampleCount = Math.Min(micInput.Length / 2, speakerOutput.Length / 2);
        byte[] output = new byte[micInput.Length];

        try
        {
            double[] micSignals = new double[sampleCount];
            double[] speakerSignals = new double[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                short micSample = BitConverter.ToInt16(micInput, i * 2);
                short speakerSample = BitConverter.ToInt16(speakerOutput, Math.Min(i * 2, speakerOutput.Length - 2));

                micSignals[i] = micSample / 32768.0;
                speakerSignals[i] = speakerSample / 32768.0;
            }

            double micEnergy = CalculateEnergy(micSignals);
            double speakerEnergy = CalculateEnergy(speakerSignals);

            micEnergyHistory.Enqueue(micEnergy);
            if (micEnergyHistory.Count > 30) micEnergyHistory.Dequeue();

            speakerEnergyHistory.Enqueue(speakerEnergy);
            if (speakerEnergyHistory.Count > 30) speakerEnergyHistory.Dequeue();

            if (!delayEstimated && speakerEnergy > 0.05)
            {
                EstimateDelay(micSignals, speakerSignals);
            }

            for (int i = 0; i < sampleCount; i++)
            {
                double micSignal = micSignals[i];
                double speakerSignal = speakerSignals[i];

                delayBuffer[delayBufferIndex] = speakerSignal;
                delayBufferIndex = (delayBufferIndex + 1) % maxDelay;

                int delayedIndex = (delayBufferIndex - estimatedDelay + maxDelay) % maxDelay;
                double delayedSpeaker = delayBuffer[delayedIndex];

                UpdateReferenceBuffer(delayedSpeaker);

                double estimatedEcho = EstimateEcho();
                double errorSignal = micSignal - estimatedEcho;

                double echoStrength = Math.Abs(estimatedEcho);
                double micStrength = Math.Abs(micSignal);

                if (speakerEnergy > 0.01 && micEnergy > 0.01)
                {
                    double currentERLE = CalculateERLE(micEnergy, CalculateEnergy(new[] { errorSignal }));
                    erleHistory.Enqueue(currentERLE);
                    if (erleHistory.Count > 20) erleHistory.Dequeue();
                    averageERLE = erleHistory.Average();
                }

                bool isDoubleTalk = DetectDoubleTalk(micEnergy, speakerEnergy, errorSignal);
                double cleanSignal;

                if (speakerEnergy > 0.02 && !isDoubleTalk)
                {
                    cleanSignal = errorSignal;

                    if (averageERLE < 15)
                    {
                        targetSuppressionGain = 0.1;
                    }
                    else if (averageERLE < 25)
                    {
                        targetSuppressionGain = 0.3;
                    }
                    else if (averageERLE < 35)
                    {
                        targetSuppressionGain = 0.5;
                    }
                    else
                    {
                        targetSuppressionGain = 0.8;
                    }

                    if (echoStrength > micStrength * 0.3)
                    {
                        targetSuppressionGain *= 0.5;
                    }

                    suppressionGain = suppressionGain * 0.9 + targetSuppressionGain * 0.1;
                    cleanSignal *= suppressionGain;

                    if (Math.Abs(cleanSignal) < 0.003)
                    {
                        cleanSignal *= 0.1;
                    }

                    if (delayEstimated)
                    {
                        UpdateAdaptiveFilter(errorSignal, speakerEnergy, isDoubleTalk);
                    }

                    convergenceCounter++;
                }
                else if (isDoubleTalk && speakerEnergy > 0.02)
                {
                    double blendFactor = 0.7;
                    cleanSignal = errorSignal * blendFactor + micSignal * (1.0 - blendFactor);
                    suppressionGain = 1.0; 
                }
                else
                {
                    cleanSignal = micSignal;
                    suppressionGain = 1.0;
                }

                outputBuffer[outputIndex] = cleanSignal;
                outputIndex = (outputIndex + 1) % outputBuffer.Length;
                cleanSignal = outputBuffer.Average();
                cleanSignal = SoftLimit(cleanSignal, 0.95);

                EchoSuppressionLevel = (micStrength > 0.001)
                    ? Math.Min(100, (echoStrength / (micStrength + 0.001)) * 100.0)
                    : 0;

                short outputSample = (short)(cleanSignal * 32767);
                byte[] sampleBytes = BitConverter.GetBytes(outputSample);
                output[i * 2] = sampleBytes[0];
                output[i * 2 + 1] = sampleBytes[1];

                ProcessedSamples++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Echo canceller error: {ex.Message}");
            return micInput;
        }

        return output;
    }

    private void EstimateDelay(double[] micSignals, double[] speakerSignals)
    {
        delayEstimationCounter++;
        if (delayEstimationCounter < 50)
            return;

        delayEstimationCounter = 0;
        double maxCorr = 0;
        int bestDelay = 0;

        for (int delay = 0; delay < maxDelay && delay < micSignals.Length; delay++)
        {
            double corr = 0;
            int count = Math.Min(micSignals.Length - delay, speakerSignals.Length);

            for (int i = 0; i < count; i++)
            {
                corr += micSignals[i + delay] * speakerSignals[i];
            }

            corr = Math.Abs(corr / count);

            if (corr > maxCorr)
            {
                maxCorr = corr;
                bestDelay = delay;
            }
        }

        if (maxCorr > 0.05)
        {
            estimatedDelay = bestDelay;
            delayEstimated = true;
            Console.WriteLine($"[Echo] Delay estimated: {EstimatedDelayMs}ms");
        }
    }

    private bool DetectDoubleTalk(double micEnergy, double speakerEnergy, double errorSignal)
    {
        if (micEnergy > 0.04 && speakerEnergy > 0.04)
        {
            double ratio = micEnergy / (speakerEnergy + 0.001);
            if (ratio > 0.6)
                return true;
        }

        double errorEnergy = errorSignal * errorSignal;
        if (errorEnergy > micEnergy * 0.7 && speakerEnergy > 0.05)
        {
            return true;
        }

        if (averageERLE < 10 && micEnergy > 0.05 && speakerEnergy > 0.05)
        {
            return true;
        }

        return false;
    }

    private double CalculateEnergy(double[] signal)
    {
        double energy = 0;
        foreach (var s in signal)
        {
            energy += s * s;
        }
        return Math.Sqrt(energy / signal.Length);
    }

    private double CalculateERLE(double micEnergy, double residualEnergy)
    {
        if (residualEnergy < 1e-10)
            return 60;

        double erle = micEnergy / residualEnergy;
        return 10 * Math.Log10(erle);
    }

    private void UpdateReferenceBuffer(double sample)
    {
        for (int i = referenceBuffer.Length - 1; i > 0; i--)
        {
            referenceBuffer[i] = referenceBuffer[i - 1];
        }
        referenceBuffer[0] = sample;
    }

    private double EstimateEcho()
    {
        double echo = 0;
        for (int i = 0; i < filterLength; i++)
        {
            echo += adaptiveFilter[i] * referenceBuffer[i];
        }
        return echo;
    }

    private void UpdateAdaptiveFilter(double error, double speakerEnergy, bool isDoubleTalk)
    {
        if (isDoubleTalk)
            return;

        double power = 0;
        for (int i = 0; i < filterLength; i++)
        {
            power += referenceBuffer[i] * referenceBuffer[i];
        }

        double currentStep = stepSize;

        if (convergenceCounter < 500)
        {
            currentStep = stepSize * 2.0;
        }
        else if (averageERLE > 30)
        {
            currentStep = stepSize * 0.3;
        }
        else if (speakerEnergy < 0.05)
        {
            currentStep = stepSize * 0.1;
        }

        double mu = currentStep / (power + regularization);
        mu = Math.Min(mu, 0.5);

        for (int i = 0; i < filterLength; i++)
        {
            adaptiveFilter[i] += mu * error * referenceBuffer[i];
            adaptiveFilter[i] = Math.Max(-1.5, Math.Min(1.5, adaptiveFilter[i]));
        }
    }

    private double SoftLimit(double signal, double threshold)
    {
        if (Math.Abs(signal) > threshold)
        {
            double sign = Math.Sign(signal);
            double excess = Math.Abs(signal) - threshold;
            return sign * (threshold + Math.Tanh(excess * 5) * 0.05);
        }
        return signal;
    }

    public void Reset()
    {
        Array.Clear(adaptiveFilter, 0, filterLength);
        Array.Clear(referenceBuffer, 0, referenceBuffer.Length);
        Array.Clear(delayBuffer, 0, maxDelay);
        Array.Clear(outputBuffer, 0, outputBuffer.Length);

        micEnergyHistory.Clear();
        speakerEnergyHistory.Clear();
        erleHistory.Clear();

        ProcessedSamples = 0;
        EchoSuppressionLevel = 0;
        estimatedDelay = 0;
        delayEstimated = false;
        delayEstimationCounter = 0;
        convergenceCounter = 0;
        averageERLE = 0;
        suppressionGain = 1.0;
        targetSuppressionGain = 1.0;
        delayBufferIndex = 0;
        outputIndex = 0;
    }

    public string GetStats()
    {
        return $"Echo: {(IsEnabled ? "ON" : "OFF")}, ERLE={averageERLE:F1}dB, Delay={EstimatedDelayMs}ms, Suppression={EchoSuppressionLevel:F1}%";
    }
}