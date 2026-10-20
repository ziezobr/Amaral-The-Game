#define AppVersion "0.21.1"
[Setup]
AppId={{6D39F473-8490-428C-A2C6-54E1427ACF81}
AppName=Amaral The Game - Ponto Cego
AppVersion={#AppVersion}
AppPublisher=Makeicos Developers
DefaultDirName={localappdata}\Programs\AmaralTheGame
DefaultGroupName=Amaral The Game
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0
OutputDir=..\Builds\Distribution0211
OutputBaseFilename=Amaral-The-Game-0.21.1-Windows-Setup
Compression=lzma2/fast
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\PontoCego.exe
DisableProgramGroupPage=yes
[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"
[Tasks]
Name: "desktopicon"; Description: "Criar atalho na área de trabalho"; Flags: unchecked
[Files]
Source: "..\Builds\Demo0211\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "*_BackUpThisFolder_ButDontShipItWithYourGame\*,*.pdb"
[Icons]
Name: "{group}\Amaral The Game"; Filename: "{app}\PontoCego.exe"; WorkingDir: "{app}"
Name: "{autodesktop}\Amaral The Game"; Filename: "{app}\PontoCego.exe"; WorkingDir: "{app}"; Tasks: desktopicon
[Run]
Filename: "{app}\PontoCego.exe"; Description: "Jogar Amaral The Game"; Flags: nowait postinstall skipifsilent
