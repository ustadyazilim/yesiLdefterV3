using CefSharp;
using CefSharp.WinForms;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraReports.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using Tkn_DevColumn;
using Tkn_Events;
using Tkn_Forms;
using Tkn_InputPanel;
using Tkn_Menu;
using Tkn_TablesRead;
using Tkn_ToolBox;
using Tkn_Variable;
using YesiLdefter.CEFSharp;
using CefSharp;
using CefSharp.WinForms;
using DevExpress.XtraGauges.Win;
using DevExpress.XtraGauges.Core.Model;
using DevExpress.XtraGauges.Core.Base;
using DevExpress.XtraGauges.Win.Gauges.Digital;
using DevExpress.XtraGauges.Core.Drawing;
using System.Drawing;
using DevExpress.Utils.DragDrop;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.BandedGrid;
using System.ComponentModel;

namespace Tkn_Layout
{
    class tLayout : tBase
    {
        public void Create_Layout(Form tForm, string FormCode, Control tabPageCntrl)
        {
            tTablesRead tr = new tTablesRead();

            DataSet ds_Layout = new DataSet();

            //tr.MS_LayoutOrItems_Read(ds_Layout, FormCode, 1);
            tr.MS_Layout_Read(ds_Layout, FormCode);

            Create_Layout(tForm, tabPageCntrl, ds_Layout);
        }

        public void Create_Layout(Form tForm, string FormCode)
        {
            tTablesRead tr = new tTablesRead();

            DataSet ds_Layout = new DataSet();

            //tr.MS_LayoutOrItems_Read(ds_Layout, FormCode, 1);
            tr.MS_Layout_Read(ds_Layout, FormCode);

            tForm.AccessibleName = FormCode; // v.con_Source_FormCodeAndName

            if (tForm.Name.IndexOf("ms_Reports") > -1)
                tForm.AccessibleName = v.con_Source_FormCode;

            Create_Layout(tForm, null, ds_Layout);
        }


        private void Create_Layout(Form tForm, Control subView, DataSet ds_Layout)
        {
            Cursor.Current = Cursors.Hand;

            tToolBox t = new tToolBox();

            if (t.IsNotNull(ds_Layout) == false) return;

            int pos = -1;
            int itemCount = ds_Layout.Tables[0].Rows.Count;
            short MasterLayoutType = 0;
            string LayoutType = string.Empty;
            string visible = string.Empty;
            
            string setfocus_TableIPCode = string.Empty;
            string setfocus_FieldName = string.Empty;
            string setfocus_CmpName = string.Empty;
            //string setfocus_FormName = string.Empty;

            bool dockPanel = false;

            // Form için tanımlanmış
            foreach (DataRow row in ds_Layout.Tables[0].Rows)
            {
                pos++;
                MasterLayoutType = t.myInt16(row["MASTER_LAYOUT_TYPE"].ToString());
                LayoutType = row["LAYOUT_TYPE"].ToString();
                visible = t.Set(row["CMP_VISIBLE"].ToString(), "", "");

                if ((MasterLayoutType == 1) && (LayoutType == ""))
                {
                    // sf : SetFocus
                    setfocus_TableIPCode = row["TABLEIPCODE"].ToString();
                    setfocus_FieldName = row["FIELD_NAME"].ToString();
                    setfocus_CmpName = row["CMP_NAME"].ToString();

                    if (subView == null)
                        lForm_Preparing(tForm, row);
                }

                if (visible == "True")
                {
                    // subView varsa menü oluşturma yoksa oluştur
                    if (LayoutType == v.lyt_menu) //&& (subView == null))
                        lMenu_Preparing(tForm, subView, ds_Layout, row, pos);

                    if ((LayoutType == v.lyt_dockPanel) && (dockPanel == false))
                    {
                        lDockPanel_Preparing(tForm, ds_Layout, row);
                        dockPanel = true;
                    }
                    if (LayoutType == v.lyt_tableLayoutPanel) 
                        ltableLayoutPanel_Preparing(tForm, subView, ds_Layout, row, pos);
                    if (LayoutType == v.lyt_splitContainer) 
                        lSplitContainer_Preparing(tForm, subView, ds_Layout, row, pos);
                    if (LayoutType == v.lyt_groupControl)
                        lGroupControl_Preparing(tForm, subView, ds_Layout, row, pos);
                    if (LayoutType == v.lyt_panelControl) 
                        lPanelControl_Preparing(tForm, subView, ds_Layout, row, pos);

                    if (LayoutType == v.lyt_tabPane) 
                        lTabPane_Preparing(tForm, subView, ds_Layout, row, pos);
                    if (LayoutType == v.lyt_tabNavigationPage) 
                        lTabNavigationPage_Preparing(tForm, ds_Layout, row, pos);

                    if (LayoutType == v.lyt_navigationPane) 
                        lNavigationPane_Preparing(tForm, subView, ds_Layout, row, pos);
                    if (LayoutType == v.lyt_navigationPage) 
                        lNavigationPage_Preparing(tForm, ds_Layout, row, pos);

                    if (LayoutType == v.lyt_tabControl) 
                        lTabControl_Preparing(tForm, subView, ds_Layout, row, pos);
                    if (LayoutType == v.lyt_tabPage) 
                        lTabPage_Preparing(tForm, ds_Layout, row, pos);

                    if (LayoutType == v.lyt_backstageViewControl) 
                        lBackstageViewControl_Preparing(tForm, subView, ds_Layout, row, pos);
                    if (LayoutType == v.lyt_backstageViewTabItem) 
                        lBackstageViewTabItem_Preparing(tForm, ds_Layout, row, pos);
                    if (LayoutType == v.lyt_backstageViewButtonItem) 
                        lBackstageViewButtonItem_Preparing(tForm, ds_Layout, row, pos);
                    if (LayoutType == v.lyt_backstageViewItemSeparator) 
                        lBackstageViewItemSeparator_Preparing(tForm, ds_Layout, row, pos);

                    if (LayoutType == v.lyt_webBrowser) 
                        lWebBrowser_Preparing(tForm, subView, ds_Layout, row, pos);
                    if (LayoutType == v.lyt_cefWebBrowser) 
                        lCefWebBrowser_Preparing(tForm, subView, ds_Layout, row, pos);

                    if (LayoutType == v.lyt_headerPanel) 
                        lHeaderPanel_Preparing(tForm, subView, ds_Layout, row, pos);
                    if (LayoutType == v.lyt_editPanel) 
                        lEditPanel_Preparing(tForm, subView, ds_Layout, row, pos);
                    if (LayoutType == v.lyt_labelControl) 
                        lLabelControl_Preparing(tForm, subView, ds_Layout, row, pos);
                    if (LayoutType == v.lyt_BarcodeControl1) 
                        lBarcodControl1_Preparing(tForm, subView, ds_Layout, row, pos);
                    if (LayoutType == v.lyt_ComponentControl) 
                        lComponentControl_Preparing(tForm, subView, ds_Layout, row, pos);
                    if (LayoutType == v.lyt_SimpleButton)
                        lSimpleButton_Preparing(tForm, subView, ds_Layout, row, pos);
                    if (LayoutType == v.lyt_DigitalGauge)
                        lDigitalPanel_Preparing(tForm, subView, ds_Layout, row, pos);

                    if (LayoutType == v.lyt_documentViewerFast) 
                        lDocumentViewerFast_Preparing(tForm, subView, ds_Layout, row, pos);
                    if (LayoutType == v.lyt_documentViewerDev)
                        lDocumentViewerDev_Preparing(tForm, subView, ds_Layout, row, pos);

                    if (LayoutType == v.lyt_documentManager) 
                        lDocumentManager_Preparing(tForm, subView, ds_Layout, row, pos);
                    if (LayoutType == v.lyt_dataWizard) 
                        lDataWizard_Preparing(tForm, ds_Layout, row, pos);
                }
            }

            // SetFocus olacak Control ayarlanıyor 
            //

            // TableIPCode ve fieldName belli ise
            //
            if (t.IsNotNull(setfocus_TableIPCode) &&
                t.IsNotNull(setfocus_FieldName))
            {
                if (tForm.AccessibleDefaultActionDescription != null)
                {
                    //MessageBox.Show("DİKKAT : Formun Set Focus Control bilgi saklama alanında sorun var ...");
                }
                else
                {
                    /// formun üzerine bu bilgiyi yapıştıralım, çünkü Yeni butonuna basılıncada 
                    /// bu sistem yeniden çalışsın
                    tForm.AccessibleDefaultActionDescription = setfocus_TableIPCode + "||" + setfocus_FieldName + "||";
                }

                t.tFormActiveControl(tForm, setfocus_TableIPCode, "Column_", setfocus_FieldName);
            }

            //  direk componnet belirtilmişse
            //
            if (t.IsNotNull(setfocus_CmpName))
            {
                t.tFormActiveControl(tForm, setfocus_CmpName);
            }

            // hiç birşey belitilmemişse
            //
            if (t.IsNotNull(setfocus_TableIPCode) == false)
            {
                setfocus_TableIPCode = t.Find_FirstTableIPCode(tForm);
            }

            // sadece TaleIPCode belirtilmişse
            //
            if (t.IsNotNull(setfocus_TableIPCode) &&
                t.IsNotNull(setfocus_FieldName) == false)
            {
                if (tForm.AccessibleDefaultActionDescription != null)
                {
                    //MessageBox.Show("DİKKAT : Formun Set Focus Control bilgi saklama alanında sorun var ...");
                }
                else
                {
                    /// formun üzerine bu bilgiyi yapıştıralım, çünkü Yeni butonuna basılıncada 
                    /// bu sistem yeniden çalışsın
                    tForm.AccessibleDefaultActionDescription = setfocus_TableIPCode + "||null||";
                }

                // focus için iptal
                t.tFormActiveControl(tForm, setfocus_TableIPCode, "", "");
            }

            Cursor.Current = Cursors.Default;

            v.con_CreatePopupContainer = false;
            v.IsWaitOpen = false;
            t.WaitFormClose();
        }

        private void lForm_Preparing(Form tForm, DataRow row)
        {
            if (tForm == null) return;

            tToolBox t = new tToolBox();
            tEventsForm evf = new tEventsForm();

            int width = t.myInt32(row["CMP_WIDTH"].ToString());
            int height = t.myInt32(row["CMP_HEIGHT"].ToString());
            string caption = row["LAYOUT_CAPTION"].ToString();

            if ((width > 0) && (height > 0))
                tForm.Size = new System.Drawing.Size(width, height);

            evf.myFormEventsAdd(tForm);

            tForm.Text = caption;
        }

        private void lMenu_Preparing(Form tForm, Control subView, DataSet ds_Layout, DataRow row, int pos)
        {
            tMenu mn = new tMenu();


            
            string TABLEIPCODE = row["TABLEIPCODE"].ToString();

            // olabilecek valueler hakkında : 
            // 1. Ribbon menude hangi page nin aktif olması gerektiğinin bilgisi geliyor
            // 2. TileNavPane için FIRM ifadesi
            //
            string fieldName = row["FIELD_NAME"].ToString();

            // olabilecek valueler hakkında : 
            // 1. TileNavPane için FIRM menüsü için kullanıulacak IPCode geliyor
            //
            string TABLEIPCODE2 = row["TABLEIPCODE2"].ToString();

            // FIRM işareti ve kullanılacak TableIPCode paketleniyor
            if (fieldName == "FIRMF")
                fieldName = "FIRMF||" + TABLEIPCODE2 + "|ds|";
            if (fieldName == "FIRMS")
                fieldName = "FIRMS||" + TABLEIPCODE2 + "|ds|";

            #region // Ust hesabı var ise

            if (TABLEIPCODE != "")
            {
                tToolBox t = new tToolBox();
                DataRow UstHesapRow = UstHesap_Get(ds_Layout, pos);

                string Prop_View = string.Empty;
                Prop_View = t.Set(row["PROP_VIEWS"].ToString(), "", "");

                if (UstHesapRow != null)
                {
                    string ustItemType = string.Empty;
                    string ustHesapRefId = string.Empty;
                    string ustHesapItemName = string.Empty;
                    string ustItemName = string.Empty;

                    ustItemType = UstHesapRow["LAYOUT_TYPE"].ToString();
                    ustHesapRefId = UstHesapRow["LAYOUT_CODE"].ToString();
                    t.Str_Replace(ref ustHesapRefId, ".", "_");
                    ustItemName = v.lyt_Name + ustHesapRefId;

                    if (t.IsNotNull(UstHesapRow["CMP_NAME"].ToString()))
                        ustItemName = UstHesapRow["CMP_NAME"].ToString();

                    //lParentControlAdd(tForm, panelControl1, ustItemName, ustItemType, row);

                    string[] controls = new string[] { };
                    Control c = t.Find_Control(tForm, ustItemName, "", controls);
                        
                        
                    if (c != null)
                    {
                        v.con_Menu_Prop_Value = Prop_View;
                        mn.Create_Menu_IN_Control(c, TABLEIPCODE, fieldName);
                        v.con_Menu_Prop_Value = string.Empty;
                    }

                }
                else if (subView != null)
                {
                    v.con_Menu_Prop_Value = Prop_View;
                    mn.Create_Menu_IN_Control(subView, TABLEIPCODE, fieldName);
                    v.con_Menu_Prop_Value = string.Empty;
                }
                else
                {
                    mn.Create_Menu_IN_Control(tForm, TABLEIPCODE, fieldName);
                }


            }
            #endregion

            //mn.Create_Menu(null, TABLEIPCODE);
        }

        /// <summary>
        /// tDockPanel, 
        /// tableLayoutPanel, splitContainer, 
        /// groupControl, panelControl
        /// </summary>

        #region tDockPanel Create
        private void lDockPanel_Preparing(Form tForm, DataSet ds_Layout, DataRow row)
        {
            tToolBox t = new tToolBox();

            DockManager manager = null;

            manager = Find_DockManager(tForm);

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
                string TableIPCode = string.Empty;
                string layout_code = string.Empty;
                string fieldName = string.Empty;
                byte IPDataType = 1;

                int i = -1;
                int RefId = 0;
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

                string s1 = "=ROW_PROP_VIEWS:";
                string s2 = (char)34 + "DOCKPANEL" + (char)34 + ": {";
                //"DOCKPANEL": {

                // DockManager e formun üzerinden find ile bulamıyorum ( şimdlik )
                // bu nedenle bir DockManager create etmişken tanımlanmış olan 
                // tüm DockPanel lerin hepsi bir defada üretilmek zorunda kalıyorum

                Int16 panelCount = 0;

                foreach (DataRow row2 in ds_Layout.Tables[0].Rows)
                {
                    LayoutType = row2["LAYOUT_TYPE"].ToString();

                    i++;

                    if (LayoutType == v.lyt_dockPanel)
                    {
                        panelCount++;
                        RefId = t.myInt32(row2["REF_ID"].ToString());
                        caption = row2["LAYOUT_CAPTION"].ToString();
                        TableIPCode = t.Set(row2["TABLEIPCODE"].ToString(), "", "");
                        fieldName = t.Set(row2["FIELD_NAME"].ToString(), "", "");
                        layout_code = t.Set(row2["LAYOUT_CODE"].ToString(), "", "");

                        IPDataType = 1;
                        if (fieldName == "KRITER") IPDataType = 2;

                        width = t.myInt32(row2["CMP_WIDTH"].ToString());
                        height = t.myInt32(row2["CMP_HEIGHT"].ToString());
                        top = t.myInt32(row2["CMP_TOP"].ToString());
                        left = t.myInt32(row2["CMP_LEFT"].ToString());

                        if (width == 0) width = 200;
                        if (height == 0) height = 200;

                        FrontBack = t.myInt16(row2["CMP_FRONT_BACK"].ToString());
                        DockType = t.myInt16(row2["CMP_DOCK"].ToString());
                        if (DockType == 0) DockType = v.dock_Top;

                        Prop_View = t.Set(row2["PROP_VIEWS"].ToString(), "", "");

                        // old
                        if (Prop_View.IndexOf(s1) > -1)
                            tabbed = t.MyProperties_Get(Prop_View, "DP_TABBED:");
                        // json
                        if (Prop_View.IndexOf(s2) > -1)
                            tabbed = t.Find_Properties_Value(Prop_View, "DP_TABBED");

                        UstHesapRow = UstHesap_Get(ds_Layout, i);

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
                            UstPanel.Hide();

                            if (tabbed == "Split")
                            {
                                if (panelCount == 2)
                                    tDockPanel = UstPanel.AddPanel();

                                if (panelCount > 2)
                                {
                                    tDockPanel = UstPanel.ParentPanel.AddPanel();
                                    UstPanel.ParentPanel.Tabbed = false;
                                }
                                //tDockPanel.DockTo(UstPanel, i);
                            }
                            if (tabbed == "Tab")
                            {
                                // Not : İki panelen tab olması için Dock tiplerininde aynı olması gerekiyor
                                if (panelCount == 2)
                                    tDockPanel = UstPanel.AddPanel();

                                if (panelCount > 2)
                                {
                                    tDockPanel = UstPanel.ParentPanel.AddPanel();
                                    UstPanel.ParentPanel.Tabbed = true;
                                }
                            }
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
                        tDockPanel.Options.ShowCloseButton = false;
                        

                        if (DockType == v.dock_Bottom) tDockPanel.Dock = DockingStyle.Bottom;
                        if (DockType == v.dock_Fill) tDockPanel.Dock = DockingStyle.Fill;
                        if (DockType == v.dock_Left) tDockPanel.Dock = DockingStyle.Left;
                        if (DockType == v.dock_Right) tDockPanel.Dock = DockingStyle.Right;
                        if (DockType == v.dock_Top) tDockPanel.Dock = DockingStyle.Top;

                        t.myControl_Size_And_Location(tDockPanel, width, height, left, top);

                        //
                        // InputPanel Create
                        //
                        InputPanel_Preparing(tForm, tDockPanel, TableIPCode, IPDataType);
                    }
                }

                manager.EndUpdate();
                //manager.EndInit();
                //((System.ComponentModel.ISupportInitialize)(manager)).EndInit();
                //tForm.ResumeLayout(false);
            }

        }
        #endregion tDockPanel 

        #region tableLayoutPanel
        private void ltableLayoutPanel_Preparing(Form tForm, Control subView, DataSet ds_Layout, DataRow row, int pos)
        {
            tToolBox t = new tToolBox();

            string CmpName = string.Empty;
            string Prop_View = string.Empty;
            string border_style = string.Empty;
            string TLP_COLUMNS = string.Empty;
            string TLP_ROWS = string.Empty;
            string block = string.Empty;
            string sizetype = string.Empty;
            string sizevalue = string.Empty;
            string layout_code = string.Empty;

            int RefId = 0;
            int width = 0;
            int height = 0;
            int top = 0;
            int left = 0;
            int DockType = 0;
            int FrontBack = 0;
            int count = 0;

            string ustItemType = string.Empty;
            string ustHesapRefId = string.Empty;
            string ustHesapItemName = string.Empty;
            string ustItemName = string.Empty;
            DataRow UstHesapRow = null;

            string s1 = "=ROW_PROP_VIEWS:";
            string s2 = (char)34 + "TLP" + (char)34 + ": {";
            string s3 = (char)39 + "TLP" + (char)39 + ": {";

            //"TLP": {
            List<TLP_COLUMNS> lst_TLP_COLUMNS = null;
            List<TLP_ROWS> lst_TLP_ROWS = null;

            RefId = t.myInt32(row["REF_ID"].ToString());

            width = t.myInt32(row["CMP_WIDTH"].ToString());
            height = t.myInt32(row["CMP_HEIGHT"].ToString());
            top = t.myInt32(row["CMP_TOP"].ToString());
            left = t.myInt32(row["CMP_LEFT"].ToString());

            FrontBack = t.myInt32(row["CMP_FRONT_BACK"].ToString());
            DockType = t.Set(row["CMP_DOCK"].ToString(), v.dock_Fill.ToString(), (int)0);
            Prop_View = t.Set(row["PROP_VIEWS"].ToString(), "", "");

            // old
            if (Prop_View.IndexOf(s1) > -1)
            {
                border_style = t.Set(t.MyProperties_Get(Prop_View, "TLP_BORDER:"), "", "");
                TLP_COLUMNS = t.Find_Properies_Get_FieldBlock(Prop_View, "TLP_COLUMNS");
                TLP_ROWS = t.Find_Properies_Get_FieldBlock(Prop_View, "TLP_ROWS");
            }

            // json
            if (Prop_View.IndexOf(s2) > -1)
            {

                PROP_VIEWS_LAYOUT packet = new PROP_VIEWS_LAYOUT();
                Prop_View = Prop_View.Replace((char)34, (char)39);
                var prop_ = JsonConvert.DeserializeAnonymousType(Prop_View, packet);

                //PROP_VIEWS_LAYOUT prop_ = t.readProp<PROP_VIEWS_LAYOUT>(Prop_View);

                border_style = prop_.TLP.TLP_BORDER.ToString();
                lst_TLP_COLUMNS = prop_.TLP.TLP_COLUMNS;
                lst_TLP_ROWS = prop_.TLP.TLP_ROWS;
            }

            CmpName = t.Set(row["CMP_NAME"].ToString(), "", "");
            layout_code = t.Set(row["LAYOUT_CODE"].ToString(), "", "");

            //
            //
            //
            System.Windows.Forms.TableLayoutPanel tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            // 
            // tableLayoutPanel1
            // 
            if (FrontBack == 2) tableLayoutPanel1.SendToBack();
            else tableLayoutPanel1.BringToFront();

            tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel1.Name = v.lyt_Name + t.Str_Replace(ref layout_code, ".", "_");  //RefId.ToString();
            if (t.IsNotNull(CmpName))
                tableLayoutPanel1.Name = CmpName;
            //tableLayoutPanel1.Size = new System.Drawing.Size(610, 358);

            tableLayoutPanel1.TabIndex = pos;


            #region // Çerçeve çizigileri
            // 0 None
            // 1 single
            // 2 Inset
            // 3 InsetDouble
            // 4 Outset
            // 5 OutsetDouble
            // 6 Outset Partial
            //tableLayoutPanel1.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            if (border_style == "Single") tableLayoutPanel1.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            if (border_style == "Inset") tableLayoutPanel1.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;
            if (border_style == "InsetDouble") tableLayoutPanel1.CellBorderStyle = TableLayoutPanelCellBorderStyle.InsetDouble;
            if (border_style == "Outset") tableLayoutPanel1.CellBorderStyle = TableLayoutPanelCellBorderStyle.Outset;
            if (border_style == "OutsetDouble") tableLayoutPanel1.CellBorderStyle = TableLayoutPanelCellBorderStyle.OutsetDouble;
            if (border_style == "OutsetPartial") tableLayoutPanel1.CellBorderStyle = TableLayoutPanelCellBorderStyle.OutsetPartial;
            #endregion

            // birden fazla col var ise
            #region Columns
            count = 0;

            // old
            if (Prop_View.IndexOf(s1) > -1)
            {
                while (TLP_COLUMNS.IndexOf("=ROWE_") > -1)
                {
                    block = t.Find_Properies_Get_RowBlock(ref TLP_COLUMNS, "TLP_COLUMNS");

                    sizetype = t.MyProperties_Get(block, "TLPC_SIZETYPE:");
                    sizevalue = t.MyProperties_Get(block, "TLPC_SIZEVALUE:");

                    if (t.IsNotNull(sizetype) && t.IsNotNull(sizevalue))
                    {

                        if (sizetype == "Absolute")
                            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, float.Parse(sizevalue)));

                        if (sizetype == "AutoSize")
                            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());

                        if (sizetype == "Percent")
                            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, float.Parse(sizevalue)));

                        count++;

                        //this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
                        //this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
                        //this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
                    }
                }
            } // s1 > -1

            #region // json Cols
            if (Prop_View.IndexOf(s3) > -1)
            {
                foreach (var item in lst_TLP_COLUMNS)
                {
                    sizetype = item.TLPC_SIZETYPE.ToString();
                    sizevalue = item.TLPC_SIZEVALUE.ToString();

                    if ((sizevalue == "") || (sizevalue == "null")) sizevalue = "0";

                    if (t.IsNotNull(sizetype) && t.IsNotNull(sizevalue))
                    {

                        if (sizetype == "Absolute")
                            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, float.Parse(sizevalue)));

                        if (sizetype == "AutoSize")
                            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());

                        if (sizetype == "Percent")
                            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, float.Parse(sizevalue)));

                        count++;

                        //this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
                        //this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
                        //this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
                    }
                }
            }// s2 > -1, json
            #endregion 

            if (count > 0)
            {
                tableLayoutPanel1.ColumnCount = count;
            }
            else
            {
                // eğer hiç column tanımlanmamış ise
                tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
                tableLayoutPanel1.ColumnCount = 1;
            }
            #endregion Columns

            // birden fazla row var ise
            #region Rows
            count = 0;

            // old
            if (Prop_View.IndexOf(s1) > -1)
            {
                while (TLP_ROWS.IndexOf("=ROWE_") > -1)
                {
                    block = t.Find_Properies_Get_RowBlock(ref TLP_ROWS, "TLP_ROWS");

                    sizetype = t.MyProperties_Get(block, "TLPR_SIZETYPE:");
                    sizevalue = t.MyProperties_Get(block, "TLPR_SIZEVALUE:");

                    if (t.IsNotNull(sizetype) && t.IsNotNull(sizevalue))
                    {

                        if (sizetype == "Absolute")
                            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, float.Parse(sizevalue)));

                        if (sizetype == "AutoSize")
                            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());

                        if (sizetype == "Percent")
                            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, float.Parse(sizevalue)));

                        count++;

                        //this.tableLayoutPanel1.RowCount = 3;
                        //this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
                        //this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
                        //this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
                    }
                }
            } // s1 > -1, old


            #region // json Rows
            if (Prop_View.IndexOf(s3) > -1)
            {
                foreach (var item in lst_TLP_ROWS)
                {
                    sizetype = item.TLPR_SIZETYPE.ToString();
                    sizevalue = item.TLPR_SIZEVALUE.ToString();

                    if ((sizevalue == "") || (sizevalue == "null")) sizevalue = "0";

                    if (t.IsNotNull(sizetype) && t.IsNotNull(sizevalue))
                    {
                        if (sizetype == "Absolute")
                            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, float.Parse(sizevalue)));

                        if (sizetype == "AutoSize")
                            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());

                        if (sizetype == "Percent")
                            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, float.Parse(sizevalue)));

                        count++;

                        //this.tableLayoutPanel1.RowCount = 3;
                        //this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
                        //this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
                        //this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
                    }
                }

            } // json, s2 > -1
            #endregion

            if (count > 0)
            {
                tableLayoutPanel1.RowCount = count;
            }
            else
            {
                // hiç tanımlanmamış ise
                tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
                tableLayoutPanel1.RowCount = 1;
            }
            #endregion Rows

            UstHesapRow = UstHesap_Get(ds_Layout, pos);

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

                lParentControlAdd(tForm, tableLayoutPanel1, ustItemName, ustItemType, row);
            }
            else
            {
                if (subView == null)
                    tForm.Controls.Add(tableLayoutPanel1);
                else subView.Controls.Add(tableLayoutPanel1);
            }

            Control pc = tableLayoutPanel1.Parent;
            if (pc != null)
            {
                //tableLayoutPanel1.BackColor = pc.BackColor;
                tableLayoutPanel1.Size = new System.Drawing.Size(pc.Width, pc.Height);
            }
            else
            {
                //tableLayoutPanel1.BackColor = tForm.BackColor;
                tableLayoutPanel1.Size = new System.Drawing.Size(tForm.Width, tForm.Height);
            }

            tableLayoutPanel1.BringToFront();

            #endregion

            //tableLayoutPanel1.ColumnCount = 2;
            //tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            //tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));

            //tableLayoutPanel1.RowCount = 2;
            //tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            //tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));

            t.myControl_Size_And_Location(tableLayoutPanel1, width, height, left, top);

            if (DockType == v.dock_Bottom) tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            if (DockType == v.dock_Fill) tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            if (DockType == v.dock_Left) tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Left;
            if (DockType == v.dock_Right) tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Right;
            if (DockType == v.dock_Top) tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;

        }

        #endregion tableLayoutPanel

        #region splitContainer
        private void lSplitContainer_Preparing(Form tForm, Control subView, DataSet ds_Layout, DataRow row, int pos)
        {
            tToolBox t = new tToolBox();

            string caption = string.Empty;
            string CmpName = string.Empty;
            string Prop_View = string.Empty;
            string orientation = string.Empty;
            string fixedPanel = string.Empty;
            string isSplitterFixed = string.Empty;
            string distance = string.Empty;
            string TableIPCode_Panel1 = string.Empty;
            string TableIPCode_Panel2 = string.Empty;
            string layout_code = string.Empty;
            string fieldName1 = string.Empty;
            string fieldName2 = string.Empty;

            byte IPDataTypes1 = 1;
            byte IPDataTypes2 = 1;

            int RefId = 0;
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

            string s1 = "=ROW_PROP_VIEWS:";
            string s2 = (char)34 + "SPLIT" + (char)34 + ": {";
            //"SPLIT": {

            RefId = t.myInt32(row["REF_ID"].ToString());
            caption = "  " + t.Set(row["LAYOUT_CAPTION"].ToString(), "", "") + "  ";
            width = t.myInt32(row["CMP_WIDTH"].ToString());
            height = t.myInt32(row["CMP_HEIGHT"].ToString());
            top = t.myInt32(row["CMP_TOP"].ToString());
            left = t.myInt32(row["CMP_LEFT"].ToString());

            CmpName = t.Set(row["CMP_NAME"].ToString(), "", "");
            FrontBack = t.myInt32(row["CMP_FRONT_BACK"].ToString());
            DockType = t.Set(row["CMP_DOCK"].ToString(), v.dock_Fill.ToString(), (int)0);
            Prop_View = t.Set(row["PROP_VIEWS"].ToString(), "", "");

            // old
            if (Prop_View.IndexOf(s1) > -1)
            {
                orientation = t.Set(t.MyProperties_Get(Prop_View, "SC_ORIENTATION:"), "", "");
                fixedPanel = t.Set(t.MyProperties_Get(Prop_View, "SC_FIXED:"), "", "");
                isSplitterFixed = t.Set(t.MyProperties_Get(Prop_View, "SC_ISSPLITTERFIXED:"), "false", "");
                distance = t.Set(t.MyProperties_Get(Prop_View, "SC_DISTANCE:"), "250", "");
            }

            if (Prop_View.IndexOf(s2) > -1)
            {
                orientation = t.Set(t.Find_Properties_Value(Prop_View, "SC_ORIENTATION"), "", "");
                fixedPanel = t.Set(t.Find_Properties_Value(Prop_View, "SC_FIXED"), "", "");
                isSplitterFixed = t.Set(t.Find_Properties_Value(Prop_View, "SC_ISSPLITTERFIXED"), "false", "");
                distance = t.Set(t.Find_Properties_Value(Prop_View, "SC_DISTANCE"), "250", "");
            }

            /// kriterlerin default genişliği
            if ((distance == "0") || (distance == "") || (distance == "null")) distance = "250";

            TableIPCode_Panel1 = t.Set(row["TABLEIPCODE"].ToString(), "", "");
            TableIPCode_Panel2 = t.Set(row["TABLEIPCODE2"].ToString(), "", "");
            fieldName1 = t.Set(row["FIELD_NAME"].ToString(), "", "");
            layout_code = t.Set(row["LAYOUT_CODE"].ToString(), "", "");

            if (fieldName1 == "KRITER") IPDataTypes1 = 2;
            if (fieldName2 == "KRITER") IPDataTypes2 = 2;

            //
            //
            //
            System.Windows.Forms.SplitContainer splitContainer1 = new System.Windows.Forms.SplitContainer();

            UstHesapRow = UstHesap_Get(ds_Layout, pos);

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

                lParentControlAdd(tForm, splitContainer1, ustItemName, ustItemType, row);
            }
            else
            {
                if (subView == null)
                    tForm.Controls.Add(splitContainer1);
                else subView.Controls.Add(splitContainer1);
            }

            #endregion

            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).BeginInit();
            splitContainer1.SuspendLayout();
            // 
            // splitContainer1
            // 
            if (FrontBack == 2) splitContainer1.SendToBack();
            else splitContainer1.BringToFront();

            //splitContainer1.BackColor = v.AppearanceTextColor;  //System.Drawing.Color.LightSeaGreen;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Name = v.lyt_Name + t.Str_Replace(ref layout_code, ".", "_");  //RefId.ToString();
            if (t.IsNotNull(CmpName))
                splitContainer1.Name = CmpName;
            //splitContainer1.Size = new System.Drawing.Size(v.Screen_Width, v.Screen_Height);
            Control pc = splitContainer1.Parent;
            if (pc != null)
                splitContainer1.Size = new System.Drawing.Size(pc.Width, pc.Height);
            else splitContainer1.Size = new System.Drawing.Size(tForm.Width, tForm.Height);
            splitContainer1.SplitterDistance = t.myInt32(distance);
            // 
            // splitContainer1.Panel1
            // 
            //if (splitContainer1.Parent != null)
            //    splitContainer1.Panel1.BackColor = splitContainer1.Parent.BackColor;
            //splitContainer1.Panel1.Padding = new System.Windows.Forms.Padding(v.Padding4);
            // 
            // splitContainer1.Panel2
            //
            //if (splitContainer1.Parent != null)
            //    splitContainer1.Panel2.BackColor = splitContainer1.Parent.BackColor;
            //splitContainer1.Panel2.Padding = new System.Windows.Forms.Padding(v.Padding4);

            //splitContainer1.

            t.myControl_Size_And_Location(splitContainer1, width, height, left, top);

            if (DockType == v.dock_Bottom) splitContainer1.Dock = System.Windows.Forms.DockStyle.Bottom;
            if (DockType == v.dock_Fill) splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            if (DockType == v.dock_Left) splitContainer1.Dock = System.Windows.Forms.DockStyle.Left;
            if (DockType == v.dock_Right) splitContainer1.Dock = System.Windows.Forms.DockStyle.Right;
            if (DockType == v.dock_Top) splitContainer1.Dock = System.Windows.Forms.DockStyle.Top;

            if (orientation == "Horizontal")
                splitContainer1.Orientation = Orientation.Horizontal;
            if (fixedPanel == "Panel1")
                splitContainer1.FixedPanel = FixedPanel.Panel1;
            if (fixedPanel == "Panel2")
                splitContainer1.FixedPanel = FixedPanel.Panel2;
            if (isSplitterFixed == "TRUE")
                splitContainer1.IsSplitterFixed = true;

            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).EndInit();
            splitContainer1.ResumeLayout(false);

            DevExpress.XtraEditors.GroupControl groupControl1 = new DevExpress.XtraEditors.GroupControl();
            DevExpress.XtraEditors.GroupControl groupControl2 = new DevExpress.XtraEditors.GroupControl();
            groupControl1.Name = v.lyt_Name + t.Str_Replace(ref layout_code, ".", "_") + "_GroupControl1";
            groupControl2.Name = v.lyt_Name + t.Str_Replace(ref layout_code, ".", "_") + "_GroupControl2";
            groupControl1.Dock = DockStyle.Fill;
            groupControl2.Dock = DockStyle.Fill;

            splitContainer1.Panel1.Controls.Add(groupControl1);
            splitContainer1.Panel2.Controls.Add(groupControl2);

            if (TableIPCode_Panel1 != TableIPCode_Panel2)
            {
                //
                // InputPanel Create
                //
                //InputPanel_Preparing(tForm, splitContainer1.Panel1, TableIPCode_Panel1, IPDataTypes1);
                InputPanel_Preparing(tForm, groupControl1, TableIPCode_Panel1, IPDataTypes1);
                //
                // InputPanel Create
                //
                //InputPanel_Preparing(tForm, splitContainer1.Panel2, TableIPCode_Panel2, IPDataTypes2);
                InputPanel_Preparing(tForm, groupControl2, TableIPCode_Panel2, IPDataTypes2);
            }

            // Eğer ikiside aynı IP ise ( Liste anlamına geliyor )
            // biricisi Kriterler için  ... TableIPCode_Panel1, v.IPdataType_Kriterler
            // ikincisi DataView  için  ... TableIPCode_Panel2, v.IPdataType_DataView
            //
            if (TableIPCode_Panel1 == TableIPCode_Panel2)
            {
                //
                // InputPanel Create
                //
                //InputPanel_Preparing(tForm, splitContainer1.Panel1, TableIPCode_Panel1, v.IPdataType_Kriterler);
                InputPanel_Preparing(tForm, groupControl1, TableIPCode_Panel1, v.IPdataType_Kriterler);

                //
                // InputPanel Create
                //
                //InputPanel_Preparing(tForm, splitContainer1.Panel2, TableIPCode_Panel2, v.IPdataType_DataView);
                InputPanel_Preparing(tForm, groupControl2, TableIPCode_Panel2, v.IPdataType_DataView);

                //
                //InputPanel_Preparing(tForm, splitContainer1.Panel1, TableIPCode_Panel2, v.IPdataType_Kategori);
            }

            if (t.IsNotNull(caption.Trim()))
            {
                groupControl1.Text = caption;
                groupControl2.Text = caption;
            }
            splitContainer1.TabIndex = pos;

        }
        #endregion splitContainer

        #region groupControl
        private void lGroupControl_Preparing(Form tForm, Control subView, DataSet ds_Layout, DataRow row, int pos)
        {
            tToolBox t = new tToolBox();

            string caption = string.Empty;
            string TableIPCode = string.Empty;
            string CmpName = string.Empty;
            string layout_code = string.Empty;
            string fieldName = string.Empty;
            byte IPDataType = 1;
            int RefId = 0;
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

            RefId = t.myInt32(row["REF_ID"].ToString());
            caption = "  " + t.Set(row["LAYOUT_CAPTION"].ToString(), "", "") + "  ";
            TableIPCode = t.Set(row["TABLEIPCODE"].ToString(), "", "");
            fieldName = t.Set(row["FIELD_NAME"].ToString(), "", "");
            layout_code = t.Set(row["LAYOUT_CODE"].ToString(), "", "");

            width = t.myInt32(row["CMP_WIDTH"].ToString());
            height = t.myInt32(row["CMP_HEIGHT"].ToString());
            top = t.myInt32(row["CMP_TOP"].ToString());
            left = t.myInt32(row["CMP_LEFT"].ToString());

            FrontBack = t.myInt32(row["CMP_FRONT_BACK"].ToString());
            DockType = t.Set(row["CMP_DOCK"].ToString(), v.dock_Fill.ToString(), (int)0);
            CmpName = t.Set(row["CMP_NAME"].ToString(), "", "");

            if (fieldName == "KRITER") IPDataType = 2;

            //
            // groupControl1
            //
            DevExpress.XtraEditors.GroupControl groupControl1 = new DevExpress.XtraEditors.GroupControl();
            ((System.ComponentModel.ISupportInitialize)(groupControl1)).BeginInit();

            //groupControl1.GroupStyle = DevExpress.Utils.GroupStyle.

            UstHesapRow = UstHesap_Get(ds_Layout, pos);

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

                lParentControlAdd(tForm, groupControl1, ustItemName, ustItemType, row);
            }
            else
            {
                if (subView == null)
                    tForm.Controls.Add(groupControl1);
                else subView.Controls.Add(groupControl1);
            }
            #endregion

            // 
            // groupControl1
            //
            if (FrontBack == 2) groupControl1.SendToBack();
            else groupControl1.BringToFront();

            groupControl1.Location = new System.Drawing.Point(0, 0);
            groupControl1.Name = v.lyt_Name + t.Str_Replace(ref layout_code, ".", "_"); // RefId.ToString();
            groupControl1.Size = new System.Drawing.Size(203, 435);

            if (t.IsNotNull(CmpName))
                groupControl1.Name = CmpName;

            if (DockType == v.dock_Bottom) groupControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
            if (DockType == v.dock_Fill) groupControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            if (DockType == v.dock_Left) groupControl1.Dock = System.Windows.Forms.DockStyle.Left;
            if (DockType == v.dock_Right) groupControl1.Dock = System.Windows.Forms.DockStyle.Right;
            if (DockType == v.dock_Top) groupControl1.Dock = System.Windows.Forms.DockStyle.Top;

            t.myControl_Size_And_Location(groupControl1, width, height, left, top);

            ((System.ComponentModel.ISupportInitialize)(groupControl1)).EndInit();

            //
            // InputPanel Create
            //
            InputPanel_Preparing(tForm, groupControl1, TableIPCode, IPDataType);

            groupControl1.TabIndex = pos;
            if (t.IsNotNull(caption.Trim()))
                groupControl1.Text = caption;
        }
        #endregion groupControl

        #region panelControl
        private void lPanelControl_Preparing(Form tForm, Control subView, DataSet ds_Layout, DataRow row, int pos)
        {
            tToolBox t = new tToolBox();

            string TableIPCode = string.Empty;
            string CmpName = string.Empty;
            string layout_code = string.Empty;
            string fieldName = string.Empty;
            byte IPDataType = 1;
            int RefId = 0;
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

            RefId = t.myInt32(row["REF_ID"].ToString());
            TableIPCode = t.Set(row["TABLEIPCODE"].ToString(), "", "");
            fieldName = t.Set(row["FIELD_NAME"].ToString(), "", "");
            layout_code = t.Set(row["LAYOUT_CODE"].ToString(), "", "");

            width = t.myInt32(row["CMP_WIDTH"].ToString());
            height = t.myInt32(row["CMP_HEIGHT"].ToString());
            top = t.myInt32(row["CMP_TOP"].ToString());
            left = t.myInt32(row["CMP_LEFT"].ToString());

            FrontBack = t.myInt32(row["CMP_FRONT_BACK"].ToString());
            DockType = t.Set(row["CMP_DOCK"].ToString(), v.dock_Fill.ToString(), (int)0);
            CmpName = t.Set(row["CMP_NAME"].ToString(), "", "");

            if (fieldName == "KRITER") IPDataType = 2;

            //
            //
            //
            DevExpress.XtraEditors.PanelControl panelControl1 = new DevExpress.XtraEditors.PanelControl();
            ((System.ComponentModel.ISupportInitialize)(panelControl1)).BeginInit();

            UstHesapRow = UstHesap_Get(ds_Layout, pos);

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

                lParentControlAdd(tForm, panelControl1, ustItemName, ustItemType, row);
            }
            else
            {
                if (subView == null)
                    tForm.Controls.Add(panelControl1);
                else subView.Controls.Add(panelControl1);
            }

            #endregion

            // 
            // panelControl1
            // 
            if (FrontBack == 2) panelControl1.SendToBack();
            else panelControl1.BringToFront();

            panelControl1.Location = new System.Drawing.Point(0, 0);
            panelControl1.Name = v.lyt_Name + t.Str_Replace(ref layout_code, ".", "_");  //RefId.ToString();
            if (t.IsNotNull(CmpName))
                panelControl1.Name = CmpName;
            panelControl1.Size = new System.Drawing.Size(200, 435);
            //panelControl1.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

            if (DockType == v.dock_Bottom) panelControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
            if (DockType == v.dock_Fill) panelControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            if (DockType == v.dock_Left) panelControl1.Dock = System.Windows.Forms.DockStyle.Left;
            if (DockType == v.dock_Right) panelControl1.Dock = System.Windows.Forms.DockStyle.Right;
            if (DockType == v.dock_Top) panelControl1.Dock = System.Windows.Forms.DockStyle.Top;

            t.myControl_Size_And_Location(panelControl1, width, height, left, top);

            ((System.ComponentModel.ISupportInitialize)(panelControl1)).EndInit();

            //
            // InputPanel Create
            //
            InputPanel_Preparing(tForm, panelControl1, TableIPCode, IPDataType);

            panelControl1.TabIndex = pos;
        }
        #endregion panelControl

        /// <summary>
        /// TabPane, TabNavigationPage
        /// </summary>
        #region navigation TabPane
        private void lTabPane_Preparing(Form tForm, Control subView, DataSet ds_Layout, DataRow row, int pos)
        {
            tToolBox t = new tToolBox();

            int RefId = 0;
            int width = 0;
            int height = 0;
            int top = 0;
            int left = 0;
            int DockType = 0;
            int FrontBack = 0;

            string CmpName = string.Empty;
            string layout_code = string.Empty;
            string ustItemType = string.Empty;
            string ustHesapRefId = string.Empty;
            string ustHesapItemName = string.Empty;
            string ustItemName = string.Empty;
            DataRow UstHesapRow = null;

            RefId = t.myInt32(row["REF_ID"].ToString());
            layout_code = t.Set(row["LAYOUT_CODE"].ToString(), "", "");

            width = t.myInt32(row["CMP_WIDTH"].ToString());
            height = t.myInt32(row["CMP_HEIGHT"].ToString());
            top = t.myInt32(row["CMP_TOP"].ToString());
            left = t.myInt32(row["CMP_LEFT"].ToString());

            FrontBack = t.myInt32(row["CMP_FRONT_BACK"].ToString());
            DockType = t.Set(row["CMP_DOCK"].ToString(), v.dock_Fill.ToString(), (int)0);
            CmpName = t.Set(row["CMP_NAME"].ToString(), "", "");

            //
            // tabPane1
            //
            DevExpress.XtraBars.Navigation.TabPane tabPane1 = new DevExpress.XtraBars.Navigation.TabPane();
            ((System.ComponentModel.ISupportInitialize)(tabPane1)).BeginInit();

            UstHesapRow = UstHesap_Get(ds_Layout, pos);
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

                lParentControlAdd(tForm, tabPane1, ustItemName, ustItemType, row);
            }
            else
            {
                if (subView == null)
                    tForm.Controls.Add(tabPane1);
                else subView.Controls.Add(tabPane1);
            }

            #endregion

            //
            // tabPane1
            // 
            //tabPane1.Controls.Add(this.tabNavigationPage1);
            //tabPane1.Controls.Add(this.tabNavigationPage2);
            if (FrontBack == 2) tabPane1.SendToBack();
            else tabPane1.BringToFront();

            //tabPane1.BackColor = System.Drawing.Color.Transparent;
            tabPane1.Location = new System.Drawing.Point(0, 0);
            tabPane1.Name = v.lyt_Name + t.Str_Replace(ref layout_code, ".", "_");  //RefId.ToString();
            if (t.IsNotNull(CmpName))
                tabPane1.Name = CmpName;
            //tabPane1.Pages.AddRange(new DevExpress.XtraBars.Navigation.NavigationPageBase[] {
            //tabNavigationPage1,
            //tabNavigationPage2});
            tabPane1.RegularSize = new System.Drawing.Size(v.Screen_Width, v.Screen_Height);
            //tabPane1.SelectedPage = this.tabNavigationPage2;
            tabPane1.SelectedPageIndex = 0;
            tabPane1.Size = new System.Drawing.Size(v.Screen_Width, v.Screen_Height);
            tabPane1.TabIndex = pos;
            tabPane1.Text = v.lyt_Name + RefId.ToString();

            //tabPane1. header location sadece bottom var.

            if (DockType == v.dock_Bottom) tabPane1.Dock = System.Windows.Forms.DockStyle.Bottom;
            if (DockType == v.dock_Fill) tabPane1.Dock = System.Windows.Forms.DockStyle.Fill;
            if (DockType == v.dock_Left) tabPane1.Dock = System.Windows.Forms.DockStyle.Left;
            if (DockType == v.dock_Right) tabPane1.Dock = System.Windows.Forms.DockStyle.Right;
            if (DockType == v.dock_Top) tabPane1.Dock = System.Windows.Forms.DockStyle.Top;

            t.myControl_Size_And_Location(tabPane1, width, height, left, top);

            ((System.ComponentModel.ISupportInitialize)(tabPane1)).EndInit();
            tabPane1.ResumeLayout(false);
        }
        #endregion navigation TabPane

        #region navigationTabPage
        private void lTabNavigationPage_Preparing(Form tForm, DataSet ds_Layout, DataRow row, int pos)
        {
            tToolBox t = new tToolBox();

            string CmpName = string.Empty;
            string caption = string.Empty;
            string TableIPCode = string.Empty;
            string fieldName = string.Empty;
            string layout_code = string.Empty;

            byte IPDataType = 1;
            int RefId = 0;

            string ustItemType = string.Empty;
            string ustHesapRefId = string.Empty;
            string ustHesapItemName = string.Empty;
            string ustItemName = string.Empty;
            DataRow UstHesapRow = null;

            RefId = t.myInt32(row["REF_ID"].ToString());
            layout_code = t.Set(row["LAYOUT_CODE"].ToString(), "", "");
            caption = "   " + t.Set(row["LAYOUT_CAPTION"].ToString(), "", "") + "   ";
            TableIPCode = t.Set(row["TABLEIPCODE"].ToString(), "", "");
            fieldName = t.Set(row["FIELD_NAME"].ToString(), "", "");
            CmpName = t.Set(row["CMP_NAME"].ToString(), "", "");

            if (fieldName == "KRITER") IPDataType = 2;

            //
            // tabNavigationPage1
            //
            DevExpress.XtraBars.Navigation.TabNavigationPage tabNavigationPage1 = new DevExpress.XtraBars.Navigation.TabNavigationPage();
            //tabNavigationPage1.BackColor = System.Drawing.Color.GhostWhite;

            UstHesapRow = UstHesap_Get(ds_Layout, pos);

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

                lParentControlAdd(tForm, tabNavigationPage1, ustItemName, ustItemType, row);
            }

            #endregion

            // 
            // tabNavigationPage1
            // 
            tabNavigationPage1.Name = v.lyt_Name + t.Str_Replace(ref layout_code, ".", "_");  //RefId.ToString();
            if ((ustItemName.IndexOf("tabControl_SUBVIEW") > -1) &&
                (t.IsNotNull(TableIPCode)))
            {
                string TableIPCode_NotDot = t.AntiStr_Dot(TableIPCode);
                tabNavigationPage1.AccessibleName = TableIPCode;
                tabNavigationPage1.AccessibleDescription = TableIPCode_NotDot + "|"; // + readValue;
                tabNavigationPage1.Name = "tTabPage_" + TableIPCode_NotDot;// + readValue;
            }
            if (t.IsNotNull(CmpName))
                tabNavigationPage1.Name = CmpName;


            tabNavigationPage1.Size = new System.Drawing.Size(v.Screen_Width, v.Screen_Height);//  400, 200);
                                                                                               //tabNavigationPage1.Padding = new System.Windows.Forms.Padding(v.Padding4);                                                                                      

            //
            // InputPanel Create
            //
            InputPanel_Preparing(tForm, tabNavigationPage1, TableIPCode, IPDataType);

            tabNavigationPage1.Caption = caption;
            tabNavigationPage1.PageText = caption;

            /// fieldName de aslında bakşa bir caption mesajı daha var, o yerleştiriliyor
            /// 
            if (t.IsNotNull(fieldName))
            {
                tabNavigationPage1.PageText = caption;
                tabNavigationPage1.Caption = fieldName;
            }

        }
        #endregion navigationTabPane

        /// <summary>
        /// NavigationPane, NavigationPage
        /// </summary>
        #region NavigationPane
        private void lNavigationPane_Preparing(Form tForm, Control subView, DataSet ds_Layout, DataRow row, int pos)
        {
            tToolBox t = new tToolBox();

            int RefId = 0;
            int width = 0;
            int height = 0;
            int top = 0;
            int left = 0;
            int DockType = 0;
            int FrontBack = 0;

            string CmpName = string.Empty;
            string layout_code = string.Empty;
            string ustItemType = string.Empty;
            string ustHesapRefId = string.Empty;
            string ustHesapItemName = string.Empty;
            string ustItemName = string.Empty;
            DataRow UstHesapRow = null;

            RefId = t.myInt32(row["REF_ID"].ToString());
            layout_code = t.Set(row["LAYOUT_CODE"].ToString(), "", "");

            width = t.myInt32(row["CMP_WIDTH"].ToString());
            height = t.myInt32(row["CMP_HEIGHT"].ToString());
            top = t.myInt32(row["CMP_TOP"].ToString());
            left = t.myInt32(row["CMP_LEFT"].ToString());

            FrontBack = t.myInt32(row["CMP_FRONT_BACK"].ToString());
            DockType = t.Set(row["CMP_DOCK"].ToString(), v.dock_Fill.ToString(), (int)0);
            CmpName = t.Set(row["CMP_NAME"].ToString(), "", "");

            //
            // navigationPane1
            //
            DevExpress.XtraBars.Navigation.NavigationPane navigationPane1 = new DevExpress.XtraBars.Navigation.NavigationPane();
            ((System.ComponentModel.ISupportInitialize)(navigationPane1)).BeginInit();

            UstHesapRow = UstHesap_Get(ds_Layout, pos);
            //navigationPane1.BackColor = System.Drawing.Color.AntiqueWhite;

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

                lParentControlAdd(tForm, navigationPane1, ustItemName, ustItemType, row);
            }
            else
            {
                if (subView == null)
                    tForm.Controls.Add(navigationPane1);
                else subView.Controls.Add(navigationPane1);
            }

            #endregion

            //
            // navigationPane1
            // 
            //navigationPane1.Controls.Add(this.navigationPage1);
            //navigationPane1.Controls.Add(this.navigationPage2);
            if (FrontBack == 2) navigationPane1.SendToBack();
            else navigationPane1.BringToFront();

            navigationPane1.Location = new System.Drawing.Point(0, 0);
            navigationPane1.Name = v.lyt_Name + t.Str_Replace(ref layout_code, ".", "_");  //RefId.ToString();
            if (t.IsNotNull(CmpName))
                navigationPane1.Name = CmpName;
            //navigationPane1.Pages.AddRange(new DevExpress.XtraBars.Navigation.NavigationPageBase[] {
            //navigationPage1,
            //navigationPage1});
            navigationPane1.RegularSize = new System.Drawing.Size(v.Screen_Width, v.Screen_Height); //300, 200);
            //navigationPane1.SelectedPage = this.navigationPage1;
            navigationPane1.SelectedPageIndex = 0;
            navigationPane1.Size = new System.Drawing.Size(v.Screen_Width, v.Screen_Height); //300, 200);
            navigationPane1.TabIndex = pos;
            navigationPane1.Text = "navigationPane1";

            if (DockType == v.dock_Bottom) navigationPane1.Dock = System.Windows.Forms.DockStyle.Bottom;
            if (DockType == v.dock_Fill) navigationPane1.Dock = System.Windows.Forms.DockStyle.Fill;
            if (DockType == v.dock_Left) navigationPane1.Dock = System.Windows.Forms.DockStyle.Left;
            if (DockType == v.dock_Right) navigationPane1.Dock = System.Windows.Forms.DockStyle.Right;
            if (DockType == v.dock_Top) navigationPane1.Dock = System.Windows.Forms.DockStyle.Top;

            t.myControl_Size_And_Location(navigationPane1, width, height, left, top);

            ((System.ComponentModel.ISupportInitialize)(navigationPane1)).EndInit();
            navigationPane1.ResumeLayout(false);
        }
        #endregion navigation TabPane

        #region NavigationPage
        private void lNavigationPage_Preparing(Form tForm, DataSet ds_Layout, DataRow row, int pos)
        {
            tToolBox t = new tToolBox();

            string CmpName = string.Empty;
            string caption = string.Empty;
            string TableIPCode = string.Empty;
            string fieldName = string.Empty;
            string layout_code = string.Empty;

            byte IPDataType = 1;
            int RefId = 0;

            string ustItemType = string.Empty;
            string ustHesapRefId = string.Empty;
            string ustHesapItemName = string.Empty;
            string ustItemName = string.Empty;
            DataRow UstHesapRow = null;

            RefId = t.myInt32(row["REF_ID"].ToString());
            layout_code = t.Set(row["LAYOUT_CODE"].ToString(), "", "");

            caption = "   " + t.Set(row["LAYOUT_CAPTION"].ToString(), "", "") + "   ";
            TableIPCode = t.Set(row["TABLEIPCODE"].ToString(), "", "");
            fieldName = t.Set(row["FIELD_NAME"].ToString(), "", "");
            CmpName = t.Set(row["CMP_NAME"].ToString(), "", "");

            if (fieldName == "KRITER") IPDataType = 2;

            //
            // navigationPage1
            //
            DevExpress.XtraBars.Navigation.NavigationPage navigationPage1 = new DevExpress.XtraBars.Navigation.NavigationPage();
            //navigationPage1.BackColor = System.Drawing.Color.AliceBlue;
            UstHesapRow = UstHesap_Get(ds_Layout, pos);

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

                lParentControlAdd(tForm, navigationPage1, ustItemName, ustItemType, row);
            }

            #endregion

            // 
            // navigationPage1
            // 

            navigationPage1.Name = v.lyt_Name + t.Str_Replace(ref layout_code, ".", "_");  //RefId.ToString();

            if ((ustItemName.IndexOf("tabControl_SUBVIEW") > -1) &&
                (t.IsNotNull(TableIPCode)))
            {
                string TableIPCode_NotDot = t.AntiStr_Dot(TableIPCode);
                navigationPage1.AccessibleName = TableIPCode;
                navigationPage1.AccessibleDescription = TableIPCode_NotDot + "|"; // + readValue;
                navigationPage1.Name = "tTabPage_" + TableIPCode_NotDot;// + readValue;
            }

            if (t.IsNotNull(CmpName))
                navigationPage1.Name = CmpName;

            navigationPage1.Size = new System.Drawing.Size(v.Screen_Width, v.Screen_Height);  //180, 200);
            navigationPage1.Properties.ShowCollapseButton = DevExpress.Utils.DefaultBoolean.False;
            navigationPage1.Properties.ShowExpandButton = DevExpress.Utils.DefaultBoolean.False;
            //navigationPage1.Padding = new System.Windows.Forms.Padding(v.Padding4);

            //
            // InputPanel Create
            //
            InputPanel_Preparing(tForm, navigationPage1, TableIPCode, IPDataType);

            navigationPage1.PageText = caption;

            /// fieldName de aslında bakşa bir caption mesajı daha var, o yerleştiriliyor
            /// 
            if (t.IsNotNull(fieldName))
            {
                navigationPage1.Caption = caption;
                navigationPage1.PageText = fieldName;
            }
        }
        #endregion navigationTabPane

        /// <summary>
        /// XtraTabControl, XtraTabPage
        /// </summary>
        #region xtraTabControl
        private void lTabControl_Preparing(Form tForm, Control subView, DataSet ds_Layout, DataRow row, int pos)
        {
            tToolBox t = new tToolBox();

            int RefId = 0;
            int width = 0;
            int height = 0;
            int top = 0;
            int left = 0;
            int DockType = 0;
            int FrontBack = 0;

            string Prop_View = string.Empty;
            string CmpName = string.Empty;
            string tabHeaderShow = "FALSE";
            string layout_code = string.Empty;
            string ustItemType = string.Empty;
            string ustHesapRefId = string.Empty;
            string ustHesapItemName = string.Empty;
            string ustItemName = string.Empty;
            DataRow UstHesapRow = null;

            RefId = t.myInt32(row["REF_ID"].ToString());
            layout_code = t.Set(row["LAYOUT_CODE"].ToString(), "", "");
            width = t.myInt32(row["CMP_WIDTH"].ToString());
            height = t.myInt32(row["CMP_HEIGHT"].ToString());
            top = t.myInt32(row["CMP_TOP"].ToString());
            left = t.myInt32(row["CMP_LEFT"].ToString());

            FrontBack = t.myInt32(row["CMP_FRONT_BACK"].ToString());
            DockType = t.Set(row["CMP_DOCK"].ToString(), v.dock_Fill.ToString(), (int)0);
            Prop_View = t.Set(row["PROP_VIEWS"].ToString(), "", "");
            CmpName = t.Set(row["CMP_NAME"].ToString(), "", "");
                         
            if (t.IsNotNull(Prop_View))
            {
                try
                {
                    tabHeaderShow = t.Set(t.Find_Properties_Value(Prop_View, "TABHEADER"), "TRUE", "");
                }
                catch (Exception)
                {
                }
            }

            //
            // xtraTabControl1
            //
            DevExpress.XtraTab.XtraTabControl xtraTabControl1 = new DevExpress.XtraTab.XtraTabControl();
            ((System.ComponentModel.ISupportInitialize)(xtraTabControl1)).BeginInit();

            UstHesapRow = UstHesap_Get(ds_Layout, pos);
            //xtraTabControl1.BackColor = System.Drawing.Color.Blue;

            #region // Ust hesabı var ise

            if (UstHesapRow != null)
            {
                ustItemType = UstHesapRow["LAYOUT_TYPE"].ToString();
                //ustHesapRefId = UstHesapRow["REF_ID"].ToString();
                ustHesapRefId = UstHesapRow["LAYOUT_CODE"].ToString();
                t.Str_Replace(ref ustHesapRefId, ".", "_");
                ustItemName = v.lyt_Name + ustHesapRefId;

                lParentControlAdd(tForm, xtraTabControl1, ustItemName, ustItemType, row);
            }
            else
            {
                if (subView == null)
                    tForm.Controls.Add(xtraTabControl1);
                else subView.Controls.Add(xtraTabControl1);
            }


            #endregion


            // 
            // xtraTabControl1
            // 
            if (FrontBack == 2) xtraTabControl1.SendToBack();
            else xtraTabControl1.BringToFront();

            xtraTabControl1.Location = new System.Drawing.Point(12, 12);
            xtraTabControl1.Name = v.lyt_Name + t.Str_Replace(ref layout_code, ".", "_");  //RefId.ToString();
            if (t.IsNotNull(CmpName))
                xtraTabControl1.Name = CmpName;
            xtraTabControl1.SelectedTabPageIndex = 0;
            xtraTabControl1.Size = new System.Drawing.Size(v.Screen_Width, v.Screen_Height); //300, 300);
            xtraTabControl1.TabIndex = pos;

            xtraTabControl1.HeaderLocation = DevExpress.XtraTab.TabHeaderLocation.Left;
            if (tabHeaderShow == "FALSE")
                xtraTabControl1.ShowTabHeader = DevExpress.Utils.DefaultBoolean.False;

            //xtraTabControl1.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            //xtraTabPage1,
            //xtraTabPage2});

            if (DockType == v.dock_Bottom) xtraTabControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
            if (DockType == v.dock_Fill) xtraTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            if (DockType == v.dock_Left) xtraTabControl1.Dock = System.Windows.Forms.DockStyle.Left;
            if (DockType == v.dock_Right) xtraTabControl1.Dock = System.Windows.Forms.DockStyle.Right;
            if (DockType == v.dock_Top) xtraTabControl1.Dock = System.Windows.Forms.DockStyle.Top;

            t.myControl_Size_And_Location(xtraTabControl1, width, height, left, top);

            ((System.ComponentModel.ISupportInitialize)(xtraTabControl1)).EndInit();
            xtraTabControl1.ResumeLayout(false);
        }
        #endregion xtraTabControl1

        #region XtraTabPage
        private void lTabPage_Preparing(Form tForm, DataSet ds_Layout, DataRow row, int pos)
        {
            tToolBox t = new tToolBox();

            string caption = string.Empty;
            string TableIPCode = string.Empty;
            string layout_code = string.Empty;
            string fieldName = string.Empty;

            byte IPDataType = 1;
            int RefId = 0;

            string CmpName = string.Empty;
            string ustItemType = string.Empty;
            string ustHesapRefId = string.Empty;
            string ustHesapItemName = string.Empty;
            string ustItemName = string.Empty;

            DataRow UstHesapRow = null;

            RefId = t.myInt32(row["REF_ID"].ToString());
            layout_code = t.Set(row["LAYOUT_CODE"].ToString(), "", "");
            caption = "   " + t.Set(row["LAYOUT_CAPTION"].ToString(), "", "") + "   ";
            TableIPCode = t.Set(row["TABLEIPCODE"].ToString(), "", "");
            fieldName = t.Set(row["FIELD_NAME"].ToString(), "", "");
            CmpName = row["CMP_NAME"].ToString();

            if (fieldName == "KRITER") IPDataType = 2;

            //
            // xtraTabPage1
            //
            DevExpress.XtraTab.XtraTabPage xtraTabPage1 = new DevExpress.XtraTab.XtraTabPage();
            //xtraTabPage1.BackColor = System.Drawing.Color.OrangeRed;

            UstHesapRow = UstHesap_Get(ds_Layout, pos);

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

                lParentControlAdd(tForm, xtraTabPage1, ustItemName, ustItemType, row);
            }

            #endregion

            // 
            // xtraTabPage1
            // 
            xtraTabPage1.Text = "   " + caption + "   ";

            xtraTabPage1.Name = v.lyt_Name + t.Str_Replace(ref layout_code, ".", "_");  //RefId.ToString();
            if ((ustItemName.IndexOf("tabControl_SUBVIEW") > -1) &&
                (t.IsNotNull(TableIPCode)))
            {
                string TableIPCode_NotDot = t.AntiStr_Dot(TableIPCode);
                xtraTabPage1.AccessibleName = TableIPCode;
                xtraTabPage1.AccessibleDescription = TableIPCode_NotDot + "|"; // + readValue;
                xtraTabPage1.Name = "tTabPage_" + TableIPCode_NotDot;// + readValue;
            }
            if (t.IsNotNull(CmpName))
                xtraTabPage1.Name = CmpName;

            //xtraTabPage1.Text = xtraTabPage1.Name;

            xtraTabPage1.Size = new System.Drawing.Size(v.Screen_Width, v.Screen_Height); //400, 200);
            xtraTabPage1.Padding = new System.Windows.Forms.Padding(v.Padding4);
            //xtraTabPage1.BackColor = System.Drawing.Color.BlueViolet;
            
            //
            // InputPanel Create
            //
            InputPanel_Preparing(tForm, xtraTabPage1, TableIPCode, IPDataType);

        }
        #endregion XtraTabPage

        /// <summary>
        /// BackstageViewControl, BackstageViewTabItem, 
        /// BackstageViewItemSeparator, BackstageViewButtonItem
        /// </summary>
        #region BackstageViewControl

        private void lBackstageViewControl_Preparing(Form tForm, Control subView, DataSet ds_Layout, DataRow row, int pos)
        {
            tToolBox t = new tToolBox();

            string caption = string.Empty;
            string CmpName = string.Empty;
            string layout_code = string.Empty;

            int RefId = 0;
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

            RefId = t.myInt32(row["REF_ID"].ToString());
            layout_code = t.Set(row["LAYOUT_CODE"].ToString(), "", "");

            width = t.myInt32(row["CMP_WIDTH"].ToString());
            height = t.myInt32(row["CMP_HEIGHT"].ToString());
            top = t.myInt32(row["CMP_TOP"].ToString());
            left = t.myInt32(row["CMP_LEFT"].ToString());

            FrontBack = t.myInt32(row["CMP_FRONT_BACK"].ToString());
            DockType = t.Set(row["CMP_DOCK"].ToString(), v.dock_Fill.ToString(), (int)0);
            CmpName = t.Set(row["CMP_NAME"].ToString(), "", "");
            caption = t.Set(row["LAYOUT_CAPTION"].ToString(), "", "");

            //
            // backstageViewControl1
            // 
            DevExpress.XtraBars.Ribbon.BackstageViewControl backstageViewControl1 = new DevExpress.XtraBars.Ribbon.BackstageViewControl();

            ((System.ComponentModel.ISupportInitialize)(backstageViewControl1)).BeginInit();
            backstageViewControl1.SuspendLayout();

            UstHesapRow = UstHesap_Get(ds_Layout, pos);
            //xtraTabControl1.BackColor = System.Drawing.Color.Blue;

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

                lParentControlAdd(tForm, backstageViewControl1, ustItemName, ustItemType, row);
            }
            else
            {
                if (subView == null)
                    tForm.Controls.Add(backstageViewControl1);
                else subView.Controls.Add(backstageViewControl1);
            }


            #endregion
            //backstageViewControl1.Items.
            // 
            // backstageViewControl1
            //
            if (FrontBack == 2) backstageViewControl1.SendToBack();
            else backstageViewControl1.BringToFront();

            //backstageViewControl1.BackColor = System.Drawing.Color.Gold;
            //backstageViewControl1.ColorScheme = DevExpress.XtraBars.Ribbon.RibbonControlColorScheme.Yellow;
            //backstageViewControl1.Items.Add(this.backstageViewTabItem1);
            //backstageViewControl1.Items.Add(this.backstageViewButtonItem1);
            //backstageViewControl1.Items.Add(this.backstageViewItemSeparator1);
            backstageViewControl1.Location = new System.Drawing.Point(0, 0);
            backstageViewControl1.Name = v.lyt_Name + t.Str_Replace(ref layout_code, ".", "_");  //RefId.ToString();
            if (t.IsNotNull(CmpName))
                backstageViewControl1.Name = CmpName;
            backstageViewControl1.Size = new System.Drawing.Size(800, 400);
            backstageViewControl1.TabIndex = pos;
            backstageViewControl1.Text = "backstageViewControl1";
            //backstageViewControl1.PaintStyle = BackstageViewPaintStyle.Flat;
            //backstageViewControl1.SelectedTabIndex = 0; << lBackstageViewTabItem_Preparing içinde ayarlanıyor

            if (DockType == v.dock_Bottom) backstageViewControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
            if (DockType == v.dock_Fill) backstageViewControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            if (DockType == v.dock_Left) backstageViewControl1.Dock = System.Windows.Forms.DockStyle.Left;
            if (DockType == v.dock_Right) backstageViewControl1.Dock = System.Windows.Forms.DockStyle.Right;
            if (DockType == v.dock_Top) backstageViewControl1.Dock = System.Windows.Forms.DockStyle.Top;

            t.myControl_Size_And_Location(backstageViewControl1, width, height, left, top);

            ((System.ComponentModel.ISupportInitialize)(backstageViewControl1)).EndInit();
            backstageViewControl1.ResumeLayout(false);
        }

        private void lBackstageViewTabItem_Preparing(Form tForm, DataSet ds_Layout, DataRow row, int pos)
        {
            tToolBox t = new tToolBox();

            int RefId = 0;
            string CmpName = string.Empty;
            string caption = string.Empty;
            string TableIPCode = string.Empty;
            string layout_code = string.Empty;
            string fieldName = string.Empty;
            byte IPDataType = 1;

            string ustItemType = string.Empty;
            string ustHesapRefId = string.Empty;
            string ustHesapItemName = string.Empty;
            string ustItemName = string.Empty;
            DataRow UstHesapRow = null;

            RefId = t.myInt32(row["REF_ID"].ToString());
            layout_code = t.Set(row["LAYOUT_CODE"].ToString(), "", "");
            caption = t.Set(row["LAYOUT_CAPTION"].ToString(), "", "");
            TableIPCode = t.Set(row["TABLEIPCODE"].ToString(), "", "");
            fieldName = t.Set(row["FIELD_NAME"].ToString(), "", "");

            CmpName = t.Set(row["CMP_NAME"].ToString(), "", "");

            UstHesapRow = UstHesap_Get(ds_Layout, pos);

            if (fieldName == "KRITER") IPDataType = 2;

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

                DevExpress.XtraBars.Ribbon.BackstageViewClientControl backstageViewClientControl1 = new DevExpress.XtraBars.Ribbon.BackstageViewClientControl();
                DevExpress.XtraBars.Ribbon.BackstageViewTabItem backstageViewTabItem1 = new DevExpress.XtraBars.Ribbon.BackstageViewTabItem();

                //
                // find & get = backstageViewControl1
                //
                Control c = lParentControlAdd(tForm, null, ustItemName, ustItemType, row);

                if (c == null) return;

                //
                // backstageViewControl1
                //
                ((DevExpress.XtraBars.Ribbon.BackstageViewControl)c).Controls.Add(backstageViewClientControl1);
                ((DevExpress.XtraBars.Ribbon.BackstageViewControl)c).Items.Add(backstageViewTabItem1);
                ((DevExpress.XtraBars.Ribbon.BackstageViewControl)c).SelectedTabIndex = 0;
                //backstageViewControl1.Controls.Add(this.backstageViewClientControl1);
                //backstageViewControl1.Items.Add(this.backstageViewTabItem1);

                // 
                // backstageViewClientControl1
                // 
                //backstageViewClientControl1.Controls.Add(this.panelControl1);
                backstageViewClientControl1.AccessibleName = TableIPCode;// + MultiPageID;
                //backstageViewClientControl1.BackColor = System.Drawing.Color.GhostWhite;
                backstageViewClientControl1.Location = new System.Drawing.Point(188, 0);

                backstageViewClientControl1.Name = v.lyt_Name + t.Str_Replace(ref layout_code, ".", "_");  //RefId.ToString();
                if ((ustItemName.IndexOf("tabControl_SUBVIEW") > -1) &&
                    (t.IsNotNull(TableIPCode)))
                {
                    string TableIPCode_NotDot = t.AntiStr_Dot(TableIPCode);
                    backstageViewClientControl1.AccessibleName = TableIPCode;
                    backstageViewClientControl1.AccessibleDescription = TableIPCode_NotDot + "|"; // + readValue;
                    backstageViewClientControl1.Name = "tTabPage_" + TableIPCode_NotDot;// + readValue;
                }
                if (t.IsNotNull(CmpName))
                    backstageViewClientControl1.Name = CmpName;
                //backstageViewClientControl1.Size = new System.Drawing.Size(260, 393);
                if (c != null)
                    backstageViewClientControl1.Size = new System.Drawing.Size(c.Width, c.Height);
                else backstageViewClientControl1.Size = new System.Drawing.Size(tForm.Width, tForm.Height);
                //backstageViewClientControl1.Size = new System.Drawing.Size(tForm.Width, tForm.Height);
                backstageViewClientControl1.TabIndex = pos;
                backstageViewClientControl1.Padding = new System.Windows.Forms.Padding(v.Padding4);
                // 
                // backstageViewTabItem1
                //
                //backstageViewTabItem1.Appearance.BackColor = System.Drawing.Color.DarkTurquoise;
                backstageViewTabItem1.Caption = caption;
                backstageViewTabItem1.ContentControl = backstageViewClientControl1;
                backstageViewTabItem1.Name = "backstageViewTabItem_" + RefId.ToString();

                //
                // InputPanel Create
                //
                InputPanel_Preparing(tForm, backstageViewClientControl1, TableIPCode, IPDataType);
            }

            #endregion

        }

        private void lBackstageViewItemSeparator_Preparing(Form tForm, DataSet ds_Layout, DataRow row, int pos)
        {
            tToolBox t = new tToolBox();

            string CmpName = string.Empty;
            string caption = string.Empty;

            int RefId = 0;

            string ustItemType = string.Empty;
            string ustHesapRefId = string.Empty;
            string ustHesapItemName = string.Empty;
            string ustItemName = string.Empty;
            DataRow UstHesapRow = null;

            RefId = t.myInt32(row["REF_ID"].ToString());
            caption = t.Set(row["LAYOUT_CAPTION"].ToString(), "", "");
            CmpName = t.Set(row["CMP_NAME"].ToString(), "", "");

            UstHesapRow = UstHesap_Get(ds_Layout, pos);

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

                DevExpress.XtraBars.Ribbon.BackstageViewItemSeparator backstageViewItemSeparator1 = new DevExpress.XtraBars.Ribbon.BackstageViewItemSeparator();

                //
                // find & get = backstageViewControl1
                //
                Control c = lParentControlAdd(tForm, null, ustItemName, ustItemType, row);

                if (c == null) return;

                //
                // backstageViewControl1
                //
                ((DevExpress.XtraBars.Ribbon.BackstageViewControl)c).Items.Add(backstageViewItemSeparator1);
                //backstageViewControl1.Items.Add(this.backstageViewItemSeparator1);

                // 
                // backstageViewItemSeparator1
                // 
                backstageViewItemSeparator1.Name = "backstageViewItemSeparator_" + RefId.ToString();

            }

            #endregion

        }

        private void lBackstageViewButtonItem_Preparing(Form tForm, DataSet ds_Layout, DataRow row, int pos)
        {
            tToolBox t = new tToolBox();

            string CmpName = string.Empty;
            string caption = string.Empty;

            int RefId = 0;

            string ustItemType = string.Empty;
            string ustHesapRefId = string.Empty;
            string ustHesapItemName = string.Empty;
            string ustItemName = string.Empty;
            DataRow UstHesapRow = null;

            RefId = t.myInt32(row["REF_ID"].ToString());
            caption = t.Set(row["LAYOUT_CAPTION"].ToString(), "", "");
            CmpName = t.Set(row["CMP_NAME"].ToString(), "", "");

            UstHesapRow = UstHesap_Get(ds_Layout, pos);

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

                DevExpress.XtraBars.Ribbon.BackstageViewButtonItem backstageViewButtonItem1 = new DevExpress.XtraBars.Ribbon.BackstageViewButtonItem();

                //
                // find & get = backstageViewControl1
                //
                Control c = lParentControlAdd(tForm, null, ustItemName, ustItemType, row);

                if (c == null) return;

                //
                // backstageViewControl1
                //
                ((DevExpress.XtraBars.Ribbon.BackstageViewControl)c).Items.Add(backstageViewButtonItem1);
                //backstageViewControl1.Items.Add(this.backstageViewButtonItem1);

                // 
                // backstageViewButtonItem1
                //
                backstageViewButtonItem1.Caption = caption;
                backstageViewButtonItem1.Name = "backstageViewButtonItem_" + RefId.ToString();
            }
            #endregion
        }

        #endregion BackstageViewControl

        /// <summary>
        /// documentManager
        /// </summary>
        #region documentManager

        private void lDocumentManager_Preparing(Form tForm, Control subView, DataSet ds_Layout, DataRow row, int pos)
        {
            tToolBox t = new tToolBox();

            string CmpName = string.Empty;

            int RefId = 0;

            RefId = t.myInt32(row["REF_ID"].ToString());

            //FrontBack = t.myInt32(row["CMP_FRONT_BACK"].ToString());
            //DockType = t.Set(row["CMP_DOCK"].ToString(), v.dock_Fill.ToString(), (int)0);
            CmpName = t.Set(row["CMP_NAME"].ToString(), "", "");

            System.ComponentModel.IContainer components = null;
            components = new System.ComponentModel.Container();

            DevExpress.XtraBars.Docking2010.Views.Tabbed.DockingContainer dockingContainer1 =
                new DevExpress.XtraBars.Docking2010.Views.Tabbed.DockingContainer();

            DevExpress.XtraBars.Docking2010.Views.Tabbed.DockingContainer dockingContainer2 =
                new DevExpress.XtraBars.Docking2010.Views.Tabbed.DockingContainer();


            DevExpress.XtraBars.Docking2010.DocumentManager documentManager1 =
                new DevExpress.XtraBars.Docking2010.DocumentManager(components);

            DevExpress.XtraBars.Docking2010.Views.Tabbed.TabbedView tabbedView1 =
                new DevExpress.XtraBars.Docking2010.Views.Tabbed.TabbedView(components);

            DevExpress.XtraBars.Docking2010.Views.Tabbed.DocumentGroup documentGroup1 =
                new DevExpress.XtraBars.Docking2010.Views.Tabbed.DocumentGroup(components);
            DevExpress.XtraBars.Docking2010.Views.Tabbed.DocumentGroup documentGroup2 =
                new DevExpress.XtraBars.Docking2010.Views.Tabbed.DocumentGroup(components);

            DevExpress.XtraBars.Docking2010.Views.Tabbed.Document document1 =
                new DevExpress.XtraBars.Docking2010.Views.Tabbed.Document(components);

            /*
            DevExpress.XtraBars.Docking2010.Views.Widget.WidgetView widgetView1 = 
                new DevExpress.XtraBars.Docking2010.Views.Widget.WidgetView(components);
            DevExpress.XtraBars.Docking2010.Views.NativeMdi.NativeMdiView nativeMdiView1 = 
                new DevExpress.XtraBars.Docking2010.Views.NativeMdi.NativeMdiView(components);
            DevExpress.XtraBars.Docking2010.Views.WindowsUI.WindowsUIView windowsUIView1 = 
                new DevExpress.XtraBars.Docking2010.Views.WindowsUI.WindowsUIView(components);
            */
            ((System.ComponentModel.ISupportInitialize)(documentManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(tabbedView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(documentGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(document1)).BeginInit();

            ((System.ComponentModel.ISupportInitialize)(documentGroup2)).BeginInit();

            //((System.ComponentModel.ISupportInitialize)(widgetView1)).BeginInit();
            //((System.ComponentModel.ISupportInitialize)(nativeMdiView1)).BeginInit();
            //((System.ComponentModel.ISupportInitialize)(windowsUIView1)).BeginInit();

            tForm.SuspendLayout();

            //documentManager2.View.DocumentProperties.UseFormIconAsDocumentImage = false;
            //documentManager2.View.UseDocumentSelector = DevExpress.Utils.DefaultBoolean.True;
            //tabbedView1.FloatingDocumentContainer = FloatingDocumentContainer.DocumentsHost;


            // 
            // documentManager1
            // 
            documentManager1.MdiParent = tForm;
            documentManager1.ContainerControl = tForm;
            //documentManager1.MenuManager = this.ribbonControl1;
            documentManager1.View = tabbedView1;
            documentManager1.ViewCollection.AddRange(new DevExpress.XtraBars.Docking2010.Views.BaseView[] {
            tabbedView1}); /*,
            widgetView1,
            nativeMdiView1,
            windowsUIView1}); */
            // 
            // tabbedView1
            // 
            //tabbedView1.AppearancePage.Header.BackColor = System.Drawing.Color.Red;
            tabbedView1.DocumentGroups.AddRange(new DevExpress.XtraBars.Docking2010.Views.Tabbed.DocumentGroup[] {
             documentGroup1, documentGroup2});
            tabbedView1.Documents.AddRange(new DevExpress.XtraBars.Docking2010.Views.BaseDocument[] {
             document1});
            tabbedView1.LoadingIndicatorProperties.Caption = "sdfsadf";
            tabbedView1.RootContainer.Element = null;
            dockingContainer1.Element = documentGroup1;
            dockingContainer2.Element = documentGroup2;
            tabbedView1.RootContainer.Nodes.AddRange(new DevExpress.XtraBars.Docking2010.Views.Tabbed.DockingContainer[] {
             dockingContainer1,
             dockingContainer2});

            tabbedView1.Orientation = Orientation.Vertical;
            // TabbedView.EnableFreeLayoutMode 
            tabbedView1.EnableFreeLayoutMode = DevExpress.Utils.DefaultBoolean.True;

            // 
            // documentGroup1
            // 
            //--documentGroup1.Items.AddRange(new DevExpress.XtraBars.Docking2010.Views.Tabbed.Document[] {
            //-- document1});
            // 
            // document1
            // 
            document1.Caption = "document1";
            document1.ControlName = "document1";

            //((System.ComponentModel.ISupportInitialize)(windowsUIView1)).EndInit();
            //((System.ComponentModel.ISupportInitialize)(nativeMdiView1)).EndInit();
            //((System.ComponentModel.ISupportInitialize)(widgetView1)).EndInit();

            ((System.ComponentModel.ISupportInitialize)(documentGroup2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(document1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(documentGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(tabbedView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(documentManager1)).EndInit();

            components.Add(documentManager1);

            //tForm.  //(components);

            /*
            TabbedView view = new TabbedView();
            documentManager.View = view;
            documentManager.ViewCollection.Add(view);
            documentManager.ContainerControl = ownerControl;
            view.AddDocument(new UserControl() { Text = "Document1" });
            */

            tForm.ResumeLayout(false);
            tForm.PerformLayout();

        }

        #endregion

        /// <summary>
        /// Data Wizard
        /// </summary>
        #region Data Wizard

        private void lDataWizard_Preparing(Form tForm, DataSet ds_Layout, DataRow row, int pos)
        {
            tToolBox t = new tToolBox();

            #region Tanımlar
            int RefId = 0;
            int width = 0;
            int height = 0;
            int top = 0;
            int left = 0;
            int DockType = 0;
            int FrontBack = 0;
            int pageCount = 0;
            bool singleIPCode = false;
            byte IPDataType = 1;

            string layoutCode = string.Empty;
            string caption = string.Empty;
            string TableIPCode = string.Empty;
            string fieldName = string.Empty;
            string CmpName = string.Empty;
            string ustItemType = string.Empty;
            string ustHesapRefId = string.Empty;
            string ustHesapItemName = string.Empty;
            string ustItemName = string.Empty;
            string tabControlName = string.Empty;

            DataRow UstHesapRow = null;

            RefId = t.myInt32(row["REF_ID"].ToString());
            layoutCode = t.Set(row["LAYOUT_CODE"].ToString(), "", "");
            caption = t.Set(row["LAYOUT_CAPTION"].ToString(), "", "");
            TableIPCode = t.Set(row["TABLEIPCODE"].ToString(), "", "");
            fieldName = t.Set(row["FIELD_NAME"].ToString(), "", "");

            width = t.myInt32(row["CMP_WIDTH"].ToString());
            height = t.myInt32(row["CMP_HEIGHT"].ToString());
            top = t.myInt32(row["CMP_TOP"].ToString());
            left = t.myInt32(row["CMP_LEFT"].ToString());

            FrontBack = t.myInt32(row["CMP_FRONT_BACK"].ToString());
            DockType = t.Set(row["CMP_DOCK"].ToString(), v.dock_Fill.ToString(), (int)0);
            CmpName = t.Set(row["CMP_NAME"].ToString(), "", "");

            if (fieldName == "KRITER") IPDataType = 2;

            /// single page : sadece bir tek TableIPCode nin yayınlanacağı formu işaret ediyor
            /// böylelikle diğer teferruatlar olmayacak

            if (t.IsNotNull(TableIPCode))
                singleIPCode = true;

            /// single değil ise çok sayfalı bir form olacak
            /// 
            #region singleIPCode
            if (singleIPCode == false)
            {
                string layoutType = string.Empty;

                int rowCount = ds_Layout.Tables[0].Rows.Count;

                string tabControlRefId = "";

                #region for
                for (int i = pos; i < rowCount; i++)
                {
                    if (ds_Layout.Tables[0].Rows[i]["LAYOUT_CODE"].ToString().IndexOf(layoutCode) > -1)
                    {
                        #region
                        layoutType = ds_Layout.Tables[0].Rows[i]["LAYOUT_TYPE"].ToString();
                        if ((layoutType == "tabPane") ||
                            (layoutType == "navigationPane") ||
                            (layoutType == "backstageViewControl")
                            // || (layoutType == "tabControl")
                            )
                        {
                            /// alttaki control devam eden süreçte bu ismi alacak
                            /// örnek : 
                            /// tabPane1.Name = v.lyt_Name + RefId.ToString();

                            //tabControlRefId = t.myInt32(ds_Layout.Tables[0].Rows[i]["REF_ID"].ToString());
                            //tabControlName = v.lyt_Name + tabControlRefId.ToString();

                            tabControlRefId = ds_Layout.Tables[0].Rows[i]["LAYOUT_CODE"].ToString();
                            tabControlName = v.lyt_Name + t.Str_Replace(ref tabControlRefId, ".", "_");

                        }

                        if ((layoutType == "tabNavigationPage") ||
                            (layoutType == "navigationPage") ||
                            (layoutType == "backstageViewTabItem")
                            //|| (layoutType == "tabPage")
                            )
                        {
                            /// buradaki page sayısı ile 
                            /// tableLayoutPanel_Steps deki label sayısı ayarlanacak
                            pageCount++;
                        }
                        #endregion
                    }
                }
                #endregion for
            }
            #endregion singlePage

            #endregion Tanımlar

            #region allControls
            //
            // All Controls
            //
            DevExpress.XtraEditors.GroupControl groupControl1 = new DevExpress.XtraEditors.GroupControl();
            DevExpress.XtraEditors.PanelControl panelControl_Top = new DevExpress.XtraEditors.PanelControl();
            DevExpress.XtraEditors.LabelControl labelControl_Title = new DevExpress.XtraEditors.LabelControl();

            ///---
            ((System.ComponentModel.ISupportInitialize)(groupControl1)).BeginInit();
            groupControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(panelControl_Top)).BeginInit();
            panelControl_Top.SuspendLayout();

            ///---
            if (singleIPCode == false)
            {
                System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Steps =
                dataWizard_Header_Preparing(ds_Layout, pos, pageCount);

                System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Bottom =
                dataWizard_Bottom_Preparing(tabControlName);

                groupControl1.Controls.Add(tableLayoutPanel_Bottom);
                groupControl1.Controls.Add(tableLayoutPanel_Steps);
                tableLayoutPanel_Steps.SendToBack();
            }

            #endregion allControls 

            #region // Ust hesabı var ise

            UstHesapRow = UstHesap_Get(ds_Layout, pos);

            if (UstHesapRow != null)
            {
                ustItemType = UstHesapRow["LAYOUT_TYPE"].ToString();
                //ustHesapRefId = UstHesapRow["REF_ID"].ToString();
                ustHesapRefId = UstHesapRow["LAYOUT_CODE"].ToString();
                t.Str_Replace(ref ustHesapRefId, ".", "_");
                ustItemName = "tLayout_" + ustHesapRefId;

                lParentControlAdd(tForm, groupControl1, ustItemName, ustItemType, row);
            }
            else
                tForm.Controls.Add(groupControl1);

            #endregion

            #region groupControl1 
            // 
            // groupControl1
            //
            groupControl1.Controls.Add(panelControl_Top);
            groupControl1.Location = new System.Drawing.Point(0, 0);
            groupControl1.Name = v.lyt_Name + t.Str_Replace(ref layoutCode, ".", "_"); //RefId.ToString();
            groupControl1.Size = new System.Drawing.Size(203, 435);
            groupControl1.Text = v.tMainFirm.FirmShortName;
            groupControl1.TabIndex = pos;

            if (DockType == v.dock_Bottom) groupControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
            if (DockType == v.dock_Fill) groupControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            if (DockType == v.dock_Left) groupControl1.Dock = System.Windows.Forms.DockStyle.Left;
            if (DockType == v.dock_Right) groupControl1.Dock = System.Windows.Forms.DockStyle.Right;
            if (DockType == v.dock_Top) groupControl1.Dock = System.Windows.Forms.DockStyle.Top;

            t.myControl_Size_And_Location(groupControl1, width, height, left, top);

            if (FrontBack == 2) groupControl1.SendToBack();
            else groupControl1.BringToFront();

            #endregion groupControl1

            #region panelControl_Top
            // 
            // panelControl_Top
            // 
            panelControl_Top.Controls.Add(labelControl_Title);
            panelControl_Top.Dock = System.Windows.Forms.DockStyle.Top;
            panelControl_Top.Location = new System.Drawing.Point(2, 20);
            panelControl_Top.Name = "panelControl_Top";
            panelControl_Top.Padding = new System.Windows.Forms.Padding(30, 4, 4, 4);
            panelControl_Top.Size = new System.Drawing.Size(822, 35);
            panelControl_Top.TabIndex = 0;
            // 
            // labelControl_Title
            // 
            labelControl_Title.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            //labelControl_Title.Appearance.ForeColor = System.Drawing.SystemColors.ControlDark;
            labelControl_Title.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            labelControl_Title.Dock = System.Windows.Forms.DockStyle.Fill;
            labelControl_Title.Location = new System.Drawing.Point(12, 12);
            labelControl_Title.Name = "labelControl_Title";
            labelControl_Title.Size = new System.Drawing.Size(798, 21);
            labelControl_Title.TabIndex = 0;
            labelControl_Title.Text = caption;
            #endregion

            #region // Eğer TableIPCode tanılıtmışsa alt hesapları olmadan
            // tek view hazırlanacak
            // if (t.IsNotNull(TableIPCode))
            if (singleIPCode)
            {
                //
                // navigationPane1
                //
                DevExpress.XtraBars.Navigation.NavigationPane navigationPane1 = new DevExpress.XtraBars.Navigation.NavigationPane();
                ((System.ComponentModel.ISupportInitialize)(navigationPane1)).BeginInit();

                navigationPane1.Location = new System.Drawing.Point(0, 0);
                navigationPane1.Name = "navigationPane_" + t.AntiStr_Dot(TableIPCode); //RefId.ToString(); //+ v.lyt_Name 
                navigationPane1.AccessibleName = TableIPCode;
                navigationPane1.Visible = true;
                navigationPane1.SelectedPageIndex = 0;
                navigationPane1.Size = new System.Drawing.Size(v.Screen_Width, v.Screen_Height);
                navigationPane1.TabIndex = pos;

                /// hazırlanacak bu TableIPCode nin group sayısına göre
                /// tek navigationPage veya group sayısı kadar navigationPage oluşturulacak
                /// 

                // data wizard grubuna ekleniyor
                groupControl1.Controls.Add(navigationPane1);

                navigationPane1.Dock = DockStyle.Fill;
                navigationPane1.BringToFront();

                ((System.ComponentModel.ISupportInitialize)(navigationPane1)).EndInit();

                //
                // InputPanel Create
                //

                InputPanel_Preparing(tForm, navigationPane1, TableIPCode, IPDataType);

                if (navigationPane1.Pages.Count == 1)
                {
                    navigationPane1.Pages[0].PageText = fieldName;
                    navigationPane1.Pages[0].Caption = caption;
                }
            }
            #endregion

            panelControl_Top.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(panelControl_Top)).EndInit();

            groupControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(groupControl1)).EndInit();

        }

        private System.Windows.Forms.TableLayoutPanel dataWizard_Header_Preparing(
            DataSet ds_Layout, int pos, int pageCount)
        {
            // hataya düşmesini önlemek için
            if (pageCount == 0) pageCount++;

            float oran = 0;
            oran = 100 / pageCount;

            System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Steps = new System.Windows.Forms.TableLayoutPanel();
            DevExpress.XtraEditors.LabelControl labelControl_About = new DevExpress.XtraEditors.LabelControl();

            tableLayoutPanel_Steps.SuspendLayout();

            #region
            // 
            // tableLayoutPanel_Steps
            // 
            //tableLayoutPanel_Steps.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;
            tableLayoutPanel_Steps.ColumnCount = pageCount + 2;
            tableLayoutPanel_Steps.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            tableLayoutPanel_Steps.Dock = System.Windows.Forms.DockStyle.Top;
            tableLayoutPanel_Steps.Location = new System.Drawing.Point(2, 65);
            tableLayoutPanel_Steps.Name = "tableLayoutPanel_Steps";
            tableLayoutPanel_Steps.Padding = new System.Windows.Forms.Padding(8);
            tableLayoutPanel_Steps.RowCount = 2;

            // Açıklama için kullanılan row
            //tableLayoutPanel_Steps.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            //tableLayoutPanel_Steps.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
            //tableLayoutPanel_Steps.Size = new System.Drawing.Size(822, 90);
            tableLayoutPanel_Steps.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel_Steps.Size = new System.Drawing.Size(822, 45);
            tableLayoutPanel_Steps.TabIndex = 1;
            tableLayoutPanel_Steps.Visible = true;

            string caption = string.Empty;
            string layoutType = string.Empty;

            int i2 = 0;
            int rowCount = ds_Layout.Tables[0].Rows.Count;

            for (int i = pos; i < rowCount; i++)
            {
                layoutType = ds_Layout.Tables[0].Rows[i]["LAYOUT_TYPE"].ToString();
                if ((layoutType == "tabNavigationPage") ||
                    (layoutType == "navigationPage") ||
                    (layoutType == "backstageViewTabItem")
                    // || (layoutType == "tabPage")
                    )
                {
                    caption = ds_Layout.Tables[0].Rows[i]["LAYOUT_CAPTION"].ToString();

                    tableLayoutPanel_Steps.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, oran));

                    DevExpress.XtraEditors.LabelControl labelControl_Step1 = new DevExpress.XtraEditors.LabelControl();
                    // 
                    // labelControl_Step1
                    // 

                    if (i2 == 0)
                    {
                        //labelControl_Step1.Appearance.BackColor = System.Drawing.SystemColors.InactiveCaption;
                        //labelControl_Step1.Appearance.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
                    }
                    if (i2 > 0)
                    {
                        //labelControl_Step1.Appearance.BackColor = System.Drawing.SystemColors.ControlLight;
                        //labelControl_Step1.Appearance.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
                    }

                    labelControl_Step1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
                    labelControl_Step1.Dock = System.Windows.Forms.DockStyle.Fill;
                    labelControl_Step1.Location = new System.Drawing.Point(413, 11);
                    labelControl_Step1.Name = "labelControl_Step_" + i2.ToString();
                    labelControl_Step1.Padding = new System.Windows.Forms.Padding(5);
                    labelControl_Step1.Size = new System.Drawing.Size(185, 37);
                    labelControl_Step1.TabIndex = i;
                    labelControl_Step1.Text = caption; // "labelControl_Step_" + (i+1).ToString();

                    tableLayoutPanel_Steps.Controls.Add(labelControl_Step1, i2 + 1, 0);
                    i2++;
                }
            }

            tableLayoutPanel_Steps.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));

            // 
            // labelControl_About
            // 
            labelControl_About.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            //labelControl_About.Appearance.ForeColor = System.Drawing.Color.MediumSeaGreen;
            labelControl_About.Dock = System.Windows.Forms.DockStyle.Fill;
            labelControl_About.Location = new System.Drawing.Point(222, 54);
            labelControl_About.Name = "labelControl_About";
            labelControl_About.Padding = new System.Windows.Forms.Padding(5);
            labelControl_About.Size = new System.Drawing.Size(567, 60);
            labelControl_About.TabIndex = 0;
            labelControl_About.Text = "Açıklamalar";

            //tableLayoutPanel_Steps.SetColumnSpan(labelControl_About, 3);
            //tableLayoutPanel_Steps.Controls.Add(labelControl_About, 1, 1);

            tableLayoutPanel_Steps.PerformLayout();
            tableLayoutPanel_Steps.ResumeLayout(false);

            #endregion

            return tableLayoutPanel_Steps;
        }

        private System.Windows.Forms.TableLayoutPanel dataWizard_Bottom_Preparing(string TableIPCode)
        {
            tToolBox t = new tToolBox();
            tEvents ev = new tEvents();

            /// TableIPCode 
            /// dataWizardın kendisi create edilirken aslında TableIPCode kullanılmıyor
            /// 

            ///---
            System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Bottom = new System.Windows.Forms.TableLayoutPanel();
            DevExpress.XtraEditors.SimpleButton simpleButton_sihirbaz_geri = new DevExpress.XtraEditors.SimpleButton();
            DevExpress.XtraEditors.SimpleButton simpleButton_sihirbaz_sonra = new DevExpress.XtraEditors.SimpleButton();

            tableLayoutPanel_Bottom.SuspendLayout();

            #region tableLayoutPanel_Bottom
            // 
            // tableLayoutPanel_Bottom
            // 
            //tableLayoutPanel_Bottom.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;
            tableLayoutPanel_Bottom.ColumnCount = 6;
            tableLayoutPanel_Bottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel_Bottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            tableLayoutPanel_Bottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            //tableLayoutPanel_Bottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            //tableLayoutPanel_Bottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            tableLayoutPanel_Bottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));

            tableLayoutPanel_Bottom.Controls.Add(simpleButton_sihirbaz_geri, 1, 0);
            tableLayoutPanel_Bottom.Controls.Add(simpleButton_sihirbaz_sonra, 2, 0);
            tableLayoutPanel_Bottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            tableLayoutPanel_Bottom.Location = new System.Drawing.Point(2, 522);
            tableLayoutPanel_Bottom.Name = "tableLayoutPanel_Bottom";
            tableLayoutPanel_Bottom.Padding = new System.Windows.Forms.Padding(4);
            tableLayoutPanel_Bottom.RowCount = 1;
            tableLayoutPanel_Bottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel_Bottom.Size = new System.Drawing.Size(822, 40);
            tableLayoutPanel_Bottom.TabIndex = 2;
            tableLayoutPanel_Bottom.Visible = true; //false;
            // 
            // simpleButton_sihirbaz_sonra
            // 
            simpleButton_sihirbaz_sonra.Location = new System.Drawing.Point(697, 11);
            simpleButton_sihirbaz_sonra.Name = "simpleButton_sihirbaz_sonra";
            simpleButton_sihirbaz_sonra.Size = new System.Drawing.Size(100, 23);
            simpleButton_sihirbaz_sonra.TabIndex = 8;
            simpleButton_sihirbaz_sonra.Text = "&Devam";//"&Sonra";

            //simpleButton_sihirbaz_sonra.Click += new System.EventHandler(ev.btn_DataWizard_Click);
            simpleButton_sihirbaz_sonra.Click += new System.EventHandler(ev.btn_Navigotor_Click);
            simpleButton_sihirbaz_sonra.AccessibleName = TableIPCode;
            simpleButton_sihirbaz_sonra.Image = t.Find_Glyph("SIHIRBAZDEVAM16");

            // 
            // simpleButton_Geri
            // 
            simpleButton_sihirbaz_geri.Location = new System.Drawing.Point(597, 11);
            simpleButton_sihirbaz_geri.Name = "simpleButton_sihirbaz_geri";
            simpleButton_sihirbaz_geri.Size = new System.Drawing.Size(100, 23);
            simpleButton_sihirbaz_geri.TabIndex = 9;
            simpleButton_sihirbaz_geri.Text = "&Geri";
            simpleButton_sihirbaz_geri.Enabled = false;
            //simpleButton_Geri.Click += new System.EventHandler(ev.btn_DataWizard_Click);
            simpleButton_sihirbaz_geri.Click += new System.EventHandler(ev.btn_Navigotor_Click);
            simpleButton_sihirbaz_geri.AccessibleName = TableIPCode;
            simpleButton_sihirbaz_geri.Image = t.Find_Glyph("SIHIRBAZGERI16");
            /*
            // 
            // simpleButton3
            // 
            simpleButton3.Location = new System.Drawing.Point(497, 11);
            simpleButton3.Name = "simpleButton3";
            simpleButton3.Size = new System.Drawing.Size(75, 23);
            simpleButton3.TabIndex = 10;
            simpleButton3.Text = "simpleButton3";
            simpleButton3.Visible = false;
            // 
            // simpleButton4
            // 
            simpleButton4.Location = new System.Drawing.Point(397, 11);
            simpleButton4.Name = "simpleButton4";
            simpleButton4.Size = new System.Drawing.Size(75, 23);
            simpleButton4.TabIndex = 11;
            simpleButton4.Text = "simpleButton4";
            simpleButton4.Visible = false;
            */
            #endregion

            tableLayoutPanel_Bottom.ResumeLayout(false);

            return tableLayoutPanel_Bottom;
        }

        #endregion Data Wizard

        #region Order Panels

        private void lDocumentViewerFast_Preparing(Form tForm, Control subView, DataSet ds_Layout, DataRow row, int pos)
        {
            tToolBox t = new tToolBox();

            int RefId = 0;
            string CmpName = string.Empty;
            string caption = string.Empty;
            string layout_code = string.Empty;

            string TableIPCode = string.Empty;
            string fieldName = string.Empty;

            int width = 0;

            string ustItemType = string.Empty;
            string ustHesapRefId = string.Empty;
            string ustHesapItemName = string.Empty;
            string ustItemName = string.Empty;
            DataRow UstHesapRow = null;

            RefId = t.myInt32(row["REF_ID"].ToString());
            layout_code = t.Set(row["LAYOUT_CODE"].ToString(), "", "");
            caption = t.Set(row["LAYOUT_CAPTION"].ToString(), "", "");
            TableIPCode = t.Set(row["TABLEIPCODE"].ToString(), "", "");
            fieldName = t.Set(row["FIELD_NAME"].ToString(), "", "");
            CmpName = t.Set(row["CMP_NAME"].ToString(), "", "");

            width = t.myInt32(row["CMP_WIDTH"].ToString());

            UstHesapRow = UstHesap_Get(ds_Layout, pos);

            #region // Ust hesabı var ise

            if (UstHesapRow != null)
            {
                ustItemType = UstHesapRow["LAYOUT_TYPE"].ToString();
                ustHesapRefId = UstHesapRow["LAYOUT_CODE"].ToString();
                t.Str_Replace(ref ustHesapRefId, ".", "_");
                ustItemName = v.lyt_Name + ustHesapRefId;

                if (t.IsNotNull(UstHesapRow["CMP_NAME"].ToString()))
                    ustItemName = UstHesapRow["CMP_NAME"].ToString();

                FastReport.Preview.PreviewControl documentViewer = new FastReport.Preview.PreviewControl();
                                
                // 
                // documentViewer
                // 
                documentViewer.Dock = System.Windows.Forms.DockStyle.Fill;
                documentViewer.Name = "documentViewerFast"; //v.lyt_Name + t.Str_Replace(ref layout_code, ".", "_");  //RefId.ToString();
                if (t.IsNotNull(CmpName))
                    documentViewer.Name = CmpName;

                documentViewer.Padding = new System.Windows.Forms.Padding(4);
                documentViewer.Size = new System.Drawing.Size(874, 40);
                documentViewer.TabStop = true;
                documentViewer.TabIndex = 0;

                //Panel panel1 = new Panel();
                //panel1.Controls.Add(documentViewerBar);
                //panel1.Controls.Add(documentViewer);

                lParentControlAdd(tForm, documentViewer, ustItemName, ustItemType, row);
                //lParentControlAdd(tForm, printTool, ustItemName, ustItemType, row);

                documentViewer.SendToBack();
            }

            #endregion

        }

        private void lDocumentViewerDev_Preparing(Form tForm, Control subView, DataSet ds_Layout, DataRow row, int pos)
        {
            tToolBox t = new tToolBox();

            int RefId = 0;
            string CmpName = string.Empty;
            string caption = string.Empty;
            string layout_code = string.Empty;

            string TableIPCode = string.Empty;
            string fieldName = string.Empty;

            int width = 0;

            string ustItemType = string.Empty;
            string ustHesapRefId = string.Empty;
            string ustHesapItemName = string.Empty;
            string ustItemName = string.Empty;
            DataRow UstHesapRow = null;

            RefId = t.myInt32(row["REF_ID"].ToString());
            layout_code = t.Set(row["LAYOUT_CODE"].ToString(), "", "");
            caption = t.Set(row["LAYOUT_CAPTION"].ToString(), "", "");
            TableIPCode = t.Set(row["TABLEIPCODE"].ToString(), "", "");
            fieldName = t.Set(row["FIELD_NAME"].ToString(), "", "");
            CmpName = t.Set(row["CMP_NAME"].ToString(), "", "");

            width = t.myInt32(row["CMP_WIDTH"].ToString());

            UstHesapRow = UstHesap_Get(ds_Layout, pos);

            #region // Ust hesabı var ise

            if (UstHesapRow != null)
            {
                ustItemType = UstHesapRow["LAYOUT_TYPE"].ToString();
                ustHesapRefId = UstHesapRow["LAYOUT_CODE"].ToString();
                t.Str_Replace(ref ustHesapRefId, ".", "_");
                ustItemName = v.lyt_Name + ustHesapRefId;

                if (t.IsNotNull(UstHesapRow["CMP_NAME"].ToString()))
                    ustItemName = UstHesapRow["CMP_NAME"].ToString();

                //DevExpress.XtraPrinting.Preview.DocumentViewerBarManager documentViewerBar = new DevExpress.XtraPrinting.Preview.DocumentViewerBarManager();
                DevExpress.XtraPrinting.Preview.DocumentViewer documentViewer = new DevExpress.XtraPrinting.Preview.DocumentViewer();

                //XtraReport xtraReport1 = new XtraReport();
                //DevExpress.XtraReports.UI.ReportPrintTool printTool = new DevExpress.XtraReports.UI.ReportPrintTool(xtraReport1);

                // 
                // documentViewer
                // 
                documentViewer.Dock = System.Windows.Forms.DockStyle.Fill;
                documentViewer.Name = "documentViewerDevEx"; //v.lyt_Name + t.Str_Replace(ref layout_code, ".", "_");  //RefId.ToString();
                if (t.IsNotNull(CmpName))
                    documentViewer.Name = CmpName;

                documentViewer.Padding = new System.Windows.Forms.Padding(4);
                documentViewer.Size = new System.Drawing.Size(874, 40);
                documentViewer.TabStop = true;
                documentViewer.TabIndex = 0;

                //Panel panel1 = new Panel();
                //panel1.Controls.Add(documentViewerBar);
                //panel1.Controls.Add(documentViewer);

                lParentControlAdd(tForm, documentViewer, ustItemName, ustItemType, row);
                //lParentControlAdd(tForm, printTool, ustItemName, ustItemType, row);

                documentViewer.SendToBack();
            }

            #endregion

        }

        private void lWebBrowser_Preparing(Form tForm, Control subView, DataSet ds_Layout, DataRow row, int pos)
        {
            tToolBox t = new tToolBox();

            int RefId = 0;
            string CmpName = string.Empty;
            string caption = string.Empty;
            string layout_code = string.Empty;

            //string Prop_View = string.Empty;
            string TableIPCode = string.Empty;
            string fieldName = string.Empty;

            int width = 0;

            string ustItemType = string.Empty;
            string ustHesapRefId = string.Empty;
            string ustHesapItemName = string.Empty;
            string ustItemName = string.Empty;
            DataRow UstHesapRow = null;

            RefId = t.myInt32(row["REF_ID"].ToString());
            layout_code = t.Set(row["LAYOUT_CODE"].ToString(), "", "");
            caption = t.Set(row["LAYOUT_CAPTION"].ToString(), "", "");
            TableIPCode = t.Set(row["TABLEIPCODE"].ToString(), "", "");
            fieldName = t.Set(row["FIELD_NAME"].ToString(), "", "");
            CmpName = t.Set(row["CMP_NAME"].ToString(), "", "");

            //Prop_View = t.Set(row["PROP_VIEWS"].ToString(), "", "");
            //TableIPCode = t.Set(t.MyProperties_Get(Prop_View, "HP_TABLEIPCODE:"), "", "");
            //FieldName = t.Set(t.MyProperties_Get(Prop_View, "HP_FNAME:"), "", "");

            width = t.myInt32(row["CMP_WIDTH"].ToString());

            UstHesapRow = UstHesap_Get(ds_Layout, pos);

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

                WebBrowser webBrowser1 = new WebBrowser();
                // 
                // webBrowser1
                // 
                webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
                webBrowser1.Name = "WebMain";//v.lyt_Name + t.Str_Replace(ref layout_code, ".", "_");  //RefId.ToString();
                if (t.IsNotNull(CmpName))
                    webBrowser1.Name = CmpName;
                webBrowser1.Padding = new System.Windows.Forms.Padding(4);
                webBrowser1.Size = new System.Drawing.Size(874, 40);
                webBrowser1.TabStop = true;
                webBrowser1.TabIndex = 0;

                lParentControlAdd(tForm, webBrowser1, ustItemName, ustItemType, row);

                webBrowser1.SendToBack();

                // test
                // webBrowser1.Navigate("https://ustadyazilim.com/");
            }

            #endregion
        }

        private void lCefWebBrowser_Preparing(Form tForm, Control subView, DataSet ds_Layout, DataRow row, int pos)
        {
            if (v.SP_Debug) return;
            if (v.con_NewFilesFound) return; /// ftpden yeni dosyalar indirildi

            tToolBox t = new tToolBox();

            int RefId = 0;
            int width = 0;
            int height = 0;
            int top = 0;
            int left = 0;
            int DockType = 0;

            string CmpName = string.Empty;
            string caption = string.Empty;
            string layout_code = string.Empty;

            //string Prop_View = string.Empty;
            string TableIPCode = string.Empty;
            string fieldName = string.Empty;

            string ustItemType = string.Empty;
            string ustHesapRefId = string.Empty;
            string ustHesapItemName = string.Empty;
            string ustItemName = string.Empty;
            DataRow UstHesapRow = null;

            RefId = t.myInt32(row["REF_ID"].ToString());
            layout_code = t.Set(row["LAYOUT_CODE"].ToString(), "", "");
            caption = t.Set(row["LAYOUT_CAPTION"].ToString(), "", "");
            TableIPCode = t.Set(row["TABLEIPCODE"].ToString(), "", "");
            fieldName = t.Set(row["FIELD_NAME"].ToString(), "", "");
            CmpName = t.Set(row["CMP_NAME"].ToString(), "", "");

            width = t.myInt32(row["CMP_WIDTH"].ToString());
            height = t.myInt32(row["CMP_HEIGHT"].ToString());
            top = t.myInt32(row["CMP_TOP"].ToString());
            left = t.myInt32(row["CMP_LEFT"].ToString());

            DockType = t.Set(row["CMP_DOCK"].ToString(), v.dock_Fill.ToString(), (int)0);

            UstHesapRow = UstHesap_Get(ds_Layout, pos);

            #region // Ust hesabı var ise

            if (UstHesapRow != null)
            {
                // browser için sadece panel hazırlıyoruz,
                // ms_CefSharp.cs shown sırasında bu paneli bulup panelin içine (v.cefBrowser_) add oluyor
                // böylece v.cefBrowser_ bir siteye connect olduktan sonra çalışma yapılan form kapansada bağlantı kopmuyor

                ustItemType = UstHesapRow["LAYOUT_TYPE"].ToString();
                ustHesapRefId = UstHesapRow["LAYOUT_CODE"].ToString();
                t.Str_Replace(ref ustHesapRefId, ".", "_");
                ustItemName = v.lyt_Name + ustHesapRefId;

                if (t.IsNotNull(UstHesapRow["CMP_NAME"].ToString()))
                    ustItemName = UstHesapRow["CMP_NAME"].ToString();

                string cefPath = "CEF_" + DateTime.Now.Ticks.ToString();
                //string rootPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + Application.StartupPath;
                string rootPath = v.EXE_PATH;
                string newPath = rootPath + "\\" + cefPath;

                ChromiumWebBrowser webBrowser1 = null;

                if (t.IsNotNull(TableIPCode) == false)
                    TableIPCode = "www.google.com";
                try
                {
                    // bunu çalıştıramadım ama metod güzel onun için silmedim
                    // CefSharp bileşenlerini kontrol et ve gerekirse indir
                    // CefSharpInstaller.EnsureCefSharpComponents().Wait();
                    //t.X64filesUpdates();
                    
                    /// DİKKAT : Cef ile Selenium karıştırma iki birbirinden farklı
                    /// 
                    webBrowser1 = new ChromiumWebBrowser(TableIPCode);

                    // DPI uyumluluğu için
                    webBrowser1.BrowserSettings.WindowlessFrameRate = 60;
                }
                catch (Exception ex)
                {
                    //t.X64filesUpdates();
                    //t.LaunchCommandLineApp();
                    return;
                }

                Tkn_CefSharp.tCefSharp tCefSharp = new Tkn_CefSharp.tCefSharp();
                
                webBrowser1.IsBrowserInitializedChanged += tCefSharp.OnIsBrowserInitializedChanged;
                webBrowser1.LoadingStateChanged += tCefSharp.OnLoadingStateChanged;
                webBrowser1.ConsoleMessage += tCefSharp.OnBrowserConsoleMessage;
                webBrowser1.StatusMessage += tCefSharp.OnBrowserStatusMessage;
                webBrowser1.TitleChanged += tCefSharp.OnBrowserTitleChanged;
                webBrowser1.AddressChanged += tCefSharp.OnBrowserAddressChanged;
                webBrowser1.LoadError += tCefSharp.OnBrowserLoadError;
                
#if NETCOREAPP
            // .NET Core
            var environment = string.Format("Environment: {0}, Runtime: {1}",
                System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant(),
                System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
#else
                // .NET Framework
                var bitness = Environment.Is64BitProcess ? "x64" : "x86";
                var environment = String.Format("Environment: {0}", bitness);
#endif
                lParentControlAdd(tForm, webBrowser1, ustItemName, ustItemType, row);
            }

            #endregion
        }
        private void lHeaderPanel_Preparing(Form tForm, Control subView, DataSet ds_Layout, DataRow row, int pos)
        {
            tToolBox t = new tToolBox();

            int RefId = 0;
            string CmpName = string.Empty;
            string caption = string.Empty;
            string layout_code = string.Empty;

            //string Prop_View = string.Empty;
            string TableIPCode = string.Empty;
            string fieldName = string.Empty;

            int width = 0;
            int DockType = 0;
            float font_size = (float)0;
            string ustItemType = string.Empty;
            string ustHesapRefId = string.Empty;
            string ustHesapItemName = string.Empty;
            string ustItemName = string.Empty;
            DataRow UstHesapRow = null;

            RefId = t.myInt32(row["REF_ID"].ToString());
            layout_code = t.Set(row["LAYOUT_CODE"].ToString(), "", "");
            caption = t.Set(row["LAYOUT_CAPTION"].ToString(), "", "");
            TableIPCode = t.Set(row["TABLEIPCODE"].ToString(), "", "");
            fieldName = t.Set(row["FIELD_NAME"].ToString(), "", "");
            CmpName = t.Set(row["CMP_NAME"].ToString(), "", "");
            DockType = t.Set(row["CMP_DOCK"].ToString(), v.dock_Fill.ToString(), (int)0);

            //Prop_View = t.Set(row["PROP_VIEWS"].ToString(), "", "");
            //TableIPCode = t.Set(t.MyProperties_Get(Prop_View, "HP_TABLEIPCODE:"), "", "");
            //FieldName = t.Set(t.MyProperties_Get(Prop_View, "HP_FNAME:"), "", "");

            width = t.myInt32(row["CMP_WIDTH"].ToString());
            font_size = t.Set(row["CMP_FONT_SIZE"].ToString(), "", (float)0);

            UstHesapRow = UstHesap_Get(ds_Layout, pos);

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

                DevExpress.XtraEditors.PanelControl panelControl1 = new DevExpress.XtraEditors.PanelControl();
                DevExpress.XtraEditors.LabelControl labelControl1 = new DevExpress.XtraEditors.LabelControl();
                DevExpress.XtraEditors.TextEdit textEdit1 = new DevExpress.XtraEditors.TextEdit();
                ((System.ComponentModel.ISupportInitialize)(panelControl1)).BeginInit();
                panelControl1.SuspendLayout();
                ((System.ComponentModel.ISupportInitialize)(textEdit1.Properties)).BeginInit();
                // 
                // panelControl1
                // 
                panelControl1.Controls.Add(textEdit1); 
                panelControl1.Controls.Add(labelControl1);

                if (DockType == v.dock_Bottom) panelControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
                if (DockType == v.dock_Fill) panelControl1.Dock = System.Windows.Forms.DockStyle.Fill;
                if (DockType == v.dock_Left) panelControl1.Dock = System.Windows.Forms.DockStyle.Left;
                if (DockType == v.dock_Right) panelControl1.Dock = System.Windows.Forms.DockStyle.Right;
                if (DockType == v.dock_Top) panelControl1.Dock = System.Windows.Forms.DockStyle.Top;
                
                panelControl1.Location = new System.Drawing.Point(0, 0);
                panelControl1.Name = v.lyt_Name + t.Str_Replace(ref layout_code, ".", "_");  //RefId.ToString();
                if (t.IsNotNull(CmpName))
                    panelControl1.Name = CmpName;
                panelControl1.Padding = new System.Windows.Forms.Padding(4);
                panelControl1.Size = new System.Drawing.Size(874, 40);
                panelControl1.TabStop = false;
                panelControl1.TabIndex = 0;
                // 
                // labelControl1
                // 
                labelControl1.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
                labelControl1.Appearance.Options.UseFont = true;
                labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
                labelControl1.Dock = System.Windows.Forms.DockStyle.Left;
                labelControl1.Location = new System.Drawing.Point(6, 6);
                labelControl1.Name = "labelControl_" + RefId.ToString();
                labelControl1.Size = new System.Drawing.Size(150, 28);
                labelControl1.TabIndex = 0;
                labelControl1.Text = caption;

                if (width > 0) labelControl1.Size = new System.Drawing.Size(width, 28);

                // 
                // textEdit1
                // 
                textEdit1.Dock = System.Windows.Forms.DockStyle.Fill;
                textEdit1.EditValue = "";
                textEdit1.Enabled = false;
                textEdit1.Location = new System.Drawing.Point(156, 6);
                textEdit1.Name = "textEdit_" + RefId.ToString();
                //textEdit1.Properties.Appearance.BackColor = System.Drawing.Color.FloralWhite;
                //textEdit1.Properties.Appearance.Options.UseBackColor = true;
                if (font_size == 0)
                    textEdit1.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
                else
                {
                    Single fontS = Convert.ToInt32(font_size);
                    textEdit1.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", fontS, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
                }
                textEdit1.Properties.Appearance.Options.UseFont = true;
                textEdit1.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
                textEdit1.Size = new System.Drawing.Size(712, 28);

                textEdit1.TabStop = false;
                textEdit1.TabIndex = 0;
                
                if ((TableIPCode != "") && (fieldName != ""))
                {
                    try
                    {
                        DataSet dsData = t.Find_DataSet(tForm, "", TableIPCode, "");

                        if (dsData != null)
                            textEdit1.DataBindings.Add(new Binding("EditValue", dsData.Tables[0], fieldName));
                    }
                    catch (Exception)
                    {

                        //throw;
                    }
                }
                else
                {
                    textEdit1.Visible = false;
                    labelControl1.Dock = DockStyle.Fill;
                }

                if (caption == "") labelControl1.Visible = false;

                ((System.ComponentModel.ISupportInitialize)(panelControl1)).EndInit();
                panelControl1.ResumeLayout(false);
                ((System.ComponentModel.ISupportInitialize)(textEdit1.Properties)).EndInit();

                lParentControlAdd(tForm, panelControl1, ustItemName, ustItemType, row);

                panelControl1.SendToBack();
            }

            #endregion
        }

        private void lDigitalPanel_Preparing(Form tForm, Control subView, DataSet ds_Layout, DataRow row, int pos)
        {
            tToolBox t = new tToolBox();

            int RefId = 0;
            int width = 0;
            int height = 0;
            int top = 0;
            int left = 0;
            int DockType = 0;

            string CmpName = string.Empty;
            string caption = string.Empty;
            string Prop_View = string.Empty;
            string TableIPCode = string.Empty;
            string fieldName = string.Empty;
            string tcolumntype = string.Empty;
            string tdisplayformat = "";
            string layout_code = string.Empty;

            string ustItemType = string.Empty;
            string ustHesapRefId = string.Empty;
            string ustHesapItemName = string.Empty;
            string ustItemName = string.Empty;
            DataRow UstHesapRow = null;

            string s1 = "=ROW_PROP_VIEWS:";
            string s2 = (char)34 + "EDITPANEL" + (char)34 + ": {";
            //"EDITPANEL": {

            RefId = t.myInt32(row["REF_ID"].ToString());
            caption = t.Set(row["LAYOUT_CAPTION"].ToString(), "", "");
            layout_code = t.Set(row["LAYOUT_CODE"].ToString(), "", "");
            TableIPCode = t.Set(row["TABLEIPCODE"].ToString(), "", "");
            fieldName = t.Set(row["FIELD_NAME"].ToString(), "", "");
            CmpName = t.Set(row["CMP_NAME"].ToString(), "", "");

            width = t.myInt32(row["CMP_WIDTH"].ToString());
            height = t.myInt32(row["CMP_HEIGHT"].ToString());
            top = t.myInt32(row["CMP_TOP"].ToString());
            left = t.myInt32(row["CMP_LEFT"].ToString());

            DockType = t.Set(row["CMP_DOCK"].ToString(), v.dock_Fill.ToString(), (int)0);

            Prop_View = t.Set(row["PROP_VIEWS"].ToString(), "", "");

            // default
            tcolumntype = "MemoEdit";

            // old
            if (Prop_View.IndexOf(s1) > -1)
                tcolumntype = t.Set(t.MyProperties_Get(Prop_View, "EP_CMP_TYPE:"), "MemoEdit", "");
            // json 
            if (Prop_View.IndexOf(s2) > -1)
                tcolumntype = t.Set(t.Find_Properties_Value(Prop_View, "EP_CMP_TYPE"), "MemoEdit", "");


            width = t.myInt32(row["CMP_WIDTH"].ToString());

            UstHesapRow = UstHesap_Get(ds_Layout, pos);

            #region // Ust hesabı var ise

            if (UstHesapRow != null)
            {
                tDevColumn dc = new tDevColumn();

                ustItemType = UstHesapRow["LAYOUT_TYPE"].ToString();
                //ustHesapRefId = UstHesapRow["REF_ID"].ToString();
                ustHesapRefId = UstHesapRow["LAYOUT_CODE"].ToString();
                t.Str_Replace(ref ustHesapRefId, ".", "_");
                ustItemName = v.lyt_Name + ustHesapRefId;

                /*
                GaugeControl gc = new GaugeControl();
                // Add a digital gauge.
                DigitalGauge digitalGauge = gc.AddDigitalGauge();
                // The text to be displayed.
                digitalGauge.Text = "Gauge Control";
                // The number of digits.
                digitalGauge.DigitCount = 14;
                // Use 14 segment display mode.
                digitalGauge.DisplayMode = DigitalGaugeDisplayMode.FourteenSegment;
                // Add a background layer and set its painting style.
                DigitalBackgroundLayerComponent background = digitalGauge.AddBackgroundLayer();
                background.ShapeType = DigitalBackgroundShapeSetType.Style2;
                // Set the color of digits.
                digitalGauge.AppearanceOn.ContentBrush = new SolidBrushObject(Color.Red);
                // Add the gauge control to the form.
                //gc.Size = new Size(250, 100);
                digitalGauge.Bounds = new System.Drawing.Rectangle(6, 6, 378, 288);
                digitalGauge.DigitCount = 20;

                gc.Dock = DockStyle.Fill;

                panelControl1.Controls.Add(gc);

                try
                {
                    DataSet dsData = t.Find_DataSet(tForm, "", TableIPCode, "");

                    if (dsData != null)
                        digitalGauge.DataBindings.Add(new Binding("Text", dsData.Tables[0], fieldName));
                }
                catch (Exception)
                {

                    //throw;
                }
                */

                GaugeControl gaugeControl1 = new GaugeControl();
                DigitalGauge digitalGauge1 = gaugeControl1.AddDigitalGauge();
                DigitalBackgroundLayerComponent digitalBackgroundLayerComponent1 = new DigitalBackgroundLayerComponent();
                ((System.ComponentModel.ISupportInitialize)(digitalGauge1)).BeginInit();
                ((System.ComponentModel.ISupportInitialize)(digitalBackgroundLayerComponent1)).BeginInit();
                //
                // gaugeControl1
                // 
                gaugeControl1.Dock = System.Windows.Forms.DockStyle.Fill;
                gaugeControl1.Gauges.AddRange(new DevExpress.XtraGauges.Base.IGauge[] {digitalGauge1});
                gaugeControl1.Location = new System.Drawing.Point(0, 0);
                gaugeControl1.Name = v.lyt_Name + t.Str_Replace(ref layout_code, ".", "_");  
                if (t.IsNotNull(CmpName))
                    gaugeControl1.Name = CmpName;
                gaugeControl1.Size = new System.Drawing.Size(100, 100);
                gaugeControl1.TabIndex = 0;
                // 
                // digitalGauge1
                // 
                digitalGauge1.AppearanceOff.ContentBrush = new DevExpress.XtraGauges.Core.Drawing.SolidBrushObject("Color:#00FFFFFF");
                digitalGauge1.AppearanceOn.ContentBrush = new DevExpress.XtraGauges.Core.Drawing.SolidBrushObject("Color:WhiteSmoke");
                digitalGauge1.BackgroundLayers.AddRange(new DevExpress.XtraGauges.Win.Gauges.Digital.DigitalBackgroundLayerComponent[] {
                    digitalBackgroundLayerComponent1});
                digitalGauge1.Bounds = new System.Drawing.Rectangle(6, 6, 378, 288);
                digitalGauge1.DigitCount = 20;
                digitalGauge1.Name = "";
                digitalGauge1.Text = "00,00";
                // 
                // digitalBackgroundLayerComponent1
                // 
                digitalBackgroundLayerComponent1.BottomRight = new DevExpress.XtraGauges.Core.Base.PointF2D(307.775F, 99.9625F);
                digitalBackgroundLayerComponent1.Name = "digitalBackgroundLayerComponent1";
                digitalBackgroundLayerComponent1.ShapeType = DevExpress.XtraGauges.Core.Model.DigitalBackgroundShapeSetType.Style7;
                digitalBackgroundLayerComponent1.TopLeft = new DevExpress.XtraGauges.Core.Base.PointF2D(0F, 0F);
                digitalBackgroundLayerComponent1.ZOrder = 1000;


                // Add a background layer and set its painting style.
                //DigitalBackgroundLayerComponent background = digitalGauge1.AddBackgroundLayer();
                //background.ShapeType = DigitalBackgroundShapeSetType.Style1;


                //digitalGauge1.DisplayMode = DigitalGaugeDisplayMode.FourteenSegment;

                ((System.ComponentModel.ISupportInitialize)(digitalGauge1)).EndInit();
                ((System.ComponentModel.ISupportInitialize)(digitalBackgroundLayerComponent1)).EndInit();

                if (t.IsNotNull(TableIPCode) == false)
                    TableIPCode = "null";
                if (t.IsNotNull(fieldName) == false)
                    fieldName = "null";
                
                if ((t.IsNotNull(TableIPCode)) && (t.IsNotNull(fieldName)))
                {
                    try
                    {
                        DataSet dsData = t.Find_DataSet(tForm, "", TableIPCode, "");

                        if (dsData != null)
                            digitalGauge1.DataBindings.Add(new Binding("Text", dsData.Tables[0], fieldName));
                    }
                    catch (Exception)
                    {
                        //throw;
                    }
                }
                
                gaugeControl1.AccessibleName = TableIPCode;
                gaugeControl1.AccessibleDescription = fieldName;

                if (DockType == v.dock_Bottom) gaugeControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
                if (DockType == v.dock_Fill) gaugeControl1.Dock = System.Windows.Forms.DockStyle.Fill;
                if (DockType == v.dock_Left) gaugeControl1.Dock = System.Windows.Forms.DockStyle.Left;
                if (DockType == v.dock_Right) gaugeControl1.Dock = System.Windows.Forms.DockStyle.Right;
                if (DockType == v.dock_Top) gaugeControl1.Dock = System.Windows.Forms.DockStyle.Top;

                t.myControl_Size_And_Location(gaugeControl1, width, height, left, top);
                
                gaugeControl1.ResumeLayout(false);
                lParentControlAdd(tForm, gaugeControl1, ustItemName, ustItemType, row);

                gaugeControl1.SendToBack();
            }
            #endregion 

        }
                


        private void lEditPanel_Preparing(Form tForm, Control subView, DataSet ds_Layout, DataRow row, int pos)
        {
            tToolBox t = new tToolBox();

            int RefId = 0;
            int width = 0;
            int height = 0;
            int top = 0;
            int left = 0;
            int DockType = 0;

            string CmpName = string.Empty;
            string caption = string.Empty;
            string Prop_View = string.Empty;
            string TableIPCode = string.Empty;
            string fieldName = string.Empty;
            string tcolumntype = string.Empty;
            string tdisplayformat = "";
            string layout_code = string.Empty;

            string ustItemType = string.Empty;
            string ustHesapRefId = string.Empty;
            string ustHesapItemName = string.Empty;
            string ustItemName = string.Empty;
            DataRow UstHesapRow = null;

            string s1 = "=ROW_PROP_VIEWS:";
            string s2 = (char)34 + "EDITPANEL" + (char)34 + ": {";
            //"EDITPANEL": {

            RefId = t.myInt32(row["REF_ID"].ToString());
            caption = t.Set(row["LAYOUT_CAPTION"].ToString(), "", "");
            layout_code = t.Set(row["LAYOUT_CODE"].ToString(), "", "");
            TableIPCode = t.Set(row["TABLEIPCODE"].ToString(), "", "");
            fieldName = t.Set(row["FIELD_NAME"].ToString(), "", "");
            CmpName = t.Set(row["CMP_NAME"].ToString(), "", "");

            width = t.myInt32(row["CMP_WIDTH"].ToString());
            height = t.myInt32(row["CMP_HEIGHT"].ToString());
            top = t.myInt32(row["CMP_TOP"].ToString());
            left = t.myInt32(row["CMP_LEFT"].ToString());

            DockType = t.Set(row["CMP_DOCK"].ToString(), v.dock_Fill.ToString(), (int)0);

            Prop_View = t.Set(row["PROP_VIEWS"].ToString(), "", "");

            // default
            tcolumntype = "MemoEdit";

            // old
            if (Prop_View.IndexOf(s1) > -1)
                tcolumntype = t.Set(t.MyProperties_Get(Prop_View, "EP_CMP_TYPE:"), "MemoEdit", "");
            // json 
            if (Prop_View.IndexOf(s2) > -1)
                tcolumntype = t.Set(t.Find_Properties_Value(Prop_View, "EP_CMP_TYPE"), "MemoEdit", "");

            width = t.myInt32(row["CMP_WIDTH"].ToString());

            UstHesapRow = UstHesap_Get(ds_Layout, pos);

            #region // Ust hesabı var ise

            if (UstHesapRow != null)
            {
                tDevColumn dc = new tDevColumn();

                ustItemType = UstHesapRow["LAYOUT_TYPE"].ToString();
                //ustHesapRefId = UstHesapRow["REF_ID"].ToString();
                ustHesapRefId = UstHesapRow["LAYOUT_CODE"].ToString();
                t.Str_Replace(ref ustHesapRefId, ".", "_");
                ustItemName = v.lyt_Name + ustHesapRefId;

                DevExpress.XtraEditors.PanelControl panelControl1 = new DevExpress.XtraEditors.PanelControl();
                DevExpress.XtraEditors.LabelControl labelControl1 = new DevExpress.XtraEditors.LabelControl();
                ((System.ComponentModel.ISupportInitialize)(panelControl1)).BeginInit();
                panelControl1.SuspendLayout();
                // 
                // panelControl1
                // 
                //panelControl1.Controls.Add(textEdit1);
                //panelControl1.Controls.Add(labelControl1);
                panelControl1.Location = new System.Drawing.Point(0, 0);
                panelControl1.Name = v.lyt_Name + t.Str_Replace(ref layout_code, ".", "_");  //RefId.ToString();
                if (t.IsNotNull(CmpName))
                    panelControl1.Name = CmpName;
                panelControl1.Padding = new System.Windows.Forms.Padding(2);
                panelControl1.Size = new System.Drawing.Size(200, 40);
                panelControl1.TabIndex = 0;
                // 
                // labelControl1
                // 
                labelControl1.Dock = System.Windows.Forms.DockStyle.Left;
                labelControl1.Location = new System.Drawing.Point(6, 6);
                labelControl1.Name = "labelControl_" + RefId.ToString();
                labelControl1.Size = new System.Drawing.Size(100, 13);
                labelControl1.TabIndex = 0;
                labelControl1.Text = caption;

                if (width > 0) labelControl1.Size = new System.Drawing.Size(width, 13);

                if (t.IsNotNull(TableIPCode) == false)
                    TableIPCode = "null";
                if (t.IsNotNull(fieldName) == false)
                    fieldName = "null";

                DataSet dsData = null;

                if ((t.IsNotNull(TableIPCode)) && (t.IsNotNull(fieldName)))
                {
                    dsData = new DataSet();
                    dsData = t.Find_DataSet(tForm, "", TableIPCode, "");
                }
                else
                {
                    labelControl1.Dock = DockStyle.Fill;
                }

                panelControl1.AccessibleName = TableIPCode;
                panelControl1.AccessibleDescription = fieldName;

                if (dsData != null)
                    dc.tXtraEditors_Edit(null, dsData, null, panelControl1, tcolumntype, tdisplayformat, "", 1, 0, ""); // "" = FormName
                else dc.tXtraEditors_Edit(null, null, null, panelControl1, tcolumntype, tdisplayformat, "", 1, 0, ""); // "" = FormName

                // paneli içine yeni create edilen editin Dock ayarlanıyor
                if (panelControl1.Controls.Count > 0)
                    panelControl1.Controls[0].Dock = DockStyle.Fill;
                // panele label yerleştiriliyor
                panelControl1.Controls.Add(labelControl1);

                if (caption == "") labelControl1.Visible = false;

                if (DockType == v.dock_Bottom) panelControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
                if (DockType == v.dock_Fill) panelControl1.Dock = System.Windows.Forms.DockStyle.Fill;
                if (DockType == v.dock_Left) panelControl1.Dock = System.Windows.Forms.DockStyle.Left;
                if (DockType == v.dock_Right) panelControl1.Dock = System.Windows.Forms.DockStyle.Right;
                if (DockType == v.dock_Top) panelControl1.Dock = System.Windows.Forms.DockStyle.Top;

                t.myControl_Size_And_Location(panelControl1, width, height, left, top);

                ((System.ComponentModel.ISupportInitialize)(panelControl1)).EndInit();
                panelControl1.ResumeLayout(false);
                lParentControlAdd(tForm, panelControl1, ustItemName, ustItemType, row);

                panelControl1.SendToBack();
            }
            #endregion 

        }

        private void lLabelControl_Preparing(Form tForm, Control subView, DataSet ds_Layout, DataRow row, int pos)
        {
            tToolBox t = new tToolBox();

            int RefId = 0;
            string CmpName = string.Empty;
            string caption = string.Empty;

            string TableIPCode = string.Empty;
            string fieldName = string.Empty;

            int height = 0;
            int width = 0;
            int top = 0;
            int left = 0;
            int DockType = 0;
            float font_size = 0;

            string ustItemType = string.Empty;
            string ustHesapRefId = string.Empty;
            string ustHesapItemName = string.Empty;
            string ustItemName = string.Empty;
            DataRow UstHesapRow = null;

            RefId = t.myInt32(row["REF_ID"].ToString());
            caption = t.Set(row["LAYOUT_CAPTION"].ToString(), "", "");
            TableIPCode = t.Set(row["TABLEIPCODE"].ToString(), "", "");
            fieldName = t.Set(row["FIELD_NAME"].ToString(), "", "");
            CmpName = t.Set(row["CMP_NAME"].ToString(), "", "");

            DockType = t.Set(row["CMP_DOCK"].ToString(), v.dock_Fill.ToString(), (int)0);
            height = t.myInt32(row["CMP_HEIGHT"].ToString());
            width = t.myInt32(row["CMP_WIDTH"].ToString());
            top = t.myInt32(row["CMP_TOP"].ToString());
            left = t.myInt32(row["CMP_LEFT"].ToString());
            font_size = t.Set(row["CMP_FONT_SIZE"].ToString(), "", (float)0);

            UstHesapRow = UstHesap_Get(ds_Layout, pos);

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

                DevExpress.XtraEditors.LabelControl labelControl1 = new DevExpress.XtraEditors.LabelControl();
                // 
                // labelControl1
                // 
                //labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
                //labelControl1.Dock = System.Windows.Forms.DockStyle.Left;
                labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Vertical;
                labelControl1.Location = new System.Drawing.Point(0, 0);
                labelControl1.Name = "labelControl_" + RefId.ToString();
                if (t.IsNotNull(CmpName))
                    labelControl1.Name = CmpName;
                labelControl1.Size = new System.Drawing.Size(150, 28);
                labelControl1.TabIndex = 0;
                labelControl1.Text = caption;

                t.myControl_Size_And_Location(labelControl1, width, height, left, top);

                if (DockType == v.dock_Bottom) labelControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
                if (DockType == v.dock_Fill) labelControl1.Dock = System.Windows.Forms.DockStyle.Fill;
                if (DockType == v.dock_Left) labelControl1.Dock = System.Windows.Forms.DockStyle.Left;
                if (DockType == v.dock_Right) labelControl1.Dock = System.Windows.Forms.DockStyle.Right;
                if (DockType == v.dock_Top) labelControl1.Dock = System.Windows.Forms.DockStyle.Top;

                if (DockType != v.dock_None) labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Vertical;

                if (font_size > 0)
                {
                    font_size = font_size + .25F;

                    if (font_size > .25)
                    {
                        labelControl1.Appearance.Font = new System.Drawing.Font("Tahoma", font_size, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
                    }
                }

                DataSet dsData = null;

                if ((TableIPCode != "") && (fieldName != ""))
                {
                    try
                    {
                        dsData = t.Find_DataSet(tForm, "", TableIPCode, "");

                        if (dsData != null)
                            labelControl1.DataBindings.Add(new Binding("Text", dsData.Tables[0], fieldName));
                    }
                    catch (Exception)
                    {
                        //throw;
                    }
                }
                else
                {
                    labelControl1.Dock = DockStyle.Fill;
                }

                lParentControlAdd(tForm, labelControl1, ustItemName, ustItemType, row);
            }

            #endregion
        }

        /// ComponentControl, BarcodControl 
        #region ComponentControl, BarcodControl
        public void lComponentControl_Preparing(Form tForm, Control subView, DataSet ds_Layout, DataRow row, int pos)
        {
            tToolBox t = new tToolBox();

            string TableIPCode = string.Empty;
            string CmpName = string.Empty;
            string layout_code = string.Empty;
            string fieldName = string.Empty;
            //byte IPDataType = 1;
            int RefId = 0;
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

            RefId = t.myInt32(row["REF_ID"].ToString());

            // aslında TABLEIPCODE yerine başka bir MS_LAYOUT.MasterCode geliyor 
            TableIPCode = t.Set(row["TABLEIPCODE"].ToString(), "", "");

            fieldName = t.Set(row["FIELD_NAME"].ToString(), "", "");
            layout_code = t.Set(row["LAYOUT_CODE"].ToString(), "", "");

            width = t.myInt32(row["CMP_WIDTH"].ToString());
            height = t.myInt32(row["CMP_HEIGHT"].ToString());
            top = t.myInt32(row["CMP_TOP"].ToString());
            left = t.myInt32(row["CMP_LEFT"].ToString());

            FrontBack = t.myInt32(row["CMP_FRONT_BACK"].ToString());
            DockType = t.Set(row["CMP_DOCK"].ToString(), v.dock_Fill.ToString(), (int)0);
            CmpName = t.Set(row["CMP_NAME"].ToString(), "", "");


            //
            //
            //
            DevExpress.XtraEditors.PanelControl panelControl1 = new DevExpress.XtraEditors.PanelControl();
            ((System.ComponentModel.ISupportInitialize)(panelControl1)).BeginInit();

            UstHesapRow = UstHesap_Get(ds_Layout, pos);

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

                lParentControlAdd(tForm, panelControl1, ustItemName, ustItemType, row);
            }
            else
            {
                if (subView == null)
                    tForm.Controls.Add(panelControl1);
                else subView.Controls.Add(panelControl1);
            }

            #endregion

            // 
            // panelControl1
            // 
            if (FrontBack == 2) panelControl1.SendToBack();
            else panelControl1.BringToFront();

            panelControl1.Location = new System.Drawing.Point(0, 0);
            panelControl1.Name = v.lyt_Name + t.Str_Replace(ref layout_code, ".", "_");  //RefId.ToString();
            if (t.IsNotNull(CmpName))
                panelControl1.Name = CmpName;
            panelControl1.Size = new System.Drawing.Size(200, 435);
            //panelControl1.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

            if (DockType == v.dock_Bottom) panelControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
            if (DockType == v.dock_Fill) panelControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            if (DockType == v.dock_Left) panelControl1.Dock = System.Windows.Forms.DockStyle.Left;
            if (DockType == v.dock_Right) panelControl1.Dock = System.Windows.Forms.DockStyle.Right;
            if (DockType == v.dock_Top) panelControl1.Dock = System.Windows.Forms.DockStyle.Top;

            t.myControl_Size_And_Location(panelControl1, width, height, left, top);

            ((System.ComponentModel.ISupportInitialize)(panelControl1)).EndInit();


            //
            // InputPanel Create
            //
            //InputPanel_Preparing(tForm, panelControl1, TableIPCode, IPDataType);

            string FormCode = TableIPCode;

            Create_Layout(tForm, FormCode, panelControl1);


            panelControl1.TabIndex = pos;

        }
        public void lBarcodControl1_Preparing(Form tForm, Control subView, DataSet ds_Layout, DataRow row, int pos)
        {
            tToolBox t = new tToolBox();
            tEvents ev = new tEvents();

            string caption = string.Empty;
            string TableIPCode = string.Empty;
            string layout_code = string.Empty;

            int RefId = 0;
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

            RefId = t.myInt32(row["REF_ID"].ToString());
            caption = t.Set(row["LAYOUT_CAPTION"].ToString(), "", "");
            TableIPCode = t.Set(row["TABLEIPCODE"].ToString(), "", "");
            layout_code = t.Set(row["LAYOUT_CODE"].ToString(), "", "");

            width = t.myInt32(row["CMP_WIDTH"].ToString());
            height = t.myInt32(row["CMP_HEIGHT"].ToString());
            top = t.myInt32(row["CMP_TOP"].ToString());
            left = t.myInt32(row["CMP_LEFT"].ToString());

            FrontBack = t.myInt32(row["CMP_FRONT_BACK"].ToString());
            DockType = t.Set(row["CMP_DOCK"].ToString(), v.dock_Fill.ToString(), (int)0);


            //
            // groupControl1
            //
            DevExpress.XtraEditors.GroupControl groupControl1 = new DevExpress.XtraEditors.GroupControl();
            ((System.ComponentModel.ISupportInitialize)(groupControl1)).BeginInit();

            UstHesapRow = UstHesap_Get(ds_Layout, pos);

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

                lParentControlAdd(tForm, groupControl1, ustItemName, ustItemType, row);
            }
            else
                tForm.Controls.Add(groupControl1);

            #endregion

            // 
            // groupControl1
            //
            if (FrontBack == 2) groupControl1.SendToBack();
            else groupControl1.BringToFront();

            groupControl1.Name = v.lyt_Name + t.Str_Replace(ref layout_code, ".", "_");
            groupControl1.Size = new System.Drawing.Size(312, 74);
            groupControl1.TabIndex = 0;
            groupControl1.Text = "Barkod okuma alanı";
            groupControl1.Location = new System.Drawing.Point(462, 422);
            
            TableLayoutPanel tTableLayoutPanel = new TableLayoutPanel();
            groupControl1.Controls.Add(tTableLayoutPanel);

            tTableLayoutPanel.ColumnCount = 3;
            tTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 67F));
            tTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            tTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tTableLayoutPanel.RowCount = 1;
            tTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            tTableLayoutPanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;// Single;

            DevExpress.XtraEditors.TextEdit tEditMiktar = new DevExpress.XtraEditors.TextEdit();
            tEditMiktar.Name = "tEditMiktar";
            tEditMiktar.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            tEditMiktar.Properties.Appearance.Options.UseFont = true;
            tEditMiktar.Properties.Appearance.Options.UseTextOptions = true;
            tEditMiktar.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            tEditMiktar.Properties.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            tEditMiktar.Properties.ReadOnly = true;
            //tEditMiktar.Size = new System.Drawing.Size(61, 46);
            tEditMiktar.TabIndex = 0;
            tEditMiktar.TabStop = false;
            tEditMiktar.Dock = System.Windows.Forms.DockStyle.Bottom;
            tEditMiktar.EditValue = "1";
            tEditMiktar.Enter += new System.EventHandler(ev.tXtraEdit_Enter);
            tEditMiktar.Leave += new System.EventHandler(ev.tXtraEdit_Leave);


            DevExpress.XtraEditors.TextEdit tTextEditX = new DevExpress.XtraEditors.TextEdit();
            tTextEditX.Name = "tTextEditX";
            //tTextEditX.Properties.Appearance.BackColor = System.Drawing.Color.White;
            //tTextEditX.Properties.Appearance.Options.UseBackColor = true;
            tTextEditX.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            tTextEditX.Properties.Appearance.Options.UseFont = true;
            tTextEditX.Properties.Appearance.Options.UseTextOptions = true;
            tTextEditX.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            tTextEditX.Properties.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            tTextEditX.Properties.ReadOnly = true;
            //tTextEditX.Size = new System.Drawing.Size(26, 26);
            tTextEditX.TabIndex = 1;
            tTextEditX.TabStop = false;
            tTextEditX.Dock = System.Windows.Forms.DockStyle.Bottom;
            tTextEditX.EditValue = "X";
            tTextEditX.Enter += new System.EventHandler(ev.tXtraEdit_Enter);
            tTextEditX.Leave += new System.EventHandler(ev.tXtraEdit_Leave);

            DevExpress.XtraEditors.TextEdit tTextEditBarcod = new DevExpress.XtraEditors.TextEdit();
            tTextEditBarcod.Name = "tTextEditBarcod";
            tTextEditBarcod.Dock = System.Windows.Forms.DockStyle.Bottom;
            tTextEditBarcod.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            tTextEditBarcod.Properties.Appearance.Options.UseFont = true;
            tTextEditBarcod.TabIndex = 2;
            //tTextEditBarcod.Properties.AccessibleName = Pro_Code;
            //tTextEditBarcod.KeyDown += new System.Windows.Forms.KeyEventHandler(myBarcodeEdit_KeyDown);
            tTextEditBarcod.KeyUp += new System.Windows.Forms.KeyEventHandler(ev.myBarcodeEdit_KeyUp);
            tTextEditBarcod.Enter += new System.EventHandler(ev.tXtraEdit_Enter);
            tTextEditBarcod.Leave += new System.EventHandler(ev.tXtraEdit_Leave);

            tTableLayoutPanel.Controls.Add(tEditMiktar, 0, 0);
            tTableLayoutPanel.Controls.Add(tTextEditX, 1, 0);
            tTableLayoutPanel.Controls.Add(tTextEditBarcod, 2, 0);

            if (DockType == v.dock_Bottom) groupControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
            if (DockType == v.dock_Fill) groupControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            if (DockType == v.dock_Left) groupControl1.Dock = System.Windows.Forms.DockStyle.Left;
            if (DockType == v.dock_Right) groupControl1.Dock = System.Windows.Forms.DockStyle.Right;
            if (DockType == v.dock_Top) groupControl1.Dock = System.Windows.Forms.DockStyle.Top;

            t.myControl_Size_And_Location(groupControl1, width, height, left, top);

            ((System.ComponentModel.ISupportInitialize)(groupControl1)).EndInit();
        }
        private void myBarcodeEdit_KeyDown(object sender, KeyEventArgs e)
        {
            int l1 = 0;
            if (((DevExpress.XtraEditors.TextEdit)sender).EditValue != null)
                l1 = ((DevExpress.XtraEditors.TextEdit)sender).EditValue.ToString().Length;
            //Keys.Decimal   //Keys.Subtract   //Keys.Divide  //Keys.Add  //Keys.Enter  //Keys.MulKeytiply
            if ((e.KeyCode < Keys.NumPad0) &&
                (e.KeyCode > Keys.NumPad9) &&
                (l1 > 0) && (l1 < 5))
            {
                MessageBox.Show("00");
            }

            /*
            if (e.KeyCode == Keys.Return)
            {
                string Pro_Code = ((TextEdit)sender).Properties.AccessibleName;
                string Value = ((TextEdit)sender).Text;
                string line = string.Empty;
                string line_caption = string.Empty;

                if (Value != string.Empty)
                {
                    //-----

                    Form tForm = DevExpCC.tForm_Find(sender);

                    tp.MyProperties_Set(ref line, ref line_caption, "root", "BARCOD.GET", Value);

                    tp.MyMemo_Set(tForm, line_caption, line);

                    //-----

                    //tPro.tProcedure_RUN(Pro_Code, Value);
                    tPro.tProcedure_RUN(tForm, Pro_Code);

                    ((TextEdit)sender).Text = string.Empty;
                }
            }
            */
        }
        private void lSimpleButton_Preparing(Form tForm, Control subView, DataSet ds_Layout, DataRow row, int pos)
        {
            tToolBox t = new tToolBox();
            tEvents ev = new tEvents();

            string TableIPCode = string.Empty;
            string CmpName = string.Empty;
            string layout_code = string.Empty;
            string fieldName = string.Empty;
            //byte IPDataType = 1;
            int RefId = 0;
            int width = 0;
            int height = 0;
            int top = 0;
            int left = 0;
            int DockType = 0;
            int FrontBack = 0;

            string caption = string.Empty;
            string ustItemType = string.Empty;
            string ustHesapRefId = string.Empty;
            string ustHesapItemName = string.Empty;
            string ustItemName = string.Empty;
            DataRow UstHesapRow = null;

            RefId = t.myInt32(row["REF_ID"].ToString());

            // aslında TABLEIPCODE yerine başka bir MS_LAYOUT.MasterCode geliyor 
            TableIPCode = t.Set(row["TABLEIPCODE"].ToString(), "", "");
            caption = row["LAYOUT_CAPTION"].ToString();

            fieldName = t.Set(row["FIELD_NAME"].ToString(), "", "");
            layout_code = t.Set(row["LAYOUT_CODE"].ToString(), "", "");

            width = t.myInt32(row["CMP_WIDTH"].ToString());
            height = t.myInt32(row["CMP_HEIGHT"].ToString());
            top = t.myInt32(row["CMP_TOP"].ToString());
            left = t.myInt32(row["CMP_LEFT"].ToString());

            FrontBack = t.myInt32(row["CMP_FRONT_BACK"].ToString());
            DockType = t.Set(row["CMP_DOCK"].ToString(), v.dock_Fill.ToString(), (int)0);
            CmpName = t.Set(row["CMP_NAME"].ToString(), "", "");


            //
            //
            //
            DevExpress.XtraEditors.SimpleButton simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            //
            // simpleButton1
            //
            simpleButton1.Click += new System.EventHandler(ev.btn_Number_Click);
            //simpleButton1.AccessibleName = TableIPCode; şimdilik ihtiyaç yok
            simpleButton1.AccessibleDescription = fieldName;
            simpleButton1.Location = new System.Drawing.Point(0, 0);
            simpleButton1.Text = caption;
            simpleButton1.Name = v.lyt_Name + t.Str_Replace(ref layout_code, ".", "_");  //RefId.ToString();
            if (t.IsNotNull(CmpName))
                simpleButton1.Name = CmpName;
            simpleButton1.Size = new System.Drawing.Size(20, 20);
            simpleButton1.TabStop = false;

            if (DockType == v.dock_Bottom) simpleButton1.Dock = System.Windows.Forms.DockStyle.Bottom;
            if (DockType == v.dock_Fill) simpleButton1.Dock = System.Windows.Forms.DockStyle.Fill;
            if (DockType == v.dock_Left) simpleButton1.Dock = System.Windows.Forms.DockStyle.Left;
            if (DockType == v.dock_Right) simpleButton1.Dock = System.Windows.Forms.DockStyle.Right;
            if (DockType == v.dock_Top) simpleButton1.Dock = System.Windows.Forms.DockStyle.Top;

            UstHesapRow = UstHesap_Get(ds_Layout, pos);

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

                lParentControlAdd(tForm, simpleButton1, ustItemName, ustItemType, row);
            }
            else
            {
                if (subView == null)
                    tForm.Controls.Add(simpleButton1);
                else subView.Controls.Add(simpleButton1);
            }

            #endregion
        }


        #endregion BarcodControl


        #endregion Order Panels

        #region Create SubLayout

        // yeni oluşturulan controlun üst hesabı var mı ?
        //
        private DataRow UstHesap_Get(DataSet ds_Layout, int pos)
        {
            int itemCount = ds_Layout.Tables[0].Rows.Count;

            if (itemCount <= pos) return null;

            DataRow UstHesap = null;
            string ParentCode = ds_Layout.Tables[0].Rows[pos]["PARENT_CODE"].ToString();
            string ItemCode = string.Empty;

            if (ParentCode == "") return null;

            for (int i = 0; i < itemCount; i++)
            {
                ItemCode = ds_Layout.Tables[0].Rows[i]["LAYOUT_CODE"].ToString();

                if (ParentCode == ItemCode)
                {
                    UstHesap = ds_Layout.Tables[0].Rows[i] as DataRow;
                    break;
                }
            }

            return UstHesap;
        }

        // yeni oluşturulan controlu üst controla veya formun üzerine eklenmesi (ParentControl)
        //
        private Control lParentControlAdd(Form tForm, Control newControl,
            string ustControlName, string ustControlType, DataRow row)
        {
            tToolBox t = new tToolBox();

            string[] controls = new string[] { };
            Control c = t.Find_Control(tForm, ustControlName, "", controls);

            if (c != null)
            {
                #region TableLayoutPanel
                if (c.ToString().IndexOf("System.Windows.Forms.TableLayoutPanel") > -1)
                {
                    int colNo = 0;
                    int rowNo = 0;
                    int colSpan = 0;
                    int rowSpan = 0;

                    string s1 = "=ROW_PROP_VIEWS:";
                    string s2 = (char)34 + "TLP" + (char)34 + ": {";
                    //"TLP": {

                    string Prop_Views = t.Set(row["PROP_VIEWS"].ToString(), "", "");

                    if (Prop_Views.IndexOf(s1) > -1)
                    {
                        colNo = t.myInt32(t.Set(t.MyProperties_Get(Prop_Views, "TLP_COLNO:"), "-1", "-1"));
                        rowNo = t.myInt32(t.Set(t.MyProperties_Get(Prop_Views, "TLP_ROWNO:"), "-1", "-1"));
                        colSpan = t.myInt32(t.Set(t.MyProperties_Get(Prop_Views, "TLP_COLSPAN:"), "-1", "-1"));
                        rowSpan = t.myInt32(t.Set(t.MyProperties_Get(Prop_Views, "TLP_ROWSPAN:"), "-1", "-1"));
                    }

                    if (Prop_Views.IndexOf(s2) > -1)
                    {
                        /*
                        PROP_VIEWS_LAYOUT packet = new PROP_VIEWS_LAYOUT();
                        Prop_Views = Prop_Views.Replace((char)34, (char)39);
                        //var prop_ = JsonConvert.DeserializeAnonymousType(Prop_Views, packet);
                        */
                        PROP_VIEWS_LAYOUT prop_ = t.readProp<PROP_VIEWS_LAYOUT>(Prop_Views);

                        colNo = t.myInt32(t.Set(prop_.TLP.TLP_COLNO.ToString(), "-1", "-1"));
                        rowNo = t.myInt32(t.Set(prop_.TLP.TLP_ROWNO.ToString(), "-1", "-1"));
                        colSpan = t.myInt32(t.Set(prop_.TLP.TLP_COLSPAN.ToString(), "-1", "-1"));
                        rowSpan = t.myInt32(t.Set(prop_.TLP.TLP_ROWSPAN.ToString(), "-1", "-1"));
                    }

                    if ((colNo > -1) && (rowNo > -1))
                    {
                        ((System.Windows.Forms.TableLayoutPanel)c).Controls.Add(newControl, colNo, rowNo);
                    }
                    else
                        MessageBox.Show("Dikkat : " + row["LAYOUT_CODE"].ToString() + " için Parent ( Col No / Row No ) seçimi yapılmamış.");

                    // this.tableLayoutPanel1.SetColumnSpan(this.groupControl2, 2);
                    if (colSpan > 0)
                        ((System.Windows.Forms.TableLayoutPanel)c).SetColumnSpan(newControl, colSpan);
                    if (rowSpan > 0)
                        ((System.Windows.Forms.TableLayoutPanel)c).SetRowSpan(newControl, rowSpan);

                    //tableLayoutPanel1.BackColor = System.Drawing.Color.Black;
                    ((System.Windows.Forms.TableLayoutPanel)c).BackColor = newControl.BackColor;
                    return null;
                }
                #endregion TableLayoutPanel

                #region SplitContainer
                if (c.ToString() == "System.Windows.Forms.SplitContainer")
                {
                    string Prop_View = t.Set(row["PROP_VIEWS"].ToString(), "", "");
                    string ParentPanel = t.MyProperties_Get(Prop_View, "SC_PARENTPANEL:");
                    if (ParentPanel == "")
                    {
                        MessageBox.Show("Dikkat : " + row["LAYOUT_CODE"].ToString() + " için ParentPanel (Panel1/Panel2) seçimi yapılmamış.");
                        return null;
                    }
                    if (ParentPanel == "Panel1")
                        ((System.Windows.Forms.SplitContainer)c).Panel1.Controls.Add(newControl);
                    if (ParentPanel == "Panel2")
                        ((System.Windows.Forms.SplitContainer)c).Panel2.Controls.Add(newControl);

                    return null;
                }
                #endregion

                #region TabPane, TabNavigationPage
                if (c.GetType().ToString() == "DevExpress.XtraBars.Navigation.TabPane")
                {
                    ((System.ComponentModel.ISupportInitialize)(((DevExpress.XtraBars.Navigation.TabPane)c))).BeginInit();

                    ((DevExpress.XtraBars.Navigation.TabPane)c).Controls.Add(newControl);
                    ((DevExpress.XtraBars.Navigation.TabPane)c).Pages.Add((DevExpress.XtraBars.Navigation.TabNavigationPage)newControl);

                    ((System.ComponentModel.ISupportInitialize)(((DevExpress.XtraBars.Navigation.TabPane)c))).EndInit();

                    return null;
                }

                if (c.GetType().ToString() == "DevExpress.XtraBars.Navigation.TabNavigationPage")
                {
                    ((DevExpress.XtraBars.Navigation.TabNavigationPage)c).Controls.Add(newControl);

                    return null;
                }
                #endregion TabPane, TabNavigationPage

                #region NavigationPane, NavigationPage
                if (c.GetType().ToString() == "DevExpress.XtraBars.Navigation.NavigationPane")
                {
                    ((System.ComponentModel.ISupportInitialize)(((DevExpress.XtraBars.Navigation.NavigationPane)c))).BeginInit();

                    ((DevExpress.XtraBars.Navigation.NavigationPane)c).Controls.Add(newControl);
                    ((DevExpress.XtraBars.Navigation.NavigationPane)c).Pages.Add((DevExpress.XtraBars.Navigation.NavigationPage)newControl);

                    ((System.ComponentModel.ISupportInitialize)(((DevExpress.XtraBars.Navigation.NavigationPane)c))).EndInit();

                    return null;
                }

                if (c.GetType().ToString() == "DevExpress.XtraBars.Navigation.NavigationPage")
                {
                    ((DevExpress.XtraBars.Navigation.NavigationPage)c).Controls.Add(newControl);

                    return null;
                }
                #endregion NavigationPane, NavigationPage

                #region XtraTabControl, XtraTabPage
                if (c.GetType().ToString() == "DevExpress.XtraTab.XtraTabControl")
                {
                    ((System.ComponentModel.ISupportInitialize)(((DevExpress.XtraTab.XtraTabControl)c))).BeginInit();

                    ((DevExpress.XtraTab.XtraTabControl)c).TabPages.Add((DevExpress.XtraTab.XtraTabPage)newControl);

                    ((System.ComponentModel.ISupportInitialize)(((DevExpress.XtraTab.XtraTabControl)c))).EndInit();

                    return null;
                }

                if (c.GetType().ToString() == "DevExpress.XtraTab.XtraTabPage")
                {
                    ((DevExpress.XtraTab.XtraTabPage)c).Controls.Add(newControl);

                    return null;
                }
                #endregion XtraTabControl, XtraTabPage

                #region BackstageViewControl
                if (c.GetType().ToString() == "DevExpress.XtraBars.Ribbon.BackstageViewControl")
                {
                    //
                    // Bu control tespit edilince hiç bir işlem yapılmadan çağırıldığı noktaya gönderiliyor.
                    // gittiği yerde işleme tabii tutulacak.
                    //
                    return c;
                }

                #endregion BackstageViewControl

                #region GroupControl
                if (c.GetType().ToString() == "DevExpress.XtraEditors.GroupControl")
                {
                    ((DevExpress.XtraEditors.GroupControl)c).Controls.Add(newControl);

                    return null;
                }
                #endregion GroupControl

                #region PanelControl
                if (c.GetType().ToString() == "DevExpress.XtraEditors.PanelControl")
                {
                    ((DevExpress.XtraEditors.PanelControl)c).Controls.Add(newControl);

                    return null;
                }
                #endregion PanelControl

                #region DockPanel
                if (c.ToString().IndexOf("DevExpress.XtraBars.Docking.DockPanel") > -1)
                {
                    ((DevExpress.XtraBars.Docking.DockPanel)c).Controls.Add(newControl);

                    return null;
                }
                #endregion DockPanel

            }

            #region  ustControlType == backstageViewTabItem

            if ((ustControlType.ToString() == v.lyt_backstageViewTabItem) &&
                (c != null))
            {
                // önce form üzerindeki BackstageViewControl tespit ediliyor

                Control cntrl = null;
                if (c.GetType().ToString() == "DevExpress.XtraBars.Ribbon.BackstageViewClientControl")
                {
                    cntrl = ((DevExpress.XtraBars.Ribbon.BackstageViewClientControl)c).Parent;
                }

                if (cntrl != null)
                {
                    foreach (var item2 in ((DevExpress.XtraBars.Ribbon.BackstageViewControl)cntrl).Controls)
                    {
                        if (item2.GetType().ToString() == "DevExpress.XtraBars.Ribbon.BackstageViewClientControl")
                        {
                            if (((DevExpress.XtraBars.Ribbon.BackstageViewClientControl)item2).Name.ToString() == ustControlName)
                            {
                                // Aranan bulundu ve yeni gelen control da bu BackstageViewClientControl üzerine ekleniyor
                                ((DevExpress.XtraBars.Ribbon.BackstageViewClientControl)item2).Controls.Add(newControl);
                                return null;
                            }
                        }
                    }
                }


                //foreach (var item1 in tForm.Controls)
                //{
                //    if (item1.GetType().ToString() == "DevExpress.XtraBars.Ribbon.BackstageViewControl")
                //    {
                //        // tespit edilen BackstageViewControl içindeki BackstageViewClientControl tespit edilecek
                //        foreach (var item2 in ((DevExpress.XtraBars.Ribbon.BackstageViewControl)item1).Controls)
                //        {
                //            if (item2.GetType().ToString() == "DevExpress.XtraBars.Ribbon.BackstageViewClientControl")
                //            {
                //                if (((DevExpress.XtraBars.Ribbon.BackstageViewClientControl)item2).Name.ToString() == ustControlName)
                //                {
                //                    // Aranan bulundu ve yeni gelen control da bu BackstageViewClientControl üzerine ekleniyor
                //                    ((DevExpress.XtraBars.Ribbon.BackstageViewClientControl)item2).Controls.Add(newControl);
                //                    return null;
                //                }
                //            }
                //        }
                //    }
                //}

                //MessageBox.Show("aaa");


                //foreach (var a in backstageViewControl1.Controls)
                //{
                //    if (a.GetType().ToString() == "DevExpress.XtraBars.Ribbon.BackstageViewClientControl")
                //    {
                //        MessageBox.Show(((DevExpress.XtraBars.Ribbon.BackstageViewClientControl)a).Name.ToString());

                //        foreach (var item in ((DevExpress.XtraBars.Ribbon.BackstageViewClientControl)a).Controls)
                //        {
                //            MessageBox.Show(item.ToString());
                //        }
                //    }
                //}

                //foreach (var item2 in backstageViewControl1.Items)
                //{
                //    MessageBox.Show(item2.ToString());
                //}
            }
            #endregion

            // genelde null dönüyor
            return c;
        }

        // DockManager in create işlemi
        //
        public DevExpress.XtraBars.Docking.DockManager Find_DockManager(Form tForm)
        {
            if (tForm == null) return null;

            DockManager manager = null;

            if (tForm.Container != null)
            {
                if (tForm.Container.Components != null)
                {
                    foreach (System.ComponentModel.IComponent component in tForm.Container.Components)
                    {
                        if (component is DockManager)
                            manager = (DockManager)component;
                    }
                }
            }
            else
            {
                System.ComponentModel.IContainer components = null;
                components = new System.ComponentModel.Container();

                manager = new DevExpress.XtraBars.Docking.DockManager(components);
                manager.Form = tForm;

                components.Add(manager);
            }

            //foreach (System.ComponentModel.ISupportInitialize component in tForm.Container.Components)
            //{
            //    if (component is DockManager)
            //        manager = (DockManager)component;
            //}


            if (manager != null)
            {
                return manager;
            }
            else return null;

            //t his.components = new System.ComponentModel.Container();
            //t his.dockManager1 = new DevExpress.XtraBars.Docking.DockManager(t his.components);
            //((System.ComponentModel.ISupportInitialize)(t his.dockManager1)).BeginInit();
            //t his.SuspendLayout();

        }

        // InputPanel_Preparing
        //
        private void InputPanel_Preparing_IPT(Form tForm, Control cntrl, string TableIPCode)
        {
            if (TableIPCode != string.Empty)
            {
                tInputPanel ip = new tInputPanel();
                ip.Create_InputPanel(tForm, cntrl, TableIPCode, v.IPdataType_DataView, true);
            }
        }

        private void InputPanel_Preparing(Form tForm, Control cntrl, string TableIPCode, byte IPDataType)
        {
            #region IP_VIEW_TYPE
            // * 1,  'Data View'
            // * 2,  'Kriter View'
            // * 3,  'Kategori View'
            // * 4,  'HGS View'
            // IPdataType_DataView = 1;
            // IPdataType_Kriterler = 2;
            // IPdataType_Kategori = 3;
            // IPdataType_HGSView = 4;
            #endregion IP_VIEW_TYPE

            if (TableIPCode != string.Empty)
            {
                TableIPCode = TableIPCode.Trim();
                tInputPanel ip = new tInputPanel();
                ip.Create_InputPanel(tForm, cntrl, TableIPCode, IPDataType, false);
            }
        }

        #endregion Create SubLayout

    } // public void Create_Layout
}
