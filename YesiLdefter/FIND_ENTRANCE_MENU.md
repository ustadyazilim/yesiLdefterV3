# Finding the Correct Entrance Screen Menu

## Problem

The current menu `UST/PMS/PMS/MSBOS` has **0 categories**, so no cards are displayed. The entrance screen (MEBBIS İşlem Paneli) should have **9 categories** (ItemType 201).

## How to Find the Correct Menu

### Option 1: Check Debug Output

When you run the application, look for this message in the Debug Output:

```
[MENU] ✅ This menu likely IS the entrance screen (has X categories)
```

The menu with **9 categories** is the entrance screen.

### Option 2: Database Query

Run this SQL query to find menus with categories:

```sql
-- Find menus with ItemType 106 (TileNavPane) that have ItemType 201 (Category) items
SELECT 
    m.MENU_CODE,
    m.CAPTION,
    m.MENU_TYPE,
    COUNT(i.REF_ID) as CategoryCount
FROM MS_MENU m
LEFT JOIN MS_ITEMS i ON i.MENU_CODE = m.MENU_CODE 
    AND i.ITEM_TYPE = 201 
    AND (i.ITEM_NAME IS NULL OR i.ITEM_NAME = '')
WHERE m.MENU_TYPE = 106
GROUP BY m.MENU_CODE, m.CAPTION, m.MENU_TYPE
HAVING COUNT(i.REF_ID) > 0
ORDER BY CategoryCount DESC;
```

**Look for:** A menu with **9 categories** - that's the entrance screen.

### Option 3: Visual Identification

1. Run the application
2. Login and select a firm
3. Look at the screen that appears - it should have 9 large cards/tiles
4. Check the Debug Output for the MenuCode of that screen

### Option 4: Check Application Flow

The entrance screen is typically:
- The first screen after firm selection
- Has 9 cards showing: Kursiyer Kayıt, Dönem İşlemleri, Teorik Ders, e-Sınav, etc.
- MenuCode might be something like `UST/MEB/MTS/...` or similar

## Current Status

**Current Menu:** `UST/PMS/PMS/MSBOS`
- Categories: 0
- Buttons: 1 (Exit button only)
- **This is NOT the entrance screen**

## What to Do

1. **Run the application** and check Debug Output
2. **Look for** a menu with 9 categories
3. **Note the MenuCode** of that menu
4. **Share the MenuCode** so we can verify it's the correct one

## Expected Debug Output

When you find the correct menu, you should see:

```
[MENU] ItemType 106 detected. MenuCode=XXX/XXX/XXX/XXXX, SP_UseHtmlMenu=True
[MENU] MenuCode=XXX/XXX/XXX/XXXX has 9 categories (ItemType 201) in DataSet
[MENU] ✅ This menu likely IS the entrance screen (has 9 categories)
[WebView2] Extracted from TileNavPane. JSON length: [large number]
[WebView2] HTML template loaded. MenuCode=XXX/XXX/XXX/XXXX, Cards in JSON: True
=== MENU INVENTORY ===
Total: 9 categories, X items, X buttons
```

## Next Steps

Once you identify the correct MenuCode:
1. The WebView2 menu should automatically work (if it's ItemType 106)
2. You should see 9 cards displayed
3. If not, share the MenuCode and we'll investigate

---

**Note:** The feature flag `v.SP_UseHtmlMenu = true` is already enabled in `main.cs`. All ItemType 106 menus will use WebView2. The entrance screen just needs to be identified.

