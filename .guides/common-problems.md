# Common Problems
This is an overview to common problems experienced with the app on first startup. If you're having a problem that is not on here, 
feel free to contact us on [Discord](https://discord.gg/n26gXsk5R5) or open an issue here on Github (Requires a Github account).

## Nothing happens when launching
If nothing happens when you double-click the executable, you might have encountered a problem with access rights in the folder 
(this is common for installs on the C drive). At the moment you can fix this by launching with administrator rights
(Rich-Click, Run as Admin).

## .NET not installed
This happens when you do not have [.NET 6.0 Dekstop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) installed.
After installing you may have to restart your machine.

## Steam User Not Found
This is a common problem and can be caused by several things. The most common fix is:

Create (or edit) the local.json file next to the executable. The file should contain:
```json
{
  "Paths": {},
  "LastPlayedScenario": "",
  "LastPlayedGamemode": "Victory Points",
  "LastPlayedGamemodeSetting": 1,
  "OtherOptions": {},
  "SteamData": {
    "User": 06561198003529961
  },
  "Language": 0
}
```
Replace 06561198003529961 with your steam ID (ID can be found here: https://www.steamidfinder.com/ - just search your name, dec id). 
If the error persists after having done the above, you may have Steam installed somewhere we did not plan for. As of April 25th there's no 
fix to this problem. (It's being worked on). If you are experiencing this problem, please let us know where you have installed Steam
so we can plan for it.

## Crash on entering Company Builder
This issue is likely related to the launcher failing to find the Company of Heroes 2 install location. If that is the case, you may have moved the game
from one disk to another - and you may have to delete the folder in the old install location.

## Match data was not uploaded to server
This can happen if you do not have the archive.exe file in your Company of Heroes 2 install location. Make sure you're not in the 64-bit beta on Steam.
If the archive.exe file is still not in that location, verify integirty through Steam.

If CoH2 is installed at
``C:\Program Files (x86)\Steam\steamapps\common\Company of Heroes 2``
Then make sure the archive file exists at
``C:\Program Files (x86)\Steam\steamapps\common\Company of Heroes 2\archive.exe``
