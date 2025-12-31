using System.Windows.Forms;

namespace Tkn_Forms
{
    class tForms : tBase
    {
        // kullanmak istedğin formları burada tek tek tanıtman gerekiyor

        public Form Get_Form(string FormName)
        {
            Form tForm = null;

            if (FormName == "ms_Form")
            {
                tForm = new YesiLdefter.ms_Form();
                tForm.Name = "ms_Form"; 
            }
            if (FormName == "ms_Donemler") tForm = new YesiLdefter.ms_Donemler();
            if (FormName == "ms_TileControl") tForm = new YesiLdefter.ms_TileControl();
            if (FormName == "ms_Computer") tForm = new YesiLdefter.ms_Computer();
            if (FormName == "ms_User") tForm = new YesiLdefter.ms_User();
            if (FormName == "ms_UserFirmList") tForm = new YesiLdefter.ms_UserFirmList();

            if (FormName == "ms_DBList") tForm = new YesiLdefter.ms_DBList();
            if (FormName == "ms_IPList") tForm = new YesiLdefter.ms_IPList();
            if (FormName == "ms_Layout") tForm = new YesiLdefter.ms_Layout();
            if (FormName == "ms_Menu") tForm = new YesiLdefter.ms_Menu();
            if (FormName == "ms_Pictures") tForm = new YesiLdefter.ms_Pictures();
            if (FormName == "ms_ProjeDef") tForm = new YesiLdefter.ms_ProjeDef();
            if (FormName == "ms_Reports")
            {
                tForm = new YesiLdefter.ms_Reports();
                tForm.Name = "ms_Reports";
            }
            if (FormName == "ms_ReportDesign") tForm = new YesiLdefter.ms_ReportDesign();
            if (FormName == "ms_FastReport") tForm = new YesiLdefter.ms_FastReport();
            if (FormName == "ms_ProjectTables") tForm = new YesiLdefter.ms_ProjectTables();
            if (FormName == "ms_FileUpdates") tForm = new YesiLdefter.ms_FileUpdates();

            if (FormName == "ms_IPList2") tForm = new YesiLdefter.ms_IPList2();
            if (FormName == "ms_Layout2") tForm = new YesiLdefter.ms_Layout2();
            if (FormName == "ms_Menu2") tForm = new YesiLdefter.ms_Menu2();

            if (FormName == "ms_Search") tForm = new YesiLdefter.ms_Search();
            if (FormName == "ms_Selenium") tForm = new YesiLdefter.ms_Selenium();
            if (FormName == "ms_CefSharp") tForm = new YesiLdefter.ms_CefSharp();
            if (FormName == "ms_Webbrowser") tForm = new YesiLdefter.ms_Webbrowser();
            if (FormName == "ms_MtskDersProgrami") tForm = new YesiLdefter.ms_MtskDersProgrami();
            if (FormName == "ms_MtskSinavRandevu") tForm = new YesiLdefter.ms_MtskSinavRandevu();
            if (FormName == "ms_TabimMtsk") tForm = new YesiLdefter.ms_TabimMtsk();
            if (FormName == "ms_DestekServiceTool") tForm = new YesiLdefter.ms_DestekServiceTool();
            if (FormName == "ms_Bildirimler") tForm = new YesiLdefter.ms_Bildirimler();
            if (FormName == "ms_EFatura") tForm = new YesiLdefter.ms_EFatura();
            if (FormName == "ms_XMLRead") tForm = new YesiLdefter.ms_XMLRead();

            if (FormName == "SekCevHukumlu") tForm = new YesiLdefter.Forms.SEK.SekCevHukumlu();
            if (FormName == "SekCevHukumluList") tForm = new YesiLdefter.Forms.SEK.SekCevHukumluList();


            //if (FormName == "ajn_Aranan") tForm = new YesiLdefter.ajn_Aranan();
            //if (FormName == "hp_Musteri") tForm = new YesiLdefter.hp_Musteri();
            //if (FormName == "hp_CariList") tForm = new YesiLdefter.hp_CariList();
            //if (FormName == "hp_PersonelList") tForm = new YesiLdefter.hp_PersonelList();
            //if (FormName == "fns_FisCari") tForm = new YesiLdefter.fns_FisCari();
            //if (FormName == "fns_Fis") tForm = new YesiLdefter.fns_Fis();
            //if (FormName == "fns_FinansFisleri") tForm = new YesiLdefter.fns_FinansFisleri();


            //if (FormName == "MSReportDesigner") tForm = new YesiLdefter.MSReportDesigner();
            //if (FormName == "MSReportDetail") tForm = new YesiLdefter.MSReportDetail();
            //if (FormName == "MSReportParam") tForm = new YesiLdefter.MSReportParam();
            //if (FormName == "MSReportView") tForm = new YesiLdefter.MSReportView();

            //if (FormName == "TestFormu1") tForm = new YesiLdefter.TestForm1();

            //MessageBox.Show("DİKKAT : Aranan form bulunamadı ... [ " + FormName + " ] ");

            return tForm;
        }

        public Form Get_Form(string FormName, Form SourceForm)
        {
            //if (FormName == "tn_SMSGonder") return new Tabim.MTSK.tn_SMSGonder(SourceForm);
            //if (FormName == "tn_SMSSablonuTanimla") return new Tabim.MTSK.tn_SMSSablonuTanimla(SourceForm);

            return null;
        }

    }
}


