@echo off
REM ���õ�ǰ�ű����ڵ�Ŀ¼��ԭ���̵�Scripts�ļ��У�
set "SCRIPT_DIR=%~dp0"

REM ����Copy���̵�Scripts�ļ���·�������·����
set "TARGET_DIR=%SCRIPT_DIR%..\..\..\CrossChessCopy\Assets\Scripts"

REM ���Ŀ���ļ����Ƿ���ڣ�����������򴴽�
if not exist "%TARGET_DIR%" (
    echo Ŀ���ļ��в����ڣ����ڴ���...
    mkdir "%TARGET_DIR%"
)

REM ��������.cs�ļ��������������ļ�
echo ���ڸ���.cs�ļ�...
xcopy "%SCRIPT_DIR%*.cs" "%TARGET_DIR%\" /Y /E /I

echo ������ɣ�
pause