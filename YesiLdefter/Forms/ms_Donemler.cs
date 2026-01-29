using DevExpress.XtraCharts;
using DevExpress.XtraEditors;
using DevExpress.XtraReports.Native.Templates;
using Microsoft.JScript;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Linq.Mapping;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tkn_ToolBox;
using Tkn_Variable;

namespace YesiLdefter
{
    public partial class ms_Donemler : Form
    {
        tToolBox t = new tToolBox();

        DataSet ds_KursiyerList = null;
        DataNavigator dN_KursiyerList = null;

        string TableIPCode = string.Empty;
        string menuName = "MENU_" + "UST/MEB/SRC/Donemler";
        string buttonESrc = "ButtonESrcEntegrasyonu";
        string buttonWhatsApp = "ButtonWhatsApp";

        public ms_Donemler()
        {
            InitializeComponent();
        }

        private void ms_Donemler_Shown(object sender, EventArgs e)
        {
            // Create WhatsApp button programmatically if it doesn't exist
            CreateWhatsAppButtonIfNeeded();
            
            // Try to attach click handlers
            t.Find_Button_AddClick(this, menuName, buttonESrc, myNavElementClick);
            t.Find_Button_AddClick(this, menuName, buttonWhatsApp, myNavElementClick);

            if (ds_KursiyerList == null)
            {
                if (v.SP_Firm_SectorTypeId == 201) // Mtsk
                    TableIPCode = "UST/MEB/MtskAdayTalep.DonemlerTumListesi";

                if (v.SP_Firm_SectorTypeId == 203) // Src
                    TableIPCode = "UST/MEB/SrcAdayTalep.DonemlerDonemListesi";

                if (v.SP_Firm_SectorTypeId == 204) // Src5
                    TableIPCode = "UST/MEB/SrcAdayTalep.DonemlerDonemListesiSrc5";

                t.Find_DataSet(this, ref ds_KursiyerList, ref dN_KursiyerList, TableIPCode);
            }
        }

        private void CreateWhatsAppButtonIfNeeded()
        {
            try
            {
                string[] controls = new string[] { };
                Control menuControl = t.Find_Control(this, menuName, "", controls);
                
                if (menuControl != null && menuControl.GetType().ToString() == "DevExpress.XtraBars.Navigation.TileNavPane")
                {
                    var tileNavPane = menuControl as DevExpress.XtraBars.Navigation.TileNavPane;
                    
                    // Check if button already exists
                    bool buttonExists = false;
                    for (int i = 0; i < tileNavPane.Buttons.Count; i++)
                    {
                        if (tileNavPane.Buttons[i].Element.Name == buttonWhatsApp)
                        {
                            buttonExists = true;
                            break;
                        }
                    }
                    
                    // Create button if it doesn't exist
                    if (!buttonExists)
                    {
                        var whatsAppButton = new DevExpress.XtraBars.Navigation.NavButton
                        {
                            Name = buttonWhatsApp,
                            Caption = "WhatsApp",
                            // Try to find a glyph, or use null if not available
                            Glyph = t.Find_Glyph("WHATSAPP16") ?? t.Find_Glyph("MESSAGE16") ?? null
                        };
                        
                        // Add click handler
                        whatsAppButton.ElementClick += myNavElementClick;
                        
                        // Add to the pane
                        tileNavPane.Buttons.Add(whatsAppButton);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating WhatsApp button: {ex.Message}");
            }
        }

        private void myNavElementClick(object sender, DevExpress.XtraBars.Navigation.NavElementEventArgs e)
        {
            if (sender.GetType().ToString() == "DevExpress.XtraBars.Navigation.NavButton")
            {
                if (((DevExpress.XtraBars.Navigation.NavButton)sender).Name == buttonESrc) eSrcEntegrasyonu();
                if (((DevExpress.XtraBars.Navigation.NavButton)sender).Name == buttonWhatsApp) OpenWhatsAppForm();
            }

            if (sender.GetType().ToString() == "DevExpress.XtraBars.Navigation.TileNavItem")
            {
                //if (((DevExpress.XtraBars.Navigation.TileNavItem)sender).Name == buttonInsertPaketOlustur) InsertPaketOlustur();
                //if (((DevExpress.XtraBars.Navigation.TileNavItem)sender).Name == buttonPaketiGonder) PaketiGonder();
            }
        }

        private void OpenWhatsAppForm()
        {
            try
            {
                // Validate JWT and Firm context (similar to checkedInputApi pattern)
                if (string.IsNullOrEmpty(v.tUser.JwtToken))
                {
                    MessageBox.Show(
                        "JWT token bulunamadı. Lütfen tekrar giriş yapın.",
                        "Kimlik Doğrulama Hatası",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                if (string.IsNullOrEmpty(v.tMainFirm.FirmGuid))
                {
                    MessageBox.Show(
                        "Firma bilgisi bulunamadı. Lütfen tekrar giriş yapın.",
                        "Firma Bilgisi Hatası",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                // Open WhatsApp form using the form opening pattern
                string FormName = "ms_WhatsApp";
                string FormCode = "UST/PMS/PMS/WhatsApp";
                t.OpenFormPreparing(FormName, FormCode, v.formType.Child);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"WhatsApp formu açılırken hata oluştu: {ex.Message}",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        
        private void eSrcEntegrasyonu()
        {
            if (v.SP_Firm_SectorTypeId == 201)
            {
                MessageBox.Show("Mtsk Entegrasyon butonuna tıklandı.");
                //sendESrcApi();
            }

            if (v.SP_Firm_SectorTypeId == 203)
            {
                //MessageBox.Show("Src Entegrasyon butonuna tıklandı.");
                sendESrcApi();
            }

            if (v.SP_Firm_SectorTypeId == 204)
            {
                MessageBox.Show("Src5 Entegrasyon butonuna tıklandı.");
                //sendESrcApi();
            }

        }

        private bool KullaniciOnayi()
        {
            if (ds_KursiyerList == null) return false;
            if (t.IsNotNull(ds_KursiyerList) == false) return false;

            string Donem = ds_KursiyerList.Tables[0].Rows[0]["Lkp_DonemTipi"].ToString();
            bool onay = false;
            string soru = Donem + " dönemi kursiyerleri Soru Bankasına gönderilecek, Onaylıyor musunuz ?";
            DialogResult cevap = t.mySoru(soru);
            if (DialogResult.Yes == cevap)
            {
                onay = true;
            }
            return onay;
        }


        private void sendESrcApi()
        {
            
            if (KullaniciOnayi() == false) return;

            bool onaylimi = Onaylimi(ds_KursiyerList);
            bool onay = false;

            // Mtsk Aday Talep Donemler Tablosu
            // Src Aday Talep Donemler Tablosu      

            // 1. Listeyi döngüden önce tanımlayın
            var studentList = new List<StudentDataModel>();

            foreach (DataRow row in ds_KursiyerList.Tables[0].Rows) // Mevcut döngünüz
            {
                onay = row["LKP_ONAY"].ToString().ToUpper() == "TRUE" ? true : false;

                if ((onaylimi == false) || (onaylimi == true && onay))
                {
                    // 2. Her öğrenci için yeni bir nesne örneği oluşturun
                    var item = new StudentDataModel();

                    item.KURSMAIL = t.Set(v.tMainFirm.eSrcEnt_KursKodu, v.tMainFirm.FirmCode, v.tMainFirm.DatabaseName);
                    item.PASS = v.tMainFirm.eSrcEnt_Pass;
                    item.TC = row["Lkp_TcNo"].ToString();
                    item.ADI = row["Lkp_Adi"].ToString();
                    item.SOYADI = row["Lkp_Soyadi"].ToString();
                    item.EMAIL = row["Lkp_Eposta"].ToString();
                    item.IL = "";
                    item.ILCE = "";
                    item.ADRES = row["Lkp_Adres"].ToString();
                    item.GSM = row["Lkp_CepTelefonu"].ToString();
                    item.IMG = null;
                    item.BELGE = row["Lkp_IstenenSertifikaTipi"].ToString();
                    item.CINSIYET = row["Lkp_CinsiyetTipi"].ToString();
                    item.BAKIYE = 0;
                    item.SUBESI = row["Lkp_DonemTipi"].ToString();
                    item.GRUP = row["Lkp_GrupTipi"].ToString();
                    item.DONEM = row["Lkp_SubeTipi"].ToString();

                    // 3. Hazırlanan nesneyi listeye ekleyin
                    studentList.Add(item);
                }
            }

            // 4. Döngü bittikten sonra tüm listeyi JSON'a çevirelim
            //string finalJson = JsonSerializer.Serialize(studentList, new JsonSerializerOptions { WriteIndented = true });
            string finalJson = JsonConvert.SerializeObject(studentList, Formatting.Indented);

            /// Janberk : POST işlemi başlayacak


        }
        
        private bool Onaylimi(DataSet ds)
        { 
            bool onay = false;
            string value = "";
            
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                value = "";
                try
                {
                    value = dr["LKP_ONAY"].ToString().ToUpper();
                    if (value == "TRUE") 
                    { 
                        onay = true; 
                        break;
                    }
                }
                catch (Exception)
                {
                    break;
                    //throw;
                }
            }

            return onay; 
        }


    }
}
