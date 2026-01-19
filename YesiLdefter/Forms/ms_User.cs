using DevExpress.XtraEditors;
using System;
using System.Data;
using System.Windows.Forms;
using Tkn_Events;
using Tkn_Registry;
using Tkn_Save;
using Tkn_SQLs;
using Tkn_ToolBox;
using Tkn_Variable;
using Tkn_UserFirms;

namespace YesiLdefter
{
    public partial class ms_User : DevExpress.XtraEditors.XtraForm
    {
        #region Tanımlar

        tToolBox t = new tToolBox();
        tSQLs Sqls = new tSQLs();
        tRegistry reg = new tRegistry();
        tUserFirms userFirms = new tUserFirms();

        // UL = UserLogin
        DataSet ds_UL = null;
        DataNavigator dN_UL = null;
        // NU = NewUser
        DataSet ds_NU = null;
        DataNavigator dN_NU = null;
        // UK = Key
        DataSet ds_UK = null;
        DataNavigator dN_UK = null;
        // FL = FirmList
        DataSet dsUserFirmList = null;
        DataNavigator dNUserFirmList = null;

        // sorgular için
        DataNavigator dN_Query = new DataNavigator();
        DataSet ds_Query = new DataSet();
        DataSet ds_Query2 = new DataSet();

        Control cmb_EMail = null;
        Control txt_Pass = null;
        Control btn_BHatirla = null;
        Control btn_SifremiUnuttum = null;
        
        Control uk_user_mail = null;
        Control uk_old_user_pass = null;
        Control uk_new_user_pass = null;
        Control uk_rpt_user_pass = null;


        int u_user_Last_FirmId = 0;
        string u_user_email = string.Empty;
        string u_user_key = string.Empty;
        string u_db_user_key = string.Empty;

        string tSql = string.Empty;
        string TableIPCode = string.Empty;

        string Login_TableIPCode = "UST/CRM/UstadUsers.Login_F01"; 
        string NewPass_TableIPCode = "UST/CRM/UstadUsers.NewPassword_F01";
        string NewUser_TableIPCode = "UST/CRM/UstadUsers.NewUser_F01";
        //string FirmList_TableIPCode = "UST/CRM/UstadFirms.UserFirmList_L01";
        string FirmList_TableIPCode = "UST/CRM/UstadFirmsUsers.KullanicininFirmaSecimi_L01";

        string regPath = v.registryPath;//"Software\\Üstad\\YesiLdefter";
        #endregion

        public ms_User()
        {
            /// .
            /// comp - user - firm ilişkisi ?
            /// 
            /// prog açılışında 
            /// önce  user bilgileri
            /// sonra comp bilgileri

            // burası değişti 
            /// user için tanımlı olan user_firm_guid varsa o firma/shop 
            /// yoksa 
            /// comp kartında tanımlı olan comp_firm_guid e göre firma/shop baz alınacak

            /// 
            /// yani kullanıcının kendisi için tanımlı firmaları var ise o listeye göre çalışır
            /// eğer kullanıcı için firm_guid yok ise Comp için tanımlı olan firma çalışır
            /// 
            /// comp taki veya user daki xxxx_firm_guid hangisi olursa olsun
            /// kayıtlı bu firm_guidin kendisi ve kendisine bağlı alt firm's/shop's listelenir
            /// böyle birden fazla firma ve şube gelirse 
            /// kullanıcıya hangisinde işlem yapacağı (firma listesinden) sorulur
            /// tek firma veya shop olursa kullanıcaya sorulmadan direk program o firm_id üzerinden
            /// çalışmaya başlar.
            /// 
            /// Peki comp'unda firm_guidi yoksa o zamanda kullanıcının karşına bizim TEST firmaları listelenir
            /// 

            InitializeComponent();

            tEventsForm evf = new tEventsForm();

            this.Load += new System.EventHandler(evf.myForm_Load);
            this.Shown += new System.EventHandler(evf.myForm_Shown);
            this.Shown += new System.EventHandler(this.ms_User_Shown);

            this.KeyPreview = true;
        }

        private void ms_User_Shown(object sender, EventArgs e)
        {
            //
            // Sisteme Giriş // User Login ---------------------------------------
            //
            #region
            Application.DoEvents();

            t.Find_DataSet(this, ref ds_UL, ref dN_UL, Login_TableIPCode);

            Control cntrl = null;
            string[] controls = new string[] { };
            cntrl = t.Find_Control(this, "simpleButton_ek1", Login_TableIPCode, controls);

            if (cntrl != null)
            {
                ((DevExpress.XtraEditors.SimpleButton)cntrl).Dock = DockStyle.Right;
                ((DevExpress.XtraEditors.SimpleButton)cntrl).Width = 75;
                //((DevExpress.XtraEditors.SimpleButton)cntrl).Text = "İleri";
                ((DevExpress.XtraEditors.SimpleButton)cntrl).Click += new System.EventHandler(btn_SistemeGiris_Ileri);
                ((DevExpress.XtraEditors.SimpleButton)cntrl).Image = t.Find_Glyph("SIHIRBAZDEVAM16");
            }

            btn_BHatirla = t.Find_Control(this, "checkButton_ek1", Login_TableIPCode, controls);

            btn_SifremiUnuttum = t.Find_Control(this, "simpleButton_ek2", Login_TableIPCode, controls);
            if (btn_SifremiUnuttum != null)
            {
                ((DevExpress.XtraEditors.SimpleButton)btn_SifremiUnuttum).Click += new System.EventHandler(btn_SifremiUnuttumClick);
            }

            cmb_EMail = t.Find_Control(this, "Column_UserEMail", Login_TableIPCode, controls);
            txt_Pass = t.Find_Control(this, "Column_UserKey", Login_TableIPCode, controls);

            if (cmb_EMail != null)
            {
                ((DevExpress.XtraEditors.ComboBoxEdit)cmb_EMail).EnterMoveNextControl = true;
                ((DevExpress.XtraEditors.ComboBoxEdit)cmb_EMail).EditValueChanged
                    += new System.EventHandler(cmb_EMail_EditValueChanged);
            }

            if (txt_Pass != null)
            {
                //((DevExpress.XtraEditors.TextEdit)txt_Pass).EnterMoveNextControl = true;
                ((DevExpress.XtraEditors.TextEdit)txt_Pass).KeyDown += new KeyEventHandler(txt_Pass_KeyDown);
                //((DevExpress.XtraEditors.TextEdit)txt_Pass).Properties.PasswordChar = '*';
            }
            #endregion
            //
            // Yeni Kullanıcı // New User ----------------------------------------
            // 
            #region
            t.Find_DataSet(this, ref ds_NU, ref dN_NU, NewUser_TableIPCode);

            cntrl = t.Find_Control(this, "simpleButton_ek1", NewUser_TableIPCode, controls);

            if (cntrl != null)
            {
                ((DevExpress.XtraEditors.SimpleButton)cntrl).Dock = DockStyle.Right;
                ((DevExpress.XtraEditors.SimpleButton)cntrl).Click += new System.EventHandler(btn_YeniKullanici_Kaydet);
            }
            #endregion
            //
            // Şifre Değişikliği -------------------------------------------------
            // 
            #region
            t.Find_DataSet(this, ref ds_UK, ref dN_UK, NewPass_TableIPCode);

            cntrl = t.Find_Control(this, "simpleButton_ek1", NewPass_TableIPCode, controls);

            if (cntrl != null)
            {
                ((DevExpress.XtraEditors.SimpleButton)cntrl).Dock = DockStyle.Right;
                ((DevExpress.XtraEditors.SimpleButton)cntrl).Click += new System.EventHandler(btn_Yeni_SifreClick);
                ((DevExpress.XtraEditors.SimpleButton)cntrl).Image = t.Find_Glyph("KAYDET16");
            }

            uk_user_mail = t.Find_Control(this, "Column_UserEMail", NewPass_TableIPCode, controls);
            if (uk_user_mail != null)
            {
                ((DevExpress.XtraEditors.TextEdit)uk_user_mail).EnterMoveNextControl = true;
            }

            uk_old_user_pass = t.Find_Control(this, "Column_UserFirstName", NewPass_TableIPCode, controls);
            if (uk_old_user_pass != null)
            {
                ((DevExpress.XtraEditors.TextEdit)uk_old_user_pass).EnterMoveNextControl = true;
                //((DevExpress.XtraEditors.TextEdit)uk_old_user_pass).Properties.PasswordChar = '*';
            }

            uk_new_user_pass = t.Find_Control(this, "Column_UserLastName", NewPass_TableIPCode, controls);
            if (uk_new_user_pass != null)
            {
                ((DevExpress.XtraEditors.TextEdit)uk_new_user_pass).EnterMoveNextControl = true;
                //((DevExpress.XtraEditors.TextEdit)uk_new_user_pass).Properties.PasswordChar = '*';
            }

            uk_rpt_user_pass = t.Find_Control(this, "Column_UserKey", NewPass_TableIPCode, controls);
            if (uk_rpt_user_pass != null)
            {
                ((DevExpress.XtraEditors.TextEdit)uk_rpt_user_pass).EnterMoveNextControl = true;
                //((DevExpress.XtraEditors.TextEdit)uk_rpt_user_pass).Properties.PasswordChar = '*';
            }
            #endregion

            //
            // FirmList ----------------------------------------------------------
            //
            #region
            cntrl = t.Find_Control(this, "simpleButton_ek1", FirmList_TableIPCode, controls);
            
            if (cntrl != null)
            {
                ((DevExpress.XtraEditors.SimpleButton)cntrl).Dock = DockStyle.Right;
                ((DevExpress.XtraEditors.SimpleButton)cntrl).Click += new System.EventHandler(btn_FirmListSec_Click);
                ((DevExpress.XtraEditors.SimpleButton)cntrl).Image = t.Find_Glyph("SIHIRBAZDEVAM16");
            }
            #endregion

            //
            // -------------------------------------------------------------------
            //
            GetUserRegistry();

            v.SP_UserLOGIN = false;

            if (cmb_EMail != null)
            {
                ((DevExpress.XtraEditors.ComboBoxEdit)cmb_EMail).Focus();
            }
        }

        void btn_SistemeGiris_Ileri(object sender, EventArgs e)
        {
            checkedInput();
        }

        void cmb_EMail_EditValueChanged(object sender, EventArgs e)
        {
            ((DevExpress.XtraEditors.TextEdit)txt_Pass).EditValue = "";
        }

        void txt_Pass_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                checkedInput();
            }
        }

        void checkedInput()
        {
            if (t.IsNotNull(ds_UL) && (dN_UL != null))
            {
                if (dN_UL.Position > -1)
                {
                    // buradaki bilgi databaseden gelmiyor kullanıcı elle giriyor
                    //user_email = ds_UL.Tables[0].Rows[dN_UL.Position]["USER_EMAIL"].ToString().Trim();
                    //user_key = ds_UL.Tables[0].Rows[dN_UL.Position]["USER_KEY"].ToString().Trim(); ;
                    u_user_email = ((DevExpress.XtraEditors.ComboBoxEdit)cmb_EMail).EditValue.ToString();
                    u_user_key = ((DevExpress.XtraEditors.TextEdit)txt_Pass).EditValue.ToString();

                    v.tUserRegister.UserLastLoginEMail = u_user_email;
                    v.tUserRegister.UserLastKey = u_user_key;
                    v.tUserRegister.UserRemember = ((DevExpress.XtraEditors.CheckButton)btn_BHatirla).Checked;
                                        
                    t.TableRemove(ds_Query);

                    // şimdi [ e-mail ile şifre ] databaseden kontrol ediliyor
                    tSql = Sqls.preparingUstadUsersSql(u_user_email, u_user_key, 0);
                    t.SQL_Read_Execute(v.dBaseNo.UstadCrm, ds_Query, ref tSql, "UstadUsers", "UserLogin");

                    if (t.IsNotNull(ds_Query) == false)
                    {
                        //böyle bir email varmı diye kontrol et
                        checkedUser(u_user_email, "FIND");
                    }
                    else
                    {
                        if (ds_Query.Tables.Count > 0)
                            dN_Query.DataSource = ds_Query.Tables[0];

                        if (ds_Query.Tables[0].Rows.Count == 0)
                        {
                            //böyle bir email varmı diye kontrol et
                            checkedUser(u_user_email, "FIND");
                        }

                        if (ds_Query.Tables[0].Rows.Count == 1)
                        {
                            //e-mail ve şifre girdikte sonra gelen sonucu değerlendir
                            userFirms.UserSelectFirm(this, ds_Query, dN_Query, u_user_key, 
                                ref dsUserFirmList, ref dNUserFirmList, FirmList_TableIPCode);
                        }

                        if (ds_Query.Tables[0].Rows.Count > 1)
                        {
                            /// birden fazla kayıt geliyorsa aynı emailden birden fazla mevcut 
                            /// sorun var demektir. çünki bir email, bir kayıt esasına göre çalışılacak
                            /// 
                            MessageBox.Show(u_user_email + v.ENTER2 +
                                "Veritabanında birden fazla aynı e-mail mevcut.", "Çok Ciddi Problem",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);

                            v.SP_UserLOGIN = false;
                        }
                    }
                }
            }
        }

        void read_eMail(DataSet ds, string user_EMail)
        {
            tSql = Sqls.preparingUstadUsersSql(user_EMail, "", 0);
            t.SQL_Read_Execute(v.dBaseNo.UstadCrm, ds, ref tSql, "UstadUsers", "FindUser");
        }

        void checkedUser(string user_Email, string work)
        {
            read_eMail(ds_Query, user_Email);

            if (ds_Query.Tables.Count == 1)
            {
                if (ds_Query.Tables[0].Rows.Count == 0)
                {
                    string soru = user_Email + "  böyle bir hesap bulunamadı. \r\n\r\n Yeni bir kullanıcı oluşturmak ister misiniz  ?";
                    DialogResult cevap = t.mySoru(soru);
                    if (DialogResult.Yes == cevap)
                    {
                        t.SelectPage(this, "BACKVIEW", "NEWUSER", -1);
                    }
                }

                if ((work == "FIND") & (ds_Query.Tables[0].Rows.Count == 1))
                {
                    MessageBox.Show(" Şifrenizde bir sorun olabilir.\r\n\r\n Yeniden deneyebilir veya yeni bir şifre alabilirsiniz.");
                }

                if ((work == "SEND_EMAIL") & (ds_Query.Tables[0].Rows.Count == 1))
                {
                    int Id = t.myInt32(ds_Query.Tables[0].Rows[0]["UserId"].ToString());
                    Int16 IsActive = t.myInt16(ds_Query.Tables[0].Rows[0]["IsActive"].ToString());
                    string userFullName = ds_Query.Tables[0].Rows[0]["UserFullName"].ToString();
                    u_db_user_key = ds_Query.Tables[0].Rows[0]["UserKey"].ToString();

                    // 0.pasif, 1.aktif 
                    if (IsActive < 2)
                    {
                        Send_EMail(user_Email, userFullName, u_db_user_key);
                    }
                    else
                    {

                    }
                }
            }
        }

        void Send_EMail(string myeMail, string myUserFullName, string myKey)
        {
            //
            eMail email = new eMail();

            email.toMailAddress = myeMail;
            email.subject = "Kullanıcı kodu ";
            email.message =
                "<p> Üstad Bilişim veritabanında kayıtlı olan, </p>" +
                "<p> Kullanıcı Adı Soyadı : " + myUserFullName + "<br>" +
                " Kullanıcı Şifreniz : " + myKey + "</p>";

            t.eMailSend3(email);
        }

        #region User giriş yaptı, firma seçti

        void btn_FirmListSec_Click(object sender, EventArgs e)
        {
            /// Buraya geldiysen 
            /// kullanıcı için birden fazla firma seçeneği var demek ki
            /// kullanıcı bu butona basar ve seçilen değerler alınır lets go ... main form
            ///
            if (t.IsNotNull(dsUserFirmList))
            {
                DataRow row = dsUserFirmList.Tables[0].Rows[dNUserFirmList.Position];
                userFirms.readUstadFirmAbout(this, row);
            }
        }
        
        #endregion User giriş yaptı, firma seçti

        void SetUserIsActive(int Id)
        {
            tSql = Sqls.preparingUstadUsersSql("", "", Id);
            t.SQL_Read_Execute(v.dBaseNo.UstadCrm, ds_Query2, ref tSql, "UstadUsers", "SetUserIsActive");
        }
                
        void GetUserRegistry()
        {
            userFirms.GetUserRegistry(regPath);
            
            if (cmb_EMail != null)
            {
                // e-mail listesi combo için okunuyor
                ((DevExpress.XtraEditors.ComboBoxEdit)cmb_EMail).Properties.Items.AddRange(v.tUserRegister.eMailList);
                // en son giriş yapan e-mail combonun text ine atanıyor
                if (cmb_EMail != null)
                    ((DevExpress.XtraEditors.ComboBoxEdit)cmb_EMail).EditValue = v.tUserRegister.UserLastLoginEMail;
                if (uk_user_mail != null)
                    ((DevExpress.XtraEditors.TextEdit)uk_user_mail).EditValue = v.tUserRegister.UserLastLoginEMail;
                if (btn_BHatirla != null)
                {
                    ((DevExpress.XtraEditors.CheckButton)btn_BHatirla).Checked = v.tUserRegister.UserRemember;

                    // beni hatırla onaylıysa enson girilen key okunuyor ve şifreye atanıyor
                    if (((DevExpress.XtraEditors.CheckButton)btn_BHatirla).Checked)
                        ((DevExpress.XtraEditors.TextEdit)txt_Pass).EditValue = v.tUserRegister.UserLastKey;
                }
                u_user_Last_FirmId = v.tUserRegister.UserLastFirmId;
            }
        }

        void btn_YeniKullanici_Kaydet(object sender, EventArgs e)
        {
            if (t.IsNotNull(ds_NU) && (dN_NU != null))
            {
                if (dN_NU.Position > -1)
                {
                    string user_Email = ds_NU.Tables[0].Rows[0]["UserEMail"].ToString();
                    read_eMail(ds_Query, user_Email);

                    if (ds_Query.Tables.Count == 1)
                    {
                        if (ds_Query.Tables[0].Rows.Count == 1)
                        {
                            MessageBox.Show(user_Email + "  hesabı mevcut. .....");
                            return;
                        }
                        else if (ds_Query.Tables[0].Rows.Count == 0)
                        {
                            tSave sv = new tSave();
                            if (sv.tDataSave(this, NewUser_TableIPCode))
                            {
                                //t.ButtonEnabledAll(tForm, TableIPCode, true);
                                MessageBox.Show(user_Email + " hesabınız kaydedilmiştir.\r\n\r\n Şifreniz bu hesaba GÖNDERİLECEK (Unutma bu kısım yazılmadı daha)");
                                t.SelectPage(this, "BACKVIEW", "USERLOGIN", -1);
                            }
                        }
                    }

                }
            }
        }

        void btn_SifremiUnuttumClick(object sender, EventArgs e)
        {
            u_user_email = ((DevExpress.XtraEditors.ComboBoxEdit)cmb_EMail).EditValue.ToString();

            checkedUser(u_user_email, "SEND_EMAIL");
        }

        void btn_Yeni_SifreClick(object sender, EventArgs e)
        {
            if (t.IsNotNull(ds_UK) && (dN_UK != null))
            {
                if (dN_UK.Position > -1)
                {
                    string user_email = ((DevExpress.XtraEditors.TextEdit)uk_user_mail).EditValue.ToString();
                    string user_old_pass = ((DevExpress.XtraEditors.TextEdit)uk_old_user_pass).EditValue.ToString();
                    string user_new_pass = ((DevExpress.XtraEditors.TextEdit)uk_new_user_pass).EditValue.ToString();
                    string user_rpt_pass = ((DevExpress.XtraEditors.TextEdit)uk_rpt_user_pass).EditValue.ToString();

                    // şimdi [ e-mail ile şifre ] databaseden kontrol ediliyor
                    tSql = Sqls.preparingUstadUsersSql(user_email, user_old_pass, 0); 
                    t.SQL_Read_Execute(v.dBaseNo.UstadCrm, ds_Query, ref tSql, "UstadUser", "UserLogin");

                    if (t.IsNotNull(ds_Query))
                    {
                        int userId = t.myInt32(ds_Query.Tables[0].Rows[0]["UserId"].ToString());
                        bool IsActive = Convert.ToBoolean(ds_Query.Tables[0].Rows[0]["IsActive"].ToString());
                        string userFullName = ds_Query.Tables[0].Rows[0]["UserFullName"].ToString();
                        string db_user_key = ds_Query.Tables[0].Rows[0]["Key"].ToString();

                        if (IsActive == false)
                        {
                            MessageBox.Show("DİKKAT : Bu hesap AKTİF değildir.");
                            return;
                        }

                        if (IsActive)
                        {
                            if (db_user_key == user_old_pass)
                            {
                                if ((user_new_pass == user_rpt_pass) && // yeni şifre = tekrarlanan şifre
                                    (user_new_pass != "") &&            // yeni şifre boş değilse
                                    (user_new_pass != user_old_pass)    // yeni şifre ile eski şifre  aynı değilse
                                    )
                                {
                                    tSql = Sqls.preparingUstadUsersSql("", user_new_pass, userId);
                                    bool onay = t.Data_Read_Execute(this, ds_Query2, ref tSql, "NEW_USER_KEY", null);

                                    if (onay) MessageBox.Show("Şifreniz başarıyla güncellenmiştir...");
                                }

                                if (user_new_pass == "")
                                    MessageBox.Show("Lütfen bir şifre giriniz. Boş geçemezsiniz!");

                                if (user_new_pass != user_rpt_pass)
                                    MessageBox.Show("DİKKAT : Yeni şifre ile tekrar yazdığınız şifre aynı değil...");

                            }
                        }
                    }

                    if (t.IsNotNull(ds_Query) == false)
                    {
                        //böyle bir email varmı diye kontrol et
                        checkedUser(user_email, "FIND");
                    }
                }
            }
        }

        


    }
}
