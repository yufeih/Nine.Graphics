namespace Nine.Graphics.Runner
{
    using System;
    using System.Runtime.InteropServices;

    static class WindowHelper
    {
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        const int GWL_STYLE = -16;
        const int STYLE = 0x46030000;

        public static void EmbedWindow(IntPtr guestHandle, IntPtr hostHandle)
        {
            ShowWindow(guestHandle, 0);
            
            SetWindowLong(guestHandle, GWL_STYLE, STYLE);

            SetParent(guestHandle, hostHandle);

            ShowWindow(guestHandle, 5);
        }

        public static void Resize(IntPtr handle, int width, int height)
        {
            SetWindowPos(handle, IntPtr.Zero, 0, 0, width, height, 0);
        }
    }
}
