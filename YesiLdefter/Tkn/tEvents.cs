using DevExpress.Xpo.DB;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraDataLayout;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.WinExplorer;
using DevExpress.XtraVerticalGrid;
using Microsoft.JScript;
using Newtonsoft.Json;
using Spire.Doc.Documents;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Tkn_CreateObject;
using Tkn_DataCopy;
using Tkn_DefaultValue;
using Tkn_Forms;
using Tkn_InputPanel;
using Tkn_Layout;
using Tkn_Save;
using Tkn_Search;
using Tkn_SMS;
using Tkn_SQLs;
using Tkn_TablesRead;
using Tkn_ToolBox;
using Tkn_Variable;

namespace Tkn_Events
{

    public class tEvents : tBase
    {
        tToolBox t = new tToolBox();
        tSearch se = new tSearch();

        #region dataNavigator_PositionChanged, dataSet_Row

        #region dataNavigator
        public void dataNavigator_Enter(object sender, EventArgs e)
        {
            //((DevExpress.XtraEditors.DataNavigator)sender).BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            ((DevExpress.XtraEditors.DataNavigator)sender).Appearance.Options.UseBackColor = true;
        }

        public void dataNavigator_Leave(object sender, EventArgs e)
        {
            //((DevExpress.XtraEditors.DataNavigator)sender).BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Default;
            ((DevExpress.XtraEditors.DataNavigator)sender).Appearance.Options.UseBackColor = false;
        }

        public void dataNavigator_PositionChanged(object sender, EventArgs e)
        {
            #region Giriş kontrolleri
            // LKP_ONAY için ise işlem olmasın
            if ((v.con_OnayChange) || (v.con_PositionChange)) return;
            // 
            if (v.con_Cancel == true)
            {
                v.con_Cancel = false;
                return;
            }
            #endregion
            
            //tToolBox t = new tToolBox();

            DataNavigator dN = ((DevExpress.XtraEditors.DataNavigator)sender);
            object tDataTable = dN.DataSource;
            DataSet dsData = ((DataTable)tDataTable).DataSet;

            int NewPosition = dN.Position;
            if (NewPosition == -1) return;
            DataRow dtRow = dsData.Tables[0].Rows[NewPosition];

            //((DataTable)tDataTable).Columns[""].Expression

            //dsData.Tables[0].Columns[""].Expression

            #region Detail-SubDetail Table
            // Kendisine bağlı subdetail var ise
            if (dN.IsAccessible == true)
            {
                if (dN.Position > -1)
                {
                    if (dtRow.RowState != DataRowState.Deleted)
                    {
                        Form tForm = t.Find_Form(sender);
                        string TableIPCode = dsData.DataSetName;

                        // eski süreç ** dataNavigator_PositionChanged
                        //
                        Data_Refresh(tForm, dsData, ((DevExpress.XtraEditors.DataNavigator)sender)); // dataNavigator_PositionChanged

                        // yenisi bir türlü tree de başarılı olmadı sebebini bulamadım

                        // Yeni Süreç ----
                        // 
                        //vSubWork vSW = new vSubWork();
                        //vSW._01_tForm = tForm;
                        //vSW._02_TableIPCode = "";
                        //vSW._03_WorkTD = v.tWorkTD.Refresh;
                        //vSW._04_WorkWhom = v.tWorkWhom.All;
                        //tSubWork_(vSW);
                    }
                }

            }
            #endregion Detail-SubDetail Table

            // yeni kayıt ise
            if (v.con_NewRecords)
            {
                v.con_NewRecords = false;
                ((DevExpress.XtraEditors.DataNavigator)sender).Tag = ((DevExpress.XtraEditors.DataNavigator)sender).Position;
                return;
            }

            #region HasChanges
            //if ((dsData.HasChanges()) &&
            //    (v.con_LkpOnayChange == false)) // silme işlemi değil ise
            object x = ((DevExpress.XtraEditors.DataNavigator)sender).Tag;

            int OldPosition = ((int)x);

            DataRowState state = checkedChangeValue(dsData, OldPosition);
            //DataRowState state = dsData.Tables[0].Rows[OldPosition].RowState;

            /// LKP_ONAY değişikliği var mı kontrol et
            /// 
            v.con_LkpOnayChange = t.onlyChanged_Lkp_Onay_Field(dsData, OldPosition);


            if ((state == DataRowState.Modified) &&
                (v.con_LkpOnayChange == false)) // silme işlemi değil ise
            {
                //object x = ((DevExpress.XtraEditors.DataNavigator)sender).Tag;

                //int OldPosition = ((int)x);

                // -97 Delete işlemine işaret ediyor ( Rows[position].Delete() )

                // lock row ise işlem başlamasın 
                if (t.checkedIsLockRow(dsData, OldPosition)) 
                    OldPosition = -1;

                if ((OldPosition != -97) && // Değil ise 
                    (OldPosition > -1))
                {

                    string TableIPCode = dsData.DataSetName;

                    Form tForm = t.Find_Form(sender);

                    tDefaultValue df = new tDefaultValue();
                    if (df.tDefaultValue_And_Validation(
                            tForm,
                            dsData,
                            dsData.Tables[0].Rows[OldPosition],
                            TableIPCode,
                            "tdataNavigator_PositionChanged") == true) // Table Save 
                    {
                        tSave sv = new tSave();

                        sv.tDataSave(tForm, dsData, ((DevExpress.XtraEditors.DataNavigator)sender), OldPosition);

                        v.SQL = v.ENTER + "tdataNavigator_PositionChanged [ " + TableIPCode + " ]" + v.SQL;
                        try
                        {
                            // sebebini anlamadım, fakat yukarıda tTable_Update functiona gidince orada 
                            // ds.Tables[0].AcceptChanges(); komutundan sonra position değerini unutuyor onun için  
                            // buraya geldiği sıradaki positon değeri kendisine tekrar hatırlatılıyor.

                            // ve bu yeniden hatırlatma eğer table Insert değil ise yani Edit state modunda ise yapılıyor
                            string ix = dsData.Tables[0].Rows[OldPosition][0].ToString();
                            if (ix != "")
                                ((DevExpress.XtraEditors.DataNavigator)sender).Position = NewPosition;
                        }
                        catch
                        {
                            // burada hata olarak yakalanır
                        }
                    }
                    else
                    {
                        // işlem onaylanmadı ise
                        //t.ButtonEnabledAll(tForm, TableIPCode, true);

                        v.Kullaniciya_Mesaj_Var = "İPTAL : Kayıt işleminden vazgeçildi ....";
                    }
                }
            }

            if (v.con_LkpOnayChange)
            {
                /// LKP_ONAY değişikliği anlaşıdı ve save ye gitmesi engellendi
                /// onun için görevi bitti
                v.con_LkpOnayChange = false;
            }

            #endregion HasChanges

            ((DevExpress.XtraEditors.DataNavigator)sender).Tag = ((DevExpress.XtraEditors.DataNavigator)sender).Position;

            dsData.Dispose();

            /// her kayıt satırı değiştiğinde otomatik arama aktif olsun
            v.tSearch.AutoSearch = true;

            //t.Takipci(function_name, "", '}');

        }

        private DataRowState checkedChangeValue(DataSet dsData, int position)
        {
            DataRowState state = DataRowState.Unchanged;

            if (position < 0) return state;

            int colCount = dsData.Tables[0].Columns.Count;

            string fieldName = "";
            string orjValue = "";
            //string curValue = "";
            string value = "";

            int count = dsData.Tables[0].Rows.Count;
            if (position >= count) position = count - 1;

            DataRow row = dsData.Tables[0].Rows[position];

            for (int i = 0; i < colCount; i++)
            {
                fieldName = dsData.Tables[0].Columns[i].ColumnName;
                try
                {
                    orjValue = row[fieldName, DataRowVersion.Original].ToString();
                    //curValue = row[fieldName, DataRowVersion.Current].ToString();
                    value = row[fieldName].ToString();
                }
                catch (Exception)
                {
                    break;
                    //throw;
                }

                //if (!row[fieldName, DataRowVersion.Original].Equals(row[fieldName, DataRowVersion.Current]))
                
                if (!orjValue.Equals(value))
                {
                    //string oldValue = row[fieldName, DataRowVersion.Original].ToString();
                    //string newValue = row[fieldName, DataRowVersion.Current].ToString();
                    state = DataRowState.Modified;
                    break;
                }

            }

            return state;
        }

        
        #endregion dataNavigator

        #region tDataSet Row Events

        public void tRow_Changed(object sender, DataRowChangeEventArgs e)
        {
            //Console.WriteLine("Row_Changed Event: name={0}; action={1}",
            //    e.Row.Table.TableName, e.Action);
            //    e.Row["name"], e.Action);
            //v.Kullaniciya_Mesaj_Var = String.Format("Row_Changed Event: name={0}; action={1}",
            //    e.Row.Table.TableName, e.Action);


            //ds.Tables[Table_Name].Rows[Position].CancelEdit();
            //((DataTable)sender).DataSet.Tables[0].Rows[]

            //e.Row.BeginEdit();
            tSetDataStateChanges(sender, null, "");
        }

        public void tRow_Changing(object sender, DataRowChangeEventArgs e)
        {
            //Console.WriteLine("Row_Changing Event: name={0}; action={1}",
            //    e.Row.Table.TableName, e.Action);
            //v.Kullaniciya_Mesaj_Var = String.Format("Row_Changing Event: name={0}; action={1}",
            //      e.Row.Table.TableName, e.Action);
        }

        public void tRow_Deleted(object sender, DataRowChangeEventArgs e)
        {
            //Console.WriteLine("Row_Deleted Event: name={0}; action={1}",
            //    e.Row.Table.TableName, e.Action);
            //e.Row["name", DataRowVersion.Original], e.Action);
            v.Kullaniciya_Mesaj_Var = String.Format("Row_Deleted Event: name={0}; action={1}",
                e.Row.Table.TableName, e.Action);
        }

        public void tRow_Deleting(object sender, DataRowChangeEventArgs e)
        {
            //Console.WriteLine("Row_Deleting Event: name={0}; action={1}",
            //    e.Row.Table.TableName, e.Action);
            //e.Row["name"], e.Action);
            v.Kullaniciya_Mesaj_Var = String.Format("Row_Deleting Event: name ={0}; action ={1}", e.Row.Table.TableName, e.Action);
        }

        public void tColumn_Changed(object sender, DataColumnChangeEventArgs e)
        {
            //v.Kullaniciya_Mesaj_Var = String.Format("Column_Changed Event: ColumnName={0}; RowState={1}",
            //    e.Column.ColumnName, e.Row.RowState);
            DataRow row = e.Row;
            string columnName = e.Column.ColumnName;
            
            tSetDataStateChanges(sender, row, columnName);
        }

        public void tColumn_Changing(object sender, DataColumnChangeEventArgs e)
        {
            //Console.WriteLine("Column_Changing Event: ColumnName={0}; RowState={1}",
            //    e.Column.ColumnName, e.Row.RowState);
            //v.Kullaniciya_Mesaj_Var = String.Format("Column_Changing Event: ColumnName={0}; RowState={1}",
            //    e.Column.ColumnName, e.Row.RowState);
        }

        public void tTable_NewRow(object sender, DataTableNewRowEventArgs e)
        {
            //Console.WriteLine("Table_NewRow Event: RowState={0}",
            //    e.Row.RowState.ToString());
            //v.Kullaniciya_Mesaj_Var = String.Format("Table_NewRow Event: RowState={0}",  e.Row.RowState.ToString());
        }

        public void tTable_Cleared(object sender, DataTableClearEventArgs e)
        {
            //Console.WriteLine("Table_Cleared Event: TableName={0}; Rows={1}",
            //    e.TableName, e.Table.Rows.Count.ToString());
            //v.Kullaniciya_Mesaj_Var = String.Format("Table_Cleared Event: TableName={0}; Rows={1}",  e.TableName, e.Table.Rows.Count.ToString());
        }

        public void tTable_Clearing(object sender, DataTableClearEventArgs e)
        {
            //Console.WriteLine("Table_Clearing Event: TableName={0}; Rows={1}",
            //    e.TableName, e.Table.Rows.Count.ToString());
            //v.Kullaniciya_Mesaj_Var = String.Format("Table_Clearing Event: TableName={0}; Rows={1}", e.TableName, e.Table.Rows.Count.ToString());
        }

        public bool tGetDataChanges(Form tForm)
        {
            bool onay = false;
            //tToolBox t = new tToolBox();

            #region DataNavigator Listesi Hazırlanıyor

            List<string> list = new List<string>();
            t.Find_DataNavigator_List(tForm, ref list);

            #region DataNavigator Listesi
            Control cntrl = new Control();
            string[] controls = new string[] { "DevExpress.XtraEditors.DataNavigator" };

            v.Kullaniciya_Mesaj_Var = "";
            foreach (string value in list)
            {
                cntrl = t.Find_Control(tForm, value, "", controls);
                if (cntrl != null)
                {
                    DataNavigator dN = (DevExpress.XtraEditors.DataNavigator)cntrl;

                    if (dN.DataSource != null)
                    {
                        object tDataTable = dN.DataSource;
                        DataSet dsData = ((DataTable)tDataTable).DataSet;

                        if (dsData.Namespace != null)
                        {
                            if (dsData.Namespace.IndexOf(v.dataStateUpdate) > -1)
                            {
                                onay = true;
                                break;
                            }
                        }
                    }

                } // if cntrl != null
            }//foreach

            #endregion DataNavigator Listesi

            #endregion

            return onay;
        }

        private void tSetDataStateChanges(object sender, DataRow row, string columnName)
        {
            
            //
            if (v.con_DefaultValuePreparing) return;
            // LKP_ONAY için ise işlem olmasın
            if ((v.con_OnayChange) || (v.con_PositionChange)) return;

            if (((DataTable)sender).DataSet.Namespace != null)
            {
                if (((DataTable)sender).DataSet.Namespace.IndexOf(v.dataStateNull) > -1)
                {
                    //tToolBox t = new tToolBox();
                    DataSet ds = ((DataTable)sender).DataSet;
                    t.mypropChanged(ref ds, v.dataStateNull, v.dataStateUpdate);
                }

                /// Demek ki MasterDetailColmns fields mevcut
                if (t.IsNotNull(columnName) &&
                    (v.con_ColumnChangesCount == 0) &&
                    ((DataTable)sender).DataSet.Namespace.IndexOf(v.masterDetailColumns) == -1)
                {
                    string myProp = ((DataTable)sender).DataSet.Namespace;
                    string columnNames = t.MyProperties_Get(myProp, "MasterDetailColumns:");
                    if (columnNames.IndexOf(columnName) > -1)
                    {
                        //string value = row[columnName].ToString();
                        //v.Kullaniciya_Mesaj_Var = String.Format("Column_Changed Event: ColumnName={0}; value={1}", columnName, value);
                        string formName = t.MyProperties_Get(myProp, "FormName:");
                        string tableIPCode = ((DataTable)sender).DataSet.DataSetName;

                        Form tForm = Application.OpenForms[formName];

                        DataSet ds = null;
                        DataNavigator dN = null;
                        t.Find_DataSet(tForm, ref ds, ref dN, tableIPCode);

                        Data_Refresh(tForm, ds, dN);

                        /// DatoCopy ve Save işlemleri sırasında column change yüzünden aynı row için birden fazla aynı datayı okuyup duruyor
                        /// bunu önlemek için 1 defa çalışması sağlanıyor
                        /// 
                        v.con_ColumnChangesCount += 1; 
                    }
                }
            }
        }


        #endregion tDataSet

        #endregion dataNavigator_PositionChanged, dataSet_Row

        #region 201910 new events SubFunction

        public v.tButtonType getClickType(string myProp)
        {
            //tToolBox t = new tToolBox();
            v.tButtonType propButtonType = v.tButtonType.btNone;
            PROP_NAVIGATOR prop_ = null;
            List<PROP_NAVIGATOR> propList_ = null;

            t.readProNavigator(myProp, ref prop_, ref propList_);

            if (propList_ != null)
            {
                foreach (PROP_NAVIGATOR item in propList_)
                {
                    // ilk bulduğu type gönderir
                    if (item.BUTTONTYPE.ToString() != "null")
                    {
                        return propButtonType = t.getClickType(System.Convert.ToInt32(item.BUTTONTYPE.ToString()));
                    }
                }
            }

            return propButtonType;
        }
        public v.tButtonType getClickType(int value)
        {
            if (value == 0) return v.tButtonType.btNone;
            if (value == 1) return v.tButtonType.btEnter;
            if (value == 2) return v.tButtonType.btEscape;
            if (value == 11) return v.tButtonType.btCikis;
            if (value == 999) return v.tButtonType.btCikis;

            if (value == 68) return v.tButtonType.btSihirbazDevam;
            if (value == 69) return v.tButtonType.btSihirbazGeri;

            if (value == 12) return v.tButtonType.btSecCik;
            if (value == 13) return v.tButtonType.btListeyeEkle;
            if (value == 14) return v.tButtonType.btListeHazirla;
            if (value == 15) return v.tButtonType.btListele;

            if (value == 21) return v.tButtonType.btKaydetYeni;
            if (value == 22) return v.tButtonType.btKaydet;
            if (value == 23) return v.tButtonType.btKaydetDevam;
            if (value == 24) return v.tButtonType.btKaydetCik;

            if (value == 31) return v.tButtonType.btYeniKart;
            if (value == 32) return v.tButtonType.btYeniHesap;
            if (value == 33) return v.tButtonType.btYeniBelge;
            if (value == 34) return v.tButtonType.btYeniAltHesap;

            if (value == 36) return v.tButtonType.btYeniKartSatir;
            if (value == 37) return v.tButtonType.btYeniHesapSatir;
            if (value == 38) return v.tButtonType.btYeniBelgeSatir;
            if (value == 39) return v.tButtonType.btYeniAltHesapSatir;

            if (value == 41) return v.tButtonType.btGoster;
            if (value == 42) return v.tButtonType.btKartAc;
            if (value == 43) return v.tButtonType.btHesapAc;
            if (value == 44) return v.tButtonType.btBelgeAc;
            if (value == 45) return v.tButtonType.btResimEditor;
            if (value == 46) return v.tButtonType.btReportDesign;

            // unutma
            if (value == 51) return v.tButtonType.btSilSatir;
            if (value == 52) return v.tButtonType.btSilKart;
            if (value == 53) return v.tButtonType.btSilHesap;
            if (value == 54) return v.tButtonType.btSilBelge;
            if (value == 55) return v.tButtonType.btSilListe;

            if (value == 61) return v.tButtonType.btEnSona;
            if (value == 62) return v.tButtonType.btSonrakiSayfa;
            if (value == 63) return v.tButtonType.btSonraki;
            if (value == 65) return v.tButtonType.btOnceki;
            if (value == 66) return v.tButtonType.btOncekiSayfa;
            if (value == 67) return v.tButtonType.btEnBasa;

            if (value == 71) return v.tButtonType.btCollapse;
            if (value == 72) return v.tButtonType.btExpanded;

            if (value == 73) return v.tButtonType.btOnayEkle;
            if (value == 74) return v.tButtonType.btOnayKaldir;

            if (value == 81) return v.tButtonType.btYazici;

            if (value == 91) return v.tButtonType.btEk1;
            if (value == 92) return v.tButtonType.btEk2;
            if (value == 93) return v.tButtonType.btEk3;
            if (value == 94) return v.tButtonType.btEk4;
            if (value == 95) return v.tButtonType.btEk5;
            if (value == 96) return v.tButtonType.btEk6;
            if (value == 97) return v.tButtonType.btEk7;

            if (value == 121) return v.tButtonType.btArama;
            if (value == 122) return v.tButtonType.btFormulleriHesapla;
            if (value == 123) return v.tButtonType.btDataTransferi;
            if (value == 124) return v.tButtonType.btInputBox;
            if (value == 125) return v.tButtonType.btOpenSubView;
            if (value == 126) return v.tButtonType.btExtraIslem;
            if (value == 127) return v.tButtonType.btFindListData;

            return v.tButtonType.btNone;
        }

        public v.tButtonType getButtonNameClickType(string buttonName)
        {
            int value = 0;

            int i1 = buttonName.IndexOf("simpleButton_") + 13;
            string values = buttonName.Remove(0, i1);
            try
            {
                value = System.Convert.ToInt32(values);
                value = value - 1000;
            }
            catch (Exception)
            {
                value = 0;
                //throw;
            }

            return t.getClickType(value);
        }


        
        public v.tButtonType getClickType(Form tForm, string TableIPCode,
            KeyEventArgs e, ref string propNavigator, ref string buttonName)
        {
            //tToolBox t = new tToolBox();

            if (e.KeyCode == Keys.Escape) return v.tButtonType.btEscape;
            if (e.KeyCode == v.Key_SearchEngine) return v.tButtonType.btArama;

            /// button üzerindeki key Mesajı ( 'Kaydet F3' ) kontrol edilerek 
            /// hangi buton click lenmek isteniyor, tespit edilior 
            /// 
            int value = 0;
            string keyText = e.KeyCode.ToString();

            Control tPanelNavigator = null;

            tPanelNavigator = t.Find_Control(tForm, "tPanel_Navigator_" + t.AntiStr_Dot(TableIPCode));

            if (tPanelNavigator != null)
            {
                //if (keyText == "Return")
                //    keyText = "Enter";
                if (keyText == "Delete")
                    keyText = "Del";

                string s = "";
                foreach (var item in tPanelNavigator.Controls)
                {
                    s = item.GetType().ToString();
                    if (s == "DevExpress.XtraEditors.SimpleButton")
                    {
                        if (((DevExpress.XtraEditors.SimpleButton)item).Text.IndexOf(keyText) > -1)
                        {
                            buttonName = ((DevExpress.XtraEditors.SimpleButton)item).Name;
                            value = ((DevExpress.XtraEditors.SimpleButton)item).TabIndex;

                            if (((DevExpress.XtraEditors.SimpleButton)item).AccessibleDescription != null)
                                propNavigator = ((DevExpress.XtraEditors.SimpleButton)item).AccessibleDescription;

                            return t.getClickType(value);
                        }
                    }
                }
                /// BelgeStokB headeri hatırla
                /// bu istekler var fakat butonlar görükmediği için bulamıyor ama bu istekler var
                if (e.KeyCode == v.Key_ExtraIslem)
                {
                    /*
                    DataSet ds = null;
                    DataNavigator dN = null;
                    t.Find_DataSet(tForm, ref ds, ref dN, TableIPCode);
                    string myProp = ds.Namespace.ToString();
                    if (t.IsNotNull(myProp))
                    {
                        //propNavigator = t.MyProperties_Get(myProp, "PropNavigator:");
                    }
                    */
                }
                if (e.KeyCode == v.Key_Yeni) return v.tButtonType.btYeniKartSatir;
                if (e.KeyCode == v.Key_Kaydet) return v.tButtonType.btKaydet;
                if (e.KeyCode == v.Key_KaydetYeni) return v.tButtonType.btKaydetYeni;
                if (e.KeyCode == v.Key_ExtraIslem) return v.tButtonType.btExtraIslem;
                if (e.KeyCode == Keys.None)
                {
                    propNavigator = "";
                    return v.tButtonType.btNoneButton;// 
                }
            }

            // yukarıda bulamazsa O yani None dönecek;
            return t.getClickType(value);
        }

        public void viewControlFocusedValue(Form tForm, string TableIPCode)
        {
            #region 
            //tToolBox t = new tToolBox();

            Control viewcntrl = t.Find_Control_View(tForm, TableIPCode);

            if (viewcntrl != null)
            {
                if (viewcntrl.GetType().ToString() == "DevExpress.XtraGrid.GridControl")
                {
                    if (((DevExpress.XtraGrid.GridControl)viewcntrl).MainView.GetType().ToString() == "DevExpress.XtraGrid.Views.Grid.GridView")
                    {
                        GridView view = ((DevExpress.XtraGrid.GridControl)viewcntrl).MainView as GridView;
                        if (view.FocusedValue != null)
                            view.SetFocusedValue(view.FocusedValue.ToString());

                        /// satır / row değiştirme işlemi yapıyor ileride gerekebilir
                        ///if (view.IsLastRow)
                        ///    view.MovePrev();
                        ///else view.MoveNext();
                    }
                    if (((DevExpress.XtraGrid.GridControl)viewcntrl).MainView.GetType().ToString() == "DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView")
                    {
                        AdvBandedGridView view = ((DevExpress.XtraGrid.GridControl)viewcntrl).MainView as AdvBandedGridView;
                        if (view.FocusedValue != null)
                            view.SetFocusedValue(view.FocusedValue.ToString());
                    }
                }
                if (viewcntrl.GetType().ToString() == "DevExpress.XtraTreeList.TreeList")
                {
                    //myObject = ((DevExpress.XtraTreeList.TreeList)viewcntrl).Parent;
                }
                if (viewcntrl.GetType().ToString() == "DevExpress.XtraDataLayout.DataLayoutControl")
                {
                    //myObject = ((DevExpress.XtraDataLayout.DataLayoutControl)viewcntrl).Parent;
                }
                if (viewcntrl.GetType().ToString() == "DevExpress.XtraVerticalGrid.VGridControl")
                {
                    //myObject = ((DevExpress.XtraVerticalGrid.VGridControl)viewcntrl).Parent;
                }
            }

            //((DevExpress.XtraGrid.GridControl)viewcntrl).da

            #endregion
        }

        public void setMenuPage(Form tForm, string menuValue)
        {

            // Ribbon page visible / arama 
            //tToolBox t = new tToolBox();
            Control menu = null;
            string[] controls2 = new string[] { "DevExpress.XtraBars.Ribbon.RibbonControl" };

            menu = t.Find_Control(tForm, "", "", controls2);

            if (menu != null)
            {
                foreach (RibbonPage item in ((DevExpress.XtraBars.Ribbon.RibbonControl)menu).Pages)
                {
                    // isminde -1 yok ise ( -1 : silinmesin menu Item.LineNo = -1 işaretlenmiştir)
                    if (item.Name.IndexOf("-1") == -1)
                    {
                        item.Visible = (item.Name.IndexOf(menuValue) > -1);

                        if (item.Visible)
                            ((DevExpress.XtraBars.Ribbon.RibbonControl)menu).SelectPage(item); 
                    }
                }
            }
        }

                

        #region tExtraCancel_Data // 153 ( Yeni / Vazgeç ) işlemi  <<< işe yarayabilir
        public void tExtraCancel_Data(Form tForm, string Prop_Navigator_Block)
        {


            string s = Prop_Navigator_Block;

            if (s.IndexOf("TABLEIPCODE_LIST") > -1)
            {
                //tToolBox t = new tToolBox();

                v.con_ExtraChange = true;

                string TABLEIPCODE = string.Empty;
                string WORKTYPE = string.Empty;
                string row_block = string.Empty;
                string lockE = "=ROWE_";

                while (s.IndexOf(lockE) > -1)
                {
                    row_block = t.Find_Properies_Get_RowBlock(ref s, "TABLEIPCODE_LIST");
                    WORKTYPE = t.MyProperties_Get(row_block, "WORKTYPE:");

                    /// New pozisyonunda bekleyen ds lerin oluşturulan yeni row ları iptal edilecek
                    if (WORKTYPE == "NEW")
                    {
                        TABLEIPCODE = t.MyProperties_Get(row_block, "TABLEIPCODE:");

                        if (t.IsNotNull(TABLEIPCODE))
                        {
                            tCancelData(tForm, null, TABLEIPCODE);
                        }
                    } //if (row_block.IndexOf 
                } //while

                v.con_ExtraChange = false;
            }

        }
        
        public void tCancelData(Form tForm, object sender, string TableIPCode)
        {
            //tToolBox t = new tToolBox();
            DataSet dsData = null;
            DataNavigator dN = null;
            t.Find_DataSet(tForm, ref dsData, ref dN, TableIPCode);

            if (dsData != null)
            {

                // bunlar yemiyor

                //if (dsData.HasChanges() == false)
                //if (!dsData.HasChanges(DataRowState.Modified))
                //{
                //    tForm.Dispose();
                //    return;
                //}

                //DataTable dt = dsData.Tables[0];
                //BindingContext[dt].EndCurrentEdit();
                
                //if (dsData.Tables[0].ch   .HasChanges(DataRowState.Modified) == false)
                //{
                //    tForm.Dispose();
                //    return;
                //}



                if (dsData.Tables[0].CaseSensitive == true)
                    dsData.Tables[0].CaseSensitive = false;

                //NavigatorButton btn = dN.Buttons.Remove;
                NavigatorButton btn = dN.Buttons.CancelEdit;
                dN.Buttons.DoClick(btn);
                //dsData.AcceptChanges();
                
                if (sender != null)
                {
                    if (sender.GetType().ToString().IndexOf("SimpleButton") > -1)
                    {
                        // sakladığın caption yeniden kullan ( Vazgeç <<< Yeni Hesap )
                        if (((DevExpress.XtraEditors.SimpleButton)sender).Tag != null)
                        {
                            ((DevExpress.XtraEditors.SimpleButton)sender).Text =
                                ((DevExpress.XtraEditors.SimpleButton)sender).Tag.ToString();
                            ((DevExpress.XtraEditors.SimpleButton)sender).Image = t.Find_Glyph("40_401_AddFile_16x16");
                        }
                    }
                    /*
                    if (sender.GetType().ToString().IndexOf("DevExpress.XtraEditors.ButtonEdit") > -1)
                    {
                        
                        if (((DevExpress.XtraEditors.ButtonEdit)sender).
                            != ((DevExpress.XtraEditors.ButtonEdit)sender).EditValue )
                        {
                            MessageBox.Show("modif");
                            
                        }

                        if (((DevExpress.XtraEditors.ButtonEdit)sender).IsEditorActive)
                        {
                           // MessageBox.Show("editor");

                        }
                    }
                    */
                }

                ////if (v.con_PositionChange == false)
                ////{
                ////    if (dN.IsAccessible == true)
                ////    {
                ////        vSubWork vSW = new vSubWork();
                ////        vSW._01_tForm = tForm;
                ////        vSW._02_TableIPCode = TableIPCode;
                ////        vSW._03_WorkTD = v.tWorkTD.Refresh;
                ////        vSW._04_WorkWhom = v.tWorkWhom.Childs;
                ////        //tSubWork_(vSW);
                ////    }
                ////}

                ///
                t.ViewControl_Enabled(tForm, dsData, TableIPCode);
                /// bu IPCode bağlı ExternalIPCode olabilir...
                t.ViewControl_Enabled_ExtarnalIP(tForm, dsData);
            }
        }

        #endregion

        // yeni procedure getClickType 
        private bool checkNavigatorButtons(Form tForm, string TableIPCode, string tKeys)
        {

            bool onay = false;
            /*

            if (t.IsNotNull(TableIPCode) == false)
                return onay;

            Control tPanelNavigator = null;

            tPanelNavigator = t.Find_Control(tForm, "tPanel_Navigator_" + t.AntiStr_Dot(TableIPCode));

            if (tPanelNavigator != null)
            {
                if (tKeys == "Return")
                    tKeys = "Enter";

                string s = "";
                foreach (var item in tPanelNavigator.Controls)
                {
                    s = item.GetType().ToString();
                    if (s == "DevExpress.XtraEditors.SimpleButton")
                    {
                        if (((DevExpress.XtraEditors.SimpleButton)item).Text.IndexOf(tKeys) > -1)
                        {
                            ((DevExpress.XtraEditors.SimpleButton)item).PerformClick();
                            //btn_Navigotor_Click(((DevExpress.XtraEditors.SimpleButton)item), EventArgs.Empty);
                            onay = true;
                        }
                        //((DevExpress.XtraEditors.SimpleButton)item).PerformClick
                    }
                    //(s == "DevExpress.XtraEditors.CheckButton"))
                }
            }
            */
            return onay;
        }
        // yeni procedure getClickType  in ters hali
        private bool checkOtherNavigatorButtons(Form tForm, string notTableIPCode, string tKeys)
        {
            //tToolBox t = new tToolBox();

            bool onay = false;
            Control cntrl = null;
            string TableIPCode = "";
            string[] controls = new string[] { "DevExpress.XtraEditors.DataNavigator" };

            List<string> list = new List<string>();
            t.Find_DataNavigator_List(tForm, ref list);

            foreach (string value in list)
            {
                cntrl = t.Find_Control(tForm, value, "", controls);
                if (cntrl != null)
                {
                    DataNavigator dN = (DevExpress.XtraEditors.DataNavigator)cntrl;

                    TableIPCode = dN.AccessibleName.ToString();

                    if (TableIPCode != notTableIPCode)
                        onay = checkNavigatorButtons(tForm, TableIPCode, tKeys.ToString());

                    if (onay) break;
                } // if cntrl != null
            }

            return onay;
        }


        #endregion events SubFunction
        
        //-----

        public void AutoSave(Form tForm)
        {
            // Form on All DataSet Save
            v.SQLSave = string.Empty;
            //tDataNavigatorList(tForm, "tDataSave");

            vSubWork vSW = new vSubWork();
            vSW._01_tForm = tForm;
            vSW._02_TableIPCode = "";
            vSW._03_WorkTD = v.tWorkTD.Save;
            vSW._04_WorkWhom = v.tWorkWhom.All;
            tSubWork_(vSW);
        }

        public void AutoNew(Form tForm)
        {
            // Form on All DataSet New
            v.SQLSave = string.Empty;

            vSubWork vSW = new vSubWork();
            vSW._01_tForm = tForm;
            vSW._02_TableIPCode = "";
            vSW._03_WorkTD = v.tWorkTD.NewData;
            vSW._04_WorkWhom = v.tWorkWhom.All;
            tSubWork_(vSW);
        }

        public void AutoFormFirstControlFocus(Form tForm)
        {
            // Form için SetFocus 
            if (tForm.AccessibleDefaultActionDescription != null)
            {
                //using (tToolBox t = new tToolBox())
                //{
                    string s = tForm.AccessibleDefaultActionDescription;
                    // okunacak field
                    string sf_TableIPCode = t.Get_And_Clear(ref s, "||");
                    string sf_FieldName = t.Get_And_Clear(ref s, "||");

                    t.tFormActiveControl(tForm, sf_TableIPCode, "Column_", sf_FieldName);
                //}
            }
        }

        public void SetFocus(Form tForm, string sf_TableIPCode, string sf_FieldName)
        {
            //tToolBox t = new tToolBox();
            t.tFormActiveControl(tForm, sf_TableIPCode, "Column_", sf_FieldName);
        }

        public void AutoActiveControlFocus(Form tForm, string TableIPCode)
        {
            tEventsGrid evg = new tEventsGrid();

            //tToolBox t = new tToolBox();
            Control viewCntrl = t.Find_Control_View(tForm, TableIPCode);

            if (viewCntrl != null)
            {
                if (viewCntrl.GetType().ToString() == "DevExpress.XtraGrid.GridControl")
                {
                    tForm.ActiveControl = ((DevExpress.XtraGrid.GridControl)viewCntrl);

                    GridView view = ((DevExpress.XtraGrid.GridControl)viewCntrl).MainView as GridView;
                    vGridHint tGridHint = new vGridHint();
                    evg.getGridHint_(view, ref tGridHint);

                    view.FocusedColumn = view.VisibleColumns[0];
                    if (view.FocusedColumn.ReadOnly)
                    {
                        tGridHint.currentColumn = view.FocusedColumn;
                        view.FocusedColumn = evg.GetNextFocusableColumn(tGridHint);
                    }
                    evg.gridShowEditor(tGridHint);
                }
            }
        }

        public string Preparing_REPORTVIEW(string RaporHesapKodu, string KonuOlan_TableIPCode, string KonuOlan_ID,
                                            string KonuOlanaAit_Liste_TableIPCode, string Parametre)
        {
            // -------------------------------------------------------------
            // ReportView açıldığı zaman okunacak raporlar
            // Select a.* 
            // from MS_REPORTS a
            // where 0 = 0 
            // and a.REPORT_CODE like 'KRS%' 
            string myFormLoadValue = v.myReportViewFormLoadValue;
            t.MyProperties_Set(ref myFormLoadValue, v.FormLoad, "TargetLikeValue", RaporHesapKodu);
            // -------------------------------------------------------------
            // ReportView açıldığı Konu Olan IP 
            // Konu olan Panelinde view edilecek IP (InputPanel) bilgiler set edilir
            t.MyProperties_Set(ref myFormLoadValue, v.FormLoad, "AddIP_Form", "MSREPORTVIEW");
            t.MyProperties_Set(ref myFormLoadValue, v.FormLoad, "AddIP_TableIPCode", KonuOlan_TableIPCode);
            t.MyProperties_Set(ref myFormLoadValue, v.FormLoad, "AddIP_Value", KonuOlan_ID);
            t.MyProperties_Set(ref myFormLoadValue, v.FormLoad, "AddListIP_TableIPCode", KonuOlanaAit_Liste_TableIPCode);
            t.MyProperties_Set(ref myFormLoadValue, v.FormLoad, "Parametre", Parametre);  // false or true
            t.MyProperties_Set(ref myFormLoadValue, v.FormLoad, "END", string.Empty);

            //v.FormLoad = myFormLoadValue;

            return myFormLoadValue;

            // İşlemin Devamı  
            // btn_REPORTVIEW.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(ev.formRibbonMenu_ItemClick);
            // ev.formRibbonMenu_ItemClick >> event te devam ediyor
        }

        

        // gerçek eventsler ***********************************************************

        #region buttonEdit Events
        public void buttonEdit_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            if (e.Button.Kind == DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) return;

            /*
            if (e.Button.Kind == DevExpress.XtraEditors.Controls.ButtonPredefines.Search)
            {
                //tSearch se = new tSearch();
                DevExpress.XtraEditors.Controls.ButtonPredefines button = e.Button.Kind;
                se.buttonEdit_ButtonClick_(sender, button);
            }
            if (e.Button.Kind == DevExpress.XtraEditors.Controls.ButtonPredefines.Glyph)
            {
            }
            */
            preparing_ButtonHint(sender, e.Button.Kind);
        }

        //public void preparing_ButtonHint(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e, DevExpress.XtraEditors.Controls.ButtonPredefines button)
        public void preparing_ButtonHint(object sender, DevExpress.XtraEditors.Controls.ButtonPredefines button)
        {
            bool onay = false;
            string TableIPCode = string.Empty;
            string senderType = sender.GetType().ToString();
            string editValue = "";
            string funcName = "";
            string myProp = "";
            string fieldName = "";

            if (senderType == "DevExpress.XtraEditors.ButtonEdit")
            {
                myProp = ((DevExpress.XtraEditors.ButtonEdit)sender).Properties.AccessibleDescription;
                fieldName = ((DevExpress.XtraEditors.ButtonEdit)sender).Properties.Name?.ToString();
            }
            if (senderType == "DevExpress.XtraEditors.ImageComboBoxEdit")
            {
                myProp = ((DevExpress.XtraEditors.ImageComboBoxEdit)sender).Properties.AccessibleDescription;
                fieldName = ((DevExpress.XtraEditors.ImageComboBoxEdit)sender).Properties.Name?.ToString();
            }
            //Column_LKP_LISTEYE_EKLE
            fieldName = fieldName.Replace("Column_", "");

            if (t.IsNotNull(myProp))
            {
                v.tButtonType buttonType = v.tButtonType.btNone;

                #region Find Form
                Form tForm = null;

                if (senderType == "DevExpress.XtraEditors.ImageComboBoxEdit")
                {
                    TableIPCode = ((DevExpress.XtraEditors.ImageComboBoxEdit)sender).Properties.AccessibleName;
                    tForm = ((DevExpress.XtraEditors.ImageComboBoxEdit)sender).FindForm();
                }

                if (senderType == "DevExpress.XtraEditors.ButtonEdit")
                {
                    TableIPCode = ((DevExpress.XtraEditors.ButtonEdit)sender).Properties.AccessibleName;
                    tForm = ((DevExpress.XtraEditors.ButtonEdit)sender).FindForm();
                }

                if (senderType == "DevExpress.XtraEditors.SimpleButton")
                {
                    TableIPCode = ((DevExpress.XtraEditors.SimpleButton)sender).AccessibleName;
                    tForm = ((DevExpress.XtraEditors.SimpleButton)sender).FindForm();
                }

                #endregion

                //Type: SearchEngine;[
                //   {
                //     "CAPTION": "Search",
                //     "BUTTONTYPE": "58",
                //     "TABLEIPCODE_LIST": [

                myProp = myProp.Replace((char)34, (char)39);

                funcName = t.MyProperties_Get(myProp, "Type:");

                #region Search_Engines and Buttons
                if ((funcName == v.tSearch.searchEngine) || (funcName == v.ButtonEdit))
                {
                    if (senderType == "DevExpress.XtraEditors.ImageComboBoxEdit")
                    {
                        /// atama yapılacak değer GET_FIELD_LIST={ içinden tespit edilmeli ama hangisi ?
                        /// aslında gerekte yok, çünkü gelen değer  editvalue  
                        v.con_Value_Old = string.Empty;

                        buttonType = t.getClickType(myProp);
                    }

                    if (senderType == "DevExpress.XtraEditors.ButtonEdit")
                    {
                        //v.con_Value_Old = "";
                        //v.con_SearchValue = "";

                        buttonType = t.getClickType(myProp);

                        if (button == DevExpress.XtraEditors.Controls.ButtonPredefines.Search)
                        {
                            if (funcName == v.tSearch.searchEngine)
                                buttonType = v.tButtonType.btArama;

                            if (funcName == v.ButtonEdit)
                                buttonType = v.tButtonType.btGoster;
                        }

                        if ((button == DevExpress.XtraEditors.Controls.ButtonPredefines.Ellipsis) &&
                            (funcName == v.ButtonEdit))
                            buttonType = v.tButtonType.btKartAc;

                        if ((button == DevExpress.XtraEditors.Controls.ButtonPredefines.Plus) &&
                            (funcName == v.ButtonEdit))
                            buttonType = v.tButtonType.btYeniKart;

                        if ((button == DevExpress.XtraEditors.Controls.ButtonPredefines.OK) &&
                            (funcName == v.ButtonEdit) &&
                            (buttonType == v.tButtonType.btListeyeEkle)
                            )
                        {

                        }


                        if (buttonType == v.tButtonType.btArama)
                        {
                            //if (((DevExpress.XtraEditors.ButtonEdit)sender).EditValue != null)
                            //    v.tSearch.searchInputValue = ((DevExpress.XtraEditors.ButtonEdit)sender).EditValue.ToString();
                            /// Arama ekranı boş açılsın
                            ///
                            v.tSearch.searchInputValue = "";
                        }
                    }
                }
                #endregion Search_Engines

                #region Properties & Plus 
                if ((funcName == v.Properties) ||
                    (funcName == v.PropertiesPlus))
                {
                    string TableName = t.MyProperties_Get(myProp, "TableName:");
                    string FieldName = t.MyProperties_Get(myProp, "FieldName:");
                    string Width = t.MyProperties_Get(myProp, "Width:");

                    string thisValue = ((DevExpress.XtraEditors.ButtonEdit)sender).EditValue.ToString();

                    tCreateObject co = new tCreateObject();

                    string NewValue = thisValue;

                    if (funcName == v.Properties)
                        NewValue = co.Create_PropertiesEdit_JSON(TableName, FieldName, Width, thisValue);

                    if (funcName == v.PropertiesPlus)
                        NewValue = co.Create_PropertiesPlusEdit_JSON(TableName, FieldName, Width, thisValue);

                    ((DevExpress.XtraEditors.ButtonEdit)sender).EditValue = NewValue;
                }
                #endregion Properties & Plus 

                #region PropertiesNav
                if (funcName == v.PropertiesNav)
                {
                    string thisValue = ((DevExpress.XtraEditors.ButtonEdit)sender).EditValue.ToString();

                    tCreateObject co = new tCreateObject();

                    string NewValue = thisValue;

                    NewValue = co.Create_PropertiesNavEdit(thisValue);

                    ((DevExpress.XtraEditors.ButtonEdit)sender).EditValue = NewValue;
                }
                #endregion PropertiesNav 

                if (buttonType != v.tButtonType.btNone)
                {
                    v.tButtonHint.Clear();
                    v.tButtonHint.tForm = tForm;
                    v.tButtonHint.tableIPCode = TableIPCode;
                    v.tButtonHint.columnFieldName = fieldName;
                    v.tButtonHint.propNavigator = myProp;
                    v.tButtonHint.buttonType = buttonType;
                    v.tButtonHint.columnEditValue = editValue;
                    v.tButtonHint.senderType = sender.GetType().ToString();
                    v.tButtonHint.checkedValue = editValue;

                    if (buttonType != v.tButtonType.btArama)
                    {
                        tEventsButton evb = new tEventsButton();
                        evb.btnClick(v.tButtonHint);
                    }

                    if (buttonType == v.tButtonType.btArama)
                    {
                        // btArama / search işlemi başlasın
                        if (myProp != "")
                        {
                            tSearch se = new tSearch();
                            se.search_ButtonClick(sender, v.tButtonHint);
                        }
                    }
                }
            }
        }

        public void ImageComboBoxEdit_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            #region Button[1] click ise
            if (e.Button.Index == 1)
            {
                //MessageBox.Show("aaaaa");
                buttonEdit_ButtonClick(sender, e);
            }
            #endregion  Button[1] click ise
        }
        public void ImageComboBoxEdit_HizliOnayButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            #region Button[1] click ise
            if (e.Button.Index == 1)
            {
                string formName = ((DevExpress.XtraEditors.ImageComboBoxEdit)sender).Properties.AccessibleDescription.ToString();
                string tableIPCode = ((DevExpress.XtraEditors.ImageComboBoxEdit)sender).Properties.AccessibleName?.ToString();
                string fieldName = ((DevExpress.XtraEditors.ImageComboBoxEdit)sender).Name?.ToString();
                fieldName = fieldName.Replace("tEdit_HizliOnay_", "");

                string editValue = "";
                if (((DevExpress.XtraEditors.ImageComboBoxEdit)sender).EditValue != null)
                        editValue = ((DevExpress.XtraEditors.ImageComboBoxEdit)sender).EditValue.ToString();

                //MessageBox.Show("aaaaa : " + tableIPCode + "  : " + editValue + " : ");
                if (editValue != "")
                {
                    Form tForm = Application.OpenForms[formName];
                    DataSet ds = null;
                    DataNavigator dN = null;
                    t.Find_DataSet(tForm, ref ds, ref dN, tableIPCode);
                    if (t.IsNotNull(ds))
                    {
                        tSave sv = new tSave();
                        v.con_OnayChange = false;
                        bool onay = false;
                        int i2 = ds.Tables[0].Rows.Count;
                        for (int i = 0; i < i2; i++)
                        {
                            onay = (bool)ds.Tables[0].Rows[i]["LKP_ONAY"];
                            if (onay)
                            {
                                ds.Tables[0].Rows[i][fieldName] = editValue;
                                ds.Tables[0].AcceptChanges();
                                dN.Position = i;
                                Application.DoEvents();
                                //NavigatorButton btnSave = dN.Buttons.EndEdit;
                                //dN.Buttons.DoClick(btnSave);
                                sv.tDataSave(tForm, ds, dN, dN.Position);
                            }
                        }
                    }
                }

            }
            #endregion  Button[1] click ise
        }

        public void buttonEdit_Enter(object sender, EventArgs e)
        {
            /// Buranın amacı xxx.EditValue içindeki değerleri hafızaya alıyor,
            /// çünkü arama butonuna basabilir
            if (t.IsSearchControl(sender))
            {
                if (((DevExpress.XtraEditors.ButtonEdit)sender).EditValue != null)
                    v.tSearch.searchValue = ((DevExpress.XtraEditors.ButtonEdit)sender).EditValue.ToString();
            }
        }

        public void buttonEdit_EditValueChanged(object sender, EventArgs e)
        {
            /// Burası otomatik aramayı başlatıyor ileride gerekebilir silme
            /// 

            //if (t.IsSearchControl(sender))
            //{
            //    tSearch se = new tSearch();
            //    se.searchEditValueChanged(sender, e);
            //}

        }

        public void buttonEdit_Leave(object sender, EventArgs e)
        {
            
            string tableIPCode = ((DevExpress.XtraEditors.ButtonEdit)sender).Properties.AccessibleName;
            string myProp = ((DevExpress.XtraEditors.ButtonEdit)sender).Properties.AccessibleDescription;
            string fieldName = ((DevExpress.XtraEditors.ButtonEdit)sender).Name.ToString();
            string editValue = "";
            string keyValue = "";
            string searchEngine = t.MyProperties_Get(myProp, "Type:");
            bool searchOnayi = t.IsSearchControl(sender);

            if (fieldName != "")
                fieldName = fieldName.Substring(7, fieldName.Length - 7); // = "Column_" + tFieldName;

            if (searchEngine == "SearchEngine" && searchOnayi)//v.SearchEngine)
            {
                Form tForm = t.Find_Form(sender);

                keyValue = t.Find_TableIPCode_Value(tForm, tableIPCode, "");

                if (((DevExpress.XtraEditors.ButtonEdit)sender).EditValue != null)
                    editValue = ((DevExpress.XtraEditors.ButtonEdit)sender).EditValue.ToString();

                if (t.myInt32(keyValue) <= 0 && editValue != "")
                {
                    v.tButtonHint.Clear();
                    v.tButtonHint.tForm = tForm;   //tGridHint.tForm;
                    v.tButtonHint.tableIPCode = tableIPCode; // tGridHint.tableIPCode;
                    v.tButtonHint.propNavigator = myProp; // t.Set(tGridHint.columnPropNavigator, tGridHint.gridPropNavigator, "");
                    v.tButtonHint.columnFieldName = fieldName;
                    v.tButtonHint.columnOldValue = editValue; // tGridHint.columnOldValue;
                    v.tButtonHint.columnEditValue = editValue; // tGridHint.columnEditValue;
                    v.tButtonHint.parentObject = null; // tGridHint.parentObject;
                    v.tButtonHint.sender = sender;// tGridHint.view;
                    v.tButtonHint.senderType = sender.GetType().ToString();//  //tGridHint.viewType;
                    v.tButtonHint.buttonType = v.tButtonType.btArama;//  tGridHint.buttonType;

                    se.directSearch(v.tButtonHint);
                }
            }
            
        }

        public void buttonEdit_KeyDown(object sender, KeyEventArgs e)
        {
            
        }

        public void buttonEdit_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Basılan tuşu büyük harfe dönüştür
            e.KeyChar = char.ToUpper(e.KeyChar);
            //MessageBox.Show("buttonEdit1_KeyPress");
        }

        public void buttonEdit_KeyUp(object sender, KeyEventArgs e)
        {
            //MessageBox.Show("buttonEdit1_KeyUp");
        }

        #endregion buttonEdit Events
        
        #region dataLayoutControl Events
        public void myDataLayoutControl_Enter(object sender, EventArgs e)
        {
            foreach (var item in ((DevExpress.XtraDataLayout.DataLayoutControl)sender).Root.Items)
            {

                if (item.GetType().ToString() == "DevExpress.XtraLayout.TabbedControlGroup")
                {
                    foreach (DevExpress.XtraLayout.LayoutControlGroup subItem in ((DevExpress.XtraLayout.TabbedControlGroup)item).TabPages)
                    {
                        subItem.AppearanceTabPage.HeaderActive.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
                        subItem.AppearanceTabPage.HeaderActive.Options.UseBackColor = true;
                    }
                }
            }
        }

        public void myDataLayoutControl_Leave(object sender, EventArgs e)
        {
            //this.tabbedControlGroup1 = new DevExpress.XtraLayout.TabbedControlGroup();
            //this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            foreach (var item in ((DevExpress.XtraDataLayout.DataLayoutControl)sender).Root.Items)
            {
                if (item.GetType().ToString() == "DevExpress.XtraLayout.TabbedControlGroup")
                {
                    foreach (DevExpress.XtraLayout.LayoutControlGroup subItem in ((DevExpress.XtraLayout.TabbedControlGroup)item).TabPages)
                    {
                        //subItem.AppearanceTabPage.HeaderActive.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
                        subItem.AppearanceTabPage.HeaderActive.Options.UseBackColor = false;
                    }
                }
            }
        }
        #endregion

        #region tXtraEdit_ Events

        public void tXtraEdit_ToggleSwitch_EditValueChanged(object sender, EventArgs e)
        {
            

            #region Tanımlar

            string SubDetail_TableIPCode = string.Empty;
            string mst_TableIPCode = string.Empty;
            string FieldName = string.Empty;
            string Value = string.Empty;
            string Tag = string.Empty;

            Form tForm = t.Find_TableIPCode_XtraEditors(sender, ref mst_TableIPCode, ref FieldName, ref Value, ref Tag);
            SubDetail_TableIPCode = mst_TableIPCode; // master da / sub da kendisi

            // yeniden okunacak dataseti bul
            DataSet dsSubDetail_Data = t.Find_DataSet(tForm, "", mst_TableIPCode, "");

            string myProp = dsSubDetail_Data.Namespace.ToString();
            //string SubDetail_List = t.MyProperties_Get(myProp, "About_Detail_SubDetail:");

            int DataReadType = t.myInt32(t.MyProperties_Get(myProp, "DataReadType:"));

            string SqlF = t.MyProperties_Get(myProp, "SqlFirst:");
            string SqlS = t.MyProperties_Get(myProp, "SqlSecond:");

            string Sql_OldF = SqlF;
            string Sql_OldS = SqlS;

            #endregion Tanımlar

            //***
            Preparing_OnOff(ref SqlF, FieldName, Value);

            if (Sql_OldS != "")
                Preparing_OnOff(ref SqlS, FieldName, Value);
            //***

            SubDetail_Run(tForm, dsSubDetail_Data, myProp, SqlF, Sql_OldF, SqlS, Sql_OldS, SubDetail_TableIPCode, DataReadType);

        }
                
        public void tXtraEdit_EditValueChanged(object sender, EventArgs e)
        {

            #region Giriş kontrolleri
            // LKP_ONAY için ise işlem olmasın
            if ((v.con_OnayChange) || (v.con_PositionChange)) return;
            // 
            if (v.con_Cancel == true)
            {
                v.con_Cancel = false;
                return;
            }
            
            #endregion

            #region Tanımlar

            string SubDetail_TableIPCode = string.Empty;
            string mst_TableIPCode = string.Empty;
            string FieldName = string.Empty;
            string Value = string.Empty;
            string Tag = string.Empty;

            Form tForm = t.Find_TableIPCode_XtraEditors(sender, ref mst_TableIPCode, ref FieldName, ref Value, ref Tag);

            DataSet ds = null;
            DataNavigator dN = null;
            t.Find_DataSet(tForm, ref ds, ref dN, mst_TableIPCode);

            if (mst_TableIPCode == null) return;
            if (ds == null) return;

            string fname = "";
            string newValue = "";
            byte tOperand_type = 0;
            string SubDetail_List = "";
            string subDetail_List_ = "";

            // değişiklik nesne üzerinde oluyor fakat yeni value nin dataset ulaşması gerek

            //column.Name = "Column_" + tfield_name;
            if (sender.ToString() == "DevExpress.XtraEditors.CheckEdit")
            {
                fname = ((DevExpress.XtraEditors.CheckEdit)sender).Name.Substring(7).ToString();
                newValue = ((DevExpress.XtraEditors.CheckEdit)sender).EditValue.ToString();
                tOperand_type = t.myByte(((DevExpress.XtraEditors.CheckEdit)sender).Tag.ToString());
            } 
            else if (sender.ToString() == "DevExpress.XtraEditors.ImageComboBoxEdit")
            {
                fname = ((DevExpress.XtraEditors.ImageComboBoxEdit)sender).Name.Substring(7).ToString();
                newValue = ((DevExpress.XtraEditors.ImageComboBoxEdit)sender).EditValue.ToString();
                tOperand_type = t.myByte(((DevExpress.XtraEditors.ImageComboBoxEdit)sender).Tag.ToString());
            }
            else if (sender.ToString() == "DevExpress.XtraEditors.DateEdit")
            {
                fname = ((DevExpress.XtraEditors.DateEdit)sender).Name.Substring(7).ToString();
                if (((DevExpress.XtraEditors.DateEdit)sender).EditValue != null)
                    newValue = ((DevExpress.XtraEditors.DateEdit)sender).EditValue.ToString();
                subDetail_List_ = ((DevExpress.XtraEditors.DateEdit)sender).Tag.ToString();
            }
            else
            {
                MessageBox.Show("tXtraEdit_ImageComboBoxEdit_EditValueChanged  de yeni nesne var");
            }

            if (dN != null)
            {
                if ((dN.Position > -1) &&
                    (fname != "") &&
                    (newValue != ""))
                {
                    //if ((fname.IndexOf("_BAS") == -1) &&
                    //    (fname.IndexOf("_BIT") == -1))
                    //     ds.Tables[0].Rows[dN.Position][fname] = newValue;

                    ///'KRT_OPERAND_TYPE',    1, '', 'Even (Double)');
                    ///'KRT_OPERAND_TYPE',    2, '', 'Odd  (Single)');
                    ///'KRT_OPERAND_TYPE',    3, '', 'Speed (Double)');
                    ///'KRT_OPERAND_TYPE',    4, '', 'Speed (Single)');
                    ///'KRT_OPERAND_TYPE',    5, '', 'On/Off'); True/False
                    ///'KRT_OPERAND_TYPE',   21, '', 'Source Value Field'
                    ///
                    if ((tOperand_type == 3) || (tOperand_type == 4))
                        Data_Refresh(tForm, ds, dN);

                    if (tOperand_type == 5)
                        applyOperand(tForm, ds, dN, mst_TableIPCode, fname, newValue);
                }
            }

            if ((fname.IndexOf("_BAS") > -1) ||
                (fname.IndexOf("_BIT") > -1))
            {
                string myProp = string.Empty;
                string Prop_SubView = string.Empty;

                myProp = ds.Namespace.ToString();
                SubDetail_List = t.MyProperties_Get(myProp, "About_Detail_SubDetail:");
                Prop_SubView = t.MyProperties_Get(myProp, "Prop_SubView:");

                if (t.IsNotNull(SubDetail_List) == false)
                    SubDetail_List = subDetail_List_;
                else SubDetail_List += subDetail_List_;

                #region SubDetail_List
                if (t.IsNotNull(SubDetail_List))
                {
                    int DataReadType = t.myInt32(t.MyProperties_Get(myProp, "DataReadType:"));

                    string SqlF = t.MyProperties_Get(myProp, "SqlFirst:");
                    string SqlS = t.MyProperties_Get(myProp, "SqlSecond:");

                    string Sql_OldF = SqlF;
                    string Sql_OldS = SqlS;
                    bool sourceTableIPCodeREAD = false;

                    if ((Sql_OldF != "") && (Sql_OldS == ""))
                    {
                        SubDetail_Preparing(tForm, ref SqlF,
                                            ds, dN, //mst_TableIPCode, 
                                            ds, SubDetail_List, SubDetail_TableIPCode, "", // Master_Detail_Harmony 
                                            DataReadType, FieldName, Value, 
                                            ref sourceTableIPCodeREAD);
                    }

                    if (Sql_OldS != "")
                    {
                        SubDetail_Preparing(tForm, ref SqlS,
                                            ds, dN, //mst_TableIPCode,
                                            ds, SubDetail_List, SubDetail_TableIPCode, "", // Master_Detail_Harmony
                                            DataReadType, FieldName, Value,
                                            ref sourceTableIPCodeREAD);
                    }
                    
                    SubDetail_Run(tForm, ds, myProp, SqlF, Sql_OldF, SqlS, Sql_OldS, SubDetail_TableIPCode, DataReadType);
                }
                #endregion SubDetail_List

            }

            #endregion Tanımlar
        }

        public void applyOperand(Form tForm, DataSet dsData, DataNavigator tDataNavigator, 
            string tableIPCode, string fieldName, string value)
        {
            if (t.IsNotNull(dsData) == false) return;

            string SourceFieldValue = t.Find_SourceValueField(tForm, tableIPCode);

            string myProp = dsData.Namespace;
            string dBaseNo = t.MyProperties_Get(myProp, "DBaseNo:");
            string tTableName = t.MyProperties_Get(myProp, "TableName:");
            string tWhere = "Where 0 = 0 " + t.MyProperties_Get(myProp, "Where_IP_Add:");

            string tSql = "Update " + tTableName + " set " + fieldName + " = ";

            if (value.ToLower() == "true") tSql = tSql + " 1 ";
            if (value.ToLower() == "false") tSql = tSql + " 0 ";
            if ((value.ToLower() != "true") &&
                (value.ToLower() != "false")) tSql = tSql + value;

            if (SourceFieldValue != "")
                tSql = tSql + " , " + SourceFieldValue;

            tSql = tSql + tWhere;

            DataSet dsUpdate = new DataSet();
            myProp = string.Empty;
            t.MyProperties_Set(ref myProp, "DBaseNo", dBaseNo);
            t.MyProperties_Set(ref myProp, "TableName", tTableName);
            t.MyProperties_Set(ref myProp, "SqlFirst", tSql);
            t.MyProperties_Set(ref myProp, "SqlSecond", "null");

            dsUpdate.Namespace = myProp;

            try
            {
                t.Data_Read_Execute(tForm, dsUpdate, ref tSql, tTableName, null);
                
                t.AlertMessage("", "İşlem tamamlandı ...");

                dsUpdate.Dispose();
            }
            catch (Exception)
            {
                throw;
            }

            t.TableRefresh(tForm, dsData);

            /*  Bu sistem çok ağır çalışıyor
             *  
            tSave sv = new tSave();

            v.onlyTheseFields = fieldName;

            if (Cursor.Current == Cursors.Default)
                Cursor.Current = Cursors.WaitCursor;
            
            v.IsWaitOpen = false;
            t.WaitFormOpen(tForm, "");
            v.IsWaitOpen = true;
            Control cntrl = t.Find_Control_View(tForm, tableIPCode);
            
            ((DevExpress.XtraGrid.GridControl)cntrl).MainView.BeginUpdate();
            //v.con_OnayChange = true;
            
            tDataNavigator.Position = 0;
            foreach (DataRow row in dsData.Tables[0].Rows)
            {
                row[fieldName] = value;
                //sv.tDataSave(tForm, tableIPCode);
                tDataNavigator.Position++;
            }
            tDataNavigator.Position = 0;
            
            //v.con_OnayChange = false;
            ((DevExpress.XtraGrid.GridControl)cntrl).MainView.EndUpdate();

            v.IsWaitOpen = false;
            t.WaitFormClose();

            Cursor.Current = Cursors.Default;
            
             v.onlyTheseFields = "";
             */
        }

        public void tXtraEdit_Expression_EditValueChanged(object sender, EventArgs e)
        {
            //Application.OpenForms[0].Text += ",e1";
            v.con_Expression = true;
        }

        public void tXtraEdit_EditValueChanging(object sender, DevExpress.XtraEditors.Controls.ChangingEventArgs e)
        {
            //v.SQL = v.SQL + ",2";
            //
            v.con_Expression = true;
            //Application.OpenForms[0].Text += ",e2";
        }

        public void tXtraEdit_KeyDown(object sender, KeyEventArgs e)
        {
            v.con_Expression = true;
        }

        public void tXtraEdit_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Basılan tuşu büyük harfe dönüştür
            e.KeyChar = char.ToUpper(e.KeyChar);
            v.con_Expression = true;
        }

        public void tXtraEdit_KeyUp(object sender, KeyEventArgs e)
        {
            v.con_Expression = true;
        }

        public void tXtraEdit_Enter(object sender, EventArgs e)
        {
            ((DevExpress.XtraEditors.TextEdit)sender).Properties.Appearance.BackColor = v.AppearanceFocusedColor;
        }

        public void tXtraEdit_Leave(object sender, EventArgs e)
        {
            if (((DevExpress.XtraEditors.TextEdit)sender).Properties.Appearance.BackColor != v.ColorValidation)
                ((DevExpress.XtraEditors.TextEdit)sender).Properties.Appearance.BackColor =
                             ((DevExpress.XtraEditors.TextEdit)sender).Properties.Appearance.BackColor2;

            #region Expression

            string tableIPCode = string.Empty;
            string fieldName = string.Empty;
            string value = string.Empty;
            string Tag = string.Empty;

            Form tForm = t.Find_TableIPCode_XtraEditors(sender, ref tableIPCode, ref fieldName, ref value, ref Tag);

            if (Tag == "EXPRESSION")
            {
                t.work_EXPRESSION(tForm, tableIPCode, fieldName, value);
                //ExpressionRun(tForm, TableIPCode, FieldName, Value);
                // Eğer return olmaz ise alttaki fonksiyonlar çalışıyor
                // bu nedenle return kaldırma
                return;
            }
            #endregion Expression

        }

        public void myBarcodeEdit_KeyUp(object sender, KeyEventArgs e)
        {
            if (
                ((e.KeyCode == Keys.Decimal) ||
                 (e.KeyCode == Keys.Subtract) ||
                 (e.KeyCode == Keys.Divide) ||
                 (e.KeyCode == Keys.Add) ||
                 (e.KeyCode == Keys.Multiply) ||
                 (e.KeyCode == Keys.Enter)) &&
                 (((DevExpress.XtraEditors.TextEdit)sender).EditValue != null)
                )
            {
                int l1 = 0;
                string Veri = ((DevExpress.XtraEditors.TextEdit)sender).EditValue.ToString();

                if (e.KeyCode != Keys.Enter)
                    Veri = Veri.Substring(0, Veri.Length - 1);

                l1 = Veri.Length;

                // 6 karekterden düşük ise 99999
                if ((l1 > 0) && (l1 < 6))
                {

                    Form tForm = t.Find_Form(sender);
                    string[] controls = new string[] { };
                    Control cntrl = t.Find_Control(tForm, "tEditMiktar", "", controls);

                    if (cntrl != null)
                    {
                        // editMiktar 
                        ((DevExpress.XtraEditors.TextEdit)cntrl).EditValue = Veri;

                        int fontSize = 20;
                        if (l1 > 3) fontSize = 14;
                        ((DevExpress.XtraEditors.TextEdit)cntrl).Font =
                            new Font(((DevExpress.XtraEditors.TextEdit)cntrl).Font.FontFamily, fontSize);

                        // editBarcode
                        ((DevExpress.XtraEditors.TextEdit)sender).EditValue = "";
                    }
                }
            }
        }

        private void ExpressionRun(Form tForm, string tableIPCode, string fieldName, string value)
        {
            // okunacak dataseti bul
            //DataSet dsData = null;
            //DataNavigator tDataNavigator = null;
            //t.Find_DataSet(tForm, ref dsData, ref tDataNavigator, tableIPCode);

            t.work_EXPRESSION(tForm, tableIPCode, fieldName, value);
        }


        #endregion tXtraEdit_ Events

        #region btn_Navigator

        public void btn_Navigotor_Enter(object sender, EventArgs e)
        {
            ((DevExpress.XtraEditors.SimpleButton)sender).Appearance.BackColor =
                ((DevExpress.XtraEditors.SimpleButton)sender).AppearanceHovered.BackColor;
            ((DevExpress.XtraEditors.SimpleButton)sender).Appearance.Options.UseBackColor = true;
            //((DevExpress.XtraEditors.SimpleButton)sender).BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
        }
        public void btn_Navigotor_Leave(object sender, EventArgs e)
        {
            ((DevExpress.XtraEditors.SimpleButton)sender).Appearance.BackColor =
                 ((DevExpress.XtraEditors.SimpleButton)sender).Appearance.BackColor2;
            ((DevExpress.XtraEditors.SimpleButton)sender).Appearance.Options.UseBackColor = false;
            //((DevExpress.XtraEditors.SimpleButton)sender).BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Default;

        }
        public void btn_Navigotor_Click(object sender, EventArgs e)
        {

            int Button_Click_Type = ((DevExpress.XtraEditors.SimpleButton)sender).TabIndex;

            string ButtonName = ((DevExpress.XtraEditors.SimpleButton)sender).Name.ToString();
            string Caption = ((DevExpress.XtraEditors.SimpleButton)sender).Text;

            if (Button_Click_Type == 999)
                Button_Click_Type = 11;

            // simpleButton_  ifadesi isimden siliniyor
            ButtonName = ButtonName.Substring(13, ButtonName.Length - 13);

            string TableIPCode = ((DevExpress.XtraEditors.SimpleButton)sender).AccessibleName;

            string Prop_Navigator = "";
            string Prop_Search = string.Empty;

            if (((DevExpress.XtraEditors.SimpleButton)sender).AccessibleDescription != null)
            {
                Prop_Navigator = ((DevExpress.XtraEditors.SimpleButton)sender).AccessibleDescription.ToString();
            }

            if (((DevExpress.XtraEditors.SimpleButton)sender).AccessibleDefaultActionDescription != null)
            {
                //  "simpleButton_arama" butonunda search bilgileri varsa 
                //   
                Prop_Search = ((DevExpress.XtraEditors.SimpleButton)sender).AccessibleDefaultActionDescription.ToString();

                if (t.IsNotNull(Prop_Search))
                    Prop_Navigator = Prop_Search;
            }


            // 1. formu tespit edilir
            Form tForm = t.Find_Form(sender);

            //btn_Navigotor_Click(tForm, sender, TableIPCode, ButtonName, Caption, Prop_Navigator, Button_Click_Type);

            Control view = t.Find_Control_View(tForm, TableIPCode);

            if (view.GetType().ToString() == "DevExpress.XtraScheduler.SchedulerControl")
            {
                tEventsGrid evg = new tEventsGrid();
                vGridHint tGridHint = new vGridHint();
                evg.getGridHint_(view, ref tGridHint);
                evg.mySchedulerControl(view, tGridHint);
            }

            v.tButtonHint.Clear();
            v.tButtonHint.tForm = tForm;
            v.tButtonHint.tableIPCode = TableIPCode;
            v.tButtonHint.propNavigator = Prop_Navigator;
            v.tButtonHint.buttonType = t.getClickType(Button_Click_Type);
            v.tButtonHint.sender = sender;
            v.tButtonHint.senderType = "Button";
            tEventsButton evb = new tEventsButton();
            evb.btnClick(v.tButtonHint);
            v.tButtonHint.Clear();
        }
        public void btn_Number_Click(object sender, EventArgs e)
        {
            Form tForm = t.Find_Form(sender);

            string cmpName = ((DevExpress.XtraEditors.SimpleButton)sender).AccessibleDescription;
            string caption = ((DevExpress.XtraEditors.SimpleButton)sender).Text;

            Control cntrl = t.Find_NumberControlDisplay(tForm, cmpName);

            if (cntrl != null)
            {
                if (caption == "ce")
                {
                    ((DevExpress.XtraEditors.TextEdit)cntrl).Text = "1";
                }
                else
                        if (caption == "<")
                {
                    string value = ((DevExpress.XtraEditors.TextEdit)cntrl).Text;
                    if (value.Length > 0)
                    {
                        value = value.Remove(value.Length - 1);
                        ((DevExpress.XtraEditors.TextEdit)cntrl).Text = value;
                    }
                }
                else
                        if (caption == "=")
                {
                    MessageBox.Show("Hesap yapılacak..");
                }
                else
                {
                    ((DevExpress.XtraEditors.TextEdit)cntrl).Text += caption;
                }

                /*
                if (cntrl.GetType().ToString() == "DevExpress.XtraEditors.PanelControl")
                {
                    Control cntrl2 = ((DevExpress.XtraEditors.PanelControl)cntrl).Controls[0];
                    if (cntrl2.GetType().ToString() == "DevExpress.XtraEditors.TextEdit")
                    {
                        if (caption == "ce")
                        {
                            ((DevExpress.XtraEditors.TextEdit)cntrl2).Text = "1";
                        }
                        else
                        if (caption == "<")
                        {
                            string value = ((DevExpress.XtraEditors.TextEdit)cntrl2).Text;
                            if (value.Length > 0)
                            {
                                value = value.Remove(value.Length - 1);
                                ((DevExpress.XtraEditors.TextEdit)cntrl2).Text = value;
                            }
                        }
                        else
                        if (caption == "=")
                        {
                            MessageBox.Show("Hesap yapılacak..");
                        }
                        else
                        {
                            ((DevExpress.XtraEditors.TextEdit)cntrl2).Text += caption;
                        }
                    }
                }
            */
            }

            /*
            string TableIPCode = ((DevExpress.XtraEditors.SimpleButton)sender).AccessibleName;
            string fieldName = ((DevExpress.XtraEditors.SimpleButton)sender).AccessibleDescription;
            string caption = ((DevExpress.XtraEditors.SimpleButton)sender).Text;

            DataSet ds = null;
            DataNavigator dN = null;

            t.Find_DataSet(tForm, ref ds, ref dN, TableIPCode);

            if (t.IsNotNull(ds))
            {
                ds.Tables[0].Rows[dN.Position][fieldName] += caption;
                ds.Tables[0].AcceptChanges();
            }
            */
        }
        public void btn_Navigator_PopupMenu(object sender, EventArgs e)
        {
            Form tForm = t.Find_Form(sender);

            MessageBox.Show("popup mesaj");
        }
        public void btn_Navigator_MesajOnayi_Click(object sender, EventArgs e)
        {
            string tableIPCode = ((DevExpress.XtraEditors.SimpleButton)sender).AccessibleName;
            string formName = ((DevExpress.XtraEditors.SimpleButton)sender).AccessibleDescription;
            
            //Form tForm = t.Find_Form(sender); Buton bir popupForm üzerinde 
            Form tForm = Application.OpenForms[formName];

            bool onay = (bool)((DevExpress.XtraEditors.SimpleButton)sender).Tag;

            DataSet ds = null;
            DataNavigator dN = null;

            t.Find_DataSet(tForm, ref ds, ref dN, tableIPCode);

            if (t.IsNotNull(ds))
            {
                foreach (DataRow row  in ds.Tables[0].Rows)
                {
                    row["LKP_ONAY"] = onay;
                }
            }
        }
        public void btn_Navigator_BildirimHazirla_Click(object sender, EventArgs e)
        {
            preparingBildirimHakkinda(sender);

            Form tForm = Application.OpenForms[v.tBildirim.formName];

            ///
            /// kullanıcı seçimlerini al
            ///
            getSecilenBildirimSecenekleri(tForm, v.tBildirim);

            bool onay = bildirimTarihVeSaatOnayi(v.tBildirim);

            //'WORKTYPE', 'MESSAGESINGLE', 'Single Message'
            //'WORKTYPE', 'MESSAGEMULTI',  'Multi Message'
            if (onay)
            {
                t.WaitFormOpen(v.mainForm, "");
                t.WaitFormOpen(v.mainForm, "Bildirim/lerin içeriği hazırlanıyor...");

                if (v.tBildirim.workType == "MESSAGEMULTI")
                    preparingMultiMessage(tForm, v.tBildirim);
                if (v.tBildirim.workType == "MESSAGESINGLE")
                    preparingSingleMessage(tForm, v.tBildirim);

                v.SP_OpenApplication = false;
                v.IsWaitOpen = false;
                t.WaitFormClose();

                t.AlertMessage("Yeni bildirim paketi hazırlandı", "Kontrol ettikten sonra gönderbilirsiniz.");
            }
        }
        public void btn_Navigator_BildirimGonder_Click(object sender, EventArgs e)
        {
            string formName = ((DevExpress.XtraEditors.SimpleButton)sender).AccessibleDescription;
            Form tForm = Application.OpenForms[formName];

            bool onayBildirimlerHazir = hazirlanmisBildirimVarmi(tForm, v.tBildirim);

            if (onayBildirimlerHazir)
            {
                tSMS sms = new tSMS();
                sms.bildirimleriSMSKanaliylaGonder(tForm, v.tBildirim);
                t.AlertMessage("Bildirim paketi ilgili servise gönderildi", "");
            }
        }
        public void btn_Navigator_BildirimSorgula_Click(object sender, EventArgs e)
        {
            string formName = ((DevExpress.XtraEditors.SimpleButton)sender).AccessibleDescription;
            Form tForm = Application.OpenForms[formName];

            bool onayBildirimlerHazir = hazirlanmisBildirimVarmi(tForm, v.tBildirim);
            onayBildirimlerHazir = true;
            if (onayBildirimlerHazir)
            {
                //tSMS sms = new tSMS();
                //sms.bildirimleriSMSKanalindaSorgula(tForm, v.tBildirim);
            }
        }
        public void btn_Navigator_BildirimListeleri_Click(object sender, EventArgs e)
        {
            preparingBildirimHakkinda(sender);

            string FormName = "ms_Bildirimler";
            string FormCode = v.tBildirim.bildirimListesiFormCode;

            tForms fr = new tForms();
            Form tNewForm = null;
            
            tNewForm = fr.Get_Form(FormName);

            // MS_LAYOUT Preparing
            // form code var ise boş formu kullanarak Layout dizayn et
            if (FormCode != "")
            {
                string Prop_Navigator = @"
                  0=FORMNAME:" + FormName + @";
                  0=FORMCODE:" + FormCode + @";
                  0=FORMTYPE:CHILD;
                  0=FORMSTATE:NORMAL;
";
                t.OpenForm(tNewForm, Prop_Navigator);
            }
        }
        public void btn_Navigator_YeniBildirim_Click(object sender, EventArgs e)
        {
            preparingBildirimHakkinda(sender);
            bool onay = false;
            string formName = ((DevExpress.XtraEditors.SimpleButton)sender).AccessibleDescription;
            Form tForm = Application.OpenForms[formName];

            string tableIPCodeHeader = v.tBildirim.TableIPCodeHeader;
            DataSet dsHeader = null;
            DataNavigator dNHeader = null;
            /// Bildirim header tablosu
            t.Find_DataSet(tForm, ref dsHeader, ref dNHeader, tableIPCodeHeader);
            if (dsHeader != null)
            {
                onay = t.TableRefresh(tForm, dsHeader);
            }

            string tableIPCodeLines = v.tBildirim.TableIPCodeLines;
            DataSet dsLines = null;
            DataNavigator dNLines = null;
            /// Bildirim lines tablosu
            t.Find_DataSet(tForm, ref dsLines, ref dNLines, tableIPCodeLines);
            if (dsLines != null)
            {
                onay = t.TableRefresh(tForm, dsLines);
            }

            if (onay)
                t.AlertMessage("Yeni boş bir bildirim paketi hazırlandı.", "Kullanıma hazır.");
        }
        public void btn_Navigator_KalanKontorSorgusu_Click(object sender, EventArgs e)
        {
            preparingBildirimHakkinda(sender);
            Form tForm = Application.OpenForms[v.tBildirim.formName];

            tSMS sms = new tSMS();
            sms.getSMSCreditCount(tForm, v.tBildirim);
        }
        public void tEdit_BildirimSablonlari_EditValueChanged(object sender, EventArgs e)
        {
            string formName = ((DevExpress.XtraEditors.ImageComboBoxEdit)sender).AccessibleDescription;
            getBildirimMesajiAndView(formName);
        }
        private void preparingBildirimHakkinda(object sender)
        {
            string readTableIPCode = ((DevExpress.XtraEditors.SimpleButton)sender).AccessibleName;
            string formName = ((DevExpress.XtraEditors.SimpleButton)sender).AccessibleDescription;
            string bilgiler = ((DevExpress.XtraEditors.SimpleButton)sender).AccessibleDefaultActionDescription;

            string TableIPCodeHeader = t.Get_And_Clear(ref bilgiler, "||");
            string TableIPCodeLines = t.Get_And_Clear(ref bilgiler, "||");
            string bildirimListesiFormCode = t.Get_And_Clear(ref bilgiler, "||");
            string bildirimListesiHeader = t.Get_And_Clear(ref bilgiler, "||");
            string bildirimListesiLines = t.Get_And_Clear(ref bilgiler, "||");
            string workType = t.Get_And_Clear(ref bilgiler, "||");
            string readKeyFieldNames = t.Get_And_Clear(ref bilgiler, "||");

            v.tBildirim.Clear();
            v.tBildirim.formName = formName; 
            v.tBildirim.readTableIPCode = readTableIPCode;
            v.tBildirim.TableIPCodeHeader = TableIPCodeHeader;
            v.tBildirim.TableIPCodeLines = TableIPCodeLines;
            v.tBildirim.bildirimListesiFormCode = bildirimListesiFormCode;
            v.tBildirim.bildirimListesiHeader = bildirimListesiHeader;
            v.tBildirim.bildirimListesiLines = bildirimListesiLines;
            v.tBildirim.workType = workType;
            ///
            /// readKeyFieldNames = "RKEYFNAME": "TalepId=Id##CariId=AdayId##TelefonNo=Lkp_CepTelefonu##AliciAdiSoyadi=Lkp_AdayTamAdiSoyadi##SourceTypeId=21122##SourceId=eSinavNo##"
            ///
            v.tBildirim.hedefTalepIdFName = t.Get_And_Clear(ref readKeyFieldNames, "=");      //  TalepId= 
            v.tBildirim.kaynakTalepIdFName = t.Get_And_Clear(ref readKeyFieldNames, "##");    //  Id ye ulaşıyor 
            v.tBildirim.hedefCariIdFName = t.Get_And_Clear(ref readKeyFieldNames, "=");       //  CariId= 
            v.tBildirim.kaynakCariIdFName = t.Get_And_Clear(ref readKeyFieldNames, "##");     //  AdayId ye ulaşıyor
            v.tBildirim.hedefTelefonNoFName = t.Get_And_Clear(ref readKeyFieldNames, "=");    //  TelefonNo= 
            v.tBildirim.kaynakTelefonNoFName = t.Get_And_Clear(ref readKeyFieldNames, "##");  //  Lkp_CepTelefonu ye ulaşıyor
            v.tBildirim.hedefAliciAdiFName = t.Get_And_Clear(ref readKeyFieldNames, "=");     //  AlicininAdiSoyadi= 
            v.tBildirim.kaynakAliciAdiFName = t.Get_And_Clear(ref readKeyFieldNames, "##");   //  Lkp_TamAdiSoyadi ye ulaşıyor
            v.tBildirim.hedefSourceTypeIdFName = t.Get_And_Clear(ref readKeyFieldNames, "=");      //  SourceTypeId= ye ulaşıyor
            v.tBildirim.kaynakSourceTypeIdValue = t.myInt16(t.Get_And_Clear(ref readKeyFieldNames, "##"));    //  21122 ye ulaşıyor
            v.tBildirim.hedefSourceIdFName = t.Get_And_Clear(ref readKeyFieldNames, "=");          //  SourceId= ye ulaşıyor
            v.tBildirim.kaynakSourceIdFName = t.Get_And_Clear(ref readKeyFieldNames, "##");        //  eSinavNo ya ulaşıyor

            v.tBildirim.hedefDinamik1FName = t.Get_And_Clear(ref readKeyFieldNames, "=");          //  <ESINAVUCRETI>=   ne ulaşıyor
            v.tBildirim.kaynakDinamik1FName = t.Get_And_Clear(ref readKeyFieldNames, "##");        //  Lkp_TaksitTutari  ne ulaşıyor

            v.tBildirim.hedefDinamik2FName = t.Get_And_Clear(ref readKeyFieldNames, "=");          //  <UYGSINAVUCRETI>= ne ulaşıyor
            v.tBildirim.kaynakDinamik2FName = t.Get_And_Clear(ref readKeyFieldNames, "##");        //  Lkp_TaksitTutari  ne ulaşıyor

            v.tBildirim.hedefDinamik3FName = t.Get_And_Clear(ref readKeyFieldNames, "=");          //  <?????>= ne ulaşıyor
            v.tBildirim.kaynakDinamik3FName = t.Get_And_Clear(ref readKeyFieldNames, "##");        //  Lkp_????Tutari  ne ulaşıyor
        }
        private void getBildirimMesajiAndView(string formName)
        {
            Form tForm = Application.OpenForms[formName];
            Control tEdit_BildirimSablonlari = t.Find_Control(tForm, "tEdit_BildirimSablonlari");
            string secilenMesajKodu = ((ImageComboBoxEdit)tEdit_BildirimSablonlari).EditValue.ToString();

            tSQLs Sqls = new tSQLs();
            DataSet ds = new DataSet();
            string tSql = Sqls.Sql_GetBildirimSablonlariList(" and MesajKodu = '" + secilenMesajKodu + "' ");
            t.SQL_Read_Execute(v.dBaseNo.Manager, ds, ref tSql, "HubBildirimSablonlari", "HubBildirimSablonlari");

            string mesajContext = "";
            if (t.IsNotNull(ds))
            {
                mesajContext = ds.Tables[0].Rows[0]["Mesaj"].ToString();
            }

            Control tEdit_BildirimMetni = t.Find_Control(tForm, "tEdit_BildirimMetni");
            ((MemoEdit)tEdit_BildirimMetni).EditValue = mesajContext;
        }
        private void preparingMultiMessage(Form tForm, bildirimPaketi tBildirim_)
        {
            /// listede onaylı kişi varmı kontrol et
            bool listeOnayi = listedeOnayliKisiVarmi(tForm, tBildirim_);

            if (listeOnayi)
            {
                yeniBildirimPaketBasligiOlustur(tForm, tBildirim_);
                yeniBildirimPaketSatirlariOlustur(tForm, tBildirim_);
            }
            else
            {
                MessageBox.Show("Dikkat : Listede onaylı bir kayıt bulunamdı...");
            }
        }
        private void preparingSingleMessage(Form tForm, bildirimPaketi tBildirim_)
        {
            yeniBildirimPaketBasligiOlustur(tForm, tBildirim_);
            yeniBildirimPaketSatirlariOlustur(tForm, tBildirim_);
        }
        private bool listedeOnayliKisiVarmi(Form tForm, bildirimPaketi tBildirim_)
        {
            string readTableIPCode = tBildirim_.readTableIPCode;
            DataSet dsData = null;
            DataNavigator dNData = null;
            /// kaynak olarak okunacak liste
            t.Find_DataSet(tForm, ref dsData, ref dNData, readTableIPCode);

            bool onay = false;
            if (t.IsNotNull(dsData))
            {
                foreach (DataRow row in dsData.Tables[0].Rows)
                {
                    onay = (bool)row["LKP_ONAY"];
                    if (onay)
                    {
                        /// listede bir tane onaylı kişi bulması yeterli
                        break;
                    }
                }
            }
            return onay;
        }
        private void getSecilenBildirimSecenekleri(Form tForm, bildirimPaketi tBildirim_)
        {
            try
            {
                Control tEdit_BildirimKanali = t.Find_Control(tForm, "tEdit_BildirimKanali");
                tBildirim_.secilenKanalTypeId = t.myInt16(((ImageComboBoxEdit)tEdit_BildirimKanali).EditValue.ToString());

                tBildirim_.secilenSablonId = 0;

                Control tEdit_BildirimSablonlari = t.Find_Control(tForm, "tEdit_BildirimSablonlari");
                tBildirim_.secilenMesajKodu = ((ImageComboBoxEdit)tEdit_BildirimSablonlari).EditValue.ToString();

                Control tEdit_BildirimMetni = t.Find_Control(tForm, "tEdit_BildirimMetni");
                tBildirim_.secilenBildirimMetni = ((MemoEdit)tEdit_BildirimMetni).EditValue?.ToString();

                Control tEdit_BildirimSecenekleri = t.Find_Control(tForm, "tEdit_BildirimSecenekleri");
                tBildirim_.secilenGonderimTypeId = t.myInt16(((RadioGroup)tEdit_BildirimSecenekleri).EditValue.ToString());

                Control tEdit_BidirimTarihi = t.Find_Control(tForm, "tEdit_BidirimTarihi");
                tBildirim_.secilenGonderimTarihi = System.Convert.ToDateTime(((DateEdit)tEdit_BidirimTarihi).EditValue.ToString());

                Control tEdit_BildirimSaati = t.Find_Control(tForm, "tEdit_BildirimSaati");
                tBildirim_.secilenGonderimSaati = ((TimeEdit)tEdit_BildirimSaati).EditValue.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata : Bildirim seçeneklerini okurken..." + v.ENTER + ex.Message);
                //throw;
            }
        }
        private bool bildirimTarihVeSaatOnayi(bildirimPaketi tBildirim_)
        {
            bool onay = true;
            if (tBildirim_.secilenGonderimTypeId == 2)
            {
                string soru = "Zamanlanmış gönderim seçeneği için " + v.ENTER2 +
                    "  Tarih : " + tBildirim_.secilenGonderimTarihi.ToString().Substring(0,10) + v.ENTER +
                    "  Saat  : " + tBildirim_.secilenGonderimSaati + v.ENTER2 +
                    "Onaylıyor musunuz ?";
                DialogResult cevap = t.mySoru(soru);
                if (DialogResult.Yes != cevap)
                {
                    onay = false;
                }
            }
            return onay;
        }
        private void yeniBildirimPaketBasligiOlustur(Form tForm, bildirimPaketi tBildirim_)
        {
            string tableIPCode = tBildirim_.TableIPCodeHeader;
            DataSet dsHeader = null;
            DataNavigator dNHeader = null;
            /// Bildirim header tablosu
            t.Find_DataSet(tForm, ref dsHeader, ref dNHeader, tableIPCode);

            int Id = 0;
            if (dsHeader != null)
            {
                if (dsHeader.Tables[0].Rows.Count > 0)
                {
                    Id = t.myInt32(dsHeader.Tables[0].Rows[0]["Id"].ToString());
                    if (Id == 0)
                    {
                        dsHeader.Tables[0].Rows[0]["IsActive"] = 1;
                        dsHeader.Tables[0].Rows[0]["KanalTypeId"] = tBildirim_.secilenKanalTypeId;
                        dsHeader.Tables[0].Rows[0]["SablonId"] = tBildirim_.secilenSablonId;
                        dsHeader.Tables[0].Rows[0]["MesajKodu"] = tBildirim_.secilenMesajKodu;
                        dsHeader.Tables[0].Rows[0]["BildirimMetni"] = tBildirim_.secilenBildirimMetni;
                        dsHeader.Tables[0].Rows[0]["GonderimTypeId"] = tBildirim_.secilenGonderimTypeId;
                        dsHeader.Tables[0].Rows[0]["GonderimTarihi"] = tBildirim_.secilenGonderimTarihi;
                        dsHeader.Tables[0].Rows[0]["GonderimSaati"] = tBildirim_.secilenGonderimSaati;

                        if (tBildirim_.workType == "MESSAGESINGLE") dsHeader.Tables[0].Rows[0]["WorkTypeId"] = 1;
                        if (tBildirim_.workType == "MESSAGEMULTI") dsHeader.Tables[0].Rows[0]["WorkTypeId"] = 2;

                        dsHeader.Tables[0].AcceptChanges();

                        tSave sv = new tSave();
                        sv.tDataSave(tForm, dsHeader, dNHeader, dNHeader.Position);

                        tBildirim_.headerTableIdValue = t.myInt32(dsHeader.Tables[0].Rows[0]["Id"].ToString());
                    }
                    else
                    {
                        tBildirim_.headerTableIdValue = Id;
                    }
                }
            }
        }
        private void yeniBildirimPaketSatirlariOlustur(Form tForm, bildirimPaketi tBildirim_)
        {
            if (tBildirim_.headerTableIdValue == 0)
            {
                MessageBox.Show("Dikkat : Bildirim paketi Id bulunamadı...");
                return;
            }
            string readTableIPCode = tBildirim_.readTableIPCode;
            string tableIPCodeLines = tBildirim_.TableIPCodeLines;


            DataSet dsData = null;
            DataNavigator dNData = null;
            /// Kaynak olarak okunacak tablo
            t.Find_DataSet(tForm, ref dsData, ref dNData, readTableIPCode);

            DataSet dsLines = null;
            DataNavigator dNLines = null;
            /// Bildirim lines tablosu
            t.Find_DataSet(tForm, ref dsLines, ref dNLines, tableIPCodeLines);


            bool onay = false;
            if (t.IsNotNull(dsData))
            {
                foreach (DataRow row in dsData.Tables[0].Rows)
                {
                    try
                    {
                        if (tBildirim_.workType == "MESSAGEMULTI")
                            onay = (bool)row["LKP_ONAY"];
                        if (tBildirim_.workType == "MESSAGESINGLE")
                            onay = true;
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Dikkat : LKP_ONAY  isimli onay fieldi bulunamadı...");
                        break;
                        //throw;
                    }
                    

                    if (onay)
                    {
                        if (t.IsNotNull(tBildirim_.kaynakTalepIdFName))
                            tBildirim_.kaynakTalepIdValue = (int)row[tBildirim_.kaynakTalepIdFName];

                        if (t.IsNotNull(tBildirim_.kaynakCariIdFName))
                            tBildirim_.kaynakCariIdValue = (int)row[tBildirim_.kaynakCariIdFName];

                        if (t.IsNotNull(tBildirim_.kaynakTelefonNoFName))
                            tBildirim_.kaynakTelefonNoValue = (string)row[tBildirim_.kaynakTelefonNoFName];

                        if (t.IsNotNull(tBildirim_.kaynakAliciAdiFName))
                            tBildirim_.kaynakAliciAdiValue = (string)row[tBildirim_.kaynakAliciAdiFName];

                        if (t.IsNotNull(tBildirim_.hedefSourceTypeIdFName))
                            tBildirim_.kaynakSourceTypeIdValue = tBildirim_.kaynakSourceTypeIdValue;

                        if (t.IsNotNull(tBildirim_.kaynakSourceIdFName))
                            tBildirim_.kaynakSourceIdValue = t.myInt32(row[tBildirim_.kaynakSourceIdFName].ToString());

                        if (t.IsNotNull(tBildirim_.kaynakDinamik1FName))
                            tBildirim_.kaynakDinamik1Value = row[tBildirim_.kaynakDinamik1FName].ToString();

                        if (t.IsNotNull(tBildirim_.kaynakDinamik2FName))
                            tBildirim_.kaynakDinamik2Value = row[tBildirim_.kaynakDinamik2FName].ToString();

                        if (t.IsNotNull(tBildirim_.kaynakDinamik3FName))
                            tBildirim_.kaynakDinamik3Value = row[tBildirim_.kaynakDinamik3FName].ToString();

                        setBildirimLines(tForm, dsLines, dNLines, tBildirim_);
                    }
                }
            }
        }
        private void setBildirimLines(Form tForm, DataSet dsLines, DataNavigator dNLines, bildirimPaketi tBildirim_)
        {
            string tableIPCode = tBildirim_.TableIPCodeLines;
            string value = "";

            DataRow newRow = dsLines.Tables[0].NewRow();

            using (tDefaultValue df = new tDefaultValue())
            {
                df.tDefaultValue_And_Validation
                    (tForm,
                     dsLines,
                     newRow,
                     tableIPCode,
                     "tData_NewRecord");
            }

            newRow["MesajKodu"] = tBildirim_.secilenMesajKodu;
            newRow["CrsBildirimBId"] = tBildirim_.headerTableIdValue;
            newRow[tBildirim_.hedefTalepIdFName] = tBildirim_.kaynakTalepIdValue.ToString();
            newRow[tBildirim_.hedefCariIdFName] = tBildirim_.kaynakCariIdValue.ToString();
            newRow[tBildirim_.hedefTelefonNoFName] = tBildirim_.kaynakTelefonNoValue.ToString();
            newRow[tBildirim_.hedefAliciAdiFName] = tBildirim_.kaynakAliciAdiValue.ToString();
            newRow[tBildirim_.hedefSourceTypeIdFName] = t.myInt16(tBildirim_.kaynakSourceTypeIdValue.ToString());
            newRow[tBildirim_.hedefSourceIdFName] = t.myInt32(tBildirim_.kaynakSourceIdValue.ToString());

            /// <UYGSINAVUCRETI>   <<<--- 1350
            if (t.IsNotNull(tBildirim_.kaynakDinamik1FName) && t.IsNotNull(tBildirim_.kaynakDinamik1Value))
            {
                value = tBildirim_.kaynakDinamik1Value.ToString();
                if (value.Length > 6)
                {
                    value = value.Replace(",00000", "");
                    value = value.Replace(".00000", "");
                }
                newRow[tBildirim_.hedefDinamik1FName] = value;
            }
            if (t.IsNotNull(tBildirim_.kaynakDinamik2FName) && t.IsNotNull(tBildirim_.kaynakDinamik2Value))
            {
                value = tBildirim_.kaynakDinamik2Value.ToString();
                if (value.Length > 6)
                {
                    value = value.Replace(",00000", "");
                    value = value.Replace(".00000", "");
                }
                newRow[tBildirim_.hedefDinamik2FName] = value;
            }
            if (t.IsNotNull(tBildirim_.kaynakDinamik3FName) && t.IsNotNull(tBildirim_.kaynakDinamik3Value))
            {
                value = tBildirim_.kaynakDinamik3Value.ToString();
                if (value.Length > 6)
                {
                    value = value.Replace(",00000", "");
                    value = value.Replace(".00000", "");
                }
                newRow[tBildirim_.hedefDinamik3FName] = value;
            }

            dsLines.Tables[0].Rows.Add(newRow);

            NavigatorButton btnL = dNLines.Buttons.Last;
            dNLines.Buttons.DoClick(btnL);

            tSave sv = new tSave();
            sv.tDataSave(tForm, dsLines, dNLines, dNLines.Position);
        }
        private bool hazirlanmisBildirimVarmi(Form tForm, bildirimPaketi tBildirim_)
        {
            if (tBildirim_.headerTableIdValue == 0)
            {
                bool listeOnayi = listedeOnayliKisiVarmi(tForm, tBildirim_);

                if (listeOnayi)
                {
                    yeniBildirimPaketBasligiOlustur(tForm, tBildirim_);
                }

                if (tBildirim_.headerTableIdValue == 0)
                {
                    MessageBox.Show("Dikkat : Hazırlanmış bildirim paketi bulunamadı...");
                    return false;
                }
            }
            string tableIPCodeLines = tBildirim_.TableIPCodeLines;

            DataSet dsLines = null;
            DataNavigator dNLines = null;
            /// Bildirim lines tablosu
            t.Find_DataSet(tForm, ref dsLines, ref dNLines, tableIPCodeLines);

            bool onay = t.IsNotNull(dsLines);

            if (onay == false)
                MessageBox.Show("Dikkat : Henüz hazırlanmış bildirimler bulunmadı...");

            return onay;
        }

        /// 
        /// Bildirim sürecinin sonu
        /// 

        public void btn_CheckButton_CheckedChanged(object sender, EventArgs e)
        {
            bool checked_ = ((DevExpress.XtraEditors.CheckButton)sender).Checked;
            if (checked_)
            {
                ((DevExpress.XtraEditors.CheckButton)sender).Text = "@ " + ((DevExpress.XtraEditors.CheckButton)sender).Text;
                //((DevExpress.XtraEditors.CheckButton)sender).Appearance.BackColor = System.Drawing.Color.LightGreen;

            }
            else
            {
                ((DevExpress.XtraEditors.CheckButton)sender).Text = ((DevExpress.XtraEditors.CheckButton)sender).Text.Replace("@ ", "");
                //((DevExpress.XtraEditors.CheckButton)sender).Appearance.BackColor = System.Drawing.Color.LightYellow;

            }
        }

        #endregion

        // düzenle
        public void tdataNavigator_ButtonClick(object sender, NavigatorButtonClickEventArgs e)
        {
            if (e.Button.ButtonType.ToString() == "EndEdit")
            {

                object tDataTable = ((DevExpress.XtraEditors.DataNavigator)sender).DataSource;
                DataSet dsData = ((DataTable)tDataTable).DataSet;

                object x = ((DevExpress.XtraEditors.DataNavigator)sender).Tag;
                int Old_ID = ((int)x);

                if (e.Button.ButtonType.ToString() == "EndEdit")
                {
                    /// bu şarta gerek duyarsan 
                    /// NavigatorButton btnEnd = dN_MSFieldsIP.Buttons.EndEdit; çalıştırdğın satırdan önce
                    /// dN_MSFieldsIP.Tag = dN_MSFieldsIP.Position;   satırını yerleştirin
                    /// 
                    ////if (
                    ////(((DevExpress.XtraEditors.DataNavigator)sender).Position > -1) &&
                    ////(((DevExpress.XtraEditors.DataNavigator)sender).Position > Rec_ID)
                    ////)
                    ////    Old_ID = ((DevExpress.XtraEditors.DataNavigator)sender).Position;

                    // burası standart tDataSave değil 
                    tSave sv = new tSave();
                    Form tForm = t.Find_Form((DataNavigator)sender);
                    sv.tDataSave(tForm, dsData, ((DevExpress.XtraEditors.DataNavigator)sender), Old_ID);
                }

                dsData.Dispose();
            }

            //e.Button.ButtonType == NavigatorButtonType.Edit
            if (e.Button.ButtonType.ToString() == "Append")
            {
                // Append İşlemini durdur
                if (v.con_Cancel) e.Handled = true;
            }

            // hangi butona basıldığının bilgisini saklıyor
            // text e tDetail_SubDetail_Read bilgisi atılıyor onun için başka bir properties bul eğer gerekirse

            //((DevExpress.XtraEditors.DataNavigator)sender).Text = e.Button.ButtonType.ToString();
        }
        
        // bunu silme bu bir nesne eventi 
        public void barButtonItem_NavigotorItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            string TableIPCode = string.Empty;
            string ButtonName = string.Empty;
            string Caption = string.Empty;
            string Prop_Navigator = string.Empty;
            int Button_Click_Type = 0;

            if (e.Item.GetType().ToString() == "DevExpress.XtraBars.BarLargeButtonItem")
            {
                ButtonName = ((DevExpress.XtraBars.BarLargeButtonItem)e.Item).Name.ToString();
                Caption = ((DevExpress.XtraBars.BarLargeButtonItem)e.Item).Caption;
                TableIPCode = ((DevExpress.XtraBars.BarLargeButtonItem)e.Item).AccessibleName;
                if (((DevExpress.XtraBars.BarLargeButtonItem)e.Item).Tag != null)
                    Button_Click_Type = (int)((DevExpress.XtraBars.BarLargeButtonItem)e.Item).Tag;
            }

            if (e.Item.GetType().ToString() == "DevExpress.XtraBars.BarButtonItem")
            {
                ButtonName = ((DevExpress.XtraBars.BarButtonItem)e.Item).Name.ToString();
                Caption = ((DevExpress.XtraBars.BarButtonItem)e.Item).Caption;
                TableIPCode = ((DevExpress.XtraBars.BarButtonItem)e.Item).AccessibleName;
                if (((DevExpress.XtraBars.BarButtonItem)e.Item).Tag != null)
                    Button_Click_Type = (int)((DevExpress.XtraBars.BarButtonItem)e.Item).Tag;
            }

            // simpleButton_  ifadesi isimden siliniyor
            ButtonName = ButtonName.Substring(13, ButtonName.Length - 13);

            Prop_Navigator = "BarButtonItem";

            // 1. formu tespit edilir
            Form tForm = t.Find_Form(sender);
            //btn_Navigotor_Click(tForm, null, TableIPCode, ButtonName, Caption, Prop_Navigator, Button_Click_Type);

            v.tButtonHint.Clear();
            v.tButtonHint.tForm = tForm;
            v.tButtonHint.tableIPCode = TableIPCode;
            v.tButtonHint.buttonType = t.getClickType(Button_Click_Type);
            tEventsButton evb = new tEventsButton();
            evb.btnClick(v.tButtonHint);

        }


        //*****************************************************************************


        #region SubFunctions for Event

        public string ButtonEdit_AND(DataRow Row)
        {
            //  TLKP_TYPE
            //--101 = Proje Seçenekleri (Lkp)
            //--103 = Kullanıcı Seçenekleri (Lkp)
            //--105 = Data Seçenekleri (Lkp)

            //  FLKP_TYPE
            //--4 = Join for 1 Field (Kaynak Tablo)
            //--5 = Join for 2 Fields (Kaynak Tablo)
            //--6 = Join for Partner Fields

            string s = string.Empty;
            Int16 Lkp_Type = System.Convert.ToInt16(Row["LKP_FLKP_TYPE"].ToString());

            if (Row["LKP_PARTNER_FIELD_NAME"] != null)
                if (Row["LKP_PARTNER_FIELD_NAME"].ToString() != "")
                {
                    s = " and a.TABLE_CODE = '" + Row["LKP_TABLE_CODE"].ToString() + "' " + v.ENTER +
                        " and a.TABLE_ID_FNAME = '" + Row["LKP_PARTNER_FIELD_NAME"].ToString() + "' " + v.ENTER;
                }

            return s;
        }

        #endregion SubFunctions for Event
               

        #region tProperties Buttons Click  >>

        public void btn_PropertiesNav_Click(object sender, EventArgs e)
        {


            #region Tanımlar
            // tüm bu olayların oluştuğu form tespit ediliyor
            Form tForm = t.Find_Form(sender);
            DataSet dsData = null;
            DataNavigator dN = null;

            string TableIPCode = ((DevExpress.XtraEditors.SimpleButton)sender).AccessibleName;
            string tbutton = ((DevExpress.XtraEditors.SimpleButton)sender).Text;

            #endregion Tanımlar

            #region Close
            if (tbutton == "&Close")
            {
                tForm.Close();
            }
            #endregion Close

            #region Update
            if (tbutton == "&Update")
            {
                t.Find_DataSet(tForm, ref dsData, ref dN, TableIPCode);

                if (t.IsNotNull(dsData))
                {
                    string onay = string.Empty;
                    string lkp_Caption = string.Empty;
                    string lkp_newCaption = string.Empty;
                    string lkp_newWidth = string.Empty;
                    string value = string.Empty;

                    int i2 = dsData.Tables[0].Rows.Count;
                    for (int i = 0; i < i2; i++)
                    {
                        onay = dsData.Tables[0].Rows[i]["LKP_ONAY"].ToString();
                        if (onay == "True")
                        {
                            lkp_Caption = dsData.Tables[0].Rows[i]["LKP_CAPTION"].ToString();
                            lkp_newCaption = dsData.Tables[0].Rows[i]["LKP_NEW_CAPTION"].ToString();
                            lkp_newWidth = dsData.Tables[0].Rows[i]["LKP_NEW_WIDTH"].ToString();

                            if (t.IsNotNull(lkp_newCaption) == false) lkp_newCaption = "null";
                            if (t.IsNotNull(lkp_newWidth) == false) lkp_newWidth = "0";

                            /// onaylı satırlar toparlanarak kaydedilmesi için 
                            /// çağrıldğı noktaya gönderilecek
                            /// 
                            value = value
                                + "btn:"
                                + i.ToString() + "||" // index
                                + lkp_Caption + "||"
                                + lkp_newCaption + "||"
                                + lkp_newWidth + "|ds|" + v.ENTER;
                        }
                    }

                    tForm.AccessibleDefaultActionDescription = value;

                    tForm.Close();
                }
            }
            #endregion Update

            #region Clear
            if (tbutton == "&Clear")
            {
                t.Find_DataSet(tForm, ref dsData, ref dN, TableIPCode);

                int i2 = dsData.Tables[0].Rows.Count;
                for (int i = 0; i < i2; i++)
                {
                    dsData.Tables[0].Rows[i]["LKP_ONAY"] = 0;
                }
            }
            #endregion Clear
        }

        public void btn_Properties_Click(object sender, EventArgs e)
        {


            string prp_type = ((DevExpress.XtraEditors.SimpleButton)sender).AccessibleName;

            #region Tanımlar
            // tüm bu olayların oluştuğu form tespit ediliyor
            Form tForm = t.Find_Form(sender);

            string tbutton = ((DevExpress.XtraEditors.SimpleButton)sender).Text;

            // VgridControl tespit ediliyor
            string[] controls = new string[] { "DevExpress.XtraVerticalGrid.VGridControl" };
            Control cntrl = t.Find_Control(tForm, "", "tProperties", controls);

            #endregion Tanımlar

            #region Close
            if (tbutton == "&Close")
            {
                tPreperties_SingleBlock_Add(tForm, (DevExpress.XtraVerticalGrid.VGridControl)cntrl, prp_type);

                tForm.Close();
            }
            if (tbutton == "&Close Memo")
            {
                tForm.Close();
            }
            #endregion Close

            #region Update
            if (tbutton == "&Update")
            {
                tPreperties_SingleBlock_Add(tForm, (DevExpress.XtraVerticalGrid.VGridControl)cntrl, prp_type);
            }
            #endregion Update

            #region Update Memo (properties) / Memo Add (propertiesplus)
            if ((tbutton == "&Update Memo") ||
                (tbutton == "&Memo Add"))
            {
                string memoName = "";

                //if (tbutton == "&Update Memo") memoName = "memoEdit_PropertiesValue";// "MemoEdit1";
                //if (tbutton == "&Memo Add")

                memoName = "memoEdit_PropertiesValue";

                Control memo = t.Find_Control(tForm, memoName);

                if (memo != null)
                {
                    string value = string.Empty;
                    if (((DevExpress.XtraEditors.MemoEdit)memo).EditValue != null)
                        value = ((DevExpress.XtraEditors.MemoEdit)memo).EditValue.ToString();

                    if (t.IsNotNull(value) == false)
                    {
                        string soru = "Veriler silinecek, Onaylıyor musunuz ?";
                        DialogResult cevap = t.mySoru(soru);
                        if (DialogResult.Yes == cevap)
                        {
                            tForm.AccessibleDefaultActionDescription = value;
                        }
                    }
                    else tForm.AccessibleDefaultActionDescription = value;
                }
            }
            #endregion Update

            #region Clear
            if (tbutton == "&Clear")
            {
                ((DevExpress.XtraVerticalGrid.VGridControl)cntrl).FocusPrev();

                tProperties_Button_Clear((DevExpress.XtraVerticalGrid.VGridControl)cntrl);
            }
            #endregion Clear

            #region RowCollapse
            if (tbutton == "Kpt")
            {
                ((DevExpress.XtraVerticalGrid.VGridControl)cntrl).CollapseAllRows();
            }
            #endregion RowCollapse

            #region RowExpand
            if (tbutton == "Aç")
            {
                ((DevExpress.XtraVerticalGrid.VGridControl)cntrl).ExpandAllRows();
            }
            #endregion RowExpand

            #region Add (New Block Add)
            if (tbutton == "&Add")
            {
                tProperties_Button_Add(tForm, (DevExpress.XtraVerticalGrid.VGridControl)cntrl, prp_type);
            }
            #endregion Add

            #region Memo Add (New Memo Add)
            if (tbutton == "&Memo Add")
            {

            }
            #endregion Add

            #region Delete
            if (tbutton == "&Delete")
            {
                tProperties_Button_Delete(tForm, (DevExpress.XtraVerticalGrid.VGridControl)cntrl);
            }
            #endregion Delete

        }

        public void btn_Properties_JSON_Click(object sender, EventArgs e)
        {


            string prp_type = ((DevExpress.XtraEditors.SimpleButton)sender).AccessibleName;

            #region Tanımlar
            // tüm bu olayların oluştuğu form tespit ediliyor
            Form tForm = t.Find_Form(sender);

            string tbutton = ((DevExpress.XtraEditors.SimpleButton)sender).Text;

            // VgridControl tespit ediliyor
            string[] controls = new string[] { "DevExpress.XtraVerticalGrid.VGridControl" };
            Control cntrl = t.Find_Control(tForm, "", "tProperties", controls);

            #endregion Tanımlar

            #region Close
            if (tbutton == "&Close")
            {
                tPreperties_SingleBlock_Add_JSON(tForm, (DevExpress.XtraVerticalGrid.VGridControl)cntrl);//, prp_type);

                tForm.Close();
            }
            if (tbutton == "&Close Memo")
            {
                tForm.Close();
            }
            #endregion Close

            #region Update
            if (tbutton == "&Update")
            {
                tPreperties_SingleBlock_Add_JSON(tForm, (DevExpress.XtraVerticalGrid.VGridControl)cntrl);//, prp_type);
            }
            #endregion Update

            #region Update Memo (properties) / Memo Add (propertiesplus)
            if ((tbutton == "&Update Memo") ||
                (tbutton == "&Memo Add"))
            {
                string memoName = "";

                //if (tbutton == "&Update Memo") memoName = "MemoEdit1";
                //if (tbutton == "&Memo Add") memoName = "memoEdit_PropertiesValue";
                memoName = "memoEdit_PropertiesValue";

                Control memo = t.Find_Control(tForm, memoName);

                if (memo != null)
                {
                    string value = string.Empty;
                    if (((DevExpress.XtraEditors.MemoEdit)memo).EditValue != null)
                        value = ((DevExpress.XtraEditors.MemoEdit)memo).EditValue.ToString();

                    if (t.IsNotNull(value) == false)
                    {
                        string soru = "Veriler silinecek, Onaylıyor musunuz ?";
                        DialogResult cevap = t.mySoru(soru);
                        if (DialogResult.Yes == cevap)
                        {
                            tForm.AccessibleDefaultActionDescription = value;
                        }
                    }
                    else tForm.AccessibleDefaultActionDescription = value;
                }
            }
            #endregion Update

            #region Clear
            if (tbutton == "&Clear")
            {
                ((DevExpress.XtraVerticalGrid.VGridControl)cntrl).FocusPrev();

                tProperties_Button_Clear((DevExpress.XtraVerticalGrid.VGridControl)cntrl);
            }
            #endregion Clear

            #region RowCollapse
            if (tbutton == "Kpt")
            {
                ((DevExpress.XtraVerticalGrid.VGridControl)cntrl).CollapseAllRows();
            }
            #endregion RowCollapse

            #region RowExpand
            if (tbutton == "Aç")
            {
                ((DevExpress.XtraVerticalGrid.VGridControl)cntrl).ExpandAllRows();
            }
            #endregion RowExpand

            #region Add (New Block Add)
            if (tbutton == "&Add")
            {
                tProperties_Button_Add_JSON(tForm, (DevExpress.XtraVerticalGrid.VGridControl)cntrl);//, prp_type);
            }
            #endregion Add

            #region Memo Add (New Memo Add)
            if (tbutton == "&Memo Add")
            {

            }
            #endregion Add

            #region Delete
            if (tbutton == "&Delete")
            {
                tProperties_Button_Delete(tForm, (DevExpress.XtraVerticalGrid.VGridControl)cntrl);
            }
            #endregion Delete

        }


        #region Sub tProperties Functions

        private void tProperties_Button_Clear(VGridControl VGrid)
        {

            string function_name = "tProperties_Button_Clear";
            t.Takipci(function_name, "", '{');

            int t1 = VGrid.Rows.Count;
            int t2 = 0;

            for (int i1 = 0; i1 < t1; i1++)
            {
                // category altındaki edittext sayısı
                t2 = VGrid.Rows[i1].ChildRows.Count;

                for (int i2 = 0; i2 < t2; i2++)
                {
                    VGrid.Rows[i1].ChildRows[i2].Properties.Value = "";
                }
            }

            t.Takipci(function_name, "", '}');
        }

        private void tProperties_Button_Add(Form tForm, VGridControl VGrid, string prp_type)
        {

            string function_name = "tProperties Button_Add";
            t.Takipci(function_name, "", '{');

            if (VGrid != null)
            {
                // Mevcut VGridi olanı Blockla / Paketle
                tPreperties_SingleBlock_Add(tForm, VGrid, prp_type);

                // Yeni ID tespit edilecek
                string MainFName = VGrid.AccessibleDescription.ToString();
                string OldValue = tProperties_Memo_Value_Get(tForm);
                int NewId = tProperties_New_Block_Id(MainFName, OldValue);

                // Yeni ID tespit edildiyse 
                if (NewId > 0)
                {
                    // testpit edilen yeni ID yi ekrandaki textEdit e set et
                    tProperties_textEdit_Value_Set(tForm, NewId.ToString());

                    // VGridi önce temizle
                    tProperties_Button_Clear(VGrid);

                    tForm.Refresh();

                    // boş bir şekilde yeni blokla / pakele ve onuda FullBlocks üzerine set et
                    tPreperties_SingleBlock_Add(tForm, VGrid, prp_type);
                }
            }

            t.Takipci(function_name, "", '}');
        }

        private void tProperties_Button_Add_JSON(Form tForm, VGridControl VGrid)//, string prp_type)
        {


            if (VGrid != null)
            {
                // Mevcut VGridi olanı Blockla / Paketle
                tPreperties_SingleBlock_Add_JSON(tForm, VGrid);//, prp_type);

                string TableName = "";
                if (VGrid.Tag != null)
                    VGrid.Tag.ToString();
                string Main_FieldName = VGrid.AccessibleDescription.ToString();

                /// yeni boş bir blok oluşturmak için
                string thisValues = "";
                string TableIPCode = "tPropertiesPlusEdit";
                string paket_basi_Mevcut = "";

                string paket_turu = tPropertiesPacketValue_Preparing(TableName, Main_FieldName,
                       ref thisValues, ref paket_basi_Mevcut, TableIPCode);

                tProperties_VGrid_Value_Set_JSON(VGrid, thisValues);
            }

        }

        private void tProperties_Button_Delete(Form tForm, VGridControl VGrid)
        {


            DialogResult answer = t.mySoru("sil");

            switch (answer)
            {
                case DialogResult.No:
                    {
                        string OldFullBlockValues = tProperties_Memo_Value_Get(tForm);
                        string remove_value = string.Empty;
                        if (VGrid.AccessibleDefaultActionDescription != null)
                            remove_value = VGrid.AccessibleDefaultActionDescription.ToString();
                        //string remove_value = v.con_Value_Old + v.ENTER;

                        t.Str_Remove(ref OldFullBlockValues, remove_value);

                        OldFullBlockValues = OldFullBlockValues + v.ENTER;

                        // kırpıntı kalırsa sıfırlasın 
                        if (OldFullBlockValues.Length < 20)
                        {
                            OldFullBlockValues = "";

                            // block Id yide sıfırla
                            tProperties_textEdit_Value_Set(tForm, "0");

                            // En son olarak VGridi temizle
                            VGrid.FocusPrev();
                            tProperties_Button_Clear(VGrid);
                        }

                        // Memoya kalanı set et
                        tProperties_Memo_Value_Set(tForm, OldFullBlockValues);
                        // listboxtan sil
                        tProperties_listBoxControl_Value_Remove(tForm, remove_value);
                        // VGrid Enabledi düzenle
                        tProperties_VGrid_Enabled(tForm, VGrid);

                        break; // break ifadesini sakın silme
                    }
            }

        }

        //-------------------

        public void tProperties_VGrid_Enabled(Form tForm, VGridControl VGrid)
        {


            //Form tForm = Application.OpenForms[""];

            string i = tProperties_textEdit_Value_Get(tForm);

            VGrid.Enabled = (t.myInt32(i) > 0);
        }

        //---JSON        
        private void tPreperties_SingleBlock_Add_JSON(Form tForm, VGridControl VGrid)//, string prp_type)
        {
            //

            string OldBlock = string.Empty;
            string NewBlock = string.Empty;

            tProperties_VGrid_Values_Get_JSON(tForm, VGrid, ref OldBlock, ref NewBlock);

            // Paketlenen yeni block u mevcut FullBlocks ile add veya update et

            tProperties_FullBlocks_Add_JSON(tForm, VGrid, OldBlock, NewBlock);
        }

        private void tProperties_VGrid_Values_Get_JSON(Form tForm, VGridControl VGrid,
                     ref string OldValue, ref string NewValue)
        {
            string block = string.Empty;
            string fname = string.Empty;
            string value = string.Empty;

            int t1 = VGrid.Rows.Count;
            int t2 = 0;

            for (int i1 = 0; i1 < t1; i1++)
            {
                // category altındaki edittext sayısı
                t2 = VGrid.Rows[i1].ChildRows.Count;

                for (int i2 = 0; i2 < t2; i2++)
                {
                    fname = VGrid.Rows[i1].ChildRows[i2].Properties.FieldName.ToString();
                    value = VGrid.Rows[i1].ChildRows[i2].Properties.Value.ToString();
                    if (value == "") value = "null";

                    /// "animal": "dog",
                    /// şeklinde fieldName ve value birleştirmesi gerçekleştiriliyor
                    /// en başta iki adet boşluk var silme
                    /// en sonda bir adet virgül var 
                    if ((value.IndexOf("{") > -1) && (value.IndexOf("{") < 5))
                    {
                        block = block +
                            "  " + (char)34 + fname + (char)34 + ": " + value + "," + v.ENTER;
                    }
                    else if (value.IndexOf(": [") > -1)
                    {
                        block = block + value + "," + v.ENTER;
                    }
                    else
                    {
                        block = block +
                            "  " + (char)34 + fname + (char)34 + ": " + (char)34 + value + (char)34 + "," + v.ENTER;
                    }

                }
            }

            /// boşluklar alınıyor 
            /// block = block.Trim(); 
            /// ? trim en baştaki boşlukları siliyor, bunu silinmemesi gerekiyor 
            /// en sondaki boşluk ve ENTER işareti silinecek 
            block = block.TrimEnd();
            ///
            /// en sondaki virgül siliniyor
            ///
            block = block.Substring(0, block.Length - 1);

            /// artık min seviyedeki paket hazır
            /// 
            ///* örnek çıkış           
            ///
            ///{
            ///  "CAPTION": "A konusu",
            ///  "SV_VALUE": "null",
            ///  "TABLEIPCODE": "null"
            ///}
            NewValue = "{" + v.ENTER + block + v.ENTER + "}";

            /// "SV_ENABLED": "null",
            /// "SV_KEYFNAME": "null",
            /// "SV_CAPTION_FNAME": "null",
            /// "SV_CMP_TYPE": "null",
            /// "SV_CMP_LOCATION": "null",
            /// "SV_VIEW_TYPE": "null",
            /// "SV_LIST": []

            if (VGrid.AccessibleDefaultActionDescription != null)
                OldValue = VGrid.AccessibleDefaultActionDescription.ToString();
            else OldValue = "";

            VGrid.AccessibleDefaultActionDescription = NewValue;
        }

        private void tProperties_FullBlocks_Add_JSON(Form tForm, VGridControl VGrid, string OldBlock, string NewBlock)
        {



            int itemId = 0;
            string s1 = string.Empty;
            string s2 = string.Empty;
            string s3 = string.Empty;
            bool list = false;

            string OldFullBlock = tProperties_Memo_Value_Get(tForm);
            Newtonsoft.Json.Linq.JArray myList = null;

            if ((t.IsNotNull(OldFullBlock)) &&
                (OldFullBlock.IndexOf("[") > -1) &&
                (OldFullBlock.IndexOf("[") < 5))
            {

                myList = (Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject(OldFullBlock);

                s1 = myList.ToString();

                /// myList içinde [] işaretleri var ise array yapısı mevcut olduğunu gösterir
                /// Eğer bu işaretler var ise diğerlerinide aynı yapıya çevirmek gerkerir.
                ///

                OldBlock = "[" + v.ENTER + OldBlock + v.ENTER + "]";
                NewBlock = "[" + v.ENTER + NewBlock + v.ENTER + "]";

                list = true;
            }
            /// yoksa birde bunu kontrol et
            /// 
            if ((t.IsNotNull(OldFullBlock)) &&
                (OldFullBlock.IndexOf("{") > -1) &&
                (OldFullBlock.IndexOf("{") < 5))
            {
                //myList 
                var myL = JsonConvert.DeserializeObject(OldFullBlock);

                s1 = myL.ToString();
            }

            var myOldList = JsonConvert.DeserializeObject(OldBlock);
            var myNewList = JsonConvert.DeserializeObject(NewBlock);
            try
            {
                s2 = myOldList.ToString();
                s3 = myNewList.ToString();
            }
            catch (Exception)
            {
                s2 = "";
                s3 = "";
            }
            
            if ((list) && (s2.Length > 6) && (s3.Length > 6))
            {
                s2 = s2.Remove(0, 3);
                s2 = s2.Remove(s2.Length - 3, 3);
                s3 = s3.Remove(0, 3);
                s3 = s3.Remove(s3.Length - 3, 3);
            }

            ///if (OldFullBlock.IndexOf(OldBlock) > -1)
            if (s1.IndexOf(s2) > -1)
            {
                /// eğer tüm liste içinde değiştirilen liste var ise
                /// mevcut liste içinde bu eski item yeni item ile değiştirilecek

                //t.Str_Replace(ref OldFullBlock, OldBlock, NewBlock);
                t.Str_Replace(ref s1, s2, s3);

                OldFullBlock = s1;

                /*
                var aa = ((Newtonsoft.Json.Linq.JArray)myList)[0];
                var xx = aa.Children();
                foreach (var item in xx)
                {

                }
                */
            }
            else
            {
                /// eğer tüm liste içinde bu liste yok ise 
                /// mevcut listeye eklenecek
                /// 
                ///((Newtonsoft.Json.Linq.JArray)myList).Add(myNewList);
                var s4 = JsonConvert.DeserializeObject(s3);

                if (myList != null)
                {
                    ((Newtonsoft.Json.Linq.JArray)myList).Add(s4);
                    OldFullBlock = JsonConvert.SerializeObject(myList, Newtonsoft.Json.Formatting.Indented);
                }
                else
                {
                    OldFullBlock = JsonConvert.SerializeObject(s4, Newtonsoft.Json.Formatting.Indented);
                }

                itemId = -1;
            }

            /// Memoya set et
            ///
            tProperties_Memo_Value_Set(tForm, OldFullBlock);

            /// Listeye Captionunu ekle
            tProperties_listBoxControl_Value_Set_JSON(tForm, NewBlock, itemId);

            /// 
            tForm.AccessibleDefaultActionDescription = OldFullBlock;

        }

        public void tProperties_listBoxControl_SelectedIndexChanged_JSON(object sender, EventArgs e)
        {
            if (((DevExpress.XtraEditors.ListBoxControl)sender).Tag != null)
                if (((DevExpress.XtraEditors.ListBoxControl)sender).Tag.ToString() == "1") return;

            string line_value = ((DevExpress.XtraEditors.ListBoxControl)sender).SelectedValue.ToString();
            string prp_type = ((DevExpress.XtraEditors.ListBoxControl)sender).AccessibleName;
            string MainFName = ((DevExpress.XtraEditors.ListBoxControl)sender).AccessibleDescription;

            tProperties_listBoxControlSelectedChanged_JSON(line_value, MainFName);
        }

        private void tProperties_listBoxControlSelectedChanged_JSON(string line_value, string MainFName)
        {


            //Form tForm = Application.OpenForms["tForm_" + prp_type];
            Form tForm = Application.OpenForms["tForm_" + MainFName];

            if (tForm != null)
            {
                Control cntrl = tProperties_VGrid_Get(tForm);
                //string MainFName = ((DevExpress.XtraVerticalGrid.VGridControl)cntrl).AccessibleDescription.ToString();
                string Block_Id = t.Get_And_Clear(ref line_value, ":");
                string FullBlockValue = tProperties_Memo_Value_Get(tForm);

                int Id = t.myInt32(Block_Id);

                string SelectBlockValue = Properties_SingleBlockValue_Get_JSON(FullBlockValue, Id);

                tProperties_textEdit_Value_Set(tForm, Id.ToString());

                if (cntrl != null)
                {
                    tProperties_VGrid_Value_Set_JSON(((DevExpress.XtraVerticalGrid.VGridControl)cntrl), SelectBlockValue);
                }

            }
        }

        private void tProperties_listBoxControl_Value_Set_JSON(Form tForm, string NewValue, int itemId)
        {


            string[] controls = new string[] { };
            Control cntrl = t.Find_Control(tForm, "listControl1", "", controls);


            if (cntrl != null)
            {
                Control vcntrl = tProperties_VGrid_Get(tForm);
                string MainFName = ((DevExpress.XtraVerticalGrid.VGridControl)vcntrl).AccessibleDescription.ToString();

                string Caption = Properties_SingleBlockValue_Get_JSON(NewValue, -1);

                string value = string.Empty;
                int Ic = ((DevExpress.XtraEditors.ListBoxControl)cntrl).ItemCount;
                int Id = ((DevExpress.XtraEditors.ListBoxControl)cntrl).SelectedIndex;

                // listeyi guncelle
                if (itemId == 0)
                {
                    for (int i = 0; i < Ic; i++)
                    {
                        value = ((DevExpress.XtraEditors.ListBoxControl)cntrl).Items[i].ToString();

                        //value = ((DevExpress.XtraEditors.ListBoxControl)cntrl).SelectedValue.ToString();
                        if (value.IndexOf(Id.ToString()) > -1)
                        {
                            Caption = " " + i.ToString() + " : " + Caption;
                            ((DevExpress.XtraEditors.ListBoxControl)cntrl).Tag = 1;
                            ((DevExpress.XtraEditors.ListBoxControl)cntrl).Items.RemoveAt(i);
                            ((DevExpress.XtraEditors.ListBoxControl)cntrl).Items.Insert(i, Caption);
                            ((DevExpress.XtraEditors.ListBoxControl)cntrl).SelectedIndex = i; // yeniden seçsin
                            ((DevExpress.XtraEditors.ListBoxControl)cntrl).Tag = 0;
                            break;
                        }
                    }
                }

                if (itemId == -1)
                {
                    Caption = " " + Ic.ToString() + " : " + Caption;
                    ((DevExpress.XtraEditors.ListBoxControl)cntrl).Items.Add(Caption);

                    ((DevExpress.XtraEditors.ListBoxControl)cntrl).SelectedIndex =
                        ((DevExpress.XtraEditors.ListBoxControl)cntrl).Items.Count - 1;
                }
            }
        }

        private void tProperties_VGrid_Value_Set_JSON(VGridControl VGrid, string SelectBlockValue)
        {
            int t1 = VGrid.Rows.Count;
            int t2 = 0;
            string s = string.Empty;
            string fname = string.Empty;
            string tValue = string.Empty;

            VGrid.AccessibleDefaultActionDescription = SelectBlockValue;

            for (int i1 = 0; i1 < t1; i1++)
            {
                // category altındaki edittext sayısı
                t2 = VGrid.Rows[i1].ChildRows.Count;

                for (int i2 = 0; i2 < t2; i2++)
                {
                    fname = VGrid.Rows[i1].ChildRows[i2].Properties.FieldName.ToString();

                    tValue = t.Find_Properties_Value(SelectBlockValue, fname);

                    VGrid.Rows[i1].ChildRows[i2].Properties.Value = tValue;
                }
            }
        }

        public string tPropertiesPacketValue_Preparing(string TableName, string Main_FieldName,
            ref string thisValues, ref string packetHeader, string packetType)
        {


            string paket_turu = "";
            string paket_basi_A = (char)34 + Main_FieldName + (char)34 + ": ["; //"GRID": [
            string paket_basi_B = (char)34 + Main_FieldName + (char)34 + ": {"; //"GRID": {
            string paket_basi_C = (char)34 + Main_FieldName + (char)34 + ": ";  //"GRID": 

            string paket_sonu_A = "]";
            string paket_sonu_B = "}";

            if (t.IsNotNull(thisValues) == false)
            {
                /// boş bir yeni model yapı isteniyor
                thisValues = t.Create_PropertiesEdit_Model_JSON(TableName, Main_FieldName);

                if (packetType == "tPropertiesPlusEdit")
                {
                    thisValues = "[" + v.ENTER + thisValues + v.ENTER + "]";
                    paket_turu = "[]";
                }
            }

            int j1 = thisValues.IndexOf(paket_basi_A);
            if (j1 > -1)
            {
                j1 = j1 + (paket_basi_A.Length - 1); // [ işareti silinmeden
                int j2 = thisValues.IndexOf(paket_sonu_A, j1) + paket_sonu_A.Length;
                thisValues = thisValues.Substring(j1, (j2 - j1)); // ] işareti silinmeden
                thisValues = thisValues.Trim();
                paket_turu = "[]";
                packetHeader = "A";
            }

            /// paket array ise  
            /// 
            if (thisValues == "[]")
            {
                /// boş bir yeni model yapı isteniyor
                thisValues = t.Create_PropertiesEdit_Model_JSON(TableName, Main_FieldName);
                /// bunları eklediğimiz zaman Array durumuna geçiyor
                thisValues = "[" + v.ENTER + thisValues + v.ENTER + "]";
                paket_turu = "[]";
                packetHeader = "A";
            }

            j1 = thisValues.IndexOf(paket_basi_B);
            if (j1 > -1)
            {
                j1 = j1 + (paket_basi_B.Length - 1); // { işareti silinmeden
                int j2 = thisValues.IndexOf(paket_sonu_B, j1) + paket_sonu_B.Length;
                thisValues = thisValues.Substring(j1, (j2 - j1)); // } işareti silinmeden
                thisValues = thisValues.Trim();
                paket_turu = "{}";
                packetHeader = "B";
            }



            /// packet object ise
            /// 
            ////if ((thisValues.IndexOf(paket_basi_B) > -1))
            ////{
            ////    /// boş bir yeni model yapı isteniyor
            ////    thisValues = t.Create_PropertiesEdit_Model_JSON(TableName, Main_FieldName);
            ////    paket_turu = "{}";
            ////    MessageBox.Show("if ((thisValues.IndexOf(paket_basi_B) > -1))");
            ////}

            if ((t.IsNotNull(thisValues)) &&
                (paket_turu == "") &&
                (packetType == "tPropertiesPlusEdit")
                )
            {
                paket_turu = "[]";
            }

            if ((t.IsNotNull(thisValues)) &&
                (paket_turu == "") &&
                (packetType == "tPropertiesEdit")
                )
            {
                paket_turu = "{}";
            }
            return paket_turu;
        }


        //---JSON

        //--- OLD 

        private void tPreperties_SingleBlock_Add(Form tForm, VGridControl VGrid, string prp_type)
        {

            string function_name = "tPreperties SingleBlock Add";
            t.Takipci(function_name, "", '{');

            // PropertiesPlus ta olan textEdit üzerindeki valueyi okuyacak 
            string objno = tProperties_textEdit_Value_Get(tForm);

            tProperties_VGrid_Enabled(tForm, VGrid);

            if (objno == "0") return;

            //VGrid üzerindeki Veriyi Paketle / Blokla

            objno = t.myInt32(objno).ToString();

            string OldBlock = string.Empty;
            string NewBlock = string.Empty;

            tProperties_VGrid_Values_Get(tForm, VGrid, ref OldBlock, ref NewBlock, objno);

            // Paketlenen yeni block u mevcut FullBlocks ile add/update et

            tProperties_FullBlocks_Add(tForm, VGrid, t.myInt32(objno), OldBlock, NewBlock);

            t.Takipci(function_name, "", '}');

        }

        private void tProperties_VGrid_Values_Get(Form tForm, VGridControl VGrid,
                     ref string OldValue, ref string NewValue, string objno)
        {


            string block = string.Empty;
            string fname = string.Empty;
            string value = string.Empty;
            string paket_basi = string.Empty;
            //string paket_sonu = string.Empty;

            string MainFName = VGrid.AccessibleDescription.ToString();

            int t1 = VGrid.Rows.Count;
            int t2 = 0;

            t.MyProperties_Set(ref block, objno, "ROW_" + MainFName, objno);   //"ROW_FieldName:1");

            for (int i1 = 0; i1 < t1; i1++)
            {
                // category altındaki edittext sayısı
                t2 = VGrid.Rows[i1].ChildRows.Count;

                for (int i2 = 0; i2 < t2; i2++)
                {
                    fname = VGrid.Rows[i1].ChildRows[i2].Properties.FieldName.ToString();
                    value = VGrid.Rows[i1].ChildRows[i2].Properties.Value.ToString();

                    paket_basi = fname + "={";
                    //paket_sonu = fname + "=}";

                    if (value.IndexOf(paket_basi) > -1)
                    {
                        block = block +
                            objno + "=" + fname + ":" + value + ";" + v.ENTER;

                        /*
                        1=ROW_MainFName:1;
                        1=CAPTION:aaaaaa1222;
                        1=BUTTONTYPE:null;
                        1=TABLEIPCODE_LIST:TABLEIPCODE_LIST={
                        1=ROW:1;
                        1=CAPTION:qqq;
                        1=ROWE:1;
                        TABLEIPCODE_LIST=}
                        1=FORMNAME:null;
                        1=FORMCODE:null;
                        1=FORMTYPE:null;
                        1=FORMSTATE:null;
                        1=ROWE_MainFName:1;
                        */
                    }
                    else
                    {
                        t.MyProperties_Set(ref block, objno, fname, value);
                    }
                }
            }

            t.MyProperties_Set(ref block, objno, "ROWE_" + MainFName, objno);   //"ROWE_FieldName"

            NewValue = block.Trim();

            if (VGrid.AccessibleDefaultActionDescription != null)
                OldValue = VGrid.AccessibleDefaultActionDescription.ToString();
            else OldValue = "";

            VGrid.AccessibleDefaultActionDescription = NewValue;

            /* örnek            
            1=ROW:1;
            1=CAPTION:null;
            1=BUTTONTYPE:null;
            1=TABLEIPCODE_LIST:null;
            1=FORMNAME:null;
            1=FORMCODE:null;
            1=FORMTYPE:null;
            1=FORMSTATE:null;
            1=ROWE:1;
            */
        }

        private void tProperties_FullBlocks_Add(Form tForm, VGridControl VGrid, int Block_ID, string OldBlock, string NewBlock)
        {


            string OldFullBlockValues = tProperties_Memo_Value_Get(tForm);

            //=PROP_NAVIGATOR:{;
            string MainFName = VGrid.AccessibleDescription.ToString();
            string s = "=ROW_" + MainFName + ":" + Block_ID.ToString();

            if (OldFullBlockValues.IndexOf(s) > -1) // varsa
            {
                // Eskiyi block paketini iptal et yerine yeni paketle değiştir

                NewBlock = NewBlock.Trim();

                // ------------------------------------------------------------------------
                // DİKKAT : CAPTION ismi kullanırken 1234567890 gibi rakamlar kullan MA !!!
                // ------------------------------------------------------------------------

                t.Str_Replace(ref OldFullBlockValues, OldBlock, NewBlock);
            }
            else
            {
                // Mevcuda yeni block ekle
                if (OldFullBlockValues == "")
                    OldFullBlockValues = OldFullBlockValues.Trim() + NewBlock.Trim();
                else OldFullBlockValues = OldFullBlockValues.Trim() + v.ENTER + NewBlock.Trim();

            }

            // Memoya set et
            tProperties_Memo_Value_Set(tForm, OldFullBlockValues);

            // Listeye Captionunu ekle
            tProperties_listBoxControl_Value_Set(tForm, NewBlock);

            tForm.AccessibleDefaultActionDescription = OldFullBlockValues;

        }

        //---OLD



        private Control tProperties_VGrid_Get(Form tForm)
        {


            // properties VGrid tespit et
            string[] controls = new string[] { "DevExpress.XtraVerticalGrid.VGridControl" };
            Control cntrl = t.Find_Control(tForm, "", "tProperties", controls);

            return cntrl;
        }

        // old value vgridin üzerine set ediliyor
        private void tProperties_VGrid_Value_Set(VGridControl VGrid, string singleBlockValue)
        {
            //******************************
            //v.con_Value_Old = singleBlockValue;
            //******************************


            string function_name = "t_Properties_VGrid_Value_Set";
            t.Takipci(function_name, "", '{');

            int t1 = VGrid.Rows.Count;
            int t2 = 0;
            int j1, j2 = 0;
            string s = string.Empty;
            string fname = string.Empty;
            string tValue = string.Empty;

            string paket_basi = string.Empty;
            string paket_sonu = string.Empty;

            VGrid.AccessibleDefaultActionDescription = singleBlockValue;

            for (int i1 = 0; i1 < t1; i1++)
            {
                // category altındaki edittext sayısı
                t2 = VGrid.Rows[i1].ChildRows.Count;

                for (int i2 = 0; i2 < t2; i2++)
                {
                    s = singleBlockValue;

                    fname = VGrid.Rows[i1].ChildRows[i2].Properties.FieldName.ToString();

                    paket_basi = fname + "={" + v.ENTER;
                    paket_sonu = fname + "=}";

                    if (s.IndexOf(paket_basi) > -1)
                    {
                        j1 = s.IndexOf(paket_basi);
                        j2 = ((s.IndexOf(paket_sonu)) + paket_sonu.Length) - j1;
                        tValue = s.Substring(j1, j2);
                    }
                    else
                    {
                        tValue = t.MyProperties_Get(s, fname + ":");
                    }

                    tValue = tValue.Trim();

                    VGrid.Rows[i1].ChildRows[i2].Properties.Value = tValue;
                }
            }

            t.Takipci(function_name, "", '}');
        }

        private string tProperties_Memo_Value_Get(Form tForm)
        {


            string OldValue = string.Empty;
            string[] controls = new string[] { };
            Control cntrl = t.Find_Control(tForm, "memoEdit_PropertiesValue", "", controls);

            if (cntrl != null)
            {
                if (((DevExpress.XtraEditors.MemoEdit)cntrl).EditValue != null)
                {
                    OldValue = ((DevExpress.XtraEditors.MemoEdit)cntrl).EditValue.ToString();
                }
            }

            return OldValue;
        }

        private void tProperties_Memo_Value_Set(Form tForm, string NewValue)
        {


            string[] controls = new string[] { };
            Control cntrl = t.Find_Control(tForm, "memoEdit_PropertiesValue", "", controls);

            if (cntrl != null)
            {
                if (((DevExpress.XtraEditors.MemoEdit)cntrl).EditValue != null)
                {
                    ((DevExpress.XtraEditors.MemoEdit)cntrl).EditValue = NewValue;
                }
            }
        }

        private string tProperties_textEdit_Value_Get(Form tForm)
        {


            string OldValue = string.Empty;
            string[] controls = new string[] { };
            Control cntrl = t.Find_Control(tForm, "textEdit_ViewID", "", controls);

            if (cntrl != null)
            {
                if (((DevExpress.XtraEditors.TextEdit)cntrl).EditValue != null)
                {
                    OldValue = ((DevExpress.XtraEditors.TextEdit)cntrl).EditValue.ToString();
                }
            }

            return OldValue;
        }

        private void tProperties_textEdit_Value_Set(Form tForm, string NewValue)
        {


            string[] controls = new string[] { };
            Control cntrl = t.Find_Control(tForm, "textEdit_ViewID", "", controls);

            if (cntrl != null)
            {
                ((DevExpress.XtraEditors.TextEdit)cntrl).EditValue = NewValue;
            }
        }

        private void tProperties_listBoxControl_Value_Set(Form tForm, string NewValue)
        {


            string[] controls = new string[] { };
            Control cntrl = t.Find_Control(tForm, "listControl1", "", controls);
            int Id = 0;

            if (cntrl != null)
            {
                Control vcntrl = tProperties_VGrid_Get(tForm);
                string MainFName = ((DevExpress.XtraVerticalGrid.VGridControl)vcntrl).AccessibleDescription.ToString();

                // SingleBlock son hali
                string Caption = Properties_for_List_Caption(MainFName, NewValue, ref Id);

                bool onay = false;
                string value = string.Empty;
                int j = ((DevExpress.XtraEditors.ListBoxControl)cntrl).ItemCount;

                for (int i = 0; i < j; i++)
                {
                    value = ((DevExpress.XtraEditors.ListBoxControl)cntrl).Items[i].ToString();
                    //SelectedIndex = i;

                    //value = ((DevExpress.XtraEditors.ListBoxControl)cntrl).SelectedValue.ToString();
                    if (value.IndexOf(Id.ToString()) > -1)
                    {
                        ((DevExpress.XtraEditors.ListBoxControl)cntrl).Tag = 1;
                        ((DevExpress.XtraEditors.ListBoxControl)cntrl).Items.RemoveAt(i);
                        ((DevExpress.XtraEditors.ListBoxControl)cntrl).Items.Insert(i, Caption);
                        ((DevExpress.XtraEditors.ListBoxControl)cntrl).SelectedIndex = i; // yeniden seçsin
                        ((DevExpress.XtraEditors.ListBoxControl)cntrl).Tag = 0;
                        onay = true;
                        break;
                    }
                }

                if (onay == false)
                {
                    ((DevExpress.XtraEditors.ListBoxControl)cntrl).Items.Add(Caption);

                    ((DevExpress.XtraEditors.ListBoxControl)cntrl).SelectedIndex =
                        ((DevExpress.XtraEditors.ListBoxControl)cntrl).Items.Count - 1;
                }
            }
        }

        private void tProperties_listBoxControl_Value_Remove(Form tForm, string OldValue)
        {


            string[] controls = new string[] { };
            Control cntrl = t.Find_Control(tForm, "listControl1", "", controls);
            int Id = 0;

            if (cntrl != null)
            {
                Control vcntrl = tProperties_VGrid_Get(tForm);
                string MainFName = ((DevExpress.XtraVerticalGrid.VGridControl)vcntrl).AccessibleDescription.ToString();

                // SingleBlock son hali
                string Caption = Properties_for_List_Caption(MainFName, OldValue, ref Id);

                string value = string.Empty;
                int j = ((DevExpress.XtraEditors.ListBoxControl)cntrl).ItemCount;

                for (int i = 0; i < j; i++)
                {
                    ((DevExpress.XtraEditors.ListBoxControl)cntrl).SelectedIndex = i;

                    value = ((DevExpress.XtraEditors.ListBoxControl)cntrl).SelectedValue.ToString();

                    if (value.IndexOf(Id.ToString()) > -1)
                    {
                        ((DevExpress.XtraEditors.ListBoxControl)cntrl).Tag = 1;
                        ((DevExpress.XtraEditors.ListBoxControl)cntrl).Items.RemoveAt(i);
                        ((DevExpress.XtraEditors.ListBoxControl)cntrl).Tag = 0;

                        if (((DevExpress.XtraEditors.ListBoxControl)cntrl).ItemCount > 0)
                        {
                            tProperties_listBoxControlSelectedChanged(
                                ((DevExpress.XtraEditors.ListBoxControl)cntrl).SelectedValue.ToString(),
                                ((DevExpress.XtraEditors.ListBoxControl)cntrl).AccessibleName.ToString());
                        }

                        //((DevExpress.XtraEditors.ListBoxControl)cntrl).SelectedIndex = 0; // yeniden seçsin

                        break;
                    }
                }
            }
        }

        public void tProperties_listBoxControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((DevExpress.XtraEditors.ListBoxControl)sender).Tag != null)
                if (((DevExpress.XtraEditors.ListBoxControl)sender).Tag.ToString() == "1") return;

            string line_value = ((DevExpress.XtraEditors.ListBoxControl)sender).SelectedValue.ToString();
            string prp_type = ((DevExpress.XtraEditors.ListBoxControl)sender).AccessibleName;
            string MainFName = ((DevExpress.XtraEditors.ListBoxControl)sender).AccessibleDescription;

            tProperties_listBoxControlSelectedChanged(line_value, MainFName);
        }

        private void tProperties_listBoxControlSelectedChanged(string line_value, string MainFName)
        {


            //t.Find_Form(sender); listboxtan bulamadı

            //Form tForm = Application.OpenForms["tForm_" + prp_type];
            Form tForm = Application.OpenForms["tForm_" + MainFName];

            if (tForm != null)
            {
                Control cntrl = tProperties_VGrid_Get(tForm);
                //string MainFName = ((DevExpress.XtraVerticalGrid.VGridControl)cntrl).AccessibleDescription.ToString();
                string Block_Id = t.Get_And_Clear(ref line_value, ":");
                string FullBlockValue = tProperties_Memo_Value_Get(tForm);

                int Id = t.myInt32(Block_Id);

                string BlockValue = Properties_SingleBlockValue_Get(MainFName, FullBlockValue, Id);

                tProperties_textEdit_Value_Set(tForm, Id.ToString());

                if (cntrl != null)
                {
                    tProperties_VGrid_Value_Set(((DevExpress.XtraVerticalGrid.VGridControl)cntrl), BlockValue);
                }
            }
        }

        private int tProperties_New_Block_Id(string MainFName, string OldValue)
        {

            int j = 0;

            v.con_Value_Max = 0;
            string block = string.Empty;
            string bname = string.Empty;

            while (OldValue.IndexOf("ROWE_" + MainFName) > -1)
            {
                block = t.Get_And_Clear(ref OldValue, "ROWE_" + MainFName);

                bname = t.MyProperties_Get(block, "ROW_" + MainFName + ":");

                j = t.myMaxValue(bname);
            }
            // j ile en büyük değer geliyor ve 1 arttırılıyor
            j++;
            return j;
        }

        #region PropertiesPlus SubFunctions

        public string Properties_SingleBlockValue_Get_JSON(string FullBlocks, int BlockID)
        {
            // old - bir şeklide eski yapı gelirse 
            if (FullBlocks.IndexOf("={") > -1)
            {
                return "";
            }

            /// change " çift tırnağı  ' tek tırnak ile değiştir
            ///
            FullBlocks = FullBlocks.Replace((char)34, (char)39);
            //FullBlocks = FullBlocks.Replace((char)39, (char)134 + (char)39);
            //FullBlocks = FullBlocks.Replace("\"", "\\'");

            var myList = JsonConvert.DeserializeObject(FullBlocks);

            int i = 0;
            string value = string.Empty;

            // Type = Object  or  Type = Array
            var x = myList.GetType();
            if (x.Name == "JArray")
            {
                foreach (var item in (Newtonsoft.Json.Linq.JArray)myList)
                {
                    if (i == BlockID)
                    {
                        value = value + (Object)item.ToString();
                        break;
                    }

                    /// (BlockID == -1) ise
                    /// ilk nesnenin valuesi alınacak
                    /// bu genelde CAPTION value oluyor
                    if (BlockID == -1)
                    {
                        foreach (var item2 in (Newtonsoft.Json.Linq.JObject)item)
                        {
                            value = item2.Value.ToString();
                            break;
                        }
                    }

                    i++;
                }
            }

            if (x.Name == "JObject")
            {
                value = ((Newtonsoft.Json.Linq.JObject)myList).ToString();
                ///value = "[" + v.ENTER + value + v.ENTER + "]";
                ///

                /// ilk nesnenin valuesi alınacak
                /// bu genelde CAPTION value oluyor
                if (BlockID == -1)
                {
                    foreach (var item in (Newtonsoft.Json.Linq.JObject)myList)
                    {
                        value = item.Value.ToString();
                        break;
                    }
                }
            }

            return value;
        }

        public string Properties_SingleBlockValue_Get(string MainFName, string FullBlocks, int BlockID)
        {


            string s = FullBlocks;
            string s2 = string.Empty;
            string s3 = string.Empty;
            string block = string.Empty;
            bool onay = false;

            while (s.IndexOf("=ROWE_" + MainFName + ":") > -1)
            {
                block = t.BeforeGet_And_AfterClear(ref s, "=ROWE_" + MainFName + ":", true);

                if (block.IndexOf("ROW_" + MainFName + ":" + BlockID.ToString()) > -1)
                {
                    s2 = BlockID.ToString() + "=ROW_" + MainFName + ":" + BlockID.ToString();

                    s3 = t.AfterGet_And_BeforeClear(ref block, s2, false);

                    block = s3 + BlockID.ToString() + ";";
                    onay = true;
                    break;
                }
            }

            if (onay == false)
            {
                // eğer bulamaz ise ilk bloğu gönder
                s = FullBlocks;
                block = t.BeforeGet_And_AfterClear(ref s, "=ROWE_" + MainFName + ":", true);
            }

            return block = block.Trim();
        }

        public string Properties_for_List_Caption(string MainFName, string Single_Block_Value, ref int Id)
        {

            string bname = t.MyProperties_Get(Single_Block_Value, "ROW_" + MainFName + ":");
            string about = t.MyProperties_Get(Single_Block_Value, "CAPTION:");
            string capiton = "  " + bname.PadRight(3) + ": " + about.PadRight(50);
            Id = t.myInt32(bname);
            return capiton;
        }

        // Maximum Id
        public int Properies_Max_Block_Id(string MainFName, string OldBlockValues)
        {


            string block = string.Empty;
            string bname = string.Empty;
            int Id = 0;

            while (OldBlockValues.IndexOf("=ROWE_" + MainFName + ":") > -1)
            {
                block = t.Get_And_Clear(ref OldBlockValues, "=ROWE_" + MainFName + ":");
                bname = t.MyProperties_Get(block, "ROW_" + MainFName + ":");
                Id = t.myMaxValue(bname);
            }

            return Id++;
        }

        // Minumum Id
        public int Properies_Min_Block_Id(string MainFName, string OldBlockValues)
        {


            string block = string.Empty;
            string bname = string.Empty;
            int Id = 0;

            while (OldBlockValues.IndexOf("=ROWE_" + MainFName + ":") > -1)
            {
                block = t.Get_And_Clear(ref OldBlockValues, "=ROWE_" + MainFName + ":");
                bname = t.MyProperties_Get(block, "ROW_" + MainFName + ":");
                Id = t.myMinValue(bname);
            }

            //if (Id == 0) Id++;

            return Id;
        }

        #endregion PropertiesPlus SubFunctions

        #endregion Sub tProperties Functions

        //------------------

        #endregion tProperties Buttons Click << 

        #region Prop_RunTime

        public void Prop_RunTimeClick(Form tForm, DataSet dsData, string TableIPCode, v.tButtonType buttonType, v.tBeforeAfter beforeAfter)
        {
            Control cntrl = null;
            cntrl = t.Find_Control_View(tForm, TableIPCode);

            if (cntrl != null)
            {
                if (cntrl.AccessibleDescription != null)
                {
                    string Prop_RunTime = cntrl.AccessibleDescription.ToString();

                    //string s1 = "=ROW_PROP_RUNTIME:";
                    string s2 = (char)34 + "AUTO_LST" + (char)34 + ": [";
                    /// "AUTO_LST": [
                    if (Prop_RunTime.IndexOf(s2) > -1)
                    {
                        // bunlar AUTO_REFRESH_IP den önceki işler
                        if (beforeAfter == v.tBeforeAfter.Before)
                        {
                            if (buttonType == v.tButtonType.btAutoInsert)
                                Prop_RunTime_Work_AUTO_INSERT(tForm, dsData, Prop_RunTime);


                            if ((buttonType == v.tButtonType.btKaydet) ||
                                (buttonType == v.tButtonType.btSilSatir))
                                Prop_RunTime_Work_AUTO_LIST(tForm, Prop_RunTime, buttonType);
                        }

                        // AUTO_REFRESH_IP bu özellik yeni eklendi
                        // mevcut dataset ten sonra refresh olması istenen başka bir TableIPCode için
                        if (beforeAfter == v.tBeforeAfter.After)
                        {
                            if ((buttonType == v.tButtonType.btListele) ||
                                (buttonType == v.tButtonType.btListeyeEkle ) ||
                                (buttonType == v.tButtonType.btSilListe) ||
                                (buttonType == v.tButtonType.btSilSatir) ||
                                (buttonType == v.tButtonType.btSilBelge) ||
                                (buttonType == v.tButtonType.btSilHesap)
                                )
                            {
                                Prop_RunTime_Work_AUTO_REFRESH(tForm, Prop_RunTime);
                            }
                        }
                    }
                }
            }
        }

        public void Prop_RunTime_Work_AUTO_LIST(Form tForm, string Prop_RunTime, v.tButtonType buttonType)//byte Button_Type)
        {
            string RunTime = string.Empty;
            string Navigator = string.Empty;
            t.String_Parcala(Prop_RunTime, ref RunTime, ref Navigator, "|ds|");

            PROP_RUNTIME prop_ = t.readProp<PROP_RUNTIME>(RunTime);

            string TABLEIPCODE = string.Empty;

            #region AUTO_LST

            foreach (var item in prop_.AUTO_LST)
            {
                /// v.nv_22_Kaydet 
                /// v.nv_26_Sil_Satir
                /// 
                //MessageBox.Show("Alo : ButtonType kontrol etmen gerekiyor");

                if (item.BUTTONTYPE.ToString() == System.Convert.ToString((byte)buttonType)) //Button_Type.ToString())
                {
                    foreach (var item2 in item.TABLEIPCODE_LIST2)
                    {
                        TABLEIPCODE = item2.TABLEIPCODE.ToString();

                        if (t.IsNotNull(TABLEIPCODE))
                        {
                            DataSet ds = null;
                            DataNavigator dN = null;
                            t.Find_DataSet(tForm, ref ds, ref dN, TABLEIPCODE);
                            if (ds != null)
                            {
                                t.TableRefresh(tForm, ds);
                            }
                        }
                    }
                }
            }
            #endregion AUTO_LST

        }
        
        private void Prop_RunTime_Work_AUTO_REFRESH(Form tForm, string Prop_RunTime)
        {
            /// Kendisi kayıt olduktan sonra başka TableIPCode nin refresh olması sağlanıyor
            ///
            string RunTime = string.Empty;
            string Navigator = string.Empty;
            t.String_Parcala(Prop_RunTime, ref RunTime, ref Navigator, "|ds|");

            PROP_RUNTIME prop_ = t.readProp<PROP_RUNTIME>(RunTime);

            string tableIPCode = string.Empty;

            if (prop_ != null)
            {
                tableIPCode = prop_.AUTO_REFRESH_IP;
                Prop_RunTime_Work_AUTO_REFRESH_(tForm, tableIPCode);
            }
            if (prop_.AUTO_REFRESH_IP2 != null)
            {
                tableIPCode = prop_.AUTO_REFRESH_IP2;
                Prop_RunTime_Work_AUTO_REFRESH_(tForm, tableIPCode);
            }
            if (prop_.AUTO_REFRESH_IP3 != null)
            {
                tableIPCode = prop_.AUTO_REFRESH_IP3;
                Prop_RunTime_Work_AUTO_REFRESH_(tForm, tableIPCode);
            }
        }

        private void Prop_RunTime_Work_AUTO_REFRESH_(Form tForm, string tableIPCode)
        {
            if (t.IsNotNull(tableIPCode))
            {
                DataSet ds = null;
                DataNavigator dN = null;
                t.Find_DataSet(tForm, ref ds, ref dN, tableIPCode);
                if (ds != null)
                {
                    // buraya position nerede olacak seçeneği ekle
                    // position first
                    // position normal
                    // position last

                    // mevcut normale göre çalışıyor 
                    v.con_PositionChange = true;
                    int pos = dN.Position;
                    t.TableRefresh(tForm, ds);
                    dN.Position = pos;
                    //dN.Position = 0;
                    v.con_PositionChange = false;
                }
            }
        }

        private void Prop_RunTime_Work_AUTO_INSERT(Form tForm, DataSet dsData, string Prop_RunTime)
        {
            /// Kendisinden önce kayıt oluşturması gereken bir TableIPCode var ise o çalışacak
            /// ??= Yani fiş satırının ilk kaydı oluşacaksa kendinden önce fiş başlığının 
            ///     kaydı oluşması gerekir. okey

            tEventsButton evb = new tEventsButton();

            string RunTime = string.Empty;
            string Navigator = string.Empty;
            t.String_Parcala(Prop_RunTime, ref RunTime, ref Navigator, "|ds|");
            
            PROP_RUNTIME prop_ = t.readProp<PROP_RUNTIME>(RunTime);

            string Header_TableIPCode = string.Empty;

            Header_TableIPCode = prop_.AUTO_INS_MST_IP.ToString();

            if (t.IsNotNull(Header_TableIPCode))
            {
                if (dsData != null)
                {
                    if (dsData.Tables[0].Rows.Count == 1)
                    {
                        string myProp = dsData.Namespace;
                        string KeyFName = t.MyProperties_Get(myProp, "KeyFName:");

                        if (dsData.Tables[0].Rows[0][KeyFName].ToString() == "")
                        {
                            DataSet dsDataHeader = t.Find_DataSet(tForm, "", Header_TableIPCode, "");

                            if (dsDataHeader != null)
                            {
                                if (dsDataHeader.Tables[0].Rows.Count == 0)
                                {
                                    evb.newData(tForm, Header_TableIPCode);
                                }

                                tSave sv = new tSave();
                                sv.tDataSave(tForm, Header_TableIPCode);
                                //t.ButtonEnabledAll(tForm, Header_TableIPCode, true);
                            }
                        }
                    }
                }
            }
        }

        #endregion Prop_RunTime

        #region Kriter Buttons Click >>

        public void btn_KriterListele_Click(object sender, EventArgs e)
        {

            string function_name = "btn_KriterListe_Click";
            t.Takipci(function_name, "", '{');

            #region Tanımlar

            string TableIPCode = string.Empty;
            string VGridName = string.Empty;

            // tüm bu olayların oluştuğu form tespit ediliyor
            Form tForm = t.Find_Form(sender);

            string tbutton = ((DevExpress.XtraEditors.SimpleButton)sender).Text;
            string Where_Add = ((DevExpress.XtraEditors.SimpleButton)sender).AccessibleDefaultActionDescription;

            //WaitForm WaitForm2 = new WaitForm();
            //SplashScreenManager.ShowForm(tForm, typeof(WaitForm2), true, true, false);

            // nesneler arasında bağlantıyı sağlayacak olan Table Kodu ve IP Kodu tespit ediliyor
            TableIPCode = ((DevExpress.XtraEditors.SimpleButton)sender).AccessibleName;

            // butonun AccessibleDescription üzerinde kendisinin bağımlı olduğu VGrid in adı mevcut
            // bu button create edilirken buraya yazılmıştı.
            VGridName = ((DevExpress.XtraEditors.SimpleButton)sender).AccessibleDescription;
            // VgridControl tespit ediliyor
            DevExpress.XtraVerticalGrid.VGridControl VGrid = t.Find_VGridControl(tForm, VGridName, "");

            string Prop_RunTime = VGrid.AccessibleDescription;

            #endregion Tanımlar

            #region Listele
            if (tbutton == "&Listele")
            {
                DataSet dsData = t.Find_DataSet(tForm, "", TableIPCode, function_name);
                if (dsData != null)
                {
                    Control cntrl = new Control();
                    cntrl = t.Find_Control_View(tForm, TableIPCode);

                    VGrid.FocusPrev();
                    Kriterleri_Uygula(tForm, dsData, VGrid, Where_Add, cntrl);

                    #region diğer işlemler
                    if (v.con_GotoRecord == "ON")
                    {
                        t.tGotoRecord(tForm, dsData, v.con_TableIPCode, v.con_GotoRecord_FName, v.con_GotoRecord_Value, -1);
                    }

                    if (v.con_SearchValue != "")
                    {
                        if (cntrl != null)
                        {
                            if (cntrl.GetType().ToString() == "DevExpress.XtraGrid.GridControl")
                            {
                                // unutma TKN 

                                //GridView tView = ((DevExpress.XtraGrid.GridControl)cntrl).MainView as GridView;
                                //tView.ApplyFindFilter(sp.con_SearchValue);
                                //tView
                            }
                        }
                    }
                    #endregion diğer işlemler

                    #region RunTime / Paralel TableIPCode

                    if (t.IsNotNull(Prop_RunTime))
                        Prop_RunTime_PRL_KRT(tForm, VGrid, Prop_RunTime, Where_Add);

                    #endregion RunTime

                }
                else
                {
                    MessageBox.Show("DİKKAT : Kriterlerin uygulanacağı DataControl tespit edilemedi ...");
                }

            }
            #endregion Listele

            #region Temizle
            if (tbutton == "&Temizle")
            {
                VGrid.FocusPrev();
                Kriterleri_Temizle(VGrid, TableIPCode);
            }
            #endregion Temizle

            #region RowCollapse
            if (tbutton == "Kpt")
            {
                VGrid.CollapseAllRows();
            }
            #endregion RowCollapse

            #region RowExpand
            if (tbutton == "Aç")
            {
                VGrid.ExpandAllRows();
            }
            #endregion RowExpand

            //Close Wait Form
            //SplashScreenManager.CloseForm(false);

            t.Takipci(function_name, "", '}');
        }

        private void Prop_RunTime_PRL_KRT(Form tForm, DevExpress.XtraVerticalGrid.VGridControl VGrid, string Prop_RunTime, string Where_Add)
        {
            string s1 = "=ROW_PROP_RUNTIME:";
            string s2 = (char)34 + "PRL_KRT" + (char)34 + ": [";
            // "PRL_KRT": [

            if (Prop_RunTime.IndexOf(s1) > -1)
                Prl_Krt(tForm, VGrid, Prop_RunTime, Where_Add);

            if (Prop_RunTime.IndexOf(s2) > -1)
                Prl_Krt_JSON(tForm, VGrid, Prop_RunTime, Where_Add);
        }

        private void Prl_Krt_JSON(Form tForm, DevExpress.XtraVerticalGrid.VGridControl VGrid, string Prop_RunTime, string Where_Add)
        {


            string RunTime = string.Empty;
            string Navigator = string.Empty;
            t.String_Parcala(Prop_RunTime, ref RunTime, ref Navigator, "|ds|");
            /*
            PROP_RUNTIME packet = new PROP_RUNTIME();
            RunTime = RunTime.Replace((char)34, (char)39);
            //var prop_ = JsonConvert.DeserializeAnonymousType(RunTime, packet);
            */
            PROP_RUNTIME prop_ = t.readProp<PROP_RUNTIME>(RunTime);

            DataSet dsData_Prl = null;
            string Prl_TableIPCode = string.Empty;
            Control cntrl = new Control();

            foreach (var item in prop_.PRL_KRT)
            {
                Prl_TableIPCode = item.TABLEIPCODE.ToString();
                if (t.IsNotNull(Prl_TableIPCode))
                {
                    cntrl = t.Find_Control_View(tForm, Prl_TableIPCode);
                    dsData_Prl = t.Find_DataSet(tForm, "", Prl_TableIPCode, "");
                    if (dsData_Prl != null)
                    {
                        VGrid.FocusPrev();
                        Kriterleri_Uygula(tForm, dsData_Prl, VGrid, Where_Add, cntrl);
                    }
                }
            }

            #region örnek kod
            /*
            "PRL_KRT": [
            {
              "CAPTION": "Finans Bakiye",
              "TABLEIPCODE": "FBKY.FBKY_01"
            }
            ],
            */
            #endregion örnek kod
        }

        private void Prl_Krt(Form tForm, DevExpress.XtraVerticalGrid.VGridControl VGrid, string Prop_RunTime, string Where_Add)
        {


            if (t.IsData(ref Prop_RunTime, "PRL_KRT"))
            {
                string lockE = "=ROWE_";
                string row_block = string.Empty;

                DataSet dsData_Prl = null;
                string Prl_TableIPCode = string.Empty;
                Control cntrl = new Control();

                while (Prop_RunTime.IndexOf(lockE) > -1)
                {
                    row_block = t.Find_Properies_Get_RowBlock(ref Prop_RunTime, "PRL_KRT");

                    Prl_TableIPCode = t.MyProperties_Get(row_block, "TABLEIPCODE:");
                    if (t.IsNotNull(Prl_TableIPCode))
                    {
                        cntrl = t.Find_Control_View(tForm, Prl_TableIPCode);
                        dsData_Prl = t.Find_DataSet(tForm, "", Prl_TableIPCode, "");
                        if (dsData_Prl != null)
                        {
                            VGrid.FocusPrev();
                            Kriterleri_Uygula(tForm, dsData_Prl, VGrid, Where_Add, cntrl);
                        }
                    }
                }

                #region örnek kod
                /*
                1=ROW_PRL_KRT:1;
                1=CAPTION:Finans Bakiye;
                1=TABLEIPCODE:FBKY.FBKY_01;
                1=ROWE_PRL_KRT:1;
                */
                #endregion örnek kod
            }
        }

        public void External_Kriterleri_Uygula(Form tForm, DataSet dsData, string alias, string Where_Add, Control cntrl)
        {

            string function_name = "External Kriterleri_Uygula";
            t.Takipci(function_name, "", '{');

            #region Tanımlar

            // ŞİMDİLİK BUNU TEMİZLİĞİ External_Kriterleri_Uygula yı çağırdığın yerde yap
            // daha sonra /*xxxxKRITERLER*/ arasını temizleyerek yeni kriterleri at
            // böylece temizleğe gerek kalmaz her yeni kriter gerektiğinde yenisi çaılışır

            //t.tSqlSecond_Clear(ref dsData);
            // -----


            string myProp = dsData.Namespace.ToString();

            if (t.IsNotNull(alias) == false)
                alias = t.MyProperties_Get(myProp, "=TableLabel:");

            t.Alias_Control(ref alias);

            string aSQL = t.MyProperties_Get(myProp, "=SqlFirst:");
            aSQL = t.kisitlamalarClear(aSQL);

            int fKRITERLER = aSQL.IndexOf("/*" + alias + "KRITERLER*/") + 14 + alias.Length;

            #endregion Tanımlar

            #region SQL'i çalıştır

            if ((fKRITERLER > 15) && t.IsNotNull(Where_Add))
            {
                aSQL = aSQL.Insert(fKRITERLER, Where_Add);

                t.Data_Read_Execute(tForm, dsData, ref aSQL, "", cntrl);
            }
            #endregion SQL'i çalıştır

            t.Takipci(function_name, "", '}');
        }

        private void Kriterleri_Uygula(Form tForm, DataSet dsData, VGridControl VGrid, string Where_Add, Control cntrl)
        {

            string function_name = "Kriterleri_Uygula";
            t.Takipci(function_name, "", '{');

            #region Tanımlar
            //t.tSqlSecond_Clear(ref dsData);
            string myProp = dsData.Namespace.ToString();
            string alias = t.MyProperties_Get(myProp, "=TableLabel:");
            
            //string aSQL = t.MyProperties_Get(myProp, "=SqlFirst:");
            string SqlFirst = t.MyProperties_Get(myProp, "SqlFirst:");
            string SqlSecond = t.MyProperties_Get(myProp, "SqlSecond:");
            string aSQL = t.Set(SqlSecond, SqlFirst, "");

            string fullname = string.Empty;
            string fname = string.Empty;
            string fvalue = string.Empty;
            string fvalue2 = string.Empty;
            string tmpkriter = string.Empty;
            string fkriter = v.ENTER;
            string foperand = string.Empty;
            string foperand_id = string.Empty;
            string ttable_alias = string.Empty;
            string tkrt_alias = string.Empty;
            int t1 = VGrid.Rows.Count;
            int t2 = 0;
            int ffieldtype = 0;
            //int fKRITERLER = 0;
            aSQL = t.kisitlamalarClear(aSQL);

            string softCode = "";
            string projectCode = "";

            #endregion Tanımlar

            #region Vgrid row döngüsü
            // Vgrid üzerindeki category sayısı
            for (int i1 = 0; i1 < t1; i1++)
            {
                // category altındaki edittext sayısı
                t2 = VGrid.Rows[i1].ChildRows.Count;

                // t2 = 4 ise sorgu tipi even  (başlangıç, bitiş)
                // t2 = 2 ise sorgu tipi odd   (tek sorgu)

                #region categori altı
                for (int i2 = 0; i2 < t2; i2++)
                {
                    fullname = VGrid.Rows[i1].ChildRows[i2].Properties.FieldName.ToString();

                    t.TableIPCode_Get(fullname, ref softCode, ref projectCode, ref ttable_alias, ref fname);

                    tkrt_alias = VGrid.Rows[i1].ChildRows[i2].Properties.CustomizationCaption.ToString();

                    if (t.IsNotNull(tkrt_alias))
                    {
                        //if (tkrt_alias.IndexOf(".") < 0)
                        //   fname = tkrt_alias + "." + fname;
                        //else fname = tkrt_alias + fname;

                        if (fullname.IndexOf(".") < 0)
                        {
                            fname = tkrt_alias + fname;
                        }
                    }

                    fvalue = string.Empty;
                    fvalue2 = string.Empty;
                    foperand = string.Empty;
                    foperand_id = string.Empty;
                    ffieldtype = 0;

                    #region // fieldin tipi nedir, tespiti

                    if (VGrid.Rows[i1].ChildRows[i2].Tag != null)
                        ffieldtype = System.Convert.ToInt32(VGrid.Rows[i1].ChildRows[i2].Tag);

                    // field tipine göre veri hazırlnacak 
                    if (VGrid.Rows[i1].ChildRows[i2].Properties.Value != null)
                    {
                        fvalue = VGrid.Rows[i1].ChildRows[i2].Properties.Value.ToString();

                        #region // Date or SmallDatetime
                        //if ((ffieldtype == 40) || (ffieldtype == 58)) 
                        //{
                        //    if ((i2 == 1) && (t2 == 4)) // even ise
                        //    {
                        //        //fvalue = fvalue.Substring(0, 12) + " 59:00:00'";
                        //        if (t.IsNotNull(fvalue))
                        //        {
                        //            // TARIHI 1 GUN ARTTIRARAK SAAT 00:00:00 SİKİNTİSİ

                        //            //DateTime x = Convert.ToDateTime(fvalue);
                        //            //x = x.AddDays(1);
                        //            //fvalue = Convert.ToString(x);
                        //        }
                        //    }
                        //}
                        #endregion

                        fvalue = t.tData_Convert(fvalue, ffieldtype);

                        // bu ikinci veriye Like lar yüzünden ihtiyaç duyuldu.
                        fvalue2 = t.Str_Check(VGrid.Rows[i1].ChildRows[i2].Properties.Value.ToString());
                    }
                    #endregion

                    #region // Vgridin satırı sorgu çeşitleri değilse ve elle girilmiş veri var  ise

                    if ((fname.IndexOf("bas_sorgu_") == -1) &&
                        (fname.IndexOf("bit_sorgu_") == -1) &&
                        (fname.IndexOf("null") == -1) &&
                        ((t.IsNotNull(fvalue)) || (t.IsNotNull(fvalue2)))
                        )
                    {
                        // operand tipi belirlenecek
                        if (t2 == 4) // even ise (çift)
                        {
                            if (VGrid.Rows[i1].ChildRows[i2 + 2].Properties.Value != null)
                                foperand_id = VGrid.Rows[i1].ChildRows[i2 + 2].Properties.Value.ToString();
                        }

                        if (t2 == 2) // odd ise (tek)
                        {
                            if (VGrid.Rows[i1].ChildRows[i2 + 1].Properties.Value != null)
                                foperand_id = VGrid.Rows[i1].ChildRows[i2 + 1].Properties.Value.ToString();
                        }

                        foperand = Kriter_Operand_Find(foperand_id);
                        //---

                        // parçalar bir araya geldi, şimdi and ile başlayan yeni bir where parçası oluştulacak

                        if (foperand == "%%")
                            tmpkriter = tmpkriter + " and " + fullname + " like '%" + fvalue2 + "%' " + v.ENTER;
                        else if (foperand == "%")
                            tmpkriter = tmpkriter + " and " + fullname + " like '" + fvalue2 + "%' " + v.ENTER;
                        else if (foperand != string.Empty && ffieldtype != 40 && ffieldtype != 58 && ffieldtype != 61)
                            tmpkriter = tmpkriter + " and " + fullname + " " + foperand + " " + fvalue + v.ENTER;
                        else if (foperand != string.Empty && (ffieldtype == 40 || ffieldtype == 58 || ffieldtype == 61))
                            tmpkriter = tmpkriter + " and convert(date, " + fullname + ", 103) " + foperand + " " + fvalue + v.ENTER;
                        // ------

                        if (t.IsNotNull(tmpkriter))
                        {
                            //if (t.IsNotNull(tkrt_alias))
                            //{
                            //    aSQL = t.SQLWhereAdd(aSQL, tkrt_alias, tmpkriter, "KRITER");
                            //}
                            //else
                            //{
                            //    fkriter = fkriter + tmpkriter;
                            //}

                            /// vgrid üzerinde oluşan kriterleri topla
                            /// 
                            fkriter = fkriter + tmpkriter;
                            tmpkriter = string.Empty;
                        }

                    }
                    #endregion

                    // --------------------------

                }
                #endregion categori altı


            }
            #endregion Vgrid row döngüsü

            #region SQL'i çalıştır

            if ((t.IsNotNull(fkriter)) || (t.IsNotNull(Where_Add)))
            {
                //if (t.IsNotNull(fkriter))
                //    t.tKriter_Ekle(ref aSQL, alias, fkriter);
                //if (t.IsNotNull(Where_Add))
                //    t.tKriter_Ekle(ref aSQL, alias, Where_Add);

                t.tKriter_Clear(ref aSQL);

                if (t.IsNotNull(fkriter))
                    t.tKriter_Add(ref aSQL, fkriter);
                if (t.IsNotNull(Where_Add))
                    t.tKriter_Add(ref aSQL, Where_Add);
            }

            aSQL = t.kisitlamalarClear(aSQL);

            t.Data_Read_Execute(tForm, dsData, ref aSQL, "", cntrl);

            //for (int i1 = 0; i1 < t1; i1++)
            //{
            //    // category altındaki edittext sayısı
            //    t2 = VGrid.Rows[i1].ChildRows.Count;
            //    for (int i2 = 0; i2 < t2; i2++)
            //    {
            //        VGrid.Rows[i1].ChildRows[i2].Expanded = true;
            //    }
            // veya
            //    VGrid.FullExpandRow(VGrid.Rows[i1]);
            // ------------------   
            //    VGrid.Rows[i1].Expanded = true;
            //}

            #endregion SQL'i çalıştır

            t.Takipci(function_name, "", '}');
        }

        private string Kriter_Operand_Find(string operand_id)
        {
            string sonuc = string.Empty;

            if (operand_id == string.Empty) sonuc = "";
            if (operand_id == "0") sonuc = "";
            if (operand_id == "1") sonuc = ">=";
            if (operand_id == "2") sonuc = ">";
            if (operand_id == "3") sonuc = "=";
            if (operand_id == "4") sonuc = "<=";
            if (operand_id == "5") sonuc = "<";
            if (operand_id == "6") sonuc = "%%";
            if (operand_id == "7") sonuc = "%";

            return sonuc;
        }

        private void Kriterleri_Temizle(VGridControl VGrid, string TableIPCode)
        {

            string function_name = "Kriterleri_Temizle";
            t.Takipci(function_name, "", '{');

            //VGrid.Rows.Count;
            int t1 = VGrid.Rows.Count;
            int t2 = 0;
            string fname = string.Empty;
            string fname2 = string.Empty;


            DataSet ds_Fields = new DataSet();

            tDefaultValue df = new tDefaultValue();
            tTablesRead tr = new tTablesRead();
            tr.MS_Fields_IP_Read(ds_Fields, TableIPCode);

            string tdefault1 = "";
            string tdefault2 = "";
            string tcolumn_type = "";

            for (int i1 = 0; i1 < t1; i1++)
            {
                // category altındaki edittext sayısı
                t2 = VGrid.Rows[i1].ChildRows.Count;

                // t2 = 4 ise sorgu tipi even  (başlangıç, bitiş)
                // t2 = 2 ise sorgu tipi odd   (tek sorgu)

                for (int i2 = 0; i2 < t2; i2++)
                {
                    fname = VGrid.Rows[i1].ChildRows[i2].Properties.FieldName.ToString();
                    tcolumn_type = VGrid.Rows[i1].ChildRows[i2].Tag?.ToString();

                    fname2 = t.Alias_Clear(fname);

                    getDefaultValuesType(ds_Fields, fname2, ref tdefault1, ref tdefault2);

                    if ((fname.IndexOf("bas_sorgu_") == -1) &&
                        (fname.IndexOf("bit_sorgu_") == -1))
                    {
                        VGrid.Rows[i1].ChildRows[i2].Properties.Value = "";

                        if (t2 == 4) // even ise
                        {
                            if (i2 == 0)
                                VGrid.Rows[i1].ChildRows[i2 + 2].Properties.Value = ((short)(v.EsitVeBuyuk));

                            if (i2 == 1)
                                VGrid.Rows[i1].ChildRows[i2 + 2].Properties.Value = ((short)(v.EsitVeKucuk));
                            
                            if (tcolumn_type == "DateEdit" || tcolumn_type == "40" || tcolumn_type == "58" || tcolumn_type == "61")
                            {
                                if (t.IsNotNull(tdefault1) && i2 == 0)
                                    VGrid.Rows[i1].ChildRows[i2].Properties.Value = df.tSP_Value_Load(tdefault1);

                                if (t.IsNotNull(tdefault1) && t.IsNotNull(tdefault2) == false && i2 == 1)
                                    VGrid.Rows[i1].ChildRows[i2].Properties.Value = df.tSP_Value_Load(tdefault1);

                                if (t.IsNotNull(tdefault2) && i2 == 1)
                                    VGrid.Rows[i1].ChildRows[i2].Properties.Value = df.tSP_Value_Load(tdefault2);
                            }
                        }

                        if (t2 == 2) // odd ise
                        {
                            if (VGrid.Rows[i1].ChildRows[i2 + 1].Properties.RowEdit.Name.ToString() == "ODD_LIKE")
                                VGrid.Rows[i1].ChildRows[i2 + 1].Properties.Value = ((short)(v.Benzerleri_Tam));

                            if (VGrid.Rows[i1].ChildRows[i2 + 1].Properties.RowEdit.Name.ToString() == "ODD_NOTLIKE")
                                VGrid.Rows[i1].ChildRows[i2 + 1].Properties.Value = ((short)(v.Esit));

                            if (tcolumn_type == "DateEdit" || tcolumn_type == "40" || tcolumn_type == "58" || tcolumn_type == "61")
                            {
                                if (t.IsNotNull(tdefault1) && i2 == 0)
                                    VGrid.Rows[i1].ChildRows[i2].Properties.Value = df.tSP_Value_Load(tdefault1);
                            }
                        }
                    }

                }
            }

            t.Takipci(function_name, "", '}');
        }

        private void getDefaultValuesType(DataSet dsFields, string fieldName, ref string tdefault1, ref string tdefault2)
        {
            string fname = "";
            tdefault1 = "";
            tdefault2 = "";

            foreach (DataRow Row in dsFields.Tables[0].Rows)
            {
                fname = t.Set(Row["KRT_CAPTION"].ToString(), Row["LKP_FIELD_NAME"].ToString(), "null");
                if (fieldName == fname)
                {
                    tdefault1 = t.Set(Row["KRT_DEFAULT1"].ToString(), "", "");
                    tdefault2 = t.Set(Row["KRT_DEFAULT2"].ToString(), "", "");
                    if (tdefault1 != "0" && tdefault2 == "0") tdefault2 = "";
                    break;
                }
            }
        }

        #endregion  Kriter Buttons Click <<

        #region DataWizard Buttons Click >>
        public void btn_DataWizard_Click(object sender, EventArgs e)
        {


            // şimdilik gerek kalmadı
        }
        #endregion DataWizard Buttons Click <<

        #region Param Buttons Click >>

        public void btn_Param_Click(object sender, EventArgs e)
        {

            string function_name = "btn Param_Click";
            t.Takipci(function_name, "", '{');

            // TableName / TableIPCode geliyor
            string prp_type = ((DevExpress.XtraEditors.SimpleButton)sender).AccessibleName;
            string Kriterler = string.Empty;

            #region Tanımlar
            // tüm bu olayların oluştuğu form tespit ediliyor
            Form tForm = t.Find_Form(sender);

            string tbutton = ((DevExpress.XtraEditors.SimpleButton)sender).Text;

            // VgridControl tespit ediliyor
            string[] controls = new string[] { "DevExpress.XtraVerticalGrid.VGridControl" };
            Control cntrl = t.Find_Control(tForm, "", "tPARAMS_" + prp_type, controls);
            #endregion Tanımlar

            #region Cloase
            if (tbutton == "&Çıkış")
            {
                tForm.AccessibleDefaultActionDescription = "CLOSE";
                tForm.Close();
            }
            #endregion Close

            #region Listele
            if (tbutton == "&Listele")
            {
                string ftype = string.Empty;
                if (tForm.Tag != null)
                    ftype = tForm.Tag.ToString();

                Kriterler = string.Empty;
                Kriterler = t.Read_ParamsValue(tForm, (DevExpress.XtraVerticalGrid.VGridControl)cntrl, prp_type);

                if (ftype == "DIALOG")
                {
                    tForm.AccessibleDefaultActionDescription = Kriterler;
                    tForm.Close();
                }
                else
                {
                    // Devam işlemi 
                    ((DevExpress.XtraEditors.SimpleButton)sender).AccessibleDefaultActionDescription = Kriterler;
                }
            }
            #endregion Listele

            #region Daha fazla
            if (tbutton == "Daha fazla")
            {
                ((DevExpress.XtraEditors.SimpleButton)sender).Text = "Kısıtlı";
                Param_Type_Create(tForm, (DevExpress.XtraVerticalGrid.VGridControl)cntrl, prp_type, false,
                              t.myInt32(((DevExpress.XtraVerticalGrid.VGridControl)cntrl).Tag.ToString()));
            }
            #endregion Daha fazla

            #region Kısıtlı
            if (tbutton == "Kısıtlı")
            {
                ((DevExpress.XtraEditors.SimpleButton)sender).Text = "Daha fazla";
                Param_Type_Create(tForm, (DevExpress.XtraVerticalGrid.VGridControl)cntrl, prp_type, true,
                              t.myInt32(((DevExpress.XtraVerticalGrid.VGridControl)cntrl).Tag.ToString()));
            }
            #endregion Kısıtlı

            #region Temizle
            if (tbutton == "&Temizle")
            {
                ((DevExpress.XtraVerticalGrid.VGridControl)cntrl).FocusPrev();

                t.Clear_ParamsValue(tForm, prp_type);
                //tProperties_Button_Clear((DevExpress.XtraVerticalGrid.VGridControl)cntrl);
            }
            #endregion Temizle

            #region RowCollapse
            if (tbutton == "Kapat")
            {
                ((DevExpress.XtraVerticalGrid.VGridControl)cntrl).CollapseAllRows();
            }
            #endregion RowCollapse

            #region RowExpand
            if (tbutton == "Aç")
            {
                ((DevExpress.XtraVerticalGrid.VGridControl)cntrl).ExpandAllRows();
            }
            #endregion RowExpand

            t.Takipci(function_name, "", '}');
        }

        #region Sub Param Functions

        private void Param_Type_Create(Form tForm, VGridControl tVGridControl, string TableName, Boolean Kist, int tTableType)
        {


            DataSet dsKrtr = t.Find_DataSet(tForm, "", TableName, "");

            if (dsKrtr != null)
            {
                tCreateObject co = new tCreateObject();

                //tVGridControl.Rows.
                tVGridControl.Rows.Clear();

                co.Create_Param_Columns(tVGridControl, dsKrtr, TableName, Kist, tTableType);
            }

        }

        #endregion Sub Param Functions

        #endregion  Param Buttons Click <<


        #region Category Events

        public void ctg_DataNavigator_CatList_PositionChanged(object sender, EventArgs e)
        {


            Form tForm = t.Find_Form(sender);

            ctg_Category_Detail_Read(tForm, ((DevExpress.XtraEditors.DataNavigator)sender));
        }

        public void ctg_DataNavigator_CatDetail_PositionChanged(object sender, EventArgs e)
        {


            Form tForm = t.Find_Form(sender);

            int NewPosition = ((DevExpress.XtraEditors.DataNavigator)sender).Position;
            object tDataTable = ((DevExpress.XtraEditors.DataNavigator)sender).DataSource;
            DataSet tdsData = ((DataTable)tDataTable).DataSet;

            if (NewPosition == -1) return;

            Control cntrl = new Control();
            cntrl = t.Find_Control_View(tForm, "tTreeList_CatDetail_");

            if (cntrl != null)
            {
                string FieldName = ((DevExpress.XtraTreeList.TreeList)cntrl).AccessibleDescription;
                Int16 FieldType = System.Convert.ToInt16(((DevExpress.XtraTreeList.TreeList)cntrl).AccessibleDefaultActionDescription);

                string TableIPCode = tdsData.Namespace;
                string fvalue = tdsData.Tables[0].Rows[NewPosition]["LKP_ID"].ToString();

                string softCode = "";
                string projectCode = "";

                string TableCode = string.Empty;
                string IPCode = string.Empty;

                t.TableIPCode_Get(TableIPCode, ref softCode, ref projectCode, ref TableCode, ref IPCode);
                //---

                string Where_And = " and  [" + TableCode + "]." + FieldName + " = " + t.tData_Convert(fvalue, FieldType);

                //--- Tümü seçilmiş ise
                if (fvalue == "-100")
                    Where_And = string.Empty;

                string[] controls = new string[] { "DevExpress.XtraEditors.SimpleButton" };
                Control c = t.Find_Control(tForm, "simpleButton_Kriter_Listele", TableIPCode, controls);
                if (c != null)
                {
                    ((DevExpress.XtraEditors.SimpleButton)c).AccessibleDefaultActionDescription = Where_And;
                    //((DevExpress.XtraEditors.SimpleButton)c).PerformClick();
                    btn_KriterListele_Click(((DevExpress.XtraEditors.SimpleButton)c), EventArgs.Empty);

                }

                //MessageBox.Show(Where_And);
            }
        }

        private void ctg_Category_Detail_Read(Form tForm, DevExpress.XtraEditors.DataNavigator tDataNavigator)
        {

            int NewPosition = tDataNavigator.Position;
            object tDataTable = tDataNavigator.DataSource;
            DataSet tdsData = ((DataTable)tDataTable).DataSet;

            if (NewPosition == -1) return;

            string TableName = tdsData.Namespace;

            string FieldName = tdsData.Tables[0].Rows[NewPosition]["FIELD_NAME"].ToString();
            Int16 FieldType = System.Convert.ToInt16(tdsData.Tables[0].Rows[NewPosition]["FIELD_TYPE"].ToString());

            string Type_Name = tdsData.Tables[0].Rows[NewPosition]["LOOKUP_CODE"].ToString();
            Int16 Type = System.Convert.ToInt16(tdsData.Tables[0].Rows[NewPosition]["TLKP_TYPE"].ToString());

            string SQL = string.Empty;

            // Data Seçenekleri
            if (Type == 107)
            {
                //SQL = tSql.DATA_Types_SQL(Type_Name);
            }
            else
            {
                //SQL = tSql.ImageComboBox_Fill_SQL(Type_Name, Type);
            }

            if (SQL != "")
            {
                ctg_Category_Detail_Read(tForm, TableName, SQL, FieldName, FieldType);
            }

        }

        private void ctg_Category_Detail_Read(Form tForm, string TableName, string SQL,
                                              string FieldName, Int16 FieldType)
        {


            string function_name = "Category_Detail_Read";

            Control cntrl = new Control();
            cntrl = t.Find_Control_View(tForm, "tTreeList_CatDetail_");

            if (cntrl != null)
            {
                ((DevExpress.XtraTreeList.TreeList)cntrl).AccessibleDescription = FieldName;
                ((DevExpress.XtraTreeList.TreeList)cntrl).AccessibleDefaultActionDescription = FieldType.ToString();

                object tDataTable = ((DevExpress.XtraTreeList.TreeList)cntrl).DataSource;
                DataSet tdsData = ((DataTable)tDataTable).DataSet;

                t.TableRemove(tdsData);

                t.SQL_Read_Execute(v.dBaseNo.Project, tdsData, ref SQL, "", function_name);

                DataNavigator tDataNavigator = null;
                tDataNavigator = t.Find_DataNavigator(tForm, "tDataNavigator_CatDetail");//, function_name);

                ((DevExpress.XtraTreeList.TreeList)cntrl).DataSource = tdsData.Tables[0];
                tDataNavigator.DataSource = tdsData.Tables[0];
            }

        }

        private void ctg_Category_Form_Shown(Form tForm)
        {

            //string function_name = "Category_Form_Shown";

            DataNavigator tDataNavigator = null;

            tDataNavigator = t.Find_DataNavigator(tForm, "tDataNavigator_CatList");//, function_name);

            if (tDataNavigator != null)
            {
                ctg_Category_Detail_Read(tForm, tDataNavigator);
            }

        }

        #endregion Category Events

        #region WizardControl Button Click

        public void tWizardControl_CancelClick(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string Question = string.Empty;
            Question = "İşlemden vazgeçmek istediğinize emin misiniz?";

            DialogResult answer = MessageBox.Show(Question, "Onay İşlemi", MessageBoxButtons.YesNo);

            switch (answer)
            {
                case DialogResult.Yes:
                    {
                        Control c = ((DevExpress.XtraWizard.WizardControl)sender).Parent;

                        ((DevExpress.XtraWizard.WizardControl)sender).Dispose();

                        c.Dispose();

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

        public void tWizardControl_FinishClick(object sender, System.ComponentModel.CancelEventArgs e)
        {


            Form tForm = t.Find_Form(sender);
            string TableIPCode = ((DevExpress.XtraWizard.WizardControl)sender).AccessibleName;

            v.tButtonHint.Clear();
            v.tButtonHint.tForm = tForm;
            v.tButtonHint.tableIPCode = TableIPCode;
            v.tButtonHint.buttonType = v.tButtonType.btKaydet;

            tEventsButton evb = new tEventsButton();
            evb.btnClick(v.tButtonHint);

            //t.Find_DataNavigator_ButtonLink(tForm, TableIPCode, v.nv_22_Kaydet.ToString());
        }

        public void tWizardControl_HelpClick(object sender, DevExpress.XtraWizard.WizardButtonClickEventArgs e)
        {
            //MessageBox.Show("help..");
        }

        public void tWizardControl_NextClick(object sender, DevExpress.XtraWizard.WizardCommandButtonClickEventArgs e)
        {
            //MessageBox.Show("next..  " + sender.ToString() + " // " + e.Page.Name.ToString());

            Form tForm = ((DevExpress.XtraWizard.WizardControl)sender).FindForm();
            Control page = t.Find_Control(tForm, e.Page.Name.ToString());
            if (page != null)
            {
                MessageBox.Show("next..  " + sender.ToString() + " // " + e.Page.Name.ToString());
            }
        }

        public void tWizardControl_PrevClick(object sender, DevExpress.XtraWizard.WizardCommandButtonClickEventArgs e)
        {
            //MessageBox.Show("prev..");
        }

        #endregion WizardControl Button Click

        #region AccordionDinamikElement_Click
        public void tAccordionDinamikElement_Click(object sender, EventArgs e)
        {
            DevExpress.XtraBars.Navigation.AccordionControl menuControl =
                ((DevExpress.XtraBars.Navigation.AccordionControlElement)sender).AccordionControl;

            if (menuControl.Elements.Count <= 0) return;

            //

            string ButtonName = string.Empty;
            string Caption = string.Empty;
            string selectItemValue = string.Empty;
            string selectItemHint = string.Empty;

            ButtonName = ((DevExpress.XtraBars.Navigation.AccordionControlElement)sender).Name.ToString();
            Caption = ((DevExpress.XtraBars.Navigation.AccordionControlElement)sender).Text;

            if (((DevExpress.XtraBars.Navigation.AccordionControlElement)sender).Tag != null)
                selectItemValue = ((DevExpress.XtraBars.Navigation.AccordionControlElement)sender).Tag.ToString();


            // RibbonPage Visible için gerekli olan değer
            // Yani AccordionControl üzerinde hangi item tıklandıysa
            // onunla ilgili bir TabPage oluşturuluyor
            // birde konuyla ilgili RibbonPage visible = true ediliyor
            // bu üç nesne arasındaki bağlantı accordion menu için hazırlanan selectteki GRUPVALUE ile çözülüyor

            // click lenen itemın hangi grup altında olduğunu AccordionControl üzerinde bir yere işaret bırakıyor
            // bu işarete bakarak kendisine bağlı olan Ribbon page leri ayarlanıyor (visible)

            if (((DevExpress.XtraBars.Navigation.AccordionControlElement)sender).Hint != null)
                selectItemHint = ((DevExpress.XtraBars.Navigation.AccordionControlElement)sender).Hint.ToString();

            // bunula yeni açılan tabPage bağlı RibbonPage tespit etmeye yarıyor
            menuControl.AccessibleDefaultActionDescription = selectItemHint;

            // selectItemHint değerini yeni oluşturulan tabPage nin  AccessibleDefaultActionDescription set ettiğimizdede
            // kullanıcı manuel tabPage değişitirince hangi RibbonPage aktif etmesi gerektiğini biliyor

            //if (t.IsNotNull(selectItemValue) == false) return;
            if ((selectItemValue == string.Empty) ||
                (selectItemValue == "")) return;

            string Read_TableIPCode = menuControl.AccessibleName;
            string fNames = menuControl.AccessibleDescription; // VezneId||FinansId||
            string GroupFName = t.Get_And_Clear(ref fNames, "||");
            string DetailFName = t.Get_And_Clear(ref fNames, "||");

            //MessageBox.Show(
            //     ((DevExpress.XtraBars.Navigation.AccordionControlElement)sender).Text + " : " + selectItemValue + " : " + Read_TableIPCode+ " : " + DetailFName);


            //((DevExpress.XtraToolbox.ToolboxControl)sender).OptionsView.MenuButtonCaption = e.Item.Caption;

            // Group altındaki hesaplar / item lar
            if (ButtonName.IndexOf("item_MainItem_") == -1)
            {
                DevExpress.XtraBars.Navigation.AccordionControlElement owner =
                    ((DevExpress.XtraBars.Navigation.AccordionControlElement)sender).OwnerElement;

                AccordionDimanik_SelectedDataSet(menuControl, Read_TableIPCode, DetailFName, selectItemValue);
                AccordionDinamik_SubPage_Preparing(menuControl, owner, Read_TableIPCode, DetailFName, selectItemValue, Caption, selectItemHint); // selectItemHint = MenuValue
            }

            // Main group altındaki diğer group lar
            if (ButtonName.IndexOf("item_MainItem_") > -1)
            {
                AccordionDinamik_SubGroup_Preparing(menuControl, Read_TableIPCode, GroupFName, selectItemValue);
            }

        }

        private void AccordionDimanik_SelectedDataSet(DevExpress.XtraBars.Navigation.AccordionControl menuControl,
            string Read_TableIPCode,
            string DetailFName,
            string selectItemValue
            )
        {
            // bunun amacı sadece :
            // kullanıcını menuden seçtiği menuitem ın sonucunda 
            // finans/detay isimlerinin olduğu dataset doğru dNavigator.Position set etmek

            if (t.IsNotNull(Read_TableIPCode) &&
                t.IsNotNull(DetailFName))
            {
                #region
                Form tForm = menuControl.FindForm();
                DataSet dsRead = null;
                DataNavigator tdN_Read = null;

                t.Find_DataSet(tForm, ref dsRead, ref tdN_Read, Read_TableIPCode);

                int i = -1;
                if (t.IsNotNull(dsRead))
                    i = t.Find_GotoRecord(dsRead, DetailFName, selectItemValue);
                tdN_Read.Position = i;
                #endregion
            }
        }

        private void AccordionDinamik_SubPage_Preparing(
            DevExpress.XtraBars.Navigation.AccordionControl menuControl,
            DevExpress.XtraBars.Navigation.AccordionControlElement owner,
            string Read_TableIPCode,
            string selectFName,
            string selectItemValue,
            string Caption,
            string MenuValue
            )
        {
            if (owner.Tag == null)
            {
                MessageBox.Show("DİKKAT : Açılacak sayfa için gerekli bilgiler tanımlanmamış ...");
            }

            if (owner.Tag != null)
            {
                Form tForm = menuControl.FindForm();
                string Prop_Navigator = owner.Tag.ToString();

                //MessageBox.Show(owner.Name.ToString() + v.ENTER + Prop_Navigator);

                AccordionDinamikView_(tForm, Prop_Navigator, selectItemValue, Caption, MenuValue);
            }
        }

        private void AccordionDinamik_SubGroup_Preparing(
              DevExpress.XtraBars.Navigation.AccordionControl menuControl,
              string Read_TableIPCode, string GroupFName, string selectItemValue)
        {
            // menu hazır olan ile yeni seçilmiş olan MainItem aynı olabilir
            // yani menu zaten hazır tekrar aynı item clicke basılmış olabilir
            // yeniden aynı sub menuleri tekrarlamsın diye geri dön marş marş

            if (menuControl.Tag != null)
            {
                if (menuControl.Tag.ToString() == selectItemValue)
                    return;
            }

            menuControl.Tag = selectItemValue;


            if (t.IsNotNull(Read_TableIPCode) &&
                t.IsNotNull(GroupFName)
                )
            {
                #region
                tEvents ev = new tEvents();

                Form tForm = menuControl.FindForm();
                DataSet dsRead = null;
                DataNavigator tdN_Read = null;

                t.Find_DataSet(tForm, ref dsRead, ref tdN_Read, Read_TableIPCode);

                if (t.IsNotNull(dsRead) == false)
                {
                    v.Kullaniciya_Mesaj_Var = "Data yok : " + Read_TableIPCode;
                    return;
                }

                int i2 = dsRead.Tables[0].Rows.Count;
                int elementCount = 1;

                string read_CaptionFName = string.Empty;
                string read_KeyFName = string.Empty;
                string chc_FName = string.Empty;
                string chc_Value = string.Empty;

                string Prop_Navigator = string.Empty;
                string itemCaption = string.Empty;
                string itemValue = string.Empty;
                string itemName = string.Empty;
                string s1, s2 = string.Empty;

                // tüm grupları sırayla ele alalım
                foreach (DevExpress.XtraBars.Navigation.AccordionControlElement pGroup in menuControl.Elements)
                {
                    // MainGroup ise ellenmez, yani ana hesaplar (Banka - Şubeleri)
                    // diğer gruplar ise önce temizleniyor
                    // sonra seçilen hesabın (Banka - Şube nin ) .... açıklamaya devam et ?
                    #region
                    if ((pGroup.Name.IndexOf("item_MainGroup_") == -1) &&
                        (pGroup.Name.IndexOf("item_Group_") > -1))
                    {
                        elementCount = 1;
                        pGroup.Elements.Clear();
                        pGroup.Appearance.Normal.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
                        pGroup.Appearance.Normal.ForeColor = System.Drawing.Color.Black;
                        pGroup.Appearance.Normal.Options.UseFont = true;
                        pGroup.Appearance.Normal.Options.UseForeColor = true;

                        #region
                        if (pGroup.Tag != null)
                        {
                            Prop_Navigator = pGroup.Tag.ToString();

                            s1 = "=ROW_PROP_NAVIGATOR:";
                            s2 = (char)34 + "TABLEIPCODE_LIST" + (char)34 + ": [";
                            // "TABLEIPCODE_LIST": [

                            // OLD
                            if (Prop_Navigator.IndexOf(s1) > -1)
                            {
                                // Group bilgileri için 
                                read_CaptionFName = t.MyProperties_Get(Prop_Navigator, "RTABLEIPCODE:"); // esasında okunan display edilecek field
                                read_KeyFName = t.MyProperties_Get(Prop_Navigator, "RKEYFNAME:");  // esasında okunan ref Id field

                                chc_FName = t.MyProperties_Get(Prop_Navigator, "CHC_FNAME:");  // esasında okunan check field name
                                chc_Value = t.MyProperties_Get(Prop_Navigator, "CHC_VALUE:");  // esasında okunan check value field name
                            }

                            // JSON
                            if (Prop_Navigator.IndexOf(s2) > -1)
                            {
                                /*
                                PROP_NAVIGATOR packet = new PROP_NAVIGATOR();
                                Prop_Navigator = Prop_Navigator.Replace((char)34, (char)39);
                                //var prop_ = JsonConvert.DeserializeAnonymousType(Prop_Navigator, packet);
                                */
                                if (((Prop_Navigator.IndexOf("[") > -1) &&
                                     (Prop_Navigator.IndexOf("[") < 5)) == true)
                                {
                                    Prop_Navigator = Prop_Navigator.Remove(Prop_Navigator.IndexOf("["), 1);
                                    Prop_Navigator = Prop_Navigator.Remove(Prop_Navigator.IndexOf("]", (Prop_Navigator.Length - 5)), 1);
                                }

                                PROP_NAVIGATOR prop_ = t.readProp<PROP_NAVIGATOR>(Prop_Navigator);

                                // Group bilgileri için 
                                foreach (var item in prop_.TABLEIPCODE_LIST)
                                {
                                    read_CaptionFName = item.RTABLEIPCODE;
                                    read_KeyFName = item.RKEYFNAME;
                                }

                                chc_FName = prop_.CHC_FNAME;
                                chc_Value = prop_.CHC_VALUE;
                            }

                            #region
                            for (int i = 0; i < i2; i++)
                            {
                                if ((dsRead.Tables[0].Rows[i][chc_FName].ToString() == chc_Value) &&
                                    (dsRead.Tables[0].Rows[i][GroupFName].ToString() == selectItemValue))
                                {

                                    itemCaption = dsRead.Tables[0].Rows[i][read_CaptionFName].ToString();
                                    itemValue = dsRead.Tables[0].Rows[i][read_KeyFName].ToString();

                                    //DevExpress.XtraToolbox.ToolboxItem barButtonItem = new DevExpress.XtraToolbox.ToolboxItem();
                                    DevExpress.XtraBars.Navigation.AccordionControlElement barButtonItem =
                                       new DevExpress.XtraBars.Navigation.AccordionControlElement();

                                    itemName = "item_" + itemValue;// refid;

                                    barButtonItem.Name = itemName;
                                    barButtonItem.Text = itemCaption;
                                    barButtonItem.Tag = itemValue;
                                    barButtonItem.Hint = chc_Value;

                                    barButtonItem.Appearance.Normal.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
                                    barButtonItem.Appearance.Normal.ForeColor = System.Drawing.Color.Black;
                                    barButtonItem.Appearance.Normal.Options.UseFont = true;
                                    barButtonItem.Appearance.Normal.Options.UseForeColor = true;

                                    barButtonItem.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
                                    barButtonItem.Click += new System.EventHandler(tAccordionDinamikElement_Click);

                                    pGroup.Elements.Add(barButtonItem);
                                    elementCount++;
                                }
                            }
                            #endregion

                            if (elementCount > 1)
                            {
                                pGroup.Appearance.Normal.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
                                //pGroup.Appearance.Normal.ForeColor = System.Drawing.Color.Green;
                                pGroup.Appearance.Normal.Options.UseFont = true;
                                pGroup.Appearance.Normal.Options.UseForeColor = true;
                            }
                        }
                        #endregion
                    }
                    #endregion
                }
                #endregion
            }
        }

        private void AccordionDinamikView_(Form tForm, string Prop_Navigator, string selectItemValue, string caption, string MenuValue)
        {


            #region 

            /// 0=ROW_PROP_NAVIGATOR:0;
            /// 0=CAPTION:null;
            /// 0=BUTTONTYPE:null;
            /// 0=TABLEIPCODE_LIST:TABLEIPCODE_LIST={
            /// 1=ROW_TABLEIPCODE_LIST:1;
            /// 1=CAPTION:Detail Group Bilgileri;
            ///
            /// 1=TABLEIPCODE:FNLNVOL.FNLNVOL_KSL01;
            /// 1=TABLEALIAS:PARAM_SET;
            /// 1=KEYFNAME:AND ( [FNLNVOL].BHESAP_ID = {0} OR [FNLNVOL].AHESAP_ID = {0} );
            ///
            /// 1=RTABLEIPCODE:LKP_FULL_NAME;
            /// 1=RKEYFNAME:LKP_REF_ID;
            /// 1=MSETVALUE:null;
            /// 1=WORKTYPE:READ;
            /// 1=CONTROLNAME:null;
            /// 1=ROWE_TABLEIPCODE_LIST:1;
            /// TABLEIPCODE_LIST=};
            ///
            /// 0=FORMNAME:null;
            /// 0=FORMCODE:null;
            /// 0=FORMTYPE:null;
            /// 0=FORMSTATE:null;
            /// 0=CHC_IPCODE:HPBNK.HPBNK_02;
            /// 0=CHC_FNAME:LKP_FNS_TIPI;
            /// 0=CHC_VALUE:2020;
            /// 0=CHC_OPERAND:=;
            /// 0=ROWE_PROP_NAVIGATOR:0;                    

            #endregion

            string TableIPCode = string.Empty;
            string TableAlias = string.Empty;
            string KeyFName = string.Empty;

            string s1 = "=ROW_PROP_NAVIGATOR:";
            string s0 = ":TABLEIPCODE_LIST={";
            string s2 = (char)34 + "TABLEIPCODE_LIST" + (char)34 + ": [";
            string s3 = (char)39 + "TABLEIPCODE_LIST" + (char)39 + ": [";
            // "TABLEIPCODE_LIST": [

            // OLD
            if ((Prop_Navigator.IndexOf(s1) > -1) ||
                (Prop_Navigator.IndexOf(s0) > -1))
            {
                TableIPCode = t.MyProperties_Get(Prop_Navigator, "=TABLEIPCODE:");
                TableAlias = t.MyProperties_Get(Prop_Navigator, "=TABLEALIAS:");
                KeyFName = t.MyProperties_Get(Prop_Navigator, "=KEYFNAME:");
            }


            // JSON
            if ((Prop_Navigator.IndexOf(s2) > -1) ||
                (Prop_Navigator.IndexOf(s3) > -1))
            {
                if (((Prop_Navigator.IndexOf("[") > -1) &&
                     (Prop_Navigator.IndexOf("[") < 5)) == true)
                {
                    Prop_Navigator = Prop_Navigator.Remove(Prop_Navigator.IndexOf("["), 1);
                    Prop_Navigator = Prop_Navigator.Remove(Prop_Navigator.IndexOf("]", (Prop_Navigator.Length - 5)), 1);
                }

                var prop_ = t.readProp<PROP_NAVIGATOR>(Prop_Navigator);

                // Group bilgileri için 
                foreach (var item in prop_.TABLEIPCODE_LIST)
                {
                    TableIPCode = item.TABLEIPCODE;
                    TableAlias = item.TABLEALIAS;
                    KeyFName = item.KEYFNAME;

                    if (TableAlias.IndexOf("[") == -1)
                        TableAlias = "[" + TableAlias + "]";
                }
            }

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

            subViewExec(tForm, "TabPage", "", TableIPCode, "", selectItemValue, caption, MenuValue);
        }
        
        #endregion AccordionDinamikElement_Click

        #region toolBoxDinamikElement_Click

        #endregion toolBoxDinamikElement_Click

        //-------------------------------

        #region SubFunctions

        #region Data_Refresh
        public void Data_Refresh(Form tForm, DataSet dsData, DataNavigator tDataNavigator)
        {
            /// *** işin sıralaması değişti

            // eski sürüm
            tSubDetail_Refresh(dsData, tDataNavigator); // liste sil ekleniyor
            
            // View/Show işi yapılıyor
            //
            if (tDataNavigator.AccessibleDescription != null)
                propSubView(tForm, dsData, tDataNavigator);
        }
        #endregion Data_Refresh

        #region AddIP_TableIPCode Run
        public void AddIP_TableIPCode(Form tForm)
        {


            string AddIP_Form = string.Empty;
            string AddIP_TableIPCode = string.Empty;
            string AddIP_TableCode = string.Empty;
            string AddIP_FieldName = string.Empty;
            string AddIP_Value = string.Empty;
            string AddListIP_TableIPCode = string.Empty;
            string Parametre = string.Empty;

            string form_prp = t.Set(t.myFormBox_Values(tForm, v.FormLoad),  // Formun üzerindeki MemoEdit ten alınıyor
                                     v.con_FormLoadValue,                    // Gecici Hafızaya aktarılan bilgi
                                     string.Empty);

            AddIP_Form = t.MyProperties_Get(form_prp, "AddIP_Form:");
            // Konu olan bilgisi ( GRUP bilgisi )
            AddIP_TableIPCode = t.MyProperties_Get(form_prp, "AddIP_TableIPCode:");
            AddIP_TableCode = t.MyProperties_Get(form_prp, "AddIP_TableCode:");
            AddIP_FieldName = t.MyProperties_Get(form_prp, "AddIP_FieldName:");
            AddIP_Value = t.Set(t.MyProperties_Get(form_prp, "AddIP_Value:"), "", "0");
            // Konu olana ait alt liste ( Konu olan = GRUP, Alt Liste = Gruba Kayıtlı Öğrenci Listesi )
            AddListIP_TableIPCode = t.MyProperties_Get(form_prp, "AddListIP_TableIPCode:");

            Parametre = t.Set(t.MyProperties_Get(form_prp, "Parametre:"), "", "TRUE");

            // bu özellik şimdilik sadece MSReportView için eklendi
            if (AddIP_Form == "MSREPORTVIEW")
            {
                Control cntrl = null;
                string[] controls = new string[] { };

                cntrl = t.Find_Control(tForm, "groupControl_Konu_Olan", "", controls);

                if ((cntrl != null) && (t.IsNotNull(AddIP_TableIPCode)))
                {
                    tInputPanel ip = new tInputPanel();

                    ((DevExpress.XtraEditors.GroupControl)cntrl).Visible = true;
                    ip.Create_InputPanel(tForm, ((DevExpress.XtraEditors.GroupControl)cntrl), AddIP_TableIPCode, 1, true);

                    v.con_AddIP_TableCode = AddIP_TableCode;
                    v.con_AddIP_FieldName = AddIP_FieldName;
                    v.con_AddIP_Value = AddIP_Value;
                    v.con_AddListIP_TableIPCode = AddListIP_TableIPCode;
                }

                if (Parametre.ToUpper() == "FALSE")
                    v.con_Parametre = false;
                else v.con_Parametre = true;

                if (t.IsNotNull(AddListIP_TableIPCode))
                {
                    // her durumuda gösterilsin
                    v.con_Parametre = true;
                    subViewExec(tForm, "dockPanel", "dockPanel_List", AddListIP_TableIPCode, "", "", "", "");
                }
            }

        }
        #endregion AddIP_TableIPCode Run

        #region AutoNewRecords Tables Run
        public void AutoNewRecords(Form tForm)
        {

        }
        #endregion AutoNewRecords Tables Run

        #region Preparing_OnOff

        private void Preparing_OnOff(ref string Sql, string FieldName, string Value)
        {


            // / *TEORIK_SINAV_TARIHI.ON* /
            // and convert(smalldatetime, ISNULL(K.TEORIK_SINAV_TARIHI,"01.01.1900"), 103) <= TR.SINAV_TARIHI
            // / *TEORIK_SINAV_TARIHI.OFF* /

            string str_bgn = "/*" + FieldName + ".ON*/";
            string str_end = "/*" + FieldName + ".OFF*/";
            int i_bgn = Sql.IndexOf(str_bgn) + str_bgn.Count() + 2;
            int i_end = Sql.IndexOf(str_end);

            if ((i_bgn > -1) && (i_end > -1))
            {
                string old_and = Sql.Substring(i_bgn, i_end - i_bgn);
                string new_and = old_and;

                if (old_and.IndexOf("-- and") > -1)
                {
                    t.Str_Replace(ref new_and, "-- and", "and");
                }
                else
                {
                    t.Str_Replace(ref new_and, "and", "-- and");
                }

                t.Str_Replace(ref Sql, old_and, new_and);
            }
            //MessageBox.Show("||" + old_and + "||" + new_and+"||");
        }

        #endregion Preparing_OnOff

        #region Prop_Navigator örnek kod

        /*
            PROP_NAVIGATOR={
            1=ROW_PROP_NAVIGATOR:1;
            1=CAPTION:aaaaa;
   >>       1=BUTTONTYPE:51;
            1=TABLEIPCODE_LIST:TABLEIPCODE_LIST={
            1=ROW_TABLEIPCODE_LIST:1;
            1=CAPTION:qqqqq;
            1=TABLEIPCODE:null;
            1=TABLEALIAS:null;
            1=KEYFNAME:null;
            1=RTABLEIPCODE:null;
            1=RKEYFNAME:null;
            1=MSETVALUE:null;
            1=WORKTYPE:null;
            1=ROWE_TABLEIPCODE_LIST:1;
            TABLEIPCODE_LIST=};
            1=FORMNAME:null;
            1=FORMCODE:null;
            1=FORMTYPE:null;
            1=FORMSTATE:null;
            1=ROWE_PROP_NAVIGATOR:1;
         * 
            2=ROW_PROP_NAVIGATOR:2;
            2=CAPTION:bbbbb;
   >>       2=BUTTONTYPE:52;
            2=TABLEIPCODE_LIST:TABLEIPCODE_LIST={
            1=ROW_TABLEIPCODE_LIST:1;
            1=CAPTION:qqqqq;
            1=TABLEIPCODE:null;
            1=TABLEALIAS:null;
            1=KEYFNAME:null;
            1=RTABLEIPCODE:null;
            1=RKEYFNAME:null;
            1=MSETVALUE:null;
            1=WORKTYPE:null;
            1=ROWE_TABLEIPCODE_LIST:1;
            2=ROW_TABLEIPCODE_LIST:2;
            2=CAPTION:wwwww;
            2=TABLEIPCODE:shssddsg;
            2=TABLEALIAS:sdfsadf;
            2=KEYFNAME:sdfsadf;
            2=RTABLEIPCODE:sdfasdfdf;
            2=RKEYFNAME:sdfdfsg;
            2=MSETVALUE:234;
            2=WORKTYPE:READ;
            2=ROWE_TABLEIPCODE_LIST:2;
            TABLEIPCODE_LIST=};
            2=FORMNAME:null;
            2=FORMCODE:null;
            2=FORMTYPE:null;
            2=FORMSTATE:null;
            2=ROWE_PROP_NAVIGATOR:2;
         *          
            3=ROW_PROP_NAVIGATOR:3;
            3=CAPTION:ccccccc;
     >>     3=BUTTONTYPE:56;
            3=TABLEIPCODE_LIST:null;
            3=FORMNAME:null;
            3=FORMCODE:null;
            3=FORMTYPE:null;
            3=FORMSTATE:null;
            3=ROWE_PROP_NAVIGATOR:3;
            PROP_NAVIGATOR=}
            */

        #endregion Prop_Navigator

        #region Find_ParentValue
        public string Find_ParentValue(Form tForm, string TableIPCode,
            string Key_FName, string Parent_FName, string hesapName)
        {

            string function_name = "tParentValue";

            /// yeni_hesap
            /// yeni_alt_hesap

            if ((t.IsNotNull(Key_FName) == false) &&
                (hesapName == "yeni_hesap"))
            {
                MessageBox.Show("DİKKAT : Alt hesap oluşturmak için gerekli olan tablonun KEY FIELDNAME belli değil...", function_name);
                return null;
            }
            if ((t.IsNotNull(Parent_FName) == false) &&
                (hesapName == "yeni_alt_hesap"))
            {
                MessageBox.Show("DİKKAT : Alt hesap oluşturmak için gerekli olan tablonun PARENT FIELDNAME belli değil...", function_name);
                return null;
            }

            DataSet ds = null;
            DataNavigator dN = null;
            string find_parentvalue = string.Empty;

            t.Find_DataSet(tForm, ref ds, ref dN, TableIPCode);

            if (t.IsNotNull(ds))
            {
                if ((dN.Position > -1) && (hesapName == "yeni_hesap"))
                    find_parentvalue = ds.Tables[0].Rows[dN.Position][Parent_FName].ToString();
                if ((dN.Position > -1) && (hesapName == "yeni_alt_hesap"))
                    find_parentvalue = ds.Tables[0].Rows[dN.Position][Key_FName].ToString();
            }

            return find_parentvalue;
        }
        #endregion Find_ParentValue

                 

        #region layoutControlGroup_DoubleClick
        public void layoutControlGroup_DoubleClick(object sender, EventArgs e)
        {
            //MessageBox.Show("dbclick");

            //(((DevExpress.XtraLayout.LayoutControlGroup)sender).cus
            /*
            System.IO.Stream stream;
            stream = new System.IO.MemoryStream();
            layoutControl1.SaveLayoutToStream(stream);
            stream.Seek(0, System.IO.SeekOrigin.Begin);

            // ...
            layoutControl1.RestoreLayoutFromStream(stream);
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            */
            /*
            string layoutFileName = "layout.xml";
            //...
            private void Form1_Load(object sender, EventArgs e)
            {
                if (System.IO.File.Exists(layoutFileName))
                   layoutControl1.RestoreLayoutFromXml(layoutFileName);
            }

            private void Form1_Closing(object sender, FormClosingEventArgs e)
            {
                layoutControl1.SaveLayoutToXml(layoutFileName);
            }

                private void btnRestoreAll_Click(object sender, EventArgs e) {
        // Restores all the hidden layout items and groups from the Customization Form. 
        while (this.OwnerControl.HiddenItems.Count > 0)
            this.OwnerControl.HiddenItems[0].RestoreFromCustomization();
             }


           */
        }
        #endregion 

        #region myEdit_
        public void myEdit_(object tEdit, ref string TableIPCode, ref string tag)
        {

            // Find_TableIPCode_XtraEditors ile aynı işi yapıyor

            string tcolumn_type = string.Empty;
            tcolumn_type = tEdit.GetType().Name;

            #region
            if (tcolumn_type == "ButtonEdit")
            {
                TableIPCode = ((DevExpress.XtraEditors.ButtonEdit)tEdit).Properties.AccessibleName;
                tag = ((DevExpress.XtraEditors.ButtonEdit)tEdit).Tag.ToString();
                //tForm = ((DevExpress.XtraEditors.ButtonEdit)tEdit).FindForm();
            }
            if (tcolumn_type == "CalcEdit")
            {
                TableIPCode = ((DevExpress.XtraEditors.CalcEdit)tEdit).Properties.AccessibleName;
                tag = ((DevExpress.XtraEditors.CalcEdit)tEdit).Tag.ToString();
                //tForm = ((DevExpress.XtraEditors.CalcEdit)tEdit).FindForm();
            }
            if (tcolumn_type == "CheckButton")
            {
                TableIPCode = ((DevExpress.XtraEditors.CheckButton)tEdit).AccessibleName;
                tag = ((DevExpress.XtraEditors.CheckButton)tEdit).Tag.ToString();
                //tForm = ((DevExpress.XtraEditors.CheckButton)tEdit).FindForm();
            }
            if (tcolumn_type == "CheckedComboBoxEdit")
            {
                TableIPCode = ((DevExpress.XtraEditors.CheckedComboBoxEdit)tEdit).Properties.AccessibleName;
                tag = ((DevExpress.XtraEditors.CheckedComboBoxEdit)tEdit).Tag.ToString();
                //tForm = ((DevExpress.XtraEditors.CheckedComboBoxEdit)tEdit).FindForm();
            }
            if (tcolumn_type == "CheckEdit")
            {
                TableIPCode = ((DevExpress.XtraEditors.CheckEdit)tEdit).Properties.AccessibleName;
                tag = ((DevExpress.XtraEditors.CheckEdit)tEdit).Tag.ToString();
                //tForm = ((DevExpress.XtraEditors.CheckEdit)tEdit).FindForm();
            }
            if (tcolumn_type == "CheckedListBoxControl")
            {
                TableIPCode = ((DevExpress.XtraEditors.CheckedListBoxControl)tEdit).AccessibleName;
                tag = ((DevExpress.XtraEditors.CheckedListBoxControl)tEdit).Tag.ToString();
                //tForm = ((DevExpress.XtraEditors.CheckedListBoxControl)tEdit).FindForm();
            }
            if (tcolumn_type == "ComboBoxEdit")
            {
                TableIPCode = ((DevExpress.XtraEditors.ComboBoxEdit)tEdit).Properties.AccessibleName;
                tag = ((DevExpress.XtraEditors.ComboBoxEdit)tEdit).Tag.ToString();
                //tForm = ((DevExpress.XtraEditors.ComboBoxEdit)tEdit).FindForm();
            }
            if (tcolumn_type == "DateEdit")
            {
                TableIPCode = ((DevExpress.XtraEditors.DateEdit)tEdit).Properties.AccessibleName;
                tag = ((DevExpress.XtraEditors.DateEdit)tEdit).Tag.ToString();
                //tForm = ((DevExpress.XtraEditors.DateEdit)tEdit).FindForm();
            }
            if (tcolumn_type == "HyperLinkEdit")
            {
                TableIPCode = ((DevExpress.XtraEditors.HyperLinkEdit)tEdit).Properties.AccessibleName;
                tag = ((DevExpress.XtraEditors.HyperLinkEdit)tEdit).Tag.ToString();
                //tForm = ((DevExpress.XtraEditors.HyperLinkEdit)tEdit).FindForm();
            }
            if (tcolumn_type == "ImageComboBoxEdit")
            {
                TableIPCode = ((DevExpress.XtraEditors.ImageComboBoxEdit)tEdit).Properties.AccessibleName;
                tag = ((DevExpress.XtraEditors.ImageComboBoxEdit)tEdit).Tag.ToString();
                //tForm = ((DevExpress.XtraEditors.ImageComboBoxEdit)tEdit).FindForm();
            }
            if (tcolumn_type == "ImageEdit")
            {
                TableIPCode = ((DevExpress.XtraEditors.ImageEdit)tEdit).Properties.AccessibleName;
                tag = ((DevExpress.XtraEditors.ImageEdit)tEdit).Tag.ToString();
                //tForm = ((DevExpress.XtraEditors.ImageEdit)tEdit).FindForm();
            }
            if (tcolumn_type == "ImageListBoxControl")
            {
                TableIPCode = ((DevExpress.XtraEditors.ImageListBoxControl)tEdit).AccessibleName;
                tag = ((DevExpress.XtraEditors.ImageListBoxControl)tEdit).Tag.ToString();
                //tForm = ((DevExpress.XtraEditors.ImageListBoxControl)tEdit).FindForm();
            }
            if (tcolumn_type == "LabelControl")
            {
                TableIPCode = ((DevExpress.XtraEditors.LabelControl)tEdit).AccessibleName;
                tag = ((DevExpress.XtraEditors.LabelControl)tEdit).Tag.ToString();
                //tForm = ((DevExpress.XtraEditors.LabelControl)tEdit).FindForm();
            }
            if (tcolumn_type == "ListBoxControl")
            {
                TableIPCode = ((DevExpress.XtraEditors.ListBoxControl)tEdit).AccessibleName;
                tag = ((DevExpress.XtraEditors.ListBoxControl)tEdit).Tag.ToString();
                //tForm = ((DevExpress.XtraEditors.ListBoxControl)tEdit).FindForm();
            }
            if (tcolumn_type == "LookUpEdit")
            {
                TableIPCode = ((DevExpress.XtraEditors.LookUpEdit)tEdit).Properties.AccessibleName;
                tag = ((DevExpress.XtraEditors.LookUpEdit)tEdit).Tag.ToString();
                //tForm = ((DevExpress.XtraEditors.LookUpEdit)tEdit).FindForm();
            }
            if (tcolumn_type == "MemoEdit")
            {
                TableIPCode = ((DevExpress.XtraEditors.MemoEdit)tEdit).Properties.AccessibleName;
                tag = ((DevExpress.XtraEditors.MemoEdit)tEdit).Tag.ToString();
                //tForm = ((DevExpress.XtraEditors.MemoEdit)tEdit).FindForm();
            }
            if (tcolumn_type == "MemoExEdit")
            {
                TableIPCode = ((DevExpress.XtraEditors.MemoExEdit)tEdit).Properties.AccessibleName;
                tag = ((DevExpress.XtraEditors.MemoExEdit)tEdit).Tag.ToString();
                //tForm = ((DevExpress.XtraEditors.MemoExEdit)tEdit).FindForm();
            }
            if (tcolumn_type == "MRUEdit")
            {
                TableIPCode = ((DevExpress.XtraEditors.MemoExEdit)tEdit).Properties.AccessibleName;
                tag = ((DevExpress.XtraEditors.MemoExEdit)tEdit).Tag.ToString();
                //tForm = ((DevExpress.XtraEditors.MemoExEdit)tEdit).FindForm();
            }
            if (tcolumn_type == "PictureEdit")
            {
                TableIPCode = ((DevExpress.XtraEditors.PictureEdit)tEdit).Properties.AccessibleName;
                tag = ((DevExpress.XtraEditors.PictureEdit)tEdit).Tag.ToString();
                //tForm = ((DevExpress.XtraEditors.PictureEdit)tEdit).FindForm();
            }
            if (tcolumn_type == "PopupContainerControl")
            {
                TableIPCode = ((DevExpress.XtraEditors.PopupContainerControl)tEdit).AccessibleName;
                tag = ((DevExpress.XtraEditors.PopupContainerControl)tEdit).Tag.ToString();
                //tForm = ((DevExpress.XtraEditors.PopupContainerControl)tEdit).FindForm();
            }
            if (tcolumn_type == "PopupContainerEdit")
            {
                TableIPCode = ((DevExpress.XtraEditors.PopupContainerEdit)tEdit).Properties.AccessibleName;
                tag = ((DevExpress.XtraEditors.PopupContainerEdit)tEdit).Tag.ToString();
                //tForm = ((DevExpress.XtraEditors.PopupContainerEdit)tEdit).FindForm();
            }
            if (tcolumn_type == "RadioGroup")
            {
                TableIPCode = ((DevExpress.XtraEditors.RadioGroup)tEdit).Properties.AccessibleName;
                tag = ((DevExpress.XtraEditors.RadioGroup)tEdit).Tag.ToString();
                //tForm = ((DevExpress.XtraEditors.RadioGroup)tEdit).FindForm();
            }
            if (tcolumn_type == "RangeTrackBarControl")
            {
                TableIPCode = ((DevExpress.XtraEditors.RangeTrackBarControl)tEdit).Properties.AccessibleName;
                tag = ((DevExpress.XtraEditors.RangeTrackBarControl)tEdit).Tag.ToString();
                //tForm = ((DevExpress.XtraEditors.RangeTrackBarControl)tEdit).FindForm();
            }
            if (tcolumn_type == "SpinEdit")
            {
                TableIPCode = ((DevExpress.XtraEditors.SpinEdit)tEdit).Properties.AccessibleName;
                tag = ((DevExpress.XtraEditors.SpinEdit)tEdit).Tag.ToString();
                //tForm = ((DevExpress.XtraEditors.SpinEdit)tEdit).FindForm();
            }
            if (tcolumn_type == "TextEdit")
            {
                TableIPCode = ((DevExpress.XtraEditors.TextEdit)tEdit).Properties.AccessibleName;
                tag = ((DevExpress.XtraEditors.TextEdit)tEdit).Tag.ToString();
                //tForm = ((DevExpress.XtraEditors.TextEdit)tEdit).FindForm();
            }
            if (tcolumn_type == "TimeEdit")
            {
                TableIPCode = ((DevExpress.XtraEditors.TimeEdit)tEdit).Properties.AccessibleName;
                tag = ((DevExpress.XtraEditors.TimeEdit)tEdit).Tag.ToString();
                //tForm = ((DevExpress.XtraEditors.TimeEdit)tEdit).FindForm();
            }
            if (tcolumn_type == "TrackBarControl")
            {
                TableIPCode = ((DevExpress.XtraEditors.TrackBarControl)tEdit).Properties.AccessibleName;
                tag = ((DevExpress.XtraEditors.TrackBarControl)tEdit).Tag.ToString();
                //tForm = ((DevExpress.XtraEditors.TrackBarControl)tEdit).FindForm();
            }
            #endregion

        }
        #endregion 

        #region barEditItem_WinExplorerViewStyle_EditValueChanged
        public void barEditItem_WinExplorerViewStyle_EditValueChanged(object sender, EventArgs e)
        {
            //var orientation = (Orientation)barEditItem2.EditValue;
            //tileView1.OptionsTiles.Orientation = orientation;


            var WinExpViewStyle = (WinExplorerViewStyle)((ImageComboBoxEdit)sender).EditValue;

            string TableIPCode = ((ImageComboBoxEdit)sender).Properties.AccessibleName;

            Form tForm = ((ImageComboBoxEdit)sender).FindForm();

            Control cntrl = null;
            cntrl = t.Find_Control_View(tForm, TableIPCode);
            if (cntrl != null)
            {
                DevExpress.XtraGrid.Views.WinExplorer.WinExplorerView tGridView = ((GridControl)cntrl).MainView as WinExplorerView;
                tGridView.OptionsView.Style = (WinExplorerViewStyle)WinExpViewStyle;
            }
        }
        #endregion


        #region Search Engine textEdit_Find / Arama Motoru Events

        // burası EventsButtons a taşındı

        public void textEdit_Find_EditValueChanged(object sender, EventArgs e)
        {
            //tToolBox t = new tToolBox();
            Control cntrl = null;
            Form tForm = ((DevExpress.XtraEditors.TextEdit)sender).FindForm();
            string TableIPCode = ((DevExpress.XtraEditors.TextEdit)sender).Properties.AccessibleDefaultActionDescription;
            cntrl = t.Find_Control_View(tForm, TableIPCode);

            if (cntrl != null)
            {
                if (cntrl.GetType().ToString() == "DevExpress.XtraGrid.GridControl")
                {
                    if (((DevExpress.XtraGrid.GridControl)cntrl).MainView.GetType().ToString() == "DevExpress.XtraGrid.Views.Grid.GridView")
                    {
                        GridView view = ((DevExpress.XtraGrid.GridControl)cntrl).MainView as GridView;
                        view.FindFilterText = "\"" + ((DevExpress.XtraEditors.TextEdit)sender).Text + "\"";
                    }

                    if (((DevExpress.XtraGrid.GridControl)cntrl).MainView.GetType().ToString() == "DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView")
                    {
                        AdvBandedGridView view = ((DevExpress.XtraGrid.GridControl)cntrl).MainView as AdvBandedGridView;
                        view.FindFilterText = "\"" + ((DevExpress.XtraEditors.TextEdit)sender).Text + "\"";
                    }
                }
                if (cntrl.GetType().ToString() == "DevExpress.XtraTreeList.TreeList")
                {
                    ((DevExpress.XtraTreeList.TreeList)cntrl).FindFilterText =
                        "\"" + ((DevExpress.XtraEditors.TextEdit)sender).Text + "\"";
                }

                //if (!string.IsNullOrEmpty(gridView1.FindFilterText) && !gridView1.FindFilterText.Contains('"'))
                //    gridView1.FindFilterText = "\"" + gridView1.FindFilterText + "\"";
            }

            /// ( 100 * find )  neden bunu yapıyorum ?
            /// findDelay = 100 ise standart find
            /// findDelay = 200 ise list && data  yı işaret ediyor 
            int findType = t.myInt32(((DevExpress.XtraEditors.TextEdit)sender).Tag.ToString());

            int valueCount = ((DevExpress.XtraEditors.TextEdit)sender).Text.Length;

            if ((findType == 200) &&
                (v.searchCount > 0) &&
                (v.searchCount > valueCount))
                InData_Close(tForm, TableIPCode);
        }

        public void textEdit_Find_KeyDown(object sender, KeyEventArgs e)// ***Taşındı
        {
            
            if (t.findAttendantKey(e))
            {
                string propNavigator = "";
                string buttonName = "";

                vButtonHint tButtonHint = new vButtonHint();
                v.tButtonHint.Clear();
                v.tButtonHint.tForm = ((DevExpress.XtraEditors.TextEdit)sender).FindForm();
                v.tButtonHint.tableIPCode = ((DevExpress.XtraEditors.TextEdit)sender).Properties.AccessibleDefaultActionDescription;
                
                //if (((DevExpress.XtraEditors.TextEdit)sender).Properties.AccessibleDescription != null)
                //    v.tButtonHint.propNavigator = t.Set(((DevExpress.XtraEditors.TextEdit)sender).Properties.AccessibleDescription,"","");
                
                if (((DevExpress.XtraEditors.TextEdit)sender).EditValue != null)
                    v.tButtonHint.columnEditValue = ((DevExpress.XtraEditors.TextEdit)sender).EditValue.ToString();
                
                
                v.tButtonHint.buttonType = getClickType(v.tButtonHint.tForm, v.tButtonHint.tableIPCode, e, ref propNavigator, ref buttonName);

                if (propNavigator != "")
                    v.tButtonHint.propNavigator = propNavigator;
                if (buttonName != "")
                    v.tButtonHint.buttonName = buttonName;

                tEventsButton evb = new tEventsButton();
                evb.btnClick(v.tButtonHint);
                return;
            }

            if (t.findDirectionKey(e) ||
                t.findReturnKey(e))
            {
                tEventsGrid evg = new tEventsGrid();
                // preparing tGridHint
                vGridHint tGridHint = new vGridHint();

                Control cntrl = null;
                Form tForm = ((DevExpress.XtraEditors.TextEdit)sender).FindForm();
                string TableIPCode = ((DevExpress.XtraEditors.TextEdit)sender).Properties.AccessibleDefaultActionDescription;
                cntrl = t.Find_Control_View(tForm, TableIPCode);

                if (cntrl != null)
                {
                    evg.getGridHint_(cntrl, ref tGridHint);

                    if (t.findDirectionKey(e))
                        evg.gridSpeedKeys(tGridHint, e);
                    if (t.findReturnKey(e))
                        evg.commonGridClick(sender, e, tGridHint);
                }
                return;
            }

            ///******************* altlar eski

            if ((e.KeyCode == Keys.Up) ||
                (e.KeyCode == Keys.Down) ||
                (e.KeyCode == Keys.PageUp) ||
                (e.KeyCode == Keys.PageDown) ||
                (e.KeyCode == Keys.Home) ||
                (e.KeyCode == Keys.End) ||
                (e.KeyCode == Keys.Return) ||
                (e.KeyCode == Keys.Enter) ||
                (e.KeyCode == Keys.Escape) ||
                (e.KeyCode == Keys.Delete) ||
                (e.KeyCode == Keys.ShiftKey) ||
                (e.KeyCode == Keys.Add) ||
                (e.KeyCode == Keys.ControlKey)
                )
            {
                tEventsGrid evg = new tEventsGrid();
                // preparing tGridHint
                vGridHint tGridHint = new vGridHint();

                Control cntrl = null;
                Form tForm = ((DevExpress.XtraEditors.TextEdit)sender).FindForm();
                string TableIPCode = ((DevExpress.XtraEditors.TextEdit)sender).Properties.AccessibleDefaultActionDescription;
                cntrl = t.Find_Control_View(tForm, TableIPCode);

                if (cntrl != null)
                {
                    //evg.getGridHint_(cntrl, ref tGridHint);
                }

                if ((e.KeyCode == Keys.Down) |
                    (e.KeyCode == Keys.Up) |
                    (e.KeyCode == Keys.PageDown) |
                    (e.KeyCode == Keys.PageUp) |
                    (e.KeyCode == Keys.Home) |
                    (e.KeyCode == Keys.End) |
                    (e.KeyCode == Keys.Escape))
                {
                   // evg.gridSpeedKeys(tGridHint, e);
                }


                #region temizle
                /*
                string Prop_Navigator = string.Empty;

                if (cntrl != null)
                {
                    #region GridControl
                    if (cntrl.GetType().ToString() == "DevExpress.XtraGrid.GridControl")
                    {
                        if (((DevExpress.XtraGrid.GridControl)cntrl).MainView != null)
                        {
                            #region GridView
                            if (((DevExpress.XtraGrid.GridControl)cntrl).MainView.GetType().ToString() == "DevExpress.XtraGrid.Views.Grid.GridView")
                            {
                                GridView view = ((DevExpress.XtraGrid.GridControl)cntrl).MainView as GridView;
                                //if (((DevExpress.XtraGrid.GridControl)cntrl).AccessibleDescription != null)
                                //    Prop_Navigator = ((DevExpress.XtraGrid.GridControl)cntrl).AccessibleDescription.ToString();

                                #region
                                if (e.KeyCode == Keys.Down)
                                {
                                    if (view.FocusedRowHandle < view.DataRowCount - 1)
                                    {
                                        view.UnselectRow(view.FocusedRowHandle);
                                        view.FocusedRowHandle += 1;
                                        view.SelectRow(view.FocusedRowHandle);
                                        e.Handled = true;
                                    }
                                }
                                if (e.KeyCode == Keys.Up)
                                {
                                    if (view.FocusedRowHandle > 0)
                                    {
                                        view.UnselectRow(view.FocusedRowHandle);
                                        view.FocusedRowHandle -= 1;
                                        view.SelectRow(view.FocusedRowHandle);
                                        e.Handled = true;
                                    }
                                }
                                if (e.KeyCode == Keys.PageDown)
                                {
                                    if ((view.FocusedRowHandle + 20) < view.DataRowCount - 1)
                                    {
                                        view.UnselectRow(view.FocusedRowHandle);
                                        view.FocusedRowHandle += 20;
                                        view.SelectRow(view.FocusedRowHandle);
                                        e.Handled = true;
                                    }
                                }
                                if (e.KeyCode == Keys.PageUp)
                                {
                                    if ((view.FocusedRowHandle - 20) > 0)
                                    {
                                        view.UnselectRow(view.FocusedRowHandle);
                                        view.FocusedRowHandle -= 20;
                                        view.SelectRow(view.FocusedRowHandle);
                                        e.Handled = true;
                                    }
                                }
                                if (e.KeyCode == Keys.Home)
                                {
                                    if (view.FocusedRowHandle > 0)
                                    {
                                        view.UnselectRow(view.FocusedRowHandle);
                                        view.FocusedRowHandle = 0;
                                        view.SelectRow(view.FocusedRowHandle);
                                        e.Handled = true;
                                    }
                                }
                                if (e.KeyCode == Keys.End)
                                {
                                    if (view.DataRowCount > 0)
                                    {
                                        view.UnselectRow(view.FocusedRowHandle);
                                        view.FocusedRowHandle = view.DataRowCount - 1;
                                        view.SelectRow(view.FocusedRowHandle);
                                        e.Handled = true;
                                    }
                                }
                                
                                if (e.KeyCode == Keys.Enter)
                                    evg.getGridHint_(sender, ref tGridHint);
                                
                                #endregion

                            }
                            #endregion GridView

                            #region AdvBandedGridView
                            if (((DevExpress.XtraGrid.GridControl)cntrl).MainView.GetType().ToString() == "DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView")
                            {
                                AdvBandedGridView view = ((DevExpress.XtraGrid.GridControl)cntrl).MainView as AdvBandedGridView;
                                //if (((DevExpress.XtraGrid.GridControl)cntrl).AccessibleDescription != null)
                                //    Prop_Navigator = ((DevExpress.XtraGrid.GridControl)cntrl).AccessibleDescription.ToString();

                                #region
                                if (e.KeyCode == Keys.Down)
                                {
                                    if (view.FocusedRowHandle < view.DataRowCount - 1)
                                    {
                                        view.UnselectRow(view.FocusedRowHandle);
                                        view.FocusedRowHandle += 1;
                                        view.SelectRow(view.FocusedRowHandle);
                                        e.Handled = true;
                                    }
                                }
                                if (e.KeyCode == Keys.Up)
                                {
                                    if (view.FocusedRowHandle > 0)
                                    {
                                        view.UnselectRow(view.FocusedRowHandle);
                                        view.FocusedRowHandle -= 1;
                                        view.SelectRow(view.FocusedRowHandle);
                                        e.Handled = true;
                                    }
                                }
                                if (e.KeyCode == Keys.PageDown)
                                {
                                    if ((view.FocusedRowHandle + 20) < view.DataRowCount - 1)
                                    {
                                        view.UnselectRow(view.FocusedRowHandle);
                                        view.FocusedRowHandle += 20;
                                        view.SelectRow(view.FocusedRowHandle);
                                        e.Handled = true;
                                    }
                                }
                                if (e.KeyCode == Keys.PageUp)
                                {
                                    if ((view.FocusedRowHandle - 20) > 0)
                                    {
                                        view.UnselectRow(view.FocusedRowHandle);
                                        view.FocusedRowHandle -= 20;
                                        view.SelectRow(view.FocusedRowHandle);
                                        e.Handled = true;
                                    }
                                }
                                if (e.KeyCode == Keys.Home)
                                {
                                    if (view.FocusedRowHandle > 0)
                                    {
                                        view.UnselectRow(view.FocusedRowHandle);
                                        view.FocusedRowHandle = 0;
                                        view.SelectRow(view.FocusedRowHandle);
                                        e.Handled = true;
                                    }
                                }
                                if (e.KeyCode == Keys.End)
                                {
                                    if (view.DataRowCount > 0)
                                    {
                                        view.UnselectRow(view.FocusedRowHandle);
                                        view.FocusedRowHandle = view.DataRowCount - 1;
                                        view.SelectRow(view.FocusedRowHandle);
                                        e.Handled = true;
                                    }
                                }

                                if (e.KeyCode == Keys.Enter)
                                    evg.getGridHint_(sender, ref tGridHint);

                                #endregion
                            }
                            #endregion AdvBandedGridView
                        }
                    }
                    #endregion GridControl

                    #region TreeList
                    if (cntrl.GetType().ToString() == "DevExpress.XtraTreeList.TreeList")
                    {
                        if (((DevExpress.XtraTreeList.TreeList)cntrl).AccessibleDescription != null)
                            Prop_Navigator = ((DevExpress.XtraTreeList.TreeList)cntrl).AccessibleDescription.ToString();

                        if (e.KeyCode == Keys.Down)
                        {
                            //if (((DevExpress.XtraTreeList.TreeList)cntrl).FocusedRowHandle < ((DevExpress.XtraTreeList.TreeList)cntrl).DataRowCount - 1)
                            //{
                            //((DevExpress.XtraTreeList.TreeList)cntrl).FocusedNode.NextNode. //.FocusedRowHandle += 1;
                            e.Handled = true;
                            //}
                        }
                        if (e.KeyCode == Keys.Up)
                        {
                            //if (view.FocusedRowHandle > 0)
                            //{
                            //    view.FocusedRowHandle -= 1;
                            e.Handled = true;
                            //}
                        }
                    }
                    #endregion
                }
                */
                #endregion temizle


                #region Keys.Return & v.search_CARI_ARAMA_TD
                if (((e.KeyCode == Keys.Return) ||
                     (e.KeyCode == Keys.Enter)) &&
                     (v.searchCount == 0))
                {
                    if (v.search_CARI_ARAMA_TD == v.search_inData)
                    {
                        //MessageBox.Show("indata " + TableIPCode);
                        //InData_RunSQL(tForm, TableIPCode, ((DevExpress.XtraEditors.TextEdit)sender).Text, null, null);

                        e.Handled = true;
                    }

                    if (v.search_CARI_ARAMA_TD == v.search_onList)
                    {
                        //MessageBox.Show("onlist");
                    }
                }
                #endregion Keys.Return

                if (e.Control)
                {
                    //evg.commonGridClick(sender, e, tGridHint);
                }

                #region Keys.Return & e.handle = false
                if (((e.KeyCode == Keys.Return) ||
                     (e.KeyCode == Keys.Enter)) &&
                     (e.Handled == false)
                   )
                {
                    v.searchCount = 0;

                    //MessageBox.Show("yeni btnClick işi var.. textEdit_Find_KeyDown / Keys.Return & e.handle = false");
                    //navigatorButtonExec_Keys(tForm, e.KeyCode, TableIPCode, Prop_Navigator);

                    
                    //evg.commonGridClick(sender, e, tGridHint);

                    //e.Handled = true;

                    if (((DevExpress.XtraEditors.TextEdit)sender).Text.IndexOf("Aradığınız") == -1)
                    {
                        //v.searchCount = ((DevExpress.XtraEditors.TextEdit)sender).Text.Length;
                        //((DevExpress.XtraEditors.TextEdit)sender).SelectionStart = v.searchCount + 1;
                    }
                }
                #endregion Keys.Return & e.handle = false

                #region Keys.Delete
                if ((e.KeyCode == Keys.Delete) &&
                    (v.search_CARI_ARAMA_TD == v.search_inData))
                {
                    //Search_New(tForm, TableIPCode, ((DevExpress.XtraEditors.TextEdit)sender).Properties.AccessibleName);

                    e.Handled = true;
                }
                #endregion Keys.Delete

            }

            /*
            if (e.KeyCode == Keys.Return)
            {
                view.StartIncrementalSearch(((DevExpress.XtraEditors.TextEdit)sender).Text);
                view.FocusedColumn = view.Columns["TAMADI"];
            }
            */
        }

        public void textEdit_Find_Enter(object sender, EventArgs e)
        {
            int findType = 0;

            if (sender.GetType().ToString() == "DevExpress.XtraEditors.ButtonEdit")
            {
                findType = (int)((DevExpress.XtraEditors.ButtonEdit)sender).Tag;
                v.searchCount = ((DevExpress.XtraEditors.ButtonEdit)sender).Text.Length;

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
                v.searchCount = ((DevExpress.XtraEditors.TextEdit)sender).Text.Length;
                ((DevExpress.XtraEditors.TextEdit)sender).Properties.Appearance.BackColor = v.AppearanceFocusedColor;
            }
            if (findType == 100) v.search_CARI_ARAMA_TD = v.search_onList;
            if (findType == 200) v.search_CARI_ARAMA_TD = v.search_inData;
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
                    dsData.Tables[0].Clear();
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
                        External_Kriterleri_Uygula(tForm, dsData, alias, newValue, null);

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
                tEventsButton evb = new tEventsButton();

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
                    evb.newData(tForm, Target_TableIPCode);


                    //
                    // Arama Motoru Listesini Temizle 
                    //
                    v.searchCount = 0;
                    //InData_Close(tForm, Search_TableIPCode);
                    //   textEdit_Find_EditValueChanged gittiğinde bu fonk. zaten çalışıyor

                }
            }
        }

        #endregion Search


        #endregion SubFunctions

        #region SUBVIEW 

        // SubView işlemi 3 değişik şekilde çalışmakta
        // 1. PROP_SUBVIEW   : DataNavigator Change üzerinde çalışıyor
        // 2. Buton Click te : PROP_NAVIGATOR üzerinde  tButtonType.btOpenSubView = 125 şeklinde
        // 3. Yine Button Click ExtraIslemler için çalışmaktadır : PROP_NAVIGATOR.TABLEIPCODE_LIST.WORKTYPE üzerinde 

        #region CreateOrSelect TabPage 
        public void CreateOrSelect_Page(Form tForm, string TabControlName, string tabPageCode, string TableIPCode, string readValue, string headCaption, bool show)
        {
            // tabControl_SUBVIEW -- CreateOrSelect_Page

            string TableIPCode_NotDot = t.AntiStr_Dot(TableIPCode);

            // TabPage aranıyor
            Control c = null;

            if (t.IsNotNull(tabPageCode))
            {
                c = t.Find_Control(tForm, tabPageCode);
            }
            
            if (t.IsNotNull(TableIPCode_NotDot))
            {
                c = t.Find_Control(tForm, TableIPCode_NotDot + readValue);
                if (c == null)
                    c = t.Find_Control(tForm, "tTabPage_" + TableIPCode_NotDot + readValue);
            }

            /// SelectedPage
            /// 
            ///     "DevExpress.XtraTab.XtraTabPage"
            ///     "DevExpress.XtraBars.Navigation.NavigationPage"
            ///     "DevExpress.XtraBars.Navigation.TabNavigationPage"
            ///     "DevExpress.XtraBars.Ribbon.BackstageViewClientControl"
            ///     

            #region SelectedPage var ise bulduğu sayfayı select et geri dön

            if ((c != null) && (show))
            {
                #region XtraTabPage
                if (c.GetType().ToString() == "DevExpress.XtraTab.XtraTabPage")
                {
                    // hangi sayfayı gösteriyor ( TableIPCode )
                    ((DevExpress.XtraTab.XtraTabPage)c).TabControl.AccessibleName =
                        ((DevExpress.XtraTab.XtraTabPage)c).AccessibleName;

                    ((DevExpress.XtraTab.XtraTabPage)c).TabControl.SelectedTabPage =
                        ((DevExpress.XtraTab.XtraTabPage)c);
                }
                #endregion

                #region NavigationPage
                if (c.GetType().ToString() == "DevExpress.XtraBars.Navigation.NavigationPage")
                {
                    ((DevExpress.XtraBars.Navigation.NavigationPage)c).Parent.AccessibleName =
                        ((DevExpress.XtraBars.Navigation.NavigationPage)c).AccessibleName;

                    Control cntrl = ((DevExpress.XtraBars.Navigation.NavigationPage)c).Parent;
                    if (cntrl.GetType().ToString() == "DevExpress.XtraBars.Navigation.NavigationPane")
                    {
                        ((DevExpress.XtraBars.Navigation.NavigationPane)cntrl).SelectedPage =
                            ((DevExpress.XtraBars.Navigation.NavigationPage)c);
                    }
                }
                #endregion

                #region TabNavigationPage
                if (c.GetType().ToString() == "DevExpress.XtraBars.Navigation.TabNavigationPage")
                {
                    ((DevExpress.XtraBars.Navigation.TabNavigationPage)c).Parent.AccessibleName =
                        ((DevExpress.XtraBars.Navigation.TabNavigationPage)c).AccessibleName;

                    Control cntrl = ((DevExpress.XtraBars.Navigation.TabNavigationPage)c).Parent;
                    if (cntrl.GetType().ToString() == "DevExpress.XtraBars.Navigation.TabPane")
                    {
                        ((DevExpress.XtraBars.Navigation.TabPane)cntrl).SelectedPage =
                            ((DevExpress.XtraBars.Navigation.TabNavigationPage)c);
                    }
                }
                #endregion

                #region DevExpress.XtraBars.Ribbon.BackstageViewTabItem
                if (c.GetType().ToString() == "DevExpress.XtraBars.Ribbon.BackstageViewClientControl")
                {
                    t.selectBackstageViewTabItem(tForm, TabControlName, "tTabPage_" + TableIPCode_NotDot + readValue, tabPageCode);

                    //((DevExpress.XtraBars.Ribbon.BackstageViewControl)c).Parent.AccessibleName =
                    //    ((DevExpress.XtraBars.Ribbon.BackstageViewControl)c).AccessibleName;

                    //Control cntrl = ((DevExpress.XtraBars.Ribbon.BackstageViewTabItem)c).Parent;
                    //if (cntrl.GetType().ToString() == "DevExpress.XtraBars.Ribbon.BackstageViewControl")
                    //{
                    //    ((DevExpress.XtraBars.Ribbon.BackstageViewControl)cntrl).SelectedPage =
                    //        ((DevExpress.XtraBars.Ribbon.BackstageViewTabItem)c);
                    //}
                }
                #endregion

                return;
            }

            if ((c != null) && (show == false)) 
                return; 

            #endregion

            // ------------------------------------------------------------------------------

            #region // TabPageyi iki şekilde bulabiliriz
            // 1. Form üzerindeki TabPage aranır ve ilk bulunan getirilir
            // 2. TabPage nin MasterKey i belirtilmiş ise direk o TabPage aranır

            string Item_MasterKey = string.Empty;
            string Caption = string.Empty;

            if (t.IsNotNull(TableIPCode_NotDot))
            {
                Item_MasterKey = t.Find_Button_Value(tForm, TableIPCode_NotDot, "=FormCode:");
                Caption = t.Find_Button_Value(tForm, TableIPCode_NotDot, "=Caption:");
            }

            if (t.IsNotNull(headCaption))
                Caption = headCaption;

            // TabControl aranıyor 
            if (t.IsNotNull(Item_MasterKey) == false)
            {
                c = t.Find_Control(tForm, TabControlName);
            }

            if (t.IsNotNull(Item_MasterKey))
            {
                c = t.Find_Control_Tag(tForm, Item_MasterKey);
            }

            if (c == null)
            {
                c = t.Find_Control(tForm, TabControlName);
            }

            #endregion

            /// TabControl or 
            /// TabPane or
            /// NavigationPane or
            /// BackstageViewControl
            /// 
            /// bulunduysa yeni Page oluştur

            if (c != null)
            {
                #region XtraTab.XtraTabControl
                if (c.GetType().ToString() == "DevExpress.XtraTab.XtraTabControl")
                {
                    // yeni TabPage hazırlanıyor
                    DevExpress.XtraTab.XtraTabPage tTabPage = new DevExpress.XtraTab.XtraTabPage();

                    tTabPage.AccessibleName = TableIPCode;
                    tTabPage.AccessibleDescription = TableIPCode_NotDot + "|" + readValue;

                    tTabPage.Name = "tTabPage_" + TableIPCode_NotDot + readValue;
                    tTabPage.Tag = Item_MasterKey;
                    tTabPage.Text = Caption;

                    tTabPage.Padding = new System.Windows.Forms.Padding(v.Padding4);

                    //this.xtraTabControl1.SelectedPageChanged 
                    //      += new DevExpress.XtraTab.TabPageChangedEventHandler(this.xtraTabControl1_SelectedPageChanged);
                    if (((DevExpress.XtraTab.XtraTabControl)c).TabPages.Count <= 1)
                        ((DevExpress.XtraTab.XtraTabControl)c).SelectedPageChanged
                             += new DevExpress.XtraTab.TabPageChangedEventHandler(xtraTabControl_SelectedPageChanged);

                    // Bulunan TabControl ün üzerine yeni bir TabPage ekleniyor
                    ((DevExpress.XtraTab.XtraTabControl)c).AccessibleName = TableIPCode;
                    ((DevExpress.XtraTab.XtraTabControl)c).AccessibleDescription = TableIPCode_NotDot + "|" + readValue;
                    ((DevExpress.XtraTab.XtraTabControl)c).TabPages.Add(tTabPage);
                    if (show) ((DevExpress.XtraTab.XtraTabControl)c).SelectedTabPage = tTabPage;
                }
                #endregion

                #region Navigation.NavigationPane
                if (c.GetType().ToString() == "DevExpress.XtraBars.Navigation.NavigationPane")
                {
                    // yeni TabPage hazırlanıyor
                    DevExpress.XtraBars.Navigation.NavigationPage tTabPage =
                        new DevExpress.XtraBars.Navigation.NavigationPage();

                    tTabPage.AccessibleName = TableIPCode;
                    tTabPage.AccessibleDescription = TableIPCode_NotDot + "|" + readValue;

                    tTabPage.Caption = Caption;
                    tTabPage.Text = Caption;
                    tTabPage.Name = "tTabPage_" + TableIPCode_NotDot + readValue;
                    tTabPage.Tag = Item_MasterKey;

                    //tTabPage.Padding = new System.Windows.Forms.Padding(v.Padding4);

                    ((System.ComponentModel.ISupportInitialize)(((DevExpress.XtraBars.Navigation.NavigationPane)c))).BeginInit();

                    if (((DevExpress.XtraBars.Navigation.NavigationPane)c).Pages.Count <= 1)
                        ((DevExpress.XtraBars.Navigation.NavigationPane)c).SelectedPageChanged
                             += new DevExpress.XtraBars.Navigation.SelectedPageChangedEventHandler(navigationPane_SelectedPageChanged);

                    // Bulunan TabControl ün üzerine yeni bir TabPage ekleniyor
                    ((DevExpress.XtraBars.Navigation.NavigationPane)c).AccessibleName = TableIPCode;
                    ((DevExpress.XtraBars.Navigation.NavigationPane)c).AccessibleDescription = TableIPCode_NotDot + "|" + readValue;
                    ((DevExpress.XtraBars.Navigation.NavigationPane)c).Controls.Add(tTabPage);
                    ((DevExpress.XtraBars.Navigation.NavigationPane)c).Pages.Add((DevExpress.XtraBars.Navigation.NavigationPage)tTabPage);
                    if (show) ((DevExpress.XtraBars.Navigation.NavigationPane)c).SelectedPage = tTabPage;

                    ((System.ComponentModel.ISupportInitialize)(((DevExpress.XtraBars.Navigation.NavigationPane)c))).EndInit();

                }
                #endregion

                #region Navigation.TabPane
                if (c.GetType().ToString() == "DevExpress.XtraBars.Navigation.TabPane")
                {
                    // yeni TabPage hazırlanıyor
                    DevExpress.XtraBars.Navigation.TabNavigationPage tTabPage =
                        new DevExpress.XtraBars.Navigation.TabNavigationPage();

                    tTabPage.AccessibleName = TableIPCode;
                    tTabPage.AccessibleDescription = TableIPCode_NotDot + "|" + readValue;

                    tTabPage.Name = "tTabPage_" + TableIPCode_NotDot + readValue;
                    tTabPage.Tag = Item_MasterKey;
                    tTabPage.Text = Caption;
                    tTabPage.Caption = Caption;
                    //tTabPage.Padding = new System.Windows.Forms.Padding(v.Padding4);

                    ((System.ComponentModel.ISupportInitialize)(((DevExpress.XtraBars.Navigation.TabPane)c))).BeginInit();

                    if (((DevExpress.XtraBars.Navigation.TabPane)c).Pages.Count <= 1)
                        ((DevExpress.XtraBars.Navigation.TabPane)c).SelectedPageChanged
                             += new DevExpress.XtraBars.Navigation.SelectedPageChangedEventHandler(tabPane_SelectedPageChanged);

                    // Bulunan TabControl ün üzerine yeni bir TabPage ekleniyor
                    ((DevExpress.XtraBars.Navigation.TabPane)c).AccessibleName = TableIPCode;
                    ((DevExpress.XtraBars.Navigation.TabPane)c).AccessibleDescription = TableIPCode_NotDot + "|" + readValue;
                    ((DevExpress.XtraBars.Navigation.TabPane)c).Controls.Add(tTabPage);
                    ((DevExpress.XtraBars.Navigation.TabPane)c).Pages.Add((DevExpress.XtraBars.Navigation.TabNavigationPage)tTabPage);
                    if (show) ((DevExpress.XtraBars.Navigation.TabPane)c).SelectedPage = tTabPage;

                    ((System.ComponentModel.ISupportInitialize)(((DevExpress.XtraBars.Navigation.TabPane)c))).EndInit();

                }
                #endregion

                #region DevExpress.XtraBars.Ribbon.BackstageViewControl
                if (c.GetType().ToString() == "DevExpress.XtraBars.Ribbon.BackstageViewControl")
                {
                    DevExpress.XtraBars.Ribbon.BackstageViewClientControl backstageViewClientControl1 = new DevExpress.XtraBars.Ribbon.BackstageViewClientControl();
                    DevExpress.XtraBars.Ribbon.BackstageViewTabItem backstageViewTabItem1 = new DevExpress.XtraBars.Ribbon.BackstageViewTabItem();
                    // 
                    // backstageViewClientControl1
                    // 
                    backstageViewClientControl1.AccessibleName = TableIPCode;
                    backstageViewClientControl1.AccessibleDescription = TableIPCode_NotDot + "|" + readValue;
                    backstageViewClientControl1.Name = "tTabPage_" + TableIPCode_NotDot;
                    backstageViewClientControl1.Size = new System.Drawing.Size(c.Width, c.Height);
                    backstageViewClientControl1.TabIndex = ((DevExpress.XtraBars.Ribbon.BackstageViewControl)c).Controls.Count;
                    backstageViewClientControl1.Padding = new System.Windows.Forms.Padding(v.Padding4);
                    //
                    // backstageViewTabItem1
                    //
                    backstageViewTabItem1.Caption = Caption;
                    backstageViewTabItem1.ContentControl = backstageViewClientControl1;
                    backstageViewTabItem1.Name = "backstageViewTabItem_" + TableIPCode_NotDot;

                    ((DevExpress.XtraBars.Ribbon.BackstageViewControl)c).Items.Add(backstageViewTabItem1);

                    if (show) ((DevExpress.XtraBars.Ribbon.BackstageViewControl)c).SelectedTabIndex =
                        ((DevExpress.XtraBars.Ribbon.BackstageViewControl)c).Items.Count;

                }
                #endregion
            }
        }

        #endregion CreateOrSelect TabPage

        #region subViewExec  // oldName = tSubView_Preparing

        public bool subViewExec(Form tForm,
            string ViewType, string FormCode, string TableIPCode, string tabPageCode,
            string ReadValue, string ReadCaption, string menuValue)
        {
            //tToolBox t = new tToolBox();
            bool onay = false;
            bool show = true;
            if (menuValue == "DONTSHOW") show = false;

            #region TabPage ise
            if ((ViewType == "TabPage") ||
                (ViewType == "TabPage2") ||
                (ViewType == "TabPage3") ||
                (ViewType == "TabPage4") ||
                (ViewType == "TabPage5") ||
                (ViewType == "TabPage6") ||
                (ViewType == "TabPage7") ||
                (ViewType == "TabPage8") ||
                (ViewType == "TabPage9") ||
                (ViewType == "TabPage10") 
                )
            {
                // yukarıda oluşturulan TabPage aranıyor
                Control cntrl = null;
                string[] controls = new string[] { };
                //    "DevExpress.XtraTab.XtraTabPage",
                //    "DevExpress.XtraBars.Navigation.NavigationPage",
                //    "DevExpress.XtraBars.Navigation.TabNavigationPage",
                //    "DevExpress.XtraBars.Ribbon.BackstageViewTabItem"
                //};

                // yok ise yeni TabPage oluşturulacak
                // var ise Show etmesi sağlanacak
                string TabControlName = "";
                if (ViewType == "TabPage") TabControlName = "tabControl_SUBVIEW";
                if (ViewType == "TabPage2") TabControlName = "tabControl_SUBVIEW2";
                if (ViewType == "TabPage3") TabControlName = "tabControl_SUBVIEW3";
                if (ViewType == "TabPage4") TabControlName = "tabControl_SUBVIEW4";
                if (ViewType == "TabPage5") TabControlName = "tabControl_SUBVIEW5";
                if (ViewType == "TabPage6") TabControlName = "tabControl_SUBVIEW6";
                if (ViewType == "TabPage7") TabControlName = "tabControl_SUBVIEW7";
                if (ViewType == "TabPage8") TabControlName = "tabControl_SUBVIEW8";
                if (ViewType == "TabPage9") TabControlName = "tabControl_SUBVIEW9";
                if (ViewType == "TabPage10") TabControlName = "tabControl_SUBVIEW10";


                if (t.IsNotNull(TableIPCode))
                {
                    CreateOrSelect_Page(tForm, TabControlName, "", TableIPCode, ReadValue, ReadCaption, show);
                    cntrl = t.Find_Control(tForm, "tTabPage_" + t.AntiStr_Dot(TableIPCode + ReadValue), "", controls);
                }

                if (t.IsNotNull(FormCode))
                {
                    CreateOrSelect_Page(tForm, TabControlName, "", FormCode, ReadValue, ReadCaption, show);
                    //tTabPage_UST/OMS/FNS/MASRAFKAYDET|
                    if (FormCode.IndexOf("tTabPage_") == -1)
                        cntrl = t.Find_Control(tForm, "tTabPage_" + t.AntiStr_Dot(FormCode), "", controls);
                    if (cntrl == null)
                        cntrl = t.Find_Control(tForm, t.AntiStr_Dot(FormCode), "", controls);
                    if (cntrl == null)
                        cntrl = t.Find_BackstageViewTabItem(tForm, TabControlName, "tTabPage_" + t.AntiStr_Dot(FormCode) + ReadValue);
                }

                if (t.IsNotNull(tabPageCode))
                {
                    CreateOrSelect_Page(tForm, TabControlName, tabPageCode, "", ReadValue, ReadCaption, show);
                    cntrl = t.Find_Control(tForm, tabPageCode, "", controls);
                }


                // TabPage bulunduysa üzerinde istenen IP (InputPanel) oluşturuluyor
                if (cntrl != null)
                {
                    /// TabPage nin içi boş ise InputPanelin içi hazırlanması gerekiyor
                    /// if (((DevExpress.XtraTab.XtraTabPage)cntrl).Controls.Count == 0)
                    /// 
                    if (cntrl.Controls.Count == 0)
                    {
                        // bir Tab page içine  tek TableIPCode et
                        if (t.IsNotNull(TableIPCode))
                            subViewExec_(tForm, ViewType, TableIPCode, ReadValue, ReadCaption, menuValue, cntrl);

                        // bir Tab page içine  birden fazla TableIPCode dizayn et
                        if (t.IsNotNull(FormCode))
                            subViewLayoutExec_(tForm, FormCode, cntrl);

                        onay = true;
                    }
                    else onay = true;
                }

            }
            #endregion TabPage

            #region dockPanel ise
            if (ViewType == "dockPanel")
            {
                subViewExec_(tForm, FormCode, TableIPCode);
            }
            #endregion dockPanel ise

            if (t.IsNotNull(menuValue) && (menuValue != "DONTSHOW"))
                setMenuPage(tForm, menuValue);

            return onay;
        }

        private void subViewExec_(Form tForm, string ViewType, string TableIPCode,
            string ReadValue, string ReadCaption, string menuValue, Control cntrl)
        {
            tInputPanel ip = new tInputPanel();

            #region TabPage
            if ((ViewType == "TabPage") ||
                (ViewType == "TabPage2") ||
                (ViewType == "TabPage3") ||
                (ViewType == "TabPage4") ||
                (ViewType == "TabPage5") ||
                (ViewType == "TabPage6") ||
                (ViewType == "TabPage7") ||
                (ViewType == "TabPage8") ||
                (ViewType == "TabPage9") ||
                (ViewType == "TabPage10")
                )
            {
                ip.Create_InputPanel(tForm, cntrl, TableIPCode, ReadValue, 1, true);

                if (t.IsNotNull(ReadCaption))
                    cntrl.Text = "   " + ReadCaption + "   ";

                // 'DEFAULT_TYPE', 22, '', 'Source ParentControl.Tag READ'); 
                //  Viewin içinde bulunduğu control ün tag ındaki value yi okumak için
                // 
                if ((ReadValue != "") && (ReadCaption != ""))
                {
                    cntrl.Tag = ReadValue + "||" + ReadCaption + "||" + menuValue;
                    //
                    v.con_Source_ParentControl_Tag_Value = ReadValue + "||" + ReadCaption + "||" + menuValue;
                }
                // tabPage ile RibbonPage bağlantısını sağlamak için gerekiyor
                // tabPege değiştiğinde RibbonPagede ona göre değişsin 
                if (t.IsNotNull(menuValue) && (menuValue != "DONTSHOW"))
                    cntrl.AccessibleDefaultActionDescription = menuValue;

                //tTabPage_FNLNVOL_FNLNVOL_BNL0123
                //FNLNVOL.FNLNVOL_BNL01|23   grid.

                vSubWork vSW = new vSubWork();
                vSW._01_tForm = tForm;
                vSW._02_TableIPCode = TableIPCode;
                vSW._03_WorkTD = v.tWorkTD.NewAndRef;
                vSW._04_WorkWhom = v.tWorkWhom.Only;
                tSubWork_(vSW);
            }
            #endregion TabPage

            /* İPTAL
            #region TabPage2
            if (ViewType == "TabPage2")
            {
                SplitContainer splitContainer1 = new System.Windows.Forms.SplitContainer();
                // 
                // splitContainer1
                // 
                splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
                splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
                splitContainer1.Location = new System.Drawing.Point(0, 0);
                splitContainer1.Name = "splitContainer1";
                splitContainer1.Size = new System.Drawing.Size(958, 308);
                splitContainer1.SplitterDistance = 195;
                splitContainer1.TabIndex = 0;

                ((DevExpress.XtraTab.XtraTabPage)cntrl).Controls.Add(splitContainer1);

                ip.Create_InputPanel(tForm, splitContainer1.Panel1, TableIPCode, 2);
                ip.Create_InputPanel(tForm, splitContainer1.Panel2, TableIPCode, 1);

                ((DevExpress.XtraTab.XtraTabPage)cntrl).Text = splitContainer1.Panel2.Text;
            }
            #endregion TabPage2
            */
        }

        private void subViewExec_(Form tForm, string FormCode, string TableIPCode)
        {
            // yukarıda oluşturulan TabPage aranıyor
            Control cntrl = null;
            string[] controls = new string[] { "DevExpress.XtraBars.Docking.DockPanel" };
            cntrl = t.Find_Control(tForm, FormCode, "", controls);

            // TabPage bulunduysa üzerinde istenen IP (InputPanel) oluşturuluyor
            if (cntrl != null)
            {
                // dockPanel boş ise InputPanelin içi hazırlanması gerekiyor
                if (((DevExpress.XtraBars.Docking.DockPanel)cntrl).Controls.Count == 1)
                {
                    tInputPanel ip = new tInputPanel();
                    ip.Create_InputPanel(tForm, ((DevExpress.XtraBars.Docking.DockPanel)cntrl), TableIPCode, 1, true);
                }
            }
        }

        private void subViewLayoutExec_(Form tForm, string FormCode, Control tabPageControl)
        {
            tLayout l = new tLayout();
            l.Create_Layout(tForm, FormCode, tabPageControl);

            vSubWork vSW = new vSubWork();
            vSW._01_tForm = tForm;
            vSW._02_TableIPCode = "";
            vSW._03_WorkTD = v.tWorkTD.NewAndRef;
            vSW._04_WorkWhom = v.tWorkWhom.All;
            vSW._05_tabPageControl = tabPageControl;

            tSubWork_(vSW);
        }

        #endregion
                
        #region SubView1 : PROP_SUBVIEW : Burası 1 numaralı işlem
        private void propSubView(Form tForm, DataSet dsData, DevExpress.XtraEditors.DataNavigator tDataNavigator)
        {
            string subView = tDataNavigator.AccessibleDescription;
            //  "SV_ENABLED": "TRUE"
            string s = (char)34 + "SUBVIEW_ENABLED" + (char)34 + ": " + (char)34 + "TRUE" + (char)34;
            if (subView.IndexOf(s) > -1)
            {
                propSubView_(tForm, dsData, tDataNavigator);
            }
        }
        private void propSubView_(Form tForm, DataSet dsData, DevExpress.XtraEditors.DataNavigator tDataNavigator)
        {
            if (t.IsNotNull(dsData) == false) return;
            string readValue = "";
            string readValue2 = "";
            PROP_SUBVIEW prop_ = propSubViewReadValue(dsData, tDataNavigator, ref readValue, ref readValue2);
            if (prop_ == null) return;

            List<SUBVIEW_LIST> subViewList = prop_.SUBVIEW_LIST;
            string subViewType = prop_.SUBVIEW_TYPE.ToString();

            bool onay = false;
            string TableIPCode = "";
            string FormCode = "";
            string showMenuPageName = "";
            string elseShowTableIPCode = "";
            string elseShowMenuPageName = "";
            foreach (var item in subViewList)
            {
                if ((item.SUBVIEW_VALUE == "ELSE"))
                {
                    elseShowTableIPCode = item.SUBVIEW_TABLEIPCODE.ToString();
                    elseShowMenuPageName = item.SHOWMENU_PAGENAME.ToString();
                }
                if ((item.SUBVIEW_VALUE.IndexOf(readValue) > -1 && readValue.Length > 0) ||
                    (item.SUBVIEW_VALUE.IndexOf(readValue2) > -1 && readValue2.Length > 0))
                {
                    TableIPCode = item.SUBVIEW_TABLEIPCODE.ToString();
                    if (item.SUBVIEW_FORMCODE != null)
                        FormCode = item.SUBVIEW_FORMCODE.ToString();

                    if (t.IsNotNull(TableIPCode))
                    {
                        /// PageName var ise bir menunün (Ribbon un) page ismidir ve bu page show edilececek
                        /// Show Menu Page Name
                        showMenuPageName = item.SHOWMENU_PAGENAME.ToString();

                        onay = subViewExec(tForm, subViewType, "", TableIPCode, "", "", "", showMenuPageName);
                        break;
                    }
                    
                    if (t.IsNotNull(FormCode))
                    {
                        onay = subViewExec(tForm, subViewType, FormCode, "", "", "", item.CAPTION.ToString(), "");
                    }

                    /// tabPage in CmpName yazdığını kod ile tetiklenmesini sağlayan table.Rows.Column.value eşleşiyorsa çalışacak
                    ///
                    if (t.IsNotNull(item.SUBVIEW_VALUE) &&
                        t.IsNotNull(TableIPCode) == false &&
                        t.IsNotNull(FormCode) == false
                        )
                    {
                        string subViewValue = item.SUBVIEW_VALUE;
                        onay = subViewExec(tForm, subViewType, "", "", subViewValue, "", "", "");
                        break;
                    }

                }
            }

            if (onay == false)
            {
                onay = subViewExec(tForm, subViewType, "", elseShowTableIPCode, "", "", "", elseShowMenuPageName);
            }

        }

        private PROP_SUBVIEW propSubViewReadValue(DataSet dsData, DevExpress.XtraEditors.DataNavigator tDataNavigator, ref string readValue, ref string readValue2)
        {
            string subView = tDataNavigator.AccessibleDescription;
            PROP_SUBVIEW prop_ = t.readProp<PROP_SUBVIEW>(subView);

            string subViewKeyFName = prop_.SUBVIEW_KEYFNAME.ToString();
            string subViewKeyFName2 = "";

            subViewKeyFName2 = prop_.SUBVIEW_KEYFNAME2?.ToString();

            //string subViewCaptionFName = prop_.SUBVIEW_CAPTION_FNAME.ToString();
            //string subViewCmpType = prop_.SUBVIEW_CMP_TYPE.ToString();
            //string subViewCmpLocation = prop_.SUBVIEW_CMP_LOCATION.ToString();
            //string subViewType = prop_.SUBVIEW_TYPE.ToString();
            //List<SUBVIEW_LIST> subViewList = prop_.SUBVIEW_LIST;

            if (t.IsNotNull(subViewKeyFName))
            {
                // SUBVIEW ilk okunduğunda ve yine ilk önce hangi position gitmesi gerekiyorsa o sağlanıyor
                if ((v.con_SubView_FIRST_POSITION > -1) &&
                    (v.con_SubView_FIRST_POSITION != tDataNavigator.Position))
                {
                    // burası hata veriyor bu nedenle kapattım
                    // ilk position değişikliğinde gereken view çalışmıyordu

                    //tDataNavigator.Position = v.con_SubView_FIRST_POSITION;
                    
                    v.con_SubView_FIRST_POSITION = -1;
                }

                //Now_Value = detail_Row[SV_KeyFName].ToString();
                if (tDataNavigator.Position > -1)
                {
                    try
                    {
                        readValue = dsData.Tables[0].Rows[tDataNavigator.Position][subViewKeyFName].ToString();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message.ToString());
                    }
                }
            }

            if (t.IsNotNull(subViewKeyFName2))
            {
                //Now_Value = detail_Row[SV_KeyFName].ToString();
                if (tDataNavigator.Position > -1)
                {
                    try
                    {
                        readValue2 = dsData.Tables[0].Rows[tDataNavigator.Position][subViewKeyFName2].ToString();
                    }
                    catch //(Exception e)
                    {
                        //MessageBox.Show(e.Message.ToString());
                    }
                }

            }

            return prop_;
        }

        #endregion SubView1 : PROP_SUBVIEW

        #endregion SUBVIEW



        #region tExtraCreateView // burası OLD
        public void tExtraCreateView(Form tForm) // burası OLD
        {
            MessageBox.Show("Bu procedureyi kontrol et : tExtraCreateView() ");

            string form_prp = t.Set(t.myFormBox_Values(tForm, v.FormLoad), v.con_FormLoadValue, string.Empty);

            if (t.IsNotNull(form_prp) == false) return;

            string s = form_prp;

            if (s.IndexOf("TABLEIPCODE_LIST") > -1)
            {
                string TABLEIPCODE = string.Empty;
                string TABLEALIAS = string.Empty;
                string KEYFNAME = string.Empty;
                string MSETVALUE = string.Empty;
                string WORKTYPE = string.Empty;
                string CONTROLNAME = string.Empty;

                string row_block = string.Empty;
                string lockE = "=ROWE_";

                while (s.IndexOf(lockE) > -1)
                {
                    row_block = t.Find_Properies_Get_RowBlock(ref s, "TABLEIPCODE_LIST");

                    if (row_block.IndexOf("CREATEVIEW") > -1)
                    {
                        TABLEIPCODE = t.MyProperties_Get(row_block, "TABLEIPCODE:");
                        CONTROLNAME = t.MyProperties_Get(row_block, "CONTROLNAME:");

                        if (t.IsNotNull(CONTROLNAME))
                        {
                            Control cntrl = null;
                            string[] controls = new string[] { };
                            cntrl = t.Find_Control(tForm, CONTROLNAME, "", controls);

                            if (cntrl != null)
                            {
                                tInputPanel ip = new tInputPanel();
                                ip.Create_InputPanel(tForm, cntrl, TABLEIPCODE, 1, true);
                            } // if (cntrl != 
                            else
                            {
                                MessageBox.Show("DİKKAT : ExtraCreateView için " + CONTROLNAME + " isimli kontrol bulunamadı");
                            }
                        } // if (t.IsNotNull(
                    } //if (row_block.IndexOf 
                } //while ( 
            } // if (s

            //v.con_FormLoadValue = string.Empty; 
        }
        
        #endregion
                

        #region ...SelectedPageChanged

        private void xtraTabControl_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
        {
            DevExpress.XtraTab.XtraTabPage page =
              ((DevExpress.XtraTab.XtraTabControl)sender).SelectedTabPage;

            if (page == null) return;

            if (page.AccessibleDefaultActionDescription != null)
            {
                Form tForm = ((DevExpress.XtraTab.XtraTabControl)sender).FindForm();

                setMenuPage(tForm, page.AccessibleDefaultActionDescription.ToString());
            }
        }

        private void navigationPane_SelectedPageChanged(object sender, DevExpress.XtraBars.Navigation.SelectedPageChangedEventArgs e)
        {
            if (((DevExpress.XtraBars.Navigation.NavigationPage)e.Page).AccessibleDefaultActionDescription != null)
            {
                Form tForm = ((DevExpress.XtraBars.Navigation.NavigationPane)sender).FindForm();
                setMenuPage(tForm, ((DevExpress.XtraBars.Navigation.NavigationPage)e.Page).AccessibleDefaultActionDescription.ToString());
            }
        }

        private void tabPane_SelectedPageChanged(object sender, DevExpress.XtraBars.Navigation.SelectedPageChangedEventArgs e)
        {
            /*DevExpress.XtraBars.Navigation.TabNavigationPage page =
              ((DevExpress.XtraBars.Navigation.TabPane)sender).SelectedPage;
            
            if (page.AccessibleDefaultActionDescription != null)
            {
                Form tForm = ((DevExpress.XtraBars.Navigation.TabPane)sender).FindForm();

                setMenuPage(tForm, page.AccessibleDefaultActionDescription.ToString());
            }*/
            if (((DevExpress.XtraBars.Navigation.TabNavigationPage)e.Page).AccessibleDefaultActionDescription != null)
            {
                Form tForm = ((DevExpress.XtraBars.Navigation.TabPane)sender).FindForm();
                setMenuPage(tForm, ((DevExpress.XtraBars.Navigation.TabNavigationPage)e.Page).AccessibleDefaultActionDescription.ToString());
            }
        }

        #endregion ...SelectedPageChanged

        #region skinGalery
        public void skinPaletteRibbonGalleryBarItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //
            //MessageBox.Show("skinPaletteRibbonGalleryBarItem_ItemClick");
        }

        public void skinPaletteRibbonGalleryBarItem_GalleryItemClick(object sender, DevExpress.XtraBars.Ribbon.GalleryItemClickEventArgs e)
        {
            //
            //MessageBox.Show("skinPaletteRibbonGalleryBarItem_GalleryItemClick");
        }

        public void skinRibbonGalleryBarItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //
            //MessageBox.Show("skinRibbonGalleryBarItem_ItemClick");
        }

        public void skinRibbonGalleryBarItem_GalleryItemClick(object sender, DevExpress.XtraBars.Ribbon.GalleryItemClickEventArgs e)
        {
            //
            //MessageBox.Show("skinRibbonGalleryBarItem_GalleryItemClick");
            //MessageBox.Show(UserLookAndFeel.Default.SkinName.ToString()); //ActiveSkinName.ToString());
            //MessageBox.Show(e.Item.Value.ToString());



        }

        public void skinDropDownButtonItem_DownChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //
            //MessageBox.Show("skinDropDownButtonItem_DownChanged");
        }

        public void skinDropDownButtonItem_HyperlinkClick(object sender, DevExpress.Utils.HyperlinkClickEventArgs e)
        {
            //
            //MessageBox.Show("skinDropDownButtonItem_HyperlinkClick");
        }

        public void skinDropDownButtonItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //
            //MessageBox.Show("skinDropDownButtonItem_ItemClick");
        }
        #endregion

        #region tSubWork.. 

        /// new sub functions 
        /// -------------------------------------------------
        /// tForm
        /// TableIPCode
        /// WorkTD = NewData, Refresh
        /// WorkWhom = All, Only, Childs
        ///
        public void tSubWork_(vSubWork vSW)
        {
            if (v.con_ExtraChange == true) return;
            
            tEventsGrid evg = new tEventsGrid();
            tEventsButton evb = new tEventsButton();

            #region Tanımlar

            bool onay = true;
            bool onay2 = true;
            bool tBreak = false;
            bool old_PositionChange = false;
            string TableIPCode = string.Empty;
            string myProp = string.Empty;
            string AutoInsert = string.Empty;
            string DataFind = string.Empty;
            string DetailSubDetail = string.Empty;
            string DataCopyCode = string.Empty;
            string SqlFirst = string.Empty;
            string SqlSecond = string.Empty;
            string Prop_SubView = string.Empty;
            string activeTableIPCode = string.Empty;
            byte btnType = 0;
            string tabPageName = string.Empty;

            Form tForm = vSW._01_tForm;
            
            /// Eğer bir TableIPCode gelmişse
            /// ya kendisi için işlem yapılacak, 
            /// ya da kendisine bağlı (Master-Detail ile) childs ları
            string targetTableIPCode = vSW._02_TableIPCode;

            if (vSW._05_tabPageControl != null)
               tabPageName = vSW._05_tabPageControl.Name.ToString();


            #endregion Tanımlar

            #region DataNavigator Listesi Hazırlanıyor

            List<string> list = new List<string>();
            t.Find_DataNavigator_List(tForm, ref list, tabPageName);

            Control btn = null;
            Control cntrl = new Control();
            string[] controls = new string[] { "DevExpress.XtraEditors.DataNavigator" };
            string[] controls2 = new string[] { "DevExpress.XtraEditors.SimpleButton" };

            // formun keydown ununda new işlemi başlatılmışsa 
            if (v.formLastActiveControl != null)
            {
                if (v.formLastActiveControl.AccessibleName != null)
                {
                    activeTableIPCode = v.formLastActiveControl.AccessibleName.ToString();
                }
                else
                {
                    // datalayout içindeki nesnelere odaklanınca
                    //
                    if (v.formLastActiveControl.AccessibleDefaultActionDescription != null)
                    {
                        string s = v.formLastActiveControl.AccessibleDefaultActionDescription.ToString();

                        activeTableIPCode = t.Get_And_Clear(ref s, "||");
                    }
                }
            }
            //AccessibleName = "SYSPROJ.SYSPROJ_L01"

            #endregion

            #region DataNavigator Listesi
            
            v.con_SubWork_Run = true;
            
            foreach (string value in list)
            {
                cntrl = t.Find_Control(tForm, value, "", controls);
                if (cntrl != null)
                {
                    onay = true;
                    DataNavigator dN = (DevExpress.XtraEditors.DataNavigator)cntrl;

                    if ((dN.Name.IndexOf("CatList") > -1) ||
                        (dN.Name.IndexOf("CatDetail") > -1))
                    {
                        onay = false;
                    }

                    TableIPCode = dN.AccessibleName.ToString();

                    if (targetTableIPCode != "")
                    {
                        if ((vSW._04_WorkWhom == v.tWorkWhom.Childs) &&
                            (targetTableIPCode != TableIPCode)) onay = false;

                        if ((vSW._04_WorkWhom == v.tWorkWhom.Only) &&
                            (targetTableIPCode != TableIPCode)) onay = false;
                    }

                    if (dN.DataSource == null)
                        onay = false;

                    if (onay)
                    {
                        object tDataTable = dN.DataSource;
                        DataSet dsData = ((DataTable)tDataTable).DataSet;

                        myProp = dsData.Namespace;
                        AutoInsert = t.MyProperties_Get(myProp, "AutoInsert:");
                        DataFind = t.MyProperties_Get(myProp, "DataFind:");
                        DetailSubDetail = t.MyProperties_Get(myProp, "DetailSubDetail:");
                        DataCopyCode = t.MyProperties_Get(myProp, "DataCopyCode:");
                        SqlFirst = t.MyProperties_Get(myProp, "SqlFirst:");
                        SqlSecond = t.MyProperties_Get(myProp, "SqlSecond:");
                        Prop_SubView = t.MyProperties_Get(myProp, "Prop_SubView:");

                        #region tWorkTD.Save - tDataSave 
                        if (vSW._03_WorkTD == v.tWorkTD.Save)
                        {
                            //
                            using (tSave sv = new tSave())
                            {
                                //
                                old_PositionChange = v.con_PositionChange;
                                v.con_PositionChange = true;
                                // saveden onay gelmezse işlemi yarıda kes
                                tBreak = sv.tDataSave(tForm, TableIPCode);
                                //
                                if (old_PositionChange == false)
                                    v.con_PositionChange = false;

                                // işlemi yarıda bırak diğer IPlerin saveleri için uğraşma
                                if (tBreak == false)
                                {
                                    //MessageBox.Show("bekle");
                                    break;
                                }
                            }
                        }
                        #endregion tDataSave

                        #region Refresh_SubDetail
                        if ((vSW._03_WorkTD == v.tWorkTD.Refresh_SubDetail) ||
                            (vSW._03_WorkTD == v.tWorkTD.NewAndRef))
                        {
                            tSubWork_Refresh_(tForm, dsData, dN);

                            if (t.IsNotNull(Prop_SubView))
                            {
                                if (dN.AccessibleDescription != null)
                                    propSubView(tForm, dsData, dN);
                            }

                        }
                        #endregion Refresh_SubDetail

                        #region NewData
                        if ((vSW._03_WorkTD == v.tWorkTD.NewData) ||
                            (vSW._03_WorkTD == v.tWorkTD.NewAndRef))
                        {
                            btn = null;
                            btnType = 0;

                            if (t.IsNotNull(v.con_Source_ParentControl_Tag_Value))
                                Source_ParentControl_Tag_Value(tForm, TableIPCode);

                            // eğer data var ise 
                            if (dsData.Tables[0].Rows.Count > 0)
                            {
                                if (vSW._03_WorkTD == v.tWorkTD.NewData)
                                {
                                    btn = t.Find_Control(tForm, "simpleButton_yeni_kart", TableIPCode, controls2);
                                    if (btn == null)
                                    {
                                        btn = t.Find_Control(tForm, "simpleButton_yeni_hesap", TableIPCode, controls2);
                                        btnType = 1;
                                    }
                                    if (btn == null)
                                    {
                                        btn = t.Find_Control(tForm, "simpleButton_yeni_belge", TableIPCode, controls2);
                                        btnType = 1;
                                    }
                                    if (btn == null)
                                    {
                                        btn = t.Find_Control(tForm, "simpleButton_yeni_alt_hesap", TableIPCode, controls2);
                                        btnType = 1;
                                    }
                                    if (btn == null)
                                    {
                                        btn = t.Find_Control(tForm, "simpleButton_kaydet_yeni", TableIPCode, controls2);
                                        btnType = 1;
                                    }

                                    if (btn == null)
                                    {
                                        btn = t.Find_Control(tForm, "simpleButton_yeni_kart_satir", TableIPCode, controls2);
                                        btnType = 2;
                                    }
                                    if (btn == null)
                                    {
                                        btn = t.Find_Control(tForm, "simpleButton_yeni_hesap_satir", TableIPCode, controls2);
                                        btnType = 2;
                                    }
                                    if (btn == null)
                                    {
                                        btn = t.Find_Control(tForm, "simpleButton_yeni_belge_satir", TableIPCode, controls2);
                                        btnType = 2;
                                    }
                                    if (btn == null)
                                    {
                                        btn = t.Find_Control(tForm, "simpleButton_yeni_alt_hesap_satir", TableIPCode, controls2);
                                        btnType = 2;
                                    }

                                    if (btn != null)
                                    {
                                        // activeTableIPCode var ise sadece doğru IPCode gelince çalışsın
                                        // activeTableIPCode yok ise tüm bulduğu IPCode ler çalışsın
                                        if (t.IsNotNull(activeTableIPCode))
                                        {
                                            if (activeTableIPCode == TableIPCode)
                                            {
                                                //
                                                evb.preparingNewData(tForm, TableIPCode);
                                                //
                                                if (btnType == 1)
                                                {
                                                    tSave sv = new tSave();
                                                    if (sv.tDataSave(tForm, TableIPCode))
                                                    {
                                                        //t.ButtonEnabledAll(tForm, TableIPCode, true);
                                                        evb.newData(tForm, TableIPCode);

                                                        /*
                                                        Control viewCntrl = t.Find_Control_View(tForm, TableIPCode);

                                                        if (viewCntrl != null)
                                                        {
                                                            //
                                                            v.formLastActiveControl = viewCntrl; // ((DevExpress.XtraGrid.GridControl)viewCntrl);

                                                            if (viewCntrl.GetType().ToString() == "DevExpress.XtraGrid.GridControl")
                                                            {
                                                                GridView view = ((DevExpress.XtraGrid.GridControl)viewCntrl).MainView as GridView;

                                                                vGridHint tGridHint = new vGridHint();
                                                                evg.getGridHint_(view, ref tGridHint);

                                                                view.FocusedColumn = view.VisibleColumns[0];

                                                                tGridHint.currentColumn = view.FocusedColumn;

                                                                if (view.FocusedColumn.ReadOnly)
                                                                    view.FocusedColumn = evg.GetNextFocusableColumn(tGridHint);

                                                                evg.gridShowEditor(tGridHint);
                                                            }
                                                        }
                                                        */
                                                    }
                                                }
                                                if (btnType == 2)
                                                {
                                                    evb.newData(tForm, TableIPCode);

                                                    // sorun çıkarırsa burayı kapat
                                                    //
                                                    AutoFormFirstControlFocus(tForm);
                                                    //---
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //
                                            evb.preparingNewData(tForm, TableIPCode);
                                            //
                                            evb.newData(tForm, TableIPCode);

                                            // view e focus la
                                            Control viewCntrl = t.Find_Control_View(tForm, TableIPCode);

                                            if (viewCntrl != null)
                                            {
                                                //
                                                v.formLastActiveControl = viewCntrl;// ((DevExpress.XtraGrid.GridControl)viewCntrl);
                                            }
                                            else AutoFormFirstControlFocus(tForm);
                                            //---
                                        }
                                    }
                                }

                            }
                            else
                            // data yoksa Yeni buton işlemi gerçekleştir 
                            // if (dsData.Tables[0].Rows.Count == 0)
                            {
                                // bu tablolar Master-Detail ile bir yere bağlı olmayan tablolardır
                                // DataFind   /* 0= Find yok, 1. standart, 2. List&Data   */

                                if ((DataFind == "0") &&
                                    (AutoInsert == "True"))
                                {
                                    old_PositionChange = v.con_PositionChange;
                                    v.con_PositionChange = true;

                                    ///
                                    evb.newData(tForm, TableIPCode);

                                    if (old_PositionChange == false)
                                        v.con_PositionChange = false;

                                    /// DataCopyCode bilgisi dragdrop sırasında 
                                    /// ( work_type = 5 ) sırasında atanıyor
                                    /// 
                                    #region
                                    if (t.IsNotNull(DataCopyCode))
                                    {
                                        // tDC_Run gidince oradaki 
                                        // if ((work_type == 3) && (v.con_DragDropEdit == false))  işlemine başlamasın
                                        v.con_DragDropEdit = true;
                                        //***

                                        /// şimdi buda nerden çıktı deme
                                        /// dataCopy çalışınca targetIP için kendi üzerindeki bakar 
                                        /// fakat aynı dataCopy i birden fazla formlar ve onların üzerindeki targetIP ler içinde kullanabiliriz
                                        /// bu nedenle yeni açılan formun üzerindeki tableIP alırız TargetIP yaparız
                                        /// böylelikle bir dataCopy tanımlarıyla birden fazla form (work_type = 5) için kullanmış oluruz :) nasıl ?
                                        /// 

                                        v.con_DragDropOpenFormInTableIPCode = TableIPCode;

                                        tDataCopy dc = new tDataCopy();
                                        dc.tDC_Run(tForm, DataCopyCode);

                                        v.con_DragDropEdit = false;
                                        //***
                                    }
                                    #endregion
                                }
                            }

                            //= DetailSubDetail:True;
                            //About_Detail_SubDetail:
                            //= Detail_SubDetail:AVI_DOS.AVI_DOS_05 || ID ||[AVI_DCK].ICRA_DOSYA_ID || 56 || 31 || 0 |||||| 578 | ds |;

                        }
                        #endregion NewData

                        #region Refresh_Data
                        if ((vSW._03_WorkTD == v.tWorkTD.Refresh_Data) ||
                            (vSW._03_WorkTD == v.tWorkTD.Refresf_DataYilAy))
                        {
                            onay2 = true;

                            // :@@YILAY işareti yoksa boşuna çalışmasın
                            if (vSW._03_WorkTD == v.tWorkTD.Refresf_DataYilAy)
                            {
                                if (SqlFirst.IndexOf(":@@YILAY") == -1 &&
                                    SqlFirst.IndexOf(":DONEM_YILAY") == -1 &&
                                    SqlFirst.IndexOf("DonemTipiId") == -1
                                    )   onay2 = false;
                            
                                if (SqlSecond.IndexOf(v.DONEMTIPI_YILAY_OLD.ToString()) > -1)
                                {
                                    string newSqlSecond = SqlSecond.Replace(v.DONEMTIPI_YILAY_OLD.ToString(), v.DONEMTIPI_YILAY.ToString());
                                    myProp = myProp.Replace(SqlSecond, newSqlSecond);
                                    dsData.Namespace = myProp;
                                }
                            }

                            if (onay2)
                            {
                                t.TableRefresh(tForm, dsData, TableIPCode);
                                // 15.5.2018
                                t.ViewControl_Enabled(tForm, dsData, TableIPCode);
                                // bu IPCode bağlı ExternalIPCode olabilir...
                                t.ViewControl_Enabled_ExtarnalIP(tForm, dsData);
                            }
                        }
                        #endregion 

                    }// if onay

                } // if cntrl != null
            }//foreach

            v.con_SubWork_Run = false;

            if (v.formLastActiveControl != null)
                if (v.formLastActiveControl.GetType().ToString().IndexOf("Control") == -1)
                    tForm.ActiveControl = v.formLastActiveControl;
            
            #endregion DataNavigator Listesi

        }
                
        public void tSubWork_Refresh_(Form tForm, DataSet dsData, DataNavigator dN)
        {
            /// SubDetail_TableIPCode
            /// 
            /// Master-Detail ile bu tabloya bağlı olan  
            /// SubDetail listesi sırayla refresh olacak

            if (v.con_SubWork_Refresh == false) return;

            string SubDetail_List = dN.Text;

            if ((SubDetail_List == string.Empty) ||
                (SubDetail_List == "")) return;

            if (tForm == null)
                tForm = t.Find_Form(dN);

            int position = dN.Position;
            string SubDetail_TableIPCode = string.Empty;
            string TableIPCode = dN.AccessibleName;

            //v.sayac++;
            //Application.OpenForms[0].Text = Application.OpenForms[0].Text + ";" + v.sayac.ToString();// + ";" + TableIPCode;

            v.con_PositionChange = true;

            // =MaliDonemChanger:UST/MEB/MTS/TakipFormuAday||DonemTipiId||;
            // =SubDetail_TableIPCode:UST / MEB / MtskAdayTakipSonuc.eSinav;
            //
            if (SubDetail_List.IndexOf("MaliDonemChanger") > -1)
            {
                string sil = "";
                sil = t.Get_And_Clear(ref SubDetail_List, ";");
            }
            while (SubDetail_List.IndexOf("SubDetail_TableIPCode:") > -1)
            {
                SubDetail_TableIPCode = t.Get_And_Clear(ref SubDetail_List, ";");
                SubDetail_TableIPCode = t.MyProperties_Get(SubDetail_TableIPCode, "SubDetail_TableIPCode:");

                tSubDetail_Read(tForm, dsData, dN, TableIPCode, SubDetail_TableIPCode);
            }

            v.con_PositionChange = false;
        }

        #region Detail_SubDetail_Read

        public bool tSubDetail_Refresh(DataSet ds_Master, DataNavigator dN_Master)
        {

            Form tForm = t.Find_Form(dN_Master);
            int position = dN_Master.Position;
            bool onay = false;

            string SubDetail_List = dN_Master.Text;
            string SubDetail_TableIPCode = string.Empty;
            string mst_TableIPCode = dN_Master.AccessibleName;
            
            // Master/Detail Tabloya bağlı olan  SubDetail listesi sırayla refresh olacak
            while (SubDetail_List.IndexOf("SubDetail_TableIPCode:") > -1)
            {
                SubDetail_TableIPCode = t.Get_And_Clear(ref SubDetail_List, ";");
                SubDetail_TableIPCode = t.MyProperties_Get(SubDetail_TableIPCode, "SubDetail_TableIPCode:");

                onay = tSubDetail_Read(tForm, ds_Master, dN_Master, mst_TableIPCode, SubDetail_TableIPCode);
            }

            SubDetail_List = dN_Master.Text;
            if (SubDetail_List.IndexOf("MaliDonemChanger:null") == -1)
            {
                string menuCodeAndFieldName = t.Get_And_Clear(ref SubDetail_List, ";");
                menuCodeAndFieldName = t.MyProperties_Get(menuCodeAndFieldName, "MaliDonemChanger:");
                string menuCode = t.Get_And_Clear(ref menuCodeAndFieldName, "||");
                string fieldName = t.Get_And_Clear(ref menuCodeAndFieldName, "||");
                maliDonemChanger(tForm, ds_Master, dN_Master, menuCode, fieldName);
            }

            return onay;
        }

        private void maliDonemChanger(Form tForm, DataSet ds, DataNavigator dN, string menuCode, string fieldName)
        {
            if (t.IsNotNull(ds))
            {
                int pos = dN.Position;
                if (pos == -1 && ds.Tables[0].Rows.Count > 0)
                    pos = 0;
                    
                if (pos > -1 && t.IsNotNull(fieldName))
                {
                    tEventsMenu evm = new tEventsMenu();
                    v.DONEMTIPI_YILAY_OLD = v.DONEMTIPI_YILAY;
                    int value =  t.myInt32(ds.Tables[0].Rows[pos][fieldName].ToString());
                    string caption = evm.getYilAyCaption(value);
                    v.DONEMTIPI_YILAY = value;
                    evm.setYilAyCaption(tForm, v.DONEMTIPI_YILAY, caption, "MENU_" + menuCode, t);
                    //evm.yilAyFormRefresh(tForm);
                }
            }
        }

        public bool tSubDetail_Read(Form tForm,
            DataSet ds_Master,
            DataNavigator dN_Master,
            string mst_TableIPCode,
            string SubDetail_TableIPCode)
        {
            bool onay = false;

            tEventsButton evb = new tEventsButton();
                        
            #region Tanımlar
            // yeniden okunacak SubDetail dataseti bul
            DataSet dsSubDetail_Data = null;
            DataNavigator dN_SubDetail = null;
            t.Find_DataSet(tForm, ref dsSubDetail_Data, ref dN_SubDetail, SubDetail_TableIPCode);

            if (dsSubDetail_Data == null)
            {
                //v.Kullaniciya_Mesaj_Var =
                //    "DİKKAT : " + SubDetail_TableIPCode + " için DataSet tespit edilemedi...";
                return onay;
            }

            if (dsSubDetail_Data.Tables[0].CaseSensitive == true)
                dsSubDetail_Data.Tables[0].CaseSensitive = false;

            string myProp = dsSubDetail_Data.Namespace.ToString();

            int DataReadType = t.myInt32(t.MyProperties_Get(myProp, "DataReadType:"));

            string SubDetail_List = t.MyProperties_Get(myProp, "About_Detail_SubDetail:");
            string Master_Detail_Harmony = t.MyProperties_Get(myProp, "MasterDetailHarmony:");

            string AutoInsert = t.MyProperties_Get(myProp, "AutoInsert:");

            string SqlF = t.MyProperties_Get(myProp, "SqlFirst:");
            string SqlS = t.MyProperties_Get(myProp, "SqlSecond:");
            string Sql_OldF = SqlF;
            string Sql_OldS = SqlS;
            bool sourceTableIPCodeREAD = false;

            #endregion Tanımlar

            if ((Sql_OldF != "") && (Sql_OldS == ""))
            {
                onay = SubDetail_Preparing(tForm, ref SqlF,
                                    ds_Master, dN_Master, //mst_TableIPCode,
                                    dsSubDetail_Data, SubDetail_List, SubDetail_TableIPCode,
                                    Master_Detail_Harmony,
                                    DataReadType, "", "", ref sourceTableIPCodeREAD);
                if (onay)
                {
                    onay = SubDetail_Run(tForm, dsSubDetail_Data, myProp,
                              SqlF, Sql_OldF, SqlS, Sql_OldS,
                              SubDetail_TableIPCode, DataReadType);
                }
            }

            if (Sql_OldS != "")
            {
                onay = SubDetail_Preparing(tForm, ref SqlS,
                                    ds_Master, dN_Master, //mst_TableIPCode,
                                    dsSubDetail_Data, SubDetail_List, SubDetail_TableIPCode,
                                    Master_Detail_Harmony,
                                    DataReadType, "", "", ref sourceTableIPCodeREAD);
                if (onay)
                {
                    onay = SubDetail_Run(tForm, dsSubDetail_Data, myProp,
                              SqlF, Sql_OldF, SqlS, Sql_OldS,
                              SubDetail_TableIPCode, DataReadType);
                }
            }
            
            if (sourceTableIPCodeREAD)
            {
                // Sadece yeni kayıt ise defaulValue çalışsın
                if (t.IsNotNull(dsSubDetail_Data))
                {
                    string value = dsSubDetail_Data.Tables[0].Rows[dN_SubDetail.Position][0].ToString();
                    if (value == "")
                    {
                        DataRow newRow = dsSubDetail_Data.Tables[0].Rows[dN_SubDetail.Position];

                        using (tDefaultValue df = new tDefaultValue())
                        {
                            df.tDefaultValue_And_Validation
                                (tForm,
                                 dsSubDetail_Data,
                                 newRow,
                                 SubDetail_TableIPCode,
                                 "tData_NewRecord");
                        }
                    }
                }
            }

            if (AutoInsert == "True")
            {
                if (ds_Master == null)
                {
                    if (dsSubDetail_Data.Tables[0].Rows.Count == 0)
                    {
                        string unutma = v.SP_NewWorkType;
                        v.SP_NewWorkType = "SubDetailNEW";
                        onay = evb.newData(tForm, SubDetail_TableIPCode);
                        v.SP_NewWorkType = unutma;
                    }
                }
                else
                {
                    if ((ds_Master.Tables[0].Rows.Count > 0) &&
                        (dsSubDetail_Data.Tables[0].Rows.Count == 0))
                    {
                        string unutma = v.SP_NewWorkType;
                        v.SP_NewWorkType = "SubDetailNEW";
                        onay = evb.newData(tForm, SubDetail_TableIPCode);
                        v.SP_NewWorkType = unutma;
                    }
                }
            }

            return onay;
        }

        
        public bool SubDetail_Preparing(
                   Form tForm, 
                   ref string Sql,
                   DataSet ds_Master, 
                   DataNavigator dN_Master,
                   DataSet dsSubDetail_Data,
                   string SubDetail_List, 
                   string SubDetail_TableIPCode,
                   string Master_Detail_Harmony,
                   int DataReadType, 
                   string Speed_FName, 
                   string Speed_Value,
                   ref bool sourceTableIPCodeREAD)
        {


            #region Tanımlar
            bool IsChange = false;
            string satir = string.Empty;
            string new_And = string.Empty;
            string newAnd_ = string.Empty;
            string read_mst_TableIPCode = string.Empty;
            string read_mst_FName = string.Empty;
            string read_sub_FName = string.Empty;
            string read_sub_FName2 = string.Empty;
            //string read_sub_FName3 = string.Empty;
            string read_field_type = string.Empty;
            string OperandType = string.Empty;
            string read_RefId = string.Empty;
            string read_mst_value = string.Empty;
            string mst_TableIPCode = string.Empty;
            string mst_CheckFName = string.Empty;
            string mst_CheckValue = string.Empty;
            string Operand = string.Empty;
            string clean_TableIPCode = string.Empty;

            string str_bgn = string.Empty;
            string str_end = string.Empty;
            int i_bgn = 0;
            int i_end = 0;
            int f_bgn = 0;
            int f_end = 0;

            int ga1 = -1; // @ güzel a
            int ga2 = -1; // @ güzel a
            int pos = 0;
            byte default_type = 0;
            byte buldu = 0;
            bool harmony_buldu = false; 
            bool harmony_onayi = false;

            DataRow mst_Row = null;
            #endregion Tanımlar

            // yeni setfocus olan Master satırın bilgileri alınıyor

            mst_TableIPCode = t.Find_TableIPCode(dN_Master);

            if (t.IsNotNull(ds_Master))
            {
                if (dN_Master.Position == -1) return IsChange;
                mst_Row = ds_Master.Tables[0].Rows[dN_Master.Position] as DataRow;
            }

            if (t.IsNotNull(mst_TableIPCode) == false)
                mst_TableIPCode = SubDetail_TableIPCode;

            /// Konuyla ilgili ÖRNEKLER
            /// 
            /// SubDetail table ait About_Detail_SubDetail listesi okunarak 
            /// sırayla Master/Detail tablodan veriler okunacak ve işlenecek
            /// 
            /// About_Detail_SubDetail:
            ///
            /// =Detail_SubDetail:UST/MEB/SrcAday.TalepFormu||Id||[SrcAdayTalep].AdayId||56||31||0||||||65205|ds|
            /// =Detail_SubDetail:UST/MEB/SrcAdayTalep.TalepFormuListesi||Id||[SrcAdayTalep].Id||56||31||0||||||65202|ds|
            /// 
            /// =Detail_SubDetail: Master >>> UST/MEB/SrcAday.TalepFormu||Id||             <<< Master : Detail >>>  [SrcAdayTalep].AdayId||56||31||0||||||65205|ds|  <<< Detail
            /// =Detail_SubDetail: Master >>> UST/MEB/SrcAdayTalep.TalepFormuListesi||Id|| <<< Master : Detail >>>  [SrcAdayTalep].Id||56||31||0||||||65202|ds|      <<< Detail
            /// 
            /// 'KRT_OPERAND_TYPE',    0,  ''
            /// 'KRT_OPERAND_TYPE',    1,  'Even (Double)'
            /// 'KRT_OPERAND_TYPE',    2,  'Odd  (Single)'
            /// 'KRT_OPERAND_TYPE',    3,  'Speed (Double)'
            /// 'KRT_OPERAND_TYPE',    4,  'Speed (Single)'
            /// 'KRT_OPERAND_TYPE',    5,  'On/Off'
            /// 'KRT_OPERAND_TYPE',    9,  'Visible=False'
            /// 'KRT_OPERAND_TYPE',   11,   > =
            /// 'KRT_OPERAND_TYPE',   12,   >
            /// 'KRT_OPERAND_TYPE',   13,   K = 
            ///  KRT_OPERAND_TYPE ,   14,   K 
            ///  KRT_OPERAND_TYPE ,   15,   K >
            /// 'KRT_OPERAND_TYPE',   16,  "Benzerleri (%abc%)"
            /// 'KRT_OPERAND_TYPE',   17,  "Benzerleri (abc%)"
            ///  K harfi < 
            ///  

            /// =Master_Detail_Harmony:UST/MEB/SrcAdayTalep.TalepFormuListesi||TalepTipiId||[SrcAdayTalep].TalepTipiId||52||34||0||||||65210|ds|
            while (Master_Detail_Harmony.IndexOf("Master_Detail_Harmony:") > -1)
            {
                harmony_buldu = true;

                satir = t.Get_And_Clear(ref Master_Detail_Harmony, "|ds|") + "||";

                t.Get_And_Clear(ref satir, "Master_Detail_Harmony:");

                new_And = "";
                // okunacak field
                read_mst_TableIPCode = t.Get_And_Clear(ref satir, "||");
                read_mst_FName = t.Get_And_Clear(ref satir, "||");
                read_sub_FName = t.Get_And_Clear(ref satir, "||"); // burada okunan alias.fieldName
                read_sub_FName2 = read_sub_FName.Replace("[", "");
                read_sub_FName2 = read_sub_FName2.Replace("]", "");
                read_sub_FName = t.Get_And_Clear(ref satir, "||"); // Harmony için target fieldName bu
                read_field_type = t.Get_And_Clear(ref satir, "||");
                default_type = System.Convert.ToByte(t.Get_And_Clear(ref satir, "||"));
                OperandType = t.Get_And_Clear(ref satir, "||");
                mst_CheckFName = t.Get_And_Clear(ref satir, "||");
                mst_CheckValue = t.Get_And_Clear(ref satir, "||");
                read_RefId = t.Get_And_Clear(ref satir, "||");

                if (OperandType == "=") Operand = " = ";
                if (OperandType == "0") Operand = " = ";
                if (OperandType == "2") Operand = " = "; // Odd  (Single) 
                if (OperandType == "3") Operand = " = "; // 
                if (OperandType == "11") Operand = " >= ";
                if (OperandType == "12") Operand = " > ";
                if (OperandType == "13") Operand = " <= ";
                if (OperandType == "14") Operand = " < ";
                if (OperandType == "15") Operand = " <> ";


                DataSet dsTarget = null;
                DataNavigator dNTarget = null;
                
                string harmony_master_value = string.Empty;
                string harmany_target_value = string.Empty;

                /// target tableIPCode
                if (t.IsNotNull(SubDetail_TableIPCode))
                {
                    t.Find_DataSet(tForm, ref dsTarget, ref dNTarget, SubDetail_TableIPCode);

                    if (t.IsNotNull(dsTarget))
                    {
                        /// Eğer herhangi bir datası yoksa onayla
                        if (dNTarget.Position == -1) harmony_onayi = true;
                        try
                        {
                            harmany_target_value = dsTarget.Tables[0].Rows[dNTarget.Position][read_sub_FName].ToString();
                        }
                        catch (Exception)
                        {
                            harmony_onayi = true; // target field bulunamadı ise yinede onayla kontrol edecek field yok
                            harmany_target_value = "";
                        }
                    }
                }

                if (mst_TableIPCode == read_mst_TableIPCode)
                {
                    if (t.IsNotNull(ds_Master))
                    {
                        if (dN_Master.Position == -1) harmony_onayi = true;
                        harmony_master_value = ds_Master.Tables[0].Rows[dN_Master.Position][read_mst_FName].ToString();
                    }
                }

                /// string bbb = SubDetail_TableIPCode;

                if (t.IsNotNull(harmany_target_value) && t.IsNotNull(harmony_master_value))
                {
                    if (harmany_target_value == harmony_master_value) 
                        harmony_onayi = true;    // eğer harmony uymu varsa sub_detail data tekrar okuna bilir
                    else harmony_onayi = false;  // eğer harmony uymu yoksa sub_detail data tekrar okunmasın
                }
            }


            #region SubDetail_List Read
            //Application.OpenForms[0].Text = Application.OpenForms[0].Text + ";sdR3;" + mst_TableIPCode;
            while (SubDetail_List.IndexOf("Detail_SubDetail:") > -1)
            {
                #region read values
                satir = t.Get_And_Clear(ref SubDetail_List, "|ds|") + "||";

                t.Get_And_Clear(ref satir, "Detail_SubDetail:");

                //=Detail_SubDetail:3S_MSTBLIP.3S_MSTBLIP_02||@TABLEIPCODE||[3S_MSTBLIP_VWO].LKP_TABLEIPCODE||167||0||0||||||1065|ds|

                new_And = "";
                // okunacak field
                read_mst_TableIPCode = t.Get_And_Clear(ref satir, "||");
                read_mst_FName = t.Get_And_Clear(ref satir, "||");
                // atanacak field
                // [HPCARI].ID
                read_sub_FName = t.Get_And_Clear(ref satir, "||");
                // HPCARI.ID
                read_sub_FName2 = read_sub_FName.Replace("[", "");
                read_sub_FName2 = read_sub_FName2.Replace("]", "");
                // ID
                //read_sub_FName3 = t.Alias_Clear(read_sub_FName);
                // --
                read_field_type = t.Get_And_Clear(ref satir, "||");
                // --
                default_type = System.Convert.ToByte(t.Get_And_Clear(ref satir, "||"));
                OperandType = t.Get_And_Clear(ref satir, "||");
                mst_CheckFName = t.Get_And_Clear(ref satir, "||");
                mst_CheckValue = t.Get_And_Clear(ref satir, "||");
                read_RefId = t.Get_And_Clear(ref satir, "||");

                if (OperandType == "=") Operand = " = ";
                if (OperandType == "0") Operand = " = ";
                if (OperandType == "2") Operand = " = "; // Odd  (Single) 
                if (OperandType == "3") Operand = " = "; // 
                if (OperandType == "11") Operand = " >= ";
                if (OperandType == "12") Operand = " > ";
                if (OperandType == "13") Operand = " <= ";
                if (OperandType == "14") Operand = " < ";
                if (OperandType == "15") Operand = " <> ";

                #endregion set

                // eğer sql de -99 var ise sql boşu boşuna çalışmasın 
                // eğer value  -98 ise bulunduğu and koşulu iptal edilecek

                // MultiPageID var ise
                // FNLNVOL.FNLNVOL_BNL01 == FNLNVOL.FNLNVOL_BNL01|23
                // if (read_mst_TableIPCode == mst_TableIPCode)

                clean_TableIPCode = mst_TableIPCode;
                if (mst_TableIPCode.IndexOf("|") > -1)
                {
                    clean_TableIPCode = mst_TableIPCode.Substring(0, mst_TableIPCode.IndexOf("|"));
                }
                //---

                // evet sourceTableIPCodeREAD bilgisi mevcut
                if ((read_mst_TableIPCode == clean_TableIPCode) &&
                    (default_type == 21))
                    sourceTableIPCodeREAD = true;

                if (((read_mst_TableIPCode == clean_TableIPCode) ||
                     (read_mst_TableIPCode == mst_TableIPCode)) &&
                    (default_type != 21)) // 21 = Source TableIPCode READ
                {
                    // buldu = 9 ile başlar, eğer 9 değişirse bir işlem olmuştur demek
                    buldu = 9;  // bu metod olmadı


                    // Set @xxxx   mı yoksa  and GCB_ID = x  mı    
                    ga1 = read_mst_FName.IndexOf("@");
                    ga2 = read_sub_FName.IndexOf("@");

                    if (ga1 > -1)
                        read_mst_FName = read_mst_FName.Substring(ga1, read_mst_FName.Length - ga1);
                    if (ga2 > -1)
                        read_sub_FName = read_sub_FName.Substring(ga2, read_sub_FName.Length - ga2);

                    // normal Where in altındaki and ile başlayan bağlantılar için

                    #region SpeedKriter
                    if (t.IsNotNull(Speed_FName) &&
                        ((OperandType == "3") || // SpeedKriter Double
                         (OperandType == "4"))   // SpeedKriter Single
                        )
                    {
                        // SpeedKriter Double
                        if (OperandType == "3")
                        {
                            /// read_field_type = 58
                            /// and Convert(Datetime, [AJLNTLK].REC_DATE, 103) >= Convert(Datetime, '25.12.2016', 103)--:D.SD.2809:-->=
                            /// and Convert(Datetime, [AJLNTLK].REC_DATE, 103) <= Convert(Datetime, '25.12.2016', 103)--:D.SD.2809:--<=

                            /// >= işlemleri
                            ///  and Convert(Datetime, [BDekont].BelgeTarihi, 103)  >= Convert(Datetime, '27.07.2022', 103)     -- :D.SD.43301: -->=
                            ///  and Convert(Datetime, [BDekont].BelgeTarihi, 103)  <= Convert(Datetime, '27.07.2022', 103)     -- :D.SD.43301: --<=
                            ///
                            /// and Convert(Date, isnull([BDekont].BelgeTarihi, getdate()), 103)  <> Convert(Date, '01.01.1900', 103)-- :D.SD.46524: -->=
                            /// and Convert(Date, [BDekont].BelgeTarihi, 103) <= Convert(Date, '03.09.2025', 103)-- :D.SD.46524: --<=

                            if (Speed_FName.IndexOf("_BAS") > -1)
                            {
                                str_bgn = " and " + read_sub_FName + "  >=";
                                if (read_field_type == "40" || read_field_type == "61")
                                    str_bgn = " and Convert(Date, " + read_sub_FName + ", 103)  >=";
                                if (read_field_type == "58")
                                    str_bgn = " and Convert(Datetime, " + read_sub_FName + ", 103)  >=";

                                i_bgn = Sql.IndexOf(str_bgn);

                                if (i_bgn == -1)
                                {
                                    if (read_field_type == "40" || read_field_type == "61")
                                        str_bgn = " and Convert(Date, isnull(" + read_sub_FName + ",getdate()), 103)";
                                    if (read_field_type == "58")
                                        str_bgn = " and Convert(Datetime, isnull(" + read_sub_FName + ",getdate()), 103)";

                                    i_bgn = Sql.IndexOf(str_bgn);
                                }

                                str_end = "   -- :D.SD.";// + read_RefId + ": -->=";
                                i_end = Sql.IndexOf(str_end, i_bgn);

                                /// önceki yapı
                                /// new_And = " and " + t.Set_FieldName_Value_(System.Convert.ToInt16(read_field_type), read_sub_FName, Speed_Value, "and", ">=");

                                newAnd_ = t.Set_FieldName_Value_(System.Convert.ToInt16(read_field_type), read_sub_FName, Speed_Value, "and", ">=");

                                if (newAnd_.IndexOf("--") == -1)
                                    new_And = " and " + newAnd_;
                                else
                                {
                                    /// null veya boş değer geldiyse sorgudaki parametre iptal olsun
                                    newAnd_ = newAnd_.Substring(2) + " = ???";//   -- :D.SD." + read_RefId + ": --" + v.ENTER;  gerek yok
                                    new_And = " --and " + newAnd_;
                                }
                            }

                            /// <= işlemleri
                            if (Speed_FName.IndexOf("_BIT") > -1)
                            {
                                str_bgn = " and " + read_sub_FName + "  <=";
                                if (read_field_type == "40" || read_field_type == "61")
                                    str_bgn = " and Convert(Date, " + read_sub_FName + ", 103)  <=";
                                if (read_field_type == "58")
                                    str_bgn = " and Convert(Datetime, " + read_sub_FName + ", 103)  <=";

                                i_bgn = Sql.IndexOf(str_bgn);

                                if (i_bgn == -1)
                                {
                                    if (read_field_type == "40" || read_field_type == "61")
                                        str_bgn = " and Convert(Date, isnull(" + read_sub_FName + ",getdate()), 103)";
                                    if (read_field_type == "58")
                                        str_bgn = " and Convert(Datetime, isnull(" + read_sub_FName + ",getdate()), 103)";

                                    i_bgn = Sql.IndexOf(str_bgn);
                                }

                                str_end = "   -- :D.SD.";// + read_RefId + ": --<=";
                                i_end = Sql.IndexOf(str_end, i_bgn);

                                /// önceki yapı
                                /// new_And = " and " + t.Set_FieldName_Value_(System.Convert.ToInt16(read_field_type), read_sub_FName, Speed_Value, "and", "<=");

                                newAnd_ = t.Set_FieldName_Value_(System.Convert.ToInt16(read_field_type), read_sub_FName, Speed_Value, "and", "<=");

                                if (newAnd_.IndexOf("--") == -1)
                                    new_And = " and " + newAnd_;
                                else
                                {
                                    /// null veya boş değer geldiyse sorgudaki parametre iptal olsun
                                    newAnd_ = newAnd_.Substring(2) + " = ???";//   -- :D.SD." + read_RefId + ": --" + v.ENTER;  gerek yok
                                    new_And = " --and " + newAnd_;
                                }
                            }

                            ///------
                            /// Stored Procedures için hazırlık biraz aşağıda devam ediyor
                        }

                        // SpeedKriter Single
                        if (OperandType == "4")
                        {
                            /// read_field_type = 58
                            /// and Convert(Datetime, [AJLNTLK].REC_DATE, 103)  =  Convert(Datetime, '25.12.2016', 103)     -- :D.SD.2809: --
                            
                            /// and Convert(Date, isnull([MtskAdayTakip].uSinavPlanTarihi,getdate()), 103)  <>  Convert(Date, '01.01.1900', 103)   -- :D.SD.59384: --

                            str_bgn = " and " + read_sub_FName;
                            if (read_field_type == "40" || read_field_type == "61")
                                str_bgn = " and Convert(Date, isnull(" + read_sub_FName ;
                            if (read_field_type == "58")
                                str_bgn = " and Convert(Datetime, isnull(" + read_sub_FName;

                            i_bgn = Sql.IndexOf(str_bgn);

                            if (i_bgn == -1)
                            {
                                if (read_field_type == "40" || read_field_type == "61")
                                    str_bgn = " and Convert(Date, " + read_sub_FName;
                                if (read_field_type == "58")
                                    str_bgn = " and Convert(Datetime, " + read_sub_FName;

                                i_bgn = Sql.IndexOf(str_bgn);
                            }
                            if (i_bgn == -1)
                            {
                                if (read_field_type == "40" || read_field_type == "61")
                                    str_bgn = " and Convert(Date, isnull(" + read_sub_FName + ",getdate()), 103)";
                                if (read_field_type == "58")
                                    str_bgn = " and Convert(Datetime, isnull(" + read_sub_FName + ",getdate()), 103)";

                                i_bgn = Sql.IndexOf(str_bgn);
                            }

                            str_end = "   -- :D.SD.";// + read_RefId + ": --";
                            i_end = Sql.IndexOf(str_end, i_bgn);

                            /// önceki yapı
                            /// new_And = " and " + t.Set_FieldName_Value_(System.Convert.ToInt16(read_field_type), read_sub_FName, Speed_Value, "and", OperandType);

                            newAnd_ = t.Set_FieldName_Value_(System.Convert.ToInt16(read_field_type), read_sub_FName, Speed_Value, "and", OperandType);

                            if (newAnd_.IndexOf("--") == -1)
                                new_And = " and " + newAnd_;
                            else
                            {
                                /// null veya boş değer geldiyse sorgudaki parametre iptal olsun
                                newAnd_ = newAnd_.Substring(2) + " = ???";//   -- :D.SD." + read_RefId + ": --" + v.ENTER;  gerek yok
                                new_And = " --and " + newAnd_;
                            }
                        }

                        // Stored Procedures için hazırlık burada
                        if ((Sql.IndexOf("set @" + Speed_FName) > -1) &&
                            (Speed_FName.IndexOf(mst_CheckFName) > -1))
                        {
                            // OperandType == "3" olunca Speed_FName xxx_BAS ve xxx_BIT şeklinde geliyor
                            // OperandType == "4" olunca Speed_FName == mst_CheckFName şeklinde

                            /// Aşağıdaki sql manuel hazırlanıyor
                            /// xxx_BAS ve xxx_BIT değişkenleri 
                            /// SpeedKriterde field olarak kullanılan isim 
                            /// @fieldName_BAS ve @fieldName_BIT  şeklinde hazırlanır
                            /// NOT : {baslamaTarihi} ve {bitisTarihi}  değişkenleri SQL_Preparing içinde tanımlı

                            /// declare @ISLEM_TRHS_BAS date
                            /// declare @ISLEM_TRHS_BIT date
                            /// set @ISLEM_TRHS_BAS = {baslamaTarihi}  
                            /// set @ISLEM_TRHS_BIT = {bitisTarihi}
                            /// EXEC [dbo].[prc_FN_LINEVOL_FULLHESAP]
                            ///  @baslamaTarihi = @ISLEM_TRHS_BAS,
                            ///  @bitisTarihi = @ISLEM_TRHS_BIT

                            str_bgn = "set @" + Speed_FName + " = ";

                            i_bgn = Sql.IndexOf(str_bgn);

                            str_end = "\r\n";
                            i_end = Sql.IndexOf(str_end, i_bgn);

                            new_And = "set " + t.Set_FieldName_Value_(System.Convert.ToInt16(read_field_type), Speed_FName, Speed_Value, "@", "=");
                        }

                    }
                    #endregion

                    // smalldatetime = 58
                    // date = 40, 61
                    // time = 41

                    #region and [a].GCB = x   değişimi
                    if ((ga1 == -1) && (ga2 == -1) && (default_type == 31))
                    {
                        str_bgn = " and " + read_sub_FName + Operand;
                        i_bgn = Sql.IndexOf(str_bgn);

                        if (i_bgn == -1)
                        {
                            str_bgn = " and " + read_sub_FName2 + Operand;
                            i_bgn = Sql.IndexOf(str_bgn);
                        }

                        if (i_bgn == -1)
                        {
                            str_bgn = " --and " + read_sub_FName + Operand;
                            i_bgn = Sql.IndexOf(str_bgn);
                        }

                        if (i_bgn == -1)
                        {
                            // and Convert(Date, [SYSOTV].TARIH, 103)  = Convert(Datetime,'01.01.1900', 103)    -- :D.SD.5839: --
                            str_bgn = "--and Convert(Date, " + read_sub_FName + ", 103) " + Operand;
                            i_bgn = Sql.IndexOf(str_bgn);
                        }
                        if (i_bgn == -1)
                        {
                            // and Convert(Date, [SYSOTV].TARIH, 103)  = Convert(Datetime,'01.01.1900', 103)    -- :D.SD.5839: --
                            str_bgn = " and Convert(Date, " + read_sub_FName + ", 103) " + Operand;
                            i_bgn = Sql.IndexOf(str_bgn);
                        }

                        if (i_bgn == -1)
                        {
                            // and Convert(Datetime, [SYSOTV].TARIH, 103)  = Convert(Datetime,'01.01.1900', 103)    -- :D.SD.5839: --
                            str_bgn = "--and Convert(Datetime, " + read_sub_FName + ", 103) " + Operand;
                            i_bgn = Sql.IndexOf(str_bgn);
                        }
                        if (i_bgn == -1)
                        {
                            // and Convert(Datetime, [SYSOTV].TARIH, 103)  = Convert(Datetime,'01.01.1900', 103)    -- :D.SD.5839: --
                            str_bgn = " and Convert(Datetime, " + read_sub_FName + ", 103) " + Operand;
                            i_bgn = Sql.IndexOf(str_bgn);
                        }

                        if (i_bgn == -1)
                        {
                            // and Convert(Datetime, [SYSOTV].TARIH, 103)  = Convert(Datetime,'01.01.1900', 103)    -- :D.SD.5839: --
                            str_bgn = "--and Convert(SmallDatetime, " + read_sub_FName + ", 103) " + Operand;
                            i_bgn = Sql.IndexOf(str_bgn);
                        }
                        if (i_bgn == -1)
                        {
                            // and Convert(Datetime, [SYSOTV].TARIH, 103)  = Convert(Datetime,'01.01.1900', 103)    -- :D.SD.5839: --
                            str_bgn = " and Convert(SmallDatetime, " + read_sub_FName + ", 103) " + Operand;
                            i_bgn = Sql.IndexOf(str_bgn);
                        }

                        if (i_bgn > -1)
                        {
                            str_end = "   -- :D.SD.";// + read_RefId + ": --";
                            i_end = Sql.IndexOf(str_end, i_bgn);

                            if (i_end == -1)
                            {
                                str_end = "-- :D.SD.";// + read_RefId + ": --";
                                i_end = Sql.IndexOf(str_end, i_bgn);
                            }
                        }
                        
                        if (mst_Row != null)
                        {
                            try
                            {
                                read_mst_value = mst_Row[read_mst_FName].ToString();
                            }
                            catch (Exception e)
                            {
                                read_mst_value = "-1";
                                //throw;
                            }
                        }
                        else
                        {
                            read_mst_value = "-1";
                        }

                        if ((read_field_type == "40" || read_field_type == "58" || read_field_type == "61") &&
                            (read_mst_value == "")) 
                            read_mst_value = "01.01.1900";

                        if (read_mst_FName == ":DONEM_YILAY")
                        {
                            read_mst_value = v.DONEMTIPI_YILAY.ToString();
                        }

                        if (read_mst_value != "-98" && read_mst_value != "")
                        {
                            new_And = " and " + t.Set_FieldName_Value_(System.Convert.ToInt16(read_field_type), read_sub_FName, read_mst_value, "and", OperandType);
                            buldu = 8;
                        }

                        // satırı geçici iptal etmek için
                        if ((read_mst_value == "-98" || read_mst_value == "") &&
                            (default_type != 31)) // MasterDetail değilse 
                        {
                            new_And = " --and " + t.Set_FieldName_Value_(System.Convert.ToInt16(read_field_type), read_sub_FName, read_mst_value, "and", OperandType);
                            buldu = 8;
                        }
                    }
                    #endregion

                    #region /*prm*/ @partId = -98  veya  set @partId = -98
                    
                    if (//((ga2 > -1) || (OperandType == "2")) && // Odd  (Single)
                        (default_type == 31) &&  // master-detail 
                        (Sql.IndexOf(" EXEC ") > -1))
                    {
                        // EXEC [dbo].[LST_VR_VARDIYA_IOLN_GCF]
                        //  /*prm*/ @partId = -98   -- :D.SD.2004: --

                        // Stored Procedures fieldlerinde kriter_FName, kriter_Operand_Type (odd single) olunca
                        // ve kriter_FName de @ işareti koymaya gerek kalmadı
                        // yalınız kriter fieldname parametre ismiyle aynı olsun 
                        if (ga2 == -1)
                        {
                            read_sub_FName = "@" + t.Alias_Clear(read_sub_FName);
                            // alttaki yorumlara takılmasın diye
                            ga2 = 1;
                        }

                        // MSSQL için
                        str_bgn = "  /*prm*/ " + read_sub_FName + Operand;
                        i_bgn = Sql.IndexOf(str_bgn);
                        //buldu = 9;

                        if (i_bgn == -1)
                        {
                            str_bgn = ", /*prm*/ " + read_sub_FName + Operand;
                            i_bgn = Sql.IndexOf(str_bgn);
                            buldu = 1;
                        }


                        // MySQL için
                        if (i_bgn == -1)
                        {
                            str_bgn = "  /*prm." + read_sub_FName + "*/";
                            i_bgn = Sql.IndexOf(str_bgn);
                            buldu = 0;
                        }

                        if (i_bgn == -1)
                        {
                            str_bgn = ", /*prm." + read_sub_FName + "*/ ";
                            i_bgn = Sql.IndexOf(str_bgn);
                            buldu = 1;
                        }
                        
                        // MSSQL için 
                        if (i_bgn == -1)
                        {
                            //set @yil = 2016  
                            //set @ay = 4
                            //set @partId = -98
                            str_bgn = "set " + read_sub_FName + Operand;
                            i_bgn = Sql.IndexOf(str_bgn);
                            buldu = 2;
                        }

                        str_end = "   -- :D.SD.";// + read_RefId + ": --";
                        i_end = Sql.IndexOf(str_end, i_bgn);

                        if ((i_end == -1) && (i_bgn > -1))
                        {
                            //i_end == -1 ise satır sonunu bulalım
                            for (int i = i_bgn; i < Sql.Length; i++)
                            {
                                if (Sql[i] == '\r') { i_end = i; break; }
                            }
                        }

                        if (mst_Row != null)
                        {
                            read_mst_value = mst_Row[read_mst_FName].ToString();
                        }
                        else
                        {
                            read_mst_value = "0";
                        }

                        // MSSQL ise
                        if (Sql.IndexOf("EXEC") > -1)
                        {
                            if (buldu == 0)
                                new_And = "  /*prm*/ " + t.Set_FieldName_Value_(System.Convert.ToInt16(read_field_type), read_sub_FName, read_mst_value, "and", OperandType);
                            if (buldu == 1)
                                new_And = ", /*prm*/ " + t.Set_FieldName_Value_(System.Convert.ToInt16(read_field_type), read_sub_FName, read_mst_value, "and", OperandType);
                            if (buldu == 2)
                                new_And = "set " + t.Set_FieldName_Value_(System.Convert.ToInt16(read_field_type), read_sub_FName, read_mst_value, "@", OperandType);
                        }
                        // MySQL ise
                        if (Sql.IndexOf("CALL") > -1)
                        {
                            if (buldu == 0)
                                new_And = "  /*prm." + read_sub_FName + "*/ " + t.Set_FieldName_Value_(System.Convert.ToInt16(read_field_type), "", read_mst_value, "and", "null");
                            if (buldu == 1)
                                new_And = ", /*prm." + read_sub_FName + "*/ " + t.Set_FieldName_Value_(System.Convert.ToInt16(read_field_type), "", read_mst_value, "and", "null");

                            //if (buldu == 2)
                            //    new_And = "set " + t.Set_FieldName_Value_(System.Convert.ToInt16(read_field_type), read_sub_FName, read_mst_value, "and", OperandType);
                        }

                    }
                    
                    if ((default_type == 31) && (new_And == ""))
                    {
                        // bu metod olmadı
                        if (buldu == 9)
                            if (Sql.IndexOf("EXEC") > -1)
                            {
                                MessageBox.Show(
                                ":: DİKKAT : " + v.ENTER2 +
                                ":: /*prm*/ : [ " + read_mst_FName + "  ==  " + read_sub_FName + " ]"+ v.ENTER +
                                ":: Stored Procedures /*prm*/ için " + v.ENTER +
                                ":: kriter_FName, kriter_Operand_Type (odd single) olsun " + v.ENTER +
                                ":: @kriter_FName şeklinde @ işareti koymaya gerek kalmadı " + v.ENTER +
                                ":: yalnız kriter fieldName parametre ismiyle aynı olsun ");
                            }
                    }
                    
                    #endregion

                    #region default_type == 51, 52, 53
                    if ((ga1 == -1) && (ga2 == -1) &&
                        ((default_type == 51) || (default_type == 52) || (default_type == 53)
                         ))
                    {
                        //EXEC [dbo].[prc_FINANS_BAKIYE]
                        //  /*prm*/ @IslemTarihi_Bas = '15.11.2015'    -- :D.SD.4922: --
                        //, /*prm*/ @IslemTarihi_Bit = '15.11.2015'    -- :D.SD.4923: --

                        str_bgn = "/*prm*/ " + read_sub_FName + Operand;
                        i_bgn = Sql.IndexOf(str_bgn);

                        //if (i_bgn == -1)
                        //{
                        //    str_bgn = "/*prm*/ " + read_sub_FName3 + Operand;
                        //    i_bgn = Sql.IndexOf(str_bgn);
                        //}

                        str_end = "   -- :D.SD.";// + read_RefId + ": --";
                        i_end = Sql.IndexOf(str_end, i_bgn);

                        read_mst_value = t.Find_Kriter_Value(tForm, read_mst_TableIPCode, read_mst_FName, default_type);
                        if (read_mst_value == "") read_mst_value = "0";

                        if (read_mst_value != "-98")
                        {
                            new_And = "/*prm*/ " + t.Set_FieldName_Value_(System.Convert.ToInt16(read_field_type), read_sub_FName, read_mst_value, "and", OperandType);
                        }
                    }
                    #endregion

                    #region and [a].GCB in ( x1, x2 ... ) değişimi
                    if ((ga1 == -1) && (default_type == 32))
                    {
                        str_bgn = "/*" + read_sub_FName + "_INLIST*/";
                        str_end = "/*" + read_sub_FName + "_INEND*/";
                        i_bgn = Sql.IndexOf(str_bgn) + str_bgn.Count() + 2;
                        i_end = Sql.IndexOf(str_end);

                        read_mst_value = Preparing_Select_Value_List(ds_Master, read_mst_FName, mst_CheckFName, mst_CheckValue);
                        new_And = read_mst_value + v.ENTER;

                        /*
                             and [K].GRUP_ID in (   -- :D.SD.2774: --
                             / *[K].GRUP_ID_INLIST* /
                               2990
                             , 2983
                             / *[K].GRUP_ID_INEND* /
                             )
                       */

                    }
                    #endregion

                    #region  Set @GCB_ID = x    değişimi
                    // Declare @xxxxx  ve  
                    // Set @xxxxx ifadeleri için 

                    // veya

                    // <*declareBegin*>
                    // declare @PlakaNumarasi varchar(800)
                    // declare @PersonelId varchar(800)

                    // <*setBegin*>
                    // set @PlakaNumarasi = ''-- :D.SD.36191: --
                    // set @PersonelId = ''-- :D.SD.36192: --

                    if (((ga1 > -1) && (ga2 == -1)) ||
                       (Sql.IndexOf("/*declareBegin*/") > -1)) // sonradan eklendi
                    {
                        // atama yapılcak field
                        // read_sub_FName 
                        // read_field_type 
                        read_sub_FName = t.Alias_Clear(read_sub_FName);

                        //str_bgn = "set " + read_mst_FName;
                        str_bgn = "set @" + read_sub_FName;
                        str_end = "   -- :D.SD." + read_RefId + ": --";

                        if (mst_Row != null)
                        {
                            if (read_mst_FName.IndexOf("@") > -1)
                                 read_mst_value = mst_Row[read_mst_FName.Substring(1, read_mst_FName.Length - 1)].ToString();
                            else read_mst_value = mst_Row[read_mst_FName].ToString();
                        }
                        else
                        {
                            read_mst_value = "0";
                        }

                        if (read_mst_value != "-98")
                        {
                            i_bgn = Sql.IndexOf(str_bgn);
                            i_end = Sql.IndexOf(str_end);

                            new_And = "set " + t.Set_FieldName_Value_(System.Convert.ToInt16(read_field_type), read_sub_FName, read_mst_value, "@", OperandType);
                            //new_And = "set " + t.Set_FieldName_Value_(System.Convert.ToInt16(read_field_type), read_mst_FName, read_mst_value, "@", OperandType);
                        }
                    }
                    #endregion


                    #region // Data Collection değil ise
                    if (DataReadType != 5)
                    {
                        if ((new_And != "") && (i_bgn >= 0) && (i_end > 0))
                        {
                            string kimGeldi = SubDetail_TableIPCode;
                            
                            /// Sql üzerindeki Id value şartı ile yeni new_And aynı ise bir daha yani defalarca çalışmasına gerek yok
                            /// olduğu gibi geri dönsün ve databaseden tekrar okuma yapmasın, 
                            /// çünkü çok ağırlaşıyor : Örnek AdayTaleFormu : Talep listesi üzerinde gezinirken defalarca aynı dataları okumaya çaışıyor
                            /// 
                            //if (Sql.IndexOf(new_And) > -1)
                            //{
                            //    return false;
                            //}

                            /// harmony/uyum kontrolü varsa
                            /// harmony_onayi = true;   // eğer harmony uymu varsa sub_detail data tekrar okuna bilir
                            /// harmony_onayi = false;  // eğer harmony uymu yoksa sub_detail data tekrar okunmasın
                            if (harmony_buldu == true)  
                            {
                                // demekki harmony/uyumlu değil buradan geri dön, yani sub_detail data tekrar okumasın
                                if (harmony_onayi == false)
                                {
                                    return false;
                                }
                                // eğer harmony/uyumlu ve new_and de yine aynıysa sub_detail data tekrar okumasın
                                if (Sql.IndexOf(new_And) > -1)
                                {
                                    return false;
                                }
                            }

                            IsChange = true;
                            f_bgn = 0;
                            f_end = 0;
                            //int i1 = Sql.IndexOf(FullHeader1, f1);
                            while ((i_end > f_end) && (f_end > -1) && (i_end > i_bgn))
                            {
                                Sql = Sql.Remove(i_bgn, (i_end - i_bgn));

                                Sql = Sql.Insert(i_bgn, new_And);

                                //f_bgn = Sql.IndexOf(str_bgn, i_bgn + new_And.Length);
                                //f_end = Sql.IndexOf(str_end, i_end + 2);// new_And.Length);
                                //int f_bgn_test = Sql.IndexOf(str_bgn, i_bgn + 2);
                                //int f_end_test = Sql.IndexOf(str_end, f_bgn_test + 2);

                                if (Sql.IndexOf("--" + str_bgn.TrimStart() , i_bgn - 5) > -1)
                                    i_bgn = Sql.IndexOf("--" + str_bgn.TrimStart(), i_bgn) + 10;

                                f_bgn = Sql.IndexOf(str_bgn, i_bgn + 2);
                                f_end = Sql.IndexOf(str_end, f_bgn + 2);
                                if (f_bgn == -1) f_end = -1;

                                if ((f_bgn > 0) && (f_end > 0))
                                {
                                    i_bgn = f_bgn;
                                    i_end = f_end;
                                    f_end = 0;
                                }
                            }
                        }
                    }
                    #endregion

                    #region // Data Collection ise
                    if (DataReadType == 5)
                    {
                        #region // sql de -99 yok ise Value Atama işlemleri gerçekleşsin
                        if (Sql.IndexOf("-99") == -1)
                        {
                            try
                            {
                                if (t.IsNotNull(dsSubDetail_Data))
                                {
                                    read_sub_FName = read_sub_FName.Substring(read_sub_FName.IndexOf(".") + 1);
                                    pos = t.Find_DataNavigator_Position(tForm, SubDetail_TableIPCode);
                                    dsSubDetail_Data.Tables[0].Rows[pos][read_sub_FName] = read_mst_value;
                                }
                            }
                            catch (Exception)
                            {
                                //..
                                throw;
                            }
                        }
                        #endregion

                        #region // sql de -99 varsa onlarda silinsin
                        if (Sql.IndexOf("-99") > -1)
                        {
                            if ((new_And != "") && (i_bgn >= 0) && (i_end > 0))
                            {
                                Sql = Sql.Remove(i_bgn, (i_end - i_bgn));
                                Sql = Sql.Insert(i_bgn, new_And);
                            }
                        }
                        #endregion
                    }
                    #endregion // Data Collection ise

                }
            }

            return IsChange;
            #endregion SubDetail_List Read
        }


        public bool SubDetail_Run(Form tForm, DataSet dsSubDetail_Data,
                                   string myProp,
                                   string SqlF, string Sql_OldF, string SqlS, string Sql_OldS,
                                   string TableIPCode, int DataReadType)
        {
            #region Sonuçları işle ve Gerekirse dsSubDetail i yeniden oku ( t.Data_Read_Execute )

            bool onay = true;

            // yeni SQLi SqlFirste yerleştir 
            //t.Str_Replace(ref myProp, Sql_OldF, SqlF);

            // yeni SQLi SqlSecond yerleştir 
            if (Sql_OldS != "")
                t.Str_Replace(ref myProp, Sql_OldS, SqlS);

            // yerine ata
            dsSubDetail_Data.Namespace = myProp;
            
            string Sql = string.Empty;

            if (Sql_OldS != "")
                Sql = SqlS;
            else Sql = SqlF;

            // Sql içinde Detail-SubDetail e bağlı atama kalmadığı zaman
            // SubDetail i yeniden çalıştırabiliriz.
            if (Sql.IndexOf("-99") == -1)
            {
                Control cntrl = null;
                cntrl = t.Find_Control_View(tForm, TableIPCode);

                // Data Collection değilse
                if (DataReadType != 5)
                {
                    //
                    //v.SQLSave = v.ENTER2 + TableIPCode + v.ENTER + Sql + v.SQLSave;
                    //v.SQLSave = v.ENTER + TableIPCode + v.SQLSave;
                    //---

                    //
                    try
                    {
                        t.Data_Read_Execute(tForm, dsSubDetail_Data, ref Sql, "", cntrl);
                    }
                    catch (Exception)
                    {
                        onay = false;
                        //throw;
                    }
                    
                    /// External_TableIPCode kullanan diğer cntrl leride bulup enabled özelliğini tetiklemek gerekiyor
                    ///

                    /// 20.03.18
                    /// yeni deneme şimdilik kapatalım 

                    ///t.External_Controls_Enabled(tForm, dsSubDetail_Data, cntrl);

                    // eğer okunan data boş geliyorsa ve kendine bağlı alt datasetler olabilir

                    // ***   BURASI GEREKMEYE BİLİR DENEME YAPILIYOR
                    ///  burası gerekiyormuşşşşşş
                    ///  okunan data sıfır kayıt olunca positionChange çalışmıyor
                    ///  bu nedenle kendisine bağlı diğer dsDatalarda yeniden refreshlenmeli
                    ///  
                    if (dsSubDetail_Data.Tables[0].Rows.Count == 0)
                    {
                        string SubDetail_List = t.MyProperties_Get(myProp, "About_Detail_SubDetail:");

                        if (t.IsNotNull(SubDetail_List))
                        {
                            //DataNavigator dN = null;
                            //dN = t.Find_DataNavigator(tForm, TableIPCode);

                            // sonra database okunuyor
                            //tSubDetail_Refresh(dsSubDetail_Data, dN); eski hali

                            /// 20.03.18 kapatalım
                            ///tSubWork_Refresh_(tForm, dsSubDetail_Data, dN);
                        }
                    }

                    // YENİ EKLENDİ
                    t.ViewControl_Enabled(tForm, dsSubDetail_Data, TableIPCode);
                    // bu IPCode bağlı ExternalIPCode olabilir...
                    t.ViewControl_Enabled_ExtarnalIP(tForm, dsSubDetail_Data);

                }

            }

            return onay;

            #endregion Sonuçları işle ve Gerekirse dsSubDetail i yeniden oku
        }


        private string Preparing_Select_Value_List(DataSet dsData, string read_mst_FName,
                        string mst_CheckFName, string mst_CheckValue)
        {


            string s = string.Empty;

            if ((t.IsNotNull(dsData) == false) ||
                (t.IsNotNull(read_mst_FName) == false) ||
                (t.IsNotNull(mst_CheckFName) == false) ||
                (t.IsNotNull(mst_CheckValue) == false)
                )
            {
                MessageBox.Show("DİKKAT : Eksik bilgi mevcut ..." + v.ENTER +
                    "Master FieldName : " + read_mst_FName + v.ENTER +
                    "Check  FieldName : " + mst_CheckFName + v.ENTER +
                    "Check  Value : " + mst_CheckValue + v.ENTER2 +
                    " + Master Data olmaya bilir ..."
                    );
                return null;
            }

            int i1 = dsData.Tables[0].Rows.Count;

            for (int i = 0; i < i1; i++)
            {
                if (dsData.Tables[0].Rows[i][mst_CheckFName].ToString() == mst_CheckValue)
                {
                    s = s + " , " + dsData.Tables[0].Rows[i][read_mst_FName].ToString() + v.ENTER;
                }
            }

            if (s.Length > 2) s = s.Remove(0, 2);

            if (t.IsNotNull(s) == false) s = " -1 ";

            return s;
        }

        #endregion Detail_SubDetail_Read


        private void Source_ParentControl_Tag_Value(Form tForm, string TableIPCode)
        {


            #region v.con Source_ParentControl_Tag_Value
            /// 
            /// IPCode tanımların 
            /// Default value de ( Source ParentControl Tag READ ) var ise
            /// geçici hafıza ile buraya gelen value yi ilgili Control_View.Parent  in tag ına
            /// yüklemek gerekiyor
            /// Şimdilik bunu çözüm buldum ( İlgili : Banka Hareketleri ekranı ) 
            /// Bu özellikle MultiPageID özelliği kullanılan sayfalarda 
            /// yeni bağımsız fiş ekranı açıldğında gerek duyuluyor
            /// 
            if (t.IsNotNull(v.con_Source_ParentControl_Tag_Value))
            {
                Control vCntrl = t.Find_Control_View(tForm, TableIPCode);// + MultiPageID);
                if (vCntrl != null)
                {
                    Control parent = vCntrl.Parent;
                    if (parent != null)
                    {
                        parent.Tag = v.con_Source_ParentControl_Tag_Value;

                        // burası açılacak
                        v.con_Source_ParentControl_Tag_Value = string.Empty;
                    }
                }
            }
            #endregion con_Source_ParentControl_Tag_Value
        }



        #endregion tSubWork.. 

        #region KULLANILMAYACAK FUNC

        #region tDataNavigatorList
        public void tDataNavigatorList(Form tForm, string WorkType)
        {
            tEventsButton evb = new tEventsButton();

            // WorkType == "tDataSave"
            // WorkType == "tNewData"
            // WorkType == "Detail_SubDetail_Refresh"

            #region Tanımlar

            string TableIPCode = string.Empty;
            //string MultiPageID = string.Empty;

            Control cntrl = new Control();

            string[] controls = new string[] { "DevExpress.XtraEditors.DataNavigator" };
            List<string> list = new List<string>();
            t.Find_Control_List(tForm, list, controls);

            #endregion Tanımlar

            #region DataNavigator Listesi

            foreach (string value in list)
            {
                cntrl = t.Find_Control(tForm, value, "", controls);

                if (cntrl != null)
                {
                    if (((DevExpress.XtraEditors.DataNavigator)cntrl).AccessibleName != null)
                    {
                        TableIPCode = ((DevExpress.XtraEditors.DataNavigator)cntrl).AccessibleName.ToString();

                        if (t.IsNotNull(TableIPCode))
                        {
                            #region tDataSave
                            if (WorkType == "tDataSave")
                            {
                                tSave sv = new tSave();
                                sv.tDataSave(tForm, TableIPCode);

                                // gerekipi germediğini bilmiyorum
                                //ViewControl_Enabled(tForm, dsData, TableIPCode);
                            }
                            #endregion tDataSave

                            #region order
                            if ((WorkType == "tNewData") ||
                                (WorkType == "Detail_SubDetail_Refresh"))
                            {
                                object tDataTable = ((DevExpress.XtraEditors.DataNavigator)cntrl).DataSource;
                                DataSet dsData = ((DataTable)tDataTable).DataSet;

                                string myProp = dsData.Namespace;
                                string AutoInsert = t.MyProperties_Get(myProp, "AutoInsert:");
                                string DetailSubDetail = t.MyProperties_Get(myProp, "DetailSubDetail:");

                                if (dsData != null)  //(t.IsNotNull(dsData))
                                {
                                    #region tNewData
                                    if ((WorkType == "tNewData") && (AutoInsert == "True"))
                                    {
                                        #region v.con Source_ParentControl_Tag_Value
                                        /// IPCode tanımların 
                                        /// Default value de ( Source ParentControl Tag READ ) var ise
                                        /// geçici hafıza ile buraya gelen value yi ilgili Control_View.Parent  in tag ına
                                        /// yüklemek gerekiyor
                                        /// Şimdilik bunu çözüm buldum ( İlgili : Vezne Hareketleri ekranı ) 
                                        /// Bu özellikle MultiPageID özelliği kullanılan sayfalarda 
                                        /// yeni bağımsız fiş ekranı açıldğında gerek duyuluyor
                                        /// 

                                        if (t.IsNotNull(v.con_Source_ParentControl_Tag_Value))
                                        {
                                            Control vCntrl = t.Find_Control_View(tForm, TableIPCode);// + MultiPageID);
                                            if (vCntrl != null)
                                            {
                                                Control parent = vCntrl.Parent;
                                                if (parent != null)
                                                {
                                                    parent.Tag = v.con_Source_ParentControl_Tag_Value;

                                                    v.con_Source_ParentControl_Tag_Value = string.Empty;
                                                }
                                            }
                                        }
                                        #endregion con Source_ParentControl_Tag_Value

                                        if (dsData.Tables[0].Rows.Count == 0)
                                        {
                                            /// bu tablolar Master-Detail ile bir yere bağlı olmayan tablolardır
                                            if (DetailSubDetail != "True")
                                            {
                                                //dsData.Tables[0].Namespace = "NewRecord";
                                                evb.newData(tForm, TableIPCode);
                                            }
                                        }

                                    }
                                    #endregion tNewData

                                    if (WorkType == "tNewData")
                                    {
                                        //= DetailSubDetail:True;
                                        //About_Detail_SubDetail:
                                        //= Detail_SubDetail:AVI_DOS.AVI_DOS_05 || ID ||[AVI_DCK].ICRA_DOSYA_ID || 56 || 31 || 0 |||||| 578 | ds |;
                                        if (DetailSubDetail == "True")
                                        {
                                            //
                                            t.ViewControl_Enabled(tForm, dsData, TableIPCode);
                                            // bu IPCode bağlı ExternalIPCode olabilir...
                                            t.ViewControl_Enabled_ExtarnalIP(tForm, dsData);
                                        }
                                    }

                                    #region Detail_SubDetail_Refresh
                                    if (WorkType == "Detail_SubDetail_Refresh")
                                    {
                                        if (t.IsNotNull(dsData) &&
                                            (((DevExpress.XtraEditors.DataNavigator)cntrl).Position > -1))
                                        {

                                            // önce View/Show işi yapılıyor 

                                            //propSubView(tForm, dsData, ((DevExpress.XtraEditors.DataNavigator)cntrl));

                                            // sonra database okunuyor
                                            //Application.OpenForms[0].Text = Application.OpenForms[0].Text + ";dnL_sdR1;";

                                            //tSubDetail_Refresh(dsData, ((DevExpress.XtraEditors.DataNavigator)cntrl));
                                            tSubWork_Refresh_(tForm, dsData, ((DevExpress.XtraEditors.DataNavigator)cntrl));
                                        }

                                    }
                                    #endregion Detail_SubDetail_Refresh

                                } // if (dsData != null)
                            } // if ((WorkType ==
                            #endregion order
                        }

                    }
                } // if (cntrl != null)
            }

            #endregion DataNavigator Listesi 


        }
        #endregion tDataNavigatorList

        


        #endregion KULLANILMAYACAK

    }

}
