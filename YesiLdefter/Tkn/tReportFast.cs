using DevExpress.XtraEditors;
using FastReport;
using FastReport.Data;
using FastReport.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tkn_Save;
using Tkn_ToolBox;
using Tkn_Variable;

namespace Tkn_Report
{
    class tReportFast
    {
        tToolBox t = new tToolBox();
        
        Form tFormReports = null;
        

        public void ReportDocumentViewer(Form tForm, DataSet dsMsReports, DataNavigator dNMsReports, List<string> dataSetList, string sourceFormCodeAndName, ref FastReport.Preview.PreviewControl documentViewer1, ref bool isRepertSelected)

        {

            // Raporların olduğu form
            tFormReports = tForm;

            // Raporlar Formunu çağıran formun tespiti
            string formName = t.getFormName(sourceFormCodeAndName);
            Form tFormSource = Application.OpenForms[formName];

            string reportCode = dsMsReports.Tables[0].Rows[dNMsReports.Position]["ReportCode"].ToString();
            string reportName = dsMsReports.Tables[0].Rows[dNMsReports.Position]["ReportName"].ToString();
            string reportCaption = dsMsReports.Tables[0].Rows[dNMsReports.Position]["ReportCaption"].ToString();

            string reportCode2 = reportCode;
            string reportCaption2 = reportCaption;
            reportCode = reportCode.Replace(".", "_");
            reportCaption = "_" + reportCaption.Replace(" ", "_") + "_";

            if (reportCaption.ToLower().IndexOf("seçin") > -1)
            {
                isRepertSelected = false;
                return;
            }
            isRepertSelected = true;

            t.WaitFormOpen(v.mainForm, "Rapor hazırlanıyor ...");
            v.SP_OpenApplication = true;


            //13.07.2025 13:33:58
            //20250713_1333 
            string tarih_ = t.Formatli_TarihSaat_yyyyMMdd_hhmm();

            // load the report from a stream contained in the "ReportStream" datacolumn
            byte[] reportBytes = null;

            try
            {
                reportBytes = (byte[])dsMsReports.Tables[0].Rows[dNMsReports.Position]["ReportTemp"];
            }
            catch (Exception)
            {
                //
            }

            //if (t.IsNotNull(reportName) == false)
            //    reportName = reportCode;

            //string reportFileName = v.EXE_FastReportsPath + reportCode + reportCaption + tarih_ + ".frx";
            string reportFileName = v.EXE_FastReportsPath + reportCode + reportCaption + ".frx";

            if (!File.Exists(@"" + reportFileName) && (reportBytes == null || reportBytes?.Length == 0))
            {
                preparingAutoFastReportFile(reportFileName, reportCaption);
                v.IsWaitOpen = false;
                t.WaitFormClose();
                return;
            }

            try 
            {
                //  previewControl1
                if (documentViewer1.Report != null)
                {
                    documentViewer1.Clear();
                    documentViewer1.Report.Dispose();
                }
                try
                {
                    Report FReport = new Report();

                    FReport.Preview = documentViewer1; // previewControl1;

                    // Eğer database kayıtlı bir rapor yok ise dosyadan oku
                    if (reportBytes == null || reportBytes?.Length == 0)
                    {
                        FReport.Load(reportFileName);
                    }
                    else
                    {
                        // database kayıtlı bir rapor var ise onu oku
                        using (MemoryStream stream = new MemoryStream(reportBytes))
                        {
                            FReport.Load(stream);
                        }
                    }

                    preparingReportHeaderAndFooter(dsMsReports, dNMsReports, FReport, reportFileName);

                    allRegisterData(tForm, FReport, dataSetList);

                    FReport.Show();
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message);
                }
            }
            finally
            {
                //lockSelect = false;
            }

            v.IsWaitOpen = false;
            t.WaitFormClose();
        }

        private void preparingAutoFastReportFile(string reportFileName, string reportCaption)
        {
            /// Auto FastReport Page Create
            ///
            Report report = new Report();
            ReportPage page1 = new ReportPage();
            DataBand dataBand1 = new DataBand();

            page1.Name = "Page1";
            dataBand1.Name = "DataBand1";
            dataBand1.Height = Units.Centimeters * (float)2.0;

            //page1.Bands.Add(reportTitleBand1);
            //page1.Bands.Add(pageHeaderBand1);
            page1.Bands.Add(dataBand1);
            //page1.Bands.Add(reportSummaryBand1);
            //page1.Bands.Add(pageFooterBand1);

            report.Pages.Add(page1);

            report.Save(reportFileName);
            MessageBox.Show(reportCaption + " : için yeni rapor dosyası oluşturuldu.");
        }

        private void preparingReportHeaderAndFooter(DataSet dsMsReports, DataNavigator dNMsReports, Report FReport, string reportFileName)
        {
            string reportHeader = "";
            string pageHeader = "";
            string reportSummary = "";
            string pageSummary = "";
            
            if (t.IsNotNull(dsMsReports.Tables[0].Rows[dNMsReports.Position]["ReportHeader"].ToString()))
                reportHeader = dsMsReports.Tables[0].Rows[dNMsReports.Position]["ReportHeader"].ToString();
            if (t.IsNotNull(dsMsReports.Tables[0].Rows[dNMsReports.Position]["PageHeader"].ToString()))
                pageHeader = dsMsReports.Tables[0].Rows[dNMsReports.Position]["PageHeader"].ToString();
            if (t.IsNotNull(dsMsReports.Tables[0].Rows[dNMsReports.Position]["ReportSummary"].ToString()))
                reportSummary = dsMsReports.Tables[0].Rows[dNMsReports.Position]["ReportSummary"].ToString();
            if (t.IsNotNull(dsMsReports.Tables[0].Rows[dNMsReports.Position]["PageSummary"].ToString()))
                pageSummary = dsMsReports.Tables[0].Rows[dNMsReports.Position]["PageSummary"].ToString();

            if ((reportHeader == "") &&
                (pageHeader == "") &&
                (reportSummary == "") &&
                (pageSummary == "")) return;

            string bandFileName = v.EXE_FastReportsPath;

            Report fReport = new Report();

            FReport.BeginInit();

            if (t.IsNotNull(reportHeader))
            {
                reportHeader = reportHeader.Replace(".","_");
                bandFileName = v.EXE_FastReportsPath + reportHeader + ".frx";
                BandBase bandReportHeader = getBandBase(fReport, bandFileName, 1);
                //((ReportPage)(FReport.Pages[0])).Bands.Add(bandReportHeader);
                ((ReportPage)(FReport.Pages[0])).ReportTitle = (ReportTitleBand)bandReportHeader;
            }
            if (t.IsNotNull(pageHeader))
            {
                pageHeader = pageHeader.Replace(".", "_");
                bandFileName = v.EXE_FastReportsPath + pageHeader + ".frx";
                BandBase bandPageHeader = getBandBase(fReport, bandFileName, 2);
                //((ReportPage)(FReport.Pages[0])).Bands.Add(bandPageHeader);
                ((ReportPage)(FReport.Pages[0])).PageHeader = (PageHeaderBand)bandPageHeader;
            }

            if (t.IsNotNull(reportSummary))
            {
                reportSummary = reportSummary.Replace(".", "_");
                bandFileName = v.EXE_FastReportsPath + reportSummary + ".frx";
                BandBase bandReportSummary = getBandBase(fReport, bandFileName, 3);
                //((ReportPage)(FReport.Pages[0])).Bands.Add(bandReportSummary);
                ((ReportPage)(FReport.Pages[0])).ReportSummary = (ReportSummaryBand)bandReportSummary;
            }
            if (t.IsNotNull(pageSummary))
            {
                pageSummary = pageSummary.Replace(".", "_");
                bandFileName = v.EXE_FastReportsPath + pageSummary + ".frx";
                BandBase bandPageSummary = getBandBase(fReport, bandFileName, 4);
                //((ReportPage)(FReport.Pages[0])).Bands.Add(bandPageSummary);
                ((ReportPage)(FReport.Pages[0])).PageFooter = (PageFooterBand)bandPageSummary;
            }

            FReport.EndInit();
            FReport.Refresh();
            FReport.Save(reportFileName);



            /*
            ReportTitleBand reportTitleBand1 = new ReportTitleBand();
            PageHeaderBand pageHeaderBand1 = new PageHeaderBand();
            ReportSummaryBand reportSummaryBand1 = new ReportSummaryBand();
            PageFooterBand pageFooterBand1 = new PageFooterBand();

            reportTitleBand1.Name = "ReportTitleBand1";
            pageHeaderBand1.Name = "PageHeaderBand1";
            reportSummaryBand1.Name = "ReportSummaryBand1";
            pageFooterBand1.Name = "PageFooterBand1";

            reportTitleBand1.Height = Units.Centimeters * (float)1.0;
            pageHeaderBand1.Height = Units.Centimeters * (float)0.75;
            reportSummaryBand1.Height = Units.Centimeters * (float)0.75;
            pageFooterBand1.Height = Units.Centimeters * (float)0.75;

            page1.Bands.Add(reportTitleBand1);
            //page1.Bands.Add(pageHeaderBand1);
            //page1.Bands.Add(reportSummaryBand1);
            page1.Bands.Add(pageFooterBand1);
            */

            /*
                                Report report = FReport.Report;
                                PageCollection page = report.Pages;
                                PageBase page1 = page[0];

                                ReportSummaryBand reportSummaryBand1 = new ReportSummaryBand();
                                reportSummaryBand1.Name = "UstadYazilim";
                                reportSummaryBand1.Height = Units.Centimeters * (float)2.0;
                                DataBand dataBand2 = new DataBand();
                                dataBand2.Name = "DataBand2";
                                dataBand2.Height = Units.Centimeters * (float)4.0;
                                */
            /*
            PageHeaderBand pageHeaderBand1 = new PageHeaderBand();
            pageHeaderBand1.Name = "PageHeaderBand1";
            pageHeaderBand1.Height = Units.Centimeters * (float)4.0;

            FastReport.TextObject textObject = new TextObject();
            textObject.Name = "text1";
            textObject.Text = "Üstad Yazılım";
            textObject.Left = Units.Centimeters * (float)0;
            textObject.Height = Units.Centimeters * (float)0.5;
            textObject.HorzAlign = HorzAlign.Center;
            textObject.Dock = DockStyle.Fill;

            pageHeaderBand1.AddChild(textObject);

            FReport.BeginInit();
            ((ReportPage)(FReport.Pages[0])).Bands.Add(pageHeaderBand1);
            FReport.EndInit();
            FReport.Refresh();
            FReport.Save(reportFileName);
            */

            /*
            report.BeginInit();
            ((ReportPage)(report.Pages[0])).Bands.Add(pageHeaderBand1);
            report.EndInit();
            report.Refresh();
            report.Save(reportFileName);
            */



        }

        private BandBase getBandBase(Report fReport, string bandFileName, Int16 pos)
        {
            fReport.Load(bandFileName);

            PageCollection page = fReport.Pages;
            PageBase page1 = page[0];
            BandBase bandBase = null;

            if (pos == 1) bandBase = ((ReportPage)page1).ReportTitle;
            if (pos == 2) bandBase = ((ReportPage)page1).PageHeader;
            if (pos == 3) bandBase = ((ReportPage)page1).ReportSummary;
            if (pos == 4) bandBase = ((ReportPage)page1).PageFooter;

            // dataBand için geçerli galiba
            //BandBase band1 = ((ReportPage)page1).Bands[0];
            //BandBase band1 = ((ReportPage)(fReport.Pages[0])).Bands[0];

            return bandBase;
        }


        private void allRegisterData(Form tForm, Report FReport, List<string> dataSetList)
        {
            DataSet dsData = null;
            DataNavigator dN = null;
            object tDataTable = null;
            string tableName = "";
            string tableIPCode = "";

            string softwareCode = "";
            string projectCode = "";
            string TableCode = "";
            string IPCode = "";
            
            Control cntrl = new Control();
            string[] controls = new string[] { "DevExpress.XtraEditors.DataNavigator" };
            foreach (string value in dataSetList)
            {
                cntrl = t.Find_Control(tForm, value, "", controls);
                if (cntrl != null)
                {
                    dN = (DevExpress.XtraEditors.DataNavigator)cntrl;
                    if (dN.DataSource != null)
                    {
                        tDataTable = dN.DataSource;
                        
                        tableIPCode = dN.AccessibleName?.ToString();
                        t.TableIPCode_Get(tableIPCode, ref softwareCode, ref projectCode, ref TableCode, ref IPCode);
                        tableName = IPCode;

                        dsData = ((DataTable)tDataTable).DataSet;
                       
                        FReport.RegisterData(dsData);
                        FReport.GetDataSource(tableName).Enabled = true;
                    }
                }
            }
        }

        private void PreviewReport(Report FReport, string tableName)
        {
            FReport.Show();
        }

    }
}
