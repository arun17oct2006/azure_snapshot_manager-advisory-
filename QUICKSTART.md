# Quick Start (Copy-Paste Commands)

**Assumes:** .NET 8, Git, Azure CLI installed, and `az login` already done.

## 1. Build & test locally
```powershell
cd path\to\AzureSnapshotManager
dotnet restore AzureSnapshotManager.sln
dotnet build AzureSnapshotManager.sln
dotnet test AzureSnapshotManager.sln
```

## 2. Run the web dashboard
```powershell
cd src\AzureSnapshotManager.Api
dotnet run
```
Open browser to `http://localhost:5080/` → paste subscription ID → click "Run audit"

Press `Ctrl+C` to stop.

## 3. Build the .exe
```powershell
.\desktop\publish-exe.ps1
```

## 4. Test the .exe
```powershell
cd .\desktop\dist
.\Run Azure Snapshot Manager.bat
```
Browser opens → auto-detects subscription → runs audit. Close the window to stop.

## 5. Push to GitHub

### Create empty repo on GitHub first
- Go to https://github.com/new
- Name: `AzureSnapshotManager`
- Click **Create repository** (leave everything else empty)
- Copy the HTTPS URL

### Then:
```powershell
git remote add origin https://github.com/YOUR_USERNAME/AzureSnapshotManager.git
git branch -M main
git push -u origin main
```

(If prompted for password, use a Personal Access Token from https://github.com/settings/tokens)

## 6. Create a release (share the .exe)
```powershell
cd .\desktop
Compress-Archive -Path dist -DestinationPath AzureSnapshotManager-Desktop-v1.0.0.zip
```

Then on GitHub:
- Go to **Releases**
- **New release**
- Tag: `v1.0.0`
- Upload the `.zip` file
- Click **Publish**

Done. Share the GitHub release link.
