namespace Nine.Graphics.Runner
{
    using System;
    using System.Text;
    using System.Runtime.InteropServices;

    enum MessageType
    {
        HostWindow,
        HostResize,
        GuestWindowAttached,
        GuestShutdown,
        GuestRequestSharedMemory,
        GuestRemoveSharedMemory,
    }

    [StructLayout(LayoutKind.Sequential)]
    unsafe struct Message
    {
        public const int MaxBytesLength = 260;

        public MessageType MessageType;
        public IntPtr Pointer;
        public int Width;
        public int Height;
        public short ByteCount;
        public fixed byte Bytes[MaxBytesLength];

        public Message(MessageType type) : this()
        {
            MessageType = type;
        }

        public Message WithName(string name)
        {
            var byteCount = Encoding.UTF8.GetByteCount(name);
            if (byteCount > MaxBytesLength)
            {
                throw new ArgumentOutOfRangeException($"{ name } exceeded the max length { MaxBytesLength }");
            }
            
            fixed (byte* pDest = Bytes)
            {
                fixed (char* pSrc = name)
                {
                    ByteCount = (short)Encoding.UTF8.GetBytes(pSrc, name.Length, pDest, byteCount);
                }
            }

            return this;
        }

        public string GetName()
        {
            if (ByteCount <= 0) return "";

            var bytes = new byte[ByteCount];

            fixed (byte* p = Bytes)
            {
                Marshal.Copy(new IntPtr(p), bytes, 0, ByteCount);
            }

            return Encoding.UTF8.GetString(bytes);
        }
    }
}
