namespace YesiLdefter
{
    partial class main
    {
        /// <summary>
        ///Gerekli tasarımcı değişkeni.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///Kullanılan tüm kaynakları temizleyin.
        /// </summary>
        ///<param name="disposing">yönetilen kaynaklar dispose edilmeliyse doğru; aksi halde yanlış.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer üretilen kod

        /// <summary>
        /// Tasarımcı desteği için gerekli metot - bu metodun 
        ///içeriğini kod düzenleyici ile değiştirmeyin.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(main));
            this.timer_Kullaniciya_Mesaj_Var = new System.Windows.Forms.Timer(this.components);
            this.timer_Mesaj_Suresini_Bitir = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // timer_Kullaniciya_Mesaj_Var
            // 
            this.timer_Kullaniciya_Mesaj_Var.Tick += new System.EventHandler(this.timer_Kullaniciya_Mesaj_Var_Tick);
            // 
            // timer_Mesaj_Suresini_Bitir
            // 
            this.timer_Mesaj_Suresini_Bitir.Interval = 6000;
            this.timer_Mesaj_Suresini_Bitir.Tick += new System.EventHandler(this.timer_Mesaj_Suresini_Bitir_Tick);
            // 
            // main
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1455, 739);
            this.IconOptions.Icon = ((System.Drawing.Icon)(resources.GetObject("main.IconOptions.Icon")));
            this.Name = "main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "yesiLdefter";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.main_FormClosing);
            this.Load += new System.EventHandler(this.mainForm_Load);
            this.Shown += new System.EventHandler(this.mainForm_Shown);
            this.DpiChangedAfterParent += new System.EventHandler(this.main_DpiChangedAfterParent);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer timer_Kullaniciya_Mesaj_Var;
        private System.Windows.Forms.Timer timer_Mesaj_Suresini_Bitir;
    }
}

