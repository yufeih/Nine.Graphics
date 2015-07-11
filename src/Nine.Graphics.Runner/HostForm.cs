namespace Nine.Graphics.Runner
{
    using System;
    using System.Drawing;
    using System.Drawing.Text;
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    class HostForm : Form
    {
        class AwesomeLabel : Label
        {
            protected override void OnPaint(PaintEventArgs pe)
            {
                pe.Graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                base.OnPaint(pe);
            }
        }

        private readonly bool topMost;
        private readonly string locationSettingsFile;
        private readonly Font largeFont = new Font("Segoe UI Light", 72);
        private readonly Font smallFont = new Font("Consolas", 18);
        private readonly Label label = new AwesomeLabel
        {
            Dock = DockStyle.Fill,
            AutoEllipsis = true,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.FromArgb(160, 160, 160),
        };

        public HostForm(string appName, int? width, int? height, bool topMost)
        {
            BackColor = Color.FromArgb(60, 60, 60);
            Controls.Add(label);
            Text = appName;
            ShowIcon = false;
            ShowInTaskbar = false;

            this.topMost = topMost;
            this.locationSettingsFile = Path.Combine(Path.GetTempPath(), "Nine.Hosting", appName + ".location");

            if (!Directory.Exists(locationSettingsFile))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(locationSettingsFile));
            }

            LoadLocation(width, height);
        }

        // http://stackoverflow.com/questions/3729899/opening-a-winform-with-topmost-true-but-not-having-it-steal-focus
        protected override bool ShowWithoutActivation => topMost;

        private const int WS_EX_TOPMOST = 0x00000008;
        protected override CreateParams CreateParams
        {
            get
            {
                if (topMost)
                {
                    CreateParams createParams = base.CreateParams;
                    createParams.ExStyle |= WS_EX_TOPMOST;
                    return createParams;
                }
                return base.CreateParams;
            }
        }

        public void SetText(string text)
        {
            if (IsHandleCreated)
            {
                BeginInvoke(new Action(() => UpdateText(text)));
            }
            else
            {
                UpdateText(text);
            }
        }

        private void UpdateText(string text)
        {
            label.Text = text ?? "";

            var isLongText = text.Contains("\n");
            if (isLongText)
            {
                label.TextAlign = ContentAlignment.MiddleLeft;
                label.Font = smallFont;
            }
            else
            {
                label.TextAlign = ContentAlignment.MiddleCenter;
                label.Font = largeFont;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            // LoadLocation(null, null);
            base.OnShown(e);
        }

        private void LoadLocation(int? width, int? height)
        {
            string[] location = null;

            try
            {
                location = (File.ReadAllText(locationSettingsFile) ?? "").Split(',');
            }
            catch { }

            var left = GetNumber(location, 0);
            var top = GetNumber(location, 1);

            if (left.HasValue && top.HasValue)
            {
                this.Left = left.Value;
                this.Top = top.Value;

                StartPosition = FormStartPosition.Manual;
            }
            else
            {
                StartPosition = FormStartPosition.WindowsDefaultLocation;
            }

            Width = width ?? GetNumber(location, 2) ?? 1024;
            Height = height ?? GetNumber(location, 3) ?? 768;
        }

        private int? GetNumber(string[] location, int index)
        {
            int result;
            if (location?.Length > index && int.TryParse(location[index], out result))
            {
                return result;
            }
            return null;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            SaveLocationWithDebounce();
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            SaveLocationWithDebounce();
        }

        private DateTime lastSavedTime;

        private async void SaveLocationWithDebounce()
        {
            if (DateTime.UtcNow - lastSavedTime < TimeSpan.FromSeconds(1))
            {
                await Task.Delay(1000);
            }

            if (DateTime.UtcNow - lastSavedTime >= TimeSpan.FromSeconds(1))
            {
                lastSavedTime = DateTime.UtcNow;
                SaveLocation();
            }
        }

        private void SaveLocation()
        {
            try
            {
                File.WriteAllText(locationSettingsFile, $"{Left},{Top},{Width},{Height}");
            }
            catch { }
        }
    }
}
