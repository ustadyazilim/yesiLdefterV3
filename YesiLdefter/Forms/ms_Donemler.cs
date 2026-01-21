using DevExpress.XtraEditors;
using Microsoft.JScript;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
                MessageBox.Show("Src Entegrasyon butonuna tıklandı.");
                //sendESrcApi();
            }


        }
        
        
        private void sendESrcApi()
        {
            // Mtsk Aday Talep Donemler Tablosu
            // Src Aday Talep Donemler Tablosu      

            foreach (DataRow dr in ds_KursiyerList.Tables[0].Rows)
            {
                /*
                string donemKodu = t.DataRow_Get_String(dr, "DonemKodu");
                string donemAdi = t.DataRow_Get_String(dr, "DonemAdi");
                DateTime baslangicTarihi = t.DataRow_Get_DateTime(dr, "BaslangicTarihi");
                DateTime bitisTarihi = t.DataRow_Get_DateTime(dr, "BitisTarihi");
                */
            }   


        }
        
    }
}
