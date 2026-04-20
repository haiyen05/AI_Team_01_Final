using System;
using System.Drawing;
using System.Windows.Forms;

namespace AI
{
    public partial class frmResult : Form
    {
        private readonly string _resultText;

        public frmResult() : this("Người chơi 1 thắng!")
        {
        }

        public frmResult(string resultText)
        {
            InitializeComponent();
            _resultText = string.IsNullOrWhiteSpace(resultText) ? "Người chơi 1 thắng!" : resultText;

            Load += Result_Load;
            btnReturn.Click += BtnReturn_Click;
            guna2Button3.Click += Guna2Button3_Click;
        }

        private void BtnReturn_Click(object sender, EventArgs e)
        {
            AppState.PlayUiClickSound();
            DialogResult = DialogResult.Retry;
            Close();
        }

        private void Guna2Button3_Click(object sender, EventArgs e)
        {
            AppState.PlayUiClickSound();
            DialogResult = DialogResult.Abort;
            Close();
        }

        private void Result_Load(object sender, EventArgs e)
        {
            lblResult.Text = _resultText;
            lblResult.AutoSize = true;
            lblResult.Left = Math.Max(20, (guna2Panel2.Width - lblResult.Width) / 2);
            lblResult.Top = 30;

            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Opacity = 1d;
            DoubleBuffered = true;

            if (Owner != null)
            {
                Bounds = Owner.Bounds;
                BackgroundImage = CreateDimmedOwnerSnapshot();
                BackgroundImageLayout = ImageLayout.Stretch;
            }
            else
            {
                WindowState = FormWindowState.Maximized;
                BackColor = Color.FromArgb(70, 70, 70);
            }

            guna2Panel1.FillColor = Color.FromArgb(250, 241, 220);
            guna2Panel2.FillColor = Color.FromArgb(250, 241, 220);
            guna2Panel1.Left = (ClientSize.Width - guna2Panel1.Width) / 2;
            guna2Panel1.Top = (ClientSize.Height - guna2Panel1.Height) / 2;
            guna2Panel1.BringToFront();
        }

        private Image CreateDimmedOwnerSnapshot()
        {
            if (Owner == null)
            {
                return null;
            }

            Bitmap bitmap = new Bitmap(Math.Max(1, Owner.ClientSize.Width), Math.Max(1, Owner.ClientSize.Height));
            Owner.DrawToBitmap(bitmap, new Rectangle(Point.Empty, Owner.ClientSize));
            using (Graphics graphics = Graphics.FromImage(bitmap))
            using (Brush overlay = new SolidBrush(Color.FromArgb(135, 0, 0, 0)))
            {
                graphics.FillRectangle(overlay, new Rectangle(Point.Empty, bitmap.Size));
            }

            return bitmap;
        }
    }
}
