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
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=260)]
        public string Text;
    }
}
