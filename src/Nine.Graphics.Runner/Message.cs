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
    }
}
