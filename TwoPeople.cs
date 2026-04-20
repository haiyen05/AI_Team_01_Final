using System;
using System.Drawing;
using System.Windows.Forms;

namespace AI
{
    public partial class frmTwoPeople : Form
    {
        public frmTwoPeople()
        {
            InitializeComponent();
            Load += TwoPeople_Load;
            btnStart.Click += BtnStart_Click;
            btnBack.Click += BtnBack_Click;
            txtNhapTen1.Enter += TxtNhapTen1_Enter;
            txtNhapTen1.Leave += TxtNhapTen1_Leave;
            txtNhapTen2.Enter += TxtNhapTen2_Enter;
            txtNhapTen2.Leave += TxtNhapTen2_Leave;
            ConfigureNameTextBoxes();
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
                this.Close();
                return;
            }
            this.Show();
        }
        private void TwoPeople_Load(object sender, EventArgs e)
        {
            txtNhapTen1.Text = "Người chơi 1";
            txtNhapTen2.Text = "Người chơi 2";
            ConfigureNameTextBoxes();
        }
        private void BtnStart_Click(object sender, EventArgs e)
        {
            AppState.PlayUiClickSound();
            string player1 = txtNhapTen1.Text.Trim();
            string player2 = txtNhapTen2.Text.Trim();
            if (string.IsNullOrWhiteSpace(player1) || string.IsNullOrWhiteSpace(player2))
            {
                MessageBox.Show("Vui lòng nhập tên", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (string.IsNullOrWhiteSpace(player1))
                {
                    txtNhapTen1.Focus();
                }
                else
                {
                    txtNhapTen2.Focus();
                }
                return;
            }
            frmGame Game = new frmGame(player1, player2);
            ShowForm(Game);
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            AppState.PlayUiClickSound();
            this.Close();
        }
        private void TxtNhapTen1_Enter(object sender, EventArgs e)
        {
            if (txtNhapTen1.Text == "Người chơi 1")
            {
                txtNhapTen1.Clear();
            }
        }
        private void TxtNhapTen1_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNhapTen1.Text))
            {
                txtNhapTen1.Text = "Người chơi 1";
            }
        }

        private void TxtNhapTen2_Enter(object sender, EventArgs e)
        {
            if (txtNhapTen2.Text == "Người chơi 2")
            {
                txtNhapTen2.Clear();
            }
        }
        private void TxtNhapTen2_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNhapTen2.Text))
            {
                txtNhapTen2.Text = "Người chơi 2";
            }
        }

        private void ConfigureNameTextBoxes()
        {
            foreach (var textBox in new[] { txtNhapTen1, txtNhapTen2 })
            {
                textBox.BorderColor = Color.FromArgb(96, 51, 15);
                textBox.HoverState.BorderColor = Color.FromArgb(96, 51, 15);
                textBox.FocusedState.BorderColor = Color.FromArgb(96, 51, 15);
                textBox.FillColor = Color.FromArgb(229, 212, 166);
                textBox.ForeColor = Color.FromArgb(86, 48, 13);
            }
        }

        private void frmTwoPeople_FormClosing(object sender, FormClosingEventArgs e)
        {

        }
    }
}
