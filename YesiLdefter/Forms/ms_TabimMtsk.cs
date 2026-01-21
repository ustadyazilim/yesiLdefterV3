using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Tkn_CreateDatabase;
using Tkn_Events;
using Tkn_Save;
using Tkn_ToolBox;
using Tkn_Variable;
using Tkn_IniFile;
using Tkn_SQLs;
using Tkn_UserFirms;

namespace YesiLdefter
{
    public partial class ms_TabimMtsk : DevExpress.XtraEditors.XtraForm
    {
        tToolBox t = new tToolBox();
        tDatabase db = new tDatabase();
        tSQLs Sqls = new tSQLs();
        tUserFirms userFirms = new tUserFirms();

        DataSet ds_DbConnValues = null;
        DataNavigator dN_DbConnValues = null;
        // UL = UserLogin
        DataSet ds_UL = null;
        DataNavigator dN_UL = null;
        // UserAbout
        DataSet ds_UserAbout = null;
        DataNavigator dN_UserAbout = null;
        // FL = FirmList
        //DataSet dsUserFirmList = null;
        //DataNavigator dNUserFirmList = null;
        // Sorgular için
        DataSet ds_Query = new DataSet();
        DataNavigator dN_Query = new DataNavigator();
        // UstadFirms için
        DataSet ds_UstadFirms = null;
        DataNavigator dN_UstadFirms = null;

        Control cmb_Username_ = null;
        Control txt_Password_ = null;
        Control btn_Card_F01_Save = null;

        bool firstConnect = false;
        bool onayUserInput = false;

        string DbConnect_TableIPCode = "UST/MEB/DbConnect.Form_F01";
        string Login_TableIPCode = "UST/MEB/users.Login_F01";
        string UserAbout_TableIPCode = "UST/MEB/users.Card_F01";
        string UstadFirms_TableIPCode = "UST/CRM/UstadFirms.Tabim_F01";
        
        string tSql = string.Empty;
        string u_user_name = string.Empty;
        string u_user_key = string.Empty;
        string regPath = v.registryPath;//"Software\\Üstad\\YesiLdefter";

        Control backstageViewControl_ = null;
        Control kullaniciBilgileri_ = null;
        Control firmaBilgileri_ = null;
            
        public ms_TabimMtsk()
        {
            InitializeComponent();

            tEventsForm evf = new tEventsForm();

            this.Load += new System.EventHandler(evf.myForm_Load);
            this.Shown += new System.EventHandler(evf.myForm_Shown);
            this.Shown += new System.EventHandler(this.ms_TabimMtsk_Shown);

            this.KeyPreview = true;

            if (v.active_DB.localServerName.ToLower() == "null")
            {
                /// 1. işlem
                /// 
                readTabSurucuIni();
            }
        }

        private void ms_TabimMtsk_Shown(object sender, EventArgs e)
        {
            if (v.SP_ApplicationExit)
            {
                ((Form)sender).Close();
                return;
            }

            Application.DoEvents();
            
            v.SP_UserLOGIN = false;

            /// ViewControls ayarlar
            /// 
            preparingViewControls();
            
            /// Local Database hazırla
            /// 2. işlem
            /// 
            preparingLocalDatabase();
        }

        private void preparingViewControls()
        {
            backstageViewControl_ = t.Find_Control(this, v.lyt_Name + "20");
            if (backstageViewControl_ != null)
            {
                //DevExpress.XtraBars.Ribbon.BackstageViewControl
            }

            firmaBilgileri_ = t.Find_Control_View(this, UstadFirms_TableIPCode);
            if (firmaBilgileri_ != null)
            {
                ((DevExpress.XtraDataLayout.DataLayoutControl)firmaBilgileri_).Visible = false;
            }

            kullaniciBilgileri_ = t.Find_Control(this, v.lyt_Name + "20_50_10");
            if (kullaniciBilgileri_ != null)
            {
                ((System.Windows.Forms.TableLayoutPanel)kullaniciBilgileri_).Visible = false;
            }
        }
        private void preparingLocalDatabase()
        {
            bool onay = false;
            
            if (v.active_DB.localMSSQLConn != null)
            {
                onay = dbConnect(false);

                if ((onay) && (v.SP_TabimIniWrite))
                    writeTabSurucuIni();
            }

            if (onay)
            {
                /// Surucu07 içinde DbUpdates tablo kontrolleri
                /// yoksa oluştur
                /// 
                InitDbUpdatesTable();

                /// Surucu07 için database update var mı?
                /// MsDbUpdates de güncelleme var mı kontrolet
                /// varsa Surucu07 yi update et
                /// 
                t.dbUpdatesChecked();

                /// user tablosu hazır mı ?
                /// 
                onayUserInput = checkedSurucu07UserTable();
                                
                if (onayUserInput)
                {
                    /// DataSet ler tespit et
                    InitDataSets();
                    /// Butonları tespit et
                    InitButtons();
                    /// User tablosuna yeni eklenen fieldler olamdığı için 
                    /// ds tekrar okunarak yeni fieldlerin gelmesi sağlanıyor
                    if (ds_UserAbout != null)
                    {
                        /// field list tablosunu sil 
                        /// yeni fieldleri okusun
                        
                        t.TableFieldsListRefresh(this, ds_UserAbout);

                        t.TableRefresh(this, ds_UserAbout);
                    }
                    

                    /// Kullanıcı Adı combo sunun içi dolduruluyor
                    /// 
                    getUserRegistry();

                    /// Kullanıcı Adı combo su setFocus oluyor
                    /// 
                    if (cmb_Username_ != null)
                    {
                        ((DevExpress.XtraEditors.ComboBoxEdit)cmb_Username_).Focus();
                    }
                }
            }
        }

        private void InitDbUpdatesTable()
        {
            bool onay = false;
            /// Surucu07 database üzerinde yapılacak değişiklikleri takip için 
            /// dbo.DbUpdates ekleniyor

            /// 
            /// UstadMtsk
            /// Bunu 201 yapmamızın sebebi dbo.DbUpdates tablosunu scriptini almak için 
            /// iş bitimde 211 olacak
            v.SP_Firm_SectorTypeId = 201;

            /// DbUpdates table kontrolü
            /// yok ise dbo.DbUpdates ekleneyiyor
            onay = t.preparingCreateTable(v.dBaseNo.Local, "dbo", "DbUpdates", 201);

            /// TabimMtsk olsun
            /// 
            v.SP_Firm_SectorTypeId = 211;
            onay = t.preparingCreateTable(v.dBaseNo.Local, "dbo", "FileUpdates", 211);
        }
        private void InitDataSets()
        {
            t.Find_DataSet(this, ref ds_DbConnValues, ref dN_DbConnValues, DbConnect_TableIPCode);
            t.Find_DataSet(this, ref ds_UL, ref dN_UL, Login_TableIPCode);
            t.Find_DataSet(this, ref ds_UserAbout, ref dN_UserAbout, UserAbout_TableIPCode);
            t.Find_DataSet(this, ref ds_UstadFirms, ref dN_UstadFirms, UstadFirms_TableIPCode);
        }
        private void InitButtons()
        {
            Control cntrl = null;
            string[] controls = new string[] { };
                        
            #region DbConnection buttons

            cntrl = t.Find_Control(this, "simpleButton_ek1", DbConnect_TableIPCode, controls);
            if (cntrl != null)
            {
                ((DevExpress.XtraEditors.SimpleButton)cntrl).Dock = DockStyle.Right;
                ((DevExpress.XtraEditors.SimpleButton)cntrl).Click += new System.EventHandler(btn_DbConnectionTest);
            }

            cntrl = t.Find_Control(this, "simpleButton_ek2", DbConnect_TableIPCode, controls);
            if (cntrl != null)
            {
                ((DevExpress.XtraEditors.SimpleButton)cntrl).Dock = DockStyle.Left;
                ((DevExpress.XtraEditors.SimpleButton)cntrl).Click += new System.EventHandler(btn_GetDbConnectionValues);
            }

            #endregion DbConnection buttons

            #region User Login buttons

            cntrl = t.Find_Control(this, "simpleButton_ek1", Login_TableIPCode, controls);
            if (cntrl != null)
            {
                ((DevExpress.XtraEditors.SimpleButton)cntrl).Dock = DockStyle.Right;
                ((DevExpress.XtraEditors.SimpleButton)cntrl).Width = 75;
                ((DevExpress.XtraEditors.SimpleButton)cntrl).Click += new System.EventHandler(btn_SistemeGiris_Ileri);
                ((DevExpress.XtraEditors.SimpleButton)cntrl).Image = t.Find_Glyph("SIHIRBAZDEVAM16");
            }

            //btn_SifremiUnuttum = t.Find_Control(this, "simpleButton_ek2", Login_TableIPCode, controls);
            //if (btn_SifremiUnuttum != null)
            //{
            //    ((DevExpress.XtraEditors.SimpleButton)btn_SifremiUnuttum).Click += new System.EventHandler(btn_SifremiUnuttumClick);
            //}

            cmb_Username_ = t.Find_Control(this, "Column_Username_", Login_TableIPCode, controls);
            txt_Password_ = t.Find_Control(this, "Column_Password_", Login_TableIPCode, controls);
            btn_Card_F01_Save = t.Find_Control(this, "simpleButton_kaydet", UserAbout_TableIPCode, controls);

            if (txt_Password_ != null)
            {
                //((DevExpress.XtraEditors.TextEdit)txt_Pass).EnterMoveNextControl = true;
                //((DevExpress.XtraEditors.TextEdit)txt_Password_).Properties.PasswordChar = '*';
                ((DevExpress.XtraEditors.TextEdit)txt_Password_).KeyDown += new KeyEventHandler(txt_Password_KeyDown);
            }

            if (cmb_Username_ != null)
            {
                ((DevExpress.XtraEditors.ComboBoxEdit)cmb_Username_).EnterMoveNextControl = true;
                ((DevExpress.XtraEditors.ComboBoxEdit)cmb_Username_).EditValueChanged
                    += new System.EventHandler(cmb_Username_EditValueChanged);
            }

            if (btn_Card_F01_Save != null)
            {
                ((DevExpress.XtraEditors.SimpleButton)btn_Card_F01_Save).Click += new System.EventHandler(btn_Card_F01_SaveClick);
            }
                        
            #endregion User Login buttons
        }
                
        private bool checkedSurucu07UserTable()
        {
            bool onay = false;

            /// users tablosuna yeni fieldlerin eklenmesi gerekiyor
            /// bu fieldleri eklenip eklenmediğini kontrol ediyoruz
            /// 
            vTable vt = new vTable();
            vt.DBaseNo = v.dBaseNo.Local;
            vt.SchemasCode = "dbo";
            vt.TableName = "users";
            vt.ParentTable = "";
            vt.SqlScript = "";

            onay = db.tFieldFind(vt, "MebbisCode");

            return onay;
        }
        private void readTabSurucuIni()
        {
            bool onay = t.readTABSURUCU_INI();

            if (onay)
            {
                t.preparingLocalDbConnectionText();

                setDbConnectionValues();

                /// Database bağlantısını test et
                /// 
                onay = dbConnectionTest();

                /// onay false ise program kapansın
                /// 
                v.SP_ApplicationExit = !onay;
            }

            /*
            var tabSurucuIni = new tIniFile("c:\\Windows\\TABSURUCU.INI");
            if (tabSurucuIni != null)
            {
                v.active_DB.localServerName = tabSurucuIni.Read("cbServer_Text", "TfrmLoginDB");
                v.active_DB.localUserName = tabSurucuIni.Read("edUsername_Text", "TfrmLoginDB");
                v.active_DB.localPsw = tabSurucuIni.Read("edPassword_Text", "TfrmLoginDB");

                if (v.active_DB.localUserName == "") v.active_DB.localUserName = "TABIM";
                if (v.active_DB.localPsw == "") v.active_DB.localPsw = "312";
                if (t.IsNotNull(v.active_DB.localDBName) == false) v.active_DB.localDBName = "Surucu07";

                if (v.active_DB.localServerName == "")
                    v.active_DB.localServerName = t.Find_ListAvailableMSSQLServers();

                t.preparingLocalDbConnectionText();

                setDbConnectionValues();
                
                /// Database bağlantısını test et
                /// 
                bool onay = dbConnectionTest();
                
                /// onay false ise program kapansın
                /// 
                v.SP_ApplicationExit = !onay;
            }
            */
        }
        private void setDbConnectionValues()
        {
            if (ds_DbConnValues != null)
            {
                ds_DbConnValues.Tables[0].Rows[0]["ServerName"] = v.active_DB.localServerName;
                ds_DbConnValues.Tables[0].Rows[0]["DatabaseName"] = v.active_DB.localDBName;
                ds_DbConnValues.Tables[0].Rows[0]["UserName"] = v.active_DB.localUserName;
                ds_DbConnValues.Tables[0].Rows[0]["Pass"] = v.active_DB.localPsw;
                Application.DoEvents();
            }
        }
        private bool writeTabSurucuIni()
        {
            return t.writeYesiLdefterTabimIni();
        }
        private bool dbConnectionTest()
        {
            bool onay = dbConnect(true);
            if (onay)
            {
                writeTabSurucuIni();
                MessageBox.Show(":) " + v.active_DB.localDBName + " database bağlantısı başarıyla sağlanmıştır ...");
            }
            else
            {
                MessageBox.Show(":( " + v.active_DB.localDBName + " için database bağlantısı malesef gerçekleşmedi ..." 
                    + v.ENTER2 + "Lütfen Tabim Destek masasından yardım isteyiniz...");
            }
            return onay;
        }
        private void getUserRegistry()
        {
            try
            {
                userFirms.GetUserRegistry(regPath);

                if (cmb_Username_ != null)
                {
                    v.tUserRegister.userNameList.AddRange(getUserNameList());

                    /// cmb_Username_ listesi combo için okunuyor
                    ((DevExpress.XtraEditors.ComboBoxEdit)cmb_Username_).Properties.Items.AddRange(v.tUserRegister.userNameList);

                    /// en son giriş yapan username combonun text ine atanıyor
                    /// username_ in boş olması uygulama direkt olarak çalıştırıldı demek
                    /// username_ in dolu olması ise diğer exe tarafından çalıştırıldı demektir
                    /// 
                    if ((cmb_Username_ != null) && (t.IsNotNull(v.tUser.Username_) == false))
                        ((DevExpress.XtraEditors.ComboBoxEdit)cmb_Username_).EditValue = v.tUserRegister.UserLastLoginEMail;
                    else ((DevExpress.XtraEditors.ComboBoxEdit)cmb_Username_).EditValue = v.tUser.Username_;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Tabim kullanıcı giriş isminizi ve şifresinizi giriniz... Örnek : ADMIN, ******* ");
                //throw;
            }
            
        }
        private List<object> getUserNameList()
        {
            DataSet ds = t.getTabimValues("users", "and AKTIF = 1");

            List<object> itemList = new List<object>();

            if (ds != null)
            {
                foreach (DataRow item in ds.Tables[0].Rows)
                {
                    itemList.Add(item["Username_"].ToString());
                }
            }

            return itemList;
        }
        private bool dbConnect(bool Test)
        {
            bool onay = false;

            t.preparingLocalDbConnectionText();

            if (firstConnect == false)
            {
                v.active_DB.localMSSQLConn.StateChange += new StateChangeEventHandler(t.DBConnectStateProject);
                firstConnect = true;
            }

            if (v.active_DB.localMSSQLConn != null)
            {
                onay = t.Db_Open(v.active_DB.localMSSQLConn);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[ms_TabimMtsk] WARNING: localMSSQLConn is null");
                onay = false;
            }

            return onay;
        }

        #region buttons click
        private void btn_DbConnectionTest(object sender, EventArgs e)
        {
            dbConnectionTest();
        }
        private void btn_GetDbConnectionValues(object sender, EventArgs e)
        {
            setDbConnectionValues();
        }
        void btn_SistemeGiris_Ileri(object sender, EventArgs e)
        {
            checkedUserInput();
        }
        void cmb_Username_EditValueChanged(object sender, EventArgs e)
        {
            ((DevExpress.XtraEditors.TextEdit)txt_Password_).EditValue = "";
        }
        void txt_Password_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                checkedUserInput();
            }
        }
        void btn_Card_F01_SaveClick(object sender, EventArgs e)
        {
            //sil 
            //t.TableFieldsListRefresh(ds_UserAbout);

            v.tUser.FirstName = ds_UserAbout.Tables[0].Rows[0]["UserFirstName"].ToString();
            v.tUser.LastName = ds_UserAbout.Tables[0].Rows[0]["UserLastName"].ToString();
            v.tUser.MobileNo = ds_UserAbout.Tables[0].Rows[0]["UserMobileNo"].ToString();
            v.tUser.MebbisCode = ds_UserAbout.Tables[0].Rows[0]["MebbisCode"].ToString();
            v.tUser.MebbisPass = ds_UserAbout.Tables[0].Rows[0]["MebbisPass"].ToString();

            if (t.IsNotNull(v.tUser.FirstName) &&
                t.IsNotNull(v.tUser.LastName) &&
                t.IsNotNull(v.tUser.MobileNo) &&
                t.IsNotNull(v.tUser.MebbisCode) && 
                t.IsNotNull(v.tUser.MebbisPass))
            {
                v.SP_UserLOGIN = true;
                ///
                /// form close
                ///
                this.Close();
            }
        }

        /// kullanıcıyı konrol et
        /// firmGUID kontrol et 
        /// kullanıcının Mebbis kodunu kontrol et
        /// 
        void checkedUserInput()
        {
            bool onayFirmGuid = false;
            bool onayMebbisCode = false;

            // UL = UserLogin_F01
            if (t.IsNotNull(ds_UL) && (dN_UL != null))
            {
                if (dN_UL.Position > -1)
                {
                    /// buradaki bilgi databaseden gelmiyor kullanıcı elle giriyor
                    /// 
                    u_user_name = ((DevExpress.XtraEditors.ComboBoxEdit)cmb_Username_).EditValue.ToString();
                    u_user_key = ((DevExpress.XtraEditors.TextEdit)txt_Password_).EditValue.ToString();

                    v.tUserRegister.UserLastLoginEMail = u_user_name;
                    //v.tUserRegister.UserLastKey = u_user_key;

                    u_user_key = preparingPassword(u_user_key);

                    t.TableRemove(ds_Query);

                    /// şimdi [ userName ve anahtar ] databaseden kontrol ediliyor
                    /// 
                    tSql = Sqls.preparingTabimUsersSql(u_user_name, u_user_key, 0); // checkedUser
                    t.SQL_Read_Execute(v.dBaseNo.Local, ds_Query, ref tSql, "TabimUsers", "TabimLogin");

                    /// userName ve anahtar sonucu döndü ve kontrol zamanı
                    /// 
                    if (t.IsNotNull(ds_Query) == false)
                    {
                        /// böyle bir userName varmı diye kontrol et
                        /// 
                        checkedUser(u_user_name, "FIND");
                    }
                    else
                    {
                        if (ds_Query.Tables.Count > 0)
                            dN_Query.DataSource = ds_Query.Tables[0];

                        if (ds_Query.Tables[0].Rows.Count == 0)
                        {
                            /// böyle bir userName varmı diye kontrol et
                            /// 
                            checkedUser(u_user_name, "FIND");
                        }

                        if (ds_Query.Tables[0].Rows.Count > 1)
                        {
                            /// birden fazla kayıt geliyorsa aynı emailden birden fazla mevcut 
                            /// sorun var demektir. çünki bir email, bir kayıt esasına göre çalışılacak
                            /// 
                            MessageBox.Show(u_user_name + v.ENTER2 +
                                "Veritabanında birden fazla aynı kullanıcı ismi mevcut." + v.ENTER2 + "Lütfen Tabim Destek ekibine başvurun...", "Problem",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);

                            v.SP_UserLOGIN = false;
                        }

                        /// Başarılı giriş
                        /// 
                        if (ds_Query.Tables[0].Rows.Count == 1)
                        {
                            /// username ve pass girdikte sonra gelen sonucu değerlendir
                            /// user hakkındaki bilgileri  v.tUser  üzerine yükle
                            userFirms.UserTabimFirm(this, ds_Query, u_user_key);
                            //ref dsUserFirmList, ref dNUserFirmList, FirmList_TableIPCode);

                            /// FirmGUID belli değilse
                            onayFirmGuid = checkedFirm();
                            /// user Mebbis kodu varmı
                            onayMebbisCode = checkedUserMebbisCode();

                            /// Login onayı
                            ///
                            if ((onayFirmGuid) && (onayMebbisCode)) 
                            {
                                v.SP_UserLOGIN = true;
                                /// form close
                                ///
                                this.Close();
                            }
                            else
                            {
                                v.SP_UserLOGIN = false;
                                /// form close
                                ///
                                //MessageBox.Show("DİKKAT : Kullanıcı giriş hatası ... 1005 ");
                                //this.Close();

                                // Kullanıcı bilgileri alınıyor ona göre kontrol et

                            }
                        }
                    }
                }
            }
        }
        #endregion buttons click

        #region checkedUser
        void checkedUser(string userName, string work)
        {
            /// buraya kontrol için geliniyor
            /// userName : böyle bir kullanıcı adı var mı ?
            /// 
            read_UserNameControl(ds_Query, userName);

            if (ds_Query.Tables.Count == 1)
            {
                /// userName : kullanıcı yok ise
                /// 
                if (ds_Query.Tables[0].Rows.Count == 0)
                {
                    //string soru = u_user_name + "  böyle bir hesap bulunamadı. \r\n\r\n Yeni bir kullanıcı oluşturmak ister misiniz  ?";
                    //DialogResult cevap = t.mySoru(soru);
                    //if (DialogResult.Yes == cevap)
                    //{
                    //    t.SelectPage(this, "BACKVIEW", "NEWUSER", -1);
                    //}
                    MessageBox.Show(userName + "  böyle bir hesap bulunamadı.");
                }

                /// userName : kullanıcı ismi 1 adet var ise
                /// 
                if ((work == "FIND") & (ds_Query.Tables[0].Rows.Count == 1))
                {
                    MessageBox.Show(" Şifrenizde bir sorun olabilir.\r\n\r\n Yeniden deneyebilir veya yeni bir şifre alabilirsiniz.");
                }

                if ((work == "SEND_EMAIL") & (ds_Query.Tables[0].Rows.Count == 1))
                {
                    //int Id = t.myInt32(ds_Query.Tables[0].Rows[0]["UserId"].ToString());
                    //Int16 IsActive = t.myInt16(ds_Query.Tables[0].Rows[0]["IsActive"].ToString());
                    //string userFullName = ds_Query.Tables[0].Rows[0]["UserFullName"].ToString();
                    //u_db_user_key = ds_Query.Tables[0].Rows[0]["UserKey"].ToString();

                    //// 0.pasif, 1.aktif 
                    //if (IsActive < 2)
                    //{
                    //    Send_EMail(user_Email, userFullName, u_db_user_key);
                    //}
                    //else
                    //{

                    //}
                }
            }
        }
        void read_UserNameControl(DataSet ds, string userName)
        {
            tSql = Sqls.preparingTabimUsersSql(userName, "", -1); // read UserNameControl
            t.SQL_Read_Execute(v.dBaseNo.Local, ds, ref tSql, "TabimUsers", "FindUser");
        }
        #endregion checkedUser

        #region FirmGUID
        private bool checkedFirm()
        {
            bool onay = false;

            /// Surucu07 üzerindeki paramalt tablosundaki gerekli bilgileri oku
            /// GIBeArsivFaturaKullanici ve sifresi için sürekli okumak gerekiyor
            read_FirmAbout();


            if (v.tUser.UserFirmGUID == "")
            {
                /// Surucu07 üzerindeki paramalt tablosundaki gerekli bilgileri oku
                /// 
                /// read_FirmAbout();

                /// UstadCrm e kaydet ve FirmGUID üret
                /// Üretilen FirmGUID de hem paramalt tablosuna 
                /// hemde users tablosundaki tüm kullanıcılara ekle
                /// 
                onay = InitFirmGUID();
            }

            /// UstadCrm den gelen Firmaya ait bilgileri v.tMainFirm üzerine oku
            /// 
            onay = userFirms.getFirmAboutWithUserFirmGUID(v.tMainFirm.FirmGuid);
 


            return onay;
        }
        private void read_FirmAbout()
        {
            /// şimdi Mtsk Firma bilgileri databaseden okunuyor
            /// Surucu07.paramalt tablosundan gereken bilgileri oku
            /// 
            DataSet ds = new DataSet();
            tSql = Sqls.SQL_TabimValues("paramalt", " and ULASPARAMTOP = 1 or (ULASPARAMTOP = 3 and KOD in (321,322)  )  ");
            t.SQL_Read_Execute(v.dBaseNo.Local, ds, ref tSql, "paramalt", "KurumHakkinda");

            if (t.IsNotNull(ds))
            {
                foreach (DataRow item in ds.Tables[0].Rows)
                {
                    if (item["KOD"].ToString() == "101") v.tTabimFirm.KursunAdi = item["DEGER"].ToString();
                    if (item["KOD"].ToString() == "102") v.tTabimFirm.Adres1 = item["DEGER"].ToString();
                    if (item["KOD"].ToString() == "103") v.tTabimFirm.Adres2 = item["DEGER"].ToString();
                    if (item["KOD"].ToString() == "104") v.tTabimFirm.Telefon = item["DEGER"].ToString();
                    if (item["KOD"].ToString() == "105") v.tTabimFirm.KursMuduru = item["DEGER"].ToString();
                    if (item["KOD"].ToString() == "106") v.tTabimFirm.KurucuAdi = item["DEGER"].ToString();
                    if (item["KOD"].ToString() == "113") v.tTabimFirm.KursunKodu = item["DEGER"].ToString();
                    if (item["KOD"].ToString() == "321") v.tTabimFirm.MebbisKullaniciAdi = item["DEGER"].ToString();
                    if (item["KOD"].ToString() == "322") v.tTabimFirm.MebbisSifresi = item["DEGER"].ToString();
                    if (item["KOD"].ToString() == "119") v.tTabimFirm.FirmGUID = item["DEGER"].ToString();
                    // 21.02.2025 de eklendi 
                    if (item["KOD"].ToString() == "121") v.tTabimFirm.GIBeArsivFaturaKullaniciAdi = item["DEGER"].ToString();
                    if (item["KOD"].ToString() == "122") v.tTabimFirm.GIBeArsivFaturaSifresi = item["DEGER"].ToString();

                    /// ULASPARAMTOP = 1 and KOD = 119  FirmGUID 
                    /// aslında böyle bir bilgi TabimMtsk da yok
                    /// Bu FirmGUID = UstadCrm.UstadFirms.FirmGUID tarafından üretiliyor
                    /// 
                }
            }

            v.tMainFirm.FirmGIBeArsivFaturaCode = v.tTabimFirm.GIBeArsivFaturaKullaniciAdi;
            v.tMainFirm.FirmGIBeArsivFaturaPass = v.tTabimFirm.GIBeArsivFaturaSifresi;

            ds.Dispose();
        }
        private bool InitFirmGUID()
        {
            // FirmGUID yok ise
            bool onay = false;

            /// Kullanıcının firmGUID yok ise
            /// paramalt tablosundan gelen firmGUID var ise
            /// onu users tablosundaki tüm kullanıcalara update et
            /// 
            if ((v.tUser.UserFirmGUID == "") &&
                t.IsNotNull(v.tTabimFirm.FirmGUID))
            {
                /// mevcut FirmGUID bilgisini 
                /// Surucu07.paramalt tablosuna ve users tablosundaki tüm kullanıcılara ekle
                ///
                setUsersFirmGUID();

                onay = true;
            }    

            if (t.IsNotNull(v.tTabimFirm.FirmGUID) == false)
            {
                if (firmaBilgileri_ != null)
                {
                    ((DevExpress.XtraDataLayout.DataLayoutControl)firmaBilgileri_).Visible = true;
                }

                if (t.IsNotNull(ds_UstadFirms))
                {
                    ds_UstadFirms.Tables[0].Rows[0]["FirmLongName"] = v.tTabimFirm.KursunAdi;
                    ds_UstadFirms.Tables[0].Rows[0]["FirmShortName"] = v.tTabimFirm.KursunAdi;
                    ds_UstadFirms.Tables[0].Rows[0]["Address1"] = v.tTabimFirm.Adres1;
                    ds_UstadFirms.Tables[0].Rows[0]["Address2"] = v.tTabimFirm.Adres2;
                    ds_UstadFirms.Tables[0].Rows[0]["FirmPhone"] = v.tTabimFirm.Telefon;
                    ds_UstadFirms.Tables[0].Rows[0]["ManagerName"] = v.tTabimFirm.KursMuduru;
                    ds_UstadFirms.Tables[0].Rows[0]["FounderName"] = v.tTabimFirm.KurucuAdi;
                    ds_UstadFirms.Tables[0].Rows[0]["FirmCode"] = v.tTabimFirm.KursunKodu;
                    ds_UstadFirms.Tables[0].Rows[0]["MebbisCode"] = v.tTabimFirm.MebbisKullaniciAdi;
                    ds_UstadFirms.Tables[0].Rows[0]["MebbisPass"] = v.tTabimFirm.MebbisSifresi;

                    ds_UstadFirms.Tables[0].Rows[0]["SourceServerNameIP"] = v.active_DB.localServerName;
                    ds_UstadFirms.Tables[0].Rows[0]["SourceDatabaseName"] = v.active_DB.localDBName;
                    ds_UstadFirms.Tables[0].Rows[0]["SourceDbLoginName"] = v.active_DB.localUserName;
                    ds_UstadFirms.Tables[0].Rows[0]["SourceDbPass"] = v.active_DB.localPsw;

                    if (v.SP_TabimParamsKurumTipi == "SRC")
                    {
                        ds_UstadFirms.Tables[0].Rows[0]["SectorTypeId"] = 213; // v.SP_Firm_SectorTypeId;
                        ds_UstadFirms.Tables[0].Rows[0]["MenuCode"] = "UST/MEB/TB2/MAIN";
                    }

                    ds_UstadFirms.Tables[0].AcceptChanges();

                    /// UstadCrm database kaydet
                    /// 
                    tSave sv = new tSave();
                    sv.tDataSave(this, ds_UstadFirms, dN_UstadFirms, dN_UstadFirms.Position);

                    /// FirmGUID geldi
                    /// 
                    v.tTabimFirm.FirmGUID = ds_UstadFirms.Tables[0].Rows[0]["FirmGUID"].ToString();
                    v.tUser.UserFirmGUID = v.tTabimFirm.FirmGUID;

                    if (v.tTabimFirm.FirmGUID != "")
                        onay = true;

                    /// yeni üretilen FirmGUID bilgisini 
                    /// Surucu07.paramalt tablosuna ve users tablosundaki tüm kullanıcılara ekle
                    ///
                    setUsersFirmGUID();
                }
            }
            
            return onay;
        }
        void setUsersFirmGUID()
        {
            /// yeni FirmGUID Surucu07 database ekleniyor
            ///
            if (v.tTabimFirm.FirmGUID != "")
            {
                DataSet ds = new DataSet();
                tSql = Sqls.updateTabimFirmGUIDSql(v.tTabimFirm.FirmGUID);
                t.SQL_Read_Execute(v.dBaseNo.Local, ds, ref tSql, "FirmGUID", "newFirmGUID");

                if (t.IsNotNull(ds))
                {
                    v.tUser.UserFirmGUID = v.tTabimFirm.FirmGUID;
                }
            }
        }
        #endregion FirmGUID

        #region checkedUserMebbisCode
        private bool checkedUserMebbisCode()
        {
            bool onay = false;
            if (t.IsNotNull(v.tUser.MebbisCode) && t.IsNotNull(v.tUser.MebbisPass))
            {
                onay = true;
            }
            else
            {
                if (backstageViewControl_ != null)
                {
                    ((DevExpress.XtraBars.Ribbon.BackstageViewControl)backstageViewControl_).SelectedTabIndex = 4;
                }
                if (kullaniciBilgileri_ != null)
                {
                    ((System.Windows.Forms.TableLayoutPanel)kullaniciBilgileri_).Visible = true;
                }
                preparingUserMebbisCode(ds_UserAbout);
            }
            return onay;
        }
        private bool preparingUserMebbisCode(DataSet ds)
        {
            bool onay = false;

            if (v.tUser.UserGUID == "")
                v.tUser.UserGUID = Guid.NewGuid().ToString();

            ds.Tables[0].Rows[0]["Ulas"] = v.tUser.UserId;
            ds.Tables[0].Rows[0]["Username_"] = v.tUser.Username_;
            ds.Tables[0].Rows[0]["UserGUID"] = v.tUser.UserGUID;
            ds.Tables[0].Rows[0]["FirmGUID"] = v.tUser.UserFirmGUID;
            ds.Tables[0].Rows[0]["UserFullName"] = v.tUser.FirstName + ' ' + v.tUser.LastName;
            ds.Tables[0].Rows[0]["UserFirstName"] = v.tUser.FirstName;
            ds.Tables[0].Rows[0]["UserLastName"] = v.tUser.LastName;
            ds.Tables[0].Rows[0]["UserTcNo"] = v.tUser.UserTcNo;
            ds.Tables[0].Rows[0]["UserEMail"] = v.tUser.eMail;
            ds.Tables[0].Rows[0]["UserMobileNo"] = v.tUser.MobileNo;
            ds.Tables[0].Rows[0]["MebbisCode"] = v.tUser.MebbisCode;
            ds.Tables[0].Rows[0]["MebbisPass"] = v.tUser.MebbisPass;

            return onay;
        }
        #endregion checkedUserMebbisCode

        #region Password
        private string preparingPassword(string inputCase)
        {
            string newCase = "";
            char ch;
            int xx = 0;

            for (int i = 1; i <= inputCase.Length; i++)
            {
                switch (i % 4)
                {
                    case 0:
                        {
                            ch = inputCase[i-1];
                            xx = 7 + checkedChar(ch);
                            newCase += (char)xx;
                            break; // break ifadesini sakın silme
                        }
                    case 1:
                        {
                            ch = inputCase[i-1];
                            xx = 145 - checkedChar(ch);
                            newCase += (char)xx;
                            break; 
                        }
                    case 2:
                        {
                            ch = inputCase[i-1];
                            xx = 155 - checkedChar(ch);
                            newCase += (char)xx;
                            break; 
                        }
                    case 3:
                        {
                            ch = inputCase[i-1];
                            xx = (134 - checkedChar(ch)) + 7;
                            newCase += (char)xx;
                            break; 
                        }
                }
            }
            return newCase;
        }
        private char checkedChar(char ch)
        {
            //if (ch == 286) ch = (char)208; // Ğ
            //if (ch == 350) ch = (char)222; // Ş
            if (ch == 304) ch = (char)221; // İ
            //if (ch == 287) ch = (char)240; // ğ
            //if (ch == 351) ch = (char)254; // ş
            return ch;
        }
        private string preparingEncodePassword(string inputCase)
        {
            string newCase = "";
            char ch;
            int xx = 0;

            for (int i = 1; i <= inputCase.Length; i++)
            {
                switch (i % 4)
                {
                    case 0:
                        {
                            ch = inputCase[i - 1];
                            xx = ch - 7;
                            newCase += (char)xx;
                            break; // break ifadesini sakın silme
                        }
                    case 1:
                        {
                            ch = inputCase[i - 1];
                            xx = 145 - ch;
                            newCase += (char)xx;
                            break;
                        }
                    case 2:
                        {
                            ch = inputCase[i - 1];
                            xx = 155 - ch;
                            newCase += (char)xx;
                            break;
                        }
                    case 3:
                        {
                            ch = inputCase[i - 1];
                            xx = (134 - ch) + 7;
                            newCase += (char)xx;
                            break;
                        }
                }
            }

            return newCase;
        }
        #endregion Password

        

    }

}
