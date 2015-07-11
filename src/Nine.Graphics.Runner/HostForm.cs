namespace Nine.Graphics.Runner
{
    using System;
    using System.Drawing;
    using System.Drawing.Text;
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
        private readonly Font largeFont = new Font("Segoe UI Light", 72);
        private readonly Font smallFont = new Font("Consolas", 18);
        private readonly Label label = new AwesomeLabel
        {
            Dock = DockStyle.Fill,
            AutoEllipsis = true,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.FromArgb(160, 160, 160),
        };

        public HostForm(bool topMost)
        {
            BackColor = Color.FromArgb(60, 60, 60);
            Controls.Add(label);

            this.topMost = topMost;
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
    }
}
