using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace AI
{
    public partial class frmGame : Form
    {
        private sealed class PitState
        {
            public int Dan { get; set; }
            public int Quan { get; set; }
        }

        private sealed class PlayerScore
        {
            public string Name { get; set; } = string.Empty;
            public int DanCaptured { get; set; }
            public int QuanCaptured { get; set; }
            public int BorrowedDan { get; set; }
            public int TotalPoints => DanCaptured + (QuanCaptured * 10);
        }

        private readonly string _player1Name;
        private readonly string _player2Name;
        private readonly bool _isOnePlayer;
        private readonly Random _random = new Random();

        private readonly Dictionary<int, Guna2Panel> _pitPanels = new Dictionary<int, Guna2Panel>();
        private readonly Dictionary<int, List<Guna2CircleButton>> _pitStoneButtons = new Dictionary<int, List<Guna2CircleButton>>();
        private readonly Dictionary<int, List<Point>> _pitStonePoints = new Dictionary<int, List<Point>>();
        private readonly Dictionary<int, List<Guna2CircleButton>> _storeDanButtons = new Dictionary<int, List<Guna2CircleButton>>();
        private readonly PitState[] _board = new PitState[12];
        private readonly PlayerScore[] _scores = new PlayerScore[2];

        private Button _btnLeft;
        private Button _btnRight;
        private int _selectedPitIndex = -1;
        private bool _isMuted;
        private bool _isPaused;
        private bool _isBusy;
        private bool _gameEnded;
        private int _currentPlayerIndex;
        private int _startingPlayerIndex;
        private int _secondsRemaining;

        private readonly System.Windows.Forms.Timer _turnTimer = new System.Windows.Forms.Timer();
        private readonly System.Windows.Forms.Timer _machineTimer = new System.Windows.Forms.Timer();

        public frmGame() : this("Người chơi 1", "Người chơi 2", 0, false)
        {
        }

        public frmGame(string playerName) : this(playerName, AppState.GetDifficultyText(), 0, true)
        {
        }

        public frmGame(string player1Name, string player2Name) : this(player1Name, player2Name, 0, false)
        {
        }

        public frmGame(string player1Name, string player2Name, int startingPlayerIndex, bool isOnePlayer = false)
        {
            InitializeComponent();
            _player1Name = string.IsNullOrWhiteSpace(player1Name) ? "Người chơi 1" : player1Name.Trim();
            _player2Name = string.IsNullOrWhiteSpace(player2Name) ? (isOnePlayer ? AppState.GetDifficultyText() : "Người chơi 2") : player2Name.Trim();
            _startingPlayerIndex = NormalizePlayerIndex(startingPlayerIndex);
            _currentPlayerIndex = _startingPlayerIndex;
            _isOnePlayer = isOnePlayer;
            _isMuted = AppState.IsMuted;
            _scores[0] = new PlayerScore { Name = _player1Name };
            _scores[1] = new PlayerScore { Name = _player2Name };

            for (int i = 0; i < _board.Length; i++)
            {
                _board[i] = new PitState();
            }

            _turnTimer.Interval = 1000;
            _turnTimer.Tick += TurnTimer_Tick;

            _machineTimer.Interval = 800;
            _machineTimer.Tick += MachineTimer_Tick;

            Load += Game_Load;
            FormClosed += FrmGame_FormClosed;
            btnHome.Click += BtnHome_Click;
            Resize += (_, __) => { RefreshPlayerHeaderLayouts(); CenterGamePanel(); };
        }

        public void ShowForm(Form f)
        {
            f.Size = Size;
            f.Location = Location;
            f.StartPosition = FormStartPosition.Manual;
            f.ShowDialog(this);
        }

        private void Game_Load(object sender, EventArgs e)
        {
            lblNguoiChoi.Text = _player1Name;
            lblMay.Text = _player2Name;

            BuildBoardControlMap();
            CreateDirectionButtons();
            ConfigureBoardColors();
            CreateStoreDanButtons();
            panelGame.Anchor = AnchorStyles.None;
            panelBoard.Anchor = AnchorStyles.None;
            ResetBoard();
            RenderAll();
            CenterGamePanel();
            StartTurn(_startingPlayerIndex);
        }

        private void CenterGamePanel()
        {
            panelGame.Left = Math.Max(0, (ClientSize.Width - panelGame.Width) / 2);
            panelGame.Top = Math.Max(0, (ClientSize.Height - panelGame.Height) / 2);
        }

        private void FrmGame_FormClosed(object sender, FormClosedEventArgs e)
        {
            _turnTimer.Stop();
            _machineTimer.Stop();
        }

        private void BuildBoardControlMap()
        {
            _pitPanels.Clear();
            _pitStoneButtons.Clear();
            _pitStonePoints.Clear();

            _pitPanels[0] = panelO0;
            _pitPanels[1] = panelO1;
            _pitPanels[2] = panelO2;
            _pitPanels[3] = panelO3;
            _pitPanels[4] = panelO4;
            _pitPanels[5] = panelO5;
            _pitPanels[6] = panelO6;
            _pitPanels[7] = panelO7;
            _pitPanels[8] = panelO8;
            _pitPanels[9] = panelO9;
            _pitPanels[10] = panelO10;
            _pitPanels[11] = panelO11;

            _pitStoneButtons[1] = new List<Guna2CircleButton> { btnDan26, btnDan27, btnDan28, btnDan29, btnDan30 };
            _pitStoneButtons[2] = new List<Guna2CircleButton> { btnDan31, btnDan32, btnDan33, btnDan34, btnDan35 };
            _pitStoneButtons[3] = new List<Guna2CircleButton> { btnDan36, btnDan37, btnDan38, btnDan39, btnDan40 };
            _pitStoneButtons[4] = new List<Guna2CircleButton> { btnDan41, btnDan42, btnDan43, btnDan44, btnDan45 };
            _pitStoneButtons[5] = new List<Guna2CircleButton> { btnDan46, btnDan47, btnDan48, btnDan49, btnDan50 };
            _pitStoneButtons[7] = new List<Guna2CircleButton> { btnDan21, btnDan22, btnDan23, btnDan24, btnDan25 };
            _pitStoneButtons[8] = new List<Guna2CircleButton> { btnDan16, btnDan17, btnDan18, btnDan19, btnDan20 };
            _pitStoneButtons[9] = new List<Guna2CircleButton> { btnDan11, btnDan12, btnDan13, btnDan14, btnDan15 };
            _pitStoneButtons[10] = new List<Guna2CircleButton> { btnDan6, btnDan7, btnDan8, btnDan9, btnDan10 };
            _pitStoneButtons[11] = new List<Guna2CircleButton> { btnDan1, btnDan2, btnDan3, btnDan4, btnDan5 };

            foreach (var kvp in _pitStoneButtons)
            {
                _pitStonePoints[kvp.Key] = kvp.Value.Select(x => x.Location).ToList();
            }

            foreach (var kvp in _pitPanels)
            {
                kvp.Value.Tag = kvp.Key;
                kvp.Value.Cursor = Cursors.Hand;
                kvp.Value.Click -= PitPanel_Click;
                kvp.Value.Click += PitPanel_Click;

                foreach (Control child in kvp.Value.Controls)
                {
                    child.Tag = kvp.Key;
                    child.Cursor = Cursors.Hand;
                    child.Click -= PitChild_Click;
                    child.Click += PitChild_Click;
                }
            }
        }

        private void ConfigureBoardColors()
        {
            foreach (var list in _pitStoneButtons.Values)
            {
                foreach (var btn in list)
                {
                    btn.FillColor = Color.Black;
                    btn.DisabledState.FillColor = Color.Black;
                    btn.DisabledState.BorderColor = Color.Black;
                    btn.DisabledState.CustomBorderColor = Color.Black;
                    btn.DisabledState.ForeColor = Color.White;
                    btn.Text = string.Empty;
                    btn.Enabled = false;
                }
            }

            btnQuan1.FillColor = Color.Black;
            btnQuan2.FillColor = Color.Black;
            btnQuan1.DisabledState.FillColor = Color.Black;
            btnQuan1.DisabledState.BorderColor = Color.Black;
            btnQuan1.DisabledState.CustomBorderColor = Color.Black;
            btnQuan1.DisabledState.ForeColor = Color.White;
            btnQuan2.DisabledState.FillColor = Color.Black;
            btnQuan2.DisabledState.BorderColor = Color.Black;
            btnQuan2.DisabledState.CustomBorderColor = Color.Black;
            btnQuan2.DisabledState.ForeColor = Color.White;
            btnQuan1.Text = "Quan";
            btnQuan2.Text = "Quan";
            btnQuan1.Enabled = false;
            btnQuan2.Enabled = false;
        }

        private void CreateStoreDanButtons()
        {
            _storeDanButtons.Clear();
            _storeDanButtons[0] = CreateStoreDanButtonsForPanel(panelO0);
            _storeDanButtons[6] = CreateStoreDanButtonsForPanel(panelO6);
        }

        private List<Guna2CircleButton> CreateStoreDanButtonsForPanel(Guna2Panel panel)
        {
            var existingButtons = panel.Controls.OfType<Guna2CircleButton>().Where(x => x != btnQuan1 && x != btnQuan2).ToList();
            foreach (var control in existingButtons)
            {
                panel.Controls.Remove(control);
                control.Dispose();
            }

            foreach (var oldLabel in panel.Controls.OfType<Label>().Where(x => x.Name.StartsWith("lblCount", StringComparison.Ordinal)).ToList())
            {
                panel.Controls.Remove(oldLabel);
                oldLabel.Dispose();
            }

            const int buttonSize = 22;
            const int gap = 5;
            const int maxButtons = 60;
            const int edgeMargin = 10;
            const int countReserve = 48;

            var quanButton = panel.Controls.OfType<Guna2CircleButton>().FirstOrDefault(x => x == btnQuan1 || x == btnQuan2);
            Rectangle avoidRect = quanButton != null
                ? new Rectangle(quanButton.Left - 10, quanButton.Top - 10, quanButton.Width + 20, quanButton.Height + 20)
                : Rectangle.Empty;

            Rectangle usableRect = new Rectangle(edgeMargin, edgeMargin, panel.Width - (edgeMargin * 2), panel.Height - edgeMargin - countReserve);
            var candidateCenters = new List<Point>();
            int radiusX = (panel.Width / 2) - edgeMargin;
            int radiusY = (panel.Height / 2) - edgeMargin;
            int centerX = panel.Width / 2;
            int centerY = panel.Height / 2;
            int buttonRadius = buttonSize / 2;

            bool IsInsideEllipse(int x, int y)
            {
                double dx = x - centerX;
                double dy = y - centerY;
                double value = ((dx * dx) / (double)(radiusX * radiusX)) + ((dy * dy) / (double)(radiusY * radiusY));
                return value <= 0.94d;
            }

            bool IsValidCenter(Point center)
            {
                if (!usableRect.Contains(center))
                {
                    return false;
                }

                if (avoidRect.Contains(center))
                {
                    return false;
                }

                var corners = new[]
                {
                    new Point(center.X - buttonRadius, center.Y - buttonRadius),
                    new Point(center.X + buttonRadius, center.Y - buttonRadius),
                    new Point(center.X - buttonRadius, center.Y + buttonRadius),
                    new Point(center.X + buttonRadius, center.Y + buttonRadius)
                };

                return corners.All(c => IsInsideEllipse(c.X, c.Y)) && !corners.Any(c => avoidRect.Contains(c));
            }

            for (int y = usableRect.Top + buttonRadius; y <= usableRect.Bottom - buttonRadius; y += buttonSize + gap)
            {
                for (int x = usableRect.Left + buttonRadius; x <= usableRect.Right - buttonRadius; x += buttonSize + gap)
                {
                    var center = new Point(x, y);
                    if (IsValidCenter(center))
                    {
                        candidateCenters.Add(center);
                    }
                }
            }

            candidateCenters = candidateCenters
                .OrderBy(c => Math.Abs(c.Y - centerY))
                .ThenBy(c => Math.Abs(c.X - centerX))
                .ThenBy(c => c.Y)
                .ThenBy(c => c.X)
                .ToList();

            var result = new List<Guna2CircleButton>();
            for (int i = 0; i < maxButtons; i++)
            {
                Point center = i < candidateCenters.Count
                    ? candidateCenters[i]
                    : new Point(centerX, Math.Min(usableRect.Bottom - buttonRadius, usableRect.Top + buttonRadius + ((i - candidateCenters.Count) * (buttonSize + gap))));

                var btn = new Guna2CircleButton
                {
                    Size = new Size(buttonSize, buttonSize),
                    Location = new Point(center.X - buttonRadius, center.Y - buttonRadius),
                    FillColor = Color.Black,
                    ForeColor = Color.White,
                    Text = string.Empty,
                    Enabled = false,
                    Visible = false,
                    Name = $"storeStone_{panel.Name}_{i}"
                };
                btn.ShadowDecoration.Mode = Guna.UI2.WinForms.Enums.ShadowMode.Circle;
                panel.Controls.Add(btn);
                result.Add(btn);
            }

            return result;
        }

        private void CreateDirectionButtons()
        {
            _btnLeft = new Button
            {
                Size = new Size(42, 42),
                Text = "◀",
                Font = new Font("Arial", 12F, FontStyle.Bold),
                BackColor = Color.FromArgb(149, 82, 30),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Visible = false
            };
            _btnLeft.FlatAppearance.BorderSize = 0;
            _btnLeft.Click += (_, __) => ExecuteSelectedMove(-1);

            _btnRight = new Button
            {
                Size = new Size(42, 42),
                Text = "▶",
                Font = new Font("Arial", 12F, FontStyle.Bold),
                BackColor = Color.FromArgb(149, 82, 30),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Visible = false
            };
            _btnRight.FlatAppearance.BorderSize = 0;
            _btnRight.Click += (_, __) => ExecuteSelectedMove(1);

            Controls.Add(_btnLeft);
            Controls.Add(_btnRight);
            _btnLeft.BringToFront();
            _btnRight.BringToFront();
        }

        private void ResetBoard()
        {
            for (int i = 0; i < _board.Length; i++)
            {
                _board[i].Dan = 0;
                _board[i].Quan = 0;
            }

            for (int i = 1; i <= 5; i++)
            {
                _board[i].Dan = 5;
            }

            for (int i = 7; i <= 11; i++)
            {
                _board[i].Dan = 5;
            }

            _board[0].Quan = 1;
            _board[6].Quan = 1;

            foreach (var score in _scores)
            {
                score.DanCaptured = 0;
                score.QuanCaptured = 0;
                score.BorrowedDan = 0;
            }

            _gameEnded = false;
            _isBusy = false;
            _isPaused = false;
            _selectedPitIndex = -1;
            HideDirectionButtons();
        }

        private void RenderAll()
        {
            RenderPitStates();
            UpdateScoreLabels();
            UpdateTurnIndicators();
            RefreshPlayerHeaderLayouts();
        }

        private void RenderPitStates()
        {
            foreach (var index in Enumerable.Range(1, 5).Concat(Enumerable.Range(7, 5)))
            {
                RenderSmallPit(index);
            }

            RenderStore(0, btnQuan2);
            RenderStore(6, btnQuan1);
        }

        private void RenderSmallPit(int pitIndex)
        {
            var buttons = _pitStoneButtons[pitIndex];
            var points = _pitStonePoints[pitIndex];
            var pit = _board[pitIndex];
            var panel = _pitPanels[pitIndex];

            int visibleCount = Math.Min(buttons.Count, pit.Dan);
            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].Visible = i < visibleCount;
                if (i < points.Count)
                {
                    buttons[i].Location = points[i];
                }
            }

            panel.FillColor = Color.FromArgb(241, 222, 190);
            panel.BorderColor = _selectedPitIndex == pitIndex
                ? Color.FromArgb(96, 51, 15)
                : Color.FromArgb(137, 66, 4);
            panel.BorderThickness = _selectedPitIndex == pitIndex ? 5 : 4;

            string text = pit.Dan.ToString();
            var countLabel = panel.Controls.OfType<Label>().FirstOrDefault();
            if (countLabel == null)
            {
                countLabel = new Label
                {
                    AutoSize = true,
                    Font = new Font("Arial", 10F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(86, 48, 13),
                    BackColor = Color.Transparent,
                    Name = $"lblCount{pitIndex}"
                };
                panel.Controls.Add(countLabel);
                countLabel.BringToFront();
            }

            countLabel.Text = text;
            countLabel.Location = new Point((panel.Width - countLabel.PreferredWidth) / 2, panel.Height - 38);
            countLabel.Click -= PitChild_Click;
            countLabel.Click += PitChild_Click;
            countLabel.Tag = pitIndex;
        }

        private void RenderStore(int pitIndex, Guna2CircleButton button)
        {
            var pit = _board[pitIndex];
            var panel = _pitPanels[pitIndex];

            button.Text = "Quan";
            button.Visible = pit.Quan > 0;
            button.FillColor = Color.Black;
            button.DisabledState.FillColor = Color.Black;
            button.DisabledState.BorderColor = Color.Black;
            button.DisabledState.CustomBorderColor = Color.Black;
            button.DisabledState.ForeColor = Color.White;
            button.Size = new Size(94, 94);

            if (_storeDanButtons.TryGetValue(pitIndex, out var buttons))
            {
                int visibleCount = Math.Min(buttons.Count, pit.Dan);
                for (int i = 0; i < buttons.Count; i++)
                {
                    buttons[i].Visible = i < visibleCount;
                    buttons[i].FillColor = Color.Black;
                    buttons[i].DisabledState.FillColor = Color.Black;
                    buttons[i].DisabledState.BorderColor = Color.Black;
                    buttons[i].DisabledState.CustomBorderColor = Color.Black;
                }
            }

            var countLabel = panel.Controls.OfType<Label>().FirstOrDefault(x => string.Equals(x.Name, $"lblCount{pitIndex}", StringComparison.Ordinal));
            if (countLabel == null)
            {
                countLabel = new Label
                {
                    AutoSize = true,
                    Font = new Font("Arial", 10F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(86, 48, 13),
                    BackColor = Color.Transparent,
                    Name = $"lblCount{pitIndex}"
                };
                panel.Controls.Add(countLabel);
            }

            int totalPitPoints = pit.Dan + (pit.Quan * 10);
            countLabel.Text = totalPitPoints.ToString();
            int labelY = panel.Height - 25;
            int labelX = pitIndex == 0 ? 26 : panel.Width - countLabel.PreferredWidth - 26;
            countLabel.Location = new Point(labelX, labelY);
            countLabel.Tag = pitIndex;
            countLabel.Click -= PitChild_Click;
            countLabel.Click += PitChild_Click;
            countLabel.BringToFront();
            button.BringToFront();
        }

        private void UpdateScoreLabels()
        {
            lblQuan1.Text = $"Quan: {_scores[0].QuanCaptured}";
            lblDan1.Text = $"Dân: {_scores[0].DanCaptured}";
            lblDiem1.Text = $"Điểm: {_scores[0].TotalPoints}";

            lblQuan.Text = $"Quan: {_scores[1].QuanCaptured}";
            lblDan.Text = $"Dân: {_scores[1].DanCaptured}";
            lblDiem.Text = $"Điểm: {_scores[1].TotalPoints}";
        }

        private void UpdateTurnIndicators()
        {
            bool player1Turn = _currentPlayerIndex == 0;
            lblSecond1.Text = $"{_secondsRemaining}s";
            lblSecond2.Text = $"{_secondsRemaining}s";
            guna2Panel5.Visible = player1Turn && !_gameEnded;
            lblTimer.Visible = !player1Turn && !_gameEnded;
        }

        private void RefreshPlayerHeaderLayouts()
        {
            CenterNameWithTimer(guna2Panel2, lblNguoiChoi, guna2Panel5, 8, 220, 315);
            CenterNameWithTimer(guna2Panel1, lblMay, lblTimer, 9, 169, 264);
        }

        private static void CenterNameWithTimer(Guna2Panel header, Guna2HtmlLabel nameLabel, Control timerPanel, int iconLeft, int timerMinLeft, int timerMaxLeft)
        {
            int availableLeft = iconLeft + 58;
            int desiredNameLeft = availableLeft;
            string originalText = nameLabel.Text ?? string.Empty;

            nameLabel.AutoSize = true;
            nameLabel.MaximumSize = new Size(320, 0);
            nameLabel.Text = originalText;
            nameLabel.Left = desiredNameLeft;
            nameLabel.Top = (header.Height - nameLabel.Height) / 2;

            int timerLeft = Math.Max(timerMinLeft, nameLabel.Right + 10);
            timerLeft = Math.Min(timerLeft, timerMaxLeft);
            timerPanel.Left = timerLeft;
            timerPanel.Top = (header.Height - timerPanel.Height) / 2;
        }

        private void StartTurn(int playerIndex)
        {
            if (_gameEnded)
            {
                return;
            }

            _currentPlayerIndex = NormalizePlayerIndex(playerIndex);

            if (!TryRefillSideIfEmpty(_currentPlayerIndex))
            {
                EndGame();
                return;
            }

            _secondsRemaining = 45;
            _selectedPitIndex = -1;
            HideDirectionButtons();
            UpdateTurnIndicators();
            RenderPitStates();

            _turnTimer.Stop();
            _machineTimer.Stop();

            if (_isOnePlayer && _currentPlayerIndex == 1)
            {
                _machineTimer.Start();
            }
            else
            {
                _turnTimer.Start();
            }
        }

        private bool TryRefillSideIfEmpty(int playerIndex)
        {
            var pits = playerIndex == 0 ? Enumerable.Range(1, 5).ToArray() : Enumerable.Range(7, 5).ToArray();
            if (pits.Any(x => _board[x].Dan > 0))
            {
                return true;
            }

            if (!HasAnyDanOnBoard())
            {
                return true;
            }

            int needed = 5;
            int own = Math.Min(needed, _scores[playerIndex].DanCaptured);
            _scores[playerIndex].DanCaptured -= own;
            needed -= own;

            if (needed > 0)
            {
                int otherPlayer = 1 - playerIndex;
                int borrowed = Math.Min(needed, _scores[otherPlayer].DanCaptured);
                _scores[otherPlayer].DanCaptured -= borrowed;
                _scores[playerIndex].BorrowedDan += borrowed;
                needed -= borrowed;
            }

            if (needed > 0)
            {
                return false;
            }

            foreach (var pit in pits)
            {
                _board[pit].Dan = 1;
            }

            RenderAll();
            Application.DoEvents();
            Thread.Sleep(180);
            return true;
        }

        private void TurnTimer_Tick(object sender, EventArgs e)
        {
            if (_isPaused || _gameEnded || _isBusy)
            {
                return;
            }

            _secondsRemaining = Math.Max(0, _secondsRemaining - 1);
            UpdateTurnIndicators();

            if (_secondsRemaining <= 0)
            {
                _turnTimer.Stop();
                AutoMoveForCurrentPlayer();
            }
        }

        private void MachineTimer_Tick(object sender, EventArgs e)
        {
            _machineTimer.Stop();
            if (_isPaused || _gameEnded || _isBusy || !(_isOnePlayer && _currentPlayerIndex == 1))
            {
                return;
            }

            AutoMoveForCurrentPlayer();
        }

        private void PitPanel_Click(object sender, EventArgs e)
        {
            if (sender is Control control && control.Tag is int pitIndex)
            {
                HandlePitSelection(pitIndex);
            }
        }

        private void PitChild_Click(object sender, EventArgs e)
        {
            if (sender is Control control && control.Tag is int pitIndex)
            {
                HandlePitSelection(pitIndex);
            }
        }

        private void HandlePitSelection(int pitIndex)
        {
            if (_gameEnded || _isPaused || _isBusy)
            {
                return;
            }

            if (_isOnePlayer && _currentPlayerIndex == 1)
            {
                return;
            }

            if (!IsSelectablePit(pitIndex) || _board[pitIndex].Dan <= 0)
            {
                return;
            }

            _selectedPitIndex = pitIndex;
            ShowDirectionButtonsForPit(pitIndex);
            RenderPitStates();
        }

        private bool IsSelectablePit(int pitIndex)
        {
            if (_currentPlayerIndex == 0)
            {
                return pitIndex >= 1 && pitIndex <= 5;
            }

            return pitIndex >= 7 && pitIndex <= 11;
        }

        private void ShowDirectionButtonsForPit(int pitIndex)
        {
            if (_btnLeft == null || _btnRight == null)
            {
                return;
            }

            var panel = _pitPanels[pitIndex];
            Point basePoint = panelBoard.PointToScreen(panel.Location);
            basePoint = PointToClient(basePoint);

            int y = Math.Max(90, basePoint.Y + (panel.Height / 2) - 20);
            _btnLeft.Location = new Point(Math.Max(10, basePoint.X - 50), y);
            _btnRight.Location = new Point(Math.Min(ClientSize.Width - 52, basePoint.X + panel.Width + 8), y);
            _btnLeft.Visible = true;
            _btnRight.Visible = true;
            _btnLeft.BringToFront();
            _btnRight.BringToFront();
        }

        private void HideDirectionButtons()
        {
            if (_btnLeft != null)
            {
                _btnLeft.Visible = false;
            }

            if (_btnRight != null)
            {
                _btnRight.Visible = false;
            }
        }

        private void ExecuteSelectedMove(int direction)
        {
            if (_selectedPitIndex < 0)
            {
                return;
            }

            int actualDirection = _currentPlayerIndex == 1 ? -direction : direction;
            ExecuteMove(_selectedPitIndex, actualDirection, _currentPlayerIndex);
        }

        private int EstimateOpponentBest(PitState[] board, int opponent)
        {
            int best = 0;

            var pits = opponent == 0
                ? Enumerable.Range(1, 5)
                : Enumerable.Range(7, 5);

            foreach (int pit in pits)
            {
                if (board[pit].Dan <= 0) continue;

                foreach (int dir in new[] { -1, 1 })
                {
                    var cloneBoard = CloneBoard(board);
                    var cloneScore = CloneScores(_scores);

                    int gain = SimulateMove(cloneBoard, cloneScore, pit, dir, opponent);
                    best = Math.Max(best, gain);
                }
            }

            return best;
        }

        private int EvaluateMoveBasic(int pitIndex, int direction)
        {
            if (_board[pitIndex].Dan <= 0)
                return int.MinValue;

            // clone để simulate
            var boardClone = CloneBoardState();
            var scoreClone = CloneScores();

            int gained = SimulateMove(boardClone, scoreClone, pitIndex, direction, _currentPlayerIndex);

            int score = gained;

            // ưu tiên ô nhiều quân (tránh chọn ô 1 quân)
            score += _board[pitIndex].Dan;

            // phạt nếu dễ bị đối thủ ăn lại
            int opponentRisk = EstimateOpponentBest(boardClone, 1 - _currentPlayerIndex);
            score -= opponentRisk;

            // phạt nếu phải vay
            if (WouldNeedRefillAfterMove(boardClone, _currentPlayerIndex))
                score -= 5;

            return score;
        }

        private void ChooseEasyMove(List<int> validPits, out int pitIndex, out int direction)
        {
            // 90% random
            if (_random.NextDouble() < 0.9)
            {
                pitIndex = validPits[_random.Next(validPits.Count)];
                direction = _random.Next(2) == 0 ? -1 : 1;
                return;
            }

            // 10% chọn nước "không quá ngu"
            int bestScore = int.MinValue;
            pitIndex = validPits[0];
            direction = 1;

            foreach (int pit in validPits)
            {
                foreach (int dir in new[] { -1, 1 })
                {
                    int score = EvaluateMoveBasic(pit, dir);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        pitIndex = pit;
                        direction = dir;
                    }
                }
            }
        }

        private void ChooseMediumMove(List<int> validPits, out int pitIndex, out int direction)
        {
            int bestScore = int.MinValue;
            pitIndex = validPits[0];
            direction = 1;

            foreach (int pit in validPits)
            {
                foreach (int dir in new[] { -1, 1 })
                {
                    var b = CloneBoardState();
                    var s = CloneScores();

                    SimulateMove(b, s, pit, dir, 1);

                    // minimax depth 1 (tổng depth = 2)
                    int score = Minimax(b, s, 1, false, int.MinValue, int.MaxValue);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        pitIndex = pit;
                        direction = dir;
                    }
                }
            }
        }

        private int QuickEvaluateMove(int pit, int dir)
        {
            if (_board[pit].Dan <= 0) return int.MinValue;

            var b = CloneBoardState();
            var s = CloneScores();

            int gain = SimulateMove(b, s, pit, dir, 1);
            return gain * 10 + _board[pit].Dan;
        }

        private void ChooseHardMove(List<int> validPits, out int pitIndex, out int direction)
        {
            int depth = 4;

            int bestScore = int.MinValue;
            pitIndex = validPits[0];
            direction = 1;

            var allMoves = new List<(int pit, int dir, int score)>();

            foreach (int pit in validPits)
            {
                foreach (int dir in new[] { -1, 1 })
                {
                    int quick = QuickEvaluateMove(pit, dir);
                    allMoves.Add((pit, dir, quick));
                }
            }

            // sort trước khi minimax
            var orderedMoves = allMoves.OrderByDescending(m => m.score);

            foreach (var move in orderedMoves)
            {
                var b = CloneBoardState();
                var s = CloneScores();

                SimulateMove(b, s, move.pit, move.dir, 1);

                int score = Minimax(b, s, depth - 1, false, int.MinValue, int.MaxValue);

                if (score > bestScore)
                {
                    bestScore = score;
                    pitIndex = move.pit;
                    direction = move.dir;
                }
            }
        }

        private void AutoMoveForCurrentPlayer()
        {
            List<int> validPits = GetPlayablePitsForCurrentPlayer();
            if (validPits.Count == 0)
            {
                HandleNoValidMove();
                return;
            }

            int pitIndex;
            int direction;

            if (_isOnePlayer && _currentPlayerIndex == 1)
            {
                ChooseMachineMove(validPits, out pitIndex, out direction);
            }
            else
            {

                ChooseEasyMove(validPits, out pitIndex, out direction);
            }

            ExecuteMove(pitIndex, direction, _currentPlayerIndex);
        }

        private int EvaluateBoard(PitState[] board, PlayerScore[] scores)
        {
            int my = scores[1].DanCaptured + scores[1].QuanCaptured * 10;
            int opp = scores[0].DanCaptured + scores[0].QuanCaptured * 10;

            int score = (my - opp) * 12;

            int myDan = Enumerable.Range(7, 5).Sum(i => board[i].Dan);
            int oppDan = Enumerable.Range(1, 5).Sum(i => board[i].Dan);

            score += (myDan - oppDan) * 3;

            // thưởng thế ăn dây chuyền
            if (Enumerable.Range(7, 5).Any(i => board[i].Dan == 0))
                score += 5;

            if (Enumerable.Range(1, 5).Any(i => board[i].Dan == 0))
                score -= 5;

            // tránh bị vét sạch
            if (!Enumerable.Range(7, 5).Any(i => board[i].Dan > 0))
                score -= 40;

            if (!Enumerable.Range(1, 5).Any(i => board[i].Dan > 0))
                score += 40;

            return score;
        }

        private PitState[] CloneBoard(PitState[] original)
        {
            PitState[] clone = new PitState[original.Length];
            for (int i = 0; i < original.Length; i++)
            {
                clone[i] = new PitState
                {
                    Dan = original[i].Dan,
                    Quan = original[i].Quan
                };
            }
            return clone;
        }

        private PlayerScore[] CloneScores(PlayerScore[] original)
        {
            return new[]
            {
        new PlayerScore
        {
            Name = original[0].Name,
            DanCaptured = original[0].DanCaptured,
            QuanCaptured = original[0].QuanCaptured,
            BorrowedDan = original[0].BorrowedDan
        },
        new PlayerScore
        {
            Name = original[1].Name,
            DanCaptured = original[1].DanCaptured,
            QuanCaptured = original[1].QuanCaptured,
            BorrowedDan = original[1].BorrowedDan
        }
    };
        }

        private List<int> GetPlayablePits(PitState[] board, int player)
        {
            if (player == 0)
                return Enumerable.Range(1, 5).Where(i => board[i].Dan > 0).ToList();
            else
                return Enumerable.Range(7, 5).Where(i => board[i].Dan > 0).ToList();
        }

        private bool IsGameOver(PitState[] board)
        {
            return (board[0].Dan == 0 && board[0].Quan == 0) &&
                   (board[6].Dan == 0 && board[6].Quan == 0);
        }

        private int Minimax(PitState[] board, PlayerScore[] scores, int depth, bool isMax, int alpha, int beta)
        {
            if (depth == 0 || IsGameOver(board))
                return EvaluateBoard(board, scores);

            int player = isMax ? 1 : 0;
            var moves = GetPlayablePits(board, player);

            if (moves.Count == 0)
                return EvaluateBoard(board, scores);

            if (isMax)
            {
                int maxEval = int.MinValue;

                foreach (int pit in moves)
                {
                    foreach (int dir in new[] { -1, 1 })
                    {
                        var b = CloneBoard(board);
                        var s = CloneScores(scores);

                        SimulateMove(b, s, pit, dir, player);

                        int eval = Minimax(b, s, depth - 1, false, alpha, beta);

                        maxEval = Math.Max(maxEval, eval);
                        alpha = Math.Max(alpha, eval);

                        if (beta <= alpha)
                            break;
                    }
                }

                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;

                foreach (int pit in moves)
                {
                    foreach (int dir in new[] { -1, 1 })
                    {
                        var b = CloneBoard(board);
                        var s = CloneScores(scores);

                        SimulateMove(b, s, pit, dir, player);

                        int eval = Minimax(b, s, depth - 1, true, alpha, beta);

                        minEval = Math.Min(minEval, eval);
                        beta = Math.Min(beta, eval);

                        if (beta <= alpha)
                            break;
                    }
                }

                return minEval;
            }
        }

        private void ChooseMachineMove(List<int> validPits, out int pitIndex, out int direction)
        {
            switch (AppState.Difficulty)
            {
                case GameDifficulty.Easy:
                    ChooseEasyMove(validPits, out pitIndex, out direction);
                    break;

                case GameDifficulty.Medium:
                    ChooseMediumMove(validPits, out pitIndex, out direction);
                    break;

                case GameDifficulty.Hard:
                    ChooseHardMove(validPits, out pitIndex, out direction);
                    break;

                default:
                    ChooseEasyMove(validPits, out pitIndex, out direction);
                    break;
            }
        }

        private PitState[] CloneBoardState()
        {
            PitState[] clone = new PitState[_board.Length];
            for (int i = 0; i < _board.Length; i++)
            {
                clone[i] = new PitState
                {
                    Dan = _board[i].Dan,
                    Quan = _board[i].Quan
                };
            }

            return clone;
        }

        private PlayerScore[] CloneScores()
        {
            return new[]
            {
                new PlayerScore
                {
                    Name = _scores[0].Name,
                    DanCaptured = _scores[0].DanCaptured,
                    QuanCaptured = _scores[0].QuanCaptured,
                    BorrowedDan = _scores[0].BorrowedDan
                },
                new PlayerScore
                {
                    Name = _scores[1].Name,
                    DanCaptured = _scores[1].DanCaptured,
                    QuanCaptured = _scores[1].QuanCaptured,
                    BorrowedDan = _scores[1].BorrowedDan
                }
            };
        }

        private int SimulateMove(PitState[] board, PlayerScore[] scores, int pitIndex, int direction, int actingPlayer)
        {
            if (pitIndex < 0 || pitIndex >= board.Length || IsQuanPit(pitIndex))
            {
                return 0;
            }

            int stones = board[pitIndex].Dan;
            if (stones <= 0)
            {
                return 0;
            }

            int capturedBefore = scores[actingPlayer].DanCaptured + (scores[actingPlayer].QuanCaptured * 10);
            board[pitIndex].Dan = 0;
            int current = pitIndex;

            while (true)
            {
                while (stones > 0)
                {
                    current = NormalizeIndex(current + direction);
                    board[current].Dan++;
                    stones--;
                }

                int nextPit = NormalizeIndex(current + direction);
                if (IsQuanPit(nextPit))
                {
                    break;
                }

                if (board[nextPit].Dan > 0)
                {
                    stones = board[nextPit].Dan;
                    board[nextPit].Dan = 0;
                    current = nextPit;
                    continue;
                }

                int capturePit = NormalizeIndex(nextPit + direction);
                while (board[nextPit].Dan == 0 && board[nextPit].Quan == 0
                    && (board[capturePit].Dan > 0 || board[capturePit].Quan > 0))
                {
                    scores[actingPlayer].DanCaptured += board[capturePit].Dan;
                    scores[actingPlayer].QuanCaptured += board[capturePit].Quan;
                    board[capturePit].Dan = 0;
                    board[capturePit].Quan = 0;

                    nextPit = NormalizeIndex(capturePit + direction);
                    capturePit = NormalizeIndex(nextPit + direction);
                }

                break;
            }

            int capturedAfter = scores[actingPlayer].DanCaptured + (scores[actingPlayer].QuanCaptured * 10);
            return capturedAfter - capturedBefore;
        }

        private bool WouldNeedRefillAfterMove(PitState[] board, int playerIndex)
        {
            IEnumerable<int> pits = playerIndex == 0 ? Enumerable.Range(1, 5) : Enumerable.Range(7, 5);
            return !pits.Any(i => board[i].Dan > 0);
        }

        private List<int> GetPlayablePitsForCurrentPlayer()
        {
            if (_currentPlayerIndex == 0)
            {
                return Enumerable.Range(1, 5).Where(x => _board[x].Dan > 0).ToList();
            }

            return Enumerable.Range(7, 5).Where(x => _board[x].Dan > 0).ToList();
        }

        private void HandleNoValidMove()
        {
            if (_gameEnded)
            {
                return;
            }

            if (ShouldEndGame())
            {
                EndGame();
                return;
            }

            StartTurn(1 - _currentPlayerIndex);
        }

        private void ExecuteMove(int pitIndex, int direction, int actingPlayer)
        {
            if (_gameEnded || _isBusy)
            {
                return;
            }

            _isBusy = true;
            _turnTimer.Stop();
            _machineTimer.Stop();
            HideDirectionButtons();
            _selectedPitIndex = -1;

            if (!IsSelectablePit(pitIndex))
            {
                _isBusy = false;
                StartTurn(actingPlayer);
                return;
            }

            int stones = _board[pitIndex].Dan;
            if (stones <= 0)
            {
                _isBusy = false;
                StartTurn(actingPlayer);
                return;
            }

            _board[pitIndex].Dan = 0;
            int current = pitIndex;
            RenderAll();
            Application.DoEvents();

            while (true)
            {
                while (stones > 0)
                {
                    current = NormalizeIndex(current + direction);
                    _board[current].Dan++;
                    stones--;
                    RenderPitStates();
                    Application.DoEvents();
                    Thread.Sleep(300);
                }

                int nextPit = GetNextPitIndex(current, direction);
                if (IsQuanPit(nextPit))
                {
                    break;
                }

                if (_board[nextPit].Dan > 0)
                {
                    stones = TakeAllMovableStones(nextPit);
                    current = nextPit;
                    RenderAll();
                    Application.DoEvents();
                    Thread.Sleep(300);
                    continue;
                }

                int capturePit = GetNextPitIndex(nextPit, direction);
                if (HasAnyStoneOrQuan(capturePit))
                {
                    HandleCaptureFromEmptyThenOccupied(nextPit, capturePit, direction, actingPlayer);
                }
                break;
            }

            RenderAll();

            if (ShouldEndGame())
            {
                EndGame();
                return;
            }

            _isBusy = false;
            StartTurn(1 - actingPlayer);
        }

        private void HandleCaptureFromEmptyThenOccupied(int emptyPitIndex, int capturePitIndex, int direction, int actingPlayer)
        {
            int checkEmpty = emptyPitIndex;
            int captureIndex = capturePitIndex;

            while (IsPitEmpty(checkEmpty) && HasAnyStoneOrQuan(captureIndex))
            {
                int danCaptured = _board[captureIndex].Dan;
                int quanCaptured = _board[captureIndex].Quan;
                _scores[actingPlayer].DanCaptured += danCaptured;
                _scores[actingPlayer].QuanCaptured += quanCaptured;

                _board[captureIndex].Dan = 0;
                _board[captureIndex].Quan = 0;

                PlayCaptureSound();
                RenderAll();
                Application.DoEvents();
                Thread.Sleep(150);

                checkEmpty = GetNextPitIndex(captureIndex, direction);
                captureIndex = GetNextPitIndex(checkEmpty, direction);
            }
        }

        private int GetNextPitIndex(int startIndex, int direction)
        {
            return NormalizeIndex(startIndex + direction);
        }

        private bool IsPitEmpty(int index)
        {
            return _board[index].Dan == 0 && _board[index].Quan == 0;
        }

        private bool HasAnyStoneOrQuan(int index)
        {
            return _board[index].Dan > 0 || _board[index].Quan > 0;
        }

        private bool ShouldEndGame()
        {
            return IsPitEmpty(0) && IsPitEmpty(6);
        }

        private void EndGame()
        {
            _gameEnded = true;
            _isBusy = false;
            _turnTimer.Stop();
            _machineTimer.Stop();
            HideDirectionButtons();

            for (int i = 1; i <= 5; i++)
            {
                _scores[0].DanCaptured += _board[i].Dan;
                _board[i].Dan = 0;
            }

            for (int i = 7; i <= 11; i++)
            {
                _scores[1].DanCaptured += _board[i].Dan;
                _board[i].Dan = 0;
            }

            if (_scores[0].BorrowedDan > 0)
            {
                int paidBack = Math.Min(_scores[0].BorrowedDan, _scores[0].DanCaptured);
                _scores[0].DanCaptured -= paidBack;
                _scores[1].DanCaptured += paidBack;
                _scores[0].BorrowedDan -= paidBack;
            }

            if (_scores[1].BorrowedDan > 0)
            {
                int paidBack = Math.Min(_scores[1].BorrowedDan, _scores[1].DanCaptured);
                _scores[1].DanCaptured -= paidBack;
                _scores[0].DanCaptured += paidBack;
                _scores[1].BorrowedDan -= paidBack;
            }

            UpdateScoreLabels();
            RenderPitStates();
            PlayWinSound();

            int winnerIndex = DetermineWinnerIndex();
            int replayStarter = winnerIndex;
            string resultText;
            if (_scores[0].TotalPoints == _scores[1].TotalPoints)
            {
                resultText = "Hòa!";
                replayStarter = _startingPlayerIndex;
            }
            else
            {
                resultText = $"{_scores[winnerIndex].Name} thắng!";
            }

            using frmResult resultForm = new frmResult(resultText);
            var result = resultForm.ShowDialog(this);

            if (result == DialogResult.Retry)
            {
                RestartGame(replayStarter);
                return;
            }

            if (result == DialogResult.Abort)
            {
                AppState.GoHome = true;
                if (Modal)
                {
                    DialogResult = DialogResult.Cancel;
                }
                Close();
                return;
            }

            Close();
        }

        private int DetermineWinnerIndex()
        {
            return _scores[0].TotalPoints >= _scores[1].TotalPoints ? 0 : 1;
        }

        private void RestartGame(int starterIndex)
        {
            frmGame newGame = new frmGame(_player1Name, _player2Name, starterIndex, _isOnePlayer)
            {
                Size = Size,
                Location = Location,
                StartPosition = FormStartPosition.Manual
            };

            Hide();
            newGame.ShowDialog(Owner ?? this);
            Close();
        }

        private void BtnHome_Click(object sender, EventArgs e)
        {
            AppState.PlayUiClickSound();
            if (_gameEnded)
            {
                return;
            }

            _isPaused = true;
            _turnTimer.Stop();
            _machineTimer.Stop();
            HideDirectionButtons();

            using frmPause pauseForm = new frmPause();
            pauseForm.SetSoundState(_isMuted);

            var result = pauseForm.ShowDialog(this);
            _isMuted = pauseForm.IsMuted;
            AppState.IsMuted = _isMuted;

            // Đồng bộ lại trạng thái âm thanh sau khi đóng Pause
            if (_isMuted)
            {
                SoundManager.StopBackgroundMusic();
            }
            else
            {
                SoundManager.EnsureBackgroundMusic();
            }

            if (result == DialogResult.Retry)
            {
                RestartGame(_startingPlayerIndex);
                return;
            }

            if (result == DialogResult.Abort)
            {
                // Đặt cờ về trang chủ, các form trung gian sẽ tự đóng
                AppState.GoHome = true;
                if (Modal)
                {
                    DialogResult = DialogResult.Cancel;
                }
                Close();
                return;
            }

            _isPaused = false;
            StartTurn(_currentPlayerIndex);
        }

        private int NormalizeIndex(int index)
        {
            int mod = index % 12;
            return mod < 0 ? mod + 12 : mod;
        }

        private int NormalizePlayerIndex(int index)
        {
            return index == 1 ? 1 : 0;
        }

        private bool IsQuanPit(int pitIndex)
        {
            return pitIndex == 0 || pitIndex == 6;
        }

        private bool HasAnyDanOnBoard()
        {
            return Enumerable.Range(0, 12).Any(i => _board[i].Dan > 0);
        }

        private int TakeAllMovableStones(int pitIndex)
        {
            if (IsQuanPit(pitIndex))
            {
                return 0;
            }

            int total = _board[pitIndex].Dan;
            _board[pitIndex].Dan = 0;
            return total;
        }

        private void PlayCaptureSound()
        {
            if (!_isMuted)
            {
                SoundManager.PlayEat();
            }
        }

        private void PlayWinSound()
        {
            if (!_isMuted)
            {
                SoundManager.PlayWin();
            }
        }

        private void frmGame_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }
    }
}