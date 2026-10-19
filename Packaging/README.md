# Compilar e empacotar no Windows

Unity 6000.6.6f1, módulo Windows Build Support (Mono). Inno Setup 6.7.3 para o instalador.

```powershell
$env:PONTO_RELEASE_OUTPUT = 'Builds/Release0201'
& 'C:\Program Files\Unity\Hub\Editor\6000.6.6f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath (Get-Location).Path -executeMethod PontoCego.Editor.BuildProject.BuildWindows -logFile build.log
& 'C:\Program Files (x86)\Inno Setup 6\ISCC.exe' Packaging/Windows.iss
```

Confirme `PONTO_CEGO_WINDOWS_BUILD_OK` no log. O instalador sai em `Builds/Distribution`. Ajuste apenas o caminho do compilador Inno conforme sua instalação. O jogo é instalado por usuário e não remove saves ao desinstalar. O pacote não é assinado digitalmente.
