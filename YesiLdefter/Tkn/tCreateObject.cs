using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.WinExplorer;
using DevExpress.XtraVerticalGrid;
using DevExpress.XtraVerticalGrid.Rows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Tkn_DefaultValue;
using Tkn_DevColumn;
using Tkn_DevView;
using Tkn_Events;
using Tkn_InputPanel;
using Tkn_SQLs;
using Tkn_TablesRead;
using Tkn_ToolBox;
using Tkn_Variable;

namespace Tkn_CreateObject
{
    public class tCreateObject : tBase
    {
        tToolBox t = new tToolBox();

        #region Create_Navigator

        public void Create_Navigator(Form tForm, Control tPanelControl,
                                     DataRow row_Table, DataSet dsData, DataSet ds_Fields,
                                     string TableIPCode, string External_TableIPCode)
        {
            tToolBox t = new tToolBox();
            tEvents ev = new tEvents();

            #region Tanımlar

            int RefID = t.Set(row_Table["REF_ID"].ToString(), "", 0);
            string TableName = t.Set(row_Table["LKP_TABLE_NAME"].ToString(), "", "");
            string navigator = t.Set(row_Table["NAVIGATOR"].ToString(), "", "");
            byte DBaseNo = t.Set(row_Table["LKP_DBASE_TYPE"].ToString(), "", (byte)3);

            string Key_FName = t.Set(row_Table["LKP_KEY_FNAME"].ToString(), "", "");
            string Master_Key_FName = t.Set(row_Table["MASTER_KEY_FNAME"].ToString(), row_Table["LKP_MASTER_KEY_FNAME"].ToString(), "");
            string Foreing_FName = t.Set(row_Table["FOREING_FNAME"].ToString(), row_Table["LKP_FOREING_FNAME"].ToString(), "");
            string Parent_FName = t.Set(row_Table["PARENT_FNAME"].ToString(), row_Table["LKP_PARENT_FNAME"].ToString(), "");

            string Prop_Navigator = t.Set(row_Table["PROP_NAVIGATOR"].ToString(), "", "");
            string Prop_Search = t.Set(row_Table["PROP_SEARCH"].ToString(), "", "");
            string Prop_Views = t.Set(row_Table["PROP_VIEWS"].ToString(), "", "");
            string SubView = t.Set(row_Table["PROP_SUBVIEW"].ToString(), row_Table["LKP_PROP_SUBVIEW"].ToString(), "");

            // sonuna yapılıyor, bu bilgi  simpleButton_yeni_alt_hesap  için gerekli 
            t.MyProperties_Set(ref Prop_Navigator, "KEY_FNAME", Key_FName);
            t.MyProperties_Set(ref Prop_Navigator, "MASTER_KEY_FNAME", Master_Key_FName);
            t.MyProperties_Set(ref Prop_Navigator, "FOREING_FNAME", Foreing_FName);
            t.MyProperties_Set(ref Prop_Navigator, "PARENT_FNAME", Parent_FName);


            // SubView False ise
            // if (SubView.IndexOf("=SV_ENABLED:TRUE;") == -1) SubView = string.Empty;

            string value = string.Empty;

            //  "SUBVIEW_ENABLED": "TRUE",
            string s2 = (char)34 + "SUBVIEW_ENABLED" + (char)34 + ": " + (char)34 + "TRUE" + (char)34;
                                    
            //if (SubView.IndexOf(s2) > -1)
            //    value = t.Find_Properties_Value(SubView, "SV_ENABLED");
            //if (value != "TRUE") SubView = string.Empty;

            if (SubView.IndexOf(s2) == -1) 
                SubView = string.Empty;


            /// DATA_READ_TYPE
            /// 1, '', 'Not Read Data'
            /// 2, '', 'Read RefID'
            /// 3, '', 'Detail Table'
            /// 4, '', 'SubDetail Table'
            /// 5, '', 'Data Collection'
            /// 6, '', 'Read SubView'
            int Data_Read_Type = t.Set(row_Table["DATA_READ"].ToString(), "", (byte)1);


            #endregion Tanımlar

            #region DataNavigator

            /// daha sonra datanavigator List için çekildiğinde bunları create sırasıyla gelmesi için
            /// oluşturulması sırasına göre Name oluşturuluyor, 
            /// isim sıralaması 10 sonra başlaması için  +11 verilmiştir 
            List<string> list = new List<string>();
            t.Find_DataNavigator_List(tForm, ref list);
            int dNCount = list.Count + 11;

            DevExpress.XtraEditors.DataNavigator tDataNavigator = new DevExpress.XtraEditors.DataNavigator();
            tDataNavigator.Visible = true;
            tDataNavigator.Width = 80;
            //tDataNavigator.Name = "tDataNavigator_" + RefID.ToString();
            tDataNavigator.Name = "tDataNavigator_" + dNCount.ToString();
            if (dsData != null)
            {
                if (dsData.Tables.Count > 0)
                {
                    tDataNavigator.DataSource = dsData.Tables[0];

                    // Add a RowChanged event handler.
                    dsData.Tables[0].RowChanged += new DataRowChangeEventHandler(ev.tRow_Changed);

                    // Add a RowChanging event handler.
                    dsData.Tables[0].RowChanging += new DataRowChangeEventHandler(ev.tRow_Changing);

                    // Add a RowDeleted event handler.
                    dsData.Tables[0].RowDeleted += new DataRowChangeEventHandler(ev.tRow_Deleted);

                    // Add a RowDeleting event handler.
                    dsData.Tables[0].RowDeleting += new DataRowChangeEventHandler(ev.tRow_Deleting);

                    // Add a ColumnChanged event handler.
                    dsData.Tables[0].ColumnChanged += new
                        DataColumnChangeEventHandler(ev.tColumn_Changed);

                    // Add a ColumnChanging event handler.
                    dsData.Tables[0].ColumnChanging += new
                        DataColumnChangeEventHandler(ev.tColumn_Changing);

                    // Add a TableNewRow event handler.
                    dsData.Tables[0].TableNewRow += new
                        DataTableNewRowEventHandler(ev.tTable_NewRow);

                    // Add a TableCleared event handler.
                    dsData.Tables[0].TableCleared += new
                        DataTableClearEventHandler(ev.tTable_Cleared);

                    // Add a TableClearing event handler.
                    dsData.Tables[0].TableClearing += new
                        DataTableClearEventHandler(ev.tTable_Clearing);


                    preparingMaliDonemChanger(dsData, tDataNavigator, t);
                }
            
                if (dsData.Tables.Count == 0)
                {
                    MessageBox.Show("DİKKAT : " + TableIPCode + v.ENTER + "Create_Navigator sırasında dsData.Tables.Count == 0 geliyor. Bu nedenle DataSet e ulaşamazsın. Muhakkak tablo oluşturulmalı...");
                }
            }

            //tDataNavigator.TextStringFormat = "Kayıt {0}, {1}";
            tDataNavigator.TextStringFormat = "{0}, {1}";
            tDataNavigator.TextLocation = NavigatorButtonsTextLocation.Begin;
            tDataNavigator.PositionChanged += new System.EventHandler(ev.dataNavigator_PositionChanged);
            tDataNavigator.ButtonClick += new DevExpress.XtraEditors.NavigatorButtonClickEventHandler(ev.tdataNavigator_ButtonClick);
            tDataNavigator.Dock = DockStyle.Right;
            tDataNavigator.TabStop = false;
            tDataNavigator.Enabled = false;
            
            //tDataNavigator.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Default;
            tDataNavigator.Appearance.BackColor = v.colorFocus;
            tDataNavigator.Appearance.Options.UseBackColor = false;

            tDataNavigator.Enter += new System.EventHandler(ev.dataNavigator_Enter);
            tDataNavigator.Leave += new System.EventHandler(ev.dataNavigator_Leave);

            tDataNavigator.Buttons.First.Visible = false;
            tDataNavigator.Buttons.Last.Visible = false;
            tDataNavigator.Buttons.Next.Visible = false;
            tDataNavigator.Buttons.NextPage.Visible = false;
            tDataNavigator.Buttons.Prev.Visible = false;
            tDataNavigator.Buttons.PrevPage.Visible = false;
            tDataNavigator.Buttons.Append.Visible = false;
            tDataNavigator.Buttons.CancelEdit.Visible = false;
            tDataNavigator.Buttons.EndEdit.Visible = false;
            tDataNavigator.Buttons.Remove.Visible = false;

            // Tablonu konumlandığı ID yi tutuyor, db.MyRecord() için gerekli
            tDataNavigator.Tag = 0;
            // runtime sırasında DataNavigator e ulaşmak için 
            tDataNavigator.AccessibleName = TableIPCode;
            // Tablonun Adını tutuyor, db.MyRecord() için gerekli
            tDataNavigator.AccessibleDefaultActionDescription = TableName;
            // Database in Türünü tutuyor, db.MyRecord() için gerekli
            tDataNavigator.AccessibleDescription = DBaseNo.ToString();

            #endregion DataNavigator

            #region SubView
            if (t.IsNotNull(SubView))
            {
                tDataNavigator.AccessibleDescription = SubView;
                tDataNavigator.IsAccessible = true;
            }
            #endregion SubView

            #region Button Paneli

            DevExpress.XtraEditors.PanelControl NavPanel = new DevExpress.XtraEditors.PanelControl();
            NavPanel.Name = "tPanel_Navigator_" + t.AntiStr_Dot(TableIPCode);// RefID.ToString();
            NavPanel.Height = 26;
            NavPanel.Dock = DockStyle.Bottom;
            NavPanel.SendToBack();
            NavPanel.TabIndex = 1000; //0;
            NavPanel.TabStop = true;// false;
            // navigator butonlarının gösterilmesi istenmiyor ise panelide gizleyelim
            if (navigator == "")
            {
                NavPanel.Height = 2; // Visible = false kullanma
            }

            #endregion Button Paneli

            #region Create Buttons

            Create_Navigator_Buttons(tForm, 
                                     NavPanel,
                                     tDataNavigator,
                                     ds_Fields,
                                     TableIPCode,
                                     External_TableIPCode,
                                     navigator,
                                     Prop_Navigator,
                                     Prop_Search,
                                     Data_Read_Type);

            #endregion Create Buttons

            //tPanelControl.BackColor = v.colorExit;
            tPanelControl.TabIndex = 0;
            tPanelControl.TabStop = true;
            tPanelControl.Controls.Add(NavPanel);
        }

        private void preparingMaliDonemChanger(DataSet dsData, DataNavigator tDataNavigator, tToolBox t)
        {
            string myProp = dsData.Namespace;

            if (myProp.IndexOf("MaliDonemChanger:") > -1 && myProp.IndexOf("MaliDonemChanger:null") == -1)
            {
                // gerekli bilgiler toplandıysa  
                string fieldName = t.MyProperties_Get(myProp, "MaliDonemChanger:");
                string s = "";
                t.MyProperties_Set(ref s, "MaliDonemChanger", fieldName);
                if (tDataNavigator.Text.IndexOf("MaliDonemChanger") == -1)
                {
                    tDataNavigator.IsAccessible = true;
                    tDataNavigator.Text = tDataNavigator.Text + s;
                }
            }
        }

        private void createDropDownButton(vNavigatorButton nButton)
        {
            int i = nButton.navigatorList.IndexOf(nButton.tabIndex.ToString() + " ");
            if (i == -1) i = nButton.navigatorList.IndexOf("[" + nButton.tabIndex.ToString() + "]");
            if (i > -1)
            {
                Int16 height = 23;

                using (tEvents ev = new tEvents())
                {
                    DevExpress.XtraEditors.DropDownButton dragDownButton_ek = new DevExpress.XtraEditors.DropDownButton();

                    dragDownButton_ek.Name = nButton.buttonName; 
                    dragDownButton_ek.Size = new System.Drawing.Size(nButton.width + 17, height);
                    dragDownButton_ek.Dock = nButton.dock; 
                    dragDownButton_ek.TabIndex = nButton.tabIndex; 
                    dragDownButton_ek.TabStop = false;
                    dragDownButton_ek.Text = ":.";
                    dragDownButton_ek.AccessibleName = nButton.TableIPCode;
                    if (nButton.events)
                        dragDownButton_ek.Click += new System.EventHandler(ev.btn_Navigotor_Click);

                    dragDownButton_ek.Enter += new System.EventHandler(ev.btn_Navigotor_Enter);
                    dragDownButton_ek.Leave += new System.EventHandler(ev.btn_Navigotor_Leave);

                    if (nButton.backColor != null)
                    {
                        dragDownButton_ek.Appearance.BackColor = nButton.backColor;   //System.Drawing.Color.LightGreen;
                        dragDownButton_ek.Appearance.Options.UseBackColor = true;
                        dragDownButton_ek.AppearancePressed.BackColor = System.Drawing.Color.LightGreen;
                        dragDownButton_ek.AppearancePressed.Options.UseBackColor = true;


                    }
                    nButton.navigatorPanel.Controls.Add(dragDownButton_ek);
                    Buttons_CaptionChange(dragDownButton_ek, nButton.navigatorList, "");
                }
            }
        }

        private void createCheckButton(vNavigatorButton nButton)
        {
            int i = nButton.navigatorList.IndexOf(nButton.tabIndex.ToString() + " ");
            if (i == -1) i = nButton.navigatorList.IndexOf("[" + nButton.tabIndex.ToString() + "]");
            if (i > -1)
            {
                Int16 height = 23;

                using (tEvents ev = new tEvents())
                {
                    DevExpress.XtraEditors.CheckButton checkButton_ek = new DevExpress.XtraEditors.CheckButton();

                    checkButton_ek.Name = nButton.buttonName; 
                    checkButton_ek.Size = new System.Drawing.Size(nButton.width, height);
                    checkButton_ek.Dock = nButton.dock;
                    checkButton_ek.TabIndex = nButton.tabIndex;
                    checkButton_ek.TabStop = false;
                    checkButton_ek.Text = ":.";
                    checkButton_ek.AccessibleName = nButton.TableIPCode;

                    if (nButton.buttonText.IndexOf("<") > -1)
                        checkButton_ek.AllowHtmlDraw = DevExpress.Utils.DefaultBoolean.True;

                    checkButton_ek.CheckedChanged += new System.EventHandler(ev.btn_CheckButton_CheckedChanged);

                    checkButton_ek.Enter += new System.EventHandler(ev.btn_Navigotor_Enter);
                    checkButton_ek.Leave += new System.EventHandler(ev.btn_Navigotor_Leave);

                    //if (nButton.backColor != null)
                    //{
                    checkButton_ek.Appearance.BackColor = System.Drawing.Color.AliceBlue;
                    checkButton_ek.Appearance.Options.UseBackColor = true;
                    checkButton_ek.Appearance.ForeColor = System.Drawing.Color.Black;
                    checkButton_ek.Appearance.Options.UseForeColor = true;
                    checkButton_ek.LookAndFeel.UseDefaultLookAndFeel = false;
                    //}
                    nButton.navigatorPanel.Controls.Add(checkButton_ek);
                    Buttons_CaptionChange(checkButton_ek, nButton.navigatorList, "");
                }
            }
        }

        private SimpleButton createNavigatorButton(vNavigatorButton nButton)
        {
            //Control navigatorPanel,
            //string navigatorList,
            //string TableIPCode,
            //string buttonName,
            //string buttonText,
            //Int16 tabIndex,
            //Int16 width,
            //DockStyle dock,
            //string imageName

            int i = nButton.navigatorList.IndexOf(nButton.tabIndex.ToString() + " ");

            if (i == -1) i = nButton.navigatorList.IndexOf("[" + nButton.tabIndex.ToString() + "]");
            
            // 71 varsa 72 de eklensin
            if (nButton.tabIndex == 72)
                i = nButton.navigatorList.IndexOf("[71]");
            // 73 varsa 74 de eklensin
            if (nButton.tabIndex == 74)
                i = nButton.navigatorList.IndexOf("[73]");

            if (i > -1)
            {
                Int16 height = 23;

                using (tEvents ev = new tEvents())
                {
                    DevExpress.XtraEditors.SimpleButton simpleButton = new DevExpress.XtraEditors.SimpleButton();

                    /// search ekranı açıldığında textEdit_Find'e focuslanması için Navigator butonlarının tabstopları false edildi
                    /// eğer tekrar açmak gerekirse true yap, 
                    /// Create_MyFindPanel_ için navigator butonlarını tekrar burada false yap
                    /// 
                    simpleButton.Name = nButton.buttonName; 
                    simpleButton.Size = new System.Drawing.Size(nButton.width + 10, height);
                    simpleButton.Dock = nButton.dock; 
                    simpleButton.TabIndex = nButton.tabIndex;
                    simpleButton.TabStop = nButton.tabStop;
                    //simpleButton.Text = simpleButton.TabIndex.ToString() +";" + nButton.buttonText;
                    simpleButton.Text = nButton.buttonText;

                    //if (nButton.buttonKey != "")
                    //    simpleButton.Text = nButton.buttonText + " " + nButton.buttonKey;

                    simpleButton.AccessibleName = nButton.TableIPCode;
                    simpleButton.AccessibleDescription = nButton.propNavigator;
                    simpleButton.AccessibleDefaultActionDescription = nButton.propSearch;
                    simpleButton.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
                    simpleButton.Image = t.Find_Glyph(nButton.imageName);  //("CLOSE16");
                    
                    if ((nButton.buttonText.IndexOf("<") > -1) ||
                        (nButton.buttonKey != ""))
                        simpleButton.AllowHtmlDraw = DevExpress.Utils.DefaultBoolean.True;

                    if ((nButton.events) &&
                        (nButton.tabIndex < 141)) // webScraping değilse
                        simpleButton.Click += new System.EventHandler(ev.btn_Navigotor_Click);

                    simpleButton.Enter += new System.EventHandler(ev.btn_Navigotor_Enter);
                    simpleButton.Leave += new System.EventHandler(ev.btn_Navigotor_Leave);

                    if (nButton.backColor != null)
                    {
                        //simpleButton.Appearance.BackColor = nButton.backColor;   
                        //simpleButton.Appearance.Options.UseBackColor = true; 

                        simpleButton.AppearanceHovered.BackColor = nButton.backColor;
                        simpleButton.AppearanceHovered.Options.UseBackColor = true;
                    }

                    Buttons_CaptionChange(simpleButton, nButton.navigatorList, nButton.buttonKey);

                    /// + butonundan önce nButton.navigatorPanel.Controls.Add  eklenmesi gerekiyor
                    if (nButton.tabIndex == 73)
                        preparigHizliOnay(nButton, simpleButton);

                    nButton.navigatorPanel.Controls.Add(simpleButton);

                    if (nButton.tabIndex == 75)
                        Create_PopupMenu_Add(nButton, simpleButton, ev);


                    return simpleButton;
                }
            
                
            
            }
            return null;
        }

        private void preparigHizliOnay(vNavigatorButton nButton, DevExpress.XtraEditors.SimpleButton simpleButton_)
        {
            /// Buraya gelmeden önce Buttons_CaptionChange uğradı 
            /// + > <uSinavaHazirmiTipiId> şeklinde değişti
            string caption = simpleButton_.Text;
            if (caption.Trim() != "+")
            {
                /// şimdi tekrar görünmesi gerekene çeviriyoruz
                simpleButton_.Text = "+ ";
                /// çalışmamız gerek fieldi tespit ediyoruz
                string tFieldName = caption.Replace("<", "");
                tFieldName = tFieldName.Replace(">", "");
                tFieldName = tFieldName.Trim();

                ImageComboBoxEdit tEdit_HizliOnay = new DevExpress.XtraEditors.ImageComboBoxEdit();
                tEdit_HizliOnay.AccessibleDescription = nButton.tForm.Name;
                tEdit_HizliOnay.Name = "tEdit_HizliOnay_" + tFieldName;
                tEdit_HizliOnay.Properties.AccessibleName = nButton.TableIPCode;
                tEdit_HizliOnay.EnterMoveNextControl = true;
                tEdit_HizliOnay.Dock = DockStyle.Left;
                tEdit_HizliOnay.Width = 120;
                //AddBildirimSablonlariList(tEdit_BildirimSablonlari, mesajKodu);
                //tEdit_HizliOnay.EditValueChanged += new System.EventHandler(ev.tEdit_BildirimSablonlari_EditValueChanged);

                DevExpress.XtraEditors.Controls.EditorButton tBtn = new DevExpress.XtraEditors.Controls.EditorButton();

                tBtn.Caption = tFieldName;

                tEdit_HizliOnay.Properties.Buttons.Add(tBtn);
                tEvents ev = new tEvents();
                tEdit_HizliOnay.ButtonClick += new
                   DevExpress.XtraEditors.Controls.ButtonPressedEventHandler(ev.ImageComboBoxEdit_HizliOnayButtonClick);

                tEdit_HizliOnay.Properties.Buttons[1].Kind = DevExpress.XtraEditors.Controls.ButtonPredefines.OK;

                DataRow row_Fields = t.getDataRow(nButton.ds_Fields, tFieldName);
                if (row_Fields != null)
                {
                    tDevColumn dc = new tDevColumn();
                    dc.XtraEditorsImageComboBox_Fill(tEdit_HizliOnay, row_Fields, "", 0);
                }

                nButton.navigatorPanel.Controls.Add(tEdit_HizliOnay);
            }
        }

        #region Bilidirim SMS, WhatsApp, e-Mail, Mobil App
        private void createNavigatorPopupEdit(vNavigatorButton nButton)
        {
            int i = nButton.navigatorList.IndexOf(nButton.tabIndex.ToString() + " ");
            
            if (i == -1) i = nButton.navigatorList.IndexOf("[" + nButton.tabIndex.ToString() + "]");
            
            //v.con_CreatePopupContainer = false;

            if (i > -1)
            {
                string mesajKodu = "";
                string workType = "";
                string readKeyFieldNames = "";
                getMessagePropNavigator(nButton.propNavigator, ref mesajKodu, ref workType, ref readKeyFieldNames);

                Int16 height = 23;

                tEvents ev = new tEvents();

                v.con_CreatePopupContainer = true;

                //
                // tableLayoutPanel1
                // 
                #region
                System.Windows.Forms.TableLayoutPanel tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
                tableLayoutPanel1.SuspendLayout();
                //tableLayoutPanel1.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;
                tableLayoutPanel1.ColumnCount = 8;
                tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));
                tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
                tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
                tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));
                tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 280F));
                tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
                tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
                tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));
                tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
                tableLayoutPanel1.Location = new System.Drawing.Point(2, 2);
                tableLayoutPanel1.Name = "tableLayoutPanel_" + i.ToString();
                tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(8);
                tableLayoutPanel1.RowCount = 13;
                tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
                tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
                tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
                tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
                tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
                tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
                tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F)); // 6 nolu satır
                tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F)); // 7
                tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F)); // 8
                tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F)); // 9
                tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F)); // 10
                tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F)); // 11

                tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
                tableLayoutPanel1.Size = new System.Drawing.Size(200, 200);
                tableLayoutPanel1.TabIndex = 0;
                tableLayoutPanel1.Visible = true;
                #endregion
                ///
                /// Bildirim componentleri
                /// 
                #region
                ///
                /// Bildirim onayları
                /// 
                SimpleButton simpleButtonOnayla = new SimpleButton();
                simpleButtonOnayla.AccessibleName = nButton.TableIPCode;
                simpleButtonOnayla.AccessibleDescription = nButton.tForm.Name;
                simpleButtonOnayla.Dock = DockStyle.Fill;
                simpleButtonOnayla.Text = "Listeyi onayla";
                simpleButtonOnayla.Click += new System.EventHandler(ev.btn_Navigator_MesajOnayi_Click);
                simpleButtonOnayla.Tag = true; // onay işlemi

                SimpleButton simpleButtonOnayIptal = new SimpleButton();
                simpleButtonOnayIptal.AccessibleName = nButton.TableIPCode;
                simpleButtonOnayIptal.AccessibleDescription = nButton.tForm.Name;
                simpleButtonOnayIptal.Dock = DockStyle.Fill;
                simpleButtonOnayIptal.Text = "Onayları kaldır";
                simpleButtonOnayIptal.Click += new System.EventHandler(ev.btn_Navigator_MesajOnayi_Click);
                simpleButtonOnayIptal.Tag = false; // iptal işlemi

                ///
                /// Bildirim şablonları
                ///
                LabelControl tLabel_Sablon = new DevExpress.XtraEditors.LabelControl();
                tLabel_Sablon.Text = "Bildirim şablonları";
                tLabel_Sablon.Dock = DockStyle.Fill;
                ImageComboBoxEdit tEdit_BildirimSablonlari = new DevExpress.XtraEditors.ImageComboBoxEdit();
                tEdit_BildirimSablonlari.AccessibleDescription = nButton.tForm.Name;
                tEdit_BildirimSablonlari.Name = "tEdit_BildirimSablonlari";
                //tEdit_BildirimSablonlari.Properties.AccessibleName = TableIPCode;
                tEdit_BildirimSablonlari.EnterMoveNextControl = true;
                tEdit_BildirimSablonlari.Dock = DockStyle.Fill;
                AddBildirimSablonlariList(tEdit_BildirimSablonlari, mesajKodu);
                tEdit_BildirimSablonlari.EditValueChanged += new System.EventHandler(ev.tEdit_BildirimSablonlari_EditValueChanged);

                ///
                /// Bildirim Kanal seçimi
                ///
                LabelControl tLabel_KanalType = new DevExpress.XtraEditors.LabelControl();
                tLabel_KanalType.Text = "Bildirim kanalı";
                tLabel_KanalType.Dock = DockStyle.Fill;
                ImageComboBoxEdit tEdit_BildirimKanali = new DevExpress.XtraEditors.ImageComboBoxEdit();
                tEdit_BildirimKanali.Name = "tEdit_BildirimKanali";
                //tEdit_BildirimKanali.Properties.AccessibleName = TableIPCode;
                tEdit_BildirimKanali.EnterMoveNextControl = true;
                tEdit_BildirimKanali.Dock = DockStyle.Fill;
                AddBildirimKanallariList(tEdit_BildirimKanali);

                ///
                /// Bildirim seçenekleri
                ///
                LabelControl tLabel_BildirimSecenekleri = new DevExpress.XtraEditors.LabelControl();
                tLabel_BildirimSecenekleri.Text = "Bildirim seçenekleri";
                tLabel_BildirimSecenekleri.Dock = DockStyle.Fill;
                RadioGroup tEdit_BildirimSecenekleri = new DevExpress.XtraEditors.RadioGroup();
                tEdit_BildirimSecenekleri.Name = "tEdit_BildirimSecenekleri";
                //tEdit_BildirimSecenekleri.Properties.AccessibleName = TableIPCode;
                tEdit_BildirimSecenekleri.EnterMoveNextControl = true;
                tEdit_BildirimSecenekleri.Dock = DockStyle.Fill;
                AddBildirimSecenekleri(tEdit_BildirimSecenekleri);
                tEdit_BildirimSecenekleri.SelectedIndex = 0;

                ///
                /// Bildirim tarihi
                /// 
                LabelControl tLabel_BildirimTarihi = new DevExpress.XtraEditors.LabelControl();
                tLabel_BildirimTarihi.Text = "Bildirim tarihi";
                tLabel_BildirimTarihi.Dock = DockStyle.Fill;
                DateEdit tEdit_BidirimTarihi = new DevExpress.XtraEditors.DateEdit();
                tEdit_BidirimTarihi.Name = "tEdit_BidirimTarihi";
                //tEdit_BidirimTarihi.Properties.AccessibleName = TableIPCode;
                tEdit_BidirimTarihi.Dock = DockStyle.Fill;
                tEdit_BidirimTarihi.EnterMoveNextControl = true;
                tEdit_BidirimTarihi.EditValue = v.BUGUN_TARIH;

                ///
                /// Bildirim Saati
                /// 
                LabelControl tLabel_BildirimSaati = new DevExpress.XtraEditors.LabelControl();
                tLabel_BildirimSaati.Text = "Bildirim saati";
                tLabel_BildirimSaati.Dock = DockStyle.Fill;
                TimeEdit tEdit_BildirimSaati = new DevExpress.XtraEditors.TimeEdit();
                tEdit_BildirimSaati.Name = "tEdit_BildirimSaati";
                //tEdit_BildirimSaati.Properties.AccessibleName = TableIPCode;
                tEdit_BildirimSaati.Dock = DockStyle.Fill;
                tEdit_BildirimSaati.EnterMoveNextControl = true;
                tEdit_BildirimSaati.EditValue = DateTime.Now.ToShortTimeString();

                ///
                /// Bildirim Metni
                /// 
                LabelControl tLabel_BildirimMetni = new DevExpress.XtraEditors.LabelControl();
                tLabel_BildirimMetni.Text = "Bildirim metni";
                tLabel_BildirimMetni.Dock = DockStyle.Fill;
                MemoEdit tEdit_BildirimMetni = new DevExpress.XtraEditors.MemoEdit();
                tEdit_BildirimMetni.Name = "tEdit_BildirimMetni";
                //tEdit_OzelBildirim.Properties.AccessibleName = TableIPCode;
                tEdit_BildirimMetni.Dock = DockStyle.Fill;
                tEdit_BildirimMetni.EnterMoveNextControl = true;
                tEdit_BildirimMetni.Properties.Appearance.Font = new Font(FontFamily.GenericMonospace, (float)9.25);
                /// 
                /// default gelen mesaj kodun içeriğini memoya bas
                /// 
                getBildirimSablonuContext(tEdit_BildirimMetni, mesajKodu);

                ///
                /// Bildirimleri hazırla
                /// 
                SimpleButton simpleButtonHazirla = new SimpleButton();
                simpleButtonHazirla.AccessibleName = nButton.TableIPCode;
                simpleButtonHazirla.AccessibleDescription = nButton.tForm.Name;
                simpleButtonHazirla.Text = "Bildirimleri hazırla / oluştur";
                simpleButtonHazirla.Dock = DockStyle.Fill;
                simpleButtonHazirla.Click += new System.EventHandler(ev.btn_Navigator_BildirimHazirla_Click);


                ///
                /// Bildirimleri gönder
                /// 
                SimpleButton simpleButtonGonder = new SimpleButton();
                simpleButtonGonder.AccessibleName = nButton.TableIPCode;
                simpleButtonGonder.AccessibleDescription = nButton.tForm.Name;
                simpleButtonGonder.Text = "Bildirimleri gönder";
                simpleButtonGonder.Dock = DockStyle.Fill;
                simpleButtonGonder.Click += new System.EventHandler(ev.btn_Navigator_BildirimGonder_Click);

                ///
                /// Yeni bildirim paketi
                /// 
                SimpleButton simpleButtonYeniBildirim = new SimpleButton();
                simpleButtonYeniBildirim.AccessibleName = nButton.TableIPCode;
                simpleButtonYeniBildirim.AccessibleDescription = nButton.tForm.Name;
                simpleButtonYeniBildirim.Text = "Yeni bildirim paketi";
                simpleButtonYeniBildirim.Dock = DockStyle.Fill;
                simpleButtonYeniBildirim.Click += new System.EventHandler(ev.btn_Navigator_YeniBildirim_Click);

                ///
                /// Bildirim Listeleri
                /// 
                SimpleButton simpleButtonBildirimListesi = new SimpleButton();
                simpleButtonBildirimListesi.AccessibleName = nButton.TableIPCode;
                simpleButtonBildirimListesi.AccessibleDescription = nButton.tForm.Name;
                simpleButtonBildirimListesi.Text = "Bildirim listeleri ...";
                simpleButtonBildirimListesi.Dock = DockStyle.Fill;
                simpleButtonBildirimListesi.Click += new System.EventHandler(ev.btn_Navigator_BildirimListeleri_Click);

                ///
                /// Kalan kontör sorgu butonu
                /// 
                SimpleButton simpleButtonKalanKontor = new SimpleButton();
                simpleButtonKalanKontor.AccessibleName = nButton.TableIPCode;
                simpleButtonKalanKontor.AccessibleDescription = nButton.tForm.Name;
                simpleButtonKalanKontor.Text = "Kalan kontör ?";
                simpleButtonKalanKontor.Dock = DockStyle.Fill;
                simpleButtonKalanKontor.Click += new System.EventHandler(ev.btn_Navigator_KalanKontorSorgusu_Click);

                LabelControl tLabel_KalanKontorSayisi = new DevExpress.XtraEditors.LabelControl();
                tLabel_KalanKontorSayisi.Name = "tLabel_KalanKontorSayisi";
                tLabel_KalanKontorSayisi.Text = "?";
                tLabel_KalanKontorSayisi.Dock = DockStyle.Fill;
                tLabel_KalanKontorSayisi.Appearance.Font = new Font(FontFamily.GenericMonospace, (float)14.25);



                /////// Bu bilgiler Manager.CrossSettings tablosundan okunacak 
                ///
                ///  UNUTMA BildirimListesiFormCode ataması ms_Bilidirimler formunda da var
                /// 
                /// bunlar gönderim sırasında kullanılan TableIPCodeler 
                string TableIPCodeHeader = "UST/PMS/CrsBildirimB.Kayit_F01";
                string TableIPCodeLines = "UST/PMS/CrsBildirimS.Kayit_F01";
                
                /// bunlar gönderilen bildirimleri listelendiği ayrıca açılan liste ekranı
                /// 
                string BildirimListesiFormCode = "UST/PMS/CRS/SmsBildirimleri";
                string BildirimListesiHeader = "UST/PMS/CrsBildirimB.List_01";
                string BildirimListesiLines = "UST/PMS/CrsBildirimS.List_01";

                simpleButtonHazirla.AccessibleDefaultActionDescription =
                    TableIPCodeHeader + "||" +
                    TableIPCodeLines + "||" +
                    BildirimListesiFormCode + "||" +
                    BildirimListesiHeader + "||" +
                    BildirimListesiLines + "||" +
                    workType + "||" +
                    readKeyFieldNames + "||";

                simpleButtonBildirimListesi.AccessibleDefaultActionDescription = 
                    simpleButtonHazirla.AccessibleDefaultActionDescription;
                simpleButtonKalanKontor.AccessibleDefaultActionDescription =
                    simpleButtonHazirla.AccessibleDefaultActionDescription;
                simpleButtonYeniBildirim.AccessibleDefaultActionDescription =
                    simpleButtonHazirla.AccessibleDefaultActionDescription;
                #endregion


                Control panel1 = prepraringCreateCrossBildirim(TableIPCodeHeader, nButton.tForm.Name);
                Control panel2 = prepraringCreateCrossBildirim(TableIPCodeLines, nButton.tForm.Name);

                //=SubDetail_TableIPCode:UST/PMS/CrsBildirimS.Kayit_F01;
                //DataSet dsBildirimB = null;
                //DataNavigator dNBildirimB = null;


                if (workType == "MESSAGESINGLE")
                {
                    simpleButtonOnayla.Enabled = false;
                    simpleButtonOnayIptal.Enabled = false;
                    simpleButtonHazirla.Text = "Bildirimi hazırla / oluştur";
                    simpleButtonGonder.Text = "Bildirimi gönder";
                }

                ///
                /// tableLayoutPanel1.Controls.Add
                /// 
                #region
                //if (colSpan > 0)
                //    ((System.Windows.Forms.TableLayoutPanel)c).SetColumnSpan(newControl, colSpan);
                //if (rowSpan > 0)
                //    ((System.Windows.Forms.TableLayoutPanel)c).SetRowSpan(newControl, rowSpan);
                /// Bildirim onayları
                tableLayoutPanel1.Controls.Add(simpleButtonOnayla, 1, 0);
                tableLayoutPanel1.Controls.Add(simpleButtonOnayIptal, 2, 0);
                /// Bildirim şablonları
                tableLayoutPanel1.Controls.Add(tLabel_Sablon, 1, 1);
                tableLayoutPanel1.Controls.Add(tEdit_BildirimSablonlari, 1, 2);
                tableLayoutPanel1.SetColumnSpan(tEdit_BildirimSablonlari, 2);
                /// Bildirim Kanal seçimi
                tableLayoutPanel1.Controls.Add(tLabel_KanalType, 1, 3);
                tableLayoutPanel1.Controls.Add(tEdit_BildirimKanali, 1, 4);
                tableLayoutPanel1.SetColumnSpan(tEdit_BildirimKanali, 2);
                /// Bildirim seçenekleri
                tableLayoutPanel1.Controls.Add(tLabel_BildirimSecenekleri, 1, 5);
                tableLayoutPanel1.Controls.Add(tEdit_BildirimSecenekleri, 1, 6);
                tableLayoutPanel1.SetColumnSpan(tEdit_BildirimSecenekleri, 2);
                /// Bildirim tarihi
                tableLayoutPanel1.Controls.Add(tLabel_BildirimTarihi, 1, 7);
                tableLayoutPanel1.Controls.Add(tEdit_BidirimTarihi, 1, 8);
                /// Bildirim Saati
                tableLayoutPanel1.Controls.Add(tLabel_BildirimSaati, 2, 7);
                tableLayoutPanel1.Controls.Add(tEdit_BildirimSaati, 2, 8);
                /// Kredi sorgu butonu
                tableLayoutPanel1.Controls.Add(simpleButtonKalanKontor, 1, 9);
                /// Kredi göstergesi
                tableLayoutPanel1.Controls.Add(tLabel_KalanKontorSayisi, 2, 9);


                /// Bildirim metni
                tableLayoutPanel1.Controls.Add(tLabel_BildirimMetni, 4, 1);
                tableLayoutPanel1.Controls.Add(tEdit_BildirimMetni, 4, 2);
                tableLayoutPanel1.SetRowSpan(tEdit_BildirimMetni, 7);
                /// Bildirimleri hazırla
                tableLayoutPanel1.Controls.Add(simpleButtonHazirla, 4, 9);
                /// Bildirimleri gönder
                tableLayoutPanel1.Controls.Add(simpleButtonGonder, 4, 10);
                /// Yeni Bildirim 
                tableLayoutPanel1.Controls.Add(simpleButtonYeniBildirim, 4, 11);
                /// Bildirimleri sorgula
                tableLayoutPanel1.Controls.Add(simpleButtonBildirimListesi, 4, 12);

                tableLayoutPanel1.Controls.Add(panel1, 6, 0);
                tableLayoutPanel1.SetRowSpan(panel1, 6);

                tableLayoutPanel1.Controls.Add(panel2, 6, 6);
                tableLayoutPanel1.SetRowSpan(panel2, 7);

                tableLayoutPanel1.ResumeLayout(false);
                #endregion


                // 
                //  DevExpress.XtraEditors.PopupContainerControl  açılan pencere
                // 
                #region
                DevExpress.XtraEditors.PopupContainerControl popupContainerControlMesaj = new DevExpress.XtraEditors.PopupContainerControl();
                ((System.ComponentModel.ISupportInitialize)(popupContainerControlMesaj)).BeginInit();
                popupContainerControlMesaj.SuspendLayout();

                popupContainerControlMesaj.Controls.Add(tableLayoutPanel1);
                popupContainerControlMesaj.Location = new System.Drawing.Point(0, 306);
                popupContainerControlMesaj.Name = "popupContainerControlSample";
                popupContainerControlMesaj.Padding = new System.Windows.Forms.Padding(4);
                popupContainerControlMesaj.Size = new System.Drawing.Size(1250, 450);
                popupContainerControlMesaj.TabIndex = 2;

                #endregion

                // 
                // DevExpress.XtraEditors.PopupContainerEdit -- penceriyi açan combo nesnesi, Navigotor panelinde görünen nesne
                // 
                #region

                DevExpress.XtraEditors.PopupContainerEdit popupContainerEditMesaj = new DevExpress.XtraEditors.PopupContainerEdit();
                ((System.ComponentModel.ISupportInitialize)(popupContainerEditMesaj.Properties)).BeginInit();

                popupContainerEditMesaj.EditValue = "Mesaj gönder";
                popupContainerEditMesaj.Location = new System.Drawing.Point(32, 150);
                popupContainerEditMesaj.Name = "popupContainerEditMesaj";
                popupContainerEditMesaj.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
        new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
                popupContainerEditMesaj.Properties.PopupControl = popupContainerControlMesaj;
                popupContainerEditMesaj.Size = new System.Drawing.Size(nButton.width, height);
                //popupContainerEditSample.StyleController = this.layoutControl1;
                popupContainerEditMesaj.TabIndex = 0;
                /// bunlar ile comboda görünmesini istediğin textleri ayarlayabiliyorsun
                /// popupContainerEditSample.QueryResultValue += new DevExpress.XtraEditors.Controls.QueryResultValueEventHandler(this.popupContainerEditSample_QueryResultValue);
                /// popupContainerEditSample.QueryPopUp += new System.ComponentModel.CancelEventHandler(this.popupContainerEditSample_QueryPopUp);
                popupContainerEditMesaj.BackColor = System.Drawing.Color.DeepSkyBlue;
                popupContainerEditMesaj.ForeColor = System.Drawing.Color.White;
                popupContainerEditMesaj.Dock = nButton.dock;

                ((System.ComponentModel.ISupportInitialize)(popupContainerEditMesaj.Properties)).EndInit();
                ((System.ComponentModel.ISupportInitialize)(popupContainerControlMesaj)).EndInit();
                popupContainerControlMesaj.ResumeLayout(false);
                #endregion

                nButton.navigatorPanel.Controls.Add(popupContainerEditMesaj);

            }
        }
        private Control prepraringCreateCrossBildirim(string TableIPCode, string formName)
        {
            //Form tForm = t.Find_Form(sender); Buton bir popupForm üzerinde 
            Form tForm = Application.OpenForms[formName];

            DevExpress.XtraEditors.GroupControl panelControl1 = new DevExpress.XtraEditors.GroupControl();
            ((System.ComponentModel.ISupportInitialize)(panelControl1)).BeginInit();
                        
            tInputPanel ip = new tInputPanel();
            ip.Create_InputPanel(tForm, panelControl1, TableIPCode, 1, false);
            
            panelControl1.Dock = DockStyle.Fill;

            if (TableIPCode.IndexOf("CrsBildirimB") > -1)
                panelControl1.Text = "Bildirim paketi hakkında";
            if (TableIPCode.IndexOf("CrsBildirimS") > -1)
                panelControl1.Text = "Detaylı bildirim listesi";

            return panelControl1;
        }
        private void getMessagePropNavigator(string propNavigator, ref string mesajKodu, ref string workType, ref string readKeyFieldNames)
        {
            //if (propNavigator.IndexOf("=KEY_FNAME:") > -1) return "";
            int i2 = propNavigator.IndexOf("=KEY_FNAME:");
            if (i2 > -1)
                propNavigator = propNavigator.Substring(0, i2);

            // yinede varsa kontrol 
            i2 = propNavigator.IndexOf("KEY_FNAME:");
            if (i2 > -1) return; 

                        
            PROP_NAVIGATOR prop_ = null;
            List<PROP_NAVIGATOR> propList_ = null;
            
            propNavigator = propNavigator.Replace((char)34, (char)39);
            if (propNavigator != "")
            {
                t.readProNavigator(propNavigator, ref prop_, ref propList_);
            }

            if(propList_ != null)
            {
                foreach (PROP_NAVIGATOR propItem_ in propList_)
                {
                    if (propItem_.BUTTONTYPE.ToString() == Convert.ToString((byte)v.tButtonType.btMesajGonder))
                    {
                        foreach (TABLEIPCODE_LIST item_ in propItem_.TABLEIPCODE_LIST)
                        {
                            workType = item_.WORKTYPE;
                            mesajKodu = item_.MSETVALUE;
                            readKeyFieldNames = item_.RKEYFNAME;
                            break;
                        }
                    }
                }
            }
        }

        void AddBildirimSablonlariList(ImageComboBoxEdit tEdit, string mesajKodu)
        {
            tSQLs Sqls = new tSQLs();
            DataSet ds = new DataSet();
            string tSql = Sqls.Sql_GetBildirimSablonlariList("");
            t.SQL_Read_Execute(v.dBaseNo.Manager, ds, ref tSql, "HubBildirimSablonlari", "HubBildirimSablonlari");

            Int16 bildirimNo = 1;
            if (t.IsNotNull(ds))
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    //tEdit.Properties.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem(row["MesajBasligi"].ToString() + "        [" + row["MesajKodu"].ToString() + "]", (int)row["Id"], -1));
                    tEdit.Properties.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem(row["MesajBasligi"].ToString(),  (string)row["MesajKodu"].ToString(), -1));
                    bildirimNo++;
                }
            }
                        
            if (bildirimNo == 1) 
               tEdit.Properties.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("Bildirim mesajları bulunamadı.", (string)"", -1));

            if (mesajKodu != "") tEdit.EditValue = (string)mesajKodu;
            else tEdit.EditValue = (string)"";
        }
        void AddBildirimKanallariList(ImageComboBoxEdit tEdit)
        {
            tSQLs Sqls = new tSQLs();
            DataSet ds = new DataSet();
            string tSql = Sqls.Sql_GetBildirimKanallariList("");
            t.SQL_Read_Execute(v.dBaseNo.Manager, ds, ref tSql, "HubBildirimKanallari", "HubBildirimKanallari");

            Int16 bildirimNo = 1;
            if (t.IsNotNull(ds))
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    //tEdit.Properties.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem(row["MesajBasligi"].ToString() + "        [" + row["MesajKodu"].ToString() + "]", (int)row["Id"], -1));
                    tEdit.Properties.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem(row["KanalType"].ToString(), (int)row["Id"], -1));
                    bildirimNo++;
                }
            }

            if (bildirimNo == 1) // kamera bulamadıysa
                tEdit.Properties.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("Bildirim kanalları bulunamadı.", (int)1, -1));

            tEdit.EditValue = (int)1;
        }
        void getBildirimSablonuContext(MemoEdit tEdit, string mesajKodu)
        {
            tSQLs Sqls = new tSQLs();
            DataSet ds = new DataSet();
            string tSql = Sqls.Sql_GetBildirimSablonlariList(" and MesajKodu = '" + mesajKodu + "' ");
            t.SQL_Read_Execute(v.dBaseNo.Manager, ds, ref tSql, "HubBildirimSablonlari", "HubBildirimSablonlari");

            string mesajContext = "";
            if (t.IsNotNull(ds))
            {
                mesajContext = ds.Tables[0].Rows[0]["Mesaj"].ToString();
            }
            tEdit.EditValue = mesajContext;
        }
        void AddBildirimSecenekleri(DevExpress.XtraEditors.RadioGroup tEdit_RG)
        {
            tEdit_RG.Properties.Items.Add(new DevExpress.XtraEditors.Controls.RadioGroupItem(Convert.ToInt16("1"), "Hemen gönder"));
            tEdit_RG.Properties.Items.Add(new DevExpress.XtraEditors.Controls.RadioGroupItem(Convert.ToInt16("2"), "Zamanlanmış gönderim (Tarih ve saat belirterek)"));
        }

        #endregion Bilidirim SMS, WhatsApp, e-Mail, Mobil App

        private void Create_PopupMenu_Add(vNavigatorButton nButton, DevExpress.XtraEditors.SimpleButton simpleButton, tEvents ev)
        {
            //BarManager existingBarManager = null;
            //foreach (Control control in nButton.tForm.Controls)
            //{
            //    if (control.GetType().ToString() == "DevExpress.XtraBars.BarManager")
            //    {
            //        existingBarManager = ((BarManager)control);
            //        break;
            //    }
            //}
            
            BarManager barManager = new BarManager(); // PopupMenu için bir BarManager gerekir
            barManager.Form = nButton.tForm;

            PopupMenu popupMenu = new PopupMenu();
            popupMenu.Manager = barManager;


            simpleButton.Click += ev.btn_Navigator_PopupMenu;

        }

        private void Create_Navigator_Buttons(
            Form tForm, 
            Control NavPanel, 
            DataNavigator tDataNavigator,
            DataSet ds_Fields,
            string TableIPCode,
            string External_TableIPCode,
            string navigator,
            string Prop_Navigator,
            string Prop_Search,
            int Data_Read_Type)
        {
            int i = -1;
            Int16 width25 = 28; // w1
            //Int16 width35 = 35; // w2
            Int16 width70 = 70; // width
            Int16 width80 = 80; // w3
            Int16 width90 = 90; // w3
            Int16 width100 = 100; // w4

            string xTableIPCode = TableIPCode;

            // kaydet ve yeni buttonları için gerekli
            if (t.IsNotNull(External_TableIPCode))
                xTableIPCode = External_TableIPCode;

            DevExpress.XtraEditors.SimpleButton simpleButton = null;

            vNavigatorButton nButton = new vNavigatorButton();
            nButton.tForm = tForm;
            nButton.ds_Fields = ds_Fields;
            nButton.navigatorPanel = NavPanel;
            nButton.TableIPCode = TableIPCode;
            nButton.navigatorList = navigator;
            nButton.propNavigator = Prop_Navigator;
            nButton.backColor = v.colorOrder;
            nButton.events = true;

            #region dragDownButton_ek, checkButton_ek, simpleButton_ek   91.92.101.111

            nButton.buttonName = "dragDownButton_ek1";
            nButton.buttonText = "<b>:.</b>";
            nButton.dock = DockStyle.Left;
            nButton.tabIndex = 111;
            nButton.tabStop = false;
            nButton.imageName = "";
            nButton.width = width90;
            nButton.events = false;
            createDropDownButton(nButton);

            nButton.buttonName = "checkButton_ek1";
            nButton.buttonText = "<b>:.</b>";
            nButton.dock = DockStyle.Left;
            nButton.tabIndex = 101;
            nButton.tabStop = false;
            nButton.imageName = "";
            nButton.width = width90;
            nButton.events = false;
            createCheckButton(nButton);

            nButton.buttonName = "simpleButton_ek7";
            nButton.buttonText = "<b>:.</b>";
            nButton.dock = DockStyle.Left;
            nButton.tabIndex = 97;
            nButton.tabStop = false;
            nButton.imageName = "";
            nButton.width = width90;
            nButton.events = true;
            createNavigatorButton(nButton);

            nButton.buttonName = "simpleButton_ek6";
            nButton.buttonText = "<b>:.</b>";
            nButton.dock = DockStyle.Left;
            nButton.tabIndex = 96;
            nButton.tabStop = false;
            nButton.imageName = "";
            nButton.width = width90;
            nButton.events = true;
            createNavigatorButton(nButton);

            nButton.buttonName = "simpleButton_ek5";
            nButton.buttonText = "<b>:.</b>";
            nButton.dock = DockStyle.Left;
            nButton.tabIndex = 95;
            nButton.tabStop = false;
            nButton.imageName = "";
            nButton.width = width90;
            nButton.events = true;
            createNavigatorButton(nButton);

            nButton.buttonName = "simpleButton_ek4";
            nButton.buttonText = "<b>:.</b>";
            nButton.dock = DockStyle.Left;
            nButton.tabIndex = 94;
            nButton.tabStop = false;
            nButton.imageName = "";
            nButton.width = width90;
            nButton.events = true;
            createNavigatorButton(nButton);

            nButton.buttonName = "simpleButton_ek3";
            nButton.buttonText = "<b>:.</b>";
            nButton.dock = DockStyle.Left;
            nButton.tabIndex = 93;
            nButton.tabStop = false;
            nButton.imageName = "";
            nButton.width = width90;
            nButton.events = true;
            createNavigatorButton(nButton);

            nButton.buttonName = "simpleButton_ek2";
            nButton.buttonText = "<b>:.</b>";
            nButton.dock = DockStyle.Left;
            nButton.tabIndex = 92;
            nButton.tabStop = false;
            nButton.imageName = "";
            nButton.width = width90;
            nButton.events = true;
            createNavigatorButton(nButton);

            nButton.buttonName = "simpleButton_ek1";
            nButton.buttonText = "<b>:.</b>";
            nButton.dock = DockStyle.Left;
            nButton.tabIndex = 91;
            nButton.tabStop = false;
            nButton.imageName = "";
            nButton.width = width90;
            nButton.events = false;
            createNavigatorButton(nButton);

            #endregion

            //--------------

            #region collapse, expand, onay, yazıcı butonları  71..81

            nButton.buttonName = "simpleButton_Coll";
            nButton.buttonText = "Kp";
            nButton.dock = DockStyle.Left;
            nButton.tabIndex = 71;
            nButton.tabStop = false;
            nButton.imageName = "30_319_AutoExpand_16x16";
            nButton.width = width25;
            nButton.events = true;
            createNavigatorButton(nButton);

            nButton.buttonName = "simpleButton_Exp";
            nButton.buttonText = "Aç";
            nButton.dock = DockStyle.Left;
            nButton.tabIndex = 72;
            nButton.tabStop = false;
            nButton.imageName = "30_319_AutoExpand_16x16";
            nButton.width = width25;
            nButton.events = true;
            createNavigatorButton(nButton);

            //simpleButton_collexp.GroupIndex = -1;
            // onayEkle
            // onayKaldir
            nButton.buttonName = "simpleButton_onay_Ekle";
            nButton.buttonText = "+";
            nButton.dock = DockStyle.Left;
            nButton.tabIndex = 73;
            nButton.tabStop = false;
            nButton.imageName = "";
            nButton.width = width25;
            nButton.events = true;
            createNavigatorButton(nButton);

            nButton.buttonName = "simpleButton_onay_Kaldir";
            nButton.buttonText = "-";
            nButton.dock = DockStyle.Left;
            nButton.tabIndex = 74;
            nButton.tabStop = false;
            nButton.imageName = "";
            nButton.width = width25;
            nButton.events = true;
            createNavigatorButton(nButton);

            nButton.buttonName = "simpleButton_mesaj";
            nButton.buttonText = "Mesaj gönder";
            nButton.dock = DockStyle.Left;
            nButton.tabIndex = 75;
            nButton.tabStop = false;
            nButton.imageName = "30_327_Send_16x16";
            nButton.width = width90;
            nButton.events = true;
            createNavigatorPopupEdit(nButton);

            nButton.buttonName = "simpleButton_yazici";
            nButton.buttonText = "Yazdır...";
            nButton.dock = DockStyle.Left;
            nButton.tabIndex = 81;
            nButton.tabStop = false;
            nButton.imageName = "30_338_Printer_16x16";
            nButton.width = width70;
            nButton.events = true;
            createNavigatorButton(nButton);

            nButton.buttonName = "simpleButton_rapor";
            nButton.buttonText = "Yazdır...";
            nButton.dock = DockStyle.Left;
            nButton.tabIndex = 82;
            nButton.tabStop = false;
            nButton.imageName = "30_338_Printer_16x16";
            nButton.width = width70;
            nButton.events = true;
            createNavigatorButton(nButton);


            nButton.buttonName = "simpleButton_arama";
            nButton.buttonText = "Arama...";
            nButton.dock = DockStyle.Left;
            nButton.tabIndex = 121;
            nButton.tabStop = false;
            nButton.imageName = "";// "30_338_Printer_16x16";
            nButton.width = width90;
            nButton.events = true;
            createNavigatorButton(nButton);

            #endregion

            //--------------

            #region   DataNavigator ve yön butonları 61.... e41.... 

            nButton.backColor = v.colorOrder; //colorNavigator;

            nButton.buttonName = "simpleButton_en_basa";
            nButton.buttonText = "|<";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 67;
            nButton.tabStop = false;
            nButton.imageName = "";// "40_404_First_16x16";
            nButton.width = width25;
            nButton.events = true;
            simpleButton = createNavigatorButton(nButton);
            
            nButton.buttonName = "simpleButton_onceki_syf";
            nButton.buttonText = "<<";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 66;
            nButton.tabStop = false;
            nButton.imageName = "";// "40_404_DoublePrev_16x16";
            nButton.width = width25;
            nButton.events = true;
            simpleButton = createNavigatorButton(nButton);
            
            nButton.buttonName = "simpleButton_onceki";
            nButton.buttonText = "<";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 65;
            nButton.tabStop = false;
            nButton.imageName = "";// "NAVGERI16";
            nButton.width = width25;
            nButton.events = true;
            simpleButton = createNavigatorButton(nButton);
            
            #region DataNavigator

            // dataNavigator Her şartta visible = false de olsa panelin içinde bulunması gerekmektedir
            if (tDataNavigator != null)
            {
                tDataNavigator.TabIndex = 64;

                i = navigator.IndexOf(tDataNavigator.TabIndex.ToString());

                if (i == -1)
                {
                    //tDataNavigator.TabStop = false;
                    tDataNavigator.Width = 1;// 10;
                    tDataNavigator.Dock = DockStyle.Left;
                    tDataNavigator.TextLocation = NavigatorButtonsTextLocation.None;
                }    //    tDataNavigator.Visible = false;

                //tDataNavigator.TabIndex = 244;
                NavPanel.Controls.Add(tDataNavigator);
            }
            //---
            #endregion

            nButton.buttonName = "simpleButton_sonraki";
            nButton.buttonText = ">";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 63;
            nButton.tabStop = false;
            nButton.imageName = "";
            nButton.width = width25;
            nButton.events = true;
            simpleButton = createNavigatorButton(nButton);
            
            nButton.buttonName = "simpleButton_sonraki_syf";
            nButton.buttonText = ">>";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 62;
            nButton.tabStop = false;
            nButton.imageName = "";// "40_404_DoubleNext_16x16";
            nButton.width = width25;
            nButton.events = true;
            simpleButton = createNavigatorButton(nButton);
            
            nButton.buttonName = "simpleButton_en_sona";
            nButton.buttonText = ">|";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 61;
            nButton.tabStop = false;
            nButton.imageName = "";// "40_404_Last_16x16";
            nButton.width = width25;
            nButton.events = true;
            simpleButton = createNavigatorButton(nButton);

            #endregion

            //--------------

            #region 51... silSatır, silKart, silHesap, silBelge    e26...  

            if ((nButton.navigatorList.IndexOf("[51]") > -1) ||
                (nButton.navigatorList.IndexOf("[52]") > -1) ||
                (nButton.navigatorList.IndexOf("[53]") > -1) ||
                (nButton.navigatorList.IndexOf("[54]") > -1) ||
                (nButton.navigatorList.IndexOf("[55]") > -1))

            {
                // Sil butonu yanındaki boşluk
                DevExpress.XtraEditors.PanelControl PanelSil = new DevExpress.XtraEditors.PanelControl();
                PanelSil.Name = "tPanel1";
                PanelSil.Width = width25;
                PanelSil.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
                PanelSil.Dock = DockStyle.Right;
                NavPanel.Controls.Add(PanelSil);
                //---
            }

            nButton.backColor = v.colorDelete;

            nButton.buttonName = "simpleButton_sil_liste";
            nButton.buttonText = "Listeyi Sil";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 55;
            nButton.tabStop = false;
            nButton.imageName = "30_308_ChartTitlesNone_16x16";
            nButton.width = width90;
            nButton.events = true;
            createNavigatorButton(nButton);

            nButton.buttonName = "simpleButton_sil_belge";
            nButton.buttonText = "Belgeyi Sil";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 54; //27;
            nButton.tabStop = false;
            nButton.imageName = "30_308_ChartTitlesNone_16x16";
            nButton.width = width90;
            nButton.events = true;
            createNavigatorButton(nButton);

            nButton.buttonName = "simpleButton_sil_hesap";
            nButton.buttonText = "Hesabı Sil";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 53; //27;
            nButton.tabStop = false;
            nButton.imageName = "30_308_ChartTitlesNone_16x16";
            nButton.width = width90;
            nButton.events = true;
            createNavigatorButton(nButton);

            nButton.buttonName = "simpleButton_sil_kart";
            nButton.buttonText = "Kartı Sil";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 52; //27;
            nButton.tabStop = false;
            nButton.imageName = "30_308_ChartTitlesNone_16x16";
            nButton.width = width90;
            nButton.events = true;
            createNavigatorButton(nButton);

            nButton.buttonName = "simpleButton_sil_satir";
            //nButton.buttonText = "Satır Sil <i>" + Keys.Delete.ToString().Substring(0, 3) + "</i>";
            nButton.buttonText = "Satır Sil";//<b>Ct+" + Keys.Delete.ToString().Substring(0, 3) + "</b>";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 51; //26;
            nButton.tabStop = false;
            nButton.imageName = "40_408_Delete_16x16";
            nButton.width = width90;
            nButton.events = true;
            createNavigatorButton(nButton);

            if ((nButton.navigatorList.IndexOf("[51]") > -1) ||
                (nButton.navigatorList.IndexOf("[52]") > -1) ||
                (nButton.navigatorList.IndexOf("[53]") > -1) ||
                (nButton.navigatorList.IndexOf("[54]") > -1) ||
                (nButton.navigatorList.IndexOf("[55]") > -1))
            {
                // Sil butonu yanındaki boşluk
                DevExpress.XtraEditors.PanelControl PanelSil = new DevExpress.XtraEditors.PanelControl();
                PanelSil.Name = "tPanel1";
                PanelSil.Width = 10;// width25;
                PanelSil.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
                PanelSil.Dock = DockStyle.Right;
                NavPanel.Controls.Add(PanelSil);
                //---
            }

            #endregion

            //--------------

            #region kaydetYeni, kaydet, kaydetDevam, kaydetÇık , 21....

            //-------------------------------------
            nButton.backColor = v.colorSave;
            nButton.TableIPCode = xTableIPCode;

            //if (nButton.tabIndex == 23) nButton.tabIndex = 1;
            //if (nButton.tabIndex == 24) nButton.tabIndex = 2;

            nButton.buttonName = "simpleButton_kaydet_yeni";
            nButton.buttonText = "Kaydet, Yeni";// <b>" + v.Key_KaydetYeni.ToString() + "</b>";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 21;
            nButton.tabStop = true;
            nButton.imageName = "30_341_SaveAndNew_16x16";
            nButton.width = width90;
            nButton.events = true;
            createNavigatorButton(nButton);

            nButton.buttonName = "simpleButton_kaydet";
            nButton.buttonText = "Kaydet";// <b>" + v.Key_Kaydet.ToString() + "</b>";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 22; // 24;
            nButton.tabStop = true;
            nButton.imageName = "KAYDET16";
            nButton.width = width70;
            nButton.events = true;
            createNavigatorButton(nButton);

            nButton.buttonName = "simpleButton_kaydet_devam";
            nButton.buttonText = "Kaydet, Sonraki";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 23;// 25;
            nButton.tabStop = true;
            nButton.imageName = "30_341_SaveDialog_16x16";
            nButton.width = width90;
            nButton.events = true;
            createNavigatorButton(nButton);

            nButton.buttonName = "simpleButton_kaydet_cik";
            nButton.buttonText = "Kaydet, Çık";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 24;
            nButton.tabStop = true;
            nButton.imageName = "30_341_SaveAndClose_16x16";
            nButton.width = width90;
            nButton.events = true;
            createNavigatorButton(nButton);

            #endregion

            //--------------


            #region yeniKartAç, yeniHeap, yeniAltHesap, 31.... e52.... 

            nButton.backColor = v.colorNew;
            nButton.TableIPCode = xTableIPCode;

            nButton.buttonName = "simpleButton_yeni_kart";
            nButton.buttonText = "Yeni Kart";
            nButton.buttonKey = "<b>" + v.Key_Yeni + "</b>";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 31;//52;
            nButton.tabStop = true;
            nButton.imageName = "30_301_AddFile_16x16";
            nButton.width = width90;
            nButton.events = true;
            createNavigatorButton(nButton);

            nButton.buttonName = "simpleButton_yeni_hesap";
            nButton.buttonText = "Yeni Hesap";
            nButton.buttonKey = "<b>" + v.Key_Yeni + "</b>";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 32;// 53;
            nButton.tabStop = true;
            nButton.imageName = "YENIHESAP16";
            nButton.width = width90;
            nButton.events = true;
            createNavigatorButton(nButton);

            nButton.buttonName = "simpleButton_yeni_belge";
            nButton.buttonText = "Yeni Belge";
            nButton.buttonKey = "<b>" + v.Key_Yeni + "</b>";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 33;
            nButton.tabStop = true;
            nButton.imageName = "YENIHESAP16";
            nButton.width = width90;
            nButton.events = true;
            createNavigatorButton(nButton);

            nButton.buttonName = "simpleButton_yeni_alt_hesap";
            nButton.buttonText = "Yeni Alt";
            nButton.buttonKey = "<b>" + v.Key_NewSub + "</b>";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 34;// 54;
            nButton.tabStop = true;
            nButton.imageName = "40_408_Delete_16x16";
            nButton.width = width90;
            nButton.events = true;
            createNavigatorButton(nButton);

            // Satır butonları

            nButton.buttonName = "simpleButton_yeni_kart_satir";
            nButton.buttonText = "Yeni Kart";
            nButton.buttonKey = "<b>" + v.Key_Yeni + "</b>";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 36;
            nButton.tabStop = true;
            nButton.imageName = "30_301_AddFile_16x16";
            nButton.width = width90;
            nButton.events = true;
            createNavigatorButton(nButton);

            nButton.buttonName = "simpleButton_yeni_hesap_satir";
            nButton.buttonText = "Yeni Hesap";
            nButton.buttonKey = "<b>" + v.Key_Yeni + "</b>";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 37;
            nButton.tabStop = true;
            nButton.imageName = "YENIHESAP16";
            nButton.width = width90;
            nButton.events = true;
            createNavigatorButton(nButton);

            nButton.buttonName = "simpleButton_yeni_belge_satir";
            nButton.buttonText = "Yeni Belge";
            nButton.buttonKey = "<b>" + v.Key_Yeni + "</b>";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 38;
            nButton.tabStop = true;
            nButton.imageName = "YENIHESAP16";
            nButton.width = width90;
            nButton.events = true;
            createNavigatorButton(nButton);

            nButton.buttonName = "simpleButton_yeni_alt_hesap_satir";
            nButton.buttonText = "Yeni Alt";
            nButton.buttonKey = "<b>" + v.Key_NewSub + "</b>";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 39;
            nButton.tabStop = true;
            nButton.imageName = "40_408_Delete_16x16";
            nButton.width = width90;
            nButton.events = true;
            createNavigatorButton(nButton);

            #endregion

            //--------------

            #region Göster, kartAç, hesapAç, belgeAç 41... e50...  

            nButton.TableIPCode = TableIPCode;
            nButton.backColor = v.colorFocus;

            // Hesap Aç // Hesap oku (aynı form içinde listedeki bir kaydın detayını başka IP ye aç/oku )
            nButton.buttonName = "simpleButton_goster"; // "simpleButton_hesap_ac";
            nButton.buttonText = "Göster";
            nButton.buttonKey = "<b>" + v.Key_Goster.ToString() + "</b>";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 41; // 50;
            nButton.tabStop = true;
            nButton.imageName = "30_348_Article_16x16";
            nButton.width = width70;
            nButton.events = true;
            createNavigatorButton(nButton);

            // Kart Aç  
            nButton.buttonName = "simpleButton_kart_ac"; //"simpleButton_kart_ac";
            nButton.buttonText = "Kartı Aç";
            nButton.buttonKey = "<b>" + v.Key_HesapKartiniAc.ToString() + "</b>";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 42;// yeni;
            nButton.tabStop = true;
            nButton.imageName = "40_427_Article_16x16";
            nButton.width = width70;
            nButton.events = true;
            createNavigatorButton(nButton);

            // Hesap Aç  
            nButton.buttonName = "simpleButton_hesap_ac"; //"simpleButton_kart_ac";
            nButton.buttonText = "Hesabı Aç";
            nButton.buttonKey = "<b>" + v.Key_HesapKartiniAc.ToString() + "</b>";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 43;// yeni;
            nButton.tabStop = true;
            nButton.imageName = "40_427_Article_16x16";
            nButton.width = width90;
            nButton.events = true;
            createNavigatorButton(nButton);

            // belgeyi Aç 
            nButton.buttonName = "simpleButton_belge_ac"; //"simpleButton_kart_ac";
            nButton.buttonText = "Belgeyi Aç";
            nButton.buttonKey = "<b>" + v.Key_BelgeyiAc.ToString() + "</b>";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 44;// 51;
            nButton.tabStop = true;
            nButton.imageName = "40_427_Article_16x16";
            nButton.width = width90;
            nButton.events = true;
            createNavigatorButton(nButton);

            // Resim Editörü
            nButton.buttonName = "simpleButton_resim_edit"; 
            nButton.buttonText = "Resim Ekle";
            nButton.buttonKey = "<b>" + v.Key_ResimEditor.ToString() + "</b>";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 45;
            nButton.tabStop = true;
            nButton.imageName = "40_427_Article_16x16";
            nButton.width = width90;
            nButton.events = true;
            createNavigatorButton(nButton);

            // Rapor Dizaynı
            nButton.buttonName = "simpleButton_report_design";
            nButton.buttonText = "Rapor Dizaynı";
            nButton.buttonKey = "";// "<b>" + v.Key_ResimEditor.ToString() + "</b>";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 46;
            nButton.tabStop = true;
            nButton.imageName = "40_427_Article_16x16";
            nButton.width = width90;
            nButton.events = true;
            createNavigatorButton(nButton);

            #endregion kartAç, hesapAç

            //--------------

            #region çıkış, sihirbazDevam, sihirbazGeri, listele, seç, ekle, Liste_Hazirla, 11...

            nButton.TableIPCode = TableIPCode;
            nButton.backColor = v.colorFocus;

            nButton.buttonName = "simpleButton_sec";
            nButton.buttonText = "Seç, Çık";
            nButton.buttonKey = "<b>" + v.Key_SecCik.ToString() + "</b>";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 12;
            nButton.tabStop = true;
            nButton.imageName = "40_401_Apply_16x16";
            nButton.width = width90;
            nButton.events = true;
            createNavigatorButton(nButton);
            //simpleButton_sec.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            //simpleButton_sec.Appearance.Options.UseFont = true;

            nButton.buttonName = "simpleButton_listeye_ekle";
            nButton.buttonText = "Listeye Ekle";
            nButton.buttonKey = "<b>" + v.Key_ListyeEkle.ToString() + "</b>";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 13;
            nButton.tabStop = true;
            nButton.imageName = "40_401_Apply_16x16";
            nButton.width = width90;
            nButton.events = true;
            createNavigatorButton(nButton);
            //simpleButton_listeye_ekle.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            //simpleButton_listeye_ekle.Appearance.Options.UseFont = true;

            nButton.buttonName = "simpleButton_liste_hazirla";
            nButton.buttonText = "Liste Hazırla";
            nButton.buttonKey = "<b>" + v.Key_ListeHazirla.ToString() + "</b>";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 14;
            nButton.tabStop = true;
            nButton.imageName = "40_429_Zoom_16x16";
            nButton.width = width90;
            nButton.events = true;
            nButton.propSearch = Prop_Search;
            createNavigatorButton(nButton);
            nButton.propSearch = "";

            nButton.buttonName = "simpleButton_listele";
            nButton.buttonText = "Yenile";//"Listele";
            nButton.buttonKey = "<b>" + v.Key_Listele.ToString() + "</b>";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 15;
            nButton.tabStop = true;
            nButton.imageName = "30_319_ListBox_16x16";
            nButton.width = width70;
            nButton.events = true;
            createNavigatorButton(nButton);

            
            nButton.buttonName = "simpleButton_sihirbaz_geri";
            nButton.buttonText = "Geri";
            nButton.buttonKey = "";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 69;
            nButton.tabStop = true;
            nButton.imageName = "SIHIRBAZGERI16";
            nButton.width = width70;
            nButton.events = true;
            createNavigatorButton(nButton);

            nButton.buttonName = "simpleButton_sihirbaz_devam";
            nButton.buttonText = "Devam";
            nButton.buttonKey = "";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 68;
            nButton.tabStop = true;
            nButton.imageName = "SIHIRBAZDEVAM16";
            nButton.width = width70;
            nButton.events = true;
            createNavigatorButton(nButton);

            // Çıkış butonu yanındaki boşluk
            DevExpress.XtraEditors.PanelControl Panel1 = new DevExpress.XtraEditors.PanelControl();
            Panel1.Name = "tPanel1";
            Panel1.Width = 14;// width25;
            Panel1.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            Panel1.Dock = DockStyle.Right;
            NavPanel.Controls.Add(Panel1);
            //---

            nButton.backColor = v.colorExit;

            nButton.buttonName = "simpleButton_cikis";
            nButton.buttonText = "Çıkış";
            nButton.buttonKey = "<i>" + v.Key_Exit.ToString().Substring(0, 3) + "</i>";
            nButton.dock = DockStyle.Right;
            nButton.tabIndex = 11;
            nButton.tabStop = false;
            nButton.imageName = "CLOSE16";
            nButton.width = width70;
            nButton.events = true;
            simpleButton = createNavigatorButton(nButton);
            if (simpleButton != null)
                simpleButton.TabIndex = 999;

            //-----
            #endregion

        }

        private void Buttons_CaptionChange(DevExpress.XtraEditors.SimpleButton simpleButton_, string navigator, string buttonKey)
        {
            string buttonIndex = "[" + simpleButton_.TabIndex.ToString() + "]";

            string satir = string.Empty;
            string values = navigator;
            string index = string.Empty;
            string lkp_Caption = string.Empty;
            string lkp_newCaption = string.Empty;
            string lkp_newWidth = string.Empty;

            //btn:1||[11] Çıkış||null||0|ds|

            #region read values
            while (values.IndexOf("btn:") > -1)
            {
                satir = t.Get_And_Clear(ref values, "|ds|") + "||";

                t.Get_And_Clear(ref satir, "btn:");

                index = t.Get_And_Clear(ref satir, "||");
                lkp_Caption = t.Get_And_Clear(ref satir, "||");
                lkp_newCaption = t.Get_And_Clear(ref satir, "||");
                lkp_newWidth = t.Get_And_Clear(ref satir, "||");

                if (lkp_Caption.IndexOf(buttonIndex) > -1)
                {
                    if (lkp_newCaption != "null")
                    {
                        if (buttonKey != "")
                            simpleButton_.Text = lkp_newCaption;// + " " + buttonKey;
                        else simpleButton_.Text = lkp_newCaption;
                    }
                    if (lkp_newWidth != "0")
                        simpleButton_.Width = t.myInt32(lkp_newWidth);

                    break;
                }

            }
            #endregion

        }

        #endregion Create_Navigator

        #region Create_Form

        public Form Create_Form(string FormCode, string Caption, string MenuType, byte FormType, byte FormState)
        {
            Form tForm = Create_Form(FormCode, Caption, MenuType);

            tFormView(tForm, FormType, FormState);

            return tForm;
        }

        public Form Create_Form(string FormCode, string Caption, string MenuType)
        {
            tToolBox t = new tToolBox();
            tEventsForm evf = new tEventsForm();

            Form tForm = null;
            //RibbonForm tForm = new RibbonForm();

            if (MenuType != "Ribbon")
            {
                tForm = new Form();
                tForm.Padding = new System.Windows.Forms.Padding(v.Padding4);
            }
            if (MenuType == "Ribbon")
            {
                tForm = new RibbonForm();
                tForm.Padding = new System.Windows.Forms.Padding(0);
            }

            //System.ComponentModel.IContainer components = null;
            //components = new System.ComponentModel.Container();
            //components.Add(tForm);

            tForm.AccessibleDescription = FormCode;
            tForm.Name = "tForm_" + t.AntiStr_Dot(FormCode);
            tForm.Text = Caption;
            tForm.FormBorderStyle = FormBorderStyle.SizableToolWindow;

            //tForm.Tag = FormType;

            evf.myFormEventsAdd(tForm);

            // Formun üzerine gizli MemoEdit ekleniyor
            Create_MyFormBox(tForm, string.Empty);

            return tForm;
        }

        public Form tFormView(Form tForm, byte FormType, byte FormState)
        {
            tToolBox t = new tToolBox();

            //-------------
            // FormType
            // * 0, 1  : Normal Form
            // * 2 : MdiChild Form
            // * 3 : Main Form
            // * 4 : Dialog Form

            if (FormType <= 1) // Normal Form
            {
                //tForm_Properties(ds_FormItems, tForm, 0);

                if (FormState == 1) t.NormalForm_View(tForm, FormWindowState.Normal);
                if (FormState == 2) t.NormalForm_View(tForm, FormWindowState.Minimized);
                if (FormState == 3) t.NormalForm_View(tForm, FormWindowState.Maximized);

                //DevExp.Form_Design(tForm, ds_FormItems, Referans);
            }
            if (FormType == 2) // MdiChild Form
            {
                //DevExp.tForm_Properties(ds, tForm, 0);

                if (FormState == 1) t.ChildForm_View(tForm, Application.OpenForms[0], FormWindowState.Normal);
                if (FormState == 2) t.ChildForm_View(tForm, Application.OpenForms[0], FormWindowState.Minimized);
                if (FormState == 3) t.ChildForm_View(tForm, Application.OpenForms[0], FormWindowState.Maximized);
                //DevExp.Form_Design(tForm, ds_FormItems, Referans);
            }
            if (FormType == 3) // Main Form
            {
                //DevExp.tForm_Properties(ds_FormItems, mdifrm, 0);
                //DevExp.Form_Design(mdifrm, ds_FormItems, Referans);
            }
            if (FormType == 4) // Dialog Form
            {
                //DevExp.tForm_Properties(ds_FormItems, tForm, 0);
                //DevExp.Form_Design(tForm, ds_FormItems, Referans);

                if (FormState == 1) t.DialogForm_View(tForm, FormWindowState.Normal);
                if (FormState == 2) t.DialogForm_View(tForm, FormWindowState.Minimized);
                if (FormState == 3) t.DialogForm_View(tForm, FormWindowState.Maximized);
            }
            //--------------

            //----------
            return tForm;
        }

        #region tForm_Properties
        public void tForm_Properties(DataSet ds_FormItems, Form tForm, int i)
        {
            /*
            #region Tanımlamalar

            int Ref_Id = 0;
            int Item_Id = 0;
            byte Item_Type3 = 0;
            byte Item_Type4 = 0;
            byte Item_Type5 = 0;
            byte Item_Type6 = 0;
            byte Item_Type7 = 0;

            int width = 0;
            int height = 0;

            string Caption = string.Empty;
            string Dock_Style = string.Empty;

            #endregion Tanımlamalar

            Ref_Id = Convert.ToInt32(ds_FormItems.Tables[0].Rows[i]["REF_ID"]);
            Item_Id = Convert.ToInt32(ds_FormItems.Tables[0].Rows[i]["ITEM_ID"]);
            Caption = ds_FormItems.Tables[0].Rows[i]["ABOUT"].ToString();

            width = Convert.ToInt32(ds_FormItems.Tables[0].Rows[i]["SIZE_WIDTH"]);
            height = Convert.ToInt32(ds_FormItems.Tables[0].Rows[i]["SIZE_HEIGHT"]);

            Item_Type3 = Convert.ToByte(ds_FormItems.Tables[0].Rows[i]["ITEM_TYPE3"]);
            Item_Type4 = Convert.ToByte(ds_FormItems.Tables[0].Rows[i]["ITEM_TYPE4"]);
            Item_Type5 = Convert.ToByte(ds_FormItems.Tables[0].Rows[i]["ITEM_TYPE5"]);
            Item_Type6 = Convert.ToByte(ds_FormItems.Tables[0].Rows[i]["ITEM_TYPE6"]);
            Item_Type7 = Convert.ToByte(ds_FormItems.Tables[0].Rows[i]["ITEM_TYPE7"]);

            if (ds_FormItems.Tables[0].Rows[i]["CAPTION"].ToString() != "")
                Caption = ds_FormItems.Tables[0].Rows[i]["CAPTION"].ToString();

            if (Item_Id == sp.fr_Form)
            {
                tForm.Text = Caption;

                if ((width > 0) && (height > 0)) tForm.Size = new System.Drawing.Size(width, height);
                if ((width > 0) && (height == 0)) tForm.Size = new System.Drawing.Size(width, sp.Ekran_Height);
                if ((width == 0) && (height > 0)) tForm.Size = new System.Drawing.Size(sp.Ekran_Width, height);


                if (Item_Type3 == 1) tForm.FormBorderStyle = FormBorderStyle.None;
                if (Item_Type3 == 2) tForm.FormBorderStyle = FormBorderStyle.FixedSingle;
                if (Item_Type3 == 3) tForm.FormBorderStyle = FormBorderStyle.Fixed3D;
                if (Item_Type3 == 4) tForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                if (Item_Type3 == 5) tForm.FormBorderStyle = FormBorderStyle.Sizable;
                if (Item_Type3 == 6) tForm.FormBorderStyle = FormBorderStyle.FixedToolWindow;
                if (Item_Type3 == 7) tForm.FormBorderStyle = FormBorderStyle.SizableToolWindow;

                if (Item_Type4 == 1) tForm.StartPosition = FormStartPosition.Manual;
                if (Item_Type4 == 2) tForm.StartPosition = FormStartPosition.CenterScreen;
                if (Item_Type4 == 3) tForm.StartPosition = FormStartPosition.WindowsDefaultLocation;
                if (Item_Type4 == 4) tForm.StartPosition = FormStartPosition.WindowsDefaultBounds;
                if (Item_Type4 == 5) tForm.StartPosition = FormStartPosition.CenterParent;

                if (Item_Type5 == 1) tForm.TopMost = true;
                if (Item_Type5 == 2) tForm.TopMost = false;

                if (Item_Type6 == 1) tForm.IsMdiContainer = true;
                if (Item_Type6 == 2) tForm.IsMdiContainer = false;

                // TabbedMdiManeger 
                if (Item_Type7 == 1)
                {
                    System.ComponentModel.Container components = new System.ComponentModel.Container();
                    DevExpress.XtraTabbedMdi.XtraTabbedMdiManager tXtraTabbedMdiManager = new DevExpress.XtraTabbedMdi.XtraTabbedMdiManager(components);

                    tForm.Container.Add(tXtraTabbedMdiManager);
                    tXtraTabbedMdiManager.MdiParent = tForm;
                }
            }
            */

        }
        #endregion tForm_Properties

        #endregion Create Form

        #region Create_Properties >> 

        public string Create_PropertiesNavEdit(string thisValues)
        {
            tToolBox t = new tToolBox();

            v.con_OnayChange = true;

            thisValues = thisValues.Trim();

            Form tForm = Create_Form("", "PropertiesNavEdit", "");

            tForm.AccessibleDefaultActionDescription = thisValues;

            Create_PropertiesNav_Pages(tForm);

            int i = 500;
            tForm.Width = i;
            tForm.Height = i + (i / 3);

            tFormView(tForm, 4, 1);

            if (tForm.AccessibleDefaultActionDescription != null)
                thisValues = tForm.AccessibleDefaultActionDescription.ToString();

            v.con_LkpOnayChange = false;
            v.con_OnayChange = false;

            return thisValues;
        }

        public string Create_PropertiesPlusEdit(string TableName, string Main_FieldName,
                      string Width, string thisValues)
        {
            tToolBox t = new tToolBox();

            string function_name = "Create PropertiesPlusEdit";
            t.Takipci(function_name, "", '{');

            #region Tanımlar

            tTablesRead tr = new tTablesRead();
            //tCreateObject co = new tCreateObject();
            tDevView dv = new tDevView();
            tEvents ev = new tEvents();
            DataSet ds_PropList = new DataSet();

            // PropertiesPlus da birden fazla blok oluşmakta
            // ilk block value okunsun diye ilk Id tespit ediliyor

            string paket_basi = Main_FieldName + "={" + v.ENTER;
            string paket_sonu = Main_FieldName + "=}";

            t.Str_Remove(ref thisValues, paket_basi);
            t.Str_Remove(ref thisValues, paket_sonu);

            thisValues = thisValues.Trim();

            //*********************************
            v.con_Value_Min = 999;
            //*********************************

            int Id = ev.Properies_Min_Block_Id(Main_FieldName, thisValues);

            string FirstBlockValue = ev.Properties_SingleBlockValue_Get(Main_FieldName, thisValues, Id);
            string TableIPCode = "tPropertiesPlusEdit";

            tr.MS_Properties_Read(ds_PropList, TableName, Main_FieldName);

            #endregion Tanımlar

            #region new Controls

            //Form tForm = Create_Form(TableIPCode, "PropertiesPlusEdit", "");
            Form tForm = Create_Form(Main_FieldName, "PropertiesPlusEdit", "");

            DevExpress.XtraEditors.PanelControl panelControl1 = new DevExpress.XtraEditors.PanelControl();
            DevExpress.XtraTab.XtraTabControl tTabControl1 = new DevExpress.XtraTab.XtraTabControl();
            DevExpress.XtraTab.XtraTabPage tTabPage1 = new DevExpress.XtraTab.XtraTabPage();
            DevExpress.XtraTab.XtraTabPage tTabPage2 = new DevExpress.XtraTab.XtraTabPage();
            DevExpress.XtraEditors.GroupControl groupControl = new DevExpress.XtraEditors.GroupControl();
            // 
            // tTabControl1
            // 
            tTabControl1.Dock = DockStyle.Fill;
            tTabControl1.SelectedTabPage = tTabPage1;
            tTabControl1.TabIndex = 0;
            tTabControl1.Width = 300;
            tTabControl1.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            tTabPage1,
            tTabPage2});
            // 
            // xtraTabPage1
            // 
            tTabPage1.Name = "xtraTabPage1";
            tTabPage1.Size = new System.Drawing.Size(355, 400);
            tTabPage1.Text = "Properties List";
            tTabPage1.Padding = new System.Windows.Forms.Padding(v.Padding4);
            // 
            // xtraTabPage2
            // 
            tTabPage2.Name = "xtraTabPage2";
            tTabPage2.Size = new System.Drawing.Size(355, 400);
            tTabPage2.Text = "Properties Block";
            tTabPage2.Padding = new System.Windows.Forms.Padding(v.Padding4);

            // 
            // panelControl
            // 
            panelControl1.Name = "panelControl1";
            panelControl1.Dock = DockStyle.Left;
            panelControl1.TabIndex = 0;
            panelControl1.Width = 300;
            panelControl1.Visible = true;
            //panelControl1.Controls.Add(groupControl);
            panelControl1.Controls.Add(tTabControl1);

            // 
            // groupControl
            // 
            groupControl.Text = " [ " + TableName + "." + Main_FieldName + " ]";
            groupControl.Visible = true;
            groupControl.Dock = DockStyle.Fill;
            groupControl.Padding = new System.Windows.Forms.Padding(v.Padding4);

            #endregion new Controls

            #region new VerticalGrid, listBoxControl, Columns, Buttons Create
            //
            // VerticalGrid Create
            //
            Control cntrl = new Control();
            cntrl = dv.Create_View_(v.obj_vw_VGridSingle, 0, "tProperties", "");

            if (cntrl != null)
            {
                groupControl.Controls.Add(cntrl);

                Create_Properties_Columns((VGridControl)cntrl, ds_PropList, FirstBlockValue, TableIPCode);
                Create_Properties_Buttons((VGridControl)cntrl, TableIPCode);
                Create_Properties_List(tTabControl1, panelControl1, TableIPCode, thisValues, Main_FieldName, Id);

                ((VGridControl)cntrl).Name = "tForm_" + Main_FieldName;
                ((VGridControl)cntrl).AccessibleDescription = Main_FieldName;

                tForm.AccessibleDefaultActionDescription = thisValues;

                tForm.Controls.Add(groupControl);
                tForm.Controls.Add(panelControl1);

                ev.tProperties_VGrid_Enabled(tForm, (VGridControl)cntrl);

                tForm.Width = 700;
                tForm.Height = 600;

                tFormView(tForm, 4, 1);

                if (tForm.AccessibleDefaultActionDescription != null)
                    thisValues = tForm.AccessibleDefaultActionDescription.ToString();

                if (thisValues.Length > 5)
                {
                    thisValues = paket_basi + thisValues + v.ENTER + paket_sonu;
                }
            }

            #endregion new VerticalGrid Create

            return thisValues;

        }

        public string Create_PropertiesPlusEdit_JSON(string TableName, string Main_FieldName,
                      string Width, string thisValues)
        {
            tToolBox t = new tToolBox();

            #region Tanımlar

            tTablesRead tr = new tTablesRead();
            //tCreateObject co = new tCreateObject();
            tDevView dv = new tDevView();
            tEvents ev = new tEvents();
            DataSet ds_PropList = new DataSet();

            /// PropertiesPlus da birden fazla blok oluşmakta
            ///

            /// "SV_LIST": [
            /// {
            /// "CAPTION": "Anull",
            /// "SV_VALUE": "null",
            /// "TABLEIPCODE": "null"
            /// },
            /// {
            /// "CAPTION": "Bnull",
            /// "SV_VALUE": "null",
            /// "TABLEIPCODE": "null"
            /// },
            /// {
            /// "CAPTION": "Cnull",
            /// "SV_VALUE": "null",
            /// "TABLEIPCODE": "null"
            /// }
            /// ]

            string TableIPCode = "tPropertiesPlusEdit";
            string paket_basi_A = (char)34 + Main_FieldName + (char)34 + ": ["; //"GRID": [
            string paket_basi_B = (char)34 + Main_FieldName + (char)34 + ": {"; //"GRID": {
            string paket_basi_C = (char)34 + Main_FieldName + (char)34 + ": ";  //"GRID": 
            string paket_basi_Mevcut = "";

            //string paket_sonu_A = "]";
            //string paket_sonu_B = "}";

            string paket_turu = ev.tPropertiesPacketValue_Preparing(TableName, Main_FieldName, ref thisValues, ref paket_basi_Mevcut, TableIPCode);

            string FirstBlockValue = ev.Properties_SingleBlockValue_Get_JSON(thisValues, 0);

            tr.MS_Properties_Read(ds_PropList, TableName, Main_FieldName);

            #endregion Tanımlar

            #region new Controls

            //Form tForm = Create_Form(TableIPCode, "PropertiesPlusEdit", "");
            Form tForm = Create_Form(Main_FieldName, "PropertiesPlusEdit", "");

            DevExpress.XtraEditors.PanelControl panelControl1 = new DevExpress.XtraEditors.PanelControl();
            DevExpress.XtraTab.XtraTabControl tTabControl1 = new DevExpress.XtraTab.XtraTabControl();
            DevExpress.XtraTab.XtraTabPage tTabPage1 = new DevExpress.XtraTab.XtraTabPage();
            DevExpress.XtraTab.XtraTabPage tTabPage2 = new DevExpress.XtraTab.XtraTabPage();
            DevExpress.XtraEditors.GroupControl groupControl = new DevExpress.XtraEditors.GroupControl();
            // 
            // tTabControl1
            // 
            tTabControl1.Dock = DockStyle.Fill;
            tTabControl1.SelectedTabPage = tTabPage1;
            tTabControl1.TabIndex = 0;
            tTabControl1.Width = 300;
            tTabControl1.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            tTabPage1,
            tTabPage2});
            // 
            // xtraTabPage1
            // 
            tTabPage1.Name = "xtraTabPage1";
            tTabPage1.Size = new System.Drawing.Size(355, 400);
            tTabPage1.Text = "Properties List.JSON";
            tTabPage1.Padding = new System.Windows.Forms.Padding(v.Padding4);
            // 
            // xtraTabPage2
            // 
            tTabPage2.Name = "xtraTabPage2";
            tTabPage2.Size = new System.Drawing.Size(355, 400);
            tTabPage2.Text = "Properties Block.JSON";
            tTabPage2.Padding = new System.Windows.Forms.Padding(v.Padding4);

            // 
            // panelControl
            // 
            panelControl1.Name = "panelControl1";
            panelControl1.Dock = DockStyle.Left;
            panelControl1.TabIndex = 0;
            panelControl1.Width = 300;
            panelControl1.Visible = true;
            panelControl1.Controls.Add(tTabControl1);

            // 
            // groupControl
            // 
            groupControl.Text = " [ " + TableName + "." + Main_FieldName + " ]";
            groupControl.Visible = true;
            groupControl.Dock = DockStyle.Fill;
            groupControl.Padding = new System.Windows.Forms.Padding(v.Padding4);

            #endregion new Controls

            #region new VerticalGrid, listBoxControl, Columns, Buttons Create
            //
            // VerticalGrid Create
            //
            Control cntrl = new Control();
            cntrl = dv.Create_View_(v.obj_vw_VGridSingle, 0, "tProperties", "");

            if (cntrl != null)
            {
                groupControl.Controls.Add(cntrl);

                Create_Properties_Columns_JSON((VGridControl)cntrl, ds_PropList, FirstBlockValue, TableIPCode);
                Create_Properties_Buttons_JSON((VGridControl)cntrl, TableIPCode);
                Create_Properties_List_JSON(tTabControl1, panelControl1, TableIPCode, thisValues, Main_FieldName);

                ((VGridControl)cntrl).Name = "tForm_" + Main_FieldName; // "tProperties";
                ((VGridControl)cntrl).AccessibleDescription = Main_FieldName;

                ((VGridControl)cntrl).Tag = TableName;

                tForm.AccessibleDefaultActionDescription = thisValues;
                tForm.Controls.Add(groupControl);
                tForm.Controls.Add(panelControl1);

                //ev.tProperties_VGrid_Enabled(tForm, (VGridControl)cntrl);

                tForm.Width = 700;
                tForm.Height = 600;

                tFormView(tForm, 4, 1);

                if (tForm.AccessibleDefaultActionDescription != null)
                    thisValues = tForm.AccessibleDefaultActionDescription.ToString();

                if (thisValues.Length > 5)
                {
                    /// pakete [ ] işareti ekleniyor 

                    if (paket_turu == "[]")
                    {
                        if (paket_basi_Mevcut == "A")
                            thisValues = paket_basi_C + thisValues;
                        //thisValues = paket_basi_A + thisValues + v.ENTER + paket_sonu_A;

                        //if (thisValues.IndexOf(": [") == -1)
                        //{
                        //    thisValues = paket_basi_A + thisValues + v.ENTER + paket_sonu_A;
                        //}
                        //else 
                        //{
                        //    thisValues = paket_basi_C + thisValues;
                        //}
                    }
                    else if (paket_turu == "{}")
                    {
                        if (paket_basi_Mevcut == "B")
                            thisValues = paket_basi_C + thisValues;
                    }
                    else if (paket_turu == "")
                    {
                        thisValues = paket_basi_C + thisValues;
                    }
                }
            }

            #endregion new VerticalGrid Create

            return thisValues;

        }

        public string Create_PropertiesEdit(string TableName, string Main_FieldName,
                      string Width, string thisValues)
        {
            tToolBox t = new tToolBox();

            string function_name = "Create_PropertiesEdit";
            t.Takipci(function_name, "", '{');

            tTablesRead tr = new tTablesRead();
            tDevView dv = new tDevView();
            //tCreateObject co = new tCreateObject();
            DataSet ds_PropList = new DataSet();

            string TableIPCode = "tPropertiesEdit";
            string FirstValues = thisValues;

            tr.MS_Properties_Read(ds_PropList, TableName, Main_FieldName);

            string paket_basi = Main_FieldName + "={" + v.ENTER;
            string paket_sonu = Main_FieldName + "=}";

            t.Str_Remove(ref thisValues, paket_basi);
            t.Str_Remove(ref thisValues, paket_sonu);

            thisValues = thisValues.Trim();

            // Properties de sadece tek blok oluşmakta 

            //Form tForm = Create_Form(TableIPCode, "PropertiesEdit", "");
            Form tForm = Create_Form(Main_FieldName, "PropertiesEdit", "");

            Create_Properties_Pages(tForm);

            //DevExpress.XtraEditors.GroupControl groupControl = new DevExpress.XtraEditors.GroupControl();
            //groupControl.Text = " [ " + TableName + "." + Main_FieldName + " ]";
            //groupControl.Visible = true;
            //groupControl.Dock = DockStyle.Fill;

            Control page1 = t.Find_Control(tForm, "tabNavigationPage1");
            Control page2 = t.Find_Control(tForm, "tabNavigationPage2");

            Control cntrl = new Control();
            cntrl = dv.Create_View_(v.obj_vw_VGridSingle, 0, "tProperties", "");
            if (cntrl != null)
            {
                //groupControl.Controls.Add(cntrl);
                page1.Controls.Add(cntrl);

                Create_Properties_Columns((VGridControl)cntrl, ds_PropList, thisValues, TableIPCode);
                Create_Properties_Buttons((VGridControl)cntrl, TableIPCode);

                ((VGridControl)cntrl).Name = "tForm_" + Main_FieldName;
                ((VGridControl)cntrl).AccessibleDescription = Main_FieldName;


                MemoEdit tEdit = new DevExpress.XtraEditors.MemoEdit();
                tEdit.Name = "memoEdit_PropertiesValue"; //"MemoEdit1";
                tEdit.Dock = DockStyle.Fill;
                tEdit.EditValue = thisValues; // FirstValues;

                page2.Controls.Add(tEdit);

                Create_PropertiesMemo_Buttons(page2, TableIPCode);

                tForm.AccessibleDefaultActionDescription = thisValues;

                //tForm.Controls.Add(groupControl);

                int i = t.myInt32(Width);
                if (i <= 100) i = 400;

                tForm.Width = i;
                tForm.Height = i + (i / 3);

                tFormView(tForm, 4, 1);

                if (tForm.AccessibleDefaultActionDescription != null)
                    thisValues = tForm.AccessibleDefaultActionDescription.ToString();

                if (thisValues.Length > 5)
                {
                    thisValues = paket_basi + thisValues + v.ENTER + paket_sonu;
                }
            }

            return thisValues;

            // işlem bittikten sonra kendisini create eden buttonEdite dönen değer
        }

        public string Create_PropertiesEdit_JSON(string TableName, string Main_FieldName,
                      string Width, string thisValues)
        {
            tToolBox t = new tToolBox();
            tTablesRead tr = new tTablesRead();
            tDevView dv = new tDevView();
            //tCreateObject co = new tCreateObject();
            tEvents ev = new tEvents();

            string TableIPCode = "tPropertiesEdit";
            string FirstValues = thisValues;

            thisValues = thisValues.Trim();

            /// eğer boş value geldiyse main_field in Modeline bakarak boş array istenir
            //if ((t.IsNotNull(thisValues) == false) || (thisValues == "[]") || (thisValues == "{}"))
            //     thisValues = t.Create_PropertiesEdit_Model_JSON(TableName, Main_FieldName);

            string paket_basi_A = (char)34 + Main_FieldName + (char)34 + ": ["; //"GRID": [
            string paket_basi_B = (char)34 + Main_FieldName + (char)34 + ": {"; //"GRID": {
            string paket_basi_C = (char)34 + Main_FieldName + (char)34 + ": ";  //"GRID": 
            //string paket_sonu_A = "]";
            //string paket_sonu_B = "}";
            string paket_basi_Mevcut = "";

            string paket_turu = ev.tPropertiesPacketValue_Preparing(TableName, Main_FieldName,
                   ref thisValues, ref paket_basi_Mevcut, TableIPCode);

            // Properties de sadece tek blokla oluşmakta 

            //Form tForm = Create_Form(TableIPCode, "PropertiesEdit", "");
            Form tForm = Create_Form(Main_FieldName, "PropertiesEdit", "");

            Create_Properties_Pages(tForm);

            Control page1 = t.Find_Control(tForm, "tabNavigationPage1");
            Control page2 = t.Find_Control(tForm, "tabNavigationPage2");

            Control cntrl = new Control();
            cntrl = dv.Create_View_(v.obj_vw_VGridSingle, 0, "tProperties", "");
            if (cntrl != null)
            {
                page1.Controls.Add(cntrl);

                DataSet ds_PropList = new DataSet();
                tr.MS_Properties_Read(ds_PropList, TableName, Main_FieldName);

                Create_Properties_Columns_JSON((VGridControl)cntrl, ds_PropList, thisValues, TableIPCode);
                Create_Properties_Buttons_JSON((VGridControl)cntrl, TableIPCode);

                ((VGridControl)cntrl).Name = "tForm_" + Main_FieldName;
                ((VGridControl)cntrl).AccessibleDescription = Main_FieldName;
                ((VGridControl)cntrl).Tag = TableName;

                MemoEdit tEdit = new DevExpress.XtraEditors.MemoEdit();
                tEdit.Name = "memoEdit_PropertiesValue";// "MemoEdit1";
                tEdit.Dock = DockStyle.Fill;
                tEdit.EditValue = thisValues; // FirstValues;

                page2.Controls.Add(tEdit);

                Create_PropertiesMemo_Buttons(page2, TableIPCode);

                tForm.AccessibleDefaultActionDescription = thisValues;

                int i = t.myInt32(Width);
                if (i <= 100) i = 400;

                tForm.Width = i;
                tForm.Height = i + (i / 3);

                tFormView(tForm, 4, 1);

                if (tForm.AccessibleDefaultActionDescription != null)
                    thisValues = tForm.AccessibleDefaultActionDescription.ToString();

                if (thisValues.Length > 5)
                {
                    /// pakete [ ] işareti ekleniyor 

                    if (paket_turu == "[]")
                    {
                        if (paket_basi_Mevcut == "A")
                            thisValues = paket_basi_C + thisValues;
                        //thisValues = paket_basi_A + thisValues + v.ENTER + paket_sonu_A;
                    }
                    else if (paket_turu == "{}")
                    {
                        if (paket_basi_Mevcut == "B")
                            thisValues = paket_basi_C + thisValues;
                    }
                    else if (paket_turu == "")
                    {
                        /// herhangi bir işleme gerek yok
                        //thisValues = thisValues;
                    }
                }
            }

            return thisValues;

            // işlem bittikten sonra kendisini create eden buttonEdite dönen değer
        }


        //----------------------------------------------------------------------------
        private void Create_PropertiesNav_Pages(Form tForm)
        {
            DevExpress.XtraEditors.GroupControl groupControl1 = new DevExpress.XtraEditors.GroupControl();

            tForm.Controls.Add(groupControl1);

            //
            // groupControl1
            // 
            groupControl1.Name = "groupControl1";
            groupControl1.Size = new System.Drawing.Size(400, 500);
            groupControl1.TabIndex = 1;
            groupControl1.Text = "Button Listesi";
            groupControl1.Dock = DockStyle.Fill;

            //      string TableIPCode = "3S_MSNAV.3S_MSNAV_01";
            string TableIPCode = "UST/T01/3S_MSNAV.3S_MSNAV_01";
            //
            // InputPanel Create
            //
            tInputPanel ip = new tInputPanel();
            ip.Create_InputPanel(tForm, groupControl1, TableIPCode, v.IPdataType_DataView, true);

            Create_PropertiesNav_Buttons(tForm, TableIPCode);

            string thisValues = string.Empty;
            if (tForm.AccessibleDefaultActionDescription != null)
                thisValues = tForm.AccessibleDefaultActionDescription.ToString();

            if (t.IsNotNull(thisValues))
                Set_PropertiesNavEditValue(tForm, TableIPCode, thisValues);
        }

        private void Set_PropertiesNavEditValue(Form tForm, string TableIPCode, string thisValues)
        {
            tToolBox t = new tToolBox();

            string satir = string.Empty;
            string values = thisValues;
            string index = string.Empty;
            string lkp_Caption = string.Empty;
            string lkp_newCaption = string.Empty;
            string lkp_newWidth = string.Empty;
            int kontrol = 0;

            DataSet dsData = null;
            DataNavigator dN = null;
            t.Find_DataSet(tForm, ref dsData, ref dN, TableIPCode);

            if (t.IsNotNull(dsData) == false) return;

            int i2 = dsData.Tables[0].Rows.Count;

            //btn:1||[11] Çıkış||null||0|ds|

            #region read values
            while (values.IndexOf("btn:") > -1)
            {
                satir = t.Get_And_Clear(ref values, "|ds|") + "||";

                t.Get_And_Clear(ref satir, "btn:");

                index = t.Get_And_Clear(ref satir, "||");
                lkp_Caption = t.Get_And_Clear(ref satir, "||");
                lkp_newCaption = t.Get_And_Clear(ref satir, "||");
                lkp_newWidth = t.Get_And_Clear(ref satir, "||");

                for (int i = 0; i < i2; i++)
                {
                    if (dsData.Tables[0].Rows[i]["LKP_CAPTION"].ToString() == lkp_Caption)
                    {
                        dsData.Tables[0].Rows[i]["LKP_ONAY"] = 1;

                        if (lkp_newCaption != "null")
                            dsData.Tables[0].Rows[i]["LKP_NEW_CAPTION"] = lkp_newCaption;

                        if (lkp_newWidth != "0")
                            dsData.Tables[0].Rows[i]["LKP_NEW_WIDTH"] = lkp_newWidth;

                        kontrol++;

                        break;
                    }
                }


            }
            #endregion

            #region eski  kayıt düzenini okuma
            /// navigatorun eski kayıt sistemini okumak ve göstermek için
            /// [11] Çıkış, [24] Kaydet, [42] Sonraki Syf, [43] Sonraki, [44] Kayıt Adedi, [45] Önceki, [46] Önceki Syf, [53] Yeni Hesap

            if (kontrol == 0)
            {
                values = thisValues + ",";
                while (values.IndexOf(",") > -1)
                {
                    lkp_Caption = t.Get_And_Clear(ref values, ",");
                    lkp_Caption = lkp_Caption.Trim();

                    for (int i = 0; i < i2; i++)
                    {
                        if (dsData.Tables[0].Rows[i]["LKP_CAPTION"].ToString().IndexOf(lkp_Caption) > -1)
                        {
                            dsData.Tables[0].Rows[i]["LKP_ONAY"] = 1;
                            break;
                        }
                    }
                }
            } // if kontrol
            #endregion eski  kayıt düzenini okuma


        }

        private void Create_Properties_Pages(Form tForm)
        {

            DevExpress.XtraBars.Navigation.TabPane tabPane1 = new DevExpress.XtraBars.Navigation.TabPane();
            DevExpress.XtraBars.Navigation.TabNavigationPage tabNavigationPage1 = new DevExpress.XtraBars.Navigation.TabNavigationPage();
            DevExpress.XtraBars.Navigation.TabNavigationPage tabNavigationPage2 = new DevExpress.XtraBars.Navigation.TabNavigationPage();
            ((System.ComponentModel.ISupportInitialize)(tabPane1)).BeginInit();
            tabPane1.SuspendLayout();
            // 
            // tabPane1
            // 
            tabPane1.Controls.Add(tabNavigationPage1);
            tabPane1.Controls.Add(tabNavigationPage2);
            tabPane1.Location = new System.Drawing.Point(39, 29);
            tabPane1.Name = "tabPane1";
            tabPane1.Pages.AddRange(new DevExpress.XtraBars.Navigation.NavigationPageBase[] {
            tabNavigationPage1,
            tabNavigationPage2});
            tabPane1.RegularSize = new System.Drawing.Size(588, 245);
            tabPane1.SelectedPage = tabNavigationPage1;
            tabPane1.Size = new System.Drawing.Size(588, 245);
            tabPane1.TabIndex = 0;
            tabPane1.Text = "tabPane1";
            tabPane1.Dock = DockStyle.Fill;
            // 
            // tabNavigationPage1
            // 
            tabNavigationPage1.Caption = "Properties";
            tabNavigationPage1.Name = "tabNavigationPage1";
            tabNavigationPage1.Size = new System.Drawing.Size(570, 200);
            // 
            // tabNavigationPage2
            // 
            tabNavigationPage2.Caption = "Full Text";
            tabNavigationPage2.Name = "tabNavigationPage2";
            tabNavigationPage2.Size = new System.Drawing.Size(570, 200);

            tForm.Controls.Add(tabPane1);
        }

        private void Create_PropertiesNav_Buttons(Control cntrl, string TableIPCode)
        {
            tEvents ev = new tEvents();

            DevExpress.XtraEditors.GroupControl groupControl_PropertiesButtons = new DevExpress.XtraEditors.GroupControl();
            System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Properties = new System.Windows.Forms.TableLayoutPanel();

            DevExpress.XtraEditors.SimpleButton simpleButton_Update = new DevExpress.XtraEditors.SimpleButton();
            DevExpress.XtraEditors.SimpleButton simpleButton_Tamam = new DevExpress.XtraEditors.SimpleButton();
            DevExpress.XtraEditors.SimpleButton simpleButton_Temizle = new DevExpress.XtraEditors.SimpleButton();

            cntrl.Controls.Add(groupControl_PropertiesButtons);

            //this.SuspendLayout();
            //
            // groupControl_PropertiesButtons
            // 
            groupControl_PropertiesButtons.Controls.Add(tableLayoutPanel_Properties);
            groupControl_PropertiesButtons.Name = "groupControl_PropertiesButtons_Memo";
            groupControl_PropertiesButtons.Size = new System.Drawing.Size(293, 52);
            groupControl_PropertiesButtons.TabIndex = 1;
            groupControl_PropertiesButtons.Text = "İşlemler";
            groupControl_PropertiesButtons.Dock = DockStyle.Bottom;
            // 
            // tableLayoutPanel_Properties
            // 
            //tableLayoutPanel_Properties.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;
            tableLayoutPanel_Properties.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            tableLayoutPanel_Properties.ColumnCount = 5;
            tableLayoutPanel_Properties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            tableLayoutPanel_Properties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            tableLayoutPanel_Properties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            tableLayoutPanel_Properties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            tableLayoutPanel_Properties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            tableLayoutPanel_Properties.Controls.Add(simpleButton_Temizle, 0, 0);
            //tableLayoutPanel_Properties.Controls.Add(simpleButton_Collapse, 1, 0);
            //tableLayoutPanel_Properties.Controls.Add(simpleButton_Expand, 2, 0);
            tableLayoutPanel_Properties.Controls.Add(simpleButton_Update, 3, 0);
            tableLayoutPanel_Properties.Controls.Add(simpleButton_Tamam, 4, 0);
            tableLayoutPanel_Properties.Location = new System.Drawing.Point(2, 21);
            tableLayoutPanel_Properties.Name = "tableLayoutPanel1";
            tableLayoutPanel_Properties.RowCount = 1;
            tableLayoutPanel_Properties.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel_Properties.TabIndex = 0;
            tableLayoutPanel_Properties.Dock = System.Windows.Forms.DockStyle.Fill;

            // 
            // simpleButton_Tamam
            // 
            simpleButton_Tamam.Dock = System.Windows.Forms.DockStyle.Top;
            simpleButton_Tamam.Location = new System.Drawing.Point(189, 6);
            simpleButton_Tamam.Name = "simpleButton_Close1";
            simpleButton_Tamam.Size = new System.Drawing.Size(94, 23);
            simpleButton_Tamam.TabIndex = 1;
            simpleButton_Tamam.Text = "&Close";

            simpleButton_Tamam.Click += new System.EventHandler(ev.btn_PropertiesNav_Click);
            simpleButton_Tamam.AccessibleName = TableIPCode;

            // 
            // simpleButton_Update
            // 
            simpleButton_Update.Dock = System.Windows.Forms.DockStyle.Top;
            simpleButton_Update.Location = new System.Drawing.Point(189, 6);
            simpleButton_Update.Name = "simpleButton_Update1";
            simpleButton_Update.Size = new System.Drawing.Size(94, 23);
            simpleButton_Update.TabIndex = 2;
            simpleButton_Update.Text = "&Update";

            simpleButton_Update.Click += new System.EventHandler(ev.btn_PropertiesNav_Click);
            simpleButton_Update.AccessibleName = TableIPCode;
            // 
            // simpleButton_Temizle
            // 
            simpleButton_Temizle.Dock = System.Windows.Forms.DockStyle.Top;
            simpleButton_Temizle.Location = new System.Drawing.Point(6, 6);
            simpleButton_Temizle.Name = "simpleButton_Clear1";
            simpleButton_Temizle.Size = new System.Drawing.Size(93, 23);
            simpleButton_Temizle.TabIndex = 3;
            simpleButton_Temizle.Text = "&Clear";

            simpleButton_Temizle.Click += new System.EventHandler(ev.btn_PropertiesNav_Click);
            simpleButton_Temizle.AccessibleName = TableIPCode;
            /*
            // 
            // simpleButton_Collapse
            // 
            simpleButton_Collapse.Dock = System.Windows.Forms.DockStyle.Top;
            simpleButton_Collapse.Location = new System.Drawing.Point(105, 6);
            simpleButton_Collapse.Name = "simpleButton_Collapse";
            simpleButton_Collapse.Size = new System.Drawing.Size(36, 23);
            simpleButton_Collapse.TabIndex = 4;
            simpleButton_Collapse.Text = "Coll";

            simpleButton_Collapse.Click += new System.EventHandler(ev.btn_Properties_Click);
            simpleButton_Collapse.AccessibleName = TableIPCode;
            simpleButton_Collapse.AccessibleDescription = tVGrid.Name.ToString();
            // 
            // simpleButton_Expand
            // 
            simpleButton_Expand.Dock = System.Windows.Forms.DockStyle.Top;
            simpleButton_Expand.Location = new System.Drawing.Point(147, 6);
            simpleButton_Expand.Name = "simpleButton_Expand";
            simpleButton_Expand.Size = new System.Drawing.Size(36, 24);
            simpleButton_Expand.TabIndex = 5;
            simpleButton_Expand.Text = "Exp";

            simpleButton_Expand.Click += new System.EventHandler(ev.btn_Properties_Click);
            simpleButton_Expand.AccessibleName = TableIPCode;
            simpleButton_Expand.AccessibleDescription = tVGrid.Name.ToString();
            */

        }

        private void Create_PropertiesMemo_Buttons(Control page, string TableIPCode)
        {
            tEvents ev = new tEvents();

            DevExpress.XtraEditors.GroupControl groupControl_PropertiesButtons = new DevExpress.XtraEditors.GroupControl();
            System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Properties = new System.Windows.Forms.TableLayoutPanel();

            //DevExpress.XtraEditors.SimpleButton simpleButton_Temizle = new DevExpress.XtraEditors.SimpleButton();
            //DevExpress.XtraEditors.SimpleButton simpleButton_Collapse = new DevExpress.XtraEditors.SimpleButton();
            //DevExpress.XtraEditors.SimpleButton simpleButton_Expand = new DevExpress.XtraEditors.SimpleButton();
            DevExpress.XtraEditors.SimpleButton simpleButton_Update = new DevExpress.XtraEditors.SimpleButton();
            DevExpress.XtraEditors.SimpleButton simpleButton_Tamam = new DevExpress.XtraEditors.SimpleButton();

            //Control cntrl = tVGrid.Parent;
            page.Controls.Add(groupControl_PropertiesButtons);

            //this.SuspendLayout();
            //
            // groupControl_PropertiesButtons
            // 
            groupControl_PropertiesButtons.Controls.Add(tableLayoutPanel_Properties);
            groupControl_PropertiesButtons.Name = "groupControl_PropertiesButtons_Memo";
            groupControl_PropertiesButtons.Size = new System.Drawing.Size(293, 52);
            groupControl_PropertiesButtons.TabIndex = 1;
            groupControl_PropertiesButtons.Text = "İşlemler";
            groupControl_PropertiesButtons.Dock = DockStyle.Bottom;
            // 
            // tableLayoutPanel_Properties
            // 
            //tableLayoutPanel_Properties.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;
            tableLayoutPanel_Properties.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            tableLayoutPanel_Properties.ColumnCount = 5;
            tableLayoutPanel_Properties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            tableLayoutPanel_Properties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            tableLayoutPanel_Properties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            tableLayoutPanel_Properties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            tableLayoutPanel_Properties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            //tableLayoutPanel_Properties.Controls.Add(simpleButton_Temizle, 0, 0);
            //tableLayoutPanel_Properties.Controls.Add(simpleButton_Collapse, 1, 0);
            //tableLayoutPanel_Properties.Controls.Add(simpleButton_Expand, 2, 0);
            tableLayoutPanel_Properties.Controls.Add(simpleButton_Update, 3, 0);
            tableLayoutPanel_Properties.Controls.Add(simpleButton_Tamam, 4, 0);
            tableLayoutPanel_Properties.Location = new System.Drawing.Point(2, 21);
            tableLayoutPanel_Properties.Name = "tableLayoutPanel_Kriter_Memo";
            tableLayoutPanel_Properties.RowCount = 1;
            tableLayoutPanel_Properties.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel_Properties.TabIndex = 0;
            tableLayoutPanel_Properties.Dock = System.Windows.Forms.DockStyle.Fill;

            // 
            // simpleButton_Tamam
            // 
            simpleButton_Tamam.Dock = System.Windows.Forms.DockStyle.Top;
            simpleButton_Tamam.Location = new System.Drawing.Point(189, 6);
            simpleButton_Tamam.Name = "simpleButton_Close_Memo";
            simpleButton_Tamam.Size = new System.Drawing.Size(94, 23);
            simpleButton_Tamam.TabIndex = 1;
            simpleButton_Tamam.Text = "&Close Memo";

            simpleButton_Tamam.Click += new System.EventHandler(ev.btn_Properties_Click);
            simpleButton_Tamam.AccessibleName = TableIPCode;
            //simpleButton_Tamam.AccessibleDescription = tVGrid.Name.ToString();

            // 
            // simpleButton_Update
            // 
            simpleButton_Update.Dock = System.Windows.Forms.DockStyle.Top;
            simpleButton_Update.Location = new System.Drawing.Point(189, 6);
            simpleButton_Update.Name = "simpleButton_Update_Memo";
            simpleButton_Update.Size = new System.Drawing.Size(94, 23);
            simpleButton_Update.TabIndex = 2;
            simpleButton_Update.Text = "&Update Memo";

            simpleButton_Update.Click += new System.EventHandler(ev.btn_Properties_Click);
            simpleButton_Update.AccessibleName = TableIPCode;
            //simpleButton_Update.AccessibleDescription = tVGrid.Name.ToString();
            /*
            // 
            // simpleButton_Temizle
            // 
            simpleButton_Temizle.Dock = System.Windows.Forms.DockStyle.Top;
            simpleButton_Temizle.Location = new System.Drawing.Point(6, 6);
            simpleButton_Temizle.Name = "simpleButton_Clear";
            simpleButton_Temizle.Size = new System.Drawing.Size(93, 23);
            simpleButton_Temizle.TabIndex = 3;
            simpleButton_Temizle.Text = "&Clear";

            simpleButton_Temizle.Click += new System.EventHandler(ev.btn_Properties_Click);
            simpleButton_Temizle.AccessibleName = TableIPCode;
            simpleButton_Temizle.AccessibleDescription = tVGrid.Name.ToString();
            // 
            // simpleButton_Collapse
            // 
            simpleButton_Collapse.Dock = System.Windows.Forms.DockStyle.Top;
            simpleButton_Collapse.Location = new System.Drawing.Point(105, 6);
            simpleButton_Collapse.Name = "simpleButton_Collapse";
            simpleButton_Collapse.Size = new System.Drawing.Size(36, 23);
            simpleButton_Collapse.TabIndex = 4;
            simpleButton_Collapse.Text = "Coll";

            simpleButton_Collapse.Click += new System.EventHandler(ev.btn_Properties_Click);
            simpleButton_Collapse.AccessibleName = TableIPCode;
            simpleButton_Collapse.AccessibleDescription = tVGrid.Name.ToString();
            // 
            // simpleButton_Expand
            // 
            simpleButton_Expand.Dock = System.Windows.Forms.DockStyle.Top;
            simpleButton_Expand.Location = new System.Drawing.Point(147, 6);
            simpleButton_Expand.Name = "simpleButton_Expand";
            simpleButton_Expand.Size = new System.Drawing.Size(36, 24);
            simpleButton_Expand.TabIndex = 5;
            simpleButton_Expand.Text = "Exp";

            simpleButton_Expand.Click += new System.EventHandler(ev.btn_Properties_Click);
            simpleButton_Expand.AccessibleName = TableIPCode;
            simpleButton_Expand.AccessibleDescription = tVGrid.Name.ToString();
            */

        }

        //--- JSON
        private void Create_Properties_Columns_JSON(VGridControl tVGrid, DataSet dsFields,
             string thisValue, string prp_type)
        {
            tToolBox t = new tToolBox();
            tDevColumn dc = new tDevColumn();
            tDefaultValue df = new tDefaultValue();

            string s = string.Empty;
            string tTableName = string.Empty;
            string tRowFName = string.Empty;
            string tRowCaption = string.Empty;
            string tColumnType = string.Empty;
            string tTypeName = string.Empty;
            string tValue = string.Empty;
            //string paket_basi = string.Empty;
            //string paket_sonu = string.Empty;

            byte tRowType = 0;
            byte tCategoryNo = 0;
            byte tRowLineNo = 0;
            Int16 tRowFieldType = 0;

            // emanete ver
            tVGrid.AccessibleDefaultActionDescription = thisValue;

            foreach (DataRow Row in dsFields.Tables[0].Rows)
            {
                tTableName = t.Set(Row["TABLENAME"].ToString(), "", "null");
                tRowFName = t.Set(Row["ROW_FIELDNAME"].ToString(), "", "null");
                tRowType = t.Set(Row["ROW_TYPE"].ToString(), "", (byte)2);
                tCategoryNo = t.Set(Row["ROW_CATEGORY_NO"].ToString(), "", (byte)0);
                tRowLineNo = t.Set(Row["ROW_LINE_NO"].ToString(), "", (byte)0);
                tRowCaption = t.Set(Row["ROW_CAPTION"].ToString(), "", tRowFName);
                tRowFieldType = t.Set(Row["ROW_FIELDTYPE"].ToString(), "", (Int16)167);
                tColumnType = t.Set(Row["ROW_COLUMN_TYPE"].ToString(), "", "TextEdit");
                tTypeName = t.Set(Row["LIST_TYPES_NAME"].ToString(), "", "");

                if (tRowType == 1)
                {
                    CategoryRow CatRow = new CategoryRow(tRowCaption);
                    CatRow.Name = "CategoryRow_" + tCategoryNo.ToString();
                    tVGrid.Rows.Add(CatRow);
                }

                if (tRowType == 2)
                {

                    tValue = t.Find_Properties_Value(thisValue, tRowFName);

                    EditorRow EditRow = new EditorRow("EditRow_" + tRowFName);
                    EditRow.Properties.Caption = tRowCaption;
                    EditRow.Properties.FieldName = tRowFName;
                    EditRow.Tag = tRowFieldType;
                    EditRow.Properties.Value = tValue;

                    s = string.Empty;
                    t.MyProperties_Set(ref s, "TableName", tTableName);
                    t.MyProperties_Set(ref s, "FieldName", tRowFName);
                    t.MyProperties_Set(ref s, "ColumnType", tColumnType);
                    t.MyProperties_Set(ref s, "TypeName", tTypeName);
                    t.MyProperties_Set(ref s, "Width", "0");

                    dc.VGrid_ColumnEdit_(Row, EditRow, s, "", 1); //Tumu = hayır

                    tVGrid.Rows["CategoryRow_" + tCategoryNo.ToString()].ChildRows.Add(EditRow);
                }

            } // foreach

        }

        private void Create_Properties_Buttons_JSON(VGridControl tVGrid, string TableIPCode)
        {
            tEvents ev = new tEvents();

            DevExpress.XtraEditors.GroupControl groupControl_PropertiesButtons = new DevExpress.XtraEditors.GroupControl();
            System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Properties = new System.Windows.Forms.TableLayoutPanel();

            DevExpress.XtraEditors.SimpleButton simpleButton_Temizle = new DevExpress.XtraEditors.SimpleButton();
            DevExpress.XtraEditors.SimpleButton simpleButton_Collapse = new DevExpress.XtraEditors.SimpleButton();
            DevExpress.XtraEditors.SimpleButton simpleButton_Expand = new DevExpress.XtraEditors.SimpleButton();
            DevExpress.XtraEditors.SimpleButton simpleButton_Update = new DevExpress.XtraEditors.SimpleButton();
            DevExpress.XtraEditors.SimpleButton simpleButton_Tamam = new DevExpress.XtraEditors.SimpleButton();

            Control cntrl = tVGrid.Parent;
            cntrl.Controls.Add(groupControl_PropertiesButtons);

            //this.SuspendLayout();
            //
            // groupControl_PropertiesButtons
            // 
            groupControl_PropertiesButtons.Controls.Add(tableLayoutPanel_Properties);
            groupControl_PropertiesButtons.Name = "groupControl_PropertiesButtons";
            groupControl_PropertiesButtons.Size = new System.Drawing.Size(293, 52);
            groupControl_PropertiesButtons.TabIndex = 1;
            groupControl_PropertiesButtons.Text = "İşlemler.JS";
            groupControl_PropertiesButtons.Dock = DockStyle.Bottom;
            // 
            // tableLayoutPanel_Properties
            // 
            //tableLayoutPanel_Properties.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;
            tableLayoutPanel_Properties.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            tableLayoutPanel_Properties.ColumnCount = 5;
            tableLayoutPanel_Properties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            tableLayoutPanel_Properties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            tableLayoutPanel_Properties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            tableLayoutPanel_Properties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            tableLayoutPanel_Properties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            tableLayoutPanel_Properties.Controls.Add(simpleButton_Temizle, 0, 0);
            tableLayoutPanel_Properties.Controls.Add(simpleButton_Collapse, 1, 0);
            tableLayoutPanel_Properties.Controls.Add(simpleButton_Expand, 2, 0);
            tableLayoutPanel_Properties.Controls.Add(simpleButton_Update, 3, 0);
            tableLayoutPanel_Properties.Controls.Add(simpleButton_Tamam, 4, 0);
            tableLayoutPanel_Properties.Location = new System.Drawing.Point(2, 21);
            tableLayoutPanel_Properties.Name = "tableLayoutPanel_Kriter";
            tableLayoutPanel_Properties.RowCount = 1;
            tableLayoutPanel_Properties.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel_Properties.TabIndex = 0;
            tableLayoutPanel_Properties.Dock = System.Windows.Forms.DockStyle.Fill;

            // 
            // simpleButton_Tamam
            // 
            simpleButton_Tamam.Dock = System.Windows.Forms.DockStyle.Top;
            simpleButton_Tamam.Location = new System.Drawing.Point(189, 6);
            simpleButton_Tamam.Name = "simpleButton_Close";
            simpleButton_Tamam.Size = new System.Drawing.Size(94, 23);
            simpleButton_Tamam.TabIndex = 1;
            simpleButton_Tamam.Text = "&Close";

            simpleButton_Tamam.Click += new System.EventHandler(ev.btn_Properties_JSON_Click);
            simpleButton_Tamam.AccessibleName = TableIPCode;
            simpleButton_Tamam.AccessibleDescription = tVGrid.Name.ToString();
            // 
            // simpleButton_Update
            // 
            simpleButton_Update.Dock = System.Windows.Forms.DockStyle.Top;
            simpleButton_Update.Location = new System.Drawing.Point(189, 6);
            simpleButton_Update.Name = "simpleButton_Update";
            simpleButton_Update.Size = new System.Drawing.Size(94, 23);
            simpleButton_Update.TabIndex = 2;
            simpleButton_Update.Text = "&Update";

            simpleButton_Update.Click += new System.EventHandler(ev.btn_Properties_JSON_Click);
            simpleButton_Update.AccessibleName = TableIPCode;
            simpleButton_Update.AccessibleDescription = tVGrid.Name.ToString();
            // 
            // simpleButton_Temizle
            // 
            simpleButton_Temizle.Dock = System.Windows.Forms.DockStyle.Top;
            simpleButton_Temizle.Location = new System.Drawing.Point(6, 6);
            simpleButton_Temizle.Name = "simpleButton_Clear";
            simpleButton_Temizle.Size = new System.Drawing.Size(93, 23);
            simpleButton_Temizle.TabIndex = 3;
            simpleButton_Temizle.Text = "&Clear";

            simpleButton_Temizle.Click += new System.EventHandler(ev.btn_Properties_JSON_Click);
            simpleButton_Temizle.AccessibleName = TableIPCode;
            simpleButton_Temizle.AccessibleDescription = tVGrid.Name.ToString();
            // 
            // simpleButton_Collapse
            // 
            simpleButton_Collapse.Dock = System.Windows.Forms.DockStyle.Top;
            simpleButton_Collapse.Location = new System.Drawing.Point(105, 6);
            simpleButton_Collapse.Name = "simpleButton_Collapse";
            simpleButton_Collapse.Size = new System.Drawing.Size(36, 23);
            simpleButton_Collapse.TabIndex = 4;
            simpleButton_Collapse.Text = "Coll";

            simpleButton_Collapse.Click += new System.EventHandler(ev.btn_Properties_JSON_Click);
            simpleButton_Collapse.AccessibleName = TableIPCode;
            simpleButton_Collapse.AccessibleDescription = tVGrid.Name.ToString();
            // 
            // simpleButton_Expand
            // 
            simpleButton_Expand.Dock = System.Windows.Forms.DockStyle.Top;
            simpleButton_Expand.Location = new System.Drawing.Point(147, 6);
            simpleButton_Expand.Name = "simpleButton_Expand";
            simpleButton_Expand.Size = new System.Drawing.Size(36, 24);
            simpleButton_Expand.TabIndex = 5;
            simpleButton_Expand.Text = "Exp";

            simpleButton_Expand.Click += new System.EventHandler(ev.btn_Properties_JSON_Click);
            simpleButton_Expand.AccessibleName = TableIPCode;
            simpleButton_Expand.AccessibleDescription = tVGrid.Name.ToString();

        }

        private void Create_Properties_List_JSON(
                     DevExpress.XtraTab.XtraTabControl tTabControl1,
                     DevExpress.XtraEditors.PanelControl panelControl1,
                     string TableIPCode, string thisValue, string MainFName)//, int BlockId)
        {
            // old - bir şeklide eski yapı gelirse 
            if (thisValue.IndexOf("={") > -1)
            {
                return;
            }

            tToolBox t = new tToolBox();
            tEvents ev = new tEvents();

            #region Controls
            System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Properties = new System.Windows.Forms.TableLayoutPanel();
            DevExpress.XtraEditors.GroupControl groupControl_PropertiesPlusButtons = new DevExpress.XtraEditors.GroupControl();
            DevExpress.XtraEditors.ListBoxControl listBoxControl1 = new DevExpress.XtraEditors.ListBoxControl();
            DevExpress.XtraEditors.MemoEdit memoEdit1 = new DevExpress.XtraEditors.MemoEdit();
            DevExpress.XtraEditors.TextEdit textEdit_ViewID = new DevExpress.XtraEditors.TextEdit();
            DevExpress.XtraEditors.SimpleButton simpleButton_PrpSil = new DevExpress.XtraEditors.SimpleButton();
            DevExpress.XtraEditors.SimpleButton simpleButton_PrpEkle = new DevExpress.XtraEditors.SimpleButton();
            DevExpress.XtraEditors.SimpleButton simpleButton_BlokEkle = new DevExpress.XtraEditors.SimpleButton();

            //
            // groupControl_PropertiesButtons
            // 
            groupControl_PropertiesPlusButtons.Controls.Add(tableLayoutPanel_Properties);
            groupControl_PropertiesPlusButtons.Name = "groupControl_PropertiesPlusButtons";
            groupControl_PropertiesPlusButtons.Size = new System.Drawing.Size(293, 52);
            groupControl_PropertiesPlusButtons.TabIndex = 1;
            groupControl_PropertiesPlusButtons.Text = "İşlemler.JSON";
            groupControl_PropertiesPlusButtons.Dock = DockStyle.Bottom;
            // 
            // tableLayoutPanel_Properties
            // 
            tableLayoutPanel_Properties.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            tableLayoutPanel_Properties.ColumnCount = 3;
            tableLayoutPanel_Properties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            tableLayoutPanel_Properties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            tableLayoutPanel_Properties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            tableLayoutPanel_Properties.Controls.Add(simpleButton_PrpSil, 0, 0);
            tableLayoutPanel_Properties.Controls.Add(textEdit_ViewID, 1, 0);
            tableLayoutPanel_Properties.Controls.Add(simpleButton_PrpEkle, 2, 0);
            tableLayoutPanel_Properties.Location = new System.Drawing.Point(2, 21);
            tableLayoutPanel_Properties.Name = "tableLayoutPanel_PropertiesPlus";
            tableLayoutPanel_Properties.RowCount = 1;
            tableLayoutPanel_Properties.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel_Properties.TabIndex = 0;
            tableLayoutPanel_Properties.Dock = System.Windows.Forms.DockStyle.Fill;
            // 
            // textEdit_ViewID
            // 
            textEdit_ViewID.Dock = System.Windows.Forms.DockStyle.Top;
            textEdit_ViewID.Name = "textEdit_ViewID";
            textEdit_ViewID.TabIndex = 3;
            textEdit_ViewID.Properties.ReadOnly = true;
            textEdit_ViewID.Properties.Appearance.Options.UseTextOptions = true;
            textEdit_ViewID.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            textEdit_ViewID.Properties.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            textEdit_ViewID.Text = "0";//BlockId.ToString();
            // 
            // simpleButton_PrpEkle
            // 
            simpleButton_PrpEkle.Dock = System.Windows.Forms.DockStyle.Top;
            simpleButton_PrpEkle.Location = new System.Drawing.Point(189, 6);
            simpleButton_PrpEkle.Name = "simpleButton_Ekle";
            simpleButton_PrpEkle.Size = new System.Drawing.Size(94, 23);
            simpleButton_PrpEkle.TabIndex = 1;
            simpleButton_PrpEkle.Text = "&Add";
            simpleButton_PrpEkle.Click += new System.EventHandler(ev.btn_Properties_JSON_Click);
            // 
            // simpleButton_PrpSil
            // 
            simpleButton_PrpSil.Dock = System.Windows.Forms.DockStyle.Top;
            simpleButton_PrpSil.Location = new System.Drawing.Point(6, 6);
            simpleButton_PrpSil.Name = "simpleButton_Sil";
            simpleButton_PrpSil.Size = new System.Drawing.Size(93, 23);
            simpleButton_PrpSil.TabIndex = 2;
            simpleButton_PrpSil.Text = "&Delete";
            simpleButton_PrpSil.Click += new System.EventHandler(ev.btn_Properties_JSON_Click);
            //
            // Properties listesi 
            //
            listBoxControl1.Name = "listControl1";
            listBoxControl1.Dock = DockStyle.Fill;
            listBoxControl1.AccessibleName = TableIPCode;
            listBoxControl1.SelectedIndexChanged += new System.EventHandler(ev.tProperties_listBoxControl_SelectedIndexChanged_JSON);
            listBoxControl1.AccessibleDescription = MainFName;
            //
            // memoEdit1 
            //
            memoEdit1.Name = "memoEdit_PropertiesValue";
            memoEdit1.Dock = DockStyle.Fill;
            memoEdit1.Properties.AccessibleName = TableIPCode;
            memoEdit1.EditValue = thisValue;
            //
            // simpleButton_BlokEkle
            // 
            simpleButton_BlokEkle.Dock = System.Windows.Forms.DockStyle.Bottom;
            simpleButton_BlokEkle.Location = new System.Drawing.Point(6, 6);
            simpleButton_BlokEkle.Name = "simpleButton_BlokEkle";
            simpleButton_BlokEkle.Size = new System.Drawing.Size(93, 23);
            simpleButton_BlokEkle.TabIndex = 3;
            simpleButton_BlokEkle.Text = "&Memo Add";
            simpleButton_BlokEkle.Click += new System.EventHandler(ev.btn_Properties_JSON_Click);

            #endregion Controls

            /// 'Table1': [
            /// {
            ///    'id': 0,
            ///    'item': 'item 0'
            /// },
            /// {
            ///    'id': 1,
            ///    'item': 'item 1'
            /// }
            /// ]
            ///
            /// {
            ///   "Name": "Acme Ltd.",
            ///   "Employees": [
            ///     {
            ///       "$id": "1",
            ///       "Name": "George-Michael",
            ///       "Manager": null
            ///     },
            ///     {
            ///       "$id": "2",
            ///       "Name": "Maeby",
            ///       "Manager": {
            ///         "$ref": "1"
            ///       }
            ///     }
            ///   ]
            /// }

            #region listBoxControl u doldurma işlemi

            listBoxControl1.Tag = 1;

            if (((thisValue.IndexOf("[") > -1) &&
                 (thisValue.IndexOf("[") < 5)) == false)
                thisValue = "[" + v.ENTER + thisValue + v.ENTER + "]";

            var myList = JsonConvert.DeserializeObject(thisValue);
            int q = 0;
            int i = 0;
            string value = string.Empty;
            foreach (Newtonsoft.Json.Linq.JToken item in (Newtonsoft.Json.Linq.JArray)myList)
            {
                q = 0;
                foreach (var item2 in (Newtonsoft.Json.Linq.JToken)item)
                {
                    // ilk bilgiyi alıp listBox a ekleyelim, genelde caption value
                    if (q == 0)
                    {
                        value = item2.ToString();
                        value = t.MyProperties_Get(value, (char)34 + ": " + (char)34); //"
                        value = " " + i.ToString() + " : " + value.Trim();
                        listBoxControl1.Items.Add(value);
                        break;
                    }
                    q++;
                }
                i++;
            }

            listBoxControl1.Tag = 0;
            listBoxControl1.SelectedIndex = 0;

            #endregion listBoxControl

            tTabControl1.TabPages[0].Controls.Add(listBoxControl1);
            tTabControl1.TabPages[1].Controls.Add(memoEdit1);
            tTabControl1.TabPages[1].Controls.Add(simpleButton_BlokEkle);
            panelControl1.Controls.Add(groupControl_PropertiesPlusButtons);

        }

        ///--- JSON

        ///--- OLD  
        private void Create_Properties_Columns(VGridControl tVGrid, DataSet dsFields,
                     string thisValue, string prp_type)
        {
            tToolBox t = new tToolBox();
            string function_name = "Create_Properties_Column";
            t.Takipci(function_name, "", '{');

            tDevColumn dc = new tDevColumn();
            tDefaultValue df = new tDefaultValue();

            string s = string.Empty;
            string tTableName = string.Empty;
            string tRowFName = string.Empty;
            string tRowCaption = string.Empty;
            string tColumnType = string.Empty;
            string tTypeName = string.Empty;
            string tValue = string.Empty;
            string paket_basi = string.Empty;
            string paket_sonu = string.Empty;
            int j1, j2 = 0;

            byte tRowType = 0;
            byte tCategoryNo = 0;
            byte tRowLineNo = 0;
            Int16 tRowFieldType = 0;

            // emanete ver
            tVGrid.AccessibleDefaultActionDescription = thisValue;

            foreach (DataRow Row in dsFields.Tables[0].Rows)
            {
                tTableName = t.Set(Row["TABLENAME"].ToString(), "", "null");
                tRowFName = t.Set(Row["ROW_FIELDNAME"].ToString(), "", "null");
                tRowType = t.Set(Row["ROW_TYPE"].ToString(), "", (byte)2);
                tCategoryNo = t.Set(Row["ROW_CATEGORY_NO"].ToString(), "", (byte)0);
                tRowLineNo = t.Set(Row["ROW_LINE_NO"].ToString(), "", (byte)0);
                tRowCaption = t.Set(Row["ROW_CAPTION"].ToString(), "", tRowFName);
                tRowFieldType = t.Set(Row["ROW_FIELDTYPE"].ToString(), "", (Int16)167);
                tColumnType = t.Set(Row["ROW_COLUMN_TYPE"].ToString(), "", "TextEdit");
                tTypeName = t.Set(Row["LIST_TYPES_NAME"].ToString(), "", "");

                if (tRowType == 1)
                {
                    CategoryRow CatRow = new CategoryRow(tRowCaption);
                    CatRow.Name = "CategoryRow_" + tCategoryNo.ToString();
                    tVGrid.Rows.Add(CatRow);
                }

                if (tRowType == 2)
                {
                    s = thisValue;

                    paket_basi = tRowFName + "={" + v.ENTER;
                    paket_sonu = tRowFName + "=};" + v.ENTER;

                    if (s.IndexOf(paket_basi) > -1)
                    {
                        j1 = s.IndexOf(paket_basi);
                        j2 = s.IndexOf(paket_sonu) + paket_sonu.Length;
                        tValue = s.Substring(j1, j2 - j1);
                    }
                    else
                    {
                        tValue = t.MyProperties_Get(s, "=" + tRowFName + ":");
                        tValue = tValue.Trim();
                    }

                    tValue = tValue.Trim();

                    if ((tValue == "null;\r\n") ||
                            (tValue == "null")) tValue = "";

                    if (tValue.Length > 0)
                    {
                        if (tValue.Substring(tValue.Length - 1, 1) == ";")
                        {
                            tValue = tValue.Substring(0, tValue.Length - 1);
                        }
                    }


                    EditorRow EditRow = new EditorRow("EditRow_" + tRowFName);
                    EditRow.Properties.Caption = tRowCaption;
                    EditRow.Properties.FieldName = tRowFName;
                    EditRow.Tag = tRowFieldType;
                    EditRow.Properties.Value = tValue;

                    s = string.Empty;
                    t.MyProperties_Set(ref s, "TableName", tTableName);
                    t.MyProperties_Set(ref s, "FieldName", tRowFName);
                    t.MyProperties_Set(ref s, "ColumnType", tColumnType);
                    t.MyProperties_Set(ref s, "TypeName", tTypeName);
                    t.MyProperties_Set(ref s, "Width", "0");

                    dc.VGrid_ColumnEdit_(Row, EditRow, s, "", 1); //Tumu = hayır

                    tVGrid.Rows["CategoryRow_" + tCategoryNo.ToString()].ChildRows.Add(EditRow);
                }

            } // foreach

            t.Takipci(function_name, "", '}');
        }

        private void Create_Properties_Buttons(VGridControl tVGrid, string TableIPCode)
        {
            tEvents ev = new tEvents();

            DevExpress.XtraEditors.GroupControl groupControl_PropertiesButtons = new DevExpress.XtraEditors.GroupControl();
            System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Properties = new System.Windows.Forms.TableLayoutPanel();

            DevExpress.XtraEditors.SimpleButton simpleButton_Temizle = new DevExpress.XtraEditors.SimpleButton();
            DevExpress.XtraEditors.SimpleButton simpleButton_Collapse = new DevExpress.XtraEditors.SimpleButton();
            DevExpress.XtraEditors.SimpleButton simpleButton_Expand = new DevExpress.XtraEditors.SimpleButton();
            DevExpress.XtraEditors.SimpleButton simpleButton_Update = new DevExpress.XtraEditors.SimpleButton();
            DevExpress.XtraEditors.SimpleButton simpleButton_Tamam = new DevExpress.XtraEditors.SimpleButton();

            Control cntrl = tVGrid.Parent;
            cntrl.Controls.Add(groupControl_PropertiesButtons);

            //this.SuspendLayout();
            //
            // groupControl_PropertiesButtons
            // 
            groupControl_PropertiesButtons.Controls.Add(tableLayoutPanel_Properties);
            groupControl_PropertiesButtons.Name = "groupControl_PropertiesButtons";
            groupControl_PropertiesButtons.Size = new System.Drawing.Size(293, 52);
            groupControl_PropertiesButtons.TabIndex = 1;
            groupControl_PropertiesButtons.Text = "İşlemler";
            groupControl_PropertiesButtons.Dock = DockStyle.Bottom;
            // 
            // tableLayoutPanel_Properties
            // 
            //tableLayoutPanel_Properties.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;
            tableLayoutPanel_Properties.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            tableLayoutPanel_Properties.ColumnCount = 5;
            tableLayoutPanel_Properties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            tableLayoutPanel_Properties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            tableLayoutPanel_Properties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            tableLayoutPanel_Properties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            tableLayoutPanel_Properties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            tableLayoutPanel_Properties.Controls.Add(simpleButton_Temizle, 0, 0);
            tableLayoutPanel_Properties.Controls.Add(simpleButton_Collapse, 1, 0);
            tableLayoutPanel_Properties.Controls.Add(simpleButton_Expand, 2, 0);
            tableLayoutPanel_Properties.Controls.Add(simpleButton_Update, 3, 0);
            tableLayoutPanel_Properties.Controls.Add(simpleButton_Tamam, 4, 0);
            tableLayoutPanel_Properties.Location = new System.Drawing.Point(2, 21);
            tableLayoutPanel_Properties.Name = "tableLayoutPanel_Kriter";
            tableLayoutPanel_Properties.RowCount = 1;
            tableLayoutPanel_Properties.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel_Properties.TabIndex = 0;
            tableLayoutPanel_Properties.Dock = System.Windows.Forms.DockStyle.Fill;

            // 
            // simpleButton_Tamam
            // 
            simpleButton_Tamam.Dock = System.Windows.Forms.DockStyle.Top;
            simpleButton_Tamam.Location = new System.Drawing.Point(189, 6);
            simpleButton_Tamam.Name = "simpleButton_Close";
            simpleButton_Tamam.Size = new System.Drawing.Size(94, 23);
            simpleButton_Tamam.TabIndex = 1;
            simpleButton_Tamam.Text = "&Close";

            simpleButton_Tamam.Click += new System.EventHandler(ev.btn_Properties_Click);
            simpleButton_Tamam.AccessibleName = TableIPCode;
            simpleButton_Tamam.AccessibleDescription = tVGrid.Name.ToString();
            // 
            // simpleButton_Update
            // 
            simpleButton_Update.Dock = System.Windows.Forms.DockStyle.Top;
            simpleButton_Update.Location = new System.Drawing.Point(189, 6);
            simpleButton_Update.Name = "simpleButton_Update";
            simpleButton_Update.Size = new System.Drawing.Size(94, 23);
            simpleButton_Update.TabIndex = 2;
            simpleButton_Update.Text = "&Update";

            simpleButton_Update.Click += new System.EventHandler(ev.btn_Properties_Click);
            simpleButton_Update.AccessibleName = TableIPCode;
            simpleButton_Update.AccessibleDescription = tVGrid.Name.ToString();
            // 
            // simpleButton_Temizle
            // 
            simpleButton_Temizle.Dock = System.Windows.Forms.DockStyle.Top;
            simpleButton_Temizle.Location = new System.Drawing.Point(6, 6);
            simpleButton_Temizle.Name = "simpleButton_Clear";
            simpleButton_Temizle.Size = new System.Drawing.Size(93, 23);
            simpleButton_Temizle.TabIndex = 3;
            simpleButton_Temizle.Text = "&Clear";

            simpleButton_Temizle.Click += new System.EventHandler(ev.btn_Properties_Click);
            simpleButton_Temizle.AccessibleName = TableIPCode;
            simpleButton_Temizle.AccessibleDescription = tVGrid.Name.ToString();
            // 
            // simpleButton_Collapse
            // 
            simpleButton_Collapse.Dock = System.Windows.Forms.DockStyle.Top;
            simpleButton_Collapse.Location = new System.Drawing.Point(105, 6);
            simpleButton_Collapse.Name = "simpleButton_Collapse";
            simpleButton_Collapse.Size = new System.Drawing.Size(36, 23);
            simpleButton_Collapse.TabIndex = 4;
            simpleButton_Collapse.Text = "Coll";

            simpleButton_Collapse.Click += new System.EventHandler(ev.btn_Properties_Click);
            simpleButton_Collapse.AccessibleName = TableIPCode;
            simpleButton_Collapse.AccessibleDescription = tVGrid.Name.ToString();
            // 
            // simpleButton_Expand
            // 
            simpleButton_Expand.Dock = System.Windows.Forms.DockStyle.Top;
            simpleButton_Expand.Location = new System.Drawing.Point(147, 6);
            simpleButton_Expand.Name = "simpleButton_Expand";
            simpleButton_Expand.Size = new System.Drawing.Size(36, 24);
            simpleButton_Expand.TabIndex = 5;
            simpleButton_Expand.Text = "Exp";

            simpleButton_Expand.Click += new System.EventHandler(ev.btn_Properties_Click);
            simpleButton_Expand.AccessibleName = TableIPCode;
            simpleButton_Expand.AccessibleDescription = tVGrid.Name.ToString();

        }

        private void Create_Properties_List(
                     DevExpress.XtraTab.XtraTabControl tTabControl1,
                     DevExpress.XtraEditors.PanelControl panelControl1,
                     string TableIPCode, string thisValue, string MainFName, int BlockId)
        {
            tToolBox t = new tToolBox();
            tEvents ev = new tEvents();

            #region Controls
            System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Properties = new System.Windows.Forms.TableLayoutPanel();
            DevExpress.XtraEditors.GroupControl groupControl_PropertiesPlusButtons = new DevExpress.XtraEditors.GroupControl();
            DevExpress.XtraEditors.ListBoxControl listBoxControl1 = new DevExpress.XtraEditors.ListBoxControl();
            DevExpress.XtraEditors.MemoEdit memoEdit1 = new DevExpress.XtraEditors.MemoEdit();
            DevExpress.XtraEditors.TextEdit textEdit_ViewID = new DevExpress.XtraEditors.TextEdit();
            DevExpress.XtraEditors.SimpleButton simpleButton_PrpSil = new DevExpress.XtraEditors.SimpleButton();
            DevExpress.XtraEditors.SimpleButton simpleButton_PrpEkle = new DevExpress.XtraEditors.SimpleButton();
            DevExpress.XtraEditors.SimpleButton simpleButton_BlokEkle = new DevExpress.XtraEditors.SimpleButton();

            //
            // groupControl_PropertiesButtons
            // 
            groupControl_PropertiesPlusButtons.Controls.Add(tableLayoutPanel_Properties);
            groupControl_PropertiesPlusButtons.Name = "groupControl_PropertiesPlusButtons";
            groupControl_PropertiesPlusButtons.Size = new System.Drawing.Size(293, 52);
            groupControl_PropertiesPlusButtons.TabIndex = 1;
            groupControl_PropertiesPlusButtons.Text = "İşlemler";
            groupControl_PropertiesPlusButtons.Dock = DockStyle.Bottom;
            // 
            // tableLayoutPanel_Properties
            // 
            tableLayoutPanel_Properties.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            tableLayoutPanel_Properties.ColumnCount = 3;
            tableLayoutPanel_Properties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            tableLayoutPanel_Properties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            tableLayoutPanel_Properties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            tableLayoutPanel_Properties.Controls.Add(simpleButton_PrpSil, 0, 0);
            tableLayoutPanel_Properties.Controls.Add(textEdit_ViewID, 1, 0);
            tableLayoutPanel_Properties.Controls.Add(simpleButton_PrpEkle, 2, 0);
            tableLayoutPanel_Properties.Location = new System.Drawing.Point(2, 21);
            tableLayoutPanel_Properties.Name = "tableLayoutPanel_PropertiesPlus";
            tableLayoutPanel_Properties.RowCount = 1;
            tableLayoutPanel_Properties.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel_Properties.TabIndex = 0;
            tableLayoutPanel_Properties.Dock = System.Windows.Forms.DockStyle.Fill;
            // 
            // textEdit_ViewID
            // 
            textEdit_ViewID.Dock = System.Windows.Forms.DockStyle.Top;
            textEdit_ViewID.Name = "textEdit_ViewID";
            textEdit_ViewID.TabIndex = 3;
            textEdit_ViewID.Properties.ReadOnly = true;
            textEdit_ViewID.Properties.Appearance.Options.UseTextOptions = true;
            textEdit_ViewID.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            textEdit_ViewID.Properties.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            textEdit_ViewID.Text = BlockId.ToString();
            // 
            // simpleButton_PrpEkle
            // 
            simpleButton_PrpEkle.Dock = System.Windows.Forms.DockStyle.Top;
            simpleButton_PrpEkle.Location = new System.Drawing.Point(189, 6);
            simpleButton_PrpEkle.Name = "simpleButton_Ekle";
            simpleButton_PrpEkle.Size = new System.Drawing.Size(94, 23);
            simpleButton_PrpEkle.TabIndex = 1;
            simpleButton_PrpEkle.Text = "&Add";
            simpleButton_PrpEkle.Click += new System.EventHandler(ev.btn_Properties_Click);
            // 
            // simpleButton_PrpSil
            // 
            simpleButton_PrpSil.Dock = System.Windows.Forms.DockStyle.Top;
            simpleButton_PrpSil.Location = new System.Drawing.Point(6, 6);
            simpleButton_PrpSil.Name = "simpleButton_Sil";
            simpleButton_PrpSil.Size = new System.Drawing.Size(93, 23);
            simpleButton_PrpSil.TabIndex = 2;
            simpleButton_PrpSil.Text = "&Delete";
            simpleButton_PrpSil.Click += new System.EventHandler(ev.btn_Properties_Click);
            //
            // Properties listesi 
            //
            listBoxControl1.Name = "listControl1";
            listBoxControl1.Dock = DockStyle.Fill;
            listBoxControl1.AccessibleName = TableIPCode;
            listBoxControl1.SelectedIndexChanged += new System.EventHandler(ev.tProperties_listBoxControl_SelectedIndexChanged);
            listBoxControl1.AccessibleDescription = MainFName;
            //
            // memoEdit1 
            //
            memoEdit1.Name = "memoEdit_PropertiesValue";
            memoEdit1.Dock = DockStyle.Fill;
            memoEdit1.Properties.AccessibleName = TableIPCode;
            memoEdit1.EditValue = thisValue;
            //
            // simpleButton_BlokEkle
            // 
            simpleButton_BlokEkle.Dock = System.Windows.Forms.DockStyle.Bottom;
            simpleButton_BlokEkle.Location = new System.Drawing.Point(6, 6);
            simpleButton_BlokEkle.Name = "simpleButton_BlokEkle";
            simpleButton_BlokEkle.Size = new System.Drawing.Size(93, 23);
            simpleButton_BlokEkle.TabIndex = 3;
            simpleButton_BlokEkle.Text = "&Memo Add";
            simpleButton_BlokEkle.Click += new System.EventHandler(ev.btn_Properties_Click);

            #endregion Controls

            string blok = string.Empty;
            string caption = string.Empty;
            string fcaption = string.Empty;
            int Id = 0;
            listBoxControl1.Tag = 1;

            string lockE = "=ROWE_" + MainFName + ":";
            /* örnek            
            1=ROW_PROP_NAVIGATOR:1;    <<< başlangıç
            1=CAPTION:null;
            1=BUTTONTYPE:null;
            1=TABLEIPCODE_LIST:null;
            1=FORMNAME:null;
            1=FORMCODE:null;
            1=FORMTYPE:null;
            1=FORMSTATE:null;
            1=ROWE_PROP_NAVIGATOR:1;   <<< bitiş
            */

            #region // doğru caption bilgisi için aradaki paketler çıkarılacak

            int i1, i2, j1, j2 = 0;
            i1 = 0;
            i2 = thisValue.Length;

            string paket_fname = string.Empty;
            string paket_basi = string.Empty;
            string paket_sonu = string.Empty;

            // 1=TABLEIPCODE_LIST:TABLEIPCODE_LIST={    <<< başlangıç
            // 1=ROW_TABLEIPCODE_LIST:1;                
            // .....                                    <<< bu satırlar arası silinecek
            // 1=ROWE_TABLEIPCODE_LIST:1;
            // TABLEIPCODE_LIST=};                      <<< bitiş

            // iş bitiminde 
            // 1=TABLEIPCODE_LIST:null;                 <<< şekline dönecek 

            while (thisValue.IndexOf("={") > 0)
            {
                if (thisValue[i1] == '{')
                {
                    i1--;    // ={  eşittir işareti için 
                    j2 = i1; // bitiş no
                    j1 = i1;
                    while (thisValue[j1] != ':')
                    {
                        j1--; // başlangıç no
                    }

                    if ((j1 > 0) && (j2 > 0))
                    {
                        j1++; // : sonrası başlasın diye
                        paket_fname = thisValue.Substring(j1, (j2 - j1));
                        paket_basi = paket_fname + "={";
                        paket_sonu = paket_fname + "=};" + v.ENTER;

                        j1 = thisValue.IndexOf(paket_basi);
                        j2 = thisValue.IndexOf(paket_sonu) + paket_sonu.Length;

                        thisValue = thisValue.Remove(j1, j2 - j1); // paket sil
                        thisValue = thisValue.Insert(j1, "null;" + v.ENTER); // yerine null koy

                        i2 = thisValue.Length;
                    }
                }
                i1++;
            }
            #endregion

            while (thisValue.IndexOf(lockE) > -1)
            {
                blok = t.Get_And_Clear(ref thisValue, lockE);
                caption = ev.Properties_for_List_Caption(MainFName, blok, ref Id);
                if (fcaption == string.Empty) fcaption = caption;
                listBoxControl1.Items.Add(caption);
            }

            listBoxControl1.Tag = 0;
            listBoxControl1.SelectedIndex = 0;

            tTabControl1.TabPages[0].Controls.Add(listBoxControl1);
            tTabControl1.TabPages[1].Controls.Add(memoEdit1);
            tTabControl1.TabPages[1].Controls.Add(simpleButton_BlokEkle);
            panelControl1.Controls.Add(groupControl_PropertiesPlusButtons);

        }

        ///--- OLD

        #endregion Create_Properties  <<

        #region Create AdvBandedGrid_Group_Buttons
        public void Create_AdvBandedGrid_Group_Buttons(Form tForm, Control tPanelControl,
                                                       DataRow row_Table, DataSet ds_Fields, string TableIPCode)
        {
            tToolBox t = new tToolBox();
            tDevView dv = new tDevView();

            int RefID = t.Set(row_Table["REF_ID"].ToString(), "", 0);
            //string TableIPCode = row_Table["TABLE_CODE"].ToString() + "." + row_Table["IP_CODE"].ToString();

            DevExpress.XtraEditors.PanelControl tButtonPanel = Create_GroupButtons_Panel("tAdvGridGroupButtons");

            // Add Bands >> Buton Paneli ve butonları oluştur
            dv.GridBands_Add(null, null, tButtonPanel, ds_Fields, TableIPCode);

            tPanelControl.Controls.Add(tButtonPanel);
        }
        #endregion Create AdvBandedGrid_Group_Buttons

        public DevExpress.XtraEditors.PanelControl Create_GroupButtons_Panel(string panelName)
        {
            #region Button Paneli
            // burda değişiklik yaparsan  myMenuShortKeyClick i kontrol etmeyi unutma
            //
            DevExpress.XtraEditors.PanelControl tButtonPanel = new DevExpress.XtraEditors.PanelControl();
            //buttonsPanelControl.Name = "tPanel_Navigator_" + RefID.ToString();
            tButtonPanel.Name = panelName;
            tButtonPanel.Height = 30;
            tButtonPanel.Dock = DockStyle.Top;
            tButtonPanel.SendToBack();
            tButtonPanel.TabStop = false;
            #endregion Button Paneli

            return tButtonPanel;
        }


        #region Create SpeedKriter / On/Off Kriter

        public void Create_SpeedKriter(Form tForm, Control tPanelControl,
                                       DataRow row_Table, DataSet dsFields, string MultiPageID)
        {
            tToolBox t = new tToolBox();

            int RefID = t.Set(row_Table["REF_ID"].ToString(), "", 0);
            
            string TableIPCode = row_Table["TABLE_CODE"].ToString() + "." + row_Table["IP_CODE"].ToString();
            string SoftwareCode = row_Table["SOFTWARE_CODE"].ToString();
            string ProjectCode = row_Table["PROJECT_CODE"].ToString();

            if (t.IsNotNull(SoftwareCode))
                TableIPCode = SoftwareCode + "/" + ProjectCode + "/" + TableIPCode;
            
            if (t.IsNotNull(MultiPageID))
                if (MultiPageID.IndexOf("|") == -1)
                    MultiPageID = "|" + MultiPageID;

            int i2 = dsFields.Tables[0].Rows.Count;
            int i3 = 0;
            byte toperand_type = 0;

            // SpeedKriter Tespiti yapılıyor
            for (int i = 0; i < i2; i++)
            {
                toperand_type = t.Set(dsFields.Tables[0].Rows[i]["KRT_OPERAND_TYPE"].ToString(), "", (byte)0);
                if ((toperand_type == 3) || 
                    (toperand_type == 4) || 
                    (toperand_type == 5) || 
                    (toperand_type == 21))
                    i3++;
            }

            if (i3 == 0) return;

            tDevColumn dc = new tDevColumn();

            ///'KRT_OPERAND_TYPE',    0, '', '');
            ///'KRT_OPERAND_TYPE',    1, '', 'Even (Double)');
            ///'KRT_OPERAND_TYPE',    2, '', 'Odd  (Single)');
            ///'KRT_OPERAND_TYPE',    3, '', 'Speed (Double)');
            ///'KRT_OPERAND_TYPE',    4, '', 'Speed (Single)');
            ///'KRT_OPERAND_TYPE',    5, '', 'On/Off');
            ///'KRT_OPERAND_TYPE',    9, '', 'Visible=False');
            ///'KRT_OPERAND_TYPE',   11, '', '>=');
            ///'KRT_OPERAND_TYPE',   12, '', '>');
            ///'KRT_OPERAND_TYPE',   13, '', '<=');
            ///'KRT_OPERAND_TYPE',   14, '', '<');
            ///'KRT_OPERAND_TYPE',   15, '', '<>');
            ///'KRT_OPERAND_TYPE',   16, '', 'Benzerleri (%abc%)');
            ///'KRT_OPERAND_TYPE',   17, '', 'Benzerleri (abc%)');
            ///'KRT_OPERAND_TYPE',   21, '', 'Source Value Field');
            ///
            #region tableLayoutPanel

            System.Windows.Forms.TableLayoutPanel tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            //tableLayoutPanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;
            //tableLayoutPanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            tableLayoutPanel.Name = "tPanel_SpeedKriter_" + t.AntiStr_Dot(TableIPCode) + MultiPageID; //RefID.ToString();
            tableLayoutPanel.Height = 60;
            tableLayoutPanel.ColumnCount = i3 + 1;
            tableLayoutPanel.RowCount = 2;

            //tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());

            tableLayoutPanel.Padding = new System.Windows.Forms.Padding(v.Padding4);
            tableLayoutPanel.TabIndex = 0;
            tableLayoutPanel.Dock = DockStyle.Top;
            tableLayoutPanel.SendToBack();

            tPanelControl.Controls.Add(tableLayoutPanel);

            tPanelControl.Height = 300;

            #endregion tableLayoutPanel

            #region  Tanımlar
            int row_no = 0;
            int index = 11; // Arama Texti yüzünden buradan başlıyor
            int twidth = 0;
            int defaultType = 0;
            Int16 defaultValue = 0;
            string tfield_name = string.Empty;
            string tcolumntype = string.Empty;
            string tcaption = string.Empty;
            string thint = string.Empty;
            Boolean tenabled = false;
            Boolean tvisible = false;
            #endregion

            #region // row içine Edit ler yerleştiriliyor
            foreach (DataRow Row in dsFields.Tables[0].Rows)
            {
                tvisible = t.Set(Row["CMP_VISIBLE"].ToString(), "", true);
                toperand_type = t.Set(Row["KRT_OPERAND_TYPE"].ToString(), "", (byte)0);

                if ((toperand_type == 3) || // 3,  Speed (Double)
                    (toperand_type == 4) || // 4,  Speed (Single)
                    (toperand_type == 5) || // 5,  On/Off
                    (toperand_type == 21)   // 21, Source Value Field
                    )
                {
                    //-----
                    tfield_name = t.Set(Row["LKP_FIELD_NAME"].ToString(), "", "");
                    tcolumntype = t.Set(Row["CMP_COLUMN_TYPE"].ToString(), Row["LKP_CMP_COLUMN_TYPE"].ToString(), "TextEdit");
                    tcaption = t.Set(Row["FCAPTION"].ToString(), Row["LKP_FCAPTION"].ToString(), tfield_name);
                    thint = t.Set(Row["FHINT"].ToString(), "", "");
                    tenabled = t.Set(Row["CMP_ENABLED"].ToString(), Row["LKP_FENABLED"].ToString(), true);
                    twidth = t.Set(Row["CMP_WIDTH"].ToString(), Row["LKP_CMP_WIDTH"].ToString(), 150);
                    //tgrp_sirano = t.Set(Row["KRT_LINE_NO"].ToString(), "", -1); // GROUP_LINE_NO

                    if (toperand_type == 21)   // 21, Source Value Field
                    {
                        defaultType = t.Set(Row["DEFAULT_TYPE"].ToString(), Row["LKP_DEFAULT_TYPE"].ToString(), 0);
                        defaultValue = t.Set(Row["DEFAULT_INT"].ToString(), "0", (Int16)0);
                    }

                    /// birinci nesne, Başlangıç
                    #region bir/bas
                    System.Windows.Forms.Panel item1 = new System.Windows.Forms.Panel();
                    DevExpress.XtraEditors.LabelControl labelControl1 = new DevExpress.XtraEditors.LabelControl();

                    item1.Name = "Panel_" + tfield_name;
                    item1.AccessibleName = TableIPCode + MultiPageID;
                    item1.Width = twidth;
                    labelControl1.Text = tcaption;
                    labelControl1.ForeColor = System.Drawing.SystemColors.ButtonShadow;

                    if (tcolumntype == "DateEdit") tcolumntype = "DateEdit_SpeedKriter_BAS";
                    if (tcolumntype == "tImageComboBoxEdit2Button") tcolumntype = "ImageComboBoxEdit";

                    //5, '', 'On/Off'
                    //if (toperand_type == 5) tcolumntype = "ToggleSwitch";

                    dc.tXtraEditors_Edit(Row, null, null, item1, tcolumntype, "", "", 2, toperand_type, ""); // .. ,dsData , ... , FormName

                    item1.Width = 100;// item1.Controls[0].Width + 1;
                    item1.Height = item1.Controls[0].Height + 1;

                    item1.TabStop = true;
                    item1.TabIndex = index;
                    item1.Controls[0].TabStop = true;
                    item1.Controls[0].TabIndex = index;

                    if (toperand_type == 21)   // 21, Source Value Field
                    {
                        if (tcolumntype == "ImageComboBoxEdit")
                        {
                            ((DevExpress.XtraEditors.ImageComboBoxEdit)item1.Controls[0]).EditValue = defaultValue;
                            ((DevExpress.XtraEditors.ImageComboBoxEdit)item1.Controls[0]).ReadOnly = true;
                        }

                    }

                    tableLayoutPanel.Controls.Add(labelControl1, row_no, 0);
                    tableLayoutPanel.Controls.Add(item1, row_no, 1);
                    #endregion bir/bas

                    /// ikinci nesne (bitiş)
                    #region ikinci/bit
                    if (toperand_type == 3) // 3,  Speed (Double)
                    {
                        System.Windows.Forms.Panel item2 = new System.Windows.Forms.Panel();
                        DevExpress.XtraEditors.LabelControl labelControl2 = new DevExpress.XtraEditors.LabelControl();

                        item2.Name = "Panel_" + tfield_name;
                        item2.AccessibleName = TableIPCode + MultiPageID;
                        item2.Width = twidth;
                        labelControl2.Text = "";// tcaption;
                        labelControl2.ForeColor = System.Drawing.SystemColors.ButtonShadow;

                        if (tcolumntype == "DateEdit_SpeedKriter_BAS") tcolumntype = "DateEdit_SpeedKriter_BIT";

                        dc.tXtraEditors_Edit(Row, null, null, item2, tcolumntype, "", "", 2, toperand_type, ""); // .. ,dsData , ... , FormName

                        item2.Width = item2.Controls[0].Width + 1;
                        item2.Height = item2.Controls[0].Height + 1;

                        item2.TabStop = true;
                        item2.TabIndex = index + 1;
                        item2.Controls[0].TabStop = true;
                        item2.Controls[0].TabIndex = index + 1;

                        tableLayoutPanel.ColumnCount++;

                        tableLayoutPanel.Controls.Add(labelControl2, row_no + 1, 0);
                        tableLayoutPanel.Controls.Add(item2, row_no + 1, 1);

                        row_no++;
                        index = index + 2;
                    }
                    #endregion ikinci/bit

                    //-----
                    row_no++;
                }
            }
            #endregion

        }

        #endregion Create SpeedKriter

        public void Create_GridFindPanel(Form tForm, Control tPanelControl, DevExpress.XtraGrid.GridControl cntrl)
        {
            bool find = false;

            string TableIPCode = ((DevExpress.XtraGrid.GridControl)cntrl).AccessibleName;
            string cntrlName = ((DevExpress.XtraGrid.GridControl)cntrl).Name.ToString();
            string propNavigator = "";
            if (((DevExpress.XtraGrid.GridControl)cntrl).AccessibleDescription != null)
                propNavigator = ((DevExpress.XtraGrid.GridControl)cntrl).AccessibleDescription.ToString();

            int findType = 0;
            /// ( 100 * find )  neden bunu yapıyorum ?
            /// findDelay = 100 ise standart find
            /// findDelay = 200 ise list && data  yı işaret ediyor 

            if (((DevExpress.XtraGrid.GridControl)cntrl).MainView.GetType().ToString() == "DevExpress.XtraGrid.Views.Grid.GridView")
            {
                GridView view = ((DevExpress.XtraGrid.GridControl)cntrl).MainView as GridView;
                find = (view.OptionsFind.AlwaysVisible);
                // aslını gizleyelim yerine kendi panelimizi ekleyelim
                if (find) view.OptionsFind.AlwaysVisible = false;
                findType = view.OptionsFind.FindDelay; //= 100 * find;

                //new
                //view.OptionsFind.AllowFindPanel = false;
                //view.OptionsFind.AlwaysVisible = true;
                //view.OptionsFind.FindMode = DevExpress.XtraEditors.FindMode.Always;
                view.OptionsFind.Behavior = FindPanelBehavior.Filter;
            }

            if (((DevExpress.XtraGrid.GridControl)cntrl).MainView.GetType().ToString() == "DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView")
            {
                AdvBandedGridView view = ((DevExpress.XtraGrid.GridControl)cntrl).MainView as AdvBandedGridView;
                find = (view.OptionsFind.AlwaysVisible);
                // aslını gizleyelim yerine kendi panelimizi ekleyelim
                if (find) view.OptionsFind.AlwaysVisible = false;
                findType = view.OptionsFind.FindDelay; //= 100 * find;
            }

            Create_MyFindPanel_(tForm, tPanelControl, find, TableIPCode, propNavigator, cntrlName, findType);
            
            if (tForm?.Name.ToString().IndexOf("tSearchForm") > -1)
            {
                ((DevExpress.XtraGrid.GridControl)cntrl).TabStop = false;
            }

            /// search ekranı açıldığında textEdit_Find'e focuslanması için Navigator butonlarının tabstopları false edildi
            /// eğer tekrar açmak gerekirse true yap, 
            /// Create_MyFindPanel_ için navigator butonlarını tekrar burada false yap
            /// gerekirse buraya ekleyeceksin

        }

        public void Create_TreeFindPanel(Form tForm, Control tPanelControl, DevExpress.XtraTreeList.TreeList cntrl)
        {
            bool find = false;

            string TableIPCode = ((DevExpress.XtraTreeList.TreeList)cntrl).AccessibleName;
            string cntrlName = ((DevExpress.XtraTreeList.TreeList)cntrl).Name.ToString();
            string propNavigator = "";
            if (((DevExpress.XtraTreeList.TreeList)cntrl).AccessibleDescription != null)
               propNavigator = ((DevExpress.XtraTreeList.TreeList)cntrl).AccessibleDescription.ToString();

            int findType = 0;
            /// ( 100 * find )  neden bunu yapıyorum ?
            /// findDelay = 100 ise standart find
            /// findDelay = 200 ise list && data  yı işaret ediyor 

            find = ((DevExpress.XtraTreeList.TreeList)cntrl).OptionsFind.AlwaysVisible;
            // aslını gizleyelim yerine kendi panelimizi ekleyelim
            if (find) ((DevExpress.XtraTreeList.TreeList)cntrl).OptionsFind.AlwaysVisible = false;
            findType = ((DevExpress.XtraTreeList.TreeList)cntrl).OptionsFind.FindDelay; // = 100 * find;

            Create_MyFindPanel_(tForm, tPanelControl, find, TableIPCode, propNavigator, cntrlName, findType);
        }

        private void Create_MyFindPanel_(Form tForm,
            Control tPanelControl,
            bool find,
            string TableIPCode,
            string propNavigator,
            string cntrlName,
            int findType
            )
        {
            if (find)
            {
                
                tEventsButton evb = new tEventsButton();
                //tSearch se = new tSearch();

                #region find edit create

                /// label1
                /// 
                DevExpress.XtraEditors.LabelControl label1_Find = new DevExpress.XtraEditors.LabelControl();
                label1_Find.Text = "Aradığınız ?";
                label1_Find.ForeColor = System.Drawing.SystemColors.ButtonShadow;
                /// label2
                /// 
                DevExpress.XtraEditors.LabelControl label2_Find = new DevExpress.XtraEditors.LabelControl();
                label2_Find.Text = "Arama Yöntemi";
                label2_Find.ForeColor = System.Drawing.SystemColors.ButtonShadow;

                ///
                /// textEdit_Find
                /// 
                DevExpress.XtraEditors.TextEdit textEdit_Find = new DevExpress.XtraEditors.TextEdit();

                textEdit_Find.Properties.AccessibleName = TableIPCode;
                textEdit_Find.Location = new System.Drawing.Point(30, 1);
                textEdit_Find.Size = new System.Drawing.Size(200, 20);
                textEdit_Find.Name = "textEdit_Find_" + t.AntiStr_Dot(TableIPCode); //cntrlName;
                textEdit_Find.SelectionStart = 100;


                /// Veri girişi ile arama bşalangıcı sırasındaki kullanıcının girdiği veriyi search ekranına taşımak tam istediğim gibi olmadı
                /// bu nedenle burayı çözüm bulana kadar kapattım
                /// Sorunlar nedir ?
                /// veri girişi başlayınca otomatik search ekranına gidince kullanıcı alışamadı
                /// form üzerinde veri varken tekrar search ekranını açınca haliyle searchInputValue dolu geldiği için sadece bu kayıt ekrana geliyor başaka bir kaydı aramak için
                /// kullanıcı otomatik gelen veri silmek zorunda kalıyor. onun için bu metodda sorunlu


                //if (v.tSearch.searchInputValue != "")
                //{
                //    textEdit_Find.Text = v.tSearch.searchInputValue;
                //    v.tSearch.searchInputValue = "";
                //}


                //textEdit_Find.Properties.Appearance.BackColor = v.AppearanceFocusedColor;
                //textEdit_Find.Properties.Appearance.Options.UseBackColor = true;
                textEdit_Find.Properties.Appearance.ForeColor = v.AppearanceTextColor;
                textEdit_Find.Properties.Appearance.Options.UseForeColor = true;
                textEdit_Find.Properties.AppearanceFocused.ForeColor = v.AppearanceFocusedTextColor;
                textEdit_Find.Properties.AppearanceFocused.Options.UseForeColor = true;
                textEdit_Find.Properties.NullText = "Aramak için buraya yazın";// "Aradığınızı bulmak için, kelime girin ... ";

                textEdit_Find.Enter += new System.EventHandler(evb.textEdit_Find_Enter);
                textEdit_Find.Leave += new System.EventHandler(evb.textEdit_Find_Leave);
                textEdit_Find.KeyDown += new System.Windows.Forms.KeyEventHandler(evb.textEdit_Find_KeyDown);
                textEdit_Find.KeyUp += new System.Windows.Forms.KeyEventHandler(evb.textEdit_Find_KeyUp);
                textEdit_Find.KeyPress += new System.Windows.Forms.KeyPressEventHandler(evb.textEdit_Find_KeyPress);

                textEdit_Find.EditValueChanged += new System.EventHandler(evb.textEdit_Find_EditValueChanged);

                // kullanılacak find type takibi için
                textEdit_Find.Tag = findType;
                textEdit_Find.TabIndex = 2;

                /// diğer tDevColumn lada beraber çalışması için   AccessibleDefaultActionDescription seçilmiştir.
                /// if ((tcolumn_type == "ButtonEdit") || (tcolumn_type == "tSearchEdit"))  { .. }
                ///  
                textEdit_Find.Properties.AccessibleDefaultActionDescription = TableIPCode;
                textEdit_Find.Properties.AccessibleDescription = propNavigator;

                ///
                /// tToggleSwitch
                /// 
                ToggleSwitch tToggleSwitch = new DevExpress.XtraEditors.ToggleSwitch();
                tToggleSwitch.Name = "toggleSwitch_" + cntrlName;
                tToggleSwitch.Location = new System.Drawing.Point(250, 1);
                tToggleSwitch.Size = new System.Drawing.Size(125, 24);//(200, 24);
                tToggleSwitch.Properties.AccessibleDefaultActionDescription = TableIPCode;
                tToggleSwitch.Properties.OffText = "Listeden Arama"; //"Liste üzerinde Arama";
                tToggleSwitch.Properties.OnText = "Şartlı Arama";  //"Data içinde Arama";
                tToggleSwitch.TabIndex = 3;
                tToggleSwitch.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
                
                //tToggleSwitch.IsOn = (v.search_CARI_ARAMA_TD == v.search_inData);
                /// findType = 100 ise standart find
                /// findType = 200 ise list && data  yı işaret ediyor 
                tToggleSwitch.IsOn = (findType == 200);
                tToggleSwitch.EditValueChanged += new System.EventHandler(evb.toggleSwitch_Find_EditValueChanged);
                
                // standart ise
                if (findType == 100)
                    v.search_CARI_ARAMA_TD = v.search_onList;

                /// 
                /// TableLayoutPanel
                /// 
                /// System.Windows.Forms.TableLayoutPanel 
                Control tableLayoutPanel = null;

                tableLayoutPanel = t.Find_Control(tForm, "tPanel_SpeedKriter_" + t.AntiStr_Dot(TableIPCode));

                if (tableLayoutPanel == null)
                {
                    tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
                    //((TableLayoutPanel)tableLayoutPanel).CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;
                    ((TableLayoutPanel)tableLayoutPanel).CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
                    tableLayoutPanel.Name = "tPanel_SpeedKriter_" + t.AntiStr_Dot(TableIPCode); //RefID.ToString();
                    tableLayoutPanel.Height = 60;
                    ((TableLayoutPanel)tableLayoutPanel).ColumnCount = 3;
                    ((TableLayoutPanel)tableLayoutPanel).RowCount = 2;
                    ((TableLayoutPanel)tableLayoutPanel).Dock = DockStyle.Top;
                    ((TableLayoutPanel)tableLayoutPanel).SendToBack();
                    //((TableLayoutPanel)tableLayoutPanel).BackColor = v.colorExit;

                    ((TableLayoutPanel)tableLayoutPanel).Controls.Add(label1_Find, 0, 0);
                    ((TableLayoutPanel)tableLayoutPanel).Controls.Add(textEdit_Find, 0, 1);

                    // list && Data ise
                    if (findType == 200)
                    {
                        ((TableLayoutPanel)tableLayoutPanel).Controls.Add(label2_Find, 1, 0);
                        ((TableLayoutPanel)tableLayoutPanel).Controls.Add(tToggleSwitch, 1, 1);
                    }

                    ((TableLayoutPanel)tableLayoutPanel).TabStop = true;
                    ((TableLayoutPanel)tableLayoutPanel).TabIndex = 1;

                    tPanelControl.Controls.Add(((TableLayoutPanel)tableLayoutPanel));
                }
                else
                {
                    /// SpeedKriterde yerleştirilmiş olan neseneler iki column kaydırılyor
                    /// 

                    ((TableLayoutPanel)tableLayoutPanel).ColumnCount = ((TableLayoutPanel)tableLayoutPanel).ColumnCount + 2;
                    int i2 = ((TableLayoutPanel)tableLayoutPanel).ColumnCount;

                    for (int i = i2 - 2; i >= 0; i--)
                    {
                        Control c1 = ((TableLayoutPanel)tableLayoutPanel).GetControlFromPosition(i, 0);
                        Control c2 = ((TableLayoutPanel)tableLayoutPanel).GetControlFromPosition(i, 1);

                        if (c1 != null && c2 != null)
                        {
                            ((TableLayoutPanel)tableLayoutPanel).SetColumn(c1, i + 2);  //SetRow(c2, 0);
                            ((TableLayoutPanel)tableLayoutPanel).SetColumn(c2, i + 2);  //SetRow(c1, 1);
                        }
                    }

                    // arama text i
                    ((TableLayoutPanel)tableLayoutPanel).Controls.Add(label1_Find, 0, 0);
                    ((TableLayoutPanel)tableLayoutPanel).Controls.Add(textEdit_Find, 0, 1);

                    // list && Data ise
                    if (findType == 200)
                    {
                        // arama toggleSwitch
                        ((TableLayoutPanel)tableLayoutPanel).Controls.Add(label2_Find, 1, 0);
                        ((TableLayoutPanel)tableLayoutPanel).Controls.Add(tToggleSwitch, 1, 1);
                    }
                }


                // Eğer Listeden arama aktif ise Tüme datanın dolması gerek
                if (tToggleSwitch.IsOn == false)
                    evb.InData_RunSQL(tForm, TableIPCode, "", null, null);

                /// Search açılırken bir value nin ATANMASI 
                /// yani direk olarak aranacak kelimenin açılış sırasında yüklenmesi
                /// 

                if (t.IsNotNull(v.tSearch.searchInputValue))
                {
                    if ((TableIPCode.IndexOf("3S_MSTBL") > -1) ||
                        (TableIPCode.IndexOf("3S_MSTBLIP") > -1)
                        )
                    {
                        /// bu bölüm biraz özel çözüm oldu
                        /// 
                        string softCode = "";
                        string projectCode = "";
                        string TableCode = string.Empty;
                        string IPCode = string.Empty;
                        t.TableIPCode_Get(v.tSearch.searchInputValue, ref softCode, ref projectCode, ref TableCode, ref IPCode);

                        if (TableIPCode.IndexOf("3S_MSTBL") > -1)
                            textEdit_Find.Text = TableCode;
                        if (TableIPCode.IndexOf("3S_MSTBLIP") > -1)
                            textEdit_Find.Text = IPCode;
                    }
                    else
                    {
                        /// search ekranı açıldığında textEdit_Find'e focuslanması için Navigator butonlarının tabstopları false edildi
                        /// eğer tekrar açmak gerekirse true yap, 
                        /// Create_MyFindPanel_ için navigator butonlarını tekrar burada false yap
                        /// 
                        /// genel aramalar
                        v.con_SearchTableIPCode = TableIPCode;
                        textEdit_Find.DeselectAll();
                    }

                }

                #endregion find edit create
            }

        }

        public void Create_WinExplorerBands_Add(Control tPanelControl, GridControl tGridControl)
        {
            tEvents ev = new tEvents();

            ImageComboBoxEdit tEdit = new DevExpress.XtraEditors.ImageComboBoxEdit();
            tEdit.Name = "Column_OptionsViewStyles";
            tEdit.Dock = DockStyle.Left;
            //tGridControl.AccessibleName = TableIPCode;
            tEdit.Properties.AccessibleName = tGridControl.AccessibleName;

            foreach (WinExplorerViewStyle vs in Enum.GetValues(typeof(WinExplorerViewStyle)))
            {
                DevExpress.XtraEditors.Controls.ImageComboBoxItem item =
                new DevExpress.XtraEditors.Controls.ImageComboBoxItem(vs.ToString(), vs, -1);
                tEdit.Properties.Items.Add(item);
            }

            tEdit.EditValueChanged += new System.EventHandler(ev.barEditItem_WinExplorerViewStyle_EditValueChanged);
            //new System.EventHandler(ev.myForm_Activated);

            //tEdit.EditValue = 

            /*
        if (tEdit_ICB != null)
            tEdit_ICB.Properties.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem(caption, Convert.ToInt16(value), type));


        foreach (Orientation orientation in Enum.GetValues(typeof(Orientation)))
        {
            ImageComboBoxItem cbItem = new ImageComboBoxItem(orientation.ToString(), orientation);
            repositoryItemImageComboBox1.Items.Add(cbItem);
        }
        barEditItem2.EditValue = Orientation.Horizontal;
        barEditItem2.EditValueChanged += barEditItem2_EditValueChanged;



        if (!e.Item.Checked)
            return;
        switch (e.Item.Caption)
        {
            case "Extra large icons":
                allowAsyncLoad = true;
                this.winExplorerView1.OptionsView.Style = WinExplorerViewStyle.ExtraLarge;
                break;
            case "Large icons":
                allowAsyncLoad = true;
                this.winExplorerView1.OptionsView.Style = WinExplorerViewStyle.Large;
                break;
            case "Medium icons":
                allowAsyncLoad = true;
                this.winExplorerView1.OptionsView.Style = WinExplorerViewStyle.Medium;
                break;
            case "Small icons":
                this.winExplorerView1.OptionsView.Style = WinExplorerViewStyle.Small;
                break;
            case "List":
                this.winExplorerView1.OptionsView.Style = WinExplorerViewStyle.List;
                break;
            case "Tiles":
                this.winExplorerView1.OptionsView.Style = WinExplorerViewStyle.Tiles;
                break;
            case "Content":
                this.winExplorerView1.OptionsView.Style = WinExplorerViewStyle.Content;
                break;
        }
        UpdateGridOptionsImageLoad(allowAsyncLoad);

        */
            #region Button Paneli

            DevExpress.XtraEditors.PanelControl buttonsPanelControl = new DevExpress.XtraEditors.PanelControl();
            buttonsPanelControl.Name = "tPanel_Navigator_";// + RefID.ToString();
            buttonsPanelControl.Height = 26;
            buttonsPanelControl.Dock = DockStyle.Top;
            buttonsPanelControl.SendToBack();

            buttonsPanelControl.Controls.Add(tEdit);

            #endregion Button Paneli

            tPanelControl.Controls.Add(buttonsPanelControl);
        }

        #region Create_PictureControl

        public void Create_PictureControl(Control tPanelControl,
               string TableName, string FieldName)
        {
            tToolBox t = new tToolBox();

            // DevExpress.XtraLayout.BaseLayoutItem bl_item,
            //DataRow row_Table, DataSet ds_Fields,

            //int Item_Id = 0;
            string Dock_Style = "Top";

            DevExpress.XtraEditors.PanelControl tPanelControl_Picture0 = new DevExpress.XtraEditors.PanelControl();
            DevExpress.XtraEditors.PictureEdit tPictureEdit = new DevExpress.XtraEditors.PictureEdit();
            DevExpress.XtraEditors.PanelControl tPanelControl_Picture1 = new DevExpress.XtraEditors.PanelControl();
            DevExpress.XtraEditors.PanelControl tPanelControl_Picture2 = new DevExpress.XtraEditors.PanelControl();

            DevExpress.XtraEditors.SimpleButton tSimpleButton_Delete = new DevExpress.XtraEditors.SimpleButton();
            DevExpress.XtraEditors.SimpleButton tSimpleButton_Load = new DevExpress.XtraEditors.SimpleButton();
            DevExpress.XtraEditors.SimpleButton tSimpleButton_SaveAs = new DevExpress.XtraEditors.SimpleButton();
            DevExpress.XtraEditors.SimpleButton tSimpleButton_Save = new DevExpress.XtraEditors.SimpleButton();

            DevExpress.XtraEditors.SimpleButton tSimpleButton_Full_Size = new DevExpress.XtraEditors.SimpleButton();
            DevExpress.XtraEditors.SimpleButton tSimpleButton_Fit = new DevExpress.XtraEditors.SimpleButton();
            DevExpress.XtraEditors.ZoomTrackBarControl tZoomTrackBarControl = new DevExpress.XtraEditors.ZoomTrackBarControl();

            // 
            // tPictureEdit
            // 
            tPictureEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            tPictureEdit.Location = new System.Drawing.Point(10, 29);
            //tPictureEdit.MenuManager = this.barManager1;
            tPictureEdit.Name = "tPictureEdit_" + FieldName;
            tPictureEdit.Size = new System.Drawing.Size(177, 165);
            tPictureEdit.TabIndex = 0;
            tPictureEdit.Properties.ShowZoomSubMenu = DevExpress.Utils.DefaultBoolean.True;
            tPictureEdit.Properties.ShowScrollBars = true;
            tPictureEdit.AccessibleName = FieldName;
            // 
            // tSimpleButton_Delete
            // 
            tSimpleButton_Delete.Dock = System.Windows.Forms.DockStyle.Left;
            //tSimpleButton_Delete.Location = new System.Drawing.Point(47, 2);
            tSimpleButton_Delete.Name = "tSimpleButton_Delete";
            tSimpleButton_Delete.Size = new System.Drawing.Size(32, 23);
            tSimpleButton_Delete.TabIndex = 3;
            tSimpleButton_Delete.Text = "Sil";
            tSimpleButton_Delete.AccessibleName = FieldName;
            // 
            // tSimpleButton_Load
            // 
            tSimpleButton_Load.Dock = System.Windows.Forms.DockStyle.Left;
            //tSimpleButton_Load.Location = new System.Drawing.Point(79, 2);
            tSimpleButton_Load.Name = "tSimpleButton_Load";
            tSimpleButton_Load.Size = new System.Drawing.Size(32, 23);
            tSimpleButton_Load.TabIndex = 2;
            tSimpleButton_Load.Text = "Yükle";
            tSimpleButton_Load.AccessibleName = FieldName;
            //tSimpleButton_Load.Click += new System.EventHandler(DevExpCC.btn_Picture_Load_Click);
            // 
            // tSimpleButton_SaveAs
            // 
            tSimpleButton_SaveAs.Dock = System.Windows.Forms.DockStyle.Left;
            //tSimpleButton_SaveAs.Location = new System.Drawing.Point(111, 2);
            tSimpleButton_SaveAs.Name = "tSimpleButton_SaveAs";
            tSimpleButton_SaveAs.Size = new System.Drawing.Size(32, 23);
            tSimpleButton_SaveAs.TabIndex = 1;
            tSimpleButton_SaveAs.Text = "Farklı Kaydet";
            tSimpleButton_SaveAs.AccessibleName = FieldName;
            //tSimpleButton_SaveAs.Click += new System.EventHandler(DevExpCC.btn_Picture_SaveAs_Click);
            // 
            // tSimpleButton_Save
            // 
            tSimpleButton_Save.Dock = System.Windows.Forms.DockStyle.Fill;
            //tSimpleButton_Save.Location = new System.Drawing.Point(143, 2);
            tSimpleButton_Save.Name = "tSimpleButton_Save";
            tSimpleButton_Save.Size = new System.Drawing.Size(32, 23);
            tSimpleButton_Save.TabIndex = 0;
            tSimpleButton_Save.Text = "Kaydet";
            tSimpleButton_Save.AccessibleName = FieldName;
            // 
            // tPanelControl_Picture1
            // 
            //defaultToolTipController1.SetAllowHtmlText(this.tPanelControl_Picture, DevExpress.Utils.DefaultBoolean.Default);
            tPanelControl_Picture1.Controls.Add(tSimpleButton_Save);
            tPanelControl_Picture1.Controls.Add(tSimpleButton_SaveAs);
            tPanelControl_Picture1.Controls.Add(tSimpleButton_Load);
            tPanelControl_Picture1.Controls.Add(tSimpleButton_Delete);
            tPanelControl_Picture1.Dock = System.Windows.Forms.DockStyle.Bottom;
            tPanelControl_Picture1.Location = new System.Drawing.Point(10, 194);
            tPanelControl_Picture1.Name = "tPanelControl_Picture1";
            tPanelControl_Picture1.Padding = new System.Windows.Forms.Padding(2);
            tPanelControl_Picture1.Size = new System.Drawing.Size(177, 27);
            tPanelControl_Picture1.TabIndex = 0;

            // 
            // tZoomTrackBarControl
            // 
            tZoomTrackBarControl.Dock = System.Windows.Forms.DockStyle.Fill;
            tZoomTrackBarControl.EditValue = 100;
            tZoomTrackBarControl.Value = 100;
            tZoomTrackBarControl.Location = new System.Drawing.Point(68, 4);
            //tZoomTrackBarControl.MenuManager = this.barManager1;
            tZoomTrackBarControl.Name = "tZoomTrackBarControl_" + FieldName;
            tZoomTrackBarControl.Properties.LargeChange = 25;
            tZoomTrackBarControl.Properties.Maximum = 400;
            tZoomTrackBarControl.Properties.Minimum = 10;
            tZoomTrackBarControl.Value = 100;
            tZoomTrackBarControl.Properties.ScrollThumbStyle = DevExpress.XtraEditors.Repository.ScrollThumbStyle.ArrowDownRight;
            //tZoomTrackBarControl.Properties.ValueChanged += new System.EventHandler(tZoomTrackBarControl_Properties_ValueChanged);
            tZoomTrackBarControl.Properties.SmallChange = 10;
            tZoomTrackBarControl.Size = new System.Drawing.Size(105, 16);
            tZoomTrackBarControl.AccessibleName = FieldName;
            tZoomTrackBarControl.TabIndex = 2;
            // 
            // tSimpleButton_Fit
            // 
            tSimpleButton_Fit.Dock = System.Windows.Forms.DockStyle.Left;
            tSimpleButton_Fit.Location = new System.Drawing.Point(36, 4);
            tSimpleButton_Fit.Name = "tSimpleButton_Fit";
            tSimpleButton_Fit.Size = new System.Drawing.Size(32, 16);
            tSimpleButton_Fit.TabIndex = 1;
            tSimpleButton_Fit.Text = "Fit";
            tSimpleButton_Fit.AccessibleName = FieldName;
            //tSimpleButton_Fit.Click += new System.EventHandler(DevExpCC.btn_Picture_Fit_Click);
            // 
            // tSimpleButton_Full_Size
            // 
            tSimpleButton_Full_Size.Dock = System.Windows.Forms.DockStyle.Left;
            tSimpleButton_Full_Size.Location = new System.Drawing.Point(4, 4);
            tSimpleButton_Full_Size.Name = "tSimpleButton_Full_Size";
            tSimpleButton_Full_Size.Size = new System.Drawing.Size(32, 16);
            tSimpleButton_Full_Size.TabIndex = 0;
            tSimpleButton_Full_Size.Text = "Full";
            tSimpleButton_Full_Size.AccessibleName = FieldName;
            //tSimpleButton_Full_Size.Click += new System.EventHandler(DevExpCC.btn_Picture_Full_Click);
            // 
            // tPanelControl_Picture2
            // 
            //defaultToolTipController1.SetAllowHtmlText(this.tPanelControl_Picture, DevExpress.Utils.DefaultBoolean.Default);
            tPanelControl_Picture2.Controls.Add(tZoomTrackBarControl);
            tPanelControl_Picture2.Controls.Add(tSimpleButton_Fit);
            tPanelControl_Picture2.Controls.Add(tSimpleButton_Full_Size);
            tPanelControl_Picture2.Dock = System.Windows.Forms.DockStyle.Bottom;
            tPanelControl_Picture2.Location = new System.Drawing.Point(10, 194);
            tPanelControl_Picture2.Name = "tPanelControl_Picture2";
            tPanelControl_Picture2.Padding = new System.Windows.Forms.Padding(2);
            tPanelControl_Picture2.Size = new System.Drawing.Size(177, 27);
            tPanelControl_Picture2.TabIndex = 1;
            // 
            // tGroupControl_Picture
            // 
            //defaultToolTipController1.SetAllowHtmlText(this.tGroupControl_Picture, DevExpress.Utils.DefaultBoolean.Default);
            tPanelControl_Picture0.Controls.Add(tPictureEdit);
            tPanelControl_Picture0.Controls.Add(tPanelControl_Picture2);
            tPanelControl_Picture0.Controls.Add(tPanelControl_Picture1);
            tPanelControl_Picture0.Location = new System.Drawing.Point(8, 16);
            tPanelControl_Picture0.Name = "tPanelControl_Picture0";
            tPanelControl_Picture0.Padding = new System.Windows.Forms.Padding(8);
            tPanelControl_Picture0.Size = new System.Drawing.Size(197, 251);
            tPanelControl_Picture0.TabIndex = 42;


            if (Dock_Style != string.Empty)
            {
                if (Dock_Style == "Bottom") tPanelControl_Picture0.Dock = DockStyle.Bottom;
                if (Dock_Style == "Fill") tPanelControl_Picture0.Dock = DockStyle.Fill;
                if (Dock_Style == "Left") tPanelControl_Picture0.Dock = DockStyle.Left;
                if (Dock_Style == "None") tPanelControl_Picture0.Dock = DockStyle.None;
                if (Dock_Style == "Right") tPanelControl_Picture0.Dock = DockStyle.Right;
                if (Dock_Style == "Top") tPanelControl_Picture0.Dock = DockStyle.Top;
            }

            if (tPanelControl != null)
                tPanelControl.Controls.Add(tPanelControl_Picture0);

            //if (bl_item != null)
            //    ((LayoutControlGroup)bl_item).

            /*
            if (Item_Id == sp.fr_GroupControl)
            {
                ((GroupControl)sender).Controls.Add(tPanelControl_Picture0);
            }

            if (Item_Id == sp.fr_PanelControl)
            {
                ((PanelControl)sender).Controls.Add(tPanelControl_Picture0);
            }

            if (Item_Id == sp.fr_TabPage)
            {
                ((TabPage)sender).Controls.Add(tPanelControl_Picture0);
            }

            if (Item_Id == sp.fr_DockContainer)
            {
                ((DockPanel)sender).Controls.Add(tPanelControl_Picture0);
            }

            if (Item_Id == sp.fr_DockPanel)
            {
                ((DockPanel)sender).Controls.Add(tPanelControl_Picture0);
            }
            */
        }


        #endregion Create_PictureControl

        #region Create_Parametre

        public string Create_Param_Form(DataSet dsKrtr, string TableName, Boolean tKisitli, int tTableType)
        {
            Form tForm = Create_Form(TableName, "Parametre", "");

            DevExpress.XtraEditors.PanelControl panelControl1 = new DevExpress.XtraEditors.PanelControl();
            // 
            // panelControl
            // 
            panelControl1.Name = "panelControl1";
            panelControl1.Dock = DockStyle.Fill;
            panelControl1.TabIndex = 0;
            panelControl1.Width = 225;
            panelControl1.Visible = true;

            tForm.Controls.Add(panelControl1);

            //int i = t.myInt32(Width);
            //if (i <= 100) i = 550;

            tForm.Tag = "DIALOG";
            tForm.Width = 400;//i;
            tForm.Height = 600;//i;

            return Create_Param_Panel(tForm, panelControl1, dsKrtr, TableName, tKisitli, tTableType);

        }

        public string Create_Param_Panel(Form tForm, Control panelControl1, DataSet dsKrtr,
            string TableName, Boolean tKisitli, int tTableType)
        {
            tToolBox t = new tToolBox();
            tDevView dv = new tDevView();
            tEventsGrid evg = new tEventsGrid();

            string function_name = "Create_Param";
            t.Takipci(function_name, "", '{');

            #region VievControl Create

            #region tDataControl_Krtr_Fields_

            // Sorgular için tablonun field listesi ve fieldlere ait bilgiler okunuyor

            // Sorgu için gerekeli olan bilgiler dsKrtr üzerinde bulunmaktadır
            // bu bilgilere daha sonra runtime sırasında da gerek duyulmakta
            // bu DataSet e ulaşmak için başka bir işe yaramayan bir control oluşturulmakta ve
            // bu controle bağlanmak, runtime sırasında önce bu controle onun sayesinde de bu DataSet e 
            // tekrar ulaşılmakta

            DevExpress.XtraDataLayout.DataLayoutControl DataLControl1 = new DevExpress.XtraDataLayout.DataLayoutControl();
            DataLControl1.AccessibleName = TableName;
            DataLControl1.Name = "tDataControl_Krtr_" + TableName;
            DataLControl1.Visible = false;

            if (dsKrtr != null)
                DataLControl1.DataSource = dsKrtr.Tables[0];

            #endregion tDataControl_Krtr_Fields_

            Control cntrl = new Control();
            cntrl = dv.Create_View_(v.obj_vw_VGridMulti, 0, TableName, "");

            ((VGridControl)cntrl).BeginUpdate();
            ((VGridControl)cntrl).AccessibleName = "tPARAMS_" + TableName;
            ((VGridControl)cntrl).Tag = tTableType;
            ((VGridControl)cntrl).KeyDown += new System.Windows.Forms.KeyEventHandler(evg.myVGridControl_Kriter_KeyDown);
            ((VGridControl)cntrl).KeyUp += new System.Windows.Forms.KeyEventHandler(evg.myVGridControl_Kriter_KeyUp);
            ((VGridControl)cntrl).KeyPress += new System.Windows.Forms.KeyPressEventHandler(evg.myVGridControl_Kriter_KeyPress);
            // RunTime Sırasında yapılacak işlerin listesi
            //if (t.IsNotNull(Prop_Runtime))
            //{
            //    cntrl.AccessibleDescription = Prop_Runtime;
            //}

            panelControl1.Controls.Add(DataLControl1);
            panelControl1.Controls.Add(cntrl);

            Create_Param_Columns((VGridControl)cntrl, dsKrtr, TableName, tKisitli, tTableType);

            Create_Param_Buttons((VGridControl)cntrl, TableName, tKisitli);

            ((VGridControl)cntrl).EndUpdate();


            #endregion VievControl Create

            #region dsParamValue Create


            DataSet dsParamValue = new DataSet();
            string xsql = t.RSQL_ParamFields(dsKrtr, tTableType);

            #region örnek
            /*
             * tTableType == 1 için şeklinde sql geliyor
             * 
              select t1.* , t2.* , t3.*
              from KURSIYER t1, ADRES t2, KIMLIK t3
              where t1.ID = -1  
              and t2.ID = -1  
              and t3.ID = -1 ";
             *
             * tTableType == 6 için şeklinde sql geliyor
             * 
              declare @PART_ID int
              declare @PART_NAME varchar(20)
              declare @BAS_TARIH smalldatetime
              declare @GIRIS_YOK int
              declare @GIRIS_YAPTI int

             Select 
                @PART_ID PART_ID
              , @PART_NAME PART_NAME
              , @BAS_TARIH BAS_TARIH
              , @GIRIS_YOK GIRIS_YOK
              , @GIRIS_YAPTI GIRIS_YAPTI
             * 
             */
            #endregion örnek

            t.Data_Read_Execute(tForm, dsParamValue, ref xsql, TableName + "_ParamValue", null);

            if (tTableType == 1) // Table ise
            {
                DataRow str1 = dsParamValue.Tables[0].NewRow();
                DataRow str2 = dsParamValue.Tables[0].NewRow();

                dsParamValue.Tables[0].Rows.Add(str1);
                dsParamValue.Tables[0].Rows.Add(str2);
            }

            if (tTableType == 6) // Select/TableIPCode ise
            {
                DataRow str1 = dsParamValue.Tables[0].NewRow();
                //DataRow str2 = dsParamValue.Tables[0].NewRow();

                dsParamValue.Tables[0].Rows.Add(str1);
                //dsParamValue.Tables[0].Rows.Add(str2);
            }

            ((VGridControl)cntrl).DataSource = dsParamValue.Tables[0];

            string newKriterler = string.Empty;
            string ftype = string.Empty;

            if (tForm.Tag != null)
                ftype = tForm.Tag.ToString();

            if (ftype == "DIALOG")
            {
                tFormView(tForm, 4, 1);
                newKriterler = tForm.AccessibleDefaultActionDescription;
            }

            #endregion dsWhere

            t.Takipci(function_name, "", '}');

            return newKriterler;

        }

        public void Create_Param_Columns(VGridControl tVGrid, DataSet dsKrtr,
                     string TableName, Boolean Kist, int tTableType)
        {
            tToolBox t = new tToolBox();
            string function_name = "Create_Param_Columns";
            t.Takipci(function_name, "", '{');

            //tDefaultValue df = new tDefaultValue();

            string tTableName = string.Empty;
            string FindTable = "_FULL";

            if (Kist) FindTable = "_KIST";

            #region fields list
            /*
            ,[TABLE_CODE]
      ,[FIELD_NO]
      ,[FIELD_NAME]
      ,[FIELD_TYPE]
      ,[FIELD_LENGTH]
      ,[FAUTOINC]
      ,[FNOTNULL]
      ,[FREADONLY]
      ,[FENABLED]
      ,[FVISIBLE]
      ,[FINDEX]
      ,[FPICTURE]
      ,[FCAPTION]
      ,[FHINT]
      ,[DEFAULT_TYPE]
      ,[DEFAULT_NUMERIC]
      ,[DEFAULT_TEXT]
      ,[DEFAULT_INT]
      ,[DEFAULT_SP]
      ,[DEFAULT_SETUP]
      ,[CMP_COLUMN_TYPE]
      ,[CMP_WIDTH]
      ,[CMP_DISPLAY_FORMAT]
      ,[VALIDATION_OPERATOR]
      ,[VALIDATION_VALUE1]
      ,[VALIDATION_VALUE2]
      ,[VALIDATION_ERRORTEXT]
      ,[VALIDATION_ERRORTYPE]
      ,[MASTER_TABLEIPCODE]
      ,[SEARCH_TABLEIPCODE]
      ,[MASTER_KEY_FNAME]
      ,[LIST_TYPES_NAME]
      ,[GROUP_NO]
      ,[GROUP_LINE_NO]
      ,[FJOIN_TYPE]
      ,[FJOIN_TABLE_NAME]
      ,[FJOIN_TABLE_ALIAS]
      ,[FJOIN_KEY_FNAME]
      ,[FJOIN_WHERE]
      ,[KRT_LINE_NO]
      ,[KRT_CAPTION]
      ,[KRT_OPERAND_TYPE]
      ,[KRT_LIKE]
      ,[KRT_DEFAULT1]
      ,[KRT_DEFAULT2]
      ,[KRT_ALIAS]
      ,[KRT_TABLE_ALIAS]

            */
            #endregion fields list

            int i2 = dsKrtr.Tables.Count;
            int TableNo = 0;

            for (int i = 0; i < i2; i++)
            {
                tTableName = dsKrtr.Tables[i].TableName.ToString();

                if (tTableName.IndexOf(FindTable) > 0)
                {
                    // Kısıtlı liste yok ise full hazırla
                    if ((Kist) &&
                        (dsKrtr.Tables[tTableName].Rows.Count == 0))
                    {
                        Kist = false;
                        FindTable = "_FULL";
                    }

                    Create_Param_Columns_(tVGrid, dsKrtr, tTableName, TableNo, Kist, tTableType);
                    TableNo++;


                }

            } // for (int i

            t.Takipci(function_name, "", '}');
        }


        private void Create_Param_Columns_(VGridControl tVGrid, DataSet dsKrtr,
                     string TableName, int TableNo, Boolean Kist, int tTableType)
        {
            tDevColumn dc = new tDevColumn();

            string s = string.Empty;
            string tTableName = string.Empty;
            string tTableCode = string.Empty;
            string tRowFName = string.Empty;
            string tKrtFName = string.Empty;
            string tRowCaption = string.Empty;
            string tColumnType = string.Empty;
            string tTypeName = string.Empty;
            string tValue = string.Empty;
            string paket_basi = string.Empty;
            string paket_sonu = string.Empty;
            string TableCaption = string.Empty;

            //byte tRowType = 0;
            int tCategoryNo = 0;
            //byte tRowLineNo = 0;
            Int16 tRowFieldType = 0;

            TableCaption = t.Set(dsKrtr.Tables[TableName].Namespace.ToString(), TableName, ":.");
            CategoryRow CatRow = new CategoryRow(TableCaption);
            CatRow.Name = "CategoryRow_" + TableNo.ToString();

            tVGrid.Rows.Add(CatRow);

            foreach (DataRow Row in dsKrtr.Tables[TableName].Rows)
            {
                tCategoryNo = TableNo;
                tTableName = TableName; //t.Set(Row["TABLENAME"].ToString(), "", "null");


                if (tTableType == 1) // Table ise
                {
                    tTableCode = t.Set(Row["KRT_ALIAS"].ToString(), Row["KRT_TABLE_ALIAS"].ToString(), Row["TABLE_CODE"].ToString());
                    tRowFName = t.Set(Row["FIELD_NAME"].ToString(), "", "null");
                    tKrtFName = t.Set(Row["KRT_CAPTION"].ToString(), Row["FIELD_NAME"].ToString(), "");
                    tRowCaption = t.Set(Row["FCAPTION"].ToString(), "", tRowFName);
                    tRowFieldType = t.Set(Row["FIELD_TYPE"].ToString(), "", (Int16)167);
                    tColumnType = t.Set(Row["CMP_COLUMN_TYPE"].ToString(), "", "TextEdit");
                    tTypeName = t.Set(Row["LIST_TYPES_NAME"].ToString(), "", "");
                }

                if (tTableType == 6) // TableIPCode ise
                {
                    tTableCode = t.Set(Row["LKP_KRT_ALIAS"].ToString(), Row["LKP_KRT_TABLE_ALIAS"].ToString(), "");

                    if (t.IsNotNull(tTableCode) == false)
                        tTableCode = t.Set(Row["KRT_ALIAS"].ToString(), Row["KRT_TABLE_ALIAS"].ToString(), Row["TABLE_CODE"].ToString());

                    tRowFName = t.Set(Row["LKP_FIELD_NAME"].ToString(), "", "null");
                    tKrtFName = t.Set(Row["KRT_CAPTION"].ToString(), Row["LKP_KRT_CAPTION"].ToString(), Row["LKP_FIELD_NAME"].ToString());
                    tRowCaption = t.Set(Row["FCAPTION"].ToString(), Row["LKP_FCAPTION"].ToString(), tRowFName);
                    tRowFieldType = t.Set(Row["LKP_FIELD_TYPE"].ToString(), "", (Int16)167);
                    tColumnType = t.Set(Row["CMP_COLUMN_TYPE"].ToString(), Row["LKP_CMP_COLUMN_TYPE"].ToString(), "TextEdit");
                    tTypeName = t.Set(Row["LIST_TYPES_NAME"].ToString(), Row["LKP_LIST_TYPES_NAME"].ToString(), "");
                }


                //if (tRowFName.IndexOf("LKP_") == -1)
                //{
                EditorRow EditRow = new EditorRow("EditRow_" + tRowFName);
                EditRow.Properties.Caption = tRowCaption;
                EditRow.Properties.FieldName = tRowFName;
                EditRow.Tag = tRowFieldType;
                EditRow.Properties.Value = tValue;
                if (tTableCode.IndexOf("[") == -1)
                    EditRow.Name = "[" + tTableCode + "]." + tKrtFName;
                else EditRow.Name = tTableCode + "." + tKrtFName;

                //if (tRowFName == "AD")
                //{
                //    EditRow.Properties.UnboundType = DevExpress.Data.UnboundColumnType.String;
                //    EditRow.Properties.UnboundExpression = "[AD] == ':.'";

                //}

                s = string.Empty;
                t.MyProperties_Set(ref s, "TableName", tTableName);
                t.MyProperties_Set(ref s, "FieldName", tRowFName);
                t.MyProperties_Set(ref s, "ColumnType", tColumnType);
                t.MyProperties_Set(ref s, "TypeName", tTypeName);
                t.MyProperties_Set(ref s, "Width", "0");

                dc.VGrid_ColumnEdit_(Row, EditRow, s, "", 1); // Tumu = hayır

                tVGrid.Rows["CategoryRow_" + tCategoryNo.ToString()].ChildRows.Add(EditRow);
                //}
            }
        }

        private void Create_Param_Buttons(VGridControl tVGrid, string TableIPCode, Boolean Kist)
        {
            tEvents ev = new tEvents();


            DevExpress.XtraEditors.GroupControl groupControl_ParamButtons = new DevExpress.XtraEditors.GroupControl();
            System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Param = new System.Windows.Forms.TableLayoutPanel();

            DevExpress.XtraEditors.SimpleButton simpleButton_Temizle = new DevExpress.XtraEditors.SimpleButton();
            DevExpress.XtraEditors.SimpleButton simpleButton_Collapse = new DevExpress.XtraEditors.SimpleButton();
            DevExpress.XtraEditors.SimpleButton simpleButton_Expand = new DevExpress.XtraEditors.SimpleButton();
            DevExpress.XtraEditors.SimpleButton simpleButton_Type = new DevExpress.XtraEditors.SimpleButton();
            DevExpress.XtraEditors.SimpleButton simpleButton_Tamam = new DevExpress.XtraEditors.SimpleButton();
            DevExpress.XtraEditors.SimpleButton simpleButton_Close = new DevExpress.XtraEditors.SimpleButton();

            Control cntrl = tVGrid.Parent;
            cntrl.Controls.Add(groupControl_ParamButtons);

            //this.SuspendLayout();
            //
            // groupControl_PropertiesButtons
            // 
            groupControl_ParamButtons.Controls.Add(tableLayoutPanel_Param);
            groupControl_ParamButtons.Name = "groupControl_ParamButtons";
            groupControl_ParamButtons.Size = new System.Drawing.Size(293, 52);
            groupControl_ParamButtons.TabIndex = 1;
            groupControl_ParamButtons.Text = "İşlemler";
            groupControl_ParamButtons.Dock = DockStyle.Bottom;
            // 
            // tableLayoutPanel_Properties
            // 
            //tableLayoutPanel_Param.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;
            tableLayoutPanel_Param.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            tableLayoutPanel_Param.ColumnCount = 6;
            tableLayoutPanel_Param.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            tableLayoutPanel_Param.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            tableLayoutPanel_Param.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            tableLayoutPanel_Param.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            tableLayoutPanel_Param.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            tableLayoutPanel_Param.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            tableLayoutPanel_Param.Controls.Add(simpleButton_Temizle, 0, 0);
            tableLayoutPanel_Param.Controls.Add(simpleButton_Collapse, 1, 0);
            tableLayoutPanel_Param.Controls.Add(simpleButton_Expand, 2, 0);
            tableLayoutPanel_Param.Controls.Add(simpleButton_Type, 3, 0);
            tableLayoutPanel_Param.Controls.Add(simpleButton_Tamam, 4, 0);
            tableLayoutPanel_Param.Controls.Add(simpleButton_Close, 5, 0);

            tableLayoutPanel_Param.Location = new System.Drawing.Point(2, 21);
            tableLayoutPanel_Param.Name = "tableLayoutPanel_Param";
            tableLayoutPanel_Param.RowCount = 1;
            tableLayoutPanel_Param.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel_Param.TabIndex = 0;
            tableLayoutPanel_Param.Dock = System.Windows.Forms.DockStyle.Fill;

            // 
            // simpleButton_Close
            // 
            simpleButton_Close.Dock = System.Windows.Forms.DockStyle.Top;
            simpleButton_Close.Location = new System.Drawing.Point(189, 6);
            simpleButton_Close.Name = "simpleButton_Close";
            simpleButton_Close.Size = new System.Drawing.Size(94, 23);
            simpleButton_Close.TabIndex = 1;
            simpleButton_Close.Text = "&Çıkış";

            simpleButton_Close.Click += new System.EventHandler(ev.btn_Param_Click);
            simpleButton_Close.AccessibleName = TableIPCode;
            simpleButton_Close.AccessibleDescription = tVGrid.Name.ToString();

            // 
            // simpleButton_Tamam
            // 
            simpleButton_Tamam.Dock = System.Windows.Forms.DockStyle.Top;
            simpleButton_Tamam.Location = new System.Drawing.Point(189, 6);
            simpleButton_Tamam.Name = "simpleButton_Tamam";
            simpleButton_Tamam.Size = new System.Drawing.Size(94, 23);
            simpleButton_Tamam.TabIndex = 1;
            simpleButton_Tamam.Text = "&Listele";

            simpleButton_Tamam.Click += new System.EventHandler(ev.btn_Param_Click);
            simpleButton_Tamam.AccessibleName = TableIPCode;
            simpleButton_Tamam.AccessibleDescription = tVGrid.Name.ToString();
            // 
            // simpleButton_Type
            // 
            simpleButton_Type.Dock = System.Windows.Forms.DockStyle.Top;
            simpleButton_Type.Location = new System.Drawing.Point(189, 6);
            simpleButton_Type.Name = "simpleButton_Type";
            simpleButton_Type.Size = new System.Drawing.Size(94, 23);
            simpleButton_Type.TabIndex = 2;
            if (Kist) simpleButton_Type.Text = "Daha fazla";
            else simpleButton_Type.Text = "Kısıtlı";

            simpleButton_Type.Click += new System.EventHandler(ev.btn_Param_Click);
            simpleButton_Type.AccessibleName = TableIPCode;
            simpleButton_Type.AccessibleDescription = tVGrid.Name.ToString();
            // 
            // simpleButton_Temizle
            // 
            simpleButton_Temizle.Dock = System.Windows.Forms.DockStyle.Top;
            simpleButton_Temizle.Location = new System.Drawing.Point(6, 6);
            simpleButton_Temizle.Name = "simpleButton_Clear";
            simpleButton_Temizle.Size = new System.Drawing.Size(93, 23);
            simpleButton_Temizle.TabIndex = 3;
            simpleButton_Temizle.Text = "&Temizle";

            simpleButton_Temizle.Click += new System.EventHandler(ev.btn_Param_Click);
            simpleButton_Temizle.AccessibleName = TableIPCode;
            simpleButton_Temizle.AccessibleDescription = tVGrid.Name.ToString();
            // 
            // simpleButton_Collapse
            // 
            simpleButton_Collapse.Dock = System.Windows.Forms.DockStyle.Top;
            simpleButton_Collapse.Location = new System.Drawing.Point(105, 6);
            simpleButton_Collapse.Name = "simpleButton_Collapse";
            simpleButton_Collapse.Size = new System.Drawing.Size(36, 23);
            simpleButton_Collapse.TabIndex = 4;
            simpleButton_Collapse.Text = "Kapat";

            simpleButton_Collapse.Click += new System.EventHandler(ev.btn_Param_Click);
            simpleButton_Collapse.AccessibleName = TableIPCode;
            simpleButton_Collapse.AccessibleDescription = tVGrid.Name.ToString();
            // 
            // simpleButton_Expand
            // 
            simpleButton_Expand.Dock = System.Windows.Forms.DockStyle.Top;
            simpleButton_Expand.Location = new System.Drawing.Point(147, 6);
            simpleButton_Expand.Name = "simpleButton_Expand";
            simpleButton_Expand.Size = new System.Drawing.Size(36, 24);
            simpleButton_Expand.TabIndex = 5;
            simpleButton_Expand.Text = "Aç";

            simpleButton_Expand.Click += new System.EventHandler(ev.btn_Param_Click);
            simpleButton_Expand.AccessibleName = TableIPCode;
            simpleButton_Expand.AccessibleDescription = tVGrid.Name.ToString();

        }


        #endregion Create_Parametre

        #region Create_Delete_Function

        public DialogResult Create_Delete_Form(DataSet dsData, string TableIPCode, v.tRowCount rowCount)
        {

            string TableName = "DELETE";
            string kayit = "";
            if (rowCount == v.tRowCount.SingleRow) kayit = "Kayıt";
            if (rowCount == v.tRowCount.MultiRows) kayit = "Kayıtları";

            Form tForm = Create_Form(TableName, kayit + " Silme Onayı", "");

            DevExpress.XtraEditors.PanelControl panelControl1 = new DevExpress.XtraEditors.PanelControl();
            DevExpress.XtraEditors.PanelControl panelControl2 = new DevExpress.XtraEditors.PanelControl();
            // 
            // panelControl
            // 
            panelControl1.Name = "panelControl1";
            panelControl1.Dock = DockStyle.Fill;
            panelControl1.TabIndex = 0;
            panelControl1.Width = 225;
            panelControl1.Visible = true;

            panelControl2.Name = "panelControl2";
            panelControl2.Dock = DockStyle.Bottom;
            panelControl2.TabIndex = 0;
            panelControl2.Height = 80;
            panelControl2.Visible = true;

            // 
            // labelControl1
            // 
            DevExpress.XtraEditors.LabelControl labelControl1 = new DevExpress.XtraEditors.LabelControl();
            labelControl1.Appearance.Font = new System.Drawing.Font("Tahoma", 10F);
            labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            labelControl1.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            labelControl1.LineColor = System.Drawing.SystemColors.GradientActiveCaption;
            labelControl1.LineLocation = DevExpress.XtraEditors.LineLocation.Bottom;
            labelControl1.LineVisible = true;
            labelControl1.Name = "labelControl1";
            labelControl1.Padding = new System.Windows.Forms.Padding(4);
            labelControl1.Size = new System.Drawing.Size(298, 33);
            labelControl1.TabIndex = 0;
            labelControl1.Dock = DockStyle.Top;
            labelControl1.Text = kayit + " silinecek onaylıyor musunuz ?";

            System.Windows.Forms.TableLayoutPanel tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            DevExpress.XtraEditors.SimpleButton simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            DevExpress.XtraEditors.SimpleButton simpleButton2 = new DevExpress.XtraEditors.SimpleButton();

            // 
            // tableLayoutPanel1
            //
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.ColumnCount = 5;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15.71429F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30.71429F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 7.142857F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30.71429F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15.71429F));
            tableLayoutPanel1.Controls.Add(simpleButton1, 3, 1);
            tableLayoutPanel1.Controls.Add(simpleButton2, 1, 1);
            //tableLayoutPanel1.Location = new System.Drawing.Point(524, 260);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            tableLayoutPanel1.Size = new System.Drawing.Size(295, 44);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // simpleButton1
            // 
            simpleButton1.DialogResult = System.Windows.Forms.DialogResult.No;
            simpleButton1.Dock = System.Windows.Forms.DockStyle.Fill;
            //simpleButton1.Image = ((System.Drawing.Image)(resources.GetObject("simpleButton1.Image")));
            simpleButton1.Location = new System.Drawing.Point(160, 11);
            simpleButton1.Name = "simpleButton1";
            simpleButton1.Size = new System.Drawing.Size(99, 20);
            simpleButton1.TabIndex = 0;
            simpleButton1.Text = "Hayır";
            // 
            // simpleButton2
            // 
            simpleButton2.DialogResult = System.Windows.Forms.DialogResult.Yes;
            simpleButton2.Dock = System.Windows.Forms.DockStyle.Fill;
            //simpleButton2.Image = ((System.Drawing.Image)(resources.GetObject("simpleButton2.Image")));
            simpleButton2.Location = new System.Drawing.Point(34, 11);
            simpleButton2.Name = "simpleButton2";
            simpleButton2.Size = new System.Drawing.Size(99, 20);
            simpleButton2.TabIndex = 1;
            simpleButton2.Text = "Evet";

            panelControl2.Controls.Add(tableLayoutPanel1);
            panelControl2.Controls.Add(labelControl1);

            // Define the border style of the form to a dialog box.
            tForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            // Set the accept button of the form to button1.
            tForm.AcceptButton = simpleButton2;
            // Set the cancel button of the form to button2.
            tForm.CancelButton = simpleButton1;

            tForm.Controls.Add(panelControl2);
            tForm.Controls.Add(panelControl1);

            tDevView dv = new tDevView();
            Control cntrl = new Control();

            if (rowCount == v.tRowCount.SingleRow)
                cntrl = dv.Create_View_(v.obj_vw_VGridSingle, 0, TableName, "");
            if (rowCount == v.tRowCount.MultiRows)
                cntrl = dv.Create_View_(v.obj_vw_GridView, 0, TableName, "");

            DataSet ds_Table = new DataSet();
            DataSet ds_Fields = new DataSet();

            tTablesRead tr = new tTablesRead();

            tr.MS_Tables_IP_Read(ds_Table, TableIPCode);
            tr.MS_Fields_IP_Read(ds_Fields, TableIPCode);

            DataRow row_Table = ds_Table.Tables[0].Rows[0] as DataRow;

            if (rowCount == v.tRowCount.SingleRow)
                dv.tVGrid_Create(row_Table, ds_Fields, dsData, (DevExpress.XtraVerticalGrid.VGridControl)cntrl, 1);

            if (rowCount == v.tRowCount.MultiRows)
            {
                bool tabStop = false;
                dv.tGridView_Create(row_Table, ds_Fields, dsData, (DevExpress.XtraGrid.GridControl)cntrl, TableIPCode, ref tabStop);
            }

            panelControl1.Controls.Add(cntrl);

            tForm.Tag = "DIALOG";

            if (rowCount == v.tRowCount.SingleRow)
            {
                tForm.Width = 500;
                tForm.Height = 400;
            }
            if (rowCount == v.tRowCount.MultiRows)
            {
                tForm.Width = 1100;
                tForm.Height = 600;
            }

            tFormView(tForm, 4, 1);
            DialogResult cevap = tForm.DialogResult;

            return cevap;
        }

        #endregion Create_Delete_Function

        #region // tMyFormBox Create -- Her formun üzerindeki gizli bir memo ekleniyor
        public void Create_MyFormBox(Form tForm, string myFormLoadValue)
        {
            // Bu memo üzerine Form create sırasında gerekli olan bilgiler Set ediliyor
            // Formun için create olan IP ler buraya bakarak gerekli olan bilgileri Get edebiliyor
            if (tForm == null) return;

            tToolBox t = new tToolBox();
            string[] controls = new string[] { };
            Control c = t.Find_Control(tForm, "tMyFormBox", "", controls);

            if (c == null)
            {
                DevExpress.XtraEditors.MemoEdit tMyFormBox = new DevExpress.XtraEditors.MemoEdit();

                tMyFormBox.Visible = v.myFormBox_Visible;
                tMyFormBox.Name = "tMyFormBox_" + tForm.Name;
                tMyFormBox.Dock = DockStyle.Left;
                tMyFormBox.Size = new System.Drawing.Size(320, 500);
                tMyFormBox.Parent = tForm;
                tForm.Controls.Add(tMyFormBox);

                if (t.IsNotNull(myFormLoadValue))
                    tMyFormBox.EditValue = myFormLoadValue;
            }
        }
        #endregion


        public void Create_AlertForm()
        {
            /*
            using DevExpress.XtraBars.Alerter;

            // Create a regular custom button.
            AlertButton btn1 = new AlertButton(Image.FromFile(@"c:\folder-16x16.png"));
            btn1.Hint = "Open file";
            btn1.Name = "buttonOpen";
            // Create a check custom button.
            AlertButton btn2 = new AlertButton(Image.FromFile(@"c:\clock-16x16.png"));
            btn2.Style = AlertButtonStyle.CheckButton;
            btn2.Down = true;
            btn2.Hint = "Alert On";
            btn2.Name = "buttonAlert";
            // Add buttons to the AlertControl and subscribe to the events to process button clicks
            alertControl1.Buttons.Add(btn1);
            alertControl1.Buttons.Add(btn2);
            alertControl1.ButtonClick += new AlertButtonClickEventHandler(alertControl1_ButtonClick);
            alertControl1.ButtonDownChanged +=
                new AlertButtonDownChangedEventHandler(alertControl1_ButtonDownChanged);

            // Show a sample alert window.
            AlertInfo info = new AlertInfo("New Window", "Text");
            alertControl1.Show(this, info);

            void alertControl1_ButtonDownChanged(object sender,
            AlertButtonDownChangedEventArgs e) {
                if (e.ButtonName == "buttonOpen")
                {
        //...
    }
            }

void alertControl1_ButtonClick(object sender, AlertButtonClickEventArgs e) {
    if (e.ButtonName == "buttonAlert")
    {
        //...
    }
}

            */
        }
    }
}
