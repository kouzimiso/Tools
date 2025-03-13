@echo off
REM コンパイル用バッチファイル
set CSC=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\csc.exe
set ProgramName=ConvertLF


if not exist "%CSC%" (
    echo CSC.exeが見つかりません。
    pause
    exit /b 1
)

echo コンパイル中...
"%CSC%" /out:%ProgramName%.exe /target:exe /reference:"System.Windows.Forms.dll","Newtonsoft.Json.dll" %ProgramName%.cs

if %errorlevel% neq 0 (
    echo コンパイルに失敗しました。
    pause
    exit /b 1
)

echo コンパイルが完了しました。
pause
