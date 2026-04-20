using System;
using System.Windows.Forms;

namespace AI
{
    public partial class frmStart : Form
    {
        public frmStart()
        {
            InitializeComponent();

            btnQuayLai.Click += BtnQuayLai_Click;
            btnOnePeople.Click += BtnOnePeople_Click;
            btnTwoPeople.Click += BtnTwoPeople_Click;
        }
        public void ShowForm(Form f)
        {
            f.Size = this.Size;
            f.Location = this.Location;
            f.StartPosition = FormStartPosition.Manual;
            this.Hide();
            f.ShowDialog(this);
            if (AppState.GoHome)
            {
                AppState.GoHome = false;
                this.Close();
                return;
            }
            this.Show();
        }
        private void BtnQuayLai_Click(object sender, EventArgs e)
        {
            AppState.PlayUiClickSound();
            this.Close();
        }

        private void BtnOnePeople_Click(object sender, EventArgs e)
        {
            AppState.PlayUiClickSound();
            frmOnePeople f = new frmOnePeople();
            ShowForm(f);
        }

        private void BtnTwoPeople_Click(object sender, EventArgs e)
        {
            AppState.PlayUiClickSound();
            frmTwoPeople f = new frmTwoPeople();
            ShowForm(f);
        }

        private void Start_Load_1(object sender, EventArgs e)
        {

        }

        private void frmStart_FormClosing(object sender, FormClosingEventArgs e)
        {

        }
    }
}