using System;
using System.Drawing;
using System.Windows.Forms;
using Tkn_Variable;

namespace YesiLdefter.Codes
{
    /// <summary>
    /// Simple status bar component for WhatsApp form that displays connection status, unread count, and last sync time
    /// </summary>
    public class WhatsAppStatusBar : Panel
    {
        private Label _statusLabel;
        private Label _unreadLabel;
        private Label _syncLabel;
        private Timer _updateTimer;

        public WhatsAppStatusBar()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Height = 25;
            this.Dock = DockStyle.Bottom;
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.BorderStyle = BorderStyle.FixedSingle;

            // Status label (left)
            _statusLabel = new Label
            {
                Text = "Durum: Bağlanıyor...",
                AutoSize = false,
                Dock = DockStyle.Left,
                Width = 200,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5, 0, 0, 0),
                ForeColor = Color.Gray
            };

            // Unread count label (center)
            _unreadLabel = new Label
            {
                Text = "Okunmamış: 0",
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Blue
            };

            // Sync time label (right)
            _syncLabel = new Label
            {
                Text = "Son güncelleme: -",
                AutoSize = false,
                Dock = DockStyle.Right,
                Width = 200,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 0, 5, 0),
                ForeColor = Color.Gray
            };

            this.Controls.Add(_statusLabel);
            this.Controls.Add(_unreadLabel);
            this.Controls.Add(_syncLabel);

            // Update timer for sync time
            _updateTimer = new Timer();
            _updateTimer.Interval = 1000; // 1 second
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            // This will be updated by the form
        }

        public void UpdateStatus(string status, Color? color = null)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateStatus(status, color)));
                return;
            }

            _statusLabel.Text = $"Durum: {status}";
            _statusLabel.ForeColor = color ?? GetStatusColor(status);
        }

        public void UpdateUnreadCount(int count)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<int>(UpdateUnreadCount), count);
                return;
            }

            _unreadLabel.Text = $"Okunmamış: {count}";
            _unreadLabel.ForeColor = count > 0 ? Color.Red : Color.Blue;
            _unreadLabel.Font = count > 0 ? new Font(_unreadLabel.Font, FontStyle.Bold) : new Font(_unreadLabel.Font, FontStyle.Regular);
        }

        public void UpdateLastSync(DateTime lastSync)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<DateTime>(UpdateLastSync), lastSync);
                return;
            }

            var elapsed = (DateTime.Now - lastSync).TotalSeconds;
            _syncLabel.Text = $"Son güncelleme: {elapsed:F0} saniye önce";
        }

        private Color GetStatusColor(string status)
        {
            switch (status?.ToUpper())
            {
                case "CONNECTED":
                    return Color.Green;
                case "NEEDS_QR":
                    return Color.Orange;
                case "CONNECTING":
                    return Color.Yellow;
                case "DISCONNECTED":
                    return Color.Red;
                default:
                    return Color.Gray;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _updateTimer?.Stop();
                _updateTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
