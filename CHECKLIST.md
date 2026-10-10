# Quick Checklist: Local Build → GitHub

Copy-paste and check off as you go.

## Prerequisites
- [ ] `dotnet --version` (shows 8.x.x)
- [ ] `git --version` (shows 2.x.x)
- [ ] `az --version` (shows latest)
- [ ] `az login` (signed in to Azure)
- [ ] `az account show` (can see your subscription)
- [ ] `git config --global user.name "Your Name"` (configured)

## Part 1: Verify Build
```powershell
cd path\to\AzureSnapshotManager
dotnet restore AzureSnapshotManager.sln
dotnet build AzureSnapshotManager.sln
dotnet test AzureSnapshotManager.sln
```
- [ ] Restore succeeded
- [ ] Build succeeded
- [ ] Tests passed (5 passed)

## Part 2: Run Dashboard Locally
```powershell
cd src\AzureSnapshotManager.Api
dotnet run
```
- [ ] Server listening on http://localhost:5080
- [ ] Browser opened automatically
- [ ] Pasted a real subscription ID
- [ ] Clicked "Run audit"
- [ ] Saw findings appear (~10 seconds)
- [ ] Pressed Ctrl+C to stop

## Part 3: Build Desktop .exe
```powershell
.\desktop\publish-exe.ps1
```
- [ ] Build completed without errors
- [ ] `.\desktop\dist\` folder exists
- [ ] `AzureSnapshotManager.Api.exe` exists (~80-100 MB)
- [ ] `Run Azure Snapshot Manager.bat` exists

## Part 4: Test the .exe
```powershell
cd .\desktop\dist
.\Run Azure Snapshot Manager.bat
```
- [ ] Browser opened automatically
- [ ] Dashboard auto-detected subscription
- [ ] Audit ran automatically
- [ ] Orphaned snapshots appeared
- [ ] Closed the PowerShell window to stop

## Part 5: Commit & Push to GitHub

### Create empty repo on GitHub
- [ ] Went to https://github.com/new
- [ ] Named it `AzureSnapshotManager`
- [ ] Left everything else empty
- [ ] Clicked "Create repository"
- [ ] Copied the HTTPS URL

### Push code
```powershell
git remote add origin https://github.com/YOUR_USERNAME/AzureSnapshotManager.git
git branch -M main
git push -u origin main
```
- [ ] First `git push` succeeded (or used Personal Access Token when prompted)
- [ ] Went to GitHub repo URL
- [ ] Saw all files appear in `main` branch
- [ ] Went to Actions tab
- [ ] CI workflow ran and turned green ✅

## Part 6: Create a Release (optional but recommended)
```powershell
cd .\desktop
Compress-Archive -Path dist -DestinationPath AzureSnapshotManager-Desktop-v1.0.0.zip
```
- [ ] Zip file created in `desktop/` folder
- [ ] Went to Releases on GitHub
- [ ] Created new release with tag `v1.0.0`
- [ ] Uploaded the `.zip` file
- [ ] Clicked "Publish release"

## Done! 🎉

You now have:
- ✅ A working dashboard (local)
- ✅ A standalone `.exe` (for offline use)
- ✅ A GitHub repo with CI/CD (for updates)
- ✅ A downloadable Release (for sharing)

Share the GitHub Release link with anyone who wants to try it.
