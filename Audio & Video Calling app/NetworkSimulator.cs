using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class NetworkSimulator
{
    private Random random = new Random();

    public double PacketLossRate { get; set; } = 0.0;
    public int BaseLatency { get; set; } = 20;
    public int JitterRange { get; set; } = 0;
    public double ReorderRate { get; set; } = 0.0;
    public bool IsEnabled { get; set; } = false;
    public int TotalPackets { get; private set; }
    public int DroppedPackets { get; private set; }
    public int DelayedPackets { get; private set; }
    public int ReorderedPackets { get; private set; }

    private Queue<DelayedPacket> packetQueue = new Queue<DelayedPacket>();
    private object queueLock = new object();

    public event Action<byte[], Action<byte[]>> OnPacketReady;

    public void ProcessPacket(byte[] packet, Action<byte[]> deliverCallback)
    {
        TotalPackets++;

        if (!IsEnabled)
        {
            deliverCallback(packet);
            return;
        }

        if (ShouldDropPacket())
        {
            DroppedPackets++;
            return;
        }

        int delay = CalculateDelay();

        if (ShouldReorderPacket() && packetQueue.Count > 0)
        {
            ReorderedPackets++;
            lock (queueLock)
            {
                var oldPacket = packetQueue.Dequeue();
                ScheduleDelivery(packet, delay, deliverCallback);
                ScheduleDelivery(oldPacket.Data, oldPacket.Delay / 2, deliverCallback);
            }
        }
        else
        {
            ScheduleDelivery(packet, delay, deliverCallback);
        }
    }

    private bool ShouldDropPacket()
    {
        return random.NextDouble() * 100 < PacketLossRate;
    }

    private int CalculateDelay()
    {
        int jitter = random.Next(-JitterRange, JitterRange + 1);
        int totalDelay = Math.Max(0, BaseLatency + jitter);

        if (totalDelay > BaseLatency)
            DelayedPackets++;

        return totalDelay;
    }

    private bool ShouldReorderPacket()
    {
        return random.NextDouble() * 100 < ReorderRate;
    }


    private void ScheduleDelivery(byte[] packet, int delay, Action<byte[]> callback)
    {
        if (delay == 0)
        {
            callback(packet);
        }
        else
        {
            Task.Delay(delay).ContinueWith(_ => callback(packet));
        }
    }

    public void ResetStats()
    {
        TotalPackets = 0;
        DroppedPackets = 0;
        DelayedPackets = 0;
        ReorderedPackets = 0;
    }

    public string GetStats()
    {
        double lossRate = TotalPackets > 0 ? (DroppedPackets * 100.0 / TotalPackets) : 0;
        return $"Simulator Stats: " +
               $"Total={TotalPackets}, " +
               $"Dropped={DroppedPackets} ({lossRate:F1}%), " +
               $"Delayed={DelayedPackets}, " +
               $"Reordered={ReorderedPackets}";
    }

    public void ApplyPreset(NetworkCondition condition)
    {
        switch (condition)
        {
            case NetworkCondition.Perfect:
                PacketLossRate = 0;
                BaseLatency = 10;
                JitterRange = 0;
                ReorderRate = 0;
                IsEnabled = false;
                break;

            case NetworkCondition.Good:
                PacketLossRate = 0.5;
                BaseLatency = 20;
                JitterRange = 5;
                ReorderRate = 0.1;
                IsEnabled = true;
                break;

            case NetworkCondition.Fair:
                PacketLossRate = 2.0;
                BaseLatency = 50;
                JitterRange = 15;
                ReorderRate = 1.0;
                IsEnabled = true;
                break;

            case NetworkCondition.Poor:
                PacketLossRate = 5.0;
                BaseLatency = 100;
                JitterRange = 30;
                ReorderRate = 3.0;
                IsEnabled = true;
                break;

            case NetworkCondition.VeryPoor:
                PacketLossRate = 10.0;
                BaseLatency = 200;
                JitterRange = 50;
                ReorderRate = 5.0;
                IsEnabled = true;
                break;
        }
    }
}

internal class DelayedPacket
{
    public byte[] Data { get; set; }
    public int Delay { get; set; }
}

public enum NetworkCondition
{
    Perfect,
    Good,
    Fair,
    Poor,
    VeryPoor
}