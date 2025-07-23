# ?? Troubleshooting Guide - Nicolas Qui Paie

## ?? Fixing 404 Errors on Startup

### Quick Fix Checklist

#### ? **1. Start Both Projects**
The Blazor WebAssembly app depends on the API. You need both running:

```bash
# Terminal 1 - Start API
cd NicolasQuiPaieAPI
dotnet run

# Terminal 2 - Start Web
cd NicolasQuiPaieWeb  
dotnet run
```

**Or in Visual Studio:**
1. Right-click Solution ? Properties
2. Set "Multiple startup projects"
3. Set both `NicolasQuiPaieAPI` and `NicolasQuiPaieWeb` to "Start"

#### ? **2. Check API Connectivity**
1. Navigate to: `https://localhost:5001/diagnostics`
2. Check API status - should show "? Connecté"
3. If API is down, see troubleshooting below

#### ? **3. Verify Ports**
Default configuration:
- **API**: `https://localhost:7051`
- **Web**: `https://localhost:5001`

Check `NicolasQuiPaieWeb/wwwroot/appsettings.json`:
```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:7051"
  }
}
```

---

## ?? Common Issues & Solutions

### ?? **Issue: "Failed to fetch" or API Errors**

**Symptoms:**
- Home page shows spinning loader indefinitely
- Console shows CORS or connection errors
- Diagnostics page shows "? API non disponible"

**Solutions:**
1. **Start the API project first**
   ```bash
   cd NicolasQuiPaieAPI
   dotnet run
   ```

2. **Check API is responding**
   - Visit: `https://localhost:7051/swagger`
   - Should see Swagger documentation

3. **Verify CORS configuration in API**
   - Check `Program.cs` in API project
   - Ensure Blazor origin is allowed

### ?? **Issue: Blazor App Shows 404**

**Symptoms:**
- White page with "404 Not Found"
- Browser console shows routing errors

**Solutions:**
1. **Clear browser cache**
   - Hard refresh: `Ctrl+Shift+R` (Chrome/Edge)
   - Or clear all browsing data

2. **Check base href in index.html**
   ```html
   <base href="/" />
   ```

3. **Verify App.razor routing**
   - Router should be configured properly
   - Check for missing `@page` directives

### ?? **Issue: Authentication Errors**

**Symptoms:**
- Login doesn't work
- JWT token errors in console
- "Unauthorized" responses

**Solutions:**
1. **Clear Local Storage**
   - F12 ? Application ? Local Storage ? Clear
   - Or use Diagnostics page "Vider le Cache"

2. **Check JWT configuration**
   - Same secret key in both projects
   - Correct token expiration

---

## ?? **Development Setup**

### **Visual Studio Setup:**
1. Set multiple startup projects:
   - `NicolasQuiPaieAPI` (Start)
   - `NicolasQuiPaieWeb` (Start)

2. Configure ports in `launchSettings.json`:
   ```json
   {
     "NicolasQuiPaieAPI": "https://localhost:7051",
     "NicolasQuiPaieWeb": "https://localhost:5001"
   }
   ```

### **Command Line Setup:**
```bash
# Start API (Terminal 1)
cd NicolasQuiPaieAPI
dotnet watch run

# Start Web (Terminal 2)  
cd NicolasQuiPaieWeb
dotnet watch run
```

---

## ?? **Built-in Diagnostics**

### **Diagnostics Page**
Visit `https://localhost:5001/diagnostics` for:
- ? API connectivity status
- ?? Configuration verification  
- ?? Troubleshooting steps
- ?? Cache clearing tools

### **Fallback Mode**
The app works in "degraded mode" when API is unavailable:
- Shows demo data
- Yellow warning banner
- Limited functionality
- Retry connection button

---

## ?? **Debug Mode Features**

### **Enhanced Error Handling:**
- Better error messages in console
- Graceful API failure handling
- Automatic retry mechanisms
- User-friendly error notifications

### **Development Tools:**
- Detailed logging in browser console
- API health monitoring
- Connection status indicators
- Configuration verification

---

## ?? **Still Having Issues?**

### **Check These Logs:**
1. **Browser Console** (F12)
   - Network tab for failed requests
   - Console tab for JavaScript errors

2. **API Logs** (Terminal/Visual Studio)
   - Connection errors
   - CORS issues
   - Authentication failures

3. **Blazor Server Logs**
   - SignalR connection issues
   - Routing problems

### **Emergency Reset:**
```bash
# Nuclear option - clean everything
dotnet clean
dotnet restore
# Clear browser cache completely
# Restart Visual Studio
dotnet build
dotnet run
```

---

## ? **Features When Working:**

? **Live Home Page** with real-time stats  
? **Trending Proposals** from API  
? **User Authentication** with JWT  
? **Voting System** with real vote counts  
? **Analytics Dashboard** with live data  
? **Comment System** with nested replies  
? **Badge Progression** based on activity  

---

*?? Nicolas Qui Paie - La démocratie souveraine numérique !*