using System;
using System.Drawing;
using System.Windows.Forms;

namespace AI
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
            btnAbout.Click += BtnAbout_Click;
            btnGuide.Click += BtnGuide_Click;
            btnStart.Click += BtnStart_Click;
            btnSoundOn.Click += BtnSound_Click;
            btnSoundOff.Click += BtnSound_Click;
            Load += FrmMain_Load;
        }

        public void ShowForm(Form f)
        {
            f.Size = this.Size;
            f.Location = this.Location;
            f.StartPosition = FormStartPosition.Manual;
            this.Hide();
            f.ShowDialog(this);
            AppState.GoHome = false; // Reset cờ nếu còn set
            this.Show();
            UpdateSoundButtonUi();
            SoundManager.EnsureBackgroundMusic();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            UpdateSoundButtonUi();
            SoundManager.EnsureBackgroundMusic();
        }

        private void BtnSound_Click(object sender, EventArgs e)
        {
            bool wasMuted = AppState.IsMuted;
            AppState.IsMuted = !AppState.IsMuted;
            UpdateSoundButtonUi();

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

        private void UpdateSoundButtonUi()
        {
            btnSoundOn.Visible = !AppState.IsMuted;
            btnSoundOff.Visible = AppState.IsMuted;

            btnSoundOn.Text = "🔊 ÂM THANH: BẬT";
            btnSoundOff.Text = "🔇 ÂM THANH: TẮT";

            btnSoundOn.BringToFront();
            btnSoundOff.BringToFront();
        }

        private void BtnAbout_Click(object sender, EventArgs e)
        {
            AppState.PlayUiClickSound();
            frmAbout f = new frmAbout();
            ShowForm(f);
        }

        private void BtnGuide_Click(object sender, EventArgs e)
        {
            AppState.PlayUiClickSound();
            frmGuide f = new frmGuide();
            ShowForm(f);
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            AppState.PlayUiClickSound();
            frmStart f = new frmStart();
            ShowForm(f);
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {

        }
    }
}
