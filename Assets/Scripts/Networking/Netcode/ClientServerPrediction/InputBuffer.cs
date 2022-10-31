using System;

public class InputPacket<T>
{
    public T input;
    public uint serverTick = 0;
    public uint clientTick = 0;
}
public class InputBuffer<T>
{


    public readonly uint bufferSize;
    private InputPacket<T>[] buffer;
    private uint unprocessedCount;
    private int lastProcessed;

    public uint Count
    {
        get
        {
            return unprocessedCount;
        }
    }

    public bool BeenProcessed
    {
        get
        {
            return lastProcessed != -1;
        }
    }

    public InputBuffer(uint size)
    {
        bufferSize = size;
        buffer = new InputPacket<T>[bufferSize];
        unprocessedCount = 0;
        lastProcessed = -1;
    }

    public void Enqueue(T input, uint clientTick)
    {
        if(BeenProcessed && buffer[(lastProcessed + unprocessedCount) % bufferSize].clientTick == clientTick)
        {
            return;
        }

        InputPacket<T> inputPacket = new InputPacket<T> { input = input, clientTick = clientTick };
        unprocessedCount++;
        buffer[(lastProcessed + unprocessedCount) % bufferSize] = inputPacket;
    }

    public InputPacket<T> Dequeue(uint serverTick)
    {
        if(unprocessedCount == 0)
        {
            throw new InvalidOperationException("Input buffer has no unprocessed items");
        }

        lastProcessed++;
        lastProcessed %= (int)bufferSize;
        unprocessedCount--;

        buffer[lastProcessed].serverTick = serverTick;

        return buffer[lastProcessed];
    }

    public InputPacket<T> LastProcessed()
    {
        return buffer[lastProcessed];
    }

    public InputPacket<T> LastRecieved()
    {
        return buffer[(lastProcessed + unprocessedCount) % bufferSize];
    }

    public void Clear()
    {
        unprocessedCount = 0;
    }
}
