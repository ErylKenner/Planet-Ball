public struct InputPacket<T>
{
    public T input;
    public uint serverTick;
    public uint clientTick;
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

    public bool Ready
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

    public void Enqueue(InputPacket<T> inputPacket)
    {
        unprocessedCount++;
        buffer[(lastProcessed + unprocessedCount) % bufferSize] = inputPacket;
    }

    public InputPacket<T> Dequeue(uint serverTick)
    {

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
}
