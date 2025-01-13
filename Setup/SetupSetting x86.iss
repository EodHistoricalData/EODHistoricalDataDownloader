; -------------------------------------------------------------------------------------------------------------------------------------------------
; Основные переменные установщика
; -------------------------------------------------------------------------------------------------------------------------------------------------
#define MyAppName "EODHistoricalDataDownloader"                                               ; Название приложения
#define MyAppExeName "EODHistoricalDataDownloader.exe"                                        ; Имя файла exe 
#define MyAppPublisher "Micro-Solution LLC"
#define MyAppURL "https://eodhistoricaldata.com/"
#define MyAppIco    "C:\Users\vkomy\source\repos\EODHistoricalDataDownloader\Setup\appIcon.ico"                      ; Файл с иконкой
#define SetupPath   "C:\Users\vkomy\source\repos\EODHistoricalDataDownloader\Setup\"                        ; Корневая папка
#define FilesPath   "C:\Users\vkomy\source\repos\EODHistoricalDataDownloader\EODHistoricalDataDownloader\bin\Release\net6.0-windows\publish\"  ; Папка с файлами, которые необходимо упаковать
#define ReleasePath "C:\Users\vkomy\source\repos\EODHistoricalDataDownloader\Setup\Release\"                ; Выходная папка

#define MyAppVersion   GetFileVersion(FilesPath+MyAppName+'.exe')                                           ; Версия программы

; -------------------------------------------------------------------------------------------------------------------------------------------------
; Настройка Net6
; -------------------------------------------------------------------------------------------------------------------------------------------------

#define Net6Setup "windowsdesktop-runtime-6.0.36-win-x64.exe"
#define Net6Version "Microsoft.WindowsDesktop.App 6.0.36"

#define UseNetCoreCheck
#ifdef UseNetCoreCheck
  #define UseDotNet60Desktop
#endif

; -------------------------------------------------------------------------------------------------------------------------------------------------
; Подписывание программы
; -------------------------------------------------------------------------------------------------------------------------------------------------
#define SignTool    "C:\Program Files (x86)\Windows Kits\10\bin\10.0.20348.0\x64\signtool.exe"
#define SingNameSSL "Micro-Solution LLC" ; Имя сертификата


[Setup]
;Подписывание кода
;SignTool=byparam {#SignTool} sign /a /n $q{#SingNameSSL}$q /t http://timestamp.comodoca.com/authenticode /d $q{#MyAppName}$q $f

;Использовать сгенерируемый VS GUI
AppId           = {{AE85539E-2BFA-44C9-9153-A2A036BB5AA8}}
AppName         = {#MyAppName}
AppVersion      = {#MyAppVersion}
AppPublisher    = {#MyAppPublisher}
AppPublisherURL = {#MyAppURL}
;AppSupportURL   = {#MyAppURL}
;AppUpdatesURL   = {#MyAppURL}

DefaultDirName          = {autopf}\Micro-Solution\{#MyAppName}                           
DisableProgramGroupPage = yes
DefaultGroupName        = Micro-Solution\{#MyAppName}
UninstallDisplayIcon    = {#MyAppIco}
UninstallDisplayName    = {#MyAppName}
AllowNoIcons            = yes                            

;Файл лицензионного соглашения
;LicenseFile=C:\Users\zhelt\source\repos\DocFiller\DocFiller\bin\Release\License.rtf

; Результат компиляции установщика
OutputDir            = {#ReleasePath}
OutputBaseFilename   = Setup {#MyAppName}
SetupIconFile        = {#MyAppIco}
Compression          = lzma
SolidCompression     = yes
WizardStyle          = modern
WizardImageFile      = {#SetupPath}side.bmp 
WizardSmallImageFile = {#SetupPath}icon.bmp
DisableWelcomePage   = no

[Languages]
;Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Messages]
;При необходимости изменение сообщений установщика
;WelcomeLabel1=Вас приветствует Мастер установки программы [name]
;WelcomeLabel2=Программа установит [name/ver] на ваш компьютер.%n%nПожалуйста, закройте все файлы Excel перед тем, как продолжить.
;ReadyLabel1=Все настройки выполнены и можно приступить к установке [name] на ваш компьютер.
;FinishedLabel=Программа [name] установлена на ваш компьютер. Программа запускается вместе с программой Microsoft Excel.

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Перечислить все файлы необходимые для работы программы
Source: "{#FilesPath}*.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#FilesPath}*.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#FilesPath}EODHistoricalDataDownloader.exe"; DestDir: "{app}"; Flags: ignoreversion sign   

; .NET 6
Source: "{#SetupPath}{#Net6Setup}"; DestDir: "{tmp}"; Flags: deleteafterinstall; Check: not IsDotNetInstalled('6')

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
;Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
;Name: "{group}\{cm:ProgramOnTheWeb,{#MyAppName}}"; Filename: "{#MyAppURL}"


[Registry]
; Записи в реестре при необходимости
;Root: HKCU; Subkey: "Software\Microsoft\Office\Excel\Addins\{#MyAppName}"; ValueType: string; ValueName: "Description"; ValueData: "{#MyAppName}";  Flags: uninsdeletekey
;Root: HKCU; Subkey: "Software\Microsoft\Office\Excel\Addins\{#MyAppName}"; ValueType: string; ValueName: "FriendlyName"; ValueData: "{#MyAppName}"; Flags: uninsdeletekey
;Root: HKCU; Subkey: "Software\Microsoft\Office\Excel\Addins\{#MyAppName}"; ValueType: dword; ValueName: "LoadBehavior"; ValueData: 3; Flags: uninsdeletekey
;Root: HKCU; Subkey: "Software\Microsoft\Office\Excel\Addins\{#MyAppName}"; ValueType: string; ValueName: "Manifest"; ValueData: "file:///{app}\Doc.filler.vsto|vstolocal"; Flags: uninsdeletekey

[Code]
// Поиск запущенного приложения
function FindApp(const AppName: String): Boolean;
  var
    WMIService: Variant;
    WbemLocator: Variant;
    WbemObjectSet: Variant;
  begin
    WbemLocator := CreateOleObject('WbemScripting.SWbemLocator');
    WMIService := WbemLocator.ConnectServer('localhost', 'root\CIMV2');
    WbemObjectSet := WMIService.ExecQuery('SELECT * FROM Win32_Process Where Name="' + AppName + '"');
    if not VarIsNull(WbemObjectSet) and (WbemObjectSet.Count > 0) then
    begin
      Log(AppName + ' is up and running');
      Result := True
    end;
end;

function IsDotNetInstalled(DotNetName: string): Boolean;
var
  Cmd, Args: string;
  FileName: string;
  Output: AnsiString;
  Command: string;
  ResultCode: Integer;
begin
  FileName := ExpandConstant('{tmp}\dotnet.txt');
  Cmd := ExpandConstant('{cmd}');
  Command := 'dotnet --list-runtimes';
  Args := '/C ' + Command + ' > "' + FileName + '" 2>&1';
  if Exec(Cmd, Args, '', SW_HIDE, ewWaitUntilTerminated, ResultCode) and
     (ResultCode = 0) then
  begin
    if LoadStringFromFile(FileName, Output) then
    begin
      if Pos(DotNetName, Output) > 0 then
      begin
        Log('"' + DotNetName + '" found in output of "' + Command + '"');
        Result := True;
      end
        else
      begin
        Log('"' + DotNetName + '" not found in output of "' + Command + '"');
        Result := False;
      end;
    end
      else
    begin
      Log('Failed to read output of "' + Command + '"');
    end;
  end
    else
  begin
    Log('Failed to execute "' + Command + '"');
    Result := False;
  end;
  DeleteFile(FileName);
end;


[Run]
Filename: {tmp}\{#Net6Setup}; Parameters: "/install /quiet /norestart"; Check: not IsDotNetInstalled('Microsoft.NETCore.App 6.0.36'); StatusMsg: Microsoft Framework {#Net6Version} installing. Please wait...
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent