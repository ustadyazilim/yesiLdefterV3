using DevExpress.Utils;
//using System.Threading;

using DevExpress.XtraEditors;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Spire.Additions.Chrome;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tesseract;
using Tkn_CookieReader;
using Tkn_Events;
using Tkn_ExeUpdate;
using Tkn_Save;
using Tkn_ToolBox;
using Tkn_UserFirms;
using Tkn_Variable;
using YesiLdefter.Entities;
using YesiLdefter.Selenium;
using YesiLdefter.Selenium.Helpers;

namespace YesiLdefter
{
    public partial class ms_Selenium : Form
    {
        #region Tanımlar
        tToolBox t = new tToolBox();

        //IWebDriver webDriver;
        MsWebPagesService msPagesService = new MsWebPagesService();
        MsScrapingService msScraping = new MsScrapingService();

        /// Scraping
        DataSet ds_MsWebPages = null;
        DataNavigator dN_MsWebPages = null;
        DataSet ds_MsWebNodes = null;
        DataNavigator dN_MsWebNodes = null;
        // sadece login için kullanılan dataset
        DataSet ds_LoginPageNodes = null;
        // analiz için kullanılan dataset
        DataSet ds_AnalysisNodes = null;
        DataNavigator dN_AnalysisNodes = null;
        // web sayfalarının adını gösteren viewControl
        Control view_MsWebPages = null;
        WebBrowser webTest = null;

        // Test butonları
        Control btn_LineGet = null;   // test get
        Control btn_LinePost = null;  // test set
        Control btn_FullGetTest1 = null;   // test get butonu
        Control btn_FullGetTest2 = null;   // test get butonu
        Control btn_FullPostTest1 = null;  // test set butonu
        Control btn_FullPostTest2 = null;  // test set butonu

        string menuScraping = "MENU_" + "UST/PMS/PMS/WEBSCRAPING";
        string buttonErrorFalse = "buttonErrorFalse";

        string menuName = "MENU_" + "UST/PMS/PMS/SeleniumMebbis";
        string buttonMebbisGiris = "ButtonMebbisGiris";
        string buttonAutoSave = "ButtonOtomatikMebbisKaydet";
        string buttonManuelSave = "ButtonManuelMebbisKaydet";

        DevExpress.XtraBars.Navigation.NavElement buttonManuelSaveControl = null;
        DevExpress.XtraBars.Navigation.NavElement buttonAutoSaveControl = null;


        DevExpress.XtraBars.Navigation.NavButton navButtonMebbisKaydet = null;

        string msWebPages_TableIPCode = string.Empty;

        //IWebDriver webDriver_ = null;

        List<MsWebPage> msWebPage_ = null;
        List<MsWebPage> msWebPages_ = null;
        
        List<MsWebNode> msWebNodes_ = null;
        List<MsWebNode> msWebLoginNodes_ = null;

        List<MsWebScrapingDbFields> msWebScrapingDbFields_ = null;
        List<webNodeItemsList> aktifPageNodeItemsList_ = null;

        webWorkPageNodes workPageNodes_ = new webWorkPageNodes();
        webForm f = new webForm();

        bool yuklendi = false;

        #endregion

        public ms_Selenium()
        {
            InitializeComponent();

            tEventsForm evf = new tEventsForm();

            this.Load += new System.EventHandler(evf.myForm_Load);
            this.Shown += new System.EventHandler(evf.myForm_Shown);
            this.Shown += new System.EventHandler(this.ms_SeleniumAnalysis_Shown);

            this.KeyPreview = true;
        }
        #region Form preparing
        private void ms_SeleniumAnalysis_Shown(object sender, EventArgs e)
        {
            MsWebNodesButtonsPreparing();
            MsWebPagesButtonsPreparing();
            preparingWebPagesViewControl();
            msPagesService.preparingMsWebLoginPage(f, ds_LoginPageNodes, this.msWebLoginNodes_);

            preparingKaydetButons();

            // TileNavMenu buttons
            t.Find_Button_AddClick(this, menuName, buttonMebbisGiris, myNavElementClick);
            t.Find_Button_AddClick(this, menuName, buttonManuelSave, myNavElementClick);
            t.Find_Button_AddClick(this, menuName, buttonAutoSave, myNavElementClick);
            t.Find_NavButton_Control(this, menuName, buttonManuelSave, ref buttonManuelSaveControl);
            t.Find_NavButton_Control(this, menuName, buttonAutoSave, ref buttonAutoSaveControl);
            if (buttonAutoSaveControl != null)
                myNavElementClick(buttonAutoSaveControl, null);


            t.Find_Button_AddClick(this, menuScraping, buttonErrorFalse, myNavElementClick);

            // scraping ilişkisi olan TableIPCode ve ilgili fieldler
            // 
            this.msWebScrapingDbFields_ = msPagesService.readScrapingTablesAndFields(this.msWebPages_);
            
            /// DataSet ve DataNavigatorleri işaretle
            /// 
            msPagesService.preparingDataSets(this, dataNavigator_PositionChanged);

            // web sayfalarının adını gösteren viewControl
            

            string TableIPCode = "UST/PMS/MsWebNodes.Analysis_L01";
            t.Find_DataSet(this, ref this.ds_AnalysisNodes, ref this.dN_AnalysisNodes, TableIPCode);

            if (this.dN_AnalysisNodes != null)
            {
                this.dN_AnalysisNodes.PositionChanged += new System.EventHandler(dN_AnalysisNodes_PositionChanged);
                this.webTest = new WebBrowser();
                this.webTest = (WebBrowser)t.Find_Control(this, "webTest");
            }
            
        }

        private void preparingKaydetButons()
        {
            /// Kaydet butonları
            /// 
            List<Control> list = new List<Control>();
            t.Find_Controls_List(this, "DevExpress.XtraEditors.SimpleButton", ref list);
            foreach (Control button in list)
            {
                if (button.Name.IndexOf("simpleButton_kaydet") > -1)
                {
                        ((DevExpress.XtraEditors.SimpleButton)button).Click += new System.EventHandler(mySaveClick);
                }
            }
        }
        public void mySaveClick(object sender, EventArgs e)
        {
            f.tableIPCodeIsSave = "";
        }
        private void preparingWebMain()
        {
            /*
            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            chromeDriverService.HideCommandPromptWindow = true;
            webMain = new ChromeDriver(chromeDriverService, new ChromeOptions());
            //webMain.Manage().Window.Maximize();
            webMain.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(SeleniumHelper.ImplicitlyWait);
            webMain.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(SeleniumHelper.PageLoadTimeout);
            */
            if (v.webDriver_ == null)
            {
                f.loginPageRun = false;
                SeleniumHelper.ResetDriver();
                v.webDriver_ = SeleniumHelper.WebDriver;
                preparingWebDriverChangeSize(v.webDriver_);
            }
        }
        private void preparingWebDriverChangeSize(IWebDriver wb)
        {
            /// Main Form size change
            /// 
            Form mainF = Application.OpenForms[0];

            Screen screen = Screen.FromControl(mainF); // formun bulunduğu ekranı al
            //string text = screen.DeviceName; // ekranın adını etikete yaz
            bool primary = screen.Primary; // Birinci ekran mı kontrol et 
            
            /// Birinci ekranda ise
            /// 
            if (primary)
            {
                mainF.WindowState = FormWindowState.Normal;
                mainF.Left = 0;
                mainF.Top = 0;
                mainF.Size = new Size(v.Primary_Screen_Width / 2, v.Primary_Screen_Height);

                wb.Manage().Window.Size = new Size(v.Primary_Screen_Width / 2, v.Primary_Screen_Height);
                wb.Manage().Window.Position = new Point(v.Primary_Screen_Width / 2, 0);
            }
            else
            {
                int x = 0;
                int y = 0;

                Screen[] screens = Screen.AllScreens;
                if (screens.Length > 1)
                {
                    // İkinci monitörün sol üst köşesinin koordinatlarını alın
                    //x = screens[1].Bounds.X;
                    y = screens[1].Bounds.Y;
                    x = screens[0].Bounds.Width + screens[1].Bounds.Width / 2;
                }
                
                /// Başka bir ekranda ise
                /// 
                mainF.WindowState = FormWindowState.Normal;
                mainF.Left = screen.Bounds.X;
                mainF.Top = screen.Bounds.Y;
                mainF.Size = new Size(screen.Bounds.Width / 2, screen.Bounds.Height - 50);
                 
                wb.Manage().Window.Position = new Point(x + 10, screen.Bounds.Y);
                wb.Manage().Window.Size = new Size(screen.Bounds.Width / 2, screen.Bounds.Height - 50);
            }

            //Screen[] screens = Screen.AllScreens; // tüm ekranları al
            //if (screens.Length > 1) // birden fazla ekran varsa
            //{
            //    Screen secondScreen = screens[1]; // ikinci ekranı al
            //    this.Location = secondScreen.WorkingArea.Location; // formun konumunu ikinci ekranın çalışma alanının konumuna ayarla
            //}


            /// Main Form size change
            /// 
            //Application.OpenForms[0].WindowState = FormWindowState.Normal;
            //Application.OpenForms[0].Left = 0;
            //Application.OpenForms[0].Top = 0;
            //Application.OpenForms[0].Size = new Size(v.Primary_Screen_Width / 2, v.Primary_Screen_Height);

            /// Form boyutunu değiştirmek için,
            /// Bu özellik, formun genişliğini ve yüksekliğini belirten bir System.Drawing.Size nesnesi alır
            /// driver.Manage().Window.Size = new System.Drawing.Size(800, 600);
            /// 
            //wb.Manage().Window.Size = new Size(v.Primary_Screen_Width / 2, v.Primary_Screen_Height);

            /// Form konumunu değiştirmek için, 
            /// Bu özellik, formun ekranın sol üst köşesine göre x ve y koordinatlarını belirten bir System.Drawing.Point nesnesi alır
            /// driver.Manage().Window.Position = new System.Drawing.Point(0, 0);
            /// 
            //wb.Manage().Window.Position = new Point(v.Primary_Screen_Width / 2, 0);

            /// Formu tam ekran yapmak için, 
            /// Bu metot, formu mevcut ekran çözünürlüğüne göre en büyük boyuta getirir
            /// driver.Manage().Window.Maximize();
            /// 
        }

        #region MsWebPagesButtonsPreparing
        private void MsWebPagesButtonsPreparing()
        {
            /// MsWebPages tablosu
            /// 

            msWebPages_TableIPCode = t.Find_TableIPCode(this, "MsWebPages");

            if (t.IsNotNull(msWebPages_TableIPCode) == false) return;
            
            t.Find_DataSet(this, ref ds_MsWebPages, ref dN_MsWebPages, msWebPages_TableIPCode);

            if (t.IsNotNull(ds_MsWebPages))
            {
                f.Clear();
                f.tForm = this;
                f.browserType = v.tBrowserType.Selenium;
                
                dN_MsWebPages.PositionChanged += new System.EventHandler(dNScrapingPages_PositionChanged);
                msPagesService.preparingMsWebPages(ds_MsWebPages, ref msWebPages_);
                msPagesService.preparingMsWebNodesFields(this, 
                  ref this.msWebPage_,
                  ref this.msWebNodes_,
                  ref this.workPageNodes_,
                  this.msWebScrapingDbFields_,
                  ds_MsWebPages,
                  ds_MsWebNodes,
                  dN_MsWebPages );

                /// Analysis butonu için
                msPagesService.preparingMsWebPagesButtons(this, f, "UST/PMS/MsWebNodes.Analysis_L01");
                /// standart pageler için butonları tespit et
                msPagesService.preparingMsWebPagesButtons(this, f, msWebPages_TableIPCode);
                
                preparingMsWebPagesButtons_();
            }
        }
        private void preparingMsWebPagesButtons_()
        {
            if (f.btn_PageView != null)
                ((DevExpress.XtraEditors.SimpleButton)f.btn_PageView).Click += new System.EventHandler(myPageViewClick);
            if (f.btn_Analysis != null)
                ((DevExpress.XtraEditors.SimpleButton)f.btn_Analysis).Click += new System.EventHandler(myPageAnalysisClick);


            // Bilgileri Sorgula / AlwaysSet  (TcNo sorgula gibi) // Bu henüz hiç kullanılmadı 
            if (f.btn_AlwaysSet != null)
                ((DevExpress.XtraEditors.SimpleButton)f.btn_AlwaysSet).Click += new System.EventHandler(myAlwaysSetClick);
            // Bilgileri Al 1
            if (f.btn_FullGet1 != null)
                ((DevExpress.XtraEditors.SimpleButton)f.btn_FullGet1).Click += new System.EventHandler(myFullGet1Click);
            // Bilgileri Al 2
            if (f.btn_FullGet2 != null)
                ((DevExpress.XtraEditors.SimpleButton)f.btn_FullGet2).Click += new System.EventHandler(myFullGet2Click);
            // Bilgileri Gönder 1
            if (f.btn_FullPost1 != null)
                ((DevExpress.XtraEditors.SimpleButton)f.btn_FullPost1).Click += new System.EventHandler(myFullPost1Click);
            // Bilgileri Gönder 2
            if (f.btn_FullPost2 != null)
                ((DevExpress.XtraEditors.SimpleButton)f.btn_FullPost2).Click += new System.EventHandler(myFullPost2Click);
            // Bilgileri Kaydet
            if (f.btn_FullSave != null)
                ((DevExpress.XtraEditors.SimpleButton)f.btn_FullSave).Click += new System.EventHandler(myFullSaveClick);
            // Otomatik kaydet için
            if (f.btn_AutoSubmit != null)
                ((DevExpress.XtraEditors.SimpleButton)f.btn_AutoSubmit).Click += new System.EventHandler(myAutoSubmit);
        }
        #endregion MsWebPagesButtonsPreparing

        private void MsWebNodesButtonsPreparing()
        {
            /// MsWebNodes tablosu
            /// 
            string TableIPCode = t.Find_TableIPCode(this, "MsWebNodes");

            if (t.IsNotNull(TableIPCode) == false)
            {
                MessageBox.Show("DİKKAT : MsWebNodes tablosu bulunamadı...(Form bilgisi olmayabilir... ( MS_LAYOUT )) ");
                return;
            }
            t.Find_DataSet(this, ref ds_MsWebNodes, ref dN_MsWebNodes, TableIPCode);

            /// Selenium ve CEFSharp için henüz gerek olmadı, Microsoft.WebBrowser de kulanıldı
            ///

            string[] controls = new string[] { };

            btn_LineGet = t.Find_Control(this, "simpleButton_ek1", TableIPCode, controls);
            if (btn_LineGet != null)
            {
                ((DevExpress.XtraEditors.SimpleButton)btn_LineGet).Click += new System.EventHandler(myLineGetTestClick);
            }
            btn_LinePost = t.Find_Control(this, "simpleButton_ek2", TableIPCode, controls);
            if (btn_LinePost != null)
            {
                ((DevExpress.XtraEditors.SimpleButton)btn_LinePost).Click += new System.EventHandler(myLinePostTestClick);
            }

            btn_FullGetTest1 = t.Find_Control(this, "simpleButton_ek3", TableIPCode, controls);
            if (btn_FullGetTest1 != null)
            {
                ((DevExpress.XtraEditors.SimpleButton)btn_FullGetTest1).Click += new System.EventHandler(myFullGet1Click);
            }
            btn_FullGetTest2 = t.Find_Control(this, "simpleButton_ek4", TableIPCode, controls);
            if (btn_FullGetTest2 != null)
            {
                ((DevExpress.XtraEditors.SimpleButton)btn_FullGetTest2).Click += new System.EventHandler(myFullGet2Click);
            }

            btn_FullPostTest1 = t.Find_Control(this, "simpleButton_ek5", TableIPCode, controls);
            if (btn_FullPostTest1 != null)
            {
                ((DevExpress.XtraEditors.SimpleButton)btn_FullPostTest1).Click += new System.EventHandler(myFullPost1Click);
            }
            btn_FullPostTest2 = t.Find_Control(this, "simpleButton_ek6", TableIPCode, controls);
            if (btn_FullPostTest2 != null)
            {
                ((DevExpress.XtraEditors.SimpleButton)btn_FullPostTest2).Click += new System.EventHandler(myFullPost2Click);
            }
            
        }
        private void preparingWebPagesViewControl()
        {
            string TableIPCode = t.Find_TableIPCode(this, "MsWebPages");
            view_MsWebPages = t.Find_Control_View(this, TableIPCode);
            if (view_MsWebPages != null)
            {
                string type = view_MsWebPages.GetType().ToString();
                if (type == "DevExpress.XtraGrid.GridControl")
                {
                    DevExpress.XtraGrid.GridControl gridControl = (DevExpress.XtraGrid.GridControl)view_MsWebPages; 

                    string viewName = gridControl.MainView.GetType().ToString();
                    
                    if (viewName == "DevExpress.XtraGrid.Views.Tile.TileView")
                    {
                        DevExpress.XtraGrid.Views.Tile.TileView tTileView = (DevExpress.XtraGrid.Views.Tile.TileView)gridControl.MainView;
                        tTileView.ItemClick += new DevExpress.XtraGrid.Views.Tile.TileViewItemClickEventHandler(myTileView_ItemClick);
                    }
                }
            }
        }
        private async void myNavElementClick(object sender, DevExpress.XtraBars.Navigation.NavElementEventArgs e)
        {
            if (sender.GetType().ToString() == "DevExpress.XtraBars.Navigation.NavButton")
            {
                if (((DevExpress.XtraBars.Navigation.NavButton)sender).Name == buttonMebbisGiris)
                {
                    await seleniumLoginPageViev();
                }
                if (((DevExpress.XtraBars.Navigation.NavButton)sender).Name == buttonManuelSave)
                {
                    f.autoSubmit = false;
                    buttonManuelSaveControl.Appearance.BackColor = v.colorAutoSave;
                    buttonManuelSaveControl.Appearance.ForeColor = v.AppearanceTextColor;

                    buttonAutoSaveControl.Appearance.BackColor = ((DevExpress.XtraBars.Navigation.NavButton)sender).Appearance.BackColor2;
                    buttonAutoSaveControl.Appearance.ForeColor = ((DevExpress.XtraBars.Navigation.NavButton)sender).AppearanceSelected.ForeColor;
                }
                if (((DevExpress.XtraBars.Navigation.NavButton)sender).Name == buttonAutoSave)
                {
                    f.autoSubmit = true;
                    buttonManuelSaveControl.Appearance.BackColor = ((DevExpress.XtraBars.Navigation.NavButton)sender).Appearance.BackColor2;
                    buttonManuelSaveControl.Appearance.ForeColor = ((DevExpress.XtraBars.Navigation.NavButton)sender).AppearanceSelected.ForeColor;

                    buttonAutoSaveControl.Appearance.BackColor = v.colorAutoSave;
                    buttonAutoSaveControl.Appearance.ForeColor = v.AppearanceTextColor;
                }


                //buttonErrorFalse
                if (((DevExpress.XtraBars.Navigation.NavButton)sender).Name == buttonErrorFalse)
                {
                    f.anErrorOccurred = false;
                    t.FlyoutMessage(this, "Bilgilendirme", "Hata durumu sıfırlandı...");
                }


                if (f.btn_FullSave != null)
                    f.btn_FullSave.Visible = !f.autoSubmit;
            }

            if (sender.GetType().ToString() == "DevExpress.XtraBars.Navigation.TileNavItem")
            {
                //if (((DevExpress.XtraBars.Navigation.TileNavItem)sender).Name == buttonInsertPaketOlustur) InsertPaketOlustur();
            }
        }


        public async void myTileView_ItemClick(object sender, DevExpress.XtraGrid.Views.Tile.TileViewItemClickEventArgs e)
        {
            if (t.IsNotNull(ds_MsWebPages) == false) return;
            if (dN_MsWebPages.Position == -1) return;

            if (v.webDriver_ != null)
            {
                try
                {
                    int adet = v.webDriver_.WindowHandles.Count;
                    var title = v.webDriver_.Title;
                }
                catch (Exception)
                {
                    t.AlertMessage("Uyarı", "Web tarayıcısı kapatılmış.");
                    v.webDriver_ = null;
                    f.loginPageUrl = "";
                    f.loginPageRun = false;
                }
            }

            /// burası 

            bool onay = false;
            if (v.webDriver_ == null)// && (dN_MsWebPages.Position == 0)
                onay = msPagesService.LoginOnayi(ds_MsWebPages, dN_MsWebPages, f);

            if (onay)
            {
                t.WaitFormOpen(this, "Google Chrome yükleniyor ...");
                await seleniumLoginPageViev();
                v.IsWaitOpen = false;
                t.WaitFormClose();
                this.yuklendi = true;
            }
        }
        
        #region Test buttons click
        private async void myLineGetTestClick(object sender, EventArgs e)
        {
            if (ds_MsWebNodes != null)
            {
                webNodeValue wnv = new webNodeValue();
                wnv.workRequestType = v.tWebRequestType.get;

                DataRow row = ds_MsWebNodes.Tables[0].Rows[dN_MsWebNodes.Position];

                msPagesService.nodeValuesPreparing(row, ref wnv, f.aktifPageCode);
                                
                // node nin items okunacak (getNodeItems)
                // sonrada MsWebNodeItems tablosuna yazılacak  
                if (wnv.TagName == "select")
                    wnv.workRequestType = v.tWebRequestType.getNodeItems;

                //await WebScrapingAsync(v.webDriver_, wnv);
                await msScraping.WebScrapingAsync(wnv, f);

                if (wnv.TagName == "select")
                {
                    /// select node ye ait (value, text) listesinide MsWebNodeItems tablosuna yaz
                    //transferFromWebSelectToDatabase(wnv);
                    //MessageBox.Show("İşlem tamamlandı...");
                }
            }
        }
        private async void myLinePostTestClick(object sender, EventArgs e)
        {
            if (ds_MsWebNodes != null)
            {
                webNodeValue wnv = new webNodeValue();
                wnv.workRequestType = v.tWebRequestType.post;

                DataRow row = ds_MsWebNodes.Tables[0].Rows[dN_MsWebNodes.Position];

                msPagesService.nodeValuesPreparing(row, ref wnv, f.aktifPageCode);

                // node nin items okunacak (getNodeItems)
                // sonrada MsWebNodeItems tablosuna yazılacak  
                //if (wnv.TagName == "select")
                //    wnv.workRequestType = v.tWebRequestType.getNodeItems;

                //await WebScrapingAsync(v.webDriver_, wnv);
                await msScraping.WebScrapingAsync(wnv, f);

                //if (wnv.TagName == "select")
                //{
                /// select node ye ait (value, text) listesinide MsWebNodeItems tablosuna yaz
                //transferFromWebSelectToDatabase(wnv);
                //MessageBox.Show("İşlem tamamlandı...");
                //}
            }
        }

        private void myFullGetTest1Click(object sender, EventArgs e)
        {

        }
        private void myFullGetTest2Click(object sender, EventArgs e)
        {

        }
        private void myFullPostTest1Click(object sender, EventArgs e)
        {

        }
        private void myFullPostTest2Click(object sender, EventArgs e)
        {

        }
        
        #endregion Test buttons click

        private async void dNScrapingPages_PositionChanged(object sender, EventArgs e)
        {
            v.con_EditSaveControl = true;
            v.con_EditSaveCount = 0;

            await subPageOpenControl(v.webDriver_, "");

            preparingMsWebNodesFields();
            preparingAktifPageLoad();

            msPagesService.scrapingPages_PositionChanged(this.workPageNodes_, this.f);
        }
        private async void dataNavigator_PositionChanged(object sender, EventArgs e)
        {
            /// 
            if (f.tableIPCodesInLoad != "")
            {
                object tDataTable = ((DevExpress.XtraEditors.DataNavigator)sender).DataSource;
                DataSet dsData = ((DataTable)tDataTable).DataSet;
                string tableIPCode_ = "";
                if (dsData.DataSetName != null)
                    tableIPCode_ = dsData.DataSetName.ToString();
                
                if ((f.tableIPCodesInLoad.IndexOf(tableIPCode_) > -1) && 
                    (f.tableIPCodeIsSave == "") && // save olan Dataset ise atla
                    (v.con_Listele_TableIPCode != tableIPCode_)
                    ) 
                {
                    t.WaitFormOpen(this, "Web sayfası değiştiriliyor ...");
                    f.loadWorking = false;
                    f.pageRefreshWorking = true;
                    try
                    {
                        await startNodes(this.msWebNodes_, this.workPageNodes_, v.tWebRequestType.post, v.tWebEventsType.load);
                    }
                    catch (Exception)
                    {
                        //
                    }
                    f.loadWorking = true;
                    v.IsWaitOpen = false;
                    t.WaitFormClose();
                }
            }
        }
        private async void preparingAktifPageLoad()
        {
            if (v.webDriver_ != null)
            {
                await myPageViewClickAsync(v.webDriver_, this.msWebPage_);
            }
        }
        private void preparingMsWebNodesFields()
        {
            this.f.Clear();
            this.f.tForm = this;
            this.f.browserType = v.tBrowserType.Selenium;
            
            this.msWebPage_ = t.RunQueryModelsSingle<MsWebPage>(ds_MsWebPages, dN_MsWebPages.Position);
            this.msWebNodes_ = t.RunQueryModels<MsWebNode>(ds_MsWebNodes);
            this.workPageNodes_.Clear();
            this.workPageNodes_.aktifPageCode = this.msWebPage_[0].PageCode;
            this.workPageNodes_.aktifPageUrl = this.msWebPage_[0].PageUrl;

            msPagesService.checkedSiraliIslemVarmi(this, this.workPageNodes_, this.msWebScrapingDbFields_);
            //this.btn_SiraliIslem = this.workPageNodes_.siraliIslem_Btn;
        }
        

        #endregion Form preparing
        /// 
        /// Userın kullanığı butonlar
        /// 
        #region user buttons
        private async void myPageViewClick(object sender, EventArgs e)
        {
            await seleniumLoginPageViev();
        }
        private async void myPageAnalysisClick(object sender, EventArgs e)
        {
            await seleniumAnalysis();
        }
        private async Task seleniumLoginPageViev()
        {
            /// Firmanın Mebbis konu ve şifresini yeniden oku
            /// değiştirmiş olabilir
            /// 
            tUserFirms userFirms = new tUserFirms();
            userFirms.getFirmAboutWithUserFirmGUID(v.tMainFirm.FirmGuid);
                
            /// Kullanıcının mebbisCode ve şifresini yeniden oku
            /// değiştirmiş olabilir
            /// 
            msPagesService.getMebbisCode();
            
            f.Clear();
            f.tForm = this;
            f.browserType = v.tBrowserType.Selenium;

            if (v.webDriver_ == null)
                preparingWebMain();

            await myPageViewClickAsync(v.webDriver_, this.msWebPage_);

            //await myPageViewClickAsync(f.wbSel, this.msWebPage_);
        }
        private async Task seleniumAnalysis()
        {
            if (v.webDriver_ != null)
            {
                string pageSource = v.webDriver_.PageSource;
                HtmlAgilityPack.HtmlNode htmlBody = null;
                loadBody(pageSource, ref htmlBody);

                f.analysisNodeId = 1;
                f.analysisParentId = 0;

                if (htmlBody != null)
                    msPagesService.listNode_(htmlBody.ChildNodes, this.ds_AnalysisNodes, f);
            }
        }

        private void loadBody(string htmlDocumentBody, ref HtmlAgilityPack.HtmlNode htmlBody)
        {
            /// htmlDocumentBody ile okunan sayfanın html yapısı string olarak geliyor
            /// burada herbir tag node nesnesine dönüşüyor
            /// 
            if (string.IsNullOrEmpty(htmlDocumentBody)) return;
            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(htmlDocumentBody);
            htmlBody = htmlDoc.DocumentNode.SelectSingleNode("//body");

            // tüm page bilgisi alındıktan (head+body) sonra bu şekilde body e ulaşılmaya çalışıldığında 
            // input nodeler gelmiyor
            //
            //HtmlAgilityPack.HtmlNode htmlBody = htmlDoc.DocumentNode.SelectSingleNode("//body");
        }
        private void myAlwaysSetClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            //startNodesBefore();
            //startNodesRun(ds_MsWebNodes, v.tWebRequestType.alwaysSet, v.tWebEventsType.buttonAlwaysSet);
        }

        private async void viewFinalMessage()
        {
            /// tableIPCodesInLoad view lerin enabled = true yap
            await preparingViewControls(true);
            ///
            if (f.anErrorOccurred == false)
                t.FlyoutMessage(this, "Bilgilendirme", "İşlem tamamlandı...");
            else t.AlertMessage("Uyarı", "İşleminiz gerçekleştirilemedi...");
        }
        private async void myFullGet1Click(object sender, EventArgs e)
        {
            // test
            //webNodeValue wnv = new webNodeValue();
            //msScraping.preparingImageFile("", wnv);

            Cursor.Current = Cursors.WaitCursor;
            f.anErrorOccurred = false;
            try
            {
                await startNodes(this.msWebNodes_, this.workPageNodes_, v.tWebRequestType.get, v.tWebEventsType.button3);
            }
            catch (Exception)
            {
                //throw;
            }
            viewFinalMessage();
        }

        private async void myFullGet2Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            f.anErrorOccurred = false;
            try
            {
                await startNodes(this.msWebNodes_, this.workPageNodes_, v.tWebRequestType.get, v.tWebEventsType.button4);
            }
            catch (Exception)
            {
                //throw;
            }
            viewFinalMessage();
        }

        private async void myFullPost1Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            f.anErrorOccurred = false;
            try
            {
                await startNodes(this.msWebNodes_, this.workPageNodes_, v.tWebRequestType.post, v.tWebEventsType.button5);
            }
            catch (Exception)
            {
                //throw;
            }
            viewFinalMessage();
        }

        private async void myFullPost2Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            f.anErrorOccurred = false;
            try
            {
                await startNodes(this.msWebNodes_, this.workPageNodes_, v.tWebRequestType.post, v.tWebEventsType.button6);
            }
            catch (Exception)
            {
                //throw;
            }
            viewFinalMessage();
        }

        private async void myFullSaveClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            await startNodes(this.msWebNodes_, this.workPageNodes_, v.tWebRequestType.post, v.tWebEventsType.button7);
        }
        private void myAutoSubmit(object sender, EventArgs e)
        {
            //Cursor.Current = Cursors.WaitCursor;
            //startNodesRun(ds_MsWebNodes, v.tWebRequestType.post, v.tWebEventsType.button7);
        }
        #endregion user buttons

        #region Scraping functions               
        private async Task startNodes(List<MsWebNode> msWebNodes, webWorkPageNodes workPageNodes,  v.tWebRequestType workRequestType, v.tWebEventsType workEventsType)
        {
            bool onayList = false;
            bool onayRequest = false;
            bool onayPageRefresh = false;
            bool onayValue = false;
            bool onaySiraliIslem = false;
            bool onayWebScrapingAfter = true;

            f.tableIPCodeIsSave = "";
            f.wbSel = v.webDriver_;
            //f.tableIPCodesInLoad = ""; Açma 

            msPagesService.driverControl(f);            

            if (f.anErrorOccurred)
            {
                /// tableIPCodesInLoad view lerin enabled = true yap
                await preparingViewControls(true);
                return;
            }

            /// daha çalışanları temizle yani
            /// load çalıştı diyelim
            /// kullanıcı veri gönder/veri al talebinde bulununca 
            /// loada ait çalışma listesini temizle 
            ///
            workPageNodes.nodeIdList = "";
            /// kullanıcı tekrar istekte bulundu onun için hatayı kapat ve 
            /// save işlemini sıfırla
            /// 


            /// şimdilik emanet burada dursun doğru yeri bulunca değiştirirsin
            msScraping.KlasorIcindekileriSil(v.EXE_GIBDownloadPath);

            /// tableIPCodesInLoad view lerin enabled = false yap
            await preparingViewControls(false);

            //if (f.tableIPCodesInLoad != "")

            foreach (MsWebNode item in msWebNodes)
            {
                /// hata var ise
                if (f.anErrorOccurred) break;
                /// save sırasında tekrar eden kayıtların kesilmesi istenirse
                if (onayWebScrapingAfter == false) break;

                // 2. adımda 
                // IsActive = 1, nodeIdList += item.Id
                // 
                onayList = listControl(item, workPageNodes);

                // 3. adımda
                // v.tWebRequestType workRequestType,
                // v.tWebEventsType  workEventsType,
                // eventsType = (v.tWebEventsType)t.myInt16(row["EventsType"].ToString());
                // injectType = (v.tWebInjectType)t.myInt16(row["InjectType"].ToString());
                // kontrolleri
                onayRequest = requestContol(item, workRequestType, workEventsType);
                                
                // PageRefresh satırımı kontrol ediliyor
                // pageRefresh ise sub functionda çalışsın ve false dönsün
                // true dönerse pageRefresh değil işleme devam et
                onayPageRefresh = await pageRefresh(v.webDriver_, item);

                if ((onayList) && (onayRequest) && (onayPageRefresh))
                {
                    webNodeValue wnv = new webNodeValue();
                    wnv.workRequestType = workRequestType;
                    wnv.workEventsType = workEventsType;
                    wnv.pageCode = workPageNodes.aktifPageCode;
                    msPagesService.preparingNodeValues(item, ref wnv);

                    /// Başka bir NodeId nin valuesini okuyacak
                    /// 
                    if (wnv.CheckNodeId > 0)
                    {
                        wnv.CheckReadValue = await msPagesService.getCheckNodeIdValue(msWebNodes, wnv.CheckNodeId, f);
                    }

                    // 4. adım 
                    //
                    // WebScrapingAsync öncesi (set için) yapılacak işler
                    // bazı atamalar olumsuz olabiliyor, örn : resim datası yok ise
                    //
                    onayValue = await WebScrapingBefore(wnv);

                    // OperandControl kriter karşılaştırma var mı?
                    if (onayValue)
                        onayValue = valueChecked(wnv);

                    // 5. adım scraping
                    // WebScrapingAsync işlemi yapılsın
                    //
                    if (onayValue)
                    {
                        //await WebScrapingAsync(v.webDriver_, wnv);
                        await msScraping.WebScrapingAsync(wnv, f);

                        // 4. adım
                        //
                        // WebScrapingAsync sonrası (get için save) yapılacak işler

                        // save sırasında tekrar eden kayıtların kesilmesi istenirse
                        onayWebScrapingAfter = await WebScrapingAfter(wnv);
                    }
                }
            }

            /// Sıralı islem butonu çalışıcak
            /// 
            if ((workEventsType == v.tWebEventsType.button3) ||
                (workEventsType == v.tWebEventsType.button4) ||
                (workEventsType == v.tWebEventsType.button5) ||
                (workEventsType == v.tWebEventsType.button6))
            {
                if (((workRequestType == v.tWebRequestType.get || workRequestType == v.tWebRequestType.post))   &&
                    (onayWebScrapingAfter) &&
                    (workPageNodes.siraliIslemVar) &&
                    (f.anErrorOccurred == false))
                {
                    /// kullanıcı onayı sıralı işlemi iptal edebilir
                    workPageNodes.siraliIslemAktif = ((DevExpress.XtraEditors.CheckButton)workPageNodes.siraliIslem_Btn).Checked;

                    if (workPageNodes.siraliIslemAktif)
                    {
                        onaySiraliIslem = nextDataSetRow(workPageNodes);
                        if (onaySiraliIslem)
                            await startNodes(msWebNodes, workPageNodes, workRequestType, workEventsType);
                    }
                }
            }

            /// tableIPCodesInLoad view lerin enabled = true yap
            await preparingViewControls(true);

            /// iş bitiminde de sıfırlamak gerekiyor.
            /// Örnek : Tarih al  dan sonra yeni tarihlere göre sayfalar değişmiyor
            /// 
            f.tableIPCodeIsSave = "";
        }
        private bool nextDataSetRow(webWorkPageNodes workPageNodes)
        {
            bool onay = false;
            DataSet ds = workPageNodes.siraliIslem_ds;
            DataNavigator dN = workPageNodes.siraliIslem_dN;
            if (ds != null)
            {
                if (dN.Tag == null) dN.Tag = -1;
                //if ((int)dN.Tag == dN.Position) return false;
                if (ds.Tables[0].Rows.Count == (int)dN.Tag + 1)
                {
                    t.FlyoutMessage(this, "Bilgilendirme", "Sıralı atma işlemi tamamlandı...");
                    return false;
                }
                NavigatorButton btnNext = dN.Buttons.Next;
                dN.Buttons.DoClick(btnNext);
                dN.Tag = dN.Position;
                Application.DoEvents();
                onay = true;
            }
            return onay;
        }
        private async Task<bool> pageRefresh(IWebDriver wb, MsWebNode item)
        {
            bool onay = true;
            if ((item.PageRefresh == true) &&
                (item.IsActive))
            {
                /// PageRefresh isteği şimdilik sadece DataNavigator position değişikliğinde aktif ediliyor
                /// 
                if (f.pageRefreshWorking)
                {
                    f.pageRefreshWorking = false;
                    await myPageViewClickAsync(wb, this.msWebPage_);
                    /// pageRefresh satırı (item) buarda çalıştı, dönüşte tekrar çalışmasın diye false olarak geri dönüyor
                    /// 
                    onay = false;
                }
            }
            return onay; 
        }
        private async Task<bool> WebScrapingBefore(webNodeValue wnv)
        {
            /// database deki veriyi web e aktar için 
            /// db deki veriyi wnv.writeValue üzerine aktar
            /// her işlem için onay ver, 
            /// eğer hatalı bir durum oluşursa onaylama
            /// 
            bool onay = true;

            if (wnv.InjectType == v.tWebInjectType.Set ||
                wnv.InjectType == v.tWebInjectType.AlwaysSet ||
               (wnv.InjectType == v.tWebInjectType.GetAndSet && wnv.workRequestType == v.tWebRequestType.post))
            {
                if (wnv.TagName != "table")
                    onay = msPagesService.transferFromDatabaseToWeb(this, wnv, msWebScrapingDbFields_);

                if (wnv.TagName == "table")
                {
                    /// webe transfer edilecek tablonun field bilgilerini databaseden al
                    //if (wnv.tTable == null) // değişti
                    if (wnv.dynamicTable == null)
                    {
                       msPagesService.findRightDbTables(wnv, msWebScrapingDbFields_);
                    }
                    onay = true;
                }

                /// Load işlemleri sırasında ve set işlemi için bir dataSet ten bilgi alındı ise
                /// kullanıcı bu dataSet üzerinde position değiştirdiğinde 
                /// bulunduğu web sayfası tekrar yenilenmesi gerekiyor
                /// Örnek : kullanıcı SınavTarih listesinde pos değiştirince Mebbis sinav listesi sayfasıda ona göre değişmesi gerekiyor
                /// 
                if ((wnv.workEventsType == v.tWebEventsType.load) && (onay)) 
                {
                    if (wnv.ds != null)
                        if (wnv.ds.DataSetName != null)
                        {
                            string code = wnv.ds.DataSetName.ToString();
                            if (f.tableIPCodesInLoad.IndexOf(code) == -1)
                            {
                                f.tableIPCodesInLoad += "||" + code + "||";
                            }
                        }
                }

                if ((wnv.workEventsType == v.tWebEventsType.load) && (onay == false))
                    onay = true;

                /// Özel istisnai durumlar
                if (onay == false)
                {
                    // item item ile çalışma eventi aynı olmalı : örn : button5 == button5
                    if ((wnv.EventsType == wnv.workEventsType) ||
                        (wnv.EventsType == v.tWebEventsType.none))
                    {
                        /// kayıt butonu ise onayla
                        if (wnv.EventsType == v.tWebEventsType.button7) onay = true;
                        if (wnv.AttRole == "Button") onay = true;
                        if (wnv.AttType == "submit") onay = true;
                        if (wnv.InvokeMember == v.tWebInvokeMember.autoSubmit) onay = true;
                        if (t.IsNotNull(wnv.writeValue)) onay = true; //
                        if (wnv.TagName == "div") onay = true;  // testresult yüzünden eklendi
                        if (wnv.TagName == "span") onay = true; // kayıt sonucunu yazan element
                    }
                }
            }

            if (wnv.writeValue != null)
                if (wnv.writeValue.IndexOf("Error") > -1)
                    f.anErrorOccurred = true;
            
            return onay;
        }
        private async Task<bool> WebScrapingAfter(webNodeValue wnv)
        {
            //  web deki veriyi database aktar
            //  webden alınan veriyi (readValue yi)  db ye aktar
            bool onay = true;

            if ((wnv.InjectType == v.tWebInjectType.Get ||
                (wnv.InjectType == v.tWebInjectType.GetAndSet && wnv.workRequestType == v.tWebRequestType.get)) &&
                (wnv.IsInvoke == false)) //(this.myTriggerInvoke == false)) // invoke gerçekleşmişse hiç başlama : get sırasında set edip bilgi çağrılıyor demekki
            {

                if (wnv.TagName != "table" && wnv.AttRole != "GIBeArsivFaturaIndir")
                    msPagesService.transferFromWebToDatabase(this, wnv, msWebScrapingDbFields_, aktifPageNodeItemsList_, f);

                if (wnv.TagName == "table")
                {
                    /// okunan tabloyu db ye yaz
                    //if (wnv.tTable != null) // değişti
                    if (wnv.dynamicTable != null || wnv.htmlTable != null)
                    {
                        ////https://mebbis.meb.gov.tr/SKT/skt02006.aspx
                        //if (webMain.Url.ToString() == "https://mebbis.meb.gov.tr/SKT/skt02006.aspx")
                        //{
                        //    vUserInputBox iBox = new vUserInputBox();
                        //    iBox.Clear();
                        //    iBox.title = "Kaydın başlamasını istediğiniz sıra no";
                        //    iBox.promptText = "Sıra No  :";
                        //    iBox.value = "1";
                        //    iBox.displayFormat = "";
                        //    iBox.fieldType = 0;

                        //    // ınput box ile sorulan ise kimden kopyalanacak (old) bilgisi
                        //    if (t.UserInpuBox(iBox) == DialogResult.OK)
                        //    {
                        //        this.kayitBaslangisId = int.Parse(iBox.value);
                        //    }
                        //}
                        //v.con_EditSaveCount = 0;
                        //this.myTriggeringTable = true;
                        //this.myTriggerTableWnv = null;
                        //this.myTriggerTableWnv = wnv.Copy();

                        //this.myTriggeringItemButton = false;
                        //this.myTriggerTableCount = wnv.tTable.tRows.Count;
                        //this.myTriggerTableRowNo = 0;

                        webNodeValue myTriggerTableWnv = wnv.Copy();

                        if (myTriggerTableWnv.dynamicTable != null)
                            onay = await msPagesService.transferFromWebTableToDatabase(this, myTriggerTableWnv, msWebNodes_, msWebScrapingDbFields_, aktifPageNodeItemsList_);

                        if (myTriggerTableWnv.htmlTable != null)
                            onay = await msPagesService.transferFromWebTableToDatabaseFast(this, myTriggerTableWnv, msWebNodes_, msWebScrapingDbFields_, aktifPageNodeItemsList_);

                        t.TableRefresh(this, myTriggerTableWnv.TableIPCode);
                    }
                }

                if (wnv.TagName == "select")
                {
                    /// okunan tabloyu db ye yaz
                    if (wnv.dynamicTable != null)
                    {
                        if (wnv.dynamicTable.Count > 0)
                        {
                            /// select node ye ait (value, text) listesinide ilgili tabloya yaz
                            msPagesService.transferFromWebSelectToDatabase(this, wnv, msWebScrapingDbFields_, aktifPageNodeItemsList_, f);
                        }
                    }
                    if (wnv.htmlTable != null)
                    {
                        if (wnv.htmlTable.Count > 0)
                        {
                            /// select node ye ait (value, text) listesinide ilgili tabloya yaz
                            msPagesService.transferFromWebSelectToDatabaseFast(this, wnv, msWebScrapingDbFields_, aktifPageNodeItemsList_, f);
                        }
                    }

                    t.TableRefresh(this, wnv.TableIPCode);
                }

                if (wnv.AttRole == "GIBeArsivFaturaIndir")
                {
                    /// okunan tabloyu db ye yaz
                    //if (wnv.tTable != null) // değişti
                    if (wnv.dynamicTable != null)
                    {
                        webNodeValue myTriggerTableWnv = wnv.Copy();

                        onay = await msPagesService.transferFromWebTableToDatabase(this, myTriggerTableWnv, msWebNodes_, msWebScrapingDbFields_, aktifPageNodeItemsList_);

                        t.TableRefresh(this, myTriggerTableWnv.TableIPCode);
                    }
                }
            }
            return onay;

        }
        
        private bool listControl(MsWebNode item, webWorkPageNodes workPageNodes)
        {
            // 1. aktif mi
            // 2. daha önce çalışmış mı çalışmamış mı ?
            bool onay = true;
            
            onay = item.IsActive;

            // debug durdurmak için
            //if (item.NodeId.ToString() == "3372")
            //    onay = true;

            if (onay)
            {
                // nodeId ile bu satır çalıştı mı diye kontrol için sadece
                string nodeNo = "|" + item.NodeId.ToString() + "|";
                int pos = workPageNodes.nodeIdList.IndexOf(nodeNo);
                // -1 ise daha önce çalışmamış 
                if (pos == -1)
                    workPageNodes.nodeIdList += nodeNo;
                else onay = false;
            }

            return onay;
        }
        private bool requestContol(MsWebNode item, v.tWebRequestType workRequestType, v.tWebEventsType workEventsType)
        {
            /// istek çeşitleri
            /// startNodes(v.tWebRequestType.get,  v.tWebEventsType.button3);
            /// startNodes(v.tWebRequestType.get,  v.tWebEventsType.button4);
            /// startNodes(v.tWebRequestType.post, v.tWebEventsType.button5);
            /// startNodes(v.tWebRequestType.post, v.tWebEventsType.button6);
            /// startNodes(v.tWebRequestType.post, v.tWebEventsType.button7);

            /// tWebInjectType
            /// none,
            /// AlwaysSet,
            /// Get,
            /// Set,
            /// GetAndSet

            /// tWebRequestType
            /// none,
            /// get,
            /// post,
            /// put,
            /// delete,
            /// getNodeItems,
            /// postNodeItems,
            /// alwaysSet

            /// tWebEventsType
            /// none = 0,
            /// load = 1,
            /// displayNone = 2,
            /// buttonAlwaysSet = 140,
            /// button3 = 143, 
            /// button4 = 144, 
            /// button5 = 145, 
            /// button6 = 146, 
            /// button7 = 147, 
            /// tableField = 148, 
            /// pageRefresh = 200

            bool onay = false;

            v.tWebInjectType injectType_ = (v.tWebInjectType)item.InjectType;
            v.tWebEventsType eventsType_ = (v.tWebEventsType)item.EventsType;
                        
            // istek load ise ve
            // event = node load ise
            if (workEventsType == v.tWebEventsType.load)
                if (eventsType_ ==  v.tWebEventsType.load) onay = true;

            // istek button3 ise
            // item.event = button3 veya none ise : true
            if (workEventsType == v.tWebEventsType.button3)
                if (eventsType_ == v.tWebEventsType.button3 || eventsType_ == v.tWebEventsType.none) onay = true;
            if (workEventsType == v.tWebEventsType.button4)
                if (eventsType_ == v.tWebEventsType.button4 || eventsType_ == v.tWebEventsType.none) onay = true;
            if (workEventsType == v.tWebEventsType.button5)
                if (eventsType_ == v.tWebEventsType.button5 || eventsType_ == v.tWebEventsType.none) onay = true;
            if (workEventsType == v.tWebEventsType.button6)
                if (eventsType_ == v.tWebEventsType.button6 || eventsType_ == v.tWebEventsType.none) onay = true;
            
            if (workEventsType == v.tWebEventsType.button7)
                if (eventsType_ == v.tWebEventsType.button7) onay = true;

            // post işlemleri ve autoSubmit var ise
            if ((workEventsType == v.tWebEventsType.button5) ||
                (workEventsType == v.tWebEventsType.button6))
                    if (item.InvokeMember == (Int16)v.tWebInvokeMember.autoSubmit) onay = true;

            if (workEventsType == v.tWebEventsType.tableField)
                if (eventsType_ == v.tWebEventsType.tableField) onay = true;

            if (onay)
            {
                // istek get ise
                // item.inject = none veya set veya alwaysset ise 
                if (workRequestType == v.tWebRequestType.get)
                    if (injectType_ == v.tWebInjectType.none) onay = false;
                //if (injectType_ == v.tWebInjectType.none ||
                //    injectType_ == v.tWebInjectType.Set ||
                //    injectType_ == v.tWebInjectType.AlwaysSet) onay = false;

                // istek post
                // item.inject = none veya get ise 
                if (workRequestType == v.tWebRequestType.post)
                    if (injectType_ == v.tWebInjectType.none) onay = false;
                //if (injectType_ == v.tWebInjectType.none ||
                //    injectType_ == v.tWebInjectType.Get) onay = false;
            }

            if (eventsType_ == v.tWebEventsType.displayNone) onay = true;
            
            // false olabilir
            if (item.IsActive == false) onay = false;

            return onay;
        }
        private bool valueChecked(webNodeValue wnv)
        {
            bool onay = true;

            if (wnv.KrtOperandType == null) return onay;
            if (wnv.KrtOperandType == "") return onay;

            if (wnv.CheckNodeId > 0)
                 onay = t.myOperandControl(wnv.CheckReadValue , wnv.CheckOperandValue, wnv.KrtOperandType); // başka bir nodenin valuesi kontrol ediliyor
            else onay = t.myOperandControl(wnv.writeValue, wnv.CheckOperandValue, wnv.KrtOperandType);     // databaseden gelen ve set edilmeye çalışılan value kontrol ediliyor 

            return onay;
        }
        #endregion Scraping functions

        #region subFunctions
        private bool IsDriverAlive(IWebDriver wb)
        {
            try
            {
                var _ = wb.WindowHandles;
                return true;
            }
            catch { return false; }
        }

        private async Task<bool> myPageViewClickAsync(IWebDriver wb, List<MsWebPage> msWebPage)
        {
            bool driverAlive = IsDriverAlive(wb);

            if (driverAlive == false)
            {
                
                v.webDriver_ = null;
                preparingWebMain();
                wb = v.webDriver_;
            }

            bool onay = false;
            if (msWebPage[0].Id > 0) 
            {

                msPagesService.getPageUrls(f, msWebPage);

                if (f.loginPageUrl == f.talepEdilenUrl && f.loginPageRun) return true;

                if (f.aktifUrl != f.talepEdilenUrl)
                {
                    if (t.IsNotNull(f.talepOncesiUrl))
                        onay = await loadPageUrl(wb, f.talepOncesiUrl);
                    else
                    {
                        if (f.loginPageUrl != f.talepEdilenUrl && f.loginPageRun )
                            onay = await loadPageUrl(wb, f.talepEdilenUrl);

                        if (f.loginPageUrl == f.talepEdilenUrl && f.loginPageRun == false)
                        {
                            onay = await loadPageUrl(wb, f.talepEdilenUrl);
                            f.loginPageRun = onay;
                        }
                    }
                }
                else
                {
                    onay = await loadPageUrl(wb, f.talepEdilenUrl);
                }
            }
            return onay;
        }
        private async Task<bool> loadPageUrl(IWebDriver wb, string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                /// mevcutta yeni alt page açılmış ise
                /// ve tekrar yeni sayfa okunacaksa tekra main page dön ve orada yeni url yi aç

                if (f.loginPageUrl == url)
                {
                    if (f.loginPageRun) return true;
                }

                try
                {
                    //wb.Url = url;
                    wb.Navigate().GoToUrl(url);

                    f.aktifUrl = wb.Url;

                    openPageControlAsync(wb);
                    return true;

                }
                catch (Exception)
                {
                    bool onay = await subPageOpenControl(wb, url); 
                    if (onay) return true;

                    /// Kullanıcı manuel webBrowser kapatmış olabilir
                    /// kapatmışsa terar oluşturmasın
                    /// 
                    f.loginPageRun = false;
                    v.webDriver_ = null;
                    wb = null;
                    t.FlyoutMessage(this, "Uyarı", "Mevcut açık olan Google Chrome ulaşamıyorum, kapanmış olabilir. Yeniden giriş yapmak istiyorsanız tekrar giriş yapmayı deneyin.");
                    return false;

                    /*
                    preparingWebMain();
                    wb = v.webDriver_;
                    try
                    {
                        wb.Navigate().GoToUrl(url);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error Navigate ( " + url + " )" + v.ENTER2 + ex.Message);
                        //throw;
                    }
                    */
                    //throw;
                }
            }
            else
            {
                MessageBox.Show("DİKKAT : Lütfen  - page url - yi girin...");
                return false;
            }
        }
        private async Task<bool> subPageOpenControl(IWebDriver wb, string url)
        {
            /// sonradan açılan ikinci sayfa kapatılmış sa
            try
            {
                if (t.IsNotNull(f.seleniumMainPage) &&
                    t.IsNotNull(f.seleniumNewSubPage) &&
                    t.IsNotNull(f.seleniumActivePage))
                {
                    if (f.seleniumNewSubPage == f.seleniumActivePage)
                    {
                        wb.SwitchTo().Window(f.seleniumMainPage);

                        f.seleniumActivePage = f.seleniumMainPage;

                        if (t.IsNotNull(url))
                            wb.Navigate().GoToUrl(url);

                        f.aktifUrl = wb.Url;

                        openPageControlAsync(wb);
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                //throw;
            }
            return false;
        }
        private async Task openPageControlAsync(IWebDriver wb)
        {
            if (t.IsNotNull(f.talepEdilenUrl) == false)
                return;

            bool onay = false;

            if (f.talepEdilenUrl != f.aktifUrl)
            {
                if (f.aktifUrl.IndexOf("oturumsonu") > -1)
                {
                    return;
                }

                /// aktif url Login page değil ise nedir ?
                /// 
                /// talep öncesi başka bir url çağrısı var ise
                ///
                if (t.IsNotNull(f.talepOncesiUrl))
                {
                    if (f.aktifUrl == "https://mebbis.meb.gov.tr/default.aspx?NoSession?sc") return;
                    if ((f.talepOncesiUrl != f.aktifUrl) &&
                        (f.talepEdilenUrl != f.aktifUrl) &&
                        (f.aktifUrl == f.loginPageUrl)) 
                        return;


                        /// aktif url ne talep edilen ne de talep öncesi url değil ise 
                        /// talepOncesiUrl yi çağır
                        ///
                        if ((f.talepOncesiUrl != f.aktifUrl) &&
                        (f.talepEdilenUrl != f.aktifUrl))
                        loadPageUrl(wb, f.talepOncesiUrl);
                    //await runLoginPage(wb);// v.webDriver_);
                    

                    /// talep öncesi geldiyse sıra esas talep edilen url ye sıra geldi
                    ///
                    if (f.talepOncesiUrl == f.aktifUrl)
                    {
                        loadPageUrl(wb, f.talepEdilenUrl);
                            
                        // nodenin içindeki itemValue ve itemText listesi (combo içerikleri)
                        if (t.IsNotNull(f.aktifPageCode))
                            aktifPageNodeItemsList_ = msPagesService.readNodeItems(f.aktifPageCode);
                    }
                }
                else
                {
                    /// talepOncesiUrl yok ise şimdi talepEdilenUrl yi çağıralım 
                    ///
                    loadPageUrl(wb, f.talepEdilenUrl);
                }
            }

            if (f.talepEdilenUrl == f.aktifUrl)
            {
                if (f.loadWorking == false)
                {
                    t.WaitFormOpen(this, "Sayfa yükleniyor ...");
                    await startNodes(this.msWebNodes_, this.workPageNodes_, v.tWebRequestType.post, v.tWebEventsType.load);
                    f.loadWorking = true;
                    v.IsWaitOpen = false;
                    t.WaitFormClose();
                }
            }
        }
        private async Task runLoginPage(IWebDriver wb)
        {
            /// burası kullanıcı 
            if (f.errorPageUrl == f.aktifUrl)
            {
                loadPageUrl(wb, f.loginPageUrl);
            }
            else if (f.aktifUrl.IndexOf("oturumsonu") > -1)
            {
                return;
            }
            else
            {
                t.AlertMessage("Bağlantı kopukluğu :", "Bağlıntınız kopmuş durumda yeniden giriş işlemi yapılacaktır ...");
                /// Login page bilgilerine focus ol
                if (t.IsNotNull(ds_MsWebPages))
                {
                    dN_MsWebPages.Position = 0;
                    Application.DoEvents();
                    ((DevExpress.XtraGrid.GridControl)view_MsWebPages).MainView.Focus();
                }
            }
        }
        private async Task preparingViewControls(bool value)
        {
            if (t.IsNotNull(f.tableIPCodesInLoad) == false) return;

            /// tableIPCodesInLoad üzerindeki tableIPCode viewleri tespit et ve Enabled durumunu değiştir
            /// 
            string tableIPCode = "";
            string list = f.tableIPCodesInLoad;
            while (list.IndexOf("||") > -1)
            {
                tableIPCode = t.Get_And_Clear(ref list, "||");
                if (t.IsNotNull(tableIPCode))
                    t.ViewControl_Enabled(this, tableIPCode, value);
            }
        }

        private void dN_AnalysisNodes_PositionChanged(object sender, EventArgs e)
        {
            if (f.btn_AnalysisNodeView != null)
            {
                if (((DevExpress.XtraEditors.CheckButton)f.btn_AnalysisNodeView).Checked)
                {
                    analysisNodeHtmlView();
                }
            }
        }
        private void analysisNodeHtmlView()
        {
            if (ds_AnalysisNodes != null)
            {
                if (dN_AnalysisNodes.Position > -1)
                {
                    string html = ds_AnalysisNodes.Tables[0].Rows[dN_AnalysisNodes.Position]["outerHtml"].ToString();
                    this.webTest.DocumentText = html;
                }
            }
        }

        #endregion


        private void convertHtmlToPDFSpire()
        {
            //Specify the input URL and output PDF file path
            string inputUrl = @"https://www.e-iceblue.com/Tutorials/Spire.PDF/Spire.PDF-Program-Guide/C-/VB.NET-Convert-Image-to-PDF.html";
            string outputFile = @"HtmlToPDF.pdf";
            
            //Specify the path to the Chrome plugin
            string chromeLocation = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
            //Create an instance of the ChromeHtmlConverter class
            ChromeHtmlConverter converter = new ChromeHtmlConverter(chromeLocation);
            // Create an instance of the ConvertOptions class
            ConvertOptions options = new ConvertOptions();
            //Set conversion timeout
            options.Timeout = 10 * 3000;
            //Set paper size and page margins of the converted PDF
            options.PageSettings = new PageSettings()
            {
                PaperWidth = 8.27,
                PaperHeight = 11.69,
                MarginTop = 0,
                MarginLeft = 0,
                MarginRight = 0,
                MarginBottom = 0
            };
            //Convert the URL to PDF
            converter.ConvertToPdf(inputUrl, outputFile, options);

        }

    }

}
