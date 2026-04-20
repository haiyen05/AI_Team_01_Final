using System;
using System.Drawing;
using System.Windows.Forms;

namespace AI
{
    public partial class frmPause : Form
    {
        private bool _isMuted;

        public frmPause()
        {
            InitializeComponent();

            Load += Pause_Load;
            btnContinue.Click += BtnContinue_Click;
            btnReturn.Click += BtnReturn_Click;
            btnQuayLai.Click += BtnQuayLai_Click;
            btnSoundOn.Click += BtnSound_Click;
            btnSoundOff.Click += BtnSound_Click;
        }

        public bool IsMuted => _isMuted;

        public void SetSoundState(bool isMuted)
        {
            _isMuted = isMuted;
            AppState.IsMuted = isMuted;
            UpdateSoundButtonText();
        }

        private void Pause_Load(object sender, EventArgs e)
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            MinimizeBox = false;
            MaximizeBox = false;
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
                BackColor = Color.FromArgb(70, 70, 70);
            }

            //guna2Panel1.FillColor = Color.FromArgb(247, 239, 220);
            guna2Panel1.Anchor = AnchorStyles.None;
            guna2Panel1.Left = (ClientSize.Width - guna2Panel1.Width) / 2;
            guna2Panel1.Top = (ClientSize.Height - guna2Panel1.Height) / 2;
            guna2Panel1.BringToFront();
            UpdateSoundButtonText();
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

        private void BtnContinue_Click(object sender, EventArgs e)
        {
            AppState.PlayUiClickSound();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnReturn_Click(object sender, EventArgs e)
        {
            AppState.PlayUiClickSound();
            DialogResult = DialogResult.Retry;
            Close();
        }

        private void BtnQuayLai_Click(object sender, EventArgs e)
        {
            AppState.PlayUiClickSound();
            DialogResult = DialogResult.Abort;
            Close();
        }

        private void BtnSound_Click(object sender, EventArgs e)
        {
            bool wasMuted = AppState.IsMuted;
            _isMuted = !AppState.IsMuted;
            AppState.IsMuted = _isMuted;
            UpdateSoundButtonText();

            if (AppState.IsMuted)
            {
                SoundManager.StopBackgroundMusic();
            }
            else
            {
                SoundManager.EnsureBackgroundMusic();
                if (wasMuted)
                {
                    AppState.PlayToggleOnSound();
                }
            }
        }

        private void UpdateSoundButtonText()
        {
            btnSoundOn.Visible = !_isMuted;
            btnSoundOff.Visible = _isMuted;
            btnSoundOn.BringToFront();
            btnSoundOff.BringToFront();
        }
    }
}
