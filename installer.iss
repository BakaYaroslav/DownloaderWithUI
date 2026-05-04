[Setup]
AppName=Downloader
AppVersion=1.10
DefaultDirName={autopf}\Downloader
DefaultGroupName=Downloader
OutputDir=Output
OutputBaseFilename=DownloaderSetup
Compression=lzma
SolidCompression=yes

[Files]
Source: "C:\Users\jaros\source\repos\Downloader\Downloader\bin\Release\net10.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

[Icons]
Name: "{group}\Downloader"; Filename: "{app}\Downloader.exe"
Name: "{commondesktop}\Downloader"; Filename: "{app}\Downloader.exe"

[Run]
Filename: "{app}\Downloader.exe"; Description: "Запустить Downloader"; Flags: nowait postinstall skipifsilent