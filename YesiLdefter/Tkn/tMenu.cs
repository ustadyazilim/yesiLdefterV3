using DevExpress.Utils;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Navigation;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using DevExpress.XtraNavBar;
using DevExpress.XtraToolbox;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Tkn_Events;
using Tkn_InputPanel;
using Tkn_Report;
using Tkn_TablesRead;
using Tkn_ToolBox;
using Tkn_Variable;
using Newtonsoft.Json;
using YesiLdefter;

namespace Tkn_Menu
{
    public class tMenu : tBase
    {
        #region Create Menu
        /*
        'MENU_ITEM',  
         201, '', 'Category'
         202, '', 'Page/ToolBar'
         203, '', 'Group'
         204, '', 'SubMenu'
         205, '', 'Button'
         206, '', 'NavBarItem'
        */
        
        public void Create_Menu(Control menuControl, string MenuCode, string fieldName)
        {
            tToolBox t = new tToolBox();
            tTablesRead tr = new tTablesRead();

            DataSet ds_Items = new DataSet();

            //tr.MS_LayoutOrItems_Read(ds_Items, MenuCode, 3);
            tr.MS_Menu_Read(ds_Items, MenuCode);

            if (t.IsNotNull(ds_Items) == false) return;

            short ItemType = t.myInt16(ds_Items.Tables[0].Rows[0]["ITEM_TYPE"].ToString());
            string Prop_View = t.Set(ds_Items.Tables[0].Rows[0]["PROP_VIEWS"].ToString(), "", "");
            
            bool dontReport = false;
            bool dontEDI = false;
            bool dontExit = false;
            string reportTableIPCode = "";
            string reportFormCode = "";

            PROP_VIEWS_ITEMS JSON_PropView = null;

            if (t.IsNotNull(Prop_View))
            {
                JSON_PropView = t.readProp<PROP_VIEWS_ITEMS>(Prop_View);

                if (JSON_PropView.ALLMENU != null)
                {
                    if (JSON_PropView.ALLMENU.DONTEDI != null)
                        if (t.IsNotNull(JSON_PropView.ALLMENU.DONTEDI))
                            dontEDI = Convert.ToBoolean(JSON_PropView.ALLMENU.DONTEDI);
                    if (JSON_PropView.ALLMENU.DONTREPORT != null)
                        if (t.IsNotNull(JSON_PropView.ALLMENU.DONTREPORT))
                            dontReport = Convert.ToBoolean(JSON_PropView.ALLMENU.DONTREPORT);
                    if (JSON_PropView.ALLMENU.DONTEXIT != null)
                        if (t.IsNotNull(JSON_PropView.ALLMENU.DONTEXIT))
                            dontExit = Convert.ToBoolean(JSON_PropView.ALLMENU.DONTEXIT);
                    if (JSON_PropView.ALLMENU.RPRT_TABLEIPCODE != null)
                        if (t.IsNotNull(JSON_PropView.ALLMENU.RPRT_TABLEIPCODE))
                            reportTableIPCode = JSON_PropView.ALLMENU.RPRT_TABLEIPCODE;
                    if (JSON_PropView.ALLMENU.RPRT_FORMCODE != null)
                        if (t.IsNotNull(JSON_PropView.ALLMENU.RPRT_FORMCODE))
                            reportFormCode = JSON_PropView.ALLMENU.RPRT_FORMCODE;
                }
            }
            //if (ItemType == 101) Create_BarManager((DevExpress.XtraBars.BarManager)menuControl, ds_Items);
            if (ItemType == 102) Create_Ribbon((DevExpress.XtraBars.Ribbon.RibbonControl)menuControl, ds_Items, MenuCode);
            if (ItemType == 103) Create_NavBar((DevExpress.XtraNavBar.NavBarControl)menuControl, ds_Items);
            if (ItemType == 104) 
                Create_TileBar((DevExpress.XtraBars.Navigation.TileBar)menuControl, ds_Items);
            if (ItemType == 105) 
                Create_TileControl((DevExpress.XtraEditors.TileControl)menuControl, ds_Items);

            //--- Genelde Kullanılan Menü Tipi

            // 1. NOTE(@Janberk): ItemType 106 = TileNavPane
            // this is the most common menu type for dashboard tiles.
            // Create_TileNavPane() builds the colored tile groups seen on the start screen
            // (Dönem İşlemleri, e-Sınav İşlemleri, etc.).
            if (ItemType == 106) Create_TileNavPane((DevExpress.XtraBars.Navigation.TileNavPane)menuControl, ds_Items, fieldName, dontReport, dontEDI, dontExit, reportTableIPCode);
            if (ItemType == 107) Create_NavigationPane((DevExpress.XtraBars.Navigation.NavigationPane)menuControl, ds_Items);
            if (ItemType == 108) Create_AccordionControl((DevExpress.XtraBars.Navigation.AccordionControl)menuControl, ds_Items);
            if (ItemType == 109) Create_ToolBoxControl((DevExpress.XtraToolbox.ToolboxControl)menuControl, ds_Items);

            //if (ItemType == 110) Create_PopupMenu((DevExpress.XtraBars.PopupMenu)menuControl, ds_Items);

            /* 'MENU_TYPE'
            101, '', 'BarManager'
            102, '', 'RibbonControl'
            103, '', 'NavBarControl'
            104, '', 'TileBar'
            105, '', 'TileControl'
            106, '', 'TileNavPane'
            107, '', 'NavigationPane'
            108, '', 'AccordionControl'
            109, '', 'ToolBoxControl'  
            110, '', 'AccordionDinamik'             
            111, '', 'PopupMenu' 
            */
        }

        // 2. NOTE(@Janberk): Create_Menu_IN_Control() is the entry point for building menus from metadata.
        // MenuCode (same as TABLEIPCODE from layout) => definition to load from MS_ITEMS table.
        // ItemType determines which DevExpress control to create 
        // (102=Ribbon, 103=NavBar, 106=TileNavPane, 108=Accordion, etc.).
        // Colors and captions come from ds_Items columns: 
        // CMP_BACKCOLOR, MENU_COLOR, CAPTION, etc.
        public void Create_Menu_IN_Control(Control mainControl, string MenuCode, string ExtraValue)
        {
            try
            {
                tToolBox t = new tToolBox();
                tEvents ev = new tEvents();
                tEventsMenu evm = new tEventsMenu();
                tTablesRead tr = new tTablesRead();
                DataSet ds_Items = new DataSet();

            //tr.MS_LayoutOrItems_Read(ds_Items, MenuCode, 3);
            tr.MS_Menu_Read(ds_Items, MenuCode);

            if (t.IsNotNull(ds_Items) == false)
            {
                System.Diagnostics.Debug.WriteLine($"tMenu.Create_Menu_IN_Control: No menu data found for MenuCode={MenuCode}");
                return;
            }

            int RefId = t.myInt32(ds_Items.Tables[0].Rows[0]["REF_ID"].ToString());
            short ItemType = t.myInt16(ds_Items.Tables[0].Rows[0]["ITEM_TYPE"].ToString());
            string MasterCode = t.Set(ds_Items.Tables[0].Rows[0]["MASTER_CODE"].ToString(), "", "");
            string caption = t.Set(ds_Items.Tables[0].Rows[0]["CAPTION"].ToString(), "", "");
            string about = t.Set(ds_Items.Tables[0].Rows[0]["ABOUT"].ToString(), "", "");
            string Prop_View = t.Set(ds_Items.Tables[0].Rows[0]["PROP_VIEWS"].ToString(), "", "");

            int DockType = t.myInt16(ds_Items.Tables[0].Rows[0]["DOCK_TYPE"].ToString());
            if (DockType == 0) DockType = v.dock_Top;
            int width = t.myInt32(ds_Items.Tables[0].Rows[0]["CMP_WIDTH"].ToString());
            int height = t.myInt32(ds_Items.Tables[0].Rows[0]["CMP_HEIGHT"].ToString());

            string s1 = "=ROW_PROP_VIEWS:";
            string s2 = (char)34 + "NAVIGATIONPANE" + (char)34 + ": {";
            //"NAVIGATIONPANE": {
            /*
             "ALLMENU": {
                 "DONTREPORT": "TRUE",
                 "DONTEDI": "TRUE"
                 },            
            */
            bool dontReport = false;
            bool dontEDI = false;
            bool dontExit = false;
            string reportTableIPCode = "";
            string reportFormCode = "";

            PROP_VIEWS_ITEMS JSON_PropView = null;

            if (t.IsNotNull(Prop_View))
            {
                JSON_PropView = t.readProp<PROP_VIEWS_ITEMS>(Prop_View);

                if (JSON_PropView.ALLMENU != null)
                {
                    if (JSON_PropView.ALLMENU.DONTEDI != null)
                        if (t.IsNotNull(JSON_PropView.ALLMENU.DONTEDI))
                            dontEDI = Convert.ToBoolean(JSON_PropView.ALLMENU.DONTEDI);
                    if (JSON_PropView.ALLMENU.DONTREPORT != null)
                        if (t.IsNotNull(JSON_PropView.ALLMENU.DONTREPORT))
                            dontReport = Convert.ToBoolean(JSON_PropView.ALLMENU.DONTREPORT);
                    if (JSON_PropView.ALLMENU.DONTEXIT != null)
                        if (t.IsNotNull(JSON_PropView.ALLMENU.DONTEXIT))
                            dontExit = Convert.ToBoolean(JSON_PropView.ALLMENU.DONTEXIT);
                    if (JSON_PropView.ALLMENU.RPRT_TABLEIPCODE != null)
                        if (t.IsNotNull(JSON_PropView.ALLMENU.RPRT_TABLEIPCODE))
                            reportTableIPCode = JSON_PropView.ALLMENU.RPRT_TABLEIPCODE;
                    if (JSON_PropView.ALLMENU.RPRT_FORMCODE != null)
                        if (t.IsNotNull(JSON_PropView.ALLMENU.RPRT_FORMCODE))
                            reportFormCode = JSON_PropView.ALLMENU.RPRT_FORMCODE;
                }
            }


            #region // 102 - RibbonControl
            if (ItemType == 102)
            {
                DevExpress.XtraBars.Ribbon.RibbonControl menuControl =
                    new DevExpress.XtraBars.Ribbon.RibbonControl();

                ((System.ComponentModel.ISupportInitialize)(menuControl)).BeginInit();

                // 
                // ribbonControl1
                // 
                menuControl.ExpandCollapseItem.Id = 0;
                menuControl.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
                     menuControl.ExpandCollapseItem
                });
                menuControl.Location = new System.Drawing.Point(0, 0);
                menuControl.Name = "MENU_" + MasterCode; // RefId.ToString();
                menuControl.Text = "REF_ID[" + RefId.ToString() + "],CAPTION[" + caption + "]";
                menuControl.Size = new System.Drawing.Size(783, 141);
                menuControl.ToolbarLocation = DevExpress.XtraBars.Ribbon.RibbonQuickAccessToolbarLocation.Hidden;

                Create_Ribbon(menuControl, ds_Items, MenuCode);

                if (mainControl is Form)
                {
                    ((Form)mainControl).Controls.Add(menuControl);

                    if (DockType == v.dock_Bottom) menuControl.Dock = DockStyle.Bottom;
                    if (DockType == v.dock_Fill) menuControl.Dock = DockStyle.Fill;
                    if (DockType == v.dock_Left) menuControl.Dock = DockStyle.Left;
                    if (DockType == v.dock_None) menuControl.Dock = DockStyle.None;
                    if (DockType == v.dock_Right) menuControl.Dock = DockStyle.Right;
                    if (DockType == v.dock_Top) menuControl.Dock = DockStyle.Top;

                }

                //DevExpress.XtraBars.BarButtonItem iExit =
                //    new DevExpress.XtraBars.BarButtonItem();
                DevExpress.XtraBars.BarLargeButtonItem iExit =
                    new DevExpress.XtraBars.BarLargeButtonItem();

                // 
                // iExit
                // 
                iExit.Caption = "Çıkış";
                iExit.Description = ""; //Closes this program after prompting you to save unsaved data.
                iExit.Hint = "";
                iExit.Id = 99;
                iExit.ImageIndex = 6;
                iExit.LargeImageIndex = 6;
                iExit.Name = "item_FEXIT";
                iExit.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(evm.formRibbonMenu_ItemClick);

                iExit.Glyph = t.Find_Glyph("30_301_Close_16x16");
                iExit.LargeGlyph = t.Find_Glyph("30_301_Close_32x32");

                menuControl.Items.Add(iExit);

                DevExpress.XtraBars.Ribbon.RibbonPageGroup exitRibbonPageGroup =
                    new DevExpress.XtraBars.Ribbon.RibbonPageGroup();

                // 
                // exitRibbonPageGroup
                // 
                exitRibbonPageGroup.ItemLinks.Add(iExit);
                exitRibbonPageGroup.Name = "exitRibbonPageGroup";
                exitRibbonPageGroup.Text = "Exit";

                if (menuControl.Pages.Count > 0)
                    menuControl.Pages[0].Groups.Add(exitRibbonPageGroup);

                ((System.ComponentModel.ISupportInitialize)(menuControl)).EndInit();

                // ExtraValue = ActivePageName
                if (t.IsNotNull(ExtraValue))
                    t.RibbonPagesSet(menuControl, ExtraValue);

            }
            #endregion

            #region // 103 - NavBarControl
            if (ItemType == 103)
            {
                DevExpress.XtraNavBar.NavBarControl menuControl =
                    new DevExpress.XtraNavBar.NavBarControl();

                Create_NavBar(menuControl, ds_Items);

                mainControlAdd(mainControl, menuControl);
                /*
                if (mainControl.GetType().ToString() == "DevExpress.XtraBars.Navigation.NavigationPage")
                {
                    ((DevExpress.XtraBars.Navigation.NavigationPage)mainControl).Controls.Add(menuControl);
                }

                if (mainControl.GetType().ToString() == "DevExpress.XtraBars.Navigation.TabNavigationPage")
                {
                    ((DevExpress.XtraBars.Navigation.TabNavigationPage)mainControl).Controls.Add(menuControl);
                }

                if (mainControl.GetType().ToString() == "DevExpress.XtraTab.XtraTabPage")
                {
                    ((DevExpress.XtraTab.XtraTabPage)mainControl).Controls.Add(menuControl);
                }
                */
                string paintName = string.Empty;

                paintName = t.Find_Properties_Value(Prop_View, "NB_PAINTSTYLENAME");
                if (t.IsNotNull(paintName))
                    menuControl.PaintStyleName = paintName;

                menuControl.Dock = DockStyle.Fill;
                menuControl.OptionsNavPane.ShowOverflowPanel = false;
                menuControl.OptionsNavPane.NavPaneState = NavPaneState.Expanded;

                if (mainControl is Form)
                {
                    if (DockType == v.dock_Bottom) menuControl.Dock = DockStyle.Bottom;
                    if (DockType == v.dock_Fill) menuControl.Dock = DockStyle.Fill;
                    if (DockType == v.dock_Left) menuControl.Dock = DockStyle.Left;
                    if (DockType == v.dock_None) menuControl.Dock = DockStyle.None;
                    if (DockType == v.dock_Right) menuControl.Dock = DockStyle.Right;
                    if (DockType == v.dock_Top) menuControl.Dock = DockStyle.Top;

                    ((Form)mainControl).Controls.Add(menuControl);
                }

            }
            #endregion

            #region // 105 - TileControl
            if (ItemType == 105)
            {
                int groupCount = menuControl.Groups.Count;
                /*
                // 
                // tileControl1
                // 
                    this.tileControl1.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Default;
                    this.tileControl1.Dock = System.Windows.Forms.DockStyle.Fill;
                    this.tileControl1.Groups.Add(this.tileGroup1);
                    this.tileControl1.Location = new System.Drawing.Point(8, 8);
                    this.tileControl1.MaxId = 4;
                    this.tileControl1.Name = "tileControl1";
                    this.tileControl1.ShowGroupText = true;
                    this.tileControl1.ShowText = true;
                    this.tileControl1.Size = new System.Drawing.Size(784, 434);
                    this.tileControl1.TabIndex = 0;
                    this.tileControl1.Text = "tileControl1";
                    */
                    DevExpress.XtraEditors.TileControl menuControl =
                        new DevExpress.XtraEditors.TileControl();
                    // 
                    // tileControl1
                    //
                    menuControl.AllowSelectedItem = true;
                    menuControl.AllowSelectedItemBorder = true;
                    menuControl.AllowItemHover = true;
                    //menuControl.al
                    menuControl.AppearanceItem.Selected.BackColor = v.AppearanceFocusedColor;
                    menuControl.AppearanceItem.Selected.Options.UseBackColor = true;
                    menuControl.AppearanceItem.Selected.BorderColor = v.AppearanceTextColor;
                    menuControl.AppearanceItem.Selected.Options.UseBorderColor = true;
                    menuControl.AppearanceItem.Selected.ForeColor = v.AppearanceFocusedTextColor;
                    menuControl.AppearanceItem.Selected.Options.UseForeColor = true;

                    menuControl.AppearanceItem.Hovered.BackColor = v.AppearanceFocusedColor;
                    menuControl.AppearanceItem.Hovered.Options.UseBackColor = true;
                    menuControl.AppearanceItem.Hovered.ForeColor = v.AppearanceFocusedTextColor;
                    menuControl.AppearanceItem.Hovered.Options.UseForeColor = true;

                    menuControl.AppearanceItem.Pressed.BackColor = v.AppearanceFocusedColor;
                    menuControl.AppearanceItem.Pressed.Options.UseBackColor = true;
                    menuControl.AppearanceItem.Pressed.ForeColor = v.AppearanceFocusedTextColor;
                    menuControl.AppearanceItem.Pressed.Options.UseForeColor = true;

                    //menuControl.
                    menuControl.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Default;
                    menuControl.Name = "MENU_" + MasterCode;
                    menuControl.ShowGroupText = true;
                    menuControl.ShowText = true;
                    menuControl.TabIndex = 0;
                    menuControl.Text = caption;
                    t.myControl_Size_And_Location(menuControl, width, height, 0, 0);

                    // forma şimdi ekleyelelim
                    //((Form)mainControl).Controls.Add(menuControl);
                    //((Form)mainControl).Padding = new System.Windows.Forms.Padding(8);

                    if (DockType == v.dock_Bottom) menuControl.Dock = DockStyle.Bottom;
                    if (DockType == v.dock_Fill) menuControl.Dock = DockStyle.Fill;
                    if (DockType == v.dock_Left) menuControl.Dock = DockStyle.Left;
                    if (DockType == v.dock_None) menuControl.Dock = DockStyle.None;
                    if (DockType == v.dock_Right) menuControl.Dock = DockStyle.Right;
                    if (DockType == v.dock_Top) menuControl.Dock = DockStyle.Top;

                    if (mainControl is Form)
                    {
                        ((Form)mainControl).Controls.Add(menuControl);
                    }
                    else
                    {
                        mainControl.Controls.Add(menuControl);
                    }

                    Create_TileControl(menuControl, ds_Items, MenuCode);
                    Create_TileControl(menuControl, ds_Items);
                }
                
            }
            #endregion TileControl

            #region // 106 - TileNavPane << --- Genelde Kullanılan Menü Tipi
            if (ItemType == 106)
            {
                // Traditional DevExpress rendering (used when SP_UseHtmlMenu is false or WebView2 fails)
                DevExpress.XtraBars.Navigation.TileNavPane menuControl = new DevExpress.XtraBars.Navigation.TileNavPane();

                menuControl.Name = "MENU_" + MasterCode; // RefId.ToString();
                menuControl.Text = "REF_ID[" + RefId.ToString() + "],CAPTION[" + caption + "]";
                menuControl.TabStop = false;
                menuControl.OptionsPrimaryDropDown.CloseOnOuterClick = DefaultBoolean.True;

                    if (mainControl is Form)
                    {
                        ((Form)mainControl).Controls.Add(menuControl);
                    }
                    else
                    {
                        mainControl.Controls.Add(menuControl);
                    }

                    if (DockType == v.dock_Bottom) menuControl.Dock = DockStyle.Bottom;
                    if (DockType == v.dock_Fill) menuControl.Dock = DockStyle.Fill;
                    if (DockType == v.dock_Left) menuControl.Dock = DockStyle.Left;
                    if (DockType == v.dock_None) menuControl.Dock = DockStyle.None;
                    if (DockType == v.dock_Right) menuControl.Dock = DockStyle.Right;
                    if (DockType == v.dock_Top) menuControl.Dock = DockStyle.Top;

                    // ExtraValue = "FIRM||" + TABLEIPCODE2 + "|ds|"
                    Create_TileNavPane(menuControl, ds_Items, ExtraValue, dontReport, dontEDI, dontExit, reportTableIPCode);
                }
            }
            #endregion

            #region // 108 - AccordionControl
            if (ItemType == 108)
            {
                DevExpress.XtraBars.Navigation.AccordionControl menuControl =
                    new DevExpress.XtraBars.Navigation.AccordionControl();

                menuControl.Name = "MENU_" + MenuCode;
                menuControl.ExpandElementMode = DevExpress.XtraBars.Navigation.ExpandElementMode.Multiple;

                Create_AccordionControl((DevExpress.XtraBars.Navigation.AccordionControl)menuControl, ds_Items);

                menuControl.ScrollBarMode = DevExpress.XtraBars.Navigation.ScrollBarMode.Auto;

                if (t.IsNotNull(Prop_View))
                {
                    string myAccFilter = string.Empty;
                    string myAccExpanded = string.Empty;

                    // old silinecek
                    if (Prop_View.IndexOf(s1) > -1)
                    {
                        myAccFilter = t.MyProperties_Get(Prop_View, "ACCFILTER:");
                        myAccExpanded = t.MyProperties_Get(Prop_View, "ACCEXPANDED:");
                    }

                    if (Prop_View.IndexOf(s2) > -1)
                    {
                        myAccFilter = t.Find_Properties_Value(Prop_View, "ACCFILTER");
                        myAccExpanded = t.Find_Properties_Value(Prop_View, "ACCEXPANDED");
                    }

                    if (myAccFilter == "TRUE")
                        menuControl.ShowFilterControl = DevExpress.XtraBars.Navigation.ShowFilterControl.Always;

                    if (myAccExpanded == "TRUE")
                        menuControl.ExpandAll();
                    else menuControl.CollapseAll();

                    //

                }

                mainControlAdd(mainControl, menuControl);

                /*
                if (mainControl.GetType().ToString() == "DevExpress.XtraBars.Navigation.NavigationPage")
                {
                    menuControl.Dock = DockStyle.Fill;

                    ((DevExpress.XtraBars.Navigation.NavigationPage)mainControl).Controls.Add(menuControl);
                }

                if (mainControl.GetType().ToString() == "DevExpress.XtraBars.Navigation.TabNavigationPage")
                {
                    menuControl.Dock = DockStyle.Fill;

                    ((DevExpress.XtraBars.Navigation.TabNavigationPage)mainControl).Controls.Add(menuControl);
                }

                if (mainControl.GetType().ToString() == "DevExpress.XtraTab.XtraTabPage")
                {
                    menuControl.Dock = DockStyle.Fill;

                    ((DevExpress.XtraTab.XtraTabPage)mainControl).Controls.Add(menuControl);
                }
                */

                if (mainControl is Form)
                {
                    ((Form)mainControl).Controls.Add(menuControl);

                    if (DockType == v.dock_Bottom) menuControl.Dock = DockStyle.Bottom;
                    if (DockType == v.dock_Fill) menuControl.Dock = DockStyle.Fill;
                    if (DockType == v.dock_Left) menuControl.Dock = DockStyle.Left;
                    if (DockType == v.dock_None) menuControl.Dock = DockStyle.None;
                    if (DockType == v.dock_Right) menuControl.Dock = DockStyle.Right;
                    if (DockType == v.dock_Top) menuControl.Dock = DockStyle.Top;

                    //menuControl.Size = new System.Drawing.Size(300, 500);
                    t.myControl_Size_And_Location(menuControl, width, height, 0, 0);

                    menuControl.BringToFront();

                    //deneme
                    //if (mainControl is Form)
                    //{
                    if (t.IsNotNull(v.con_Source_ParentControl_Tag_Value))
                        ((Form)mainControl).AccessibleName = "con_Source_ParentControl_Tag_Value||" + v.con_Source_ParentControl_Tag_Value;
                    //}
                    //---
                }

                if (mainControl.ToString().IndexOf("System.Windows.Forms.TableLayoutPanel") > -1)
                {
                    menuControl.Dock = DockStyle.Fill;

                    int colNo = 0;
                    int rowNo = 0;
                    int colSpan = 0;
                    int rowSpan = 0;

                    string Prop_Views = v.con_Menu_Prop_Value;

                    if (t.IsNotNull(Prop_Views))
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
                        ((System.Windows.Forms.TableLayoutPanel)mainControl).Controls.Add(menuControl, colNo, rowNo);
                    }
                    else
                        MessageBox.Show("Dikkat : " + menuControl.Name.ToString() + " için Parent ( Col No / Row No ) seçimi yapılmamış.");

                    if (colSpan > 0)
                        ((System.Windows.Forms.TableLayoutPanel)mainControl).SetColumnSpan(menuControl, colSpan);
                    if (rowSpan > 0)
                        ((System.Windows.Forms.TableLayoutPanel)mainControl).SetRowSpan(menuControl, rowSpan);
                }


            }
            #endregion

            #region // 109 - ToolBoxControl
            if (ItemType == 109)
            {
                DevExpress.XtraToolbox.ToolboxControl menuControl =
                    new DevExpress.XtraToolbox.ToolboxControl();

                // 
                // toolboxControl1
                // 
                menuControl.Caption = caption;
                menuControl.Location = new System.Drawing.Point(0, 0);
                menuControl.Name = "MENU_" + MasterCode; // RefId.ToString();
                menuControl.Text = caption;

                menuControl.OptionsView.MenuButtonCaption = about;
                menuControl.OptionsView.ShowToolboxCaption = true;
                menuControl.OptionsBehavior.ItemSelectMode = DevExpress.XtraToolbox.ToolboxItemSelectMode.Single;
                menuControl.OptionsView.ColumnCount = 1;
                menuControl.OptionsView.MenuButtonCaption = "";
                menuControl.OptionsView.ShowMenuButton = false;
                menuControl.OptionsView.ShowSearchPanel = false;
                menuControl.OptionsView.ShowToolboxCaption = true;
                
                menuControl.Size = new System.Drawing.Size(300, 500);
                t.myControl_Size_And_Location(menuControl, width, height, 0, 0);

                //menuControl.ItemClick -= ev.tToolboxControl_ItemClick;
                menuControl.ItemClick += new DevExpress.XtraToolbox.ToolboxItemClickEventHandler(evm.tToolboxControl_ItemClick);

                if (mainControlAdd(mainControl, menuControl) == false)
                {
                    DevExpress.XtraEditors.PanelControl panelControl1 = new DevExpress.XtraEditors.PanelControl();

                    panelControl1.Controls.Add(menuControl);
                    menuControl.Dock = DockStyle.Fill;

                    if (mainControl is Form)
                    {
                        ((Form)mainControl).Controls.Add(panelControl1);

                        if (DockType == v.dock_Bottom) panelControl1.Dock = DockStyle.Bottom;
                        if (DockType == v.dock_Fill) panelControl1.Dock = DockStyle.Fill;
                        if (DockType == v.dock_Left) panelControl1.Dock = DockStyle.Left;
                        if (DockType == v.dock_None) panelControl1.Dock = DockStyle.None;
                        if (DockType == v.dock_Right) panelControl1.Dock = DockStyle.Right;
                        if (DockType == v.dock_Top) panelControl1.Dock = DockStyle.Top;
                    }
                }

                Create_ToolBoxControl(menuControl, ds_Items);

                // menude tasarında veya kullanıcı seçeneklerine bağlanacak
                if (menuControl.Groups.Count > 0)
                    menuControl.SelectedGroupIndex = 0;
            }
            #endregion

            #region // 110 - AccordionDinamik
            if (ItemType == 110)
            {
                DevExpress.XtraEditors.PanelControl
                   panelControl1 = new DevExpress.XtraEditors.PanelControl();

                DevExpress.XtraBars.Navigation.AccordionControl menuControl =
                    new DevExpress.XtraBars.Navigation.AccordionControl();

                // 
                // accordionControl1
                // 
                menuControl.Location = new System.Drawing.Point(0, 0);
                menuControl.Name = "MENU_" + MasterCode; // RefId.ToString();
                menuControl.Text = caption;
                menuControl.Size = new System.Drawing.Size(300, 500);
                t.myControl_Size_And_Location(menuControl, width, height, 0, 0);

                panelControl1.Controls.Add(menuControl);
                menuControl.Dock = DockStyle.Fill;

                if (mainControl is Form)
                {
                    ((Form)mainControl).AccessibleName = "con_Source_ParentControl_Tag_Value";

                    ((Form)mainControl).Controls.Add(panelControl1);

                    if (DockType == v.dock_Bottom) panelControl1.Dock = DockStyle.Bottom;
                    if (DockType == v.dock_Fill) panelControl1.Dock = DockStyle.Fill;
                    if (DockType == v.dock_Left) panelControl1.Dock = DockStyle.Left;
                    if (DockType == v.dock_None) panelControl1.Dock = DockStyle.None;
                    if (DockType == v.dock_Right) panelControl1.Dock = DockStyle.Right;
                    if (DockType == v.dock_Top) panelControl1.Dock = DockStyle.Top;
                }

                Create_AccordionDinamik(menuControl, ds_Items);

            }
            #endregion

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"tMenu.Create_Menu_IN_Control error: {ex.Message}");
                // Log error but don't throw to prevent application crash
            }
        }

        #region Create_BarManager
        public void Create_BarManager(DevExpress.XtraBars.BarManager mControl, DataSet ds_Items)
        {
            //DevExpress.XtraBars.BarManager
        }
        #endregion Create_BarManager

        #region Create_Ribbon
        public void Create_Ribbon(DevExpress.XtraBars.Ribbon.RibbonControl mControl, DataSet ds_Items, string MenuCode)
        {
            tToolBox t = new tToolBox();
            tEvents ev = new tEvents();
            tEventsMenu evm = new tEventsMenu();


            int itemCount = ds_Items.Tables[0].Rows.Count;
            int categoryCount = mControl.Categories.Count;
            int pageCount = mControl.Pages.Count;
            int itemType = 0;

            string lineNo = string.Empty;
            string refid = string.Empty;
            string itemName = string.Empty;
            string itemCaption = string.Empty;
            string Prop_Navigator = string.Empty;
            string cmpName = string.Empty;

            Int16 clickEvents = 0;

            int ustItemType = 0;
            string ustHesapRefId = string.Empty;
            string ustHesapItemName = string.Empty;
            string ustItemName = string.Empty;

            Boolean tenabled = false;
            Boolean tvisible = false;
            DataRow UstHesapRow = null;

            mControl.Enter += new System.EventHandler(evm.tRibbonControl_Enter);
            mControl.Leave += new System.EventHandler(evm.tRibbonControl_Leave);

            for (int i = 0; i < itemCount; i++)
            {
                refid = ds_Items.Tables[0].Rows[i]["REF_ID"].ToString();
                itemType = t.myInt32(ds_Items.Tables[0].Rows[i]["ITEM_TYPE"].ToString());
                itemName = t.Set(ds_Items.Tables[0].Rows[i]["ITEM_NAME"].ToString(), "", "");
                itemCaption = t.Set(ds_Items.Tables[0].Rows[i]["CAPTION"].ToString(), "", "");
                lineNo = t.Set(ds_Items.Tables[0].Rows[i]["LINE_NO"].ToString(), "", "");
                tenabled = t.Set(ds_Items.Tables[0].Rows[i]["CMP_ENABLED"].ToString(), "", true);
                tvisible = t.Set(ds_Items.Tables[0].Rows[i]["CMP_VISIBLE"].ToString(), "", true);
                clickEvents = t.myInt16(ds_Items.Tables[0].Rows[i]["CLICK_EVENTS"].ToString());
                Prop_Navigator = t.Set(ds_Items.Tables[0].Rows[i]["PROP_NAVIGATOR"].ToString(), "", "");
                cmpName = t.Set(ds_Items.Tables[0].Rows[i]["CMP_NAME"].ToString(), "", "");

                UstHesapRow = null;
                ustHesapRefId = string.Empty;
                ustHesapItemName = string.Empty;
                ustItemName = string.Empty;

                #region page
                // yeni page oluşturma
                if ((t.IsNotNull(itemName) == false) &&
                    (itemType == 202))
                {
                    // AccordionDinamik için kullandım
                    if ((lineNo != "0") && (lineNo != "")) refid = lineNo;
                    //---

                    if ((lineNo) == "0") lineNo = refid;

                    RibbonPage page = new RibbonPage();
                    page.Name = "item_" + refid;
                    page.Text = itemCaption;
                    page.Visible = tvisible;
                    //page.PageIndex = t.myInt32(lineNo);   set olmuyor
                    mControl.Pages.Add(page);
                }

                // mevcut page ye müdehale
                if ((t.IsNotNull(itemName) == true) &&
                    (itemType == 202))
                {
                    int i2 = mControl.Pages.Count;
                    for (int i1 = 0; i1 < i2; i1++)
                    {
                        if (mControl.Pages[i1].Name.ToString() == itemName)
                        {
                            if (t.IsNotNull(itemCaption))
                                mControl.Pages[i1].Text = itemCaption;
                            mControl.Pages[i1].Visible = tvisible;
                            break;
                        }
                    }
                }
                #endregion page                

                #region group
                // yeni PageGroup oluşturma
                if ((t.IsNotNull(itemName) == false) &&
                    (itemType == 203))
                {
                    RibbonPageGroup pGroup = new RibbonPageGroup();
                    pGroup.Name = "item_" + refid;
                    pGroup.Text = itemCaption;// + "/" + pGroup.Name.ToString();
                    pGroup.Enabled = tenabled;
                    pGroup.Visible = tvisible;

                    UstHesapRow = UstHesap_Get(ds_Items, i);

                    if (UstHesapRow != null)
                    {
                        ustItemType = t.myInt32(UstHesapRow["ITEM_TYPE"].ToString());
                        ustHesapRefId = UstHesapRow["REF_ID"].ToString();
                        ustHesapItemName = UstHesapRow["ITEM_NAME"].ToString();

                        //--- AccordionDinamik için
                        string ustLineNo = UstHesapRow["LINE_NO"].ToString();
                        if ((ustLineNo != "0") && (ustLineNo != ""))
                            ustHesapRefId = ustLineNo;
                        //---

                        if (t.IsNotNull(ustHesapItemName)) ustItemName = ustHesapItemName;
                        else ustItemName = "item_" + ustHesapRefId;

                        int i3 = mControl.Pages.Count;
                        for (int i2 = 0; i2 < i3; i2++)
                        {
                            if (mControl.Pages[i2].Name.ToString() == ustItemName)
                            {
                                mControl.Pages[i2].Groups.Add(pGroup);
                                break;
                            }
                        }
                    }

                }

                // mevcut PageGroup
                if ((t.IsNotNull(itemName) == true) &&
                    (itemType == 203))
                {
                    UstHesapRow = UstHesap_Get(ds_Items, i);

                    if (UstHesapRow != null)
                    {
                        ustItemType = t.myInt32(UstHesapRow["ITEM_TYPE"].ToString());
                        ustHesapRefId = UstHesapRow["REF_ID"].ToString();
                        ustHesapItemName = UstHesapRow["ITEM_NAME"].ToString();

                        if (t.IsNotNull(ustHesapItemName)) ustItemName = ustHesapItemName;
                        else ustItemName = "item_" + ustHesapRefId;

                        int i3 = mControl.Pages.Count;
                        for (int i2 = 0; i2 < i3; i2++)
                        {
                            if (mControl.Pages[i2].Name.ToString() == ustItemName)
                            {
                                int i5 = mControl.Pages[i2].Groups.Count;
                                for (int i4 = 0; i4 < i5; i4++)
                                {
                                    if (mControl.Pages[i2].Groups[i4].Name.ToString() == itemName)
                                    {
                                        if (t.IsNotNull(itemCaption))
                                            mControl.Pages[i2].Groups[i4].Text = itemCaption;
                                        mControl.Pages[i2].Groups[i4].Visible = tvisible;
                                        break;
                                    }
                                }

                            }
                        }
                    }
                }
                #endregion group

                #region barSubItem
                if ((t.IsNotNull(itemName) == false) &&
                    (itemType == 204))
                {
                    BarSubItem barSubItem = new DevExpress.XtraBars.BarSubItem();

                    barSubItem.Id = t.myInt32(refid);
                    barSubItem.Name = "item_" + refid;
                    barSubItem.Caption = itemCaption;// + "/" + barSubItem.Name.ToString();
                    barSubItem.Enabled = tenabled;
                    if (tvisible) barSubItem.Visibility = BarItemVisibility.Always;
                    else barSubItem.Visibility = BarItemVisibility.Never;

                    mControl.Items.Add(barSubItem);

                    UstHesapRow = UstHesap_Get(ds_Items, i);

                    if (UstHesapRow != null)
                    {
                        ustItemType = t.myInt32(UstHesapRow["ITEM_TYPE"].ToString());
                        ustHesapRefId = UstHesapRow["REF_ID"].ToString();
                        ustHesapItemName = UstHesapRow["ITEM_NAME"].ToString();

                        if (t.IsNotNull(ustHesapItemName)) ustItemName = ustHesapItemName;
                        else ustItemName = "item_" + ustHesapRefId;

                        int i5 = 0;
                        int i3 = mControl.Pages.Count;
                        for (int i2 = 0; i2 < i3; i2++)
                        {
                            i5 = mControl.Pages[i2].Groups.Count;

                            for (int i4 = 0; i4 < i5; i4++)
                            {
                                if (mControl.Pages[i2].Groups[i4].Name.ToString() == ustItemName)
                                {
                                    mControl.Pages[i2].Groups[i4].ItemLinks.Add(barSubItem);
                                    break;
                                }
                            }
                        }
                    }

                }
                #endregion barSubItem

                #region Button
                if (//(t.IsNotNull(itemName) == false) &&
                    ((itemType == 205) || (itemType == 206)))
                {
                    //DevExpress.XtraBars.BarButtonItem  barButtonItem = new DevExpress.XtraBars.BarButtonItem();
                    DevExpress.XtraBars.BarLargeButtonItem barButtonItem = new DevExpress.XtraBars.BarLargeButtonItem();

                    //if (itemName != "")
                    //    itemName = itemName;

                    if (itemName == "")
                        itemName = "item_" + refid;

                    if (clickEvents > 0)
                        Preparing_ButtonName(clickEvents, ref itemName);

                    barButtonItem.Id = t.myInt32(refid);
                    barButtonItem.Name = itemName;
                    barButtonItem.Caption = itemCaption;// + "/" + barButtonItem.Name.ToString();
                    barButtonItem.Enabled = tenabled;
                    barButtonItem.Tag = Prop_Navigator;
                    barButtonItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(evm.formRibbonMenu_ItemClick);

                    //UST/PMS/PMS/MsV3Menu
                    if (MenuCode == "UST/PMS/PMS/MsV3Menu" && cmpName != "")
                        anaMenuIcinOzelDurum(barButtonItem, cmpName);

                    // image çalışıyor SİLME 
                    // button Image set

                        #region Image set
                    if (!DBNull.Value.Equals(ds_Items.Tables[0].Rows[i]["LKP_GLYPH16"]))
                    {
                        byte[] img16 = (byte[])ds_Items.Tables[0].Rows[i]["LKP_GLYPH16"];
                        if (img16 != null)
                        {
                            MemoryStream ms = new MemoryStream(img16);
                            barButtonItem.Glyph = Image.FromStream(ms);
                            ms.Dispose();
                        }
                    }

                    if (!DBNull.Value.Equals(ds_Items.Tables[0].Rows[i]["LKP_GLYPH32"]))
                    {
                        byte[] img32 = (byte[])ds_Items.Tables[0].Rows[i]["LKP_GLYPH32"];
                        if (img32 != null)
                        {
                            MemoryStream ms = new MemoryStream(img32);
                            barButtonItem.LargeGlyph = Image.FromStream(ms);
                            ms.Dispose();
                        }
                    }
                    #endregion Image set

                    //barButtonItem.Glyph

                    #region 
                    /*
                    Image x = Image.FromStream(ms);
                    x.Width;
                    x.Height;
                    x.HorizontalResolution;
                    x.VerticalResolution;
                    x.PixelFormat;
                    */

                    /*      
                    declare @newdata varbinary(max) = 
                    (
                    SELECT BulkColumn 
                    FROM Openrowset( Bulk 'C:\\Preject_S\\DevExpress.Images\\Images\\Actions\\Apply_32x32.png', Single_Blob) as image
                    )

                    update MS_ITEMS set GLYPH =  @newdata 
                    from MS_ITEMS a where a.REF_ID = 8


                    set @newdata  = 
                    (
                    SELECT BulkColumn 
                    FROM Openrowset( Bulk 'C:\\Preject_S\\DevExpress.Images\\Images\\Actions\\Close_32x32.png', Single_Blob) as image
                    )

                    update MS_ITEMS set GLYPH =  @newdata 
                    from MS_ITEMS a where a.REF_ID = 9
                    */
                    #endregion

                    if (tvisible) barButtonItem.Visibility = BarItemVisibility.Always;
                    else barButtonItem.Visibility = BarItemVisibility.Never;

                    mControl.Items.Add(barButtonItem);

                    #region UstHesap
                    UstHesapRow = UstHesap_Get(ds_Items, i);
                    if (UstHesapRow != null)
                    {
                        ustItemType = t.myInt32(UstHesapRow["ITEM_TYPE"].ToString());
                        ustHesapRefId = UstHesapRow["REF_ID"].ToString();
                        ustHesapItemName = UstHesapRow["ITEM_NAME"].ToString();

                        if (t.IsNotNull(ustHesapItemName)) ustItemName = ustHesapItemName;
                        else ustItemName = "item_" + ustHesapRefId;

                        #region üst hesap Group ise
                        if (ustItemType == 203)
                        {
                            int i3 = mControl.Pages.Count;
                            int i5 = 0;
                            for (int i2 = 0; i2 < i3; i2++)
                            {
                                i5 = mControl.Pages[i2].Groups.Count;

                                for (int i4 = 0; i4 < i5; i4++)
                                {
                                    if (mControl.Pages[i2].Groups[i4].Name.ToString() == ustItemName)
                                    {
                                        mControl.Pages[i2].Groups[i4].ItemLinks.Add(barButtonItem);
                                        break;
                                    }
                                }
                            }
                        }
                        #endregion üst hesap Group ise

                        #region üst hesap SubMenu ise
                        if (ustItemType == 204)
                        {
                            int i3 = mControl.Items.Count;
                            for (int i2 = 0; i2 < i3; i2++)
                            {
                                if (mControl.Items[i2].Name == ustItemName)
                                {
                                    //((BarSubItem)mControl.Items[i2]).LinksPersistInfo.Add(new DevExpress.XtraBars.LinkPersistInfo(barButtonItem));

                                    //((BarSubItem)mControl.Items[i2]).LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
                                    //                new DevExpress.XtraBars.LinkPersistInfo(barButtonItem)});

                                    ((BarSubItem)mControl.Items[i2]).AddItem(barButtonItem);

                                    break;
                                }
                            }
                        }
                        #endregion üst hesap SubMenu ise
                    }
                    #endregion
                }
                #endregion Button
            } // for 


            mControl.ColorScheme = RibbonControlColorScheme.Green;


            //ribbonControl1.SelectedPage = 
        }
        
        private void anaMenuIcinOzelDurum(DevExpress.XtraBars.BarLargeButtonItem barButtonItem, string cmpName)
        {
            string serverName = "";
            if (cmpName == "ButtonMainServerName") serverName = v.active_DB.managerDBName;
            if (cmpName == "ButtonPublisServerName") serverName = v.publishManager_DB.databaseName;
            barButtonItem.Caption += v.ENTER + serverName;
        }

        public void alterRibbon(DevExpress.XtraBars.Ribbon.RibbonControl mControl, string proje)
        {
            tEvents ev = new tEvents();
            tReportDevEx  rapor = new tReportDevEx();

            DevExpress.XtraBars.Ribbon.RibbonPage ribbonPage1 = new DevExpress.XtraBars.Ribbon.RibbonPage();
            DevExpress.XtraBars.Ribbon.RibbonPage ribbonPage2 = new DevExpress.XtraBars.Ribbon.RibbonPage();
            DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroup1 = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            // skin grup
            DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroupStyles = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            DevExpress.XtraBars.SkinDropDownButtonItem skinDropDownButtonItem1 = new DevExpress.XtraBars.SkinDropDownButtonItem();
            DevExpress.XtraBars.SkinRibbonGalleryBarItem skinRibbonGalleryBarItem1 = new DevExpress.XtraBars.SkinRibbonGalleryBarItem();
            DevExpress.XtraBars.SkinPaletteRibbonGalleryBarItem skinPaletteRibbonGalleryBarItem1 = new DevExpress.XtraBars.SkinPaletteRibbonGalleryBarItem();
            // reportButton
            DevExpress.XtraBars.BarLargeButtonItem barReportDesignButtonItem = new DevExpress.XtraBars.BarLargeButtonItem();



            ((System.ComponentModel.ISupportInitialize)(mControl)).BeginInit();

            mControl.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
                   barReportDesignButtonItem,
                   skinDropDownButtonItem1,
                   skinRibbonGalleryBarItem1,
                   skinPaletteRibbonGalleryBarItem1
                   });

            if (proje == "yesiL")
            {
                mControl.Pages.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPage[] {
                ribbonPage1,
                ribbonPage2
                });
            }
            else
            {
                mControl.Pages.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPage[] {
                ribbonPage2
                });
            }
            // 
            // ribbonPage1
            // 
            ribbonPage1.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
            ribbonPageGroup1
            });
            ribbonPage1.Name = "ribbonPage1";
            ribbonPage1.Text = "YeşiLdefter";
            // 
            // ribbonPageGroup1
            // 
            ribbonPageGroup1.Name = "ribbonPageGroupGenel";
            ribbonPageGroup1.Text = "Genel İşlemler";
            // 
            // ribbonPage2
            // 
            ribbonPage2.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
            ribbonPageGroupStyles
            });
            ribbonPage2.Name = "ribbonPage2";
            ribbonPage2.Text = "Ayarlar";

            
            barReportDesignButtonItem.Id = 6;
            barReportDesignButtonItem.Name = "barReportDesignButtonItem";
            barReportDesignButtonItem.Caption = "Rapor Dizaynı";
            barReportDesignButtonItem.Enabled = true;
            //barReportDesignButtonItem.Tag = Prop_Navigator;
            barReportDesignButtonItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(rapor.reportDesigner_ItemClick);
            // 
            // skinDropDownButtonItem1
            // 
            skinDropDownButtonItem1.Id = 7;
            skinDropDownButtonItem1.Name = "skinDropDownButtonItem1";
            skinDropDownButtonItem1.DownChanged += new DevExpress.XtraBars.ItemClickEventHandler(ev.skinDropDownButtonItem_DownChanged);
            skinDropDownButtonItem1.HyperlinkClick += new DevExpress.Utils.HyperlinkClickEventHandler(ev.skinDropDownButtonItem_HyperlinkClick);
            skinDropDownButtonItem1.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(ev.skinDropDownButtonItem_ItemClick);
            // 
            // skinRibbonGalleryBarItem1
            // 
            skinRibbonGalleryBarItem1.Caption = "skinRibbonGalleryBarItem1";
            skinRibbonGalleryBarItem1.Id = 8;
            skinRibbonGalleryBarItem1.Name = "skinRibbonGalleryBarItem1";
            skinRibbonGalleryBarItem1.GalleryItemClick += new DevExpress.XtraBars.Ribbon.GalleryItemClickEventHandler(ev.skinRibbonGalleryBarItem_GalleryItemClick);
            skinRibbonGalleryBarItem1.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(ev.skinRibbonGalleryBarItem_ItemClick);
            // 
            // skinPaletteRibbonGalleryBarItem1
            // 
            skinPaletteRibbonGalleryBarItem1.Caption = "skinPaletteRibbonGalleryBarItem1";
            skinPaletteRibbonGalleryBarItem1.Id = 9;
            skinPaletteRibbonGalleryBarItem1.Name = "skinPaletteRibbonGalleryBarItem1";
            // 
            // ribbonPageGroupStyles
            //
            ribbonPageGroupStyles.ItemLinks.Add(barReportDesignButtonItem);
            ribbonPageGroupStyles.ItemLinks.Add(skinDropDownButtonItem1);
            ribbonPageGroupStyles.ItemLinks.Add(skinRibbonGalleryBarItem1);
            ribbonPageGroupStyles.ItemLinks.Add(skinPaletteRibbonGalleryBarItem1);
            ribbonPageGroupStyles.Name = "ribbonPageGroupStyles";
            ribbonPageGroupStyles.Text = "Temalar";

            ((System.ComponentModel.ISupportInitialize)(mControl)).EndInit();

        }

        #endregion Create_Ribbon

        #region Create_NavBarControl
        public void Create_NavBar(DevExpress.XtraNavBar.NavBarControl mControl, DataSet ds_Items)
        {
            tToolBox t = new tToolBox();
            tEvents ev = new tEvents();
            tEventsMenu evm = new tEventsMenu();

            //if (mControl == null)
            //    mControl = new DevExpress.XtraNavBar.NavBarControl();

            int itemCount = ds_Items.Tables[0].Rows.Count;
            int groupCount = mControl.Groups.Count;
            int itemsCount = mControl.Items.Count;

            int itemType = 0;

            string lineNo = string.Empty;
            string refid = string.Empty;
            string itemName = string.Empty;
            string itemCaption = string.Empty;
            string Prop_Navigator = string.Empty;

            Int16 clickEvents = 0;

            int ustItemType = 0;
            string ustHesapRefId = string.Empty;
            string ustHesapItemName = string.Empty;
            string ustItemName = string.Empty;

            Boolean tenabled = false;
            Boolean tvisible = false;
            DataRow UstHesapRow = null;

            for (int i = 0; i < itemCount; i++)
            {
                refid = ds_Items.Tables[0].Rows[i]["REF_ID"].ToString();
                itemType = t.myInt32(ds_Items.Tables[0].Rows[i]["ITEM_TYPE"].ToString());
                itemName = t.Set(ds_Items.Tables[0].Rows[i]["ITEM_NAME"].ToString(), "", "");
                itemCaption = t.Set(ds_Items.Tables[0].Rows[i]["CAPTION"].ToString(), "", "");
                lineNo = t.Set(ds_Items.Tables[0].Rows[i]["LINE_NO"].ToString(), "", "");
                tenabled = t.Set(ds_Items.Tables[0].Rows[i]["CMP_ENABLED"].ToString(), "", true);
                tvisible = t.Set(ds_Items.Tables[0].Rows[i]["CMP_VISIBLE"].ToString(), "", true);
                clickEvents = t.myInt16(ds_Items.Tables[0].Rows[i]["CLICK_EVENTS"].ToString());
                Prop_Navigator = t.Set(ds_Items.Tables[0].Rows[i]["PROP_NAVIGATOR"].ToString(), "", "");

                UstHesapRow = null;
                ustHesapRefId = string.Empty;
                ustHesapItemName = string.Empty;
                ustItemName = string.Empty;

                //this.navBarControl1 = new DevExpress.XtraNavBar.NavBarControl();
                //this.navBarGroup1 = new DevExpress.XtraNavBar.NavBarGroup();
                //this.navBarItem1 = new DevExpress.XtraNavBar.NavBarItem();

                #region group
                // yeni Group oluşturma
                if ((t.IsNotNull(itemName) == false) &&
                    (itemType == 203))
                {
                    DevExpress.XtraNavBar.NavBarGroup pGroup = new DevExpress.XtraNavBar.NavBarGroup();
                    pGroup.Name = "item_" + refid;
                    pGroup.Caption = itemCaption;
                    pGroup.Visible = tvisible;
                    pGroup.Expanded = true;

                    mControl.Groups.Add(pGroup);
                }

                // mevcut Group
                if ((t.IsNotNull(itemName) == true) &&
                    (itemType == 203))
                {
                    int i5 = mControl.Groups.Count;
                    for (int i4 = 0; i4 < i5; i4++)
                    {
                        if (mControl.Groups[i4].Name.ToString() == itemName)
                        {
                            if (t.IsNotNull(itemCaption))
                                mControl.Groups[i4].Caption = itemCaption;
                            mControl.Groups[i4].Visible = tvisible;
                            break;
                        }
                    }
                }
                #endregion group

                #region navBarItem
                if ((t.IsNotNull(itemName) == false) &&
                    ((itemType == 205) || (itemType == 206)))
                {
                    itemName = "item_" + refid;
                    if (clickEvents > 0)
                        Preparing_ButtonName(clickEvents, ref itemName);

                    NavBarItem tItem = new NavBarItem();
                    tItem.Name = itemName;
                    tItem.Caption = itemCaption;
                    tItem.Enabled = tenabled;
                    tItem.Visible = tvisible;
                    tItem.Tag = Prop_Navigator;
                    tItem.LinkClicked += new NavBarLinkEventHandler(evm.tNavBarItem_LinkClicked);

                    mControl.Items.Add(tItem);

                    UstHesapRow = UstHesap_Get(ds_Items, i);

                    if (UstHesapRow != null)
                    {
                        ustItemType = t.myInt32(UstHesapRow["ITEM_TYPE"].ToString());
                        ustHesapRefId = UstHesapRow["REF_ID"].ToString();
                        ustHesapItemName = UstHesapRow["ITEM_NAME"].ToString();

                        if (t.IsNotNull(ustHesapItemName)) ustItemName = ustHesapItemName;
                        else ustItemName = "item_" + ustHesapRefId;

                        int i3 = mControl.Groups.Count;
                        for (int i2 = 0; i2 < i3; i2++)
                        {
                            if (mControl.Groups[i2].Name.ToString() == ustItemName)
                            {
                                mControl.Groups[i2].ItemLinks.Add(tItem);
                                break;
                            }
                        }
                    }
                }
                #endregion navBarItem

            }

        }
        #endregion Create_NavBarControl

        #region Create_TileBar
        public void Create_TileBar(DevExpress.XtraBars.Navigation.TileBar mControl, DataSet ds_Items)
        {
            /*
             DevExpress.XtraEditors.TileItemElement tileItemElement1 = new DevExpress.XtraEditors.TileItemElement();
            DevExpress.XtraEditors.TileItemElement tileItemElement2 = new DevExpress.XtraEditors.TileItemElement();
            this.tileBar1 = new DevExpress.XtraBars.Navigation.TileBar();
            this.tileBarGroup2 = new DevExpress.XtraBars.Navigation.TileBarGroup();
            this.tileBarItem1 = new DevExpress.XtraBars.Navigation.TileBarItem();
            this.tileBarItem2 = new DevExpress.XtraBars.Navigation.TileBarItem();
            this.SuspendLayout();
            // 
            // tileBar1
            // 
            this.tileBar1.AllowDrag = false;
            this.tileBar1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tileBar1.DropDownOptions.BeakColor = System.Drawing.Color.Empty;
            this.tileBar1.Groups.Add(this.tileBarGroup2);
            this.tileBar1.Location = new System.Drawing.Point(0, 0);
            this.tileBar1.MaxId = 2;
            this.tileBar1.Name = "tileBar1";
            this.tileBar1.ScrollMode = DevExpress.XtraEditors.TileControlScrollMode.ScrollButtons;
            this.tileBar1.Size = new System.Drawing.Size(874, 97);
            this.tileBar1.TabIndex = 0;
            this.tileBar1.Text = "tileBar1";
            // 
            // tileBarGroup2
            // 
            this.tileBarGroup2.Items.Add(this.tileBarItem1);
            this.tileBarGroup2.Items.Add(this.tileBarItem2);
            this.tileBarGroup2.Name = "tileBarGroup2";
            // 
            // tileBarItem1
            // 
            this.tileBarItem1.DropDownOptions.BeakColor = System.Drawing.Color.Empty;
            tileItemElement1.Text = "tileBarItem1";
            this.tileBarItem1.Elements.Add(tileItemElement1);
            this.tileBarItem1.Id = 0;
            this.tileBarItem1.ItemSize = DevExpress.XtraBars.Navigation.TileBarItemSize.Wide;
            this.tileBarItem1.Name = "tileBarItem1";
            // 
            // tileBarItem2
            // 
            this.tileBarItem2.DropDownOptions.BeakColor = System.Drawing.Color.Empty;
            tileItemElement2.Text = "tileBarItem2";
            this.tileBarItem2.Elements.Add(tileItemElement2);
            this.tileBarItem2.Id = 1;
            this.tileBarItem2.ItemSize = DevExpress.XtraBars.Navigation.TileBarItemSize.Medium;
            this.tileBarItem2.Name = "tileBarItem2";
           

            */
        }
        #endregion Create_TileBar

        #region Create_TileControl
        public void Create_TileControl(DevExpress.XtraEditors.TileControl mControl, DataSet ds_Items)
        {
            tToolBox t = new tToolBox();
            tEvents ev = new tEvents();
            tEventsMenu evm = new tEventsMenu();

            //if (mControl == null)
            //    mControl = new DevExpress.XtraEditors.TileControl();

            int itemCount = ds_Items.Tables[0].Rows.Count;
            int groupCount = mControl.Groups.Count;
            int fontColor = 0;
            int backColor = 0;
            int itemType = 0;

            string lineNo = string.Empty;
            string refid = string.Empty;
            string itemName = string.Empty;
            string itemCaption = string.Empty;
            string shortcut_keys = string.Empty;
            string Prop_Navigator = string.Empty;
            string glName = string.Empty;

            Int16 clickEvents = 0;

            int ustItemType = 0;
            string ustHesapRefId = string.Empty;
            string ustHesapItemName = string.Empty;
            string ustItemName = string.Empty;

            Boolean tenabled = false;
            Boolean tvisible = false;
            DataRow UstHesapRow = null;

            string formName = string.Empty;
            Form tForm = mControl.FindForm();

            /// tekil formu oluşturmak için ticks kullanıyorum
            /// çünkü  DevExpress.XtraBars.Navigation.NavButton FindFormu yok
            /// kendim başka bir metodla forma ulaşıyorum
            /// bu nedenle tekil name oluşturmak gerekiyor
            ///
            ///
            string tt = DateTime.Now.Ticks.ToString();

            if (tForm != null)
            {
                if (tForm.Name.ToString().IndexOf(v.spFormName) == -1)
                    tForm.Name = tForm.Name + v.spFormName + tt;

                formName = tForm.Name.ToString();
            }

            for (int i = 0; i < itemCount; i++)
            {
                refid = ds_Items.Tables[0].Rows[i]["REF_ID"].ToString();
                itemType = t.myInt32(ds_Items.Tables[0].Rows[i]["ITEM_TYPE"].ToString());
                itemName = t.Set(ds_Items.Tables[0].Rows[i]["ITEM_NAME"].ToString(), "", "");
                itemCaption = t.Set(ds_Items.Tables[0].Rows[i]["CAPTION"].ToString(), "", "");
                shortcut_keys = t.Set(ds_Items.Tables[0].Rows[i]["SHORTCUT_KEYS"].ToString(), "", "");

                lineNo = t.Set(ds_Items.Tables[0].Rows[i]["LINE_NO"].ToString(), "", "");
                tenabled = t.Set(ds_Items.Tables[0].Rows[i]["CMP_ENABLED"].ToString(), "", true);
                tvisible = t.Set(ds_Items.Tables[0].Rows[i]["CMP_VISIBLE"].ToString(), "", true);
                clickEvents = t.myInt16(ds_Items.Tables[0].Rows[i]["CLICK_EVENTS"].ToString());
                Prop_Navigator = t.Set(ds_Items.Tables[0].Rows[i]["PROP_NAVIGATOR"].ToString(), "", "");

                fontColor = t.Set(ds_Items.Tables[0].Rows[i]["CMP_FONT_COLOR"].ToString(), "", (int)0);
                backColor = t.Set(ds_Items.Tables[0].Rows[i]["CMP_BACK_COLOR"].ToString(), "", (int)0);
                glName = t.Set(ds_Items.Tables[0].Rows[i]["GYLPH_32"].ToString(), "", "");
                
                UstHesapRow = null;
                ustHesapRefId = string.Empty;
                ustHesapItemName = string.Empty;
                ustItemName = string.Empty;

                //DevExpress.XtraEditors.TileItemElement tileItemElement16 = new DevExpress.XtraEditors.TileItemElement();
                //this.tileControl1 = new DevExpress.XtraEditors.TileControl();
                //this.tileGroup1 = new DevExpress.XtraEditors.TileGroup();
                //this.tileItem1 = new DevExpress.XtraEditors.TileItem();

                #region tileControl
                // tileControl ilk satır,  dsRead tablosu hazırlanıyor
                if (itemType == 105)
                {

                }
                #endregion tileControl

                #region group
                // yeni Group oluşturma
                if ((t.IsNotNull(itemName) == false) &&
                    (itemType == 203))
                {
                    DevExpress.XtraEditors.TileGroup pGroup = new DevExpress.XtraEditors.TileGroup();
                    pGroup.Name = "item_" + refid;
                    pGroup.Text = itemCaption;
                    pGroup.Visible = tvisible;
                    pGroup.Tag = formName;

                    mControl.Groups.Add(pGroup);
                }

                // mevcut Group
                if ((t.IsNotNull(itemName) == true) &&
                    (itemType == 203))
                {
                    int i5 = mControl.Groups.Count;
                    for (int i4 = 0; i4 < i5; i4++)
                    {
                        if (mControl.Groups[i4].Name.ToString() == itemName)
                        {
                            if (t.IsNotNull(itemCaption))
                                mControl.Groups[i4].Text = itemCaption;
                            mControl.Groups[i4].Visible = tvisible;
                            break;
                        }
                    }


                }
                #endregion group

                #region TileItem
                if ((t.IsNotNull(itemName) == false) &&
                    (tvisible) &&
                    ((itemType == 205) || (itemType == 206)))
                {
                    // 
                    // tileItem1
                    // 
                    //tileItemElement13.Text = "tileItem1";
                    //this.tileItem1.Elements.Add(tileItemElement13);
                    //this.tileItem1.Id = 0;
                    //this.tileItem1.ItemSize = DevExpress.XtraEditors.TileItemSize.Medium;
                    //this.tileItem1.Name = "tileItem1";

                    itemName = "item_" + refid;
                    if (clickEvents > 0)
                        Preparing_ButtonName(clickEvents, ref itemName);

                    DevExpress.XtraEditors.TileItemElement tileItemElement = new DevExpress.XtraEditors.TileItemElement();
                    tileItemElement.Text = itemCaption;
                    tileItemElement.TextAlignment = DevExpress.XtraEditors.TileItemContentAlignment.TopLeft;

                    DevExpress.XtraEditors.TileItem tItem = new DevExpress.XtraEditors.TileItem();
                    tItem.Elements.Add(tileItemElement);
                    tItem.Name = itemName;
                    tItem.Enabled = tenabled;
                    tItem.Visible = tvisible;
                    tItem.Tag = Prop_Navigator;
                    tItem.Id = i;
                    tItem.ItemSize = DevExpress.XtraEditors.TileItemSize.Default;
                    tItem.ItemClick += new DevExpress.XtraEditors.TileItemClickEventHandler(evm.tTileItem_ItemClick);
                        Color tileBackColor = backColor != 0 ? Color.FromArgb(backColor) : Tkn_ToolBox.tDevExpressTheme.BrandPrimary;
                        Color tileForeColor = fontColor != 0 ? Color.FromArgb(fontColor) : Tkn_ToolBox.tDevExpressTheme.BgSecondary;
                        Color tileHoverColor = Tkn_ToolBox.tDevExpressTheme.GetHoverColor(tileBackColor);
                        
                        tItem.Appearance.BackColor = tileBackColor;
                        tItem.Appearance.ForeColor = tileForeColor;
                        tItem.Appearance.Font = Tkn_ToolBox.tDevExpressTheme.ModernFont;
                        tItem.Appearance.Options.UseBackColor = true;
                        tItem.Appearance.Options.UseForeColor = true;
                        tItem.Appearance.Options.UseFont = true;
                        Tkn_ToolBox.tDevExpressTheme.ApplyModernTileItemAppearance(tItem, tileBackColor, tileHoverColor);
                    if (glName != "")
                    {

                        tileItemElement.ImageOptions.ImageAlignment = TileItemContentAlignment.BottomCenter;
                        //tileItemElement.ImageOptions.ImageToTextAlignment = TileControlImageToTextAlignment.Bottom;
                        tileItemElement.ImageOptions.Image = t.Find_Glyph(glName);
                        //tileItemElement.ImageOptions.I
                        //SvgImage = global::WindowsFormsApp1.Properties.Resources.bo_attention;
                    }


                    if (t.IsNotNull(shortcut_keys))
                    {
                        DevExpress.XtraEditors.TileItemElement tileItemElement2 = tileItemElement2 = new DevExpress.XtraEditors.TileItemElement();
                        tileItemElement2.Text = "<i> ( " + shortcut_keys + " )</i>";
                        tileItemElement2.TextAlignment = DevExpress.XtraEditors.TileItemContentAlignment.BottomRight;
                        tItem.Elements.Add(tileItemElement2);
                    }

                    UstHesapRow = UstHesap_Get(ds_Items, i);

                    if (UstHesapRow != null)
                    {
                        ustItemType = t.myInt32(UstHesapRow["ITEM_TYPE"].ToString());
                        ustHesapRefId = UstHesapRow["REF_ID"].ToString();
                        ustHesapItemName = UstHesapRow["ITEM_NAME"].ToString();

                        if (t.IsNotNull(ustHesapItemName)) ustItemName = ustHesapItemName;
                        else ustItemName = "item_" + ustHesapRefId;

                        int i3 = mControl.Groups.Count;
                        for (int i2 = 0; i2 < i3; i2++)
                        {
                            if (mControl.Groups[i2].Name.ToString() == ustItemName)
                            {
                                mControl.Groups[i2].Items.Add(tItem);
                                break;
                            }
                        }
                    }
                }
                #endregion TileItem

            }
        }
        #endregion Create_TileControl

        #region Create_TileNavPane    << --- Genelde Kullanılan Menü Tipi
        // 3. NOTE(@Janberk): Create_TileNavPane() builds the dashboard tile navigation pane
        // It reads menu items from ds_Items (populated from MS_ITEMS table) and creates:
        // - TileNavCategory (itemType 201): Top-level groups like "Dönem İşlemleri", "e-Sınav İşlemleri"
        // - TileNavItem (itemType 206): Individual tiles within each category
        // - NavButton (itemType 205): Action buttons
        // Colors are applied from metadata columns: 
        // CMP_BACKCOLOR, MENU_COLOR, TILE_COLOR (varies by item type).
        // Captions come from CAPTION column. 
        // Actions (form names) come from FORM_NAME or PROP_NAVIGATOR.
        //
        // TODO(@Janberk):
        // Tile card refactoring tasks for future development:
        // 1. Create DashboardTile model class (Key, Title, Group, BackColor, ForeColor, Font, Action)
        // 2. Create DashboardTheme.cs with centralized color/font constants (CSS-style theming)
        // 3. Extract color parsing logic (hex/ARGB) into ColorHelper.ParseColor(string colorStr) method
        // 4. Replace hardcoded colors (Red, Green, Coral, Purple) with metadata-driven values
        // 5. Replace v.colorNew/v.colorFocus with DashboardTheme constants
        // 6. Create DashboardTileBuilder class to encapsulate DevExpress control creation
        // 7. Populate DashboardTile models from ds_Items before creating DevExpress controls
        // 8. Add tile hover/click animations via DevExpress appearance properties
        // 9. Extract tile icon loading (LKP_GLYPH16, LKP_GLYPH32) into IconHelper class
        // 10. Add unit tests for tile creation logic (mock ds_Items DataSet)
        // 11. Consider caching tile definitions to avoid re-reading from database on every form load
        // 12. Add support for tile badges/notifications (e.g., unread count indicators)
        public void Create_TileNavPane(DevExpress.XtraBars.Navigation.TileNavPane mControl, 
            DataSet ds_Items, string fieldName, bool dontReport, bool dontEDI, bool dontExit, string reportTableIPCode)
        {
            tToolBox t = new tToolBox();
            tEvents ev = new tEvents();
            tEventsMenu evm = new tEventsMenu();

            int itemCount = ds_Items.Tables[0].Rows.Count;
            int itemType = 0;

            string lineNo = string.Empty;
            string refid = string.Empty;
            string itemName = string.Empty;
            string itemCaption = string.Empty;
            string shortcut_keys = string.Empty;
            string Prop_Navigator = string.Empty;

            Int16 clickEvents = 0;
            int DockType = 0;
            int ustItemType = 0;
            string ustHesapRefId = string.Empty;
            string ustHesapItemName = string.Empty;
            string ustItemName = string.Empty;
            string CmpName = string.Empty;
            string menuName = "";

            Boolean tenabled = false;
            Boolean tvisible = false;
            DataRow UstHesapRow = null;

            string formName = string.Empty;
            Form tForm = mControl.FindForm();

            TileNavCategory tFirstItemCategory = null;

            /// tekil formu oluşturmak için ticks kullanıyorum
            /// çünkü  DevExpress.XtraBars.Navigation.NavButton FindFormu yok
            /// kendim başka bir metodla forma ulaşıyorum
            /// bu nedenle tekil name oluşturmak gerekiyor
            ///
            ///
            string tt = DateTime.Now.Ticks.ToString();

            if (tForm != null)
            {
                if (tForm.Name.ToString().IndexOf(v.spFormName) == -1)
                    tForm.Name = tForm.Name + v.spFormName + tt;

                formName = tForm.Name.ToString();
            }

            mControl.Enter += new System.EventHandler(evm.tTileNavPane_Enter);
            mControl.Leave += new System.EventHandler(evm.tTileNavPane_Leave);
            mControl.SelectedElementChanged += new TileNavPaneElementEventHandler(evm.tileNavPane_SelectedElementChanged);
            menuName = mControl.Name?.ToString();

            // NOTE(@Janberk): Apply entrance screen layout from the @design-system.css file.
            // TODO(@Janberk): Create a global ApplyEntranceScreenLayout() method to apply the entrance screen layout to the TileNavPane.
            Tkn_ToolBox.tDevExpressTheme.ApplyEntranceScreenLayout(mControl);

            for (int i = 0; i < itemCount; i++)
            {
                refid = ds_Items.Tables[0].Rows[i]["REF_ID"].ToString();
                itemType = t.myInt32(ds_Items.Tables[0].Rows[i]["ITEM_TYPE"].ToString());
                itemName = t.Set(ds_Items.Tables[0].Rows[i]["ITEM_NAME"].ToString(), "", "");
                itemCaption = t.Set(ds_Items.Tables[0].Rows[i]["CAPTION"].ToString(), "", "");
                shortcut_keys = t.Set(ds_Items.Tables[0].Rows[i]["SHORTCUT_KEYS"].ToString(), "", "");

                lineNo = t.Set(ds_Items.Tables[0].Rows[i]["LINE_NO"].ToString(), "", "");
                tenabled = t.Set(ds_Items.Tables[0].Rows[i]["CMP_ENABLED"].ToString(), "", true);
                tvisible = t.Set(ds_Items.Tables[0].Rows[i]["CMP_VISIBLE"].ToString(), "", true);
                clickEvents = t.myInt16(ds_Items.Tables[0].Rows[i]["CLICK_EVENTS"].ToString());
                Prop_Navigator = t.Set(ds_Items.Tables[0].Rows[i]["PROP_NAVIGATOR"].ToString(), "", "");
                DockType = t.myInt16(ds_Items.Tables[0].Rows[i]["DOCK_TYPE"].ToString());
                CmpName = t.Set(ds_Items.Tables[0].Rows[i]["CMP_NAME"].ToString(), "", "");

                UstHesapRow = null;
                ustHesapRefId = string.Empty;
                ustHesapItemName = string.Empty;
                ustItemName = string.Empty;

                if (CmpName.ToUpper() == "DESIGN")
                {
                    CmpName = "";
                    if (v.tUser.UserDbTypeId > 30) // Eğer son kullanıcı ise bu butonu görmesin
                        tvisible = false;
                }

                if (tvisible)
                {
                    #region Category veya group
                    // yeni Category/Group oluşturma
                    if ((t.IsNotNull(itemName) == false) &&
                        ((itemType == 201) || (itemType == 203)))
                    {
                        TileNavCategory pGroup = new TileNavCategory();
                        pGroup.Name = "item_" + refid;
                        pGroup.Caption = itemCaption;
                        pGroup.TileText = itemCaption;
                        pGroup.Tile.Text = itemCaption;
                        pGroup.Visible = tvisible;

                        if ((DockType == v.dock_None) | (DockType == v.dock_Left))
                            pGroup.Alignment = NavButtonAlignment.Left;
                        if (DockType == v.dock_Right)
                            pGroup.Alignment = NavButtonAlignment.Right;

                        //if (clickEvents > 0)
                        pGroup.ElementClick += new DevExpress.XtraBars.Navigation.NavElementClickEventHandler(evm.tNavButton_ElementClick);
                        
                        // NOTE(@Janberk): Apply design system colors from the @design-system.css file.
                        // TODO(@Janberk): Create a global ApplyDesignSystemAppearance() method to apply the design system colors to the TileNavCategory.

                            string backColorStr = t.Set(ds_Items.Tables[0].Rows[i]["CMP_BACK_COLOR"]?.ToString(), "", "");
                            string menuColorStr = t.Set(ds_Items.Tables[0].Rows[i]["MENU_COLOR"]?.ToString(), "", "");

                            Color categoryBackColor = Tkn_ToolBox.tDevExpressTheme.ParseColorFromDatabase(backColorStr, Tkn_ToolBox.tDevExpressTheme.BrandPrimary);
                            Color categoryHoverColor = Tkn_ToolBox.tDevExpressTheme.ParseColorFromDatabase(menuColorStr, Tkn_ToolBox.tDevExpressTheme.BrandAccent);
                            
                            Tkn_ToolBox.tDevExpressTheme.ApplyModernCategoryAppearance(pGroup, categoryBackColor, categoryHoverColor);

                        #region Image set
                        if (!DBNull.Value.Equals(ds_Items.Tables[0].Rows[i]["LKP_GLYPH16"]))
                        {
                            byte[] img16 = (byte[])ds_Items.Tables[0].Rows[i]["LKP_GLYPH16"];
                            if (img16 != null)
                            {
                                MemoryStream ms = new MemoryStream(img16);
                                pGroup.Glyph = Image.FromStream(ms);
                                ms.Dispose();
                            }
                        }

                        if (!DBNull.Value.Equals(ds_Items.Tables[0].Rows[i]["LKP_GLYPH32"]))
                        {
                            byte[] img32 = (byte[])ds_Items.Tables[0].Rows[i]["LKP_GLYPH32"];
                            if (img32 != null)
                            {
                                MemoryStream ms = new MemoryStream(img32);
                                pGroup.Glyph = Image.FromStream(ms);
                                ms.Dispose();
                            }
                        }
                        #endregion Image set

                        // Not : Category nin altına sadece TileNavItem eklenebilmekte

                        //mControl.Categories.Add(pGroup);
                        if (itemType == 201)
                        {
                                string backColorStr = t.Set(ds_Items.Tables[0].Rows[i]["CMP_BACK_COLOR"]?.ToString(), "", "");
                                string menuColorStr = t.Set(ds_Items.Tables[0].Rows[i]["MENU_COLOR"]?.ToString(), "", "");
                                
                                Color categoryBackColor = Tkn_ToolBox.tDevExpressTheme.ParseColorFromDatabase(backColorStr, Tkn_ToolBox.tDevExpressTheme.BrandPrimary);
                                Color categoryHoverColor = Tkn_ToolBox.tDevExpressTheme.ParseColorFromDatabase(menuColorStr, Tkn_ToolBox.tDevExpressTheme.BrandAccent);
                                
                                // Apply modern appearance with rounded corners, shadows, and smooth transitions
                                Tkn_ToolBox.tDevExpressTheme.ApplyModernCategoryAppearance(pGroup, categoryBackColor, categoryHoverColor);

                            mControl.Categories.Add(pGroup);
                            if (tFirstItemCategory == null)
                                tFirstItemCategory = pGroup;

                        }
                        else mControl.Buttons.Add(pGroup);

                        //tileNavPane1.Categories.AddRange(new TileNavCategory[] { cat1, cat2 });


                    }
                    #endregion Category / group

                    #region Button 

                    // Button
                    if ((t.IsNotNull(itemName) == false) &&
                        (itemType == 205))
                    {
                        itemName = "item_" + refid;
                        if (clickEvents > 0)
                            Preparing_ButtonName(clickEvents, ref itemName);

                        NavButton tItem = new DevExpress.XtraBars.Navigation.NavButton();

                        tItem.Appearance.Name = formName;
                        tItem.Name = itemName;
                        if (t.IsNotNull(CmpName))
                        {
                            if (CmpName == "IsMain")
                            {
                                tItem.IsMain = true;
                            }
                            else tItem.Name = CmpName;
                        }
                        tItem.Caption = itemCaption;

                        // itemCaption = Belge Türü Seçin
                        if (t.IsNotNull(shortcut_keys))
                        {

                            if ((shortcut_keys == "SHORTCUT_BUTTON") |
                                (shortcut_keys == "SHORTCUT"))
                            {
                                itemName = "item_SHORTCUT_" + refid;

                                // event bağlansın
                                if (clickEvents == 0)
                                    clickEvents = 99;
                                else
                                    Preparing_ButtonName(clickEvents, ref itemName);

                                tItem.Name = itemName;
                            }
                            else
                                tItem.Caption = itemCaption + "   : " + shortcut_keys;

                            //tItem.Caption = itemCaption + "   <i>" + shortcut_keys + "</i>";
                            //tItem.  AllowHtmlText = DefaultBoolean.True;
                        }

                        tItem.Enabled = tenabled;
                        tItem.Visible = tvisible;
                        if (t.IsNotNull(Prop_Navigator))
                            tItem.Tag = Prop_Navigator + "|Prop_Navigator|";
                        if (t.IsNotNull(menuName))
                            tItem.Tag += menuName + "|MenuName|";


                        //tItem.
                        // NOTE(@Janberk): Modern styling for NavButton - apply sleek appearance
                        Tkn_ToolBox.tDevExpressTheme.ApplyModernButtonAppearance(tItem);


                        if ((DockType == v.dock_None) | (DockType == v.dock_Left))
                            tItem.Alignment = NavButtonAlignment.Left;
                        if (DockType == v.dock_Right)
                            tItem.Alignment = NavButtonAlignment.Right;

                        if (clickEvents > 0)
                        {
                            tItem.ElementClick += new DevExpress.XtraBars.Navigation.NavElementClickEventHandler(evm.tNavButton_ElementClick);
                        }

                        #region Image set
                        if (!DBNull.Value.Equals(ds_Items.Tables[0].Rows[i]["LKP_GLYPH16"]))
                        {
                            byte[] img16 = (byte[])ds_Items.Tables[0].Rows[i]["LKP_GLYPH16"];
                            if (img16 != null)
                            {
                                MemoryStream ms = new MemoryStream(img16);
                                tItem.Glyph = Image.FromStream(ms);
                                ms.Dispose();
                            }
                        }

                        if (!DBNull.Value.Equals(ds_Items.Tables[0].Rows[i]["LKP_GLYPH32"]))
                        {
                            byte[] img32 = (byte[])ds_Items.Tables[0].Rows[i]["LKP_GLYPH32"];
                            if (img32 != null)
                            {
                                MemoryStream ms = new MemoryStream(img32);
                                tItem.Glyph = Image.FromStream(ms);
                                ms.Dispose();
                            }
                        }
                        #endregion Image set

                        mControl.Buttons.Add(tItem);
                    }
                    #endregion Button

                    #region // TileNavItem
                    if ((t.IsNotNull(itemName) == false) &&
                        (itemType == 206))
                    {
                        // TileNavItem mın üst hesabı Category olmalı
                        // eğer TileNavItem mında alt hesaplarını oluşturacaksan 
                        itemName = "item_" + refid;
                        if (clickEvents > 0)
                            Preparing_ButtonName(clickEvents, ref itemName);

                        UstHesapRow = UstHesap_Get(ds_Items, i);

                        if (UstHesapRow != null)
                        {
                            ustItemType = t.myInt32(UstHesapRow["ITEM_TYPE"].ToString());
                            ustHesapRefId = UstHesapRow["REF_ID"].ToString();
                            ustHesapItemName = UstHesapRow["ITEM_NAME"].ToString();

                            if (t.IsNotNull(ustHesapItemName)) ustItemName = ustHesapItemName;
                            else ustItemName = "item_" + ustHesapRefId;

                            // Category or Group
                            if ((ustItemType == 201) || (ustItemType == 203))
                            {
                                TileNavItem tItem = new TileNavItem();

                                tItem.Appearance.Name = formName;
                                tItem.Name = itemName;
                                if (t.IsNotNull(CmpName))
                                    tItem.Name = CmpName;
                                tItem.Caption = itemCaption;
                                tItem.Tile.Text = itemCaption;
                                if (t.IsNotNull(shortcut_keys))
                                    tItem.Tile.Text = itemCaption + "   <i>" + shortcut_keys + "</i>";
                                tItem.Tile.AllowHtmlText = DefaultBoolean.True;

                                tItem.Enabled = tenabled;
                                tItem.Visible = tvisible;
                                //tItem.Tag = Prop_Navigator;
                                if (t.IsNotNull(Prop_Navigator))
                                    tItem.Tag = Prop_Navigator + "|Prop_Navigator|";

                                // NOTE(@Janberk): Modern styling - read colors from database metadata, apply sleek appearance
                                string tileBackColor = t.Set(ds_Items.Tables[0].Rows[i]["CMP_BACK_COLOR"]?.ToString(), "", "");
                                string tileMenuColor = t.Set(ds_Items.Tables[0].Rows[i]["MENU_COLOR"]?.ToString(), "", "");
                                
                                Color itemBackColor = Tkn_ToolBox.tDevExpressTheme.ParseColorFromDatabase(tileBackColor, Tkn_ToolBox.tDevExpressTheme.BrandPrimary);
                                Color itemHoverColor = Tkn_ToolBox.tDevExpressTheme.ParseColorFromDatabase(tileMenuColor, Tkn_ToolBox.tDevExpressTheme.BrandAccent);
                                
                                // Apply modern tile appearance with rounded corners, shadows, and smooth transitions
                                Tkn_ToolBox.tDevExpressTheme.ApplyModernTileAppearance(tItem, itemBackColor, itemHoverColor);

                                if (clickEvents > 0)
                                {
                                    tItem.ElementClick += new DevExpress.XtraBars.Navigation.NavElementClickEventHandler(evm.tNavButton_ElementClick);
                                }

                                #region Image set
                                if (!DBNull.Value.Equals(ds_Items.Tables[0].Rows[i]["LKP_GLYPH16"]))
                                {
                                    byte[] img16 = (byte[])ds_Items.Tables[0].Rows[i]["LKP_GLYPH16"];
                                    if (img16 != null)
                                    {
                                        MemoryStream ms = new MemoryStream(img16);
                                        tItem.Glyph = Image.FromStream(ms);
                                        ms.Dispose();
                                    }
                                }

                                if (!DBNull.Value.Equals(ds_Items.Tables[0].Rows[i]["LKP_GLYPH32"]))
                                {
                                    byte[] img32 = (byte[])ds_Items.Tables[0].Rows[i]["LKP_GLYPH32"];
                                    if (img32 != null)
                                    {
                                        MemoryStream ms = new MemoryStream(img32);
                                        tItem.Glyph = Image.FromStream(ms);
                                        ms.Dispose();
                                    }
                                }
                                #endregion Image set

                                if (ustItemType == 203)
                                {
                                    int i3 = mControl.Buttons.Count;
                                    for (int i2 = 0; i2 < i3; i2++)
                                    {
                                        if (mControl.Buttons[i2].Element.Name.ToString() == ustItemName)
                                        {
                                            ((TileNavCategory)mControl.Buttons[i2].Element).Items.Add(tItem);
                                            break;
                                        }
                                    }
                                }
                                if (ustItemType == 201)
                                {
                                    int i3 = mControl.Categories.Count;
                                    for (int i2 = 0; i2 < i3; i2++)
                                    {
                                        if (mControl.Categories[i2].Name.ToString() == ustItemName)
                                        {
                                            ((TileNavCategory)mControl.Categories[i2]).Items.Add(tItem);
                                            break;
                                        }
                                    }
                                }
                            }

                            // TileNavItem ise
                            if (ustItemType == 206)
                            {
                                TileNavSubItem tItem = new TileNavSubItem();

                                tItem.Appearance.Name = formName;
                                tItem.Name = itemName;
                                if (t.IsNotNull(CmpName))
                                    tItem.Name = CmpName;
                                tItem.Caption = itemCaption;
                                tItem.Tile.Text = itemCaption;
                                if (t.IsNotNull(shortcut_keys))
                                    tItem.Tile.Text = itemCaption + "   <i>" + shortcut_keys + "</i>";
                                tItem.Tile.AllowHtmlText = DefaultBoolean.True;

                                tItem.Enabled = tenabled;
                                tItem.Visible = tvisible;
                                if (t.IsNotNull(Prop_Navigator))
                                    tItem.Tag = Prop_Navigator + "|Prop_Navigator|";
                                if (t.IsNotNull(menuName))
                                    tItem.Tag += menuName + "|MenuName|";

                                // NOTE(@Janberk): Modern styling for TileNavSubItem - read colors from database, apply sleek appearance
                                string subItemBackColor = t.Set(ds_Items.Tables[0].Rows[i]["CMP_BACK_COLOR"]?.ToString(), "", "");
                                string subItemMenuColor = t.Set(ds_Items.Tables[0].Rows[i]["MENU_COLOR"]?.ToString(), "", "");
                                
                                Color subBackColor = Tkn_ToolBox.tDevExpressTheme.ParseColorFromDatabase(subItemBackColor, Tkn_ToolBox.tDevExpressTheme.BrandPrimary);
                                Color subHoverColor = Tkn_ToolBox.tDevExpressTheme.ParseColorFromDatabase(subItemMenuColor, Tkn_ToolBox.tDevExpressTheme.BrandAccent);
                                
                                // Apply modern tile appearance
                                Tkn_ToolBox.tDevExpressTheme.ApplyModernTileAppearance(tItem, subBackColor, subHoverColor);

                                if (clickEvents > 0)
                                {
                                    tItem.ElementClick += new DevExpress.XtraBars.Navigation.NavElementClickEventHandler(evm.tNavButton_ElementClick);
                                }

                                #region Image set
                                if (!DBNull.Value.Equals(ds_Items.Tables[0].Rows[i]["LKP_GLYPH16"]))
                                {
                                    byte[] img16 = (byte[])ds_Items.Tables[0].Rows[i]["LKP_GLYPH16"];
                                    if (img16 != null)
                                    {
                                        MemoryStream ms = new MemoryStream(img16);
                                        tItem.Glyph = Image.FromStream(ms);
                                        ms.Dispose();
                                    }
                                }

                                if (!DBNull.Value.Equals(ds_Items.Tables[0].Rows[i]["LKP_GLYPH32"]))
                                {
                                    byte[] img32 = (byte[])ds_Items.Tables[0].Rows[i]["LKP_GLYPH32"];
                                    if (img32 != null)
                                    {
                                        MemoryStream ms = new MemoryStream(img32);
                                        tItem.Glyph = Image.FromStream(ms);
                                        ms.Dispose();
                                    }
                                }
                                #endregion Image set


                                int i3 = mControl.Buttons.Count;
                                for (int i2 = 0; i2 < i3; i2++)
                                {
                                    if (mControl.Buttons[i2].Element.GetType().ToString() == "DevExpress.XtraBars.Navigation.TileNavCategory")
                                    {
                                        int i5 = ((TileNavCategory)mControl.Buttons[i2].Element).Items.Count;
                                        for (int i4 = 0; i4 < i5; i4++)
                                        {
                                            //if (mControl.Buttons[i2].Element.Name.ToString() == ustItemName)
                                            if (((TileNavCategory)mControl.Buttons[i2].Element).Items[i4].Name.ToString() == ustItemName)
                                            {
                                                ((TileNavCategory)mControl.Buttons[i2].Element).Items[i4].SubItems.Add(tItem);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion / TileNavItem

                    #region // Mali Dönem - YilAy
                    if ((t.IsNotNull(itemName) == false) &&
                        (itemType == 211))
                    {
                        /*
                        NavButton tItemBack = new DevExpress.XtraBars.Navigation.NavButton();
                        NavButton tItemNext = new DevExpress.XtraBars.Navigation.NavButton();

                        //mControl.Name

                        itemName = "item_";
                        Preparing_ButtonName(21, ref itemName);
                        tItemBack.Name = itemName;
                        tItemBack.Caption = "<";
                        tItemBack.Appearance.Name = formName;
                        tItemBack.Tag = mControl.Name + "|ControlName|";

                        itemName = "item_";
                        Preparing_ButtonName(22, ref itemName);
                        tItemNext.Name = itemName;
                        tItemNext.Caption = ">";
                        tItemNext.Appearance.Name = formName;
                        tItemNext.Tag = mControl.Name + "|ControlName|";

                        tItemBack.ElementClick += new DevExpress.XtraBars.Navigation.NavElementClickEventHandler(evm.tNavButton_ElementClick);
                        tItemNext.ElementClick += new DevExpress.XtraBars.Navigation.NavElementClickEventHandler(evm.tNavButton_ElementClick);

                        //itemName = "item_" + refid;
                        //if (clickEvents > 0)
                        //    Preparing_ButtonName(clickEvents, ref itemName);

                        // ilk defa çalıştı ve henüz atama yapılmamış ise 
                        if (v.DONEMTIPI_YILAY == 0)
                            v.DONEMTIPI_YILAY = v.BUGUN_YILAY;

                        NavButton tItem = new DevExpress.XtraBars.Navigation.NavButton();

                        tItem.Appearance.Name = formName;
                        //tItem.Name = itemName;
                        tItem.Name = "item_YILAY";
                        tItem.Caption = evm.getYilAyCaption(v.DONEMTIPI_YILAY);

                        evm.changeColorYILAY(mControl, 0, tItem);

                        // itemCaption = Belge Türü Seçin
                        if (t.IsNotNull(shortcut_keys))
                        {

                            if ((shortcut_keys == "SHORTCUT_BUTTON") |
                                (shortcut_keys == "SHORTCUT"))
                            {
                                itemName = "item_SHORTCUT_" + refid;

                                // event bağlansın
                                if (clickEvents == 0)
                                    clickEvents = 99;
                                else
                                    Preparing_ButtonName(clickEvents, ref itemName);

                                tItem.Name = itemName;
                            }
                            else
                                tItem.Caption = itemCaption + "   : " + shortcut_keys;

                            //tItem.Caption = itemCaption + "   <i>" + shortcut_keys + "</i>";
                            //tItem.  AllowHtmlText = DefaultBoolean.True;
                        }

                        tItem.Enabled = tenabled;
                        tItem.Visible = tvisible;
                        if (t.IsNotNull(Prop_Navigator))
                            tItem.Tag = Prop_Navigator + "|Prop_Navigator|";

                        if ((DockType == v.dock_None) | (DockType == v.dock_Left))
                            tItem.Alignment = NavButtonAlignment.Left;
                        if (DockType == v.dock_Right)
                            tItem.Alignment = NavButtonAlignment.Right;

                        if (clickEvents > 0)
                            tItem.ElementClick += new DevExpress.XtraBars.Navigation.NavElementClickEventHandler(evm.tNavButton_ElementClick);

                        #region Image set
                        if (!DBNull.Value.Equals(ds_Items.Tables[0].Rows[i]["LKP_GLYPH16"]))
                        {
                            byte[] img16 = (byte[])ds_Items.Tables[0].Rows[i]["LKP_GLYPH16"];
                            if (img16 != null)
                            {
                                MemoryStream ms = new MemoryStream(img16);
                                tItem.Glyph = Image.FromStream(ms);
                                ms.Dispose();
                            }
                        }

                        if (!DBNull.Value.Equals(ds_Items.Tables[0].Rows[i]["LKP_GLYPH32"]))
                        {
                            byte[] img32 = (byte[])ds_Items.Tables[0].Rows[i]["LKP_GLYPH32"];
                            if (img32 != null)
                            {
                                MemoryStream ms = new MemoryStream(img32);
                                tItem.Glyph = Image.FromStream(ms);
                                ms.Dispose();
                            }
                        }
                        #endregion Image set

                        mControl.Buttons.Add(tItemBack);
                        mControl.Buttons.Add(tItem);
                        mControl.Buttons.Add(tItemNext);
                        */
                    }
                    #endregion Mali Dönem
                }

                //tileNavItemSettings.Appearance.Options.UseBackColor = true;
                //tileNavItemSettings.Appearance.BackColor = Color.LightBlue;
            }

            #region Rapor
            //
            // Rapor Butonu
            //
            /*
            NavButton tItemReportDesign = new DevExpress.XtraBars.Navigation.NavButton();
            tItemReportDesign.Appearance.Name = formName;
            tItemReportDesign.Name = "item_FREPORTDESIGN";
            tItemReportDesign.Caption = "Rapor Dizayn";
            //tItemReport.Enabled = tenabled;
            //tItemReport.Visible = tvisible;
            //tItemReport.Tag = Prop_Navigator;
            tItemReportDesign.Alignment = NavButtonAlignment.Right;
            tItemReportDesign.ElementClick += new DevExpress.XtraBars.Navigation.NavElementClickEventHandler(evm.tNavButton_ElementClick);
            tItemReportDesign.Glyph = t.Find_Glyph("40_408_Delete_32x32"); //("40_401_Close_32x32");// ("40_418_Up_32x32");//("40_401_Close_32x32");
            //tItemReport.Appearance.BackColor = v.colorNew;
            tItemReportDesign.AppearanceHovered.BackColor = v.colorExit;
            //tItemEDI.AppearanceSelected.BackColor = v.colorSave;
            //tItemEDI.Appearance.Options.UseBackColor = true;
            tItemReportDesign.AppearanceHovered.Options.UseBackColor = true;
            tItemReportDesign.AppearanceSelected.Options.UseBackColor = true;
            mControl.Buttons.Add(tItemReportDesign);
            */

            //
            // Rapor Butonu
            //
            if (dontReport == false)
            {
                //NavButton tItemReport = new DevExpress.XtraBars.Navigation.NavButton();
                //tItemReport.Appearance.Name = formName;
                //tItemReport.Name = "item_FREPORTS";
                //tItemReport.Caption = "Raporlar";
                ////tItemReport.Enabled = tenabled;
                ////tItemReport.Visible = tvisible;
                ////tItemReport.Tag = Prop_Navigator;
                //tItemReport.Alignment = NavButtonAlignment.Right;
                //tItemReport.ElementClick += new DevExpress.XtraBars.Navigation.NavElementClickEventHandler(evm.tNavButton_ElementClick);
                //tItemReport.Glyph = t.Find_Glyph("40_408_Delete_32x32"); //("40_401_Close_32x32");// ("40_418_Up_32x32");//("40_401_Close_32x32");
                //                                                         //tItemReport.Appearance.BackColor = v.colorNew;
                //tItemReport.AppearanceHovered.BackColor = v.colorExit;
                ////tItemEDI.AppearanceSelected.BackColor = v.colorSave;
                ////tItemEDI.Appearance.Options.UseBackColor = true;
                //tItemReport.AppearanceHovered.Options.UseBackColor = true;
                //tItemReport.AppearanceSelected.Options.UseBackColor = true;

                //// Rapor formu açılınca hangi raporları select edeceği bilgisini taşıyan TableIPCode 
                //tItemReport.Tag = reportTableIPCode + "|TableIPCode|";

                //mControl.Buttons.Add(tItemReport);
            }
            #endregion Report            

            #region EDİ
            // Eğitim/Destek/İstek Butonu
            //
            if (dontEDI == false)
            {
                /*
                NavButton tItemEDI = new DevExpress.XtraBars.Navigation.NavButton();
                tItemEDI.Appearance.Name = formName;
                tItemEDI.Name = "item_FEDI";
                tItemEDI.Caption = "Eğitim/Destek/İstek";
                //tItemEDI.Caption = "e.d.i";
                //tItemEDI.Enabled = tenabled;
                //tItemEDI.Visible = tvisible;
                //tItemEDI.Tag = Prop_Navigator;
                tItemEDI.Alignment = NavButtonAlignment.Right;
                tItemEDI.ElementClick += new DevExpress.XtraBars.Navigation.NavElementClickEventHandler(evm.tNavButton_ElementClick);
                tItemEDI.Glyph = t.Find_Glyph("40_408_Delete_32x32"); //("40_401_Close_32x32");// ("40_418_Up_32x32");//("40_401_Close_32x32");
                                                                      //tItemEDI.Appearance.BackColor = v.colorNew;
                tItemEDI.AppearanceHovered.BackColor = v.colorExit;
                //tItemEDI.AppearanceSelected.BackColor = v.colorSave;
                //tItemEDI.Appearance.Options.UseBackColor = true;
                tItemEDI.AppearanceHovered.Options.UseBackColor = true;
                tItemEDI.AppearanceSelected.Options.UseBackColor = true;
                mControl.Buttons.Add(tItemEDI);
                */
            }
            #endregion EDİ            

            #region Exit
            // Exit - Çıkış Butonu
            //
            if (dontExit == false)
            {
                NavButton tItemExit = new DevExpress.XtraBars.Navigation.NavButton();
                tItemExit.Appearance.Name = formName;
                tItemExit.Name = "item_FEXIT";
                tItemExit.Caption = "Çıkış";
                //tItemExit.Enabled = tenabled;
                //tItemExit.Visible = tvisible;
                //tItemExit.Tag = Prop_Navigator;
                tItemExit.Alignment = NavButtonAlignment.Right;
                tItemExit.ElementClick += new DevExpress.XtraBars.Navigation.NavElementClickEventHandler(evm.tNavButton_ElementClick);
                tItemExit.Glyph = t.Find_Glyph("40_408_Delete_32x32"); //("40_401_Close_32x32");// ("40_418_Up_32x32");//("40_401_Close_32x32");
                                                                       //tItemExit.Appearance.BackColor = v.colorNew;
                tItemExit.AppearanceHovered.BackColor = v.colorExit;
                //tItemExit.AppearanceSelected.BackColor = v.colorSave;
                //tItemExit.Appearance.Options.UseBackColor = true;
                tItemExit.AppearanceHovered.Options.UseBackColor = true;
                tItemExit.AppearanceSelected.Options.UseBackColor = true;
                mControl.Buttons.Add(tItemExit);
            }
            #endregion Exit            

            if (tFirstItemCategory != null)
                mControl.SelectedElement = tFirstItemCategory;

            //InvokeOnClick //(checkBox1, EventArgs.Empty);

            ////************************************
            #region örnek
            /*
            void InitializeTileNavPane() {
            // Create the TileNavPane control and 
            // specify the required settings. 
            tileNavPane1 = new TileNavPane();
            tileNavPane1.Dock = System.Windows.Forms.DockStyle.Top;
            tileNavPane1.AllowGlyphSkinning = true;
            tileNavPane1.OptionsPrimaryDropDown.ShowItemShadow = DevExpress.Utils.DefaultBoolean.True;
            tileNavPane1.ElementClick += tileNavPane1_ElementClick;

            // Create a button and set the IsMain property to true. 
            // When the TileNavPane control is created in code, 
            // the Main Button should also be created manually. 
            NavButton mainButton = new NavButton() {
                Caption = "Main Menu",
                IsMain = true 
            };


            // Create a custom button to be displayed in the nav bar. 
            NavButton home = new NavButton() {
                Name = "Home",
                Glyph = MyTileNavPane.Properties.Resources.home_32x32
            };
            // Assign a hint for the Home button. 
            home.SuperTip = new DevExpress.Utils.SuperToolTip();
            home.SuperTip.Items.Add("Nullifies the element currently selected in the TileNavPane");


            // Create a category to be displayed in the nav bar. 
            TileNavCategory navBarCategory = new TileNavCategory() {
                Name = "navBarCategory",
                // Align the category at the right side of the nav bar. 
                // All elements that are added to the TileNavPane.Buttons collection 
                // after a right aligned element are also right aligned. 
                Alignment = NavButtonAlignment.Right,
                Caption = "Create",
                Glyph = MyTileNavPane.Properties.Resources.add_32x32,
                GlyphAlignment = NavButtonAlignment.Right
            };
            // Create items to be added to the nav bar category. 
            TileNavItem navBarCategoryItem1 = new TileNavItem() { TileText = "New customer" };
            TileNavItem navBarCategoryItem2 = new TileNavItem() { TileText = "New manager" };
            // Add the created items to the corresponding collection of the nav bar category. 
            navBarCategory.Items.AddRange(new TileNavItem[] { navBarCategoryItem1, navBarCategoryItem2 });


            // Create a button without caption for the nav bar. 
            NavButton help = new NavButton() { Glyph = MyTileNavPane.Properties.Resources.index_32x32 };

            // Add the created buttons to the nav bar button collection. 
            // Buttons are displayed in the nav bar in the order 
            // they are added to the collection 
            // with respect to their alignment relative to the nav bar. 
            tileNavPane1.Buttons.Add(home);
            tileNavPane1.Buttons.Add(mainButton);
            tileNavPane1.Buttons.Add(navBarCategory);
            tileNavPane1.Buttons.Add(help);


            // Create three sample categories to be displayed  
            // at the root-level of the navigation hierarchy. 
            // These categories are displayed on the Main Button click. 
            TileNavCategory cat1 = new TileNavCategory() {
                Caption = "SERVICE",
                TileText = "Service",
                TileImage = MyTileNavPane.Properties.Resources.newwizard_32x32,
                Glyph = MyTileNavPane.Properties.Resources.newwizard_16x16
            };
            TileNavCategory cat2 = new TileNavCategory() {
                Caption = "HELP",
                TileText = "Help",
                TileImage = MyTileNavPane.Properties.Resources.index_32x32,
                Glyph = MyTileNavPane.Properties.Resources.index_16x16,
            };


            // Specify colors for the Service and Help category tiles. 
            cat1.Tile.AppearanceItem.Normal.BackColor = Color.Red;
            cat1.Tile.AppearanceItem.Hovered.BackColor = Color.Green;
            cat2.Tile.AppearanceItem.Normal.BackColor = Color.Blue;
            // Specify the size of the Help category tile. 
            cat2.Tile.ItemSize = TileBarItemSize.Medium;

            // Set colors for the tiles in the in the Service category's drop-down 
            // for the normal and hovered states. 
            cat1.OptionsDropDown.AppearanceItem.Normal.BackColor = Color.Coral;
            cat1.OptionsDropDown.AppearanceItem.Hovered.BackColor = Color.LightCoral;
            // Set the color for the group captions in the Service category's drop-down. 
            cat1.OptionsDropDown.AppearanceGroupText.ForeColor = Color.Purple;
            // Add the created categories to the corresponding collection. 
            tileNavPane1.Categories.AddRange(new TileNavCategory[] { cat1, cat2 });


            // Create tile items to populate the Service category. 
            TileNavItem item1 = new TileNavItem() {
                Caption = "DASHBOARDS",
                TileText = "Dashboards",
                GroupName = "My Work",
                TileImage = MyTileNavPane.Properties.Resources.pie_32x32,
                Glyph = MyTileNavPane.Properties.Resources.pie_16x16
            };
            TileNavItem item2 = new TileNavItem() {
                Caption = "ACTIVITIES",
                TileText = "Activities",
                GroupName = "My Work" 
            };
            TileNavItem item3 = new TileNavItem() {
                Caption = "ACCOUNTS",
                TileText = "Accounts",
                GroupName = "Clients",
                Enabled = false 
            };
            // Add the created items to the corresponding collection of the Service category. 
            cat1.Items.AddRange(new TileNavItem[] { item1, item2, item3 });

            // Create sub-items to populate the Dashboards item. 
            TileNavSubItem subItem1 = new TileNavSubItem() {
                Caption = "Sales pipeline",
                TileText = "Sales pipeline" 
            };
            TileNavSubItem subItem2 = new TileNavSubItem() {
                Caption = "Estimated revenue",
                TileText = "Estimated revenue" 
            };
            // Add the created sub-items to the corresponding collection of the Dashboards item. 
            item1.SubItems.AddRange(new TileNavSubItem[] { subItem1, subItem2 });
        }

        void tileNavPane1_ElementClick(object sender, NavElementEventArgs e) {
            // Determine the navigation element being clicked using the e.Element property 
            // and perform the required action. In this example, the Home button click 
            // nullifies the element currently selected in the TileNavPane. 
            if (e.Element.Name == "Home") tileNavPane1.SelectedElement = null;
        }
    }

            */
            #endregion örnek
            
        }

        #endregion Create_TileNavPane

        #region Extract Menu Structure to JSON (for WebView2)

        /// <obsolete>
        /// This method is obsolete and will be removed in the future.
        /// Use ExtractMenuStructureFromDataSet instead.
        /// </obsolete>
        /// <summary>
        /// Extracts menu structure from TileNavPane into JSON format for WebView2 rendering.
        /// Extracts top-level categories (ItemType 201) which become the entrance screen cards.
        /// </summary>
        public string ExtractMenuStructureToJson(DevExpress.XtraBars.Navigation.TileNavPane pane)
        {
            tToolBox t = new tToolBox();
            var cards = new List<Dictionary<string, object>>();

            try
            {
                // Extract Categories (ItemType 201) - these become the entrance cards
                foreach (TileNavCategory category in pane.Categories)
                {
                    if (!category.Visible) continue;

                    var card = new Dictionary<string, object>
                    {
                        ["refId"] = category.Name.Replace("item_", ""),
                        ["name"] = category.Name,
                        ["caption"] = category.Caption ?? category.TileText ?? "",
                        ["tag"] = category.Tag?.ToString() ?? "",
                        ["enabled"] = category.Enabled,
                        ["visible"] = category.Visible,
                        ["lineNo"] = 0 // LINE_NO not available from control, would need to track from ds_Items
                    };

                    // Convert icon to base64
                    if (category.Glyph != null)
                    {
                        try
                        {
                            using (var ms = new MemoryStream())
                            {
                                category.Glyph.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                byte[] imageBytes = ms.ToArray();
                                card["iconBase64"] = "data:image/png;base64," + Convert.ToBase64String(imageBytes);
                            }
                        }
                        catch
                        {
                            card["iconBase64"] = "";
                        }
                    }
                    else
                    {
                        card["iconBase64"] = "";
                    }

                    // Extract colors (fallback to defaults if not available)
                    if (category.Tile?.AppearanceItem?.Normal?.BackColor != null)
                    {
                        var color = category.Tile.AppearanceItem.Normal.BackColor;
                        card["backColor"] = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                    }
                    else
                    {
                        card["backColor"] = "#295c00"; // Default primary color
                    }

                    if (category.Tile?.AppearanceItem?.Hovered?.BackColor != null)
                    {
                        var color = category.Tile.AppearanceItem.Hovered.BackColor;
                        card["hoverColor"] = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                    }
                    else
                    {
                        card["hoverColor"] = "#8e9c78"; // Default primary-light color
                    }

                    cards.Add(card);
                }

                // Sort by lineNo (for now, maintain order as added)
                // In production, would sort by LINE_NO from database

                var result = new Dictionary<string, object>
                {
                    ["cards"] = cards,
                    ["menuName"] = pane.Name ?? ""
                };

                // Use simple JSON serialization (System.Text.Json or Newtonsoft.Json)
                // For now, return a simple format that can be parsed
                return JsonConvert.SerializeObject(result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ExtractMenuStructureToJson error: {ex.Message}");
                return "{\"cards\":[],\"menuName\":\"\"}";
            }
        }

        /// <obsolete>
        /// This method is obsolete and will be removed in the future.
        /// Use ExtractMenuStructureFromTileControl instead.
        /// </obsolete>
        /// <summary>
        /// Alternative: Extract directly from DataSet (more accurate, includes LINE_NO)
        /// </summary>
        public string ExtractMenuStructureFromDataSet(DataSet ds_Items, string menuName = "")
        {
            tToolBox t = new tToolBox();
            var cards = new List<Dictionary<string, object>>();

            try
            {
                int itemCount = ds_Items.Tables[0].Rows.Count;

                for (int i = 0; i < itemCount; i++)
                {
                    int itemType = t.myInt32(ds_Items.Tables[0].Rows[i]["ITEM_TYPE"].ToString());
                    
                    // Only extract top-level categories (ItemType 201) with no ITEM_NAME
                    if (itemType == 201 && DBNull.Value.Equals(ds_Items.Tables[0].Rows[i]["ITEM_NAME"]))
                    {
                        string refid = ds_Items.Tables[0].Rows[i]["REF_ID"].ToString();
                        string caption = t.Set(ds_Items.Tables[0].Rows[i]["CAPTION"].ToString(), "", "");
                        string propNavigator = t.Set(ds_Items.Tables[0].Rows[i]["PROP_NAVIGATOR"].ToString(), "", "");
                        bool tenabled = t.Set(ds_Items.Tables[0].Rows[i]["CMP_ENABLED"].ToString(), "", true);
                        bool tvisible = t.Set(ds_Items.Tables[0].Rows[i]["CMP_VISIBLE"].ToString(), "", true);
                        int lineNo = t.myInt32(ds_Items.Tables[0].Rows[i]["LINE_NO"].ToString());

                        if (!tvisible) continue;

                        var card = new Dictionary<string, object>
                        {
                            ["refId"] = refid,
                            ["name"] = "item_" + refid,
                            ["caption"] = caption,
                            ["tag"] = propNavigator + "|Prop_Navigator|" + menuName + "|MenuName|",
                            ["enabled"] = tenabled,
                            ["visible"] = tvisible,
                            ["lineNo"] = lineNo
                        };

                        // Convert icon to base64
                        string iconBase64 = "";
                        if (!DBNull.Value.Equals(ds_Items.Tables[0].Rows[i]["LKP_GLYPH32"]))
                        {
                            try
                            {
                                byte[] img32 = (byte[])ds_Items.Tables[0].Rows[i]["LKP_GLYPH32"];
                                if (img32 != null && img32.Length > 0)
                                {
                                    iconBase64 = "data:image/png;base64," + Convert.ToBase64String(img32);
                                }
                            }
                            catch { }
                        }
                        card["iconBase64"] = iconBase64;

                        // Extract colors from database (with fallbacks)
                        // Check if columns exist before accessing them
                        string backColorStr = "";
                        string menuColorStr = "";
                        
                        if (ds_Items.Tables[0].Columns.Contains("CMP_BACK_COLOR"))
                        {
                            backColorStr = t.Set(ds_Items.Tables[0].Rows[i]["CMP_BACK_COLOR"]?.ToString(), "", "");
                        }
                        
                        if (ds_Items.Tables[0].Columns.Contains("MENU_COLOR"))
                        {
                            menuColorStr = t.Set(ds_Items.Tables[0].Rows[i]["MENU_COLOR"]?.ToString(), "", "");
                        }

                        // Parse colors or use defaults
                        if (!string.IsNullOrEmpty(backColorStr))
                        {
                            try
                            {
                                // Try to parse as hex or Color name
                                card["backColor"] = backColorStr;
                            }
                            catch
                            {
                                card["backColor"] = "#295c00";
                            }
                        }
                        else
                        {
                            card["backColor"] = "#295c00";
                        }

                        if (!string.IsNullOrEmpty(menuColorStr))
                        {
                            card["hoverColor"] = menuColorStr;
                        }
                        else
                        {
                            card["hoverColor"] = "#8e9c78";
                        }

                        cards.Add(card);
                    }
                }

                // Sort by LINE_NO
                cards.Sort((a, b) => 
                {
                    int aLine = Convert.ToInt32(a["lineNo"]);
                    int bLine = Convert.ToInt32(b["lineNo"]);
                    return aLine.CompareTo(bLine);
                });

                var result = new Dictionary<string, object>
                {
                    ["cards"] = cards,
                    ["menuName"] = menuName
                };

                return JsonConvert.SerializeObject(result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ExtractMenuStructureFromDataSet error: {ex.Message}");
                return "{\"cards\":[],\"menuName\":\"\"}";
            }
        }

        /// <obsolete>
        /// This method is obsolete and will be removed in the future.
        /// Use ExtractMenuStructureFromTileControl instead.
        /// </obsolete>
        /// <summary>
        /// Extracts menu structure from TileControl into JSON format for WebView2 rendering.
        /// Extracts TileItems (ItemType 205/206) which become the entrance screen cards.
        /// For entrance screen: 1 Group contains 9 Items, each Item becomes a card.
        /// </summary>
        public string ExtractMenuStructureFromTileControl(DevExpress.XtraEditors.TileControl control, string menuName = "")
        {
            tToolBox t = new tToolBox();
            var cards = new List<Dictionary<string, object>>();

            try
            {
                // Extract Items (ItemType 205/206) from all Groups - these become the entrance cards
                // The entrance screen has 1 Group with 9 Items, each Item is a card
                foreach (TileGroup group in control.Groups)
                {
                    if (!group.Visible) continue;

                    // Extract each Item in the Group as a card
                    foreach (TileItem item in group.Items)
                    {
                        if (!item.Visible) continue;

                        // Get item text from first element
                        string itemText = "";
                        if (item.Elements.Count > 0)
                        {
                            itemText = item.Elements[0].Text ?? "";
                        }

                        var card = new Dictionary<string, object>
                        {
                            ["refId"] = item.Name.Replace("item_", "").Split('_')[0], // Get base refId
                            ["name"] = item.Name,
                            ["caption"] = itemText,
                            ["tag"] = item.Tag?.ToString() ?? "",
                            ["enabled"] = item.Enabled,
                            ["visible"] = item.Visible,
                            ["lineNo"] = item.Id // Use Id as lineNo
                        };

                        // Extract colors from item appearance
                        string backColor = "#295c00"; // Default
                        string hoverColor = "#8e9c78"; // Default

                        if (item.Appearance.BackColor != Color.Empty)
                        {
                            var color = item.Appearance.BackColor;
                            backColor = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                        }

                        // Icon extraction from TileItemElement.ImageOptions.Image
                        string iconBase64 = "";
                        try
                        {
                            // Check each element for an image
                            foreach (DevExpress.XtraEditors.TileItemElement element in item.Elements)
                            {
                                if (element.ImageOptions?.Image != null)
                                {
                                    using (var ms = new MemoryStream())
                                    {
                                        element.ImageOptions.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                        byte[] imageBytes = ms.ToArray();
                                        iconBase64 = "data:image/png;base64," + Convert.ToBase64String(imageBytes);
                                        break; // Use first image found
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Icon extraction error for {item.Name}: {ex.Message}");
                        }

                        card["iconBase64"] = iconBase64;
                        card["backColor"] = backColor;
                        card["hoverColor"] = hoverColor;

                        cards.Add(card);
                    }
                }

                // Sort by lineNo (for now, maintain order as added)
                var result = new Dictionary<string, object>
                {
                    ["cards"] = cards,
                    ["menuName"] = menuName
                };

                return JsonConvert.SerializeObject(result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ExtractMenuStructureFromTileControl error: {ex.Message}");
                return "{\"cards\":[],\"menuName\":\"" + menuName + "\"}";
            }
        }

        #endregion Extract Menu Structure to JSON

        #region Create_NavigationPane
        public void Create_NavigationPane(DevExpress.XtraBars.Navigation.NavigationPane mControl, DataSet ds_Items)
        {
            tToolBox t = new tToolBox();

            int itemCount = ds_Items.Tables[0].Rows.Count;
            int pageCount = mControl.Pages.Count;

            int itemType = 0;

            string lineNo = string.Empty;
            string refid = string.Empty;
            string itemName = string.Empty;
            string itemCaption = string.Empty;
            string Prop_View = string.Empty;
            //string Prop_Navigator = string.Empty;
            //Int16 clickEvents = 0;

            //int ustItemType = 0;
            //string ustHesapRefId = string.Empty;
            //string ustHesapItemName = string.Empty;
            //string ustItemName = string.Empty;

            Boolean tenabled = false;
            Boolean tvisible = false;
            //DataRow UstHesapRow = null;

            //this.navigationPane1 = new DevExpress.XtraBars.Navigation.NavigationPane();
            //this.navigationPage1 = new DevExpress.XtraBars.Navigation.NavigationPage();

            for (int i = 0; i < itemCount; i++)
            {
                refid = ds_Items.Tables[0].Rows[i]["REF_ID"].ToString();
                itemType = t.myInt32(ds_Items.Tables[0].Rows[i]["ITEM_TYPE"].ToString());
                itemName = t.Set(ds_Items.Tables[0].Rows[i]["ITEM_NAME"].ToString(), "", "");
                itemCaption = t.Set(ds_Items.Tables[0].Rows[i]["CAPTION"].ToString(), "", "");
                lineNo = t.Set(ds_Items.Tables[0].Rows[i]["LINE_NO"].ToString(), "", "");
                tenabled = t.Set(ds_Items.Tables[0].Rows[i]["CMP_ENABLED"].ToString(), "", true);
                tvisible = t.Set(ds_Items.Tables[0].Rows[i]["CMP_VISIBLE"].ToString(), "", true);
                //clickEvents = t.myInt16(ds_Items.Tables[0].Rows[i]["CLICK_EVENTS"].ToString());
                Prop_View = t.Set(ds_Items.Tables[0].Rows[i]["PROP_VIEWS"].ToString(), "", "");
                //Prop_Navigator = t.Set(ds_Items.Tables[0].Rows[i]["PROP_NAVIGATOR"].ToString(), "", "");

                //if (clickEvents > 0) Preparing_ButtonName(clickEvents, ref itemName);

                //UstHesapRow = null;
                //ustHesapRefId = string.Empty;
                //ustHesapItemName = string.Empty;
                //ustItemName = string.Empty;

                #region page
                // yeni page oluşturma
                if ((t.IsNotNull(itemName) == false) &&
                    (itemType == 202))
                {
                    if ((lineNo) == "0") lineNo = refid;

                    DevExpress.XtraBars.Navigation.NavigationPage page = new DevExpress.XtraBars.Navigation.NavigationPage();
                    page.Name = "item_" + refid;
                    page.Text = itemCaption;
                    page.PageVisible = tvisible;

                    #region Image set
                    if (!DBNull.Value.Equals(ds_Items.Tables[0].Rows[i]["LKP_GLYPH16"]))
                    {
                        byte[] img16 = (byte[])ds_Items.Tables[0].Rows[i]["LKP_GLYPH16"];
                        if (img16 != null)
                        {
                            MemoryStream ms = new MemoryStream(img16);
                            page.Image = Image.FromStream(ms);
                            ms.Dispose();
                        }
                    }

                    if (!DBNull.Value.Equals(ds_Items.Tables[0].Rows[i]["LKP_GLYPH32"]))
                    {
                        byte[] img32 = (byte[])ds_Items.Tables[0].Rows[i]["LKP_GLYPH32"];
                        if (img32 != null)
                        {
                            MemoryStream ms = new MemoryStream(img32);
                            page.Image = Image.FromStream(ms);
                            ms.Dispose();
                        }
                    }
                    #endregion Image set

                    if (t.IsNotNull(Prop_View))
                        Preparing_NavigationPage(Prop_View, page);

                    mControl.Pages.Add(page);
                }

                // mevcut page ye müdehale
                if ((t.IsNotNull(itemName) == true) &&
                    (itemType == 202))
                {
                    int i2 = mControl.Pages.Count;
                    DevExpress.XtraBars.Navigation.NavigationPage pagex = null;

                    for (int i1 = 0; i1 < i2; i1++)
                    {
                        if (mControl.Pages[i1].Name.ToString() == itemName)
                        {
                            if (t.IsNotNull(itemCaption))
                                mControl.Pages[i1].Text = itemCaption;

                            mControl.Pages[i1].PageVisible = tvisible;

                            if (t.IsNotNull(Prop_View))
                            {
                                pagex = mControl.Pages[i1] as DevExpress.XtraBars.Navigation.NavigationPage;

                                Preparing_NavigationPage(Prop_View, pagex);
                            }
                            break;
                        }
                    }
                }
                #endregion page                

            }

        }

        private void Preparing_NavigationPage(string Prop_View, DevExpress.XtraBars.Navigation.NavigationPage page)
        {
            tToolBox t = new tToolBox();

            string s1 = "=ROW_PROP_VIEWS:";
            string s2 = (char)34 + "NAVIGATIONPANE" + (char)34 + ": {";
            string menuCode = string.Empty;

            if (Prop_View.IndexOf(s1) > -1)
            {
                menuCode = t.MyProperties_Get(Prop_View, "NP_MENUCODE:");
            }

            if (Prop_View.IndexOf(s2) > -1)
            {
                menuCode = t.Find_Properties_Value(Prop_View, "NP_MENUCODE");
            }

            if (t.IsNotNull(menuCode))
            {
                Create_Menu_IN_Control(page, menuCode, string.Empty);
            }
        }
        #endregion Create_NavigationPane

        #region Create_AccordionControl
        public void Create_AccordionControl(DevExpress.XtraBars.Navigation.AccordionControl mControl, DataSet ds_Items)
        {
            tToolBox t = new tToolBox();
            tEvents ev = new tEvents();
            tEventsMenu evm = new tEventsMenu();


            int itemCount = ds_Items.Tables[0].Rows.Count;
            int groupCount = mControl.Elements.Count;
            int itemType = 0;

            string lineNo = string.Empty;
            string refid = string.Empty;
            string itemName = string.Empty;
            string itemCaption = string.Empty;
            string Prop_Views = string.Empty;
            string Prop_Navigator = string.Empty;

            Int16 clickEvents = 0;

            int ustItemType = 0;
            string ustHesapRefId = string.Empty;
            string ustHesapItemName = string.Empty;
            string ustItemName = string.Empty;

            Boolean tenabled = false;
            Boolean tvisible = false;
            DataRow UstHesapRow = null;

            for (int i = 0; i < itemCount; i++)
            {
                refid = ds_Items.Tables[0].Rows[i]["REF_ID"].ToString();
                itemType = t.myInt32(ds_Items.Tables[0].Rows[i]["ITEM_TYPE"].ToString());
                itemName = t.Set(ds_Items.Tables[0].Rows[i]["ITEM_NAME"].ToString(), "", "");
                itemCaption = t.Set(ds_Items.Tables[0].Rows[i]["CAPTION"].ToString(), "", "");
                lineNo = t.Set(ds_Items.Tables[0].Rows[i]["LINE_NO"].ToString(), "", "");
                tenabled = t.Set(ds_Items.Tables[0].Rows[i]["CMP_ENABLED"].ToString(), "", true);
                tvisible = t.Set(ds_Items.Tables[0].Rows[i]["CMP_VISIBLE"].ToString(), "", true);
                clickEvents = t.myInt16(ds_Items.Tables[0].Rows[i]["CLICK_EVENTS"].ToString());
                Prop_Views = t.Set(ds_Items.Tables[0].Rows[i]["PROP_VIEWS"].ToString(), "", "");
                Prop_Navigator = t.Set(ds_Items.Tables[0].Rows[i]["PROP_NAVIGATOR"].ToString(), "", "");

                UstHesapRow = null;
                ustHesapRefId = string.Empty;
                ustHesapItemName = string.Empty;
                ustItemName = string.Empty;

                UstHesapRow = UstHesap_Get(ds_Items, i);
                if (UstHesapRow != null)
                    ustItemType = t.myInt32(UstHesapRow["ITEM_TYPE"].ToString());

                // ust hesap menu tipi ise null yap, menu elemanı ise devam et (200...299)
                if (ustItemType < 200)
                    UstHesapRow = null;


                //this.accordionControl1 = new DevExpress.XtraBars.Navigation.AccordionControl();
                //this.accordionControlElement1 = new DevExpress.XtraBars.Navigation.AccordionControlElement();
                //accordionControlElement1.Style = Group
                //this.accordionControlElement2 = new DevExpress.XtraBars.Navigation.AccordionControlElement();
                //accordionControlElement2.Style = Item

                #region New AccordionControlElement.Item
                if (t.IsNotNull(itemName) == false)
                {
                    itemName = "item_" + refid;
                    if (clickEvents > 0)
                        Preparing_ButtonName(clickEvents, ref itemName);

                    DevExpress.XtraBars.Navigation.AccordionControlElement tItem =
                        new DevExpress.XtraBars.Navigation.AccordionControlElement();
                    tItem.Name = itemName;
                    tItem.Text = itemCaption;
                    tItem.Enabled = tenabled;
                    tItem.Visible = tvisible;
                    //tItem.

                    #region Image set
                    if (!DBNull.Value.Equals(ds_Items.Tables[0].Rows[i]["LKP_GLYPH16"]))
                    {
                        byte[] img16 = (byte[])ds_Items.Tables[0].Rows[i]["LKP_GLYPH16"];
                        if (img16 != null)
                        {
                            MemoryStream ms = new MemoryStream(img16);
                            tItem.Image = Image.FromStream(ms);
                            ms.Dispose();
                        }
                    }

                    if (!DBNull.Value.Equals(ds_Items.Tables[0].Rows[i]["LKP_GLYPH32"]))
                    {
                        byte[] img32 = (byte[])ds_Items.Tables[0].Rows[i]["LKP_GLYPH32"];
                        if (img32 != null)
                        {
                            MemoryStream ms = new MemoryStream(img32);
                            tItem.Image = Image.FromStream(ms);
                            ms.Dispose();
                        }
                    }
                    #endregion Image set

                    // Group
                    if (itemType == 203)
                    {
                        tItem.Style = DevExpress.XtraBars.Navigation.ElementStyle.Group;
                        tItem.Expanded = false;

                        if (t.IsNotNull(Prop_Views))
                        {
                            //string myBool = t.MyProperties_Get(Prop_Views, "ACCEXPANDED:");
                            //if (myBool == "TRUE")
                            if (Prop_Views.IndexOf("\"ACCEXPANDED\": \"TRUE\"") > -1)
                                tItem.Expanded = true;
                            else tItem.Expanded = false;
                        }
                    }

                    // Item
                    if (itemType == 206)
                    {
                        tItem.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
                        //tItem.Click += new System.EventArgs(ev.tAccordionControlElement_Click);
                        //tItem.Click += new ElementClickEventHandler(ev.tAccordionControlElement_Click);
                        //tItem.Click += new AccordionControlElementBase(ev.tAccordionControlElement_Click);
                        tItem.Click += new EventHandler(evm.tAccordionControlElement_Click);
                        tItem.Tag = Prop_Navigator;

                        //tItem.
                        //TableIPCode = ((DevExpress.XtraBars.BarLargeButtonItem)e.Item).AccessibleName;
                    }

                    // üst hesabı yok ise roota yaz
                    if (UstHesapRow == null)
                    {
                        mControl.Elements.Add(tItem);
                    }

                    #region // üst hesabı var ise üst hesabı bul onun altına ekle
                    if (UstHesapRow != null)
                    {
                        ustHesapRefId = UstHesapRow["REF_ID"].ToString();
                        ustHesapItemName = UstHesapRow["ITEM_NAME"].ToString();

                        if (t.IsNotNull(ustHesapItemName)) ustItemName = ustHesapItemName;
                        else ustItemName = "item_" + ustHesapRefId;

                        int i7 = 0;
                        int i5 = 0;
                        int i3 = mControl.Elements.Count;
                        for (int i2 = 0; i2 < i3; i2++)
                        {
                            if (mControl.Elements[i2].Name.ToString() == ustItemName)
                            {
                                mControl.Elements[i2].Style = DevExpress.XtraBars.Navigation.ElementStyle.Group;
                                mControl.Elements[i2].Elements.Add(tItem);
                                break;
                            }
                            if ((mControl.Elements[i2].Name.ToString() != ustItemName) &&
                                (mControl.Elements[i2].Elements.Count > 0))
                            {
                                i5 = mControl.Elements[i2].Elements.Count;
                                for (int i4 = 0; i4 < i5; i4++)
                                {
                                    if (mControl.Elements[i2].Elements[i4].Name.ToString() == ustItemName)
                                    {
                                        mControl.Elements[i2].Elements[i4].Style = DevExpress.XtraBars.Navigation.ElementStyle.Group;
                                        mControl.Elements[i2].Elements[i4].Elements.Add(tItem);
                                        break;
                                    }
                                    if ((mControl.Elements[i2].Elements[i4].Name.ToString() != ustItemName) &&
                                        (mControl.Elements[i2].Elements[i4].Elements.Count > 0))
                                    {
                                        i7 = mControl.Elements[i2].Elements[i4].Elements.Count;
                                        for (int i6 = 0; i6 < i7; i6++)
                                        {
                                            if (mControl.Elements[i2].Elements[i4].Elements[i6].Name.ToString() == ustItemName)
                                            {
                                                mControl.Elements[i2].Elements[i4].Elements[i6].Style = DevExpress.XtraBars.Navigation.ElementStyle.Group;
                                                mControl.Elements[i2].Elements[i4].Elements[i6].Elements.Add(tItem);
                                                break;
                                            }
                                        }//for
                                    }//if ((
                                }//for
                            }//if ((
                        }//for
                    }//if UstHesaoRow
                    #endregion

                    itemType = 0;
                }
                #endregion AccordionControlElement.Item

                #region Old AccordionControlElement.Item
                if (t.IsNotNull(itemName) == true)
                {
                    int i5 = mControl.Elements.Count;
                    for (int i4 = 0; i4 < i5; i4++)
                    {
                        if (mControl.Elements[i4].Name.ToString() == itemName)
                        {
                            if (t.IsNotNull(itemCaption))
                                mControl.Elements[i4].Text = itemCaption;
                            mControl.Elements[i4].Visible = tvisible;
                            break;
                        }
                    }
                    itemType = 0;
                }
                #endregion Old AccordionControlElement.Item

            }



        }
        #endregion Create_AccordionControl

        #region Create_ToolBoxControl
        public void Create_ToolBoxControl(
            DevExpress.XtraToolbox.ToolboxControl mControl,
            DataSet ds_Items)
        {
            tToolBox t = new tToolBox();
            tEventsMenu evm = new tEventsMenu();

            // form tespit et
            Form tForm = mControl.FindForm();

            mControl.AllowHtmlTextInToolTip = DefaultBoolean.True;

            // toolbox yukarıda bir panelin için eklendikten sonra
            // buraya geliyor
            Control tPanelControl = mControl.Parent;

            DataSet dsRead = null;
            int itemCount = ds_Items.Tables[0].Rows.Count;
            int itemType = 0;

            string lineNo = string.Empty;
            string refid = string.Empty;
            string itemName = string.Empty;
            string itemCaption = string.Empty;
            string shortcut_keys = string.Empty;
            string Prop_Navigator = string.Empty;

            Int16 clickEvents = 0;
            int group_count = 1;

            int ustItemType = 0;
            string ustHesapRefId = string.Empty;
            string ustHesapItemName = string.Empty;
            string ustItemName = string.Empty;

            Boolean tenabled = false;
            Boolean tvisible = false;
            DataRow UstHesapRow = null;

            foreach (DevExpress.XtraToolbox.ToolboxGroup pGroup in mControl.Groups)
            {
                pGroup.Items.Clear();
            }

            //if (mControl.Groups.Count > 1)
            //    mControl.Groups.Clear();

            while (mControl.Groups.Count > 0)
                mControl.Groups.RemoveAt(0);


            for (int i = 0; i < itemCount; i++)
            {
                refid = ds_Items.Tables[0].Rows[i]["REF_ID"].ToString();
                itemType = t.myInt32(ds_Items.Tables[0].Rows[i]["ITEM_TYPE"].ToString());
                itemName = t.Set(ds_Items.Tables[0].Rows[i]["ITEM_NAME"].ToString(), "", "");
                itemCaption = t.Set(ds_Items.Tables[0].Rows[i]["CAPTION"].ToString(), "", "");
                shortcut_keys = t.Set(ds_Items.Tables[0].Rows[i]["SHORTCUT_KEYS"].ToString(), "", "");

                lineNo = t.Set(ds_Items.Tables[0].Rows[i]["LINE_NO"].ToString(), "", "");
                tenabled = t.Set(ds_Items.Tables[0].Rows[i]["CMP_ENABLED"].ToString(), "", true);
                tvisible = t.Set(ds_Items.Tables[0].Rows[i]["CMP_VISIBLE"].ToString(), "", true);
                clickEvents = t.myInt16(ds_Items.Tables[0].Rows[i]["CLICK_EVENTS"].ToString());
                Prop_Navigator = t.Set(ds_Items.Tables[0].Rows[i]["PROP_NAVIGATOR"].ToString(), "", "");

                UstHesapRow = null;
                ustHesapRefId = string.Empty;
                ustHesapItemName = string.Empty;
                ustItemName = string.Empty;

                // ToolBox ilk satır,  dsRead tablosu hazırlanıyor
                if (itemType == 109)
                {
                    if (tPanelControl is DevExpress.XtraEditors.PanelControl)
                    {
                        if (tForm != null)
                        {
                            toolBox_Preparing(tForm, tPanelControl, mControl, null, Prop_Navigator, itemType, clickEvents, ref dsRead);
                            if (t.IsNotNull(dsRead))
                            {

                            }
                        }
                    }
                    else
                    {
                        if (mControl.Tag == null)
                        {
                            //mControl.SelectedGroupIndex  //ItemClick -= ev.tToolboxControl_ItemClick;
                            mControl.ItemClick += new DevExpress.XtraToolbox.ToolboxItemClickEventHandler(evm.tToolboxControl_ItemClick);
                            mControl.Tag = 1;
                        }
                    }
                }

                #region group
                // yeni Group oluşturma
                if ((t.IsNotNull(itemName) == false) &&
                    (tvisible) &&
                    (itemType == 203))
                {
                    DevExpress.XtraToolbox.ToolboxGroup pGroup = new DevExpress.XtraToolbox.ToolboxGroup();
                    pGroup.Name = "item_" + refid;
                    pGroup.Caption = itemCaption;
                    if (t.IsNotNull(shortcut_keys))
                        pGroup.Caption = shortcut_keys + " :    " + itemCaption;
                    pGroup.Visible = tvisible;

                    //pGroup.Appearance.

                    // Bu Grubun altına navigator de belirtilen IP okunacak

                    //0 = CHC_IPCODE:HPBNK.HPBNK_01;
                    //0 = CHC_FNAME:LKP_FULL_NAME;
                    //0 = CHC_VALUE:LKP_BANKSUBE_ID;

                    // ve okunan IP deki değerler ile
                    // grubun altına DevExpress.XtraToolbox.ToolboxItem oluşturulacak
                    // böylece dinamik liste hazırlanmış olaracak

                    //'CLICK_EVENTS', 11, '', 'TBOX_MAIN'
                    //'CLICK_EVENTS', 12, '', 'TBOX_GROUP'

                    if (clickEvents == 11)
                    {
                        if (tForm != null)
                            toolBox_Preparing(tForm, tPanelControl, null, pGroup, Prop_Navigator, itemType, clickEvents, ref dsRead);

                        // daha sonra bu ismi görünce gruba müdehale edilmeyecek
                        // çünkü bu gruptan her seçim yapıldıkca diğer gruplar altındaki
                        // items lar değişecek, ama bu sabit kalacak
                        pGroup.Name = "item_MainGroup_" + refid;

                        pGroup.Appearance.Normal.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
                        pGroup.Appearance.Normal.ForeColor = System.Drawing.Color.Crimson;
                        pGroup.Appearance.Normal.Options.UseFont = true;
                        pGroup.Appearance.Normal.Options.UseForeColor = true;
                        pGroup.Tag = Prop_Navigator;

                        mControl.SelectedGroup = pGroup;
                        mControl.SelectedGroupIndex = group_count;
                    }

                    if (clickEvents == 12)
                    {
                        if (tForm != null)
                            toolBox_Preparing(tForm, tPanelControl, null, pGroup, Prop_Navigator, itemType, clickEvents, ref dsRead);

                        pGroup.Name = "item_Group_" + refid;

                        //pGroup.Appearance.Normal.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
                        //pGroup.Appearance.Normal.ForeColor = System.Drawing.Color.
                        //pGroup.Appearance.Normal.Options.UseFont = true;
                        //pGroup.Appearance.Normal.Options.UseForeColor = true;
                        pGroup.Tag = Prop_Navigator;
                    }

                    //if (group_count == 1) pGroup

                    group_count++;

                    //if (mControl.Groups == null)

                    mControl.Groups.Add(pGroup);
                }
                #endregion group

                #region item
                if ((t.IsNotNull(itemName) == false) &&
                    (tvisible) &&
                    ((itemType == 205) || (itemType == 206)))
                {
                    DevExpress.XtraToolbox.ToolboxItem barButtonItem = new DevExpress.XtraToolbox.ToolboxItem();

                    itemName = "item_" + refid;

                    if (clickEvents > 0)
                        Preparing_ButtonName(clickEvents, ref itemName);

                    barButtonItem.Name = itemName;
                    barButtonItem.Caption = itemCaption;
                    if (t.IsNotNull(shortcut_keys))
                        barButtonItem.Caption = shortcut_keys + v.ENTER + "     " + itemCaption;
                    //barButtonItem.Caption = itemCaption + " :  " + shortcut_keys;
                    barButtonItem.Tag = Prop_Navigator;

                    if (itemCaption.IndexOf("...") > -1)
                    {
                        /* çalışmadı
                        barButtonItem.Appearance.Normal.BackColor = v.colorExit;
                        barButtonItem.Appearance.Normal.Options.UseBackColor = true;
                        barButtonItem.Appearance.Hovered.BackColor = v.colorExit;
                        barButtonItem.Appearance.Hovered.Options.UseBackColor = true;
                        */

                        if ((v.sp_SelectSkinName.ToLower().IndexOf("dark") > -1) ||
                            (v.sp_SelectSkinName.ToLower().IndexOf("black") > -1))
                            barButtonItem.Appearance.Normal.ForeColor = Color.Green;
                        else barButtonItem.Appearance.Normal.ForeColor = Color.Tomato;

                        //barButtonItem.Appearance.Normal.ForeColor = Color.Tomato;
                        //barButtonItem.Appearance.Normal.ForeColor = SystemColors.HotTrack; //SystemColors.ActiveBorder; 
                        barButtonItem.Appearance.Normal.Options.UseForeColor = true;

                        barButtonItem.Appearance.Normal.Font = new System.Drawing.Font(barButtonItem.Appearance.Normal.Font.FontFamily, barButtonItem.Appearance.Normal.Font.Size, FontStyle.Regular);
                        barButtonItem.Appearance.Normal.Options.UseFont = true;

                        
                    }
                    //barButtonItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(ev.formRibbonMenu_ItemClick);

                    // image çalışıyor SİLME 
                    // button Image set

                    #region Image set
                    if (!DBNull.Value.Equals(ds_Items.Tables[0].Rows[i]["LKP_GLYPH16"]))
                    {
                        byte[] img16 = (byte[])ds_Items.Tables[0].Rows[i]["LKP_GLYPH16"];
                        if (img16 != null)
                        {
                            MemoryStream ms = new MemoryStream(img16);
                            barButtonItem.Image = Image.FromStream(ms);
                            ms.Dispose();
                        }
                    }

                    if (!DBNull.Value.Equals(ds_Items.Tables[0].Rows[i]["LKP_GLYPH32"]))
                    {
                        byte[] img32 = (byte[])ds_Items.Tables[0].Rows[i]["LKP_GLYPH32"];
                        if (img32 != null)
                        {
                            MemoryStream ms = new MemoryStream(img32);
                            barButtonItem.Image = Image.FromStream(ms);
                            ms.Dispose();
                        }
                    }
                    #endregion Image set


                    //mControl.Items.Add(barButtonItem);

                    UstHesapRow = UstHesap_Get(ds_Items, i);

                    if (UstHesapRow != null)
                    {
                        ustItemType = t.myInt32(UstHesapRow["ITEM_TYPE"].ToString());
                        ustHesapRefId = UstHesapRow["REF_ID"].ToString();
                        ustHesapItemName = UstHesapRow["ITEM_NAME"].ToString();

                        if (t.IsNotNull(ustHesapItemName)) ustItemName = ustHesapItemName;
                        else ustItemName = "item_" + ustHesapRefId;

                        #region üst hesap Group ise
                        if (ustItemType == 203)
                        {
                            int i3 = mControl.Groups.Count;

                            for (int i2 = 0; i2 < i3; i2++)
                            {
                                if (mControl.Groups[i2].Name.ToString() == ustItemName)
                                {
                                    // gruplar ile itemlar arası boşluk oluşturalım
                                    //
                                    if (mControl.Groups[i2].Items.Count == 0)
                                    {
                                        DevExpress.XtraToolbox.ToolboxItem barButtonItemBosluk = new DevExpress.XtraToolbox.ToolboxItem();
                                        barButtonItemBosluk.Caption = "";
                                        barButtonItemBosluk.Name = "itemBosluk_" + refid;
                                        mControl.Groups[i2].Items.Add(barButtonItemBosluk);
                                    }


                                    mControl.Groups[i2].Items.Add(barButtonItem);
                                    break;
                                }
                            }
                        }
                        #endregion üst hesap Group ise

                    }

                }
                #endregion
            }

        }
        #endregion

        #region Create_AccordionDinamik
        public void Create_AccordionDinamik(
            DevExpress.XtraBars.Navigation.AccordionControl mControl,
            DataSet ds_Items)
        {
            tToolBox t = new tToolBox();
            tEvents ev = new tEvents();

            // form tespit et
            Form tForm = mControl.FindForm();
            // AccordionDinamik yukarıda bir panelin için eklendikten sonra
            // buraya geliyor
            Control tPanelControl = mControl.Parent;

            DataSet dsRead = null;
            int itemCount = ds_Items.Tables[0].Rows.Count;
            int itemType = 0;

            string lineNo = string.Empty;
            string refid = string.Empty;
            string itemName = string.Empty;
            string itemCaption = string.Empty;
            string Prop_Navigator = string.Empty;

            Int16 clickEvents = 0;
            int group_count = 1;

            int ustItemType = 0;
            string ustHesapRefId = string.Empty;
            string ustHesapItemName = string.Empty;
            string ustItemName = string.Empty;

            Boolean tenabled = false;
            Boolean tvisible = false;
            DataRow UstHesapRow = null;

            for (int i = 0; i < itemCount; i++)
            {
                refid = ds_Items.Tables[0].Rows[i]["REF_ID"].ToString();
                itemType = t.myInt32(ds_Items.Tables[0].Rows[i]["ITEM_TYPE"].ToString());
                itemName = t.Set(ds_Items.Tables[0].Rows[i]["ITEM_NAME"].ToString(), "", "");
                itemCaption = t.Set(ds_Items.Tables[0].Rows[i]["CAPTION"].ToString(), "", "");
                lineNo = t.Set(ds_Items.Tables[0].Rows[i]["LINE_NO"].ToString(), "", "");
                tenabled = t.Set(ds_Items.Tables[0].Rows[i]["CMP_ENABLED"].ToString(), "", true);
                tvisible = t.Set(ds_Items.Tables[0].Rows[i]["CMP_VISIBLE"].ToString(), "", true);
                clickEvents = t.myInt16(ds_Items.Tables[0].Rows[i]["CLICK_EVENTS"].ToString());
                Prop_Navigator = t.Set(ds_Items.Tables[0].Rows[i]["PROP_NAVIGATOR"].ToString(), "", "");

                UstHesapRow = null;
                ustHesapRefId = string.Empty;
                ustHesapItemName = string.Empty;
                ustItemName = string.Empty;

                // AccordionDinamik ilk satır,  dsRead tablosu hazırlanıyor
                // UST/OMS/HVezneFinans.Hesap_L02
                if (itemType == 110)
                {
                    if (tForm != null)
                    {
                        accordionDinamik_Preparing(tForm, tPanelControl, mControl, null, Prop_Navigator, itemType, clickEvents, ref dsRead);
                    }
                }

                #region group
                // yeni Group oluşturma
                if ((t.IsNotNull(itemName) == false) &&
                    (itemType == 203))
                {
                    //DevExpress.XtraToolbox.ToolboxGroup pGroup = new DevExpress.XtraToolbox.ToolboxGroup();
                    DevExpress.XtraBars.Navigation.AccordionControlElement pGroup =
                        new DevExpress.XtraBars.Navigation.AccordionControlElement();

                    pGroup.Name = "item_" + refid;
                    pGroup.Text = itemCaption;
                    pGroup.Enabled = tenabled;
                    pGroup.Visible = tvisible;
                    pGroup.Style = DevExpress.XtraBars.Navigation.ElementStyle.Group;
                    pGroup.Expanded = false;

                    // Bu Grubun altına navigator de belirtilen IP okunacak

                    //0 = CHC_IPCODE:HPBNK.HPBNK_01;
                    //0 = CHC_FNAME:LKP_FULL_NAME;
                    //0 = CHC_VALUE:LKP_BANKSUBE_ID;

                    // ve okunan IP deki değerler ile
                    // grubun altına DevExpress.XtraToolbox.ToolboxItem oluşturulacak
                    // böylece dinamik liste hazırlanmış olaracak

                    //'CLICK_EVENTS', 11, '', 'TBOX_MAIN'
                    //'CLICK_EVENTS', 12, '', 'TBOX_GROUP'

                    if (clickEvents == 11)
                    {
                        if (tForm != null)
                            accordionDinamik_Preparing(tForm, tPanelControl, null, pGroup, Prop_Navigator, itemType, clickEvents, ref dsRead);

                        // daha sonra bu ismi görünce gruba müdehale edilmeyecek
                        // çünkü bu gruptan her seçim yapıldıkca diğer gruplar altındaki
                        // items lar değişecek, ama bu sabit kalacak
                        pGroup.Name = "item_MainGroup_" + refid;

                        pGroup.Appearance.Normal.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
                        //pGroup.Appearance.Normal.ForeColor = System.Drawing.Color.Crimson;
                        pGroup.Appearance.Normal.Options.UseFont = true;
                        pGroup.Appearance.Normal.Options.UseForeColor = true;
                        pGroup.Tag = Prop_Navigator;
                    }

                    if (clickEvents == 12)
                    {
                        if (tForm != null)
                            accordionDinamik_Preparing(tForm, tPanelControl, null, pGroup, Prop_Navigator, itemType, clickEvents, ref dsRead);

                        pGroup.Name = "item_Group_" + refid;

                        //pGroup.Appearance.Normal.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
                        //pGroup.Appearance.Normal.ForeColor = System.Drawing.Color.
                        //pGroup.Appearance.Normal.Options.UseFont = true;
                        //pGroup.Appearance.Normal.Options.UseForeColor = true;
                        pGroup.Tag = Prop_Navigator;
                    }

                    group_count++;
                    mControl.Elements.Add(pGroup);
                }
                #endregion group

                #region item
                if ((t.IsNotNull(itemName) == false) &&
                    (tvisible) &&
                    ((itemType == 205) || (itemType == 206)))
                {
                    DevExpress.XtraToolbox.ToolboxItem barButtonItem = new DevExpress.XtraToolbox.ToolboxItem();

                    itemName = "item_" + refid;

                    if (clickEvents > 0)
                        Preparing_ButtonName(clickEvents, ref itemName);

                    barButtonItem.Name = itemName;
                    barButtonItem.Caption = itemCaption;
                    barButtonItem.Tag = Prop_Navigator;

                    //barButtonItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(ev.formRibbonMenu_ItemClick);

                    // image çalışıyor SİLME 
                    // button Image set

                    #region Image set
                    if (!DBNull.Value.Equals(ds_Items.Tables[0].Rows[i]["LKP_GLYPH16"]))
                    {
                        byte[] img16 = (byte[])ds_Items.Tables[0].Rows[i]["LKP_GLYPH16"];
                        if (img16 != null)
                        {
                            MemoryStream ms = new MemoryStream(img16);
                            barButtonItem.Image = Image.FromStream(ms);
                            ms.Dispose();
                        }
                    }

                    if (!DBNull.Value.Equals(ds_Items.Tables[0].Rows[i]["LKP_GLYPH32"]))
                    {
                        byte[] img32 = (byte[])ds_Items.Tables[0].Rows[i]["LKP_GLYPH32"];
                        if (img32 != null)
                        {
                            MemoryStream ms = new MemoryStream(img32);
                            barButtonItem.Image = Image.FromStream(ms);
                            ms.Dispose();
                        }
                    }
                    #endregion Image set


                    //mControl.Items.Add(barButtonItem);

                    UstHesapRow = UstHesap_Get(ds_Items, i);

                    if (UstHesapRow != null)
                    {
                        ustItemType = t.myInt32(UstHesapRow["ITEM_TYPE"].ToString());
                        ustHesapRefId = UstHesapRow["REF_ID"].ToString();
                        ustHesapItemName = UstHesapRow["ITEM_NAME"].ToString();

                        if (t.IsNotNull(ustHesapItemName)) ustItemName = ustHesapItemName;
                        else ustItemName = "item_" + ustHesapRefId;
                    }

                }
                #endregion
            }

            if (mControl != null)
            {
                mControl.ExpandElementMode = DevExpress.XtraBars.Navigation.ExpandElementMode.Multiple;
                mControl.ExpandAll();

                if (mControl.Elements != null)
                    if (mControl.Elements[0].Elements != null)
                        if (mControl.Elements[0].Elements.Count > 0)
                           ev.tAccordionDinamikElement_Click(mControl.Elements[0].Elements[0], EventArgs.Empty);
            }

        }
        #endregion
        
        #region Create_PopupMenu
        public void Create_PopupMenu(DevExpress.XtraBars.PopupMenu mControl, DataSet ds_Items)
        {

        }
        #endregion Create_PopupMenu

        #endregion Create Menu

        #region Create SubMenu

        private bool mainControlAdd(Control mainControl, Control menuControl)
        {
            if (mainControl.GetType().ToString() == "DevExpress.XtraBars.Navigation.NavigationPage")
            {
                menuControl.Dock = DockStyle.Fill;

                ((DevExpress.XtraBars.Navigation.NavigationPage)mainControl).Controls.Add(menuControl);

                return true;
            }

            if (mainControl.GetType().ToString() == "DevExpress.XtraBars.Navigation.TabNavigationPage")
            {
                menuControl.Dock = DockStyle.Fill;

                ((DevExpress.XtraBars.Navigation.TabNavigationPage)mainControl).Controls.Add(menuControl);

                return true;
            }

            if (mainControl.GetType().ToString() == "DevExpress.XtraTab.XtraTabPage")
            {
                menuControl.Dock = DockStyle.Fill;

                ((DevExpress.XtraTab.XtraTabPage)mainControl).Controls.Add(menuControl);

                return true;
            }

            return false;
        }

        public short getCreateMenuType(string MenuCode)
        {
            short ItemType = 109; // default

            tToolBox t = new tToolBox();
            tTablesRead tr = new tTablesRead();

            DataSet ds_Items = new DataSet();

            //tr.MS_LayoutOrItems_Read(ds_Items, MenuCode, 3);
            tr.MS_Menu_Read(ds_Items, MenuCode);

            if (t.IsNotNull(ds_Items) == false) return ItemType;

            ItemType = t.myInt16(ds_Items.Tables[0].Rows[0]["ITEM_TYPE"].ToString());

            return ItemType;
        }
        
        private DataRow UstHesap_Get(DataSet ds_Items, int pos)
        {
            int itemCount = ds_Items.Tables[0].Rows.Count;

            DataRow UstHesap = null;
            string ParentCode = ds_Items.Tables[0].Rows[pos]["PARENT_ITEM_CODE"].ToString();
            string ItemCode = string.Empty;

            if (ParentCode == "") return null;

            for (int i = 0; i < itemCount; i++)
            {
                ItemCode = ds_Items.Tables[0].Rows[i]["ITEM_CODE"].ToString();

                if (ParentCode == ItemCode)
                {
                    UstHesap = ds_Items.Tables[0].Rows[i] as DataRow;
                    break;
                }
            }

            return UstHesap;
        }

        private void Preparing_ButtonName(Int16 clickEvents, ref string buttonName)
        {
            if (clickEvents == 1) buttonName = buttonName + "_" + "FEXIT";
            if (clickEvents == 2) buttonName = buttonName + "_" + "FSAVE";
            if (clickEvents == 3) buttonName = buttonName + "_" + "FSAVENEW";
            if (clickEvents == 4) buttonName = buttonName + "_" + "FSAVEXIT";
            if (clickEvents == 5) buttonName = buttonName + "_" + "FNEW_IP";
            if (clickEvents == 6) buttonName = buttonName + "_" + "FOPEN_IP";
            if (clickEvents == 7) buttonName = buttonName + "_" + "APPEXIT";
            if (clickEvents == 8) buttonName = buttonName + "_" + "REPORTVIEW";
            if (clickEvents == 9) buttonName = buttonName + "_" + "SMS_SEND";
            if (clickEvents == 10) buttonName = buttonName + "_" + "SMS_SETUP";
            if (clickEvents == 13) buttonName = buttonName + "_" + "FOPEN_SUBVIEW";

            if (clickEvents == 21) buttonName = buttonName + "_" + "YILAY_BACK";
            if (clickEvents == 22) buttonName = buttonName + "_" + "YILAY_NEXT";

            if (clickEvents > 1000)
                buttonName = buttonName + "_" + "simpleButton_" + clickEvents;
        }

        private void toolBox_Preparing(
                Form tForm,
                Control tPanelControl,
                DevExpress.XtraToolbox.ToolboxControl menuControl,
                DevExpress.XtraToolbox.ToolboxGroup pGroup,
                string Prop_Navigator,
                int itemType,
                Int16 clickEvents,
                ref DataSet dsRead)
        {
            #region 
            /*
      0=ROW_PROP_NAVIGATOR:0;
      0=CAPTION:null;
      0=BUTTONTYPE:null;
      0=TABLEIPCODE_LIST:null;
      0=FORMNAME:null;
      0=FORMCODE:null;
      0=FORMTYPE:null;
      0=FORMSTATE:null;
      0=CHC_IPCODE:HPBNK.HPBNK_01;
      0=CHC_FNAME:LKP_FULL_NAME;
      0=CHC_VALUE:LKP_BANKSUBE_ID;
      0=ROWE_PROP_NAVIGATOR:0;
          */
            #endregion

            tToolBox t = new tToolBox();

            string Read_TableIPCode = string.Empty;
            string DetailFName = string.Empty;

            #region // ToolBox ilk satırı ise
            if (itemType == 109)
            {
                // tüp hesapların olduğu dataset
                Read_TableIPCode = t.MyProperties_Get(Prop_Navigator, "CHC_IPCODE:");

                // Ana Grup altındaki hesapların, alt detaylarına ulaşmak için işe yarayan field
                // ŞubeId ile Şubeye bağlı hesapların bağlantısı master-detail fname 
                DetailFName = t.MyProperties_Get(Prop_Navigator, "CHC_FNAME:");

                if (t.IsNotNull(Read_TableIPCode) == false)
                {
                    //MessageBox.Show("DİKKAT : Liste hazırlamak için gerekli Navigator DATA içindeki CHC_IPCODE bilgileri eksiklik mevcut ...");
                    return;
                }

                if (t.IsNotNull(DetailFName) == false)
                {
                    //MessageBox.Show("DİKKAT : Alt Detay hesaplara ulaşmak için gerekli olan Detail FieldName CHC_FNAME bilgileri eksiklik mevcut ...");
                    return;
                }

                if (tForm != null)
                {
                    tInputPanel ip = new tInputPanel();
                    ip.Create_InputPanel(tForm, tPanelControl, Read_TableIPCode, 1, true);

                    dsRead = ip.Create_DataSet(tForm, Read_TableIPCode, false);

                    menuControl.AccessibleName = Read_TableIPCode;
                    // Master-Detail FName
                    menuControl.AccessibleDescription = DetailFName;
                }
                return;
            }
            #endregion

            // Group bilgileri için 
            string read_CaptionFName = t.MyProperties_Get(Prop_Navigator, "RTABLEIPCODE:"); 
            string read_KeyFName = t.MyProperties_Get(Prop_Navigator, "RKEYFNAME:");  

            string chc_FName = t.MyProperties_Get(Prop_Navigator, "CHC_FNAME:");  
            string chc_Value = t.MyProperties_Get(Prop_Navigator, "CHC_VALUE:");  

            if ((t.IsNotNull(read_CaptionFName) == false) ||
                (t.IsNotNull(read_KeyFName) == false) ||
                (t.IsNotNull(chc_FName) == false) ||
                (t.IsNotNull(chc_Value) == false)
                )
            {
                MessageBox.Show("DİKKAT : Liste hazırlamak için gerekli Navigator içindeki RTABLEIPCODE, RKEYFNAME bilgilerinde eksiklik mevcut ...");
                return;
            }

            string itemCaption = string.Empty;
            string value = string.Empty;
            string itemName = string.Empty;

            //'CLICK_EVENTS', 11, '', 'TBOX_MAIN'
            //'CLICK_EVENTS', 12, '', 'TBOX_GROUP'

            #region 
            if ((clickEvents == 11))
            {
                if (t.IsNotNull(dsRead))
                {
                    foreach (DataRow row in dsRead.Tables[0].Rows)
                    {
                        itemCaption = row[read_CaptionFName].ToString();
                        value = row[read_KeyFName].ToString();

                        if (row[chc_FName].ToString() == chc_Value)
                        {
                            DevExpress.XtraToolbox.ToolboxItem barButtonItem = new DevExpress.XtraToolbox.ToolboxItem();

                            itemName = "item_" + value;// refid;

                            barButtonItem.Name = itemName;
                            barButtonItem.Caption = itemCaption;
                            barButtonItem.Tag = value;

                            barButtonItem.Appearance.Normal.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
                            barButtonItem.Appearance.Normal.ForeColor = System.Drawing.Color.Blue;
                            barButtonItem.Appearance.Normal.Options.UseFont = true;
                            barButtonItem.Appearance.Normal.Options.UseForeColor = true;

                            pGroup.Items.Add(barButtonItem);
                        }
                    } // foreach
                } // dsRead
            }
            #endregion

        }


        private void accordionDinamik_Preparing(
                Form tForm,
                Control tPanelControl,
                DevExpress.XtraBars.Navigation.AccordionControl menuControl,
                DevExpress.XtraBars.Navigation.AccordionControlElement pGroup,
                string Prop_Navigator,
                int itemType,
                Int16 clickEvents,
                ref DataSet dsRead)
        {
            string s1 = "=ROW_PROP_NAVIGATOR:";
            string s2 = (char)34 + "TABLEIPCODE_LIST" + (char)34 + ": [";
            // "TABLEIPCODE_LIST": [

            string s3 = "=TABLEIPCODE_LIST:";
            string s4 = "=FORMCODE:";

            if ((Prop_Navigator.IndexOf(s1) > -1) ||
                (Prop_Navigator.IndexOf(s3) > -1) ||
                (Prop_Navigator.IndexOf(s4) > -1))
            {
                accordionDinamik_Preparing_OLD(
                  tForm,
                  tPanelControl,
                  menuControl,
                  pGroup,
                  Prop_Navigator,
                  itemType,
                  clickEvents,
                  ref dsRead);
            }
            else if (Prop_Navigator.IndexOf(s2) > -1)
            {
                if (((Prop_Navigator.IndexOf("[") > -1) &&
                     (Prop_Navigator.IndexOf("[") < 5)) == true)
                {
                    Prop_Navigator = Prop_Navigator.Remove(Prop_Navigator.IndexOf("["), 1);

                    Prop_Navigator = Prop_Navigator.Remove(Prop_Navigator.IndexOf("]", (Prop_Navigator.Length - 5)), 1);

                }

                //var prop_ = JsonConvert.DeserializeObject(Prop_Navigator);
                /*
                PROP_NAVIGATOR packet = new PROP_NAVIGATOR();
                Prop_Navigator = Prop_Navigator.Replace((char)34, (char)39);
                //var prop_ = JsonConvert.DeserializeAnonymousType(Prop_Navigator, packet);
                */
                tToolBox t = new tToolBox();
                PROP_NAVIGATOR prop_ = t.readProp<PROP_NAVIGATOR>(Prop_Navigator);

                accordionDinamik_Preparing_JSON(
                  tForm,
                  tPanelControl,
                  menuControl,
                  pGroup,
                  (PROP_NAVIGATOR)prop_,
                  itemType,
                  clickEvents,
                  ref dsRead);
            }
            else
            {
                MessageBox.Show("DİKKAT : Tanımlayamadığım bir PROP_NAVIGATOR cümlesi geldi...");
            }
        }

        private void accordionDinamik_Preparing_OLD(
                Form tForm,
                Control tPanelControl,
                DevExpress.XtraBars.Navigation.AccordionControl menuControl,
                DevExpress.XtraBars.Navigation.AccordionControlElement pGroup,
                string Prop_Navigator,
                int itemType,
                Int16 clickEvents,
                ref DataSet dsRead)
        {
            #region 
            /*
      0=ROW_PROP_NAVIGATOR:0;
      0=CAPTION:null;
      0=BUTTONTYPE:null;
      0=TABLEIPCODE_LIST:null;
      0=FORMNAME:null;
      0=FORMCODE:null;
      0=FORMTYPE:null;
      0=FORMSTATE:null;
      0=CHC_IPCODE:HPBNK.HPBNK_01;
      0=CHC_FNAME:LKP_FULL_NAME;
      0=CHC_VALUE:LKP_BANKSUBE_ID;
      0=CHC_OPERAND:=;
      0=ROWE_PROP_NAVIGATOR:0;
          */
            #endregion

            tToolBox t = new tToolBox();
            tEvents ev = new tEvents();

            string Read_TableIPCode = string.Empty;
            string DetailFName = string.Empty;

            #region // AccordionDinamik ilk satırı ise
            if (itemType == 110)
            {
                // tüp hesapların olduğu dataset
                Read_TableIPCode = t.MyProperties_Get(Prop_Navigator, "CHC_IPCODE:");

                // Ana Grup altındaki hesapların, alt detaylarına ulaşmak için işe yarayan field
                // ŞubeId ile Şubeye bağlı hesapların bağlantısı master-detail fname 
                DetailFName = t.MyProperties_Get(Prop_Navigator, "CHC_FNAME:");

                if (t.IsNotNull(Read_TableIPCode) == false)
                {
                    MessageBox.Show("DİKKAT : Liste hazırlamak için gerekli Navigator DATA içindeki CHC_IPCODE bilgileri eksiklik mevcut ...");
                    return;
                }

                if (t.IsNotNull(DetailFName) == false)
                {
                    MessageBox.Show("DİKKAT : Alt Detay hesaplara ulaşmak için gerekli olan Detail FieldName CHC_FNAME bilgileri eksiklik mevcut ...");
                    return;
                }

                if (tForm != null)
                {
                    tInputPanel ip = new tInputPanel();
                    ip.Create_InputPanel(tForm, tPanelControl, Read_TableIPCode, 1, true);

                    dsRead = ip.Create_DataSet(tForm, Read_TableIPCode, false);

                    menuControl.AccessibleName = Read_TableIPCode;
                    // Master-Detail FName
                    menuControl.AccessibleDescription = DetailFName;
                }
                return;
            }
            #endregion

            // Group bilgileri için 
            string read_CaptionFName = t.MyProperties_Get(Prop_Navigator, "RTABLEIPCODE:"); // esasında okunan display edilecek field
            string read_KeyFName = t.MyProperties_Get(Prop_Navigator, "RKEYFNAME:");  // esasında okunan ref Id field

            string chc_FName = t.MyProperties_Get(Prop_Navigator, "CHC_FNAME:");  // esasında okunan check field name
            string chc_Value = t.MyProperties_Get(Prop_Navigator, "CHC_VALUE:");  // esasında okunan check value field name
            string chc_Operand = t.MyProperties_Get(Prop_Navigator, "CHC_OPERAND:");

            if ((t.IsNotNull(read_CaptionFName) == false) ||
                (t.IsNotNull(read_KeyFName) == false) ||
                (t.IsNotNull(chc_FName) == false) ||
                (t.IsNotNull(chc_Value) == false)
                )
            {
                MessageBox.Show("DİKKAT : Liste hazırlamak için gerekli Navigator içindeki RTABLEIPCODE, RKEYFNAME bilgilerinde eksiklik mevcut ...");
                return;
            }

            string itemCaption = string.Empty;
            string value = string.Empty;
            string itemName = string.Empty;

            //'CLICK_EVENTS', 11, '', 'TBOX_MAIN'
            //'CLICK_EVENTS', 12, '', 'TBOX_GROUP'

            #region 
            if ((clickEvents == 11))
            {
                if (t.IsNotNull(dsRead))
                {
                    foreach (DataRow row in dsRead.Tables[0].Rows)
                    {
                        itemCaption = row[read_CaptionFName].ToString();
                        value = row[read_KeyFName].ToString();

                        //if (row[chc_FName].ToString() == chc_Value)
                        if (t.myOperandControl(row[chc_FName].ToString(), chc_Value, chc_Operand))
                        {
                            //DevExpress.XtraToolbox.ToolboxItem barButtonItem = new DevExpress.XtraToolbox.ToolboxItem();
                            DevExpress.XtraBars.Navigation.AccordionControlElement barButtonItem =
                               new DevExpress.XtraBars.Navigation.AccordionControlElement();

                            itemName = "item_MainItem_" + value;// refid;

                            barButtonItem.Name = itemName;
                            barButtonItem.Text = itemCaption;
                            barButtonItem.Tag = value;
                            barButtonItem.Hint = chc_Value;

                            barButtonItem.Appearance.Normal.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
                            //barButtonItem.Appearance.Normal.ForeColor =    ///System.Drawing.Color.DarkBlue;
                            barButtonItem.Appearance.Normal.Options.UseFont = true;
                            barButtonItem.Appearance.Normal.Options.UseForeColor = true;

                            barButtonItem.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
                            barButtonItem.Click += new System.EventHandler(ev.tAccordionDinamikElement_Click);

                            pGroup.Elements.Add(barButtonItem);

                        }
                    } // foreach
                } // dsRead
            }
            #endregion

            #region
            /*

0=ROW_PROP_NAVIGATOR:0;
0=CAPTION:null;
0=BUTTONTYPE:null;
0=TABLEIPCODE_LIST:TABLEIPCODE_LIST={
1=ROW_TABLEIPCODE_LIST:1;
1=CAPTION:Main Group Bilgileri;
1=TABLEIPCODE:null;
1=TABLEALIAS:null;
1=KEYFNAME:null;
1=RTABLEIPCODE:LKP_FULL_NAME;
1=RKEYFNAME:LKP_REF_ID;
1=MSETVALUE:null;
1=WORKTYPE:null;
1=CONTROLNAME:null;
1=ROWE_TABLEIPCODE_LIST:1;
TABLEIPCODE_LIST=};
0=FORMNAME:null;
0=FORMCODE:null;
0=FORMTYPE:null;
0=FORMSTATE:null;
0=CHC_IPCODE:HPBNK.HPBNK_02;
0=CHC_FNAME:LKP_FNS_TIPI;
0=CHC_VALUE:2010;
0=CHC_OPERAND:=;
0=ROWE_PROP_NAVIGATOR:0;

            */
            #endregion
        }

        private void accordionDinamik_Preparing_JSON(
                Form tForm,
                Control tPanelControl,
                DevExpress.XtraBars.Navigation.AccordionControl menuControl,
                DevExpress.XtraBars.Navigation.AccordionControlElement pGroup,
                PROP_NAVIGATOR prop_,
                int itemType,
                Int16 clickEvents,
                ref DataSet dsRead)
        {
            #region 
            /*
      [
  {
    "CAPTION": "kaynak TableIPCode",
    "BUTTONTYPE": "null",
    "TABLEIPCODE_LIST": [],
    "FORMNAME": "null",
    "FORMCODE": "null",
    "FORMTYPE": "null",
    "FORMSTATE": "null",
    "CHC_IPCODE": "HPBNK.HPBNK_02",
    "CHC_FNAME": "LKP_BANKSUBE_ID",
    "CHC_VALUE": "null",
    "CHC_OPERAND": "null"
  }
]
          */
            #endregion

            tToolBox t = new tToolBox();
            tEvents ev = new tEvents();

            string Read_TableIPCode = string.Empty;
            string GroupFName = string.Empty;
            string DetailFName = string.Empty;

            #region // AccordionDinamik ilk satırı ise
            if (itemType == 110)
            {
                // tüp hesapların olduğu dataset
                Read_TableIPCode = prop_.CHC_IPCODE;

                // Ana Grup altındaki hesapların, alt detaylarına ulaşmak için işe yarayan field
                // ŞubeId ile Şubeye bağlı hesapların bağlantısı master-detail fname 
                GroupFName = prop_.CHC_FNAME;
                DetailFName = prop_.CHC_FNAME_SEC;

                if (t.IsNotNull(Read_TableIPCode) == false)
                {
                    MessageBox.Show("DİKKAT : Liste hazırlamak için gerekli Navigator DATA içindeki CHC_IPCODE bilgileri eksiklik mevcut ...");
                    return;
                }

                if (t.IsNotNull(GroupFName) == false)
                {
                    MessageBox.Show("DİKKAT : Alt Detay hesaplara ulaşmak için gerekli olan Group FieldName CHC_FNAME (VezneId) bilgileri eksiklik mevcut ...");
                    return;
                }

                if (t.IsNotNull(DetailFName) == false)
                {
                    MessageBox.Show("DİKKAT : Alt Detay hesaplara ulaşmak için gerekli olan Detail FieldName CHC_FNAME_SEC (FinansId) bilgileri eksiklik mevcut ...");
                    return;
                }

                if (tForm != null)
                {
                    tInputPanel ip = new tInputPanel();

                    // DENEME İÇİN KAPATTIM GEREKSİZ GALİBA  07.04.2018
                    // .....
                    // demek ki gereksiz değilmiş bir daha kapatma 21.04.2018
                    // çünkü daha sonra Read_TableIPCode ile Find_DataSet çalıştığında bulamıyor
                    //
                    ip.Create_InputPanel(tForm, tPanelControl, Read_TableIPCode, 1, true);

                    //dsRead = ip.Create_DataSet(tForm, Read_TableIPCode);

                    //DataSet dsRead = null;
                    DevExpress.XtraEditors.DataNavigator tdN_Read = null;

                    t.Find_DataSet(tForm, ref dsRead, ref tdN_Read, Read_TableIPCode);

                    menuControl.AccessibleName = Read_TableIPCode;
                    // Master-Detail FName
                    menuControl.AccessibleDescription = GroupFName + "||"+ DetailFName + "||";
                }
                return;
            }
            #endregion

            // Group bilgileri için 
            string read_CaptionFName = string.Empty; 
            string read_KeyFName = string.Empty; 

            string chc_FName = string.Empty; 
            string chc_Value = string.Empty; 
            string chc_Operand = string.Empty; 

            foreach (var item in prop_.TABLEIPCODE_LIST)
            {
                read_CaptionFName = item.RTABLEIPCODE;
                read_KeyFName = item.RKEYFNAME;
                chc_FName = prop_.CHC_FNAME;
                chc_Value = prop_.CHC_VALUE;
                chc_Operand = prop_.CHC_OPERAND;
            }


            if ((t.IsNotNull(read_CaptionFName) == false) ||
                (t.IsNotNull(read_KeyFName) == false) ||
                (t.IsNotNull(chc_FName) == false) ||
                (t.IsNotNull(chc_Value) == false)
                )
            {
                MessageBox.Show("DİKKAT : Liste hazırlamak için gerekli Navigator içindeki RTABLEIPCODE, RKEYFNAME, CHC_FNAME, CHC_VALUE bilgilerinde eksiklik mevcut ...");
                return;
            }

            string itemCaption = string.Empty;
            string value = string.Empty;
            string itemName = string.Empty;

            //'CLICK_EVENTS', 11, '', 'TBOX_MAIN'
            //'CLICK_EVENTS', 12, '', 'TBOX_GROUP'

            #region 
            if ((clickEvents == 11))
            {
                if (t.IsNotNull(dsRead))
                {
                    foreach (DataRow row in dsRead.Tables[0].Rows)
                    {
                        itemCaption = row[read_CaptionFName].ToString();
                        value = row[read_KeyFName].ToString();

                        //if (row[chc_FName].ToString() == chc_Value)
                        if (t.myOperandControl(row[chc_FName].ToString(), chc_Value, chc_Operand))
                        {
                            //DevExpress.XtraToolbox.ToolboxItem barButtonItem = new DevExpress.XtraToolbox.ToolboxItem();
                            DevExpress.XtraBars.Navigation.AccordionControlElement barButtonItem =
                               new DevExpress.XtraBars.Navigation.AccordionControlElement();

                            itemName = "item_MainItem_" + value;// refid;

                            barButtonItem.Name = itemName;
                            barButtonItem.Text = itemCaption;
                            barButtonItem.Tag = value;
                            barButtonItem.Hint = chc_Value;

                            barButtonItem.Appearance.Normal.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
                            barButtonItem.Appearance.Normal.ForeColor = System.Drawing.Color.Blue;
                            barButtonItem.Appearance.Normal.Options.UseFont = true;
                            barButtonItem.Appearance.Normal.Options.UseForeColor = true;

                            barButtonItem.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item;
                            barButtonItem.Click += new System.EventHandler(ev.tAccordionDinamikElement_Click);

                            pGroup.Elements.Add(barButtonItem);

                        }
                    } // foreach
                } // dsRead
            }
            #endregion

        }



        #endregion Create SubMenu


    }
}
