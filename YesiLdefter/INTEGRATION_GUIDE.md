# WebView2 Integration Guide for YesiLdefter

## Using WebView2 Instead of TileControl (ItemType 105)

### Option 1: Standalone Entrance Screen (Recommended)

Show the entrance screen **before** the main menu system loads:

```csharp
// In main.cs or tStarter.cs, before preparingMenus():

// Show entrance screen
using (var entranceScreen = new YesiLdefter.ms_EntranceScreen())
{
    var result = entranceScreen.ShowDialog(v.mainForm);
    if (result == DialogResult.OK && !string.IsNullOrEmpty(entranceScreen.SelectedModule))
    {
        // Navigate to selected module
        // entranceScreen.SelectedModule contains the module name from the card click
        // You can use this to determine which menu to load or which form to open
    }
    else if (result == DialogResult.Cancel)
    {
        // User cancelled - exit application or show login
        Application.Exit();
        return;
    }
}

// Continue with normal menu preparation
t.WaitFormOpen(v.mainForm, "Menü hazırlanıyor...");
preparingMenus();
```

### Option 2: Add as New ItemType (111 - WebView2Control)

If you want to integrate WebView2 into the menu system as a new ItemType:

**Step 1: Add to tMenu.cs Create_Menu method:**

```csharp
// Around line 99, add:
if (ItemType == 111) Create_WebView2Control((Microsoft.Web.WebView2.WinForms.WebView2)menuControl, ds_Items);
```

**Step 2: Add to Create_Menu_IN_Control method (around line 670):**

```csharp
#region // 111 - WebView2Control
if (ItemType == 111)
{
    Microsoft.Web.WebView2.WinForms.WebView2 menuControl = new Microsoft.Web.WebView2.WinForms.WebView2();
    
    menuControl.Name = "MENU_" + MasterCode;
    menuControl.Dock = DockStyle.Fill;
    
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
    else
    {
        mainControl.Controls.Add(menuControl);
    }
    
    Create_WebView2Control(menuControl, ds_Items, MenuCode);
}
#endregion
```

**Step 3: Implement Create_WebView2Control method:**

```csharp
#region Create_WebView2Control
public async void Create_WebView2Control(
    Microsoft.Web.WebView2.WinForms.WebView2 mControl, 
    DataSet ds_Items, 
    string MenuCode)
{
    tToolBox t = new tToolBox();
    
    try
    {
        // Get template path from metadata or use default
        string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "EntranceTemplate.html");
        
        // Load and inject tokens
        string html = File.ReadAllText(templatePath, Encoding.UTF8);
        
        // Inject logo and assets (similar to ms_EntranceScreen.BuildHtml())
        string logoBase64 = ""; // Load logo as base64
        html = html
            .Replace("{{logo-base64}}", logoBase64)
            .Replace("{{asset-base}}", assetBase);
        
        // Initialize WebView2
        var env = await CoreWebView2Environment.CreateAsync();
        await mControl.EnsureCoreWebView2Async(env);
        
        // Handle messages from WebView2
        mControl.CoreWebView2.WebMessageReceived += (sender, e) =>
        {
            var raw = e.TryGetWebMessageAsString();
            var payload = JsonConvert.DeserializeObject<Dictionary<string, object>>(raw);
            if (payload != null && payload.ContainsKey("action") && payload["action"].ToString() == "navigate")
            {
                // Navigate to module (similar to tEventsMenu handling)
                string module = payload.ContainsKey("module") ? payload["module"].ToString() : null;
                // Handle navigation...
            }
        };
        
        mControl.NavigateToString(html);
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"WebView2Control error: {ex.Message}");
    }
}
#endregion
```

## Key Differences: TileControl vs WebView2

### TileControl (ItemType 105)
- **Pros**: Native DevExpress control, fast rendering, database-driven tiles
- **Cons**: Limited styling, requires DevExpress license, less flexible
- **Use Case**: Simple tile grids from database metadata

### WebView2 (ItemType 111 or Standalone)
- **Pros**: Full HTML/CSS/JS control, modern design, matches web app styling
- **Cons**: Slightly heavier, requires WebView2 runtime
- **Use Case**: Entrance screens, modern UI, design consistency with web app

## Recommended Approach

For the **9-card entrance screen**, use **Option 1 (Standalone)** because:
1. It shows before authentication/menu loading
2. Cleaner separation of concerns
3. Easier to maintain and style
4. Can be shown conditionally (e.g., only on first launch)

The `ms_EntranceScreen` form is ready to use - just call `ShowDialog()` before your menu preparation code.

