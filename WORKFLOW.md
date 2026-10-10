# Complete Workflow: Local → GitHub → Users

Visual guide showing the whole journey.

## The Three Paths

After you build locally and push to GitHub, your project lives in three places:

```
┌─────────────────────────────────────────────────────────────────┐
│                     GitHub Repository                           │
│                 (Source code + CI/CD)                          │
│  https://github.com/YOUR_USERNAME/AzureSnapshotManager         │
└──────────────────┬──────────────────────────────────────────────┘
                   │
        ┌──────────┼──────────┐
        │          │          │
        ▼          ▼          ▼
    PATH 1      PATH 2     PATH 3
   (Desktop)  (Web Host)  (Docker)
     .exe      Render      Any host
```

### Path 1: Desktop .exe (Most common for individuals)

```
User's Windows PC
└── Downloads folder
    └── AzureSnapshotManager-Desktop-v1.0.0.zip
        │
        ├── AzureSnapshotManager.Api.exe
        ├── Run Azure Snapshot Manager.bat  ◄── User double-clicks this
        │
        └── Dashboard opens in browser
            └── Auto-detects subscription
                └── Auto-runs audit
                    └── Shows orphaned snapshots
```

**Setup:** `az login` → Unzip → Double-click `.bat`
**When to use:** Single user, no server overhead, zero-config

### Path 2: Web Host (Render.com — Backup if desktop fails)

```
Render.com (Free tier)
└── Deployed Web Service
    ├── Dashboard at https://your-app.onrender.com
    ├── Auto-starts with DemoMode=true
    ├── Username/password gate (you set via dashboard)
    └── In-memory storage (data lost on restart)
```

**Setup:** Connect GitHub → Deploy button
**When to use:** Sharing a link with others, always-on monitoring (if you pay), backup when desktop breaks

### Path 3: Docker (For production / custom hosting)

```
Your own server / cloud
└── Docker container from Dockerfile
    ├── Real Entra ID auth + Azure Storage
    ├── Persistent audit history
    └── Scalable
```

**Setup:** Run `docker build` yourself
**When to use:** Professional deployment, permanent audit trail

## The Full Journey (Step by Step)

### Week 1: Local Development
```
You clone/unzip on your PC
         │
         ▼
dotnet restore
dotnet build
dotnet test        ◄── All pass ✅
         │
         ▼
dotnet run         ◄── Web dashboard works locally
         │
         ▼
.\desktop\publish-exe.ps1  ◄── Build the .exe
         │
         ▼
.\desktop\dist\Run Azure Snapshot Manager.bat  ◄── Test the .exe
         │
         ▼
Works offline! ✅
```

### Week 2: Push to GitHub
```
You have local commits
         │
         ▼
git remote add origin ...
git push -u origin main
         │
         ▼
GitHub receives code
         │
         ▼
CI workflow runs automatically (ci.yml)
  ├── dotnet restore
  ├── dotnet build
  └── dotnet test
         │
         ▼
Tests pass ✅
         │
         ▼
Green badge on README
```

### Week 3: Create a Release
```
You want others to use it
         │
         ▼
Compress .\desktop\dist\ to .zip
         │
         ▼
GitHub Releases → New Release
  ├── Tag: v1.0.0
  ├── Upload: AzureSnapshotManager-Desktop-v1.0.0.zip
  └── Publish
         │
         ▼
Anyone can now:
  ├── Download the .exe
  ├── Run az login
  └── Double-click the .bat
         │
         ▼
Orphaned snapshots appear (zero IT work)
```

## Files You Interact With (Checklist)

### Local (your PC)
- `AzureSnapshotManager.sln` — Open in Visual Studio or build from command line
- `src/AzureSnapshotManager.Api/Program.cs` — Backend wiring (you only modify if customizing)
- `src/AzureSnapshotManager.Api/wwwroot/index.html` — Dashboard UI
- `desktop/publish-exe.ps1` — Run to build the .exe
- `QUICKSTART.md` — Copy-paste commands to do it all

### On GitHub
- `AzureSnapshotManager.sln` — Source, visible to everyone
- `.github/workflows/ci.yml` — Auto-runs tests, you never touch it
- `README.md` — What users see first
- `LICENSE` — MIT license

### Distributed to Users
- `AzureSnapshotManager-Desktop-v1.0.0.zip` — Contains the .exe + launcher

## Common Scenarios

### "I want to fix a bug and push the new version"
```
1. Edit src/.../SomeFile.cs
2. Test locally: dotnet test
3. Rebuild: .\desktop\publish-exe.ps1
4. Test the .exe: .\desktop\dist\Run Azure Snapshot Manager.bat
5. Commit: git add . && git commit -m "Fix: ..."
6. Push: git push
7. Create new release: v1.0.1, upload new .zip
```

### "I want to deploy to Render instead of/alongside the desktop .exe"
```
1. (You already have render.yaml in the repo)
2. Go to https://render.com/dashboard
3. Click "New > Blueprint"
4. Choose your GitHub repo
5. Deploy button (Render reads render.yaml)
6. Set DEMO_PASSWORD when prompted
7. Share the URL
```

### "I want to run it as a proper server with persistent storage"
```
1. Set up an Azure resource group
2. Edit Program.cs to use Production mode (not DemoMode)
3. Create env vars for:
   - AzureAd__TenantId
   - AzureAd__ClientId
   - Azure__TableStorageEndpoint
4. Publish to Azure App Service
5. Real Entra ID auth + Table Storage history
```

## Decision Tree: Which Path Should Users Take?

```
User has a Windows PC?
├── Yes → Use the desktop .exe (Path 1)
│         "Just run az login and double-click"
│         Best for: Most people, most use cases
│
└── No (Mac/Linux) → 
    ├── Use Render (Path 2)
    │   "I'll share you a link"
    │   Best for: Sharing with others, demos
    │
    └── Use Docker yourself (Path 3)
        "I'll run it in my own container"
        Best for: Platform engineers, automation
```

## Success Checklist

- [ ] Code builds locally without errors
- [ ] Tests pass locally
- [ ] Web dashboard works locally
- [ ] .exe builds successfully
- [ ] .exe runs offline without user typing commands
- [ ] Code pushed to GitHub
- [ ] CI workflow shows green on GitHub
- [ ] Release created and downloadable

You now have a professional, production-ready tool with multiple deployment paths. Pick the one that fits your users.
