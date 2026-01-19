using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraReports.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tkn_CreateObject;
using Tkn_DataCopy;
using Tkn_DefaultValue;
using Tkn_Forms;
using Tkn_InputPanel;
using Tkn_Report;
using Tkn_Save;
using Tkn_Search;
using Tkn_ToolBox;
using Tkn_Variable;

namespace Tkn_Events
{
    public class tEventsButton : tBase
    {
        tEvents ev = new tEvents();
        tToolBox t = new tToolBox();

        public bool btnClick(vButtonHint buttonHint)
        {
            /// buraya gelen veriler
            /// propNavigator : string olabilir
            /// prop_         : single PROP_NAVIGATOR olabilir
            /// propList_     : List < PROP_NAVIGATOR > olabilir
            /// bunlar kontrol edilerek ona göre alt procedureler çalıştırılacak
            /// 

            bool onay = false;
            bool transactionRun = false;
            bool birOncekiOnay = false;
            bool isFormOpen = true;
            bool elseOncesiCalisti = false;
            bool elseItem = false;
            Form tForm = buttonHint.tForm;

            string tableIPCode = buttonHint.tableIPCode;
            string propNavigator = buttonHint.propNavigator;

            // kullanıcının seçtiği esas button
            v.tButtonType mainButtonType = buttonHint.buttonType;
                       

            //if ((mainButtonType == v.tButtonType.btYeniHesapSatir) ||
            //    (mainButtonType == v.tButtonType.btYeniBelgeSatir) ||
            //    (mainButtonType == v.tButtonType.btYeniAltHesapSatir)) mainButtonType = v.tButtonType.btYeniKartSatir;

            if (mainButtonType == v.tButtonType.btNoneButton)
                birOncekiOnay = true;

            // değişikliği yansıtalım
            buttonHint.buttonType = mainButtonType;

            /// propNavigator ile gelen bilgiler
            ///
            v.tButtonType propButtonType = v.tButtonType.btNone;
            PROP_NAVIGATOR prop_ = null;
            List<PROP_NAVIGATOR> propList_ = null;

            t.WaitFormOpen(tForm, ""); // v.IsWaitOpen / start

            int i = propNavigator.IndexOf("KEY_FNAME:");

            if (i > -1)
            {
                buttonHint.keyFieldName_ = t.MyProperties_Get(propNavigator, "KEY_FNAME:");
                buttonHint.masterKeyFieldName_ = t.MyProperties_Get(propNavigator, "MASTER_KEY_FNAME:");
                buttonHint.foreingFieldName_ = t.MyProperties_Get(propNavigator, "FOREING_FNAME:");
                buttonHint.parentFieldName_ = t.MyProperties_Get(propNavigator, "PARENT_FNAME:");

                /// burada iki şekilde bu bilgi bloğu olabiliyor
                /// birinci =KEY_FNAME: şeklinde oluyor ve propNavigatorun sonunda yama şeklinde geliyor
                /// ikinciside KEY_FNAME: şeklinde sadece KEY_???? blokları olabiliyor
                /// 

                int i2 = propNavigator.IndexOf("=KEY_FNAME:");
                if (i2 > -1)
                    propNavigator = propNavigator.Substring(0, i2);

                // yinede varsa kontrol 
                i2 = propNavigator.IndexOf("KEY_FNAME:");
                if (i2 > -1)
                    propNavigator = "";
            }

            if (buttonHint.prop_ != null)
                prop_ = buttonHint.prop_;

            // propNavigator ü temizle ve json a çevir 
            if (propNavigator != "")
            {
                t.readProNavigator(propNavigator, ref prop_, ref propList_);
            }

            /// tek blokluk propNavigator ise
            if (prop_ != null)
            {
                /// propNavigator buraya string bilgi gelmiş olabilir 
                /// burada çevrim yapılmışsa tır 
                ///
                if (buttonHint.prop_ == null)
                    buttonHint.prop_ = prop_;

                isFormOpen = t.tWorkingCheck(tForm, prop_, tableIPCode);

                if (isFormOpen) // form açılması için onaylandı ise
                {
                    // propNavigator üzerindeki tanım

                    propButtonType = t.getClickType(Convert.ToInt32(prop_.BUTTONTYPE.ToString()));
                    if (mainButtonType == propButtonType)
                    {
                        onay = true;
                        
                        onay = singleButtonEvent(buttonHint);

                        if (onay == false)
                        {
                            v.IsWaitOpen = false;
                            t.WaitFormClose();
                            return onay;
                        }
                    }
                }
            }

            /// birden fazla propNavigator bilgisi var ise 
            if (propList_ != null)
            {
                /// buraya string bilgi gelmiş olabilir 
                /// burada çevrim yapılmıştır 
                ///
                if (buttonHint.propList_ == null)
                    buttonHint.propList_ = propList_;
                                
                foreach (PROP_NAVIGATOR item in propList_)
                {
                    isFormOpen = t.tWorkingCheck(tForm, item, tableIPCode);

                    // else satırı mı kontrol et
                    elseItem = (item.CHC_VALUE.ToString().IndexOf("ELSE") > -1);

                    // bu satır daha önce çalıştı mı ?
                    transactionRun = item.TransactionRun;

                    if ((elseItem) &&          // else satırına geldik
                        (elseOncesiCalisti) && // elseden öncede çalıştı
                        (isFormOpen))          // else satırının çalışması için onay da aldı
                        isFormOpen = false;    // fakat else den önce çalıştığı için else satırının onayı iptal, çalışmasın

                    // test sırasında işlem yakalamak için kullanılıyor
                    /*
                    if ((elseItem) &&          // else satırına geldik
                        (elseOncesiCalisti == false) && // elseden öncede çalıştı
                        (isFormOpen))          // else satırının çalışması için onayda aldı
                        isFormOpen = true;    // fakat else den önce çalıştığı için else satırının onayı iptal, çalışmasın
                    */

                    // form açılması için onaylandı ise
                    // bu işlem daha önce çalışmadıysa çalışsın
                    if ((isFormOpen) && (transactionRun == false))
                    {
                        elseOncesiCalisti = true;
                        // propNavigator üzerindeki tanım
                        if (item.BUTTONTYPE.ToString() != "null")
                        {
                            propButtonType = t.getClickType(Convert.ToInt32(item.BUTTONTYPE.ToString()));
                            buttonHint.buttonType = propButtonType;
                        }
                        else
                        {
                            buttonHint.buttonType = mainButtonType;
                        }

                        if (mainButtonType == propButtonType)
                        {
                            onay = true;
                            // propList i tek tek işlemek gerekiyor
                            buttonHint.prop_ = item;
                            buttonHint.buttonType = mainButtonType;

                            onay = singleButtonEvent(buttonHint);

                            transactionRun = onay;

                            buttonHint.tForm = tForm;

                            /// bunu sakla, propList_ dönmeye devam ediyor,
                            /// kendinden sonraki işler için kontrol olacak
                            /// 
                            birOncekiOnay = onay;

                            if (onay == false)
                            {
                                v.IsWaitOpen = false;
                                t.WaitFormClose();
                                return onay;
                            }
                        }

                        /// bu kısım Search sonrası işleri yaptırırken gündeme geldi
                        /// 
                        if ((mainButtonType != propButtonType) &&
                            (birOncekiOnay))
                        {
                            if ((propButtonType == v.tButtonType.btFormulleriHesapla) ||
                                (propButtonType == v.tButtonType.btDataTransferi) ||
                                (propButtonType == v.tButtonType.btInputBox) ||
                                (propButtonType == v.tButtonType.btOpenSubView) ||
                                (propButtonType == v.tButtonType.btExtraIslem)
                                )
                            {
                                onay = true;
                                // propList i tek tek işlemek gerekiyor
                                buttonHint.prop_ = item;
                                onay = singleButtonEvent(buttonHint);
                            }
                        }

                        if ((mainButtonType != propButtonType) &&
                            (birOncekiOnay == false))
                        {
                            buttonHint.buttonType = mainButtonType;
                        }
                    }
                }
            }

            // propNavigator göre çalışmadıysa yine denemek gerekiyor
            if (onay == false)
                onay = singleButtonEvent(buttonHint);

            v.IsWaitOpen = false;
            t.WaitFormClose();

            if (v.con_SetFocus)
            {
                if (v.con_SetFocus_FieldName.ToUpper() != "ARAMA")
                    t.tFormActiveControl(tForm, v.con_SetFocus_TableIPCode, "Column_", v.con_SetFocus_FieldName);

                if (v.con_SetFocus_FieldName.ToUpper() == "ARAMA")
                    t.tFormActiveControl(tForm, v.con_SetFocus_TableIPCode, "", "");

                v.con_SetFocus = false;
                v.con_SetFocus_TableIPCode = "";
                v.con_SetFocus_FieldName = "";
            }

            return onay;

            /// form açma işlemi 
            /// Her zamana aynı formu aç' da olabilir ( chc_xxx fieldleri boş )
            /// veya xxx fieldına bak                 ( chc_xxx fieldleri doludur )
            /// değeri 5 ise  xxxformunu aç
            /// değeri 6 ise  yyyformunu aç
            /// değeri 8 ise  zzzformunu aç
            /// veya
            /// değeri 5, 6, 10 ise xxxformunu aç
            /// değeri 7, 8, 9  ise yyyformunu aç  da olabilir
        }

        public bool singleButtonEvent(vButtonHint buttonHint)
        {

            bool onay = true;
            Form tForm = buttonHint.tForm;
            object sender = buttonHint.sender;
            string tableIPCode = buttonHint.tableIPCode;
            string buttonName = buttonHint.buttonName;
            v.tButtonType buttonType = buttonHint.buttonType;

            PROP_NAVIGATOR prop_ = buttonHint.prop_;
            List<PROP_NAVIGATOR> propList_ = buttonHint.propList_;
            int propListCount_ = 0;

            if (propList_ != null)
                propListCount_ = propList_.Count;


            #region işlevsel butonlar

            if ((sender == null) &&
                (t.IsNotNull(tableIPCode)) &&
                (t.IsNotNull(buttonName)))
            {
                string[] controls = new string[] { };
                sender = t.Find_Control(tForm, buttonName, tableIPCode, controls);
            }

            if (buttonType == v.tButtonType.btCikis) // (buttonName == "cikis") 
            {
                //if (v.cefBrowser_ != null)
                //    v.cefBrowser_.Parent = null;
                tForm.Close();
                return onay;
            }

            if (buttonType == v.tButtonType.btListele)  // (ButtonName == "listele")  // 12
            {
                return listele(tForm, tableIPCode);
            }

            if (buttonType == v.tButtonType.btSecCik)  // (ButtonName == "sec")
            {
                onay = t.TableRowGet(tForm, tableIPCode);
                if (onay)
                {
                    v.searchSet = true;
                    tForm.Dispose();
                    return onay;
                }
            }

            if (buttonType == v.tButtonType.btListeyeEkle)  // (ButtonName == "ekle")  // 14
            {
                if (t.IsNotNull(buttonHint.viewType) == false)
                    onay = listeyeEkle(tForm, propList_, buttonHint.tableIPCode, buttonHint.columnFieldName);
                
                if (buttonHint.viewType == "SearchLookUpEdit")
                {
                    string columnFieldName = buttonHint.columnFieldName;
                    string selectValue = buttonHint.columnEditValue;
                    onay = listeyeEkle_SearchLookUpEdit(tForm, propList_, tableIPCode, v.tButtonType.btListeyeEkle, columnFieldName, selectValue);
                }

                return onay;
            }

            if (buttonType == v.tButtonType.btListeHazirla)  // (ButtonName == "liste_hazirla")  // 15
            {
                // refresh 
                tForm.HelpButton = true;
                //
                onay = listeHazirla(tForm, tableIPCode, propList_);
                return onay;
            }

            if (buttonType == v.tButtonType.btSihirbazDevam)  // (ButtonName == "sihirbaz_devam")
            {
                string Caption = "";

                if (sender != null)
                    Caption = ((DevExpress.XtraEditors.SimpleButton)sender).Text;

                if (Caption == "Kaydet")
                {
                    /// devam özelliğinden çıkıp, kaydet özelliği çalışsın diye 
                    buttonType = v.tButtonType.btKaydet;
                }
                else
                {
                    t.NextPage(tForm, tableIPCode);
                    return onay;
                }
            }

            if (buttonType == v.tButtonType.btSihirbazGeri)  //  (ButtonName == "sihirbaz_geri")
            {
                t.PrevPage(tForm, tableIPCode);
                return onay;
            }

            #region kaydetler

            if (buttonType == v.tButtonType.btKaydetYeni)  //  (ButtonName == "kaydet_yeni") // 24
            {
                onay = kaydet(tForm, tableIPCode, propList_, buttonType);

                if (onay)
                {
                    v.con_ColumnValueChanging = false;
                    //t.ButtonEnabledAll(tForm, TableIPCode, true);
                    newData(tForm, tableIPCode);
                    //ev.AutoActiveControlFocus(tForm, tableIPCode);
                    return onay;
                }

                return onay;
            }

            if (buttonType == v.tButtonType.btKaydet)  // (ButtonName == "kaydet")   // 22
            {
                onay = kaydet(tForm, tableIPCode, propList_, buttonType);
                return onay;
            }

            if (buttonType == v.tButtonType.btKaydetDevam)  //  (ButtonName == "kaydet_devam")   // 25
            {
                onay = kaydet(tForm, tableIPCode, propList_, buttonType);

                if (onay)
                {
                    t.NextPage(tForm, tableIPCode);
                    return onay;
                }

                return onay;

                /*
                tSave sv = new tSave();
                onay = sv.tDataSave(tForm, tableIPCode);

                if (onay)
                {
                    t.NextPage(tForm, tableIPCode);
                    t.ButtonEnabledAll(tForm, tableIPCode, true);
                    t.tFormActiveView(tForm, tableIPCode);
                    return onay;
                }
                return onay;
                */
            }

            if (buttonType == v.tButtonType.btKaydetCik)  // (ButtonName == "kaydet_cik")   // 22
            {
                /*
                tSave sv = new tSave();
                onay = sv.tDataSave(tForm, tableIPCode);
                */
                onay = kaydet(tForm, tableIPCode, propList_, buttonType);
                if (onay)
                    tForm.Dispose();
                return onay;
            }

            #endregion kaydetler

            if ((buttonType == v.tButtonType.btSilSatir) ||
                (buttonType == v.tButtonType.btSilKart) ||
                (buttonType == v.tButtonType.btSilHesap) ||
                (buttonType == v.tButtonType.btSilBelge))
            {
                onay = satirSil(tForm, tableIPCode, propList_);

                if ((buttonType == v.tButtonType.btSilSatir))
                {
                    return onay;
                }
                else
                {
                    // Kart, Hesap, Belge silden silme onayı geldiyse Açık olan formu kapat
                    if (onay)
                        tForm.Close();
                }
            }

            if (buttonType == v.tButtonType.btSilListe)
            {
                onay = listeSil(tForm, tableIPCode, propList_);
                return onay;
            }

            if (buttonType == v.tButtonType.btGoster) //btHesapAcOku)  // (ButtonName == "hesap_ac")   // 50 - hesap aç
            {
                //hesapAc(tForm, prop_);
                //return onay;

                onay = openSubView(tForm, tableIPCode, propList_, buttonType);
                return onay;
            }

            if ((buttonType == v.tButtonType.btKartAc) ||  // (ButtonName == "kart_ac")   // 51 - kart + form
                (buttonType == v.tButtonType.btHesapAc) ||
                (buttonType == v.tButtonType.btBelgeAc))
            {
                if ((buttonHint.senderType == "GridView") ||
                    (buttonHint.senderType == "AdvBandedGridView") ||
                    (buttonHint.senderType == "TreeList") ||
                    (buttonHint.senderType == "SchedulerControl") ||
                    (buttonHint.senderType == "Button") ||
                    (buttonHint.senderType == "Menu") ||
                    (buttonHint.senderType == "DevExpress.XtraEditors.ButtonEdit") ||
                    (buttonHint.senderType == "DevExpress.XtraEditors.ImageComboBoxEdit") 
                    )
                {
                    if ((prop_ != null) && (propListCount_ <= 1))
                    {
                        Form newForm = t.OpenForm_JSON(tForm, prop_);

                        if (prop_.TABLEIPCODE_LIST.Count > 0)
                        {
                            if (newForm != null)
                                onay = extraIslemVar(newForm, tableIPCode, buttonType, v.tBeforeAfter.After, propList_);
                            else onay = extraIslemVar(tForm, tableIPCode, buttonType, v.tBeforeAfter.After, propList_);
                        }
                    }
                    
                    if (propListCount_ > 1)
                        onay = openControlForm_(tForm, tableIPCode, propList_, buttonType);

                    if (onay)
                        t.TableRefresh(tForm, tableIPCode);

                    //extraIslemVar(tForm, tableIPCode, v.tButtonType.btKartAc, v.tBeforeAfter.After, propList_);
                }
                return onay;
            }

            if (buttonType == v.tButtonType.btResimEditor)
            {
                openPictureEditForm_(tForm, tableIPCode, propList_);
            }

            if (buttonType == v.tButtonType.btReportDesign)
            {
                openReportDesignForm_(tForm, tableIPCode, propList_);
            }

            if ((buttonType == v.tButtonType.btYeniKart) || 
                (buttonType == v.tButtonType.btYeniHesap) ||
                (buttonType == v.tButtonType.btYeniBelge) ||
                (buttonType == v.tButtonType.btYeniAltHesap))
            {
                if ((prop_ != null) && (propListCount_ <= 1))
                    onay = yeniFormYeniData(tForm, prop_);

                if (propListCount_ > 1)
                    onay = openControlForm_(tForm, tableIPCode, propList_, buttonType);

                return onay;
            }

            if ((buttonType == v.tButtonType.btYeniSatir) ||    
                (buttonType == v.tButtonType.btYeniKartSatir) ||
                (buttonType == v.tButtonType.btYeniHesapSatir) ||
                (buttonType == v.tButtonType.btYeniBelgeSatir) ||
                (buttonType == v.tButtonType.btYeniAltHesapSatir))   
            {
                onay = newDataButtonClick(tForm, tableIPCode, propList_, buttonType);
            }

            #region // 53 - yeni_hesap || 54 - yeni_alt_hesap  SİLİNECEK
            /*
            if (((ButtonName == "yeni_hesap") ||       // 53 - yeni hesap
                (ButtonName == "yeni_alt_hesap")) &&  // 54 - yeni alt hesap
                (Caption.IndexOf("Yeni") > -1)
                )
            {
                /// yeni butonuna extra yük atılmış 
                /// bir yeni butonu tuşu çalıştırıldığında, 
                /// kendisinden önce başka IP lerde yeni için hazırlanabilir
                if (t.IsNotNull(Prop_Navigator))
                {
                    string s1 = "BUTTONTYPE:53";
                    string s2 = (char)34 + "BUTTONTYPE" + (char)34 + ": " + (char)34 + "53" + (char)34;

                    if ((Prop_Navigator.IndexOf(s1) > -1) ||
                        (Prop_Navigator.IndexOf(s2) > -1))
                    {
                        v.con_PositionChange = true;
                        v.con_ExtraChange = true;

                        navigatorButtonExec_(tForm, TableIPCode, Prop_Navigator, v.tNavigatorButton.nv_53_Yeni_Hesap);
                    }
                }

                string Key_FName = t.MyProperties_Get(Prop_Navigator, "KEY_FNAME:");
                string Master_Key_FName = t.MyProperties_Get(Prop_Navigator, "MASTER_KEY_FNAME:");
                string Foreing_FName = t.MyProperties_Get(Prop_Navigator, "FOREING_FNAME:");
                string Parent_FName = t.MyProperties_Get(Prop_Navigator, "PARENT_FNAME:");


                // 
                //tCancelData(tForm, sender, TableIPCode);

                preparingNewData(tForm, TableIPCode);

                // yeni_alt_hesap
                if (t.IsNotNull(Parent_FName))
                {
                    if (t.IsNotNull(Master_Key_FName))
                        Key_FName = Master_Key_FName;

                    string UstHesapValue = Find_ParentValue(tForm, TableIPCode, Key_FName, Parent_FName, ButtonName);

                    tNewData(tForm, TableIPCode, Key_FName, Parent_FName, UstHesapValue);
                }
                else // yeni_hesap
                {
                    tNewData(tForm, TableIPCode);
                }

                AutoFormFirstControlFocus(tForm);

                v.con_PositionChange = false;
                v.con_ExtraChange = false;
            }
            */
            #endregion

            if (buttonType == v.tButtonType.btArama)
            {
                onay = aramaIslemi(tForm, tableIPCode, prop_, v.tButtonHint.columnEditValue);
                return onay;
            }

            //Datası yüklenmemiş grid içindeki şartlı arama işlemi
            if (buttonType == v.tButtonType.btFindListData)
            {
                onay = findListDataIslemi(tForm, tableIPCode, prop_, v.tButtonHint.columnEditValue);
                return onay;
            }

            if (buttonType == v.tButtonType.btFormulleriHesapla)
            {
                formulleriHesapla(tForm, tableIPCode, prop_);
                return onay;
            }

            if (buttonType == v.tButtonType.btDataTransferi)
            {
                onay = dataTransferi(tForm, tableIPCode, prop_);
                return onay;
            }

            if (buttonType == v.tButtonType.btInputBox)
            {
                InputBox(tForm, tableIPCode, prop_);
                return onay;
            }

            if (buttonType == v.tButtonType.btOpenSubView)
            {
                onay = openSubView(tForm, tableIPCode, propList_, buttonType);
                return onay;
            }

            if (buttonType == v.tButtonType.btExtraIslem)
            {
                onay = extraIslemCalistir(tForm, tableIPCode, prop_); // , buttonType); şimdilik gerek yok
                return onay;
            }

            #endregion işlevsel butonlar

            #region yön butonları

            if (buttonType == v.tButtonType.btEnSona)  //  (ButtonName == "en_sona")
            {
                DataNavigator tDataNavigator = t.Find_DataNavigator(tForm, tableIPCode);
                NavigatorButton btn = tDataNavigator.Buttons.Last;
                tDataNavigator.Buttons.DoClick(btn);
                return onay;
            }

            if (buttonType == v.tButtonType.btSonrakiSayfa)  //  (ButtonName == "sonraki_syf")
            {
                DataNavigator tDataNavigator = t.Find_DataNavigator(tForm, tableIPCode);
                NavigatorButton btn = tDataNavigator.Buttons.NextPage;
                tDataNavigator.Buttons.DoClick(btn);
                return onay;
            }

            if (buttonType == v.tButtonType.btSonraki)  // (ButtonName == "sonraki")
            {
                DataNavigator tDataNavigator = t.Find_DataNavigator(tForm, tableIPCode);
                NavigatorButton btn = tDataNavigator.Buttons.Next;
                tDataNavigator.Buttons.DoClick(btn);
                return onay;
            }

            if (buttonType == v.tButtonType.btOnceki)  // (ButtonName == "onceki")
            {
                DataNavigator tDataNavigator = t.Find_DataNavigator(tForm, tableIPCode);
                NavigatorButton btn = tDataNavigator.Buttons.Prev;
                tDataNavigator.Buttons.DoClick(btn);
                return onay;
            }

            if (buttonType == v.tButtonType.btOncekiSayfa)  // (ButtonName == "onceki_syf")
            {
                DataNavigator tDataNavigator = t.Find_DataNavigator(tForm, tableIPCode);
                NavigatorButton btn = tDataNavigator.Buttons.PrevPage;
                tDataNavigator.Buttons.DoClick(btn);
                return onay;
            }

            if (buttonType == v.tButtonType.btEnBasa)  //  (ButtonName == "en_basa")
            {
                DataNavigator tDataNavigator = t.Find_DataNavigator(tForm, tableIPCode);
                NavigatorButton btn = tDataNavigator.Buttons.First;
                tDataNavigator.Buttons.DoClick(btn);
                return onay;
            }

            #endregion yön butonları

            #region onay butonları
            if ((buttonType == v.tButtonType.btOnayEkle) ||  // ((ButtonName == "onayla") | (ButtonName == "onay_iptal"))
                (buttonType == v.tButtonType.btOnayKaldir))
            {
                string Caption = "";

                if (sender != null) // + veya - 
                    Caption = ((DevExpress.XtraEditors.SimpleButton)sender).Text.Trim();

                onayIslemi(tForm, tableIPCode, Caption);
                return onay;
            }
            #endregion onay butonları

            #region collapse/expand butonları
            if ((buttonType == v.tButtonType.btCollapse) ||  // (ButtonName == "CollExp")
                (buttonType == v.tButtonType.btExpanded))
            {
                if (((DevExpress.XtraEditors.SimpleButton)sender).TabIndex == 71)
                {
                    buttonName = "Collapse";
                    //((DevExpress.XtraEditors.SimpleButton)sender).Text = "A";
                }
                else
                {
                    buttonName = "Expand";
                    //((DevExpress.XtraEditors.SimpleButton)sender).Text = "K";
                }

                collExpIslemi(tForm, tableIPCode, buttonName);
                return onay;
            }
            #endregion collapse/expand butonları

            #region yazici butonları
            if (buttonType == v.tButtonType.btYazici)  // (ButtonName == "yazici")
            {
                yaziciIslemi(tForm, tableIPCode);
                return onay;
            }
            #endregion onay butonları

            #region ek1,ek2 butonları
            if ((buttonType == v.tButtonType.btEk1) ||
                (buttonType == v.tButtonType.btEk2) ||
                (buttonType == v.tButtonType.btEk3) ||
                (buttonType == v.tButtonType.btEk4) ||
                (buttonType == v.tButtonType.btEk5) ||
                (buttonType == v.tButtonType.btEk6) ||
                (buttonType == v.tButtonType.btEk7))
            {
                if (prop_ != null)
                {
                    onay = ekButtonIslemi(tForm, tableIPCode, prop_, buttonType);
                    saveRefreshControl(tForm, tableIPCode);
                }
                return onay;
            }
            #endregion ek1,ek2 butonları
            
            return onay;
        }

        private void saveRefreshControl(Form tForm, string tableIPCode)
        {
            // kayıt işlemi gerçeklişmişse
            if ((tForm.HelpButton) || (v.con_OnaySave))
            {
                t.TableRefresh(tForm, tableIPCode);
                tForm.HelpButton = false;
                v.con_OnaySave = false;
            }
        }

        private bool listele(Form tForm, string tableIPCode)
        {
            //tToolBox t = new tToolBox();
            bool onay = false;

            /// ms_Selenium da dataNavigator_PositionChanged de kontrol için kullanılmaktadır
            /// 
            v.con_Listele_TableIPCode = tableIPCode;

            DataSet ds = null;//t.Find_DataSet(tForm, "", tableIPCode, "btnClick/listele");
            DataNavigator dN = null;
            t.Find_DataSet(tForm, ref ds, ref dN, tableIPCode);
            
            if (ds != null)
            {
                bool valueOnayi = t.checkedKeyFieldValueNegative(ds);

                if (valueOnayi)
                {
                    t.TableRefresh(tForm, ds, tableIPCode);
                    //
                    t.ViewControl_Enabled(tForm, ds, tableIPCode);
                    // bu IPCode bağlı ExternalIPCode olabilir...
                    t.ViewControl_Enabled_ExtarnalIP(tForm, ds);
                    //
                    v.con_Listele_TableIPCode = "";
                }
                return onay;
            }
            return onay;
        }

        private bool listeyeEkle(Form tForm, List<PROP_NAVIGATOR> propList_, string tableIPCode, string columnFieldName)
        {
            //tToolBox t = new tToolBox();
            bool onay = false;
            bool find_Lkp_Onay = t.Find_FieldName(tForm, tableIPCode, "LKP_ONAY");
            bool find_Selected = false;

            // Aynı listede hem LKP_ONAY var hemde LKP_LISTEYE_EKLE varsa
            if (columnFieldName != "LKP_ONAY")
                find_Lkp_Onay = false;

            if (find_Lkp_Onay)
            {
                find_Selected = t.Find_Value_In_Field(tForm, tableIPCode, "LKP_ONAY", "True");

                /// kullanıcı hiç bir seçim yapmamış fakat Listeye Ekle butonuna basmış
                /// 
                if (find_Selected == false)
                {
                    onay = listeyeEkle_(tForm, propList_, tableIPCode);
                }
                else
                {
                    /// kullanıcı listeden seçim yapmış ve Listeye Ekle butonuna basmış 
                    /// 
                    onay = siraylaListeyeEkle_(tForm, propList_, tableIPCode);
                }
            }
            else
            {
                onay = listeyeEkle_(tForm, propList_, tableIPCode);
            }

            /// Burada sadece after işlemler çalışıyor
            /// 
            if (onay)
            {
                extraIslemVar(tForm, tableIPCode, v.tButtonType.btListeyeEkle, v.tBeforeAfter.After, propList_);

                /// AUTO_REFRESH_IP üzerinde refresh varsa onlarda çalışsın
                ev.Prop_RunTimeClick(tForm, null, tableIPCode, v.tButtonType.btListeyeEkle, v.tBeforeAfter.After);
            }
            return onay;
        }//listeyeEkle

        private bool listeyeEkle_(Form tForm, List<PROP_NAVIGATOR> propList_, string tableIPCode)
        {
            bool onay = false;

            if (propList_ != null)
            {
                /// Burada before işlemler çalışıyor
                /// listeye eklede genelde 'RDC', 'Run DataCopy'  çalışıyor
                onay = extraIslemVar(tForm, tableIPCode, v.tButtonType.btListeyeEkle, v.tBeforeAfter.Before, propList_);

                //if (onay)
                //    extraIslemVar(tForm, tableIPCode, v.tButtonType.btListeyeEkle, v.tBeforeAfter.After, propList_);
            }

            return onay;
        }//listeyeEkle

        private bool siraylaListeyeEkle_(Form tForm, List<PROP_NAVIGATOR> propList_, string tableIPCode)
        {
            bool onay = false;

            DataSet ds = null;
            DataNavigator dN = null;
            t.Find_DataSet(tForm, ref ds, ref dN, tableIPCode);

            if (t.IsNotNull(ds) != false)
            {
                string value = "";
                int length = ds.Tables[0].Rows.Count;
                for (int i = 0; i < length; i++)
                {
                    value = ds.Tables[0].Rows[i]["LKP_ONAY"].ToString();
                    if (value.IndexOf("True") > -1)
                    {
                        dN.Position = i;
                        Application.DoEvents();
                        transactionRunChange(propList_, false);
                        onay = listeyeEkle_(tForm, propList_, tableIPCode);
                    }
                }
            }

            return onay;
        }

        private void transactionRunChange(List<PROP_NAVIGATOR> propList_, bool value)
        {
            if (propList_ != null)
            {
                // buttonType uyguluğuna kontrol et
                //
                foreach (PROP_NAVIGATOR item in propList_)
                {
                        // bu satır daha önce çalıştı mı ?
                        item.TransactionRun = value;
                }
            }
        }


        private bool listeyeEkle_SearchLookUpEdit(Form tForm, List<PROP_NAVIGATOR> propList_, string tableIPCode, v.tButtonType buttonType, string columnFieldName, string selectValue)
        {
            bool onay = true;// false;
            bool transactionRun = false;
            bool islemOnayi = false;
            bool isFormOpen = true;
            bool elseItem = false;

            if (propList_ != null)
            {
                // buttonType uyguluğuna kontrol et
                //
                foreach (PROP_NAVIGATOR item in propList_)
                {
                    if (item.BUTTONTYPE.ToString() == Convert.ToString((byte)buttonType))
                    {
                        isFormOpen = t.tWorkingCheck(tForm, item, tableIPCode);

                        // bu satır daha önce çalıştı mı ?
                        transactionRun = item.TransactionRun;

                        // else satırı mı kontrol et
                        elseItem = (item.CHC_VALUE.ToString().IndexOf("ELSE") > -1);

                        // form açılması için onaylandı ise
                        // eğer daha önce çalışmamış ise çalışsın
                        if ((isFormOpen) && (transactionRun == false))
                        {
                            List<TABLEIPCODE_LIST> item2 = item.TABLEIPCODE_LIST;

                            // buttonType uygun olduğu için işlemi gerçekleştir 
                            onay = listeyeEkle_SearchLookUpEdit_(tForm, tableIPCode, item2, columnFieldName, selectValue);

                            // işem çalıştı onayı
                            item.TransactionRun = islemOnayi;

                            if (onay == false)
                                break;
                        }
                    }
                }
            }
                        
            return onay;
        }

        private bool listeyeEkle_SearchLookUpEdit_(Form tForm, string targetTableIPCode, List<TABLEIPCODE_LIST> item, string columnFieldName, string selectValue)
        {
            bool onay = true;
            string readFieldName = "";
            string targetFieldName = "";

            if (columnFieldName == "") return false;
            /*
            /// SearchLookUp içindeki dataTable
            DataSet dsData_ = ((DataTable)v.dt_SearchLookUp).DataSet;
            if (dsData_ == null) return false;

            DataRow sourceRow = null;
            int length = dsData_.Tables[0].Rows.Count;
            for (int i = 0; i < length; i++)
            {
                if (dsData_.Tables[0].Rows[i][columnFieldName].ToString() == selectValue)
                    sourceRow = dsData_.Tables[0].Rows[i];
            }
            */
            DataRow foundRow = null;
            int length = v.dt_SearchLookUp.Rows.Count;
            for (int i = 0; i < length; i++)
            {
                if ((string)v.dt_SearchLookUp.Rows[i][columnFieldName] == selectValue)
                {
                    foundRow = v.dt_SearchLookUp.Rows[i];
                    break; // Doğru satırı bulunca döngüyü sonlandır
                }
            } 

            DataSet dsTarget = null;
            DataNavigator dNTarget = null;

            t.Find_DataSet(tForm, ref dsTarget, ref dNTarget, targetTableIPCode);

            if (t.IsNotNull(dsTarget))
            {
                foreach (var item2 in item)
                {
                    readFieldName = item2.RKEYFNAME;
                    targetFieldName = item2.KEYFNAME;

                    if (t.IsNotNull(readFieldName) && t.IsNotNull(targetFieldName))
                    {
                        dsTarget.Tables[0].Rows[dNTarget.Position][targetFieldName] = foundRow[readFieldName];
                    }
                }
            }
            return onay;
        }


        private bool listeHazirla(Form tForm, string tableIPCode, List<PROP_NAVIGATOR> propList_)//PROP_NAVIGATOR prop_)
        {
            //tToolBox t = new tToolBox();
            bool onay = false;

            //v.tButtonType.btListeHazirla
            onay = extraIslemVar(tForm, tableIPCode, v.tButtonType.btListeHazirla, v.tBeforeAfter.Before, propList_);

            return onay;
        }

        #region newData 

        public bool newData(Form tForm, string tableIPCode)
        {
            //string workType = "NEW";
            bool onay = newDataExecute(tForm, tableIPCode, null, v.tButtonType.btNone, v.SP_NewWorkType);

            return onay;
        }

        private bool newDataButtonClick(Form tForm, string tableIPCode, List<PROP_NAVIGATOR> propList_, v.tButtonType buttonType)
        {
            bool onay = false;

            if (propList_ != null)
                onay = extraIslemVar(tForm, tableIPCode, buttonType, v.tBeforeAfter.Before, propList_);

            string workType = "NEW";
            onay = newDataExecute(tForm, tableIPCode, propList_, buttonType, workType);

            if (propList_ != null)
                onay = extraIslemVar(tForm, tableIPCode, buttonType, v.tBeforeAfter.After, propList_);

            return onay;
        }

        private bool newDataExecute(Form tForm, string tableIPCode, List<PROP_NAVIGATOR> propList_, v.tButtonType buttonType, string workType)
        /*string keyFName,string Parent_FName, string Value) */
        {
            //tToolBox t = new tToolBox();

            DataSet dsData = null;
            DataNavigator dN = null;

            t.Find_DataSet(tForm, ref dsData, ref dN, tableIPCode);

            if (dsData == null) return false;
            //if (dsData.Tables[0].Namespace == "NewRecord") return;
            if (dsData.Tables[0].CaseSensitive == true) return false;

            bool onay = true;
            bool old_PositionChange = false;
            string myProp = dsData.Namespace;
            string DetailSubDetail = t.MyProperties_Get(myProp, "DetailSubDetail:");
            string state = string.Empty;

            /// kaydet
            if (v.con_ColumnValueChanging)
            {
                if (propList_ != null)
                {
                    onay = kaydet(tForm, tableIPCode, propList_, buttonType);

                    if (onay)
                        v.con_ColumnValueChanging = false;
                }
            }

            ///  MasterDetail bağlantısı var ise
            ///= DetailSubDetail:True;
            ///  About_Detail_SubDetail:
            ///= Detail_SubDetail:AVI_DOS.AVI_DOS_05 || ID ||[AVI_DCK].ICRA_DOSYA_ID || 56 || 31 || 0 |||||| 578 | ds |;
            if (DetailSubDetail == "True")
            {
                string SubDetail_List = t.MyProperties_Get(myProp, "About_Detail_SubDetail:");
                if (SubDetail_MasterIDValueChecked(tForm, SubDetail_List) == false)
                {
                    if (v.con_PositionChange == false)
                    {
                        //MessageBox.Show("Lütfen öncelikli olan bilgileri girmeniz gerekiyor ...");
                        //return false;
                        state = "dsFirstRow";
                    }
                }
            }

            //
            // dsFirstRow : Aslında böyle bir state durumu yok, ilk satırı oluşturmak için
            // dN.Position == -1                   ise  dsFirstRow  
            // dN.Position != -1 and Column[0] == ""  ise  dsInsert
            // dN.Position != -1 and Column[0] != ""  ise  dsEdit
            //
            if (dN.Position != -1)
            {
                string IdValue = dsData.Tables[0].Rows[dN.Position][0].ToString();
                if ((IdValue == "") ||
                    (IdValue == "-999"))
                {
                    state = "dsInsert";
                    //NavigatorButton btn = dN.Buttons.CancelEdit;
                    //dN.Buttons.DoClick(btn);
                    //return true;

                    //v.con_Cancel = true;  bunu yüzünden subview çalışmıyor 

                    /// yeni satır çalışmasını engelliyor : sms yeni satır click olunca 
                    //NavigatorButton btnR = dN.Buttons.Remove;
                    //dN.Buttons.DoClick(btnR);
                }
                else state = "dsEdit";
            }
            else state = "dsFirstRow"; // dsFirstRow yaparak ilk satırın oluşması sağlanıyor

            DataRow newRow = dsData.Tables[0].NewRow();

            using (tDefaultValue df = new tDefaultValue())
            {
                df.tDefaultValue_And_Validation
                    (tForm,
                     dsData,
                     newRow,
                     tableIPCode,
                     "tData_NewRecord");
            }

            #region
            /* silme Alt Hesap Aç çalıştığında gerek olacak
             * 
            if (t.IsNotNull(Parent_FName) && t.IsNotNull(Value))
            {
                newRow[Parent_FName] = Value;

                // fields tablosu varsa
                if (dsData.Tables.Count > 1)
                {
                    string keyFName = t.Find_Table_Ref_FieldName(dsData.Tables[1]); bu ds değişti v.ds_MsTableFields oldu
                    // field tipini öğrenelim
                    string displayFormat = string.Empty;
                    int ftype = t.Find_Field_Type_Id(dsData, keyFName, ref displayFormat);

                    if ((t.IsNotNull(keyFName)) &&
                        (t.IsNotNull(Value)) &&
                        (ftype == 167) // VarChar() ise
                        )
                    {
                        newRow[keyFName] = Value + ".";
                    }
                }
            }
            */
            #endregion
            
            old_PositionChange = v.con_PositionChange;
            v.con_NewRecords = true;
            v.con_PositionChange = true;

            dsData.Tables[0].Rows.Add(newRow);

            NavigatorButton btnL = dN.Buttons.Last;
            dN.Buttons.DoClick(btnL);
            // bu yeni eklendi
            NavigatorButton btnN = dN.Buttons.Next;
            dN.Buttons.DoClick(btnN);

            v.con_NewRecords = false;

            /// geldiğinde true ise true kalsın diye
            if (old_PositionChange == false)
                v.con_PositionChange = false;

            if (dN.IsAccessible == true)
            {
                // subView var ise silelim
                // workType == "NEW" || workType == "ADDDATA" || workType == "SubDetailNEW"
                //if (workType == "NEW")
                //    t.tRemoveTabPagesForNewData(tForm);

                // master-detail çalıştıralım
                v.SP_NewWorkType = workType;
                tNewDataAfterSubWork(tForm, tableIPCode);
            }
            // setFocus ( SubWork hariç )
            // 
            if (v.con_SubWork_Run == false)
                gridViewSetFirstColumn(tForm, tableIPCode);

            // yeni data işleminden sonra yapılacak işlemler
            tAfter_RUN(tForm, dsData, "tNewData");

            //
            // sqlChangeText(dsData);  newData dan sonra Listele çalışınca sorun oluşuyor
            //

            // Control Enabled
            t.ViewControl_Enabled(tForm, dsData, tableIPCode);
            // bu IPCode bağlı ExternalIPCode olabilir...
            t.ViewControl_Enabled_ExtarnalIP(tForm, dsData);
            // AutoSearch 
            v.tSearch.AutoSearch = true;

            return onay;
        }

        //private bool tNewData(Form tForm, string TableIPCode, string Key_FName, string Parent_FName, string Value)
        private bool tNewDataAfterSubWork(Form tForm, string TableIPCode)
        {
            bool onay = true;

            if (v.con_PositionChange == false)
            {
                vSubWork vSW = new vSubWork();
                vSW._01_tForm = tForm;
                vSW._02_TableIPCode = TableIPCode;
                vSW._03_WorkTD = v.tWorkTD.Refresh_SubDetail;
                vSW._04_WorkWhom = v.tWorkWhom.Childs;
                try
                {
                    ev.tSubWork_(vSW);
                }
                catch (Exception)
                {
                    onay = false;
                    //throw;
                }
            }

            return onay;
        }
        
        private bool SubDetail_MasterIDValueChecked(Form tForm, string SubDetail_List)
        {
            //tToolBox t = new tToolBox();

            #region Tanımlar
            bool onay = true;
            string satir = string.Empty;
            string read_mst_TableIPCode = string.Empty;
            string read_mst_FName = string.Empty;
            string read_sub_FName = string.Empty;
            string read_field_type = string.Empty;
            string read_mst_value = string.Empty;

            //string OperandType = string.Empty;
            //string read_RefId = string.Empty;
            //string mst_TableIPCode = string.Empty;
            //string mst_CheckFName = string.Empty;
            //string mst_CheckValue = string.Empty;
            //byte default_type = 0;
            #endregion Tanımlar

            //= DetailSubDetail:True;
            //About_Detail_SubDetail:
            //= Detail_SubDetail: AVI_DOS.AVI_DOS_05       || ID           || [AVI_DCK].ICRA_DOSYA_ID          || 56  || 31 || 0 |||||| 578  | ds |;
            //= Detail_SubDetail: 3S_MSTBLIP.3S_MSTBLIP_02 || @TABLEIPCODE || [3S_MSTBLIP_VWO].LKP_TABLEIPCODE || 167 || 0  || 0 |||||| 1065 | ds |;
            #region read values
            //Application.OpenForms[0].Text = Application.OpenForms[0].Text + ";sdR2;" + mst_TableIPCode;
            while (SubDetail_List.IndexOf("Detail_SubDetail:") > -1)
            {
                satir = t.Get_And_Clear(ref SubDetail_List, "|ds|") + "||";

                t.Get_And_Clear(ref satir, "Detail_SubDetail:");

                //=Detail_SubDetail:3S_MSTBLIP.3S_MSTBLIP_02||@TABLEIPCODE||[3S_MSTBLIP_VWO].LKP_TABLEIPCODE||167||0||0||||||1065|ds|

                // okunacak field
                read_mst_TableIPCode = t.Get_And_Clear(ref satir, "||");
                read_mst_FName = t.Get_And_Clear(ref satir, "||");
                // atanacak field
                read_sub_FName = t.Get_And_Clear(ref satir, "||");
                read_field_type = t.Get_And_Clear(ref satir, "||");

                /// -- şimdilik kullanılmıyor fakat silme
                /*
                default_type = System.Convert.ToByte(t.Get_And_Clear(ref satir, "||"));
                OperandType = t.Get_And_Clear(ref satir, "||");
                mst_CheckFName = t.Get_And_Clear(ref satir, "||");
                mst_CheckValue = t.Get_And_Clear(ref satir, "||");
                read_RefId = t.Get_And_Clear(ref satir, "||");
                */
                if (t.IsNotNull(read_mst_TableIPCode))
                    read_mst_value = t.Find_TableIPCode_Value(tForm, read_mst_TableIPCode, read_mst_FName);

                /// tüm verileri (master-detail) kontrol edilecek
                /// bir defa false gelse hepsi için false dönecek
                /// 
                onay = t.Get_ValueTrue(t.myInt32(read_field_type), read_mst_value);
            }
            #endregion

            return onay;
        }

        private void sqlChangeText(DataSet ds)
        {
            //tToolBox t = new tToolBox();

            if (ds.Namespace != null)
            {
                // sqlFirst in içeriği 
                // select * from x_tableName where ID = 0;
                // sqlSecond un içeriği ise
                // select * from x_tableName where ID = xxx;
                //
                // buradaki amaç : 
                // sqlSecond ı da sıfırlamak
                // sqlSecond = sqlFirst  yapmak

                string myProp = ds.Namespace.ToString();
                if (t.IsNotNull(myProp))
                {
                    string SqlF = t.MyProperties_Get(myProp, "SqlFirst:");
                    string SqlS = "=SqlSecond:" + t.MyProperties_Get(myProp, "SqlSecond:");
                    string Sql_NewS = "=SqlSecond:" + SqlF;
                    t.Str_Replace(ref myProp, SqlS, Sql_NewS);

                    ds.Namespace = myProp;
                }
            }
        }

        private void tAfter_RUN(Form tForm, DataSet dSData, string functionName)
        {
            /// ilk ele aldığında  tAfter_RUN  ismini değiştir
            /// 
            //tToolBox t = new tToolBox();

            string myProp = dSData.Namespace.ToString();

            if (functionName == "tNewData")
            {
                if (myProp.IndexOf("=DataWizardTabPage:True;") > -1)
                {
                    string TableIPCode = t.MyProperties_Get(myProp, "TableIPCode:");

                    t.PageChange(tForm, TableIPCode, "FIRST");
                }
            }
        }

        public void preparingNewData(Form tForm, string TableIPCode)
        {
            using (tToolBox t = new tToolBox())
            {
                DataSet dsData = t.Find_DataSet(tForm, "", TableIPCode, "");

                if (dsData != null)
                {
                    if (dsData.Tables[0].CaseSensitive == true)
                        dsData.Tables[0].CaseSensitive = false;
                }
            }
        }

        #endregion   tNewData

        private bool kaydet(Form tForm, string tableIPCode, List<PROP_NAVIGATOR> propList_, v.tButtonType buttonType)
        {
            //tToolBox t = new tToolBox();

            bool onay = true;

            //v.formLastActiveControl = tForm.ActiveControl;

            /// kaydet butonuna extra yük atılmış 
            /// bir kaydet butonu tuşu çalıştırıldığında, 
            /// kendisinden önce başka IP lerde kayıt ettirilebilir
            //v.tButtonType.btKaydet
            if (propList_ != null)
                onay = extraIslemVar(tForm, tableIPCode, buttonType, v.tBeforeAfter.Before, propList_);

            /// şimdi basılan butonun kaydını
            if (onay)
            {
                /// Kayıt öncesi Id value yi oku
                /// 
                v.tTableKeyFieldValue.Clear();
                t.TableKeyFieldValue(tForm, tableIPCode, "before");

                tSave sv = new tSave();
                onay = sv.tDataSave(tForm, tableIPCode);
                if (onay)
                {
                    //t.ButtonEnabledAll(tForm, tableIPCode, true);
                    // 
                    v.con_SubWork_Refresh = true;
                    tForm.HelpButton = true;

                    /// Kayıt sonrası Id value yi oku
                    t.TableKeyFieldValue(tForm, tableIPCode, "after");
                }
                //
                if (v.formLastActiveControl != null)
                    if (v.formLastActiveControl.GetType().ToString().IndexOf("Control") == -1)
                        tForm.ActiveControl = v.formLastActiveControl;
            }

            if ((onay) && (propList_ != null))
                onay = extraIslemVar(tForm, tableIPCode, buttonType, v.tBeforeAfter.After, propList_);

            return onay;
        }

        public bool gotoData_(Form tForm, TABLEIPCODE_LIST item)
        {
            /// bir tablodan bir Id oku                : ReadTableIPCode
            /// diğer tabloyuda bu Id ile yeniden oku  : TargetTableIPCode
            /// Örnek
            /// OnmBelgeStokS.BelgeStokBId : ReadTableIPCode
            /// OnmBelgeStokB.Id yenile    : TargetTableIPCode
            /// 

            //tToolBox t = new tToolBox();
            bool onay = false;

            string workType = string.Empty;
            string targetTABLEIPCODE = string.Empty;
            string targetKEYFNAME = string.Empty;
            string readTABLEIPCODE = string.Empty;
            string readKEYFNAME = string.Empty;
            string manuelSetValue = string.Empty;
            string readValue = string.Empty;

            DataSet dsTarget = null;
            DataNavigator dNTarget = null;
            DataSet dsRead = null;
            DataNavigator dNRead = null;

            workType = item.WORKTYPE.ToString();

            if (t.IsNotNull(item.TABLEIPCODE))
                targetTABLEIPCODE = t.Set(item.TABLEIPCODE.ToString(), "", "");
            if (t.IsNotNull(item.KEYFNAME))
                targetKEYFNAME = t.Set(item.KEYFNAME.ToString(), "", "");
            if (t.IsNotNull(item.RTABLEIPCODE))
                readTABLEIPCODE = t.Set(item.RTABLEIPCODE.ToString(), "", "");
            if (t.IsNotNull(item.RKEYFNAME))
                readKEYFNAME = t.Set(item.RKEYFNAME.ToString(), "", "");
            if (t.IsNotNull(item.MSETVALUE))
                manuelSetValue = t.Set(item.MSETVALUE.ToString(), "", "");
            /* read datayı burada okumaya gerek yok 
             * zaten yeni formu açmak isteyen diğer mevcut form üzerinden manuelSetValue ile okunarak geliryor
             * yani read formu ayrı, target formu ayrı
             * iki farklı form
             * 
            dsRead = null;
            dNRead = null;
            if (t.IsNotNull(readTABLEIPCODE))
                t.Find_DataSet(tForm, ref dsRead, ref dNRead, readTABLEIPCODE);

            if (t.IsNotNull(dsRead) &&
                t.IsNotNull(readKEYFNAME))
            {
                readValue = dsRead.Tables[0].Rows[dNRead.Position][readKEYFNAME].ToString();
            }
            */
            dsTarget = null;
            dNTarget = null;
            if (t.IsNotNull(targetTABLEIPCODE))
                t.Find_DataSet(tForm, ref dsTarget, ref dNTarget, targetTABLEIPCODE);

            if (t.IsNotNull(dsTarget))
            {
                int i = t.Find_GotoRecord(dsTarget, targetKEYFNAME, manuelSetValue);
                dNTarget.Position = i;
                if (i > 0) onay = true;
            }

            return onay;

        }
        public bool readData_(Form tForm, TABLEIPCODE_LIST item)
        {
            /// bir tablodan bir Id oku                : ReadTableIPCode
            /// diğer tabloyuda bu Id ile yeniden oku  : TargetTableIPCode
            /// Örnek
            /// OnmBelgeStokS.BelgeStokBId : ReadTableIPCode
            /// OnmBelgeStokB.Id yenile    : TargetTableIPCode
            /// 

            //tToolBox t = new tToolBox();
            bool onay = false;
            
            string workType = string.Empty;
            string targetTABLEIPCODE = string.Empty;
            string targetKEYFNAME = string.Empty;
            string readTABLEIPCODE = string.Empty;
            string readKEYFNAME = string.Empty;
            string manuelSetValue = string.Empty;
            string readValue = string.Empty;

            DataSet dsTarget = null;
            DataNavigator dNTarget = null;
            DataSet dsRead = null;
            DataNavigator dNRead = null;

            workType = item.WORKTYPE.ToString();

            if (t.IsNotNull(item.TABLEIPCODE))
                targetTABLEIPCODE = t.Set(item.TABLEIPCODE.ToString(), "", "");
            if (t.IsNotNull(item.KEYFNAME))
                targetKEYFNAME = t.Set(item.KEYFNAME.ToString(), "", "");
            if (t.IsNotNull(item.RTABLEIPCODE))
                readTABLEIPCODE = t.Set(item.RTABLEIPCODE.ToString(), "", "");
            if (t.IsNotNull(item.RKEYFNAME))
                readKEYFNAME = t.Set(item.RKEYFNAME.ToString(), "", "");
            if (t.IsNotNull(item.MSETVALUE))
                manuelSetValue = t.Set(item.MSETVALUE.ToString(), "", "");

            dsRead = null;
            dNRead = null;
            if (t.IsNotNull(readTABLEIPCODE))
                t.Find_DataSet(tForm, ref dsRead, ref dNRead, readTABLEIPCODE);

            if (t.IsNotNull(dsRead) &&
                t.IsNotNull(readKEYFNAME))
            {
                readValue = dsRead.Tables[0].Rows[dNRead.Position][readKEYFNAME].ToString();
            }

            dsTarget = null;
            dNTarget = null;
            if (t.IsNotNull(targetTABLEIPCODE))
                t.Find_DataSet(tForm, ref dsTarget, ref dNTarget, targetTABLEIPCODE);

            if (readValue == "" && manuelSetValue != "")
                readValue = manuelSetValue;

            //if (t.IsNotNull(dsTarget))
            if (dsTarget != null)
            {
                onay = t.TableRefreshNewValue(tForm, dsTarget, readValue, -1);
            }

            return onay;
            
        }

        private bool readAndSetData_(Form tForm, TABLEIPCODE_LIST item)
        {
            /// DİKKAT sorun çıkınca    listeyeEkle__beklet   bak


            //tToolBox t = new tToolBox();
            bool onay = true;

            string workType = string.Empty;
            string targetTABLEIPCODE = string.Empty;
            string targetKEYFNAME = string.Empty;
            string readTABLEIPCODE = string.Empty;
            string readKEYFNAME = string.Empty;
            string manuelSetValue = string.Empty;
            bool old_PositionChange = false;

            DataSet dsTarget = null;
            DataNavigator dNTarget = null;
            DataSet dsRead = null;
            DataNavigator dNRead = null;

            workType = item.WORKTYPE.ToString();

            if (t.IsNotNull(item.TABLEIPCODE))
                targetTABLEIPCODE = t.Set(item.TABLEIPCODE.ToString(), "", "");
            if (t.IsNotNull(item.KEYFNAME))
                targetKEYFNAME = t.Set(item.KEYFNAME.ToString(), "", "");
            if (t.IsNotNull(item.RTABLEIPCODE))
                readTABLEIPCODE = t.Set(item.RTABLEIPCODE.ToString(), "", "");
            if (t.IsNotNull(item.RKEYFNAME))
                readKEYFNAME = t.Set(item.RKEYFNAME.ToString(), "", "");
            if (t.IsNotNull(item.MSETVALUE))
                manuelSetValue = t.Set(item.MSETVALUE.ToString(), "", "");

            dsTarget = null;
            dNTarget = null;
            if (t.IsNotNull(targetTABLEIPCODE))
                t.Find_DataSet(tForm, ref dsTarget, ref dNTarget, targetTABLEIPCODE);

            dsRead = null;
            dNRead = null;
            if (t.IsNotNull(readTABLEIPCODE))
                t.Find_DataSet(tForm, ref dsRead, ref dNRead, readTABLEIPCODE);


            #region //eğer table halen açılmamışsa önce açmak gerekiyor
            if ((dsTarget != null) && (t.IsNotNull(targetKEYFNAME)) && (dNTarget.Position == -1))
            {
                old_PositionChange = v.con_PositionChange;
                v.con_PositionChange = true;

                //newDataExec(tForm, dsTarget, dNTarget, targetTABLEIPCODE, "", "", "");
                newData(tForm, targetTABLEIPCODE);

                // aşağıda tekrar yapmasın diye
                //oldTableIPCode = targetTABLEIPCODE;

                if (old_PositionChange == false)
                    v.con_PositionChange = false;

                //onay = true;
            }
            #endregion

            if (t.IsNotNull(dsTarget) &&
                t.IsNotNull(targetKEYFNAME) &&
                t.IsNotNull(dsRead) &&
                t.IsNotNull(readKEYFNAME)
                )
            {
                dsTarget.Tables[0].Rows[dNTarget.Position][targetKEYFNAME] =
                                    dsRead.Tables[0].Rows[dNRead.Position][readKEYFNAME];
            }
            if (t.IsNotNull(dsTarget) &&
                t.IsNotNull(targetKEYFNAME) &&
                t.IsNotNull(dsRead) == false &&
                t.IsNotNull(manuelSetValue)
                )
            {
                dsTarget.Tables[0].Rows[dNTarget.Position][targetKEYFNAME] = manuelSetValue;
            }
            if (t.IsNotNull(dsTarget) == false &&
                t.IsNotNull(dsRead) &&
                t.IsNotNull(manuelSetValue)
                )
            {
                dsRead.Tables[0].Rows[dNRead.Position][readKEYFNAME] = manuelSetValue;
            }

            //else
            //    MessageBox.Show(workType.ToString() + " için bilgilerde eksiklik mevcut...");



            return onay;
        }

        private bool messageBoxShow_(Form tForm, TABLEIPCODE_LIST item)
        {
            //tToolBox t = new tToolBox();
            bool onay = true;

            string workType = string.Empty;
            //string targetTABLEIPCODE = string.Empty;
            //string targetKEYFNAME = string.Empty;
            string readTABLEIPCODE = string.Empty;
            string readKEYFNAME = string.Empty;

            //DataSet dsTarget = null;
            //DataNavigator dNTarget = null;
            DataSet dsRead = null;
            DataNavigator dNRead = null;

            workType = item.WORKTYPE.ToString();

            //targetTABLEIPCODE = t.Set(item.TABLEIPCODE.ToString(), "", "");
            //targetKEYFNAME = t.Set(item.KEYFNAME.ToString(), "", "");
            readTABLEIPCODE = t.Set(item.RTABLEIPCODE.ToString(), "", "");
            readKEYFNAME = t.Set(item.RKEYFNAME.ToString(), "", "");

            //dsTarget = null;
            //dNTarget = null;
            //if (t.IsNotNull(targetTABLEIPCODE))
            //    t.Find_DataSet(tForm, ref dsTarget, ref dNTarget, targetTABLEIPCODE);

            string message = "";
            dsRead = null;
            dNRead = null;
            if (t.IsNotNull(readTABLEIPCODE))
                t.Find_DataSet(tForm, ref dsRead, ref dNRead, readTABLEIPCODE);


            if (t.IsNotNull(dsRead) &&
                t.IsNotNull(readKEYFNAME) &&
                dNRead.Position > -1)
            {
                message = dsRead.Tables[0].Rows[dNRead.Position][readKEYFNAME].ToString();
                onay = true;
                //MessageBox.Show(message);
                t.FlyoutMessage(tForm, "Bilgilendirme : ", message);
            }
            else
            {
                message = item.MSETVALUE;
                if (t.IsNotNull(message))
                {
                    onay = true;
                    //MessageBox.Show(message);
                    t.FlyoutMessage(tForm, "Bilgilendirme : ", message);
                }
            }
            return onay;
        }

        private bool questionShow_(Form tForm, TABLEIPCODE_LIST item, ref bool islemOnayi)
        {
            //tToolBox t = new tToolBox();
            bool onay = false;
            string Question = t.Set(item.CAPTION.ToString(), "", "");

            if (t.IsNotNull(Question) == false)
            Question = t.Set(item.MSETVALUE.ToString(), "", "");

            if (t.IsNotNull(Question))
            {
                DialogResult answer = MessageBox.Show(Question, "Onay İşlemi", MessageBoxButtons.YesNo);

                switch (answer)
                {
                    case DialogResult.Yes:
                        {
                            onay = true;
                            break; // break ifadesini sakın silme
                        }
                    case DialogResult.No:
                        {
                            break; // break ifadesini sakın silme
                        }
                    case DialogResult.Cancel:
                        {
                            break; // break ifadesini sakın silme
                        }
                }
            }
            // soru islemi gerçekleşti, soruldu. cevap ise onay üzerinde
            islemOnayi = true;

            return onay;
        }

        //

        private bool listeSil(Form tForm, string tableIPCode, List<PROP_NAVIGATOR> propList_)
        {
            //tToolBox t = new tToolBox();

            bool onay = false;

            DataNavigator tDataNavigator = t.Find_DataNavigator(tForm, tableIPCode);
            if (tDataNavigator != null)
            {
                object tDataTable = tDataNavigator.DataSource;
                DataSet dsData = ((DataTable)tDataTable).DataSet;
                int pos = tDataNavigator.Position;
                if (pos == -1) return onay;

                // Gerekli olan verileri topla
                vTable vt = new vTable();
                t.Preparing_DataSet(tForm, dsData, vt);

                
                string myProp = dsData.Namespace.ToString();
                string tTableName = t.MyProperties_Get(myProp, "TableName:");
                //string SqlF = t.MyProperties_Get(myProp, "SqlFirst:");
                string SqlS = t.MyProperties_Get(myProp, "SqlSecond:");
                string selectSql = SqlS;
                /*
                string deleteSql = SqlS.Remove(0, SqlS.ToUpper().IndexOf("WHERE"));
                if (deleteSql.ToUpper().IndexOf("ORDER BY") > -1)
                    deleteSql = deleteSql.Remove(deleteSql.ToUpper().IndexOf("ORDER BY"));
                deleteSql = "delete " + vt.TableName + " " + v.ENTER + deleteSql;
                
                v.SQL = deleteSql + v.ENTER2 + v.SQL;
                */
                //--view

                DataSet ds_MultiRows = new DataSet();
                t.SQL_Read_Execute(vt.DBaseNo, ds_MultiRows, ref selectSql, vt.TableName, "");

                if (t.IsNotNull(ds_MultiRows))
                {
                    tCreateObject co = new tCreateObject();
                    if (co.Create_Delete_Form(ds_MultiRows, tableIPCode, v.tRowCount.MultiRows) == DialogResult.Yes)
                    {
                        string deleteSql = getDeleteSql(ds_MultiRows, vt);

                        try
                        {
                            onay = true;
                            if (propList_ != null)
                                onay = extraIslemVar(tForm, tableIPCode, v.tButtonType.btSilListe, v.tBeforeAfter.Before, propList_); //26

                            if (onay)
                            {
                                onay = t.Sql_ExecuteNon(ds_MultiRows, ref deleteSql, vt);

                                // buton click ten dolayı tDataSave gidiyor, gidince fonksiyonun 
                                // girişi geri dönmesi için true ataması yapılıyor
                                v.con_LkpOnayChange = true;

                                //onay = listele(tForm, tableIPCode);
                                listele(tForm, tableIPCode);

                                if (onay) v.Kullaniciya_Mesaj_Var = "Kayıtları SİLME işlemi başarıyla sonuçlandı.!";

                                // görevi bitti
                                v.con_LkpOnayChange = false;
                            }

                            if ((onay) && (propList_ != null))
                                onay = extraIslemVar(tForm, tableIPCode, v.tButtonType.btSilListe, v.tBeforeAfter.After, propList_);

                            if (vt.RunTime)
                            {
                                ev.Prop_RunTimeClick(tForm, null, tableIPCode, v.tButtonType.btSilSatir, v.tBeforeAfter.After); // before nedeni için fonksina bak
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("DİKKAT : Silme işlemi sırasında bir sorun oluştu..." + v.ENTER2 + e.Message.ToString());
                            //throw;
                        }
                    }

                }

                return onay;
            }

            return onay;
        }

        private string getDeleteSql(DataSet ds, vTable vt)
        {
            string tSql = "";
            string keyFName = vt.KeyId_FName;
            string IdList = "";

            foreach (DataRow item in ds.Tables[0].Rows)
            {
                IdList += ", " + item[keyFName].ToString();
            }

            IdList = IdList.Remove(0, 1);

            tSql = "delete from " + vt.TableName + " where " + vt.KeyId_FName + " in ( " + IdList + " ) ";

            return tSql;
        }
        private bool satirSil(Form tForm, string tableIPCode, List<PROP_NAVIGATOR> propList_)
        {
            //tToolBox t = new tToolBox();
            bool onaySil = false;
            bool onay = true;
            bool IsLock = false;
            
            DataNavigator tDataNavigator = t.Find_DataNavigator(tForm, tableIPCode);
            if (tDataNavigator != null)
            {
                object tDataTable = tDataNavigator.DataSource;
                DataSet dsData = ((DataTable)tDataTable).DataSet;
                int pos = tDataNavigator.Position;
                if (pos == -1) return onaySil;
                Control viewCntrl = t.Find_Control_View(tForm, tableIPCode);

                // Gerekli olan verileri topla
                vTable vt = new vTable();
                t.Preparing_DataSet(tForm, dsData, vt);

                IsLock = IsLockRecord(dsData, vt, pos);

                if (t.IsNotNull(vt.TableName) && t.IsNotNull(vt.KeyId_FName) && (IsLock == false))
                {
                    string RefId = dsData.Tables[vt.IPCode].Rows[pos][vt.KeyId_FName].ToString();

                    if (t.IsNotNull(RefId))
                    {
                        string single_select =
                            " Select * from " + vt.TableName +
                            " where " + vt.KeyId_FName + " = " + RefId;

                        string delete_sql =
                            " Delete from " + vt.TableName +
                            " where " + vt.KeyId_FName + " = " + RefId;

                        DataSet ds_SingleRow = new DataSet();
                        //t.Data_Read_Execute(tForm, ds_SingleRow, ref single_select, TableName, null);
                        t.SQL_Read_Execute(vt.DBaseNo, ds_SingleRow, ref single_select, vt.TableName, "");

                        if (t.IsNotNull(ds_SingleRow))
                        {
                            tCreateObject co = new tCreateObject();
                            if (co.Create_Delete_Form(ds_SingleRow, tableIPCode, v.tRowCount.SingleRow) == DialogResult.Yes)
                            {
                                try
                                {
                                    if (propList_ != null)
                                        onay = extraIslemVar(tForm, tableIPCode, v.tButtonType.btSilSatir, v.tBeforeAfter.Before, propList_); //26

                                    //if (t.SQL_ExecuteNon(ds_SingleRow, ref delete_sql, null))
                                    if ((onay) && (t.Sql_ExecuteNon(ds_SingleRow, ref delete_sql, vt)))
                                    {
                                        // buton click ten dolayı tDataSave gidiyor, gidince fonksiyonun 
                                        // girişi geri dönmesi için true ataması yapılıyor
                                        v.con_LkpOnayChange = true;
                                        
                                        // Silindi onayı
                                        onaySil = true;
                                                                                
                                        NavigatorButton btn = tDataNavigator.Buttons.Remove;
                                        tDataNavigator.Buttons.DoClick(btn);

                                        dsData.AcceptChanges();

                                        if (vt.RunTime)
                                        {
                                            //tEvents ev = new tEvents();
                                            ev.Prop_RunTimeClick(tForm, null, tableIPCode, v.tButtonType.btSilSatir, v.tBeforeAfter.Before); // before nedeni için fonksina bak
                                        }
                                        v.Kullaniciya_Mesaj_Var = "Kayıt SİLME işlemi başarıyla sonuçlandı.!";

                                        // görevi bitti
                                        v.con_LkpOnayChange = false;
                                    }

                                    if ((onay) && (propList_ != null))
                                        onay = extraIslemVar(tForm, tableIPCode, v.tButtonType.btSilSatir, v.tBeforeAfter.After, propList_);

                                    if (vt.RunTime)
                                    {
                                        ev.Prop_RunTimeClick(tForm, null, tableIPCode, v.tButtonType.btSilSatir, v.tBeforeAfter.After); // before nedeni için fonksina bak
                                    }

                                }
                                catch (Exception e)
                                {
                                    MessageBox.Show("DİKKAT : Silme işlemi sırasında bir sorun oluştu..." + v.ENTER2 + e.Message.ToString());
                                    //throw;
                                    onaySil = false;
                                }
                            }

                        }
                    }
                    else
                    {
                        if (viewCntrl.GetType().ToString() == "DevExpress.XtraGrid.GridControl")
                        {
                            if (MessageBox.Show("Satır sil ?", "Onay", MessageBoxButtons.YesNo) != DialogResult.Yes)
                                return onaySil;

                            var xview = ((DevExpress.XtraGrid.GridControl)viewCntrl).MainView;

                            if (xview.GetType().ToString() == "DevExpress.XtraGrid.Views.Card.CardView")
                            {
                                ((DevExpress.XtraGrid.Views.Card.CardView)xview).DeleteRow(((DevExpress.XtraGrid.Views.Card.CardView)xview).FocusedRowHandle);
                            }

                            GridView view = ((DevExpress.XtraGrid.GridControl)viewCntrl).MainView as GridView;
                            view?.DeleteRow(view.FocusedRowHandle);
                        }
                    }

                    // hiç satır kalmadıysa
                    if (dsData.Tables[vt.IPCode].Rows.Count == 0)
                    {
                        v.SP_NewWorkType = "DeleteNEW";
                        newData(tForm, tableIPCode);
                    }
                }

                if (IsLock)
                {
                    t.FlyoutMessage(tForm, "Uyarı :", "Silmeye çalıştığınız kayıt kilitlendiği için silemezsiniz...");
                }
            }

            return onaySil;
        }

        private bool IsLockRecord(DataSet ds, vTable vt, int pos)
        {
            bool onay = false;

            string lockFieldName = vt.Lock_FName;

            /// IsLock Kontrolü yapılıyor
            ///
            if (t.IsNotNull(lockFieldName))
            {
                string value = ds.Tables[0].Rows[pos][lockFieldName].ToString();
                if (value == "True")
                    onay = true;
            }
            return onay;
        }

        private void hesapAc(Form tForm, PROP_NAVIGATOR prop_)
        {
            //tToolBox t = new tToolBox();

            /// aynı form içinde en az iki IP var
            /// birinci IP de liste var, ikinci IP de listedeki kaydın detayı var
            /// ama bu master-detail değil, çünkü liste üzerindeki kayıt sadece kullanıcı
            /// istediği zaman (Hesap Aç) ile detayı görmek istediği zaman açılacak
            /// 

            string WORKTYPE = string.Empty;
            string TABLEIPCODE = string.Empty;
            string KEYFNAME = string.Empty;
            string RTABLEIPCODE = string.Empty;
            string RKEYFNAME = string.Empty;
            string oldWORKTYPE = string.Empty;
            string oldTableIPCode = string.Empty;
            string readValue = string.Empty;

            DataSet dsData = null;
            DataNavigator tDataNavigator = null;
            DataSet dsDataR = null;
            DataNavigator tDataNavigatorR = null;

            foreach (var item in prop_.TABLEIPCODE_LIST)
            {
                WORKTYPE = item.WORKTYPE.ToString();

                #region
                if (WORKTYPE == "READ")
                {
                    oldWORKTYPE = WORKTYPE;
                    TABLEIPCODE = t.Set(item.TABLEIPCODE.ToString(), "", "");
                    KEYFNAME = t.Set(item.KEYFNAME.ToString(), "", "");
                    RTABLEIPCODE = t.Set(item.RTABLEIPCODE.ToString(), "", "");
                    RKEYFNAME = t.Set(item.RKEYFNAME.ToString(), "", "");

                    dsData = null;
                    tDataNavigator = null;
                    t.Find_DataSet(tForm, ref dsData, ref tDataNavigator, TABLEIPCODE);

                    if ((dsData != null) && (t.IsNotNull(KEYFNAME)))
                    {
                        dsDataR = null;
                        tDataNavigatorR = null;
                        t.Find_DataSet(tForm, ref dsDataR, ref tDataNavigatorR, RTABLEIPCODE);

                        if ((t.IsNotNull(dsDataR)) && (t.IsNotNull(RKEYFNAME)))
                        {
                            /// okunacak/açlılacak kaydın id si ( Listeden okunuyor )
                            readValue = dsDataR.Tables[0].Rows[tDataNavigatorR.Position][RKEYFNAME].ToString();

                            /// okunacak/açıklacak IP  
                            string myProp = dsData.Namespace.ToString();
                            string TableLabel = t.MyProperties_Get(myProp, "TableLabel:");
                            t.Alias_Control(ref TableLabel);
                            string newValue = "and " + TableLabel + KEYFNAME + " = " + readValue + " ";

                            // not : kriterler tekrar devreye alınca burayıda aktif hale getir
                            // 25.10.2019 tkn
                            //External_Kriterleri_Uygula(dsData, TableLabel, newValue, null);
                            MessageBox.Show("dikkat : External_Kriterleri_Uygula() procedure açılması gerekiyor ");
                        }
                    }// if dsData
                }
                #endregion
            }
        }

        private bool yeniFormYeniData(Form tForm, PROP_NAVIGATOR prop_)
        {
            if (prop_ == null) return false;

            //tToolBox t = new tToolBox();
            //
            t.OpenForm_JSON(tForm, prop_);
            //
            DataSet ds = null;
            string WORKTYPE = string.Empty;
            foreach (var item in prop_.TABLEIPCODE_LIST)
            {
                WORKTYPE = item.WORKTYPE.ToString();

                #region
                if (WORKTYPE == "GOTO")
                {
                    // save çalıştıysa
                    if (v.con_GotoRecord == "ONdialog")
                    {

                        v.con_TableIPCode = t.Set(item.TABLEIPCODE.ToString(), "", "");
                        v.con_GotoRecord_FName = t.Set(item.KEYFNAME.ToString(), "", "");

                        /// burada dataset neden ihtiyaç var diye sorabilirsin
                        /// aslında gerek yok
                        /// fakat tNewForm_NewData yu kulanan menu buttonu birden fazla ekranda kullanılabilir
                        /// yani New in ardından birden fazla GOTO komutu çağrılmış olabilir
                        /// eğer doğru form ise aşağıdaki işlemler yapılsın diye DataSet kontrol ediliyor
                        /// Örnek : Yeni Müşteri butonu için 
                        /// CR.CR_OMARA_L01 veya 
                        /// CR.CR_OMARA_L02 listesi üzerinde GOTO yapılabilir
                        /// 
                        ds = t.Find_DataSet(tForm, "", v.con_TableIPCode, "");

                        if (ds != null)
                        {
                            // refresh 
                            //tForm.HelpButton = true;
                            //
                            //myForm_Refresh(tForm, "tNewForm_NewData");

                            //
                            t.TableRefresh(tForm, ds);

                            //
                            t.tGotoRecord(tForm, ds,
                                v.con_TableIPCode,
                                v.con_GotoRecord_FName,
                                v.con_GotoRecord_Value,
                                v.con_GotoRecord_Position);
                        }
                    }
                }
                #endregion
            }

            return true;
        }

        private bool openControlForm_(Form tForm, string tableIPCode, List<PROP_NAVIGATOR> propList_, v.tButtonType buttonType)
        {
            
            bool onay = false;
            //bool isChecked = false;

            foreach (PROP_NAVIGATOR prop_ in propList_)
            {
                if (prop_.BUTTONTYPE.ToString() == Convert.ToString((byte)buttonType))
                {
                    onay = extraIslemVar(tForm, tableIPCode, buttonType, v.tBeforeAfter.Before, propList_);

                    onay = openForm_(tForm, tableIPCode, prop_);

                    if (onay)
                        onay = extraIslemVar(tForm, tableIPCode, buttonType, v.tBeforeAfter.After, propList_);
                }
            }

            saveRefreshControl(tForm, tableIPCode);

            return onay;
        }
               

        private bool openForm_(Form tForm, string tableIPCode, PROP_NAVIGATOR prop_)
        {
            //tToolBox t = new tToolBox();
            bool onay = false;

            bool isChecked = t.tWorkingCheck(tForm, prop_, tableIPCode);

            // form açılması için onaylandı ise
            if (isChecked)
            {
                t.OpenForm_JSON(tForm, prop_);
                onay = true;
                return onay;
            }
            return onay;
        }
        private void openReportDesignForm_(Form tForm, string tableIPCode, List<PROP_NAVIGATOR> propList_)
        {
            if (tableIPCode.IndexOf("MsReports") > -1)
            {
                //tToolBox t = new tToolBox();
                // v.IsWaitOpen = true > bu işlem yukarıda otomatik açılıyor. onun içinde burada da kapatılıyor.
                // burayı silme, gerekli
                v.IsWaitOpen = false;
                t.WaitFormClose();

                DataSet dsMsReports = null;
                DataNavigator dNReports = null;
                t.Find_DataSet(tForm, ref dsMsReports, ref dNReports, tableIPCode);
                                
                string sourceFormCodeAndName = "";
                if (tForm.AccessibleDescription != null)
                   sourceFormCodeAndName = tForm.AccessibleDescription;

                tReportDevEx rapor = new tReportDevEx();
                rapor.tShowReportDesigner(tForm, dsMsReports, dNReports, sourceFormCodeAndName);
            }
        }
        private bool openPictureEditForm_(Form tForm, string tableIPCode, List<PROP_NAVIGATOR> propList_)
        {
            bool onay = false;
            bool isChecked = false;

            foreach (PROP_NAVIGATOR prop_ in propList_)
            {
                if (prop_.BUTTONTYPE.ToString() == Convert.ToString((byte)v.tButtonType.btResimEditor))
                {
                    isChecked = t.tWorkingCheck(tForm, prop_, tableIPCode);

                    // form açılması için onaylandı ise
                    if (isChecked)
                    {
                        v.tResimEditor.Clear();
                        v.tResimEditor.imagesSourceFormName = tForm.Name;
                        v.tResimEditor.imagesSourceTableIPCode = tableIPCode;
                        v.tResimEditor.imagesSourceFieldName = "";
                        v.tResimEditor.formCode = prop_.FORMCODE; // kaynak kişilerin olduğu formCode
                        v.tResimEditor.formWidth = t.myInt32(prop_.FORM_WIDTH);
                        v.tResimEditor.formHeight = t.myInt32(prop_.FORM_HEIGHT);

                        /// listeli işlem mi geldi kontrol et
                        if (t.IsNotNull(v.tResimEditor.imagesMasterTableIPCode) == false)
                        {
                            foreach (TABLEIPCODE_LIST item in prop_.TABLEIPCODE_LIST)
                            {
                                v.tResimEditor.imagesMasterFormCode = item.TABLEIPCODE; // eğer targetTableIPCode de value varsa; images liste ve images veri datasını tutan formCode mevcut
                                v.tResimEditor.imagesMasterTableIPCode = item.RTABLEIPCODE; // eğer readTableIPCode de value varsa sadece images listesini tutan TableIPCode var  
                            }
                        }

                        onay = openPictureEditForm(v.tResimEditor);
                        return onay;
                    }
                }
            }
            return onay;
        }

        private bool openPictureEditForm(vResimEditor tResimEditor)
        {
            bool onay = false;

            /// Sadece bir kişi için ve sadece images listesi ise 
            /// 
            if (tResimEditor.imagesMasterTableIPCode != "" &&
                tResimEditor.imagesMasterFormCode == "" &&
                tResimEditor.listTableIPCode == "" && 
                tResimEditor.formCode == "")
            {
                // ms_Pictures içindeki Resimler için dataset hazırlanıyor
                // yani gerekli datasetin prc_xxxxx çalıştırılıyor
                // ms_Pictures açılınca buradaki dataset orada yeniden clone oluyor
                //
                Form tFormMaster = Application.OpenForms[tResimEditor.imagesSourceFormName];
                tInputPanel ip = new tInputPanel();
                v.con_ImagesMasterDataSet = ip.Create_DataSet(tFormMaster,  tResimEditor.imagesMasterTableIPCode, false);
            }

            /// Images ve datası için form var ise ve
            /// Kaynak kişi listesi için form  var ise
            /// 
            if ((tResimEditor.imagesMasterTableIPCode != "" || tResimEditor.imagesMasterFormCode != "") &&
                (tResimEditor.listTableIPCode != "" || tResimEditor.formCode != ""))
            {
                v.con_ImagesMasterDataSet = null;
                /// Listeli tarama yapılacaksa bu işleme gerek yok
                /// Çünkü listeye bağlı olarak kişi resimleri okunuyor
                ///
            }

            //tToolBox t = new tToolBox();
            tForms fr = new tForms();
            Form tNewForm = null;
            tNewForm = fr.Get_Form("ms_Pictures");

            try
            {
                t.ChildForm_View(tNewForm, Application.OpenForms[0], FormWindowState.Maximized, v.con_FormLoadValue);
                onay = true;
            }
            catch (Exception)
            {
                //
                throw;
            }

            return onay;
        }

        private void onayIslemi(Form tForm, string TableIPCode, string Caption)
        {
            //tToolBox t = new tToolBox();
            DataNavigator tDataNavigator = t.Find_DataNavigator(tForm, TableIPCode);

            if (tDataNavigator != null)
            {
                object tDataTable = tDataNavigator.DataSource;
                DataSet dsData = ((DataTable)tDataTable).DataSet;

                if (t.IsNotNull(dsData))
                {
                    v.con_OnayChange = true;

                    int i2 = dsData.Tables[0].Rows.Count;
                    for (int i = 0; i < i2; i++)
                    {
                        if (Caption == "+") // (ButtonName == "onayla")
                            dsData.Tables[0].Rows[i]["LKP_ONAY"] = 1;
                        if (Caption == "-") // (ButtonName == "onay_iptal")
                            dsData.Tables[0].Rows[i]["LKP_ONAY"] = 0;
                    }

                    if (tDataNavigator.IsAccessible == true)
                    {
                        ev.Data_Refresh(tForm, dsData, tDataNavigator);
                    }
                    v.con_OnayChange = false;
                }
            }
        }

        private void collExpIslemi(Form tForm, string TableIPCode, string buttonName)
        {
            //tToolBox t = new tToolBox();

            Control cntrl = t.Find_Control_View(tForm, TableIPCode);

            //if (ButtonName == "Collapse")
            //if (ButtonName == "Expand")

            if (cntrl != null)
            {
                if (cntrl.GetType().ToString() == "DevExpress.XtraGrid.GridControl")
                {
                    if (((DevExpress.XtraGrid.GridControl)cntrl).MainView.GetType().ToString() == "DevExpress.XtraGrid.Views.Grid.GridView")
                    {
                        GridView view = ((DevExpress.XtraGrid.GridControl)cntrl).MainView as GridView;
                        if (buttonName == "Expand")
                            view.ExpandAllGroups();
                        if (buttonName == "Collapse")
                            view.CollapseAllGroups();
                    }
                    if (((DevExpress.XtraGrid.GridControl)cntrl).MainView.GetType().ToString() == "DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView")
                    {
                        AdvBandedGridView view = ((DevExpress.XtraGrid.GridControl)cntrl).MainView as AdvBandedGridView;
                        if (buttonName == "Expand")
                            view.ExpandAllGroups();
                        if (buttonName == "Collapse")
                            view.CollapseAllGroups();
                    }
                }

                if (cntrl.GetType().ToString() == "DevExpress.XtraVerticalGrid.VGridControl")
                {
                    if (buttonName == "Collapse")
                        ((DevExpress.XtraVerticalGrid.VGridControl)cntrl).CollapseAllRows();
                    if (buttonName == "Expand")
                        ((DevExpress.XtraVerticalGrid.VGridControl)cntrl).ExpandAllRows();
                }

                if (cntrl.GetType().ToString() == "DevExpress.XtraTreeList.TreeList")
                {
                    if (buttonName == "Collapse")
                        ((DevExpress.XtraTreeList.TreeList)cntrl).CollapseAll();
                    if (buttonName == "Expand")
                        ((DevExpress.XtraTreeList.TreeList)cntrl).ExpandAll();
                }
            }
        }

        private void yaziciIslemi(Form tForm, string TableIPCode)
        {
            //tToolBox t = new tToolBox();

            Control cntrl = null;
            cntrl = t.Find_Control_View(tForm, TableIPCode);

            if (cntrl != null)
            {
                GridControl grid = null;
                if (cntrl.GetType().ToString() == "DevExpress.XtraGrid.GridControl")
                    grid = cntrl as GridControl;

                grid.ShowRibbonPrintPreview();
            }

        }

        private bool aramaIslemi(Form tForm, string tableIPCode, PROP_NAVIGATOR prop_, string columnValue)
        {
            /// Search form create
            /// 
            tSearch se = new tSearch();

            bool Onay = se.searchEngines(tForm, tableIPCode, columnValue, prop_);

            return Onay;
        }

        private bool findListDataIslemi(Form tForm, string tableIPCode, PROP_NAVIGATOR prop_, string columnValue)
        {
            /// Datası yüklenmemiş grid içindeki şartlı arama işlemi
            /// 
            tSearch se = new tSearch();

            bool Onay = se.findListDataEngines(tForm, tableIPCode, columnValue, prop_);

            return Onay;
        }
        public bool dataTransferi(Form tForm, string TableIPCode, PROP_NAVIGATOR prop_) // Run_TableIPCode
        {
            /// Run_TableIPCode ile burada okunan data ile SETDATA işlemi yapılmaktadır
            /// yani istenen datayı oku, ekrandaki başka bir dataset e ata.
            /// Örnek : Fiyat Tablosunu oku Faturya set et...
            /// 

            //tToolBox t = new tToolBox();

            // burda yeni okunacak data bulunmakta
            //
            bool onay = false;
            bool islemOnayi = false;
            string readTableIPCode = "";
            string targetTableIPCode = "";

            if (prop_.READ_TABLEIPCODE != null)
                readTableIPCode = prop_.READ_TABLEIPCODE.ToString();

            if (prop_.TARGET_TABLEIPCODE != null)
                targetTableIPCode = prop_.TARGET_TABLEIPCODE.ToString();

            if (t.IsNotNull(readTableIPCode) &&
                t.IsNotNull(targetTableIPCode))
            {

                tInputPanel ip = new tInputPanel();
                DataSet dsRead = ip.Create_DataSet(tForm, readTableIPCode, false);
                DataNavigator dNRead = new DataNavigator();
                
                if (t.IsNotNull(dsRead))
                {
                    dNRead.DataSource = dsRead.Tables[0];
                    dNRead.Position = 0;

                    DataSet dsTarget = null;
                    DataNavigator dNTarget = null;
                    t.Find_DataSet(tForm, ref dsTarget, ref dNTarget, targetTableIPCode);

                    tSetData_(prop_.TABLEIPCODE_LIST,
                              dsRead, dNRead,
                              dsTarget, dNTarget);

                    onay = extraIslemVar_(tForm, prop_, v.tBeforeAfter.After, v.tButtonType.btNone, ref islemOnayi);
                }
            }

            return islemOnayi;
        }
        private bool runStoredProcedure(Form tForm, TABLEIPCODE_LIST item, PROP_NAVIGATOR prop_)
        {
            //tToolBox t = new tToolBox();

            // burda SQL üzerindeki stored procedure çalışacak
            // 
            bool onay = false;
            string tableIPCode = "";

            string message = item.MSETVALUE;
            if (t.IsNotNull(message))
            {
                DialogResult cevap = t.mySoru(message);
                if (DialogResult.Yes != cevap)
                {
                    return false;
                }
            }

            if (item.RTABLEIPCODE != null)
                tableIPCode = item.RTABLEIPCODE.ToString();

            // halen boşsa target i kontrol et
            if (t.IsNotNull(tableIPCode) == false)
                tableIPCode = item.TABLEIPCODE.ToString();

            if (t.IsNotNull(tableIPCode) == false)
            {
                MessageBox.Show("DİKKAT : StoredProcedure tanımlı değil...");
            }

            if (t.IsNotNull(tableIPCode))
            {
                // Eğer ip master-detail ile bağlanmışsa create sırasında sql hazırlandığı için çalışarak geliyor
                // onun için burada extra bir işlem yapmaya gerek kalmıyor
                //
                tInputPanel ip = new tInputPanel();
                DataSet dsData = ip.Create_DataSet(tForm, tableIPCode, false);
                //DataNavigator dN = new DataNavigator();
                if (dsData != null)
                {
                    onay = true;

                    string readKeyFieldName = item.RKEYFNAME.ToString();
                    string mesaj = "";

                    if (t.IsNotNull(readKeyFieldName))
                    {
                        item.MSETVALUE = dsData.Tables[0].Rows[0][readKeyFieldName].ToString();

                        if (item.MSETVALUE != "")
                        {
                            mesaj = t.Set(item.MSETVALUE, "İşlem başarıyla çalıştı...", "");

                            t.FlyoutMessage(tForm, "Bilgilendirme", mesaj + v.ENTER);
                        }
                    }
                    else
                    {
                        mesaj = t.Set("İşlem başarıyla çalıştı ...", "", "");

                        t.AlertMessage("Stored Procedure" + v.ENTER2, mesaj);
                    }

                }
            }

            return onay;
        }
        private bool runFormul(Form tForm, TABLEIPCODE_LIST item, PROP_NAVIGATOR prop_)
        {
            bool onay = false;

            string tableIPCode = "";

            if (item.RTABLEIPCODE != null)
                tableIPCode = item.RTABLEIPCODE.ToString();

            if (t.IsNotNull(tableIPCode) == false)
                tableIPCode = item.TABLEIPCODE.ToString();

            if (t.IsNotNull(tableIPCode) == false)
            {
                MessageBox.Show("DİKKAT : Formul için TableIPCode tanımlı değil...");
            }
            if (t.IsNotNull(tableIPCode))
            {
                t.work_EXPRESSION(tForm, tableIPCode, "", "");
            }
            return onay;
        }
        private bool formulleriHesapla(Form tForm, string TableIPCode, PROP_NAVIGATOR prop_) // Run_Expression
        {
            //tToolBox t = new tToolBox();

            if (t.IsNotNull(prop_.TARGET_TABLEIPCODE) == false)
            {
                MessageBox.Show("DİKKAT : formulleriHesapla için TARGET_TABLEIPCODE ayarlanacak ...");
                return false;
            }

            string targetTableIPCode = prop_.TARGET_TABLEIPCODE.ToString();

            prop_.TARGET_TABLEIPCODE.ToString();

            // hesaplamalar çalışıyor
            t.work_EXPRESSION(tForm, targetTableIPCode, "", "");
            return true;
        }

        private bool InputBox(Form tForm, string TableIPCode, PROP_NAVIGATOR prop_)
        {
            bool onay = false;
            foreach (TABLEIPCODE_LIST item in prop_.TABLEIPCODE_LIST)
            {
                if (item.WORKTYPE.ToString() == "IBOX")
                {
                    onay = InputBox(tForm, TableIPCode, item, null);
                    if (onay == false)
                    {
                        break;
                    }
                }
            }
            return onay;
        }
     
        public bool InputBox(Form tForm, string TableIPCode, TABLEIPCODE_LIST item, DataRow sourceDataRow)
        {
            if (item.WORKTYPE.ToString() != "IBOX") return false;

            //tToolBox t = new tToolBox();

            vUserInputBox iBox = new vUserInputBox();

            //{
            //"CAPTION": "Miktar Girin",
            //"TABLEIPCODE": "null",
            //"TABLEALIAS": "null",
            //"KEYFNAME": "Miktar",
            //"RTABLEIPCODE": "null",
            //"RKEYFNAME": "UrunAdi",
            //"MSETVALUE": "null",
            //"WORKTYPE": "IBOX",
            //"CONTROLNAME": "null",
            //"DCCODE": "null",
            //"BEFOREAFTER": "null",

            /// TABLEIPCODE_LIST içinde ilk defa check kontrolü kullanılıyor, bu özellik başka bir yere eklenmedi henüz

            //"CHC_IPCODE": "UST/OMS/HStok.AramaTumu_L01",
            //"CHC_FNAME": "SatisMiktariniSorma",
            //"CHC_VALUE": "0",
            //"CHC_OPERAND": "="
            //},

            bool checked_ = t.tWorkingCheck(tForm, item, sourceDataRow);

            /// kontrolden dolayı onay alamadı fakat yinede true dönmesi gerekiyor
            if (checked_ == false) 
                return true;

            bool onay = true;
            string target_FName = string.Empty;
            string read_TableIPCode = string.Empty;
            string read_FName = string.Empty;
            string read_Caption = string.Empty;
            string inputbox_Message = string.Empty;
            string inputbox_Default1 = string.Empty;
            string input_Value = string.Empty;
            string displayFormat = string.Empty;
            int ftype = 0;

            target_FName = item.KEYFNAME.ToString();
            read_TableIPCode = item.RTABLEIPCODE.ToString();
            read_FName = item.RKEYFNAME.ToString();
            inputbox_Message = item.CAPTION.ToString();
            inputbox_Default1 = item.MSETVALUE.ToString();

            int pos = -1;
            DataSet dS = null;
            DataNavigator dN = null;
            
            if (t.IsNotNull(read_TableIPCode))
                t.Find_DataSet(tForm, ref dS, ref dN, read_TableIPCode);
            else
                t.Find_DataSet(tForm, ref dS, ref dN, TableIPCode);

            if (dN != null) pos = dN.Position;
                        
                        
            displayFormat = string.Empty;

            // Tutar sorulacak ise
            if (t.IsNotNull(read_TableIPCode) && t.IsNotNull(read_FName))
            {
                ftype = t.Find_Field_Type_Id(dS, read_FName, ref displayFormat);
                target_FName = read_FName;
            }
            else ftype = t.Find_Field_Type_Id(dS, target_FName, ref displayFormat);




            input_Value = "";
            if (t.IsNotNull(inputbox_Default1))
                input_Value = inputbox_Default1;

            // inputBox kimin için soru soruyor bunu anlamak için
            // RKEYFNAME de display fieldname saklanıyor, ve o field deki isim okunarak 
            // inputboxda display edilecek
            // örnek : 
            // EKMEK için           << display fieldName << RKEYFNAME
            // Lütfen Miktar girin  << inputbox_message
            // read_TableIPCode dolu ise o tablodan bir tutar okunacak ve kullanıcıya bu tutar sorulacak ( Tutar Sor )

            #region

            if (pos > -1)
            {
                if (t.IsNotNull(target_FName))
                    input_Value = dS.Tables[0].Rows[pos][target_FName].ToString();

                if (t.IsNotNull(read_TableIPCode) == false && t.IsNotNull(read_FName))
                    read_Caption = dS.Tables[0].Rows[pos][read_FName].ToString();

                // Tutar sorulacak
                if (t.IsNotNull(read_TableIPCode) && t.IsNotNull(read_FName))
                {
                    //read_Caption = inputbox_Message;
                    input_Value = dS.Tables[0].Rows[pos][read_FName].ToString();
                }
            }

            if (t.IsNotNull(input_Value) == false)
                input_Value = "1";

            inputbox_Message = read_Caption + /*" için " +*/ v.ENTER2 + inputbox_Message;

            #endregion

            iBox.Clear();
            iBox.title = "Veri Girişi";
            iBox.promptText = inputbox_Message;
            iBox.value = input_Value;
            iBox.displayFormat = displayFormat;
            iBox.fieldType = ftype;

            DialogResult dialogResult = t.UserInpuBox(iBox);

            input_Value = iBox.value;

            switch (dialogResult)
            {
                case DialogResult.OK:
                    {
                        onay = true;
                        break;
                    }
                case DialogResult.Cancel:
                    {
                        input_Value = "";
                        onay = false;
                        break;
                    }
            }

            if (onay)
            {
                // ilk atama yapılıyor
                dS.Tables[0].Rows[pos][target_FName] = input_Value;
                
                /// hesaplama görevi Run_Expression metoduna atandı
                // hesaplamalar çalışıyor
                //work_EXPRESSION(tForm, TableIPCode, v.con_Expression_FieldName, input_Value);

                // atama kabul ediliyor
                dS.Tables[0].AcceptChanges();
                // tekrar Editlemek için en son yapılan atama tekrar yapılıyor
                dS.Tables[0].Rows[pos][target_FName] = input_Value;

                // atanan veriler anında viewcontrol üzerinde görünsün
                ev.viewControlFocusedValue(tForm, TableIPCode);

                //
                iBox.Clear();
                v.con_Expression_FieldName = "";
                v.con_InputBox_FieldName = target_FName;
            }

            return onay;
        }

        private void NumberDisplayRead(Form tForm, string TableIPCode, PROP_NAVIGATOR prop_)
        {
            //
            foreach (TABLEIPCODE_LIST item in prop_.TABLEIPCODE_LIST)
            {
                if (item.WORKTYPE.ToString() == "NUMBERDISPLAY")
                {
                    ReadNumberDidplay(tForm, item);
                }
            }
        }
        public bool ReadNumberDidplay(Form tForm, TABLEIPCODE_LIST item)
        {
            if (item.WORKTYPE.ToString() != "NUMBERDISPLAY") return false;


            ///"TABLEIPCODE_LIST": [
            ///{
            ///  "CAPTION": "Miktar için NumberControl oku",
            ///  "TABLEIPCODE": "UST/OMS/BStokS.HIZLISATIS600_L01",
            ///  "TABLEALIAS": "null",
            ///  "KEYFNAME": "MIKTAR",
            ///  "RTABLEIPCODE": "null",
            ///  "RKEYFNAME": "null",
            ///  "MSETVALUE": "1",
            ///  "WORKTYPE": "NUMBERDISPLAY",
            ///  "CONTROLNAME": "NumberDisplay",
            ///  "DCCODE": "null"
            ///}

            bool onay = true;
            string controlName = string.Empty;
            controlName = item.CONTROLNAME.ToString();
            Control cntrl = t.Find_NumberControlDisplay(tForm, controlName);
            
            if (cntrl != null)
            {
                string TableIPCode = string.Empty;
                string target_FName = string.Empty;
                string mySetValue = string.Empty;
                string readValue = string.Empty;

                TableIPCode = item.TABLEIPCODE.ToString();
                target_FName = item.KEYFNAME.ToString();
                mySetValue = item.MSETVALUE.ToString();

                int pos = -1;
                DataSet dS = null;
                DataNavigator dN = null;
                t.Find_DataSet(tForm, ref dS, ref dN, TableIPCode);

                if (dN != null) pos = dN.Position;

                string displayFormat = string.Empty;
                int ftype = t.Find_Field_Type_Id(dS, target_FName, ref displayFormat);

                // Header Panel içindeki textEdit okundu
                readValue = ((DevExpress.XtraEditors.TextEdit)cntrl).Text;

                if (t.IsNotNull(mySetValue) == false) mySetValue = "";

                if (t.IsNotNull(readValue) == false) readValue = "1"; // mySetValue;

                try
                {
                    dS.Tables[0].Rows[pos][target_FName] = readValue;

                    /// hesaplama görevi Run_Expression metoduna atandı
                    // hesaplamalar çalışıyor
                    //work_EXPRESSION(tForm, TableIPCode, v.con_Expression_FieldName, input_Value);

                    // atama kabul ediliyor
                    dS.Tables[0].AcceptChanges();
                    // tekrar Editlemek için en son yapılan atama tekrar yapılıyor
                    dS.Tables[0].Rows[pos][target_FName] = readValue;

                    // mySetValue boş ta olabiliyor, yani display boşaltılıyor
                    ((DevExpress.XtraEditors.TextEdit)cntrl).Text = mySetValue;

                    // atanan veriler anında viewcontrol üzerinde görünsün
                    ev.viewControlFocusedValue(tForm, TableIPCode);
                    v.con_Expression_FieldName = "";
                    v.con_InputBox_FieldName = target_FName;
                }
                catch (Exception)
                {
                    //throw;
                }

            }

            return onay;
        }


        public void tSetData_(List<TABLEIPCODE_LIST> TableIPCodeList,
            DataSet read_dsData,
            DataNavigator read_dN,
            DataSet target_dsData,
            DataNavigator target_dN
            )
        {
            ///"TABLEIPCODE_LIST": [
            ///    {
            ///    "CAPTION": "ID > Cari ID",
            ///    "TABLEIPCODE": "null",
            ///    "TABLEALIAS": "null",
            ///    "KEYFNAME": "CARI_ID",
            ///    "RTABLEIPCODE": "null",
            ///    "RKEYFNAME": "ID",
            ///    "MSETVALUE": "null",
            ///    "WORKTYPE": "SETDATA",
            ///    "CONTROLNAME": "null",
            ///    "DCCODE": "null"
            ///    }
            ///    

            /// new search value set
            if (TableIPCodeList != null)
            {
                //tToolBox t = new tToolBox();

                string T_FNAME = string.Empty;
                string R_FNAME = string.Empty;
                string workType = string.Empty;
                
                ///{
                ///  "CAPTION": "ID > Cari ID",
                ///  "TABLEIPCODE": "null",
                ///  "TABLEALIAS": "null",
                ///  "KEYFNAME": "CARI_ID",
                ///  "RTABLEIPCODE": "null",
                ///  "RKEYFNAME": "ID",
                ///  "MSETVALUE": "null",
                ///  "WORKTYPE": "SETDATA",
                ///  "CONTROLNAME": "null",
                ///  "DCCODE": "null"
                ///},

                foreach (TABLEIPCODE_LIST item in TableIPCodeList)
                {
                    T_FNAME = item.KEYFNAME.ToString();
                    R_FNAME = item.RKEYFNAME.ToString();
                    workType = item.WORKTYPE.ToString();

                    if (t.IsNotNull(read_dsData))
                    {
                        try
                        {
                            if (t.IsNotNull(R_FNAME))
                            {
                                if (read_dN.Position > -1)
                                    item.MSETVALUE = read_dsData.Tables[0].Rows[read_dN.Position][R_FNAME].ToString();
                                else item.MSETVALUE = read_dsData.Tables[0].Rows[0][R_FNAME].ToString(); // position == -1 ise
                            }
                        }
                        catch (Exception e1)
                        {
                            //throw;
                            MessageBox.Show("HATA : " + R_FNAME + v.ENTER2 + e1.ToString());
                        }
                    }

                    if (t.IsNotNull(target_dsData) && t.IsNotNull(item.MSETVALUE))
                    {
                        try
                        {
                            if (t.IsNotNull(T_FNAME))
                            {
                                target_dsData.Tables[0].Rows[target_dN.Position][T_FNAME] = item.MSETVALUE;
                            }
                        }
                        catch (Exception e2)
                        {
                            //throw;
                            MessageBox.Show("HATA : " + T_FNAME + v.ENTER2 + e2.ToString());
                        }
                    }

                    if (workType == "MESSAGESHOW")
                    {
                        if (t.IsNotNull(item.MSETVALUE))
                            t.FlyoutMessage(null, "Bilgilendirme", item.MSETVALUE);
                            //MessageBox.Show(item.MSETVALUE);
                    }
                }
            }

        }


        //private bool ekButtonIslemi(Form tForm, string tableIPCode, List<PROP_NAVIGATOR> propList_, v.tButtonType buttonType)
        #region extraIslem ler

        private bool extraIslemVar(Form tForm, string tableIPCode, v.tButtonType buttonType,
                                   v.tBeforeAfter beforeAfter, List<PROP_NAVIGATOR> propList_)
        {
            bool onay = true;// false;
            bool transactionRun = false;
            bool islemOnayi = false;
            bool isFormOpen = true;
            bool elseItem = false;
            bool elseOncesiCalisti = false;

            if (propList_ != null)
            {
                // buttonType uyguluğuna kontrol et
                //
                foreach (PROP_NAVIGATOR item in propList_)
                {
                    if (item.BUTTONTYPE.ToString() == Convert.ToString((byte)buttonType))
                    {
                        isFormOpen = t.tWorkingCheck(tForm, item, tableIPCode);

                        // bu satır daha önce çalıştı mı ?
                        transactionRun = item.TransactionRun;

                        // else satırı mı kontrol et
                        elseItem = (item.CHC_VALUE.ToString().IndexOf("ELSE") > -1);
                        if (elseItem) // else satırına geldik
                        {
                            elseOncesiCalisti = elseOncesiniKontrolEt(propList_, buttonType);
                            if ((elseOncesiCalisti) && // elseden öncede çalıştı
                                (isFormOpen))          // else satırının çalışması için onay da aldı
                                isFormOpen = false;    // fakat else den önce çalıştığı için else satırının onayı iptal, çalışmasın
                        }

                        // form açılması için onaylandı ise
                        // eğer daha önce çalışmamış ise çalışsın
                        if ((isFormOpen) && (transactionRun == false))
                        {
                            // buttonType uygun olduğu için işlemi gerçekleştir 
                            onay = extraIslemVar_(tForm, item, beforeAfter, buttonType, ref islemOnayi);

                            // işem çalıştı onayı
                            item.TransactionRun = islemOnayi;

                            if (onay == false)
                                break;
                        }
                    }
                }
            }

            return onay;
        }

        private bool elseOncesiniKontrolEt(List<PROP_NAVIGATOR> propList_, v.tButtonType buttonType)
        {
            bool onay = false;

            foreach (PROP_NAVIGATOR item in propList_)
            {
                if (item.BUTTONTYPE.ToString() == Convert.ToString((byte)buttonType))
                {
                    if (item.TransactionRun)
                    {
                        onay = true;
                        break;
                    }
                }
            }

            return onay;
        }

        private bool ekButtonIslemi(Form tForm, string tableIPCode, PROP_NAVIGATOR prop_, v.tButtonType buttonType)
        {
            bool onay = false;

            if (prop_ != null)
            {
                //if (buttonType == v.tButtonType.btOpenSubView)
                //    onay = openSubView(tForm, tableIPCode, propList_, v.tButtonType.btOpenSubView);
                onay = extraIslemCalistir(tForm, tableIPCode, prop_);

                DataSet dsData = null;
                DataNavigator dN = null;
                t.Find_DataSet(tForm, ref dsData, ref dN, tableIPCode);
                vTable vt = new vTable();
                t.Preparing_DataSet(tForm, dsData, vt);

                if (vt.RunTime)
                {
                    ev.Prop_RunTimeClick(tForm, null, tableIPCode, v.tButtonType.btListele, v.tBeforeAfter.After); 
                }
            }
            return onay;
        }

        private bool extraIslemCalistir(Form tForm, string tableIPCode, PROP_NAVIGATOR prop_)
        {
            bool onay = false;
            bool islemOnayi = false;
            if (prop_ != null)
            {
                //onay = extraIslemVar(tForm, tableIPCode, v.tButtonType.btExtraIslem, v.tBeforeAfter.Before, propList_);
                ////if (onay)
                //onay = extraIslemVar(tForm, tableIPCode, v.tButtonType.btExtraIslem, v.tBeforeAfter.After, propList_);

                onay = extraIslemVar_(tForm, prop_, v.tBeforeAfter.Before, v.tButtonType.btNone, ref islemOnayi);

                if (onay)
                    onay = extraIslemVar_(tForm, prop_, v.tBeforeAfter.After, v.tButtonType.btNone, ref islemOnayi);
            }
            return islemOnayi;
        }
        
        private bool extraIslemVar_(Form tForm, PROP_NAVIGATOR prop_, v.tBeforeAfter tBeforeAfter, v.tButtonType istekbuttonType, ref bool islemOnayi)
        {
            //tToolBox t = new tToolBox();

            string TABLEIPCODE = string.Empty;
            string KEYFNAME = string.Empty;
            string workType = string.Empty;
            string BEFOREAFTER = string.Empty;
            string beforeAfter = string.Empty;
            
            v.tButtonType buttonType = t.getClickType(Convert.ToInt32(prop_.BUTTONTYPE.ToString()));
            bool onay = true;
            bool checked_ = false;

            if (tBeforeAfter == v.tBeforeAfter.Before)
                beforeAfter = "BEFORE";
            else beforeAfter = "AFTER";

            foreach (TABLEIPCODE_LIST item in prop_.TABLEIPCODE_LIST)
            {
                /*
                'WORKTYPE', '',           'none');
                'WORKTYPE', 'REFRESH',    'Refresh Data');
                'WORKTYPE', 'SUBREFRESH', 'Sub Refresh Data');
                'WORKTYPE', 'READ',       'Read Data');
                'WORKTYPE', 'NEW',        'New Data');
                'WORKTYPE', 'GOTO',       'Goto Record');
                'WORKTYPE', 'SVIEW',      'Sub View');
                'WORKTYPE', 'SVIEWVALUE', 'Sub View Value');
                'WORKTYPE', 'CREATEVIEW', 'Create View');
                'WORKTYPE', 'SAVEDATA',   'Save Data');
                'WORKTYPE', 'SETDATA',    'Set Data');
                'WORKTYPE', 'SETFOCUS',   'Set Focus');
                'WORKTYPE', 'RDC',        'Run DataCopy');
                'WORKTYPE', 'RPRC',       'Run StoredProcedure');
                'WORKTYPE', 'OPENFORM',   'Open Form');
                'WORKTYPE', 'IBOX',       'Input Box');
                'WORKTYPE', 'CLEARDATA',  'Clear Data');

                */
                checked_ = t.tWorkingCheck(tForm, item, null);

                /// kontrolden dolayı onay alamadı fakat yinede true dönmesi gerekiyor
                if (checked_ == false)
                {
                    continue;
                }



                workType = item.WORKTYPE.ToString();

                if ((buttonType == v.tButtonType.btDataTransferi) &&
                    (workType == "SETDATA"))
                {
                    continue;
                }

                if (item.BEFOREAFTER != null)
                {
                    BEFOREAFTER = item.BEFOREAFTER.ToString();
                    if (BEFOREAFTER == "null") BEFOREAFTER = "BEFORE";
                }

                TABLEIPCODE = t.Set(item.TABLEIPCODE.ToString(), item.RTABLEIPCODE.ToString(), "");

                #region

                if (BEFOREAFTER == beforeAfter)
                {
                    if (workType == "REFRESH")
                    {
                        if (t.IsNotNull(TABLEIPCODE))
                        {
                            DataSet ds = null; //t.Find_DataSet(tForm, "", TABLEIPCODE, "");
                            DataNavigator dN = null;
                            t.Find_DataSet(tForm, ref ds, ref dN, TABLEIPCODE);
                            
                            if (ds != null)
                            {
                                int pos = 0;
                                if (dN != null && dN.Position > 0)
                                    pos = dN.Position;

                                /// Refresh öncesi Id value yi oku
                                /// 
                                v.tTableKeyFieldValue.Clear();
                                t.TableKeyFieldValue(tForm, TABLEIPCODE, "before");
                                
                                /// sadece kendisi refresh olsun 
                                /// kendisine bağlı olanlar refresh olmasın
                                /// dataNavigator_PositionChanged( 
                                ///
                                v.con_Cancel = true;

                                onay = t.TableRefresh(tForm, ds);//, TABLEIPCODE);
                                islemOnayi = onay;

                                // refreshin yapıldığı pos a dön
                                if (pos > 0)
                                {
                                    dN.Position = pos;
                                }

                                /// Refresh den sonraki Id value yi oku
                                t.TableKeyFieldValue(tForm, TABLEIPCODE, "after");
                            }
                        }
                    }

                    if (workType == "SUBREFRESH")
                    {
                        if (t.IsNotNull(TABLEIPCODE))
                        {
                            DataSet ds = null; // t.Find_DataSet(tForm, "", TABLEIPCODE, "");
                            DataNavigator dN = null;
                            t.Find_DataSet(tForm, ref ds, ref dN, TABLEIPCODE);

                            if (ds != null)
                            {
                                ev.tSubDetail_Refresh(ds, dN);
                                /// Her zaman true dönmesi gerekiyor
                                /// 
                                onay = true;
                                islemOnayi = onay; 
                            }
                        }
                    }

                    if (workType == "READ")
                    {
                        onay = readData_(tForm, item);
                        islemOnayi = onay;
                    }

                    if (workType == "NEW" || workType == "ADDDATA")
                    {
                        onay = newDataExecute(tForm, TABLEIPCODE, null, v.tButtonType.btNone, workType);

                        if (v.con_OnaySave)
                        {
                            v.con_OnaySave = false;
                            tForm.HelpButton = false;
                        }
                        islemOnayi = onay;
                    }

                    if (workType == "GOTO")
                    {
                        onay = gotoData_(tForm, item);
                        islemOnayi = onay;
                    }

                    if (workType == "SVIEW")
                    {
                        // burayı açtığında  openSubView de tekrara düşebilirsin

                        onay = openSubView(tForm, TABLEIPCODE, prop_, buttonType);
                        onay = true;
                        islemOnayi = onay;
                    }

                    if (workType == "SVIEWVALUE")
                    { }

                    if (workType == "CREATEVIEW")
                    { }

                    if (workType == "SAVEDATA")
                    {
                        v.con_PositionChange = true;
                        v.con_ExtraChange = true;

                        if (t.IsNotNull(TABLEIPCODE))
                        {
                            tSave sv = new tSave();
                            onay = sv.tDataSave(tForm, TABLEIPCODE);
                            if (onay)
                            {
                                //t.ButtonEnabledAll(tForm, TABLEIPCODE, true);
                            }
                            islemOnayi = onay;
                        }

                        v.con_PositionChange = false;
                        v.con_ExtraChange = false;
                    }

                    if (workType == "SETDATA")
                    {
                        if (t.IsNotNull(item.RTABLEIPCODE) == false) item.RTABLEIPCODE = prop_.READ_TABLEIPCODE;
                        if (t.IsNotNull(item.TABLEIPCODE) == false) item.TABLEIPCODE = prop_.TARGET_TABLEIPCODE;

                        onay = readAndSetData_(tForm, item);
                        islemOnayi = onay;
                    }

                    if (workType == "SETFOCUS")
                    {
                        TABLEIPCODE = t.Set(item.TABLEIPCODE.ToString(), "", "");
                        KEYFNAME = t.Set(item.KEYFNAME.ToString(), "", "");
                        //t.tFormActiveControl(tForm, TABLEIPCODE, "Column_", KEYFNAME);
                        if (t.IsNotNull(TABLEIPCODE) && t.IsNotNull(KEYFNAME))
                        {
                            v.con_SetFocus = true;
                            v.con_SetFocus_TableIPCode = TABLEIPCODE;
                            v.con_SetFocus_FieldName = KEYFNAME;
                            onay = true;
                            islemOnayi = true;
                        }
                    }

                    // RDC - RunDataCopy
                    if (workType == "RDC")
                    {
                        string DataCopyCode = t.Set(item.DCCODE.ToString(), "", "");

                        if (t.IsNotNull(DataCopyCode))
                        {
                            tDataCopy dc = new tDataCopy();
                            v.con_DragDropEdit = true;
                            dc.tDC_Run(tForm, DataCopyCode);
                            v.con_DragDropEdit = false;
                            onay = true;
                            islemOnayi = true;
                        }
                        else
                        {
                            MessageBox.Show("DİKKAT : [DataCopy Code] tanımsız...");
                        }
                    }

                    // RPRC, 'Run StoredProcedure'
                    if (workType == "RPRC")
                    {
                        onay = runStoredProcedure(tForm, item, prop_);
                        islemOnayi = onay;
                    }
                    // RFORMUL, 'Run Formul'
                    if (workType == "RFORMUL")
                    {
                        onay = runFormul(tForm, item, prop_);
                        islemOnayi = onay;
                    }
                    if (workType == "OPENFORM") 
                    {
                       if (istekbuttonType != v.tButtonType.btKartAc)
                       {
                            t.OpenForm_JSON(tForm, prop_);
                            islemOnayi = true;
                       }
                    }

                    if (workType == "CLOSEFORM")
                    {
                        if (onay)
                            tForm.Close();
                    }

                    if (workType == "CLOSESVIEW")
                    {
                        if (onay)
                            t.tRemoveTabPagesForNewData(tForm);
                    }
                    
                    if (workType == "IBOX")
                    {
                        onay = InputBox(tForm, TABLEIPCODE, item, null);
                        if (onay == false)
                        {
                            break;
                        }
                    }

                    if (workType == "NUMBERDISPLAY")
                    {
                        ReadNumberDidplay(tForm, item);
                    }

                    if (workType == "CLEARDATA")
                    {
                        if (t.IsNotNull(TABLEIPCODE))
                        {
                            DataSet ds = t.Find_DataSet(tForm, "", TABLEIPCODE, "");

                            if (ds != null)
                            {
                                //dsData.Tables.RemoveAt(0); bu olmaz tablonun kendisini yok ediyor
                                ds.Tables[0].Clear();  // Bu sadece dataları siliyor
                            }
                            else onay = false;
                        }
                        else onay = false;
                        islemOnayi = onay;
                    }

                    if (workType == "MESSAGESHOW")
                    {
                        onay = messageBoxShow_(tForm, item);
                        islemOnayi = onay;
                    }
                    
                    if (workType == "QUESTION")
                    {
                        onay = questionShow_(tForm, item, ref islemOnayi);
                        if (onay == false)
                        {
                            break;
                        }
                    }

                    // birden çok işlem yapıpılırken eğer birinden olumsuz dönüş olursa işlem kesiliyor
                    if (onay == false)
                    {
                        //t.AlertMessage("UYARI : ", "İşleminiz iptal edilidi...");
                        return onay;
                    }
                
                }// beforeAfter

                #endregion
            }

            return onay;
        }

        #endregion extraIslem ler

        #region openSubView

        // SubView işlemi 3 değişik şekilde çalışmakta
        // 1. PROB_SUBVIEW   : DataNavigator Change üzerinde çalışıyor
        // 2. Buton Click te : PROP_NAVIGATOR üzerinde  tButtonType.btOpenSubView = 125 şeklinde
        // 3. Yine Button Click ExtraIslemler için çalışmaktadır : PROP_NAVIGATOR.TABLEIPCODE_LIST.WORKTYPE üzerinde 

        public void tSubView_(Form tForm,string Prop_Navigator, string selectItemValue, string caption, string MenuValue)
        {
            MessageBox.Show("tSubView : Burada eksiğin var..");
        }

        private bool openSubView(Form tForm, string tableIPCode, PROP_NAVIGATOR prop_, v.tButtonType buttonType)
        {
            //tToolBox t = new tToolBox();

            bool onay = false;
            string selectItemValue = "";
            string MenuValue = "";
            string caption = "";
            string itemCaption = "";

            string TableIPCode = tableIPCode;
            string TableAlias = string.Empty;
            string KeyFName = string.Empty;
            string workType = string.Empty;
            string formCode = string.Empty;
            string tabPageCode = string.Empty;
            string controlName = "TabPage";

            bool isChecked = t.tWorkingCheck(tForm, prop_, tableIPCode);

            // form açılması için onaylandı ise
            if (isChecked)
            {
                // bir tab panelin içine bir IP yerleştirmek yerine 
                // birform tasarımını yerleştirebiliriz
                // yani tab panel içine birden fazla IP yerleştirilmiş olur
                formCode = prop_.FORMCODE;
                // buton üzerinde tanımlanmışsa onu bas
                if (t.IsNotNull(prop_.CAPTION))
                    caption = prop_.CAPTION;

                foreach (TABLEIPCODE_LIST item in prop_.TABLEIPCODE_LIST)
                {
                    workType = item.WORKTYPE.ToString();

                    // TabPage, TabPage2 veya TabPage3 
                    if (t.IsNotNull(item.CONTROLNAME))
                        controlName = item.CONTROLNAME;

                    if (workType == "SVIEW")
                    {
                        itemCaption = item.CAPTION.ToString();
                        TableIPCode = item.TABLEIPCODE.ToString();
                        TableAlias = item.TABLEALIAS.ToString();
                        KeyFName = item.KEYFNAME.ToString();

                        if (t.IsNotNull(itemCaption))
                            caption = itemCaption;

                        // tLayout_xx_xx ile direk bir page nin namesi ile sayfanın setfocus olması sağlanabilir
                        if (t.IsNotNull(TableIPCode))
                        {
                            if (TableIPCode.IndexOf("tLayout") > -1)
                            {
                                tabPageCode = TableIPCode;
                                TableIPCode = "null";
                            }
                        }

                        if (t.IsNotNull(formCode))
                        {
                            if (formCode.IndexOf("tLayout") > -1)
                            {
                                tabPageCode = formCode;
                                formCode = "null";
                            }
                        }

                        if ((t.IsNotNull(TableIPCode) == false) &&
                            (t.IsNotNull(formCode) == false) &&
                            (t.IsNotNull(tabPageCode) == false))
                        {
                            MessageBox.Show("DİKKAT : SVIEW için ( TargetTableIPCode ), ( FormCode) or ( tLayout_xxx )  gerekiyor...");
                            return false;
                        }
                        else
                        {
                            // TableIPCode var ise
                            if (t.IsNotNull(TableIPCode))
                            {
                                if (TableAlias.IndexOf("[") == -1)
                                    TableAlias = "[" + TableAlias + "]";

                                //-----
                                // 1. Tablo
                                string myFormLoadValue = string.Empty;
                                t.MyProperties_Set(ref myFormLoadValue, v.FormLoad, "BEGIN", string.Empty);
                                t.MyProperties_Set(ref myFormLoadValue, v.FormLoad, "TargetTableIPCode", TableIPCode);
                                t.MyProperties_Set(ref myFormLoadValue, v.FormLoad, "TargetTableAlias", TableAlias);
                                t.MyProperties_Set(ref myFormLoadValue, v.FormLoad, "TargetFieldName", KeyFName);
                                t.MyProperties_Set(ref myFormLoadValue, v.FormLoad, "TargetValue", selectItemValue);
                                // 2. Tablo
                                //t.MyProperties_Set(ref myFormLoadValue, v.FormLoad, "SecondTableIPCode", TableIPCode);
                                //t.MyProperties_Set(ref myFormLoadValue, v.FormLoad, "SecondTableAlias", TableAlias);
                                //t.MyProperties_Set(ref myFormLoadValue, v.FormLoad, "SecondFieldName", "AHESAP_ID");
                                //t.MyProperties_Set(ref myFormLoadValue, v.FormLoad, "SecondValue", "1");
                                t.MyProperties_Set(ref myFormLoadValue, v.FormLoad, "END", string.Empty);

                                v.con_FormLoadValue = myFormLoadValue;

                                ev.subViewExec(tForm, controlName, "", TableIPCode, "", selectItemValue, caption, MenuValue);
                            }
                            // formCode var ise
                            if (t.IsNotNull(formCode))
                            {
                                // bu kontrole rapor formunu açarken ihtiyaç duyuldu
                                // rapor formunu create olduktan sonra raporların listesini gösteren tableIPCode yukarıda çalışyor
                                // burada tekrar çalışyor ve çift görüntü oluşuyor
                                if (buttonType != v.tButtonType.btKartAc) 
                                    ev.subViewExec(tForm, controlName, formCode, "", "", "", caption, MenuValue);
                            }
                            // tabPageCode var ise
                            if (t.IsNotNull(tabPageCode))
                            {
                                ev.subViewExec(tForm, controlName, "", "", tabPageCode, "", caption, MenuValue);
                            }
                        }
                    }
                }
            }

            return onay;
        }

        private bool openSubView(Form tForm, string tableIPCode, List<PROP_NAVIGATOR> propList_, v.tButtonType buttonType) 
        {
            //tToolBox t = new tToolBox();

            string selectItemValue = "";
            string MenuValue = "";
            string caption = "";

            bool onay = false;
            bool isChecked = true;

            if (propList_ != null)
                onay = extraIslemVar(tForm, tableIPCode, buttonType, v.tBeforeAfter.Before, propList_); 

            //---------

            string TableIPCode = string.Empty;
            string TableAlias = string.Empty;
            string KeyFName = string.Empty;
            string workType = string.Empty;
            string formCode = string.Empty;
            string tabPageCode = string.Empty;
            string controlName = "TabPage";

            foreach (PROP_NAVIGATOR prop_ in propList_)
            {
                if (prop_.BUTTONTYPE.ToString() == Convert.ToString((byte)buttonType))
                {
                    isChecked = t.tWorkingCheck(tForm, prop_, tableIPCode);

                    // form açılması için onaylandı ise
                    if (isChecked)
                    {
                        // bir tab panelin içine bir IP yerleştirmek yerine 
                        // birform tasarımını yerleştirebiliriz
                        // yani tab panel içine birden fazla IP yerleştirilmiş olur
                        formCode = prop_.FORMCODE;
                        // buton üzerinde tanımlanmışsa onu bas
                        if (t.IsNotNull(prop_.CAPTION))
                            caption = prop_.CAPTION;

                        foreach (TABLEIPCODE_LIST item in prop_.TABLEIPCODE_LIST)
                        {
                            workType = item.WORKTYPE.ToString();

                            // TabPage, TabPage2 veya TabPage3 
                            if (t.IsNotNull(item.CONTROLNAME))
                                controlName = item.CONTROLNAME;

                            if (workType == "SVIEW")
                            {
                                TableIPCode = item.TABLEIPCODE.ToString();
                                TableAlias = item.TABLEALIAS.ToString();
                                KeyFName = item.KEYFNAME.ToString();

                                // tLayout_xx_xx ile direk bir page nin namesi ile sayfanın setfocus olması sağlanabilir
                                if (t.IsNotNull(TableIPCode))
                                {
                                    if (TableIPCode.IndexOf("tLayout") > -1)
                                    {
                                        tabPageCode = TableIPCode;
                                        TableIPCode = "null";
                                    }
                                }

                                if (t.IsNotNull(formCode))
                                {
                                    if (formCode.IndexOf("tLayout") > -1)
                                    {
                                        tabPageCode = formCode;
                                        formCode = "null";
                                    }
                                }

                                if ((t.IsNotNull(TableIPCode) == false) &&
                                    (t.IsNotNull(formCode) == false) &&
                                    (t.IsNotNull(tabPageCode) == false))
                                {
                                    MessageBox.Show("DİKKAT : SVIEW için ( TargetTableIPCode ), ( FormCode) or ( tLayout_xxx )  gerekiyor...");
                                    return false;
                                }
                                else
                                {
                                    // TableIPCode var ise
                                    if (t.IsNotNull(TableIPCode))
                                    {
                                        if (TableAlias.IndexOf("[") == -1)
                                            TableAlias = "[" + TableAlias + "]";

                                        //-----
                                        // 1. Tablo
                                        string myFormLoadValue = string.Empty;
                                        t.MyProperties_Set(ref myFormLoadValue, v.FormLoad, "BEGIN", string.Empty);
                                        t.MyProperties_Set(ref myFormLoadValue, v.FormLoad, "TargetTableIPCode", TableIPCode);
                                        t.MyProperties_Set(ref myFormLoadValue, v.FormLoad, "TargetTableAlias", TableAlias);
                                        t.MyProperties_Set(ref myFormLoadValue, v.FormLoad, "TargetFieldName", KeyFName);
                                        t.MyProperties_Set(ref myFormLoadValue, v.FormLoad, "TargetValue", selectItemValue);
                                        // 2. Tablo
                                        //t.MyProperties_Set(ref myFormLoadValue, v.FormLoad, "SecondTableIPCode", TableIPCode);
                                        //t.MyProperties_Set(ref myFormLoadValue, v.FormLoad, "SecondTableAlias", TableAlias);
                                        //t.MyProperties_Set(ref myFormLoadValue, v.FormLoad, "SecondFieldName", "AHESAP_ID");
                                        //t.MyProperties_Set(ref myFormLoadValue, v.FormLoad, "SecondValue", "1");
                                        t.MyProperties_Set(ref myFormLoadValue, v.FormLoad, "END", string.Empty);

                                        v.con_FormLoadValue = myFormLoadValue;

                                        onay = ev.subViewExec(tForm, controlName, "", TableIPCode, "", selectItemValue, caption, MenuValue);
                                    }
                                    // formCode var ise
                                    if (t.IsNotNull(formCode))
                                    {
                                        onay = ev.subViewExec(tForm, controlName, formCode, "", "", "", caption, MenuValue);
                                    }
                                    // tabPageCode var ise
                                    if (t.IsNotNull(tabPageCode))
                                    {
                                        onay = ev.subViewExec(tForm, controlName, "", "", tabPageCode, "", caption, MenuValue);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //---------

            if ((onay) && (propList_ != null))
                onay = extraIslemVar(tForm, tableIPCode, buttonType, v.tBeforeAfter.After, propList_);

            return onay;
        }
        
        #endregion

        #region Search Engine textEdit_Find / Arama Motoru Events // orjinal yani önceki hali  tEvents içinde

        public void textEdit_Find_EditValueChanged(object sender, EventArgs e)
        {
            string findText = ((DevExpress.XtraEditors.TextEdit)sender).Text;
            if (findText == v.con_Search_NullText) findText = "";
            
            setFindFilterText(sender, findText);

            /// ( 100 * find )  neden bunu yapıyorum ?
            /// findDelay = 100 ise standart find
            /// findDelay = 200 ise list && data  yı işaret ediyor 
            //int findType = t.myInt32(((DevExpress.XtraEditors.TextEdit)sender).Tag.ToString());

            //int valueCount = findText.Length;

            //if ((findType == 200) &&
            //    (v.searchCount > 0) &&
            //    (v.searchCount > valueCount))
            //    InData_Close(tForm, TableIPCode);
        }

        private void notFoundMessage(Form tForm, string tableIPCode)
        {
            tEventsGrid evg = new tEventsGrid();
            vGridHint tGridHint = new vGridHint();

            /*
            Form tForm = null;
            string tableIPCode = "";
            Control cntrl = null;

            tForm = ((DevExpress.XtraEditors.TextEdit)sender).FindForm();
            tableIPCode = ((DevExpress.XtraEditors.TextEdit)sender).Properties.AccessibleDefaultActionDescription;
            */
            evg.getGridHint_(tForm, tableIPCode, ref tGridHint);

            /// Aranan kayıt yok ise, grid üzerinde focus olacak row yoktur
            /// 
            if (tGridHint.focusedRow == null)
            {
                if (v.tSearch.messageObj != null)
                    ((DevExpress.XtraEditors.LabelControl)v.tSearch.messageObj).Visible = true;

                v.tSearch.AutoSearch = false;
            }
            else
            {
                if (v.tSearch.messageObj != null)
                    ((DevExpress.XtraEditors.LabelControl)v.tSearch.messageObj).Visible = false;

                v.tSearch.AutoSearch = true;
            }

        }

        private void setFindFilterText(object sender, string findText)
        {
            if (((DevExpress.XtraEditors.TextEdit)sender).Properties.NullText == findText)
                findText = "";

            v.tSearch.searchOutputValue = findText;

            Control cntrl = null;
            Form tForm = ((DevExpress.XtraEditors.TextEdit)sender).FindForm();
            string tableIPCode = ((DevExpress.XtraEditors.TextEdit)sender).Properties.AccessibleDefaultActionDescription;
            cntrl = t.Find_Control_View(tForm, tableIPCode);

            setFindFilterText(cntrl, findText);

            notFoundMessage(tForm, tableIPCode);
        }
        private void setFindFilterText(Control cntrl, string findText)
        {
            if (cntrl != null)
            {
                if (cntrl.GetType().ToString() == "DevExpress.XtraGrid.GridControl")
                {
                    if (((DevExpress.XtraGrid.GridControl)cntrl).MainView.GetType().ToString() == "DevExpress.XtraGrid.Views.Grid.GridView")
                    {
                        GridView view = ((DevExpress.XtraGrid.GridControl)cntrl).MainView as GridView;
                        view.BeginDataUpdate();
                        view.FindFilterText = "\"" + findText + "\"";
                        //view.ApplyFindFilter("\"" + ((DevExpress.XtraEditors.TextEdit)sender).Text + "\"");
                        view.EndDataUpdate();
                    }

                    if (((DevExpress.XtraGrid.GridControl)cntrl).MainView.GetType().ToString() == "DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView")
                    {
                        AdvBandedGridView view = ((DevExpress.XtraGrid.GridControl)cntrl).MainView as AdvBandedGridView;
                        view.BeginDataUpdate();
                        view.FindFilterText = "\"" + findText + "\"";
                        view.EndDataUpdate();
                    }
                }
                if (cntrl.GetType().ToString() == "DevExpress.XtraTreeList.TreeList")
                {
                    ((DevExpress.XtraTreeList.TreeList)cntrl).FindFilterText = "\"" + findText + "\"";
                }
            }

        }

        public void textEdit_Find_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Basılan tuşu büyük harfe dönüştür
            e.KeyChar = char.ToUpper(e.KeyChar);
        }

        public void textEdit_Find_KeyUp(object sender, KeyEventArgs e)
        {
            ///
        }
        
        public void textEdit_Find_KeyDown(object sender, KeyEventArgs e)//***New Ok
        {
            tEventsGrid evg = new tEventsGrid();
            Form tForm = null;
            string tableIPCode = "";
            vGridHint tGridHint = new vGridHint();
            Control cntrl = null;

            if (t.findAttendantKey(e) ||
                t.findDirectionKey(e) ||
                t.findReturnKey(e))
            {
                tForm = ((DevExpress.XtraEditors.TextEdit)sender).FindForm();
                tableIPCode = ((DevExpress.XtraEditors.TextEdit)sender).Properties.AccessibleDefaultActionDescription;

                evg.getGridHint_(tForm, tableIPCode, ref tGridHint);
            }

            /// Enter veya görevli tuşlar çalışması için
            /// 
            if (t.findAttendantKey(e))
            {
                if (e.KeyCode == Keys.Enter)
                {
                    /// Search Engine : kullanıcı arama yaptı ve uygun data bulunmadıysa
                    /// işlem yapmasın
                    if (tGridHint.focusedRow == null && tGridHint.viewType == "GridView")
                    {
                        /// new Input Panel for Search Engine 
                        v.tSearch.searchOutputValue = ((DevExpress.XtraEditors.TextEdit)sender).EditValue.ToString();
                        v.tSearch.IsRun = false;
                        tForm.Dispose();
                        return;
                    }
                }

                vButtonHint tButtonHint = new vButtonHint();
                v.tButtonHint.Clear();
                v.tButtonHint.tForm = ((DevExpress.XtraEditors.TextEdit)sender).FindForm();
                v.tButtonHint.tableIPCode = ((DevExpress.XtraEditors.TextEdit)sender).Properties.AccessibleDefaultActionDescription;

                if (((DevExpress.XtraEditors.TextEdit)sender).OldEditValue != null)
                    v.tButtonHint.columnOldValue = ((DevExpress.XtraEditors.TextEdit)sender).OldEditValue.ToString();

                if (((DevExpress.XtraEditors.TextEdit)sender).EditValue != null)
                    v.tButtonHint.columnEditValue = ((DevExpress.XtraEditors.TextEdit)sender).EditValue.ToString();

                if ((e.KeyCode == Keys.Enter) || (e.KeyCode == Keys.Return))
                {
                    cntrl = t.Find_SimpleButton(tForm, "simpleButton_listeye_ekle", tableIPCode);
                    if (cntrl != null)  
                        v.tButtonHint.buttonType = v.tButtonType.btListeyeEkle;

                    if (cntrl == null)
                    {
                        cntrl = t.Find_SimpleButton(tForm, "simpleButton_sec", tableIPCode);
                        if (cntrl != null) 
                            v.tButtonHint.buttonType = v.tButtonType.btSecCik;
                    }
                }

                string propNavigator = t.getPropNavigator(v.tButtonHint.tForm, v.tButtonHint.tableIPCode);
                v.tButtonHint.propNavigator = propNavigator;
                v.tButtonHint.senderType = sender.GetType().ToString();
                
                //v.tButtonHint.Clear();
                //v.tButtonHint.tForm = tForm;
                //v.tButtonHint.tableIPCode = TableIPCode;
                //v.tButtonHint.propNavigator = myProp;
                //v.tButtonHint.buttonType = buttonType;
                //v.tButtonHint.columnEditValue = editValue;
                //v.tButtonHint.senderType = sender.GetType().ToString();
                //v.tButtonHint.checkedValue = editValue;
                
                btnClick(v.tButtonHint);

                return;
            }

            /// Gridin üzerinde yön tuşlarıyla gezmesini sağlamak için
            /// 
            if (t.findDirectionKey(e) ||
                t.findReturnKey(e))
            {
                // tSearch açıldığında otomatik find çalışsın diye iptal edildi
                //
                if (e.KeyCode == Keys.Home) return;
                if (e.KeyCode == Keys.End) return;

                if (tGridHint.view != null)
                {
                    if (t.findDirectionKey(e))
                        evg.gridSpeedKeys(tGridHint, e);

                    if (t.findReturnKey(e))
                    {
                        v.tSearch.searchInputValue = ((DevExpress.XtraEditors.TextEdit)sender).EditValue?.ToString();
                        evg.commonGridClick(sender, e, tGridHint);
                    }
                }
                /// Esc ile çıkış
                if (e.KeyCode == Keys.Escape)
                {
                    v.tSearch.searchOutputValue = ((DevExpress.XtraEditors.TextEdit)sender).EditValue?.ToString();
                    v.tSearch.IsRun = false;
                }

                return;
            }

            v.tSearch.searchInputValue = ((DevExpress.XtraEditors.TextEdit)sender).EditValue?.ToString();
        }

        public void textEdit_Find_Enter(object sender, EventArgs e)
        {
            int findType = 0;
            string findText = "";

            if (sender.GetType().ToString() == "DevExpress.XtraEditors.ButtonEdit")
            {
                findType = (int)((DevExpress.XtraEditors.ButtonEdit)sender).Tag;
                findText = ((DevExpress.XtraEditors.ButtonEdit)sender).Text;
                if (findText != v.con_Search_NullText)
                    v.searchCount = findText.Length;

                // create sırasında geçici yüklenmiş değer
                if (findType == -100)
                {
                    // şimdilik geçici çözüm
                    findType = 200;
                    ((DevExpress.XtraEditors.ButtonEdit)sender).Tag = 200;
                    //----
                }
            }
            if (sender.GetType().ToString() == "DevExpress.XtraEditors.TextEdit")
            {
                findType = (int)((DevExpress.XtraEditors.TextEdit)sender).Tag;
                findText = ((DevExpress.XtraEditors.TextEdit)sender).Text;

                ((DevExpress.XtraEditors.TextEdit)sender).Properties.Appearance.BackColor = v.AppearanceFocusedColor;

                if (((DevExpress.XtraEditors.TextEdit)sender).Text.Length > 0) 
                {
                    // cursorun sona yerleşmesi sağlanıyor
                    ((DevExpress.XtraEditors.TextEdit)sender).Select(100, 1);
                }

                setFindFilterText(sender, findText);

            }
            //if (findType == 100) v.search_CARI_ARAMA_TD = v.search_onList;
            //if (findType == 200) v.search_CARI_ARAMA_TD = v.search_inData;
        }

        public void textEdit_Find_Leave(object sender, EventArgs e)
        {
            if (sender.GetType().ToString() == "DevExpress.XtraEditors.TextEdit")
            {
                ((DevExpress.XtraEditors.TextEdit)sender).Properties.Appearance.BackColor =
                ((DevExpress.XtraEditors.TextEdit)sender).Properties.Appearance.BackColor2;
            }
        }

        public void toggleSwitch_Find_EditValueChanged(object sender, EventArgs e)
        {
            if (((DevExpress.XtraEditors.ToggleSwitch)sender).IsOn)
                v.search_CARI_ARAMA_TD = v.search_inData;
            else v.search_CARI_ARAMA_TD = v.search_onList;

            //if (((DevExpress.XtraEditors.ToggleSwitch)sender).IsOn)

            Form tForm = ((DevExpress.XtraEditors.ToggleSwitch)sender).FindForm();
            string TableIPCode = ((DevExpress.XtraEditors.ToggleSwitch)sender).Properties.AccessibleDefaultActionDescription;

            if (((DevExpress.XtraEditors.ToggleSwitch)sender).IsOn)
            {
                // v.search_inData
                InData_Close(tForm, TableIPCode);
            }
            else
            {
                // v.search_onList
                InData_RunSQL(tForm, TableIPCode, "<break>", null, null);
            }
        }

        public void InData_Close(Form tForm, string TableIPCode)
        {
            //tToolBox t = new tToolBox();

            if (t.IsNotNull(TableIPCode))
            {
                DataSet dsData = null;
                DataNavigator tDataNavigator = null;
                t.Find_DataSet(tForm, ref dsData, ref tDataNavigator, TableIPCode);

                if (dsData != null)
                {
                    dsData.Tables[0].Rows.Clear();// Clear();
                    v.searchCount = 0;
                }
            }
        }

        // GotoRecord   "ONdialog"  FOR SEARCH
        // SEARCH  için  GotoRecord  "ONdialog"  
        public void InData_RunSQL(Form tForm, string TableIPCode, string value,
            List<GET_FIELD_LIST> GetFieldList,
            List<TABLEIPCODE_LIST> TableIPCodeList)
        {
            //tToolBox t = new tToolBox();

            if (value.Length == 0)
            {
                //MessageBox.Show("Lütfen en azından bir harf girin ...");
                return;
            }
            if (value == "<break>") value = "";

            if (t.IsNotNull(TableIPCode))
            {
                DataSet dsData = null;
                DataNavigator tDataNavigator = null;
                t.Find_DataSet(tForm, ref dsData, ref tDataNavigator, TableIPCode);

                if (dsData != null)
                {
                    string myProp = dsData.Namespace.ToString();
                    string alias = t.MyProperties_Get(myProp, "=TableLabel:");
                    string find_FName = t.MyProperties_Get(myProp, "=FindFName:");
                    string DataFind = t.MyProperties_Get(myProp, "DataFind:");

                    t.Alias_Control(ref alias);

                    if (t.IsNotNull(find_FName) == false)
                    {
                        MessageBox.Show("DİKKAT : " + TableIPCode + " içindeki FindFName tanımlı değil !");
                        return;
                    }

                    if (t.IsNotNull(find_FName))
                        if (find_FName.IndexOf(alias) == -1)
                            find_FName = alias + find_FName;
                    // 
                    string newValue = "";
                    //"and " + find_FName + " like '%" + value + "%' ";

                    /// DataFind    0= Find yok, 1. standart, 2. List&Data   
                    if (DataFind == "2")
                        ev.External_Kriterleri_Uygula(tForm, dsData, alias, newValue, null);

                    v.searchCount = value.Length;

                    #region /// goto record edilecek kayıt mevcut

                    if (dsData.Tables[0].Rows.Count > 0)
                    {
                        //string T_FNAME = string.Empty;
                        string R_FNAME = string.Empty;
                        string MSETVALUE = string.Empty;
                        string WORKTYPE = string.Empty;

                        // aranacak fields ve value sayısı
                        int fc = 0; // TableIPCodeList.Count
                        int vc = 0; // value count
                        bool onay = false;

                        // arama listesinde bulunan kayıt sayısı
                        int rc = dsData.Tables[0].Rows.Count;
                        for (int i = 0; i < rc; i++)
                        {
                            /// aranan kayıt hakkındaki bilgileri oku ve
                            /// okunan tüm alanların eşit olmasını sağla ( ıd = xxx, Ad = yyyyy, ... ) gibi
                            vc = 0;
                            if (GetFieldList != null) fc = GetFieldList.Count;
                            if (TableIPCodeList != null) fc = TableIPCodeList.Count;

                            #region GetFieldList 
                            if (GetFieldList != null)
                            {
                                foreach (var item in GetFieldList)
                                {
                                    //T_FNAME = item.T_FNAME.ToString();
                                    R_FNAME = item.R_FNAME.ToString();       // arama listesindeki fieldin adı
                                    MSETVALUE = item.MSETVALUE.ToString();   // bu fieldin valuesi
                                                                             // Not : GetFieldList üzerinde birden fazla field ve value mevcut
                                                                             // bu fieldlerin ve value si eşit oluncaya kadar arama listesi taranır
                                                                             // cari listesinde birden fazla ( ad = tekin uçar ) olabilir 
                                                                             // doğru kaydı bulabilmek için ( id = xxx ) de aranır

                                    if (dsData.Tables[0].Rows[i][R_FNAME].ToString() == MSETVALUE)
                                    {
                                        vc++;
                                        if (fc == vc)
                                        {
                                            // aran kayıt bulundu, set oldu, işlem bitti

                                            // ama bu set çalışmıyor çünkü dialogFormda işe yaramıyor
                                            tDataNavigator.Position = i;
                                            tDataNavigator.Tag = i;

                                            // dialogForm için bu atama ile FormShown da yeniden set işlemi mevcut
                                            v.con_GotoRecord = "ONdialog";
                                            v.con_GotoRecord_Position = i;

                                            onay = true;

                                            break; // foreach e ait break
                                        }
                                    }// if
                                }// foreach
                            }//if not null
                            #endregion GetFieldList 

                            #region TableIPCodeList 
                            if (TableIPCodeList != null)
                            {
                                foreach (var item in TableIPCodeList)
                                {
                                    R_FNAME = item.RKEYFNAME.ToString();     // arama listesindeki fieldin adı
                                    MSETVALUE = item.MSETVALUE.ToString();   // bu fieldin valuesi
                                    WORKTYPE = item.WORKTYPE.ToString();     // Not : GetFieldList üzerinde birden fazla field ve value mevcut
                                                                             // bu fieldlerin ve value si eşit oluncaya kadar arama listesi taranır
                                                                             // cari listesinde birden fazla ( ad = tekin uçar ) olabilir 
                                                                             // doğru kaydı bulabilmek için ( id = xxx ) de aranır
                                                                             // listede setdata dan başka işlerde olabiliyor
                                    if (WORKTYPE != "SETDATA")
                                        fc--;

                                    if (dsData.Tables[0].Rows[i][R_FNAME].ToString() == MSETVALUE)
                                        vc++;

                                    if (fc == vc)
                                    {
                                        // aran kayıt bulundu, set oldu, işlem bitti
                                        // örnek: 3 field var, üçüde eşitlendiyse

                                        // ama bu set çalışmıyor çünkü dialogFormda işe yaramıyor
                                        tDataNavigator.Position = i;
                                        tDataNavigator.Tag = i;

                                        // dialogForm için bu atama ile FormShown da yeniden set işlemi mevcut
                                        v.con_GotoRecord = "ONdialog";
                                        v.con_GotoRecord_Position = i;

                                        onay = true;

                                        break; // foreach e ait break
                                    }


                                }// foreach
                            }//if not null
                            #endregion TableIPCodeList 

                            if (onay)
                            {
                                break; // if e ait break
                            }

                        }// for
                    }
                    #endregion
                }
            }
        }

        public void Search_New(Form tForm, string Search_TableIPCode, string Target_TableIPCode)
        {
            //tToolBox t = new tToolBox();

            if (t.IsNotNull(Target_TableIPCode))
            {
                //tEventsButton evb = new tEventsButton();

                DataSet ds = null;
                DataNavigator dN = null;
                t.Find_DataSet(tForm, ref ds, ref dN, Target_TableIPCode);

                if (ds != null)
                {

                    string state = t.Find_TableState(ds, dN, "", 0);
                    if (state != "dsInsert") return;

                    //
                    // Target ds de kullanılan mevcut yeni oluşan row'u sil
                    //
                    //NavigatorButton btn = dN.Buttons.Remove;
                    //dN.Buttons.DoClick(btn);
                    //ds.AcceptChanges();

                    /// bu Remove benim 1 günümü yedi, hatalı çalıştı
                    /// onun yerine Refreshle kurtuldum
                    t.TableRefresh(tForm, ds);

                    //
                    // Target ds de yeni row oluştur
                    //
                    ds.Tables[0].CaseSensitive = false;
                    newData(tForm, Target_TableIPCode);


                    //
                    // Arama Motoru Listesini Temizle 
                    //
                    v.searchCount = 0;
                    //InData_Close(tForm, Search_TableIPCode);
                    //   textEdit_Find_EditValueChanged gittiğinde bu fonk. zaten çalışıyor

                }
            }
        }

        #endregion Search Engine

        private void gridViewSetFirstColumn(Form tForm, string TableIPCode)
        {
            //tToolBox t = new tToolBox();
            Control viewCntrl = t.Find_Control_View(tForm, TableIPCode);

            if (viewCntrl != null)
            {
                tEventsGrid evg = new tEventsGrid();
                //
                v.formLastActiveControl = viewCntrl; // ((DevExpress.XtraGrid.GridControl)viewCntrl);

                if (viewCntrl.GetType().ToString() == "DevExpress.XtraGrid.GridControl")
                {
                    object view = ((DevExpress.XtraGrid.GridControl)viewCntrl).MainView as object;
                    //DevExpress.XtraGrid.Views.Card.CardView

                    vGridHint tGridHint = new vGridHint();
                    evg.getGridHint_(view, ref tGridHint);
                    evg.gridViewSetFirstColumn(view, tGridHint);
                    t.tFormActiveControl(tForm, viewCntrl);
                }
            }           
        }

    }//tEventsButton : tBase

}
