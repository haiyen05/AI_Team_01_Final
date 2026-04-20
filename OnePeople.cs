using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace AI
{
    public partial class frmOnePeople : Form
    {
        public frmOnePeople()
        {
            InitializeComponent();
            Load += FrmOnePeople_Load;
            btnStart.Click += BtnStart_Click;
            btnBack.Click += BtnBack_Click;
            txtNhapTen.Enter += TxtNhapTen_Enter;
            txtNhapTen.Leave += TxtNhapTen_Leave;
            btnEasy.Click += (_, __) => SelectDifficulty(GameDifficulty.Easy);
            btnMedium.Click += (_, __) => SelectDifficulty(GameDifficulty.Medium);
            btnHard.Click += (_, __) => SelectDifficulty(GameDifficulty.Hard);
            ConfigureNameTextBox();
            ConfigureDifficultyButtons();
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
        private void FrmOnePeople_Load(object sender, EventArgs e)
        {
            txtNhapTen.Text = "Người chơi 1";
            ConfigureNameTextBox();
            ConfigureDifficultyButtons();
            SelectDifficulty(AppState.Difficulty);
        }
        private void BtnStart_Click(object sender, EventArgs e)
        {
            AppState.PlayUiClickSound();
            string playerName = txtNhapTen.Text.Trim();
            if (string.IsNullOrWhiteSpace(playerName))
            {
                MessageBox.Show("Vui lòng nhập tên", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNhapTen.Focus();
                return;
            }
            frmGame Game = new frmGame(playerName);
            ShowForm(Game);
        }
        private void BtnBack_Click(object sender, EventArgs e)
        {
            AppState.PlayUiClickSound();
            this.Close();
        }
        private void TxtNhapTen_Enter(object sender, EventArgs e)
        {
            if (txtNhapTen.Text == "Người chơi 1")
            {
                txtNhapTen.Clear();
            }
        }
        private void TxtNhapTen_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNhapTen.Text))
            {
                txtNhapTen.Text = "Người chơi 1";
            }
        }

        private void SelectDifficulty(GameDifficulty difficulty)
        {
            if (AppState.Difficulty != difficulty)
            {
                AppState.PlayUiClickSound();
            }
            AppState.Difficulty = difficulty;
            StyleDifficultyButton(btnEasy, difficulty == GameDifficulty.Easy);
            StyleDifficultyButton(btnMedium, difficulty == GameDifficulty.Medium);
            StyleDifficultyButton(btnHard, difficulty == GameDifficulty.Hard);
        }

        private void ConfigureNameTextBox()
        {
            txtNhapTen.BorderColor = Color.FromArgb(96, 51, 15);
            txtNhapTen.HoverState.BorderColor = Color.FromArgb(96, 51, 15);
            txtNhapTen.FocusedState.BorderColor = Color.FromArgb(96, 51, 15);
            txtNhapTen.FillColor = Color.FromArgb(229, 212, 166);
            txtNhapTen.ForeColor = Color.FromArgb(86, 48, 13);
        }

        private void ConfigureDifficultyButtons()
        {
            foreach (var button in new[] { btnEasy, btnMedium, btnHard })
            {
                button.HoverState.BorderColor = Color.White;
                button.HoverState.FillColor = Color.FromArgb(181, 117, 49);
                button.HoverState.ForeColor = Color.White;
            }
        }

        private static void StyleDifficultyButton(Guna2Button button, bool selected)
        {
            button.FillColor = selected ? Color.FromArgb(149, 82, 30) : Color.Transparent;
            button.ForeColor = selected ? Color.White : Color.FromArgb(142, 100, 57);
            button.BorderColor = selected ? Color.White : Color.FromArgb(139, 69, 18);
            button.BorderThickness = selected ? 4 : 3;
        }

        private void frmOnePeople_FormClosing(object sender, FormClosingEventArgs e)
        {

        }
    }
}
