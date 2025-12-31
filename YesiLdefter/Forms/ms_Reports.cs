using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraPrinting.Preview;
using DevExpress.XtraReports.UI;
using FastReport;
using FastReport.Design;
using FastReport.Preview;
using FastReport.Utils;
using Tkn_Events;
using Tkn_InputPanel;
using Tkn_Report;
using Tkn_Save;
using Tkn_ToolBox;
using Tkn_Variable;

namespace YesiLdefter
{
    public partial class ms_Reports : Form
    {
        tToolBox t = new tToolBox();
        tInputPanel ip = new tInputPanel();
        tEvents ev = new tEvents();

        tReportDevEx raporDevEx = new tReportDevEx();
        tReportFast raporFast = new tReportFast();

        //private DataSet dsMsReports = null;
        //private DataNavigator dNMsReports = null;
        private DevExpress.XtraPrinting.Preview.DocumentViewer documentViewerDevEx = null;
        private FastReport.Preview.PreviewControl documentViewerFast = null;

        string sourceFormCodeAndName = "";
        int readReportPosition = -2;
        bool isReportLoaded = false;
        bool isReportFirstLoaded = false;
        bool isRepertSelected = false;

        Control cntrlReportNames = null;
        string controlNames = "controlReportNameList";

        string menuName = "MENU_" + "UST/PMS/PMS/REPORT";
        string buttonRaporYazdir = "RAPOR_YAZDIR";
        string buttonRaporOnizleme = "RAPOR_ONIZLEME";

        //private string FirmAbout_TableIPCode = "";
        //private DataSet dsFirmAbout = null;
        //private DataNavigator dNFirmAbout = null;

        Int16 desingerType = 0;
        List<string> dataSetList = null; 


        public ms_Reports()
        {
            InitializeComponent();

            tEventsForm evf = new tEventsForm();

            this.Load += new System.EventHandler(evf.myForm_Load);
            this.Shown += new System.EventHandler(evf.myForm_Shown);
            this.Shown += new System.EventHandler(this.ms_Reports_Shown);
            
            this.KeyPreview = true;

            v.dsMsReports = null;
            v.dNMsReports = null;
        }

        private void ms_Reports_Shown(object sender, EventArgs e)
        {
            string ipCodes = this.AccessibleDefaultActionDescription;
            if (this.AccessibleDescription != null)
                sourceFormCodeAndName = this.AccessibleDescription;

            /*
            cntrlReportNames = t.Find_Control(this, controlNames);

            string tableIPCode = v.con_Source_ReportTableIPCode;

            if (t.IsNotNull(tableIPCode))
            {
                tInputPanel ip = new tInputPanel();
                ip.Create_InputPanel(this, cntrlReportNames, tableIPCode, 1, true);


                t.Find_DataSet(this, ref dsMsReports, ref dNMsReports, tableIPCode);
                dNMsReports.PositionChanged += new System.EventHandler(dNMsReports_PositionChanged);
            }
            */
            documentViewerDevEx = (DevExpress.XtraPrinting.Preview.DocumentViewer)t.Find_Control(this, "documentViewerDevEx");
            documentViewerFast = (FastReport.Preview.PreviewControl)t.Find_Control(this, "documentViewerFast");

            // ustad elemanları
            if (v.tUser.UserDbTypeId < 30)
            {
                documentViewerFast.Buttons =
                 ((FastReport.PreviewButtons)((((((((((((((((FastReport.PreviewButtons.Print
                | FastReport.PreviewButtons.Open)
                | FastReport.PreviewButtons.Save)
                | FastReport.PreviewButtons.Email)
                | FastReport.PreviewButtons.Find)
                | FastReport.PreviewButtons.Zoom)
                | FastReport.PreviewButtons.Outline)
                | FastReport.PreviewButtons.PageSetup)
                | FastReport.PreviewButtons.Edit)
                | FastReport.PreviewButtons.Watermark)
                | FastReport.PreviewButtons.Navigator)
                | FastReport.PreviewButtons.Close)
                | FastReport.PreviewButtons.Design)
                | FastReport.PreviewButtons.CopyPage)
                | FastReport.PreviewButtons.DeletePage)
                | FastReport.PreviewButtons.About)));
            }
            else
            {
                documentViewerFast.Buttons =
                 ((FastReport.PreviewButtons)((((((((((FastReport.PreviewButtons.Print
                | FastReport.PreviewButtons.Save)
                | FastReport.PreviewButtons.Email)
                | FastReport.PreviewButtons.Find)
                | FastReport.PreviewButtons.Zoom)
                | FastReport.PreviewButtons.Outline)
                | FastReport.PreviewButtons.PageSetup)
                | FastReport.PreviewButtons.Watermark)
                | FastReport.PreviewButtons.Navigator)
                | FastReport.PreviewButtons.About)));

                //| FastReport.PreviewButtons.Open)
                //| FastReport.PreviewButtons.Design)
                //| FastReport.PreviewButtons.Edit)
                //| FastReport.PreviewButtons.CopyPage)
                //| FastReport.PreviewButtons.DeletePage)
                //| FastReport.PreviewButtons.Close)

            }
            
            /// FastReport config
            WireupDesignerEvents();

            /*
            // DİKKAT : Bu atamanın yerini değiştirme
            //
            // ms_Reports u çağıaran formun FormCode si
            // hangi formun içinden çağrıldığına dair işaret
            this.AccessibleDescription = v.con_Source_FormCodeAndName;
            // this.AccessibleName ise ms_Reports un kendisi
            //
            
            // Raporları listeleyen TableIPCode nin tespiti
            string ipCodes = this.AccessibleDefaultActionDescription;
            string tableIPCode = t.Get_And_Clear(ref ipCodes, "||");

            t.Find_DataSet(this, ref dsMsReports, ref dNMsReports, tableIPCode);

            dNMsReports.PositionChanged += new System.EventHandler(dNMsReports_PositionChanged);


            documentViewer = (DocumentViewer)t.Find_Control(this, "documentViewer");

            if (this.AccessibleDescription != null)
                sourceFormCodeAndName = this.AccessibleDescription;
            */

            t.Find_Button_AddClick(this, menuName, buttonRaporYazdir, myNavElementClick);
            t.Find_Button_AddClick(this, menuName, buttonRaporOnizleme, myNavElementClick);

            preparingDataSetList();

            //preparingFirmAboutDataSets();

        }

        private void preparingDataSetList()
        {
            #region DataNavigator Listesi Hazırlanıyor

            dataSetList = new List<string>();
            List<string> list = new List<string>();
            t.Find_DataNavigator_List(this, ref list);

            Control cntrl = new Control();
            string[] controls = new string[] { "DevExpress.XtraEditors.DataNavigator" };
                        
            DataNavigator dN = null;
            object tDataTable = null;
            string tableName = "";
            string myProp = "";

            this.isReportLoaded = true;

            foreach (string value in list)
            {
                cntrl = t.Find_Control(this, value, "", controls);
                if (cntrl != null)
                {
                    dN = (DevExpress.XtraEditors.DataNavigator)cntrl;

                    if (dN.DataSource != null)
                    {
                        tDataTable = dN.DataSource;

                        myProp = ((DataTable)tDataTable).Namespace.ToString();
                        tableName = t.MyProperties_Get(myProp, "TableName:");

                        if (tableName.IndexOf("MsReports") > -1)
                        {
                            v.dsMsReports = ((DataTable)tDataTable).DataSet;
                            v.dNMsReports = dN;
                            v.dNMsReports.PositionChanged -= new System.EventHandler(dNMsReports_PositionChanged);
                            v.dNMsReports.PositionChanged += new System.EventHandler(dNMsReports_PositionChanged);
                        }
                        else
                        {
                            if (isReportFirstLoaded == false)
                            {
                                dN.PositionChanged -= dN_Data_PositionChanged;
                                dN.PositionChanged += dN_Data_PositionChanged;
                                isReportFirstLoaded = true;
                            }

                            dataSetList.Add(value);
                        }
                    }

                } // if cntrl != null
            }//foreach

            this.isReportLoaded = false;

            #endregion DataNavigator Listesi
        }

        private void preparingFirmAboutDataSets()
        {
            /*
            if ((v.tMainFirm.SectorTypeId == (Int16)v.msSectorType.UstadMtsk) ||
                (v.tMainFirm.SectorTypeId == (Int16)v.msSectorType.TabimMtsk)) FirmAbout_TableIPCode = "UST/MEB/MebKurum.MsReports";

            //    (v.tMainFirm.SectorTypeId == (Int16)v.msSectorType.TabimSrc) ||
            //    (v.tMainFirm.SectorTypeId == (Int16)v.msSectorType.TabimIsmak))

            if (t.IsNotNull(FirmAbout_TableIPCode))
            {
                tInputPanel ip = new tInputPanel();
                ip.Create_InputPanel(this, cntrlReportNames, FirmAbout_TableIPCode, 1, true);
                t.Find_DataSet(this, ref dsFirmAbout, ref dNFirmAbout, FirmAbout_TableIPCode);
            }
            */
        }

        private void dN_Data_PositionChanged(object sender, EventArgs e)
        {
            if (isReportLoaded) return;
            
            if (this.isRepertSelected == false)
            {
                MessageBox.Show("Lütfen bir rapor seçiniz.");
                return;
            }

            if (t.IsNotNull(v.dsMsReports))
            {
                desingerType = Convert.ToInt16(v.dsMsReports.Tables[0].Rows[v.dNMsReports.Position]["DesignerTypeId"].ToString());

                if (desingerType == (Int16)v.ReportDesignerTool.DevExpress)
                {
                    raporDevEx.ReportDocumentViewer(this, v.dsMsReports, v.dNMsReports, sourceFormCodeAndName, ref documentViewerDevEx);
                }
                else
                {
                    raporFast.ReportDocumentViewer(this, v.dsMsReports, v.dNMsReports, dataSetList, sourceFormCodeAndName, ref documentViewerFast, ref this.isRepertSelected);
                }
            }
        }

        private void dNMsReports_PositionChanged(object sender, EventArgs e)
        {
            // LKP_ONAY için ise işlem olmasın
            if ((v.con_OnayChange) || (v.con_PositionChange)) return;

            if (v.con_Cancel == true)
            {
                v.con_Cancel = false;
                return;
            }

            if (this.readReportPosition != -2 && this.readReportPosition == v.dNMsReports.Position) return;

            if (t.IsNotNull(v.dsMsReports))
            { 
                desingerType = Convert.ToInt16(v.dsMsReports.Tables[0].Rows[v.dNMsReports.Position]["DesignerTypeId"].ToString());

                if (desingerType == (Int16)v.ReportDesignerTool.DevExpress)
                {
                    raporDevEx.ReportDocumentViewer(this, v.dsMsReports, v.dNMsReports, sourceFormCodeAndName, ref documentViewerDevEx);
                }
                else
                {
                    preparingReportKritersAndData(this, v.dsMsReports, v.dNMsReports);
                    //preparingDataSetList();
                    
                    raporFast.ReportDocumentViewer(this, v.dsMsReports, v.dNMsReports, dataSetList, sourceFormCodeAndName, ref documentViewerFast, ref this.isRepertSelected);
                }

                /// en son okuduğun rapor pos u hafızaya al 
                this.readReportPosition = v.dNMsReports.Position;
            }
            /*
            t.WaitFormOpen("Rapor hazırlanıyor ...");

            ReportToolType = t.myInt32(tMSReports.Tables["MS_REPORTS"].Rows[tDataNavigator_MSResports.Position]["REPTOOL_TYPE"].ToString());

            if (ReportToolType <= (byte)v.ReportTool.DevExpress)
            {
                dv_report = null;
                dv_report = new XtraReport();

                if (xtraTabControl_View.SelectedTabPageIndex != 0)
                {
                    xtraTabControl_View.SelectedTabPage = xtraTabPage_DevExp;

                    previewBar_FastReport.Visible = false;
                    barHeader.Caption = "DevExpress";
                    previewBar_DevExpress.Offset = 45;
                    previewBar_DevExpress.Visible = true;

                    printPreviewSubItem1.Enabled = true;
                    printPreviewSubItem2.Enabled = true;
                    printPreviewSubItem3.Enabled = true;
                }
            }

            if (ReportToolType == (byte)v.ReportTool.FastReport)
            {
                //fs_report = null;
                //fs_report = new FastReport.Report();

                if (xtraTabControl_View.SelectedTabPageIndex != 1)
                {
                    //xtraTabControl_View.SelectedTabPage = xtraTabPage_FastRep;

                    previewBar_DevExpress.Visible = false;
                    barHeader.Caption = "FastReport";
                    previewBar_FastReport.Offset = 45;
                    previewBar_FastReport.Visible = true;

                    printPreviewSubItem1.Enabled = false;
                    printPreviewSubItem2.Enabled = false;
                    printPreviewSubItem3.Enabled = false;
                }
            }

            dsData = null;
            dsKrtr = null;

            try
            {
                ReportRead();
            }
            catch (Exception)
            {
                t.WaitFormClose();
            }

            t.WaitFormClose();
            */
        }

        private void myNavElementClick(object sender, DevExpress.XtraBars.Navigation.NavElementEventArgs e)
        {
            if (sender.GetType().ToString() == "DevExpress.XtraBars.Navigation.NavButton")
            {
                if (((DevExpress.XtraBars.Navigation.NavButton)sender).Name == buttonRaporYazdir) RaporYazdir();
                if (((DevExpress.XtraBars.Navigation.NavButton)sender).Name == buttonRaporOnizleme) RaporOnizleme();
            }
            // SubItem butonlar
            if (sender.GetType().ToString() == "DevExpress.XtraBars.Navigation.TileNavItem")
            {
                //if (((DevExpress.XtraBars.Navigation.TileNavItem)sender).Name == buttonMsfMsfIP) CopyMsfMsfIP();
            }
        }

        private void RaporYazdir()
        {
            if (documentViewerDevEx != null)
                if (documentViewerDevEx.DocumentSource != null)
                    ((XtraReport)documentViewerDevEx.DocumentSource).PrintDialog();
        }

        private void RaporOnizleme()
        {
            if (documentViewerDevEx != null)
                if (documentViewerDevEx.DocumentSource != null)
                    ((XtraReport)documentViewerDevEx.DocumentSource).ShowRibbonPreview();

            //if (documentViewerFast != null)
            //    if (documentViewerFast.Report != null)
            //        documentViewerFast.PrintPreview();
        }
        private void WireupDesignerEvents()
        {
            //Config.DesignerSettings.CustomOpenDialog += new OpenSaveDialogEventHandler(DesignerSettings_CustomOpenDialog);
            //Config.DesignerSettings.CustomOpenReport += new OpenSaveReportEventHandler(DesignerSettings_CustomOpenReport);
            Config.DesignerSettings.CustomSaveDialog += new OpenSaveDialogEventHandler(DesignerSettings_CustomSaveDialog);
            Config.DesignerSettings.CustomSaveReport += new OpenSaveReportEventHandler(DesignerSettings_CustomSaveReport);
        }

        private void DesignerSettings_CustomSaveDialog(object sender, OpenSaveDialogEventArgs e)
        {
            // return the report name in the e.FileName
            string reportCode = "NoReportCode";

            int pos = t.getDataNavigatorPosition(v.dNMsReports, true);

            if (pos > -1)
            {
                reportCode = v.dsMsReports.Tables[0].Rows[pos]["ReportCode"].ToString();
                e.FileName = reportCode + ".frx";
            }
        }

        

        private void DesignerSettings_CustomSaveReport(object sender, OpenSaveReportEventArgs e)
        {
            if (e.FileName.IndexOf("NoReportCode") == -1)
                SaveFastReport(e.Report);
        }
        private void SaveFastReport(Report report)
        {
            if (t.IsNotNull(v.dsMsReports))
            {
                bool onay = false;
                int pos = t.getDataNavigatorPosition(v.dNMsReports, true);
                if (pos == -1) return;

                string reportCode = v.dsMsReports.Tables[0].Rows[pos]["ReportCode"].ToString();
                string reportCaption = v.dsMsReports.Tables[0].Rows[pos]["ReportCaption"].ToString();
                string reportCode2 = reportCode;
                string reportCaption2 = reportCaption;

                reportCode = reportCode.Replace(".", "_");
                reportCaption = "_" + reportCaption.Replace(" ", "_") + "_";

                //13.07.2025 13:33:58
                //20250713_1333 
                string tarih_ = t.Formatli_TarihSaat_yyyyMMdd_hhmm();

                try
                {
                    report.Save(v.EXE_FastReportsPath + reportCode + reportCaption + tarih_ + ".frx");
                    onay = true;
                }
                catch (Exception)
                {
                    //throw;
                }
                
                if (onay)
                {
                    string soru = " Rapor Kodu : " + reportCode2 + v.ENTER
                                + " Rapor Adı  : " + reportCaption2 + v.ENTER2
                                + " Hazırlanan rapor fiziki olarak aşağıda belirtilen klasörede kayıt edildi. " + v.ENTER2 
                                + v.EXE_FastReportsPath + reportCode + reportCaption + tarih_ + ".frx" + v.ENTER2
                                + " Bu rapor dosyası database'de kayıt edilecek, onaylıyor musunuz ?";

                    DialogResult cevap = t.mySoru(soru);
                    if (DialogResult.Yes == cevap)
                    {
                        try
                        {
                            // save the report to a stream, then put byte[] array to the datarow
                            using (MemoryStream stream = new MemoryStream())
                            {
                                report.Save(stream);

                                v.dsMsReports.Tables[0].Rows[pos]["ReportTemp"] = stream.ToArray();
                                v.dsMsReports.Tables[0].AcceptChanges();

                                v.con_Images = stream.ToArray();
                                v.con_Images_FieldName = "ReportTemp";

                                if (v.con_Images != null)
                                {
                                    tSave sv = new tSave();
                                    sv.tDataSave(this, v.dsMsReports, v.dNMsReports, pos);
                                }
                                else
                                {
                                    MessageBox.Show("Raporu kaydetmek için önce küçükte olsa bir değişiklik yapın, ondan sonra kayıt edebilirsiniz.");
                                }

                            }
                            if (v.SaveOnay)
                                t.FlyoutMessage(this, reportCaption, "Rapor dosyası başarıyla database kaydedildi.");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Dikkat : Kayıt gerçekleşmedi..." + v.ENTER2 + ex.Message);
                            //throw;
                        }
                    }
                }
            }
        }
    
        private void preparingReportKritersAndData(Form tForm, DataSet dsMsReports, DataNavigator dNMsReports)
        {
            string reportCode = dsMsReports.Tables[0].Rows[dNMsReports.Position]["ReportCode"].ToString();
            string reportName = dsMsReports.Tables[0].Rows[dNMsReports.Position]["ReportName"].ToString();
            string reportCaption = dsMsReports.Tables[0].Rows[dNMsReports.Position]["ReportCaption"].ToString();
            string tableIPCode = dsMsReports.Tables[0].Rows[dNMsReports.Position]["TableIPCode"].ToString();
            string tableIPCode2 = dsMsReports.Tables[0].Rows[dNMsReports.Position]["TableIPCode2"].ToString();
            string tableIPCode3 = dsMsReports.Tables[0].Rows[dNMsReports.Position]["TableIPCode3"].ToString();

            if (t.IsNotNull(tableIPCode))
            {
                crateReportKriters(tForm, tableIPCode, tableIPCode2, tableIPCode3);
                /// Form üzerindeki DataSet ler tespit ediliyor
                preparingDataSetList();
            }
        }
    
        private void crateReportKriters(Form tForm, string tableIPCode, string tableIPCode2, string tableIPCode3)
        {
            tableIPCode = tableIPCode.Trim();
            tableIPCode2 = tableIPCode2.Trim();
            tableIPCode3 = tableIPCode3.Trim();

            string TabControlName = "";
            string ReadValue = "";

            //if (ViewType == "TabPage") TabControlName = "tabControl_SUBVIEW";
            //if (ViewType == "TabPage2") TabControlName = "tabControl_SUBVIEW2";

            Control cntrl = null;
            Control cntrl2 = null;
            Control cntrl3 = null;
            string[] controls = new string[] {
                    "DevExpress.XtraTab.XtraTabPage",
                    "DevExpress.XtraBars.Navigation.NavigationPage",
                    "DevExpress.XtraBars.Navigation.TabNavigationPage",
                    "DevExpress.XtraBars.Ribbon.BackstageViewClientControl"
                };


            TabControlName = "tabControl_SUBVIEW";
            ReadValue = "Kriterler";

            ev.CreateOrSelect_Page(this, TabControlName, "", tableIPCode, ReadValue, "", true);
            cntrl = t.Find_Control(this, "tTabPage_" + t.AntiStr_Dot(tableIPCode + ReadValue), "", controls);

            ip.Create_InputPanel(tForm, cntrl, tableIPCode, v.IPdataType_Kriterler, false);


            TabControlName = "tabControl_SUBVIEW2";
            ReadValue = "Data";
            
            ev.CreateOrSelect_Page(this, TabControlName, "", tableIPCode, ReadValue, "", true);
            cntrl2 = t.Find_Control(this, "tTabPage_" + t.AntiStr_Dot(tableIPCode + ReadValue), "", controls);

            ip.Create_InputPanel(tForm, cntrl2, tableIPCode, v.IPdataType_DataView, false);

            /// ikinci tableIPCode varsa
            if (t.IsNotNull(tableIPCode2))
            {
                TabControlName = "tabControl_SUBVIEW2";
                ReadValue = "Data";

                ev.CreateOrSelect_Page(this, TabControlName, "", tableIPCode2, ReadValue, "", true);
                cntrl2 = t.Find_Control(this, "tTabPage_" + t.AntiStr_Dot(tableIPCode2 + ReadValue), "", controls);

                ip.Create_InputPanel(tForm, cntrl2, tableIPCode2, v.IPdataType_DataView, false);
            }
            /// üçüncü tableIPCode varsa
            if (t.IsNotNull(tableIPCode3))
            {
                TabControlName = "tabControl_SUBVIEW2";
                ReadValue = "Data";

                ev.CreateOrSelect_Page(this, TabControlName, "", tableIPCode3, ReadValue, "", true);
                cntrl3 = t.Find_Control(this, "tTabPage_" + t.AntiStr_Dot(tableIPCode3 + ReadValue), "", controls);

                ip.Create_InputPanel(tForm, cntrl3, tableIPCode3, v.IPdataType_DataView, false);
            }


            string[] controls2 = new string[] { "DevExpress.XtraEditors.SimpleButton" };
            Control c = t.Find_Control(tForm, "simpleButton_Kriter_Listele", tableIPCode, controls2);
            if (c != null)
            {
                if (((DevExpress.XtraEditors.SimpleButton)c).Tag == null)
                {
                    ((DevExpress.XtraEditors.SimpleButton)c).Tag = tableIPCode;
                    ((DevExpress.XtraEditors.SimpleButton)c).Click += new System.EventHandler(this.btn_RaporKriterListele_Click);
                }
            }
        }

        public void btn_RaporKriterListele_Click(object sender, EventArgs e)
        {
            /// Form üzerindeki DataSet ler tespit ediliyor
            //preparingDataSetList();

            if (t.IsNotNull(v.dsMsReports))
            {
                desingerType = Convert.ToInt16(v.dsMsReports.Tables[0].Rows[v.dNMsReports.Position]["DesignerTypeId"].ToString());

                if (desingerType == (Int16)v.ReportDesignerTool.DevExpress)
                {
                    raporDevEx.ReportDocumentViewer(this, v.dsMsReports, v.dNMsReports, sourceFormCodeAndName, ref documentViewerDevEx);
                }
                else
                {
                    raporFast.ReportDocumentViewer(this, v.dsMsReports, v.dNMsReports, dataSetList, sourceFormCodeAndName, ref documentViewerFast, ref this.isRepertSelected);
                }
            }
        }

    }
}
