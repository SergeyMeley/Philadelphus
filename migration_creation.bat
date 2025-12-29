@echo off
chcp 65001 >nul

echo ========================================
echo МАССОВОЕ СОЗДАНИЕ МИГРАЦИЙ
echo ========================================

:: Запрашиваем имя миграции
set /p MIGRATION_NAME=Введите имя миграции (например InitialCreate): 

if "%MIGRATION_NAME%"=="" (
    echo Имя миграции не может быть пустым!
    pause
    exit /b 1
)

echo.
echo Создание миграций с именем: %MIGRATION_NAME%
echo ========================================

echo Создание миграций для Philadelphus.Infrastructure.Persistence.EF.PostgreSQL
echo ========================================

set PROJECT_DIR=D:\MelSV_Projects\Philadelphus.General\Philadelphus.Infrastructure.Persistence.EF.PostgreSQL

cd /d "%PROJECT_DIR%"
echo Текущая директория: %CD%

echo Создание миграции для TreeRepositoriesPhiladelphusContext...
dotnet ef migrations add %MIGRATION_NAME% -c TreeRepositoriesPhiladelphusContext -o "Migrations\TreeRepositoriesPhiladelphusContextMigrations"


echo Создание миграции для MainEntitiesPhiladelphusContext...
dotnet ef migrations add %MIGRATION_NAME% -c MainEntitiesPhiladelphusContext -o "Migrations\MainEntitiesPhiladelphusContextMigrations"


::echo Создание миграции для DataStoragesPhiladelphusContext...
::dotnet ef migrations add %MIGRATION_NAME% -c DataStoragesPhiladelphusContext -o "Migrations\DataStoragesPhiladelphusContextPhiladelphusContextMigrations"


echo.
echo ========================================
echo Все миграции созданы!
echo ========================================

pause