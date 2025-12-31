using DevExpress.XtraBars.Docking;
using System.Data;
using System.Windows.Forms;
using Tkn_Events;
using Tkn_InputPanel;
using Tkn_Layout;
using Tkn_ToolBox;
using Tkn_Variable;

namespace YesiLdefter
{
    public class tMainForm : tBase
    {
        public void preparingMainForm(Form tForm)
        {
            System.Diagnostics.Debug.WriteLine($"[tMainForm.preparingMainForm] Starting. tForm.Name={tForm?.Name}, tForm.IsMdiContainer={tForm?.IsMdiContainer}");
            tEvents ev = new tEvents();
            tEventsMenu evm = new tEventsMenu();

            System.ComponentModel.Container components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources =
                new System.ComponentModel.ComponentResourceManager(typeof(main));

            DevExpress.Skins.SkinManager.EnableFormSkins();
            DevExpress.UserSkins.BonusSkins.Register();

            // -----
            DevExpress.XtraTabbedMdi.XtraTabbedMdiManager xtraTabbedMdiManager1 =
                new DevExpress.XtraTabbedMdi.XtraTabbedMdiManager(components);

            //System.Windows.Forms.Timer timer_Kullaniciya_Mesaj_Varmi = 
            //    new System.Windows.Forms.Timer(components);
            //System.Windows.Forms.Timer timer_Mesaj_Suresi_Bitti = 
            //    new System.Windows.Forms.Timer(components);

            // -----
            DevExpress.XtraBars.Ribbon.RibbonControl ribbonControl1 = new DevExpress.XtraBars.Ribbon.RibbonControl();
            /*
            DevExpress.XtraBars.Ribbon.RibbonPage ribbonPage1 = new DevExpress.XtraBars.Ribbon.RibbonPage();
            DevExpress.XtraBars.Ribbon.RibbonPage ribbonPage2 = new DevExpress.XtraBars.Ribbon.RibbonPage();
            DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroup1 = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            // skin grup
            DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroupStyles = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            DevExpress.XtraBars.SkinDropDownButtonItem skinDropDownButtonItem1 = new DevExpress.XtraBars.SkinDropDownButtonItem();
            DevExpress.XtraBars.SkinRibbonGalleryBarItem skinRibbonGalleryBarItem1 = new DevExpress.XtraBars.SkinRibbonGalleryBarItem();
            DevExpress.XtraBars.SkinPaletteRibbonGalleryBarItem skinPaletteRibbonGalleryBarItem1 = new DevExpress.XtraBars.SkinPaletteRibbonGalleryBarItem();
            */
            //-- statusBar
            DevExpress.XtraBars.Ribbon.RibbonStatusBar ribbonStatusBar1 = new DevExpress.XtraBars.Ribbon.RibbonStatusBar();
            DevExpress.XtraBars.BarMdiChildrenListItem barMdiChildrenListItem1 = new DevExpress.XtraBars.BarMdiChildrenListItem();
            DevExpress.XtraBars.BarStaticItem barMesajlar = new DevExpress.XtraBars.BarStaticItem();
            DevExpress.XtraBars.BarButtonItem barButtonGuncelleme = new DevExpress.XtraBars.BarButtonItem();
            DevExpress.XtraBars.BarEditItem mainProgressBar = new DevExpress.XtraBars.BarEditItem();
            DevExpress.XtraEditors.Repository.RepositoryItemProgressBar _mainProgressBar = new DevExpress.XtraEditors.Repository.RepositoryItemProgressBar();
            DevExpress.XtraBars.BarButtonItem barButtonServiceTool = new DevExpress.XtraBars.BarButtonItem();
            DevExpress.XtraBars.BarEditItem barEditItemCari = new DevExpress.XtraBars.BarEditItem();
            DevExpress.XtraEditors.Repository.RepositoryItemSearchControl _mainCariSearch = new DevExpress.XtraEditors.Repository.RepositoryItemSearchControl();
            DevExpress.XtraBars.BarEditItem barPrjConn = new DevExpress.XtraBars.BarEditItem();
            DevExpress.XtraEditors.Repository.RepositoryItemToggleSwitch _repositoryItemToggleSwitch1 = new DevExpress.XtraEditors.Repository.RepositoryItemToggleSwitch();
            DevExpress.XtraBars.BarEditItem barMSConn = new DevExpress.XtraBars.BarEditItem();
            DevExpress.XtraEditors.Repository.RepositoryItemToggleSwitch _repositoryItemToggleSwitch2 = new DevExpress.XtraEditors.Repository.RepositoryItemToggleSwitch();

            
            /*
            //-- backstageView
            DevExpress.XtraBars.Ribbon.BackstageViewControl backstageViewControl1 = new DevExpress.XtraBars.Ribbon.BackstageViewControl();
            DevExpress.XtraBars.Ribbon.BackstageViewClientControl backstageViewClientControl1 = new DevExpress.XtraBars.Ribbon.BackstageViewClientControl();
            DevExpress.XtraBars.Ribbon.BackstageViewTabItem backstageViewTabItem1 = new DevExpress.XtraBars.Ribbon.BackstageViewTabItem();
            */
            //-- toolBox
            DevExpress.XtraToolbox.ToolboxControl toolboxControl1 = new DevExpress.XtraToolbox.ToolboxControl();
            DevExpress.XtraToolbox.ToolboxGroup toolboxGroup0 = new DevExpress.XtraToolbox.ToolboxGroup();
            DevExpress.XtraToolbox.ToolboxGroup toolboxGroup1 = new DevExpress.XtraToolbox.ToolboxGroup();
            DevExpress.XtraToolbox.ToolboxGroup toolboxGroup2 = new DevExpress.XtraToolbox.ToolboxGroup();
            DevExpress.XtraToolbox.ToolboxGroup toolboxGroup3 = new DevExpress.XtraToolbox.ToolboxGroup();
            DevExpress.XtraToolbox.ToolboxGroup toolboxGroup4 = new DevExpress.XtraToolbox.ToolboxGroup();
            DevExpress.XtraToolbox.ToolboxGroup toolboxGroup5 = new DevExpress.XtraToolbox.ToolboxGroup();
            DevExpress.XtraToolbox.ToolboxGroup toolboxGroup6 = new DevExpress.XtraToolbox.ToolboxGroup();
            DevExpress.XtraToolbox.ToolboxGroup toolboxGroup7 = new DevExpress.XtraToolbox.ToolboxGroup();
            DevExpress.XtraToolbox.ToolboxGroup toolboxGroup8 = new DevExpress.XtraToolbox.ToolboxGroup();

            ///------
            ((System.ComponentModel.ISupportInitialize)(xtraTabbedMdiManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(ribbonControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(_mainProgressBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(_mainCariSearch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(_repositoryItemToggleSwitch1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(_repositoryItemToggleSwitch2)).BeginInit();

            //((System.ComponentModel.ISupportInitialize)(backstageViewControl1)).BeginInit();
            //backstageViewControl1.SuspendLayout();
            tForm.SuspendLayout();
            ///--------
            // 
            // xtraTabbedMdiManager1
            // 
            // CRITICAL: Main form MUST be an MDI container for child forms to work properly
            System.Diagnostics.Debug.WriteLine($"[tMainForm.preparingMainForm] Before setting IsMdiContainer: tForm.IsMdiContainer={tForm.IsMdiContainer}");
            tForm.IsMdiContainer = true;
            System.Diagnostics.Debug.WriteLine($"[tMainForm.preparingMainForm] After setting IsMdiContainer: tForm.IsMdiContainer={tForm.IsMdiContainer}");
            xtraTabbedMdiManager1.MdiParent = tForm;
            //xtraTabbedMdiManager1.HeaderLocation = DevExpress.XtraTab.TabHeaderLocation.Right;
            //.RightToLeftLayout = DevExpress.Utils.DefaultBoolean.True;
            //.SetNextMdiChildMode = DevExpress.XtraTabbedMdi.SetNextMdiChildMode.TabControl;
            //  
            // ribbonControl1
            // 
            //-ribbonControl1.ApplicationButtonDropDownControl = backstageViewControl1;
            ribbonControl1.ExpandCollapseItem.Id = 0;
            ribbonControl1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
                   ribbonControl1.ExpandCollapseItem,
                   barMdiChildrenListItem1,
                   barEditItemCari,
                   barPrjConn,
                   barMSConn,
                   barMesajlar,
                   barButtonGuncelleme,
                   mainProgressBar,
                   barButtonServiceTool,
                   //skinDropDownButtonItem1,
                   //skinRibbonGalleryBarItem1,
                   //skinPaletteRibbonGalleryBarItem1
                   });

            ribbonControl1.Location = new System.Drawing.Point(0, 0);
            ribbonControl1.MaxItemId = 10;
            ribbonControl1.Name = "ribbonControl1";
            //ribbonControl1.Pages.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPage[] {
            //ribbonPage1,
            //ribbonPage2
            //});
            ribbonControl1.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
                      _mainProgressBar,
                      _mainCariSearch,
                      _repositoryItemToggleSwitch1,
                      _repositoryItemToggleSwitch2
                  });
            ribbonControl1.Size = new System.Drawing.Size(927, 116);
            ribbonControl1.StatusBar = ribbonStatusBar1;

            ribbonControl1.ToolbarLocation = DevExpress.XtraBars.Ribbon.RibbonQuickAccessToolbarLocation.Hidden;
            ribbonControl1.Minimized = true; // default atadım
            ribbonControl1.Manager.UseF10KeyForMenu = false;
            //ribbonControl1.Manager.UseAltKeyForMenu = false;
            //ribbonControl1.Manager.ShowCloseButton = true;
            ribbonControl1.KeyDown += evm.tRibbonControl_KeyDown;
            /*
            // 
            // ribbonPage1
            // 
            ribbonPage1.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
            ribbonPageGroup1
            });
            ribbonPage1.Name = "ribbonPage1";
            ribbonPage1.Text = "YeşiLdefter";
            // 
            // ribbonPageGroup1
            // 
            ribbonPageGroup1.Name = "ribbonPageGroupGenel";
            ribbonPageGroup1.Text = "Genel İşlemler";
            // 
            // ribbonPage2
            // 
            ribbonPage2.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
            ribbonPageGroupStyles
            });
            ribbonPage2.Name = "ribbonPage2";
            ribbonPage2.Text = "Ayarlar";
            // 
            // skinDropDownButtonItem1
            // 
            skinDropDownButtonItem1.Id = 7;
            skinDropDownButtonItem1.Name = "skinDropDownButtonItem1";
            skinDropDownButtonItem1.DownChanged += new DevExpress.XtraBars.ItemClickEventHandler(ev.skinDropDownButtonItem_DownChanged);
            skinDropDownButtonItem1.HyperlinkClick += new DevExpress.Utils.HyperlinkClickEventHandler(ev.skinDropDownButtonItem_HyperlinkClick);
            skinDropDownButtonItem1.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(ev.skinDropDownButtonItem_ItemClick);
            // 
            // skinRibbonGalleryBarItem1
            // 
            skinRibbonGalleryBarItem1.Caption = "skinRibbonGalleryBarItem1";
            skinRibbonGalleryBarItem1.Id = 8;
            skinRibbonGalleryBarItem1.Name = "skinRibbonGalleryBarItem1";
            skinRibbonGalleryBarItem1.GalleryItemClick += new DevExpress.XtraBars.Ribbon.GalleryItemClickEventHandler(ev.skinRibbonGalleryBarItem_GalleryItemClick);
            skinRibbonGalleryBarItem1.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(ev.skinRibbonGalleryBarItem_ItemClick);
            // 
            // skinPaletteRibbonGalleryBarItem1
            // 
            skinPaletteRibbonGalleryBarItem1.Caption = "skinPaletteRibbonGalleryBarItem1";
            skinPaletteRibbonGalleryBarItem1.Id = 9;
            skinPaletteRibbonGalleryBarItem1.Name = "skinPaletteRibbonGalleryBarItem1";
            // 
            // ribbonPageGroupStyles
            // 
            ribbonPageGroupStyles.ItemLinks.Add(skinDropDownButtonItem1);
            ribbonPageGroupStyles.ItemLinks.Add(skinRibbonGalleryBarItem1);
            ribbonPageGroupStyles.ItemLinks.Add(skinPaletteRibbonGalleryBarItem1);
            ribbonPageGroupStyles.Name = "ribbonPageGroupStyles";
            ribbonPageGroupStyles.Text = "Temalar";
            */
            // 
            // ribbonStatusBar1
            // 
            ribbonStatusBar1.ItemLinks.Add(barMdiChildrenListItem1);
            ribbonStatusBar1.ItemLinks.Add(barMesajlar);
            ribbonStatusBar1.ItemLinks.Add(barButtonGuncelleme);
            ribbonStatusBar1.ItemLinks.Add(mainProgressBar);
            ribbonStatusBar1.ItemLinks.Add(barEditItemCari);
            ribbonStatusBar1.ItemLinks.Add(barButtonServiceTool);
            ribbonStatusBar1.ItemLinks.Add(barPrjConn);
            ribbonStatusBar1.ItemLinks.Add(barMSConn);
            ribbonStatusBar1.Location = new System.Drawing.Point(0, 391);
            ribbonStatusBar1.Name = "ribbonStatusBar1";
            ribbonStatusBar1.Ribbon = ribbonControl1;
            ribbonStatusBar1.Size = new System.Drawing.Size(927, 27);
            //--------

            // 
            // barMdiChildrenListItem1
            // 
            barMdiChildrenListItem1.Caption = "Açık Formlar";
            barMdiChildrenListItem1.Id = 1;
            barMdiChildrenListItem1.Name = "barMdiChildrenListItem1";
            // 
            // barMesajlar
            // 
            barMesajlar.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Left;
            barMesajlar.ItemAppearance.Hovered.BackColor = v.AppearanceFocusedColor;
            barMesajlar.ItemAppearance.Hovered.Options.UseBackColor = true;

            barMesajlar.ItemAppearance.Normal.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(100)));
            barMesajlar.ItemAppearance.Normal.ForeColor = System.Drawing.Color.Red;
            barMesajlar.ItemAppearance.Normal.Options.UseForeColor = true;
            barMesajlar.ItemAppearance.Normal.BackColor = System.Drawing.Color.White;
            barMesajlar.ItemAppearance.Normal.Options.UseBackColor = true;

            barMesajlar.Caption = "Mesajlar";
            barMesajlar.Id = 2;
            barMesajlar.Name = "barMesajlar";
            barMesajlar.Width = 300;
            // 
            // barButtonGuncelleme
            // 
            barButtonGuncelleme.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Left;
            barButtonGuncelleme.Caption = "   Yeni Güncelleme Mevcut   ";
            barButtonGuncelleme.Id = 3;
            barButtonGuncelleme.ItemAppearance.Normal.BackColor = System.Drawing.Color.Red;
            barButtonGuncelleme.ItemAppearance.Normal.ForeColor = System.Drawing.Color.White;
            barButtonGuncelleme.ItemAppearance.Normal.Options.UseBackColor = true;
            barButtonGuncelleme.ItemAppearance.Normal.Options.UseForeColor = true;
            barButtonGuncelleme.Name = "barButtonGuncelleme";
            barButtonGuncelleme.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
            // 
            // mainProgressBar
            // 
            mainProgressBar.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Right;
            mainProgressBar.Edit = _mainProgressBar;
            mainProgressBar.EditWidth = 150;
            mainProgressBar.Id = 4;
            mainProgressBar.Name = "mainProgressBar";
            // 
            // _mainProgressBar
            // 
            _mainProgressBar.Name = "_mainProgressBar";
            // 
            // barButtonServiceTool
            // 
            barButtonServiceTool.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Right;
            barButtonServiceTool.Caption = " Service ";
            barButtonServiceTool.Id = 5;
            //barButtonServiceTool.ItemAppearance.Normal.BackColor = System.Drawing.Color.Red;
            barButtonServiceTool.ItemAppearance.Normal.ForeColor = System.Drawing.Color.DarkGray;
            //barButtonServiceTool.ItemAppearance.Normal.Options.UseBackColor = true;
            barButtonServiceTool.ItemAppearance.Normal.Options.UseForeColor = true;
            barButtonServiceTool.Name = "barButtonServiceTool";
            barButtonServiceTool.Visibility = DevExpress.XtraBars.BarItemVisibility.Always;
            // 
            // barEditItemCari
            // 
            barEditItemCari.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Right;
            barEditItemCari.Caption = "Cari ";
            barEditItemCari.Edit = _mainCariSearch;
            barEditItemCari.EditWidth = 150;
            barEditItemCari.Id = 5;
            barEditItemCari.Name = "barEditItemCari";
            // 
            // mainCariSearch
            // 
            _mainCariSearch.AutoHeight = false;
            _mainCariSearch.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Repository.ClearButton(),
            new DevExpress.XtraEditors.Repository.SearchButton()});
            _mainCariSearch.Name = "mainCariSearch";
            // 
            // barPrjConn
            // 
            barPrjConn.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Right;
            barPrjConn.Caption = "Proje";
            barPrjConn.Edit = _repositoryItemToggleSwitch1;
            barPrjConn.Id = 6;
            barPrjConn.Name = "barPrjConn";
            // 
            // repositoryItemToggleSwitch1
            // 
            _repositoryItemToggleSwitch1.AutoHeight = false;
            _repositoryItemToggleSwitch1.Name = "repositoryItemToggleSwitch1";
            _repositoryItemToggleSwitch1.OffText = "Off";
            _repositoryItemToggleSwitch1.OnText = "On";
            // 
            // barMSConn
            // 
            barMSConn.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Right;
            barMSConn.Caption = "MPServer";
            barMSConn.Edit = _repositoryItemToggleSwitch2;
            barMSConn.Id = 7;
            barMSConn.Name = "barMSConn";
            // 
            // repositoryItemToggleSwitch2
            // 
            _repositoryItemToggleSwitch2.AutoHeight = false;
            _repositoryItemToggleSwitch2.Name = "repositoryItemToggleSwitch2";
            _repositoryItemToggleSwitch2.OffText = "Off";
            _repositoryItemToggleSwitch2.OnText = "On";

            /* --------
            // 
            // backstageViewControl1
            //  
            backstageViewControl1.Controls.Add(backstageViewClientControl1);
            backstageViewControl1.Items.Add(backstageViewTabItem1);
            backstageViewControl1.Location = new System.Drawing.Point(12, 131);
            backstageViewControl1.Name = "backstageViewControl1";
            backstageViewControl1.OwnerControl = ribbonControl1;
            backstageViewControl1.Size = new System.Drawing.Size(480, 150);
            backstageViewControl1.TabIndex = 8;

            // 
            // backstageViewClientControl1
            // 
            backstageViewClientControl1.Location = new System.Drawing.Point(188, 63);
            backstageViewClientControl1.Name = "backstageViewClientControl1";
            backstageViewClientControl1.Size = new System.Drawing.Size(291, 86);
            backstageViewClientControl1.TabIndex = 1;
            // 
            // backstageViewTabItem1
            // 
            backstageViewTabItem1.Caption = "backstageViewTabItem1";
            backstageViewTabItem1.ContentControl = backstageViewClientControl1;
            backstageViewTabItem1.Name = "backstageViewTabItem1";
            */
            // 
            // xtraTabbedMdiManager1
            // 
            xtraTabbedMdiManager1.MdiParent = tForm;
            // 
            // toolboxControl1
            // 
            toolboxControl1.Caption = "Ana Menü";
            toolboxControl1.Dock = System.Windows.Forms.DockStyle.Right;
            toolboxControl1.Location = new System.Drawing.Point(752, 116);
            toolboxControl1.MaximumSize = new System.Drawing.Size(175, 0);
            toolboxControl1.Name = "toolboxControl1";
            toolboxControl1.OptionsBehavior.ItemSelectMode = DevExpress.XtraToolbox.ToolboxItemSelectMode.Single;
            toolboxControl1.OptionsView.ColumnCount = 1;
            toolboxControl1.OptionsView.MenuButtonCaption = "";
            toolboxControl1.OptionsView.ShowMenuButton = false;
            toolboxControl1.OptionsView.ShowSearchPanel = false;
            toolboxControl1.OptionsView.ShowToolboxCaption = true;
            toolboxControl1.Size = new System.Drawing.Size(175, 275);
            toolboxControl1.TabIndex = 0;
            //toolboxControl1.Text = "Üstad Yazılım";


            #region
            /*
            toolboxControl1.Groups.Add(toolboxGroup0);
            toolboxControl1.Groups.Add(toolboxGroup1);
            toolboxControl1.Groups.Add(toolboxGroup2);
            toolboxControl1.Groups.Add(toolboxGroup3);
            toolboxControl1.Groups.Add(toolboxGroup4);
            toolboxControl1.Groups.Add(toolboxGroup5);
            toolboxControl1.Groups.Add(toolboxGroup6);
            toolboxControl1.Groups.Add(toolboxGroup7);
            toolboxControl1.Groups.Add(toolboxGroup8);
            
            //toolboxControl1.ItemClick += new DevExpress.XtraToolbox.ToolboxItemClickEventHandler(toolboxControl1_ItemClick);
            // 
            // toolboxGroup0
            // 
            toolboxGroup0.BeginGroupCaption = "";
            toolboxGroup0.Caption = "Bana Ait / İşlerim";
            toolboxGroup0.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("toolboxGroup1.ImageOptions.Image")));
            toolboxGroup0.Name = "toolboxGroup0";
            // 
            // toolboxGroup1
            // 
            toolboxGroup1.BeginGroupCaption = "";
            toolboxGroup1.Caption = "Yönetim";
            toolboxGroup1.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("toolboxGroup1.ImageOptions.Image")));
            toolboxGroup1.Name = "toolboxGroup1";
            toolboxGroup1.Tag = 2100;
            // 
            // toolboxGroup2
            // 
            toolboxGroup2.BeginGroupCaption = "";
            toolboxGroup2.Caption = "Mali ve İdari İşler";
            toolboxGroup2.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("toolboxGroup2.ImageOptions.Image")));
            toolboxGroup2.Name = "toolboxGroup2";
            toolboxGroup2.Tag = 2150;
            // 
            // toolboxGroup3
            // 
            toolboxGroup3.BeginGroupCaption = "";
            toolboxGroup3.Caption = "Satışlar";
            toolboxGroup3.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("toolboxGroup3.ImageOptions.Image")));
            toolboxGroup3.Name = "toolboxGroup3";
            toolboxGroup3.Tag = 2200;
            // 
            // toolboxGroup4
            // 
            toolboxGroup4.BeginGroupCaption = "";
            toolboxGroup4.Caption = "Pazarlama";
            toolboxGroup4.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("toolboxGroup3.ImageOptions.Image")));
            toolboxGroup4.Name = "toolboxGroup4";
            toolboxGroup4.Tag = 2250;
            // 
            // toolboxGroup5
            // 
            toolboxGroup5.BeginGroupCaption = "";
            toolboxGroup5.Caption = "Satın Alma";
            toolboxGroup5.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("toolboxGroup3.ImageOptions.Image")));
            toolboxGroup5.Name = "toolboxGroup5";
            toolboxGroup5.Tag = 2300;
            // 
            // toolboxGroup6
            // 
            toolboxGroup6.BeginGroupCaption = "";
            toolboxGroup6.Caption = "Üretim";
            toolboxGroup6.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("toolboxGroup3.ImageOptions.Image")));
            toolboxGroup6.Name = "toolboxGroup6";
            toolboxGroup6.Tag = 2350;
            // 
            // toolboxGroup7
            // 
            toolboxGroup7.BeginGroupCaption = "";
            toolboxGroup7.Caption = "Lojistik";
            toolboxGroup7.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("toolboxGroup3.ImageOptions.Image")));
            toolboxGroup7.Name = "toolboxGroup7";
            toolboxGroup7.Tag = 2400;
            // 
            // toolboxGroup8
            // 
            toolboxGroup8.BeginGroupCaption = "";
            toolboxGroup8.Caption = "İç Hizmetler";
            toolboxGroup8.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("toolboxGroup3.ImageOptions.Image")));
            toolboxGroup8.Name = "toolboxGroup8";
            toolboxGroup8.Tag = 2450;
            */
            #endregion


            //--------
            tForm.Controls.Add(toolboxControl1);
            tForm.Controls.Add(ribbonControl1);
            tForm.Controls.Add(ribbonStatusBar1);
            //      tForm.Controls.Add(backstageViewControl1);

            ((System.ComponentModel.ISupportInitialize)(ribbonControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(_mainProgressBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(_mainCariSearch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(_repositoryItemToggleSwitch1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(_repositoryItemToggleSwitch2)).EndInit();
            //     ((System.ComponentModel.ISupportInitialize)(backstageViewControl1)).EndInit();

            ((System.ComponentModel.ISupportInitialize)(xtraTabbedMdiManager1)).EndInit();

            //backstageViewControl1.ResumeLayout(false);
            tForm.ResumeLayout(false);
            tForm.PerformLayout();
        }

        public void preparingDockPanel(Form tForm, string TableIPCode)
        {
            tToolBox t = new tToolBox();
            tLayout ly = new tLayout();

            DockManager manager = null;

            manager = ly.Find_DockManager(tForm);

            if (manager != null)
            {
                //((System.ComponentModel.ISupportInitialize)(manager)).BeginInit();
                manager.Form = tForm;
                //manager.BeginInit();
                manager.BeginUpdate();
                //tForm.SuspendLayout();

                string LayoutType = string.Empty;
                string caption = string.Empty;
                string tabbed = string.Empty; // "Split";  //"Tab";
                string Prop_View = string.Empty;
                //string TableIPCode = string.Empty;
                string layout_code = string.Empty;
                string fieldName = string.Empty;
                byte IPDataType = 1;

                int i = -1;
                //int RefId = 0;
                int width = 0;
                int height = 0;
                int top = 0;
                int left = 0;
                int DockType = 0;
                int FrontBack = 0;

                string ustItemType = string.Empty;
                string ustHesapRefId = string.Empty;
                string ustHesapItemName = string.Empty;
                string ustItemName = string.Empty;
                DataRow UstHesapRow = null;

                //string s1 = "=ROW_PROP_VIEWS:";
                //string s2 = (char)34 + "DOCKPANEL" + (char)34 + ": {";
                //"DOCKPANEL": {

                // DockManager e formun üzerinden find ile bulamıyorum ( şimdlik )
                // bu nedenle bir DockManager create etmişken tanımlanmış olan 
                // tüm DockPanel lerin hepsi bir defada üretilmek zorunda kalıyorum

                //foreach (DataRow row2 in ds_Layout.Tables[0].Rows)
                //{
                LayoutType = v.lyt_dockPanel;// row2["LAYOUT_TYPE"].ToString();

                i++;

                if (LayoutType == v.lyt_dockPanel)
                {
                    //RefId = t.myInt32(row2["REF_ID"].ToString());
                    caption = "";// row2["LAYOUT_CAPTION"].ToString();
                    //TableIPCode = "";// t.Set(row2["TABLEIPCODE"].ToString(), "", "");
                    fieldName = ""; //t.Set(row2["FIELD_NAME"].ToString(), "", "");
                    layout_code = "";// t.Set(row2["LAYOUT_CODE"].ToString(), "", "");

                    IPDataType = 1;
                    if (fieldName == "KRITER") IPDataType = 2;

                    width = 200;// t.myInt32(row2["CMP_WIDTH"].ToString());
                    height = 200;// t.myInt32(row2["CMP_HEIGHT"].ToString());
                    top = 0;// t.myInt32(row2["CMP_TOP"].ToString());
                    left = 0;// t.myInt32(row2["CMP_LEFT"].ToString());

                    if (width == 0) width = 200;
                    if (height == 0) height = 200;

                    FrontBack = 0;// t.myInt16(row2["CMP_FRONT_BACK"].ToString());
                    DockType = v.dock_Left;// t.myInt16(row2["CMP_DOCK"].ToString());
                    if (DockType == 0) DockType = v.dock_Top;

                    Prop_View = "";// t.Set(row2["PROP_VIEWS"].ToString(), "", "");

                    // json
                    //if (Prop_View.IndexOf(s2) > -1)
                    tabbed = t.Find_Properties_Value(Prop_View, "DP_TABBED");

                    UstHesapRow = null;// UstHesap_Get(ds_Layout, i);

                    DockPanel tDockPanel = null;

                    #region // Ust hesabı var ise
                    if (UstHesapRow != null)
                    {
                        ustItemType = UstHesapRow["LAYOUT_TYPE"].ToString();
                        //ustHesapRefId = UstHesapRow["REF_ID"].ToString();
                        ustHesapRefId = UstHesapRow["LAYOUT_CODE"].ToString();
                        t.Str_Replace(ref ustHesapRefId, ".", "_");
                        ustItemName = v.lyt_Name + ustHesapRefId;

                        if (t.IsNotNull(UstHesapRow["CMP_NAME"].ToString()))
                            ustItemName = UstHesapRow["CMP_NAME"].ToString();

                        DockPanel UstPanel = manager.Panels[ustItemName];

                        if (tabbed == "Split")
                        {
                            tDockPanel = UstPanel.AddPanel();
                            UstPanel.ParentPanel.Tabbed = false;
                            //tDockPanel.DockTo(UstPanel, i);
                        }
                        if (tabbed == "Tab")
                        {
                            // Not : İki panelen tab olamsı için Dock tiplerininde aynı olması gerekiyor
                            tDockPanel = UstPanel.AddPanel();
                            UstPanel.ParentPanel.Tabbed = true;
                            //tDockPanel.DockAsTab(UstPanel, i);
                        }

                        //if (tabbed == "Split")
                        //    tDockPanel.Tabbed = false;
                        //if (tabbed == "Tab")
                        //    tDockPanel.Tabbed = true;
                    }
                    #endregion

                    if (tDockPanel == null)
                        tDockPanel = manager.AddPanel(DockingStyle.Float);

                    if (FrontBack == 2) tDockPanel.SendToBack();
                    else tDockPanel.BringToFront();

                    tDockPanel.Name = v.lyt_Name + t.Str_Replace(ref layout_code, ".", "_");  //RefId.ToString();
                    tDockPanel.Text = caption;
                    tDockPanel.Visibility = DockVisibility.Visible;
                    tDockPanel.TabIndex = i;

                    if (DockType == v.dock_Bottom) tDockPanel.Dock = DockingStyle.Bottom;
                    if (DockType == v.dock_Fill) tDockPanel.Dock = DockingStyle.Fill;
                    if (DockType == v.dock_Left) tDockPanel.Dock = DockingStyle.Left;
                    if (DockType == v.dock_Right) tDockPanel.Dock = DockingStyle.Right;
                    if (DockType == v.dock_Top) tDockPanel.Dock = DockingStyle.Top;

                    t.myControl_Size_And_Location(tDockPanel, width, height, left, top);

                    //
                    // InputPanel Create
                    //
                    if (TableIPCode != string.Empty)
                    {
                        tInputPanel ip = new tInputPanel();
                        ip.Create_InputPanel(tForm, tDockPanel, TableIPCode, IPDataType, true);
                    }
                }

            }


            manager.EndUpdate();
            //manager.EndInit();
            //((System.ComponentModel.ISupportInitialize)(manager)).EndInit();
            //tForm.ResumeLayout(false);

            System.Diagnostics.Debug.WriteLine($"[tMainForm.preparingMainForm] Completed. tForm.IsMdiContainer={tForm.IsMdiContainer}");

        }
    }
}
