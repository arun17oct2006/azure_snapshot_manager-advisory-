@echo off
title Azure Snapshot Manager
echo Starting Azure Snapshot Manager (offline mode)...
echo This uses whichever account you last signed in with via "az login".
echo.
AzureSnapshotManager.Api.exe --offline
pause
