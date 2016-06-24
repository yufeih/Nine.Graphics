namespace Nine.Graphics.Content
{
    using System;
    using System.Runtime.InteropServices;

    static class Interop
    {
        [DllImport("kernel32", EntryPoint = "LoadLibrary", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibrary(string lpFileName);
    }
}
