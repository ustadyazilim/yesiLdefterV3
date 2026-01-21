namespace YesiLdefter.Forms
{
    partial class ms_WhatsApp
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainerMain = new System.Windows.Forms.SplitContainer();
            this.panelLeft = new System.Windows.Forms.Panel();
            this.panelSearch = new System.Windows.Forms.Panel();
            this.checkBoxFilterUnread = new System.Windows.Forms.CheckBox();
            this.textBoxSearch = new System.Windows.Forms.TextBox();
            this.labelSearch = new System.Windows.Forms.Label();
            this.dataGridViewConversations = new System.Windows.Forms.DataGridView();
            this.panelRight = new System.Windows.Forms.Panel();
            this.panelMessageInput = new System.Windows.Forms.Panel();
            this.buttonSend = new System.Windows.Forms.Button();
            this.textBoxMessageInput = new System.Windows.Forms.TextBox();
            this.labelMessageInput = new System.Windows.Forms.Label();
            this.panelThread = new System.Windows.Forms.Panel();
            this.richTextBoxMessages = new System.Windows.Forms.RichTextBox();
            this.labelThreadHeader = new System.Windows.Forms.Label();
            this.panelTop = new System.Windows.Forms.Panel();
            this.comboBoxEnvironment = new System.Windows.Forms.ComboBox();
            this.labelEnvironment = new System.Windows.Forms.Label();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.textBoxNewPhone = new System.Windows.Forms.TextBox();
            this.labelNewPhone = new System.Windows.Forms.Label();
            this.buttonNewChat = new System.Windows.Forms.Button();
            this.whatsAppStatusBar = new YesiLdefter.Codes.WhatsAppStatusBar();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
            this.splitContainerMain.Panel1.SuspendLayout();
            this.splitContainerMain.Panel2.SuspendLayout();
            this.splitContainerMain.SuspendLayout();
            this.panelLeft.SuspendLayout();
            this.panelSearch.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewConversations)).BeginInit();
            this.panelRight.SuspendLayout();
            this.panelMessageInput.SuspendLayout();
            this.panelThread.SuspendLayout();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerMain
            // 
            this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerMain.Location = new System.Drawing.Point(0, 50);
            this.splitContainerMain.Name = "splitContainerMain";
            // 
            // splitContainerMain.Panel1
            // 
            this.splitContainerMain.Panel1.Controls.Add(this.panelLeft);
            // 
            // splitContainerMain.Panel2
            // 
            this.splitContainerMain.Panel2.Controls.Add(this.panelRight);
            this.splitContainerMain.Size = new System.Drawing.Size(1200, 650);
            this.splitContainerMain.SplitterDistance = 400;
            this.splitContainerMain.TabIndex = 0;
            // 
            // panelLeft
            // 
            this.panelLeft.Controls.Add(this.panelSearch);
            this.panelLeft.Controls.Add(this.dataGridViewConversations);
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLeft.Location = new System.Drawing.Point(0, 0);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Padding = new System.Windows.Forms.Padding(5);
            this.panelLeft.Size = new System.Drawing.Size(400, 650);
            this.panelLeft.TabIndex = 0;
            // 
            // panelSearch
            // 
            this.panelSearch.Controls.Add(this.checkBoxFilterUnread);
            this.panelSearch.Controls.Add(this.textBoxSearch);
            this.panelSearch.Controls.Add(this.labelSearch);
            this.panelSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelSearch.Location = new System.Drawing.Point(5, 5);
            this.panelSearch.Name = "panelSearch";
            this.panelSearch.Size = new System.Drawing.Size(390, 60);
            this.panelSearch.TabIndex = 1;
            // 
            // checkBoxFilterUnread
            // 
            this.checkBoxFilterUnread.AutoSize = true;
            this.checkBoxFilterUnread.Location = new System.Drawing.Point(3, 35);
            this.checkBoxFilterUnread.Name = "checkBoxFilterUnread";
            this.checkBoxFilterUnread.Size = new System.Drawing.Size(120, 17);
            this.checkBoxFilterUnread.TabIndex = 2;
            this.checkBoxFilterUnread.Text = "Sadece Okunmamış";
            this.checkBoxFilterUnread.UseVisualStyleBackColor = true;
            // 
            // textBoxSearch
            // 
            this.textBoxSearch.Location = new System.Drawing.Point(50, 8);
            this.textBoxSearch.Name = "textBoxSearch";
            this.textBoxSearch.Size = new System.Drawing.Size(337, 20);
            this.textBoxSearch.TabIndex = 1;
            // 
            // labelSearch
            // 
            this.labelSearch.AutoSize = true;
            this.labelSearch.Location = new System.Drawing.Point(3, 11);
            this.labelSearch.Name = "labelSearch";
            this.labelSearch.Size = new System.Drawing.Size(41, 13);
            this.labelSearch.TabIndex = 0;
            this.labelSearch.Text = "Ara:";
            // 
            // dataGridViewConversations
            // 
            this.dataGridViewConversations.AllowUserToAddRows = false;
            this.dataGridViewConversations.AllowUserToDeleteRows = false;
            this.dataGridViewConversations.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.None;
            this.dataGridViewConversations.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewConversations.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewConversations.Location = new System.Drawing.Point(5, 65);
            this.dataGridViewConversations.MultiSelect = false;
            this.dataGridViewConversations.Name = "dataGridViewConversations";
            this.dataGridViewConversations.ReadOnly = true;
            this.dataGridViewConversations.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewConversations.Size = new System.Drawing.Size(390, 580);
            this.dataGridViewConversations.TabIndex = 0;
            // 
            // panelRight
            // 
            this.panelRight.Controls.Add(this.panelMessageInput);
            this.panelRight.Controls.Add(this.panelThread);
            this.panelRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRight.Location = new System.Drawing.Point(0, 0);
            this.panelRight.Name = "panelRight";
            this.panelRight.Padding = new System.Windows.Forms.Padding(5);
            this.panelRight.Size = new System.Drawing.Size(796, 650);
            this.panelRight.TabIndex = 0;
            // 
            // panelMessageInput
            // 
            this.panelMessageInput.Controls.Add(this.buttonSend);
            this.panelMessageInput.Controls.Add(this.textBoxMessageInput);
            this.panelMessageInput.Controls.Add(this.labelMessageInput);
            this.panelMessageInput.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelMessageInput.Location = new System.Drawing.Point(5, 570);
            this.panelMessageInput.Name = "panelMessageInput";
            this.panelMessageInput.Size = new System.Drawing.Size(786, 75);
            this.panelMessageInput.TabIndex = 1;
            // 
            // buttonSend
            // 
            this.buttonSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSend.Location = new System.Drawing.Point(708, 45);
            this.buttonSend.Name = "buttonSend";
            this.buttonSend.Size = new System.Drawing.Size(75, 25);
            this.buttonSend.TabIndex = 2;
            this.buttonSend.Text = "Gönder";
            this.buttonSend.UseVisualStyleBackColor = true;
            // 
            // textBoxMessageInput
            // 
            this.textBoxMessageInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxMessageInput.Location = new System.Drawing.Point(3, 25);
            this.textBoxMessageInput.Multiline = true;
            this.textBoxMessageInput.Name = "textBoxMessageInput";
            this.textBoxMessageInput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxMessageInput.Size = new System.Drawing.Size(699, 45);
            this.textBoxMessageInput.TabIndex = 1;
            // 
            // labelMessageInput
            // 
            this.labelMessageInput.AutoSize = true;
            this.labelMessageInput.Location = new System.Drawing.Point(3, 9);
            this.labelMessageInput.Name = "labelMessageInput";
            this.labelMessageInput.Size = new System.Drawing.Size(45, 13);
            this.labelMessageInput.TabIndex = 0;
            this.labelMessageInput.Text = "Mesaj:";
            // 
            // panelThread
            // 
            this.panelThread.Controls.Add(this.richTextBoxMessages);
            this.panelThread.Controls.Add(this.labelThreadHeader);
            this.panelThread.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelThread.Location = new System.Drawing.Point(5, 5);
            this.panelThread.Name = "panelThread";
            this.panelThread.Size = new System.Drawing.Size(786, 565);
            this.panelThread.TabIndex = 0;
            // 
            // richTextBoxMessages
            // 
            this.richTextBoxMessages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxMessages.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.richTextBoxMessages.Location = new System.Drawing.Point(0, 25);
            this.richTextBoxMessages.Name = "richTextBoxMessages";
            this.richTextBoxMessages.ReadOnly = true;
            this.richTextBoxMessages.Size = new System.Drawing.Size(786, 540);
            this.richTextBoxMessages.TabIndex = 1;
            this.richTextBoxMessages.Text = "";
            // 
            // labelThreadHeader
            // 
            this.labelThreadHeader.AutoSize = true;
            this.labelThreadHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelThreadHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.labelThreadHeader.Location = new System.Drawing.Point(0, 0);
            this.labelThreadHeader.Name = "labelThreadHeader";
            this.labelThreadHeader.Padding = new System.Windows.Forms.Padding(5);
            this.labelThreadHeader.Size = new System.Drawing.Size(99, 25);
            this.labelThreadHeader.TabIndex = 0;
            this.labelThreadHeader.Text = "Konuşma Seçin";
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.buttonNewChat);
            this.panelTop.Controls.Add(this.textBoxNewPhone);
            this.panelTop.Controls.Add(this.labelNewPhone);
            this.panelTop.Controls.Add(this.comboBoxEnvironment);
            this.panelTop.Controls.Add(this.labelEnvironment);
            this.panelTop.Controls.Add(this.buttonRefresh);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1200, 50);
            this.panelTop.TabIndex = 1;
            // 
            // comboBoxEnvironment
            // 
            this.comboBoxEnvironment.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxEnvironment.FormattingEnabled = true;
            this.comboBoxEnvironment.Items.AddRange(new object[] {
            "Development (localhost:8080)",
            "Production (143.198.228.153:8080)"});
            this.comboBoxEnvironment.Location = new System.Drawing.Point(100, 15);
            this.comboBoxEnvironment.Name = "comboBoxEnvironment";
            this.comboBoxEnvironment.Size = new System.Drawing.Size(250, 21);
            this.comboBoxEnvironment.TabIndex = 2;
            // 
            // labelEnvironment
            // 
            this.labelEnvironment.AutoSize = true;
            this.labelEnvironment.Location = new System.Drawing.Point(12, 18);
            this.labelEnvironment.Name = "labelEnvironment";
            this.labelEnvironment.Size = new System.Drawing.Size(82, 13);
            this.labelEnvironment.TabIndex = 1;
            this.labelEnvironment.Text = "API Ortamı:";
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRefresh.Location = new System.Drawing.Point(1113, 12);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(75, 25);
            this.buttonRefresh.TabIndex = 0;
            this.buttonRefresh.Text = "Yenile";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            // 
            // textBoxNewPhone
            // 
            this.textBoxNewPhone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxNewPhone.Location = new System.Drawing.Point(740, 15);
            this.textBoxNewPhone.Name = "textBoxNewPhone";
            this.textBoxNewPhone.Size = new System.Drawing.Size(180, 20);
            this.textBoxNewPhone.TabIndex = 3;
            // 
            // labelNewPhone
            // 
            this.labelNewPhone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelNewPhone.AutoSize = true;
            this.labelNewPhone.Location = new System.Drawing.Point(620, 18);
            this.labelNewPhone.Name = "labelNewPhone";
            this.labelNewPhone.Size = new System.Drawing.Size(114, 13);
            this.labelNewPhone.TabIndex = 4;
            this.labelNewPhone.Text = "Yeni Sohbet Telefonu:";
            // 
            // buttonNewChat
            // 
            this.buttonNewChat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonNewChat.Location = new System.Drawing.Point(926, 12);
            this.buttonNewChat.Name = "buttonNewChat";
            this.buttonNewChat.Size = new System.Drawing.Size(90, 25);
            this.buttonNewChat.TabIndex = 5;
            this.buttonNewChat.Text = "Yeni Sohbet";
            this.buttonNewChat.UseVisualStyleBackColor = true;
            // 
            // whatsAppStatusBar
            // 
            this.whatsAppStatusBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.whatsAppStatusBar.Name = "whatsAppStatusBar";
            // 
            // ms_WhatsApp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 722);
            this.Controls.Add(this.splitContainerMain);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.whatsAppStatusBar);
            this.Name = "ms_WhatsApp";
            this.Text = "WhatsApp Mesajlaşma";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
            this.splitContainerMain.Panel1.ResumeLayout(false);
            this.splitContainerMain.Panel2.ResumeLayout(false);
            this.splitContainerMain.ResumeLayout(false);
            this.panelLeft.ResumeLayout(false);
            this.panelSearch.ResumeLayout(false);
            this.panelSearch.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewConversations)).EndInit();
            this.panelRight.ResumeLayout(false);
            this.panelMessageInput.ResumeLayout(false);
            this.panelMessageInput.PerformLayout();
            this.panelThread.ResumeLayout(false);
            this.panelThread.PerformLayout();
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.Panel panelSearch;
        private System.Windows.Forms.CheckBox checkBoxFilterUnread;
        private System.Windows.Forms.TextBox textBoxSearch;
        private System.Windows.Forms.Label labelSearch;
        private System.Windows.Forms.DataGridView dataGridViewConversations;
        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.Panel panelMessageInput;
        private System.Windows.Forms.Button buttonSend;
        private System.Windows.Forms.TextBox textBoxMessageInput;
        private System.Windows.Forms.Label labelMessageInput;
        private System.Windows.Forms.Panel panelThread;
        private System.Windows.Forms.RichTextBox richTextBoxMessages;
        private System.Windows.Forms.Label labelThreadHeader;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.ComboBox comboBoxEnvironment;
        private System.Windows.Forms.Label labelEnvironment;
        private System.Windows.Forms.Button buttonRefresh;
        private System.Windows.Forms.TextBox textBoxNewPhone;
        private System.Windows.Forms.Label labelNewPhone;
        private System.Windows.Forms.Button buttonNewChat;
        private YesiLdefter.Codes.WhatsAppStatusBar whatsAppStatusBar;
    }
}
