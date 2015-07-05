namespace Nine.Graphics.Runner
{
    using System;
    using System.Runtime.InteropServices;

    // http://stackoverflow.com/questions/10773003/attach-form-window-to-another-window-in-c-sharp
    static class WindowHelper
    {
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int GWL_STYLE = -16;
        const int WS_CHILD = 0x40000000;
        const int WS_CAPTION = 0xc00000;
        const int WS_THICKFRAME = 0x00040000;
        const int WS_MINIMIZE = 0x20000000;
        const int WS_MAXIMIZE = 0x1000000;
        const int WS_SYSMENU = 0x80000;

        public static void EmbedWindow(IntPtr guestHandle, IntPtr hostHandle)
        {
            ShowWindow(guestHandle, 0);

            var existingStyle = GetWindowLong(guestHandle, GWL_STYLE);
            existingStyle &= ~(WS_CAPTION | WS_THICKFRAME | WS_MINIMIZE | WS_MAXIMIZE | WS_SYSMENU);
            SetWindowLong(guestHandle, GWL_STYLE, existingStyle | WS_CHILD);

            SetParent(guestHandle, hostHandle);

            ShowWindow(guestHandle, 0);
        }
    }
}
