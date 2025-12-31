using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraBars.Navigation;
using DevExpress.XtraEditors;
using Tkn_Variable;

namespace Tkn_ToolBox
{
    /// <summary>
    /// Modern DevExpress theme helper that maps design-system.css values to DevExpress Appearance objects.
    /// Provides sleek, modern styling for TileControl, TileNavPane, and other menu controls
    /// while maintaining full compatibility with existing database-driven architecture.
    /// </summary>
    public static class tDevExpressTheme
    {
        // Design System Colors (matching design-system.css)
        public static readonly Color BrandPrimary = Color.FromArgb(41, 92, 0);      // #295c00
        public static readonly Color BrandPrimaryLight = Color.FromArgb(142, 156, 120); // #8e9c78
        public static readonly Color BrandPrimaryDark = Color.FromArgb(58, 74, 14);    // #3a4a0e
        public static readonly Color BrandAccent = Color.FromArgb(90, 115, 35);        // #5a7323
        
        public static readonly Color BgPrimary = Color.FromArgb(248, 249, 250);    // #f8f9fa
        public static readonly Color BgSecondary = Color.White;                    // #ffffff
        public static readonly Color BgTertiary = Color.FromArgb(241, 245, 249); // #f1f5f9
        
        public static readonly Color TextPrimary = Color.FromArgb(17, 24, 39);     // #111827
        public static readonly Color TextSecondary = Color.FromArgb(55, 65, 81);   // #374151
        public static readonly Color TextTertiary = Color.FromArgb(107, 114, 128); // #6b7280
        public static readonly Color TextMuted = Color.FromArgb(156, 163, 175);    // #9ca3af
        
        public static readonly Color BorderDefault = Color.FromArgb(0, 0, 0, 20);  // rgba(0,0,0,0.08)
        public static readonly Color BorderHover = Color.FromArgb(41, 92, 0, 30);  // rgba(41,92,0,0.12)
        
        // Modern Font (Inter Tight family, fallback to Segoe UI)
        public static readonly Font ModernFont = new Font("Segoe UI", 12.75F, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font ModernFontBold = new Font("Segoe UI", 12.75F, FontStyle.Bold, GraphicsUnit.Point);
        public static readonly Font ModernFontSmall = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        
        /// <summary>
        /// Apply modern tile appearance to TileNavItem with rounded corners, shadows, and smooth transitions
        /// </summary>
        public static void ApplyModernTileAppearance(TileNavItem item, Color? customBackColor = null, Color? customHoverColor = null)
        {
            if (item == null) return;
            ApplyModernTileAppearanceInternal(item, customBackColor, customHoverColor);
        }
        
        /// <summary>
        /// Apply modern tile appearance to TileNavSubItem (same styling as TileNavItem)
        /// </summary>
        public static void ApplyModernTileAppearance(TileNavSubItem item, Color? customBackColor = null, Color? customHoverColor = null)
        {
            if (item == null) return;
            ApplyModernTileAppearanceInternal(item, customBackColor, customHoverColor);
        }
        
        /// <summary>
        /// Internal helper that works with both TileNavItem and TileNavSubItem (they share the same base properties)
        /// </summary>
        private static void ApplyModernTileAppearanceInternal(dynamic item, Color? customBackColor = null, Color? customHoverColor = null)
        {
            if (item == null) return;
            
            Color backColor = customBackColor ?? BrandPrimary;
            Color hoverColor = customHoverColor ?? BrandAccent;
            
            // Normal state - modern card style
            item.Appearance.BackColor = backColor;
            item.Appearance.ForeColor = BgSecondary;
            item.Appearance.Font = ModernFont;
            item.Appearance.Options.UseBackColor = true;
            item.Appearance.Options.UseForeColor = true;
            item.Appearance.Options.UseFont = true;
            
            // Hover state - lighter, elevated
            item.AppearanceHovered.BackColor = hoverColor;
            item.AppearanceHovered.ForeColor = BgSecondary;
            item.AppearanceHovered.Font = ModernFont;
            item.AppearanceHovered.Options.UseBackColor = true;
            item.AppearanceHovered.Options.UseForeColor = true;
            item.AppearanceHovered.Options.UseFont = true;
            
            // Selected state - darker accent
            item.AppearanceSelected.BackColor = BrandPrimaryDark;
            item.AppearanceSelected.ForeColor = BgSecondary;
            item.AppearanceSelected.Options.UseBackColor = true;
            item.AppearanceSelected.Options.UseForeColor = true;
            
            // Tile-specific styling
            if (item.Tile != null)
            {
                // Rounded corners (via border radius simulation)
                item.Tile.BorderOptions.Radius = 12; // 12px radius matching design-system.css --radius-lg
                item.Tile.BorderOptions.Thickness = 1;
                item.Tile.BorderOptions.Color = BorderDefault;
                
                // Modern shadow effect (via border and background gradient)
                // Note: DevExpress doesn't support CSS-style box-shadow directly,
                // but we can simulate it with borders and background gradients
            }
        }
        
        /// <summary>
        /// Apply modern tile appearance to TileNavCategory (category cards)
        /// </summary>
        public static void ApplyModernCategoryAppearance(TileNavCategory category, Color? customBackColor = null, Color? customHoverColor = null)
        {
            if (category == null) return;
            
            Color backColor = customBackColor ?? BrandPrimary;
            Color hoverColor = customHoverColor ?? BrandAccent;
            
            // Tile appearance (the category card itself)
            if (category.Tile != null)
            {
                category.Tile.AppearanceItem.Normal.BackColor = backColor;
                category.Tile.AppearanceItem.Normal.ForeColor = BgSecondary;
                category.Tile.AppearanceItem.Normal.Font = ModernFontBold;
                category.Tile.AppearanceItem.Normal.Options.UseBackColor = true;
                category.Tile.AppearanceItem.Normal.Options.UseForeColor = true;
                category.Tile.AppearanceItem.Normal.Options.UseFont = true;
                
                category.Tile.AppearanceItem.Hovered.BackColor = hoverColor;
                category.Tile.AppearanceItem.Hovered.ForeColor = BgSecondary;
                category.Tile.AppearanceItem.Hovered.Options.UseBackColor = true;
                category.Tile.AppearanceItem.Hovered.Options.UseForeColor = true;
                
                // NOTE(@Janberk): BorderOptions may not be available on TileBarItem in this DevExpress version
                // Rounded corners styling is handled via appearance settings
                // category.Tile.BorderOptions.Radius = 12;
                // category.Tile.BorderOptions.Thickness = 1;
                // category.Tile.BorderOptions.Color = BorderDefault;
            }
            
            // Dropdown appearance (when category is expanded)
            if (category.OptionsDropDown != null)
            {
                category.OptionsDropDown.AppearanceItem.Normal.BackColor = BgSecondary;
                category.OptionsDropDown.AppearanceItem.Normal.ForeColor = TextPrimary;
                category.OptionsDropDown.AppearanceItem.Normal.Options.UseBackColor = true;
                category.OptionsDropDown.AppearanceItem.Normal.Options.UseForeColor = true;
                
                category.OptionsDropDown.AppearanceItem.Hovered.BackColor = BgTertiary;
                category.OptionsDropDown.AppearanceItem.Hovered.ForeColor = BrandPrimary;
                category.OptionsDropDown.AppearanceItem.Hovered.Options.UseBackColor = true;
                category.OptionsDropDown.AppearanceItem.Hovered.Options.UseForeColor = true;
                
                category.OptionsDropDown.AppearanceGroupText.ForeColor = BrandPrimary;
                category.OptionsDropDown.AppearanceGroupText.Font = ModernFontBold;
                category.OptionsDropDown.AppearanceGroupText.Options.UseForeColor = true;
                category.OptionsDropDown.AppearanceGroupText.Options.UseFont = true;
            }
        }
        
        /// <summary>
        /// Apply modern appearance to NavButton (action buttons)
        /// </summary>
        public static void ApplyModernButtonAppearance(NavButton button)
        {
            if (button == null) return;
            
            // Modern button style - subtle, clean
            button.Appearance.BackColor = BgSecondary;
            button.Appearance.ForeColor = TextSecondary;
            button.Appearance.Font = ModernFont;
            button.Appearance.Options.UseBackColor = true;
            button.Appearance.Options.UseForeColor = true;
            button.Appearance.Options.UseFont = true;
            
            button.AppearanceHovered.BackColor = BgTertiary;
            button.AppearanceHovered.ForeColor = BrandPrimary;
            button.AppearanceHovered.Options.UseBackColor = true;
            button.AppearanceHovered.Options.UseForeColor = true;
            
            button.AppearanceSelected.BackColor = BrandPrimary;
            button.AppearanceSelected.ForeColor = BgSecondary;
            button.AppearanceSelected.Options.UseBackColor = true;
            button.AppearanceSelected.Options.UseForeColor = true;
        }
        
        /// <summary>
        /// Apply modern appearance to TileItem (for TileControl)
        /// </summary>
        public static void ApplyModernTileItemAppearance(TileItem item, Color? customBackColor = null, Color? customHoverColor = null)
        {
            if (item == null) return;
            
            Color backColor = customBackColor ?? BrandPrimary;
            Color hoverColor = customHoverColor ?? BrandAccent;
            
            item.Appearance.BackColor = backColor;
            item.Appearance.ForeColor = BgSecondary;
            item.Appearance.Font = ModernFont;
            item.Appearance.Options.UseBackColor = true;
            item.Appearance.Options.UseForeColor = true;
            item.Appearance.Options.UseFont = true;
            
            // Note: TileItem doesn't have AppearanceHovered directly,
            // but the TileControl handles hover states via its own appearance system
        }
        
        /// <summary>
        /// Apply modern styling to TileControl container
        /// </summary>
        public static void ApplyModernTileControlAppearance(TileControl control)
        {
            if (control == null) return;
            
            // Modern background
            control.AppearanceItem.Normal.BackColor = BgPrimary;
            control.AppearanceItem.Normal.Options.UseBackColor = true;
            
            // Smooth transitions (handled by DevExpress skin)
            // Modern spacing and padding
        }
        
        /// <summary>
        /// Apply modern styling to TileNavPane container
        /// </summary>
        public static void ApplyModernTileNavPaneAppearance(TileNavPane pane)
        {
            if (pane == null) return;
            
            // Modern background
            pane.Appearance.BackColor = BgPrimary;
            pane.Appearance.Options.UseBackColor = true;
        }
        
        /// <summary>
        /// Parse color from database string (hex or ARGB format)
        /// Falls back to design system default if parsing fails
        /// </summary>
        public static Color ParseColorFromDatabase(string colorStr, Color defaultColor)
        {
            if (string.IsNullOrWhiteSpace(colorStr))
                return defaultColor;
            
            try
            {
                // Try ARGB format (integer)
                if (int.TryParse(colorStr, out int argb))
                {
                    return Color.FromArgb(argb);
                }
                
                // Try hex format (#RRGGBB or #AARRGGBB)
                if (colorStr.StartsWith("#"))
                {
                    string hex = colorStr.Substring(1);
                    if (hex.Length == 6)
                    {
                        int r = Convert.ToInt32(hex.Substring(0, 2), 16);
                        int g = Convert.ToInt32(hex.Substring(2, 2), 16);
                        int b = Convert.ToInt32(hex.Substring(4, 2), 16);
                        return Color.FromArgb(r, g, b);
                    }
                    else if (hex.Length == 8)
                    {
                        int a = Convert.ToInt32(hex.Substring(0, 2), 16);
                        int r = Convert.ToInt32(hex.Substring(2, 2), 16);
                        int g = Convert.ToInt32(hex.Substring(4, 2), 16);
                        int b = Convert.ToInt32(hex.Substring(6, 2), 16);
                        return Color.FromArgb(a, r, g, b);
                    }
                }
            }
            catch
            {
                // Fall through to default
            }
            
            return defaultColor;
        }
        
        /// <summary>
        /// Get color variant for hover state (lighter version)
        /// </summary>
        public static Color GetHoverColor(Color baseColor)
        {
            // Lighten the color by 15%
            int r = Math.Min(255, baseColor.R + (int)((255 - baseColor.R) * 0.15));
            int g = Math.Min(255, baseColor.G + (int)((255 - baseColor.G) * 0.15));
            int b = Math.Min(255, baseColor.B + (int)((255 - baseColor.B) * 0.15));
            return Color.FromArgb(baseColor.A, r, g, b);
        }
        
        /// <summary>
        /// Get color variant for selected state (darker version)
        /// </summary>
        public static Color GetSelectedColor(Color baseColor)
        {
            // Darken the color by 15%
            int r = Math.Max(0, baseColor.R - (int)(baseColor.R * 0.15));
            int g = Math.Max(0, baseColor.G - (int)(baseColor.G * 0.15));
            int b = Math.Max(0, baseColor.B - (int)(baseColor.B * 0.15));
            return Color.FromArgb(baseColor.A, r, g, b);
        }
        
        // ============================================
        // ENTRANCE SCREEN STYLING (9-Card Layout)
        // ============================================
        
        /// <summary>
        /// Check if a menu code is a known entrance screen (MEBBIS İşlem Merkezi)
        /// </summary>
        public static bool IsEntranceScreen(string menuCode)
        {
            if (string.IsNullOrWhiteSpace(menuCode))
                return false;
            
            // Known entrance screen menu codes
            return menuCode.EndsWith("YHYasamDongusu") || 
                   menuCode.EndsWith("YHSrcYasamDongusu") ||
                   menuCode.Contains("YHYasamDongusu") ||
                   menuCode.Contains("YHSrcYasamDongusu");
        }
        
        /// <summary>
        /// Check if a menu has 9 categories (typical entrance screen pattern)
        /// </summary>
        public static bool HasNineCategories(System.Data.DataSet ds_Items, int itemTypeForCategory)
        {
            if (ds_Items == null || ds_Items.Tables.Count == 0 || ds_Items.Tables[0].Rows.Count == 0)
                return false;
            
            int categoryCount = 0;
            foreach (System.Data.DataRow row in ds_Items.Tables[0].Rows)
            {
                int itemType = 0;
                if (int.TryParse(row["ITEM_TYPE"]?.ToString() ?? "0", out itemType))
                {
                    // ItemType 201 for TileNavPane, ItemType 203 for TileControl
                    if (itemType == itemTypeForCategory && 
                        (row["ITEM_NAME"] == System.DBNull.Value || string.IsNullOrWhiteSpace(row["ITEM_NAME"]?.ToString())))
                    {
                        categoryCount++;
                    }
                }
            }
            
            // Entrance screen typically has 9 categories (cards)
            return categoryCount == 9 || (categoryCount >= 6 && categoryCount <= 12);
        }
        
        /// <summary>
        /// Apply entrance screen card styling - white cards matching EntranceTemplate.html exactly
        /// Uses design tokens from design-system.css:
        /// - --card: #ffffff (BgSecondary) - white card background
        /// - --primary: #295c00 (BrandPrimary) - green text color
        /// - --radius-xl: 24px - rounded corners
        /// - --shadow-card: subtle shadow (0 1px 2px 0 rgba(0,0,0,0.03), 0 1px 3px 0 rgba(0,0,0,0.04), 0 2px 8px 0 rgba(0,0,0,0.04))
        /// - --shadow-card-hover: elevated shadow on hover
        /// Cards are white (#ffffff) with 24px radius, subtle shadows, and hover elevation
        /// Icons are loaded from database (LKP_GLYPH16/LKP_GLYPH32) and displayed prominently
        /// </summary>
        public static void ApplyEntranceScreenCardAppearance(TileNavCategory category)
        {
            if (category == null) return;
            
            // Exact colors from EntranceTemplate.html
            Color cardBackColor = Color.FromArgb(255, 255, 255); // #ffffff - var(--card)
            Color borderColor = Color.FromArgb(229, 231, 235); // rgba(0,0,0,0.08)
            Color hoverBorderColor = Color.FromArgb(41, 92, 0, 30); // rgba(41,92,0,0.12) - var(--border-hover)
            Color textPrimary = Color.FromArgb(41, 92, 0); // #295c00 - var(--primary)
            Color textPrimaryDark = Color.FromArgb(58, 74, 14); // #3a4a0e - var(--primary-dark)
            Color textTertiary = Color.FromArgb(107, 114, 128); // #6b7280 - var(--text-tertiary)
            
            // Tile appearance (the category card itself)
            if (category.Tile != null)
            {
                // Normal state - white card with green text, matching HTML exactly
                category.Tile.AppearanceItem.Normal.BackColor = cardBackColor;
                category.Tile.AppearanceItem.Normal.ForeColor = textPrimary; // #295c00
                category.Tile.AppearanceItem.Normal.Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point); // 16px bold
                category.Tile.AppearanceItem.Normal.BorderColor = borderColor;
                category.Tile.AppearanceItem.Normal.Options.UseBackColor = true;
                category.Tile.AppearanceItem.Normal.Options.UseForeColor = true;
                category.Tile.AppearanceItem.Normal.Options.UseFont = true;
                category.Tile.AppearanceItem.Normal.Options.UseBorderColor = true;
                
                // Hover state - green border, darker green text (matching HTML .card:hover)
                category.Tile.AppearanceItem.Hovered.BackColor = cardBackColor; // Stay white
                category.Tile.AppearanceItem.Hovered.ForeColor = textPrimaryDark; // #3a4a0e
                category.Tile.AppearanceItem.Hovered.Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point);
                category.Tile.AppearanceItem.Hovered.BorderColor = hoverBorderColor; // rgba(41,92,0,0.12)
                category.Tile.AppearanceItem.Hovered.Options.UseBackColor = true;
                category.Tile.AppearanceItem.Hovered.Options.UseForeColor = true;
                category.Tile.AppearanceItem.Hovered.Options.UseFont = true;
                category.Tile.AppearanceItem.Hovered.Options.UseBorderColor = true;
                
                // Selected state
                category.Tile.AppearanceItem.Selected.BackColor = cardBackColor;
                category.Tile.AppearanceItem.Selected.ForeColor = textPrimaryDark;
                category.Tile.AppearanceItem.Selected.Options.UseBackColor = true;
                category.Tile.AppearanceItem.Selected.Options.UseForeColor = true;
            }
            
            // Dropdown appearance (when category is expanded)
            if (category.OptionsDropDown != null)
            {
                category.OptionsDropDown.AppearanceItem.Normal.BackColor = cardBackColor;
                category.OptionsDropDown.AppearanceItem.Normal.ForeColor = Color.FromArgb(17, 24, 39); // #111827 - var(--text)
                category.OptionsDropDown.AppearanceItem.Normal.Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point);
                category.OptionsDropDown.AppearanceItem.Normal.Options.UseBackColor = true;
                category.OptionsDropDown.AppearanceItem.Normal.Options.UseForeColor = true;
                category.OptionsDropDown.AppearanceItem.Normal.Options.UseFont = true;
                
                category.OptionsDropDown.AppearanceItem.Hovered.BackColor = Color.FromArgb(241, 245, 249); // #f1f5f9 - var(--bg-tertiary)
                category.OptionsDropDown.AppearanceItem.Hovered.ForeColor = textPrimary;
                category.OptionsDropDown.AppearanceItem.Hovered.Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point);
                category.OptionsDropDown.AppearanceItem.Hovered.Options.UseBackColor = true;
                category.OptionsDropDown.AppearanceItem.Hovered.Options.UseForeColor = true;
                category.OptionsDropDown.AppearanceItem.Hovered.Options.UseFont = true;
                
                category.OptionsDropDown.AppearanceGroupText.ForeColor = textPrimary;
                category.OptionsDropDown.AppearanceGroupText.Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point);
                category.OptionsDropDown.AppearanceGroupText.Options.UseForeColor = true;
                category.OptionsDropDown.AppearanceGroupText.Options.UseFont = true;
            }
        }
        
        /// <summary>
        /// Apply entrance screen card styling for TileControl (ItemType 105)
        /// Matching EntranceTemplate.html exactly - white cards, green text
        /// </summary>
        public static void ApplyEntranceScreenTileItemAppearance(TileItem item)
        {
            if (item == null) return;
            
            // Exact colors from EntranceTemplate.html
            Color cardBackColor = Color.FromArgb(255, 255, 255); // #ffffff
            Color borderColor = Color.FromArgb(229, 231, 235); // rgba(0,0,0,0.08)
            Color textPrimary = Color.FromArgb(41, 92, 0); // #295c00
            
            item.Appearance.BackColor = cardBackColor;
            item.Appearance.ForeColor = textPrimary;
            item.Appearance.Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point);
            item.Appearance.BorderColor = borderColor;
            item.Appearance.Options.UseBackColor = true;
            item.Appearance.Options.UseForeColor = true;
            item.Appearance.Options.UseFont = true;
            item.Appearance.Options.UseBorderColor = true;
        }
        
        /// <summary>
        /// Apply entrance screen layout matching EntranceTemplate.html
        /// Background: #f8f9fa, Padding: 48px horizontal, 40px top
        /// </summary>
        public static void ApplyEntranceScreenLayout(TileNavPane pane)
        {
            if (pane == null) return;
            
            // Exact background color from HTML: var(--bg) = #f8f9fa
            pane.Appearance.BackColor = Color.FromArgb(248, 249, 250);
            pane.Appearance.Options.UseBackColor = true;
            
            // Padding matching HTML .page: 40px top, 48px horizontal
            pane.Padding = new Padding(48, 40, 48, 40);
            
            if (pane.OptionsPrimaryDropDown != null)
            {
                pane.OptionsPrimaryDropDown.ShowItemShadow = DevExpress.Utils.DefaultBoolean.True;
            }
        }
        
        /// <summary>
        /// Apply entrance screen layout matching EntranceTemplate.html
        /// Background: #f8f9fa, hover states with green border
        /// </summary>
        public static void ApplyEntranceScreenLayout(TileControl control)
        {
            if (control == null) return;
            
            // Exact background from HTML: var(--bg) = #f8f9fa
            control.AppearanceItem.Normal.BackColor = Color.FromArgb(248, 249, 250);
            control.AppearanceItem.Normal.Options.UseBackColor = true;
            
            // Hover states matching HTML .card:hover
            Color hoverBorderColor = Color.FromArgb(41, 92, 0, 30); // rgba(41,92,0,0.12)
            Color textPrimaryDark = Color.FromArgb(58, 74, 14); // #3a4a0e
            
            control.AppearanceItem.Hovered.BackColor = Color.FromArgb(255, 255, 255); // Stay white
            control.AppearanceItem.Hovered.ForeColor = textPrimaryDark;
            control.AppearanceItem.Hovered.BorderColor = hoverBorderColor;
            control.AppearanceItem.Hovered.Options.UseBackColor = true;
            control.AppearanceItem.Hovered.Options.UseForeColor = true;
            control.AppearanceItem.Hovered.Options.UseBorderColor = true;
        }
        
        /// <summary>
        /// Create entrance screen header panel with logo only (no text) and government logos
        /// Returns a PanelControl that should be docked to Top before the menu control
        /// </summary>
        public static DevExpress.XtraEditors.PanelControl CreateEntranceScreenHeader()
        {
            var headerPanel = new DevExpress.XtraEditors.PanelControl();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Size = new Size(0, 100); // Reduced height since no text
            headerPanel.Padding = new Padding(48, 40, 48, 0);
            headerPanel.Appearance.BackColor = Color.Transparent; // Transparent to show gradient background
            headerPanel.Appearance.Options.UseBackColor = true;
            
            // Main container using TableLayoutPanel for flexible layout
            var tableLayout = new TableLayoutPanel();
            tableLayout.Dock = DockStyle.Fill;
            tableLayout.ColumnCount = 2;
            tableLayout.RowCount = 1;
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tableLayout.AutoSize = true;
            tableLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            
            // Left side: Logo only (no text)
            var leftPanel = new Panel();
            leftPanel.Dock = DockStyle.Fill;
            leftPanel.AutoSize = true;
            leftPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            leftPanel.Padding = new Padding(0);
            
            // Logo image - load yesildefter_horizontal.png
            var logoPicture = new DevExpress.XtraEditors.PictureEdit();
            logoPicture.Size = new Size(200, 60); // Horizontal logo size
            logoPicture.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
            logoPicture.Properties.ShowMenu = false;
            logoPicture.Properties.ReadOnly = true;
            
            // Try to load logo from disk first, then embedded resources
            bool logoLoaded = false;
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string[] logoPaths = new[]
                {
                    Path.Combine(baseDir, "Forms", "Templates", "public", "yesildefter_horizontal.png"),
                    Path.Combine(baseDir, "Templates", "public", "yesildefter_horizontal.png"),
                    Path.Combine(Application.StartupPath, "Forms", "Templates", "public", "yesildefter_horizontal.png"),
                    Path.Combine(Application.StartupPath, "Templates", "public", "yesildefter_horizontal.png"),
                    Path.Combine(baseDir, "..", "..", "Forms", "Templates", "public", "yesildefter_horizontal.png"),
                    Path.Combine(Application.StartupPath, "..", "..", "Forms", "Templates", "public", "yesildefter_horizontal.png")
                };
                
                foreach (string logoPath in logoPaths)
                {
                    if (File.Exists(logoPath))
                    {
                        logoPicture.Image = Image.FromFile(logoPath);
                        logoLoaded = true;
                        System.Diagnostics.Debug.WriteLine($"[tDevExpressTheme] Logo loaded from disk: {logoPath}");
                        break;
                    }
                }
                
                // Try embedded resources if disk load failed
                if (!logoLoaded)
                {
                    var asm = System.Reflection.Assembly.GetExecutingAssembly();
                    string[] resourceNames = asm.GetManifestResourceNames();
                    string logoResource = Array.Find(resourceNames, r => 
                        r.EndsWith("yesildefter_horizontal.png", StringComparison.OrdinalIgnoreCase) ||
                        (r.Contains("yesildefter") && r.EndsWith(".png", StringComparison.OrdinalIgnoreCase)));
                    
                    if (!string.IsNullOrEmpty(logoResource))
                    {
                        using (var stream = asm.GetManifestResourceStream(logoResource))
                        {
                            if (stream != null)
                            {
                                logoPicture.Image = Image.FromStream(stream);
                                logoLoaded = true;
                                System.Diagnostics.Debug.WriteLine($"[tDevExpressTheme] Logo loaded from embedded resource: {logoResource}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[tDevExpressTheme] Failed to load logo: {ex.Message}");
            }
            
            // Fallback: Use placeholder logo if main logo not loaded
            if (!logoLoaded)
            {
                try
                {
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    string[] placeholderPaths = new[]
                    {
                        Path.Combine(baseDir, "Forms", "Templates", "public", "yesildefter-logo-leaf.png"),
                        Path.Combine(baseDir, "Templates", "public", "yesildefter-logo-leaf.png"),
                        Path.Combine(Application.StartupPath, "Forms", "Templates", "public", "yesildefter-logo-leaf.png"),
                        Path.Combine(Application.StartupPath, "Templates", "public", "yesildefter-logo-leaf.png")
                    };
                    
                    foreach (string placeholderPath in placeholderPaths)
                    {
                        if (File.Exists(placeholderPath))
                        {
                            logoPicture.Image = Image.FromFile(placeholderPath);
                            logoLoaded = true;
                            System.Diagnostics.Debug.WriteLine($"[tDevExpressTheme] Placeholder logo loaded: {placeholderPath}");
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[tDevExpressTheme] Failed to load placeholder logo: {ex.Message}");
                }
            }
            
            leftPanel.Controls.Add(logoPicture);
            leftPanel.Size = new Size(200, 60);
            
            // Right side: Government logos (as images, not text)
            var govLogosPanel = new FlowLayoutPanel();
            govLogosPanel.FlowDirection = FlowDirection.LeftToRight;
            govLogosPanel.AutoSize = true;
            govLogosPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            govLogosPanel.Margin = new Padding(24, 0, 0, 0);
            
            // Government logo containers (MEB, UAB, TABIM) - using placeholder logo
            string[] govLogoLabels = { "MEB", "UAB", "TABİM" };
            foreach (string logoLabel in govLogoLabels)
            {
                var govLogoContainer = new DevExpress.XtraEditors.PanelControl();
                govLogoContainer.Size = new Size(120, 56);
                govLogoContainer.Appearance.BackColor = BgSecondary; // White
                govLogoContainer.Appearance.Options.UseBackColor = true;
                govLogoContainer.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
                govLogoContainer.Padding = new Padding(12, 8, 12, 8);
                govLogoContainer.Margin = new Padding(0, 0, 12, 0);
                
                // Government logo as image (using placeholder)
                var govLogoPicture = new DevExpress.XtraEditors.PictureEdit();
                govLogoPicture.Size = new Size(96, 40);
                govLogoPicture.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
                govLogoPicture.Properties.ShowMenu = false;
                govLogoPicture.Properties.ReadOnly = true;
                
                // Try to load government logo images, fallback to placeholder
                bool govLogoLoaded = false;
                try
                {
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    string[] govLogoPaths = new[]
                    {
                        Path.Combine(baseDir, "Forms", "Templates", "public", $"{logoLabel.ToLower()}_logo.png"),
                        Path.Combine(baseDir, "Forms", "Templates", "public", $"{logoLabel.ToLower()}_emblem.png"),
                        Path.Combine(baseDir, "Templates", "public", $"{logoLabel.ToLower()}_logo.png"),
                        Path.Combine(Application.StartupPath, "Forms", "Templates", "public", $"{logoLabel.ToLower()}_logo.png")
                    };
                    
                    foreach (string govLogoPath in govLogoPaths)
                    {
                        if (File.Exists(govLogoPath))
                        {
                            govLogoPicture.Image = Image.FromFile(govLogoPath);
                            govLogoLoaded = true;
                            System.Diagnostics.Debug.WriteLine($"[tDevExpressTheme] Government logo loaded: {govLogoPath}");
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[tDevExpressTheme] Failed to load government logo for {logoLabel}: {ex.Message}");
                }
                
                // Fallback to placeholder logo
                if (!govLogoLoaded)
                {
                    try
                    {
                        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                        string[] placeholderPaths = new[]
                        {
                            Path.Combine(baseDir, "Forms", "Templates", "public", "yesildefter-logo-leaf.png"),
                            Path.Combine(baseDir, "Templates", "public", "yesildefter-logo-leaf.png"),
                            Path.Combine(Application.StartupPath, "Forms", "Templates", "public", "yesildefter-logo-leaf.png"),
                            Path.Combine(Application.StartupPath, "Templates", "public", "yesildefter-logo-leaf.png")
                        };
                        
                        foreach (string placeholderPath in placeholderPaths)
                        {
                            if (File.Exists(placeholderPath))
                            {
                                govLogoPicture.Image = Image.FromFile(placeholderPath);
                                govLogoLoaded = true;
                                System.Diagnostics.Debug.WriteLine($"[tDevExpressTheme] Using placeholder logo for {logoLabel}: {placeholderPath}");
                                break;
                            }
                        }
                    }
                    catch { }
                }
                
                if (govLogoLoaded)
                {
                    govLogoContainer.Controls.Add(govLogoPicture);
                }
                else
                {
                    // Last resort: text label
                    var govLogoLabel = new DevExpress.XtraEditors.LabelControl();
                    govLogoLabel.Text = logoLabel;
                    govLogoLabel.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point);
                    govLogoLabel.Appearance.ForeColor = Color.FromArgb(198, 40, 40);
                    if (logoLabel == "TABİM")
                        govLogoLabel.Appearance.ForeColor = Color.FromArgb(13, 59, 102);
                    govLogoLabel.Appearance.Options.UseFont = true;
                    govLogoLabel.Appearance.Options.UseForeColor = true;
                    govLogoLabel.Dock = DockStyle.Fill;
                    govLogoLabel.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
                    govLogoLabel.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
                    govLogoContainer.Controls.Add(govLogoLabel);
                }
                
                govLogosPanel.Controls.Add(govLogoContainer);
            }
            
            tableLayout.Controls.Add(leftPanel, 0, 0);
            tableLayout.Controls.Add(govLogosPanel, 1, 0);
            
            headerPanel.Controls.Add(tableLayout);
            
            // Calculate actual height based on content
            headerPanel.AutoSize = false;
            headerPanel.Height = Math.Max(100, Math.Max(leftPanel.Height, govLogosPanel.Height) + 40);
            
            // NOTE(@Janberk): Ensure header panel is properly configured
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Visible = true;
            headerPanel.BringToFront();
            
            System.Diagnostics.Debug.WriteLine($"[CreateEntranceScreenHeader] Header panel created: Height={headerPanel.Height}, Dock={headerPanel.Dock}");
            
            return headerPanel;
        }
    }
}

