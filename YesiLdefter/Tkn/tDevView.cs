using DevExpress.XtraCharts;
using DevExpress.XtraDataLayout;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Layout;
using DevExpress.XtraLayout;
using DevExpress.XtraLayout.Customization;
using DevExpress.XtraLayout.Utils;
using DevExpress.XtraTreeList;
using DevExpress.XtraVerticalGrid;
using DevExpress.XtraVerticalGrid.Rows;
using DevExpress.XtraScheduler;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using Tkn_CreateObject;
using Tkn_DevColumn;
using Tkn_Events;
using Tkn_ToolBox;
using Tkn_Variable;
using Tkn_DevExpImageHelper;

using System.Drawing;
using System.Threading.Tasks;
using DevExpress.Utils;
using DevExpress.Utils.DragDrop;
using DevExpress.Utils.Controls;
using DevExpress.Utils.Drawing;

namespace Tkn_DevView
{
    public class tDevView : tBase
    {
        #region All Views

        //1010, + 'GridView'
        //1020,  'BandedGridView'
        //1030, + 'AdvBandedGridView'
        //1040, + 'GridLayoutView'
        //1050,  'WinExplorerView'
        //2010, + 'VGridSingle'
        //2012, + 'VGridMulti'
        //2020, + 'TreeView'
        //2030,  'PivotGrid'
        //3010, + 'DataLayoutView'
        //4010,  'WizardControl'

        #region Create_ViewControls

        public Control Create_View(DataRow Row, string TableIPCode, string MultiPageID)
        {
            tToolBox t = new tToolBox();

            Control cntrl = null;

            int RefID = t.Set(Row["REF_ID"].ToString(), "", 0);
            int ViewType = t.Set(Row["VIEW_CMP_TYPE"].ToString(), "", v.obj_vw_GridView);
            string Prop_Runtime = t.Set(Row["PROP_RUNTIME"].ToString(), "", "");
            string Prop_Navigator = t.Set(Row["PROP_NAVIGATOR"].ToString(), "", "");

            if (
                (ViewType == v.obj_vw_GridView) ||
                (ViewType == v.obj_vw_BandedGridView) ||
                (ViewType == v.obj_vw_AdvBandedGridView) ||
                (ViewType == v.obj_vw_GridLayoutView) ||
                (ViewType == v.obj_vw_WinExplorerView) ||
                (ViewType == v.obj_vw_TileView) ||
                (ViewType == v.obj_vw_CardView) ||
                (ViewType == v.obj_vw_VGridSingle) ||
                (ViewType == v.obj_vw_VGridMulti) ||
                (ViewType == v.obj_vw_TreeListView) ||
                (ViewType == v.obj_vw_DataLayoutView) ||
                (ViewType == v.obj_vw_CalenderAndScheduler) ||
                (ViewType == v.obj_vw_ChartsView) ||
                (ViewType == v.obj_vw_WizardControl) ||
                (ViewType == v.obj_vw_HtmlEditorsView)
               )
            {
                // esas buradan dönüş yapılıyor
                cntrl = Create_View_(ViewType, RefID, TableIPCode, MultiPageID);

                // RunTime Sırasında yapılacak işlerin listesi
                if (t.IsNotNull(Prop_Runtime) || (t.IsNotNull(Prop_Navigator)))
                {
                    // propNavigator =
                    cntrl.AccessibleDescription = Prop_Runtime + v.ENTER + "|ds|" + v.ENTER + Prop_Navigator;
                }
            }

            return cntrl; // bu dönerse null döner 
        }

        public Control Create_View_(int ViewType, int RefID, string TableIPCode, string MultiPageID)
        {
            Control cntrl = null;
            tEvents ev = new tEvents();
            tEventsGrid evg = new tEventsGrid();

            #region Grid, BandedGrid, AdvBandedGrid, GridLayoutView, WinExplorerView

            if ((ViewType == v.obj_vw_GridView) ||
                (ViewType == v.obj_vw_BandedGridView) ||
                (ViewType == v.obj_vw_AdvBandedGridView) ||
                (ViewType == v.obj_vw_GridLayoutView) ||
                (ViewType == v.obj_vw_WinExplorerView) ||
                (ViewType == v.obj_vw_TileView) ||
                (ViewType == v.obj_vw_CardView)
                )
            {
                GridControl tGridControl = new GridControl();

                tGridControl.Name = "tGridControl_" + RefID.ToString();
                tGridControl.AccessibleName = TableIPCode + MultiPageID;
                tGridControl.Dock = DockStyle.Fill;

                tGridControl.AllowDrop = true;
                tGridControl.DragOver += new System.Windows.Forms.DragEventHandler(evg.myGridControl_DragOver);
                tGridControl.DragDrop += new System.Windows.Forms.DragEventHandler(evg.myGridControl_DragDrop);
                tGridControl.DragEnter += new System.Windows.Forms.DragEventHandler(evg.myGridControl_DragEnter);
                tGridControl.DragLeave += new System.EventHandler(evg.myGridControl_DragLeave);
                tGridControl.GiveFeedback += new System.Windows.Forms.GiveFeedbackEventHandler(evg.myGridControl_GiveFeedback);
                tGridControl.QueryContinueDrag += new System.Windows.Forms.QueryContinueDragEventHandler(evg.myGridControl_QueryContinueDrag);
                //tGridControl.Paint += new PaintEventHandler(evg.myGridControl_Paint);
                tGridControl.Enter += new System.EventHandler(evg.myGridControl_Enter);
                tGridControl.Leave += new System.EventHandler(evg.myGridControl_Leave);
                
                //this.gridControl1.DragOver += new System.Windows.Forms.DragEventHandler(this.gridControl1_DragOver);
                //this.gridControl1.DragLeave += new System.EventHandler(this.gridControl1_DragLeave);
                //this.gridControl1.GiveFeedback += new System.Windows.Forms.GiveFeedbackEventHandler(this.gridControl1_GiveFeedback);
                //this.gridControl1.QueryContinueDrag += new System.Windows.Forms.QueryContinueDragEventHandler(this.gridControl1_QueryContinueDrag);

                tGridControl.TabStop = true; // false; // şimdilik kapalı
                tGridControl.TabIndex = 100;   // arama paneli yüzünden yaptım

                return tGridControl; // <<<< dönen cntrl
            }

            #endregion // Grids

            #region VerticalGrid

            if ((ViewType == v.obj_vw_VGridSingle) ||
                (ViewType == v.obj_vw_VGridMulti))
            {
                DevExpress.XtraVerticalGrid.VGridControl tVGridControl =
                                  new DevExpress.XtraVerticalGrid.VGridControl();

                tVGridControl.BeginUpdate();

                tVGridControl.Cursor = System.Windows.Forms.Cursors.SizeWE;
                tVGridControl.Name = "tVGridControl_" + RefID.ToString();
                tVGridControl.AccessibleName = TableIPCode + MultiPageID;
                //tVGridControl.Controls.Clear();
                tVGridControl.Rows.Clear();
                tVGridControl.Dock = DockStyle.Fill;
                tVGridControl.AllowDrop = true;
                //tVGridControl.TreeButtonStyle = TreeButtonStyle.TreeView;

                //tVGridControl.RowHeaderWidth = 150;
                //tVGridControl.RecordWidth = 150;
                tVGridControl.LayoutStyle = DevExpress.XtraVerticalGrid.LayoutViewStyle.SingleRecordView;

                if (ViewType == v.obj_vw_VGridSingle)
                {
                    tVGridControl.LayoutStyle = DevExpress.XtraVerticalGrid.LayoutViewStyle.SingleRecordView;
                }
                if (ViewType == v.obj_vw_VGridMulti)
                {
                    tVGridControl.LayoutStyle = DevExpress.XtraVerticalGrid.LayoutViewStyle.MultiRecordView;
                    tVGridControl.RowHeaderWidth = 150;
                }

                //tVGridControl.ScrollsStyle.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Flat;
                tVGridControl.Appearance.Category.Font =
                     new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(1)));

                tVGridControl.ScrollVisibility = DevExpress.XtraVerticalGrid.ScrollVisibility.Auto;
                tVGridControl.OptionsView.AllowHtmlText = true;
                tVGridControl.HtmlImages = tImageHelper.GetGlyphs();

                /* renk caption başlığı için örnek kod
                 * 
    // Enable HTML Text Formatting and populate the image collection with glyphs.
    vGridControl.OptionsView.AllowHtmlText = true;
    vGridControl.HtmlImages = ImageHelper.GetGlyphs();

    // Assign the HypertextLabel editor to the Name row.
    BaseRow row = vGridControl.GetRowByFieldName("Name");
    row.Properties.RowEdit = new RepositoryItemHypertextLabel();

    //Enumerate records in the row, and update cell values.
    for(int i = 0; i < vGridControl.RecordCount; i++) {
        string value = (string)vGridControl.GetCellValue(row, i);
        vGridControl.SetCellValue(row, i, "<Image=" + value[0] + ">" + value);
    }

                */

                tVGridControl.Enter += new System.EventHandler(evg.myVGridControl_Enter);
                tVGridControl.Leave += new System.EventHandler(evg.myVGridControl_Leave);

                //DevExpress.XtraGrid.Views.Base
                tVGridControl.EndUpdate();

                return tVGridControl; // <<<< dönen cntrl
            }
            #endregion VerticalGrid

            #region TreeListView

            if (ViewType == v.obj_vw_TreeListView)
            {
                DevExpress.XtraTreeList.TreeList tTreeListView =
                    new DevExpress.XtraTreeList.TreeList();

                tTreeListView.Name = "tTreeListView_" + RefID.ToString();
                tTreeListView.AccessibleName = TableIPCode + MultiPageID;
                tTreeListView.Dock = DockStyle.Fill;
                tTreeListView.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
                tTreeListView.Location = new System.Drawing.Point(0, 0);
                //tTreeListView.OptionsBehavior.DragNodes = true;
                tTreeListView.OptionsView.AutoWidth = false;
                tTreeListView.OptionsBehavior.EnableFiltering = true;

                return tTreeListView; // <<<< dönen cntrl
            }

            #endregion TreeListView

            #region DataLayoutControl or HtmlEditorsView

            if ((ViewType == v.obj_vw_DataLayoutView) || (ViewType == v.obj_vw_HtmlEditorsView))
            {
                DevExpress.XtraDataLayout.DataLayoutControl tDataLayoutControl =
                                       new DevExpress.XtraDataLayout.DataLayoutControl();

                tDataLayoutControl.BeginUpdate();
                tDataLayoutControl.SuspendLayout();
                tDataLayoutControl.Name = "tDataLayoutControl" + RefID.ToString();
                // DİKKAT : burayı sakın açma, data çalışmaz oluyor
                // tDataLayoutControl.DataSource = dsData.Tables[0];
                // ****
                //tDataLayoutControl.Dock = DockStyle.Fill;
                tDataLayoutControl.AccessibleName = TableIPCode + MultiPageID;
                tDataLayoutControl.AutoScroll = false;
                //tDataLayoutControl.OptionsItemText.TextAlignMode = TextAlignMode.AlignInGroups;
                tDataLayoutControl.Root.Padding = new DevExpress.XtraLayout.Utils.Padding(0);
                tDataLayoutControl.Appearance.ControlFocused.BackColor = v.AppearanceFocusedColor;
                tDataLayoutControl.Appearance.ControlFocused.Options.UseBackColor = true;
                tDataLayoutControl.OptionsView.AllowHotTrack = true;
                tDataLayoutControl.OptionsView.DrawItemBorders = true;
                tDataLayoutControl.OptionsView.HighlightFocusedItem = true;
                tDataLayoutControl.OptionsView.DrawItemBorders = false;
                tDataLayoutControl.OptionsFocus.AllowFocusReadonlyEditors = false;
                tDataLayoutControl.OptionsFocus.MoveFocusDirection = MoveFocusDirection.DownThenAcross;

                tDataLayoutControl.Enter += new System.EventHandler(ev.myDataLayoutControl_Enter);
                tDataLayoutControl.Leave += new System.EventHandler(ev.myDataLayoutControl_Leave);

                //tDataLayoutControl.RegisterUserCustomizationForm(typeof(YesiLdefter.MSCustomization)); 

                DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1 =
                                   new DevExpress.XtraLayout.LayoutControlGroup();
                // 
                // layoutControlGroup1
                // 
                //layoutControlGroup1.BeginInit();
                //layoutControlGroup1.AppearanceItemCaption.BackColor = v.AppearanceItemCaptionColor1;
                //layoutControlGroup1.AppearanceItemCaption.BackColor2 = v.AppearanceItemCaptionColor2;
                //layoutControlGroup1.AppearanceItemCaption.Options.UseBackColor = true;

                layoutControlGroup1.Enabled = true;
                layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
                layoutControlGroup1.GroupBordersVisible = true;
                layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
                layoutControlGroup1.Name = "LCGroup_" + RefID.ToString();
                layoutControlGroup1.Text = "LCGroup_" + RefID.ToString();
                layoutControlGroup1.TextVisible = false;
                layoutControlGroup1.Size = new System.Drawing.Size(200, 200);
                layoutControlGroup1.DoubleClick += new System.EventHandler(ev.layoutControlGroup_DoubleClick);
                //layoutControlGroup1.Padding = new DevExpress.XtraLayout.Utils.Padding(0);

                tDataLayoutControl.Root = layoutControlGroup1;

                //tDataLayoutControl.Root.Add(layoutControlGroup1);

                return tDataLayoutControl;
            }

            #endregion DataLayoutControl

            #region WizardControl
            if (ViewType == v.obj_vw_WizardControl)
            {
                DevExpress.XtraWizard.WizardControl tWizardControl = new DevExpress.XtraWizard.WizardControl();

                tWizardControl.Name = "tWizardControl" + RefID.ToString();
                //tWizardControl.DataSource = dsData.Tables[0];
                tWizardControl.Dock = DockStyle.Fill;
                tWizardControl.WizardStyle = DevExpress.XtraWizard.WizardStyle.WizardAero;
                //tWizardControl.AccessibleDefaultActionDescription = ClassFieldName;
                tWizardControl.AccessibleName = TableIPCode + MultiPageID;

                return tWizardControl;
            }
            #endregion

            #region CalenderAndScheduler

            if (ViewType == v.obj_vw_CalenderAndScheduler)
            {
                DevExpress.XtraScheduler.SchedulerControl tSchedulerControl =
                    new DevExpress.XtraScheduler.SchedulerControl();

                tSchedulerControl.Name = "tSchedulerView_" + RefID.ToString();
                tSchedulerControl.AccessibleName = TableIPCode + MultiPageID;
                tSchedulerControl.Dock = DockStyle.Fill;
                tSchedulerControl.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
                tSchedulerControl.Location = new System.Drawing.Point(0, 0);
                return tSchedulerControl; // <<<< dönen cntrl
            }

            #endregion CalenderAndScheduler

            #region Charts

            if (ViewType == v.obj_vw_ChartsView)
            {
                DevExpress.XtraCharts.ChartControl tChartView = new DevExpress.XtraCharts.ChartControl();
                tChartView.Name = "tChartView_" + RefID.ToString();
                tChartView.AccessibleName = TableIPCode + MultiPageID;
                tChartView.Dock = DockStyle.Fill;
                //tChartView.  BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
                tChartView.Location = new System.Drawing.Point(0, 0);
                return tChartView;
            }

            #endregion Charts

            return cntrl; // buradan dönüyorsa null döner
        }

        #endregion Create_ViewControls

        //---        

        #region GridView Create Base
        public void tGridView_Create(DataRow row_Table, DataSet dsFields, DataSet dsData,
            GridControl tGridControl, string TableIPCode, ref bool tabStop)
        {
            tToolBox t = new tToolBox();
            tEventsGrid evg = new tEventsGrid();

            // Tablo hakkındaki bilgiler
            //DataRow row_Table = dsTable.Tables[0].Rows[0];

            #region GridView ve Columns İşlemleri

            GridView tGridView = null;

            // Gelen GridControlu içinde GridView yok ise yeni bir View ekleyelim
            // var ise olanı kullanalım
            if (tGridControl.MainView == null)
            {
                int RefId = t.Set(row_Table[0].ToString(), "", 0);
                tGridView = new GridView(tGridControl);
                tGridView.Name = "tGridView_" + RefId.ToString();
                tGridControl.MainView = tGridView;
                tGridControl.BringToFront();
            }
            else
            {
                tGridView = (DevExpress.XtraGrid.Views.Grid.GridView)tGridControl.MainView;
            }

            #endregion GridView ve Columns İşlemleri

            #region GridViewe ait properties ve events bağlantıları hazırlanıyor

            tGridView.ViewCaption = t.Set(row_Table["IP_CAPTION"].ToString(), "", "");

            int find = t.Set(row_Table["DATA_FIND"].ToString(), "0", 0);
            tGridView.OptionsFind.AllowFindPanel = false;
            tGridView.OptionsFind.AlwaysVisible = (find > 0);
            /// ( 100 * find )  neden bunu yapıyorum ?
            /// findDelay = 100 ise standart find
            /// findDelay = 200 ise list && data  yı işaret ediyor 
            tGridView.OptionsFind.FindDelay = 100 * find;
            tGridView.OptionsFind.FindMode = DevExpress.XtraEditors.FindMode.Always;  //FindClick; //Always;

            //tGridView.OptionsFind.a  //ColumnViewOptionsFind.AllowFindPanel = 

            tGridView.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
            tGridView.OptionsView.ColumnAutoWidth = false;
            tGridView.OptionsView.RowAutoHeight = true;
            //tGridView.OptionsBehavior.Editable = true;
            //tGridView.OptionsBehavior.AllowIncrementalSearch = true;
            tGridView.OptionsNavigation.EnterMoveNextColumn = true;
            tGridView.OptionsSelection.InvertSelection = (find == 0);

            // GridView in Column ları ekleniyor  
            tGrid_Columns_Create(dsFields, tGridView, dsData, TableIPCode);

            // işe yaramadı kontrol edilmesi gerekiyor
            // tablo datasını almak için bakılmıştı
            if (tGridView.Columns.Count == 0)
                tGridView.OptionsBehavior.AutoPopulateColumns = true;

            // İleride gerekebilir silme
            //GridView_Properties(tGridView, row_Table["CMP_PROPERTIES"].ToString() + "\r");

            tGridView.DoubleClick += new System.EventHandler(evg.myGridView_DoubleClick);
            tGridView.KeyDown += new System.Windows.Forms.KeyEventHandler(evg.myGridView_KeyDown);
            tGridView.KeyUp += new System.Windows.Forms.KeyEventHandler(evg.myGridView_KeyUp);
            tGridView.KeyPress += new System.Windows.Forms.KeyPressEventHandler(evg.myGridView_KeyPress);
            tGridView.FocusedColumnChanged += new DevExpress.XtraGrid.Views.Base.FocusedColumnChangedEventHandler(evg.myGridView_FocusedColumnChanged);
            tGridView.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(evg.myGridView_FocusedRowChanged);
            tGridView.CellValueChanging += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(evg.myGridView_CellValueChanging);
            tGridView.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(evg.myGridView_CellValueChanged);
            tGridView.ColumnChanged += new System.EventHandler(evg.myGridView_ColumnChanged);
            tGridView.InvalidRowException += new DevExpress.XtraGrid.Views.Base.InvalidRowExceptionEventHandler(evg.myGridView_InvalidRowException);
            tGridView.ValidateRow += new DevExpress.XtraGrid.Views.Base.ValidateRowEventHandler(evg.myGridView_ValidateRow);
            tGridView.MouseMove += new System.Windows.Forms.MouseEventHandler(evg.myGridView_MouseMove);
            tGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(evg.myGridView_MouseDown);
            tGridView.MouseUp += new System.Windows.Forms.MouseEventHandler(evg.myGridView_MouseUp);
            tGridView.ColumnFilterChanged += new System.EventHandler(evg.myGridView_ColumnFilterChanged);
            tGridView.ShownEditor += new System.EventHandler(evg.myGridView_ShownEditor);
            

            //tGridView.

            // BUNU AÇINCA CHECKBOX BOZULUYOR
            //tGridView.GotFocus += new System.EventHandler(evg.myGridView_GotFocus);
            //tGridView.LostFocus += new System.EventHandler(evg.myGridView_LostFocus);
            //tGridView.GridControl.Enter : bak
            //tGridView.GridControl.Leave : bak 
            /*
            tGridView.Appearance.FocusedCell.BackColor = v.colorNew;
            tGridView.Appearance.FocusedCell.Options.UseBackColor = true;
            tGridView.Appearance.FocusedRow.BackColor = v.colorNavigator;
            tGridView.Appearance.FocusedRow.Options.UseBackColor = true;

            
            tGridView.Appearance.FocusedCell.BorderColor = v.colorExit;
            tGridView.Appearance.FocusedCell.Options.UseBorderColor = true;
            tGridView.Appearance.FocusedRow.BorderColor = v.colorExit;
            tGridView.Appearance.FocusedRow.Options.UseBorderColor = true;
            */
            tGridView.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

            string Prop_View = t.Set(row_Table["PROP_VIEWS"].ToString(), "", "");
            PROP_VIEWS_IP JSON_PropView = null;// new PROP_VIEWS_IP();
            string Old_PropView = string.Empty;

            if (t.IsNotNull(Prop_View))
                Preparing_PropView_(Prop_View, ref Old_PropView, ref JSON_PropView);
                        
            if (JSON_PropView != null)
                Preparing_View_JSON(JSON_PropView, tGridControl, tGridView, null, null, null, null);
            else
            {
                tGridView.OptionsView.EnableAppearanceOddRow = true;
                tGridView.OptionsView.ShowGroupPanel = true;
                tGridView.OptionsView.ShowGroupedColumns = true;
                tGridView.OptionsSelection.InvertSelection = false;
            }

            tGridView.OptionsView.GroupDrawMode = GroupDrawMode.Office2003;
            //tGridView.OptionsBehavior.AutoExpandAllGroups = true;

            if (tGridView.OptionsView.NewItemRowPosition.ToString() == "Top")
            {
                tGridView.InitNewRow += new DevExpress.XtraGrid.Views.Grid.InitNewRowEventHandler(evg.myGridView_InitNewRow);
                tGridView.BeforeLeaveRow += new DevExpress.XtraGrid.Views.Base.RowAllowEventHandler(evg.myGridView_BeforeLeaveRow);
            }

            if (dsData != null)
                if (dsData.Tables.Count > 0)
                    tGridControl.DataSource = dsData.Tables[0];
                
            #endregion GridViewe ait properties ve events bağlantıları

        }
        #endregion GridView Create Base

        #region tTileView

        public void tTileView_Create(DataRow row_Table, DataSet dsFields, DataSet dsData, GridControl tGridControl)
        {
            tToolBox t = new tToolBox();
            tEventsGrid evg = new tEventsGrid();

            #region LayoutView ve Columns İşlemleri

            DevExpress.XtraGrid.Views.Tile.TileView tTileView = null;

            // Gelen GridControlu içinde GridView yok ise yeni bir View ekleyelim
            // var ise olanı kullanalım
            if ((tGridControl.MainView == null) ||
                (tGridControl.MainView.ToString() != "DevExpress.XtraGrid.Views.Tile.TileView"))
            {
                int RefId = t.Set(row_Table[0].ToString(), "", 0);
                tTileView = new DevExpress.XtraGrid.Views.Tile.TileView();
                tTileView.Name = "tGridView_" + RefId.ToString();
                tTileView.OptionsBehavior.AutoPopulateColumns = false;
                tTileView.OptionsFilter.AllowFilterEditor = false;
                tTileView.Appearance.ItemFocused.BackColor = v.AppearanceFocusedColor;
                tTileView.Appearance.ItemFocused.Options.UseBackColor = true;
                tTileView.OptionsTiles.LayoutMode = DevExpress.XtraGrid.Views.Tile.TileViewLayoutMode.Kanban;

                tGridControl.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { tTileView });
                tGridControl.BringToFront();
            }
            else
            {
                tTileView = (DevExpress.XtraGrid.Views.Tile.TileView)tGridControl.MainView;
            }


            int find = t.Set(row_Table["DATA_FIND"].ToString(), "0", 0);
            tTileView.OptionsFind.AlwaysVisible = (find > 0);
            /// ( 100 * find )  neden bunu yapıyorum ?
            /// findDelay = 100 ise standart find
            /// findDelay = 200 ise list && data  yı işaret ediyor 
            tTileView.OptionsFind.FindDelay = 100 * find;
            tTileView.OptionsFind.FindMode = DevExpress.XtraEditors.FindMode.Always;

            tTileView.DoubleClick += new System.EventHandler(evg.myGridView_DoubleClick);
            //tTileView.KeyDown += new System.Windows.Forms.KeyEventHandler(evg.myGridView_KeyDown);
            //tTileView.KeyUp += new System.Windows.Forms.KeyEventHandler(evg.myGridView_KeyUp);
            //tTileView.KeyPress += new System.Windows.Forms.KeyPressEventHandler(evg.myGridView_KeyPress);
            tTileView.FocusedColumnChanged += new DevExpress.XtraGrid.Views.Base.FocusedColumnChangedEventHandler(evg.myGridView_FocusedColumnChanged);
            //this.tTileView.FocusedColumnChanged += new DevExpress.XtraGrid.Views.Base.FocusedColumnChangedEventHandler(this.gridView1_FocusedColumnChanged);
            tTileView.CellValueChanging += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(evg.myGridView_CellValueChanging);

            tTileView.InvalidRowException += new DevExpress.XtraGrid.Views.Base.InvalidRowExceptionEventHandler(evg.myGridView_InvalidRowException);
            tTileView.ValidateRow += new DevExpress.XtraGrid.Views.Base.ValidateRowEventHandler(evg.myGridView_ValidateRow);
            tTileView.OptionsBehavior.ReadOnly = false;
            tTileView.OptionsBehavior.Editable = false;

            tTileView.MouseMove += new System.Windows.Forms.MouseEventHandler(evg.myTileView_MouseMove);
            tTileView.MouseDown += new System.Windows.Forms.MouseEventHandler(evg.myTileView_MouseDown);

            tTileView.ItemClick += new DevExpress.XtraGrid.Views.Tile.TileViewItemClickEventHandler(evg.myTileView_ItemClick);
            tTileView.ItemDoubleClick += new DevExpress.XtraGrid.Views.Tile.TileViewItemClickEventHandler(evg.myTileView_ItemDoubleClick);

            //tTileView.

            tGridControl.ForceInitialize();

            string Prop_View = t.Set(row_Table["PROP_VIEWS"].ToString(), "", "");
            PROP_VIEWS_IP JSON_PropView = null;// new PROP_VIEWS_IP();
            string Old_PropView = string.Empty;

            if (t.IsNotNull(Prop_View))
                Preparing_PropView_(Prop_View, ref Old_PropView, ref JSON_PropView);

            // Columns
            tGridTileViewColumns_Create(dsFields, tTileView, JSON_PropView);

            //if (t.IsNotNull(Prop_View))
            //    Preparing_View_(Prop_View, null, null, null, tTileView);

            if (t.IsNotNull(Old_PropView))
                Preparing_View_OLD(Old_PropView, null, null, null, tTileView);

            if (JSON_PropView != null)
                Preparing_View_JSON(JSON_PropView, tGridControl, null, null, null, tTileView, null);

            tGridControl.MainView = tTileView;

            if (dsData != null)
                if (dsData.Tables.Count > 0)
                    tGridControl.DataSource = dsData.Tables[0];

            #endregion TielView ve Columns İşlemleri
        }

        #region GridTileViewColumns Create

        private void tGridTileViewColumns_Create(DataSet dsFields,
            DevExpress.XtraGrid.Views.Tile.TileView tTileView,
            PROP_VIEWS_IP prop_)
        {
            tToolBox t = new tToolBox();
            tDevColumn dc = new tDevColumn();

            string tfield_name = string.Empty;
            string tcaption = string.Empty;
            string tTableLabel = string.Empty;
            string tfieldname = string.Empty;
            string tcolumn_type = string.Empty;

            Boolean tvisible = false;
            Int16 i = 3;
            //int tGrpNo = -1;
            //int tGrpLineNo = -1;
            int tCmpTop = 0;
            int tCmpLeft = 0;
            int tCmpFontStyle = 0;
            float font_size = 0;

            int tfieldtype = 0;

            /// ALLPROP
            string GROUPFNAME1 = string.Empty;
            string GROUPFNAME2 = string.Empty;
            string GROUPFNAME3 = string.Empty;

            /// ALLPROP
            if (prop_ != null)
            {
                GROUPFNAME1 = prop_.ALLPROP.GROUPFNAME1.ToString();
                GROUPFNAME2 = prop_.ALLPROP.GROUPFNAME2.ToString();
                GROUPFNAME3 = prop_.ALLPROP.GROUPFNAME3.ToString();
            }

            foreach (DataRow Row in dsFields.Tables[0].Rows)
            {
                tvisible = t.Set(Row["CMP_VISIBLE"].ToString(), Row["LKP_FVISIBLE"].ToString(), true);

                if (tvisible)
                {
                    tfield_name = t.Set(Row["LKP_FIELD_NAME"].ToString(), "", "null");
                    tcaption = t.Set(Row["FCAPTION"].ToString(), Row["LKP_FCAPTION"].ToString(), tfield_name);
                    tfieldtype = t.Set(Row["LKP_FIELD_TYPE"].ToString(), "", (int)167);
                    tcolumn_type = t.Set(Row["CMP_COLUMN_TYPE"].ToString(), Row["LKP_CMP_COLUMN_TYPE"].ToString(), "TextEdit");

                    tCmpTop = t.Set(Row["CMP_TOP"].ToString(), "", (int)0);
                    tCmpLeft = t.Set(Row["CMP_LEFT"].ToString(), "", (int)0);
                    tCmpFontStyle = t.Set(Row["CMP_FONT_STYLE"].ToString(), "", (int)0);
                    font_size = t.Set(Row["CMP_FONT_SIZE"].ToString(), "", (float)8);

                    if (font_size == 0) font_size = (float)8.25;

                    //CMP_FONT_COLOR
                    //CMP_BACK_COLOR

                    //tGrpNo = t.Set(Row["GROUP_NO"].ToString(), Row["LKP_GROUP_NO"].ToString(), (int)0);
                    //tGrpLineNo = t.Set(Row["GROUP_LINE_NO"].ToString(), Row["LKP_GROUP_LINE_NO"].ToString(), i);

                    DevExpress.XtraGrid.Views.Tile.TileViewItemElement tileViewItemElement1 = new DevExpress.XtraGrid.Views.Tile.TileViewItemElement();


                    DevExpress.XtraGrid.Columns.TileViewColumn column = new TileViewColumn();

                    column.FieldName = tfield_name;
                    column.Name = "Column_" + tfield_name;
                    column.Visible = tvisible;
                    column.VisibleIndex = i;
                    //column.ColumnEdit 

                    /// işe yaramadı ???
                    if ((tfield_name == GROUPFNAME1) ||
                        (tfield_name == GROUPFNAME2) ||
                        (tfield_name == GROUPFNAME3))
                    {
                        ///tTileView.GroupedColumns

                        //column.VisibleIndex = 99;
                        //i--;
                    }

                    column.Caption = t.Set(Row["FCAPTION"].ToString(), Row["LKP_FCAPTION"].ToString(), tfield_name);
                    column.OptionsColumn.AllowEdit = t.Set(Row["CMP_ENABLED"].ToString(), Row["LKP_FENABLED"].ToString(), true);

                    column.OptionsFilter.AllowFilter = false;

                    //column.Width = t.Set(Row["CMP_WIDTH"].ToString(), Row["LKP_CMP_WIDTH"].ToString(), 100);

                    tTileView.Columns.Add(column);

                    //this.tileView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
                    //  this.colREF_ID,

                    tileViewItemElement1.Column = column;
                    tileViewItemElement1.Text = column.Caption;

                    if ((tCmpTop != 0) || (tCmpLeft != 0))
                    {
                        tileViewItemElement1.TextAlignment = DevExpress.XtraEditors.TileItemContentAlignment.Manual;
                        tileViewItemElement1.TextLocation = new System.Drawing.Point(tCmpLeft, tCmpTop);
                    }
                    if ((font_size > 8.25) || (tCmpFontStyle > 0))
                    {
                        if ((tCmpFontStyle == 0) || (tCmpFontStyle == 3))
                            tileViewItemElement1.Appearance.Normal.Font = new System.Drawing.Font("Tahoma", font_size);
                        if (tCmpFontStyle == 1)
                            tileViewItemElement1.Appearance.Normal.Font = new System.Drawing.Font("Tahoma", font_size, System.Drawing.FontStyle.Bold);
                        if (tCmpFontStyle == 2)
                            tileViewItemElement1.Appearance.Normal.Font = new System.Drawing.Font("Tahoma", font_size, System.Drawing.FontStyle.Italic);
                        if (tCmpFontStyle == 4)
                            tileViewItemElement1.Appearance.Normal.Font = new System.Drawing.Font("Tahoma", font_size, System.Drawing.FontStyle.Strikeout);
                        if (tCmpFontStyle == 5)
                            tileViewItemElement1.Appearance.Normal.Font = new System.Drawing.Font("Tahoma", font_size, System.Drawing.FontStyle.Underline);

                        tileViewItemElement1.Appearance.Normal.Options.UseFont = true;

                        /// new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));

                        /// 1 = System.Drawing.FontStyle.Bold
                        /// 2 = System.Drawing.FontStyle.Italic
                        /// 3, 0 = System.Drawing.FontStyle.Regular
                        /// 4 = System.Drawing.FontStyle.Strikeout
                        /// 5 = System.Drawing.FontStyle.Underline
                    }

                    tTileView.TileTemplate.Add(tileViewItemElement1);

                    /// tileViewItemElement1.Appearance.Normal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
                    /// tileViewItemElement1.Appearance.Normal.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
                    /// tileViewItemElement1.Appearance.Normal.ForeColor = System.Drawing.Color.White;
                    /// tileViewItemElement1.Appearance.Normal.Options.UseBackColor = true;
                    /// tileViewItemElement1.Appearance.Normal.Options.UseFont = true;
                    /// tileViewItemElement1.Appearance.Normal.Options.UseForeColor = true;
                    /// tileViewItemElement1.Column = this.tileViewColumn1;
                    /// tileViewItemElement1.StretchHorizontal = true;
                    /// tileViewItemElement1.Text = "element6";
                    /// tileViewItemElement1.TextAlignment = DevExpress.XtraEditors.TileItemContentAlignment.Manual;
                    /// tileViewItemElement1.TextLocation = new System.Drawing.Point(10, 50);

                    dc.Tile_ColumnEdit(Row, column, tcolumn_type);

                    i++;
                }

            }
        }

        #endregion GridTileViewColumns Columns Create

        #endregion TileView

        #region tWinExplorerView

        public void tWinExplorerView_Create(DataRow row_Table, DataSet dsFields, DataSet dsData, GridControl tGridControl, string TableIPCode)
        {
            //DevExpress.XtraGrid.Views.WinExplorer.WinExplorerView
            tToolBox t = new tToolBox();
            tEventsGrid evg = new tEventsGrid();

            // Tablo hakkındaki bilgiler
            //DataRow row_Table = dsTable.Tables[0].Rows[0];

            #region GridView ve Columns İşlemleri

            DevExpress.XtraGrid.Views.WinExplorer.WinExplorerView tGridView = null;

            // Gelen GridControlu içinde GridView yok ise yeni bir View ekleyelim
            // var ise olanı kullanalım
            if (tGridControl.MainView == null)
            {
                int RefId = t.Set(row_Table[0].ToString(), "", 0);
                tGridView = new DevExpress.XtraGrid.Views.WinExplorer.WinExplorerView();
                tGridView.Name = "tGridView_" + RefId.ToString();
                tGridView.ViewCaption = t.Set(row_Table["IP_CAPTION"].ToString(), "", "");
                tGridView.GridControl = tGridControl;
                tGridControl.TabIndex = 0;
                tGridControl.MainView = tGridView;
                tGridControl.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
                   tGridView});
                tGridControl.BringToFront();
            }
            else
            {
                tGridView = (DevExpress.XtraGrid.Views.WinExplorer.WinExplorerView)tGridControl.MainView;
            }

            tGridView.OptionsView.ShowExpandCollapseButtons = true;
            tGridView.OptionsView.Style = DevExpress.XtraGrid.Views.WinExplorer.WinExplorerViewStyle.Large;
            tGridView.ViewCaptionHeight = 80;

            tGridView.OptionsBehavior.Editable = false;
            tGridView.OptionsSelection.AllowMarqueeSelection = true;
            //tGridView.ItemSelectionMode = DevExpress.XtraGrid.Views.WinExplorer.IconItemSelectionMode.Click;
            tGridView.OptionsSelection.MultiSelect = true;
            tGridView.OptionsView.DrawCheckedItemsAsSelected = true;
            tGridView.OptionsView.ImageLayoutMode = DevExpress.Utils.Drawing.ImageLayoutMode.Squeeze;
            tGridView.OptionsView.ShowViewCaption = true;
            tGridView.OptionsView.ShowExpandCollapseButtons = true;
            tGridView.OptionsView.ShowCheckBoxInGroupCaption = true;
            tGridView.OptionsView.DrawCheckedItemsAsSelected = true;

            // GridView in Column ları ekleniyor  
            tGrid_Columns_Create(dsFields, tGridView, TableIPCode);


            #endregion GridView ve Columns İşlemleri

            #region GridViewe ait properties ve events bağlantıları hazırlanıyor

            //tGridView.OptionsView.ColumnAutoWidth = false;
            //tGridView.OptionsView.RowAutoHeight = true;

            int find = t.Set(row_Table["DATA_FIND"].ToString(), "0", 0);
            tGridView.OptionsFind.AlwaysVisible = (find > 0);
            /// ( 100 * find )  neden bunu yapıyorum ?
            /// findDelay = 100 ise standart find
            /// findDelay = 200 ise list && data  yı işaret ediyor 
            tGridView.OptionsFind.FindDelay = 100 * find;

            tGridView.OptionsFind.FindMode = DevExpress.XtraEditors.FindMode.Always;
            tGridView.OptionsBehavior.Editable = true;
            tGridView.OptionsNavigation.EnterMoveNextColumn = true;

            //tGridView.ColumnSet.GroupColumn.SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;

            tGridView.DoubleClick += new System.EventHandler(evg.myGridView_DoubleClick);
            tGridView.KeyDown += new System.Windows.Forms.KeyEventHandler(evg.myGridView_KeyDown);
            tGridView.KeyUp += new System.Windows.Forms.KeyEventHandler(evg.myGridView_KeyUp);
            tGridView.KeyPress += new System.Windows.Forms.KeyPressEventHandler(evg.myGridView_KeyPress);

            tGridView.InvalidRowException += new DevExpress.XtraGrid.Views.Base.InvalidRowExceptionEventHandler(evg.myGridView_InvalidRowException);
            tGridView.ValidateRow += new DevExpress.XtraGrid.Views.Base.ValidateRowEventHandler(evg.myGridView_ValidateRow);

            tGridView.MouseMove += new System.Windows.Forms.MouseEventHandler(evg.myGridView_MouseMove);
            tGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(evg.myGridView_MouseDown);


            //tGridView.OptionsViewStyles = 
            //    WinExplorerViewStyle

            //if (tGridView.OptionsView.NewItemRowPosition.ToString() == "Top")
            //{
            //    tGridView.InitNewRow += new DevExpress.XtraGrid.Views.Grid.InitNewRowEventHandler(evg.myGridView_InitNewRow);
            //    tGridView.BeforeLeaveRow += new DevExpress.XtraGrid.Views.Base.RowAllowEventHandler(evg.myGridView_BeforeLeaveRow);
            //}

            if (dsData != null)
                if (dsData.Tables.Count > 0)
                    tGridControl.DataSource = dsData.Tables[0];

            #endregion GridViewe ait properties ve events bağlantıları

        }

        #endregion WinExplorerView

        #region tAdvBandedGridView Create

        public void tAdvBandedGridView_Create(Form tForm, DataRow row_Table, DataSet dsFields, DataSet dsData, GridControl tGridControl)
        {
            tToolBox t = new tToolBox();
            tEventsGrid evg = new tEventsGrid();

            // Tablo hakkındaki bilgiler
            //DataRow row_Table = dsTable.Tables[0].Rows[0];
            string SoftwareCode = row_Table["SOFTWARE_CODE"].ToString();
            string ProjectCode = row_Table["PROJECT_CODE"].ToString();
            string TableIPCode = row_Table["TABLE_CODE"].ToString() + "." + row_Table["IP_CODE"].ToString();
            if (t.IsNotNull(SoftwareCode))
                TableIPCode = SoftwareCode + "/" + ProjectCode + "/" + TableIPCode;


            #region AdvBandedGridView ve Columns İşlemleri

            AdvBandedGridView tGridView = null;
            //BandedGridView tGridView = null;
            // Gelen GridControlu içinde GridView yok ise yeni bir View ekleyelim
            // var ise olanı kullanalım
            if ((tGridControl.MainView == null) ||
                (tGridControl.MainView.ToString() != "DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView"))
            {
                int RefId = t.Set(row_Table[0].ToString(), "", 0);
                tGridView = new AdvBandedGridView(tGridControl);
                //tGridView = new BandedGridView(tGridControl);
                tGridView.Name = "tAdvBandedGridView_" + RefId.ToString();
                tGridView.ViewCaption = t.Set(row_Table["IP_CAPTION"].ToString(), "", "");
                tGridControl.MainView = tGridView;
                tGridControl.BringToFront();
                //tGridView.OptionsBehavior.EditingMode = GridEditingMode.EditForm;
            }
            else
            {
                tGridView = (DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)tGridControl.MainView;
                //tGridView = (DevExpress.XtraGrid.Views.BandedGrid.BandedGridView)tGridControl.MainView;
            }

            // Add Bands >> Gridin Üzerindeki Bands ları oluştur
            GridBands_Add(tGridView, null, null, dsFields, TableIPCode);

            // GridView in Column ları ekleniyor  
            tAdvBandedGrid_Columns_Create(dsFields, tGridView, TableIPCode);

            #endregion AdvBandedGridView ve Columns İşlemleri

            /// You can handle the AdvancedBandedGridView.ClacRowHeight event or use the 
            /// AdvancedBandedGridView.RowHeight property to set RowHeight.

            #region GridViewe ait properties ve events bağlantıları hazırlanıyor


            int find = t.Set(row_Table["DATA_FIND"].ToString(), "0", 0);
            tGridView.OptionsFind.AllowFindPanel = false;
            tGridView.OptionsFind.AlwaysVisible = (find > 0);
            /// ( 100 * find )  neden bunu yapıyorum ?
            /// findDelay = 100 ise standart find
            /// findDelay = 200 ise list && data  yı işaret ediyor 
            tGridView.OptionsFind.FindDelay = 100 * find;
            tGridView.OptionsFind.FindMode = DevExpress.XtraEditors.FindMode.Always;
                                    
            tGridView.OptionsView.ColumnHeaderAutoHeight = DevExpress.Utils.DefaultBoolean.True;
            tGridView.OptionsView.ColumnAutoWidth = false;
            tGridView.OptionsView.RowAutoHeight = true;
            //tGridView.OptionsBehavior.Editable = true;
            //tGridView.OptionsBehavior.AllowIncrementalSearch = true;
            tGridView.OptionsNavigation.EnterMoveNextColumn = true;
            tGridView.OptionsSelection.InvertSelection = (find == 0);

            /*
                tGridView.OptionsBehavior.Editable = true;
            */

            // iLERİDE GEREKEBİLİR, SİLME
            //AdvBandedGridView_Properties(tGridView, row_Table["CMP_PROPERTIES"].ToString() + "\r");

            //tGridView.InvalidRowException += new DevExpress.XtraGrid.Views.Base.InvalidRowExceptionEventHandler(evg.myGridView_InvalidRowException);
            //tGridView.ValidateRow += new DevExpress.XtraGrid.Views.Base.ValidateRowEventHandler(evg.myGridView_ValidateRow);

            tGridView.DoubleClick += new System.EventHandler(evg.myGridView_DoubleClick);
            tGridView.KeyDown += new System.Windows.Forms.KeyEventHandler(evg.myGridView_KeyDown);
            tGridView.KeyUp += new System.Windows.Forms.KeyEventHandler(evg.myGridView_KeyUp);
            tGridView.KeyPress += new System.Windows.Forms.KeyPressEventHandler(evg.myGridView_KeyPress);
            tGridView.FocusedColumnChanged += new DevExpress.XtraGrid.Views.Base.FocusedColumnChangedEventHandler(evg.myGridView_FocusedColumnChanged);
            tGridView.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(evg.myGridView_FocusedRowChanged);
            tGridView.CellValueChanging += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(evg.myGridView_CellValueChanging);
            tGridView.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(evg.myGridView_CellValueChanged);
            tGridView.ColumnChanged += new System.EventHandler(evg.myGridView_ColumnChanged);
            tGridView.InvalidRowException += new DevExpress.XtraGrid.Views.Base.InvalidRowExceptionEventHandler(evg.myGridView_InvalidRowException);
            tGridView.ValidateRow += new DevExpress.XtraGrid.Views.Base.ValidateRowEventHandler(evg.myGridView_ValidateRow);
            tGridView.MouseMove += new System.Windows.Forms.MouseEventHandler(evg.myGridView_MouseMove);
            tGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(evg.myGridView_MouseDown);
            tGridView.ColumnFilterChanged += new System.EventHandler(evg.myGridView_ColumnFilterChanged);
            tGridView.ShownEditor += new System.EventHandler(evg.myGridView_ShownEditor);
            //tGridView.

            // BUNU AÇINCA CHECKBOX DA SORUN OLUŞUYOR
            //tGridView.GotFocus += new System.EventHandler(evg.myGridView_GotFocus);
            //tGridView.LostFocus += new System.EventHandler(evg.myGridView_LostFocus);
            //tGridView.GridControl.Enter : bak
            //tGridView.GridControl.Leave : bak 
            /*
            tGridView.Appearance.FocusedCell.BackColor = v.colorNew;
            tGridView.Appearance.FocusedCell.Options.UseBackColor = true;
            tGridView.Appearance.FocusedRow.BackColor = v.colorNavigator;
            tGridView.Appearance.FocusedRow.Options.UseBackColor = true;

            tGridView.Appearance.FocusedCell.BorderColor = v.colorExit;
            tGridView.Appearance.FocusedCell.Options.UseBorderColor = true;
            tGridView.Appearance.FocusedRow.BorderColor = v.colorExit;
            tGridView.Appearance.FocusedRow.Options.UseBorderColor = true;
            */
            tGridView.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;


            string Prop_View = t.Set(row_Table["PROP_VIEWS"].ToString(), "", "");
            PROP_VIEWS_IP JSON_PropView = null; //new PROP_VIEWS_IP(); 
            //string Old_PropView = string.Empty;

            if (t.IsNotNull(Prop_View))
                JSON_PropView = t.readProp<PROP_VIEWS_IP>(Prop_View);

            if (JSON_PropView != null)
                 Preparing_View_JSON(JSON_PropView, tGridControl, null, tGridView, null, null, null);

            tGridView.OptionsView.GroupDrawMode = GroupDrawMode.Office2003;
            tGridView.OptionsBehavior.AutoExpandAllGroups = true;
            //tGridView.OptionsBehavior.AllowPartialGroups = DevExpress.Utils.DefaultBoolean.True;
            
            if (tGridView.OptionsView.NewItemRowPosition.ToString() == "Top")
            {
                tGridView.InitNewRow += new DevExpress.XtraGrid.Views.Grid.InitNewRowEventHandler(evg.myGridView_InitNewRow);
                tGridView.BeforeLeaveRow += new DevExpress.XtraGrid.Views.Base.RowAllowEventHandler(evg.myGridView_BeforeLeaveRow);
            }

            if (dsData != null)
                if (dsData.Tables.Count > 0)
                    tGridControl.DataSource = dsData.Tables[0];

            int tabIndex = evg.GetUserRegistrySelectBands(v.tUser.UserId, TableIPCode);
            if (tabIndex == 0) tabIndex = 1;
            evg.selectBands(tForm, TableIPCode, tabIndex);

            /// deneme
            tGridView.Appearance.FocusedCell.BackColor = v.colorNavigator;
            tGridView.Appearance.FocusedCell.Options.UseBackColor = true;

            tGridView.Appearance.SelectedRow.BackColor = v.colorExit;
            tGridView.Appearance.SelectedRow.Options.UseBackColor = true;

            #endregion GridViewe ait properties ve events bağlantıları

        }

        public void GridBands_Add(
            AdvBandedGridView tGridView,
            VGridControl tVGridControl,
            Control tButtonPanel,
            DataSet dsFields, string TableIPCode)
        {
            tToolBox t = new tToolBox();
            tEvents ev = new tEvents();
            tEventsGrid evg = new tEventsGrid();

            #region Tanımlar
            string caption = string.Empty;
            string caption2 = string.Empty;
            string groupno = string.Empty;
            string visible = string.Empty;
            string fixedbn = string.Empty;
            string TableCode1 = string.Empty;
            string TableCode2 = "none";

            Int16 width = 0;
            int i = 0;

            vTableAbout vTA = new vTableAbout();
            t.Table_FieldsGroups_Count(vTA, dsFields);
            int gc = vTA.GroupsCount;

            #endregion Tanımlar

            #region Hiç Tanım Yok ise
            // Eğer hiç group tanımlı değil en az bir adet 
            // BandedGroup olması gerekiyor
            if ((tGridView != null) &&
                (gc == 0))
            {
                GridBand Gband = new GridBand();
                Gband.Caption = "";
                Gband.Visible = true;
                Gband.VisibleIndex = 0;
                tGridView.Bands.AddRange(new GridBand[] { Gband });
                return;
            }
            #endregion 

            #region Group var ise
            // Group bilgileri mevcut
            if (gc > 0)
            {
                foreach (DataRow Row in dsFields.Tables[1].Rows)
                {
                    TableCode1 = t.Set(Row["TABLE_CODE"].ToString(), "", "");

                    // Group sırası önce MS_FIELDS_IP üzerinde tanımlı olanlar
                    // sonra MS_FIELDS üzerinde tanımlı olan gruplar listelenir

                    // birbirinden ayıran fark TABLE_CODE fieldi
                    // örnek tablo çıktısı aşağıda

                    // MS_FIELDS_ID üzerindekiler varsa açılacak
                    // yoksa MS_FIELDS üzerindekiler açılacak
                    if ((TableCode1 != TableCode2) && (TableCode2 != "none"))
                    {
                        break;
                    }

                    TableCode2 = TableCode1;

                    caption = t.Set(Row["FCAPTION"].ToString(), "", "");
                    groupno = t.Set(Row["FGROUPNO"].ToString(), "", "");
                    visible = t.Set(Row["FVISIBLE"].ToString(), "", "");
                    fixedbn = t.Set(Row["FIXED"].ToString(), "", "");
                    width = t.Set(Row["CMP_WIDTH"].ToString(), "", (Int16)300);

                    caption2 = caption;
                    i = caption.IndexOf("<");

                    if (i > -1)
                        caption2 = caption.Substring(0, caption.IndexOf("<"));

                    if (tGridView != null && visible == "True")
                    {
                        GridBand Gband = new GridBand();
                        Gband.Caption = caption2;
                        Gband.Visible = t.myVisible(visible);
                        Gband.VisibleIndex = t.myInt32(groupno);
                        Gband.Width = width;

                        if (fixedbn.ToUpper() == "LEFT")
                        {
                            Gband.Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
                            //Gband.View.Appearance. //= System.Drawing.Color.Azure;
                            //ConsoleColor.DarkGreen;
                        }
                        if (fixedbn.ToUpper() == "RIGHT")
                        {
                            Gband.Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Right;
                        }
                        tGridView.Bands.AddRange(new GridBand[] { Gband });
                    }

                    if (tVGridControl != null && visible == "True")
                    {
                        CategoryRow row = new CategoryRow(caption2);
                        row.Name = "CategoryRow_" + groupno;
                        row.Expanded = true;
                        tVGridControl.Rows.Add(row);
                    }

                    if (tButtonPanel != null)// && visible == "True")
                    {
                        // burda değişiklik yaparsan  myMenuShortKeyClick i kontrol etmeyi unutma
                        //
                        DevExpress.XtraEditors.CheckButton simpleButton_Gbutton = new DevExpress.XtraEditors.CheckButton();
                        simpleButton_Gbutton.AccessibleDescription = TableIPCode;
                        simpleButton_Gbutton.AllowHtmlDraw = DevExpress.Utils.DefaultBoolean.True;
                        simpleButton_Gbutton.Name = "tGbutton_" + groupno;
                        simpleButton_Gbutton.Text = caption;
                        simpleButton_Gbutton.Dock = DockStyle.Left;
                        simpleButton_Gbutton.Width = 40 + ((caption.Length) * 4);
                        simpleButton_Gbutton.GroupIndex = 1;
                        simpleButton_Gbutton.TabIndex = t.myInt32(groupno);
                        simpleButton_Gbutton.BorderStyle = BorderStyles.UltraFlat;

                        //simpleButton_Gbutton.Checked = t.myVisible(visible);
                        simpleButton_Gbutton.Click += new System.EventHandler(evg.checkGridGroupButton_Click);

                        simpleButton_Gbutton.AppearanceHovered.BackColor = v.colorOrder;
                        simpleButton_Gbutton.AppearanceHovered.Options.UseBackColor = true;

                        simpleButton_Gbutton.Enter += new System.EventHandler(ev.btn_Navigotor_Enter);
                        simpleButton_Gbutton.Leave += new System.EventHandler(ev.btn_Navigotor_Leave);

                        tButtonPanel.Controls.Add(simpleButton_Gbutton);
                        simpleButton_Gbutton.BringToFront();
                    }

                }
            }
            #endregion 

            #region örnek group table sql sonucu
            /*
MAIN_TABLE_NAME                                    TABLE_CODE           IP_CODE              FCAPTION                                           FGROUPNO FVISIBLE FIXED      TABLE_NO
-------------------------------------------------- -------------------- -------------------- -------------------------------------------------- -------- -------- ---------- -----------
MS_FIELDS_IP                                       T03_MSFIELDS         T03_MSFIELDS_01      Fields NameSSS                                     0        1        NULL       1
MS_FIELDS_IP                                       T03_MSFIELDS         T03_MSFIELDS_01      ORDER                                              1        1        NULL       1
MS_FIELDS                                          T03_MSFIELDS                              Fields Name                                        0        1        LEFT       2
MS_FIELDS                                          T03_MSFIELDS                              Field Type                                         1        1        NULL       2
MS_FIELDS                                          T03_MSFIELDS                              Companent                                          2        0        NULL       2
MS_FIELDS                                          T03_MSFIELDS                              Default Value                                      3        0        NULL       2
MS_FIELDS                                          T03_MSFIELDS                              Validation                                         4        0        NULL       2
MS_FIELDS                                          T03_MSFIELDS                              MasterTable                                        5        0        NULL       2
MS_FIELDS                                          T03_MSFIELDS                              Order <<<                                          6        0        NULL       2

(9 row(s) affected)
*/
            #endregion örnek group table sql sonucu
        }

        #endregion AdvBandedGridView

        #region tGridLayoutView Create

        public void tGridLayoutView_Create(DataRow row_Table, DataSet dsFields, DataSet dsData, GridControl tGridControl, string TableIPCode)
        {
            tToolBox t = new tToolBox();
            tEventsGrid evg = new tEventsGrid();

            // Tablo hakkındaki bilgiler
            //DataRow row_Table = dsTable.Tables[0].Rows[0];

            #region LayoutView ve Columns İşlemleri

            DevExpress.XtraGrid.Views.Layout.LayoutView tGridView = null;

            // Gelen GridControlu içinde GridView yok ise yeni bir View ekleyelim
            // var ise olanı kullanalım
            if ((tGridControl.MainView == null) ||
                (tGridControl.MainView.ToString() != "DevExpress.XtraGrid.Views.Layout.LayoutView"))
            {
                int RefId = t.Set(row_Table[0].ToString(), "", 0);
                tGridView = new DevExpress.XtraGrid.Views.Layout.LayoutView(tGridControl);

                tGridView.Name = "tGridView_" + RefId.ToString();
                tGridView.OptionsBehavior.AutoPopulateColumns = false;
                tGridView.OptionsView.ViewMode = LayoutViewMode.MultiRow;
                tGridView.OptionsView.AllowHotTrackFields = true;
                tGridView.OptionsView.ShowCardCaption = true;
                tGridView.OptionsView.ShowCardExpandButton = false;
                tGridView.OptionsFilter.AllowFilterEditor = false;

                int find = t.Set(row_Table["DATA_FIND"].ToString(), "0", 0);
                tGridView.OptionsFind.AlwaysVisible = (find > 0);
                /// ( 100 * find )  neden bunu yapıyorum ?
                /// findDelay = 100 ise standart find
                /// findDelay = 200 ise list && data  yı işaret ediyor 
                tGridView.OptionsFind.FindDelay = 100 * find;
                tGridView.OptionsFind.FindMode = DevExpress.XtraEditors.FindMode.Always;

                tGridControl.BringToFront();
            }
            else
            {
                tGridView = (DevExpress.XtraGrid.Views.Layout.LayoutView)tGridControl.MainView;
            }

            tGridView.DoubleClick += new System.EventHandler(evg.myGridView_DoubleClick);
            tGridView.KeyDown += new System.Windows.Forms.KeyEventHandler(evg.myGridView_KeyDown);
            tGridView.KeyUp += new System.Windows.Forms.KeyEventHandler(evg.myGridView_KeyUp);
            tGridView.KeyPress += new System.Windows.Forms.KeyPressEventHandler(evg.myGridView_KeyPress);
            tGridView.FocusedColumnChanged += new DevExpress.XtraGrid.Views.Base.FocusedColumnChangedEventHandler(evg.myGridView_FocusedColumnChanged);
            tGridView.CellValueChanging += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(evg.myGridView_CellValueChanging);

            tGridView.InvalidRowException += new DevExpress.XtraGrid.Views.Base.InvalidRowExceptionEventHandler(evg.myGridView_InvalidRowException);
            tGridView.ValidateRow += new DevExpress.XtraGrid.Views.Base.ValidateRowEventHandler(evg.myGridView_ValidateRow);

            tGridView.MouseMove += new System.Windows.Forms.MouseEventHandler(evg.myGridView_MouseMove);
            tGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(evg.myGridView_MouseDown);

            tGridControl.ForceInitialize();

            #region 

            // Columns
            tGridLayoutViewColumns_Create(dsFields, tGridView, TableIPCode);
            // Groups and TabPages
            tLayoutControlGroup_Add(dsFields, tGridView);
            // Move                        
            tColumnAndGroup_Move(dsFields, tGridView, null);

            #endregion

            tGridControl.MainView = tGridView;

            if (dsData != null)
                if (dsData.Tables.Count > 0)
                    tGridControl.DataSource = dsData.Tables[0];

            #endregion LayoutView ve Columns İşlemleri
        }

        #region GridLayoutViewColumns Create

        private void tGridLayoutViewColumns_Create(DataSet dsFields,
                DevExpress.XtraGrid.Views.Layout.LayoutView tGridView, string TableIPCode)
        {
            tToolBox t = new tToolBox();
            tDevColumn dc = new tDevColumn();

            string tfield_name = string.Empty;
            string tcaption = string.Empty;
            string tTableLabel = string.Empty;
            string tfieldname = string.Empty;
            string tcolumn_type = string.Empty;

            Boolean tvisible = false;
            Int16 i = 0;
            int tGrpNo = -1;
            int tGrpLineNo = -1;
            int tfieldtype = 0;

            foreach (DataRow Row in dsFields.Tables[0].Rows)
            {
                tvisible = t.Set(Row["CMP_VISIBLE"].ToString(), Row["LKP_FVISIBLE"].ToString(), true);

                if (tvisible)
                {
                    tfield_name = t.Set(Row["LKP_FIELD_NAME"].ToString(), "", "null");
                    tcaption = t.Set(Row["FCAPTION"].ToString(), Row["LKP_FCAPTION"].ToString(), tfield_name);
                    tfieldtype = t.Set(Row["LKP_FIELD_TYPE"].ToString(), "", (int)167);
                    tcolumn_type = t.Set(Row["CMP_COLUMN_TYPE"].ToString(), Row["LKP_CMP_COLUMN_TYPE"].ToString(), "TextEdit");

                    // Create columns.
                    //LayoutViewColumn colFirstName = lView.Columns.AddField("FirstName");
                    // Access corresponding card fields.
                    //LayoutViewField fieldFirstName = colFirstName.LayoutViewField;

                    LayoutViewColumn column = tGridView.Columns.AddField(tfield_name); //new LayoutViewColumn();

                    column.Caption = t.Set(Row["FCAPTION"].ToString(), Row["LKP_FCAPTION"].ToString(), tfield_name);
                    column.Name = "lvColumn_" + tfield_name;
                    column.Visible = tvisible;
                    column.OptionsColumn.AllowEdit = t.Set(Row["CMP_ENABLED"].ToString(), Row["LKP_FENABLED"].ToString(), true);
                    column.OptionsFilter.AllowFilter = false;
                    column.OptionsField.SortFilterButtonShowMode = SortFilterButtonShowMode.Nowhere;

                    column.Width = t.Set(Row["CMP_WIDTH"].ToString(), Row["LKP_CMP_WIDTH"].ToString(), 100);

                    LayoutViewField fcolumn = column.LayoutViewField;
                    column.LayoutViewField.Name = "Column_" + tfield_name;

                    tGrpNo = t.Set(Row["GROUP_NO"].ToString(), Row["LKP_GROUP_NO"].ToString(), (int)0);
                    tGrpLineNo = t.Set(Row["GROUP_LINE_NO"].ToString(), Row["LKP_GROUP_LINE_NO"].ToString(), i);

                    // Bunu Açma gruplarını böyle belirleyince çalışmıyor
                    //if (tGrpNo > 0)
                    //    column.GroupIndex = tGrpNo;

                    dc.Grid_ColumnEdit(Row, column, null, tcolumn_type, TableIPCode);

                    i++;
                }

            }
        }

        private void tLayoutControlGroup_Add(DataSet dsFields, DevExpress.XtraGrid.Views.Layout.LayoutView tGridView)
        {
            tToolBox t = new tToolBox();
            //tEventsGrid evg = new tEventsGrid();

            #region Tanımlar

            string caption = string.Empty;
            string groupno = string.Empty;
            string visible = string.Empty;
            string fixedbn = string.Empty;
            string TableCode1 = string.Empty;
            string TableCode2 = "none";

            Boolean tTabPage = false;
            Boolean tvisible = false;
            Int16 group_types = 0;
            Int16 fgroup_no = 0;
            Int16 fgroup_line_no = 0;
            Int16 j = 0;

            vTableAbout vTA = new vTableAbout();
            t.Table_FieldsGroups_Count(vTA, dsFields);
            int gc = vTA.GroupsCount;

            TabbedControlGroup LCTabbedGroup = null;

            #endregion Tanımlar

            #region Group var ise
            // Group bilgileri mevcut
            if (gc > 0)
            {
                foreach (DataRow Row in dsFields.Tables[1].Rows)
                {
                    TableCode1 = t.Set(Row["TABLE_CODE"].ToString(), "", "");

                    // Group sırası önce MS_FIELDS_IP üzerinde tanımlı olanlar
                    // sonra MS_FIELDS üzerinde tanımlı olan gruplar listelenir

                    // birbirinden ayıran fark TABLE_CODE fieldi
                    // örnek tablo çıktısı aşağıda

                    // MS_FIELDS_ID üzerindekiler varsa açılacak
                    // yoksa MS_FIELDS üzerindekiler açılacak
                    if ((TableCode1 != TableCode2) && (TableCode2 != "none"))
                    {
                        break;
                    }

                    TableCode2 = TableCode1;

                    caption = t.Set(Row["FCAPTION"].ToString(), "", "");
                    groupno = t.Set(Row["FGROUPNO"].ToString(), "", "");
                    visible = t.Set(Row["FVISIBLE"].ToString(), "", "");
                    fixedbn = t.Set(Row["FIXED"].ToString(), "", "");
                    group_types = t.Set(Row["GROUP_TYPES"].ToString(), "", (Int16)0);

                    #region // GroupPanel ise
                    if ((tGridView != null) && (group_types <= 1))
                    {
                        LayoutControlGroup LCGroup = new LayoutControlGroup();
                        LCGroup.Name = "LCGroup_" + groupno;
                        LCGroup.Text = caption;
                        tGridView.TemplateCard.AddGroup(LCGroup);

                        j = 0;
                        for (int i = 0; i < dsFields.Tables[0].Rows.Count; i++)
                        {
                            tvisible = t.Set(dsFields.Tables[0].Rows[i]["CMP_VISIBLE"].ToString(), dsFields.Tables[0].Rows[i]["LKP_FVISIBLE"].ToString(), true);
                            fgroup_no = t.Set(dsFields.Tables[0].Rows[i]["GROUP_NO"].ToString(), dsFields.Tables[0].Rows[i]["LKP_GROUP_NO"].ToString(), (Int16)0);
                            fgroup_line_no = t.Set(dsFields.Tables[0].Rows[i]["GROUP_LINE_NO"].ToString(), dsFields.Tables[0].Rows[i]["LKP_GROUP_LINE_NO"].ToString(), (Int16)0); ;
                            if (fgroup_no > 0)
                            {
                                if (t.myInt32(groupno) == fgroup_no)
                                {
                                    LCGroup.Add(tGridView.Columns[j].LayoutViewField);
                                }
                            }
                            if (tvisible) j++;
                        }
                    }
                    #endregion // GroupPanel ise

                    #region // TabPage ise
                    if ((tGridView != null) && (group_types == 2))
                    {
                        if (LCTabbedGroup == null)
                        {
                            //TabbedControlGroup LCTabbedGroup = new TabbedControlGroup();
                            LCTabbedGroup = new TabbedControlGroup(); // AddTabbedGroup();
                            LCTabbedGroup.Name = "LCTabbed_" + groupno;
                            LCTabbedGroup.Text = caption;

                            tGridView.TemplateCard.AddTabbedGroup(LCTabbedGroup);
                            tTabPage = true;
                        }

                        /*
                        // Create a tabbed group within the root group.
                           TabbedControlGroup tabbedGroup = lc.Root.AddTabbedGroup();
                           tabbedGroup.Name = "TabbedGroup";
                        // Add a new group as a tab to the tabbed group.
                           LayoutControlGroup group1 = tabbedGroup.AddTabPage() as LayoutControlGroup;
                           group1.Name = "LayoutControlGroup1";
                           group1.Text = "Photo";
                        */

                        LayoutControlGroup tabbedGroup = LCTabbedGroup.AddTabPage() as LayoutControlGroup;
                        tabbedGroup.Name = "LCTabbedGroup_" + groupno;
                        tabbedGroup.Text = caption;

                        j = 0;
                        for (int i = 0; i < dsFields.Tables[0].Rows.Count; i++)
                        {
                            tvisible = t.Set(dsFields.Tables[0].Rows[i]["CMP_VISIBLE"].ToString(), dsFields.Tables[0].Rows[i]["LKP_FVISIBLE"].ToString(), true);
                            fgroup_no = t.Set(dsFields.Tables[0].Rows[i]["GROUP_NO"].ToString(), dsFields.Tables[0].Rows[i]["LKP_GROUP_NO"].ToString(), (Int16)0);
                            fgroup_line_no = t.Set(dsFields.Tables[0].Rows[i]["GROUP_LINE_NO"].ToString(), dsFields.Tables[0].Rows[i]["LKP_GROUP_LINE_NO"].ToString(), (Int16)0); ;
                            if (fgroup_no > 0)
                            {
                                if ((t.myInt32(groupno) == fgroup_no) && (tvisible))
                                {
                                    tabbedGroup.AddItem(tGridView.Columns[j].LayoutViewField);
                                }
                            }
                            if (tvisible) j++;
                        }
                    }
                    #endregion // TabPage ise

                }

                if (tTabPage)
                {
                    LCTabbedGroup.SelectedTabPageIndex = 0;
                }
            }
            #endregion Group var ise

        }

        #endregion GridLayoutViewColumns Columns Create

        /*
        LayoutItemDragController dragController = new LayoutItemDragController(layoutControlItem2, 
        layoutControlItem1, MoveType.Inside, InsertLocation.After, LayoutType.Horizontal);
        layoutControlItem2.Move(dragController);
        */

        #endregion GridLayoutView Create

        #region tCardView Create
        //obj_vw_CardView
        public void tCardView_Create(DataRow row_Table, DataSet dsFields, DataSet dsData, GridControl tGridControl, string TableIPCode)
        {
            tToolBox t = new tToolBox();
            tEventsGrid evg = new tEventsGrid();

            // Tablo hakkındaki bilgiler
            //DataRow row_Table = dsTable.Tables[0].Rows[0];

            #region CardtView ve Columns İşlemleri

            DevExpress.XtraGrid.Views.Card.CardView tGridView = null;

            // Gelen GridControlu içinde View yok ise yeni bir View ekleyelim
            // var ise olanı kullanalım
            if ((tGridControl.MainView == null) ||
                (tGridControl.MainView.ToString() != "DevExpress.XtraGrid.Views.Card.CardView"))
            {
                int RefId = t.Set(row_Table[0].ToString(), "", 0);
                tGridView = new DevExpress.XtraGrid.Views.Card.CardView(tGridControl);

                tGridView.Name = "tGridView_" + RefId.ToString();
                tGridView.OptionsBehavior.AutoPopulateColumns = false;
                tGridView.OptionsView.ShowCardCaption = false;// true;
                tGridView.OptionsView.ShowCardExpandButton = false;// true;
                tGridView.OptionsView.ShowQuickCustomizeButton = false;
                tGridView.OptionsView.AnimationType = GridAnimationType.AnimateFocusedItem;
                tGridView.OptionsFilter.AllowFilterEditor = false;

                int find = t.Set(row_Table["DATA_FIND"].ToString(), "0", 0);
                tGridView.OptionsFind.AlwaysVisible = (find > 0);
                /// ( 100 * find )  neden bunu yapıyorum ?
                /// findDelay = 100 ise standart find
                /// findDelay = 200 ise list && data  yı işaret ediyor 
                tGridView.OptionsFind.FindDelay = 100 * find;
                tGridView.OptionsFind.FindMode = DevExpress.XtraEditors.FindMode.Always;

                tGridView.FocusedCardTopFieldIndex = 0;
                tGridView.GridControl = tGridControl;
                tGridView.VertScrollVisibility = DevExpress.XtraGrid.Views.Base.ScrollVisibility.Auto;

                tGridControl.BringToFront();
            }
            else
            {
                tGridView = (DevExpress.XtraGrid.Views.Card.CardView)tGridControl.MainView;
            }

            tGridView.DoubleClick += new System.EventHandler(evg.myGridView_DoubleClick);
            tGridView.KeyDown += new System.Windows.Forms.KeyEventHandler(evg.myGridView_KeyDown);
            tGridView.KeyUp += new System.Windows.Forms.KeyEventHandler(evg.myGridView_KeyUp);
            tGridView.KeyPress += new System.Windows.Forms.KeyPressEventHandler(evg.myGridView_KeyPress);
            tGridView.FocusedColumnChanged += new DevExpress.XtraGrid.Views.Base.FocusedColumnChangedEventHandler(evg.myGridView_FocusedColumnChanged);
            tGridView.CellValueChanging += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(evg.myGridView_CellValueChanging);

            tGridView.InvalidRowException += new DevExpress.XtraGrid.Views.Base.InvalidRowExceptionEventHandler(evg.myGridView_InvalidRowException);
            tGridView.ValidateRow += new DevExpress.XtraGrid.Views.Base.ValidateRowEventHandler(evg.myGridView_ValidateRow);

            tGridView.MouseMove += new System.Windows.Forms.MouseEventHandler(evg.myGridView_MouseMove);
            tGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(evg.myGridView_MouseDown);

            tGridView.CustomDrawCardCaption += new DevExpress.XtraGrid.Views.Card.CardCaptionCustomDrawEventHandler(evg.myCardView_CustomDrawCardCaption);

            tGridControl.ForceInitialize();

            // Columns
            tCardViewColumns_Create(dsFields, tGridView, TableIPCode);

            string Prop_View = t.Set(row_Table["PROP_VIEWS"].ToString(), "", "");

            string Old_PropView = string.Empty;

            if (t.IsNotNull(Prop_View))
            {
                /*
                PROP_VIEWS_IP packet = new PROP_VIEWS_IP();
                Prop_View = Prop_View.Replace((char)34, (char)39);
                //packet = JsonConvert.DeserializeAnonymousType(Prop_View, packet);
                */
                PROP_VIEWS_IP prop_ = t.readProp<PROP_VIEWS_IP>(Prop_View);

                if (prop_ != null)
                    Preparing_View_JSON(prop_, tGridControl, null, null, null, null, tGridView);
            }

            tGridControl.MainView = tGridView;

            if (dsData != null)
                if (dsData.Tables.Count > 0)
                    tGridControl.DataSource = dsData.Tables[0];

            #endregion CardView ve Columns İşlemleri
        }

        #region CardViewColumns Create

        private void tCardViewColumns_Create(DataSet dsFields,
                DevExpress.XtraGrid.Views.Card.CardView tGridView, string TableIPCode)
        {
            tToolBox t = new tToolBox();
            tDevColumn dc = new tDevColumn();

            string tfield_name = string.Empty;
            string tcaption = string.Empty;
            string tTableLabel = string.Empty;
            string tcolumn_type = string.Empty;

            Boolean tvisible = false;
            Int16 i = 0;
            int tGrpNo = -1;
            int tGrpLineNo = -1;
            int tfieldtype = 0;

            //this.cardView1 = new DevExpress.XtraGrid.Views.Card.CardView();
            //this.gridColumn1 = new DevExpress.XtraGrid.Columns.GridColumn();

            foreach (DataRow Row in dsFields.Tables[0].Rows)
            {
                tvisible = t.Set(Row["CMP_VISIBLE"].ToString(), Row["LKP_FVISIBLE"].ToString(), true);

                if (tvisible)
                {
                    tfield_name = t.Set(Row["LKP_FIELD_NAME"].ToString(), "", "null");
                    tcaption = t.Set(Row["FCAPTION"].ToString(), Row["LKP_FCAPTION"].ToString(), tfield_name);
                    tfieldtype = t.Set(Row["LKP_FIELD_TYPE"].ToString(), "", (int)167);
                    tcolumn_type = t.Set(Row["CMP_COLUMN_TYPE"].ToString(), Row["LKP_CMP_COLUMN_TYPE"].ToString(), "TextEdit");

                    //GridColumn column = tGridView.Columns.AddField(tfield_name);
                    GridColumn column = new DevExpress.XtraGrid.Columns.GridColumn();
                    column.FieldName = tfield_name;
                    column.Caption = t.Set(Row["FCAPTION"].ToString(), Row["LKP_FCAPTION"].ToString(), tfield_name);
                    column.Name = "Column_" + tfield_name;
                    column.Visible = tvisible;
                    column.VisibleIndex = i;
                    //column.OptionsColumn.AllowEdit = t.Set(Row["CMP_ENABLED"].ToString(), Row["LKP_FENABLED"].ToString(), true);
                    //column.OptionsFilter.AllowFilter = false;

                    //column.Width = t.Set(Row["CMP_WIDTH"].ToString(), Row["LKP_CMP_WIDTH"].ToString(), 100);

                    tGrpNo = t.Set(Row["GROUP_NO"].ToString(), Row["LKP_GROUP_NO"].ToString(), (int)0);
                    tGrpLineNo = t.Set(Row["GROUP_LINE_NO"].ToString(), Row["LKP_GROUP_LINE_NO"].ToString(), i);

                    dc.Grid_ColumnEdit(Row, column, null, tcolumn_type, TableIPCode);

                    tGridView.Columns.Add(column);

                    i++;
                }

            }
        }


        /// <summary>
        /// Buraya gerek varmı yok mu şimdilik bilmiyorum  
        /// </summary>
        //private void tLayoutControlGroup_Add(DataSet dsFields, DevExpress.XtraGrid.Views.Layout.LayoutView tGridView)
        private void tCardViewGroup_Add(DataSet dsFields, DevExpress.XtraGrid.Views.Layout.LayoutView tGridView)
        {
            tToolBox t = new tToolBox();
            //tEvents ev = new tEvents();

            #region Tanımlar

            string caption = string.Empty;
            string groupno = string.Empty;
            string visible = string.Empty;
            string fixedbn = string.Empty;
            string TableCode1 = string.Empty;
            string TableCode2 = "none";

            Boolean tTabPage = false;
            Boolean tvisible = false;
            Int16 group_types = 0;
            Int16 fgroup_no = 0;
            Int16 fgroup_line_no = 0;
            Int16 j = 0;

            vTableAbout vTA = new vTableAbout();
            t.Table_FieldsGroups_Count(vTA, dsFields);
            int gc = vTA.GroupsCount;

            TabbedControlGroup LCTabbedGroup = null;

            #endregion Tanımlar

            #region Group var ise
            // Group bilgileri mevcut
            if (gc > 0)
            {
                foreach (DataRow Row in dsFields.Tables[1].Rows)
                {
                    TableCode1 = t.Set(Row["TABLE_CODE"].ToString(), "", "");

                    // Group sırası önce MS_FIELDS_IP üzerinde tanımlı olanlar
                    // sonra MS_FIELDS üzerinde tanımlı olan gruplar listelenir

                    // birbirinden ayıran fark TABLE_CODE fieldi
                    // örnek tablo çıktısı aşağıda

                    // MS_FIELDS_ID üzerindekiler varsa açılacak
                    // yoksa MS_FIELDS üzerindekiler açılacak
                    if ((TableCode1 != TableCode2) && (TableCode2 != "none"))
                    {
                        break;
                    }

                    TableCode2 = TableCode1;

                    caption = t.Set(Row["FCAPTION"].ToString(), "", "");
                    groupno = t.Set(Row["FGROUPNO"].ToString(), "", "");
                    visible = t.Set(Row["FVISIBLE"].ToString(), "", "");
                    fixedbn = t.Set(Row["FIXED"].ToString(), "", "");
                    group_types = t.Set(Row["GROUP_TYPES"].ToString(), "", (Int16)0);

                    #region // GroupPanel ise
                    if ((tGridView != null) && (group_types <= 1))
                    {
                        LayoutControlGroup LCGroup = new LayoutControlGroup();
                        LCGroup.Name = "LCGroup_" + groupno;
                        LCGroup.Text = caption;
                        tGridView.TemplateCard.AddGroup(LCGroup);

                        j = 0;
                        for (int i = 0; i < dsFields.Tables[0].Rows.Count; i++)
                        {
                            tvisible = t.Set(dsFields.Tables[0].Rows[i]["CMP_VISIBLE"].ToString(), dsFields.Tables[0].Rows[i]["LKP_FVISIBLE"].ToString(), true);
                            fgroup_no = t.Set(dsFields.Tables[0].Rows[i]["GROUP_NO"].ToString(), dsFields.Tables[0].Rows[i]["LKP_GROUP_NO"].ToString(), (Int16)0);
                            fgroup_line_no = t.Set(dsFields.Tables[0].Rows[i]["GROUP_LINE_NO"].ToString(), dsFields.Tables[0].Rows[i]["LKP_GROUP_LINE_NO"].ToString(), (Int16)0); ;
                            if (fgroup_no > 0)
                            {
                                if (t.myInt32(groupno) == fgroup_no)
                                {
                                    LCGroup.Add(tGridView.Columns[j].LayoutViewField);
                                }
                            }
                            if (tvisible) j++;
                        }
                    }
                    #endregion // GroupPanel ise

                    #region // TabPage ise
                    if ((tGridView != null) && (group_types == 2))
                    {
                        if (LCTabbedGroup == null)
                        {
                            //TabbedControlGroup LCTabbedGroup = new TabbedControlGroup();
                            LCTabbedGroup = new TabbedControlGroup(); // AddTabbedGroup();
                            LCTabbedGroup.Name = "LCTabbed_" + groupno;
                            LCTabbedGroup.Text = caption;
                            tGridView.TemplateCard.AddTabbedGroup(LCTabbedGroup);
                            tTabPage = true;
                        }

                        /*
                        // Create a tabbed group within the root group.
                           TabbedControlGroup tabbedGroup = lc.Root.AddTabbedGroup();
                           tabbedGroup.Name = "TabbedGroup";
                        // Add a new group as a tab to the tabbed group.
                           LayoutControlGroup group1 = tabbedGroup.AddTabPage() as LayoutControlGroup;
                           group1.Name = "LayoutControlGroup1";
                           group1.Text = "Photo";
                        */

                        LayoutControlGroup tabbedGroup = LCTabbedGroup.AddTabPage() as LayoutControlGroup;
                        tabbedGroup.Name = "LCTabbedGroup_" + groupno;
                        tabbedGroup.Text = caption;

                        j = 0;
                        for (int i = 0; i < dsFields.Tables[0].Rows.Count; i++)
                        {
                            tvisible = t.Set(dsFields.Tables[0].Rows[i]["CMP_VISIBLE"].ToString(), dsFields.Tables[0].Rows[i]["LKP_FVISIBLE"].ToString(), true);
                            fgroup_no = t.Set(dsFields.Tables[0].Rows[i]["GROUP_NO"].ToString(), dsFields.Tables[0].Rows[i]["LKP_GROUP_NO"].ToString(), (Int16)0);
                            fgroup_line_no = t.Set(dsFields.Tables[0].Rows[i]["GROUP_LINE_NO"].ToString(), dsFields.Tables[0].Rows[i]["LKP_GROUP_LINE_NO"].ToString(), (Int16)0); ;
                            if (fgroup_no > 0)
                            {
                                if ((t.myInt32(groupno) == fgroup_no) && (tvisible))
                                {
                                    tabbedGroup.AddItem(tGridView.Columns[j].LayoutViewField);
                                }
                            }
                            if (tvisible) j++;
                        }
                    }
                    #endregion // TabPage ise

                }

                if (tTabPage)
                {
                    LCTabbedGroup.SelectedTabPageIndex = 0;
                }
            }
            #endregion Group var ise

        }

        #endregion CardViewColumns Columns Create

        #endregion CardView Create

        #region tVGridControl Create Base

        public void tVGrid_Create(DataRow row_Table, DataSet dsFields, DataSet dsData,
            DevExpress.XtraVerticalGrid.VGridControl tVGridControl, byte viewType)
        {
            tToolBox t = new tToolBox();
            tEventsGrid evg = new tEventsGrid();

            // Tablo hakkındaki bilgiler
            //DataRow Row = dsTable.Tables[0].Rows[0];

            #region VGridControl ve Columns İşlemleri

            // Tablo hakkındaki bilgiler
            //DataRow row_Table = dsTable.Tables[0].Rows[0];

            string SoftwareCode = row_Table["SOFTWARE_CODE"].ToString();
            string ProjectCode = row_Table["PROJECT_CODE"].ToString();
            string TableIPCode = row_Table["TABLE_CODE"].ToString() + "." + row_Table["IP_CODE"].ToString();
            if (t.IsNotNull(SoftwareCode))
                TableIPCode = SoftwareCode + "/" + ProjectCode + "/" + TableIPCode;
                                   
            // Add Bands >> Gridin Üzerindeki Bands ları oluştur
            GridBands_Add(null, tVGridControl, null, dsFields, TableIPCode);

            // VGridControl in Column ları ekleniyor  
            tVGridControl_Columns_Create(dsFields, tVGridControl, viewType);

            #endregion VGridControl ve Columns İşlemleri

            #region tVGridControl ait properties ve events bağlantıları hazırlanıyor

            tVGridControl.DoubleClick += new System.EventHandler(evg.myGridView_DoubleClick);
            tVGridControl.Enter += new System.EventHandler(evg.myVGridControl_Enter);
            tVGridControl.Leave += new System.EventHandler(evg.myVGridControl_Leave);

            tVGridControl.CellValueChanging += evg.myVGridControl_CellValueChanging;
            tVGridControl.CellValueChanged += evg.myVGridControl_CellValueChanged;

            //tVGridControl.GotFocus += new System.EventHandler(evg.myGridView_GotFocus);
            //tVGridControl.KeyDown += new System.Windows.Forms.KeyEventHandler(evg.myGridView_KeyDown);
            //tVGridControl.KeyUp += new System.Windows.Forms.KeyEventHandler(evg.myGridView_KeyUp);
            //tVGridControl.KeyPress += new System.Windows.Forms.KeyPressEventHandler(evg.myGridView_KeyPress);


            string Prop_View = t.Set(row_Table["PROP_VIEWS"].ToString(), "", "");
            PROP_VIEWS_IP JSON_PropView = null;
            string Old_PropView = string.Empty;

            if (t.IsNotNull(Prop_View))
            {
                Preparing_PropView_(Prop_View, ref Old_PropView, ref JSON_PropView);

                if (JSON_PropView != null)
                    Preparing_View_JSON(JSON_PropView, tVGridControl, null, null, null, null, null);
            }

            //tVGridControl.OptionsView.
            //tVGridControl
            /*
            tGridView.OptionsView.ColumnAutoWidth = false;
            tGridView.OptionsView.RowAutoHeight = true;
            tGridView.OptionsFind.AlwaysVisible = t.Set(Row["DATA_FIND"].ToString(), "", false);

            GridView_Properties(tGridView, Row["CMP_PROPERTIES"].ToString() + "\r");

            
            tGridView.InvalidRowException += new DevExpress.XtraGrid.Views.Base.InvalidRowExceptionEventHandler(evg.myGridView_InvalidRowException);
            tGridView.ValidateRow += new DevExpress.XtraGrid.Views.Base.ValidateRowEventHandler(evg.myGridView_ValidateRow);

            if (tGridView.OptionsView.NewItemRowPosition.ToString() == "Top")
            {
                tGridView.InitNewRow += new DevExpress.XtraGrid.Views.Grid.InitNewRowEventHandler(ev.myGridView_InitNewRow);
                tGridView.BeforeLeaveRow += new DevExpress.XtraGrid.Views.Base.RowAllowEventHandler(ev.myGridView_BeforeLeaveRow);
            }
            */

            tVGridControl.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

            if (dsData != null)
                if (dsData.Tables.Count > 0)
                    tVGridControl.DataSource = dsData.Tables[0];

            //tVGridControl.EndUpdate();

            #endregion tVGridControl ait properties ve events bağlantıları

        }

        
        #endregion tVGridControl Create Base

        #region tTreeList Create Base
        public void tTreeListView_Create(DataRow row_Table, DataSet dsFields, DataSet dsData,
            DevExpress.XtraTreeList.TreeList tTreeList, string TableIPCode)
        {
            tToolBox t = new tToolBox();
            tEventsGrid evg = new tEventsGrid();

            // Tablo hakkındaki bilgiler
            //DataRow Row = dsTable.Tables[0].Rows[0];

            #region TreeList ve Columns İşlemleri

            string Key_FName = t.Set(row_Table["LKP_KEY_FNAME"].ToString(), "", "");
            string Master_Key_FName = t.Set(row_Table["MASTER_KEY_FNAME"].ToString(), row_Table["LKP_MASTER_KEY_FNAME"].ToString(), "");
            string Foreing_FName = t.Set(row_Table["FOREING_FNAME"].ToString(), row_Table["LKP_FOREING_FNAME"].ToString(), "");
            string Parent_FName = t.Set(row_Table["PARENT_FNAME"].ToString(), row_Table["LKP_PARENT_FNAME"].ToString(), "");

            if (t.IsNotNull(Master_Key_FName))
                Key_FName = Master_Key_FName;

            tTreeList.KeyFieldName = Key_FName;
            tTreeList.ParentFieldName = Parent_FName;

            int find = t.Set(row_Table["DATA_FIND"].ToString(), "0", 0);
            /// ( 100 * find )  neden bunu yapıyorum ?
            /// findDelay = 100 ise standart find
            /// findDelay = 200 ise list && data  yı işaret ediyor 
            tTreeList.OptionsFind.FindDelay = 100 * find;
            tTreeList.OptionsFind.AlwaysVisible = (find > 0);
            tTreeList.OptionsFind.AllowFindPanel = (find > 0);
            tTreeList.OptionsFind.FindMode = DevExpress.XtraTreeList.FindMode.Always;
            tTreeList.OptionsFilter.FilterMode = DevExpress.XtraTreeList.FilterMode.Smart;

            tTreeList.OptionsNavigation.EnterMovesNextColumn = true;

            tTreeList.OptionsSelection.InvertSelection = (find == 0);
            tTreeList.OptionsSelection.UseIndicatorForSelection = true;

            //geçici
            tTreeList.OptionsView.EnableAppearanceEvenRow = true;
            tTreeList.OptionsView.EnableAppearanceOddRow = true;
            //---

            string Prop_View = t.Set(row_Table["PROP_VIEWS"].ToString(), "", "");
            PROP_VIEWS_IP JSON_PropView = null; //new PROP_VIEWS_IP();
            string Old_PropView = string.Empty;

            //if (t.IsNotNull(Prop_View))
            //    Preparing_View_(Prop_View, null, null, tTreeList, null);

            if (t.IsNotNull(Old_PropView))
                Preparing_View_OLD(Old_PropView, null, null, tTreeList, null);

            if (JSON_PropView != null)
                Preparing_View_JSON(JSON_PropView, tTreeList, null, null, tTreeList, null, null);

            // TreeListView in Column ları ekleniyor  
            tTreeList_Columns_Create(dsFields, tTreeList, TableIPCode);

            //tTreeList.KeyDown += new System.Windows.Forms.KeyEventHandler(evg.myTreeList_KeyDown);
            tTreeList.DoubleClick += new System.EventHandler(evg.myGridView_DoubleClick);
            tTreeList.KeyDown += new System.Windows.Forms.KeyEventHandler(evg.myTreeList_KeyDown);
            tTreeList.KeyUp += new System.Windows.Forms.KeyEventHandler(evg.myGridView_KeyUp);
            tTreeList.KeyPress += new System.Windows.Forms.KeyPressEventHandler(evg.myGridView_KeyPress);




            #endregion TreeList ve Columns İşlemleri

            #region tTreeList ait properties ve events bağlantıları hazırlanıyor
            /*
            tGridView.OptionsView.ColumnAutoWidth = false;
            tGridView.OptionsView.RowAutoHeight = true;
            tGridView.OptionsFind.AlwaysVisible = t.Set(Row["DATA_FIND"].ToString(), "", false);

            GridView_Properties(tGridView, Row["CMP_PROPERTIES"].ToString() + "\r");

            tGridView.DoubleClick += new System.EventHandler(ev.myGridView_DoubleClick);
            tGridView.KeyUp += new System.Windows.Forms.KeyEventHandler(ev.myGridView_KeyUp);
            tGridView.KeyPress += new System.Windows.Forms.KeyPressEventHandler(ev.myGridView_KeyPress);

            tGridView.InvalidRowException += new DevExpress.XtraGrid.Views.Base.InvalidRowExceptionEventHandler(ev.myGridView_InvalidRowException);
            tGridView.ValidateRow += new DevExpress.XtraGrid.Views.Base.ValidateRowEventHandler(ev.myGridView_ValidateRow);

            if (tGridView.OptionsView.NewItemRowPosition.ToString() == "Top")
            {
                tGridView.InitNewRow += new DevExpress.XtraGrid.Views.Grid.InitNewRowEventHandler(ev.myGridView_InitNewRow);
                tGridView.BeforeLeaveRow += new DevExpress.XtraGrid.Views.Base.RowAllowEventHandler(ev.myGridView_BeforeLeaveRow);
            }
            */
            if (dsData != null)
                if (dsData.Tables.Count > 0)
                    tTreeList.DataSource = dsData.Tables[0];

            #endregion tTreeList ait properties ve events bağlantıları

        }
        #endregion TreeList Create Base

        #region tDataLayoutView, tDataWizard Create Base

        public void tDataWizard_Create(DataRow row_Table, DataSet dsFields, DataSet dsData,
               DevExpress.XtraBars.Navigation.NavigationPane tPanelControl,
               byte ViewType, string FormName, vTableAbout vTA)
        {
            tToolBox t = new tToolBox();

            ///
            /// tPanelControl.GetType().ToString() == "DevExpress.XtraBars.Navigation.NavigationPane"
            ///

            int tableCount = vTA.TablesCount;
            int fieldCount = vTA.FieldsCount;
            int groupCount = vTA.GroupsCount;

            #region tPanelControl
            if (tPanelControl.GetType().ToString() == "DevExpress.XtraBars.Navigation.NavigationPane")
            {
                tPanelControl.BringToFront();

                // hiç grup yok ise en azından 1 adet page oluşturmak gerekiyor
                int navPageCount = 0;

                #region groupCount
                if (groupCount > 0)
                {
                    //int tGroupPanelCount = 0;
                    //int tDLTabPageCount = 0;
                    //int tTabPagesCount = 0;

                    //t.TableGroup_About(dsFields, ref tGroupPanelCount, ref tDLTabPageCount, ref tTabPagesCount);

                    // Eğer groups listesinde (GROUP_TYPES, 6, 'TabPages') var ise o kadar navigationPage açılacak
                    // yok sadece 1 adet navigationPage açılacak 

                    navPageCount = vTA.dWTabPageCount;

                    if (navPageCount == 0)
                        navPageCount = vTA.GroupsCount;

                    if (navPageCount == 0) navPageCount = 1;
                }
                #endregion

                string TableIPCode = tPanelControl.AccessibleName;

                string gcaption = string.Empty;
                //string groupno = string.Empty;
                bool gvisible = true;
                int gtypes = 0;

                tCreateObject co = new tCreateObject();

                #region for
                for (int i = 1; i < navPageCount + 1; i++)
                {
                    if (navPageCount > 1)
                    {
                        gcaption = t.Set(dsFields.Tables[1].Rows[i - 1]["FCAPTION"].ToString(), "", "");
                        //groupno  = t.Set(dsFields.Tables[1].Rows[i - 1]["FGROUPNO"].ToString(), "", "");
                        gvisible = t.Set(dsFields.Tables[1].Rows[i - 1]["FVISIBLE"].ToString(), "", true);
                        gtypes = t.Set(dsFields.Tables[1].Rows[i - 1]["GROUP_TYPES"].ToString(), "", (Int16)0);
                    }

                    #region
                    if (gvisible)
                    {
                        #region navigationPage1
                        //
                        // navigationPage1
                        // 
                        DevExpress.XtraBars.Navigation.NavigationPage navigationPage1 = new DevExpress.XtraBars.Navigation.NavigationPage();
                        navigationPage1.Name = "navigationPage_" + i.ToString();// + RefId.ToString();
                        navigationPage1.Size = new System.Drawing.Size(tPanelControl.Width, tPanelControl.Height);
                        navigationPage1.Properties.ShowCollapseButton = DevExpress.Utils.DefaultBoolean.False;
                        navigationPage1.Properties.ShowExpandButton = DevExpress.Utils.DefaultBoolean.False;
                        //navigationPage1.Padding = new System.Windows.Forms.Padding(1);
                        navigationPage1.PageVisible = true;
                        #endregion navigationPage1

                        #region tableLayoutPanel1
                        //
                        // tableLayoutPanel1
                        // 
                        System.Windows.Forms.TableLayoutPanel tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
                        tableLayoutPanel1.SuspendLayout();
                        //tableLayoutPanel1.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;
                        tableLayoutPanel1.ColumnCount = 3;
                        tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
                        tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
                        tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
                        tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
                        tableLayoutPanel1.Location = new System.Drawing.Point(2, 2);
                        tableLayoutPanel1.Name = "tableLayoutPanel_" + i.ToString();
                        tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(8);
                        tableLayoutPanel1.RowCount = 1;
                        tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
                        //tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
                        tableLayoutPanel1.Size = new System.Drawing.Size(200, 200);
                        tableLayoutPanel1.TabIndex = 0;
                        tableLayoutPanel1.Visible = true;

                        navigationPage1.Controls.Add(tableLayoutPanel1);

                        #endregion

                        /// yeni DataLayoutControl oluştur
                        /// 
                        Control newDataLayout = Create_View_(v.obj_vw_DataLayoutView, i, TableIPCode, "");

                        //navigationPage1.Controls.Add(newDataLayout);
                        tableLayoutPanel1.Controls.Add(newDataLayout, 1, 0);
                        tableLayoutPanel1.ResumeLayout(false);

                        tPanelControl.Pages.Add(navigationPage1);

                        #region // DataLayoutControl in Column ları ekleniyor  
                        if (navPageCount != 1)
                        {
                            navigationPage1.Caption = gcaption; // "navigationPage_" + i.ToString();
                            navigationPage1.PageText = " " + i.ToString() + " ";

                            tDataLayoutControl_Columns_Create(dsFields, dsData,
                                ((DevExpress.XtraDataLayout.DataLayoutControl)newDataLayout), ViewType, FormName, "", i);
                        }
                        else
                        {
                            navigationPage1.Caption = "?";
                            navigationPage1.PageText = " ? ";

                            tDataLayoutControl_Columns_Create(dsFields, dsData,
                                ((DevExpress.XtraDataLayout.DataLayoutControl)newDataLayout), ViewType, FormName, "", -1);
                        }
                        #endregion

                        #region // Groups or TabPage add, and Move function

                        //((DevExpress.XtraDataLayout.DataLayoutControl)newDataLayout).BeginUpdate();

                        // DataLayoutControl in Group ları ve TabPage leri ekleniyor

                        ////tDataLayoutControlGroup_Add(dsFields, ((DevExpress.XtraDataLayout.DataLayoutControl)newDataLayout));
                        ////tColumnAndGroup_Move(dsFields, null, ((DevExpress.XtraDataLayout.DataLayoutControl)newDataLayout));

                        ////((DevExpress.XtraDataLayout.DataLayoutControl)newDataLayout).EndUpdate();

                        #endregion // Groups or TabPage add, and Move function

                        ((DevExpress.XtraDataLayout.DataLayoutControl)newDataLayout).EndUpdate();
                        //((System.ComponentModel.ISupportInitialize)((DevExpress.XtraDataLayout.DataLayoutControl)newDataLayout)).EndInit();
                        //((DevExpress.XtraDataLayout.DataLayoutControl)newDataLayout).ResumeLayout(false);

                        ((DevExpress.XtraDataLayout.DataLayoutControl)newDataLayout).Dock = DockStyle.Fill;
                        ((DevExpress.XtraDataLayout.DataLayoutControl)newDataLayout).BringToFront();

                        if (dsData != null)
                            if (dsData.Tables.Count > 0)
                                ((DevExpress.XtraDataLayout.DataLayoutControl)newDataLayout).DataSource = dsData.Tables[0];

                    }// gvisible
                    #endregion
                }
                #endregion for

            }
            #endregion tPanelControl
        }

        public void tDataLayoutView_Create(DataRow row_Table, DataSet dsFields, DataSet dsData,
               DevExpress.XtraDataLayout.DataLayoutControl tDLayout, byte ViewType, string FormName)
        {
            tToolBox t = new tToolBox();
            //tEvents ev = new tEvents();

            // ViewType
            // 1 = Standart 
            // 2 = SpeedKriter

            // Tablo hakkındaki bilgiler
            //DataRow Row = dsTable.Tables[0].Rows[0];

            #region DataLayoutControlun Columns, Group ve TabPage İşlemleri

            tDLayout.BeginUpdate();

            // group create
            tDataLayout_Group_Add(dsFields, tDLayout);

            string External_TableIPCode = t.Set(row_Table["EXTERNAL_IP_CODE"].ToString(), "", "");

            // DataLayoutControl in Column ları ekleniyor  
            tDataLayoutControl_Columns_Create(dsFields, dsData, tDLayout, ViewType, FormName, External_TableIPCode, - 1);

            string Prop_View = t.Set(row_Table["PROP_VIEWS"].ToString(), "", "");
            PROP_VIEWS_IP JSON_PropView = null;
            string Old_PropView = string.Empty;

            if (t.IsNotNull(Prop_View))
                Preparing_PropView_(Prop_View, ref Old_PropView, ref JSON_PropView);

            if (JSON_PropView != null)
                Preparing_View_JSON(JSON_PropView, tDLayout);

            tDLayout.EndUpdate();

            #region // Groups or TabPage add, and Move function
            //tDLayout.BeginUpdate();

            // DataLayoutControl in Group ları ve TabPage leri ekleniyor
            /// tDataLayoutControlGroup_Add(dsFields, tDLayout);

            tColumnAndGroup_Move(dsFields, null, tDLayout);

            tDLayout.EndUpdate();


            #endregion // Groups or TabPage add, and Move function

            #endregion DataLayoutControlun Columns, Group ve TabPage İşlemleri

            if (dsData != null)
                if (dsData.Tables.Count > 0)
                    tDLayout.DataSource = dsData.Tables[0];

            tDLayout.BestFit();
        }


        private void tDataLayout_Group_Add(DataSet dsFields, DevExpress.XtraDataLayout.DataLayoutControl tDLayout)
        {
            tToolBox t = new tToolBox();

            #region Tanımlar

            string fname = string.Empty;
            string caption = string.Empty;
            string visible = string.Empty;
            string fixedbn = string.Empty;
            string TableCode1 = string.Empty;
            string TableCode2 = "none";
            string Prop_Views = string.Empty;

            Int16 groupNo = 0;
            Int16 group_types = 0;

            int fieldCount = 0;
            int groupCount = 0;
            int grpCount = 0;

            vTableAbout vTA = new vTableAbout();
            t.Table_FieldsGroups_Count(vTA, dsFields);
            fieldCount = vTA.FieldsCount;
            groupCount = vTA.GroupsCount;

            LayoutControlGroup LCGroup = null;
            TabbedControlGroup LCTabbed_ = null;

            #endregion Tanımlar

            #region Group var ise
            // Group bilgileri mevcut
            if (groupCount > 0)
            {
                //tDLayout.BeginUpdate();

                // layoutControlGroup1 için padding sıfırlanıyor
                tDLayout.Root.Padding = new DevExpress.XtraLayout.Utils.Padding(0);

                // Açılacak Group veya TabPage döngüsü
                foreach (DataRow Row in dsFields.Tables[1].Rows)
                {
                    TableCode1 = t.Set(Row["TABLE_CODE"].ToString(), "", "");

                    // Group sırası önce MS_FIELDS_IP üzerinde tanımlı olanlar
                    // sonra MS_FIELDS üzerinde tanımlı olan gruplar listelenir

                    // birbirinden ayıran fark TABLE_CODE fieldi
                    // örnek tablo çıktısı aşağıda

                    // MS_FIELDS_ID üzerindekiler varsa açılacak
                    // yoksa MS_FIELDS üzerindekiler açılacak
                    if ((TableCode1 != TableCode2) && (TableCode2 != "none"))
                    {
                        break;
                    }

                    TableCode2 = TableCode1;

                    caption = t.Set(Row["FCAPTION"].ToString(), "", "");
                    groupNo = t.myInt16(Row["FGROUPNO"].ToString());
                    visible = t.Set(Row["FVISIBLE"].ToString(), "", "");
                    fixedbn = t.Set(Row["FIXED"].ToString(), "", "");
                    group_types = t.Set(Row["GROUP_TYPES"].ToString(), "", (Int16)0);
                    Prop_Views = t.Set(Row["PROP_VIEWS"].ToString(), "", "");

                    /// groupNo == 0     : tDLayout yani root işaret ediyor
                    /// groupNo > 0      : ya LayoutControlGroup yada TabbedControlGroup olduğunu işaret ediyor
                    /// group_types == 1 : LayoutControlGroup
                    /// group_types == 2 : TabbedControlGroup

                    #region // Root ise
                    if ((groupNo == 0) && (tDLayout != null) && (visible == "True"))
                    {
                        if (t.IsNotNull(Prop_Views))
                            DataLayout_TableLayoutMode_Preparing(Prop_Views, tDLayout, null, null);
                    }
                    #endregion

                    #region // GroupPanel ise
                    if ((groupNo > 0) && (tDLayout != null) && (group_types == 1) && (visible == "True"))
                    {
                        LCGroup = new LayoutControlGroup();
                        LCGroup.Name = "LCGroup_" + groupNo.ToString();
                        LCGroup.Text = caption;
                        LCGroup.Padding = new DevExpress.XtraLayout.Utils.Padding(8);
                        

                        //LCGroup.ExpandButtonVisible = true;
                        //LCGroup.AllowBorderColorBlending = true;
                        //LCGroup.AppearanceGroup.BorderColor = System.Drawing.Color.YellowGreen;
                        //LCGroup.GroupBordersVisible = false;

                        if (t.IsNotNull(Prop_Views))
                            DataLayout_TableLayoutMode_Preparing(Prop_Views, null, null, LCGroup);

                        grpCount++;
                        tDLayout.Root.AddGroup(LCGroup);
                    }
                    #endregion // GroupPanel ise

                    #region // TabPage ise
                    if ((groupNo > 0) && (tDLayout != null) && (group_types == 2) && (visible == "True"))
                    {
                        if (LCTabbed_ == null)
                        {
                            LCTabbed_ = tDLayout.Root.AddTabbedGroup();
                            //LCTabbed_ = new TabbedControlGroup();
                            LCTabbed_.Name = "LCTabbed_" + groupNo.ToString();
                            LCTabbed_.Text = caption;
                            LCTabbed_.Padding = new DevExpress.XtraLayout.Utils.Padding(8);

                            //tDLayout.Root.AddTabbedGroup(LCTabbed_);
                        }

                        LCGroup = LCTabbed_.AddTabPage() as LayoutControlGroup;

                        //LCTabbed.BeginUpdate();
                        LCGroup.Name = "LCGroup_" + groupNo.ToString();
                        LCGroup.Text = caption;

                        if (t.IsNotNull(Prop_Views))
                            DataLayout_TableLayoutMode_Preparing(Prop_Views, null, LCTabbed_, LCGroup);

                        //LCTabbed.EndUpdate();
                    }
                    #endregion // TabPage ise

                }

                if (LCTabbed_ != null)
                {
                    LCTabbed_.SelectedTabPageIndex = 0;
                    //if (grpCount == 0) // sadece tabpage olursa ekle
                    //    tDLayout.Root.AddItem(new EmptySpaceItem());
                }

                tDLayout.Padding = new System.Windows.Forms.Padding(0);
                //tDLayout.EndUpdate();
                //tDLayout.BestFit();
            }
            #endregion Group var ise

        }
        // işi bitti, kullanılmıyor
        private void tDataLayoutControlGroup_Add(DataSet dsFields, DevExpress.XtraDataLayout.DataLayoutControl tDLayout)
        {
            tToolBox t = new tToolBox();
            //tEvents ev = new tEvents();

            #region Tanımlar

            string fname = string.Empty;
            string caption = string.Empty;
            string visible = string.Empty;
            string fixedbn = string.Empty;
            string TableCode1 = string.Empty;
            string TableCode2 = "none";
            string Prop_Views = string.Empty;

            Int16 groupNo = 0;
            Int16 group_types = 0;

            int fieldCount = 0;
            int groupCount = 0;
            int grpCount = 0;

            vTableAbout vTA = new vTableAbout();
            t.Table_FieldsGroups_Count(vTA, dsFields);
            fieldCount = vTA.FieldsCount;
            groupCount = vTA.GroupsCount;

            LayoutControlGroup LCGroup = null;
            TabbedControlGroup LCTabbedGroup = null;

            #endregion Tanımlar

            #region Group var ise
            // Group bilgileri mevcut
            if (groupCount > 0)
            {
                //tDLayout.BeginUpdate();

                // Açılacak Group veya TabPage döngüsü
                foreach (DataRow Row in dsFields.Tables[1].Rows)
                {
                    TableCode1 = t.Set(Row["TABLE_CODE"].ToString(), "", "");

                    // Group sırası önce MS_FIELDS_IP üzerinde tanımlı olanlar
                    // sonra MS_FIELDS üzerinde tanımlı olan gruplar listelenir

                    // birbirinden ayıran fark TABLE_CODE fieldi
                    // örnek tablo çıktısı aşağıda

                    // MS_FIELDS_ID üzerindekiler varsa açılacak
                    // yoksa MS_FIELDS üzerindekiler açılacak
                    if ((TableCode1 != TableCode2) && (TableCode2 != "none"))
                    {
                        break;
                    }

                    TableCode2 = TableCode1;

                    caption = t.Set(Row["FCAPTION"].ToString(), "", "");
                    groupNo = t.myInt16(Row["FGROUPNO"].ToString());
                    visible = t.Set(Row["FVISIBLE"].ToString(), "", "");
                    fixedbn = t.Set(Row["FIXED"].ToString(), "", "");
                    group_types = t.Set(Row["GROUP_TYPES"].ToString(), "", (Int16)0);
                    Prop_Views = t.Set(Row["PROP_VIEWS"].ToString(), "", "");

                    /// groupNo == 0     : tDLayout yani root işaret ediyor
                    /// groupNo > 0      : ya LayoutControlGroup yada TabbedControlGroup olduğunu işaret ediyor
                    /// group_types == 1 : LayoutControlGroup
                    /// group_types == 2 : TabbedControlGroup

                    #region // Root ise
                    if ((groupNo == 0) && (tDLayout != null) && (visible == "True"))
                    {
                        if (t.IsNotNull(Prop_Views))
                            DataLayout_TableLayoutMode_Preparing(Prop_Views, tDLayout, null, null);

                        DataLayout_Item_ColIndex_RowIndex_Preparing(dsFields, fieldCount, groupNo, tDLayout, null, null);
                    }
                    #endregion

                    #region // GroupPanel ise
                    if ((groupNo > 0) && (tDLayout != null) && (group_types == 1) && (visible == "True"))
                    {
                        LCGroup = tDLayout.Root.AddGroup();
                        //LCGroup = new LayoutControlGroup();
                        LCGroup.Name = "LCGroup_" + groupNo.ToString();
                        LCGroup.Text = caption;

                        LCGroup.ExpandButtonVisible = true;
                        //LCGroup.AllowBorderColorBlending = true;
                        //LCGroup.AppearanceGroup.BorderColor = System.Drawing.Color.YellowGreen;

                        grpCount++;
                        //tDLayout.Root.AddGroup(LCGroup);

                        if (t.IsNotNull(Prop_Views))
                            DataLayout_TableLayoutMode_Preparing(Prop_Views, null, null, LCGroup);

                        //DataLayout_Item_ColIndex_RowIndex_Preparing(dsFields, fieldCount, groupNo, tDLayout, LCGroup, null);
                    }
                    #endregion // GroupPanel ise

                    #region // TabPage ise
                    if ((groupNo > 0) && (tDLayout != null) && (group_types == 2) && (visible == "True"))
                    {
                        if (LCTabbedGroup == null)
                        {
                            LCTabbedGroup = tDLayout.Root.AddTabbedGroup();
                            //LCTabbedGroup = new TabbedControlGroup();
                            LCTabbedGroup.Name = "LCTabbed_" + groupNo.ToString();
                            LCTabbedGroup.Text = caption;
                            //tDLayout.Root.AddTabbedGroup(LCTabbedGroup);
                        }

                        LayoutControlGroup tabbedGroup = LCTabbedGroup.AddTabPage() as LayoutControlGroup;

                        //tabbedGroup.BeginUpdate();
                        tabbedGroup.Name = "LCTabbedGroup_" + groupNo.ToString();
                        tabbedGroup.Text = caption;

                        if (t.IsNotNull(Prop_Views))
                            DataLayout_TableLayoutMode_Preparing(Prop_Views, null, LCTabbedGroup, tabbedGroup);

                        //DataLayout_Item_ColIndex_RowIndex_Preparing(dsFields, fieldCount, groupNo, tDLayout, null, tabbedGroup);

                        //tabbedGroup.EndUpdate();
                    }
                    #endregion // TabPage ise

                }

                if (LCTabbedGroup != null)
                {
                    LCTabbedGroup.SelectedTabPageIndex = 0;
                    //if (grpCount == 0) // sadece tabpage olursa ekle
                    //    tDLayout.Root.AddItem(new EmptySpaceItem());
                }

                tDLayout.Padding = new System.Windows.Forms.Padding(0);

                //tDLayout.EndUpdate();
                tDLayout.BestFit();
            }
            #endregion Group var ise

        }
        //--
        
        private void DataLayout_TableLayoutMode_Preparing(string Prop_Views,
            DevExpress.XtraDataLayout.DataLayoutControl tDLayout,
            TabbedControlGroup LCTabbedGroup,
            LayoutControlGroup LCGroup
            )
        {
            tToolBox t = new tToolBox();
            
            var prop_ = t.readProp<PROP_VIEWS_LAYOUT>(Prop_Views);

            List<TLP_COLUMNS> lst_TLP_COLUMNS = null;
            List<TLP_ROWS> lst_TLP_ROWS = null;

            string sizetype = string.Empty;
            string sizevalue = string.Empty;

            int count = 0;

            lst_TLP_COLUMNS = prop_.TLP.TLP_COLUMNS;
            lst_TLP_ROWS = prop_.TLP.TLP_ROWS;

            if ((tDLayout != null) && (lst_TLP_COLUMNS.Count > 0))
            {
                tDLayout.Root.LayoutMode = LayoutMode.Table;
                tDLayout.Root.OptionsTableLayoutGroup.ColumnDefinitions.Clear();
                tDLayout.Root.OptionsTableLayoutGroup.RowDefinitions.Clear();
            }

            #region LCTabbedGroup
            if (LCTabbedGroup != null)
            {
                int colNo = t.myInt32(t.Set(prop_.TLP.TLP_COLNO.ToString(), "-1", "-1"));
                int rowNo = t.myInt32(t.Set(prop_.TLP.TLP_ROWNO.ToString(), "-1", "-1"));
                int colSpan = t.myInt32(t.Set(prop_.TLP.TLP_COLSPAN.ToString(), "-1", "-1"));
                int rowSpan = t.myInt32(t.Set(prop_.TLP.TLP_ROWSPAN.ToString(), "-1", "-1"));

                // bu arttırma işlemei boşluk kolonları yüzünden yapılıyor
                if (colNo > 0)
                    colNo = colNo + colNo;
                if (colSpan > 0)
                    colSpan = colSpan + colSpan;
                //---

                if (rowNo > -1)
                    LCTabbedGroup.OptionsTableLayoutItem.RowIndex = rowNo;
                if (colNo > -1)
                    LCTabbedGroup.OptionsTableLayoutItem.ColumnIndex = colNo;
                if (rowSpan > 0)
                    LCTabbedGroup.OptionsTableLayoutItem.RowSpan = rowSpan;
                if (colSpan > 0)
                    LCTabbedGroup.OptionsTableLayoutItem.ColumnSpan = colSpan;
                // bestfit i yok
            }
            #endregion LCTabbedGroup

            #region LCGroup
            if (LCGroup != null)
            {
                int colNo = t.myInt32(t.Set(prop_.TLP.TLP_COLNO.ToString(), "-1", "-1"));
                int rowNo = t.myInt32(t.Set(prop_.TLP.TLP_ROWNO.ToString(), "-1", "-1"));
                int colSpan = t.myInt32(t.Set(prop_.TLP.TLP_COLSPAN.ToString(), "-1", "-1"));
                int rowSpan = t.myInt32(t.Set(prop_.TLP.TLP_ROWSPAN.ToString(), "-1", "-1"));

                // bu arttırma işlemei boşluk kolonları yüzünden yapılıyor
                if (colNo > 0)
                    colNo = colNo + colNo;
                if (colSpan > 0)
                    colSpan = colSpan + colSpan;
                //---

                if (rowNo > -1)
                    LCGroup.OptionsTableLayoutItem.RowIndex = rowNo;
                if (colNo > -1)
                    LCGroup.OptionsTableLayoutItem.ColumnIndex = colNo;
                if (rowSpan > 0)
                    LCGroup.OptionsTableLayoutItem.RowSpan = rowSpan;
                if (colSpan > 0)
                    LCGroup.OptionsTableLayoutItem.ColumnSpan = colSpan;

                if (lst_TLP_COLUMNS.Count > 0)
                {
                    LCGroup.LayoutMode = LayoutMode.Table;
                    LCGroup.OptionsTableLayoutGroup.ColumnDefinitions.Clear();
                    LCGroup.OptionsTableLayoutGroup.RowDefinitions.Clear();
                }

                LCGroup.BestFit();
            }
            #endregion LCGroup

            /// A layout consisting of two items.
            /// İki öğeden oluşan bir düzen.
            /// When Table Layout mode is enabled, two rows and two columns are automatically created.
            /// Tablo Düzeni modu etkinleştirildiğinde, iki sıra ve iki sütun otomatik olarak oluşturulur.
            /// The first item is implicitly positioned in the cell (col=0, row=0).
            /// İlk madde hücrede örtüşen konumlandırılmıştır (col=0, row=0).
            /// The second item is explicitly positioned in the cell (col=1, row=1).
            /// İkinci madde açıkça hücreye yerleştirilir (col=1, row=1)
            ///
            /// Yani diyorki, ilk iki col ve row otomatik oluşturuluyor,
            /// İkinciden sonrakileri kodla oluşturmak gerekiyor
            ///
            /// Two rows already exist. Add one more row.
            ///     RowDefinition rowDefinition3 = new RowDefinition();
            ///     rowDefinition3.SizeType = SizeType.Percent;
            ///     rowDefinition3.Height = 50;
            ///     layoutControl.Root.OptionsTableLayoutGroup.RowDefinitions.Add(rowDefinition3);
            /// 
            /// ilk iki col ve row ayarlarını değiştirmek için :
            ///     layoutControl.Root.BeginUpdate();
            ///     RowDefinition row1 = layoutControl.Root.OptionsTableLayoutGroup.RowDefinitions[0];
            ///     row1.SizeType = SizeType.Absolute;
            ///     row1.Height = 100;
            ///     layoutControl.Root.EndUpdate();
            /// başka bir örnek      
            ///     layoutControl.Root.OptionsTableLayoutGroup.RowDefinitions[0].SizeType = SizeType.AutoSize;
            ///     LayoutControlItem item1 = layoutControl.AddItem("Picture", 
            ///           new PictureEdit() 
            ///           {
            ///            Name = "Picture",
            ///            Image = DxImageAssemblyUtil.ImageProvider.GetImage("ColorMixer", ImageSize.Size32x32, ImageType.Colored)
            ///           });
            ///     //Limit the PictureEdit control height.
            ///     item1.SizeConstraintsType = SizeConstraintsType.Custom;
            ///     item1.ControlMaxSize = new System.Drawing.Size(0, 40);
            ///
            /// istersen ilk iki col ve row silebilirsin.
            ///     LayoutControlGroup tableGroup = layoutControl.Root.AddGroup();
            ///     tableGroup.LayoutMode = LayoutMode.Table;
            ///     tableGroup.OptionsTableLayoutGroup.ColumnDefinitions.Clear();
            ///     tableGroup.OptionsTableLayoutGroup.RowDefinitions.Clear();

            // birden fazla col var ise
            #region Columns
            count = 0;

            foreach (var item in lst_TLP_COLUMNS)
            {
                sizetype = item.TLPC_SIZETYPE.ToString();
                sizevalue = item.TLPC_SIZEVALUE.ToString();

                if ((sizevalue == "") || (sizevalue == "null")) sizevalue = "0";

                if (t.IsNotNull(sizetype) && t.IsNotNull(sizevalue))
                {
                    ColumnDefinition columnDefinition = new ColumnDefinition();
                    
                    if (sizetype == "Absolute")
                        columnDefinition.SizeType = SizeType.Absolute;

                    if (sizetype == "AutoSize")
                        columnDefinition.SizeType = SizeType.AutoSize;

                    if (sizetype == "Percent")
                        columnDefinition.SizeType = SizeType.Percent;

                    columnDefinition.Width = float.Parse(sizevalue);

                    if (tDLayout != null)
                        tDLayout.Root.OptionsTableLayoutGroup.ColumnDefinitions.Add(columnDefinition);
                    if (LCGroup != null)
                        LCGroup.OptionsTableLayoutGroup.ColumnDefinitions.Add(columnDefinition);

                    // boşluk column olşuturuyor
                    ColumnDefinition columnDefinition2 = new ColumnDefinition();
                    columnDefinition2.SizeType = SizeType.Absolute;
                    columnDefinition2.Width = 10;

                    if (tDLayout != null)
                        tDLayout.Root.OptionsTableLayoutGroup.ColumnDefinitions.Add(columnDefinition2);
                    if (LCGroup != null)
                        LCGroup.OptionsTableLayoutGroup.ColumnDefinitions.Add(columnDefinition2);
                    //---

                    count++;
                }


            }

            if (count > 0)
            {
                if (tDLayout != null)
                    tDLayout.Root.OptionsTableLayoutItem.ColumnIndex = count;
                if (LCGroup != null)
                    LCGroup.OptionsTableLayoutItem.ColumnIndex = count;

            }
            #endregion Columns

            // birden fazla row var ise
            #region Rows
            count = 0;

            foreach (var item in lst_TLP_ROWS)
            {
                sizetype = item.TLPR_SIZETYPE.ToString();
                sizevalue = item.TLPR_SIZEVALUE.ToString();
                if ((sizevalue == "") || (sizevalue == "null")) sizevalue = "0";

                if (t.IsNotNull(sizetype) && t.IsNotNull(sizevalue))
                {
                    RowDefinition rowDefinition = new RowDefinition();

                    if (sizetype == "Absolute")
                        rowDefinition.SizeType = SizeType.Absolute;

                    if (sizetype == "AutoSize")
                        rowDefinition.SizeType = SizeType.AutoSize;
                    /// autosize yapınca item göre kendini ayarlıyor
                    /// aşağıdaki komutta item ın boyutunu değiştirmek için
                    /// 
                    /// Limit the PictureEdit control height.
                    ///item1.SizeConstraintsType = SizeConstraintsType.Custom;
                    ///item1.ControlMaxSize = new System.Drawing.Size(0, 40);

                    if (sizetype == "Percent")
                        rowDefinition.SizeType = SizeType.Percent;

                    rowDefinition.Height = float.Parse(sizevalue);

                    if (tDLayout != null)
                        tDLayout.Root.OptionsTableLayoutGroup.RowDefinitions.Add(rowDefinition);
                    if (LCGroup != null)
                        LCGroup.OptionsTableLayoutGroup.RowDefinitions.Add(rowDefinition);

                    count++;
                }
            }// foreach

            // en az bir adet row oluşsun
            if (lst_TLP_ROWS.Count == 0)
            {
                RowDefinition rowDefinition = new RowDefinition();
                rowDefinition.SizeType = SizeType.AutoSize;

                if (tDLayout != null)
                    tDLayout.Root.OptionsTableLayoutGroup.RowDefinitions.Add(rowDefinition);
                if (LCGroup != null)
                    LCGroup.OptionsTableLayoutGroup.RowDefinitions.Add(rowDefinition);

                count++;
            }

            if (count > 0)
            {
                if (tDLayout != null)
                    tDLayout.Root.OptionsTableLayoutItem.RowIndex = count;
                if (LCGroup != null)
                    LCGroup.OptionsTableLayoutItem.RowIndex = count;
            }
            #endregion Rows
        }

        // işi bitti, kullanılmıyor
        private void DataLayout_Item_ColIndex_RowIndex_Preparing(
                     DataSet dsFields, int fieldCount, int groupNo,
                     DevExpress.XtraDataLayout.DataLayoutControl tDLayout,
                     LayoutControlGroup LCGroup,
                     LayoutControlGroup tabbedGroup)
        {
            tToolBox t = new tToolBox();

            string fname = string.Empty;

            Boolean tvisible = false;

            Int16 fgroup_no = 0;
            Int16 fgroup_line_no = 0;

            Int16 fCol = 0;
            Int16 fRow = 0;
            Int16 fColSpan = 0;
            Int16 fRowSpan = 0;

            DevExpress.XtraLayout.BaseLayoutItem bl_item1 = null;

            for (int i = 0; i < fieldCount; i++)
            {
                tvisible = t.Set(dsFields.Tables[0].Rows[i]["CMP_VISIBLE"].ToString(), dsFields.Tables[0].Rows[i]["LKP_FVISIBLE"].ToString(), true);
                fgroup_no = t.Set(dsFields.Tables[0].Rows[i]["GROUP_NO"].ToString(), dsFields.Tables[0].Rows[i]["LKP_GROUP_NO"].ToString(), (Int16)0);
                fgroup_line_no = t.Set(dsFields.Tables[0].Rows[i]["GROUP_LINE_NO"].ToString(), dsFields.Tables[0].Rows[i]["LKP_GROUP_LINE_NO"].ToString(), (Int16)0); ;

                if (fgroup_no == -1)
                    fgroup_no = t.Set(dsFields.Tables[0].Rows[i]["LKP_GROUP_NO"].ToString(), "0", (Int16)0);
                if (fgroup_line_no == -1)
                    fgroup_line_no = t.Set(dsFields.Tables[0].Rows[i]["LKP_GROUP_LINE_NO"].ToString(), "0", (Int16)0);

                if ((groupNo == fgroup_no) && (tvisible))
                {
                    fname = "Column_" + t.Set(dsFields.Tables[0].Rows[i]["LKP_FIELD_NAME"].ToString(), "", "");
                    bl_item1 = tDLayout.Root.Items.FindByName(fname);

                    /// şimdi tespit edilen bu item in col ve row koordinatları ayarlanacak
                    /// 
                    fCol = t.myInt16(dsFields.Tables[0].Rows[i]["CMP_LEFT"].ToString());
                    fRow = t.myInt16(dsFields.Tables[0].Rows[i]["CMP_TOP"].ToString());
                    fColSpan = t.myInt16(dsFields.Tables[0].Rows[i]["CMP_WIDTH"].ToString());
                    fRowSpan = t.myInt16(dsFields.Tables[0].Rows[i]["CMP_HEIGHT"].ToString());

                    if (fColSpan == 100) fColSpan = 0;
                    if (fRowSpan == 100) fRowSpan = 0;

                    bl_item1.OptionsTableLayoutItem.ColumnIndex = fCol;
                    bl_item1.OptionsTableLayoutItem.RowIndex = fRow;
                    bl_item1.OptionsTableLayoutItem.ColumnSpan = fColSpan;
                    bl_item1.OptionsTableLayoutItem.RowSpan = fRowSpan;

                    // item lar roottan group lara ve tab lara taşınıyor
                    if (fgroup_no > 0)
                    {
                        if (LCGroup != null)
                            LCGroup.Add(bl_item1);
                        if (tabbedGroup != null)
                            tabbedGroup.Add(bl_item1);
                    }
                }

            }

        }
        //--

        #region Column, Group and TabPege Move

        private void tColumnAndGroup_Move(DataSet dsFields,
            DevExpress.XtraGrid.Views.Layout.LayoutView tGridView,
            DevExpress.XtraDataLayout.DataLayoutControl tDLayout)
        {
            tToolBox t = new tToolBox();

            #region Tanımlar

            string fname = string.Empty;
            string move_item_name = string.Empty;
            Int16 move_item_type = 0;
            Int16 move_type = 0;
            Int16 move_location = 0;
            Int16 move_layout_type = 0;
            Int16 group_types = 0;
            Int16 group_no = 0;
            bool fvisible = false;

            DevExpress.XtraLayout.BaseLayoutItem bl_item1 = null;
            DevExpress.XtraLayout.BaseLayoutItem bl_item2 = null;
            DevExpress.XtraLayout.BaseLayoutItem bl_groupitem = null;
            //DevExpress.XtraLayout.LayoutControlGroup
            DevExpress.XtraLayout.EmptySpaceItem es_item1 = null;

            LayoutItemDragController dragController = null;

            var param1 = MoveType.Inside;
            var param2 = InsertLocation.Before;
            var param3 = LayoutType.Horizontal;

            #endregion Tanımlar

            #region // Açılan Group veya TabPage döngüsü ve Move İşlemi
            for (int i = 0; i < dsFields.Tables[1].Rows.Count; i++)
            {
                ///GROUP_TYPES', 1, '', 'GroupPanel');
                ///GROUP_TYPES', 2, '', 'DataLayoutTabPage');
                ///GROUP_TYPES', 3, '', 'WelcomeWizard');
                ///GROUP_TYPES', 4, '', 'WizardPage');
                ///GROUP_TYPES', 5, '', 'CompletionWizard');
                ///GROUP_TYPES', 6, '', 'TabPages');

                group_types = t.Set(dsFields.Tables[1].Rows[i]["GROUP_TYPES"].ToString(), "", (Int16)0);
                group_no = t.Set(dsFields.Tables[1].Rows[i]["FGROUPNO"].ToString(), "", (Int16)0);
                fvisible = t.Set(dsFields.Tables[1].Rows[i]["FVISIBLE"].ToString(), "", (bool)false);

                move_item_type = t.Set(dsFields.Tables[1].Rows[i]["MOVE_ITEM_TYPE"].ToString(), "", (Int16)0);
                move_item_name = t.Set(dsFields.Tables[1].Rows[i]["MOVE_ITEM_NAME"].ToString(), "", "");
                move_type = t.Set(dsFields.Tables[1].Rows[i]["MOVE_TYPE"].ToString(), "", (Int16)0);
                move_location = t.Set(dsFields.Tables[1].Rows[i]["MOVE_LOCATION"].ToString(), "", (Int16)0);
                move_layout_type = t.Set(dsFields.Tables[1].Rows[i]["MOVE_LAYOUT_TYPE"].ToString(), "", (Int16)0);

                // move elemanı var ise
                if ((move_item_type > 0) && (t.IsNotNull(move_item_name)))
                {
                    #region komutları hazırla 
                    /* 1 Column, 2 Group, 3 TabPage */
                    if (move_item_type == 1) move_item_name = "Column_" + move_item_name;
                    if (move_item_type == 2) move_item_name = "LCGroup_" + move_item_name;
                    if (move_item_type == 3) move_item_name = "LCTabbedGroup_" + move_item_name;
                                        
                    if (move_type == 1) param1 = MoveType.Inside;
                    if (move_type == 2) param1 = MoveType.Outside;

                    if (move_location == 1) param2 = InsertLocation.After;
                    if (move_location == 2) param2 = InsertLocation.Before;

                    if (move_layout_type == 1) param3 = LayoutType.Horizontal;
                    if (move_layout_type == 2) param3 = LayoutType.Vertical;
                    #endregion

                    #region // 1. move elemanı
                    if (group_types == 1)
                        fname = "LCGroup_" + group_no;
                    if (group_types == 2)
                        fname = "LCTabbed_" + group_no;

                    if (tGridView != null)
                        bl_item1 = tGridView.Items.FindByName(fname);

                    if (tDLayout != null)
                        bl_item1 = tDLayout.Root.Items.FindByName(fname);
                    #endregion // ---

                    #region // 2. move elemanı
                    if (tGridView != null)
                        bl_item2 = tGridView.Items.FindByName(move_item_name);

                    if (tDLayout != null)
                        bl_item2 = tDLayout.Root.Items.FindByName(move_item_name);
                    #endregion // ---

                    #region move işlemini gerçekleştir
                    if ((bl_item1 != null) && (bl_item2 != null))
                    {
                        dragController = new LayoutItemDragController(
                        bl_item1,  //layoutControlItem2,
                        bl_item2,  //layoutControlItem1, 
                        param1, param2, param3);
                        //MoveType.Inside, InsertLocation.Before, LayoutType.Horizontal);

                        // move işlemini gerçekleştir
                        bl_item1.Move(dragController);
                    }
                    #endregion move işlemini gerçekleştir

                } // ((move_item_type > 0)
            }
            #endregion // Açılan Group veya TabPage döngüsü ve Move İşlemi

            #region // Field döngüsü ve Move İşlemi

            for (int i = 0; i < dsFields.Tables[0].Rows.Count; i++)
            {
                /// MOVE_ITEM_TYPE    /* 1 Column, 2 Group, 3 TabPage */
                /// MOVE_ITEM_NAME       VarChar(30)
                /// MOVE_TYPE         /* 1 Inside  2 Outside */
                /// MOVE_LOCATION     /* 1 After   2 Before  */
                /// MOVE_LAYOUT_TYPE  /* 1 Horizontal  2 Vertical */

                move_item_type = t.Set(dsFields.Tables[0].Rows[i]["MOVE_ITEM_TYPE"].ToString(), "", (Int16)0);
                move_item_name = t.Set(dsFields.Tables[0].Rows[i]["MOVE_ITEM_NAME"].ToString(), "", "");
                move_type = t.Set(dsFields.Tables[0].Rows[i]["MOVE_TYPE"].ToString(), "", (Int16)0);
                move_location = t.Set(dsFields.Tables[0].Rows[i]["MOVE_LOCATION"].ToString(), "", (Int16)0);
                move_layout_type = t.Set(dsFields.Tables[0].Rows[i]["MOVE_LAYOUT_TYPE"].ToString(), "", (Int16)0);

                #region // move elemanı var ise
                if ((move_item_type > 0) && (t.IsNotNull(move_item_name)))
                {
                    #region komutları hazırla 
                    /* 1 Column, 2 Group, 3 TabPage */
                    if (move_item_type == 1) move_item_name = "Column_" + move_item_name;
                    if (move_item_type == 2) move_item_name = "LCGroup_" + move_item_name;
                    if (move_item_type == 3) move_item_name = "LCTabbed_" + move_item_name;

                    if (move_type == 1) param1 = MoveType.Inside;
                    if (move_type == 2) param1 = MoveType.Outside;

                    if (move_location == 1) param2 = InsertLocation.After;
                    if (move_location == 2) param2 = InsertLocation.Before;

                    if (move_layout_type == 1) param3 = LayoutType.Horizontal;
                    if (move_layout_type == 2) param3 = LayoutType.Vertical;
                    #endregion komutları hazırla

                    // 1. move elemanı
                    fname = "Column_" + t.Set(dsFields.Tables[0].Rows[i]["LKP_FIELD_NAME"].ToString(), "", "");

                    // fieldin grubu var ise
                    group_no = t.Set(dsFields.Tables[0].Rows[i]["GROUP_NO"].ToString(), dsFields.Tables[0].Rows[i]["LKP_GROUP_NO"].ToString(), (Int16)0);

                    bl_item1 = null;
                    bl_item2 = null;

                    #region birinci eleman tespiti
                    if (tGridView != null)
                        bl_item1 = tGridView.Items.FindByName(fname);

                    if (tDLayout != null)
                    {
                        bl_item1 = tDLayout.Root.Items.FindByName(fname);

                        if ((bl_item1 == null) && (group_no > 0) && (move_item_type > 0))
                        {
                            // Group kontrolu
                            bl_groupitem = tDLayout.Root.Items.FindByName("LCGroup_" + group_no.ToString());
                            if ((bl_groupitem != null) && (bl_groupitem.GetType().ToString() == "DevExpress.XtraLayout.LayoutControlGroup"))
                                bl_item1 = ((DevExpress.XtraLayout.LayoutControlGroup)bl_groupitem).Items.FindByName(fname);

                            if (bl_groupitem == null)
                            {
                                // Tabbed kontrolu
                                //bl_groupitem = tDLayout.Root.Items.FindByName("LCTabbed_" + group_no.ToString());
                                //if (bl_groupitem != null)
                                //    bl_item1 = ((DevExpress.XtraLayout.TabbedControlGroup)bl_groupitem).TabPages[0].Items.FindByName(fname);

                                bl_groupitem = Find_TabbedControlGroup(tDLayout, t.myInt32(group_no.ToString()));

                                if (bl_groupitem != null)
                                    bl_item1 = ((DevExpress.XtraLayout.LayoutControlGroup)bl_groupitem).Items.FindByName(fname);

                            }
                        }
                    }
                    #endregion  birinci eleman tespiti //--

                    #region ikinci eleman tespiti
                    if (tGridView != null)
                        bl_item2 = tGridView.Items.FindByName(move_item_name);

                    if (tDLayout != null)
                    {
                        bl_item2 = tDLayout.Root.Items.FindByName(move_item_name);
                        
                        if ((bl_item2 == null) && (group_no > 0) && (move_item_type > 0))
                        {
                            // Group kontrolu
                            bl_groupitem = tDLayout.Root.Items.FindByName("LCGroup_" + group_no.ToString());
                            if ((bl_groupitem != null) && (bl_groupitem.GetType().ToString() == "DevExpress.XtraLayout.LayoutControlGroup"))
                                bl_item2 = ((DevExpress.XtraLayout.LayoutControlGroup)bl_groupitem).Items.FindByName(move_item_name);

                            if (bl_groupitem == null)
                            {
                                // Tabbed kontrolu
                                //bl_groupitem = tDLayout.Root.Items.FindByName("LCTabbed_" + group_no.ToString());
                                //if (bl_groupitem != null)
                                //    bl_item2 = ((DevExpress.XtraLayout.TabbedControlGroup)bl_groupitem).TabPages[0].Items.FindByName(move_item_name);

                                bl_groupitem = Find_TabbedControlGroup(tDLayout, t.myInt32(group_no.ToString()));

                                if (bl_groupitem != null)
                                    bl_item2 = ((DevExpress.XtraLayout.LayoutControlGroup)bl_groupitem).Items.FindByName(move_item_name);
                            }
                        }

                        if (move_item_type == 4)
                        {
                            es_item1 = new DevExpress.XtraLayout.EmptySpaceItem();
                            
                            if (group_no > 0)
                            {
                                bl_groupitem = Find_TabbedControlGroup(tDLayout, group_no);

                                if (bl_groupitem != null)
                                    ((DevExpress.XtraLayout.LayoutControlGroup)bl_groupitem).AddItem(es_item1);
                            }
                            else tDLayout.Root.AddItem(es_item1);

                            if ((es_item1 != null) && (bl_item1 != null))
                            {
                                dragController = new LayoutItemDragController(
                                    es_item1,  //layoutControlItem1, 
                                    bl_item1,  //layoutControlItem2,
                                    param1, param2, param3);
                                //MoveType.Inside, InsertLocation.Before, LayoutType.Horizontal);

                                // move işlemini gerçekleştir
                                bl_item1.Move(dragController);
                            }
                        }

                    }
                    #endregion  ikinci eleman tespiti

                    #region move işlemini gerçekleştir
                    if ((bl_item1 != null) && (bl_item2 != null))
                    {
                        // caption yok ise capiton boşluğunu kaldırıyor
                        // bl_item1 : ikinci kolon
                        if (bl_item1.CustomizationFormText.Trim() == "")
                            bl_item1.TextVisible = false;

                        dragController = new LayoutItemDragController(
                        bl_item1,  //layoutControlItem2,
                        bl_item2,  //layoutControlItem1, 
                        param1, param2, param3);
                        //MoveType.Inside, InsertLocation.Before, LayoutType.Horizontal);

                        // move işlemini gerçekleştir
                        bl_item1.Move(dragController);
                    }
                    #endregion

                }
                #endregion // move elemanı var ise
            }

            #endregion // Field döngüsü ve Move İşlemi

            if (tDLayout != null)
                tDLayout.BestFit();

        }

        private DevExpress.XtraLayout.BaseLayoutItem Find_TabbedControlGroup(
            DevExpress.XtraDataLayout.DataLayoutControl tDLayout,
            int tgroupNo)
        {
            /// Tabbed kontrolu
            /// bl_groupitem = tDLayout.Root.Items.FindByName("LCTabbed_" + tgroupNo.ToString());
            /// 
            /// böyle arayınca şöyle bir durum oluyor
            /// TabbedControlGroup oluşturuluyor onun adı LCTabbed_2 yapılıyor
            /// üzerine ilk açılan tabpage nin adı da LCGroup_2 yapılıyor
            /// bir sonraki tabpage nin adı LCGroup_3 yapılıyor
            /// zurnanın zırt dediği yer burası 
            /// .FindByName("LCTabbed_3") diye arayınca null dönüyor
            /// çünkü nesne LCTabbed_2 içindeki LCGroup_3
            /// onun için aşağıdaki döngü kuruldu

            DevExpress.XtraLayout.BaseLayoutItem bl_groupitem = null;

            bl_groupitem = tDLayout.Root.Items.FindByName("LCGroup_" + tgroupNo.ToString());

            if (bl_groupitem == null)
                bl_groupitem = tDLayout.Items.FindByName("LCGroup_" + tgroupNo.ToString());

            if (bl_groupitem == null)
            {
                for (int i = 0; i <= tgroupNo; i++)
                {
                    // Tabbed kontrolu
                    bl_groupitem = tDLayout.Root.Items.FindByName("LCTabbed_" + i.ToString());

                    if (bl_groupitem != null)
                    {
                        int i3 = ((DevExpress.XtraLayout.TabbedControlGroup)bl_groupitem).TabPages.Count;
                        for (int i2 = 0; i2 < i3; i2++)
                        {
                            if (((DevExpress.XtraLayout.TabbedControlGroup)bl_groupitem).TabPages[i2].Name == "LCGroup_" + tgroupNo.ToString())
                            {
                                return (((DevExpress.XtraLayout.TabbedControlGroup)bl_groupitem).TabPages[i2]);
                            }
                        }
                    }
                }
            }

            return bl_groupitem;
        }

        #endregion Column, Group and TabPege Move

        #endregion DataLayoutView

        #region tWizardPage Create

        public void tWizardControl_Create(DataRow row_Table, DataSet dsFields, DataSet dsData,
            DevExpress.XtraWizard.WizardControl tWizardControl)
        {
            tToolBox t = new tToolBox();
            tEvents ev = new tEvents();

            string function_name = "tWizardControl_Create";
            t.Takipci(function_name, "", '{');

            #region Tanımlar
            // Tablo hakkındaki bilgiler
            //DataRow row_Table = dsTable.Tables[0].Rows[0];

            Int16 grptype = 0;
            string caption = string.Empty;
            int groupno = -1;
            string visible = string.Empty;
            string fixedbn = string.Empty;
            string TableCode1 = string.Empty;
            string TableCode2 = "none";

            string tcaption = t.Set(row_Table["IP_CAPTION"].ToString(),
                             row_Table["LKP_TB_CAPTION"].ToString(),
                             row_Table["LKP_TABLE_NAME"].ToString());

            string SoftwareCode = row_Table["SOFTWARE_CODE"].ToString();
            string ProjectCode = row_Table["PROJECT_CODE"].ToString();
            string TableIPCode = row_Table["TABLE_CODE"].ToString() + "." + row_Table["IP_CODE"].ToString();
            if (t.IsNotNull(SoftwareCode))
                TableIPCode = SoftwareCode + "/" + ProjectCode + "/" + TableIPCode;
                        
            vTableAbout vTA = new vTableAbout();
            t.Table_FieldsGroups_Count(vTA, dsFields);
            int gc = vTA.GroupsCount;

            #endregion Tanımlar

            #region tWizardControl ve Columns İşlemleri
            tWizardControl.BeginUpdate();
            tWizardControl.Text = tcaption;
            tWizardControl.NextText = "&Devam >";
            tWizardControl.CancelText = "Vazgeç";
            tWizardControl.FinishText = "Bitir";
            tWizardControl.PreviousText = "< &Geri";
            tWizardControl.HelpText = "Yardım";
            //tWizardControl.UseAcceptButton = false;
            //tWizardControl.UseCancelButton = false;

            tWizardControl.CancelClick += new System.ComponentModel.CancelEventHandler(ev.tWizardControl_CancelClick);
            tWizardControl.FinishClick += new System.ComponentModel.CancelEventHandler(ev.tWizardControl_FinishClick);
            tWizardControl.NextClick += new DevExpress.XtraWizard.WizardCommandButtonClickEventHandler(ev.tWizardControl_NextClick);
            tWizardControl.PrevClick += new DevExpress.XtraWizard.WizardCommandButtonClickEventHandler(ev.tWizardControl_PrevClick);
            tWizardControl.HelpClick += new DevExpress.XtraWizard.WizardButtonClickEventHandler(ev.tWizardControl_HelpClick);

            DevExpress.XtraDataLayout.DataLayoutControl DataLControl = new DevExpress.XtraDataLayout.DataLayoutControl();
            DataLControl.Name = "tDataControl_WizardControl_" + TableIPCode;
            DataLControl.AccessibleName = TableIPCode;
            DataLControl.Visible = false;

            //  if (t.IsNotNull(dsData))
            DataLControl.DataSource = dsData.Tables[0];

            tWizardControl.Controls.Add(DataLControl);
            #endregion tWizardControl

            #region Group var ise
            // Group bilgileri mevcut
            if (gc > 0)
            {
                DevExpress.XtraWizard.WelcomeWizardPage tWelcomeWizardPage = null;
                DevExpress.XtraWizard.CompletionWizardPage tCompletionWizardPage = null;
                DevExpress.XtraWizard.WizardPage tWizardPage = null;
                System.Windows.Forms.TableLayoutPanel tTableLayoutPanel = null;

                foreach (DataRow Row in dsFields.Tables[1].Rows)
                {
                    TableCode1 = t.Set(Row["TABLE_CODE"].ToString(), "", "");

                    // Group sırası önce MS_FIELDS_IP üzerinde tanımlı olanlar
                    // sonra MS_FIELDS üzerinde tanımlı olan gruplar listelenir

                    // birbirinden ayıran fark TABLE_CODE fieldi
                    // örnek tablo çıktısı aşağıda

                    // MS_FIELDS_ID üzerindekiler varsa açılacak
                    // yoksa MS_FIELDS üzerindekiler açılacak
                    if ((TableCode1 != TableCode2) && (TableCode2 != "none"))
                    {
                        break;
                    }

                    TableCode2 = TableCode1;

                    grptype = t.Set(Row["GROUP_TYPES"].ToString(), "", (Int16)0);
                    caption = t.Set(Row["FCAPTION"].ToString(), "", "");
                    groupno = t.myInt32(t.Set(Row["FGROUPNO"].ToString(), "", ""));
                    visible = t.Set(Row["FVISIBLE"].ToString(), "", "");
                    fixedbn = t.Set(Row["FIXED"].ToString(), "", "");

                    // 'GROUP_TYPES'
                    //  1, '', 'GroupPanel'
                    //  2, '', 'TabPageControl'
                    //  3, '', 'WelcomeWizard'
                    //  4, '', 'WizardPage'
                    //  5, '', 'CompletionWizard'

                    if (tWizardControl != null)
                    {
                        tTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();

                        if (grptype == 3)
                        {
                            tWelcomeWizardPage = new DevExpress.XtraWizard.WelcomeWizardPage();
                            tWelcomeWizardPage.Name = "tPage_" + groupno;
                            tWizardControl_Add(dsFields, dsData, tWizardControl, tWelcomeWizardPage, tTableLayoutPanel, TableIPCode, caption, groupno);
                        }

                        if (grptype == 4)
                        {
                            tWizardPage = new DevExpress.XtraWizard.WizardPage();
                            tWizardPage.Name = "tPage_" + groupno;
                            tWizardControl_Add(dsFields, dsData, tWizardControl, tWizardPage, tTableLayoutPanel, TableIPCode, caption, groupno);
                        }

                        if (grptype == 5)
                        {
                            tCompletionWizardPage = new DevExpress.XtraWizard.CompletionWizardPage();
                            tCompletionWizardPage.Name = "tPage_" + groupno;
                            tWizardControl_Add(dsFields, dsData, tWizardControl, tCompletionWizardPage, tTableLayoutPanel, TableIPCode, caption, groupno);
                        }

                    }

                }
            }
            #endregion

            tWizardControl.EndUpdate();

            t.Takipci(function_name, "", '}');
        }

        private void tWizardControl_Add(DataSet dsFields, DataSet dsData,
                DevExpress.XtraWizard.WizardControl tWizardControl,
                DevExpress.XtraWizard.BaseWizardPage tWizardPage,
                System.Windows.Forms.TableLayoutPanel tTableLayoutPanel,
                string TableIPCode,
                string caption,
                int groupno)
        {
            tToolBox t = new tToolBox();
            tDevColumn dc = new tDevColumn();

            #region  Tanımlar
            int row_no = 0;
            int row_size = 0;
            int row_count = 0;
            //int twidth = 0;
            int j = dsFields.Tables[0].Rows.Count;
            int tgrp_no = 0;
            int tgrp_sirano = 0;

            string tfield_name = string.Empty;
            string tcolumntype = string.Empty;
            string tcaption = string.Empty;
            string thint = string.Empty;
            string tdisplayformat = string.Empty;
            string teditformat = string.Empty;
            Boolean tenabled = false;
            Boolean tvisible = false;
            byte tview_type = 1;
            byte toperand_type = 0;
            #endregion

            #region Page Add 
            tWizardPage.Text = caption;
            tWizardControl.Controls.Add(tWizardPage);
            tWizardControl.Pages.Add(tWizardPage);
            tWizardPage.Controls.Add(tTableLayoutPanel);
            //tWizardPage.
            tTableLayoutPanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;
            tTableLayoutPanel.Dock = DockStyle.Fill;
            tTableLayoutPanel.Padding = new System.Windows.Forms.Padding(3);

            // Çerçeve çizigileri
            //tTableLayoutPanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Outset;

            tTableLayoutPanel.ColumnCount = 3;
            tTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            tTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            tTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));

            #endregion

            #region Page nin içini doldurma

            #region // grubun row sayısı tespit ediliyor
            for (int i = 0; i < j; i++)
            {
                tgrp_no = t.Set(dsFields.Tables[0].Rows[i]["GROUP_NO"].ToString(), "", -1);
                tvisible = t.Set(dsFields.Tables[0].Rows[i]["CMP_VISIBLE"].ToString(), "", true);

                if ((groupno == tgrp_no) && (tvisible)) row_count++;
            }
            if (row_count == 0) return;
            #endregion 

            #region // rows Add TableLayoutPanel
            tTableLayoutPanel.RowCount = row_count;
            row_size = (100 / row_count);
            for (int i = 0; i < row_count; i++)
            {
                tTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, row_size));
            }
            #endregion

            #region // row içine Edit ler yerleştiriliyor
            foreach (DataRow Row in dsFields.Tables[0].Rows)
            {
                tgrp_no = t.Set(Row["GROUP_NO"].ToString(), "", -1);
                tvisible = t.Set(Row["CMP_VISIBLE"].ToString(), "", true);
                
                if ((groupno == tgrp_no) && (tvisible))
                {
                    //-----
                    tfield_name = t.Set(Row["LKP_FIELD_NAME"].ToString(), "", "");
                    tcolumntype = t.Set(Row["CMP_COLUMN_TYPE"].ToString(), Row["LKP_CMP_COLUMN_TYPE"].ToString(), "TextEdit");
                    tcaption = t.Set(Row["FCAPTION"].ToString(), Row["LKP_FCAPTION"].ToString(), tfield_name);
                    thint = t.Set(Row["FHINT"].ToString(), "", "");
                    tenabled = t.Set(Row["CMP_ENABLED"].ToString(), Row["LKP_FENABLED"].ToString(), true);
                    //twidth = t.Set(Row["CMP_WIDTH"].ToString(), Row["LKP_CMP_WIDTH"].ToString(), 150);
                    tgrp_sirano = t.Set(Row["GROUP_LINE_NO"].ToString(), "", -1);
                    tdisplayformat = t.Set(Row["CMP_DISPLAY_FORMAT"].ToString(), Row["LKP_CMP_DISPLAY_FORMAT"].ToString(), "");
                    teditformat = t.Set(Row["CMP_EDIT_FORMAT"].ToString(), Row["LKP_CMP_EDIT_FORMAT"].ToString(), "");

                    tview_type = 1;
                    toperand_type = t.Set(Row["KRT_OPERAND_TYPE"].ToString(), "", (byte)0);

                    if (toperand_type > 0) tview_type = 2;

                    if (tgrp_sirano <= 0) tgrp_sirano = row_no;
                    //if (twidth <= 100) twidth = 200;
                    //-----

                    //LayoutControlItem item = new LayoutControlItem();
                    System.Windows.Forms.Panel item = new System.Windows.Forms.Panel();
                    DevExpress.XtraEditors.LabelControl labelControl1 = new DevExpress.XtraEditors.LabelControl();
                    DevExpress.XtraEditors.LabelControl labelControl2 = new DevExpress.XtraEditors.LabelControl();

                    item.Name = "Panel_" + tfield_name;
                    item.AccessibleName = TableIPCode;
                    //item.BackColor = System.Drawing.Color.DarkGreen;
                    //item.Width = twidth;
                    labelControl1.Text = tcaption;
                    labelControl2.Text = thint;

                    dc.tXtraEditors_Edit(Row, dsData, null, item, tcolumntype, tdisplayformat, teditformat, tview_type, toperand_type, ""); // "" = FormName

                    item.Width = item.Controls[0].Width + 1;
                    item.Height = item.Controls[0].Height + 1;

                    tTableLayoutPanel.Controls.Add(labelControl1, 0, tgrp_sirano);
                    tTableLayoutPanel.Controls.Add(item, 1, tgrp_sirano);
                    tTableLayoutPanel.Controls.Add(labelControl2, 2, tgrp_sirano);

                    //-----
                    row_no++;
                }
            }
            #endregion 

            #endregion
        }

        #endregion tWizardPage Create

        #region CalenderAndScheduler

        private void SchedulerButtonAdd(string TableIPCode, DevExpress.XtraEditors.PanelControl tButtonPanel)
        {
            //SchedulerViewType.Agenda
            //SchedulerViewType.Day
            //SchedulerViewType.FullWeek
            //SchedulerViewType.Gantt
            //SchedulerViewType.Month
            //SchedulerViewType.Timeline
            //SchedulerViewType.Week
            //SchedulerViewType.WorkWeek

            SchedulerButtonAdd(TableIPCode, "1", "Gündem", tButtonPanel);
            SchedulerButtonAdd(TableIPCode, "2", "Gün", tButtonPanel);
            SchedulerButtonAdd(TableIPCode, "3", "Tam Hafta", tButtonPanel);
            //SchedulerButtonAdd(TableIPCode, "4", "Gantt", tButtonPanel);
            SchedulerButtonAdd(TableIPCode, "5", "Ay", tButtonPanel);
            SchedulerButtonAdd(TableIPCode, "6", "Zaman Çizelgesi", tButtonPanel);
            //SchedulerButtonAdd(TableIPCode, "7", "Hafta", tButtonPanel);
            //SchedulerButtonAdd(TableIPCode, "8", "Çalışma Haftası", tButtonPanel);
            //SchedulerWeekCounterAdd(TableIPCode, "9", "Hafta Sayısı", tButtonPanel);
        }
        private void SchedulerButtonAdd(string TableIPCode, string groupno, string caption,
            DevExpress.XtraEditors.PanelControl tButtonPanel)
        {
            tToolBox t = new tToolBox();
            tEvents ev = new tEvents();
            tEventsGrid evg = new tEventsGrid();

            DevExpress.XtraEditors.CheckButton simpleButton_Gbutton = new DevExpress.XtraEditors.CheckButton();
            simpleButton_Gbutton.AccessibleDescription = TableIPCode;
            simpleButton_Gbutton.AllowHtmlDraw = DevExpress.Utils.DefaultBoolean.True;
            simpleButton_Gbutton.Name = "tGbutton_" + groupno;
            simpleButton_Gbutton.Text = caption;
            simpleButton_Gbutton.Dock = DockStyle.Left;
            simpleButton_Gbutton.Width = 40 + ((caption.Length) * 4);
            simpleButton_Gbutton.GroupIndex = 1;
            simpleButton_Gbutton.TabIndex = t.myInt32(groupno);
            simpleButton_Gbutton.BorderStyle = BorderStyles.UltraFlat;

            //simpleButton_Gbutton.Checked = t.myVisible(visible);
            simpleButton_Gbutton.Click += new System.EventHandler(evg.checkSchedulerGroupButton_Click);

            simpleButton_Gbutton.AppearanceHovered.BackColor = v.colorOrder;
            simpleButton_Gbutton.AppearanceHovered.Options.UseBackColor = true;

            simpleButton_Gbutton.Enter += new System.EventHandler(ev.btn_Navigotor_Enter);
            simpleButton_Gbutton.Leave += new System.EventHandler(ev.btn_Navigotor_Leave);

            tButtonPanel.Controls.Add(simpleButton_Gbutton);
            simpleButton_Gbutton.BringToFront();
        }

        private void SchedulerWeekCounterAdd(string TableIPCode, string groupno, string caption,
                    DevExpress.XtraEditors.PanelControl tButtonPanel)
        {
            tToolBox t = new tToolBox();
            tEvents ev = new tEvents();
            tEventsGrid evg = new tEventsGrid();
                                    
            System.Windows.Forms.TableLayoutPanel panel1 = new System.Windows.Forms.TableLayoutPanel();
            DevExpress.XtraEditors.LabelControl labelControl1 = new DevExpress.XtraEditors.LabelControl();
            DevExpress.XtraEditors.SpinEdit tEdit1 = new DevExpress.XtraEditors.SpinEdit();

            panel1.Name = "panel1";
            panel1.Width = 150;
            panel1.Dock = DockStyle.Left;

            panel1.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            panel1.ColumnCount = 2;
            panel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            panel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));

            labelControl1.Text = "Hafta sayısı";
            labelControl1.Dock = DockStyle.Left;

            tEdit1.Name = "spinEdit1";
            tEdit1.Properties.AccessibleName = "spinEdit1_" + TableIPCode;
            tEdit1.EnterMoveNextControl = true;
            tEdit1.Dock = DockStyle.Fill;
            tEdit1.EditValueChanged += new System.EventHandler(evg.checkSchedulerWeekCount);

            tEdit1.Properties.SpinStyle = SpinStyles.Horizontal;
            tEdit1.Properties.IsFloatValue = false;
            tEdit1.Properties.MinValue = 1;
            tEdit1.Properties.MaxValue = 6;
            tEdit1.EditValue = 2;

            panel1.Controls.Add(labelControl1, 0, 0);
            panel1.Controls.Add(tEdit1, 1, 0);
            

            /*
            DevExpress.XtraEditors.CheckButton simpleButton_Gbutton = new DevExpress.XtraEditors.CheckButton();
            simpleButton_Gbutton.AccessibleDescription = TableIPCode;
            simpleButton_Gbutton.AllowHtmlDraw = DevExpress.Utils.DefaultBoolean.True;
            simpleButton_Gbutton.Name = "tGbutton_" + groupno;
            simpleButton_Gbutton.Text = caption;
            simpleButton_Gbutton.Dock = DockStyle.Left;
            simpleButton_Gbutton.Width = 40 + ((caption.Length) * 4);
            simpleButton_Gbutton.GroupIndex = 1;
            simpleButton_Gbutton.TabIndex = t.myInt32(groupno);
            simpleButton_Gbutton.BorderStyle = BorderStyles.UltraFlat;

            //simpleButton_Gbutton.Checked = t.myVisible(visible);
            simpleButton_Gbutton.Click += new System.EventHandler(evg.checkSchedulerGroupButton_Click);

            simpleButton_Gbutton.AppearanceHovered.BackColor = v.colorOrder;
            simpleButton_Gbutton.AppearanceHovered.Options.UseBackColor = true;

            simpleButton_Gbutton.Enter += new System.EventHandler(ev.btn_Navigotor_Enter);
            simpleButton_Gbutton.Leave += new System.EventHandler(ev.btn_Navigotor_Leave);
            */
            tButtonPanel.Controls.Add(panel1);
            panel1.BringToFront();
        }


        public void tCalenderAndScheduler_Create(DataRow row_Table, DataSet dsFields, DataSet dsData,
            DevExpress.XtraScheduler.SchedulerControl tSchedulerControl, Control tPanelControl)
        {
            tToolBox t = new tToolBox();
            tEventsGrid evg = new tEventsGrid();
            tCreateObject co = new tCreateObject();

            string SoftwareCode = row_Table["SOFTWARE_CODE"].ToString();
            string ProjectCode = row_Table["PROJECT_CODE"].ToString();
            string TableIPCode = row_Table["TABLE_CODE"].ToString() + "." + row_Table["IP_CODE"].ToString();
            if (t.IsNotNull(SoftwareCode))
                TableIPCode = SoftwareCode + "/" + ProjectCode + "/" + TableIPCode;

            string Prop_View = t.Set(row_Table["PROP_VIEWS"].ToString(), "", "");
            PROP_VIEWS_IP JSON_PropView = null;
            string Old_PropView = string.Empty;

            if (t.IsNotNull(Prop_View))
                Preparing_PropView_(Prop_View, ref Old_PropView, ref JSON_PropView);


            DevExpress.XtraEditors.PanelControl tButtonPanel = co.Create_GroupButtons_Panel("tSchedulerButtons");

            SchedulerButtonAdd(TableIPCode, tButtonPanel);

            tPanelControl.Controls.Add(tButtonPanel);


            // Create a new Scheduler storage.
            //DevExpress.XtraScheduler.SchedulerStorage schedulerStorage1 = new DevExpress.XtraScheduler.SchedulerStorage(this.components);
            //DevExpress.XtraScheduler.SchedulerDataStorage schedulerDataStorage1 = new DevExpress.XtraScheduler.SchedulerDataStorage(this.components);
            //SchedulerControl scheduler = new SchedulerControl();

            SchedulerDataStorage storage = new SchedulerDataStorage();
            storage.AppointmentDependencies.AutoReload = true;
            storage.Appointments.AutoReload = true;
            storage.Resources.AutoReload = true;

            //if (t.IsNotNull(dsData))
                storage.Appointments.DataSource = dsData.Tables[0];
            //storage.Appointments.DataMember = "Appointments";
            //storage.Resources.DataSource = dsData;
            //storage.Resources.DataMember = "Resources";

            string schedulerViewType = null; 

            string startDateFieldName = "";
            if (JSON_PropView != null)
                startDateFieldName = Preparing_Mappings(JSON_PropView, storage, ref schedulerViewType);

            tSchedulerControl.BeginInit();
            
            if (schedulerViewType == "Agenda")
                tSchedulerControl.ActiveViewType = SchedulerViewType.Agenda;
            if (schedulerViewType == "Day")
                tSchedulerControl.ActiveViewType = SchedulerViewType.Day;
            if (schedulerViewType == "FullWeek")
                tSchedulerControl.ActiveViewType = SchedulerViewType.FullWeek;
            if (schedulerViewType == "Gantt")
                tSchedulerControl.ActiveViewType = SchedulerViewType.Gantt;
            if (schedulerViewType == "Month")
                tSchedulerControl.ActiveViewType = SchedulerViewType.Month;
            if (schedulerViewType == "Timeline")
                tSchedulerControl.ActiveViewType = SchedulerViewType.Timeline;
            if (schedulerViewType == "Week")
                tSchedulerControl.ActiveViewType = SchedulerViewType.Week;
            if (schedulerViewType == "WorkWeek")
                tSchedulerControl.ActiveViewType = SchedulerViewType.WorkWeek;

            tSchedulerControl.MonthView.AppointmentDisplayOptions.StartTimeVisibility = AppointmentTimeVisibility.Auto;

            if (t.IsNotNull(startDateFieldName) && t.IsNotNull(dsData))
                tSchedulerControl.Start = Convert.ToDateTime(dsData.Tables[0].Rows[0][startDateFieldName].ToString());

            tSchedulerControl.DataStorage = storage;
            /// tSchedulerControl.AccessibleDescription  <<< üzerinde zaten bazı bilgiler yüklenmiş
            ///
            //tSchedulerControl.AccessibleDescription = storage.Appointments.Mappings.AppointmentId.ToString();
            //tSchedulerControl.AccessibleDefaultActionDescription = storage.Appointments.Mappings.Start.ToString();
            tSchedulerControl.AccessibleDefaultActionDescription =
                storage.Appointments.Mappings.AppointmentId.ToString() + "||" +
                storage.Appointments.Mappings.Start.ToString() + "||";
            //SchedulerViewType.Agenda
            //SchedulerViewType.Day
            //SchedulerViewType.FullWeek
            //SchedulerViewType.Gantt
            //SchedulerViewType.Month
            //SchedulerViewType.Timeline
            //SchedulerViewType.Week
            //SchedulerViewType.WorkWeek

            //tSchedulerControl.AgendaView
            //tSchedulerControl.DayView.VisibleTime = true;
            //tSchedulerControl.FullWeekView.VisibleTime =    

            tSchedulerControl.MonthView.DateTimeScrollbarVisible = true;
            tSchedulerControl.TimelineView.DateTimeScrollbarVisible = true;
            tSchedulerControl.WeekView.DateTimeScrollbarVisible = true;
            
            // Saat aralığı            
            tSchedulerControl.DayView.TimeScale = TimeSpan.FromHours(1);
            // mesai başlangıç saatlerini ayarlar
            tSchedulerControl.DayView.WorkTime.Start = TimeSpan.FromHours(7); // Başlangıç saati: 09:00
            tSchedulerControl.DayView.WorkTime.End = TimeSpan.FromHours(23); // Bitiş saati: 18:00
            // Sadece çalışma saatlerinin görünmesini sağlayın
            tSchedulerControl.DayView.ShowWorkTimeOnly = true;

            tSchedulerControl.OptionsCustomization.AllowDisplayAppointmentForm = AllowDisplayAppointmentForm.Never;
            tSchedulerControl.OptionsCustomization.AllowAppointmentCreate = UsedAppointmentType.None;

            tSchedulerControl.DoubleClick += new System.EventHandler(evg.myGridView_DoubleClick);

            tSchedulerControl.MonthView.AppointmentDisplayOptions.StartTimeVisibility = AppointmentTimeVisibility.Never;
            tSchedulerControl.MonthView.AppointmentDisplayOptions.EndTimeVisibility = AppointmentTimeVisibility.Never;
            tSchedulerControl.MonthView.AppointmentDisplayOptions.ShowReminder = false;

            tSchedulerControl.AppointmentDrag += evg.mySchedulerControl_AppointmentDrag;
            tSchedulerControl.AppointmentDrop += evg.mySchedulerControl_AppointmentDrop;

            //tSchedulerControl.WorkWeekView.VisibleTime = true;

            timeRulerAdd(tSchedulerControl);
            
            tSchedulerControl.EndInit();

        }

        private void timeRulerAdd(DevExpress.XtraScheduler.SchedulerControl scheduler)
        {
            //DevExpress.XtraScheduler.TimeRuler timeRuler = new DevExpress.XtraScheduler.TimeRuler()

            scheduler.DayView.TimeRulers.Add(v.timeRuler);  //(new DevExpress.XtraScheduler.TimeRuler());

            // Display the time marker if the view contains a current date.
            scheduler.DayView.TimeMarkerVisibility = TimeMarkerVisibility.TodayView;
            // Display the time indicator in the current date's column only.
            scheduler.DayView.TimeIndicatorDisplayOptions.Visibility = TimeIndicatorVisibility.CurrentDate;
            // Show the time indicator on top when it overlaps an appointment.
            scheduler.DayView.TimeIndicatorDisplayOptions.ShowOverAppointment = true;
            // Hide the time marker in the second time ruler.
            //scheduler.DayView.TimeRulers[1].TimeMarkerVisibility = TimeMarkerVisibility.Never;

            scheduler.DayView.TimeRulers[0].ShowCurrentTime = CurrentTimeVisibility.Always;
            scheduler.DayView.TimeRulers[0].Visible = true;
            scheduler.DayView.ShowDayHeaders = true;
            scheduler.DayView.ShowAllDayArea = false;// true;
            scheduler.DayView.ShowWorkTimeOnly = false; // Normal Çalışma saatleri 9-18 arası

            scheduler.DayView.DayCount = 14;
            scheduler.DayView.TopRowTime = DateTime.Now.AddHours(-1).TimeOfDay;

            scheduler.AgendaView.DayCount = 14;
            scheduler.AgendaView.AppointmentDisplayOptions.ShowLabel = true;
            scheduler.AgendaView.AppointmentDisplayOptions.StatusDisplayType = AppointmentStatusDisplayType.Bounds;

            scheduler.Start = DateTime.Today.AddDays(-7);

        }


        #endregion CalenderAndScheduler

        #region ChartControl
        public void tChartControl_Create(DataRow row_Table, DataSet dsFields, DataSet dsData,
            DevExpress.XtraCharts.ChartControl tChartControl)
        {
            tToolBox t = new tToolBox();
            tEventsGrid evg = new tEventsGrid();
            tCreateObject co = new tCreateObject();

            string SoftwareCode = row_Table["SOFTWARE_CODE"].ToString();
            string ProjectCode = row_Table["PROJECT_CODE"].ToString();
            string TableIPCode = row_Table["TABLE_CODE"].ToString() + "." + row_Table["IP_CODE"].ToString();
            if (t.IsNotNull(SoftwareCode))
                TableIPCode = SoftwareCode + "/" + ProjectCode + "/" + TableIPCode;

            string Prop_View = t.Set(row_Table["PROP_VIEWS"].ToString(), "", "");
            //PROP_VIEWS_IP JSON_PropView = null;

            string headerCapiton = "";

            if (t.IsNotNull(dsData))
                headerCapiton = dsData.Tables[0].Rows[0]["Lkp_GrupKonusu"].ToString();
            else return;
            //Series series = new Series(headerCapiton, ViewType.Pie);


            // Create an empty chart.
            //ChartControl pieChart = new ChartControl();

            // Create a pie series.
            //Series series1 = new Series(headerCapiton, ViewType.Pie3D);
            Series series1 = new Series(headerCapiton, ViewType.Pie);
            /*
            // Populate the series with points.
            series1.Points.Add(new SeriesPoint("Russia", 17.0752));
            series1.Points.Add(new SeriesPoint("Canada", 9.98467));
            series1.Points.Add(new SeriesPoint("USA", 9.63142));
            series1.Points.Add(new SeriesPoint("China", 9.59696));
            series1.Points.Add(new SeriesPoint("Brazil", 8.511965));
            series1.Points.Add(new SeriesPoint("Australia", 7.68685));
            series1.Points.Add(new SeriesPoint("India", 3.28759));
            series1.Points.Add(new SeriesPoint("Others", 81.2));
            */
            series1.ArgumentDataMember = "Lkp_Konu";
            series1.ValueDataMembers.AddRange(new string[] { "Lkp_AdaySayisi" });
            series1.DataSource = dsData.Tables[0];
            
            // Add the series to the chart.
            tChartControl.Series.Add(series1);

            series1.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True;
            // Format the the series labels.
            //series1.Label.TextPattern = "{A} : {V} ({VP:P2})";
            series1.Label.TextPattern = "{A} : {V} ({VP:P0})";

            // Adjust the position of series labels. 
            //((Pie3DSeriesLabel)series1.Label).Position = PieSeriesLabelPosition.TwoColumns;
            ((PieSeriesLabel)series1.Label).Position = PieSeriesLabelPosition.TwoColumns;

            // Detect overlapping of series labels.
            ((PieSeriesLabel)series1.Label).ResolveOverlappingMode = ResolveOverlappingMode.Default;

            // Access the view-type-specific options of the series.
            //Pie3DSeriesView myView = (Pie3DSeriesView)series1.View;
            PieSeriesView myView = (PieSeriesView)series1.View;

            // Show a title for the series.
            myView.Titles.Add(new SeriesTitle());
            myView.Titles[0].Text = series1.Name;

            // Specify a data filter to explode points.
            myView.ExplodedPointsFilters.Add(new SeriesPointFilter(SeriesPointKey.Value_1,
                DataFilterCondition.GreaterThanOrEqual, 9));
            myView.ExplodedPointsFilters.Add(new SeriesPointFilter(SeriesPointKey.Argument,
                DataFilterCondition.NotEqual, "Diğerleri"));
            myView.ExplodeMode = PieExplodeMode.None;
            myView.ExplodedDistancePercentage = 30;
            myView.RuntimeExploding = true;
            myView.HeightToWidthRatio = 1; // 0.75;

            // Hide the legend (if necessary).
            tChartControl.Legend.Visibility = DevExpress.Utils.DefaultBoolean.False;

            /*
            //Create legent
            Legend legend = tChartControl.Legend;

            // Display the chart control's legend.
            legend.Visible = true;

            // Define its margins and alignment relative to the diagram.
            legend.Margins.All = 8;
            legend.AlignmentHorizontal = LegendAlignmentHorizontal.RightOutside;
            legend.AlignmentVertical = LegendAlignmentVertical.Top;

            // Define the layout of items within the legend.
            legend.Direction = LegendDirection.LeftToRight;
            legend.EquallySpacedItems = true;
            legend.HorizontalIndent = 8;
            legend.VerticalIndent = 8;
            legend.TextVisible = true;
            legend.TextOffset = 8;
            legend.MarkerVisible = true;
            legend.MarkerSize = new Size(20, 20);
            legend.Padding.All = 4;

            // Define the limits for the legend to occupy the chart's space.
            legend.MaxHorizontalPercentage = 50;
            legend.MaxVerticalPercentage = 50;

            // Customize the legend appearance.
            legend.BackColor = Color.Beige;
            legend.FillStyle.FillMode = FillMode.Gradient;
            ((RectangleGradientFillOptions)legend.FillStyle.Options).Color2 = Color.Bisque;

            legend.Border.Visible = true;
            legend.Border.Color = Color.DarkBlue;
            legend.Border.Thickness = 2;

            legend.Shadow.Visible = true;
            legend.Shadow.Color = Color.LightGray;
            legend.Shadow.Size = 2;

            // Customize the legend text properties.
            legend.Antialiasing = false;
            //legend.Font = new Font("Arial", 9, FontStyle.Bold);
            //legend.TextColor = Color.DarkBlue;
            */
            
            // Dock the chart into its parent, and add it to the current form.
            tChartControl.Dock = DockStyle.Fill;

            if (dsData != null)
                if (dsData.Tables.Count > 0)
                    tChartControl.DataSource = dsData.Tables[0];

        }

        #endregion ChartControl

        #endregion All Views

        #region SubFunctions for Grid


        #region Preparing_View

        private void Preparing_PropView_(string Prop_View,
            ref string Old_PropView,
            ref PROP_VIEWS_IP JSON_PropView)
        {
            //string s1 = "=ROW_PROP_VIEWS:";
            string s2 = (char)34 + "ALLPROP" + (char)34 + ": {";

            //if (Prop_View.IndexOf(s1) > -1)
            //{
            //    Old_PropView = Prop_View;
            //    JSON_PropView = null;
            //}

            if (Prop_View.IndexOf(s2) > -1)
            {
                /*
                PROP_VIEWS_IP packet = new PROP_VIEWS_IP();
                Prop_View = Prop_View.Replace((char)34, (char)39);
                //JSON_PropView = JsonConvert.DeserializeAnonymousType(Prop_View, packet);
                */
                tToolBox t = new tToolBox();
                JSON_PropView = t.readProp<PROP_VIEWS_IP>(Prop_View);

                Old_PropView = string.Empty;
            }
        }

        private string Preparing_Mappings(PROP_VIEWS_IP prop_, SchedulerDataStorage storage, ref string schedulerViewType)
        {
            tToolBox t = new tToolBox();
             
            storage.Appointments.Mappings.AppointmentId = prop_.SCHEDULER.AppointmentId.ToString(); //"Id";
            storage.Appointments.Mappings.Start = prop_.SCHEDULER.StartDate.ToString();// "BaslamaSaati";
            storage.Appointments.Mappings.End = prop_.SCHEDULER.EndDate.ToString();// "BitisSaati";
            storage.Appointments.Mappings.Location = prop_.SCHEDULER.Location.ToString(); // "Lkp_DerslikAdiTipiId";
            storage.Appointments.Mappings.Subject = prop_.SCHEDULER.Subject.ToString();// "Lkp_DerslikTipiId";
            storage.Appointments.Mappings.Label = prop_.SCHEDULER.Label.ToString(); // "AdayNo";
            storage.Appointments.Mappings.Description = prop_.SCHEDULER.Description.ToString(); // "Lkp_DonemTipiGrupTipiSubeTipi"; 
            //storage.Appointments.Mappings.AllDay = 

            if (prop_.SCHEDULER.SchedulerViewType != null)
                schedulerViewType = prop_.SCHEDULER.SchedulerViewType;
            else schedulerViewType = "Month";

            return prop_.SCHEDULER.StartDate.ToString();
        }

        private void Preparing_View_JSON(PROP_VIEWS_IP prop_,
            DevExpress.XtraDataLayout.DataLayoutControl tDLayout)
        {
            tToolBox t = new tToolBox();

            /* DATALAYOUT VIEW */
            
            string MOVEFOCUS = prop_.DATALAYOUT.MOVEFOCUS.ToString();

            if (t.IsNotNull(MOVEFOCUS) == false)
            {
                tDLayout.OptionsFocus.MoveFocusDirection = MoveFocusDirection.DownThenAcross;
            }
            else
            {
                if (MOVEFOCUS == "ACROSS") tDLayout.OptionsFocus.MoveFocusDirection = MoveFocusDirection.AcrossThenDown;
                if (MOVEFOCUS == "DOWN") tDLayout.OptionsFocus.MoveFocusDirection = MoveFocusDirection.DownThenAcross;
            }
        }

        private void Preparing_View_JSON(PROP_VIEWS_IP prop_,
                     Control mainControl,
                     GridView tGridView,
                     AdvBandedGridView tAdvBGridView,
                     TreeList tTreeList,
                     DevExpress.XtraGrid.Views.Tile.TileView tTileView,
                     DevExpress.XtraGrid.Views.Card.CardView tCardView)
        {
            tToolBox t = new tToolBox();

            #region Tanımlar

            //PROP_VIEWS_IP packet = new PROP_VIEWS_IP();
            //jsPacket = jsPacket.Replace((char)34, (char)39);
            //var prop_ = JsonConvert.DeserializeAnonymousType(jsPacket, packet);

            /// ALLPROP
            string TABSTOP = "";
            if (prop_.ALLPROP.TABSTOP != null) TABSTOP = prop_.ALLPROP.TABSTOP.ToString();
            string GROUPFNAME1 = prop_.ALLPROP.GROUPFNAME1.ToString();
            string GROUPFNAME2 = prop_.ALLPROP.GROUPFNAME2.ToString();
            string GROUPFNAME3 = prop_.ALLPROP.GROUPFNAME3.ToString();
            /// GRID
            string ALLOWCELLMERGE = prop_.GRID.ALLOWCELLMERGE.ToString();
            string COLUMNAUTOVIEW = prop_.GRID.COLUMNAUTOVIEW.ToString();
            string EVENROW = prop_.GRID.EVENROW.ToString();
            string ODDROW = prop_.GRID.ODDROW.ToString();
            string NEWITEMROWPOSITION = prop_.GRID.NEWITEMROWPOSITION.ToString();
            string SAUTOFILTERROW = prop_.GRID.SAUTOFILTERROW.ToString();
            string SCOLUMNHEADERS = prop_.GRID.SCOLUMNHEADERS.ToString();
            string SFOOTER = prop_.GRID.SFOOTER.ToString();
            string SGROUPEDCOLUMNS = prop_.GRID.SGROUPEDCOLUMNS.ToString();
            string SGROUPPANEL = prop_.GRID.SGROUPPANEL.ToString();
            string SHORIZONTALLINES = prop_.GRID.SHORIZONTALLINES.ToString();
            string SINDICATOR = prop_.GRID.SINDICATOR.ToString();
            string SPRIVIEW = prop_.GRID.SPRIVIEW.ToString();
            string PRIVIEWFIELDNAME = prop_.GRID.PRIVIEWFIELDNAME.ToString();
            string SVERTICALLINES = prop_.GRID.SVERTICALLINES.ToString();
            string SVIEWCAPTION = prop_.GRID.SVIEWCAPTION.ToString();
            string INVERTSELECTION = prop_.GRID.INVERTSELECTION.ToString();
            string MULTISELECTION = prop_.GRID.MULTISELECTION.ToString();
            /* TILE VIEW */
            string TL_ITEMWIDTH = prop_.TILE.TL_ITEMWIDTH.ToString();
            string TL_ITEMHEIGHT = prop_.TILE.TL_ITEMHEIGHT.ToString();
            string TL_ORIENTATION = prop_.TILE.TL_ORIENTATION.ToString();
            /* TREE LIST */
            string COLLEXP = prop_.TREE.COLLEXPAND.ToString();

            #endregion Tanımlar

            #region GridView
            if (tGridView != null)
            {
                if (ALLOWCELLMERGE == "TRUE") 
                    tGridView.OptionsView.AllowCellMerge = true;
                if (ALLOWCELLMERGE == "FALSE") tGridView.OptionsView.AllowCellMerge = false;

                if (COLUMNAUTOVIEW == "TRUE") tGridView.OptionsView.ColumnAutoWidth = true;
                if (COLUMNAUTOVIEW == "FALSE") tGridView.OptionsView.ColumnAutoWidth = false;

                if (EVENROW == "TRUE") tGridView.OptionsView.EnableAppearanceEvenRow = true;
                if (EVENROW == "FALSE") tGridView.OptionsView.EnableAppearanceEvenRow = false;

                if (ODDROW == "TRUE") tGridView.OptionsView.EnableAppearanceOddRow = true;
                if (ODDROW == "FALSE") tGridView.OptionsView.EnableAppearanceOddRow = false;

                if (NEWITEMROWPOSITION == "TOP") tGridView.OptionsView.NewItemRowPosition = NewItemRowPosition.Top;
                if (NEWITEMROWPOSITION == "BOTTOM") tGridView.OptionsView.NewItemRowPosition = NewItemRowPosition.Bottom;

                if (SAUTOFILTERROW == "TRUE") tGridView.OptionsView.ShowAutoFilterRow = true;
                if (SAUTOFILTERROW == "FALSE") tGridView.OptionsView.ShowAutoFilterRow = false;

                if (SCOLUMNHEADERS == "TRUE") tGridView.OptionsView.ShowColumnHeaders = true;
                if (SCOLUMNHEADERS == "FALSE") tGridView.OptionsView.ShowColumnHeaders = false;

                if (SFOOTER == "TRUE") tGridView.OptionsView.ShowFooter = true;
                if (SFOOTER == "FALSE") tGridView.OptionsView.ShowFooter = false;

                if (SGROUPEDCOLUMNS == "TRUE") tGridView.OptionsView.ShowGroupedColumns = true;
                if (SGROUPEDCOLUMNS == "FALSE") tGridView.OptionsView.ShowGroupedColumns = false;

                if (SGROUPPANEL == "TRUE") tGridView.OptionsView.ShowGroupPanel = true;
                if (SGROUPPANEL == "FALSE") tGridView.OptionsView.ShowGroupPanel = false;

                if (SHORIZONTALLINES == "TRUE") tGridView.OptionsView.ShowHorizontalLines = DevExpress.Utils.DefaultBoolean.True;
                if (SHORIZONTALLINES == "FALSE") tGridView.OptionsView.ShowHorizontalLines = DevExpress.Utils.DefaultBoolean.False;

                if (SINDICATOR == "TRUE") tGridView.OptionsView.ShowIndicator = true;
                if (SINDICATOR == "FALSE") tGridView.OptionsView.ShowIndicator = false;

                if (SPRIVIEW == "TRUE") tGridView.OptionsView.ShowPreview = true;
                if (SPRIVIEW == "FALSE") tGridView.OptionsView.ShowPreview = false;

                if (SPRIVIEW == "TRUE") tGridView.PreviewFieldName = PRIVIEWFIELDNAME;

                if (SVERTICALLINES == "TRUE") tGridView.OptionsView.ShowVerticalLines = DevExpress.Utils.DefaultBoolean.True;
                if (SVERTICALLINES == "FALSE") tGridView.OptionsView.ShowVerticalLines = DevExpress.Utils.DefaultBoolean.True;

                if (SVIEWCAPTION == "TRUE") tGridView.OptionsView.ShowViewCaption = true;
                if (SVIEWCAPTION == "FALSE") tGridView.OptionsView.ShowViewCaption = false;

                if (INVERTSELECTION == "TRUE") tGridView.OptionsSelection.InvertSelection = true;
                if (INVERTSELECTION == "FALSE") tGridView.OptionsSelection.InvertSelection = false;

                if (MULTISELECTION == "TRUE") tGridView.OptionsSelection.MultiSelect = true;
                if (MULTISELECTION == "FALSE") tGridView.OptionsSelection.MultiSelect = false;

                //t.Gridi_Grupla(grid, Group0, Group1, Group2, ref Group0_Old, ref Group1_Old, ref Group2_Old);
                if ((GROUPFNAME1 != "") && (GROUPFNAME1 != "null"))
                    tGridView.Columns[GROUPFNAME1].GroupIndex = 0;
                if ((GROUPFNAME2 != "") && (GROUPFNAME2 != "null"))
                    tGridView.Columns[GROUPFNAME2].GroupIndex = 1;
                if ((GROUPFNAME3 != "") && (GROUPFNAME3 != "null"))
                    tGridView.Columns[GROUPFNAME3].GroupIndex = 2;
                //if ( == "TRUE") tGridView.OptionsView. = true;

                //if (COLLEXP == "EXPAND") tGridView.OptionsBehavior.AutoExpandAllGroups = true;
                if (COLLEXP == "COLLAPSE") tGridView.OptionsBehavior.AutoExpandAllGroups = false;
                else tGridView.OptionsBehavior.AutoExpandAllGroups = true;

            }
            #endregion GridView

            #region AdvBandedGridView
            if (tAdvBGridView != null)
            {
                if (ALLOWCELLMERGE == "TRUE") tAdvBGridView.OptionsView.AllowCellMerge = true;
                if (ALLOWCELLMERGE == "FALSE") tAdvBGridView.OptionsView.AllowCellMerge = false;

                if (COLUMNAUTOVIEW == "TRUE") tAdvBGridView.OptionsView.ColumnAutoWidth = true;
                if (COLUMNAUTOVIEW == "FALSE") tAdvBGridView.OptionsView.ColumnAutoWidth = false;

                if (EVENROW == "TRUE") tAdvBGridView.OptionsView.EnableAppearanceEvenRow = true;
                if (EVENROW == "FALSE") tAdvBGridView.OptionsView.EnableAppearanceEvenRow = false;

                if (ODDROW == "TRUE") tAdvBGridView.OptionsView.EnableAppearanceOddRow = true;
                if (ODDROW == "FALSE") tAdvBGridView.OptionsView.EnableAppearanceOddRow = false;

                if (NEWITEMROWPOSITION == "TOP") tAdvBGridView.OptionsView.NewItemRowPosition = NewItemRowPosition.Top;
                if (NEWITEMROWPOSITION == "BOTTOM") tAdvBGridView.OptionsView.NewItemRowPosition = NewItemRowPosition.Bottom;

                if (SAUTOFILTERROW == "TRUE") tAdvBGridView.OptionsView.ShowAutoFilterRow = true;
                if (SAUTOFILTERROW == "FALSE") tAdvBGridView.OptionsView.ShowAutoFilterRow = false;

                if (SCOLUMNHEADERS == "TRUE") tAdvBGridView.OptionsView.ShowColumnHeaders = true;
                if (SCOLUMNHEADERS == "FALSE") tAdvBGridView.OptionsView.ShowColumnHeaders = false;

                if (SFOOTER == "TRUE") tAdvBGridView.OptionsView.ShowFooter = true;
                if (SFOOTER == "FALSE") tAdvBGridView.OptionsView.ShowFooter = false;

                if (SGROUPEDCOLUMNS == "TRUE") tAdvBGridView.OptionsView.ShowGroupedColumns = true;
                if (SGROUPEDCOLUMNS == "FALSE") tAdvBGridView.OptionsView.ShowGroupedColumns = false;

                if (SGROUPPANEL == "TRUE") tAdvBGridView.OptionsView.ShowGroupPanel = true;
                if (SGROUPPANEL == "FALSE") tAdvBGridView.OptionsView.ShowGroupPanel = false;

                if (SHORIZONTALLINES == "TRUE") tAdvBGridView.OptionsView.ShowHorizontalLines = DevExpress.Utils.DefaultBoolean.True;
                if (SHORIZONTALLINES == "FALSE") tAdvBGridView.OptionsView.ShowHorizontalLines = DevExpress.Utils.DefaultBoolean.False;

                if (SINDICATOR == "TRUE") tAdvBGridView.OptionsView.ShowIndicator = true;
                if (SINDICATOR == "FALSE") tAdvBGridView.OptionsView.ShowIndicator = false;

                if (SPRIVIEW == "TRUE") tAdvBGridView.OptionsView.ShowPreview = true;
                if (SPRIVIEW == "FALSE") tAdvBGridView.OptionsView.ShowPreview = false;

                if (SPRIVIEW == "TRUE") tAdvBGridView.PreviewFieldName = PRIVIEWFIELDNAME;

                if (SVERTICALLINES == "TRUE") tAdvBGridView.OptionsView.ShowVerticalLines = DevExpress.Utils.DefaultBoolean.True;
                if (SVERTICALLINES == "FALSE") tAdvBGridView.OptionsView.ShowVerticalLines = DevExpress.Utils.DefaultBoolean.True;

                if (SVIEWCAPTION == "TRUE") tAdvBGridView.OptionsView.ShowViewCaption = true;
                if (SVIEWCAPTION == "FALSE") tAdvBGridView.OptionsView.ShowViewCaption = false;

                if (INVERTSELECTION == "TRUE") tAdvBGridView.OptionsSelection.InvertSelection = true;
                if (INVERTSELECTION == "FALSE") tAdvBGridView.OptionsSelection.InvertSelection = false;

                if (MULTISELECTION == "TRUE") tAdvBGridView.OptionsSelection.MultiSelect = true;
                if (MULTISELECTION == "FALSE") tAdvBGridView.OptionsSelection.MultiSelect = false;

                if ((GROUPFNAME1 != "") && (GROUPFNAME1 != "null"))
                    tAdvBGridView.Columns[GROUPFNAME1].GroupIndex = 0;
                if ((GROUPFNAME2 != "") && (GROUPFNAME2 != "null"))
                    tAdvBGridView.Columns[GROUPFNAME2].GroupIndex = 1;
                if ((GROUPFNAME3 != "") && (GROUPFNAME3 != "null"))
                    tAdvBGridView.Columns[GROUPFNAME3].GroupIndex = 2;

            }
            #endregion AdvBandedGridView

            #region tTreeList
            if (tTreeList != null)
            {
                if (COLLEXP == "EXPAND") tTreeList.ExpandAll();
                if (COLLEXP == "COLLAPSE") tTreeList.CollapseAll();
            }
            #endregion tTreeList

            #region tTileView
            if (tTileView != null)
            {
                if ((TL_ITEMWIDTH != "") && (TL_ITEMHEIGHT != ""))
                {
                    tTileView.OptionsTiles.ItemSize = new System.Drawing.Size(t.myInt32(TL_ITEMWIDTH), t.myInt32(TL_ITEMHEIGHT));
                }
                if (TL_ORIENTATION == "Horizontal") tTileView.OptionsTiles.Orientation = Orientation.Horizontal;
                if (TL_ORIENTATION == "Vertical") tTileView.OptionsTiles.Orientation = Orientation.Vertical;

                if (t.IsNotNull(GROUPFNAME1))
                {
                    tTileView.Columns[GROUPFNAME1].GroupIndex = 0;
                    tTileView.Columns[GROUPFNAME1].Visible = false;

                    tTileView.OptionsTiles.IndentBetweenGroups = 16;
                    tTileView.Appearance.GroupText.Font = new System.Drawing.Font("Tahoma", 10);
                    tTileView.Appearance.GroupText.ForeColor = v.AppearanceTextColor; // new System.Drawing.Color 
                }

                //if (t.IsNotNull(GROUPFNAME2))
                //{
                //    tTileView.Columns[GROUPFNAME2].GroupIndex = 1;
                //    tTileView.Columns[GROUPFNAME2].Visible = false;

                //    tTileView.OptionsTiles.IndentBetweenGroups = 8;
                //    tTileView.Appearance.GroupText.Font = new System.Drawing.Font("Tahoma", 10);
                //    tTileView.Appearance.GroupText.ForeColor = v.AppearanceTextColor; // new System.Drawing.Color 
                //}

            }
            #endregion tTileView

            if (tCardView != null)
            {

                if ((GROUPFNAME1 != "") && (GROUPFNAME1 != "null"))
                {
                    // "{LKP_FNS_TIPI}"; yemiyor
                    // "{3}"; field no  şeklinde çalışyor
                    tCardView.CardCaptionFormat = "{" + GROUPFNAME1 + "}";
                    tCardView.OptionsView.ShowCardCaption = true;
                }

                if ((GROUPFNAME2 != "") && (GROUPFNAME2 != "null"))
                {
                    // "{1}, {3}"   şeklindede çaılşyor
                    tCardView.CardCaptionFormat = "{" + GROUPFNAME1 + "} {" + GROUPFNAME2 + "}";
                    tCardView.OptionsView.ShowCardCaption = true;
                }

            }

            // GridControl or TreeList
            //

            if (TABSTOP == "FALSE")
            {
                // InputPanel in tabStop kararı burada alınıyor
                // bu fonksiyondan dönüşte mainControl ün tabStop una bakarak 
                // altındaki navigator panelide tabStop false ediliyor ..
                //
                if (mainControl.GetType().ToString() == "DevExpress.XtraGrid.GridControl")
                    ((DevExpress.XtraGrid.GridControl)mainControl).TabStop = false;
                if (mainControl.GetType().ToString() == "DevExpress.XtraTreeList.TreeList")
                    ((DevExpress.XtraTreeList.TreeList)mainControl).TabStop = false;
                if (mainControl.GetType().ToString() == "DevExpress.XtraVerticalGrid.VGridControl")
                    ((DevExpress.XtraVerticalGrid.VGridControl)mainControl).TabStop = false;
            }
        }

        private void Preparing_View_OLD(string Prop_View,
                     GridView tGridView,
                     AdvBandedGridView tAdvBGridView,
                     TreeList tTreeList,
                     DevExpress.XtraGrid.Views.Tile.TileView tTileView)
        {
            tToolBox t = new tToolBox();

            #region Tanımlar
            string ALLOWCELLMERGE = t.MyProperties_Get(Prop_View, "ALLOWCELLMERGE:");
            string COLUMNAUTOVIEW = t.MyProperties_Get(Prop_View, "COLUMNAUTOVIEW:");
            string EVENROW = t.MyProperties_Get(Prop_View, "EVENROW:");
            string ODDROW = t.MyProperties_Get(Prop_View, "ODDROW:");
            string NEWITEMROWPOSITION = t.MyProperties_Get(Prop_View, "NEWITEMROWPOSITION:");
            string SAUTOFILTERROW = t.MyProperties_Get(Prop_View, "SAUTOFILTERROW:");
            string SCOLUMNHEADERS = t.MyProperties_Get(Prop_View, "SCOLUMNHEADERS:");
            string SFOOTER = t.MyProperties_Get(Prop_View, "SFOOTER:");
            string SGROUPEDCOLUMNS = t.MyProperties_Get(Prop_View, "SGROUPEDCOLUMNS:");
            string SGROUPPANEL = t.MyProperties_Get(Prop_View, "SGROUPPANEL:");
            string SHORIZONTALLINES = t.MyProperties_Get(Prop_View, "SHORIZONTALLINES:");
            string SINDICATOR = t.MyProperties_Get(Prop_View, "SINDICATOR:");
            string SPRIVIEW = t.MyProperties_Get(Prop_View, "SPRIVIEW:");
            string PRIVIEWFIELDNAME = t.MyProperties_Get(Prop_View, "PRIVIEWFIELDNAME:");
            string SVERTICALLINES = t.MyProperties_Get(Prop_View, "SVERTICALLINES:");
            string SVIEWCAPTION = t.MyProperties_Get(Prop_View, "SVIEWCAPTION:");
            string INVERTSELECTION = t.MyProperties_Get(Prop_View, "INVERTSELECTION:");
            string MULTISELECTION = t.MyProperties_Get(Prop_View, "MULTISELECTION:");
            string GROUPFNAME1 = t.MyProperties_Get(Prop_View, "GROUPFNAME1:");
            string GROUPFNAME2 = t.MyProperties_Get(Prop_View, "GROUPFNAME2:");
            string GROUPFNAME3 = t.MyProperties_Get(Prop_View, "GROUPFNAME3:");
            /*string  = t.MyProperties_Get(Prop_View, ":");
            string  = t.MyProperties_Get(Prop_View, ":");
            string  = t.MyProperties_Get(Prop_View, ":");
            string  = t.MyProperties_Get(Prop_View, ":");
            string  = t.MyProperties_Get(Prop_View, ":");*/

            /* TILE VIEW */
            string TL_ITEMWIDTH = t.MyProperties_Get(Prop_View, "TL_ITEMWIDTH:");
            string TL_ITEMHEIGHT = t.MyProperties_Get(Prop_View, "TL_ITEMHEIGHT:");
            string TL_ORIENTATION = t.MyProperties_Get(Prop_View, "TL_ORIENTATION:");

            #endregion Tanımlar

            #region GridView
            if (tGridView != null)
            {
                if (ALLOWCELLMERGE == "TRUE") tGridView.OptionsView.AllowCellMerge = true;
                if (ALLOWCELLMERGE == "FALSE") tGridView.OptionsView.AllowCellMerge = false;

                if (COLUMNAUTOVIEW == "TRUE") tGridView.OptionsView.ColumnAutoWidth = true;
                if (COLUMNAUTOVIEW == "FALSE") tGridView.OptionsView.ColumnAutoWidth = false;

                if (EVENROW == "TRUE") tGridView.OptionsView.EnableAppearanceEvenRow = true;
                if (EVENROW == "FALSE") tGridView.OptionsView.EnableAppearanceEvenRow = false;

                if (ODDROW == "TRUE") tGridView.OptionsView.EnableAppearanceOddRow = true;
                if (ODDROW == "FALSE") tGridView.OptionsView.EnableAppearanceOddRow = false;

                if (NEWITEMROWPOSITION == "TOP") tGridView.OptionsView.NewItemRowPosition = NewItemRowPosition.Top;
                if (NEWITEMROWPOSITION == "BOTTOM") tGridView.OptionsView.NewItemRowPosition = NewItemRowPosition.Bottom;

                if (SAUTOFILTERROW == "TRUE") tGridView.OptionsView.ShowAutoFilterRow = true;
                if (SAUTOFILTERROW == "FALSE") tGridView.OptionsView.ShowAutoFilterRow = false;

                if (SCOLUMNHEADERS == "TRUE") tGridView.OptionsView.ShowColumnHeaders = true;
                if (SCOLUMNHEADERS == "FALSE") tGridView.OptionsView.ShowColumnHeaders = false;

                if (SFOOTER == "TRUE") tGridView.OptionsView.ShowFooter = true;
                if (SFOOTER == "FALSE") tGridView.OptionsView.ShowFooter = false;

                if (SGROUPEDCOLUMNS == "TRUE") tGridView.OptionsView.ShowGroupedColumns = true;
                if (SGROUPEDCOLUMNS == "FALSE") tGridView.OptionsView.ShowGroupedColumns = false;

                if (SGROUPPANEL == "TRUE") tGridView.OptionsView.ShowGroupPanel = true;
                if (SGROUPPANEL == "FALSE") tGridView.OptionsView.ShowGroupPanel = false;

                if (SHORIZONTALLINES == "TRUE") tGridView.OptionsView.ShowHorizontalLines = DevExpress.Utils.DefaultBoolean.True;
                if (SHORIZONTALLINES == "FALSE") tGridView.OptionsView.ShowHorizontalLines = DevExpress.Utils.DefaultBoolean.False;

                if (SINDICATOR == "TRUE") tGridView.OptionsView.ShowIndicator = true;
                if (SINDICATOR == "FALSE") tGridView.OptionsView.ShowIndicator = false;

                if (SPRIVIEW == "TRUE") tGridView.OptionsView.ShowPreview = true;
                if (SPRIVIEW == "FALSE") tGridView.OptionsView.ShowPreview = false;

                if (SPRIVIEW == "TRUE") tGridView.PreviewFieldName = PRIVIEWFIELDNAME;

                if (SVERTICALLINES == "TRUE") tGridView.OptionsView.ShowVerticalLines = DevExpress.Utils.DefaultBoolean.True;
                if (SVERTICALLINES == "FALSE") tGridView.OptionsView.ShowVerticalLines = DevExpress.Utils.DefaultBoolean.True;

                if (SVIEWCAPTION == "TRUE") tGridView.OptionsView.ShowViewCaption = true;
                if (SVIEWCAPTION == "FALSE") tGridView.OptionsView.ShowViewCaption = false;

                if (INVERTSELECTION == "TRUE") tGridView.OptionsSelection.InvertSelection = true;
                if (INVERTSELECTION == "FALSE") tGridView.OptionsSelection.InvertSelection = false;

                if (MULTISELECTION == "TRUE") tGridView.OptionsSelection.MultiSelect = true;
                if (MULTISELECTION == "FALSE") tGridView.OptionsSelection.MultiSelect = false;

                //t.Gridi_Grupla(grid, Group0, Group1, Group2, ref Group0_Old, ref Group1_Old, ref Group2_Old);
                if (GROUPFNAME1 != "")
                    tGridView.Columns[GROUPFNAME1].GroupIndex = 0;
                if (GROUPFNAME2 != "")
                    tGridView.Columns[GROUPFNAME2].GroupIndex = 1;
                if (GROUPFNAME3 != "")
                    tGridView.Columns[GROUPFNAME3].GroupIndex = 2;


                //if ( == "TRUE") tGridView.OptionsView. = true;
            }
            #endregion GridView

            #region AdvBandedGridView
            if (tAdvBGridView != null)
            {
                if (ALLOWCELLMERGE == "TRUE") tAdvBGridView.OptionsView.AllowCellMerge = true;
                if (ALLOWCELLMERGE == "FALSE") tAdvBGridView.OptionsView.AllowCellMerge = false;

                if (COLUMNAUTOVIEW == "TRUE") tAdvBGridView.OptionsView.ColumnAutoWidth = true;
                if (COLUMNAUTOVIEW == "FALSE") tAdvBGridView.OptionsView.ColumnAutoWidth = false;

                if (EVENROW == "TRUE") tAdvBGridView.OptionsView.EnableAppearanceEvenRow = true;
                if (EVENROW == "FALSE") tAdvBGridView.OptionsView.EnableAppearanceEvenRow = false;

                if (ODDROW == "TRUE") tAdvBGridView.OptionsView.EnableAppearanceOddRow = true;
                if (ODDROW == "FALSE") tAdvBGridView.OptionsView.EnableAppearanceOddRow = false;

                if (NEWITEMROWPOSITION == "TOP") tAdvBGridView.OptionsView.NewItemRowPosition = NewItemRowPosition.Top;
                if (NEWITEMROWPOSITION == "BOTTOM") tAdvBGridView.OptionsView.NewItemRowPosition = NewItemRowPosition.Bottom;

                if (SAUTOFILTERROW == "TRUE") tAdvBGridView.OptionsView.ShowAutoFilterRow = true;
                if (SAUTOFILTERROW == "FALSE") tAdvBGridView.OptionsView.ShowAutoFilterRow = false;

                if (SCOLUMNHEADERS == "TRUE") tAdvBGridView.OptionsView.ShowColumnHeaders = true;
                if (SCOLUMNHEADERS == "FALSE") tAdvBGridView.OptionsView.ShowColumnHeaders = false;

                if (SFOOTER == "TRUE") tAdvBGridView.OptionsView.ShowFooter = true;
                if (SFOOTER == "FALSE") tAdvBGridView.OptionsView.ShowFooter = false;

                if (SGROUPEDCOLUMNS == "TRUE") tAdvBGridView.OptionsView.ShowGroupedColumns = true;
                if (SGROUPEDCOLUMNS == "FALSE") tAdvBGridView.OptionsView.ShowGroupedColumns = false;

                if (SGROUPPANEL == "TRUE") tAdvBGridView.OptionsView.ShowGroupPanel = true;
                if (SGROUPPANEL == "FALSE") tAdvBGridView.OptionsView.ShowGroupPanel = false;

                if (SHORIZONTALLINES == "TRUE") tAdvBGridView.OptionsView.ShowHorizontalLines = DevExpress.Utils.DefaultBoolean.True;
                if (SHORIZONTALLINES == "FALSE") tAdvBGridView.OptionsView.ShowHorizontalLines = DevExpress.Utils.DefaultBoolean.False;

                if (SINDICATOR == "TRUE") tAdvBGridView.OptionsView.ShowIndicator = true;
                if (SINDICATOR == "FALSE") tAdvBGridView.OptionsView.ShowIndicator = false;

                if (SPRIVIEW == "TRUE") tAdvBGridView.OptionsView.ShowPreview = true;
                if (SPRIVIEW == "FALSE") tAdvBGridView.OptionsView.ShowPreview = false;

                if (SPRIVIEW == "TRUE") tAdvBGridView.PreviewFieldName = PRIVIEWFIELDNAME;

                if (SVERTICALLINES == "TRUE") tAdvBGridView.OptionsView.ShowVerticalLines = DevExpress.Utils.DefaultBoolean.True;
                if (SVERTICALLINES == "FALSE") tAdvBGridView.OptionsView.ShowVerticalLines = DevExpress.Utils.DefaultBoolean.True;

                if (SVIEWCAPTION == "TRUE") tAdvBGridView.OptionsView.ShowViewCaption = true;
                if (SVIEWCAPTION == "FALSE") tAdvBGridView.OptionsView.ShowViewCaption = false;

                if (INVERTSELECTION == "TRUE") tAdvBGridView.OptionsSelection.InvertSelection = true;
                if (INVERTSELECTION == "FALSE") tAdvBGridView.OptionsSelection.InvertSelection = false;

                if (MULTISELECTION == "TRUE") tAdvBGridView.OptionsSelection.MultiSelect = true;
                if (MULTISELECTION == "FALSE") tAdvBGridView.OptionsSelection.MultiSelect = false;

            }
            #endregion AdvBandedGridView

            #region tTreeList
            if (tTreeList != null)
            {

            }
            #endregion tTreeList

            #region tTileView
            if (tTileView != null)
            {
                if ((TL_ITEMWIDTH != "") && (TL_ITEMHEIGHT != ""))
                {
                    tTileView.OptionsTiles.ItemSize = new System.Drawing.Size(t.myInt32(TL_ITEMWIDTH), t.myInt32(TL_ITEMHEIGHT));
                }
                if (TL_ORIENTATION == "Horizontal") tTileView.OptionsTiles.Orientation = Orientation.Horizontal;
                if (TL_ORIENTATION == "Vertical") tTileView.OptionsTiles.Orientation = Orientation.Vertical;

                if (GROUPFNAME1 != "")
                {
                    tTileView.Columns[GROUPFNAME1].GroupIndex = 0;

                    tTileView.OptionsTiles.IndentBetweenGroups = 16;
                    tTileView.Appearance.GroupText.Font = new System.Drawing.Font("Tahoma", 14);
                    tTileView.Appearance.GroupText.ForeColor = v.AppearanceTextColor; // new System.Drawing.Color 
                }
            }
            #endregion tTileView
        }

        #endregion Preparing_View

        #region GridView Columns Create

        public void tGrid_Columns_Create(DataSet dsFields, DevExpress.XtraGrid.Views.WinExplorer.WinExplorerView tGridView, string TableIPCode)
        {
            tToolBox t = new tToolBox();
            Int16 i = 0;

            GridColumn column = null;
            string tcolumn_type = string.Empty;

            foreach (DataRow Row in dsFields.Tables[0].Rows)
            {
                if (tGridView != null)
                {
                    column = tGrid_Columns_Create(Row, i, v.obj_vw_BandedGridView, null, TableIPCode);
                    if (column != null)
                    {
                        tGridView.Columns.Add(column);

                        tcolumn_type = t.Set(Row["CMP_COLUMN_TYPE"].ToString(), Row["LKP_CMP_COLUMN_TYPE"].ToString(), "TextEdit");

                        //tGridView.ColumnSet.CheckBoxColumn = column;    

                        if (tcolumn_type == "TextColumn")
                            tGridView.ColumnSet.TextColumn = column; // this.columnName;
                        if (tcolumn_type == "DescriptionColumn")
                            tGridView.ColumnSet.DescriptionColumn = column; // this.columnName;
                        if (tcolumn_type == "GroupColumn")
                            tGridView.ColumnSet.GroupColumn = column;           // this.columnGroup;
                        if (tcolumn_type == "CheckBoxColumn")
                            tGridView.ColumnSet.CheckBoxColumn = column;        // this.columnCheck;

                        if (tcolumn_type == "PictureEdit")
                        {
                            tGridView.ColumnSet.ExtraLargeImageColumn = column; // this.columnImage;
                            tGridView.ColumnSet.LargeImageColumn = column;      // this.columnImage;
                            tGridView.ColumnSet.MediumImageColumn = column;     //  this.columnImage;
                            tGridView.ColumnSet.SmallImageColumn = column;      //  this.columnImage;
                        }

                    }
                }
                i++;
            }

            //if (tGridView.GetType().ToString() == "DevExpress.XtraGrid.Views.WinExplorer.WinExplorerView")
            //    ((DevExpress.XtraGrid.Views.WinExplorer.WinExplorerView)tGridView).Columns.Add(column);
        }

        public void tGrid_Columns_Create(DataSet dsFields, GridView tGridView, DataSet dsData, string TableIPCode)
        {
            tToolBox t = new tToolBox();
            Int16 i = 0;

            GridColumn column = null;

            foreach (DataRow Row in dsFields.Tables[0].Rows)
            {
                if (tGridView != null)
                {
                    column = tGrid_Columns_Create(Row, i, v.obj_vw_GridView, tGridView, TableIPCode);
                    if (column != null)
                        tGridView.Columns.Add(column);

                    string tFieldName = Row["LKP_FIELD_NAME"].ToString();
                    string fhint = Row["FHINT"].ToString();

                    if (t.IsNotNull(fhint))
                    {
                        column.OptionsColumn.AllowEdit = false;
                        column.ShowUnboundExpressionMenu = true;
                        column.UnboundExpression = fhint;
                        column.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;

                        //dsData.Tables[0].Columns[tFieldName].DataType = System.Type.GetType("System.Decimal");
                        //dsData.Tables[0].Columns[tFieldName].Expression = fhint;
                    }

                }
                i++;
            }
        }

        private GridColumn tGrid_Columns_Create(DataRow Row, int i, Int16 ViewType, GridView tGridView, string TableIPCode)
        {
            tToolBox t = new tToolBox();
            tDevColumn col = new tDevColumn();

            Boolean tvisible = false;
            string tcolumn_type = string.Empty;
            string tfield_name = string.Empty;

            bool tsum = false;
            int tsort = 0;
            int tsummary = 0;
            int tformattype = 0;
            string tdisplayformat = string.Empty;
            string tSumdisplayformat = string.Empty;

            tvisible = t.Set(Row["CMP_VISIBLE"].ToString(), Row["LKP_FVISIBLE"].ToString(), true);

            if (tvisible)
            {
                tfield_name = t.Set(Row["LKP_FIELD_NAME"].ToString(), "", "");
                tcolumn_type = t.Set(Row["CMP_COLUMN_TYPE"].ToString(), Row["LKP_CMP_COLUMN_TYPE"].ToString(), "TextEdit");

                tsort = t.Set(Row["CMP_SORT_TYPE"].ToString(), Row["LKP_CMP_SORT_TYPE"].ToString(), (int)0);
                tsummary = t.Set(Row["CMP_SUMMARY_TYPE"].ToString(), Row["LKP_CMP_SUMMARY_TYPE"].ToString(), (int)0);
                tformattype = t.Set(Row["CMP_FORMAT_TYPE"].ToString(), Row["LKP_CMP_FORMAT_TYPE"].ToString(), (int)0);
                tdisplayformat = t.Set(Row["CMP_DISPLAY_FORMAT"].ToString(), Row["LKP_CMP_DISPLAY_FORMAT"].ToString(), "");

                if (t.IsNotNull(tdisplayformat) == false) 
                    tdisplayformat = "n2";

                GridColumn column = new GridColumn();
                column.Name = "Column_" + tfield_name;
                column.FieldName = tfield_name;
                column.Caption = t.Set(Row["FCAPTION"].ToString(), Row["LKP_FCAPTION"].ToString(), tfield_name);
                column.Visible = tvisible;
                column.MinWidth = 10;
                column.Width = t.Set(Row["CMP_WIDTH"].ToString(), Row["LKP_CMP_WIDTH"].ToString(), 100);
                column.OptionsColumn.AllowEdit = t.Set(Row["CMP_ENABLED"].ToString(), Row["LKP_FENABLED"].ToString(), true);

                if (tfield_name == "LKP_LISTEYE_EKLE")
                    column.MinWidth = t.Set(Row["CMP_WIDTH"].ToString(), Row["LKP_CMP_WIDTH"].ToString(), 100);

                if (column.Width < 1)
                    column.Width = t.Set(Row["LKP_CMP_WIDTH"].ToString(), "100", (Int16)100);

                if (ViewType == v.obj_vw_GridView)
                {
                    column.GroupIndex = -1;
                    column.VisibleIndex = i;
                }

                #region summary add
                if (tsummary > 0)
                {
                    if (tGridView != null)
                    {
                        tsum = true;

                        if (tsummary == 1) tSumdisplayformat = "Avr {0}";
                        if (tsummary == 2) tSumdisplayformat = "Adet {0:n0}";
                        if (tsummary == 3) tSumdisplayformat = "{0}";
                        if (tsummary == 4) tSumdisplayformat = "Min {0}";
                        if (tsummary == 5) tSumdisplayformat = "Max {0}";
                        if (tsummary == 6) tSumdisplayformat = "{0:n0}";

                        var SummaryType = DevExpress.Data.SummaryItemType.None;

                        if (tsummary == 1) SummaryType = DevExpress.Data.SummaryItemType.Average;
                        if (tsummary == 2) SummaryType = DevExpress.Data.SummaryItemType.Count;
                        if (tsummary == 3) SummaryType = DevExpress.Data.SummaryItemType.Custom;
                        if (tsummary == 4) SummaryType = DevExpress.Data.SummaryItemType.Max;
                        if (tsummary == 5) SummaryType = DevExpress.Data.SummaryItemType.Min;
                        if (tsummary == 6) SummaryType = DevExpress.Data.SummaryItemType.Sum;

                        GridColumnSummaryItem item = new GridColumnSummaryItem(SummaryType, tfield_name, tSumdisplayformat);
                        column.Summary.Add(item);

                        // Create and setup the second summary item.
                        GridGroupSummaryItem item1 = new GridGroupSummaryItem();
                        item1.SummaryType = SummaryType;
                        item1.FieldName = tfield_name;
                        item1.DisplayFormat = tSumdisplayformat;
                        item1.ShowInGroupColumnFooter = column;//tGridView.Columns[tfield_name];
                                                               //item1.DisplayFormat.

                        tGridView.GroupSummary.Add(item1);

                        if (tsummary == 2)
                        {
                            column.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
                            column.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                        }
                        else
                        {
                            column.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
                            column.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                        }

                        column.DisplayFormat.FormatString = tdisplayformat;


                    }
                }
                #endregion summary add


                /*
            GridColumn summaryColumn = gridView1.Columns["Freight"];
           if(summaryColumn.GroupIndex < 0 && gridView1.GroupCount > 0) {
              GridColumn firstGroupingColumn = gridView1.SortInfo[0].Column;
              gridView1.GroupSummarySortInfo.Add(summaryItemMaxFreight, 
                ColumnSortOrder.Ascending, firstGroupingColumn);
                    */

                col.Grid_ColumnEdit(Row, column, null, tcolumn_type, TableIPCode);


                if (tsum)
                {
                    tGridView.OptionsBehavior.AutoUpdateTotalSummary = true;
                    tGridView.OptionsView.ShowFooter = true;
                    tGridView.OptionsView.GroupFooterShowMode = GroupFooterShowMode.VisibleAlways;
                }
                return column;
            }
                       
            return null;
        }

        #endregion GridView Columns Create

        #region AdvBandedGridView Columns Create
        public void tAdvBandedGrid_Columns_Create(DataSet dsFields, AdvBandedGridView tGridView, string TableIPCode)
        //public void tAdvBandedGrid_Columns_Create(DataSet dsFields, BandedGridView tGridView, string TableIPCode)
        {
            Boolean tvisible = false;
            string tcolumn_type = string.Empty;
            string tfield_name = string.Empty;

            bool tsum = false;
            int tsort = 0;
            int tsummary = 0;
            int tformattype = 0;
            string tdisplayformat = string.Empty;
            string tSumdisplayformat = string.Empty;


            Int16 q = 0;

            int fgroup_no = 0;
            int fgroup_line_no = 0;
            bool groupVisible = false;

            tToolBox t = new tToolBox();
            tDevColumn col = new tDevColumn();

            //foreach (DataRow Row in dsFields.Tables[0].Rows)
            DataRow Row = null;
            int i2 = dsFields.Tables[0].Rows.Count - 1;
            for (int i = i2; i > -1; i--)
            {
                Row = dsFields.Tables[0].Rows[i];

                tvisible = t.Set(Row["CMP_VISIBLE"].ToString(), Row["LKP_FVISIBLE"].ToString(), true);
                fgroup_no = t.Set(Row["GROUP_NO"].ToString(), "", -1);
                groupVisible = IsGroupVisible(dsFields, fgroup_no);

                if (tvisible)
                {
                    tfield_name = t.Set(Row["LKP_FIELD_NAME"].ToString(), "", "");
                    tcolumn_type = t.Set(Row["CMP_COLUMN_TYPE"].ToString(), Row["LKP_CMP_COLUMN_TYPE"].ToString(), "TextEdit");

                    tsort = t.Set(Row["CMP_SORT_TYPE"].ToString(), Row["LKP_CMP_SORT_TYPE"].ToString(), (int)0);
                    tsummary = t.Set(Row["CMP_SUMMARY_TYPE"].ToString(), Row["LKP_CMP_SUMMARY_TYPE"].ToString(), (int)0);
                    tformattype = t.Set(Row["CMP_FORMAT_TYPE"].ToString(), Row["LKP_CMP_FORMAT_TYPE"].ToString(), (int)0);
                    tdisplayformat = t.Set(Row["CMP_DISPLAY_FORMAT"].ToString(), Row["LKP_CMP_DISPLAY_FORMAT"].ToString(), "");

                    if (t.IsNotNull(tdisplayformat) == false)
                        tdisplayformat = "n2";

                    
                    BandedGridColumn column = new BandedGridColumn();
                    column.Name = "Column_" + tfield_name;
                    column.FieldName = tfield_name;
                    column.Caption = t.Set(Row["FCAPTION"].ToString(), Row["LKP_FCAPTION"].ToString(), tfield_name);
                    column.Visible = tvisible;
                    column.OptionsColumn.AllowEdit = t.Set(Row["CMP_ENABLED"].ToString(), Row["LKP_FENABLED"].ToString(), true);
                    column.Width = t.Set(Row["CMP_WIDTH"].ToString(), Row["LKP_CMP_WIDTH"].ToString(), (int)100);
                    //column.BestFit();
                    column.MinWidth = t.Set(Row["CMP_WIDTH"].ToString(), Row["LKP_CMP_WIDTH"].ToString(), (int)100);

                    // DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn

                    //column                                        
                    col.Grid_ColumnEdit(Row, null, column, tcolumn_type, TableIPCode);

                    if (tcolumn_type == "PictureEdit")
                        tGridView.RowHeight = 50;

                    if (column.Width < 10)
                        column.Width = t.Set(Row["LKP_CMP_WIDTH"].ToString(), "100", (Int16)100);

                    fgroup_no = t.Set(Row["GROUP_NO"].ToString(), "", -1);
                    fgroup_line_no = t.Set(Row["GROUP_LINE_NO"].ToString(), "", -1);

                    if (fgroup_no == -1)
                        fgroup_no = t.Set(Row["LKP_GROUP_NO"].ToString(), "0", (Int16)0);
                    if (fgroup_line_no == -1)
                        fgroup_line_no = t.Set(Row["LKP_GROUP_LINE_NO"].ToString(), "0", (Int16)0); ;


                    column.GroupIndex = fgroup_no;
                    if (fgroup_line_no < 0)
                        column.VisibleIndex = fgroup_line_no * -1;
                    else column.VisibleIndex = fgroup_line_no;

                    #region summary add
                    if (tsummary > 0)
                    {
                        if (tGridView != null)
                        {
                            tsum = true;

                            if (t.IsNotNull(tSumdisplayformat) == false)
                            {
                                if (tsummary == 1) tSumdisplayformat = "Avr {0}";
                                if (tsummary == 2) tSumdisplayformat = "Adet {0:n0}";
                                if (tsummary == 3) tSumdisplayformat = "{0}";
                                if (tsummary == 4) tSumdisplayformat = "Min {0}";
                                if (tsummary == 5) tSumdisplayformat = "Max {0}";
                                if (tsummary == 6) tSumdisplayformat = "{0:n0}";
                            }

                            var SummaryType = DevExpress.Data.SummaryItemType.None;

                            if (tsummary == 1) SummaryType = DevExpress.Data.SummaryItemType.Average;
                            if (tsummary == 2) SummaryType = DevExpress.Data.SummaryItemType.Count;
                            if (tsummary == 3) SummaryType = DevExpress.Data.SummaryItemType.Custom;
                            if (tsummary == 4) SummaryType = DevExpress.Data.SummaryItemType.Max;
                            if (tsummary == 5) SummaryType = DevExpress.Data.SummaryItemType.Min;
                            if (tsummary == 6) SummaryType = DevExpress.Data.SummaryItemType.Sum;

                            GridColumnSummaryItem item = new GridColumnSummaryItem(SummaryType, tfield_name, tSumdisplayformat);
                            column.Summary.Add(item);

                            // Create and setup the second summary item.
                            GridGroupSummaryItem item1 = new GridGroupSummaryItem();
                            item1.SummaryType = SummaryType;
                            item1.FieldName = tfield_name;
                            item1.DisplayFormat = tSumdisplayformat;
                            item1.ShowInGroupColumnFooter = column;//tGridView.Columns[tfield_name];

                            tGridView.GroupSummary.Add(item1);

                            if (tsummary == 2)
                            {
                                column.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
                                column.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                            }
                            else
                            {
                                column.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
                                column.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                            }

                            column.DisplayFormat.FormatString = tdisplayformat;
                        }
                    }
                    #endregion summary add

                    if (tGridView != null)// && groupVisible)
                    {
                        if (tGridView.Bands.Count > fgroup_no)
                            tGridView.Bands[fgroup_no].Columns.Add(column);  //Insert(tGrpLineNo, column); //Add(column);
                    }
                    q++;
                }

                if (tsum)
                {
                    tGridView.OptionsBehavior.AutoUpdateTotalSummary = true;
                    tGridView.OptionsView.ShowFooter = true;
                    tGridView.OptionsView.GroupFooterShowMode = GroupFooterShowMode.VisibleAlways;

                }
            }
        }
        
        private bool IsGroupVisible(DataSet dsFields, int groupNo)
        {
            tToolBox t = new tToolBox();
            bool onay = true;

            if (t.IsNotNull(dsFields) == false) return onay;
            if (dsFields.Tables.Count == 1) return onay;

            int length = dsFields.Tables[1].Rows.Count;
            if (groupNo > length) return false;

            for (int i = 0; i < length; i++)
            {
                if (t.myInt32(dsFields.Tables[1].Rows[i]["FGROUPNO"].ToString()) == groupNo)
                {
                    onay = (t.myInt32(dsFields.Tables[1].Rows[i]["FVISIBLE"].ToString()) == 1);
                    break;
                }
            }
            return onay;
        }
        #endregion BandedGridView Columns Create

        #region TreeList Columns Create
        public void tTreeList_Columns_Create(DataSet dsFields, TreeList tTreeList, string TableIPCode)
        {
            Boolean tvisible = false;
            string tcolumn_type = string.Empty;
            string tfield_name = string.Empty;
            Int16 i = 0;

            tToolBox t = new tToolBox();
            tDevColumn col = new tDevColumn();

            foreach (DataRow Row in dsFields.Tables[0].Rows)
            {
                tvisible = t.Set(Row["CMP_VISIBLE"].ToString(), Row["LKP_FVISIBLE"].ToString(), true);

                if (tvisible)
                {
                    tfield_name = t.Set(Row["LKP_FIELD_NAME"].ToString(), "", "");
                    tcolumn_type = t.Set(Row["CMP_COLUMN_TYPE"].ToString(), Row["LKP_CMP_COLUMN_TYPE"].ToString(), "TextEdit");

                    DevExpress.XtraTreeList.Columns.TreeListColumn column =
                        new DevExpress.XtraTreeList.Columns.TreeListColumn();

                    column.Name = "Column_" + tfield_name;
                    column.FieldName = tfield_name;
                    column.Caption = t.Set(Row["FCAPTION"].ToString(), Row["LKP_FCAPTION"].ToString(), tfield_name);
                    //column.GroupIndex = t.Set(Row["GROUP_NO"].ToString(), "", -1);
                    column.Visible = tvisible;
                    column.VisibleIndex = i;
                    column.OptionsColumn.AllowEdit = t.Set(Row["CMP_ENABLED"].ToString(), Row["LKP_FENABLED"].ToString(), true);
                    column.Width = t.Set(Row["CMP_WIDTH"].ToString(), Row["LKP_CMP_WIDTH"].ToString(), 100);

                    if (column.Width < 10)
                        column.Width = t.Set(Row["LKP_CMP_WIDTH"].ToString(), "100", (Int16)100);

                    col.Tree_ColumnEdit(Row, column, tcolumn_type, TableIPCode);

                    if (tTreeList != null)
                        tTreeList.Columns.AddRange(new DevExpress.XtraTreeList.Columns.TreeListColumn[] { column });

                    i++;
                }

            }
        }
        #endregion TreeList Columns Create

        #region VGridControl Columns Create
        public void tVGridControl_Columns_Create(DataSet dsFields, VGridControl tVGridControl, byte tview_type)
        {

            string tfield_name = string.Empty;
            string tcaption = string.Empty;
            string tTableLabel = string.Empty;
            string tfieldname = string.Empty;
            string tcolumn_type = string.Empty;

            Boolean tvisible = false;
            Int16 i = 0;
            Int16 move_item_type = 0;
            //Int16 move_type = 0;
            string move_item_name = string.Empty;

            int fgroup_no = 0;
            int tfieldtype = 0;

            tToolBox t = new tToolBox();
            tDevColumn dc = new tDevColumn();

            //CategoryRow row = new CategoryRow(tcaption);
            //EditorRow rowA1 = new EditorRow("bas_" + fname);


            foreach (DataRow Row in dsFields.Tables[0].Rows)
            {
                tvisible = t.Set(Row["CMP_VISIBLE"].ToString(), Row["LKP_FVISIBLE"].ToString(), true);

                if (tvisible)
                {
                    tfield_name = t.Set(Row["LKP_FIELD_NAME"].ToString(), "", "null");
                    tcaption = t.Set(Row["FCAPTION"].ToString(), Row["LKP_FCAPTION"].ToString(), tfield_name);
                    tfieldtype = t.Set(Row["LKP_FIELD_TYPE"].ToString(), "", (int)167);
                    tcolumn_type = t.Set(Row["CMP_COLUMN_TYPE"].ToString(), Row["LKP_CMP_COLUMN_TYPE"].ToString(), "TextEdit");
                    fgroup_no = t.Set(Row["GROUP_NO"].ToString(), "", -1);

                    move_item_type = t.Set(Row["MOVE_ITEM_TYPE"].ToString(), "", (Int16)0);
                    move_item_name = t.Set(Row["MOVE_ITEM_NAME"].ToString(), "", "");
                    //move_type = t.Set(Row["MOVE_TYPE"].ToString(), "", (Int16)0);
                    //move_location = t.Set(Row["MOVE_LOCATION"].ToString(), "", (Int16)0);
                    //move_layout_type = t.Set(Row["MOVE_LAYOUT_TYPE"].ToString(), "", (Int16)0);

                    DevExpress.XtraVerticalGrid.Rows.EditorRow column = 
                        new DevExpress.XtraVerticalGrid.Rows.EditorRow(tfield_name);

                    column.Name = "Column_" + tfield_name;
                    column.Properties.FieldName = tfield_name;
                    column.Properties.Caption = t.Set(Row["FCAPTION"].ToString(), Row["LKP_FCAPTION"].ToString(), tfield_name);
                    //column.GroupIndex = t.Set(Row["GROUP_NO"].ToString(), "", -1);
                    column.Visible = tvisible;
                    //column.Index = i;
                    column.Enabled = t.Set(Row["CMP_ENABLED"].ToString(), Row["LKP_FENABLED"].ToString(), true);
                    //column.Width = t.Set(Row["CMP_WIDTH"].ToString(), Row["LKP_CMP_WIDTH"].ToString(), 100);

                    if (fgroup_no == -1)
                        fgroup_no = t.Set(Row["LKP_GROUP_NO"].ToString(), "0", (Int16)0);

                    // category altına ekle
                    if ((fgroup_no > 0) && (move_item_type == 0))
                    {
                        string catName = "CategoryRow_" + fgroup_no.ToString();
                        BaseRow catRow = tVGridControl.Rows[catName]; // returns null if not found
                        if (catRow != null)
                        {
                            catRow.ChildRows.Add(column);
                        }
                        /*
                        try
                        {
                            if (tVGridControl.Rows.Count >= fgroup_no) 
                                tVGridControl.Rows["CategoryRow_" + fgroup_no.ToString()].ChildRows.Add(column);
                        }
                        catch (Exception ex)
                        {
                            //
                        }
                        */
                    }

                    // başka bir fieldin/column altına ekle
                    if ((move_item_type > 0) && (t.IsNotNull(move_item_name)))
                    {
                        try
                        {
                            //tVGridControl.Rows["Column_" + move_item_name].ChildRows.Add(column);
                            BaseRow oldCol = Find_VGridRow(tVGridControl, "Column_" + move_item_name);
                            //tVGridControl.Rows.GetRowByFieldName("Column_" + move_item_name);
                            if (oldCol != null)
                            {
                                ((EditorRow)oldCol).Expanded = false;
                                ((EditorRow)oldCol).ChildRows.Add(column);
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                    // root a ekle
                    if ((fgroup_no == 0) && (move_item_type == 0))
                    {
                        tVGridControl.Rows.Add(column);
                    }

                    dc.VGrid_ColumnEdit(Row, column, tcolumn_type, tview_type);

                    i++;
                }

            }
        }

        private BaseRow Find_VGridRow(VGridControl VGrid, string rowName)
        {
            BaseRow findRow = null;


            foreach (BaseRow rowitem in VGrid.Rows)
            {
                if (rowitem.Name.ToString() == rowName)
                {
                    return rowitem;
                }

                foreach (BaseRow row2item in rowitem.ChildRows)
                {
                    if (row2item.Name.ToString() == rowName)
                    {
                        return row2item;
                    }

                    foreach (BaseRow row3item in row2item.ChildRows)
                    {
                        if (row3item.Name.ToString() == rowName)
                        {
                            return row3item;
                        }
                    }
                }
            }

            return findRow;
        }

        #endregion VGridControl Columns Create

        #region tDataLayoutControl Columns_Create

        public void tDataLayoutControl_Columns_Create(DataSet dsFields, DataSet dsData,
            DataLayoutControl tDLayout,
            byte ViewType, string FormName, string External_TableIPCode, int groupNo)
        {
            tToolBox t = new tToolBox();

            string function_name = "tDataLayoutControl Columns_Create";
            string tfield_name = string.Empty;
            string tcaption = string.Empty;
            string thint = string.Empty;
            string tTableLabel = string.Empty;
            string tfieldname = string.Empty;
            string tcolumn_type = string.Empty;
            string tdisplayformat = string.Empty;
            string teditformat = string.Empty;
            Boolean tvisible = false;
            byte tview_type = 1;
            byte toperand_type = 0;

            //int tgroupNo = 0;

            int tGrpNo = 0;
            int tGrpLineNo = 0;

            int tfieldtype = 0;
            int width = 0;

            int fCol = 0;
            int fRow = 0;
            int fColSpan = 0;
            int fRowSpan = 0;

            DevExpress.XtraLayout.BaseLayoutItem bl_groupitem = null;

            // Tablo hakkındaki bilgiler
            DataRow row_Fields = dsFields.Tables[0].Rows[0];
            string SoftwareCode = row_Fields["SOFTWARE_CODE"].ToString();
            string ProjectCode = row_Fields["PROJECT_CODE"].ToString();
            string TableIPCode = row_Fields["TABLE_CODE"].ToString() + "." + row_Fields["IP_CODE"].ToString();
            if (t.IsNotNull(SoftwareCode))
                TableIPCode = SoftwareCode + "/" + ProjectCode + "/" + TableIPCode; 

            if (t.IsNotNull(External_TableIPCode))
                TableIPCode = External_TableIPCode;


            tDevColumn dc = new tDevColumn();

            foreach (DataRow Row in dsFields.Tables[0].Rows)
            {
                tvisible = t.Set(Row["CMP_VISIBLE"].ToString(), Row["LKP_FVISIBLE"].ToString(), true);

                //tgroupNo = t.Set(Row["GROUP_NO"].ToString(), Row["LKP_GROUP_NO"].ToString(), (int)0);

                tGrpNo = t.Set(Row["GROUP_NO"].ToString(), Row["LKP_GROUP_NO"].ToString(), (int)0);
                tGrpLineNo = t.Set(Row["GROUP_LINE_NO"].ToString(), Row["LKP_GROUP_LINE_NO"].ToString(), (int)0);

                if (tGrpNo == -1)
                    tGrpNo = t.Set(Row["LKP_GROUP_NO"].ToString(), "0", (Int16)0);
                if (tGrpLineNo == -1)
                    tGrpLineNo = t.Set(Row["LKP_GROUP_LINE_NO"].ToString(), "0", (Int16)0);


                if ((groupNo != -1) &&   // standart dataLayout isteği değil ise
                    (groupNo != tGrpNo)) // ve istenen group ile field group aynı değilse
                    tvisible = false;

                // SpeedKriter 
                if (ViewType == 2)
                {
                    toperand_type = t.Set(Row["KRT_OPERAND_TYPE"].ToString(), "", (byte)0);
                    if (toperand_type == 3)
                    {
                        // visible işaretlenmesede çalışsın
                        tvisible = true;
                    }
                    else tvisible = false;
                }


                //geçici düzenle
                toperand_type = t.Set(Row["KRT_OPERAND_TYPE"].ToString(), "", (byte)0);
                if (toperand_type > 0) tview_type = 2;
                //---


                #region 
                if (tvisible)
                {
                    tcaption = t.Set(Row["FCAPTION"].ToString(), Row["LKP_FCAPTION"].ToString(), tfield_name);
                    thint = t.Set(Row["FHINT"].ToString(), "", "");
                    tfield_name = t.Set(Row["LKP_FIELD_NAME"].ToString(), "", "null");
                    tfieldtype = t.Set(Row["LKP_FIELD_TYPE"].ToString(), "", (int)167);
                    tcolumn_type = t.Set(Row["CMP_COLUMN_TYPE"].ToString(), Row["LKP_CMP_COLUMN_TYPE"].ToString(), "TextEdit");
                    tdisplayformat = t.Set(Row["CMP_DISPLAY_FORMAT"].ToString(), Row["LKP_CMP_DISPLAY_FORMAT"].ToString(), "");
                    teditformat = t.Set(Row["CMP_EDIT_FORMAT"].ToString(), Row["LKP_CMP_EDIT_FORMAT"].ToString(), "");
                    width = t.Set(Row["CMP_WIDTH"].ToString(), Row["LKP_CMP_WIDTH"].ToString(), 100);

                    /// item in col ve row koordinatları ayarlanacak
                    /// 
                    fCol = t.myInt32(Row["CMP_LEFT"].ToString());
                    fRow = t.myInt32(Row["CMP_TOP"].ToString());
                    fColSpan = t.myInt32(Row["CMP_WIDTH"].ToString());
                    fRowSpan = t.myInt32(Row["CMP_HEIGHT"].ToString());

                    if (fColSpan == 100) fColSpan = 0;
                    if (fRowSpan == 100) fRowSpan = 0;

                    DevExpress.XtraLayout.LayoutControlItem column =
                           new DevExpress.XtraLayout.LayoutControlItem();

                    column.BeginInit();
                    column.Name = "Column_" + tfield_name;
                    column.ParentName = TableIPCode;
                    column.Text = t.Set(Row["FCAPTION"].ToString(), Row["LKP_FCAPTION"].ToString(), tfield_name);

                    // default top caption olsun
                    if (tcolumn_type == "PictureEdit")
                    {
                        if (t.IsNotNull(thint) == false)
                            thint = "TOPCAPTION";
                    }

                    if ((thint == "TOPCAPTION") || (thint == "CAPTIONTOP"))
                        column.TextLocation = DevExpress.Utils.Locations.Top;
                    if ((thint == "NOTCAPTION") || (thint == "CAPTIONNOT"))
                        column.TextVisible = false;

                    //column.FillControlToClientArea = false;
                    //column.Control.MaximumSize = new System.Drawing.Size(width+200, 20);
                    //column.ControlAlignment = System.Drawing.ContentAlignment.MiddleCenter;

                    // bu arttırma işlemei boşluk kolonları için yapılıyor
                    //if (fCol > 0)
                    //    fCol = fCol + fCol;
                    //if (fColSpan > 0)
                    //    fColSpan = fColSpan + fColSpan;
                    //---

                    column.OptionsTableLayoutItem.ColumnIndex = fCol * 2;
                    column.OptionsTableLayoutItem.RowIndex = fRow;
                    column.OptionsTableLayoutItem.ColumnSpan = (fColSpan * 2) - 1;
                    column.OptionsTableLayoutItem.RowSpan = fRowSpan;

                    try
                    {
                        dc.tXtraEditors_Edit(Row, dsData, tDLayout, column,
                                tcolumn_type, tdisplayformat, teditformat, tview_type, toperand_type, FormName);

                        if ((width > 50) && (width != 100))
                        {
                            column.SizeConstraintsType = SizeConstraintsType.Custom;
                            column.ControlMaxSize = new System.Drawing.Size(width, 0);
                            column.FillControlToClientArea = false;
                        }

                        column.EndInit();

                        #region GroupPanel içine add
                        if ((groupNo == -1) && // standart dataLayout isteği ise
                            (tGrpNo > 0)) // ve istenen group ile field group aynı değilse
                        {
                            bl_groupitem = Find_TabbedControlGroup(tDLayout, tGrpNo);

                            if (bl_groupitem != null)
                                ((DevExpress.XtraLayout.LayoutControlGroup)bl_groupitem).AddItem(column);
                        }

                        if (groupNo != -1) // standart dataLayout isteği ise
                        {
                            bl_groupitem = Find_TabbedControlGroup(tDLayout, groupNo);

                            if (bl_groupitem != null)
                                ((DevExpress.XtraLayout.LayoutControlGroup)bl_groupitem).AddItem(column);
                        }

                        #endregion

                        if (tGrpNo == 0)
                            tDLayout.Root.AddItem(column);

                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("DİKKAT : " + TableIPCode + " / " + tfield_name + " field de sorun var." + v.ENTER2 + e.Message.ToString(), function_name);
                    }

                }
                #endregion 
            }

            //tDLayout.BestFit();

        }

        #endregion tDataLayoutControl Columns_Create



        //--------------------

        #region GridView_Properties
        private void GridView_Properties(GridView tGridView, string g_properties)
        {
            string b1 = "";
            string b2 = "";
            string b3 = "";

            tToolBox t = new tToolBox();

            PropertyGridControl propertyGridControl1 = new PropertyGridControl();
            propertyGridControl1.SelectedObject = tGridView;

            BaseRow row = propertyGridControl1.GetFirst();

            while (g_properties.IndexOf("\r") > 0)
            {
                b1 = t.BeforeGet_And_AfterClear(ref g_properties, ">", false);
                b2 = t.BeforeGet_And_AfterClear(ref g_properties, "[", false);
                b3 = t.BeforeGet_And_AfterClear(ref g_properties, "]", false);

                Grid_Properties_Set(propertyGridControl1, row, b1, b2, b3);
            }

            propertyGridControl1.Dispose();
        }
        #endregion

        #region AdvBandedGridView_Properties
        private void AdvBandedGridView_Properties(AdvBandedGridView tGridView, string g_properties)
        {
            string b1 = "";
            string b2 = "";
            string b3 = "";

            tToolBox t = new tToolBox();

            PropertyGridControl propertyGridControl1 = new PropertyGridControl();
            propertyGridControl1.SelectedObject = tGridView;

            BaseRow row = propertyGridControl1.GetFirst();

            while (g_properties.IndexOf("\r") > 0)
            {
                b1 = t.BeforeGet_And_AfterClear(ref g_properties, ">", false);
                b2 = t.BeforeGet_And_AfterClear(ref g_properties, "[", false);
                b3 = t.BeforeGet_And_AfterClear(ref g_properties, "]", false);

                Grid_Properties_Set(propertyGridControl1, row, b1, b2, b3);
            }
        }
        #endregion

        //--------------------
        #region Grid_Properties_Set
        private void Grid_Properties_Set(PropertyGridControl t_propertyGridControl, BaseRow row, string t_CategoryName, string t_Caption, string t_Value)
        {
            tToolBox t = new tToolBox();

            t_CategoryName = t_CategoryName + " + ";

            byte i = 0;
            string category = "";
            string category1 = t.BeforeGet_And_AfterClear(ref t_CategoryName, "+", false);
            string category2 = t.BeforeGet_And_AfterClear(ref t_CategoryName, "+", false);
            string category3 = t.BeforeGet_And_AfterClear(ref t_CategoryName, "+", false);
            string category4 = t.BeforeGet_And_AfterClear(ref t_CategoryName, "+", false);

            category1 = category1.Trim();
            category2 = category2.Trim();
            category3 = category3.Trim();
            category4 = category4.Trim();
            t_Caption = t_Caption.Trim();

            while (row != null)
            {
                category = "";
                if (i == 0) category = category1;
                if (i == 1) category = category2;
                if (i == 2) category = category3;
                if (i == 3) category = category4;
                if (i == 4) category = "";

                if (row.Properties.Caption == category)
                {
                    if (row.Expanded == false) row.Expanded = true;
                    i++;
                }

                if (category == "")
                    if (row.Properties.Caption == t_Caption)
                    {
                        // string ifade insert edilirken hata veriyor sorun çözülemedi, uğraşılmadı  t_Value = "\'" + t_Value + "\'";
                        if (t_Caption != "ViewCaption")
                            t_propertyGridControl.SetCellValue(row, t_propertyGridControl.FocusedRecord, t_Value);

                        row = t_propertyGridControl.GetNext(row);
                        break;
                    }

                row = t_propertyGridControl.GetNext(row);
            }
        }
        #endregion Grid_Properties_Set

        #endregion SubFunctions for Grid


    }
}
