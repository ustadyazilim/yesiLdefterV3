using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Base.ViewInfo;
using DevExpress.XtraGrid.Views.Card;
using DevExpress.XtraGrid.Views.Card.ViewInfo;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Views.Layout;
using DevExpress.XtraGrid.Views.Layout.ViewInfo;
using DevExpress.XtraGrid.Views.Tile;
using DevExpress.XtraGrid.Views.Tile.ViewInfo;
using DevExpress.XtraGrid.Views.WinExplorer.ViewInfo;
using DevExpress.XtraVerticalGrid;
using DevExpress.XtraScheduler;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Tkn_DataCopy;
using Tkn_Save;
using Tkn_ToolBox;
using Tkn_Variable;
using Tkn_Registry;
using Tkn_Search;
using DevExpress.Utils.DragDrop;
using System.ComponentModel;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;

namespace Tkn_Events
{
    public class tEventsGrid : tBase
    {
        tEvents ev = new tEvents();
        tEventsButton evb = new tEventsButton();
        tSearch se = new tSearch();
        tToolBox t = new tToolBox();
        tRegistry reg = new tRegistry();

        DevExpress.XtraGrid.Views.WinExplorer.ViewInfo.WinExplorerViewHitInfo winExplorerHitInfo = null;
        GridHitInfo gridHitInfo = null;
        TileViewHitInfo tileHitInfo = null;

        public void myRepositoryLookUpEdit_EditValueChanged(object sender, EventArgs e)
        {
            var edit = sender as DevExpress.XtraEditors.BaseEdit;

            if (edit != null)
            {
                object selectedValue = edit.EditValue;
                //Console.WriteLine($"Yeni değer: {selectedValue}");

                // Seçilen değere göre işlemler yapabilirsiniz
                // Örneğin:
                // if (selectedValue != null && selectedValue.ToString() == "1")
                // {
                //     // Belirli bir değer seçildiğinde yapılacaklar
                // }
                //MessageBox.Show("Seçim kaydedildi!");

                vGridHint tGridHint = new vGridHint();
                getGridHint_(sender, ref tGridHint);
                System.Windows.Forms.KeyEventArgs key = new KeyEventArgs(Keys.None);
                tGridHint.buttonType = v.tButtonType.btListeyeEkle;
                v.dt_SearchLookUp = null;               
                v.dt_SearchLookUp = (DataTable)((DevExpress.XtraEditors.SearchLookUpEdit)sender).Properties.DataSource;
                
                commonGridClick(sender, key, tGridHint);
            }
        }
        public void myRepositoryLookUpEdit_CloseUp(object sender, DevExpress.XtraEditors.Controls.CloseUpEventArgs e)
        {
            //if (e.AcceptValue)
            //{
            //    //MessageBox.Show("Seçim kaydedildi!");
            //}
            //else
            //{
            //    //MessageBox.Show("Seçim iptal edildi.");
            //}
        }

        public void myRepositoryItemEdit_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Basılan tuşu büyük harfe dönüştür
            e.KeyChar = char.ToUpper(e.KeyChar);
        }
        public void myRepositoryItemEdit_KeyDown(object sender, KeyEventArgs e) //*** New Ok
        {
            #region KeyCode
            if (t.findAttendantKey(e))
            {
                /// preparing tGridHint
                vGridHint tGridHint = new vGridHint();
                getGridHint_(sender, ref tGridHint);
                commonGridClick(sender, e, tGridHint);
                return;
            }
            #endregion KeyCode
        }

        public void myRepositoryItemEdit_KeyUp(object sender, KeyEventArgs e) //*** New Ok
        {
            if (t.findDirectionKey(e)) return;
            #region KeyCode
            if (t.findAttendantKey(e))
            {
                /// preparing tGridHint
                vGridHint tGridHint = new vGridHint();
                getGridHint_(sender, ref tGridHint);
                commonGridClick(sender, e, tGridHint);
                return;
            }
            if (sender.GetType().ToString() == "DevExpress.XtraEditors.ButtonEdit")
            {
                bool isSearchControl = t.IsSearchControl(sender);
                if (isSearchControl)
                {
                    vGridHint tGridHint = new vGridHint();
                    getGridHint_(sender, ref tGridHint);
                    if ((tGridHint.editValueCount == v.tSearch.searchStartCount) && (v.tSearch.IsRun == false))
                    {
                        /// Serach işlemi başlıyor
                        /// 
                        DevExpress.XtraEditors.Controls.ButtonPredefines button = DevExpress.XtraEditors.Controls.ButtonPredefines.Search;
                        //se.buttonEdit_ButtonClick_(sender, button);
                        ev.preparing_ButtonHint(sender, button);
                        v.tSearch.IsRun = false;
                    }
                } 
            }
            #endregion KeyCode
        }

        public void myRepositoryItemEdit_EditValueChanged(object sender, EventArgs e)
        {
            #region Giriş kontrolleri
            // LKP_ONAY için ise işlem olmasın
            if ((v.con_OnayChange) || (v.con_PositionChange) || (v.con_FormOpen)) return;
            // 
            if (v.con_Cancel == true)
            {
                v.con_Cancel = false;
                return;
            }

            #endregion
            
            vGridHint tGridHint = new vGridHint();
            getGridHint_(sender, ref tGridHint);
            if (t.IsNotNull(tGridHint.columnPropNavigator))
            {
                if (tGridHint.dataNavigator.Position > -1)
                {
                    tGridHint.dataSet.Tables[0].Rows[tGridHint.dataNavigator.Position][tGridHint.columnFieldName] = tGridHint.columnEditValue;
                    tGridHint.dataSet.AcceptChanges();
                    // Keys.None == v.tButtonType.btNoneButton; olacak
                    System.Windows.Forms.KeyEventArgs key = new KeyEventArgs(Keys.None);
                    commonGridClick(sender, key, tGridHint);
                }
            }
            
        }

        public bool commonGridClick(object sender, KeyEventArgs e, vGridHint tGridHint) // my_KeyDown(object sender, KeyEventArgs e) // MAIN ***
        {
            bool onay = false;

            // işlem yapmadan dön
            if (e.Handled)
                return onay;

            Form tForm = tGridHint.tForm;
            if (tForm == null)
            {
                MessageBox.Show("DİKKAT : commonGridClick içinde ( tForm == null ) olduğu için işleme devam edemiyorum... ");
                return onay;
            }
            string tableIPCode = tGridHint.tableIPCode;
            string propNavigator = "";
            string buttonName = "";
            int searchEngine = -1;
            bool searchOnayi = false;

            v.tButtonType buttonType = v.tButtonType.btNone;

            v.tButtonHint.Clear();
            v.tButtonHint.tForm = tGridHint.tForm;
            v.tButtonHint.tableIPCode = tGridHint.tableIPCode;
            v.tButtonHint.propNavigator = t.Set(tGridHint.columnPropNavigator, tGridHint.gridPropNavigator, "");
            v.tButtonHint.columnFieldName = tGridHint.columnFieldName;
            v.tButtonHint.columnOldValue = tGridHint.columnOldValue;
            v.tButtonHint.columnEditValue = tGridHint.columnEditValue;
            v.tButtonHint.parentObject = tGridHint.parentObject;
            v.tButtonHint.sender = tGridHint.view;
            v.tButtonHint.senderType = tGridHint.viewType;
            v.tButtonHint.buttonType = tGridHint.buttonType;
            v.tButtonHint.viewType = tGridHint.viewType;

            if (v.tButtonHint.buttonType == v.tButtonType.btNone)
            {
                buttonType = v.tButtonHint.buttonType = ev.getClickType(tGridHint.tForm, tGridHint.tableIPCode, e, ref propNavigator, ref buttonName);

                if (propNavigator != "" && e.KeyCode != Keys.None)
                    v.tButtonHint.propNavigator = propNavigator;
                if (buttonName != "")
                    v.tButtonHint.buttonName = buttonName;
            }
            else
            {
                //ev.getClickType(tGridHint.tForm, tGridHint.tableIPCode, e, ref propNavigator, ref buttonName);

                buttonType = v.tButtonHint.buttonType;
            }

            // iptal edildi
            //if (v.tButtonHint.buttonType == v.tButtonType.btNone)
            //    v.tButtonHint.buttonType = ev.getClickType(e);
                        
            // editValue ile oldValue bir türlü set dataSet e set olmuyor
            // control üzerinde değişiklik yapıp o controlda çıkış yapmadıkca dataSet bir türlü newValue ye ulaşamıyor
            // 
            if (((buttonType == v.tButtonType.btKaydet) ||
                 (buttonType == v.tButtonType.btKaydetCik) ||
                 (buttonType == v.tButtonType.btKaydetDevam) ||
                 (buttonType == v.tButtonType.btKaydetYeni)) && 
                (tGridHint.parentObject == "DevExpress.XtraDataLayout.DataLayoutControl"))
            {
                DataSet dsData = null;
                DataNavigator dN = null;
                t.Find_DataSet(tGridHint.tForm, ref dsData, ref dN, tGridHint.tableIPCode);
                
                if (dsData != null)
                {
                    dsData.Tables[0].Rows[dN.Position][tGridHint.columnFieldName] = tGridHint.columnEditValue;
                }
                /* bu da yemedi
                Control cntrl = t.Find_Control_View(tGridHint.tForm, tGridHint.tableIPCode);
                if (cntrl != null)
                {
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                }
                */
            }

            if ((e.KeyCode == Keys.Enter) |
                (e.KeyCode == Keys.Return))// |  (buttonType == v.tButtonType.btEnter)
            {
                if ((tGridHint.parentObject == "DevExpress.XtraDataLayout.DataLayoutControl") &&
                    (buttonType == v.tButtonType.btSecCik))
                {
                    return onay;
                }

                if ((tGridHint.parentObject == "DevExpress.XtraGrid.GridControl") &&
                    (buttonType == v.tButtonType.btNone) &&
                    (getIsLastColumn(tGridHint))
                    )
                {
                    bool IsLatRow = getIsLastRow(tGridHint);

                    if (IsLatRow)
                        buttonType = v.tButtonType.btKaydetYeni;
                    else buttonType = v.tButtonType.btKaydet;

                    v.tButtonHint.buttonType = buttonType;
                }

                v.con_Value_Old = tGridHint.columnOldValue;
                v.con_Value_New = tGridHint.columnEditValue;

                if (tGridHint.columnPropNavigator != "")
                    searchEngine = tGridHint.columnPropNavigator.IndexOf("SearchEngine");

                //searchOnayi = t.IsSearchControl(sender);

                //if ((searchEngine > -1) && searchOnayi && tGridHint.columnEditValue != "" && tGridHint.columnEditValue != tGridHint.columnOldValue)
                //{
                //    // searchformunu  açmadan önce value yi kontrol et 
                //    // kontrol sonucu olumsuz ise formu aç
                //    //if (se.directSearch(tGridHint.tForm, tGridHint.tableIPCode, tGridHint.columnPropNavigator, v.con_Value_Old) == false)

                //    //if (se.directSearch(v.tButtonHint) == false)
                //    //{
                //    //    e.Handled = false;
                //    //}
                //    e.Handled = false;
                //}
                                
                // buttonType == v.tButtonType.btEnter gidip 
                // başka bir type ile dönebilir
                //
                if (searchEngine == -1)
                {
                    // gerek kalmadı  Enter tuşu için başka görev arıyordu

                    Control button = null;
                    propNavigator = "";
                    onay = findButton(tForm, tableIPCode, ref button, ref buttonType, ref propNavigator);
                    v.tButtonHint.buttonType = buttonType;
                    v.tButtonHint.propNavigator = propNavigator;
                }
            }

            // enter ait bir görev yoksa buradan geri dön
            if ((v.tButtonHint.buttonType == v.tButtonType.btNone) &&
                (tGridHint.columnPropNavigator == "") &&
                (e.KeyCode == Keys.Enter))
                return onay;


            if (e.KeyCode == v.Key_SearchEngine)
            {
                onay = gridViewKeySearch(sender, tGridHint);

                v.tButtonHint.columnEditValue = tGridHint.columnEditValue;
                v.tButtonHint.columnOldValue = tGridHint.columnOldValue;
                v.con_Value_Old = tGridHint.columnOldValue;
                v.tSearch.searchInputValue = tGridHint.columnEditValue;

                v.tButtonHint.propNavigator = t.Set(tGridHint.columnPropNavigator, tGridHint.gridPropNavigator, "");

                e.Handled = onay;
            }

            if (e.KeyCode == Keys.Escape)
            {
                keyEscape(sender, tGridHint);
                buttonType = v.tButtonType.btNone;
                e.Handled = true;
                onay = true;
                return onay;
            }

            if ((e.KeyCode == Keys.Down) && (e.Modifiers == Keys.Shift))
            {
                v.tButtonHint.buttonType = v.tButtonType.btKaydetYeni;
                e.Handled = true;
                //gridViewSetFirstColumn(sender, tGridHint);

            }

            //if (e.KeyCode == Keys.Delete && e.Modifiers == Keys.Control)
            //{
            //    if (MessageBox.Show("Satır Sil ? ", "Silme işlemi için onay", MessageBoxButtons.YesNo) != DialogResult.Yes)
            //        return onay; 
            //    view.DeleteRow(view.FocusedRowHandle);
            //}

            if (v.tButtonHint.buttonType != v.tButtonType.btNone)
            {
                tEventsButton evb = new tEventsButton();
                return evb.btnClick(v.tButtonHint);
            }

            return onay;
        }


        #region myGridView Events

        #region Sub Functions / 201911


        public GridView getGridView(vGridHint tGridHint)
        {
            if (tGridHint.view == null)
            {
                MessageBox.Show("getGridView() : tGridHint.view == null");
                return null;
            }

            GridView view = null;
            //object view = null;

            if (tGridHint.view.GetType().ToString() == "DevExpress.XtraGrid.Views.Grid.GridView")
                view = (DevExpress.XtraGrid.Views.Grid.GridView)tGridHint.view;

            if (tGridHint.view.GetType().ToString() == "DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView")
                view = (DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)tGridHint.view;

            if (tGridHint.view.GetType().ToString() == "DevExpress.XtraGrid.Views.Card.CardView")
            {
                view = null; // (DevExpress.XtraGrid.Views.Card.CardView.CardView)tGridHint.view;
                // aşağıdaki getCardView() gitmesi gerekiyor
            }

            return view;
        }

        public CardView getCardView(vGridHint tGridHint)
        {
            if (tGridHint.view == null)
            {
                MessageBox.Show("getCardView() : tGridHint.view == null");
                return null;
            }

            CardView view = null;

            if (tGridHint.view.GetType().ToString() == "DevExpress.XtraGrid.Views.Card.CardView")
                view = (DevExpress.XtraGrid.Views.Card.CardView)tGridHint.view;

            return view;
        }


        public void getGridHint_(Form tForm, string tableIPCode, ref vGridHint tGridHint)
        {
            Control cntrl = null;
            cntrl = t.Find_Control_View(tForm, tableIPCode);

            if (cntrl != null)
            {
                getGridHint_(cntrl, ref tGridHint);
            }
        }

        public void getGridHint_(object sender, ref vGridHint tGridHint)
        {
            tGridHint.Clear();

            getGridFormAndTableIPCode(sender, ref tGridHint);

            if (tGridHint.columnFieldName != "")
                tGridHint.columnFieldName = tGridHint.columnFieldName.Replace("Column_", "");

            string oldValue = "";
            string editValue = "";

            if (sender.GetType().ToString() == "DevExpress.XtraGrid.GridControl" )
            {
                var newSender = ((DevExpress.XtraGrid.GridControl)sender).FocusedView;
                sender = null;
                sender = newSender;
            }

            if (sender.GetType().ToString() == "DevExpress.XtraGrid.Views.Grid.GridView")
            {
                //if ((((DevExpress.XtraGrid.Views.Grid.GridView)sender).GridControl).AccessibleDescription != null)
                //    tGridHint.grid_Prop_Navigator = (((DevExpress.XtraGrid.Views.Grid.GridView)sender).GridControl).AccessibleDescription;

                tGridHint.focusedRow = ((DevExpress.XtraGrid.Views.Grid.GridView)sender).GetFocusedRow();
                
                if (((DevExpress.XtraGrid.Views.Grid.GridView)sender).FocusedColumn != null)
                {
                    if (((DevExpress.XtraGrid.Views.Grid.GridView)sender).FocusedColumn.ColumnEdit != null)
                       if (((DevExpress.XtraGrid.Views.Grid.GridView)sender).FocusedColumn.ColumnEdit.AccessibleDescription != null)
                          tGridHint.columnPropNavigator = ((DevExpress.XtraGrid.Views.Grid.GridView)sender).FocusedColumn.ColumnEdit.AccessibleDescription;
                }

                if ((((DevExpress.XtraGrid.Views.Grid.GridView)sender).GridControl).AccessibleDescription != null)
                {
                    tGridHint.gridPropNavigator = (((DevExpress.XtraGrid.Views.Grid.GridView)sender).GridControl).AccessibleDescription.ToString();
                }

                //if (tGridHint.columnValue != null)
                //    if ((tGridHint.columnValue != "") &&
                if (((DevExpress.XtraGrid.Views.Grid.GridView)sender).FocusedValue != null)
                        oldValue = ((DevExpress.XtraGrid.Views.Grid.GridView)sender).GetFocusedValue().ToString();

                if (((DevExpress.XtraGrid.Views.Grid.GridView)sender).EditingValue != null)
                    editValue = ((DevExpress.XtraGrid.Views.Grid.GridView)sender).EditingValue.ToString();

                tGridHint.parentObject = sender.GetType().ToString();

                tGridHint.columnOldValue = oldValue.Trim();
                tGridHint.columnEditValue = editValue.Trim();
            }

            if (sender.GetType().ToString() == "DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView")
            {
                tGridHint.focusedRow = ((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender).GetFocusedRow();

                if (((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender).FocusedColumn != null)
                {
                    if (((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender).FocusedColumn.ColumnEdit != null)
                        if (((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender).FocusedColumn.ColumnEdit.AccessibleDescription != null)
                            tGridHint.columnPropNavigator = ((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender).FocusedColumn.ColumnEdit.AccessibleDescription;
                }

                if ((((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender).GridControl).AccessibleDescription != null)
                {
                    tGridHint.gridPropNavigator = (((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender).GridControl).AccessibleDescription.ToString();
                }

                //if (tGridHint.columnValue != null)
                //    if ((tGridHint.columnValue != "") &&
                if (((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender).FocusedValue != null)
                        oldValue = ((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender).GetFocusedValue().ToString();

                if (((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender).EditingValue != null)
                    editValue = ((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender).EditingValue.ToString();

                tGridHint.parentObject = sender.GetType().ToString();

                tGridHint.columnOldValue = oldValue.Trim();
                tGridHint.columnEditValue = editValue.Trim();
            }

            if (sender.GetType().ToString() == "DevExpress.XtraGrid.Views.Card.CardView")
            {
                if (((DevExpress.XtraGrid.Views.Card.CardView)sender) != null)
                {
                    tGridHint.focusedRow = ((DevExpress.XtraGrid.Views.Card.CardView)sender).GetFocusedRow();

                    if (((DevExpress.XtraGrid.Views.Card.CardView)sender).FocusedColumn != null)
                    {
                        if (((DevExpress.XtraGrid.Views.Card.CardView)sender).FocusedColumn.ColumnEdit != null)
                            if (((DevExpress.XtraGrid.Views.Card.CardView)sender).FocusedColumn.ColumnEdit.AccessibleDescription != null)
                                tGridHint.columnPropNavigator = ((DevExpress.XtraGrid.Views.Card.CardView)sender).FocusedColumn.ColumnEdit.AccessibleDescription;
                    }

                    if ((((DevExpress.XtraGrid.Views.Card.CardView)sender).GridControl).AccessibleDescription != null)
                    {
                        tGridHint.gridPropNavigator = (((DevExpress.XtraGrid.Views.Card.CardView)sender).GridControl).AccessibleDescription.ToString();
                    }

                    //if (tGridHint.columnValue != null)
                    //    if ((tGridHint.columnValue != "") &&
                    if (((DevExpress.XtraGrid.Views.Card.CardView)sender).FocusedValue != null)
                        oldValue = ((DevExpress.XtraGrid.Views.Card.CardView)sender).GetFocusedValue().ToString();

                    if (((DevExpress.XtraGrid.Views.Card.CardView)sender).EditingValue != null)
                        editValue = ((DevExpress.XtraGrid.Views.Card.CardView)sender).EditingValue.ToString();

                    tGridHint.parentObject = sender.GetType().ToString();

                    tGridHint.columnOldValue = oldValue.Trim();
                    tGridHint.columnEditValue = editValue.Trim();
                }
            }

            if (sender.GetType().ToString() == "DevExpress.XtraGrid.Views.Tile.TileView")
            {
                if (((DevExpress.XtraGrid.Views.Tile.TileView)sender) != null)
                {
                    tGridHint.focusedRow = ((DevExpress.XtraGrid.Views.Tile.TileView)sender).GetFocusedRow();

                    if ((((DevExpress.XtraGrid.Views.Tile.TileView)sender).GridControl).AccessibleDescription != null)
                    {
                        tGridHint.gridPropNavigator = (((DevExpress.XtraGrid.Views.Tile.TileView)sender).GridControl).AccessibleDescription.ToString();
                    }

                    //tGridHint.parentObject = sender.GetType().ToString();
                }
            }

            if (sender.GetType().ToString() == "DevExpress.XtraEditors.ButtonEdit")
            {
                // parent ile GridControl geliyor 
                //MessageBox.Show(((DevExpress.XtraEditors.ButtonEdit)sender).Parent.Name.ToString());

                if (((DevExpress.XtraEditors.ButtonEdit)sender).Properties.AccessibleDescription != null)
                    tGridHint.columnPropNavigator = ((DevExpress.XtraEditors.ButtonEdit)sender).Properties.AccessibleDescription;
                
                if (((DevExpress.XtraEditors.ButtonEdit)sender).OldEditValue != null)
                    oldValue = ((DevExpress.XtraEditors.ButtonEdit)sender).OldEditValue.ToString();

                if (((DevExpress.XtraEditors.ButtonEdit)sender).EditValue != null)
                    editValue = ((DevExpress.XtraEditors.ButtonEdit)sender).EditValue.ToString();
                
                tGridHint.parentObject = ((DevExpress.XtraEditors.ButtonEdit)sender).Parent.GetType().ToString();

                tGridHint.columnOldValue = oldValue.Trim();
                tGridHint.columnEditValue = editValue.Trim();
            }

            if (sender.GetType().ToString() == "DevExpress.XtraEditors.ImageComboBoxEdit")
            {
                if (((DevExpress.XtraEditors.ImageComboBoxEdit)sender).Properties.AccessibleDescription != null)
                    tGridHint.columnPropNavigator = ((DevExpress.XtraEditors.ImageComboBoxEdit)sender).Properties.AccessibleDescription;

                if (((DevExpress.XtraEditors.ImageComboBoxEdit)sender).OldEditValue != null)
                    oldValue = ((DevExpress.XtraEditors.ImageComboBoxEdit)sender).OldEditValue.ToString();

                if (((DevExpress.XtraEditors.ImageComboBoxEdit)sender).EditValue != null)
                    editValue = ((DevExpress.XtraEditors.ImageComboBoxEdit)sender).EditValue.ToString();

                tGridHint.parentObject = ((DevExpress.XtraEditors.ImageComboBoxEdit)sender).Parent.GetType().ToString();

                tGridHint.columnOldValue = oldValue.Trim();
                tGridHint.columnEditValue = editValue.Trim();
            }

            if (sender.GetType().ToString() == "DevExpress.XtraEditors.TextEdit")
            {
                if (((DevExpress.XtraEditors.TextEdit)sender).Properties.AccessibleDescription != null)
                    tGridHint.columnPropNavigator = ((DevExpress.XtraEditors.TextEdit)sender).Properties.AccessibleDescription;

                
                if (((DevExpress.XtraEditors.TextEdit)sender).OldEditValue != null)
                    oldValue = ((DevExpress.XtraEditors.TextEdit)sender).OldEditValue.ToString();

                if (((DevExpress.XtraEditors.TextEdit)sender).EditValue != null)
                    editValue = ((DevExpress.XtraEditors.TextEdit)sender).EditValue.ToString();
                
                tGridHint.parentObject = ((DevExpress.XtraEditors.TextEdit)sender).Parent.GetType().ToString();
                
                tGridHint.columnOldValue = oldValue.Trim();
                tGridHint.columnEditValue = editValue.Trim();
            }

            if (sender.GetType().ToString() == "DevExpress.XtraEditors.TimeEdit")
            {
                if (((DevExpress.XtraEditors.TimeEdit)sender).Properties.AccessibleDescription != null)
                    tGridHint.columnPropNavigator = ((DevExpress.XtraEditors.TimeEdit)sender).Properties.AccessibleDescription;


                if (((DevExpress.XtraEditors.TimeEdit)sender).OldEditValue != null)
                    oldValue = ((DevExpress.XtraEditors.TimeEdit)sender).OldEditValue.ToString();

                if (((DevExpress.XtraEditors.TimeEdit)sender).EditValue != null)
                    editValue = ((DevExpress.XtraEditors.TimeEdit)sender).EditValue.ToString();

                tGridHint.parentObject = ((DevExpress.XtraEditors.TimeEdit)sender).Parent.GetType().ToString();

                tGridHint.columnOldValue = oldValue.Trim();
                tGridHint.columnEditValue = editValue.Trim();
            }


            //RepositoryItemSearchLookUpEdit

            if (sender.GetType().ToString() == "DevExpress.XtraEditors.SearchLookUpEdit")
            {
                if (((DevExpress.XtraEditors.SearchLookUpEdit)sender).Properties.AccessibleDescription != null)
                    tGridHint.columnPropNavigator = ((DevExpress.XtraEditors.SearchLookUpEdit)sender).Properties.AccessibleDescription;

                if (((DevExpress.XtraEditors.SearchLookUpEdit)sender).OldEditValue != null)
                    oldValue = ((DevExpress.XtraEditors.SearchLookUpEdit)sender).OldEditValue.ToString();

                if (((DevExpress.XtraEditors.SearchLookUpEdit)sender).EditValue != null)
                    editValue = ((DevExpress.XtraEditors.SearchLookUpEdit)sender).EditValue.ToString();
                
                tGridHint.parentObject = ((DevExpress.XtraEditors.SearchLookUpEdit)sender).Parent.GetType().ToString();

                tGridHint.columnOldValue = oldValue.Trim();
                tGridHint.columnEditValue = editValue.Trim();
            }
            
            tGridHint.editValueCount = tGridHint.columnEditValue.Length;


            /// SİLME güzel bir alternatif yolu
            //// Sender'ı BaseEdit'e dönüştür
            //var edit = sender as DevExpress.XtraEditors.BaseEdit;

            //if (edit != null)
            //{
            //    object selectedValue = edit.EditValue;
            //    Console.WriteLine($"Yeni değer: {selectedValue}");

            //    // Seçilen değere göre işlemler yapabilirsiniz
            //    // Örneğin:
            //    // if (selectedValue != null && selectedValue.ToString() == "1")
            //    // {
            //    //     // Belirli bir değer seçildiğinde yapılacaklar
            //    // }
            //}

        }

        public void getGridFormAndTableIPCode(object sender, ref vGridHint tGridHint)
        {
            //bool kontrol = false;
            string oldValue = "";
            string editValue = "";
            string senderType = sender.GetType().ToString();
            DataSet dsData = null;
            DataNavigator dN = null;

            if (senderType == "DevExpress.XtraGrid.GridControl")
            {
                sender = ((DevExpress.XtraGrid.GridControl)sender).MainView;
                senderType = sender.GetType().ToString();
            }

            if (senderType == "DevExpress.XtraEditors.ButtonEdit")
            {
                tGridHint.viewType = "ButtonEdit";

                if (((DevExpress.XtraEditors.ButtonEdit)sender).Parent != null)
                {
                    senderType = ((DevExpress.XtraEditors.ButtonEdit)sender).Parent.GetType().ToString();

                    Control parentControl = ((DevExpress.XtraEditors.ButtonEdit)sender).Parent;

                    // yer değişikliği
                    if (parentControl.GetType().ToString() == "DevExpress.XtraGrid.GridControl")
                    {
                        sender = ((DevExpress.XtraGrid.GridControl)parentControl).MainView;
                        senderType = sender.GetType().ToString();
                    }
                    else
                    {
                        tGridHint.tForm = ((DevExpress.XtraEditors.ButtonEdit)sender).FindForm();
                        tGridHint.tableIPCode = ((DevExpress.XtraEditors.ButtonEdit)sender).AccessibleName;
                        tGridHint.view = sender;
                        tGridHint.columnFieldName = ((DevExpress.XtraEditors.ButtonEdit)sender).Name.ToString();
                        tGridHint.parentObject = ((DevExpress.XtraEditors.ButtonEdit)sender).Parent.GetType().ToString();

                        if (((DevExpress.XtraEditors.ButtonEdit)sender).OldEditValue != null)
                            oldValue = ((DevExpress.XtraEditors.ButtonEdit)sender).OldEditValue.ToString();

                        if (((DevExpress.XtraEditors.ButtonEdit)sender).EditValue != null)
                            editValue = ((DevExpress.XtraEditors.ButtonEdit)sender).EditValue.ToString();

                        //return;
                    }
                }
            }

            if (senderType == "DevExpress.XtraEditors.CalcEdit")
            {
                tGridHint.viewType = "CalcEdit";

                if (((DevExpress.XtraEditors.CalcEdit)sender).Parent != null)
                {
                    senderType = ((DevExpress.XtraEditors.CalcEdit)sender).Parent.GetType().ToString();

                    Control parentControl = ((DevExpress.XtraEditors.CalcEdit)sender).Parent;

                    // yer değişikliği
                    if (parentControl.GetType().ToString() == "DevExpress.XtraGrid.GridControl")
                    {
                        sender = ((DevExpress.XtraGrid.GridControl)parentControl).MainView;
                        senderType = sender.GetType().ToString();
                    }
                    else
                    {
                        tGridHint.tForm = ((DevExpress.XtraEditors.CalcEdit)sender).FindForm();
                        tGridHint.tableIPCode = ((DevExpress.XtraEditors.CalcEdit)sender).AccessibleName;
                        tGridHint.view = sender;
                        tGridHint.columnFieldName = ((DevExpress.XtraEditors.CalcEdit)sender).Name.ToString();
                        tGridHint.parentObject = ((DevExpress.XtraEditors.CalcEdit)sender).Parent.GetType().ToString();

                        if (((DevExpress.XtraEditors.CalcEdit)sender).OldEditValue != null)
                            oldValue = ((DevExpress.XtraEditors.CalcEdit)sender).OldEditValue.ToString();

                        if (((DevExpress.XtraEditors.CalcEdit)sender).EditValue != null)
                            editValue = ((DevExpress.XtraEditors.CalcEdit)sender).EditValue.ToString();

                        //return;
                    }
                }
            }

            if (senderType == "DevExpress.XtraEditors.DateEdit")
            {
                tGridHint.viewType = "DateEdit";

                if (((DevExpress.XtraEditors.DateEdit)sender).Parent != null)
                {
                    senderType = ((DevExpress.XtraEditors.DateEdit)sender).Parent.GetType().ToString();

                    Control parentControl = ((DevExpress.XtraEditors.DateEdit)sender).Parent;

                    // yer değişikliği
                    if (parentControl.GetType().ToString() == "DevExpress.XtraGrid.GridControl")
                    {
                        sender = ((DevExpress.XtraGrid.GridControl)parentControl).MainView;
                        senderType = sender.GetType().ToString();
                    }
                    else
                    {
                        tGridHint.tForm = ((DevExpress.XtraEditors.DateEdit)sender).FindForm();
                        tGridHint.tableIPCode = ((DevExpress.XtraEditors.DateEdit)sender).AccessibleName;
                        tGridHint.view = sender;
                        tGridHint.columnFieldName = ((DevExpress.XtraEditors.DateEdit)sender).Name.ToString();
                        tGridHint.parentObject = ((DevExpress.XtraEditors.DateEdit)sender).Parent.GetType().ToString();

                        if (((DevExpress.XtraEditors.DateEdit)sender).OldEditValue != null)
                            oldValue = ((DevExpress.XtraEditors.DateEdit)sender).OldEditValue.ToString();

                        if (((DevExpress.XtraEditors.DateEdit)sender).EditValue != null)
                            editValue = ((DevExpress.XtraEditors.DateEdit)sender).EditValue.ToString();

                        //return;
                    }
                }
            }

            if (senderType == "DevExpress.XtraEditors.ImageComboBoxEdit")
            {
                tGridHint.viewType = "ImageComboBoxEdit";

                if (((DevExpress.XtraEditors.ImageComboBoxEdit)sender).Parent != null)
                {
                    senderType = ((DevExpress.XtraEditors.ImageComboBoxEdit)sender).Parent.GetType().ToString();

                    Control parentControl = ((DevExpress.XtraEditors.ImageComboBoxEdit)sender).Parent;

                    // yer değişikliği
                    if (parentControl.GetType().ToString() == "DevExpress.XtraGrid.GridControl")
                    {
                        sender = ((DevExpress.XtraGrid.GridControl)parentControl).MainView;
                        senderType = sender.GetType().ToString();
                    }
                    else
                    {
                        tGridHint.tForm = ((DevExpress.XtraEditors.ImageComboBoxEdit)sender).FindForm();
                        tGridHint.tableIPCode = ((DevExpress.XtraEditors.ImageComboBoxEdit)sender).AccessibleName;
                        tGridHint.view = sender;
                        tGridHint.columnFieldName = ((DevExpress.XtraEditors.ImageComboBoxEdit)sender).Name.ToString();
                        tGridHint.parentObject = ((DevExpress.XtraEditors.ImageComboBoxEdit)sender).Parent.GetType().ToString();

                        if (((DevExpress.XtraEditors.ImageComboBoxEdit)sender).OldEditValue != null)
                            oldValue = ((DevExpress.XtraEditors.ImageComboBoxEdit)sender).OldEditValue.ToString();

                        if (((DevExpress.XtraEditors.ImageComboBoxEdit)sender).EditValue != null)
                            editValue = ((DevExpress.XtraEditors.ImageComboBoxEdit)sender).EditValue.ToString();

                        //return;
                    }
                }
            }

            if (senderType == "DevExpress.XtraEditors.TextEdit")
            {
                tGridHint.viewType = "TextEdit";

                if (((DevExpress.XtraEditors.TextEdit)sender).Parent != null)
                {
                    senderType = ((DevExpress.XtraEditors.TextEdit)sender).Parent.GetType().ToString();

                    Control parentControl = ((DevExpress.XtraEditors.TextEdit)sender).Parent;

                    // yer değişikliği
                    if (parentControl.GetType().ToString() == "DevExpress.XtraGrid.GridControl")
                    {
                        sender = ((DevExpress.XtraGrid.GridControl)parentControl).MainView;
                        senderType = sender.GetType().ToString();
                    }
                    else
                    {
                        tGridHint.tForm = ((DevExpress.XtraEditors.TextEdit)sender).FindForm();
                        tGridHint.tableIPCode = ((DevExpress.XtraEditors.TextEdit)sender).AccessibleName;
                        tGridHint.view = sender;
                        tGridHint.columnFieldName = ((DevExpress.XtraEditors.TextEdit)sender).Name.ToString();
                        tGridHint.parentObject = ((DevExpress.XtraEditors.TextEdit)sender).Parent.GetType().ToString();

                        if (((DevExpress.XtraEditors.TextEdit)sender).OldEditValue != null)
                            oldValue = ((DevExpress.XtraEditors.TextEdit)sender).OldEditValue.ToString();

                        if (((DevExpress.XtraEditors.TextEdit)sender).EditValue != null)
                            editValue = ((DevExpress.XtraEditors.TextEdit)sender).EditValue.ToString();

                        //return;
                    }
                }
            }

            if (senderType == "DevExpress.XtraEditors.SearchLookUpEdit")
            {
                tGridHint.viewType = "SearchLookUpEdit";

                /// Sebebini bilmiyorum EditValueChanged çalışırken Name boş geliyor
                /// bu nedenle fieldName yi AccessibleDefaultActionDescription üzerinden taşımak zorunda kaldım
                /// 
                tGridHint.columnFieldName = ((DevExpress.XtraEditors.SearchLookUpEdit)sender).Name.ToString();
                tGridHint.columnFieldName = ((DevExpress.XtraEditors.SearchLookUpEdit)sender).AccessibleDefaultActionDescription.ToString();


                if (((DevExpress.XtraEditors.SearchLookUpEdit)sender).Parent != null)
                {
                    senderType = ((DevExpress.XtraEditors.SearchLookUpEdit)sender).Parent.GetType().ToString();

                    Control parentControl = ((DevExpress.XtraEditors.SearchLookUpEdit)sender).Parent;

                    // yer değişikliği
                    if (parentControl.GetType().ToString() == "DevExpress.XtraGrid.GridControl")
                    {
                        sender = ((DevExpress.XtraGrid.GridControl)parentControl).MainView;
                        senderType = sender.GetType().ToString();
                    }
                    else
                    {
                        tGridHint.tForm = ((DevExpress.XtraEditors.SearchLookUpEdit)sender).FindForm();
                        tGridHint.tableIPCode = ((DevExpress.XtraEditors.SearchLookUpEdit)sender).AccessibleName;
                        tGridHint.view = sender;
                        tGridHint.columnFieldName = ((DevExpress.XtraEditors.SearchLookUpEdit)sender).Name.ToString();
                        tGridHint.parentObject = ((DevExpress.XtraEditors.SearchLookUpEdit)sender).Parent.GetType().ToString();

                        if (((DevExpress.XtraEditors.SearchLookUpEdit)sender).OldEditValue != null)
                            oldValue = ((DevExpress.XtraEditors.SearchLookUpEdit)sender).OldEditValue.ToString();

                        if (((DevExpress.XtraEditors.SearchLookUpEdit)sender).EditValue != null)
                            editValue = ((DevExpress.XtraEditors.SearchLookUpEdit)sender).EditValue.ToString();

                        //return;
                    }
                }

            }

            if (senderType == "DevExpress.XtraEditors.TimeEdit")
            {
                tGridHint.viewType = "TimeEdit";

                if (((DevExpress.XtraEditors.TimeEdit)sender).Parent != null)
                {
                    senderType = ((DevExpress.XtraEditors.TimeEdit)sender).Parent.GetType().ToString();

                    Control parentControl = ((DevExpress.XtraEditors.TimeEdit)sender).Parent;

                    // yer değişikliği
                    if (parentControl.GetType().ToString() == "DevExpress.XtraGrid.GridControl")
                    {
                        sender = ((DevExpress.XtraGrid.GridControl)parentControl).MainView;
                        senderType = sender.GetType().ToString();
                    }
                    else
                    {
                        tGridHint.tForm = ((DevExpress.XtraEditors.TimeEdit)sender).FindForm();
                        tGridHint.tableIPCode = ((DevExpress.XtraEditors.TimeEdit)sender).AccessibleName;
                        tGridHint.view = sender;
                        tGridHint.columnFieldName = ((DevExpress.XtraEditors.TimeEdit)sender).Name.ToString();
                        tGridHint.parentObject = ((DevExpress.XtraEditors.TimeEdit)sender).Parent.GetType().ToString();

                        if (((DevExpress.XtraEditors.TimeEdit)sender).OldEditValue != null)
                            oldValue = ((DevExpress.XtraEditors.TimeEdit)sender).OldEditValue.ToString();

                        if (((DevExpress.XtraEditors.TimeEdit)sender).EditValue != null)
                            editValue = ((DevExpress.XtraEditors.TimeEdit)sender).EditValue.ToString();
                    }
                }
            }

            if (senderType == "DevExpress.XtraEditors.TimeSpanEdit")
            {
                tGridHint.viewType = "TimeSpanEdit";

                if (((DevExpress.XtraEditors.TimeSpanEdit)sender).Parent != null)
                {
                    senderType = ((DevExpress.XtraEditors.TimeSpanEdit)sender).Parent.GetType().ToString();

                    Control parentControl = ((DevExpress.XtraEditors.TimeSpanEdit)sender).Parent;

                    // yer değişikliği
                    if (parentControl.GetType().ToString() == "DevExpress.XtraGrid.GridControl")
                    {
                        sender = ((DevExpress.XtraGrid.GridControl)parentControl).MainView;
                        senderType = sender.GetType().ToString();
                    }
                    else
                    {
                        tGridHint.tForm = ((DevExpress.XtraEditors.TimeSpanEdit)sender).FindForm();
                        tGridHint.tableIPCode = ((DevExpress.XtraEditors.TimeSpanEdit)sender).AccessibleName;
                        tGridHint.view = sender;
                        tGridHint.columnFieldName = ((DevExpress.XtraEditors.TimeSpanEdit)sender).Name.ToString();
                        tGridHint.parentObject = ((DevExpress.XtraEditors.TimeSpanEdit)sender).Parent.GetType().ToString();

                        if (((DevExpress.XtraEditors.TimeSpanEdit)sender).OldEditValue != null)
                            oldValue = ((DevExpress.XtraEditors.TimeSpanEdit)sender).OldEditValue.ToString();

                        if (((DevExpress.XtraEditors.TimeSpanEdit)sender).EditValue != null)
                            editValue = ((DevExpress.XtraEditors.TimeSpanEdit)sender).EditValue.ToString();

                        //return;
                    }
                }
            }

            if (senderType == "DevExpress.XtraEditors.SpinEdit")
            {
                tGridHint.viewType = "SpinEdit";

                if (((DevExpress.XtraEditors.SpinEdit)sender).Parent != null)
                {
                    senderType = ((DevExpress.XtraEditors.SpinEdit)sender).Parent.GetType().ToString();

                    Control parentControl = ((DevExpress.XtraEditors.SpinEdit)sender).Parent;

                    // yer değişikliği
                    if (parentControl.GetType().ToString() == "DevExpress.XtraGrid.GridControl")
                    {
                        sender = ((DevExpress.XtraGrid.GridControl)parentControl).MainView;
                        senderType = sender.GetType().ToString();
                    }
                    else
                    {
                        tGridHint.tForm = ((DevExpress.XtraEditors.SpinEdit)sender).FindForm();
                        tGridHint.tableIPCode = ((DevExpress.XtraEditors.SpinEdit)sender).AccessibleName;
                        tGridHint.view = sender;
                        tGridHint.columnFieldName = ((DevExpress.XtraEditors.SpinEdit)sender).Name.ToString();
                        tGridHint.parentObject = ((DevExpress.XtraEditors.SpinEdit)sender).Parent.GetType().ToString();

                        if (((DevExpress.XtraEditors.SpinEdit)sender).OldEditValue != null)
                            oldValue = ((DevExpress.XtraEditors.SpinEdit)sender).OldEditValue.ToString();

                        if (((DevExpress.XtraEditors.SpinEdit)sender).EditValue != null)
                            editValue = ((DevExpress.XtraEditors.SpinEdit)sender).EditValue.ToString();

                        //return;
                    }
                }
            }

            if (senderType == "DevExpress.XtraGrid.Views.Grid.GridView")
            {
                if ((((DevExpress.XtraGrid.Views.Grid.GridView)sender).GridControl) != null)
                {
                    tGridHint.tForm = (((DevExpress.XtraGrid.Views.Grid.GridView)sender).GridControl).FindForm();
                    tGridHint.tableIPCode = (((DevExpress.XtraGrid.Views.Grid.GridView)sender).GridControl).AccessibleName;
                    tGridHint.view = (((DevExpress.XtraGrid.Views.Grid.GridView)sender).GridControl).MainView;

                    if ((((DevExpress.XtraGrid.Views.Grid.GridView)sender).GridControl).MainView.IsEditing)
                    {
                        //(((DevExpress.XtraGrid.Views.Grid.GridView)sender).GridControl).MainView.PostEditor();
                        //(((DevExpress.XtraGrid.Views.Grid.GridView)sender).GridControl).MainView.ShowEditor();
                    }
                    if ((((DevExpress.XtraGrid.Views.Grid.GridView)sender).GridControl).AccessibleDescription != null)
                        tGridHint.gridPropNavigator = (((DevExpress.XtraGrid.Views.Grid.GridView)sender).GridControl).AccessibleDescription;

                    //object tDataTable = ((DevExpress.XtraGrid.Views.Grid.GridView)sender).GridControl.DataSource;
                    //if (tDataTable != null)
                    //    tGridHint.dataSet = ((DataTable)tDataTable).DataSet;

                    tGridHint.parentObject = senderType;
                    if (tGridHint.viewType == "")
                        tGridHint.viewType = "GridView";
                }
                else
                    MessageBox.Show("Dikkat : GridControl.GridView null geliyor : ");
                
                //return;
            }

            if (senderType == "DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView")
            {
                if ((((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender).GridControl) != null)
                {
                    tGridHint.tForm = (((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender).GridControl).FindForm();
                    tGridHint.tableIPCode = (((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender).GridControl).AccessibleName;
                    tGridHint.view = (((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender).GridControl).MainView;

                    if ((((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender).GridControl).MainView.IsEditing)
                    {
                        //(((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender).GridControl).MainView.PostEditor();
                        //(((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender).GridControl).MainView.ShowEditor();
                    }

                    if ((((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender).GridControl).AccessibleDescription != null)
                        tGridHint.gridPropNavigator = (((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender).GridControl).AccessibleDescription;

                    //object tDataTable = ((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender).GridControl.DataSource;
                    //if (tDataTable != null)
                    //    tGridHint.dataSet = ((DataTable)tDataTable).DataSet;

                    tGridHint.parentObject = senderType;
                    if (tGridHint.viewType == "")
                        tGridHint.viewType = "AdvBandedGridView";
                }
                else
                    MessageBox.Show("Dikkat : GridControl.AdvBandedGridView null geliyor : ");
                
                //return;
            }

            if (senderType == "DevExpress.XtraGrid.Views.Tile.TileView")
            {
                if ((((DevExpress.XtraGrid.Views.Tile.TileView)sender).GridControl) != null)
                {
                    tGridHint.tForm = (((DevExpress.XtraGrid.Views.Tile.TileView)sender).GridControl).FindForm();
                    tGridHint.tableIPCode = (((DevExpress.XtraGrid.Views.Tile.TileView)sender).GridControl).AccessibleName;
                    tGridHint.view = (((DevExpress.XtraGrid.Views.Tile.TileView)sender).GridControl).MainView;

                    if ((((DevExpress.XtraGrid.Views.Tile.TileView)sender).GridControl).AccessibleDescription != null)
                        tGridHint.gridPropNavigator = (((DevExpress.XtraGrid.Views.Tile.TileView)sender).GridControl).AccessibleDescription;

                    //object tDataTable = ((DevExpress.XtraGrid.Views.Tile.TileView)sender).GridControl.DataSource;
                    //if (tDataTable != null)
                    //    tGridHint.dataSet = ((DataTable)tDataTable).DataSet;

                    tGridHint.parentObject = senderType;
                    if (tGridHint.viewType == "")
                        tGridHint.viewType = "GridView";
                }
                else
                    MessageBox.Show("Dikkat : GridControl null geliyor : ");

                //return;
            }

            if (senderType == "DevExpress.XtraTreeList.TreeList")
            {
                
                if (((DevExpress.XtraTreeList.TreeList)sender) != null)
                {
                    tGridHint.tForm = ((DevExpress.XtraTreeList.TreeList)sender).FindForm();
                    tGridHint.tableIPCode = ((DevExpress.XtraTreeList.TreeList)sender).AccessibleName;
                    tGridHint.view = ((DevExpress.XtraTreeList.TreeList)sender);

                    if (((DevExpress.XtraTreeList.TreeList)sender).AccessibleDescription != null)
                        tGridHint.gridPropNavigator = ((DevExpress.XtraTreeList.TreeList)sender).AccessibleDescription;

                    //object tDataTable = ((DevExpress.XtraTreeList.TreeList)sender).DataSource;
                    //if (tDataTable != null)
                    //    tGridHint.dataSet = ((DataTable)tDataTable).DataSet;

                    tGridHint.parentObject = senderType;
                    if (tGridHint.viewType == "")
                        tGridHint.viewType = "TreeList";
                }
                else
                    MessageBox.Show("Dikkat : TreeList null geliyor : ");
                
                //return;
            }

            if (senderType == "DevExpress.XtraScheduler.SchedulerControl")
            {
                if (((DevExpress.XtraScheduler.SchedulerControl)sender) != null)
                {
                    tGridHint.tForm = ((DevExpress.XtraScheduler.SchedulerControl)sender).FindForm();
                        
                    tGridHint.tableIPCode = ((DevExpress.XtraScheduler.SchedulerControl)sender).AccessibleName;
                    tGridHint.view = ((DevExpress.XtraScheduler.SchedulerControl)sender).Views;

                    if (((DevExpress.XtraScheduler.SchedulerControl)sender).AccessibleDescription != null)
                        tGridHint.gridPropNavigator = ((DevExpress.XtraScheduler.SchedulerControl)sender).AccessibleDescription;

                    //object tDataTable = ((DevExpress.XtraGrid.Views.Grid.GridView)sender).GridControl.DataSource;
                    //if (tDataTable != null)
                    //    tGridHint.dataSet = ((DataTable)tDataTable).DataSet;

                    tGridHint.parentObject = senderType;
                    if (tGridHint.viewType == "")
                        tGridHint.viewType = "SchedulerControl";
                }
                else
                    MessageBox.Show("Dikkat : DevExpress.XtraScheduler.SchedulerControl null geliyor : ");
            }

            if (senderType == "DevExpress.XtraGrid.Views.Card.CardView")
            {
                if (((DevExpress.XtraGrid.Views.Card.CardView)sender) != null)
                {
                    tGridHint.tForm = ((DevExpress.XtraGrid.Views.Card.CardView)sender).GridControl.FindForm();

                    tGridHint.tableIPCode = ((DevExpress.XtraGrid.Views.Card.CardView)sender).GridControl.AccessibleName;
                    tGridHint.view = sender;
                    // ((DevExpress.XtraGrid.Views.Card.CardView)sender).GridControl.Views;

                    if (((DevExpress.XtraGrid.Views.Card.CardView)sender).GridControl.AccessibleDescription != null)
                        tGridHint.gridPropNavigator = ((DevExpress.XtraGrid.Views.Card.CardView)sender).GridControl.AccessibleDescription;

                    object tDataTable = ((DevExpress.XtraGrid.Views.Card.CardView)sender).GridControl.DataSource;
                    if (tDataTable != null)
                        tGridHint.dataSet = ((DataTable)tDataTable).DataSet;

                    tGridHint.parentObject = senderType;
                    if (tGridHint.viewType == "")
                        tGridHint.viewType = "CardView";
                }
                else
                    MessageBox.Show("Dikkat : DevExpress.XtraGrid.Views.Card.CardView null geliyor : ");
            }

            tGridHint.columnOldValue = oldValue.Trim();
            tGridHint.columnEditValue = editValue.Trim();

            t.Find_DataSet(tGridHint.tForm, ref dsData, ref dN, tGridHint.tableIPCode);
            tGridHint.dataSet = dsData;
            tGridHint.dataNavigator = dN;

            //if (kontrol == false)
            //{
            if (tGridHint.parentObject == "")
                undefinedObject("getGridFormAndTableIPCode", sender.GetType().ToString());
            //}
        }
                
        private bool findButton(Form tForm, string tableIPCode,
                     ref Control button, ref v.tButtonType buttonType, ref string propNavigator)
        {
            bool onay = false;
            string[] controls = new string[] { };

            ///nButton.buttonName = "simpleButton_sihirbaz_devam";
            ///nButton.buttonName = "simpleButton_listele";
            ///nButton.buttonName = "simpleButton_sec";
            ///nButton.buttonName = "simpleButton_listeye_ekle";
            ///nButton.buttonName = "simpleButton_liste_hazirla";
            ///nButton.buttonName = "simpleButton_goster";
            ///nButton.buttonName = "simpleButton_kart_ac";
            ///nButton.buttonName = "simpleButton_hesap_ac";
            ///nButton.buttonName = "simpleButton_belge_ac";

            
            button = t.Find_Control(tForm, "simpleButton_sihirbaz_devam", tableIPCode, controls);
            if (button != null)buttonType = v.tButtonType.btSihirbazDevam;

            if (button == null)
            {
                button = t.Find_Control(tForm, "simpleButton_sec", tableIPCode, controls);
                if (button != null) buttonType = v.tButtonType.btSecCik;
            }
                        
            if (button == null)
            {
                button = t.Find_Control(tForm, "simpleButton_goster", tableIPCode, controls);
                if (button != null) buttonType = v.tButtonType.btGoster;
            }
            if (button == null)
            {
                button = t.Find_Control(tForm, "simpleButton_kart_ac", tableIPCode, controls);
                if (button != null) buttonType = v.tButtonType.btKartAc;
            }
            if (button == null)
            {
                button = t.Find_Control(tForm, "simpleButton_hesap_ac", tableIPCode, controls);
                if (button != null) buttonType = v.tButtonType.btHesapAc;
            }
            if (button == null)
            {
                button = t.Find_Control(tForm, "simpleButton_belge_ac", tableIPCode, controls);
                if (button != null) buttonType = v.tButtonType.btBelgeAc;
            }
            /*
            if (button == null)
            {
                button = t.Find_Control(tForm, "simpleButton_listele", tableIPCode, controls);
                if (button != null) buttonType = v.tButtonType.btListele;
            }
            */
            if (button == null)
            {
                button = t.Find_Control(tForm, "simpleButton_listeye_ekle", tableIPCode, controls);
                if (button != null) buttonType = v.tButtonType.btListeyeEkle;
            }
            if (button == null)
            {
                button = t.Find_Control(tForm, "simpleButton_liste_hazirla", tableIPCode, controls);
                if (button != null) buttonType = v.tButtonType.btListeHazirla;
            }

            if (button != null)
            {
                if (((DevExpress.XtraEditors.SimpleButton)button).AccessibleDescription != null)
                    propNavigator = ((DevExpress.XtraEditors.SimpleButton)button).AccessibleDescription.ToString();
            }
            
            if (button != null) onay = true;

            return onay;
        }

        private bool gridViewKeySearch(object sender, vGridHint tGridHint)
        {
            bool onay = false;
                                    
            string myProp = "";

            if (tGridHint.columnPropNavigator != null)
                myProp = tGridHint.columnPropNavigator.Replace((char)34, (char)39);

            if (myProp == "")
            {
                // Search özelliği olmayan kolonda üzerindeyken; 
                // search özelliği olan kolonu bul,
                // o kolondan gerekli veriler al ve çalıştır
                findColumn(sender, ref tGridHint, v.tFindColumnType.searchColumn);
            }
            else
                tGridHint.buttonType = v.tButtonType.btArama;
                        
            return onay;
        }

        private void keyEscape(object sender, vGridHint tGridHint)
        {

            //(tGridHint.parentObject.IndexOf("DevExpress.XtraEditors") > -1)
            
            if (tGridHint.parentObject.IndexOf("DevExpress.XtraGrid") > -1)
            {
                gridViewKeyEscape(sender, tGridHint);
            }

            if (tGridHint.parentObject.IndexOf("DevExpress.XtraDataLayout.DataLayoutControl") > -1)
            {
                //MessageBox.Show("escape yazılacak");
                ev.tCancelData(tGridHint.tForm, sender, tGridHint.tableIPCode);
            }
        }

        private void gridViewKeyEscape(object sender, vGridHint tGridHint)
        {
            object parentObj = null;

            if (sender.GetType().ToString() == "DevExpress.XtraEditors.ButtonEdit")
            {
                parentObj = ((DevExpress.XtraEditors.ButtonEdit)sender).Parent;
                
            }

            if (sender.GetType().ToString() == "DevExpress.XtraEditors.TextEdit")
            {
                parentObj = ((DevExpress.XtraEditors.TextEdit)sender).Parent;
            }

            GridView view = null;

            if (parentObj != null)
                view = ((DevExpress.XtraGrid.GridControl)parentObj).MainView as GridView;
            else
                view = sender as GridView;

            
            // edit mdounda değilse
            if (view?.IsEditing == false)
            {
                tGridHint.tForm.Close();
                return;
            }

            //if (view.IsNewItemRow(view.FocusedRowHandle))
            //    view.DeleteRow(view.FocusedRowHandle);

            // tablonun key fieldinden value getiriyor
            //
            string keyValue = t.TableKeyFieldValue(tGridHint.tForm, tGridHint.tableIPCode, "").ToString();

            //object ob = null;
            //DevExpress.XtraDataLayout.DataLayoutControl
            //((DevExpress.XtraEditors.DataNavigator)ob).is
            
            // satırda record/kayıt var ise
            //
            if (keyValue != "")
            {
                // edit modunda ise
                if (view?.IsEditing == true)
                {
                    view.HideEditor();

                    view.CancelUpdateCurrentRow();
                }
            }
            else
            {
                // insert satırı ise
                //

                // yeni açılmış satırı sil
                //
                view?.DeleteRow(view.FocusedRowHandle);

                // hiç satır kalmadıysa
                //
                if (view?.RowCount == 0)
                {
                    v.con_PositionChange = true;
                    evb.newData(tGridHint.tForm, tGridHint.tableIPCode);

                    //gridViewSetFirstColumn(sender, tGridHint);
                }
            }
        }
        
        public void gridViewSetFirstColumn(object sender, vGridHint tGridHint)
        {
            object parentObj = null;

            if (sender.GetType().ToString() == "DevExpress.XtraEditors.ButtonEdit")
            {
                parentObj = ((DevExpress.XtraEditors.ButtonEdit)sender).Parent;
            }

            if (sender.GetType().ToString() == "DevExpress.XtraEditors.TextEdit")
            {
                parentObj = ((DevExpress.XtraEditors.TextEdit)sender).Parent;
            }

            GridView view = null;
            CardView cardView = null;

            if (parentObj != null)
            {
                if (parentObj.GetType().ToString() != "DevExpress.XtraGrid.Views.Card.CardView")
                    view = ((DevExpress.XtraGrid.GridControl)parentObj).MainView as GridView;
                else cardView = ((DevExpress.XtraGrid.GridControl)parentObj).MainView as CardView;
            }
            else
            {
                if (sender.GetType().ToString() != "DevExpress.XtraGrid.Views.Card.CardView")
                    view = sender as GridView;
                else cardView = sender as CardView;
            }

            /*
            view.Focus();
            view.FocusedColumn = view.VisibleColumns[0];

            tGridHint.currentColumn = view.FocusedColumn;

            if (view.FocusedColumn.ReadOnly)
                view.FocusedColumn = GetNextFocusableColumn(tGridHint);
            */

            if (sender.GetType().ToString() == "DevExpress.XtraGrid.Views.Grid.GridView")
            {
                DevExpress.XtraGrid.Columns.GridColumn res = null;
                int count = ((DevExpress.XtraGrid.Views.Grid.GridView)view).VisibleColumns.Count;
                for (int i = 0; i < count - 1; i++)
                {
                    res = (DevExpress.XtraGrid.Columns.GridColumn)view.VisibleColumns[i];

                    if (res.ReadOnly == false)
                    {
                        tGridHint.currentColumn = res;
                        view.FocusedColumn = res;
                        gridShowEditor(tGridHint);
                        return; // res;
                    }
                }
            }
            if (sender.GetType().ToString() == "DevExpress.XtraGrid.Views.Card.CardView")
            {
                DevExpress.XtraGrid.Columns.GridColumn res = null;
                int count = ((DevExpress.XtraGrid.Views.Card.CardView)cardView).VisibleColumns.Count;
                for (int i = 0; i < count - 1; i++)
                {
                    res = (DevExpress.XtraGrid.Columns.GridColumn)cardView.VisibleColumns[i];

                    if (res.ReadOnly == false)
                    {
                        tGridHint.currentColumn = res;
                        cardView.FocusedColumn = res;
                        gridShowEditor(tGridHint);
                        return; // res;
                    }
                }
            }

            return;
        }

        public void undefinedObject(string functionName, string controlName)
        {
            MessageBox.Show("DİKKAT : Tanımlı olmayan nesne ..." + v.ENTER2 +
                    "Control  Name : " + controlName + v.ENTER +
                    "Function Name : " + functionName);
        }

        public void gridShowEditor(vGridHint tGridHint)
        {
            //if (view.ValidateEditor() == false)
            //    System.Windows.Forms.SendKeys.Send("{ENTER}");
            //
            //view.ShowEditor();
            //
            GridView view = getGridView(tGridHint);

            if (view == null) return;

            try
            {
                if (tGridHint.dataSet != null)
                {
                    if (tGridHint.dataSet.Tables[0].Rows.Count > 0)
                    {
                        if (view.GetType().ToString() == "DevExpress.XtraGrid.Views.Grid.GridView")
                            ((GridView)view).GridControl.BeginInvoke(new Action(delegate { ((GridView)view).ShowEditor(); }));
                        if (view.GetType().ToString() == "DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView")
                            ((AdvBandedGridView)view).GridControl.BeginInvoke(new Action(delegate { ((GridView)view).ShowEditor(); }));
                    }
                }
            }
            catch (Exception)
            {
                //
                //throw;
            }
        }
        
        private bool checkEnabledColumns(GridView TableView)
        {
            // herhangi bir column readOnly == false olma durumunu kontrol ediyor

            bool onay = false;

            foreach (DevExpress.XtraGrid.Columns.GridColumn item in TableView.VisibleColumns)
            {
                if (item.ReadOnly == false)
                {
                    onay = true;
                    break;
                }
            }
            return onay;
        }
        
        private bool getIsLastRow(vGridHint tGridHint)
        {
            bool onay = false;
            GridView view = getGridView(tGridHint);
            onay = view.IsLastRow;
            return onay;
        }
        private bool getIsLastColumn(vGridHint tGridHint)
        {
            bool onay = false;
            GridView view = getGridView(tGridHint);

            if (view != null)
            {
                tGridHint.currentColumn = view.FocusedColumn;

                int ind = 0;

                if (tGridHint.currentColumn == null)
                    ind = -1;
                else
                    ind = view.VisibleColumns.IndexOf(tGridHint.currentColumn);

                /// VisibleColumns field listesi 0 dan başlıyor 
                if ((ind + 1) == view.VisibleColumns.Count)
                {
                    onay = true;
                }
            }

            if (view == null)
            {
                // CardView için hazırla
            }
            return onay;
        }

        public DevExpress.XtraGrid.Columns.GridColumn GetNextFocusableColumn(vGridHint tGridHint)
        {
            GridView view = getGridView(tGridHint);

            int ind = 0;

            if (tGridHint.currentColumn == null)
                ind = -1;
            else
                ind = view.VisibleColumns.IndexOf(tGridHint.currentColumn);
            
            ind++;
            // column sonuna gelindiyse
            if (ind >= view.VisibleColumns.Count)
            {
                // son satır/row ise
                if (view.IsLastRow)
                {
                    //v.formLastActiveControl = tForm.ActiveControl;
                    //
                    //ev.AutoNew(tForm);
                    //
                    //v.formLastActiveControl = null;

                    //MessageBox.Show("last");
                    tGridHint.isLastRow = true;
                }
                else
                {
                    // sonraki satır/row a geç
                    //
                    //MessageBox.Show("move");
                    tGridHint.isLastRow = false;
                    view.MoveNext();
                }
                ind = 0;
            }

            DevExpress.XtraGrid.Columns.GridColumn res = 
                (DevExpress.XtraGrid.Columns.GridColumn)view.VisibleColumns[ind];

            //(DevExpress.XtraGrid.Columns.GridColumn)view.

            if (res == null)
                return tGridHint.currentColumn;

            // inputBox ile veri girişi yaptırılıan column ise sonrakine column a atla
            if (res.FieldName == v.con_InputBox_FieldName)
            {
                v.con_InputBox_FieldName = "";
                tGridHint.currentColumn = res;
                res = GetNextFocusableColumn(tGridHint);
            }

            if (res.ReadOnly)
            {
                if (res.VisibleIndex + 1 == view.VisibleColumns.Count)
                {

                    /* bura düzenlenecek
                          tSave sv = new tSave();
                          bool onay = sv.tDataSave(tForm, tableIPCode);
                          if (onay)
                          {
                              tEventsButton evb = new tEventsButton();
                              evb.newData(tForm, tableIPCode);
                          }
                       */
                    
                    //MessageBox.Show("last 2");

                    
                    if (view.IsLastVisibleRow)
                        myGridRowAdd(tGridHint);
                    else view.MoveNext();

                    if (v.con_ColumnValueChanging)
                    {
                        myGridRowSave(tGridHint);
                    }


                    ///FocusInvalidRow();

                    /// column bitince burada sıfırıncı kolona gidiyor
                    ///
                    res = (DevExpress.XtraGrid.Columns.GridColumn)view.VisibleColumns[0];

                    if (res.ReadOnly)
                    {
                        // enbaled column var mı
                        bool getFocusableColumn = checkEnabledColumns(view);

                        if (getFocusableColumn)
                        {
                            tGridHint.currentColumn = res;
                            res = GetNextFocusableColumn(tGridHint);
                            return res;
                        }
                        else
                            return res;
                    }
                    else
                    {
                        return res;
                    }
                    //return res;
                }
                
                tGridHint.currentColumn = res;
                res = GetNextFocusableColumn(tGridHint);
                
                return res;
            }

            return res;
        }

        private DevExpress.XtraGrid.Columns.GridColumn GetPrevFocusableColumn(vGridHint tGridHint)
        {
            GridView view = getGridView(tGridHint);

            int ind = view.VisibleColumns.IndexOf(tGridHint.currentColumn);
            ind--;
            if (ind < 0)
            {
                ind = view.VisibleColumns.Count - 1;
            }

            DevExpress.XtraGrid.Columns.GridColumn res = (DevExpress.XtraGrid.Columns.GridColumn)view.VisibleColumns[ind];

            if (res == null)
                return tGridHint.currentColumn;

            if (res.ReadOnly)
            {
                //tGridHint.currentColumn = res;
                //res = GetPrevFocusableColumn(tGridHint);
                //return res;

                // enbaled column var mı
                bool getFocusableColumn = checkEnabledColumns(view);

                if (getFocusableColumn)
                {
                    tGridHint.currentColumn = res;
                    res = GetPrevFocusableColumn(tGridHint);
                    return res;
                }
                else
                    return res;
            }
            return res;
        }

        private void findColumn(object sender, ref vGridHint tGridHint, v.tFindColumnType findColumnType)
        {
            if (sender.GetType().ToString().IndexOf("DevExpress.XtraEditors") > -1) //.TextEdit
            {
                object control = null;
                
                control = ((System.Windows.Forms.Control)sender).Parent;

                if (control.GetType().ToString() == "DevExpress.XtraGrid.GridControl")
                {
                    object view = ((DevExpress.XtraGrid.GridControl)control).MainView;

                    if (findColumnType == v.tFindColumnType.searchColumn)
                        getSearchColumn_(view, ref tGridHint);

                    //if (findColumnType == v.tFindColumnType.firstActiveColumn)
                    //    getFirstActiveColumn_(view, ref tGridHint);

                    return;
                }
            }

            if ((sender.GetType().ToString() == "DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView") |
                (sender.GetType().ToString() == "DevExpress.XtraGrid.Views.Grid.GridView"))
            {
                if (findColumnType == v.tFindColumnType.searchColumn)
                    getSearchColumn_(sender, ref tGridHint);

                //if (findColumnType == v.tFindColumnType.firstActiveColumn)
                //    getFirstActiveColumn_(sender, ref tGridHint);

                return;
            }
            return;
        }
        
        private void getSearchColumn_(object view, ref vGridHint tGridHint)
        {
            int count = 0;
            string value = "";

            if (view.GetType().ToString() == "DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView")
            {
                count = ((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)view).VisibleColumns.Count;
                for (int i = 0; i < count - 1; i++)
                {
                    if (((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)view).VisibleColumns[i].ColumnEdit.AccessibleDescription != null)
                    {
                        value = ((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)view).VisibleColumns[i].ColumnEdit.AccessibleDescription;
                        if (value.IndexOf("SearchEngine") > 0)
                        {
                            tGridHint.columnPropNavigator = value;
                            return;
                        }
                    }
                }
            }

            if (view.GetType().ToString() == "DevExpress.XtraGrid.Views.Grid.GridView")
            {
                count = ((DevExpress.XtraGrid.Views.Grid.GridView)view).VisibleColumns.Count;
                for (int i = 0; i < count - 1; i++)
                {
                    if (((DevExpress.XtraGrid.Views.Grid.GridView)view).VisibleColumns[i].ColumnEdit.AccessibleDescription != null)
                    {
                        value = ((DevExpress.XtraGrid.Views.Grid.GridView)view).VisibleColumns[i].ColumnEdit.AccessibleDescription;
                        if (value.IndexOf("SearchEngine") > 0)
                        {
                            tGridHint.columnPropNavigator = value;
                            return;
                        }
                    }
                }
            }
        }
        
        public void gridSpeedKeys(vGridHint tGridHint, KeyEventArgs e)
        {
            #region
            GridView view = getGridView(tGridHint);

            if (e.KeyCode == Keys.Escape)
            {
                gridViewKeyEscape(view, tGridHint);
                e.Handled = true;
            }
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
            #endregion
        }
        
        #endregion

        #region end func // işi bitenler

        //newGridKeys - end
        private void myGridView_Work_PropNavigator_(object sender, string eventType)
        {
            //string Prop_Navigator = "";
            //string TableIPCode = string.Empty;
            //Form tForm = myGridFormAndTableIPCode_Get(sender, ref TableIPCode);
            vGridHint tGridHint = new vGridHint();
            getGridFormAndTableIPCode(sender, ref tGridHint);
            /*
            if (eventType == "DoubleClick")
            {
                
                if (sender.GetType().ToString() == "DevExpress.XtraGrid.Views.Grid.GridView")
                {
                    Prop_Navigator = (((DevExpress.XtraGrid.Views.Grid.GridView)sender).GridControl).AccessibleDescription;
                }
                if (sender.GetType().ToString() == "DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView")
                {
                    Prop_Navigator = (((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender).GridControl).AccessibleDescription;
                }
                
            }
            */
            /*
            if (eventType == "FocusedColumn")
            {

            }

            if ((tGridHint.gridPropNavigator == "") ||
                (tGridHint.gridPropNavigator == "null") ||
                (tGridHint.gridPropNavigator == null))
            {
                MessageBox.Show("yeni btnClick işi var.. ");
                //ev.navigatorButtonExec_Keys(tGridHint.tForm, Keys.Return, tGridHint.tableIPCode, "");
                return;
            }
            else
            {
                MessageBox.Show("yeni btnClick işi var.. ");
                //ev.navigatorButtonExec_Keys(tGridHint.tForm, Keys.Return, tGridHint.tableIPCode, tGridHint.gridPropNavigator);
            }
            */
        }
        //newGridKeys - end
        private bool myWork_Keys_(object sender, KeyEventArgs e, vGridHint tGridHint)
        {
            bool onay = false;

            // işlem yapmadan dön
            if (e.Handled)
                return onay;
                        
            // yerini değiştirme

            string myProp = "";

            if (tGridHint.columnPropNavigator != null)
                myProp = tGridHint.columnPropNavigator.Replace((char)34, (char)39);

            if (e.KeyCode == Keys.Enter)
            {
                v.con_Value_Old = tGridHint.columnOldValue;

                if (//(sender.GetType().ToString() == "DevExpress.XtraEditors.ButtonEdit") &&
                    (myProp != "") &&
                    (v.con_Value_Old != ""))
                {
                    if (myProp.IndexOf("SearchEngine") > -1)
                    {

                        // searchformunu  açmadan önce value yi kontrol et 
                        // kontrol sonucu olumsuz ise formu aç
                        /*
                        if (se.directSearch(tGridHint.tForm, tGridHint.tableIPCode, myProp, v.con_Value_Old) == false)
                        {
                            e.Handled = false;

                            v.searchEnter = false; // neden bu özllik var idi? 
                            if (tGridHint.viewType != "GridView")
                                v.searchEnter = true;

                            //onay = ev.openSearchForm(tGridHint.tForm, tGridHint.tableIPCode, myProp);
                            return onay;
                        }
                        */
                    }
                }
            }

            if (e.KeyCode == v.Key_SearchEngine)
            {
                /*
                v.con_Value_Old = tGridHint.columnValue;

                if (v.con_Value_New != "-1")
                {
                    if (myProp == "")
                    {
                        // Search özelliği olmayan kolonda üzerindeyken; 
                        // search özelliği olan kolonu bul,
                        // o kolondan gerekli veriler al ve çalıştır
                        getSearchColumn(sender, ref tGridHint);

                        if (tGridHint.columnPropNavigator != null)
                            myProp = tGridHint.columnPropNavigator.Replace((char)34, (char)39);
                    }

                    if (myProp != "")
                    {
                        v.searchEnter = false; // neden bu özllik var idi? 
                        if (tGridHint.viewType != "GridView")
                            v.searchEnter = true;

                        //onay = ev.openSearchForm(tGridHint.tForm, tGridHint.tableIPCode, myProp);
                        e.Handled = true;
                        return onay;
                    }
                }
                */
            }

            /*
            if (e.KeyCode == v.Key_YeniSatir)
            {
                // grid için yeni satır oluştur
                tSave sv = new tSave();
                if (sv.tDataSave(tGridHint.tForm, tGridHint.tableIPCode))
                {
                    t.ButtonEnabledAll(tGridHint.tForm, tGridHint.tableIPCode, true);
                    evb.newData(tGridHint.tForm, tGridHint.tableIPCode);
                    t.tFormActiveView(tGridHint.tForm, tGridHint.tableIPCode);
                    return true;
                }
            }
            */
            /*
            // esc tuşu basıldı ve (rowCount == 0) ise
            if (e.KeyCode == Keys.Escape)
            {
                v.con_PositionChange = true;
                evb.newData(tGridHint.tForm, tGridHint.tableIPCode);
                t.tFormActiveView(tGridHint.tForm, tGridHint.tableIPCode);
                return true;
            }
            */

            // yukarıda herhangi bir keyse takılmadıysa buraya giderek orada keyse ait işlem varsa o çalışır
            MessageBox.Show("yeni btnClick işi var.. myWork_Keys_");
            //onay = ev.navigatorButtonExec_Keys(tGridHint.tForm, e.KeyCode, tGridHint.tableIPCode, tGridHint.gridPropNavigator);

            return onay;
        }
        //newGridKeys - end
        public void myGridView_KeyDown_OLD(object sender, KeyEventArgs e)
        {

            #region KeyCode

            //GridView view = sender as AdvBandedGridView;
            //DevExpress.XtraGrid.Columns.GridColumn focusedColumn = view.FocusedColumn;
            //GridView view = sender as GridView;
            //DevExpress.XtraGrid.Columns.GridColumn focusedColumn = view.FocusedColumn;
            //object view = null;

            if (
                ((e.KeyCode >= Keys.F1) &&
                 (e.KeyCode <= Keys.F24)) |
                 (e.KeyCode == Keys.Enter) |
                 (e.KeyCode == Keys.Return) |
                 (e.KeyCode == Keys.Escape) |
                 (e.KeyCode == Keys.Tab) |
                 (e.KeyCode == Keys.Left) |
                 (e.KeyCode == Keys.Right) |
                 ((e.KeyCode == Keys.Delete) && (e.Modifiers == Keys.Control))
               )
            {
                GridView view = sender as GridView;
                vGridHint tGridHint = new vGridHint();
                //getGridHint_(sender, ref tGridHint);
                bool onay = false;

                if (e.KeyCode == Keys.Escape)
                {
                    //gridViewKeyEscape(view, tGridHint);
                    //e.Handled = true;

                    /*
                    //if (view.IsNewItemRow(view.FocusedRowHandle))
                    //    view.DeleteRow(view.FocusedRowHandle);

                    // tablonun key fieldinden value getiriyor
                    //
                    string value = t.TableKeyFieldValue(tGridHint.tForm, tGridHint.tableIPCode);

                    // satırda record/kayıt var ise
                    //
                    if (value != "")
                    {
                        // edit mdounda değilse
                        if (view.IsEditing == false)
                        {
                            tGridHint.tForm.Close();
                        }

                        // edit modunda ise
                        if (view.IsEditing)
                        {
                            view.HideEditor();

                            view.CancelUpdateCurrentRow();
                        }
                    }
                    else
                    {
                        // insert satırı ise
                        //

                        // yeni açılmış satırı sil
                        //
                        view.DeleteRow(view.FocusedRowHandle);

                        // hiç satır kalmadıysa
                        //
                        if (view.RowCount == 0)
                        {
                            myWork_Keys_(sender, e, tGridHint);

                            view.FocusedColumn = view.VisibleColumns[0];

                            if (view.FocusedColumn.ReadOnly)
                                view.FocusedColumn = GetNextFocusableColumn(tGridHint.tForm, view, view.FocusedColumn);
                        }

                        gridShowEditor(view, tGridHint);

                        v.formLastActiveControl = view.GridControl;
                    }
                    
                    e.Handled = true;
                    */
                }

                if (((e.KeyCode >= Keys.F1) &&
                     (e.KeyCode <= Keys.F24)) |
                     (e.KeyCode == Keys.Enter) |
                     (e.KeyCode == Keys.Return) |
                     (e.KeyCode == Keys.Delete)
                    )
                {
                    //onay = myWork_Keys_(sender, e, tGridHint);

                    if (onay == false)
                    {
                        if (view.IsEditorFocused == false)
                        {
                            if (e.KeyCode == Keys.F2)
                                e.Handled = true;
                        }
                        /*
                        if ((e.KeyCode == Keys.Return) ||
                            (e.KeyCode == Keys.Enter) ||
                            (e.KeyCode == v.Key_SearchEngine))
                            e.Handled = true;*/
                    }
                }

                // enabled = false olan kolonları atlasın en yakın editable kolona gitsin
                //
                if (((e.KeyCode == Keys.Tab) |
                     //(e.KeyCode == Keys.Right) |  bunun sayesinde gridin tüm kolonlarını gezebilsin açma
                     (e.KeyCode == Keys.Enter)) &&
                     (e.Handled == false) &&
                     (onay == false) &&
                     (view != null))
                {
                    GridControl grid = view.GridControl as GridControl;
                    Form tForm = grid.FindForm();

                    /*if (view.IsEditing == false)
                    {
                        DevExpress.XtraGrid.Columns.GridColumn focusedColumn = view.VisibleColumns[0];

                        //view.FocusedColumn = GetNextFocusableColumn(tForm, tGridHint.tableIPCode, view, view.FocusedColumn);

                        gridShowEditor(view, tGridHint);
                    }
                    else
                    {*/
                    
                        //tGridHint.currentColumn = view.FocusedColumn;

                        //view.FocusedColumn = GetNextFocusableColumn(tGridHint);

                        //gridShowEditor(view, tGridHint);
                    //}


                    //e.Handled = true;
                }

                if ((e.KeyCode == Keys.Left) &&
                    (e.Handled == false))
                {
                    //tGridHint.currentColumn = view.FocusedColumn;
                    //view.FocusedColumn = GetPrevFocusableColumn(tGridHint);

                    //gridShowEditor(tGridHint);

                    //e.Handled = true;
                }
                //--- enabled = false ...

                if (tGridHint.tForm != null)
                {
                    //if (tGridHint.tForm.Name.ToString() == "tSearchForm")
                    //    e.Handled = true;
                }
            }
            #endregion KeyCode

        }

        #endregion newGridKeys - end


        public void myTreeList_KeyDown(object sender, KeyEventArgs e) //*** New Ok
        {
            if (e.Handled) return;
            
            if (t.findAttendantKey(e) ||
                t.findReturnKey(e) ||
                t.findDirectionKey(e))
            {
                DevExpress.XtraTreeList.TreeList view = sender as DevExpress.XtraTreeList.TreeList;

            }
        }

            //newGridKeys - ok 
        public void myGridView_KeyDown(object sender, KeyEventArgs e) //*** New Ok
        {
            if (e.Handled) return;

            v.con_Expression = true;

            /*if (
                ((e.KeyCode >= Keys.F1) &&
                 (e.KeyCode <= Keys.F24)) |
                 (e.KeyCode == Keys.Enter) |
                 (e.KeyCode == Keys.Return) |
                 (e.KeyCode == Keys.Escape) |
                 //(e.KeyCode == Keys.Tab) |
                 (e.KeyCode == Keys.Left) |
                 (e.KeyCode == Keys.Right) |
                 (e.KeyCode == Keys.Delete) |
                 ((e.KeyCode == Keys.Delete) && (e.Modifiers == Keys.Control))
               )*/
            if (t.findAttendantKey(e) || 
                t.findReturnKey(e) || 
                t.findDirectionKey(e))
            {
                GridView view = sender as GridView;
                vGridHint tGridHint = new vGridHint();
                getGridHint_(sender, ref tGridHint);
                bool onay = false;

                if ((e.KeyCode != Keys.F2) &
                    (e.KeyCode != Keys.Enter))
                    onay = commonGridClick(sender, e, tGridHint);


                if (onay == false && view != null)
                {
                    if (view.IsEditorFocused == false)
                    {
                        if ((e.KeyCode == Keys.F2) |
                            (e.KeyCode == Keys.Enter))
                        {
                            gridShowEditor(tGridHint);
                            e.Handled = true;
                        }
                    }

                    /// enabled = false olan kolonları atlasın en yakın editable kolona gitsin
                    ///
                    ///(e.KeyCode == Keys.Right) |  bu tuşu ekleme, bu sayede gridin tüm kolonlarını gezebilsin, açma
                    ///
                    #region Editable column focusble
                    if (//((e.KeyCode == Keys.Tab) |
                         (e.KeyCode == Keys.Enter) &&
                         //(e.Handled == false) &&
                         (onay == false) &&
                         (view != null))
                    {
                        tGridHint.currentColumn = view.FocusedColumn;
                        var res = GetNextFocusableColumn(tGridHint);
                        view.FocusedColumn = res;
                        gridShowEditor(tGridHint);
                        e.Handled = true;
                    }

                    if ((e.KeyCode == Keys.Left) &&
                        (e.Handled == false))
                    {
                        tGridHint.currentColumn = view.FocusedColumn;
                        view.FocusedColumn = GetPrevFocusableColumn(tGridHint);
                        gridShowEditor(tGridHint);
                        e.Handled = true;
                    }
                    #endregion
                }

                if (tGridHint.tForm != null)
                {
                    if (tGridHint.tForm.Name.ToString() == "tSearchForm")
                        e.Handled = true;
                }
            }

            return;
        }

        //newGridKeys - ok
        public void myGridView_FocusedColumnChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedColumnChangedEventArgs e)
        {
            vGridHint tGridHint = new vGridHint();
            getGridHint_(sender, ref tGridHint);

            if (string.IsNullOrEmpty(tGridHint.columnPropNavigator) == false)
            {
                tGridHint.columnPropNavigator = tGridHint.columnPropNavigator.Replace((char)34, (char)39);
                // InputBox
                if (tGridHint.columnPropNavigator.IndexOf("'BUTTONTYPE': '57'") > -1)
                {
                    v.tButtonHint.Clear();
                    v.tButtonHint.tForm = tGridHint.tForm;
                    v.tButtonHint.tableIPCode = tGridHint.tableIPCode;
                    v.tButtonHint.propNavigator = tGridHint.columnPropNavigator;
                    v.tButtonHint.buttonType = v.tButtonType.btInputBox;
                    tEventsButton evb = new tEventsButton();
                    evb.btnClick(v.tButtonHint);

                    v.con_Expression = true;
                    v.con_Expression_FieldName = e.FocusedColumn.FieldName;

                    //ev.navigatorButtonExec_(tGridHint.tForm, tGridHint.tableIPCode, tGridHint.columnPropNavigator, v.tNavigatorButton.nv_57_Input_Box);
                }
            }
        }
                
        //newGridKeys - ok
        public void myGridView_DoubleClick(object sender, EventArgs e)
        {
            
            GridView view = sender as GridView;
            vGridHint tGridHint = new vGridHint();
            getGridHint_(sender, ref tGridHint);
            // doubleClick i Enter tuşu etkisine çevir
            System.Windows.Forms.KeyEventArgs key = new KeyEventArgs(Keys.Enter);

            if (sender.GetType().ToString() == "DevExpress.XtraScheduler.SchedulerControl")
                mySchedulerControl(sender, tGridHint);

            commonGridClick(sender, key, tGridHint);
        }

        public void mySchedulerControl(object sender, vGridHint tGridHint)
        {
            var cntrl = ((DevExpress.XtraScheduler.SchedulerControl)sender);

            for (int i = 0; i < cntrl.SelectedAppointments.Count; i++)
            {
                Appointment apt = cntrl.SelectedAppointments[i];

                var selectId = apt.Id;
                
                DataSet ds = tGridHint.dataSet;
                DataNavigator dN = tGridHint.dataNavigator;
                string myProp = ds.Namespace;
                string KeyFName = t.MyProperties_Get(myProp, "KeyFName:");

                for (int i2 = 0; i2 < ds.Tables[0].Rows.Count; i2++)
                {
                    if (ds.Tables[0].Rows[i2][KeyFName].ToString() == selectId.ToString())
                    {
                        dN.Position = i2;
                        break;
                    }
                }
                // Create new appointment using copy operation.
                //Appointment newApt = apt.Copy();
                // Add one month to the new appointment's start time.
                //newApt.Start = apt.Start.AddMonths(1);
                // Add new appointment to the appointment collection.
                //cntrl.Storage.Appointments.Add(newApt);
            }
        }


        public void myGridView_ColumnFilterChanged(object sender, EventArgs e)
        {
            //
            //e.ToString();

            //Form f = ((DevExpress.XtraGrid.Views.Grid.GridView)sender).GridControl.FindForm();
            //f.Text =  ((DevExpress.XtraGrid.Views.Grid.GridView)sender).RowCount.ToString();
        }

        public void myGridView_CellValueChanging(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == "LKP_ONAY")
            {
                //v.con_LkpOnayChange = true;

                vGridHint tGridHint = new vGridHint();
                getGridFormAndTableIPCode(sender, ref tGridHint);

                DataSet dsData = null;
                DataNavigator tDataNavigator = null;
                t.Find_DataSet(tGridHint.tForm, ref dsData, ref tDataNavigator, tGridHint.tableIPCode);

                #region Detail-SubDetail Table
                // Kendisine bağlı subdetail var ise
                if (tDataNavigator.IsAccessible == true)
                {
                    if (tDataNavigator.Position > -1)
                    {
                        string value = dsData.Tables[0].Rows[tDataNavigator.Position]["LKP_ONAY"].ToString();
                        if (value == "1")
                        {
                            dsData.Tables[0].Rows[tDataNavigator.Position]["LKP_ONAY"] = 0;
                        }
                        else
                        {
                            dsData.Tables[0].Rows[tDataNavigator.Position]["LKP_ONAY"] = 1;
                        }
                        ev.Data_Refresh(tGridHint.tForm, dsData, tDataNavigator);
                    }
                }
                #endregion Detail-SubDetail Table

            }
            //e.Column.UnboundExpression
        }

        public void myGridView_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            //Application.OpenForms[0].Text = "myGridView_CellValueChanged : " + sender.ToString() + "; " + e.Column.FieldName.ToString();

            v.con_ColumnValueChanging = true;
            
            #region 
            if (e.Column.Tag != null)
            {
                if (e.Column.Tag.ToString() == "EXPRESSION")
                {
                    //string TableIPCode = string.Empty;
                    //Form tForm = myGridFormAndTableIPCode_Get(sender, ref TableIPCode);
                    vGridHint tGridHint = new vGridHint();
                    getGridFormAndTableIPCode(sender, ref tGridHint);

                    t.work_EXPRESSION(tGridHint.tForm, tGridHint.tableIPCode, e.Column.FieldName, e.Value.ToString());
                    
                    // Eğer return olmaz ise alttaki fonksiyonlar çalışıyor
                    // bu nedenle return kaldırma

                    //((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender).Invalidate();

                    return;
                }

            }
            #endregion

        }

        public void myGridView_ColumnChanged(object sender, EventArgs e)
        {
            //Application.OpenForms[0].Text = "ColumnChanged : " + sender.ToString() + "; " + e.ToString();

        }

        public void myGridView_GotFocus(object sender, EventArgs e)
        {
            //MessageBox.Show("got");
            //v.SQL = "got" + v.ENTER + v.SQL;
        }

        public void myGridView_LostFocus(object sender, EventArgs e)
        {
            //MessageBox.Show("lost");
            /// DİKKAT : BURAYI AÇINDA change ler ÇALIŞMAMAYA BAŞLIYOR
            /// 
            //v.SQL = "lost" + v.ENTER + v.SQL;
        }
        
        public void myGridView_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            //MessageBox.Show("myGridView_FocusedRowChanged");
            //if (!gridView1.IsMultiSelect)
            //    Test();

            //MessageBox.Show("save ye uygun row change");
            //(tGridHint.dataSet.HasChanges())


            /// Bu işleme gerek yok çünkü değişikliği 
            /// dataNavigator_PositionChanged  kaydediyor
            /// 

            
            if (v.con_ColumnValueChanging)
            {
                vGridHint tGridHint = new vGridHint();
                getGridHint_(sender, ref tGridHint);
                myGridRowSave(tGridHint);
            }
            
        }

        private void myGridRowSave(vGridHint tGridHint)
        {
            ((GridView)tGridHint.view).PostEditor(); // gridView1.PostEditor();
            System.Windows.Forms.KeyEventArgs key = new KeyEventArgs(v.Key_ExtraIslem);
            tGridHint.buttonType = v.tButtonType.btExtraIslem;
            commonGridClick(tGridHint.view, key, tGridHint);

            v.con_ColumnValueChanging = false;
        }

        private void myGridRowAdd(vGridHint tGridHint)
        {
            System.Windows.Forms.KeyEventArgs key = new KeyEventArgs(v.Key_KaydetYeni);
            tGridHint.buttonType = v.tButtonType.btKaydetYeni;
            commonGridClick(tGridHint.view, key, tGridHint);
        }

        public void myGridView_KeyPress(object sender, KeyPressEventArgs e)
        {
            v.con_Expression = true;

            //Application.OpenForms[0].Text = Application.OpenForms[0].Text + ":" + e.KeyChar.ToString();

        }

        public void myGridView_KeyUp(object sender, KeyEventArgs e)
        {
            //...
            v.con_Expression = true;

            //Application.OpenForms[0].Text = Application.OpenForms[0].Text + ";" + e.KeyCode.ToString();

        }

        public void myGridView_InvalidRowException(object sender, DevExpress.XtraGrid.Views.Base.InvalidRowExceptionEventArgs e)
        {
            e.ExceptionMode = DevExpress.XtraEditors.Controls.ExceptionMode.NoAction;
        }

        public void myGridView_ValidateRow(object sender, DevExpress.XtraGrid.Views.Base.ValidateRowEventArgs e)
        {
            /*
            string TableIPCode = (((DevExpress.XtraGrid.Views.Grid.GridView)sender).GridControl).AccessibleName;
            Form tForm = (((DevExpress.XtraGrid.Views.Grid.GridView)sender).GridControl).FindForm();
            DataRow row = ((DevExpress.XtraGrid.Views.Grid.GridView)sender).GetFocusedDataRow();

            tDefaultValue df = new tDefaultValue();
            e.Valid = df. tDefaultValue_Fill_Check(tForm, row, TableIPCode);
            */


            /*  şimdilik gerek değil 
             * 

            //...
            // (((DevExpress.XtraGrid.Views.Grid.GridView)sender).ChildGridLevelName == "true");
            // (((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender).ChildGridLevelName == "true")

            Form tForm = null;
            string TableIPCode = string.Empty;
            string s = string.Empty;

            if (sender.GetType().ToString() == "DevExpress.XtraGrid.Views.Grid.GridView")
            {
                tForm = (((DevExpress.XtraGrid.Views.Grid.GridView)sender).GridControl).FindForm();
                TableIPCode = (((DevExpress.XtraGrid.Views.Grid.GridView)sender).GridControl).AccessibleName;
                //s = ((DevExpress.XtraGrid.Views.Grid.GridView)sender).ChildGridLevelName;
                ((DevExpress.XtraGrid.Views.Grid.GridView)sender).ChildGridLevelName = "";

            }
            if (sender.GetType().ToString() == "DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView")
            {
                tForm = (((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender).GridControl).FindForm();
                TableIPCode = (((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender).GridControl).AccessibleName;
                //s = ((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender).ChildGridLevelName;
                ((DevExpress.XtraGrid.Views.Grid.GridView)sender).ChildGridLevelName = "";
            }
            */

            //if ((tForm != null) && (t.IsNotNull(TableIPCode)))
            //{
            //tButton btn = new tButton();
            //e.Valid = btn.tTable_Save(tForm, TableIPCode, false, function_name);
            //}

        }

        public void myGridView_InitNewRow(object sender, InitNewRowEventArgs e)
        {
            //public static void myGridView_InitNewRow(object sender, DevExpress.XtraGrid.Views.Grid.InitNewRowEventArgs e)
            //...

            if (sender.GetType().ToString() == "DevExpress.XtraGrid.Views.Grid.GridView")
            {
                ((DevExpress.XtraGrid.Views.Grid.GridView)sender).ChildGridLevelName = "true";
            }

            if (sender.GetType().ToString() == "DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView")
            {
                ((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender).ChildGridLevelName = "true";
            }
        }

        public void myGridView_BeforeLeaveRow(object sender, RowAllowEventArgs e)
        {
            /*            
            //...
            // (((DevExpress.XtraGrid.Views.Grid.GridView)sender).ChildGridLevelName == "true");
            // (((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender).ChildGridLevelName == "true")

            Form tForm = null;
            string TableIPCode = string.Empty;
            string s = string.Empty;

            if (sender.GetType().ToString() == "DevExpress.XtraGrid.Views.Grid.GridView")
            {
                tForm = (((DevExpress.XtraGrid.Views.Grid.GridView)sender).GridControl).FindForm();
                TableIPCode = (((DevExpress.XtraGrid.Views.Grid.GridView)sender).GridControl).AccessibleName;
                s = ((DevExpress.XtraGrid.Views.Grid.GridView)sender).ChildGridLevelName;
                ((DevExpress.XtraGrid.Views.Grid.GridView)sender).ChildGridLevelName = "";

            }
            if (sender.GetType().ToString() == "DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView")
            {
                tForm = (((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender).GridControl).FindForm();
                TableIPCode = (((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender).GridControl).AccessibleName;
                s = ((DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender).ChildGridLevelName;
                ((DevExpress.XtraGrid.Views.Grid.GridView)sender).ChildGridLevelName = "";
            }

            if (s == "true")
            {
                //tButton btn = new tButton();
                //btn.tTable_Save(tForm, TableIPCode, true, function_name);
            }
            */

        }

        public void myGridView_DragEnter(object sender, DragEventArgs e)
        {
            //e.Effect = DragDropEffects.Copy;
            if (e.Data.GetDataPresent(typeof(DataRow))) 
            { e.Effect = DragDropEffects.Move; } 
            else 
            { e.Effect = DragDropEffects.None; }
        }

        public void myGridView_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            v.con_DragDropEdit = false;
        }
        public void myGridView_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {

            //v.myGrid_Location = e.Location;

            //Application.OpenForms[0].Text = "MouseDown : " + v.myGrid_Location.ToString();

            if (sender.GetType().ToString() == "DevExpress.XtraGrid.Views.Grid.GridView")
            {
                GridView view = sender as GridView;

                gridHitInfo = null;

                GridHitInfo hitInfo = view.CalcHitInfo(new Point(e.X, e.Y));

                if (Control.ModifierKeys != Keys.None)
                    return;

                if (e.Button == MouseButtons.Left && hitInfo.InRow && hitInfo.RowHandle != GridControl.NewItemRowHandle)
                    gridHitInfo = hitInfo;

                v.con_DragDropEdit = true;
            }

            if (sender.GetType().ToString() == "DevExpress.XtraGrid.Views.WinExplorer.WinExplorerView")
            {
                DevExpress.XtraGrid.Views.WinExplorer.WinExplorerView view = sender as DevExpress.XtraGrid.Views.WinExplorer.WinExplorerView;

                winExplorerHitInfo = null;

                WinExplorerViewHitInfo hitInfo = view.CalcHitInfo(new Point(e.X, e.Y));

                if (Control.ModifierKeys != Keys.None)
                    return;

                if (e.Button == MouseButtons.Left && hitInfo.InItem && hitInfo.RowHandle != GridControl.NewItemRowHandle)
                    winExplorerHitInfo = hitInfo;
            }


        }

        public void myGridView_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //Application.OpenForms[0].Text = "MouseMove : ";

            //myGridView_MouseMove_(sender, e);

            #region Drag anında mouse nin ucunda kutu ve + işareti oluşturyor

            if (sender.GetType().ToString() == "DevExpress.XtraGrid.Views.Layout.LayoutView")
            {
                LayoutView view = sender as DevExpress.XtraGrid.Views.Layout.LayoutView;
                if (e.Button == MouseButtons.Left && gridHitInfo != null)
                {
                    Size dragSize = SystemInformation.DragSize;
                    Rectangle dragRect = new Rectangle(new Point(gridHitInfo.HitPoint.X - dragSize.Width / 2,
                        gridHitInfo.HitPoint.Y - dragSize.Height / 2), dragSize);

                    if (!dragRect.Contains(new Point(e.X, e.Y)))
                    {
                        view.GridControl.DoDragDrop(gridHitInfo, DragDropEffects.All);
                        gridHitInfo = null;
                    }
                    
                }
            }

            if (sender.GetType().ToString() == "DevExpress.XtraGrid.Views.Grid.GridView")
            {
                
                GridView view = sender as GridView;
                if (e.Button == MouseButtons.Left && gridHitInfo != null)
                {
                    Size dragSize = SystemInformation.DragSize;
                    Rectangle dragRect = new Rectangle(new Point(gridHitInfo.HitPoint.X - dragSize.Width / 2,
                        gridHitInfo.HitPoint.Y - dragSize.Height / 2), dragSize);
                    
                    if (!dragRect.Contains(new Point(e.X, e.Y)))
                    {
                        view.GridControl.DoDragDrop(gridHitInfo, DragDropEffects.All);
                        gridHitInfo = null;
                    }
                    
                }
                
            }

            if (sender.GetType().ToString() == "DevExpress.XtraGrid.Views.WinExplorer.WinExplorerView")
            {
                DevExpress.XtraGrid.Views.WinExplorer.WinExplorerView view =
                    sender as DevExpress.XtraGrid.Views.WinExplorer.WinExplorerView;

                if (e.Button == MouseButtons.Left && winExplorerHitInfo != null)
                {
                    Size dragSize = SystemInformation.DragSize;
                    Rectangle dragRect = new Rectangle(new Point(winExplorerHitInfo.HitPoint.X - dragSize.Width / 2,
                        winExplorerHitInfo.HitPoint.Y - dragSize.Height / 2), dragSize);

                    if (!dragRect.Contains(new Point(e.X, e.Y)))
                    {
                        view.GridControl.DoDragDrop(winExplorerHitInfo, DragDropEffects.All);
                        winExplorerHitInfo = null;
                    }
                }
            }
            
            #endregion Drag anında

        }

        private void myGridView_MouseMove_(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            #region Mouse gezerken bulunduğu kaydın position değişmesi sağlanıyor

            /// DİKKAT : Burası DataCopy içindeki tDC_Run tarafından tetikleniyor yani çalışıyor 
            /// DC.WORK_TYPE > de 
            /// 3 = Satır Düzenle, Row.Update()   işlemini yapmak için önce 
            /// burada mouse hangi kaydın üzerinde ise 
            /// DataNavigator ün Position u değiştirmek gerekiyor


            if (v.con_DragDropEdit) // true idi
            {
                // yukarıda sender olan nesne gridControl içindeki view
                // bu view aracılığıyla gridControla ulaşıyoruz
                GridControl grid = null;

                if (sender.GetType().ToString() == "DevExpress.XtraGrid.Views.Layout.LayoutView")
                {
                    LayoutView view = sender as DevExpress.XtraGrid.Views.Layout.LayoutView;

                    /// Get a View at the current point.
                    //BaseView view = grid.GetViewAt(e.Location);
                    /// Retrieve information on the current View element.
                    BaseHitInfo baseHI = view.CalcHitInfo(e.Location);
                    LayoutViewHitInfo gridHI = baseHI as LayoutViewHitInfo;

                    if ((gridHI != null) && (gridHI.RowHandle > -1))
                    {
                        //string rowID = view.GetRowCellValue(gridHI.RowHandle, view.Columns[0]).ToString();
                        //string rowID = view.GetFocusedDataSourceRowIndex().ToString();

                        v.con_GridMouseValue = view.GetDataSourceRowIndex(gridHI.RowHandle).ToString();
                        grid = ((DevExpress.XtraGrid.Views.Layout.LayoutView)sender).GridControl as GridControl;
                    }
                }

                if (sender.GetType().ToString() == "DevExpress.XtraGrid.Views.Grid.GridView")
                {
                    GridView view = sender as GridView;
                    BaseHitInfo baseHI = view.CalcHitInfo(e.Location);
                    GridHitInfo gridHI = baseHI as GridHitInfo;

                    if ((gridHI != null) && (gridHI.RowHandle > 0))
                    {
                        v.con_GridMouseValue = view.GetDataSourceRowIndex(gridHI.RowHandle).ToString();
                        grid = ((DevExpress.XtraGrid.Views.Grid.GridView)sender).GridControl as GridControl;
                    }

                }

                myGridDataNavigatorPositionSetPreparing(grid);
            }

            #endregion Mouse gezerken
        }

        private void myGridDataNavigatorPositionSetPreparing(GridControl grid)
        {
            //Application.OpenForms[0].Text = "myGridDataNavigatorPositionSetPreparing : " + v.con_GridMouseValue;
            if (grid != null)
            {
                Form tForm = t.Find_Form(grid);
                string Prop_RunTime = grid.AccessibleDescription;

                // Herzaman yemiyor
                //if (gridHI.InLayoutItem)
                //    view.FocusedRowHandle = gridHI.RowHandle;

                // bunun yerine manuel yaptım 
                myGridDataNavigatorPositionSet(grid, t.myInt32(v.con_GridMouseValue));

                myDragDrop_RUN_(tForm, Prop_RunTime);

                grid.Invalidate();

                // işi sonlandır
                //v.con_DragDropEdit = false;
            }
        }

        public void myGridView_ShownEditor(object sender, EventArgs e)
        {
            //gridView.ShownEditor += (s, e) => {
            //    GridView view = s as GridView;
            // The editor can be accessed using the ActiveEditor property.
            //    gridView.ActiveEditor.BackColor = Color.DodgerBlue;
            //};
            if (sender.GetType().ToString() == "DevExpress.XtraGrid.Views.Grid.GridView")
            {
                GridView view = sender as GridView;

                view.ActiveEditor.BackColor = v.AppearanceFocusedColor;
            }

            if (sender.GetType().ToString() == "DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView")
            {
                GridView view = sender as AdvBandedGridView;

                view.ActiveEditor.BackColor = v.AppearanceFocusedColor;
            }

        }

        public void gridView_ShowingEditor(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
        }

        public void myBehavior_DragDropToGrid1(object sender, DragDropEventArgs e)
        {
            //
            v.Kullaniciya_Mesaj_Var = "grid1";
        }
        public void myBehavior_DragDropToGrid2(object sender, DragDropEventArgs e)
        {
            //
            v.Kullaniciya_Mesaj_Var = "grid2";
        }

        public void mySchedulerControl_AppointmentDrag(object sender, AppointmentDragEventArgs e)
        { 
            // Sürüklenen randevuyu belirgin yap
            e.EditedAppointment.LabelKey = 1;
        }
        public void mySchedulerControl_AppointmentDrop(object sender, AppointmentDragEventArgs e) 
        { 
            // Bırakılan randevuyu belirgin yap
            e.EditedAppointment.LabelKey = 10;

            if (((DevExpress.XtraScheduler.SchedulerControl)sender).AccessibleName == null) return;
            //if (((DevExpress.XtraScheduler.SchedulerControl)sender).AccessibleDescription == null) return;
            if (((DevExpress.XtraScheduler.SchedulerControl)sender).AccessibleDefaultActionDescription == null) return;

            string gun = e.EditedAppointment.Start.Date.ToString();
            int saat = e.EditedAppointment.Start.Hour;
            string saat_ = "";
            if (saat < 10) saat_ = "0" + saat.ToString() + ":00:11";
            else saat_ = saat.ToString() + ":00:11";

            //05.12.2024 00:00:00 >>  05.12.2024 ??:00:11  şekline getiriliyor
            string gun2 = gun.Replace("00:00:00", saat_);

            Form tForm = ((DevExpress.XtraScheduler.SchedulerControl)sender).FindForm();
            string tableIPCode = ((DevExpress.XtraScheduler.SchedulerControl)sender).AccessibleName;
            //string IdFieldName = ((DevExpress.XtraScheduler.SchedulerControl)sender).AccessibleDescription;
            //string startDateFieldName = ((DevExpress.XtraScheduler.SchedulerControl)sender).AccessibleDefaultActionDescription;
            string value = ((DevExpress.XtraScheduler.SchedulerControl)sender).AccessibleDefaultActionDescription;
            string IdFieldName = t.Get_And_Clear(ref value, "||");
            string startDateFieldName = t.Get_And_Clear(ref value, "||");
            DataSet ds = null;
            DataNavigator dN = null;
            t.Find_DataSet(tForm, ref ds, ref dN, tableIPCode);

            object  pattern = e.SourceAppointment.RowHandle;
            DataRow row = ((DataRowView)pattern).Row;

            if (ds != null)
            {
                string IdValue = row[IdFieldName].ToString();
                row[startDateFieldName] = gun2;
                ds.Tables[0].AcceptChanges();

                /// datnın positionu bul
                int pos = 0;
                foreach (DataRow item in ds.Tables[0].Rows)
                {
                    if (item[IdFieldName].ToString() == IdValue) break;
                    pos += 1;
                }

                tSave sv = new tSave();
                sv.tDataSave(tForm, ds, dN, pos);

                t.TableRefresh(tForm, ds);
            }

            // Örneğin, 2 numaralı etiketle belirgin yapın
            v.Kullaniciya_Mesaj_Var = "mySchedulerControl_AppointmentDrop";
        }


        #endregion myGridView Events

        #region myGridControl

        public void myGridControl_DragLeave(object sender, EventArgs e)
        {
            //Application.OpenForms[0].Text = "DragLeave : " + sender.ToString(); 
        }

        public void myGridControl_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            // Application.OpenForms[0].Text = "GiveFeedback : " + sender.ToString();
        }

        public void myGridControl_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            // Application.OpenForms[0].Text = "QueryContinueDrag : " + sender.ToString();
        }

        public void myGridControl_DragEnter(object sender, DragEventArgs e)
        {
            //Application.OpenForms[0].Text = "DragEnter : " + sender.ToString();

            //e.Effect = DragDropEffects.Copy;

            //GridControl grid = sender as GridControl;
            //GridView view = grid.MainView as GridView;

            //v.Row_DragOver = view.GetFocusedDataRow();
        }

        public void myGridControl_DragOver(object sender, System.Windows.Forms.DragEventArgs e)
        {
            
            //v.myGrid_Location = new Point(e.X, e.Y);
            ///Application.OpenForms[0].Text = "DragOver : " + sender.ToString() + " : " + v.myGrid_Location.ToString();

            //if (e.Data.GetDataPresent(typeof(DataRow)))
            //    e.Effect = DragDropEffects.Move;
            //else e.Effect = DragDropEffects.None;

            //GridControl grid = sender as GridControl;
            //GridView view = grid.MainView as GridView;

            if (e.Data.GetDataPresent(typeof(DataRow)))
                e.Effect = DragDropEffects.Move;
            else
                e.Effect = DragDropEffects.All;  //None;

            if (v.con_DragDropSourceTableIPCode == "")
            {
                GridControl grid = sender as GridControl;
                v.con_DragDropSourceTableIPCode = grid.AccessibleName;
            }
                        
            myGridView_DragOver_(sender, e);
        }

        private void myGridView_DragOver_(object sender, System.Windows.Forms.DragEventArgs e)
        {
            #region Mouse gezerken bulunduğu kaydın position değişmesi sağlanıyor

            /// DİKKAT : Burası DataCopy içindeki tDC_Run tarafından tetikleniyor yani çalışıyor 
            /// DC.WORK_TYPE > de 
            /// 3 = Satır Düzenle, Row.Update()   işlemini yapmak için önce 
            /// burada mouse hangi kaydın üzerinde ise 
            /// DataNavigator ün Position u değiştirmek gerekiyor

            if (v.con_DragDropEdit) // true idi
            {
                GridControl grid = sender as GridControl;

                if (grid.MainView.GetType().ToString() == "DevExpress.XtraGrid.Views.Layout.LayoutView")
                {
                    LayoutView view = grid.MainView as LayoutView;
                    //DataRow row = view.GetFocusedDataRow();
                    Point pt = grid.PointToClient(new Point(e.X, e.Y));

                    LayoutViewHitInfo hitInfo = view.CalcHitInfo(pt);
                    //if (hitInfo.HitTest == LayoutViewHitTest.? //  GridHitTest.EmptyRow)
                    //    v.DropTargetRowHandle = view.DataRowCount;
                    //else
                    v.DropTargetRowHandle = hitInfo.RowHandle;

                    if (v.DropTargetRowHandle < 0)
                        v.DropTargetRowHandle = view.SourceRowHandle;

                    v.con_GridMouseValue = view.GetDataSourceRowIndex(v.DropTargetRowHandle).ToString();

                    grid.Invalidate();
                }

                if (grid.MainView.GetType().ToString() == "DevExpress.XtraGrid.Views.Grid.GridView")
                {
                    GridView view = grid.MainView as GridView;
                    Point pt = grid.PointToClient(new Point(e.X, e.Y));

                    GridHitInfo hitInfo = view.CalcHitInfo(pt);
                    if (hitInfo.HitTest == GridHitTest.EmptyRow)
                        v.con_GridMouseValue = view.DataRowCount.ToString();
                    else
                        v.con_GridMouseValue = hitInfo.RowHandle.ToString();
                    /*
                    if (v.DropTargetRowHandle < 0)
                        v.DropTargetRowHandle = view.SourceRowHandle;

                    v.con_GridMouseValue = view.GetDataSourceRowIndex(v.DropTargetRowHandle).ToString();
                    */
                    //v.con_GridMouseValue = v.DropTargetRowHandle.ToString();
                }

                //Application.OpenForms[0].Text = "myGridView_DragOver_ : " + v.con_GridMouseValue;
                myGridDataNavigatorPositionSetPreparing(grid);
            }

            #endregion Mouse gezerken
        }

        public void myGridControl_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            //v.myGrid_Location = new Point(e.X, e.Y);
            //Application.OpenForms[0].Text = "DragDrop : " + sender.ToString() + " : " + v.myGrid_Location.ToString();

            GridControl grid = sender as GridControl;

            #region  

            if (grid.MainView.GetType().ToString() == "DevExpress.XtraGrid.Views.Layout.LayoutView")
            {
                LayoutView view = grid.MainView as LayoutView;
                //DataRow row = view.GetFocusedDataRow();
                Point pt = grid.PointToClient(new Point(e.X, e.Y));

                LayoutViewHitInfo hitInfo = view.CalcHitInfo(pt);
                //if (hitInfo.HitTest == LayoutViewHitTest.? //  GridHitTest.EmptyRow)
                //    v.DropTargetRowHandle = view.DataRowCount;
                //else
                v.DropTargetRowHandle = hitInfo.RowHandle;

                if (v.DropTargetRowHandle < 0)
                    v.DropTargetRowHandle = view.SourceRowHandle;

                v.con_GridMouseValue = view.GetDataSourceRowIndex(v.DropTargetRowHandle).ToString();

                grid.Invalidate();
            }

            if (grid.MainView.GetType().ToString() == "DevExpress.XtraGrid.Views.Grid.GridView")
            {
                //GridControl grid = sender as GridControl;
                //GridView view = grid.MainView as GridView;
                //GridHitInfo srcHitInfo = e.Data.GetData(typeof(GridHitInfo)) as GridHitInfo;
                //GridHitInfo hitInfo = view.CalcHitInfo(grid.PointToClient(new Point(e.X, e.Y)));
                //int sourceRow = srcHitInfo.RowHandle;
                //int targetRow = hitInfo.RowHandle;
                //Application.OpenForms[0].Text = view.GetDataSourceRowIndex[sourceRow].ToString();
                //DataRow row = ((DevExpress.XtraGrid.Views.Grid.GridView)sender).GetFocusedDataRow();
                //MoveRow(view, sourceRow, targetRow);


                /// Mouse nin hangi row üzerinde olduğunu bulmak için
                /// aşağıdaki işlemler yapılıyor

                // gridControl içindeki viewi tespit et
                GridView view = grid.MainView as GridView;
                Point pt = grid.PointToClient(new Point(e.X, e.Y));

                GridHitInfo hitInfo = view.CalcHitInfo(pt);
                if (hitInfo.HitTest == GridHitTest.EmptyRow)
                    v.DropTargetRowHandle = view.DataRowCount;
                else
                    v.DropTargetRowHandle = hitInfo.RowHandle;

                /*
                BaseHitInfo baseHI = view.CalcHitInfo(v.myGrid_Location);
                GridHitInfo gridHI = baseHI as GridHitInfo;
                if (gridHI.HitTest == GridHitTest.EmptyRow)
                    v.DropTargetRowHandle = view.DataRowCount;
                else
                    v.DropTargetRowHandle = gridHI.RowHandle;
                */

                if (v.DropTargetRowHandle < 0)
                    v.DropTargetRowHandle = view.SourceRowHandle;

                v.con_GridMouseValue = view.GetDataSourceRowIndex(v.DropTargetRowHandle).ToString();

                grid.Invalidate();
            }

            if (grid.MainView.GetType().ToString() == "DevExpress.XtraGrid.Views.WinExplorer.WinExplorerView")
            {
                //DevExpress.XtraGrid.Views.WinExplorer.WinExplorerView view =
                //    grid.MainView as DevExpress.XtraGrid.Views.WinExplorer.WinExplorerView;

                ////DevExpress.XtraGrid.Views.WinExplorer.ViewInfo.WinExplorerViewInfo
                ////WinExplorerViewHitInfo;

                //WinExplorerViewHitInfo srcHitInfo =
                //    e.Data.GetData(typeof(WinExplorerViewHitInfo)) as WinExplorerViewHitInfo;

                //int sourceRow = srcHitInfo.RowHandle;
            }

            if (grid.MainView.GetType().ToString() == "DevExpress.XtraGrid.Views.Card.CardView")
            {
                // gridControl içindeki viewi tespit et
                CardView view = grid.MainView as CardView;
                Point pt = grid.PointToClient(new Point(e.X, e.Y));

                CardHitInfo hitInfo = view.CalcHitInfo(pt);
                //if (hitInfo.HitTest == CardHitTest.EmptyRow)
                //    v.DropTargetRowHandle = view.DataRowCount;
                //else
                v.DropTargetRowHandle = hitInfo.RowHandle;

                if (v.DropTargetRowHandle < 0)
                    v.DropTargetRowHandle = view.SourceRowHandle;

                v.con_GridMouseValue = view.GetDataSourceRowIndex(v.DropTargetRowHandle).ToString();

                grid.Invalidate();
            }

            if (grid.MainView.GetType().ToString() == "DevExpress.XtraGrid.Views.Tile.TileView")
            {
                DevExpress.XtraGrid.Views.Tile.TileView view = grid.MainView as TileView;

                Point pt = grid.PointToClient(new Point(e.X, e.Y));

                //TileControlHitTest.

                TileViewHitInfo hitInfo = view.CalcHitInfo(pt);
                //if (hitInfo.HitTest == TileControlHitTest.  EmptyRow)
                //    v.DropTargetRowHandle = view.DataRowCount;
                //else
                v.DropTargetRowHandle = hitInfo.RowHandle;

                if (v.DropTargetRowHandle < 0)
                    v.DropTargetRowHandle = view.SourceRowHandle;

                v.con_GridMouseValue = view.GetDataSourceRowIndex(v.DropTargetRowHandle).ToString();

                grid.Invalidate();
            }

            if (grid.MainView.GetType().ToString() == "DevExpress.XtraVerticalGrid.VGridControl")
            {

            }
            #endregion

            if (grid != null)
            {
                if (t.IsNotNull(grid.AccessibleDescription))
                {
                    Form tForm = t.Find_Form(sender);
                    string Prop_RunTime = grid.AccessibleDescription;
                    v.con_DragDropTargetTableIPCode = grid.AccessibleName;

                    myGridDataNavigatorPositionSet(grid, t.myInt32(v.con_GridMouseValue));

                    myDragDrop_RUN_(tForm, Prop_RunTime);

                    grid.Invalidate();
                }
            }
            v.con_DragDropEdit = false;
        }

        public void myGridControl_Paint(object sender, PaintEventArgs e)
        {
            /*
            // aşağıdaki kodlar çalışmasın diye
            if (v.DropTargetRowHandle != -5) return;
            ///------------------------------------


            if (v.DropTargetRowHandle < 0) return;

            GridControl grid = (GridControl)sender;

            if (grid.MainView.GetType().ToString() == "DevExpress.XtraGrid.Views.Grid.GridView")
            {
                GridView view = (GridView)grid.MainView;

                bool isBottomLine = (v.DropTargetRowHandle + 1) == view.DataRowCount;

                GridViewInfo viewInfo = view.GetViewInfo() as GridViewInfo;
                //GridRowInfo rowInfo = viewInfo.GetGridRowInfo(isBottomLine ? v.DropTargetRowHandle - 1 : v.DropTargetRowHandle);
                GridRowInfo rowInfo = viewInfo.GetGridRowInfo(isBottomLine ? v.DropTargetRowHandle : v.DropTargetRowHandle);

                if (rowInfo == null) return;

                Point p1, p2;

                // satırın üsütünü çiyor buna gerek yok sürekli altını çizsin

                //if (isBottomLine)
                //{
                //    p1 = new Point(rowInfo.Bounds.Left, rowInfo.Bounds.Bottom - 1);
                //    p2 = new Point(rowInfo.Bounds.Right, rowInfo.Bounds.Bottom - 1);
                //}
                //else {
                //    p1 = new Point(rowInfo.Bounds.Left, rowInfo.Bounds.Top - 1);
                //    p2 = new Point(rowInfo.Bounds.Right, rowInfo.Bounds.Top - 1);
                //}

                p1 = new Point(rowInfo.Bounds.Left, rowInfo.Bounds.Bottom - 1);
                p2 = new Point(rowInfo.Bounds.Right, rowInfo.Bounds.Bottom - 1);

                e.Graphics.DrawLine(Pens.Blue, p1, p2);

                //v.DropTargetRowHandle = -1;
            }
            */
        }
               
        public void myGridControl_Enter(object sender, EventArgs e)
        {
            //MessageBox.Show("Form1_Enter");

            GridControl grid = sender as GridControl;
            Form tForm = grid.FindForm();
            v.formLastActiveControl = grid;
            vGridHint tGridHint = new vGridHint();

            v.formLastActiveControl = grid;

            if (grid.MainView.GetType().ToString() == "DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView")
            {
                AdvBandedGridView view = grid.MainView as AdvBandedGridView;
                view.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
                DevExpress.XtraGrid.Columns.GridColumn focusedColumn = view.FocusedColumn;
                if (view.FocusedColumn == null)
                {
                    getGridHint_(view, ref tGridHint);
                    tGridHint.currentColumn = view.FocusedColumn;
                    // kolonların tamamı readonly ise insert row oluşuyor
                    //view.FocusedColumn = GetNextFocusableColumn(tGridHint);
                    gridShowEditor(tGridHint);
                }
            }

            if (grid.MainView.GetType().ToString() == "DevExpress.XtraGrid.Views.Grid.GridView")
            {
                GridView view = grid.MainView as GridView;
                view.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
                DevExpress.XtraGrid.Columns.GridColumn focusedColumn = view.FocusedColumn;
                if (view.FocusedColumn == null)
                {
                    getGridHint_(view, ref tGridHint);
                    tGridHint.currentColumn = view.FocusedColumn;
                    // kolonların tamamı readonly ise insert row oluşuyor
                    //view.FocusedColumn = GetNextFocusableColumn(tGridHint);     
                    gridShowEditor(tGridHint);
                }
            }

            if (grid.MainView.GetType().ToString() == "DevExpress.XtraGrid.Views.Tile.TileView")
            {
                DevExpress.XtraGrid.Views.Tile.TileView view = grid.MainView as DevExpress.XtraGrid.Views.Tile.TileView;
                view.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
                view.Appearance.ItemFocused.Options.UseBackColor = true;
            }
        }

        public void myGridControl_Leave(object sender, EventArgs e)
        {
            //MessageBox.Show("Form1_Leave");

            GridControl grid = sender as GridControl;

            if (grid.MainView.GetType().ToString() == "DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView")
            {
                AdvBandedGridView view = grid.MainView as AdvBandedGridView;
                view.CloseEditor();
                view.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            }

            if (grid.MainView.GetType().ToString() == "DevExpress.XtraGrid.Views.Grid.GridView")
            {
                GridView view = grid.MainView as GridView;
                view.CloseEditor();
                view.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            }

            if (grid.MainView.GetType().ToString() == "DevExpress.XtraGrid.Views.Tile.TileView")
            {
                DevExpress.XtraGrid.Views.Tile.TileView view = grid.MainView as DevExpress.XtraGrid.Views.Tile.TileView;
                view.CloseEditor();
                view.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
                view.Appearance.ItemFocused.Options.UseBackColor = false;
            }
        
        }

        private void myGridDataNavigatorPositionSet(GridControl grid, int tPosition)
        {
            
            Form tForm = t.Find_Form(grid);
            string target_tableipcode = grid.AccessibleName;

            if (tPosition > -1)
            {
                DataNavigator tDataNavigator = t.Find_DataNavigator(tForm, target_tableipcode);

                tDataNavigator.Position = tPosition;
                //Application.OpenForms[0].Text = ":"+tPosition.ToString();
            }
           
        }

        private void myDragDrop_RUN_(Form tForm, string Prop_RunTime)
        {
            if ((Prop_RunTime == "") || (Prop_RunTime == null)) return;

            string s1 = "=ROW_PROP_RUNTIME:";
            string s2 = (char)34 + "DRAGDROP" + (char)34 + ": [";

            if (Prop_RunTime.IndexOf(s1) > -1)
            {
                Prop_RunTime = t.Find_Properies_Get_FieldBlock(Prop_RunTime, "DRAGDROP");
                myDragDrop_RUN_OLD(tForm, Prop_RunTime);
            }
            /// JSON
            if (Prop_RunTime.IndexOf(s2) > -1)
            {
                myDragDrop_RUN_JSON(tForm, Prop_RunTime);
            }

        }

        private void myDragDrop_RUN_JSON(Form tForm, string Prop_RunTime)
        {
            tDataCopy dc = new tDataCopy();

            string RunTime = string.Empty;
            string Prop_Navigator = string.Empty;
            t.String_Parcala(Prop_RunTime, ref RunTime, ref Prop_Navigator, "|ds|");
            /*
            PROP_RUNTIME packet = new PROP_RUNTIME();
            RunTime = RunTime.Replace((char)34, (char)39);
            //var prop_ = JsonConvert.DeserializeAnonymousType(RunTime, packet);
            */
            PROP_RUNTIME prop_ = t.readProp<PROP_RUNTIME>(RunTime);

            bool sonuc = false;
            string JBTYPE = string.Empty;
            string JBCODE = string.Empty;
            string JBAFTER = string.Empty;
            string JBNAVIGATOR = string.Empty;

            foreach (var item in prop_.DRAGDROP)
            {
                JBTYPE = item.JBTYPE.ToString();
                JBCODE = item.JBCODE.ToString();
                JBAFTER = item.JBAFTER.ToString();
                if (item.JBNAVIGATOR != null)
                    JBNAVIGATOR = item.JBNAVIGATOR.ToString();

                #region JBTYPE "DC"
                if (JBTYPE == "DC")
                {
                    if (t.IsNotNull(JBCODE))
                    {
                        //sonuc = dc.tDC_Run(tForm, v.SP_Conn_Proje_MSSQL, JBCODE);
                        sonuc = dc.tDC_Run(tForm, JBCODE);
                    }
                }
                #endregion JBTYPE

                #region JBTYPE "RPN" = Run Prop Navigator
                if (JBTYPE == "RPN")
                {
                    // tDC_Run() da  work_type = 5 için gerekiyor
                    v.con_DragDropForm = tForm;

                    if (JBNAVIGATOR == "52")
                    {
                        //ev.navigatorButtonExec_(tForm, "", Prop_Navigator, v.tNavigatorButton.nv_52_Yeni_Kart_FormIle);

                        v.tButtonHint.Clear();
                        v.tButtonHint.tForm = tForm;
                        v.tButtonHint.tableIPCode = "";
                        v.tButtonHint.propNavigator = Prop_Navigator;
                        v.tButtonHint.buttonType = v.tButtonType.btYeniKart; //btYeniHesapForm;
                        tEventsButton evb = new tEventsButton();
                        evb.btnClick(v.tButtonHint);
                    }
                    //v.con_DragDropSourceTableIPCode = "";
                }
                #endregion


                if (sonuc)
                {
                    if ((JBAFTER == "DRD") && t.IsNotNull(v.con_DragDropSourceTableIPCode))
                    {
                        #region Find DataSet
                        DataSet dsData = t.Find_DataSet(tForm, "", v.con_DragDropSourceTableIPCode, "");
                        if (dsData != null)
                        {
                            DataNavigator tDataNavigator = t.Find_DataNavigator(tForm, v.con_DragDropSourceTableIPCode);
                            int pos = tDataNavigator.Position;

                            #region pos > -1
                            if (pos > -1)
                            {
                                // Delete işlemi gerçekleşecek ve 
                                // tdataNavigator_PositionChanged gidecek
                                // Gittiğinde silme olduğu için tDataSave işlemi gerçekleşmeyecek
                                tDataNavigator.Tag = -97;

                                dsData.Tables[0].Rows[pos].Delete();
                                dsData.Tables[0].AcceptChanges();

                                v.con_DragDropSourceTableIPCode = "";

                                // Her zaman tdataNavigator_PositionChanged tetiklenmiyor ( özellikle 0 position da )
                                // bu nedenle atama yapılıyor 
                                tDataNavigator.Tag = tDataNavigator.Position;

                                // kaydet
                                //btn_Navigotor_Click(tForm, target_tableipcode, "kaydet", "22");
                            }
                            #endregion pos > -1
                        }
                        #endregion Find DataSet
                    }
                }
            }
        }

        private void myDragDrop_RUN_OLD(Form tForm, string Prop_RunTime)
        {
            #region DRAGDROP var ise

            if (t.IsNotNull(Prop_RunTime))
            {
                bool sonuc = false;
                sonuc = myJobs_RUN(tForm, Prop_RunTime);

                #region // JobAfter = JBAFTER:DRD; -- Drop Row Delete
                if ((sonuc) &&
                    (Prop_RunTime.IndexOf("JBAFTER:DRD") > -1))
                {
                    if (t.IsNotNull(v.con_DragDropSourceTableIPCode))
                    {
                        #region Find DataSet
                        DataSet dsData = t.Find_DataSet(tForm, "", v.con_DragDropSourceTableIPCode, "");
                        if (dsData != null)
                        {
                            //int pos = t.Find_DataNavigator_Position(tForm, v.con_DragDropSourceTableIPCode);
                            DataNavigator tDataNavigator = t.Find_DataNavigator(tForm, v.con_DragDropSourceTableIPCode);
                            int pos = tDataNavigator.Position;

                            #region pos > -1
                            if (pos > -1)
                            {
                                // Delete işlemi gerçekleşecek ve 
                                // tdataNavigator_PositionChanged gidecek
                                // Gittiğinde silme olduğu için tDataSave işlemi gerçekleşmeyecek
                                tDataNavigator.Tag = -97;

                                dsData.Tables[0].Rows[pos].Delete();
                                dsData.Tables[0].AcceptChanges();

                                v.con_DragDropSourceTableIPCode = "";

                                // Her zaman tdataNavigator_PositionChanged tetiklenmiyor ( özellikle 0 position da )
                                // bu nedenle atama yapılıyor 
                                tDataNavigator.Tag = tDataNavigator.Position;

                                // kaydet
                                //btn_Navigotor_Click(tForm, target_tableipcode, "kaydet", "22");
                            }
                            #endregion pos > -1
                        }
                        #endregion Find DataSet
                    }
                }
                /*//<ALOCK_0>
                    =BEGIN:;
                    =DRAGDROP://<BLOCK_0>
                    1=BEGIN:1;
                    1=CAPTION:Personel Copy;
                    1=JBTYPE:DC;
                    1=JBCODE:DC_VR_02;
                    1=JBAFTER:DRD;
                    1=END:1;
                    //<BLOCK_1>;
                    =DRAGDROP:FIN;
                    =BUTTON:null;
                    =BUTTON:FIN;
                    =END:;
                    //<ALOCK_1>
                */
                #endregion // JobAfter
            }

            #endregion DRAGDROP
        }

        public bool myJobs_RUN(Form tForm, string jobs)
        {
            tDataCopy dc = new tDataCopy();

            //string lockE = t.myBlock("tPropertiesPlusEdit", "END");
            string row_block = string.Empty;
            string caption = string.Empty;
            string JBTYPE = string.Empty;
            string JBCODE = string.Empty;
            string JBAFTER = string.Empty;
            bool sonuc = false;
            /* 
            DRAGDROP={
            1=ROW_DRAGDROP:1;
            1=CAPTION:Personeli Vard. Listene Ekle;
            1=JBTYPE:DC;
            1=JBCODE:DC_VR_02;
            1=JBAFTER:DRD;
            1=ROWE_DRAGDROP:1;
            DRAGDROP=};
            */

            string lockE = "=ROWE_";

            while (jobs.IndexOf(lockE) > -1)
            {
                row_block = t.Find_Properies_Get_RowBlock(ref jobs, "DRAGDROP");

                caption = t.MyProperties_Get(row_block, "CAPTION:");
                JBTYPE = t.MyProperties_Get(row_block, "JBTYPE:");
                JBCODE = t.MyProperties_Get(row_block, "JBCODE:");
                JBAFTER = t.MyProperties_Get(row_block, "JBAFTER:");

                if (JBTYPE == "DC")
                {
                    if (t.IsNotNull(JBCODE))
                    {
                        //sonuc = dc.tDC_Run(tForm, v.SP_Conn_Proje_MSSQL, JBCODE);
                        sonuc = dc.tDC_Run(tForm, JBCODE);
                    }
                }
            }

            return sonuc;
        }

        private void MoveRow(GridView view, int sourceRow, int targetRow)
        {

            if (sourceRow == targetRow || sourceRow == targetRow + 1)
                return;

            //GridView view = gridFieldView;
            DataRow row1 = view.GetDataRow(targetRow);
            DataRow row2 = view.GetDataRow(targetRow + 1);
            DataRow dragRow = view.GetDataRow(sourceRow);

            //decimal val1 = (decimal)row1[OrderFieldName];
            //if (row2 == null)
            //    dragRow[OrderFieldName] = val1 + 1;
            //else
            //{
            //    decimal val2 = (decimal)row2[OrderFieldName];
            //    dragRow[OrderFieldName] = (val1 + val2) / 2;
            //}
        }

        #endregion myGridControl Events

        #region myTileView

        public void myTileView_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            TileView view = sender as TileView;
            if (view == null) return;
            tileHitInfo = null;
            TileViewHitInfo hitInfo = view.CalcHitInfo(new Point(e.X, e.Y));
            if (Control.ModifierKeys != Keys.None) return;
            if (e.Button == MouseButtons.Left && hitInfo.RowHandle >= 0)
                tileHitInfo = hitInfo;
        }

        public void myTileView_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            TileView view = sender as TileView;
            if (view == null) return;
            if (e.Button == MouseButtons.Left && tileHitInfo != null)
            {
                Size dragSize = SystemInformation.DragSize;
                Rectangle dragRect = new Rectangle(new Point(tileHitInfo.HitPoint.X - dragSize.Width / 2,
                    tileHitInfo.HitPoint.Y - dragSize.Height / 2), dragSize);

                if (!dragRect.Contains(new Point(e.X, e.Y)))
                {
                    DataRow row = view.GetDataRow(tileHitInfo.RowHandle);
                    view.GridControl.DoDragDrop(row, DragDropEffects.Move);
                    tileHitInfo = null;
                    DevExpress.Utils.DXMouseEventArgs.GetMouseArgs(e).Handled = true;
                }
            }
        }

        public void myTileView_ItemClick(object sender, DevExpress.XtraGrid.Views.Tile.TileViewItemClickEventArgs e)
        {
            //cntrl.AccessibleDescription = Prop_Runtime + v.ENTER + "|ds|" + v.ENTER + Prop_Navigator;
            var item = e.Item;
            object obj = sender;

            vGridHint tGridHint = new vGridHint();
            getGridHint_(sender, ref tGridHint);

            // default
            tGridHint.buttonType = v.tButtonType.btListeyeEkle;

            System.Windows.Forms.KeyEventArgs key = new KeyEventArgs(v.Key_ExtraIslem);
            bool onay = commonGridClick(sender, key, tGridHint);

            //MessageBox.Show("aaa");
        }

        public void myTileView_ItemDoubleClick(object sender, DevExpress.XtraGrid.Views.Tile.TileViewItemClickEventArgs e)
        {
            //
            //MessageBox.Show("ddd");
        }

        #endregion myTileView

        #region myCardView 

        public void myCardView_CustomDrawCardCaption(object sender, DevExpress.XtraGrid.Views.Card.CardCaptionCustomDrawEventArgs e)
        {
            CardView view = sender as CardView;
            bool isFocused = e.RowHandle == view.FocusedRowHandle;
            // A brush to draw the background of the card caption.
            Brush backBrush;
            if (isFocused)
            {
                //backBrush = e.Cache.GetGradientBrush(e.Bounds, Color.LavenderBlush, Color.Navy,
                backBrush = e.Cache.GetGradientBrush(e.Bounds, v.AppearanceTextColor, Color.White,
                  LinearGradientMode.Vertical);

                Rectangle r = e.Bounds;
                // Draw a 3D border.

                ControlPaint.DrawBorder3D(e.Graphics, r, Border3DStyle.RaisedInner);

                r.Inflate(-1, -1);
                // Fill the background.
                e.Graphics.FillRectangle(backBrush, r);
                r.Inflate(-2, 0);

                // Draw the text.
                Brush foreBrush = Brushes.Black; //  v.AppearanceFocusedTextColor;
                e.Appearance.DrawString(e.Cache, view.GetCardCaption(e.RowHandle), r, foreBrush);
                // Default painting is not required.
                e.Handled = true;
            }

            /*
   CardView view = sender as CardView;
   bool isFocused = e.RowHandle == view.FocusedRowHandle;
   // A brush to draw the background of the card caption.
   Brush backBrush;
   if(isFocused)            
      backBrush = e.Cache.GetGradientBrush(e.Bounds, Color.LavenderBlush, Color.Navy, 
        LinearGradientMode.Vertical);
   else
      backBrush = e.Cache.GetGradientBrush(e.Bounds, Color.Cornsilk, Color.DarkKhaki, 
        LinearGradientMode.Vertical);
   // A brush to draw the text.
   Brush foreBrush = isFocused ? Brushes.White : Brushes.Chocolate;
   Rectangle r = e.Bounds;
   // Draw a 3D border.
   ControlPaint.DrawBorder3D(e.Graphics, r, Border3DStyle.RaisedInner);
   r.Inflate(-1, -1);
   // Fill the background.
   e.Graphics.FillRectangle(backBrush, r);         
   r.Inflate(-2, 0);         
   // Draw the text.
   e.Appearance.DrawString(e.Cache, view.GetCardCaption(e.RowHandle), r, foreBrush);
   // Default painting is not required.
   e.Handled = true;
            */
        }

        #endregion myCardView

        #region myTreeList
        /*
        public void myTreeList_KeyDown(object sender, KeyEventArgs e) //***
        {
            v.con_Expression = true;
            bool onay = false;

            DevExpress.XtraTreeList.TreeList view = sender as DevExpress.XtraTreeList.TreeList;

            if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                // MessageBox.Show("esc ...");

            }
            //view.edit

            if ((e.KeyCode == Keys.Return) ||
                (e.KeyCode == Keys.Enter) ||
                (e.KeyCode == v.Key_SearchEngine) ||
                (e.KeyCode == v.Key_Kaydet) ||
                (e.KeyCode == v.Key_YeniSatir)
                )
            {
                // preparing tGridHint
                //myGridHint_(sender, ref tGridHint);
                //onay = myWork_Keys_(sender, e, tGridHint);

                if (onay == false)
                {
                    /*
                    if (view.IsEditorFocused == false)
                    {
                        // editor açılsın kürsör aktif hale gelsin
                        if ((e.KeyCode == Keys.Return) ||
                            (e.KeyCode == Keys.Enter) ||
                            (e.KeyCode == Keys.F2))
                            e.Handled = true;
                    }* /
                }
            }

            if (((e.KeyCode == Keys.Tab) |
                 (e.KeyCode == Keys.Return) |
                 (e.KeyCode == Keys.Enter) |
                 (e.KeyCode == Keys.Right)) &&
                 (e.Handled == false) &&
                 (onay == false))
            {

                DevExpress.XtraTreeList.Columns.TreeListColumn focusedColumn = view.FocusedColumn;

                //view.FocusedColumn = GetNextFocusableColumn(view, view.FocusedColumn);
                view.ShowEditor();
                e.Handled = true;
            }

            if (e.KeyCode == Keys.Left)
            {
                //view.FocusedColumn = GetPrevFocusableColumn(view, view.FocusedColumn);
                view.ShowEditor();
                e.Handled = true;
            }

            //MessageBox.Show(e.Handled.ToString());
        }
        */
        #endregion myTreeList

        #region myVGridControl

        /// <summary>
        /// VGridControl 
        /// </summary>
        
        public void myVGridControl_Enter(object sender, EventArgs e)
        {
            DevExpress.XtraVerticalGrid.VGridControl grid = sender as DevExpress.XtraVerticalGrid.VGridControl;
            grid.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            Form tForm = grid.FindForm();
            v.formLastActiveControl = grid;
        }
        public void myVGridControl_Leave(object sender, EventArgs e)
        {
            DevExpress.XtraVerticalGrid.VGridControl grid = sender as DevExpress.XtraVerticalGrid.VGridControl;
            grid.CloseEditor();
            grid.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
        }

        public void myVGridControl_CellValueChanging(object sender, DevExpress.XtraVerticalGrid.Events.CellValueChangedEventArgs e)
        {
            v.con_ColumnChangesCount = 0;
        }
        public void myVGridControl_CellValueChanged(object sender, DevExpress.XtraVerticalGrid.Events.CellValueChangedEventArgs e)
        {
            //v.con_ColumnChangesCount = 0;
        }

        #endregion myVGridControl Events

        #region myAdvBandedGridGroupButton Events

        public void checkGridGroupButton_Click(object sender, EventArgs e)
        {
            Form tForm = t.Find_Form(sender);
            int h = ((DevExpress.XtraEditors.CheckButton)sender).TabIndex;
            string TableIPCode = ((DevExpress.XtraEditors.CheckButton)sender).AccessibleDescription;

            selectBands(tForm, TableIPCode, h);
        }

        public void selectBands(Form tForm, string TableIPCode, int tabIndex)
        {
            Control cntrl = null;
            cntrl = t.Find_Control_View(tForm, TableIPCode);

            if (cntrl != null)
            {
                //AdvBandedGridView tView = ((DevExpress.XtraGrid.GridControl)cntrl).MainView as AdvBandedGridView;
                BandedGridView tView = ((DevExpress.XtraGrid.GridControl)cntrl).MainView as BandedGridView;
                tView.BeginInit();
                int j = tView.Bands.Count;
                for (int i = 0; i < j; i++)
                {
                    if (tView.Bands[i].Fixed == DevExpress.XtraGrid.Columns.FixedStyle.None)
                    {
                        tView.Bands[i].Visible = (i == tabIndex);
                        if ((i == tabIndex))
                            SetUserRegistrySelectBands(v.tUser.UserId, TableIPCode, tabIndex);
                    }
                }
                tView.EndInit();
                tView.Focus();
            }
        }

        void SetUserRegistrySelectBands(int UserId, string TableIPCode, int tabIndex)
        {
            if (tabIndex == 0) return;
            reg.SetUstadRegistry("userSelectBands||" + UserId.ToString() + "||"+ TableIPCode, tabIndex.ToString());
        }

        public int GetUserRegistrySelectBands(int UserId, string TableIPCode)
        {
            //tRegistry reg = new tRegistry();
            //object tabIndex = null;

            int tabIndex = 0;

            try
            {
                tabIndex = Convert.ToInt32(reg.getRegistryValue("userSelectBands||" + UserId.ToString() + "||" + TableIPCode));
            }
            catch (Exception)
            {
                tabIndex = 1;
            }

            return tabIndex;
        }

        #endregion myAdvBandedGridGroupButton Events

        #region mySchedulerGroupButton Events

        public void checkSchedulerGroupButton_Click(object sender, EventArgs e)
        {
            Form tForm = t.Find_Form(sender);
            int h = ((DevExpress.XtraEditors.CheckButton)sender).TabIndex;
            string TableIPCode = ((DevExpress.XtraEditors.CheckButton)sender).AccessibleDescription;

            selectSchedulerView(tForm, TableIPCode, h);
        }
        public void checkSchedulerWeekCount(object sender, EventArgs e)
        {
            Form tForm = t.Find_Form(sender);
            
            string TableIPCode = ((DevExpress.XtraEditors.SpinEdit)sender).AccessibleName;

            //selectSchedulerView(tForm, TableIPCode, h);
        }


        public void selectSchedulerView(Form tForm, string TableIPCode, int tabIndex)
        {
            Control cntrl = null;
            cntrl = t.Find_Control_View(tForm, TableIPCode);

            if (cntrl != null)
            {
                //DevExpress.XtraScheduler.SchedulerControl tView = 
                //    ((DevExpress.XtraScheduler.SchedulerControl)cntrl) as DevExpress.XtraScheduler.SchedulerControl;

                //tView.BeginInit();

                //SchedulerViewType.Agenda
                //SchedulerViewType.Day
                //SchedulerViewType.FullWeek
                //SchedulerViewType.Gantt
                //SchedulerViewType.Month
                //SchedulerViewType.Timeline
                //SchedulerViewType.Week
                //SchedulerViewType.WorkWeek
                
                ((DevExpress.XtraScheduler.SchedulerControl)cntrl).BeginInit();
                
                if (tabIndex == 1) 
                    ((DevExpress.XtraScheduler.SchedulerControl)cntrl).ActiveViewType = DevExpress.XtraScheduler.SchedulerViewType.Agenda;
                if (tabIndex == 2)
                {
                    ((DevExpress.XtraScheduler.SchedulerControl)cntrl).ActiveViewType = DevExpress.XtraScheduler.SchedulerViewType.Day;
                    ((DevExpress.XtraScheduler.SchedulerControl)cntrl).DayView.TimeRulers.Add(v.timeRuler);
                    ((DevExpress.XtraScheduler.SchedulerControl)cntrl).DayView.TimeRulers[0].Visible = true;
                }
                if (tabIndex == 3)
                {
                    ((DevExpress.XtraScheduler.SchedulerControl)cntrl).ActiveViewType = DevExpress.XtraScheduler.SchedulerViewType.FullWeek;
                    ((DevExpress.XtraScheduler.SchedulerControl)cntrl).FullWeekView.TimeRulers.Add(v.timeRuler);
                    ((DevExpress.XtraScheduler.SchedulerControl)cntrl).FullWeekView.TimeRulers[0].Visible = true;
                    //((DevExpress.XtraScheduler.SchedulerControl)cntrl).FullWeekView.ShowMoreButtonsOnEachColumn = true;
                }
                if (tabIndex == 4) 
                    ((DevExpress.XtraScheduler.SchedulerControl)cntrl).ActiveViewType = DevExpress.XtraScheduler.SchedulerViewType.Gantt;
                if (tabIndex == 5) 
                    ((DevExpress.XtraScheduler.SchedulerControl)cntrl).ActiveViewType = DevExpress.XtraScheduler.SchedulerViewType.Month;
                if (tabIndex == 6) 
                    ((DevExpress.XtraScheduler.SchedulerControl)cntrl).ActiveViewType = DevExpress.XtraScheduler.SchedulerViewType.Timeline;
                if (tabIndex == 7)
                {
                    ((DevExpress.XtraScheduler.SchedulerControl)cntrl).ActiveViewType = DevExpress.XtraScheduler.SchedulerViewType.Week;
                    //((DevExpress.XtraScheduler.SchedulerControl)cntrl).WeekView.TimeRulers.Add(v.timeRuler);
                    //((DevExpress.XtraScheduler.SchedulerControl)cntrl).WeekView.TimeRulers[0].Visible = true;
                }
                if (tabIndex == 8)
                {
                    ((DevExpress.XtraScheduler.SchedulerControl)cntrl).ActiveViewType = DevExpress.XtraScheduler.SchedulerViewType.WorkWeek;
                    ((DevExpress.XtraScheduler.SchedulerControl)cntrl).WorkWeekView.TimeRulers.Add(v.timeRuler);
                    ((DevExpress.XtraScheduler.SchedulerControl)cntrl).WorkWeekView.TimeRulers[0].Visible = true;
                }

                ((DevExpress.XtraScheduler.SchedulerControl)cntrl).EndInit();

                /*
                int j = tView.Bands.Count;
                for (int i = 0; i < j; i++)
                {
                    if (tView.Bands[i].Fixed == DevExpress.XtraGrid.Columns.FixedStyle.None)
                    {
                        tView.Bands[i].Visible = (i == tabIndex);
                        if ((i == tabIndex))
                            SetUserRegistrySelectBands(v.tUser.UserId, TableIPCode, tabIndex);
                    }
                }
                */

                //tView.EndInit();
                ((DevExpress.XtraScheduler.SchedulerControl)cntrl).Focus();
            }
        }


        #endregion mySchedulerGroupButton Events

        #region vGridView Kriter Key Functions  >>

        public void myVGridControl_Kriter_KeyDown(object sender, KeyEventArgs e) //***
        {
            string myObject = sender.ToString();

            object mySender = null;

            if (e.KeyCode == Keys.Return)
            {
                if (myObject != "DevExpress.XtraVerticalGrid.VGridControl")
                {

                    #region Objeects
                    if (myObject == "DevExpress.XtraEditors.ButtonEdit")
                        mySender = ((DevExpress.XtraEditors.ButtonEdit)sender).Parent;

                    if (myObject == "DevExpress.XtraEditors.CalcEdit")
                        mySender = ((DevExpress.XtraEditors.CalcEdit)sender).Parent;

                    if (myObject == "DevExpress.XtraEditors.CheckButton")
                        mySender = ((DevExpress.XtraEditors.CheckButton)sender).Parent;

                    if (myObject == "DevExpress.XtraEditors.CheckedComboBoxEdit")
                        mySender = ((DevExpress.XtraEditors.CheckedComboBoxEdit)sender).Parent;

                    if (myObject == "DevExpress.XtraEditors.CheckEdit")
                        mySender = ((DevExpress.XtraEditors.CheckEdit)sender).Parent;

                    if (myObject == "DevExpress.XtraEditors.CheckedListBoxControl")
                        mySender = ((DevExpress.XtraEditors.CheckedListBoxControl)sender).Parent;

                    if (myObject == "DevExpress.XtraEditors.ComboBoxEdit")
                        mySender = ((DevExpress.XtraEditors.ComboBoxEdit)sender).Parent;

                    if (myObject == "DevExpress.XtraEditors.DateEdit")
                        mySender = ((DevExpress.XtraEditors.DateEdit)sender).Parent;

                    if (myObject == "DevExpress.XtraEditors.HyperLinkEdit")
                        mySender = ((DevExpress.XtraEditors.HyperLinkEdit)sender).Parent;

                    if (myObject == "DevExpress.XtraEditors.ImageComboBoxEdit")
                        mySender = ((DevExpress.XtraEditors.ImageComboBoxEdit)sender).Parent;

                    if (myObject == "DevExpress.XtraEditors.ImageEdit")
                        mySender = ((DevExpress.XtraEditors.ImageEdit)sender).Parent;

                    if (myObject == "DevExpress.XtraEditors.ImageListBoxControl")
                        mySender = ((DevExpress.XtraEditors.ImageListBoxControl)sender).Parent;

                    if (myObject == "DevExpress.XtraEditors.LabelControl")
                        mySender = ((DevExpress.XtraEditors.LabelControl)sender).Parent;

                    if (myObject == "DevExpress.XtraEditors.ListBoxControl")
                        mySender = ((DevExpress.XtraEditors.ListBoxControl)sender).Parent;

                    if (myObject == "DevExpress.XtraEditors.LookUpEdit")
                        mySender = ((DevExpress.XtraEditors.LookUpEdit)sender).Parent;

                    if (myObject == "DevExpress.XtraEditors.MemoEdit")
                        mySender = ((DevExpress.XtraEditors.MemoEdit)sender).Parent;

                    if (myObject == "DevExpress.XtraEditors.MemoExEdit")
                        mySender = ((DevExpress.XtraEditors.MemoExEdit)sender).Parent;

                    if (myObject == "DevExpress.XtraEditors.MRUEdit")
                        mySender = ((DevExpress.XtraEditors.MRUEdit)sender).Parent;

                    if (myObject == "DevExpress.XtraEditors.PictureEdit")
                        mySender = ((DevExpress.XtraEditors.PictureEdit)sender).Parent;

                    if (myObject == "DevExpress.XtraEditors.PopupContainerControl")
                        mySender = ((DevExpress.XtraEditors.PopupContainerControl)sender).Parent;

                    if (myObject == "DevExpress.XtraEditors.PopupContainerEdit")
                        mySender = ((DevExpress.XtraEditors.PopupContainerEdit)sender).Parent;

                    if (myObject == "DevExpress.XtraEditors.RangeTrackBarControl")
                        mySender = ((DevExpress.XtraEditors.RangeTrackBarControl)sender).Parent;

                    if (myObject == "DevExpress.XtraEditors.SpinEdit")
                        mySender = ((DevExpress.XtraEditors.SpinEdit)sender).Parent;

                    if (myObject == "DevExpress.XtraEditors.TextEdit")
                        mySender = ((DevExpress.XtraEditors.TextEdit)sender).Parent;

                    if (myObject == "DevExpress.XtraEditors.TimeEdit")
                        mySender = ((DevExpress.XtraEditors.TimeEdit)sender).Parent;

                    if (myObject == "DevExpress.XtraEditors.TrackBarControl")
                        mySender = ((DevExpress.XtraEditors.TrackBarControl)sender).Parent;

                    //if (myObject == "DevExpress.XtraEditors.")
                    //    mySender = ((DevExpress.XtraEditors.)sender).Parent;

                    //DevExpress.XtraEditors.ButtonEdit
                    //DevExpress.XtraEditors.CalcEdit
                    //DevExpress.XtraEditors.CheckButton
                    //DevExpress.XtraEditors.CheckedComboBoxEdit
                    //DevExpress.XtraEditors.CheckEdit
                    //DevExpress.XtraEditors.CheckedListBoxControl
                    //DevExpress.XtraEditors.ComboBoxEdit
                    //DevExpress.XtraEditors.DateEdit
                    //DevExpress.XtraEditors.HyperLinkEdit
                    //DevExpress.XtraEditors.ImageComboBoxEdit
                    //DevExpress.XtraEditors.ImageEdit
                    //DevExpress.XtraEditors.ImageListBoxControl
                    //DevExpress.XtraEditors.LabelControl
                    //DevExpress.XtraEditors.ListBoxControl
                    //DevExpress.XtraEditors.LookUpEdit
                    //DevExpress.XtraEditors.MemoEdit
                    //DevExpress.XtraEditors.MemoExEdit
                    //DevExpress.XtraEditors.MRUEdit
                    //DevExpress.XtraEditors.PictureEdit
                    //DevExpress.XtraEditors.PopupContainerControl
                    //DevExpress.XtraEditors.PopupContainerEdit
                    //DevExpress.XtraEditors.RangeTrackBarControl
                    //DevExpress.XtraEditors.SpinEdit
                    //DevExpress.XtraEditors.TextEdit
                    //DevExpress.XtraEditors.TimeEdit
                    //DevExpress.XtraEditors.TrackBarControl
                    #endregion Objeects
                }
            }

            if (mySender != null)
            {
                if (mySender.GetType().ToString() == "DevExpress.XtraVerticalGrid.VGridControl")
                {
                    if (e.KeyCode == Keys.Return)
                    {
                        Form tForm = ((DevExpress.XtraVerticalGrid.VGridControl)mySender).FindForm();

                        string tTableIPCode = string.Empty;

                        // tGridControl.AccessibleName = "KRITER_" + TableIPCode;
                        int j = ((DevExpress.XtraVerticalGrid.VGridControl)mySender).AccessibleName.IndexOf("KRITER_");
                        if (j > -1)
                        {
                            j = j + 7; // "KRITER_" length
                            tTableIPCode = ((DevExpress.XtraVerticalGrid.VGridControl)mySender).AccessibleName.ToString();
                            tTableIPCode = tTableIPCode.Substring(j, tTableIPCode.Length - j);
                        }
                        else
                        {
                            tTableIPCode = ((DevExpress.XtraVerticalGrid.VGridControl)mySender).AccessibleName.ToString();
                        }

                        string[] controls = new string[] { "DevExpress.XtraEditors.SimpleButton" };
                        Control c = t.Find_Control(tForm, "simpleButton_Kriter_Listele", tTableIPCode, controls);
                        if (c != null)
                            ev.btn_KriterListele_Click(((DevExpress.XtraEditors.SimpleButton)c), EventArgs.Empty);
                        //((DevExpress.XtraEditors.SimpleButton)c).PerformClick();
                    }
                }
            }
        }

        public void myVGridControl_Kriter_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        public void myVGridControl_Kriter_KeyUp(object sender, KeyEventArgs e)
        {

        }

        #endregion vGridView Kriter Key Functions <<

    }

}
