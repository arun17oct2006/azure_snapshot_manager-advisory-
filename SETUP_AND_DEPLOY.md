# Setup, Build, Test & Deploy Guide

Complete walkthrough for your Windows PC.

## Part 1: Prerequisites (one-time setup)

### 1.1 Install required tools

**Check if you already have these:**
```powershell
dotnet --version
git --version
az --version
```

If any are missing:

- **.NET 8 SDK** — download from https://dotnet.microsoft.com/download
  - Run the installer, restart your PC
  - Verify: `dotnet --version` (should show 8.x.x)

- **Git** — download from https://git-scm.com/download/win
  - Run the installer (default options are fine)
  - Restart PowerShell/cmd
  - Verify: `git --version`

- **Azure CLI** — download from https://aka.ms/cli
  - Run the installer
  - Verify: `az --version`

### 1.2 Sign in to Azure

```powershell
az login
```

A browser opens → sign in with your Azure account → close the browser. Your session is saved.

Verify which subscription is active:
```powershell
az account show
```

If you have multiple subscriptions and want to use a different one:
```powershell
az account list --output table
az account set --subscription "NAME_OR_ID"
```

### 1.3 Set up Git on your PC

Tell Git who you are (one-time):
```powershell
git config --global user.name "Your Name"
git config --global user.email "your.email@example.com"
```

## Part 2: Download the project

### Option A: Clone from GitHub (if you already pushed it)
```powershell
git clone https://github.com/YOUR_USERNAME/AzureSnapshotManager.git
cd AzureSnapshotManager
```

### Option B: Use the zip from this conversation
```powershell
# Extract the zip to a folder, then:
cd path\to\AzureSnapshotManager
git init
git add .
git commit -m "Initial commit"
```

## Part 3: Verify the build (no Azure resources needed yet)

```powershell
# Restore all NuGet packages
dotnet restore AzureSnapshotManager.sln

# Build (should show "Build succeeded")
dotnet build AzureSnapshotManager.sln

# Run tests (should show "5 passed")
dotnet test AzureSnapshotManager.sln
```

If any step fails, stop here and share the error message.

## Part 4: Run the dashboard locally

```powershell
cd src\AzureSnapshotManager.Api
dotnet run
```

You should see:
```
Now listening on: http://localhost:5080
Now listening on: https://localhost:7001
```

Open your browser to `http://localhost:5080/` (or `https://localhost:7001/`)

**Test it:**
1. Paste a real subscription ID into the top bar
2. Click **Run audit**
3. Wait ~10 seconds
4. You should see:
   - Stat row: count of orphaned snapshots, daily/monthly savings
   - Findings table: list of orphaned snapshots with portal links
   - Retention rules panel

**Stop the server:** Press `Ctrl+C` in PowerShell

## Part 5: Build the desktop .exe

```powershell
# From the repo root:
.\desktop\publish-exe.ps1
```

This takes 30-60 seconds. You should see:
```
Publishing self-contained single-file exe (win-x64)...
Done. Distributable folder: C:\...\desktop\dist
```

Check what was built:
```powershell
dir .\desktop\dist
```

You should see:
- `AzureSnapshotManager.Api.exe` (~80-100 MB)
- `Run Azure Snapshot Manager.bat`
- A few other runtime files

## Part 6: Test the .exe locally

```powershell
cd .\desktop\dist
.\Run Azure Snapshot Manager.bat
```

Your browser should open automatically to `http://127.0.0.1:<random-port>/`

The dashboard should:
1. Auto-detect your subscription
2. Show a toast: "Auditing..."
3. Auto-run the audit
4. Display orphaned snapshots

Close the PowerShell window to stop the exe.

## Part 7: Prepare to push to GitHub

### 7.1 Make sure everything is committed locally

```powershell
cd path\to\AzureSnapshotManager
git status
```

Should show:
```
On branch main
nothing to commit, working tree clean
```

If you see uncommitted changes, commit them:
```powershell
git add .
git commit -m "Local build, ready for GitHub"
```

### 7.2 Create a GitHub repo (if you haven't already)

1. Go to https://github.com/new
2. Repository name: `AzureSnapshotManager`
3. **Leave everything else empty** (don't add README, .gitignore, license — you already have them)
4. Click **Create repository**
5. You'll see a page with commands — copy the HTTPS URL (looks like `https://github.com/YOUR_USERNAME/AzureSnapshotManager.git`)

### 7.3 Push your code to GitHub

```powershell
git remote add origin https://github.com/YOUR_USERNAME/AzureSnapshotManager.git
git branch -M main
git push -u origin main
```

If you get an auth error:
- GitHub no longer accepts passwords — you need a **Personal Access Token**
- Generate one at https://github.com/settings/tokens (scope: `repo`)
- When `git push` asks for a password, paste the token instead

Watch GitHub Actions run:
1. Go to https://github.com/YOUR_USERNAME/AzureSnapshotManager/actions
2. You should see a workflow running (the `ci.yml` you included)
3. Wait for it to turn green ✅ (means tests passed)

## Part 8: Create a Release (share the .exe)

Once tests pass on GitHub:

1. Go to https://github.com/YOUR_USERNAME/AzureSnapshotManager/releases
2. Click **Create a new release**
3. Tag version: `v1.0.0`
4. Release title: `Azure Snapshot Manager Desktop v1.0.0`
5. Description:
   ```
   Offline Windows desktop tool for detecting orphaned Azure snapshots.
   
   **Setup:**
   1. Run `az login` once
   2. Unzip this folder
   3. Double-click `Run Azure Snapshot Manager.bat`
   
   Dashboard opens automatically, detects your subscription, runs audit.
   ```
6. **Attach files:** Drag your `desktop/dist/` folder as a zip into the upload area
   ```powershell
   # From repo root, in PowerShell:
   cd desktop
   Compress-Archive -Path dist -DestinationPath AzureSnapshotManager-Desktop-v1.0.0.zip
   ```
   Then attach that `.zip` to the release.

7. Click **Publish release**

Now anyone can download your `.exe` from GitHub Releases without needing to clone the repo or build it themselves.

## Troubleshooting

### "Build failed" or package errors
```powershell
dotnet clean AzureSnapshotManager.sln
dotnet restore AzureSnapshotManager.sln
dotnet build AzureSnapshotManager.sln
```

### "Could not list Azure subscriptions" error when running
Make sure `az login` session is still active:
```powershell
az account show
```
If it fails, re-run `az login`.

### Browser doesn't auto-open in the .exe
The exe still runs fine — just manually go to the URL shown in the console window (looks like `http://127.0.0.1:12345/`).

### Git push fails with "repository not found"
Make sure:
1. You created the empty repo on GitHub first
2. The URL in `git remote -v` matches exactly (check for typos)
3. You're using a Personal Access Token, not your password (if prompted)

## What you've accomplished

✅ Built the entire C# project locally  
✅ Ran tests  
✅ Verified the web dashboard works  
✅ Packaged it as a standalone `.exe`  
✅ Pushed everything to GitHub  
✅ Set up CI (tests run automatically on every push)  
✅ Created a Release for distribution

You now have:
- A professional GitHub repo with passing tests
- A downloadable Windows `.exe` that needs nothing but `az login` beforehand
- An automated Render deployment as a backup (from the earlier steps)

All three work independently — your users can pick whichever fits them.
