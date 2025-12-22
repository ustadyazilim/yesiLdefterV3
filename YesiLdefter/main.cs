using DevExpress.LookAndFeel;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tkn_Events;
using Tkn_Ftp;
using Tkn_InputPanel;
using Tkn_Menu;
using Tkn_Registry;
using Tkn_SQLs;
using Tkn_Starter;
using Tkn_ToolBox;
using Tkn_Variable;

namespace YesiLdefter
{
    public partial class main : XtraForm
    {
        #region tanımlar

        //tEvents ev = new tEvents();
        tEventsForm evf = new tEventsForm();
        tToolBox t = new tToolBox();
        tInputPanel ip = new tInputPanel();
        tMenu mn = new tMenu();

        
        string TableIPCODE = string.Empty;

        Control toolboxControl1 = null;
        Control ribbonControl1 = null;
        object skinUserFirm = null;

        DevExpress.XtraBars.Ribbon.RibbonControl ribbon = null;

        DevExpress.XtraBars.BarStaticItem barMesajlar = null;
        DevExpress.XtraBars.BarButtonItem barButtonGuncelleme = null;
        DevExpress.XtraBars.BarButtonItem barButtonServiceTool = null;
        DevExpress.XtraBars.BarEditItem mainProgressBar = null;
        DevExpress.XtraBars.BarEditItem barEditItemCari = null;

        DevExpress.XtraBars.BarEditItem barPrjConn_ = null;
        DevExpress.XtraBars.BarEditItem barMSConn_ = null;

        /// <summary>
        /// 
        /// </summary>        
        CihazLogs cl = new CihazLogs();
        //DevExpress.XtraGrid.Views.Tile.TileView cihazTileView = null;
        /*
        System.Windows.Forms.Timer timerCihazLogGetIcmal = null;
        DataSet dsCihazLogIcmal = null;
        DataNavigator dNCihazLogIcmal = null;
        //bool cihazLogIcmalRefresh = false;
        //int cihazLogIcmalPos = 0;
        */

        #endregion

        private void mainForm_Load(object sender, EventArgs e)
        {
            //
        }
        private void mainForm_Shown(object sender, EventArgs e)
        {
            this.PerformLayout();
        }
        public main(string[] args)
        {
            preparinDefaultValues();

            #region Read Parameters
            bool params_ = false;
            if (args.Length > 0)
            {
                foreach (string arg in args)
                {
                    if (arg != "")
                    {
                        v.EXE_Arguments += " " + arg; 

                        if (arg.IndexOf("CEF") > -1)
                        {
                            // CefSharp ın çalışmasını durdur, çalışmasın.
                            // Aynı Exe birden fazla aynı anda çalışınca CefSharp hata veriyor
                            v.SP_Debug = true;
                        }
                        if (arg.IndexOf("UserId=") > -1)
                        {
                            v.tUser.UserId = t.myInt32(arg.Replace("UserId=", ""));
                            params_ = true;
                        }
                        if (arg.IndexOf("KurumTipi=") > -1)
                        {
                            v.SP_TabimParamsKurumTipi = arg.Replace("KurumTipi=", "");

                        }
                        if (arg.IndexOf("ServerName=") > -1)
                        {
                            v.SP_TabimParamsServerName = arg.Replace("ServerName=", "");
                        }
                    }
                }
            }

            //v.SP_Debug = true;
            //MessageBox.Show(v.EXE_Arguments);

            /// params giriş TESTi için 
            //params_ = true;
            //v.tUser.UserId = 60;// 12; 
            //v.SP_TabimParamsKurumTipi = "MTSK"; //"SRC";// 
            //v.SP_TabimParamsServerName = "LAPTOP-ACER1\\SQLEXPRESS";
            ////MessageBox.Show(params_.ToString() + " : " + v.tUser.UserId.ToString() + " : " + v.SP_TabimParamsKurumTipi + " : " + v.SP_TabimParamsServerName);

            #endregion

            #region preparing mainForm

            // formun kendisi
            InitializeComponent();

            /// Computer hakkındaki verileri topla
            /// 
            t.WaitFormOpen(v.mainForm, "Bilgisayar hakkındaki bilgiler okunuyor...");
            //Task task1 = new Task(() =>
            //{
               t.Get_MacAddress();
            //});
            //task1.Start();
            //Task task2 = new Task(() =>
            //{
               t.Get_ComputerAbout();
            //});
            //task2.Start();


            // ana ekranının hazırlanması
            // yani menuler burada hazırlanıyor
            using (tMainForm f = new tMainForm())
            {
                f.preparingMainForm(this);
                //f.preparingDockPanel(this, "SEK/CEV/prcCihazLogGetIcmal.Icmal_L01");
                v.timer_Kullaniciya_Mesaj_Var_ = timer_Kullaniciya_Mesaj_Var;
            }

            #endregion mainForm

            #region Starter
            
            t.WaitFormOpen(v.mainForm, "Program hazırlanmaya başlıyor ...");
            using (tStarter s = new tStarter())
            {
               s.InitStart();
            }

            v.SP_OpenApplication = false;
            v.IsWaitOpen = false;
            t.WaitFormOpen(v.mainForm, "");

            /// Main form size
            /// 
            this.Top = 0;
            this.Left = 0;
            this.Width = v.Primary_Screen_Width;
            this.Height = v.Primary_Screen_Height;

            #endregion

            v.Kullaniciya_Mesaj_Var = "YolHaritasi";
            timer_Mesaj_Suresini_Bitir.Interval = 500;
            v.timer_Kullaniciya_Mesaj_Var_.Start();

            #region UserLOGIN

            if (v.SP_UserLOGIN)
            {
                // application set skins
                t.getUserLookAndFeelSkins();

                // login işlemleri
                Login();

                if (v.active_DB.localDbUses == false)
                    t.DBUpdatesDataTransferOff();
            }
            
            if ((params_) && (v.tUser.UserId > 0))
            {
                /// Surucu07 için database update var mı?
                /// MsDbUpdates de güncelleme var mı kontrolet
                /// varsa Surucu07 yi update et
                /// 
                if (v.SP_TabimParamsKurumTipi == "MTSK") v.SP_Firm_SectorTypeId = 211; // TabimMtsk
                if (v.SP_TabimParamsKurumTipi == "ISMAK") v.SP_Firm_SectorTypeId = 212;
                if (v.SP_TabimParamsKurumTipi == "SRC") v.SP_Firm_SectorTypeId = 213;

                //MessageBox.Show("2 : " + params_.ToString() + " ; " + v.tUser.UserId.ToString());

                t.dbUpdatesChecked();

            }

            if ((v.SP_TabimParamsKurumTipi == "MTSK") ||
                (v.SP_TabimParamsKurumTipi == "ISMAK") ||
                (v.SP_TabimParamsKurumTipi == "SRC"))
            {
                //MessageBox.Show("TABIM");
                t.TabimMebAdayCountWrite();
            }

            #endregion

            if (v.tMainFirm.MenuCode == "SEK/CEV/AYR/MAINTOP")
            {
                System.Windows.Forms.Timer timerCihazLogGetIcmal = new System.Windows.Forms.Timer(this.components);
                cl.CihazLog(this, timerCihazLogGetIcmal);
            }

            #region -- açılışın sonu

            v.SP_OpenApplication = false;
            v.IsWaitOpen = false;
            t.WaitFormClose();

            //SplashScreenManager.CloseForm(false);
            v.SQL = "";
            #endregion
        }
        private void preparinDefaultValues()
        {
            // ... 
            // 
            System.Globalization.CultureInfo tr = new System.Globalization.CultureInfo("tr-TR");
            System.Threading.Thread.CurrentThread.CurrentCulture = tr;

            #region appOpenSetDefaaultSkin
            WindowsFormsSettings.EnableFormSkins();
            
            UserLookAndFeel.Default.SetSkinStyle(SkinSvgPalette.Bezier.Grasshopper);
            //UserLookAndFeel.Default.SetSkinStyle(SkinStyle.Sharp);//   Whiteprint);

            UserLookAndFeel.Default.StyleChanged += Default_StyleChanged;
            #endregion

            v.mainForm = this;
            v.Wait_Caption = v.Wait_Caption.PadRight(100);

            chechkedPaths();
            
            this.Load += new System.EventHandler(evf.myForm_Load);
            this.Shown += new System.EventHandler(evf.myForm_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(mainForm_KeyDown);
            //this.Activated += new System.EventHandler(mainForm_Activated);
            //this.Deactivate += new System.EventHandler(mainForm_Deactivate);
            this.KeyPreview = true;

            v.con_Search_NullText = "Arama listesi için  " + v.Key_SearchEngine + "  basın ...";

            t.WaitFormOpen(v.mainForm, "yesiLdefter hazırlanıyor ...");
            v.SP_OpenApplication = true;

        }

        #region Login

        void Login()
        {
            if (v.SP_UserLOGIN)
            {
                setMenuItems();
                
                t.WaitFormOpen(v.mainForm, "ManagerDB bağlantısı gerçekleşiyor...");
                t.Db_Open(v.active_DB.managerMSSQLConn);

                if (v.active_DB.projectDBType == v.dBaseType.MSSQL)
                {
                    t.WaitFormOpen(v.mainForm, "ProjectDB bağlantısı gerçekleşiyor...");
                    //t.Db_Open(v.active_DB.projectMSSQLConn);
                }
                
                t.WaitFormOpen(v.mainForm, "SysTypes tanımları okunuyor...");
                Task task1 = new Task(() =>
                {
                    t.SYS_Types_Read();
                });
                task1.Start();

                t.WaitFormOpen(v.mainForm, "Dönem listesi okunuyor...");
                Task task2 = new Task(() =>
                {
                    t.DonemTipiYilAyRead();
                });
                task2.Start();


                //t.WaitFormOpen(v.mainForm, "Read : SysGlyph ...");
                //t.SYS_Glyph_Read();

                t.WaitFormOpen(v.mainForm, "Menü hazırlanıyor...");
                preparingMenus();

                /// Ustad Crm ve TabimMtsk değil ise DbUpdates çalışacak
                /// TabimMtsk ya ait güncellemeler ise ms_TabimMtsk.cs içinde çalışıyor
                /// 
                if ((v.SP_Firm_SectorTypeId != (Int16)v.msSectorType.UstadCrm) && // Crm
                    (v.SP_Firm_SectorTypeId != (Int16)v.msSectorType.TabimMtsk))
                {
                    Task task3 = new Task(() =>
                    {
                        t.dbUpdatesChecked();
                    });
                    task3.Start();

                    if (v.SP_TabimParamsKurumTipi == "")
                    {
                        t.WaitFormOpen(v.mainForm, "İl ve İlçe listesi okunuyor...");
                        t.SYS_IL_Read();
                        t.SYS_Ilce_Read();
                        // Frmanın Il ve Ilçe adı atanıyor
                        t.preparing_FirmILAdi_IlceAdi();

                        t.WaitFormOpen(v.mainForm, "Takvim kontrolü yapılıyor...");
                        t.CrsTakvimiAyarla();

                        t.WaitFormOpen(v.mainForm, "......");
                    }
                }

                setMainFormCaption();
            }
        }

        async Task execute_prc_MtskGunlukTakipler()
        {
            tSQLs sqls = new tSQLs();
            DataSet ds = new DataSet();

            string sql = sqls.Sql_prc_MtskGunlukTakipler();

            t.SQL_Read_Execute(v.dBaseNo.Project, ds, ref sql, "prc_MtskGunlukTakipler", "");
            t.AlertMessage("Mesaj","Günlük başlangıç işlemler tamamlandı.");
        }

        void autoOpenForm(string FormCode, string FormName)
        {
            if (FormName == "")
                FormName = "null";

            string Prop_Navigator = @"
            0=FORMNAME:" + FormName + @";
            0=FORMCODE:" + FormCode + @";
            0=FORMTYPE:CHILD;
            0=FORMSTATE:NORMAL;
            ";

            t.OpenForm(new ms_Form(), Prop_Navigator);
        }

        void setMainFormCaption()
        {
            string verNo = "1.0";

            if (v.SP_TabimParamsKurumTipi == "")
                verNo = "2.0";
            else verNo = "1.0";

            this.Text = "YeşiL defter  " + verNo + ",   Sürüm : " +
                v.tExeAbout.activeVersionNo.Substring(2, 6) + "." +
                v.tExeAbout.activeVersionNo.Substring(9, 4) +
                "    [ " + v.tMainFirm.FirmId.ToString() + " : " + v.tMainFirm.FirmShortName + " ] ";
        }

        void chechkedPaths()
        {
            //string subPath = "Temp"; // Your code goes here
            //bool exists = System.IO.Directory.Exists(Server.MapPath(subPath));

            //if (!exists)
            
            getDriver();

        }

        private void MakeFolderWritable(string Folder)
        {
            if (IsFolderReadOnly(Folder))
            {
                System.IO.DirectoryInfo oDir = new System.IO.DirectoryInfo(Folder);
                oDir.Attributes = oDir.Attributes & ~System.IO.FileAttributes.ReadOnly;
                MessageBox.Show("path : Read Only");
            }
        }
        private bool IsFolderReadOnly(string Folder)
        {
            System.IO.DirectoryInfo oDir = new System.IO.DirectoryInfo(Folder);
            return ((oDir.Attributes & System.IO.FileAttributes.ReadOnly) > 0);
        }

        private void getDriver()
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            foreach (DriveInfo d in allDrives)
            {
                //if (v.EXE_PATH.IndexOf(d.Name) > -1)
                if (Path.GetDirectoryName(Application.ExecutablePath).IndexOf(d.Name) > -1)
                {
                    v.EXE_DRIVE = d.Name.ToString();
                    break;
                }

                /*
                Console.WriteLine("Drive {0}", d.Name);
                Console.WriteLine("  Drive type: {0}", d.DriveType);
                if (d.IsReady == true)
                {
                    Console.WriteLine("  Volume label: {0}", d.VolumeLabel);
                    Console.WriteLine("  File system: {0}", d.DriveFormat);
                    Console.WriteLine(
                        "  Available space to current user:{0, 15} bytes",
                        d.AvailableFreeSpace);

                    Console.WriteLine(
                        "  Total available space:          {0, 15} bytes",
                        d.TotalFreeSpace);

                    Console.WriteLine(
                        "  Total size of drive:            {0, 15} bytes ",
                        d.TotalSize);
                }
                */
            }
        }

        #endregion

        #region YolHaritasi
        void YolHaritasi()
        {
            bool onay = t.getSettingsBoolValue((Int16)v.settings.BaslangictaYapilmasiGerekenlerMenu);

            if (onay)
            {
                // Ön Muhasebe için başlangıç işlemleri
                //if (v.SP_Firm_SectorTypeId == (Int16)v.msSectorType.OnMuhasebe)
                //    autoOpenForm("UST/OMS/FNS/MALIISLEM","");

                //autoOpenForm("UST/OMS/AYR/YHBaslangic");
                if ((v.SP_TabimParamsKurumTipi == "") &&
                    ((v.SP_Firm_SectorTypeId == (Int16)v.msSectorType.UstadMtsk) ||
                    (v.SP_Firm_SectorTypeId == (Int16)v.msSectorType.TabimMtsk)))
                    autoOpenForm("UST/MEB/MTS/YHYasamDongusu", "");
                //autoOpenForm("UST/MEB/MTS/YHBaslangic","");

                if ((v.SP_TabimParamsKurumTipi == "") &&
                    ((v.SP_Firm_SectorTypeId == (Int16)v.msSectorType.UstadSrc) ||
                    (v.SP_Firm_SectorTypeId == (Int16)v.msSectorType.TabimSrc)))
                    autoOpenForm("UST/MEB/SRC/YHSrcYasamDongusu", "");

                ///// şimdilik geçici burada. Daha sonra günde bir defa çalışacak şekilde ayarlamak gerekiyor
                ///// 
                if (v.SP_Firm_SectorTypeId == (Int16)v.msSectorType.UstadMtsk)
                {
                    Task task1 = new Task(() =>
                    {
                        execute_prc_MtskGunlukTakipler();
                    });
                    task1.Start();
                }
            }
            else
            {
                autoOpenForm("UST/PMS/HUB/MainWebMtsk", "ms_CefSharp");
            }
        }
        #endregion

        #region preparingMenus

        void preparingMenus()
        {
            short menuType = mn.getCreateMenuType(v.tMainFirm.MenuCode);

            // kullanıcının seçtiği firmanın kullandığı menü
            //
            v.tMainFirm.MenuCodeOld = v.tMainFirm.MenuCode;
            //
            if (menuType == 102)
            {
                mn.Create_Menu(ribbon, v.tMainFirm.MenuCode, "");
                mn.alterRibbon(ribbon, "");

                ribbon.Minimized = false;
            }
            if (menuType == 109)
            {
                mn.Create_Menu(toolboxControl1, v.tMainFirm.MenuCode, "");
                mn.alterRibbon(ribbon, "yesiL");

                // MSV3 MENU gereklin olunca aç yoksa kapat
                //
                // mn.Create_Menu(ribbon, "UST/PMS/PMS/MsV3Menu", "");
            }

            // Kullanıcıların Ortak Menüsü
            //
            if (v.SP_TabimDbConnection == false)
                mn.Create_Menu(ribbon, "UST/PMS/PMS/PublicUser", "");

            if (v.tUser.UserDbTypeId == 21)  // Ustad Admin
            {
                t.WaitFormOpen(v.mainForm, "MsV3Menu menüsü hazırlanıyor...");
                mn.Create_Menu(ribbon, "UST/PMS/PMS/MsV3Menu", "");
            }

            setMenuItems();
        }

        void setMenuItems()
        {
            toolboxControl1 = t.Find_Control(this, "toolboxControl1");
            ribbonControl1 = t.Find_Control(this, "ribbonControl1");

            ribbon = ((DevExpress.XtraBars.Ribbon.RibbonControl)ribbonControl1);

            foreach (var item in ribbon.Items)
            {
                if (item.GetType().ToString() == "DevExpress.XtraBars.BarStaticItem")
                {
                    if ((item.ToString() == "Mesajlar") &&
                        (barMesajlar == null))
                    {
                        barMesajlar = (DevExpress.XtraBars.BarStaticItem)item;
                        barMesajlar.Caption = "";
                    }
                }

                if (item.GetType().ToString() == "DevExpress.XtraBars.BarButtonItem")
                {
                    if ((((DevExpress.XtraBars.BarButtonItem)item).Name.ToString() == "barButtonGuncelleme") &&
                        (barButtonGuncelleme == null))
                    {
                        barButtonGuncelleme = (DevExpress.XtraBars.BarButtonItem)item;
                        barButtonGuncelleme.ItemClick +=
                            new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonGuncelleme_ItemClick);
                    }
                }

                //barButtonServiceTool
                if (item.GetType().ToString() == "DevExpress.XtraBars.BarButtonItem")
                {
                    if ((((DevExpress.XtraBars.BarButtonItem)item).Name.ToString() == "barButtonServiceTool") &&
                        (barButtonServiceTool == null))
                    {
                        barButtonServiceTool = (DevExpress.XtraBars.BarButtonItem)item;
                        barButtonServiceTool.ItemClick +=
                            new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonServiceTool_ItemClick);
                    }
                }

                if (item.GetType().ToString() == "DevExpress.XtraBars.BarEditItem")
                {
                    if (((DevExpress.XtraBars.BarEditItem)item).Name == "mainProgressBar")
                        mainProgressBar = (DevExpress.XtraBars.BarEditItem)item;
                    if (((DevExpress.XtraBars.BarEditItem)item).Name == "barEditItemCari")
                        barEditItemCari = (DevExpress.XtraBars.BarEditItem)item;

                    if (((DevExpress.XtraBars.BarEditItem)item).Name == "barPrjConn")
                        barPrjConn_ = (DevExpress.XtraBars.BarEditItem)item;
                    if (((DevExpress.XtraBars.BarEditItem)item).Name == "barMSConn")
                        barMSConn_ = (DevExpress.XtraBars.BarEditItem)item;
                }
            }

            //string ss = "";

            int i3 = ribbon.Pages.Count;
            int i5 = 0;
            for (int i2 = 0; i2 < i3; i2++)
            {
                i5 = ribbon.Pages[i2].Groups.Count;

                for (int i4 = 0; i4 < i5; i4++)
                {
                    foreach (var item in ribbon.Pages[i2].Groups[i4].ItemLinks)
                    {
                        //ss = ss + item.GetType().ToString() + v.ENTER;

                        if (item.GetType().ToString() == "DevExpress.XtraBars.BarLargeButtonItemLink")
                        {
                            if (((DevExpress.XtraBars.BarLargeButtonItemLink)item).Item.Name.ToString() == "btnExeCompress")
                            {
                                ((DevExpress.XtraBars.BarLargeButtonItemLink)item).Item.ItemClick
                                      += new DevExpress.XtraBars.ItemClickEventHandler(this.btnExeCompress_ItemClick);
                            }
                            if (((DevExpress.XtraBars.BarLargeButtonItemLink)item).Item.Name.ToString() == "btnExeFtpUpload")
                            {
                                ((DevExpress.XtraBars.BarLargeButtonItemLink)item).Item.ItemClick
                                      += new DevExpress.XtraBars.ItemClickEventHandler(this.btnExeUpload_ItemClick);
                            }
                            if (((DevExpress.XtraBars.BarLargeButtonItemLink)item).Item.Name.ToString() == "btnExeFtpUploadTest")
                            {
                                ((DevExpress.XtraBars.BarLargeButtonItemLink)item).Item.ItemClick
                                      += new DevExpress.XtraBars.ItemClickEventHandler(this.btnExeUploadTest_ItemClick);
                            }

                            //btnFirmList
                            if (((DevExpress.XtraBars.BarLargeButtonItemLink)item).Item.Name.ToString() == "btnFirmList_FOPEN_IP")
                            {
                                ((DevExpress.XtraBars.BarLargeButtonItemLink)item).Item.ItemClick
                                      += new DevExpress.XtraBars.ItemClickEventHandler(this.btnFirmList_ItemClick);
                            }
                            //btnTEST (her türlü test kodları için bu eventi kullanabilirsin)
                            if (((DevExpress.XtraBars.BarLargeButtonItemLink)item).Item.Name.ToString() == "btnTEST")
                            {
                                ((DevExpress.XtraBars.BarLargeButtonItemLink)item).Item.ItemClick
                                      += new DevExpress.XtraBars.ItemClickEventHandler(this.btnTEST_ItemClick);
                            }
                        }
                    }

                }
            }

            selectedGroups((DevExpress.XtraToolbox.ToolboxControl)toolboxControl1);

            if (this.barMSConn_ != null)
            {
                if (v.active_DB.managerDBName != v.publishManager_DB.databaseName)
                    this.barMSConn_.Hint = v.active_DB.managerDBName + " ; " + v.publishManager_DB.databaseName;
                else this.barMSConn_.Hint = v.publishManager_DB.databaseName;
            }
            if (this.barPrjConn_ != null)
                this.barPrjConn_.Hint = v.active_DB.projectDBName;
                       
        }
               

        private void selectedGroups(DevExpress.XtraToolbox.ToolboxControl toolboxControl1)
        {
            /// Sağdaki Toolbox ın sıfırıncı group kullanıcı için seç  
            /// 
            if (toolboxControl1.Groups.Count == 1)
                toolboxControl1.SelectedGroupIndex = 0;
        }

        #endregion

        #region buttonsClick
        //
        private void ribbon_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            tToolBox t = new tToolBox();

            #region Tanımlar 
            string TableIPCode = string.Empty;
            string ButtonName = string.Empty;
            //string Button_Click_Type = string.Empty;
            string myFormLoadValue = string.Empty;
            #endregion Tanımlar

            if (e.Item.GetType().ToString() == "DevExpress.XtraBars.BarLargeButtonItem")
            {
                ButtonName = ((DevExpress.XtraBars.BarLargeButtonItem)e.Item).Name.ToString();
                TableIPCode = ((DevExpress.XtraBars.BarLargeButtonItem)e.Item).AccessibleName;
                myFormLoadValue = ((DevExpress.XtraBars.BarLargeButtonItem)e.Item).AccessibleDescription;

                // AccessibleDescription NEDEN boşalıyor sebebi bulunamadı
                if ((t.IsNotNull(myFormLoadValue) == false) &&
                    ((DevExpress.XtraBars.BarLargeButtonItem)e.Item).Tag != null)
                    myFormLoadValue = ((DevExpress.XtraBars.BarLargeButtonItem)e.Item).Tag.ToString();
            }

            if (e.Item.GetType().ToString() == "DevExpress.XtraBars.BarButtonItem")
            {
                ButtonName = ((DevExpress.XtraBars.BarButtonItem)e.Item).Name.ToString();
                TableIPCode = ((DevExpress.XtraBars.BarButtonItem)e.Item).AccessibleName;
                myFormLoadValue = ((DevExpress.XtraBars.BarButtonItem)e.Item).AccessibleDescription;

                // AccessibleDescription NEDEN boşalıyor sebebi bulunamadı
                if ((t.IsNotNull(myFormLoadValue) == false) &&
                    ((DevExpress.XtraBars.BarButtonItem)e.Item).Tag != null)
                    myFormLoadValue = ((DevExpress.XtraBars.BarButtonItem)e.Item).Tag.ToString();
            }

            //Form tForm = t.Find_Form(sender);

            //ButtonClickRUN(tForm, ButtonName, TableIPCode, myFormLoadValue);

        }

        //btnFirmList
        private void btnFirmList_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            // ms_UserFirmList formundaki 
            // btn_FirmListSec sonrası
            //
            t.AllFormsClose();
            //
            setMainFormCaption();

            // kullanıcının seçtiği firmanın kullandığı menü
            //
            if (v.tMainFirm.MenuCodeOld != v.tMainFirm.MenuCode)
            {
                t.WaitFormOpen(v.mainForm, "Dönem listesi okunuyor...");
                t.DonemTipiYilAyRead();

                // değişien firmanın menüsü 
                v.tMainFirm.MenuCodeOld = v.tMainFirm.MenuCode;

                t.WaitFormOpen(v.mainForm, "Menüler oluşturuluyor...");
                mn.Create_Menu(toolboxControl1, v.tMainFirm.MenuCode, "");

                selectedGroups((DevExpress.XtraToolbox.ToolboxControl)toolboxControl1);

                v.IsWaitOpen = false;
                t.WaitFormClose();
            }

            t.getUserLookAndFeelSkins();

            t.read_Settings();

            t.preparing_FirmILAdi_IlceAdi();

            YolHaritasi();
        }

        //btnTEST
        private void btnTEST_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            /// burada istediğin test kodunu çalıştırabilirsin
            /// işin bitince yazdıklarını silersen iyi olur
            ///

            //MessageBox.Show(UserLookAndFeel.Default.SkinName.ToString());
            MessageBox.Show(
                " SkinName : " + UserLookAndFeel.Default.SkinName.ToString() + " // " + v.ENTER +
                " ActiveSkinName : " + UserLookAndFeel.Default.ActiveSkinName.ToString() + " // " + v.ENTER +
                " ActiveStyle : " + UserLookAndFeel.Default.ActiveStyle.ToString() + " // " + v.ENTER +
                " ActiveSvgPaletteName : " + UserLookAndFeel.Default.ActiveSvgPaletteName.ToString()
                );
        }

        #endregion buttonsClick

        #region Compress
        private void btnExeCompress_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            t.CompressFile(v.tExeAbout, v.fileType.ActiveExe);
        }
                
        #endregion

        #region ftpUpload

        private void btnExeUpload_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (t.IsNotNull(v.tExeAbout.newPacketName) == false)
            {
                MessageBox.Show("DİKKAT : Önce -Exeyi Paketle- işlemini çalıştırın");
                return;
            }

            string soru = "Hazırlanan yeni EXE son kullanıcı için yüklenecek eminmisiniz ?";
            DialogResult Cevap = t.mySoru(soru);
            if (DialogResult.Yes == Cevap)
            {
                /// Son kullanıcı için // vt : UstadManagerV3.dbo.MsExeUpdates tablosuna yazılıyor 
                ///
                bool onay = t.ftpUpload(v.tExeAbout);
                if (onay)
                {
                    tSQLs sqls = new tSQLs();
                    DataSet ds = new DataSet();
                    string sql = sqls.Sql_MsExeUpdates_Insert();
                    t.SQL_Read_Execute(v.dBaseNo.publishManager, ds, ref sql, "", "MsExeUpdates");
                }
            }
        }

        private void btnExeUploadTest_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (t.IsNotNull(v.tExeAbout.newPacketName) == false)
            {
                MessageBox.Show("DİKKAT : Önce -Exeyi Paketle- işlemini çalıştırın");
                return;
            }
            /// Test exesi // vt : MainManagerV3.dbo.MsExeUpdates tablosuna yazılıyor
            ///
            bool onay = t.ftpTesterUpload(v.tExeAbout);
            if (onay)
            {
                tSQLs sqls = new tSQLs();
                DataSet ds = new DataSet();
                string sql = sqls.Sql_MsExeUpdates_Insert();
                t.SQL_Read_Execute(v.dBaseNo.Manager, ds, ref sql, "", "MsExeUpdates");
            }
        }
        #endregion ftpUpload

        #region ftpDownload 

        private void barButtonGuncelleme_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //RunExeUpdate();
        }
        //barButtonServiceTool
        private void barButtonServiceTool_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            vUserInputBox iBox = new vUserInputBox();

            iBox.Clear();
            iBox.title = "Service tools";
            iBox.promptText = "Service tool code  :";
            iBox.value = "";
            iBox.displayFormat = "*";
            iBox.fieldType = 0;

            // ınput box ile sorulan ise kimden kopyalanacak (old) bilgisi
            if (t.UserInpuBox(iBox) == DialogResult.OK)
            {
                string userCode = iBox.value;

                if (userCode == v.destekServiceToolCode)
                {
                    string FormName = "ms_DestekServiceTool";
                    string FormCode = "UST/PMS/PMS/DestekServiceTool";

                    if (v.SP_TabimDbConnection)
                        FormCode = "UST/PMS/PMS/DestekServiceTabim";

                    t.OpenFormPreparing(FormName, FormCode, v.formType.Child);
                }
            }

        }
        #endregion

        #region skinChanged

        private void Default_StyleChanged(object sender, EventArgs e)
        {
            //throw new NotImplementedException();

            if (v.sp_activeSkinName != "READ")
            {
                v.sp_SelectSkinName = UserLookAndFeel.Default.ActiveSkinName.ToString();
                if (UserLookAndFeel.Default.ActiveSkinName.ToString().IndexOf(v.sp_deactiveSkinName) > -1) return;
                if (UserLookAndFeel.Default.ActiveSvgPaletteName.ToString().IndexOf(v.sp_deactiveSkinName) > -1) return;
            }
            //else  v.sp_activeSkinName = "";

            // eğer yeni load oluyarsa buradaki işlemler çalışmasın
            if (skinUserFirm != null) return;
            
            if (v.sp_activeSkinName != "STARTER")
                setUserLookAndFeelSkins();
        }

        private void setUserLookAndFeelSkins()
        {
            var item = UserLookAndFeel.Default.ActiveSkinName.ToString();
            v.sp_SelectSkinName = item.ToString();

            if (UserLookAndFeel.Default.ActiveSvgPaletteName.ToString() != "")
            {
                /// object olarak set edileceği zaman
                /// UserLookAndFeel.Default.SetSkinStyle(SkinSvgPalette.Office2019Colorful.Forest);
                /// UserLookAndFeel.Default.SetSkinStyle(SkinSvgPalette.Bezier.LeafRustle);

                /// string olarak set edileceği zaman
                /// Office 2019 Colorful||Forest
                /// The Bezier||LeafRustle
                item = item + "||" + UserLookAndFeel.Default.ActiveSvgPaletteName.ToString();
            }

            tRegistry reg = new tRegistry();
            // tüm kullanıcılar için geçerli
            //reg.SetUstadRegistry("userSkin", e.Item.Value.ToString());
            // kullanıcı bazlı
            //reg.SetUstadRegistry("userSkin_" + v.tUser.UserId.ToString(), e.Item.Value.ToString());
            // kullanıcı kullandığı her firma için ayrı ayrı
            reg.SetUstadRegistry("userSkin_" + v.tUser.UserId.ToString() + "_" + v.tMainFirm.FirmId.ToString(),  //v.SP_FIRM_ID.ToString(),
                                 item.ToString());
            /*
            MessageBox.Show(
               " SkinName : " + UserLookAndFeel.Default.SkinName.ToString() + " // " + v.ENTER +
               " ActiveSkinName : " + UserLookAndFeel.Default.ActiveSkinName.ToString() + " // " + v.ENTER +
               " ActiveStyle : " + UserLookAndFeel.Default.ActiveStyle.ToString() + " // " + v.ENTER +
               " ActiveSvgPaletteName : " + UserLookAndFeel.Default.ActiveSvgPaletteName.ToString() + v.ENTER2 +
               " newName : " + item.ToString()
               );
            */

            //this.Text = this.Text + ",s";
        }

        #endregion

        #region timer_

        private void timer_Mesaj_Suresini_Bitir_Tick(object sender, EventArgs e)
        {
            if (v.Kullaniciya_Mesaj_Var == "YolHaritasi")
            {
                YolHaritasi();
                v.Kullaniciya_Mesaj_Var = "";
                timer_Mesaj_Suresini_Bitir.Interval = 6000;
                return;
            }

            /// mesaj gösteriminni bitir, mesaj kutusunu temizle
            if (barMesajlar.Caption != "")
            {
                barMesajlar.Caption = "";
                v.Kullaniciya_Mesaj_Var = "";
                timer_Mesaj_Suresini_Bitir.Enabled = false;
            }
        }

        private void timer_Kullaniciya_Mesaj_Var_Tick(object sender, EventArgs e)
        {
            if (barMesajlar == null) return;

            this.barMSConn_.EditValue = v.SP_ConnBool_Manager;

            this.barPrjConn_.EditValue = v.SP_ConnBool_Project;

            if (!string.IsNullOrEmpty(v.Kullaniciya_Mesaj_Var))
            {
                barMesajlar.Caption = v.Kullaniciya_Mesaj_Var;
                timer_Mesaj_Suresini_Bitir.Enabled = true;
                //v.Kullaniciya_Mesaj_Var = "";
            }
        }

        #endregion

        #region Events
        public void mainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Handled) return;

            tEventsForm evf = new tEventsForm();

            // sadece main form açıkken
            if (Application.OpenForms.Count == 1)
            {
                e.Handled = evf.myMenuShortKeyClick((Form)sender, e.KeyCode.ToString());

                if (e.Handled == false)
                    if ((e.Control) && (e.KeyCode != Keys.ControlKey))
                    {
                        e.Handled = evf.myMenuShortKeyClick((Form)sender, "C+" + e.KeyCode);
                    }

                if (e.Handled == false)
                    if (e.KeyCode == Keys.Escape)
                    {
                        tToolBox t = new tToolBox();
                        //Program kapatılacak, kapatmak istediğinize emin misiniz
                        DialogResult cevap = t.mySoru("EXIT");
                        if (DialogResult.Yes == cevap)
                        {
                            Application.Exit();
                        }
                    }
            }
        }
               
        public void mainForm_Activated(object sender, EventArgs e)
        {
            //v.Kullaniciya_Mesaj_Var = "Form Activated";
        }

        public void mainForm_Deactivate(object sender, EventArgs e)
        {
            //MessageBox.Show("myForm_Deactivate : " + ((Form)sender).Text);
        }

        private void main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (v.SP_ApplicationExit == false)
            {
                tToolBox t = new tToolBox();
                // Program kapatılacak, kapatmak istediğinize emin misiniz
                DialogResult cevap = t.mySoru("EXIT");
                if (DialogResult.Yes == cevap)
                {
                    //
                }
                else e.Cancel = true; // Main formun kapanmasını engeller
            }
        }

        private void main_DpiChangedAfterParent(object sender, EventArgs e)
        {
            MessageBox.Show("aaaaa");
        }


        #endregion Events

        /*
        internal class CustomApplicationSettings : ApplicationSettingsBase
        {
            [UserScopedSetting(), DefaultSettingValue("Sharp")]
            public string SkinName
            {
                get { return this("SkinName").ToString(); }
                set { this("SkinName") = value; }
            }
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            CustomApplicationSettings ApplicationSettings = new CustomApplicationSettings();
            UserLookAndFeel.Default.SkinName = ApplicationSettings.SkinName;
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            ApplicationSettings.SkinName = UserLookAndFeel.Default.SkinName;
            ApplicationSettings.Save();
        }
        */
    }
}
