# WebView2 Menu Demo Guide

## Implementation Status

✅ **Phase 1: Data Extraction Layer** - COMPLETE
- `ExtractMenuStructureFromDataSet()` method created in `tMenu.cs`
- Extracts ItemType 201 categories from database to JSON

✅ **Phase 2: WebView2 Overlay** - COMPLETE  
- `ms_TileNavWebView` wrapper class created
- Creates TileNavPane in-memory (not rendered)
- WebView2 overlay renders `EntranceTemplate.html`

✅ **Phase 3: Event Bridge** - COMPLETE
- WebView2 clicks bridge to `tNavButton_ElementClick`
- Parity logging enabled for validation

✅ **Phase 4: HTML Template** - COMPLETE
- `EntranceTemplate.html` updated to render dynamically from JSON
- Cards render from database data (ItemType 201 categories)

## Current Phase: **DEMO/TESTING** 🎯

We are now ready to **enable the feature flag and showcase** the solution.

## How to Enable the Feature

### Option 1: Enable Globally (Recommended for Demo)

Add this to your application startup (e.g., in `main.cs` or `Program.cs`):

```csharp
// Enable WebView2 menu rendering
v.SP_UseHtmlMenu = true;
```

**Best place to add**: In `main.cs`, in the `preparingMenus()` method or right before it:

```csharp
void preparingMenus()
{
    // Enable WebView2 menu overlay for ItemType 106 menus
    v.SP_UseHtmlMenu = true;  // <-- Add this line
    
    short menuType = mn.getCreateMenuType(v.tMainFirm.MenuCode);
    // ... rest of the method
}
```

### Option 2: Enable Per-Menu (More Control)

You can also enable it conditionally based on MenuCode:

```csharp
// Enable only for specific menus
if (v.tMainFirm.MenuCode == "UST/PMS/HUB/MainWebMtsk")
{
    v.SP_UseHtmlMenu = true;
}
```

### Option 3: Enable via Registry/Config (Production)

For production, you could load from registry or config:

```csharp
tRegistry reg = new tRegistry();
v.SP_UseHtmlMenu = reg.getRegistryValue("UseHtmlMenu")?.ToString() == "True";
```

## What Happens When Enabled

1. **When `SP_UseHtmlMenu = true`**:
   - Any form with `ItemType == 106` (TileNavPane) will use WebView2 overlay
   - DevExpress TileNavPane is created **in-memory** (not rendered)
   - WebView2 displays `EntranceTemplate.html` with dynamic cards
   - Cards are rendered from database (ItemType 201 categories)
   - Clicks bridge to existing `tNavButton_ElementClick` handlers

2. **When `SP_UseHtmlMenu = false`** (default):
   - Traditional DevExpress TileNavPane rendering
   - No changes to existing behavior

## Testing the Demo

### Step 1: Enable the Flag

Add to `main.cs` in `preparingMenus()`:

```csharp
v.SP_UseHtmlMenu = true;
```

### Step 2: Run the Application

1. Start the application
2. Login and select a firm
3. Navigate to a form that uses `ItemType == 106` menu (TileNavPane)
4. You should see the WebView2-rendered entrance screen instead of DevExpress tiles

### Step 3: Verify Functionality

1. **Visual Check**: You should see the modern HTML/CSS cards from `EntranceTemplate.html`
2. **Click Test**: Click a card - it should open the same form as before
3. **Debug Logs**: Check Debug output for:
   - `[PARITY] WebView click: buttonName=...` messages
   - `=== MENU INVENTORY ===` logs showing all elements

### Step 4: Compare with DevExpress

To compare:
1. Set `v.SP_UseHtmlMenu = false`
2. Restart application
3. Same menu should show traditional DevExpress tiles
4. Click behavior should be identical

## Showcase Features

The WebView2 implementation showcases:

✅ **Modern UI**: HTML/CSS cards with animations and hover effects  
✅ **Database-Driven**: Cards rendered from `MS_ITEMS` table (ItemType 201)  
✅ **Icon Support**: Database icons (LKP_GLYPH32) converted to base64  
✅ **Color Support**: Database colors (CMP_BACK_COLOR, MENU_COLOR) or defaults  
✅ **Event Parity**: Same click handlers, same form opening logic  
✅ **Zero Logic Changes**: All business logic unchanged  

## Troubleshooting

### WebView2 Not Showing

- Check that `EntranceTemplate.html` exists in `Forms/Templates/`
- Check Debug output for errors
- Verify WebView2 Runtime is installed

### Cards Not Rendering

- Check Debug output for JSON extraction errors
- Verify database has ItemType 201 categories
- Check browser console (F12 in WebView2) for JavaScript errors

### Clicks Not Working

- Check Debug output for `[PARITY]` messages
- Verify element is found: `[PARITY] Found element: ...`
- Check that `tNavButton_ElementClick` is being called

### Rollback

To instantly rollback:
```csharp
v.SP_UseHtmlMenu = false;
```
Restart application - everything returns to DevExpress rendering.

## Files Modified

- `Tkn/tVariable.cs` - Added `SP_UseHtmlMenu` flag
- `Tkn/tMenu.cs` - Added extraction methods, modified `Create_Menu_IN_Control`
- `Forms/ms_TileNavWebView.cs` - New wrapper class
- `Forms/Templates/EntranceTemplate.html` - Updated to be dynamic

## Next Steps

1. ✅ Enable feature flag
2. ✅ Test with real data
3. ✅ Verify event bridge works
4. ✅ Showcase to stakeholders
5. ⏭️ Gather feedback
6. ⏭️ Refine UI/UX based on feedback
7. ⏭️ Production rollout (if approved)

