# Splash Screen Implementation - Master Context

## Current State

The application uses a **WebView2-based splash screen** (`ms_WebViewSplash`) that displays an HTML template (`LoadingTemplate.html`) during application initialization. However, the splash screen is currently **not rendering properly** - it shows a white/empty screen instead of the expected UI with logo, loading indicators, and status text.

## Architecture

### Key Components

1. **`ms_WebViewSplash.cs`** - Main splash screen form using WebView2
   - Location: `YesiLdefter/Forms/ms_WebViewSplash.cs`
   - Uses WebView2 control to render HTML template
   - Has fallback to WinForms controls if WebView2 fails

2. **`LoadingTemplate.html`** - HTML/CSS/JavaScript template
   - Location: `YesiLdefter/Forms/Templates/LoadingTemplate.html`
   - Embedded resource: `YesiLdefter.Forms.Templates.LoadingTemplate.html`
   - Contains:
     - Logo image placeholder: `{{logo-base64}}{{asset-base}}yesildefter_horizontal.png`
     - Status text placeholder
     - Loading animations
     - Google Fonts (Inter Tight) via CDN

3. **Logo Asset**
   - Expected location: `YesiLdefter/Forms/Templates/public/yesildefter_horizontal.png`
   - Should be embedded as resource in `YesiLdefter.g.resources` or `Properties.Resources`
   - Currently extraction is failing

### Initialization Flow

1. **Program.cs** - Calls `ms_WebViewSplash.ShowSplash()` before creating main form
2. **tStarter.cs** - Calls `ms_WebViewSplash.UpdateStatus("...")` during initialization steps
3. **main.cs** - Calls `ms_WebViewSplash.UpdateStatus("...")` during login process
4. **ms_WebViewSplash.CloseSplash()** - Called after initialization completes

## Current Issues

### 1. Splash Screen Not Rendering
- **Symptom**: White/empty screen, no HTML content visible
- **Possible Causes**:
  - HTML template not loading correctly
  - External resources (Google Fonts) blocked
  - Logo extraction failing
  - WebView2 navigation issues
  - CSS/JavaScript errors

### 2. Logo Loading
- **Current Approach**: Tries multiple methods:
  1. `Properties.Resources` ResourceManager
  2. Search all embedded resources by pattern
  3. ResourceSet from `.resources` files
  4. Direct stream reading
  5. File system fallback
- **Status**: Logo extraction is failing - ResourceSet only finds `resources/xsltfile.xslt`, not the logo
- **Debug Output**: `⚠️ Logo image not found in ResourceSet. Available keys: - resources/xsltfile.xslt`

### 3. Navigation Method
- **Current**: Using `NavigateToString()` to allow external resources (Google Fonts)
- **Issue**: Logo must be embedded as base64 data URI since `NavigateToString()` can't access local files
- **Previous**: Used `file://` protocol with `Navigate()` but this blocked external resources

## Key Code Sections

### Template Loading (ms_WebViewSplash.cs)

**Embedded Resource Path** (lines ~185-343):
- Loads template from embedded resource: `YesiLdefter.Forms.Templates.LoadingTemplate.html`
- Extracts logo using multiple fallback methods
- Embeds logo as base64 data URI if extracted
- Uses `NavigateToString()` to render

**File System Path** (lines ~350-520):
- Falls back to file system if embedded resource not found
- Looks for template at: `Forms/Templates/LoadingTemplate.html` or `Templates/LoadingTemplate.html`
- Same logo extraction and base64 embedding logic

### Logo Extraction Methods

1. **Properties.Resources** (preferred):
   ```csharp
   var resourceManager = new ResourceManager("YesiLdefter.Properties.Resources", assembly);
   var logoImage = resourceManager.GetObject("yesildefter_horizontal") as Image;
   ```

2. **ResourceSet from .resources files**:
   ```csharp
   using (var resourceSet = new ResourceSet(stream))
   {
       var logoImage = resourceSet.GetObject("yesildefter_horizontal") as Image;
   }
   ```

3. **Direct stream** (for direct PNG resources):
   ```csharp
   byte[] buffer = new byte[stream.Length];
   stream.Read(buffer, 0, buffer.Length);
   File.WriteAllBytes(tempLogoPath, buffer);
   ```

### Status Updates

- `ms_WebViewSplash.ShowSplash()` - Shows splash screen
- `ms_WebViewSplash.UpdateStatus("text")` - Updates status text via JavaScript injection
- `ms_WebViewSplash.CloseSplash()` - Hides/closes splash screen

## Debug Output Clues

From recent runs:
```
[Splash] WebView2 initialized and ready
[Splash] Template not found on disk. Trying embedded resource...
[Splash] YesiLdefter.g.resources stream opened
[Splash] ResourceSet created, searching for logo keys...
[Splash] 'yesildefter_horizontal' not found, trying 'yesildefter_horizontal_color'
[Splash] ⚠️ Logo image not found in ResourceSet. Available keys:
[Splash]   - resources/xsltfile.xslt
[Splash] ⚠️ Template updated: logo src removed (logo not extracted)
[Splash] Template written to temp file: ... (18467 bytes)
[Splash] Navigated using NavigateToString (allows external resources)
[Splash] DOM content loaded
[Splash] Navigation completed: Success=True
```

## What Needs to Be Fixed

### Priority 1: Splash Screen Rendering
- [ ] Verify HTML template is loading correctly
- [ ] Check if CSS/JavaScript is executing
- [ ] Verify external resources (Google Fonts) are loading
- [ ] Check WebView2 console for errors
- [ ] Ensure `NavigateToString()` is working properly

### Priority 2: Logo Loading
- [ ] Verify logo is actually embedded in the assembly
- [ ] Check correct resource key name (might be different than expected)
- [ ] Verify logo file exists in source: `Forms/Templates/public/yesildefter_horizontal.png`
- [ ] Check if logo needs to be added to project as embedded resource
- [ ] Test logo extraction from Properties.Resources

### Priority 3: Status Updates
- [ ] Verify `UpdateStatus()` JavaScript injection is working
- [ ] Check if status text element exists in HTML template
- [ ] Ensure WebView2 is ready before injecting JavaScript

## Related Files

- `YesiLdefter/Forms/ms_WebViewSplash.cs` - Main splash screen implementation
- `YesiLdefter/Forms/Templates/LoadingTemplate.html` - HTML template
- `YesiLdefter/Forms/Templates/public/yesildefter_horizontal.png` - Logo file (should exist)
- `YesiLdefter/Program.cs` - Application entry point (calls ShowSplash)
- `YesiLdefter/Tkn/tStarter.cs` - Initialization (calls UpdateStatus)
- `YesiLdefter/main.cs` - Main form (calls UpdateStatus)
- `YesiLdefter/Forms/ms_User_Standalone.cs` - Login form (has working logo loading example)

## Working Reference

The login form (`ms_User_Standalone.cs`) has a working logo loading implementation:
- Method: `LoadLogoFromEmbeddedResource()` (line ~1170)
- Successfully loads logo from embedded resources
- Converts to base64 for use in HTML
- Uses similar resource search patterns

## Next Steps

1. **Debug HTML rendering**:
   - Add WebView2 console message handler to see JavaScript errors
   - Verify template HTML structure is correct
   - Check if CSS is loading/applying

2. **Fix logo extraction**:
   - Verify logo file exists in project
   - Check if it's marked as embedded resource
   - Test Properties.Resources approach first
   - Verify resource key names match

3. **Test incrementally**:
   - Start with minimal HTML (no external resources)
   - Add logo once basic rendering works
   - Add external resources last

## Notes

- WebView2 requires Microsoft Edge WebView2 Runtime
- `NavigateToString()` allows external resources but can't access local files
- Logo must be embedded as base64 data URI when using `NavigateToString()`
- File system path fallback exists but may not be used in production
- Debug output shows navigation succeeds but content doesn't render

