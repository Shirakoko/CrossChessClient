@echo off
REM 设置当前脚本所在的目录（原工程的Scripts文件夹）
set "SCRIPT_DIR=%~dp0"

REM 设置Copy工程的Scripts文件夹路径（相对路径）
set "TARGET_DIR=%SCRIPT_DIR%..\..\..\CrossChessCopy\Assets\Scripts"

REM 检查目标文件夹是否存在，如果不存在则创建
if not exist "%TARGET_DIR%" (
    echo 目标文件夹不存在，正在创建...
    mkdir "%TARGET_DIR%"
)

REM 复制所有.cs文件，并覆盖重名文件
echo 正在复制.cs文件...
xcopy "%SCRIPT_DIR%*.cs" "%TARGET_DIR%\" /Y /E /I

echo 复制完成！
pause