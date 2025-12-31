using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using Microsoft.AspNetCore.SignalR.Client;
using UstadDesktop.WhatsAppIntegration.WinFormsClient.Models;

namespace UstadDesktop.WhatsAppIntegration.WinFormsClient.Forms
{
    /// <summary>
    /// Main WhatsApp customer service interface
    /// Integrates with Ustad Desktop UI patterns and DevExpress components
    /// </summary>
    public partial class WhatsAppMainForm : DevExpress.XtraEditors.XtraForm
    {
        private readonly string _jwtToken;
        private readonly string _operatorName;
        private readonly string _role;
        private readonly int _operatorId;
        private readonly int _firmId;
        private readonly string _apiBaseUrl;
        
        private HubConnection _hubConnection;
        private List<ConversationViewModel> _conversations;
        private List<MessageViewModel> _currentMessages;
        private int? _selectedConversationId;
        private Timer _reconnectTimer;

        public WhatsAppMainForm(string jwtToken, string operatorName, string role, int operatorId, int firmId, string apiBaseUrl = "http://localhost:5000")
        {
            InitializeComponent();
            
            _jwtToken = jwtToken;
            _operatorName = operatorName;
            _role = role;
            _operatorId = operatorId;
            _firmId = firmId;
            _apiBaseUrl = apiBaseUrl;
            
            _conversations = new List<ConversationViewModel>();
            _currentMessages = new List<MessageViewModel>();
            
            InitializeUI();
            InitializeSignalR();
        }

        private void InitializeUI()
        {
            // Set form title with operator info
            this.Text = $"Ustad WhatsApp - {_operatorName} ({_role})";
            
            // Apply Ustad styling
            ApplyUstadStyling();
            
            // Configure grids
            ConfigureConversationsGrid();
            ConfigureMessagesGrid();
            
            // Set up data sources
            gridControlConversations.DataSource = _conversations;
            gridControlMessages.DataSource = _currentMessages;
            
            // Configure role-based UI
            ConfigureRoleBasedUI();
            
            // Set initial status
            lblStatus.Text = "Bağlanıyor...";
            lblOperatorInfo.Text = $"{_operatorName} - {_role}";
        }

        private void ApplyUstadStyling()
        {
            // Apply Ustad Desktop color scheme
            this.BackColor = ColorTranslator.FromHtml("#F5F7F9");
            
            // Configure send button
            btnSendMessage.Appearance.BackColor = ColorTranslator.FromHtml("#295C00");
            btnSendMessage.Appearance.ForeColor = Color.White;
            btnSendMessage.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            
            // Configure admin buttons
            if (_role == "Admin")
            {
                btnAdminPanel.Appearance.BackColor = ColorTranslator.FromHtml("#295C00");
                btnAdminPanel.Appearance.ForeColor = Color.White;
            }
            
            // Configure status colors
            lblStatus.Appearance.ForeColor = ColorTranslator.FromHtml("#6B7280");
            lblOperatorInfo.Appearance.ForeColor = ColorTranslator.FromHtml("#1F2937");
        }

        private void ConfigureConversationsGrid()
        {
            var view = gridViewConversations;
            view.OptionsView.ShowGroupPanel = false;
            view.OptionsView.ShowIndicator = false;
            view.OptionsSelection.EnableAppearanceFocusedCell = false;
            view.OptionsSelection.EnableAppearanceHideSelection = false;
            
            // Configure columns
            view.Columns.Clear();
            
            var colCustomer = view.Columns.AddField("CustomerNumber");
            colCustomer.Caption = "Müşteri";
            colCustomer.Width = 120;
            colCustomer.VisibleIndex = 0;
            
            var colName = view.Columns.AddField("CustomerName");
            colName.Caption = "İsim";
            colName.Width = 100;
            colName.VisibleIndex = 1;
            
            var colLastMessage = view.Columns.AddField("LastMessage");
            colLastMessage.Caption = "Son Mesaj";
            colLastMessage.Width = 200;
            colLastMessage.VisibleIndex = 2;
            
            var colStatus = view.Columns.AddField("Status");
            colStatus.Caption = "Durum";
            colStatus.Width = 80;
            colStatus.VisibleIndex = 3;
            
            var colAssigned = view.Columns.AddField("AssignedOperatorName");
            colAssigned.Caption = "Atanan";
            colAssigned.Width = 100;
            colAssigned.VisibleIndex = 4;
            
            var colUnread = view.Columns.AddField("UnreadCount");
            colUnread.Caption = "Okunmamış";
            colUnread.Width = 80;
            colUnread.VisibleIndex = 5;
            
            var colTime = view.Columns.AddField("LastMessageTime");
            colTime.Caption = "Zaman";
            colTime.Width = 120;
            colTime.VisibleIndex = 6;
            colTime.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            colTime.DisplayFormat.FormatString = "dd.MM.yyyy HH:mm";
            
            // Handle selection change
            view.FocusedRowChanged += GridViewConversations_FocusedRowChanged;
        }

        private void ConfigureMessagesGrid()
        {
            var view = gridViewMessages;
            view.OptionsView.ShowGroupPanel = false;
            view.OptionsView.ShowIndicator = false;
            view.OptionsSelection.EnableAppearanceFocusedCell = false;
            view.OptionsSelection.EnableAppearanceHideSelection = false;
            
            // Configure columns
            view.Columns.Clear();
            
            var colDirection = view.Columns.AddField("Direction");
            colDirection.Caption = "Yön";
            colDirection.Width = 60;
            colDirection.VisibleIndex = 0;
            
            var colMessage = view.Columns.AddField("Body");
            colMessage.Caption = "Mesaj";
            colMessage.Width = 300;
            colMessage.VisibleIndex = 1;
            
            var colOperator = view.Columns.AddField("OperatorName");
            colOperator.Caption = "Operatör";
            colOperator.Width = 100;
            colOperator.VisibleIndex = 2;
            
            var colTime = view.Columns.AddField("CreatedAt");
            colTime.Caption = "Zaman";
            colTime.Width = 120;
            colTime.VisibleIndex = 3;
            colTime.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            colTime.DisplayFormat.FormatString = "dd.MM.yyyy HH:mm:ss";
            
            // Custom row appearance for message direction
            view.RowStyle += (s, e) =>
            {
                if (e.RowHandle >= 0)
                {
                    var direction = view.GetRowCellValue(e.RowHandle, "Direction")?.ToString();
                    if (direction == "In")
                    {
                        e.Appearance.BackColor = ColorTranslator.FromHtml("#E5F3FF");
                    }
                    else if (direction == "Out")
                    {
                        e.Appearance.BackColor = ColorTranslator.FromHtml("#F0FDF4");
                    }
                }
            };
        }

        private void ConfigureRoleBasedUI()
        {
            // Show/hide admin features
            btnAdminPanel.Visible = (_role == "Admin");
            btnAssignConversation.Visible = (_role == "Admin");
            
            // Configure context menu based on role
            if (_role == "Admin")
            {
                // Add admin context menu items
                var contextMenu = new ContextMenuStrip();
                contextMenu.Items.Add("Konuşmayı Ata", null, (s, e) => AssignSelectedConversation());
                contextMenu.Items.Add("Durumu Değiştir", null, (s, e) => ChangeConversationStatus());
                contextMenu.Items.Add("-");
                contextMenu.Items.Add("Geçmişi Görüntüle", null, (s, e) => ViewConversationHistory());
                
                gridControlConversations.ContextMenuStrip = contextMenu;
            }
        }

        private async void InitializeSignalR()
        {
            try
            {
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl($"{_apiBaseUrl}/messagehub", options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(_jwtToken);
                    })
                    .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30) })
                    .Build();

                // Register event handlers
                RegisterSignalRHandlers();

                // Start connection
                await _hubConnection.StartAsync();
                
                lblStatus.Text = "Bağlı";
                lblStatus.Appearance.ForeColor = ColorTranslator.FromHtml("#10B981");
                
                // Load initial data
                await LoadConversations();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Bağlantı Hatası";
                lblStatus.Appearance.ForeColor = ColorTranslator.FromHtml("#EF4444");
                
                XtraMessageBox.Show(
                    $"SignalR bağlantısı kurulamadı:\n{ex.Message}",
                    "Bağlantı Hatası",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                
                StartReconnectTimer();
            }
        }

        private void RegisterSignalRHandlers()
        {
            // Customer message received
            _hubConnection.On<string, string, string, int>("CustomerMessageReceived", (customerNumber, message, messageType, conversationId) =>
            {
                this.Invoke(() =>
                {
                    HandleIncomingMessage(customerNumber, message, messageType, conversationId);
                });
            });

            // Operator message sent
            _hubConnection.On<object>("MessageSent", (messageData) =>
            {
                this.Invoke(() =>
                {
                    HandleMessageSent(messageData);
                });
            });

            // Conversation assigned
            _hubConnection.On<object>("ConversationAssigned", (assignmentData) =>
            {
                this.Invoke(() =>
                {
                    HandleConversationAssigned(assignmentData);
                });
            });

            // Operator status changed
            _hubConnection.On<string, string>("OperatorStatusChanged", (operatorName, status) =>
            {
                this.Invoke(() =>
                {
                    UpdateOperatorStatus(operatorName, status);
                });
            });

            // Connection events
            _hubConnection.Reconnecting += (error) =>
            {
                this.Invoke(() =>
                {
                    lblStatus.Text = "Yeniden bağlanıyor...";
                    lblStatus.Appearance.ForeColor = ColorTranslator.FromHtml("#F59E0B");
                });
                return Task.CompletedTask;
            };

            _hubConnection.Reconnected += (connectionId) =>
            {
                this.Invoke(() =>
                {
                    lblStatus.Text = "Bağlı";
                    lblStatus.Appearance.ForeColor = ColorTranslator.FromHtml("#10B981");
                });
                return Task.CompletedTask;
            };

            _hubConnection.Closed += (error) =>
            {
                this.Invoke(() =>
                {
                    lblStatus.Text = "Bağlantı Kesildi";
                    lblStatus.Appearance.ForeColor = ColorTranslator.FromHtml("#EF4444");
                    StartReconnectTimer();
                });
                return Task.CompletedTask;
            };
        }

        private void HandleIncomingMessage(string customerNumber, string message, string messageType, int conversationId)
        {
            // Update conversation list
            var conversation = _conversations.FirstOrDefault(c => c.Id == conversationId);
            if (conversation != null)
            {
                conversation.LastMessage = message;
                conversation.LastMessageTime = DateTime.Now;
                conversation.UnreadCount++;
            }
            else
            {
                // Add new conversation
                _conversations.Insert(0, new ConversationViewModel
                {
                    Id = conversationId,
                    CustomerNumber = customerNumber,
                    LastMessage = message,
                    LastMessageTime = DateTime.Now,
                    Status = "Open",
                    UnreadCount = 1
                });
            }

            // Update current conversation messages if this is the selected conversation
            if (_selectedConversationId == conversationId)
            {
                _currentMessages.Insert(0, new MessageViewModel
                {
                    Direction = "In",
                    MessageType = messageType,
                    Body = message,
                    CreatedAt = DateTime.Now,
                    IsRead = false
                });
                gridControlMessages.RefreshDataSource();
            }

            // Refresh conversations grid
            gridControlConversations.RefreshDataSource();
            
            // Show notification
            ShowMessageNotification(customerNumber, message);
        }

        private void HandleMessageSent(object messageData)
        {
            // Update conversation and message lists
            // Implementation depends on the exact structure of messageData
            gridControlConversations.RefreshDataSource();
            if (_selectedConversationId.HasValue)
            {
                gridControlMessages.RefreshDataSource();
            }
        }

        private void HandleConversationAssigned(object assignmentData)
        {
            // Update conversation assignment
            gridControlConversations.RefreshDataSource();
        }

        private void UpdateOperatorStatus(string operatorName, string status)
        {
            // Update operator status in UI (could be a status panel or notification)
            var statusMessage = status == "online" ? "çevrimiçi" : "çevrimdışı";
            // Could show in a status bar or notification area
        }

        private void ShowMessageNotification(string customerNumber, string message)
        {
            // Show balloon notification or toast
            var shortMessage = message.Length > 50 ? message.Substring(0, 50) + "..." : message;
            
            // Could use Windows notifications or a custom notification system
            // For now, just flash the taskbar
            this.WindowState = FormWindowState.Normal;
            this.Activate();
        }

        private async Task LoadConversations()
        {
            try
            {
                // This would typically load from API or be received via SignalR
                // For now, we'll wait for real-time updates
                await Task.Delay(100); // Placeholder
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Konuşmalar yüklenirken hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void GridViewConversations_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            if (e.FocusedRowHandle >= 0)
            {
                var conversation = _conversations[e.FocusedRowHandle];
                _selectedConversationId = conversation.Id;
                
                // Load conversation messages
                await LoadConversationMessages(conversation.Id);
                
                // Update UI
                txtCustomerNumber.Text = conversation.CustomerNumber;
                lblSelectedConversation.Text = $"Konuşma: {conversation.CustomerNumber}";
                
                // Enable message controls
                txtMessage.Enabled = true;
                btnSendMessage.Enabled = true;
            }
        }

        private async Task LoadConversationMessages(int conversationId)
        {
            try
            {
                if (_hubConnection?.State == HubConnectionState.Connected)
                {
                    await _hubConnection.InvokeAsync("GetConversationHistory", conversationId, 50, 0);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Mesajlar yüklenirken hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void btnSendMessage_Click(object sender, EventArgs e)
        {
            await SendMessage();
        }

        private async void txtMessage_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter && !string.IsNullOrWhiteSpace(txtMessage.Text))
            {
                e.Handled = true;
                await SendMessage();
            }
        }

        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(txtMessage.Text) || !_selectedConversationId.HasValue)
                return;

            var customerNumber = txtCustomerNumber.Text.Trim();
            var message = txtMessage.Text.Trim();

            try
            {
                if (_hubConnection?.State == HubConnectionState.Connected)
                {
                    await _hubConnection.InvokeAsync("SendMessage", customerNumber, message, "text");
                    txtMessage.Clear();
                    txtMessage.Focus();
                }
                else
                {
                    XtraMessageBox.Show("Bağlantı yok. Lütfen bekleyin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Mesaj gönderilirken hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAdminPanel_Click(object sender, EventArgs e)
        {
            if (_role == "Admin")
            {
                var adminForm = new AdminPanelForm(_jwtToken, _apiBaseUrl);
                adminForm.ShowDialog(this);
            }
        }

        private async void AssignSelectedConversation()
        {
            if (_selectedConversationId.HasValue && _role == "Admin")
            {
                // Show operator selection dialog
                var assignForm = new ConversationAssignmentForm(_jwtToken, _apiBaseUrl);
                if (assignForm.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        await _hubConnection.InvokeAsync("AssignConversation", _selectedConversationId.Value, assignForm.SelectedOperatorId);
                    }
                    catch (Exception ex)
                    {
                        XtraMessageBox.Show($"Atama hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async void ChangeConversationStatus()
        {
            if (_selectedConversationId.HasValue)
            {
                var statuses = new[] { "Open", "Closed", "Pending" };
                var selectedStatus = XtraInputBox.Show("Yeni durum seçin:", "Durum Değiştir", "Open", statuses);
                
                if (!string.IsNullOrEmpty(selectedStatus))
                {
                    try
                    {
                        await _hubConnection.InvokeAsync("UpdateConversationStatus", _selectedConversationId.Value, selectedStatus);
                    }
                    catch (Exception ex)
                    {
                        XtraMessageBox.Show($"Durum değiştirme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ViewConversationHistory()
        {
            if (_selectedConversationId.HasValue)
            {
                var historyForm = new ConversationHistoryForm(_selectedConversationId.Value, _jwtToken, _apiBaseUrl);
                historyForm.ShowDialog(this);
            }
        }

        private void StartReconnectTimer()
        {
            _reconnectTimer?.Stop();
            _reconnectTimer = new Timer();
            _reconnectTimer.Interval = 5000; // 5 seconds
            _reconnectTimer.Tick += async (s, e) =>
            {
                try
                {
                    if (_hubConnection?.State == HubConnectionState.Disconnected)
                    {
                        await _hubConnection.StartAsync();
                        _reconnectTimer.Stop();
                    }
                }
                catch
                {
                    // Continue trying
                }
            };
            _reconnectTimer.Start();
        }

        private async void WhatsAppMainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                _reconnectTimer?.Stop();
                if (_hubConnection != null)
                {
                    await _hubConnection.DisposeAsync();
                }
            }
            catch
            {
                // Ignore disposal errors
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _reconnectTimer?.Dispose();
                _hubConnection?.DisposeAsync();
            }
            base.Dispose(disposing);
        }
    }
}
