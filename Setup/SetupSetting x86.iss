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
; Настройка Net5
; -------------------------------------------------------------------------------------------------------------------------------------------------

#define Net6Setup "windowsdesktop-runtime-6.0.1-win-x64.exe"
#define Net6Version "Microsoft.WindowsDesktop.App 6.0.1"

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
SignTool=byparam {#SignTool} sign /a /n $q{#SingNameSSL}$q /t http://timestamp.comodoca.com/authenticode /d $q{#MyAppName}$q $f

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
Source: "{#SetupPath}{#Net6Setup}"; DestDir: "{tmp}"; Flags: deleteafterinstall; Check: not IsNetCoreInstalled('6')

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




function CompareVersion(V1, V2: string): Integer;
var
  P, N1, N2: Integer;
begin
  Result := 0;
  while (Result = 0) and ((V1 <> '') or (V2 <> '')) do
  begin
    P := Pos('.', V1);
    if P > 0 then
    begin
      N1 := StrToInt(Copy(V1, 1, P - 1));
      Delete(V1, 1, P);
    end
      else
    if V1 <> '' then
    begin
      N1 := StrToInt(V1);
      V1 := '';
    end
      else
    begin
      N1 := 0;
    end;
    P := Pos('.', V2);
    if P > 0 then
    begin
      N2 := StrToInt(Copy(V2, 1, P - 1));
      Delete(V2, 1, P);
    end
      else
    if V2 <> '' then
    begin
      N2 := StrToInt(V2);
      V2 := '';
    end
      else
    begin
      N2 := 0;
    end;
    if N1 < N2 then Result := -1
      else
    if N1 > N2 then Result := 1;
  end;
end;



function IsNetCoreInstalled(version: string) : boolean;
var
    runtimes: TArrayOfString;
    I: Integer;
    versionCompare: Integer;
    registryKey: string;
begin
    registryKey := 'SOFTWARE\WOW6432Node\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.NETCore.App';
    if(not IsWin64) then
       registryKey :=  'SOFTWARE\dotnet\Setup\InstalledVersions\x86\sharedfx\Microsoft.NETCore.App';
       
    Log('[.NET] Look for version ' + version);
       
    if not RegGetValueNames(HKLM, registryKey, runtimes) then
    begin
      Log('[.NET] Issue getting runtimes from registry');
      Result := False;
      Exit;
    end;
    
    for I := 0 to GetArrayLength(runtimes)-1 do
    begin
      versionCompare := CompareVersion(runtimes[I], version);
      Log(Format('[.NET] Compare: %s/%s = %d', [runtimes[I], version, versionCompare]));
      if(not (versionCompare = -1)) then
      begin
        Log(Format('[.NET] Selecting %s', [runtimes[I]]));
        Result := True;
          Exit;
      end;
    end;
    Log('[.NET] No compatible found');
    Result := False;
end;


// Callback-функция, вызываемая при инициализации удаления приложения
function  InitializeUninstall(): Boolean;
  begin
    Result := True;
    //if (FindApp('excel.exe')) then
    //begin
    //  MsgBox('Пожалуйста, закройте все файлы Excel перед удалением программы!', mbError, MB_OK);
    //   Result := False;
    //end;
    
end;



[Run]
Filename: {tmp}\{#Net6Setup}; Parameters: "/install /quiet /norestart"; Check: not IsNetCoreInstalled('6'); StatusMsg: Microsoft Framework {#Net6Version} installing. Please wait...
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent