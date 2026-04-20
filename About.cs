using System;
using System.Drawing;
using System.Windows.Forms;

namespace AI
{
    public partial class frmAbout : Form
    {
        public frmAbout()
        {
            InitializeComponent();
            btnOK.Click += BtnOK_Click;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            AppState.PlayUiClickSound();
            this.Close();
        }

        private void frmAbout_FormClosing(object sender, FormClosingEventArgs e)
        {

        }
    }
}