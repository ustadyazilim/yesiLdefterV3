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

        public ms_Donemler()
        {
            InitializeComponent();
        }

        private void ms_Donemler_Shown(object sender, EventArgs e)
        {
            t.Find_Button_AddClick(this, menuName, buttonESrc, myNavElementClick);

            if (ds_KursiyerList == null)
            {
                if (v.SP_Firm_SectorTypeId == 201) // Mtsk
                    TableIPCode = "UST/MEB/MtskAdayTalep.DonemlerTumListesi";

                if (v.SP_Firm_SectorTypeId == 203) // Src
                    TableIPCode = "UST/MEB/SrcAdayTalep.DonemlerDonemListesi";

                t.Find_DataSet(this, ref ds_KursiyerList, ref dN_KursiyerList, TableIPCode);
            }
        }

        private void myNavElementClick(object sender, DevExpress.XtraBars.Navigation.NavElementEventArgs e)
        {
            if (sender.GetType().ToString() == "DevExpress.XtraBars.Navigation.NavButton")
            {
                if (((DevExpress.XtraBars.Navigation.NavButton)sender).Name == buttonESrc) eSrcEntegrasyonu();
            }

            if (sender.GetType().ToString() == "DevExpress.XtraBars.Navigation.TileNavItem")
            {
                //if (((DevExpress.XtraBars.Navigation.TileNavItem)sender).Name == buttonInsertPaketOlustur) InsertPaketOlustur();
                //if (((DevExpress.XtraBars.Navigation.TileNavItem)sender).Name == buttonPaketiGonder) PaketiGonder();
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
