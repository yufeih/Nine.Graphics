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

        private readonly Font largeFont = new Font("Segoe UI Light", 72);
        private readonly Font smallFont = new Font("Consolas", 18);
        private readonly Label label = new AwesomeLabel
        {
            Dock = DockStyle.Fill,
            AutoEllipsis = true,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.FromArgb(160, 160, 160),
        };

        public HostForm()
        {
            BackColor = Color.FromArgb(60, 60, 60);
            Controls.Add(label);
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
