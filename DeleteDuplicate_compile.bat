@echo off
REM �R���p�C���p�o�b�`�t�@�C��
set CSC=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\csc.exe
set ProgramName=DeleteDuplicate


if not exist "%CSC%" (
    echo CSC.exe��������܂���B
    pause
    exit /b 1
)

echo �R���p�C����...
"%CSC%" /out:%ProgramName%.exe /target:exe /reference:"System.Windows.Forms.dll","Newtonsoft.Json.dll" %ProgramName%.cs

if %errorlevel% neq 0 (
    echo �R���p�C���Ɏ��s���܂����B
    pause
    exit /b 1
)

echo �R���p�C�����������܂����B
pause
