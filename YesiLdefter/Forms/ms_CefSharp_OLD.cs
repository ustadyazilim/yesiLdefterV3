using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using CefSharp;
using CefSharp.WinForms;
using CefSharp.SchemeHandler;
using Tkn_Events;
using Tkn_ToolBox;
using YesiLdefter.Selenium;
using Tkn_Variable;
using YesiLdefter.CEFSharp;
using DevExpress.XtraEditors;
using YesiLdefter.Entities;

namespace YesiLdefter
{
    public partial class ms_CefSharp : Form
    {
        #region Tanımlar
        tToolBox t = new tToolBox();

        CefSharp.WinForms.ChromiumWebBrowser cefBrowser_ = null;
        CefSharp.WinForms.Host.ChromiumHostControl cefHostControl_ = null;

        MsWebPagesService msPagesService = new MsWebPagesService();

        /// Scraping
        DataSet ds_MsWebPages = null;
        DataNavigator dN_MsWebPages = null;
        DataSet ds_MsWebNodes = null;
        DataNavigator dN_MsWebNodes = null;
        // sadece login için kullanılan dataset
        DataSet ds_LoginPageNodes = null;
        // web sayfalarının adını gösteren viewControl
        Control view_MsWebPages = null;

        /*
        // User butonları
        Control btn_PageView = null;
        //Control btn_PageViewAnalysis = null;
        Control btn_AlwaysSet = null;
        Control btn_FullGet1 = null;  // birinci get butonu
        Control btn_FullGet2 = null;  // ikinci  get butonu
        Control btn_FullPost1 = null; // birinci post butonu
        Control btn_FullPost2 = null; // ikinci  post butonu
        Control btn_FullSave = null;  // save    post butonu
        Control btn_AutoSubmit = null;// auto    kaydet butonu 
        */
        string TableIPCode = string.Empty;

        List<MsWebPage> msWebPage_ = null;
        List<MsWebPage> msWebPages_ = null;

        List<MsWebNode> msWebNodes_ = null;
        List<MsWebNode> msWebLoginNodes_ = null;

        List<MsWebScrapingDbFields> msWebScrapingDbFields_ = null;
        List<webNodeItemsList> aktifPageNodeItemsList_ = null;

        webWorkPageNodes workPageNodes_ = new webWorkPageNodes();
        webForm f = new webForm();


        #endregion Tanımlar

        public ms_CefSharp()
        {
            InitializeComponent();

            tEventsForm evf = new tEventsForm();

            this.Load += new System.EventHandler(evf.myForm_Load);
            this.Shown += new System.EventHandler(evf.myForm_Shown);
            this.Shown += new System.EventHandler(this.ms_CefSharp_Shown);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ms_CefSharp_Closing);
            
            this.KeyPreview = true;
        }
        
        #region Form preparing
        private void ms_CefSharp_Shown(object sender, EventArgs e)
        {
            preparingCefBrowserView();

            MsWebNodesButtonsPreparing();
            MsWebPagesButtonsPreparing();
            //preparingWebPagesViewControl();
            msPagesService.preparingMsWebLoginPage(f, ds_LoginPageNodes, this.msWebLoginNodes_);

            if (this.msWebPages_ == null) return;

            // scraping ilişkisi olan TableIPCode ve ilgili fieldler
            // 
            this.msWebScrapingDbFields_ = msPagesService.readScrapingTablesAndFields(this.msWebPages_);

            /// DataSet ve DataNavigatorleri işaretle
            /// 
            //msPagesService.preparingDataSets(this, dataNavigator_PositionChanged);
            
        }

        private void ms_CefSharp_Closing(object sender, FormClosingEventArgs e)
        {
            if (cefBrowser_ != null)
            {
                cefBrowser_.Dispose();
                cefBrowser_ = null;
                // Use CEFHelper.Shutdown() instead of direct Cef.Shutdown() to ensure thread safety
                // Only shutdown if this is the last form using CefSharp
                // Cef.Shutdown() should be called from main form closing, not individual forms
            }
        }

        private void preparingCefBrowserView()
        {
            Control cntrl = t.Find_Control(this, "CefSharpBrowser");
            
            if (cntrl != null)
            {
                //CefSharp.WinForms.ChromiumWebBrowser cefBrowser_2 = null;

                if (v.cefBrowserLoading == false)
                {
                    if (cefBrowser_ == null)
                    {
                        cefBrowser_ = CEFHelper.CreateBrowser;
                        cefBrowser_.Dock = DockStyle.Fill;
                        
                    }
                    //v.cefBrowser_.LoadUrl("https://google.com/");
                    cefBrowser_.LoadUrl("https://ustadyazilim.com/");
                    //v.cefBrowser_.LoadUrl("https://google.com/");
                    //v.cefBrowserLoading = true;
                }
                cntrl.Controls.Add(cefBrowser_);
                cefBrowser_.AddressChanged += OnBrowserAddressChanged;
                cefBrowser_.LoadingStateChanged += OnLoadingStateChanged;


            }
        }

        private void preparingWebMain()
        {
            if (cefBrowser_ == null)
            {
                cefBrowser_ = CEFHelper.CreateBrowser;
            }
        }

        private void MsWebNodesButtonsPreparing()
        { 
            /// Selenium ve CEFSharp için henüz gerek olmadı, Microsoft.WebBrowser de kulanıldı
        }

        #region MsWebPagesButtonsPreparing                
        private void MsWebPagesButtonsPreparing()
        {
            /// MsWebPages tablosu
            /// 

            TableIPCode = t.Find_TableIPCode(this, "MsWebPages");

            if (t.IsNotNull(TableIPCode) == false) return;

            //Control cntrl = null;
            string[] controls = new string[] { };

            t.Find_DataSet(this, ref ds_MsWebPages, ref dN_MsWebPages, TableIPCode);

            if (t.IsNotNull(ds_MsWebPages))
            {
                f.Clear();
                f.tForm = this;
                f.browserType = v.tBrowserType.CefSharp;

                dN_MsWebPages.PositionChanged += new System.EventHandler(dNScrapingPages_PositionChanged);
                msPagesService.preparingMsWebPages(ds_MsWebPages, ref msWebPages_);
                msPagesService.preparingMsWebNodesFields(this, 
                  ref this.msWebPage_,
                  ref this.msWebNodes_,
                  ref this.workPageNodes_,
                  this.msWebScrapingDbFields_,
                  ds_MsWebPages,
                  ds_MsWebNodes,
                  dN_MsWebPages);

                msPagesService.preparingMsWebPagesButtons(this, f, TableIPCode);
                preparingMsWebPagesButtons_();
            }
        }
        private void preparingMsWebPagesButtons_()
        {
            /*
            if (f.btn_PageView != null)
                ((DevExpress.XtraEditors.SimpleButton)f.btn_PageView).Click += new System.EventHandler(myPageViewClick);
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
            */
        }
        #endregion MsWebPagesButtonsPreparing

        private void preparingWebPagesViewControl()
        {
            TableIPCode = t.Find_TableIPCode(this, "MsWebPages");
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
        public async void myTileView_ItemClick(object sender, DevExpress.XtraGrid.Views.Tile.TileViewItemClickEventArgs e)
        {
            if (t.IsNotNull(ds_MsWebPages) == false) return;
            if (dN_MsWebPages.Position == -1) return;

            if (msPagesService.LoginOnayi(ds_MsWebPages, dN_MsWebPages, f))
                await cefSharpLoginPageViev();
        }


        private void dNScrapingPages_PositionChanged(object sender, EventArgs e)
        {
            
            //**** preparingMsWebNodesFields();
            //**** preparingAktifPageLoad();

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
                    (f.tableIPCodesInLoad.IndexOf(f.tableIPCodeIsSave) <= 0) && // save olan Dataset ise atla
                    (v.con_Listele_TableIPCode != tableIPCode_)
                    )
                {
                    t.WaitFormOpen(this, "Web sayfası değiştiriliyor ...");
                    f.loadWorking = false;
                    f.pageRefreshWorking = true;
//                    await startNodes(this.msWebNodes_, this.workPageNodes_, v.tWebRequestType.post, v.tWebEventsType.load);
                    f.loadWorking = true;
                    v.IsWaitOpen = false;
                    t.WaitFormClose();
                }
            }
        }
                
        #endregion Form preparing
        /// 
        /// Userın kullanığı butonlar
        /// 
        #region user buttons
        private async Task cefSharpLoginPageViev()
        {
            /// Kullanıcının mebbisCode ve şifresini yeniden oku
            /// değiştirmiş olabilir
            /// 
            msPagesService.getMebbisCode();
            //MessageBox.Show("Mebbis : " + v.tUser.MebbisCode + " : " + v.tUser.MebbisPass);

            f.Clear();
            f.tForm = this;
            f.browserType = v.tBrowserType.CefSharp;

            //if (v.webDriver_ == null)
            //    preparingWebMain();

            await myPageViewClickAsync(cefBrowser_ , this.msWebPage_);
        }

        #endregion user buttons

        #region Scraping functions               
        private async Task startNodes(List<MsWebNode> msWebNodes, webWorkPageNodes workPageNodes, v.tWebRequestType workRequestType, v.tWebEventsType workEventsType)
        {

        }

        #endregion Scraping functions               

        #region subFunctions
        private async Task myPageViewClickAsync(ChromiumWebBrowser wb, List<MsWebPage> msWebPage)
        {
            if (msWebPage[0].Id > 0)
            {
                bool onay = false;

                msPagesService.getPageUrls(f, msWebPage);

                if (f.aktifUrl != f.talepEdilenUrl)
                {
                    if (t.IsNotNull(f.talepOncesiUrl))
                        onay = await loadPageUrl(wb, f.talepOncesiUrl);
                    else
                        onay = await loadPageUrl(wb, f.talepEdilenUrl);
                }
                else
                {
                    onay = await loadPageUrl(wb, f.talepEdilenUrl);
                }
            }
        }
        private async Task<bool> loadPageUrl(ChromiumWebBrowser wb, string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    wb.LoadUrl(url);
                    f.aktifUrl = url;// bu hatalı 
                    v.cefBrowserLoading = true;
                }
                catch (Exception)
                {
                    /// Kullanıcı manuel webBrowser kapatmış olabilir
                    /// 
                    cefBrowser_ = null;
                    wb = null;
                    preparingWebMain();
                    wb = cefBrowser_;
                    try
                    {
                        wb.LoadUrl(url);
                        f.aktifUrl = url;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error Navigate ( " + url + " )" + v.ENTER2 + ex.Message);
                        //throw;
                    }
                    //throw;
                }

                // NULL geliyor
                //f.aktifUrl = wb.Address;

                //openPageControlAsync(wb);

                /*
                string script = "document.title;";
                var task = wb.EvaluateScriptAsync(script);
                var response = await task;

                if (response.Success && response.Result != null)
                {
                    string title = response.Result.ToString();
                    MessageBox.Show($"Page title is: {title}");
                }
                */
                return true;
            }
            else
            {
                MessageBox.Show("DİKKAT : Lütfen  - page url - yi girin...");
                return false;
            }
        }
        private async Task openPageControlAsync(ChromiumWebBrowser wb)
        {
            if (t.IsNotNull(f.talepEdilenUrl) == false)
                return;

            //this.Text += '+';

            /*
            string script = "document.title;";
            var task = wb.EvaluateScriptAsync(script);
            var response = await task;

            if (response.Success && response.Result != null)
            {
                string title = response.Result.ToString();
                MessageBox.Show($"Page title is: {title}");
            }
            */

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
                    /// aktif url ne talep edilen ne de talep öncesi url değil ise 
                    /// talepOncesiUrl yi çağır
                    ///
                    if ((f.talepOncesiUrl != f.aktifUrl) &&
                        (f.talepEdilenUrl != f.aktifUrl))
                        await runLoginPage(cefBrowser_);


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
        private async Task runLoginPage(ChromiumWebBrowser wb)
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

        private void OnBrowserAddressChanged(object sender, AddressChangedEventArgs e)
        {
            f.aktifUrl = e.Address;
            //this.Invoke(new Action(() => f.aktifUrl = e.Address));
        }
        private void OnLoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (!e.IsLoading)
            {
                //this.Invoke(new Action(() => f.aktifUrl = v.cefBrowser_.Address));
                f.aktifUrl = cefBrowser_.Address;
                openPageControlAsync(cefBrowser_);
            }
        }

        #endregion subFunctions
        
    }
}
