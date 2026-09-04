#define MyAppName "桌伴"
#ifndef MyAppVersion
#define MyAppVersion "0.15.4"
#endif
#define MyAppPublisher "DeskBound"
#define MyAppExeName "桌伴.exe"

[Setup]
AppId={{F53934AE-27E8-45A9-93B0-FBD98470B9E3}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName=DeskBound {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL=https://github.com/bestdrduck/DeskBound
AppSupportURL=https://github.com/bestdrduck/DeskBound/issues
AppUpdatesURL=https://github.com/bestdrduck/DeskBound/releases/latest
DefaultDirName={localappdata}\Programs\DeskBound
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
DisableDirPage=no
UsePreviousAppDir=yes
PrivilegesRequired=lowest
OutputDir=..\outputs
OutputBaseFilename=DeskBound-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no
SetupIconFile=..\assets\DeskBound.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
ArchitecturesAllowed=x86 x64compatible
VersionInfoVersion={#MyAppVersion}.0
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription=DeskBound Setup
VersionInfoProductName=DeskBound

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "..\outputs\桌伴.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\解除安裝 {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Parameters: "--repair-startup"; Flags: runhidden waituntilterminated
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "{app}\{#MyAppExeName}"; Parameters: "--remove-startup"; Flags: runhidden waituntilterminated skipifdoesntexist; RunOnceId: "RemoveDeskBoundStartup"
