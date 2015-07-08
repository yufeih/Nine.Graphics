namespace Nine.Graphics.Runner
{
    public interface ISharedMemory
    {
        byte[] Get(string name, int sizeInBytes);

        bool Remove(string name);
    }
}
