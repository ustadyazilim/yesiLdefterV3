# TileNavPane Architecture Analysis & WebView2 Refactoring Plan

## Current Architecture Overview

### 1. Database-Driven Menu System

**Data Source**: `ds_Items` DataSet (from `MS_ITEMS` table)

**Key Columns**:
- `REF_ID`: Unique identifier
- `ITEM_TYPE`: Determines control type (201=Category, 203=Group, 205=NavButton, 206=TileNavItem)
- `CAPTION`: Display text
- `ITEM_NAME`: Internal name (null for categories/groups)
- `PROP_NAVIGATOR`: Form name to open (stored in `Tag` property)
- `CMP_BACK_COLOR`, `MENU_COLOR`: Color metadata (currently hardcoded)
- `LKP_GLYPH16`, `LKP_GLYPH32`: Icon images (byte arrays)
- `CLICK_EVENTS`: Event handler ID
- `CMP_ENABLED`, `CMP_VISIBLE`: State flags
- `DOCK_TYPE`: Layout position
- `SHORTCUT_KEYS`: Keyboard shortcuts
- `LINE_NO`: Ordering

### 2. Rendering Process (`Create_TileNavPane`)

**Flow**:
```
ds_Items (DataSet)
  ↓
Loop through rows
  ↓
Check ITEM_TYPE
  ↓
Create DevExpress control:
  - 201/203 → TileNavCategory (groups)
  - 205 → NavButton (action buttons)
  - 206 → TileNavItem (tiles within categories)
  ↓
Set properties:
  - Name = "item_" + REF_ID
  - Caption = CAPTION
  - Tag = PROP_NAVIGATOR + "|Prop_Navigator|" + menuName + "|MenuName|"
  - Appearance.BackColor = v.colorNew (hardcoded)
  - AppearanceHovered.BackColor = v.colorFocus (hardcoded)
  - Glyph = LKP_GLYPH16/32 (from database)
  ↓
Wire events:
  - ElementClick → evm.tNavButton_ElementClick
  ↓
Add to hierarchy:
  - Categories → mControl.Categories.Add()
  - Buttons → mControl.Buttons.Add()
  - Items → category.Items.Add()
```

### 3. Event Handling (`tNavButton_ElementClick`)

**Event Flow**:
```
User clicks tile
  ↓
ElementClick fires
  ↓
tNavButton_ElementClick(sender, e)
  ↓
Extract metadata:
  - ButtonName = sender.Name
  - formName = sender.Appearance.Name
  - values = sender.Tag (contains PROP_NAVIGATOR, TableIPCode, MenuName)
  ↓
Parse Tag:
  - myFormLoadValue = Extract "|Prop_Navigator|"
  - TableIPCode = Extract "|TableIPCode|"
  - menuName = Extract "|MenuName|"
  ↓
Call commonMenuClick(tForm, ButtonName, TableIPCode, myFormLoadValue)
  ↓
Opens form or executes action
```

**Critical Properties**:
- `sender.Name`: Used to identify which tile was clicked
- `sender.Tag`: Contains navigation metadata (pipe-delimited)
- `sender.Appearance.Name`: Form name where menu exists
- `sender.Caption`: Display text (for shortcuts)

### 4. Hierarchy Structure

```
TileNavPane
├── Categories (ItemType 201)
│   └── Items (ItemType 206)
│       └── SubItems (ItemType 206 with parent ItemType 206)
└── Buttons (ItemType 203/205)
    └── Items (ItemType 206)
```

## WebView2 Refactoring Strategy

### Multiple Approaches: Choosing the Best Strategy

There are several ways to integrate WebView2 with the existing DevExpress system. Let's evaluate each:

#### Approach 1: Hidden DevExpress Controls (Initial Proposal)
**Principle**: Create DevExpress controls, add to form, then hide them. WebView2 overlay renders on top.

#### Approach 2: In-Memory DevExpress Controls (RECOMMENDED) ⭐
**Principle**: Create DevExpress controls but **never add them to the form**. They exist only in memory for their metadata and event wiring. WebView2 is the only visible UI.

**Implementation**:
```csharp
// Create TileNavPane but DON'T add to form
var tileNavPane = new TileNavPane();
tileNavPane.Visible = false;  // Safety measure
// tileNavPane.Parent = null;  // Never set parent
// form.Controls.Add(tileNavPane);  // DON'T DO THIS

// Build menu structure (creates TileNavCategory, TileNavItem, etc.)
Create_TileNavPane(tileNavPane, ds_Items, ...);

// All controls exist in memory with:
// - Name, Tag, Caption properties set
// - ElementClick events wired
// - But they're never rendered

// Create WebView2 overlay
var webView = new WebView2();
webView.Dock = DockStyle.Fill;
form.Controls.Add(webView);  // Only WebView2 is visible

// Extract menu structure to JSON
var menuJson = ExtractMenuStructureToJson(tileNavPane);

// Load HTML template with menu data
webView.NavigateToString(BuildHtmlTemplate(menuJson));
```

**Pros**:
- ✅ DevExpress controls exist for event wiring
- ✅ No rendering overhead (controls never drawn)
- ✅ Cleaner architecture (only WebView2 renders)
- ✅ Same event handlers work unchanged
- ✅ Can still access all DevExpress properties

**Cons**:
- Controls not in form hierarchy (minor - doesn't affect functionality)
- Need to store tileNavPane reference for event bridge

**Why This is Better**: DevExpress controls serve as **data containers and event wiring**, not as UI. This is exactly what we need - their properties (Name, Tag, Caption) and event handlers, without the rendering.

#### Approach 3: Lightweight Adapter Pattern
**Principle**: Create a lightweight class that implements the same interface as DevExpress controls, but doesn't render anything. Pass this adapter to `tNavButton_ElementClick`.
**Why This is Risky**: The event handler does `sender.GetType().ToString() == "DevExpress.XtraBars.Navigation.TileNavItem"` - an adapter won't match this check.

#### Approach 4: CSS Override / DevExpress Skinning
**Principle**: Use DevExpress's Appearance system or skinning to customize look, but keep DevExpress rendering.

**Reality Check**:
- DevExpress uses **WinForms properties** (Appearance.BackColor, etc.), not CSS
- DevExpress has **predefined skins** (Office 2016, VS2019, etc.) - you can't inject custom HTML/CSS

**Verdict**: ❌ **Not feasible**. DevExpress controls are WinForms controls, not web components. You cannot inject CSS or HTML into them.

#### Approach 5: Hybrid - DevExpress for Structure, WebView2 for Styling
**Principle**: Use DevExpress controls for layout structure (categories, items), but render their content in WebView2.

**Implementation**:
```csharp
// Create DevExpress controls
var tileNavPane = new TileNavPane();
Create_TileNavPane(tileNavPane, ds_Items, ...);

// Extract structure
var structure = ExtractStructure(tileNavPane);

// Render in WebView2 with custom styling
// But use DevExpress for actual click handling
```

**Pros**:
- DevExpress handles structure/logic
- WebView2 handles visual styling

**Cons**:
- Still requires DevExpress rendering (defeats the purpose)
- Complex synchronization between two systems
- Doesn't solve the styling limitation

**Verdict**: ❌ **Not better than Approach 2**. Still renders DevExpress.

### Recommended Approach: In-Memory DevExpress Controls (Approach 2)

**Why This is Optimal**:

1. **Zero Functional Changes**: All existing code works unchanged
   - `Create_TileNavPane()` works as-is
   - Event wiring works as-is
   - `tNavButton_ElementClick()` works as-is
   - Tag parsing works as-is

2. **Clean Architecture**: 
   - DevExpress = Data Model + Event System
   - WebView2 = View Layer
   - Clear separation of concerns

3. **Performance**: 
   - No rendering overhead (controls never drawn)
   - Only WebView2 renders (modern, hardware-accelerated)
   - Memory footprint: Controls exist but not rendered

4. **Maintainability**:
   - Database-driven logic unchanged
   - Visual changes = HTML/CSS only
   - Easy to toggle back to DevExpress (just add controls to form)

5. **Proven Pattern**:
   - Similar to MVVM pattern (Model exists, View is separate)
   - DevExpress controls = ViewModel
   - WebView2 = View

### Implementation Details for Approach 2

**Key Insight**: In WinForms, a control doesn't need to be in a form's `Controls` collection to exist and function. It just won't be rendered.

```csharp
public class TileNavWebViewWrapper
{
    private TileNavPane _tileNavPane;  // Exists but not rendered
    private WebView2 _webView;         // Only visible UI
    private Form _parentForm;
    
    public void Initialize(Form parent, DataSet ds_Items, ...)
    {
        _parentForm = parent;
        
        // Create DevExpress control but DON'T add to form
        _tileNavPane = new TileNavPane();
        _tileNavPane.Name = "MENU_" + masterCode;
        // _tileNavPane.Parent = null;  // Explicitly no parent
        // _parentForm.Controls.Add(_tileNavPane);  // DON'T ADD
        
        // Build menu structure (creates all TileNavCategory, TileNavItem, etc.)
        var menu = new tMenu();
        menu.Create_TileNavPane(_tileNavPane, ds_Items, ...);
        
        // All controls now exist in memory with events wired
        // But they're never rendered
        
        // Create WebView2 (only visible UI)
        _webView = new WebView2();
        _webView.Dock = DockStyle.Fill;
        _webView.WebMessageReceived += WebView_WebMessageReceived;
        _parentForm.Controls.Add(_webView);
        
        // Extract menu structure
        var menuJson = ExtractMenuStructureToJson(_tileNavPane);
        
        // Load HTML template
        var html = BuildHtmlTemplate(menuJson);
        _webView.NavigateToString(html);
    }
    
    private void WebView_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        var message = JsonConvert.DeserializeObject<dynamic>(e.TryGetWebMessageAsString());
        if (message.action == "tile-click")
        {
            // Find DevExpress control by name
            var element = FindElementByName(_tileNavPane, message.buttonName);
            
            if (element != null)
            {
                // Trigger the EXACT SAME event handler
                var evm = new tEventsMenu();
                var args = new NavElementEventArgs(element);
                evm.tNavButton_ElementClick(element, args);
                
                // This calls:
                // → Tag parsing
                // → commonMenuClick()
                // → Form opening
                // → All existing logic
            }
        }
    }
    
    private BaseNavElement FindElementByName(TileNavPane pane, string name)
    {
        // Search Categories
        foreach (var cat in pane.Categories)
        {
            if (cat.Name == name) return cat;
            foreach (var item in cat.Items)
            {
                if (item.Name == name) return item;
                // ... recursive search
            }
        }
        // ... search Buttons
        return null;
    }
}
```

**Benefits Over Hiding**:
- ✅ No rendering overhead
- ✅ Cleaner code (explicit intent: controls not for rendering)
- ✅ Better performance
- ✅ Easier to understand (controls = data, WebView2 = view)

### Final Recommendation

**Use Approach 2: In-Memory DevExpress Controls**

This gives you:
- ✅ Full WebView2 features (modern CSS, animations, responsive design)
- ✅ Zero functional changes to existing code
- ✅ Best performance (no DevExpress rendering)
- ✅ Clean architecture (separation of data/view)
- ✅ Easy rollback (just add controls to form if needed)

**Key Principle**: DevExpress controls become **data containers and event wiring**, not UI components. WebView2 is the **only rendering layer**.

---

## Event-Coverage Assurance (manager worry: “what about unknown events?”)

**We are not faking or re-implementing events.** We keep the real DevExpress elements alive in memory with all their existing handlers and properties.

1) **Real elements, real handlers**  
   - `Create_TileNavPane` still builds every `TileNavCategory/TileNavItem/NavButton`.  
   - `ElementClick` is already wired (e.g., `evm.tNavButton_ElementClick`). We reuse it.  
   - We do **not** imitate or guess events; we call the same handler on the same element instance.

2) **Bridge calls the same pipeline**  
   - WebView2 posts `{ action: 'tile-click', buttonName, tag }`.  
   - C# finds the DevExpress element by `Name` (or `Tag`).  
   - Calls `tNavButton_ElementClick(element, args);` → Tag parsing → `commonMenuClick` → open form.  
   - If an element isn’t found, we log it (safety net) and can add alternate lookup (e.g., Tag).

3) **Parity/coverage proof**  
   - Optional one-time inventory: iterate all DevExpress elements, log `Name/Tag/ItemType` and whether `ElementClick` is wired. Shows nothing is skipped.  
   - Parity logging: compare DevExpress click vs WebView click (same `Name/Tag/Prop_Navigator` and resulting form).  
   - Because DevExpress is still there (in memory), we can temporarily show it for validation.

4) **Pattern already proven**  
   - Login and Firm Selection use WebView2 for the UI but invoke existing C# logic; no logic rewrites there. This is the same pattern for menus.

**Bottom line:** We do not have to know every event upfront. We keep the original elements and their handlers; WebView2 only replaces the visuals.

---

### Original Approach: Visual Overlay with Event Bridge

**Key Principle**: Keep ALL DevExpress controls and ALL event handlers intact, but hide the DevExpress visuals. Render an HTML/CSS "skin" in WebView2 that triggers the **same existing click pipeline**.

### Why this is applicable in a DB-driven UI (and why it does NOT change logic)

Your system is already “metadata → UI → events → business logic”:

- **Metadata**: `ds_Items` (DB-driven) decides what to show (caption, icon, visibility, navigator, etc.).
- **UI controls**: DevExpress renders the metadata into `TileNavCategory` / `TileNavItem` / `NavButton`.
- **Logic is NOT in the UI**: The real behavior happens in `tEventsMenu.tNavButton_ElementClick()`:
  - It reads `sender.Name`, `sender.Tag`, `sender.Appearance.Name`
  - It parses `Tag` values like `|Prop_Navigator|`, `|TableIPCode|`, `|MenuName|`
  - It calls `commonMenuClick(...)`

So if we keep the DevExpress objects and keep calling **the same handler** with **the same `Name` + `Tag`**, then:

- ✅ All variable sets stay the same (`v.*`)
- ✅ All registry behavior stays the same (`tRegistry`, `tToolBox` helper calls, etc.)
- ✅ The “crazy” runtime routing stays the same (the `Tag` parsing and `commonMenuClick` entry)
- ✅ The UI definition remains DB-driven (we still build from `ds_Items`)

**What changes** is only the render surface:

- DevExpress becomes the *invisible model of record* (still created exactly as today).
- WebView2/HTML becomes the *visible view layer*.

This is the same concept already proven in the project:

- `ms_User_Standalone.cs` uses **WebView2** for the UI but keeps WinForms-side logic: registry state, API calls, success/failure handling.
- `ms_UserFirmSelect.cs` uses **WebView2** for the UI but returns the same `SelectedFirm` back to the existing flow.

In both cases, **HTML/CSS replaced the UI**, not the business rules. The TileNav overlay follows the same pattern.

### What we are NOT doing (to keep the manager comfortable)

- **No rewriting menu logic** (no replacing `tNavButton_ElementClick`, no rewriting `commonMenuClick`).
- **No changing DB schema / query output** (`MS_ITEMS` and `ds_Items` remain the source of truth).
- **No changing naming conventions** (we preserve `item_{REF_ID}` and the existing `Tag` format).
- **No changing the click routing contract** (we trigger the same handler the same way).

### What the overlay bridge actually does (concretely)

1. Build DevExpress menu exactly as today (`Create_TileNavPane(...)`).
2. Extract a JSON snapshot for the web UI **from the same source**:
   - Preferred: from the already-created DevExpress elements (guarantees identical naming/Tag)
   - Alternative: directly from `ds_Items` using the same naming rules
3. Render HTML/CSS cards (matching your Next.js design language).
4. When user clicks a card in HTML, WebView2 posts:

```json
{ "action": "tile-click", "name": "item_12345" }
```

5. C# receives that message, finds the matching DevExpress element by `Name`, and invokes:
   - either `tEventsMenu.tNavButton_ElementClick(element, null)` (direct)
   - or a small wrapper that calls `commonMenuClick` the same way the handler does

Either way, the “source of behavior” stays the same.

### Rollout strategy that avoids risk (recommended for DB manager buy-in)

- **Feature flag / config switch**:
  - `UseHtmlMenu = false` by default → current DevExpress visuals
  - `UseHtmlMenu = true` → overlay visuals
  - Zero-risk rollback: flip the flag
- **Migrate one menu first**:
  - Choose 1 `MenuCode` / 1 screen
  - Validate “every click opens the same form” before expanding
- **Parity tests**:
  - Log `ButtonName`, parsed `Prop_Navigator`, `TableIPCode`, `MenuName` for both DevExpress click and HTML click
  - Confirm identical outputs

### The only real engineering work

We need a clean “bridge” layer that does two things reliably:

- **Extract**: convert the existing menu structure to a JSON model for HTML rendering
- **Route**: map HTML click → the exact DevExpress element → call the same handler

Everything else is CSS/HTML work (the part you want to optimize using your Next.js design system).

### Implementation Plan

#### Phase 1: Data Extraction Layer

Create a method to extract menu structure from TileNavPane into JSON:

```csharp
public class MenuItemData
{
    public string RefId { get; set; }
    public int ItemType { get; set; }
    public string Caption { get; set; }
    public string Name { get; set; }
    public string Tag { get; set; }
    public string GlyphBase64 { get; set; } // Convert image to base64
    public Color BackColor { get; set; }
    public Color HoverColor { get; set; }
    public bool Enabled { get; set; }
    public bool Visible { get; set; }
    public List<MenuItemData> Children { get; set; }
}

public string ExtractMenuStructureToJson(TileNavPane pane)
{
    // Traverse Categories, Buttons, Items
    // Convert to JSON payload
    // Include all metadata needed for HTML rendering
}
```

#### Phase 2: WebView2 Overlay

1. **Hide DevExpress Control**:
   ```csharp
   tileNavPane.Visible = false;
   tileNavPane.Enabled = false; // Prevent native clicks
   ```

2. **Create WebView2 Container**:
   ```csharp
   WebView2 webView = new WebView2();
   webView.Dock = DockStyle.Fill;
   webView.Location = tileNavPane.Location;
   webView.Size = tileNavPane.Size;
   parentControl.Controls.Add(webView);
   ```

3. **Load HTML Template**:
   - Use `EntranceTemplate.html` (or create `TileNavTemplate.html`)
   - Inject JSON payload: `{{menuStructure}}`
   - Inject design tokens: `{{designTokens}}`
   - Inject asset paths: `{{asset-base}}`

#### Phase 3: Event Bridge

**JavaScript → C# Bridge**:

```javascript
// In HTML template
function postClick(buttonName, tag) {
    if (window.chrome && window.chrome.webview && window.chrome.webview.postMessage) {
        window.chrome.webview.postMessage(JSON.stringify({
            action: 'tile-click',
            buttonName: buttonName,
            tag: tag
        }));
    }
}
```

**C# Event Handler**:

```csharp
private void WebView_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
{
    try
    {
        var message = JsonConvert.DeserializeObject<dynamic>(e.TryGetWebMessageAsString());
        if (message.action == "tile-click")
        {
            // Find the corresponding DevExpress control
            var control = FindControlByName(tileNavPane, message.buttonName);
            if (control != null)
            {
                // Trigger the original event handler
                var evm = new tEventsMenu();
                var args = new NavElementEventArgs(control.Element);
                evm.tNavButton_ElementClick(control.Element, args);
            }
        }
    }
    catch { }
}

private NavButton FindControlByName(TileNavPane pane, string name)
{
    // Search Categories
    foreach (var cat in pane.Categories)
    {
        if (cat.Name == name) return new NavButton { Element = cat };
        foreach (var item in cat.Items)
        {
            if (item.Name == name) return new NavButton { Element = item };
            foreach (var subItem in item.SubItems)
            {
                if (subItem.Name == name) return new NavButton { Element = subItem };
            }
        }
    }
    
    // Search Buttons
    foreach (var btn in pane.Buttons)
    {
        if (btn.Element.Name == name) return btn;
        // ... recursive search for items
    }
    
    return null;
}
```

#### Phase 4: HTML Template Structure

**Template Structure** (`TileNavTemplate.html`):

```html
<!DOCTYPE html>
<html>
<head>
  <!-- Design tokens from LoginTemplate.html -->
  <style>
    :root {
      --primary: #295c00;
      --primary-light: #8e9c78;
      /* ... all tokens from LoginTemplate.html ... */
    }
  </style>
</head>
<body>
  <div class="menu-container">
    <!-- Categories (left sidebar) -->
    <div class="categories">
      <div class="category" data-refid="..." data-name="item_...">
        <div class="category-header">Category Name</div>
        <div class="category-items">
          <!-- TileNavItems (cards) -->
          <div class="tile-card" 
               data-name="item_..." 
               data-tag="..."
               onclick="postClick('item_...', '...')">
            <div class="tile-icon"><img src="data:image/png;base64,..."></div>
            <div class="tile-title">Tile Caption</div>
          </div>
        </div>
      </div>
    </div>
    
    <!-- Buttons (top/right) -->
    <div class="buttons">
      <div class="nav-button" 
           data-name="item_..." 
           data-tag="..."
           onclick="postClick('item_...', '...')">
        Button Caption
      </div>
    </div>
  </div>
  
  <script>
    // Parse injected JSON
    const menuData = JSON.parse('{{menuStructure}}');
    
    // Render menu structure
    function renderMenu(data) {
      // Build HTML from JSON structure
    }
    
    // Click handler
    function postClick(buttonName, tag) {
      window.chrome.webview.postMessage(JSON.stringify({
        action: 'tile-click',
        buttonName: buttonName,
        tag: tag
      }));
    }
    
    // Initial render
    renderMenu(menuData);
  </script>
</body>
</html>
```

### Implementation Steps

1. **Create `ExtractMenuStructureToJson` method** in `tMenu.cs`
   - Traverse TileNavPane hierarchy
   - Extract all properties (Name, Caption, Tag, Colors, Icons)
   - Convert icons to base64
   - Build JSON tree structure

2. **Create `TileNavTemplate.html`**
   - Use same design tokens as `LoginTemplate.html`
   - Create 9-card grid layout (or flexible grid)
   - Add hover/click animations
   - Include governmental logos section

3. **Create `ms_TileNavWebView.cs` wrapper class**
   - Wraps TileNavPane + WebView2
   - Hides TileNavPane
   - Loads HTML template
   - Bridges JavaScript clicks to DevExpress events

4. **Modify `Create_TileNavPane`** (optional enhancement)
   - After creating TileNavPane, wrap it with WebView2 overlay
   - OR create a new method `Create_TileNavPane_WebView2` that does both

5. **Test Event Bridge**
   - Verify all clicks trigger correct `tNavButton_ElementClick`
   - Verify `Tag` parsing works correctly
   - Verify forms open correctly

### Benefits

✅ **Zero functional changes**: All event handling remains identical  
✅ **Database-driven**: Still reads from `ds_Items`, no hardcoding  
✅ **Modern UI**: HTML/CSS with design tokens matching Next.js  
✅ **Maintainable**: Visual changes only require HTML/CSS updates  
✅ **Backward compatible**: Can toggle between DevExpress and WebView2  

### Risks & Considerations

⚠️ **Performance**: WebView2 adds overhead (but minimal for static menus)  
⚠️ **Complexity**: Two rendering systems (DevExpress hidden + WebView2 visible)  
⚠️ **Maintenance**: Need to keep HTML structure in sync with DevExpress structure  
⚠️ **Testing**: Must verify all event paths work correctly  

### Alternative: Hybrid Approach

Instead of hiding TileNavPane completely, use a **hybrid** approach:

1. Keep TileNavPane for structure/events
2. Use WebView2 for **visual styling only** (CSS overlay)
3. Map WebView2 clicks to TileNavPane programmatic clicks

This reduces risk but adds complexity.

## Entrance Screen Integration Strategy

### Understanding the Entrance Screen Role

The `EntranceTemplate.html` represents a **9-card navigation dashboard** that serves as the main entry point after login/firm selection. This is conceptually the **first level** of the menu hierarchy, where users select which major module/category to enter.

**Key Design Elements**:
- **9-card grid** (3x3 layout) - Each card represents a major module/category
- **Government logos** (MEB, UAB, TABİM, MEBBİS) - Branding/credibility
- **YesiLdefter branding** - Logo and title
- **Card structure**: Icon + Title + Description
- **Module identifiers**: `data-module` attributes (kursiyer, donem, teorik, sinav, etc.)

### How Entrance Screen Maps to Database Structure

**Current Static Cards** → **Database-Driven Mapping**:

```
EntranceTemplate.html (Static)          →  MS_ITEMS Table (Dynamic)
─────────────────────────────────────────────────────────────────────
Card: "Kursiyer Kayıt İşlemleri"        →  ITEM_TYPE=201 (Category)
  data-module="kursiyer"                →  CAPTION="Kursiyer İşlemleri"
  onclick="selectModule('kursiyer')"   →  PROP_NAVIGATOR="ms_Kursiyer"
                                         →  REF_ID (unique identifier)
                                         →  LKP_GLYPH32 (icon from DB)
                                         →  LINE_NO (display order)

Card: "Dönem, Grup ve Şube"             →  ITEM_TYPE=201 (Category)
  data-module="donem"                   →  CAPTION="Dönem İşlemleri"
  onclick="selectModule('donem')"       →  PROP_NAVIGATOR="ms_Donem"
                                         →  REF_ID, LKP_GLYPH32, LINE_NO
```

**Critical Insight**: The entrance screen cards are **TileNavCategory items (ItemType 201)** from the database. Each card represents a top-level category that, when clicked, should either:
1. Open a submenu (another TileNavPane with ItemType 206 tiles)
2. Open a form directly (if PROP_NAVIGATOR points to a form)

### Entrance Screen Flow Integration

**Application Flow**:
```
1. Login (ms_User_Standalone) ✅ Already WebView2
   ↓
2. Firm Selection (ms_UserFirmSelect) ✅ Already WebView2
   ↓
3. **Entrance Screen** (NEW - WebView2 overlay on TileNavPane)
   - Shows 9 cards from database (ItemType 201 Categories)
   - User clicks a card
   - Triggers tNavButton_ElementClick for that category
   - Opens submenu or form
   ↓
4. Submenu/Form (Existing TileNavPane or Form)
```

**Where Entrance Screen Appears**:
- **Option A**: Replace the main TileNavPane rendering (ItemType 106) with entrance screen
- **Option B**: Entrance screen is a separate screen that appears before TileNavPane
- **Option C**: Entrance screen IS the TileNavPane, but rendered as WebView2 overlay

**Recommended**: **Option C** - Entrance screen is the WebView2 overlay for TileNavPane when it contains top-level categories (ItemType 201).

### Card Rendering from Database

**Data Extraction for Entrance Screen**:

```csharp
// Pseudo-code for extracting entrance cards
public class EntranceCardData
{
    public string RefId { get; set; }           // From REF_ID
    public string Name { get; set; }             // "item_" + REF_ID
    public string Caption { get; set; }         // From CAPTION
    public string Description { get; set; }     // Could be from DB or derived
    public string ModuleId { get; set; }        // Derived from CAPTION or ITEM_NAME
    public string Tag { get; set; }             // PROP_NAVIGATOR + "|Prop_Navigator|" + menuName
    public string IconBase64 { get; set; }       // From LKP_GLYPH32 (converted)
    public int LineNo { get; set; }              // From LINE_NO (for ordering)
    public bool Enabled { get; set; }           // From CMP_ENABLED
    public bool Visible { get; set; }           // From CMP_VISIBLE
    public Color BackColor { get; set; }         // From CMP_BACK_COLOR or default
    public Color HoverColor { get; set; }       // From MENU_COLOR or default
}

// Extract only top-level categories (ItemType 201) for entrance screen
public List<EntranceCardData> ExtractEntranceCards(DataSet ds_Items)
{
    var cards = new List<EntranceCardData>();
    
    foreach (DataRow row in ds_Items.Tables[0].Rows)
    {
        int itemType = Convert.ToInt32(row["ITEM_TYPE"]);
        
        // Only extract top-level categories (ItemType 201)
        // These become the 9 cards
        if (itemType == 201 && row["ITEM_NAME"] == DBNull.Value)
        {
            cards.Add(new EntranceCardData
            {
                RefId = row["REF_ID"].ToString(),
                Name = "item_" + row["REF_ID"].ToString(),
                Caption = row["CAPTION"].ToString(),
                // ... extract all properties
            });
        }
    }
    
    // Sort by LINE_NO
    return cards.OrderBy(c => c.LineNo).Take(9).ToList();
}
```

**HTML Template Injection**:

The `EntranceTemplate.html` needs to be **dynamic**, not static. Instead of hardcoded cards:

```html
<!-- STATIC (Current) -->
<div class="card" data-module="kursiyer" onclick="selectModule('kursiyer')">
  <h3 class="card-title">Kursiyer Kayıt İşlemleri</h3>
</div>
```

We need:

```html
<!-- DYNAMIC (From Database) -->
{{#each cards}}
<div class="card" 
     data-refid="{{RefId}}"
     data-name="{{Name}}"
     data-tag="{{Tag}}"
     onclick="selectModule('{{Name}}', '{{Tag}}')">
  <div class="card-icon">
    <img src="{{IconBase64}}" alt="{{Caption}}" />
  </div>
  <h3 class="card-title">{{Caption}}</h3>
  <p class="card-description">{{Description}}</p>
</div>
{{/each}}
```

Since we're using simple string replacement (like LoginTemplate), we'll inject JSON and render with JavaScript:

```html
<script>
  const cardsData = JSON.parse('{{cardsJson}}');
  
  function renderCards() {
    const grid = document.querySelector('.grid');
    grid.innerHTML = '';
    
    cardsData.forEach(card => {
      const cardEl = document.createElement('div');
      cardEl.className = 'card';
      cardEl.setAttribute('data-refid', card.RefId);
      cardEl.setAttribute('data-name', card.Name);
      cardEl.setAttribute('data-tag', card.Tag);
      cardEl.onclick = () => selectModule(card.Name, card.Tag);
      
      cardEl.innerHTML = `
        <div class="card-icon">
          <img src="${card.IconBase64}" alt="${card.Caption}" />
        </div>
        <h3 class="card-title">${escapeHtml(card.Caption)}</h3>
        <p class="card-description">${escapeHtml(card.Description || '')}</p>
      `;
      
      grid.appendChild(cardEl);
    });
  }
  
  renderCards();
</script>
```

### Event Bridge: selectModule() → tNavButton_ElementClick

**Current EntranceTemplate.html**:
```javascript
function selectModule(moduleId) {
  const message = {
    type: 'MODULE_SELECTED',
    module: moduleId,
    timestamp: new Date().toISOString()
  };
  window.chrome.webview.postMessage(JSON.stringify(message));
}
```

**Needs to become**:
```javascript
function selectModule(buttonName, tag) {
  // buttonName = "item_123" (from database REF_ID)
  // tag = "ms_Kursiyer|Prop_Navigator|MENU_Main|MenuName|"
  
  const message = {
    action: 'tile-click',  // Match existing pattern
    buttonName: buttonName,
    tag: tag,
    formName: '{{formName}}'  // Injected from C#
  };
  
  window.chrome.webview.postMessage(JSON.stringify(message));
}
```

**C# Event Bridge** (in WebView2 message handler):
```csharp
private void WebView_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
{
    var message = JsonConvert.DeserializeObject<dynamic>(e.TryGetWebMessageAsString());
    
    if (message.action == "tile-click")
    {
        // Find the DevExpress control by Name
        var control = FindControlByName(tileNavPane, message.buttonName);
        
        if (control != null)
        {
            // Create event args
            var args = new NavElementEventArgs(control.Element);
            
            // Call the EXACT SAME event handler that DevExpress would call
            var evm = new tEventsMenu();
            evm.tNavButton_ElementClick(control.Element, args);
            
            // This triggers:
            // → Tag parsing (PROP_NAVIGATOR extraction)
            // → commonMenuClick()
            // → Form opening
            // → All existing logic unchanged
        }
    }
}
```

### Card Description Source

**Challenge**: The database `MS_ITEMS` table may not have a `DESCRIPTION` column. Options:

1. **Derive from CAPTION**: Use CAPTION as both title and description
2. **Add to database**: New column `DESCRIPTION` or `CARD_DESCRIPTION`
3. **Hardcode mapping**: C# dictionary mapping module IDs to descriptions
4. **Use ITEM_NAME**: If ITEM_NAME exists, use it as description

**Recommended**: Start with Option 1 (use CAPTION), then add database column later if needed.

### Government Logos Integration

**Current**: Static image paths in HTML
```html
<img src="public/meb_emblem.png" alt="MEB" class="gov-logo">
```

**Options**:
1. **Keep static**: Logos are branding assets, not database-driven
2. **Database-driven**: Add `GOV_LOGO_PATH` column if logos vary by firm
3. **Configuration**: Store logo paths in `appsettings.json`

**Recommended**: Keep static for now (Option 1). Logos are consistent across all firms.

### Card Icon Handling

**Database Source**: `LKP_GLYPH32` (byte array)

**Conversion Process**:
```csharp
public string ConvertGlyphToBase64(byte[] glyphBytes)
{
    if (glyphBytes == null || glyphBytes.Length == 0)
        return GetDefaultIconBase64(); // Fallback SVG or default icon
    
    // Convert byte array to base64 data URI
    return "data:image/png;base64," + Convert.ToBase64String(glyphBytes);
}
```

**Fallback Strategy**:
- If `LKP_GLYPH32` is null/empty → Use SVG icon based on module type
- If conversion fails → Use default YesiLdefter icon
- Match icon style to card color gradient (from `data-module` attribute)

### Card Color System

**Current EntranceTemplate.html**: Hardcoded gradients per module
```css
.card[data-module="kursiyer"] .card-icon { 
  background: linear-gradient(135deg, #1565C0, #42A5F5); 
}
```

**Database-Driven Approach**:
- Use `CMP_BACK_COLOR` from database (if exists)
- Use `MENU_COLOR` for hover state
- Fallback to module-based defaults if database colors not set
- Maintain color consistency with design tokens

### Grid Layout Adaptation

**Current**: Fixed 3x3 grid (9 cards)
```css
.grid {
  grid-template-columns: repeat(3, 280px);
}
```

**Dynamic Approach**:
- If database returns < 9 cards → Adjust grid (2x2, 2x3, etc.)
- If database returns > 9 cards → Pagination or scroll
- Use `LINE_NO` for ordering
- Respect `CMP_VISIBLE` flag (hide disabled cards)

### Integration Points

**Where Entrance Screen is Created**:

1. **In `tMenu.Create_Menu()`**:
   - When `ItemType == 106` (TileNavPane)
   - Check if `ds_Items` contains mostly ItemType 201 (Categories)
   - If yes → Create WebView2 overlay with entrance screen
   - If no → Use traditional TileNavPane rendering

2. **In `Create_TileNavPane()`**:
   - After creating TileNavPane control
   - Extract categories (ItemType 201)
   - If count == 9 (or reasonable number) → Render as entrance screen
   - Otherwise → Use traditional category/item rendering

3. **As Separate Method**:
   - `Create_EntranceScreen(Control parent, DataSet ds_Items)`
   - Called before or instead of `Create_TileNavPane()`
   - Determined by menu configuration or user preference

**Recommended**: Integrate into `Create_TileNavPane()` as an overlay option, controlled by a flag or automatic detection.

## Next Steps

1. **Confirm approach**: Visual overlay vs hybrid
2. **Create extraction method**: `ExtractMenuStructureToJson` (includes entrance cards)
3. **Create HTML template**: `EntranceTemplate.html` with dynamic card rendering
4. **Create wrapper class**: `ms_TileNavWebView.cs` (or enhance existing)
5. **Test with one menu**: Verify event bridge works
6. **Roll out**: Apply to all TileNavPane instances

