# Troubleshooting

## Build Problems

### "dotnet: command not found"
**.NET 8 SDK is not installed or not in your PATH.**

Fix:
1. Download .NET 8 from https://dotnet.microsoft.com/download
2. Run the installer
3. **Restart PowerShell completely** (close and re-open)
4. Try `dotnet --version` again

### "Build failed: NU1605 package downgrade"
**Package version mismatch.** Shouldn't happen with the latest zip, but if it does:

```powershell
dotnet clean AzureSnapshotManager.sln
dotnet restore AzureSnapshotManager.sln
dotnet build AzureSnapshotManager.sln
```

### "Tests failed"
**Run tests individually to see which one:**

```powershell
dotnet test tests\AzureSnapshotManager.Tests\AzureSnapshotManager.Tests.csproj -v normal
```

If you see timeout or network errors, it usually means `az login` session expired or isn't active. Try:
```powershell
az account show
```
If that fails, re-run `az login`.

## Dashboard Not Starting

### "Could not list Azure subscriptions" error
**Your `az login` session is not active.**

```powershell
az login
```

Then try the dashboard again.

### "Connection refused" or "port already in use"
**Something is already running on port 5080 or 7001.**

Either:
1. Find and close the other thing using that port, or
2. Kill the dotnet process:
   ```powershell
   Stop-Process -Name dotnet
   ```

### Dashboard loads but says "No subscription set"
**This is normal if it's your first time.** Paste a real subscription ID into the top bar manually.

If auto-detect should be working but isn't, check browser console (F12):
- Open DevTools → Console tab
- Look for errors like "Failed to fetch /api/account/subscriptions"
- If you see that, it usually means the backend didn't start properly — check the PowerShell window where you ran `dotnet run`

## .exe Build Problems

### "publish-exe.ps1 is not digitally signed"
**PowerShell security policy blocking script execution.**

Workaround (one-time):
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

Then run the script again.

### "publish-exe.ps1: command not found"
**You're not in the repo root, or the path is wrong.**

```powershell
# Make sure you're in the repo root
cd path\to\AzureSnapshotManager
.\desktop\publish-exe.ps1
```

### .exe build takes too long or seems stuck
**First build takes 1-2 minutes.** Subsequent builds are faster. If it's been >5 minutes:

Kill it with `Ctrl+C` and try again:
```powershell
dotnet clean src\AzureSnapshotManager.Api\AzureSnapshotManager.Api.csproj
.\desktop\publish-exe.ps1
```

## .exe Not Working

### "Browser doesn't auto-open"
**Run the exe anyway — it still works.** Check the PowerShell console for a URL like `http://127.0.0.1:12345/`, copy it, paste it into your browser manually.

### ".exe won't start at all"
**Run it from PowerShell to see the error:**

```powershell
cd .\desktop\dist
.\AzureSnapshotManager.Api.exe --offline
```

Look at the error message. Common ones:

- `"Could not list Azure subscriptions"` → run `az login` again
- `"The system cannot find the file specified"` → .exe may be corrupted; rebuild with `.\desktop\publish-exe.ps1`

### "Azure subscription not found"
**Your `az login` session expired.**

```powershell
az login
# Then close and re-open the .bat launcher
```

## Git/GitHub Problems

### "fatal: repository 'https://github.com/...' not found"
**The GitHub repo doesn't exist, or the URL is wrong.**

Double-check:
1. The repo exists at https://github.com/YOUR_USERNAME/AzureSnapshotManager
2. The URL in `git remote -v` is exactly right (no typos)

### "Authentication failed" on git push
**GitHub no longer accepts passwords.**

You need a Personal Access Token:
1. Go to https://github.com/settings/tokens
2. Click **Generate new token** → **Generate new token (classic)**
3. Give it a name, check `repo` scope
4. Copy the token (long string)
5. When `git push` asks for a password, paste the token instead
6. It works once, then you may need to do it again later (GitHub tokens can expire)

Alternatively, use GitHub Desktop (GUI) — it handles auth for you.

### "branch 'main' doesn't match any remote repository"
**You haven't set the remote yet.**

```powershell
git remote add origin https://github.com/YOUR_USERNAME/AzureSnapshotManager.git
git branch -M main
git push -u origin main
```

### "hint: You have divergent branches and need to merge"
**You have local commits that GitHub doesn't have, and vice versa.**

If you're sure you want to overwrite everything with your local copy:
```powershell
git push --force-with-lease origin main
```

(Don't do this if someone else is working on the repo.)

### CI workflow on GitHub shows red ❌
**Tests failed on GitHub's servers.**

Click the red ❌ in the Actions tab to see the full log. Most common:
- Azure credential not available (expected — the GitHub Actions runner doesn't have `az login`)
- A package isn't found

If you see `NU1605 package downgrade`, run locally and see if tests pass there. If they do, the GitHub issue is environmental and usually resolves on the next push.

## "It worked locally but not on Render or in the .exe"

**Most likely causes, in order:**

1. **`az login` session not active** — `az account show` fails
   - **Fix:** Run `az login` again in a fresh PowerShell window

2. **Wrong subscription ID** — pasted a subscription ID that your account doesn't have access to
   - **Fix:** Try `az account list` to see which ones you can see

3. **.exe was built when `az login` wasn't active** — rebuilt with `publish-exe.ps1`
   - **Fix:** `az login` again, then rebuild with `publish-exe.ps1`

4. **Credentials cached in a weird state** — unlikely but possible
   - **Fix:** `az account clear`, then `az login` again from scratch

5. **Something's actually broken in the code** — least likely
   - **Fix:** Check the browser console (F12) for error messages; share them if you're stuck

## Still stuck?

1. Check this file for your exact error
2. Check the full `SETUP_AND_DEPLOY.md` for step-by-step detail
3. Share:
   - The exact error message (copy the whole thing)
   - Which step you're on (build? test? GitHub push?)
   - Output of `dotnet --version` and `az account show`
