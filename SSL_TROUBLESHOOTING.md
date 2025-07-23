# ?? Fixing "Your connection is not private" Errors

This guide helps you resolve SSL certificate issues when running the Nicolas Qui Paie projects in development.

## ?? Quick Solutions

### Option 1: Use HTTP-only Development (Easiest)
Run projects with the HTTP-only profiles:
```bash
# For Web project
cd NicolasQuiPaieWeb
dotnet run --launch-profile http-dev

# For API project  
cd NicolasQuiPaieAPI
dotnet run --launch-profile http-dev
```

URLs will be:
- **NicolasQuiPaieWeb**: http://localhost:5208
- **NicolasQuiPaieAPI**: http://localhost:5170

### Option 2: Fix SSL Certificates (Recommended for full testing)

#### Windows (Run as Administrator):
```powershell
# Run the provided script
.\Fix-SslCertificates.ps1

# Or manually:
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

#### macOS:
```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

#### Linux:
```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

### Option 3: Browser Bypass (Temporary)

When you see "Your connection is not private":

#### Chrome/Edge:
1. Click anywhere on the page
2. Type `thisisunsafe` (no input field needed)
3. Page will load automatically

#### Firefox:
1. Click "Advanced"
2. Click "Accept the Risk and Continue"

## ?? Project URLs

### HTTPS (with valid certificates):
- **NicolasQuiPaieWeb**: https://localhost:7084
- **NicolasQuiPaieAPI**: https://localhost:7051

### HTTP (no SSL issues):
- **NicolasQuiPaieWeb**: http://localhost:5208  
- **NicolasQuiPaieAPI**: http://localhost:5170

## ?? Configuration Files

### For HTTP-only development:
The projects are configured with:
- HTTP-only launch profiles (`http-dev`)
- Configurable HTTPS redirection
- Development-friendly CORS policies

### appsettings files:
- `appsettings.Development.json` - HTTPS configuration
- `appsettings.Development.Http.json` - HTTP-only configuration

## ?? Troubleshooting

### Certificate Issues:
```powershell
# Check certificate status
dotnet dev-certs https --check --trust

# Reset certificates completely
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

### Port Conflicts:
If ports are in use, you can change them in:
- `Properties/launchSettings.json` files
- `appsettings.Development.json` files

### CORS Issues:
The API is configured to allow all localhost origins in development mode.

## ?? Visual Studio Configuration

### To run HTTP-only in Visual Studio:
1. Right-click project ? Properties
2. Debug tab ? Launch profiles
3. Select "http-dev" profile
4. Set as default

### To run multiple projects:
1. Right-click Solution ? Properties
2. Startup Project ? Multiple startup projects
3. Set both projects to "Start"
4. Choose HTTP profiles for both

## ?? Production Notes

In production:
- Always use HTTPS
- SSL certificates are handled by hosting platform
- CORS is restricted to specific domains
- No development bypasses are active

## ? Quick Test

After applying fixes:
1. Start both projects
2. Navigate to web application
3. Test API integration at `/api-example`
4. Verify no SSL warnings

---

**?? Tip**: For development, HTTP-only mode is often the fastest way to get started without certificate hassles!