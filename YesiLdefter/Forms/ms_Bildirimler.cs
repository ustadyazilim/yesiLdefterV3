using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tkn_Events;
using Tkn_SMS;
using Tkn_ToolBox;
using Tkn_Variable;
using YesiLdefter.Codes;

namespace YesiLdefter
{
    public partial class ms_Bildirimler : Form
    {
        #region Tanımlar
        tToolBox t = new tToolBox();

        string menuName = "MENU_" + "UST/PMS/CRS/Bildirimler";
        string buttonBildirimleriRaporla = "ButtonBildirimleriRaporla";
        string buttonSMSKontorSayisi = "ButtonSMSKontorSayisi";
        string buttonBildirimleriGonder = "ButtonBildirimleriGonder";
        string buttonWhatsAppGonder = "ButtonWhatsAppGonder";

        DataSet ds_bildirimListesiHeader = null;
        DataSet ds_bildirimListesiLines = null;
        DataNavigator dN_bildirimListesiHeader = null;
        DataNavigator dN_bildirimListesiLines = null;
        vSMSSettings settings = null;

        #endregion

        public ms_Bildirimler()
        {
            InitializeComponent();

            tEventsForm evf = new tEventsForm();

            this.Load += new System.EventHandler(evf.myForm_Load);
            this.Shown += new System.EventHandler(evf.myForm_Shown);
            this.Shown += new System.EventHandler(this.ms_Bildirimler_Shown);

            this.KeyPreview = true;

        }
        private void ms_Bildirimler_Shown(object sender, EventArgs e)
        {
            t.Find_Button_AddClick(this, menuName, buttonBildirimleriRaporla, myNavElementClick);
            t.Find_Button_AddClick(this, menuName, buttonSMSKontorSayisi, myNavElementClick);
            t.Find_Button_AddClick(this, menuName, buttonBildirimleriGonder, myNavElementClick);
            t.Find_Button_AddClick(this, menuName, buttonWhatsAppGonder, myNavElementClick);
        }
        private async void myNavElementClick(object sender, DevExpress.XtraBars.Navigation.NavElementEventArgs e)
        {
            if (sender.GetType().ToString() == "DevExpress.XtraBars.Navigation.NavButton")
            {
                if (((DevExpress.XtraBars.Navigation.NavButton)sender).Name == buttonBildirimleriRaporla)
                {
                    bildirimleriRaporla();
                }

                if (((DevExpress.XtraBars.Navigation.NavButton)sender).Name == buttonSMSKontorSayisi)
                {
                    smsKontorSayisi();
                }
                if (((DevExpress.XtraBars.Navigation.NavButton)sender).Name == buttonBildirimleriGonder)
                {
                    bildirimleriGonder();
                }
                if (((DevExpress.XtraBars.Navigation.NavButton)sender).Name == buttonWhatsAppGonder)
                {
                    bildirimleriWhatsAppGonder();
                }
                
            }

            if (sender.GetType().ToString() == "DevExpress.XtraBars.Navigation.TileNavItem")
            {
                //if (((DevExpress.XtraBars.Navigation.TileNavItem)sender).Name == buttonInsertPaketOlustur) InsertPaketOlustur();
            }
        }

        private bool preparingSmsKanalBilgileri(Form tForm, bildirimPaketi tBildirim_, ref vSMSSettings settings)
        {
            bool onay = true;

            tSMS sms = new tSMS();
            onay = sms.getSmsSettings(ref settings);

            if (onay == false) return onay;

            if (tBildirim_.bildirimListesiHeader == "")
            {
                /////// Bu bilgiler Manager.CrossSettings tablosundan okunacak 
                ///
                ///string BildirimListesiFormCode = "UST/PMS/CRS/SmsBildirimleri";
                string BildirimListesiHeader = "UST/PMS/CrsBildirimB.List_01";
                string BildirimListesiLines = "UST/PMS/CrsBildirimS.List_01";

                tBildirim_.bildirimListesiHeader = BildirimListesiHeader;
                tBildirim_.bildirimListesiLines = BildirimListesiLines;
            }

            string bildirimListesiHeader = tBildirim_.bildirimListesiHeader;
            string bildirimListesiLines = tBildirim_.bildirimListesiLines;


            if (this.ds_bildirimListesiHeader == null)
            {
                t.Find_DataSet(tForm, ref this.ds_bildirimListesiHeader, ref this.dN_bildirimListesiHeader, bildirimListesiHeader);
                t.Find_DataSet(tForm, ref this.ds_bildirimListesiLines, ref this.dN_bildirimListesiLines, bildirimListesiLines);
            }

            if (t.IsNotNull(this.ds_bildirimListesiHeader))
            {
                if (this.dN_bildirimListesiHeader.Position > -1)
                {
                    try
                    {
                        tBildirim_.secilenGonderimTypeId = t.myInt16(this.ds_bildirimListesiHeader.Tables[0].Rows[this.dN_bildirimListesiHeader.Position]["GonderimTypeId"].ToString());
                        tBildirim_.secilenGonderimTarihi = (DateTime)this.ds_bildirimListesiHeader.Tables[0].Rows[this.dN_bildirimListesiHeader.Position]["GonderimTarihi"];
                        tBildirim_.secilenGonderimSaati = this.ds_bildirimListesiHeader.Tables[0].Rows[this.dN_bildirimListesiHeader.Position]["GonderimSaati"].ToString();
                    }
                    catch (Exception)
                    {
                        //throw;
                    }
                }
            }


            return onay;
        }

        /// <summary>
        /// WhatsApp gönderimi için de aynı bildirim listesi header/lines setini hazırlar.
        /// SMS tarafındaki kısımlar (ayar okuma vb.) burada tekrar edilmez.
        /// </summary>
        private bool preparingWhatsAppKanalBilgileri(Form tForm, bildirimPaketi tBildirim_)
        {
            bool onay = true;

            if (tBildirim_.bildirimListesiHeader == "")
            {
                string BildirimListesiHeader = "UST/PMS/CrsBildirimB.List_01";
                string BildirimListesiLines = "UST/PMS/CrsBildirimS.List_01";

                tBildirim_.bildirimListesiHeader = BildirimListesiHeader;
                tBildirim_.bildirimListesiLines = BildirimListesiLines;
            }

            string bildirimListesiHeader = tBildirim_.bildirimListesiHeader;
            string bildirimListesiLines = tBildirim_.bildirimListesiLines;

            if (this.ds_bildirimListesiHeader == null)
            {
                t.Find_DataSet(tForm, ref this.ds_bildirimListesiHeader, ref this.dN_bildirimListesiHeader, bildirimListesiHeader);
                t.Find_DataSet(tForm, ref this.ds_bildirimListesiLines, ref this.dN_bildirimListesiLines, bildirimListesiLines);
            }

            return onay && t.IsNotNull(this.ds_bildirimListesiLines);
        }

        private void bildirimleriRaporla()
        {
            bool onay = preparingSmsKanalBilgileri(this, v.tBildirim, ref this.settings);

            if (onay == false) return;

            if (t.IsNotNull(ds_bildirimListesiLines))
            {
                tSMS sms = new tSMS();
                sms.bildirimleriSMSKanalindaSorgula(this, v.tBildirim, this.settings, this.ds_bildirimListesiLines, this.dN_bildirimListesiLines);
                t.AlertMessage("Bilgilendirme", "SMS raporlama işlemi tamamlandı...");
            }
        }

        private void smsKontorSayisi()
        {
            tSMS sms = new tSMS();

            bool onay = true;

            vSMSSettings settings = null;
            onay = sms.getSmsSettings(ref settings);

            if (onay == false) return;

            string KontorSayisi = sms.getSmsCredit(settings);

            t.FlyoutMessage(this, "SMS kalan kontör sayısı", KontorSayisi);

        }
 
        private void bildirimleriGonder()
        {
            bool onay = preparingSmsKanalBilgileri(this, v.tBildirim, ref this.settings);

            if (onay == false) return;

            if (t.IsNotNull(ds_bildirimListesiLines))
            {
                tSMS sms = new tSMS();
                onay = sms.bildirimleriSMSKanaliylaGonder_(this, v.tBildirim, this.ds_bildirimListesiLines, this.dN_bildirimListesiLines, this.settings);

                t.AlertMessage("Bilgilendirme", "SMS gönderme işlemi tamamlandı...");
            }
        }

        private void bildirimleriWhatsAppGonder()
        {
            // WhatsApp için sadece bildirim listelerini hazırla; kanal/ayar bilgisi Go API üzerinden JWT ile gelir.
            bool onay = preparingWhatsAppKanalBilgileri(this, v.tBildirim);

            if (onay == false) return;

            if (t.IsNotNull(ds_bildirimListesiLines))
            {
                tWhatsAppNotify wa = new tWhatsAppNotify();
                onay = wa.bildirimleriWhatsAppKanaliylaGonder_(this, v.tBildirim, this.ds_bildirimListesiLines, this.dN_bildirimListesiLines);
            }
        }
    }
}
