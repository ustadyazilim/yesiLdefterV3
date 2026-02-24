using System;
using System.Windows.Forms;

using Tkn_ToolBox;
using Tkn_Variable;
using Tkn_ExeUpdate;
using System.Drawing;

namespace Tkn_Events
{
    public class tEventsForm : tBase
    {
        tEvents ev = new tEvents();
        //tEventsButton evb = new tEventsButton();

        tToolBox t = new tToolBox();

        #region Form Events Functions
        public void myFormEventsAdd(Form tForm)
        {

            tForm.Load += new System.EventHandler(myForm_Load);
            tForm.Shown += new System.EventHandler(myForm_Shown);
            tForm.Enter += new System.EventHandler(myForm_Enter);
            tForm.Leave += new System.EventHandler(myForm_Leave);

            tForm.Activated += new System.EventHandler(myForm_Activated);
            tForm.Deactivate += new System.EventHandler(myForm_Deactivate);
            tForm.Validating += new System.ComponentModel.CancelEventHandler(myForm_Validating);
            tForm.Validated += new System.EventHandler(myForm_Validated);

            tForm.FormClosing += new System.Windows.Forms.FormClosingEventHandler(myForm_Closing);
            tForm.FormClosed += new System.Windows.Forms.FormClosedEventHandler(myForm_Closed);

            tForm.KeyDown += new System.Windows.Forms.KeyEventHandler(myForm_KeyDown);
            tForm.KeyPreview = true;


        }
        
        public bool setNewDateValue(Form tForm, string param)
        {
            tToolBox t = new tToolBox();

            bool onay = false;

            string compName = null;
            if (param.IndexOf("-") > -1) compName = "Column_ISLEM_TARIHI_BAS";
            //if (param.IndexOf("+") > -1) compName = "Column_ISLEM_TARIHI_BIT";
            if (param.IndexOf("+") > -1) compName = "Column_ISLEM_TARIHI_BAS";
            Control cntrl = null;
            string[] controls = new string[] { };

            cntrl = t.Find_Control(tForm, compName, "", controls);

            if (cntrl != null)
            {
                if (param == "-G")
                    ((DevExpress.XtraEditors.DateEdit)cntrl).DateTime =
                      ((DevExpress.XtraEditors.DateEdit)cntrl).DateTime.AddDays(-1);
                if (param == "+G")
                    ((DevExpress.XtraEditors.DateEdit)cntrl).DateTime =
                      ((DevExpress.XtraEditors.DateEdit)cntrl).DateTime.AddDays(1);
                if (param == "-H")
                    ((DevExpress.XtraEditors.DateEdit)cntrl).DateTime =
                      ((DevExpress.XtraEditors.DateEdit)cntrl).DateTime.AddDays(-7);
                if (param == "+H")
                    ((DevExpress.XtraEditors.DateEdit)cntrl).DateTime =
                      ((DevExpress.XtraEditors.DateEdit)cntrl).DateTime.AddDays(7);
                if (param == "-A")
                    ((DevExpress.XtraEditors.DateEdit)cntrl).DateTime =
                      ((DevExpress.XtraEditors.DateEdit)cntrl).DateTime.AddMonths(-1);
                if (param == "+A")
                    ((DevExpress.XtraEditors.DateEdit)cntrl).DateTime =
                      ((DevExpress.XtraEditors.DateEdit)cntrl).DateTime.AddMonths(1);
                if (param == "-Y")
                    ((DevExpress.XtraEditors.DateEdit)cntrl).DateTime =
                      ((DevExpress.XtraEditors.DateEdit)cntrl).DateTime.AddYears(-1);
                if (param == "+Y")
                    ((DevExpress.XtraEditors.DateEdit)cntrl).DateTime =
                      ((DevExpress.XtraEditors.DateEdit)cntrl).DateTime.AddYears(1);

                onay = true;
            }

            return onay;
        }

        public bool myMenuShortKeyClick(Form tForm, string keyCode)
        {
            tEventsMenu evm = new tEventsMenu();


            //Application.OpenForms[0].Text = v.sp_Sakla + "," + tForm.Name;
            v.SQLSave = v.SQLSave + tForm.Name + v.ENTER;
            v.sp_Sakla = tForm.Name;

            bool keyFind = false;

            #region preparing keyCode
            //ctrl+Oem1 Şş
            //ctrl+Oem5 Çç
            //ctrl+Oem6 Üü
            //ctrl+Oem7 İi
            //ctrl+OemQuestion Öö
            //ctrl+OemOpenBrackets Ğğ

            if (keyCode.IndexOf("Oem") > -1)
            {
                keyCode = keyCode.Replace("Oem1", "Ş");
                keyCode = keyCode.Replace("Oem5", "Ç");
                keyCode = keyCode.Replace("Oem6", "Ü");
                keyCode = keyCode.Replace("Oem7", "İ");
                keyCode = keyCode.Replace("OemQuestion", "Ö");
                keyCode = keyCode.Replace("OemOpenBrackets", "Ğ");
            }

            if (keyCode.IndexOf("ctrl+D") > -1)
            {
                // ctrl+D1  >> ctrl+1 
                if (keyCode.Length == 7)
                {
                    keyCode = keyCode.Replace("ctrl+D", "C+");

                    /// DİKKAT : ctrl ve 1..9,0  TUŞLARI TAMAMEN ANA FORMA AYRILMIŞTIR ...
                    /// 
                    tForm = Application.OpenForms[0];
                }
            }

            if (keyCode.IndexOf("ctrl+F") > -1)
            {
                /// DİKKAT : ctrl + f1 TUŞLARI TAMAMEN ANA FORMA AYRILMIŞTIR ...
                /// 
                if (keyCode.Length >= 7)
                    tForm = Application.OpenForms[0];

                // ctrl+F2  >> F2
                if (keyCode.Length == 7)
                    keyCode = keyCode.Substring(5, 2);
                if (keyCode.Length == 8)
                    keyCode = keyCode.Substring(5, 3);
            }

            if (keyCode.IndexOf("alt+D") > -1)
            {
                if (keyCode.Length == 6)
                    keyCode = keyCode.Replace("alt+D", "A+");
            }

            #endregion

            // öncelikli olan tileControl menü var mı ?
            Control cntrl = t.findControlMenu(tForm, "DevExpress.XtraEditors.TileControl", "");

            // diğer menü tipleri
            if (cntrl == null)
                cntrl = t.findControlMenu(tForm);

            if (cntrl != null)
            {
                // öncelikli olan tile ana menüler 
                if (cntrl.GetType().ToString() == "DevExpress.XtraEditors.TileControl")
                {
                    DevExpress.XtraEditors.TileControl
                        mControl = ((DevExpress.XtraEditors.TileControl)cntrl);

                    keyFind = evm.findKeyCode(mControl, keyCode);

                    // aranan kısayol tile ana menüde çıkmadıysa form üzerindeki diğer menüler kontrol edilsin
                    if (keyFind == false)
                        cntrl = t.findControlMenu(tForm);
                }

                // normal formlarddaki tileNavpane menü
                if (cntrl.GetType().ToString() == "DevExpress.XtraBars.Navigation.TileNavPane")
                {
                    DevExpress.XtraBars.Navigation.TileNavPane
                        mControl = ((DevExpress.XtraBars.Navigation.TileNavPane)cntrl);

                    keyFind = evm.findKeyCode(mControl, keyCode);
                }

                // mainForm üzerindeki toolBoxControl
                if (cntrl.GetType().ToString() == "DevExpress.XtraToolbox.ToolboxControl")
                {
                    DevExpress.XtraToolbox.ToolboxControl
                        mControl = ((DevExpress.XtraToolbox.ToolboxControl)cntrl);

                    keyFind = evm.findKeyCode(mControl, keyCode);
                }
            }

            if (keyFind == false)
            {
                // AdvGridGroupButtons içindeki buttons
                keyFind = evm.findKeyAdvGridGroupButtons(tForm, keyCode);
            }

            return keyFind;
        }

        public void myForm_KeyDown(object sender, KeyEventArgs e) //*** FORM
        {
            //return;

            //if (e.Handled) return;

            // sadece main form açıkken
            if (Application.OpenForms.Count == 1)
            {
                e.Handled = myMenuShortKeyClick((Form)sender, e.KeyCode.ToString());

                if (e.Handled == false)
                    if ((e.Control) && (e.KeyCode != Keys.ControlKey))
                    {
                        e.Handled = myMenuShortKeyClick((Form)sender, "C+" + e.KeyCode);
                    }

                if (e.Handled == false)

                    if (e.KeyCode == Keys.Escape)
                    {
                        // Program kapatılacak, kapatmak istediğinize emin misiniz
                        DialogResult cevap = t.mySoru("EXIT");
                        if (DialogResult.Yes == cevap)
                        {
                            Application.Exit();
                        }
                    }
            }
            else
            {
                //string controlType = "";
                //string TableIPCode = myFormGetActiveControlTableIPCode(((Form)sender), ref controlType);

                // burada grid ve DataLayout dışında key eventleri oluştuğunda ihtiyacın olacak
                // controlType bak ona göre düzenle
            }

            #region temizlenecek
            /* eski kodlar
            else // diğer formlardan biri veya bir kaçı açıkken
            {
                if (e.Handled == false)
                    if ((e.Control) && (e.KeyCode != Keys.ControlKey))
                    {
                        e.Handled = myMenuShortKeyClick(((Form)sender), "C+" + e.KeyCode);
                    }

                if (e.Handled == false)
                    if (e.Alt && e.KeyData != (Keys.RButton | Keys.ShiftKey | Keys.Alt))
                    {
                        // ...
                        e.Handled = myMenuShortKeyClick((Form)sender, "A+" + e.KeyCode);
                    }

                if (e.Handled == false)
                    if ((e.Control) && (e.KeyCode == Keys.F12))
                    {
                        myFormGetActiveControl((Form)sender);
                    }

                /+* yeni çözümü  commonGridClick te. Yalnız grid için var
                if (e.Handled == false)
                    if (e.KeyCode == Keys.Escape)
                    {
                        bool onay = true;
                        string controlType1 = "";
                        string controlType2 = "";
                        string controlType3 = "";

                        v.formLastActiveControl = ((Form)sender).ActiveControl;

                        if (v.formLastActiveControl != null)
                        {
                            controlType1 = v.formLastActiveControl.GetType().ToString();

                            if (v.formLastActiveControl.Parent != null)
                                controlType2 = v.formLastActiveControl.Parent.GetType().ToString();

                            if (v.formLastActiveControl.Parent.Parent != null)
                                controlType3 = v.formLastActiveControl.Parent.Parent.GetType().ToString();
                        }
                        // eğer activeControl grid ise myGridView_KeyDown unun çalışması için buradaki işlem iptal ediliyor
                        if ((controlType1 == "DevExpress.XtraGrid.GridControl") |
                            (controlType2 == "DevExpress.XtraGrid.GridControl") |
                            (controlType3 == "DevExpress.XtraGrid.GridControl")) onay = false;

                        if (onay)
                        {
                            if (ev.tGetDataChanges((Form)sender))
                            {
                                //t.WaitFormOpen(v.mainForm, "Update gerekiyor...");

                                v.Kullaniciya_Mesaj_Var = "Update ok...";

                                if (v.formLastActiveControl != null)
                                    if (v.formLastActiveControl.GetType().ToString().IndexOf("Control") == -1)
                                        ((Form)sender).ActiveControl = v.formLastActiveControl;

                                e.Handled = true;
                                t.WaitFormClose(1000);

                                DialogResult cevap = t.mySoru("Formdan çıkacak mısınız ?");
                                if (DialogResult.Yes == cevap)
                                {
                                    ((Form)sender).Close();
                                }
                                //else e.Handled = true;
                            }
                            else
                            {
                                e.Handled = true;

                                if (Application.OpenForms.Count > 1)
                                    ((Form)sender).Close();
                            }
                        }

                    }
                *+/

                if (e.Handled == false)

                    if ((e.Control == false) && (e.KeyCode == v.Key_Kaydet))
                    {
                        /*  ctrl K için çalıştır
                         *  
                            //if (formActiveControl(((Form)sender)))
                            //{
                            // DataLayout üzerinde kayıt başlarsa
                            v.formLastActiveControl = ((Form)sender).ActiveControl;
                            ev.AutoSave(((Form)sender));
                            if (v.formLastActiveControl != null)
                                ((Form)sender).ActiveControl = v.formLastActiveControl;
                            //}
                            e.Handled = true;
                            *+/
                    }

                if (e.Handled == false)
                    if ((e.Control == false) && (e.KeyCode == v.Key_Yeni))
                    {
                        /*
                        v.formLastActiveControl = ((Form)sender).ActiveControl;
                        ev.AutoNew(((Form)sender));
                        e.Handled = true;
                        *+/
                    }

                if (e.Handled == false)
                    if ((e.Control) && (e.KeyCode == v.Key_Yeni))
                    {
                        //item_490_FNEW_IP

                        ev.AutoFormFirstControlFocus((Form)sender);
                        v.formLastActiveControl = ((Form)sender).ActiveControl;
                        ev.AutoNew(((Form)sender));
                        e.Handled = true;
                    }

                if (e.Handled == false)
                    if ((e.KeyCode == v.Key_BelgeyiAc) |
                        (e.KeyCode == v.Key_Listele) |
                        (e.KeyCode == v.Key_ListeHazirla))
                    {
                        string TableIPCode = myFormGetActiveControlTableIPCode(((Form)sender));
                        MessageBox.Show("yeni btnClick işi var.. myForm_KeyDown / e.Handled == false ");
                        //e.Handled = ev.navigatorButtonExec_Keys(((Form)sender), e.KeyCode, TableIPCode, "");// Prop_Navigator);
                    }

                if (e.Handled == false)
                    if ((e.Control) && (e.Alt == false))
                    {
                        string TableIPCode = myFormGetActiveControlTableIPCode(((Form)sender));

                        if (e.KeyCode == Keys.G)
                            e.Handled = setNewDateValue(((Form)sender), "-G");

                        if (e.KeyCode == Keys.H)
                            e.Handled = setNewDateValue(((Form)sender), "-H");

                        if (e.KeyCode == Keys.Y)
                            e.Handled = setNewDateValue(((Form)sender), "-Y");

                        if (e.KeyCode == Keys.A)
                            e.Handled = setNewDateValue(((Form)sender), "-A");
                    }

                if (e.Handled == false)
                    if ((e.Alt) && (e.Control))
                    {
                        if (e.KeyCode == Keys.G)
                            e.Handled = setNewDateValue(((Form)sender), "+G");

                        if (e.KeyCode == Keys.H)
                            e.Handled = setNewDateValue(((Form)sender), "+H");

                        if (e.KeyCode == Keys.A)
                            e.Handled = setNewDateValue(((Form)sender), "+A");

                        if (e.KeyCode == Keys.Y)
                            e.Handled = setNewDateValue(((Form)sender), "+Y");
                    }

                /*
                if (e.Handled == false)
                    if ((e.KeyCode != v.Key_SearchEngine) &
                    (e.KeyCode != v.Key_Kaydet) &
                    (e.KeyCode != v.Key_Yeni) &
                    (e.KeyCode != v.Key_KaydetYeni) &
                    (e.KeyCode != v.Key_NewSub) &
                    (e.KeyCode != v.Key_Listele) &
                    (e.KeyCode != v.Key_ListeHazirla) &
                    (e.KeyCode != v.Key_ListyeEkle) &
                    (e.KeyCode != v.Key_SecCik) &
                    (e.KeyCode != v.Key_BelgeyiAc) &
                    (e.KeyCode != v.Key_Prior) &
                    (e.KeyCode != Keys.Escape) &
                    (e.KeyCode != Keys.Delete) &
                    (e.KeyCode != Keys.Return) & // Returnle belge Aç ,Kart Aç yapınca focus kayboluyor ????
                    ((e.KeyCode < Keys.NumPad0) & (e.KeyCode > Keys.NumPad9)) &
                    ((e.KeyCode < Keys.A) & (e.KeyCode > Keys.Z))
                    )
                    {
                        string TableIPCode = myFormGetActiveControlTableIPCode(((Form)sender));
                        MessageBox.Show("yeni btnClick işi var.. myForm_KeyDown / e.Handled == false 2 ");
                        //e.Handled = ev.navigatorButtonExec_Keys(((Form)sender), e.KeyCode, TableIPCode, "");
                    }
                    *+/
            }
            */
            #endregion
        }

        public void myForm_Activated(object sender, EventArgs e)
        {
            //v.Kullaniciya_Mesaj_Var = "Form Activated";

            //this.Text = this.Text + ",a";

        }

        public void myForm_Deactivate(object sender, EventArgs e)
        {
            //MessageBox.Show("myForm_Deactivate : " + ((Form)sender).Text);

            //if (Application.OpenForms.Count > 0)
            //    Application.OpenForms[0].Text = Application.OpenForms[0].Text + ";Deact";

            //v.Kullaniciya_Mesaj_Var = "Form Deactivate";
        }

        public void myForm_Load(object sender, EventArgs e)
        {
            //MessageBox.Show("myForm_Load");
            //Application.OpenForms[0].Text = Application.OpenForms[0].Text + ";Load";
        }

        public void myForm_Shown(object sender, EventArgs e)
        {
            //MessageBox.Show("myForm_Shown");
            //Application.OpenForms[0].Text = Application.OpenForms[0].Text + ";Show";
            if (v.SP_ApplicationExit)
            {
                Application.Exit();
                return;
            }

            if (v.SP_CheckedNewApplication == false)
            {
                tExeUpdate exe = new tExeUpdate();
                string formName = ((Form)sender).Name;
                if (formName == "main")
                {
                    /// Exe sadece açılışta bir defa kontrol edilsin
                    ///
                    v.SP_CheckedNewApplication = true;

                    /// Exe Güncellendiyse
                    bool onay = exe.versionChecked((Form)sender);
                    if (onay) 
                        Application.Exit();
                }
            }
                        
            if ((v.SP_UserLOGIN == false) &&
                (v.SP_UserIN))
                Application.Exit();

            if (v.SP_ApplicationExit) Application.Exit();

            v.formLastActiveControl = null;

            if (v.con_FormAfterCreateView)
            {
                ev.tExtraCreateView((Form)sender);

                v.con_FormAfterCreateView = false;
            }

            Screen screen = Screen.FromControl((Form)sender);
            //string ekran = "";
            //ekran  = $"Bulunduğu Ekran: {screen.DeviceName}" + v.ENTER;
            //ekran += $"Çözünürlük: {screen.Bounds.Width}x{screen.Bounds.Height}" + v.ENTER;
            //ekran += $"Konum: X={screen.Bounds.X}, Y={screen.Bounds.Y}" + v.ENTER;
            //MessageBox.Show(ekran);

            if ((screen.Bounds.Width < 1920) ||
                (screen.Bounds.Height < 1080))
            {
                if (((Form)sender).Tag != null && ((Form)sender).Tag != "DIALOG")
                {
                    ((Form)sender).AutoScroll = true;
                    ((Form)sender).AutoScrollMinSize = new Size(1820, 990); // Minimum scroll boyutu
                }
            }


            //if (v.con_AutoNewRecords)
            //{
            //    //AutoNewRecords((Form)sender);
            //    v.con_PositionChange = true;

            //    tDataNavigatorList((Form)sender, "tNewData");

            //    v.con_PositionChange = false;
            //    v.con_AutoNewRecords = false;
            //}

            #region Detail-SubDetail varmı?
            if (((Form)sender).IsAccessible == true)
            {
                // IsAccessible == true  ise formun üzerinde 
                // kendisine (detail'e) bağlı alt tablolar(subdetail) vardır 

                // Detail Table
                //    |____ SubDetail Table

                // Detail Table da bir row değişikliği olduğunda buna bağlı olan  
                // SubDetail Tablolarında yeniden okunması gerekiyor

                //---iptal---Detail_SubDetail_Refresh((Form)sender);

                //tDataNavigatorList((Form)sender, "Detail_SubDetail_Refresh");
            }
            #endregion Detail-SubDetail varmı?

            if (v.con_GotoRecord == "ONdialog")
            {
                // DialogForm da bu işlem çalışmadığı için
                // FormShown sırasında bu işlemleri tekrar yapmak gerekiyor

                // search Engine içinden tetikleniyor

                t.tGotoRecord((Form)sender, null,
                    v.con_TableIPCode,
                    v.con_GotoRecord_FName,
                    v.con_GotoRecord_Value,
                    v.con_GotoRecord_Position);
            }
            
            /// Yeni Süreç ----
            /// 
            v.SP_NewWorkType = "NEW";
            
            vSubWork vSW = new vSubWork();
            vSW._01_tForm = (Form)sender;
            vSW._02_TableIPCode = "";
            vSW._03_WorkTD = v.tWorkTD.NewAndRef;
            vSW._04_WorkWhom = v.tWorkWhom.All;
            ev.tSubWork_(vSW);

            if (t.IsNotNull(v.tSearch.searchInputValue))
            {
                Control cntrl = t.Find_Control(((Form)sender), "textEdit_Find_" + t.AntiStr_Dot(v.tSearch.SearchTableIPCode));

                if (cntrl != null)
                {
                    ((DevExpress.XtraEditors.TextEdit)cntrl).EditValue = v.tSearch.searchInputValue;
                    ((DevExpress.XtraEditors.TextEdit)cntrl).SelectionStart = v.tSearch.searchInputValue.Length + 1;
                    //((DevExpress.XtraEditors.TextEdit)cntrl).Focus();
                }
            }


            //v.con_AutoNewRecords = false;
            /// end yeni süreç
        }

        public void myForm_Refresh(Form tForm, string Kim)
        {
            //if (tForm.ActiveControl != null)
            //    v.Kullaniciya_Mesaj_Var = tForm.ActiveControl.ToString();

            //MessageBox.Show("myForm_Refresh");
            //Application.OpenForms[0].Text = Application.OpenForms[0].Text + ";Refr=" + Kim;

            /// Yeni Süreç ----
            /// 
            vSubWork vSW = new vSubWork();
            vSW._01_tForm = tForm;
            vSW._02_TableIPCode = "";
            vSW._03_WorkTD = v.tWorkTD.Refresh_Data;
            vSW._04_WorkWhom = v.tWorkWhom.All;
            ev.tSubWork_(vSW);


            if (v.con_GotoRecord == "ONdialog")
            {
                // DialogForm da bu işlem çalışmadığı için
                // FormLoad sırasında bu işlemleri tekrar yapmak gerekiyor

                // search Engine içinden tetikleniyor

                t.tGotoRecord(tForm, null,
                    v.con_TableIPCode,
                    v.con_GotoRecord_FName,
                    v.con_GotoRecord_Value,
                    v.con_GotoRecord_Position);

                //tForm.HelpButton = false;
            }


            /// Açma : ? : Fiş ve sepet formları arasında ikiside açıkken 
            /// kullanıcı manuel olarak git gel yapabiliyor
            ///
            //tForm.HelpButton = false;
        }

        public void myForm_Enter(object sender, EventArgs e)
        {
            v.formLastActiveControl = null;
            //MessageBox.Show("myForm_Enter : " + ((Form)sender).Text);
            //Application.OpenForms[0].Text = Application.OpenForms[0].Text + ";Ent";

            // Kayıt işlemi gerçekleştiyse
            //if (((Form)sender).HelpButton == true)
            //{
            //    myForm_Refresh(((Form)sender), "myForm_Enter");
            //}

            //if (((Form)sender).ActiveControl != null)
            //    v.Kullaniciya_Mesaj_Var = ((Form)sender).ActiveControl.ToString();

            //v.Kullaniciya_Mesaj_Var = "Form Enter";
        }

        public void myForm_Leave(object sender, EventArgs e)
        {
            //MessageBox.Show("myForm_Leave : " + ((Form)sender).Text);
            //Application.OpenForms[0].Text = Application.OpenForms[0].Text + ";Lea";
            
            //v.Kullaniciya_Mesaj_Var = "Form Leave";

        }

        public void myForm_Validating(object sender, EventArgs e)
        {
            //MessageBox.Show("myForm_Validating");
            //Application.OpenForms[0].Text = Application.OpenForms[0].Text + ";Valing";
        }

        public void myForm_Validated(object sender, EventArgs e)
        {
            //MessageBox.Show("myForm_Validated");
        }

        public void myForm_Closing(object sender, FormClosingEventArgs e)
        {
            //if (v.cefBrowser_ != null)
            //    v.cefBrowser_.Parent = null;
            // 1. numara bu çalışıyor
            //MessageBox.Show("myForm_FormClosing : " + ((Form)sender).Text);
        }

        public void myForm_Closed(object sender, FormClosedEventArgs e)
        {
            // 2. numara bu çalışıyor
            //MessageBox.Show("myForm_FormClosed : " + ((Form)sender).Text);
        }


        public string myFormGetActiveControlTableIPCode(Form tForm, ref string controlType)
        {
            string TableIPCode = null;

            if (tForm.ActiveControl != null)
            {
                Control ctrl = tForm.ActiveControl;

                if (ctrl != null) controlType = ctrl.GetType().ToString();

                if (ctrl.AccessibleName != null)
                {
                    TableIPCode = ctrl.AccessibleName.ToString();
                }

                if (ctrl.GetType().ToString() == "DevExpress.XtraBars.Ribbon.BackstageViewClientControl")
                {
                    if (((DevExpress.XtraBars.Ribbon.BackstageViewClientControl)ctrl).ActiveControl != null)
                    {
                        Control item = ((DevExpress.XtraBars.Ribbon.BackstageViewClientControl)ctrl).ActiveControl;
                        if (item.GetType().ToString() == "DevExpress.XtraDataLayout.DataLayoutControl")
                        {
                            if (((DevExpress.XtraDataLayout.DataLayoutControl)item).AccessibleName != null)
                                TableIPCode = ((DevExpress.XtraDataLayout.DataLayoutControl)item).AccessibleName;
                        }
                    }
                }

                if (ctrl.GetType().ToString() == "DevExpress.XtraBars.Ribbon.BackstageViewControl")
                {
                    int i = ((DevExpress.XtraBars.Ribbon.BackstageViewControl)ctrl).SelectedTabIndex + 1;

                    if (((DevExpress.XtraBars.Ribbon.BackstageViewControl)ctrl).Controls[i].AccessibleName != null)
                    {
                        TableIPCode = ((DevExpress.XtraBars.Ribbon.BackstageViewControl)ctrl).Controls[i].AccessibleName.ToString();
                    }
                }
            }
            // eğer herhangi bir TableIPCode bulamadıysa
            if (TableIPCode == null)
            {
                using (tToolBox t = new tToolBox())
                {
                    TableIPCode = t.Find_FirstTableIPCode(tForm);
                }
            }

            return TableIPCode;
        }

        public bool myFormGetActiveControl(Form tForm)
        {
            bool onay = true;

            // formun activeControlu GridControl ise onay verme
            //if (tForm.ActiveControl.GetType().ToString() == "DevExpress.XtraGrid.GridControl")
            if (tForm.ActiveControl.ToString() == "DevExpress.XtraGrid.GridControl")
            {
                onay = false;
            }
            // focus nerede sorusu için al sana cevabı
            //
            v.Kullaniciya_Mesaj_Var = "Focus : " + tForm.ActiveControl.ToString();
            //
            return onay;
        }

        #endregion Form Events Functions




    }

}
