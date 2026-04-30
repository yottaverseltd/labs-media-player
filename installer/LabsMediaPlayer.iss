; Build after: dotnet publish LabsMediaPlayer.csproj -c Release -f net9.0-desktop -r win-x64 -p:SelfContained=true -p:TargetFrameworks=net9.0-desktop -o publish/desktop
; Compile: "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer\LabsMediaPlayer.iss

#define MyAppName "Labs Media Player"
#define MyAppVersion "1.0"
#define MyAppPublisher "yottaverseltd"
#define MyAppExeName "LabsMediaPlayer.exe"
#define SourceDir "..\\publish\\desktop"

[Setup]
AppId={{A8F3E2D1-4B5C-4E6F-9A0B-1C2D3E4F5A6B}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\Programs\{#MyAppName}
DisableProgramGroupPage=yes
OutputDir=..\artifacts\installer
OutputBaseFilename=labs-media-player-win-x64-setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
