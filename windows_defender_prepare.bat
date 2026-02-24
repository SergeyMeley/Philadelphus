@echo off
chcp 65001 >nul

echo ========================================
echo Добавляем сборки приложения в список доверенных приложений для функции Controlled Folder Access в Microsoft Defender.
echo ========================================

powershell.exe -ExecutionPolicy Bypass -Command "Add-MpPreference -ControlledFolderAccessAllowedApplications 'D:\\MelSV_Projects\\Philadelphus.General\\Philadelphus.Presentation.Wpf.UI\\bin\\Debug'"
powershell.exe -ExecutionPolicy Bypass -Command "Add-MpPreference -ControlledFolderAccessAllowedApplications 'D:\\MelSV_Projects\\Philadelphus.General\\Philadelphus.Presentation.Wpf.UI\\bin\\Debug\\net8.0-windows7.0\\Philadelphus.Presentation.Wpf.UI.exe'"
powershell.exe -ExecutionPolicy Bypass -Command "Add-MpPreference -ControlledFolderAccessAllowedApplications 'D:\\MelSV_Projects\\Philadelphus.General\\Philadelphus.Presentation.Wpf.UI\\bin\\Debug\\net8.0-windows7.0\\Philadelphus.Infrastructure.Persistence.dll'"
powershell.exe -ExecutionPolicy Bypass -Command "Add-MpPreference -ControlledFolderAccessAllowedApplications 'D:\MelSV_Projects\Philadelphus.General\Philadelphus.Presentation.Wpf.UI\bin\Debug'"
powershell.exe -ExecutionPolicy Bypass -Command "Add-MpPreference -ControlledFolderAccessAllowedApplications 'D:\MelSV_Projects\Philadelphus.General\Philadelphus.Presentation.Wpf.UI\bin\Debug\net8.0-windows7.0\Philadelphus.Presentation.Wpf.UI.exe'"
powershell.exe -ExecutionPolicy Bypass -Command "Add-MpPreference -ControlledFolderAccessAllowedApplications 'D:\MelSV_Projects\Philadelphus.General\Philadelphus.Presentation.Wpf.UI\bin\Debug\net8.0-windows7.0\Philadelphus.Infrastructure.Persistence.dll'"




if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo Все сборки добавлены успешно!
    echo ========================================
) else (
    echo.
    echo ========================================
    echo Ошибка при добавлении! Проверьте права администратора и Defender.
    echo ========================================
)

pause
