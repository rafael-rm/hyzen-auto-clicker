using System.Runtime.InteropServices;
using HyzenAutoClicker.Core;

namespace HyzenAutoClicker.WFA
{
    public class AutoClickerForm : Form
    {
        private readonly AutoClicker _autoClicker;
        private readonly TrackBar _cpsTrackBar;
        private readonly TrackBar _jitterTrackBar;
        private readonly Label _statusValueLabel;
        private readonly Label _cpsValueLabel;
        private readonly Label _jitterValueLabel;
        private readonly Button _changeHotkeyButton;
        private uint _currentHotkey = 0x77; // Bind F8
        private string _currentHotkeyText = "F8";
        private bool _isChangingHotkey;
        public sealed override Color BackColor { get => base.BackColor; set => base.BackColor = value; }
        public sealed override string Text { get => base.Text; set => base.Text = value; }
        
        private const int InitialCps = 10;
        private const int InitialJitterPercentage = 20;

        public AutoClickerForm()
        {
            _autoClicker = new AutoClicker(InitialCps, InitialJitterPercentage);
            
            Text = "Hyzen Auto Clicker";
            Size = new Size(350, 300);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.White;

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


            var statusLabel = new Label { Text = "Status", TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Arial", 12), Dock = DockStyle.Fill};
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
            
            _jitterTrackBar.ValueChanged += JitterTrackBar_ValueChanged;

            tableLayout.Controls.Add(jitterLabel, 0, 3);
            tableLayout.Controls.Add(_jitterTrackBar, 1, 3);
            tableLayout.Controls.Add(_jitterValueLabel, 2, 3);

            _changeHotkeyButton = new Button
            {
                Text = "Alterar Hotkey (Atualmente: F8)",
                Font = new Font("Arial", 10),
                BackColor = Color.Gray,
                Dock = DockStyle.Fill
            };
            
            _changeHotkeyButton.Click += ChangeHotkeyButton_Click;
            tableLayout.Controls.Add(_changeHotkeyButton, 0, 5);
            tableLayout.SetColumnSpan(_changeHotkeyButton, 3);
            
            Controls.Add(tableLayout);
            
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
        }

        protected override void WndProc(ref Message m)
        {
            const int wmHotkey = 0x0312;
            if (m.Msg == wmHotkey && m.WParam.ToInt32() == 1)
            {
                if (!_isChangingHotkey)
                    ToggleClicker(null, null);
            }
            base.WndProc(ref m);
        }

        private void ToggleClicker(object sender, EventArgs e)
        {
            if (_statusValueLabel.Text == "OFF")
            {
                _statusValueLabel.Text = "ON";
                _statusValueLabel.ForeColor = Color.Green;
                _autoClicker.Start(SimulateClick);
            }
            else
            {
                _statusValueLabel.Text = "OFF";
                _statusValueLabel.ForeColor = Color.Red;
                _autoClicker.Stop();
            }
        }

        private void SimulateClick()
        {
            mouse_event(0x0002 | 0x0004, 0, 0, 0, UIntPtr.Zero);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            UnregisterHotKey(Handle, 1);
            base.OnFormClosing(e);
        }
    }
}
