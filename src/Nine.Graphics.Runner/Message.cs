namespace Nine.Graphics.Runner
{
    using System;
    using System.Runtime.InteropServices;

    enum MessageType
    {
        HostWindow,
        HostResize,
        GuestWindowAttached,
        GuestShutdown,
    }

    [StructLayout(LayoutKind.Sequential)]
    struct Message
    {
        public MessageType MessageType;
        public IntPtr Pointer;
        public int Width;
        public int Height;
        
        public byte[] ToBytes()
        {
            int size = Marshal.SizeOf(this);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(this, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

        public static Message FromBytes(byte[] arr, int length)
        {
            Message str = new Message();

            int size = Marshal.SizeOf(str);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(arr, 0, ptr, size);

            str = (Message)Marshal.PtrToStructure(ptr, typeof(Message));
            Marshal.FreeHGlobal(ptr);

            return str;
        }
    }
}
