using System.Runtime.InteropServices;
using HyzenAutoClicker.Core;

namespace HyzenAutoClicker.WFA
{
    public class AutoClickerForm : Form
    {
        private const int InitialCps = 10;
        private const int InitialJitterPercentage = 20;
        private const bool SimulateHumanBehavior = true;
        private const int JitterWarningThreshold = 9; // To simulate human behavior, jitter must be at least 10%
        private const int WmHotkey = 0x0312;
        private const uint MouseLeftdown = 0x0002;
        private const uint MouseLeftup = 0x0004;

        private AutoClicker _autoClicker;
        private uint _currentHotkey;
        private string _currentHotkeyText;
        private bool _isChangingHotkey;

        private TrackBar _cpsTrackBar;
        private TrackBar _jitterTrackBar;
        private Label _statusValueLabel;
        private Label _cpsValueLabel;
        private Label _jitterValueLabel;
        private Button _changeHotkeyButton;
        private Label _jitterWarningLabel;

        public sealed override Color BackColor { get => base.BackColor; set => base.BackColor = value; }
        public sealed override string Text { get => base.Text; set => base.Text = value; }

        public AutoClickerForm()
        {
            InitializeDefaultValues();
            InitializeFormProperties();
            InitializeLayout();
            RegisterInitialHotkey();
        }

        private void InitializeDefaultValues()
        {
            _autoClicker = new AutoClicker(InitialCps, InitialJitterPercentage, SimulateHumanBehavior);
            _currentHotkey = 0x77; // F8 key
            _currentHotkeyText = "F8";
            _isChangingHotkey = false;
        }

        private void InitializeFormProperties()
        {
            Text = "Hyzen Auto Clicker";
            Size = new Size(400, 350);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.White;
        }

        private void InitializeLayout()
        {
            var tableLayout = CreateTableLayout();
            InitializeStatusSection(tableLayout);
            InitializeCpsSection(tableLayout);
            InitializeJitterSection(tableLayout);
            InitializeHumanBehaviorSection(tableLayout);
            InitializeHotkeyButton(tableLayout);
            Controls.Add(tableLayout);
        }

        private TableLayoutPanel CreateTableLayout()
        {
            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 6,
                ColumnCount = 3,
                Padding = new Padding(10),
                AutoSize = true,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };

            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));

            return tableLayout;
        }

        private void InitializeStatusSection(TableLayoutPanel tableLayout)
        {
            var statusLabel = new Label 
            { 
                Text = "Status", 
                TextAlign = ContentAlignment.MiddleCenter, 
                Font = new Font("Arial", 12), 
                Dock = DockStyle.Fill 
            };
            tableLayout.SetColumnSpan(statusLabel, 3);
            tableLayout.Controls.Add(statusLabel, 0, 0);

            _statusValueLabel = new Label
            {
                Text = "OFF",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.Red,
                Dock = DockStyle.Fill
            };
            tableLayout.SetColumnSpan(_statusValueLabel, 3);
            tableLayout.Controls.Add(_statusValueLabel, 0, 1);
        }

        private void InitializeCpsSection(TableLayoutPanel tableLayout)
        {
            var cpsLabel = new Label
            {
                Text = "CPS",
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Arial", 10)
            };

            _cpsTrackBar = new TrackBar
            {
                Minimum = 1,
                Maximum = 25,
                Value = InitialCps,
                TickFrequency = 1,
                LargeChange = 1,
                SmallChange = 1,
                Dock = DockStyle.Fill
            };

            _cpsValueLabel = new Label
            {
                Text = _cpsTrackBar.Value.ToString(),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Arial", 10)
            };

            _cpsTrackBar.ValueChanged += CpsTrackBar_ValueChanged;

            tableLayout.Controls.Add(cpsLabel, 0, 2);
            tableLayout.Controls.Add(_cpsTrackBar, 1, 2);
            tableLayout.Controls.Add(_cpsValueLabel, 2, 2);
        }

        private void InitializeJitterSection(TableLayoutPanel tableLayout)
        {
            var jitterLabel = new Label
            {
                Text = "Jitter",
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Arial", 10)
            };

            _jitterTrackBar = new TrackBar
            {
                Minimum = 0,
                Maximum = 50,
                Value = InitialJitterPercentage,
                TickFrequency = 1,
                LargeChange = 5,
                SmallChange = 1,
                Dock = DockStyle.Fill
            };

            _jitterValueLabel = new Label
            {
                Text = _jitterTrackBar.Value + "%",
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Arial", 10)
            };

            _jitterWarningLabel = new Label
            {
                Text = "Jitter reduzido. Possível imprecisão no comportamento.",
                ForeColor = Color.Red,
                Font = new Font("Arial", 10, FontStyle.Italic),
                Visible = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _jitterTrackBar.ValueChanged += JitterTrackBar_ValueChanged;

            tableLayout.Controls.Add(jitterLabel, 0, 3);
            tableLayout.Controls.Add(_jitterTrackBar, 1, 3);
            tableLayout.Controls.Add(_jitterValueLabel, 2, 3);
            tableLayout.Controls.Add(_jitterWarningLabel, 0, 5);
            tableLayout.SetColumnSpan(_jitterWarningLabel, 3);
        }

        private void InitializeHumanBehaviorSection(TableLayoutPanel tableLayout)
        {
            var simulateHumanCheckbox = new CheckBox
            {
                Text = "Simular comportamento humano",
                AutoSize = true,
                Checked = SimulateHumanBehavior,
                Dock = DockStyle.Fill
            };

            simulateHumanCheckbox.CheckedChanged += (_, _) =>
            {
                _autoClicker.SetSimulateHumanBehavior(simulateHumanCheckbox.Checked);
                _jitterWarningLabel.Visible = EnableJitterWarning();
            };

            tableLayout.Controls.Add(simulateHumanCheckbox, 0, 4);
            tableLayout.SetColumnSpan(simulateHumanCheckbox, 3);
        }

        private void InitializeHotkeyButton(TableLayoutPanel tableLayout)
        {
            _changeHotkeyButton = new Button
            {
                Text = $"Alterar Hotkey (Atualmente: {_currentHotkeyText})",
                Font = new Font("Arial", 10),
                BackColor = Color.Gray,
                Dock = DockStyle.Fill
            };

            _changeHotkeyButton.Click += ChangeHotkeyButton_Click;
            tableLayout.Controls.Add(_changeHotkeyButton, 0, 6);
            tableLayout.SetColumnSpan(_changeHotkeyButton, 3);
        }

        private void RegisterInitialHotkey()
        {
            RegisterHotKey(Handle, 1, 0x0000, _currentHotkey);
        }

        private void ChangeHotkeyButton_Click(object sender, EventArgs e)
        {
            _isChangingHotkey = true;
            _changeHotkeyButton.Text = "Pressione uma tecla...";
            _changeHotkeyButton.BackColor = Color.LightGreen;
            KeyPreview = true;
            KeyDown += CaptureNewHotkey;
        }

        private void CaptureNewHotkey(object sender, KeyEventArgs e)
        {
            UnregisterHotKey(Handle, 1);
            
            _currentHotkey = (uint)e.KeyCode;
            _currentHotkeyText = e.KeyCode.ToString();
            _changeHotkeyButton.Text = $"Alterar Hotkey (Atualmente: {_currentHotkeyText})";
            _changeHotkeyButton.BackColor = Color.LightBlue;
            RegisterHotKey(Handle, 1, 0x0000, _currentHotkey);
            KeyPreview = false;
            KeyDown -= CaptureNewHotkey;
            _isChangingHotkey = false;
        }

        private void CpsTrackBar_ValueChanged(object sender, EventArgs e)
        {
            var cps = _cpsTrackBar.Value;
            _autoClicker.SetCps(cps);
            _cpsValueLabel.Text = cps.ToString();
        }

        private void JitterTrackBar_ValueChanged(object sender, EventArgs e)
        {
            var jitterPercentage = _jitterTrackBar.Value;
            _autoClicker.SetJitter(jitterPercentage);
            _jitterValueLabel.Text = jitterPercentage + "%";
            
            _jitterWarningLabel.Visible = EnableJitterWarning();
        }

        private void ToggleClicker(object sender, EventArgs e)
        {
            bool isActive = _statusValueLabel.Text == "ON";
            _statusValueLabel.Text = isActive ? "OFF" : "ON";
            _statusValueLabel.ForeColor = isActive ? Color.Red : Color.Green;

            if (isActive)
                _autoClicker.Stop();
            else
                _autoClicker.Start(SimulateClick);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WmHotkey && m.WParam.ToInt32() == 1 && !_isChangingHotkey)
            {
                ToggleClicker(null, null);
            }
            base.WndProc(ref m);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            UnregisterHotKey(Handle, 1);
            base.OnFormClosing(e);
        }

        private void SimulateClick()
        {
            mouse_event(MouseLeftdown | MouseLeftup, 0, 0, 0, UIntPtr.Zero);
        }
        
        private bool EnableJitterWarning()
        {
            return _autoClicker.IsSimulatingHumanBehavior() && _jitterTrackBar.Value <= JitterWarningThreshold;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}