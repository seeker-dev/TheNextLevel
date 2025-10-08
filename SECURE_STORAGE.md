# Secure Credential Storage

This application uses platform-specific secure storage to protect your database credentials.

## How It Works

### Secure Storage
- **Windows**: Windows Credential Manager
- **macOS**: Keychain
- **Android**: Android Keystore
- **iOS**: iOS Keychain

All credentials are encrypted and stored outside the application directory, never in source code or configuration files.

## Setup Instructions

### Initial Setup

1. **Launch the app**
2. **Click the settings icon (⚙️)** in the top-right header
3. **Select database provider**:
   - `SQLite` - Local database (default, no setup needed)
   - `Azure SQL Server` - Cloud database

4. **For Azure SQL Server**:
   - Paste your connection string from Azure Portal
   - Click "Test Connection" to verify
   - Click "Save Settings"
   - **Restart the app** for changes to take effect

### Getting Azure Connection String

1. Go to Azure Portal (portal.azure.com)
2. Navigate to your SQL Database
3. Click "Connection strings" in the left menu
4. Copy the **ADO.NET** connection string
5. Replace `{your_password}` with your actual password

Example format:
```
Server=tcp:yourserver.database.windows.net,1433;Initial Catalog=YourDB;User ID=yourusername;Password=yourpassword;Encrypt=True;TrustServerCertificate=False;
```

### Azure Firewall Configuration

**Important**: Your Azure SQL Server must allow connections from your IP address.

1. In Azure Portal, go to your SQL Server (not database)
2. Click "Networking" or "Firewalls and virtual networks"
3. Add your client IP address
4. Or enable "Allow Azure services and resources to access this server"

## Migration from appsettings.json

If you previously stored credentials in `appsettings.json`, the app automatically migrates them to secure storage on first launch. Your credentials are then safely stored in platform-specific secure storage.

## Security Features

✅ **Encrypted Storage**: All credentials encrypted by OS
✅ **No Git Commits**: Credentials never in source control
✅ **Platform Integration**: Uses native security features
✅ **Test Before Save**: Verify connections before storing
✅ **Easy Clear**: Remove all credentials with one click

## Troubleshooting

### Connection Test Fails
- Verify connection string format
- Check Azure firewall settings
- Ensure password doesn't contain `{your_password}` placeholder
- Confirm database exists and is running

### Settings Don't Apply
- Restart the app after saving settings
- Database context is initialized at startup

### Clear All Settings
Use the "Clear All Settings" button in Settings page to reset to defaults (SQLite).

## Production Deployment

For production MAUI apps:
- Users configure their own connection strings via Settings UI
- Never embed production credentials in the app
- Consider implementing a backend API for data access
- Use managed identities when deploying to Azure

## Environment Variables (Alternative)

For automated deployments, you can also use environment variables with prefix `TNL_`:

```bash
TNL_DatabaseSettings__Provider=SqlServer
TNL_DatabaseSettings__ConnectionStrings__SqlServer=YourConnectionString
```

These override secure storage and appsettings.json.
