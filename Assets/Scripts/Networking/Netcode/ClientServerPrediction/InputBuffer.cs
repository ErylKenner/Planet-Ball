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
    private int lastRecievedTick;
    private int lastProcessed;

    public bool IsEmpty
    {
        get
        {
            return lastRecievedTick == lastProcessed;
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
        lastRecievedTick = -1;
        lastProcessed = -1;
    }

    public void Enqueue(T input, uint clientTick)
    {

        InputPacket<T> inputPacket = new InputPacket<T> { input = input, clientTick = clientTick };
        
        buffer[clientTick % bufferSize] = inputPacket;

        if(clientTick > lastRecievedTick)
        {

            for(int i = lastRecievedTick + 1; i < clientTick; i++)
            {
                buffer[i % bufferSize] = null;
            }
            lastRecievedTick = (int)clientTick;
        }
    }

    public InputPacket<T> Dequeue(uint serverTick)
    {
        if(lastProcessed >= lastRecievedTick)
        {
            throw new InvalidOperationException("Input buffer has no unprocessed items");
        }

        if(lastProcessed == -1)
        {
            lastProcessed = lastRecievedTick - 1;
        }

        InputPacket<T> packet = null;
        lastProcessed++;
        for (; lastProcessed <= lastRecievedTick; lastProcessed++)
        {
            if(buffer[lastProcessed % bufferSize] != null)
            {
                buffer[lastProcessed % bufferSize].serverTick = serverTick;
                packet = buffer[lastProcessed % bufferSize];
                break;
            }
        }

        return packet;
    }

    public InputPacket<T> LastProcessed()
    {
        return buffer[lastProcessed % bufferSize];
    }

    public InputPacket<T> LastRecieved()
    {
        return buffer[lastRecievedTick % bufferSize];
    }

    public void Clear()
    {
        for (int i = lastProcessed + 1; i <= lastRecievedTick; i++)
        {
            buffer[i % bufferSize] = null;
        }
        lastRecievedTick = lastProcessed;
    }
}
