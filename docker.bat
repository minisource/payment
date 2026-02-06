@echo off
REM Payment Service Docker Management Script for Windows

SETLOCAL EnableDelayedExpansion

REM Check if Docker is running
docker info >nul 2>&1
IF %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Docker is not running. Please start Docker and try again.
    exit /b 1
)

REM Parse command
SET COMMAND=%1
SET ARG=%2

IF "%COMMAND%"=="" (
    CALL :show_help
    EXIT /B 0
)

IF "%COMMAND%"=="start" CALL :start_services
IF "%COMMAND%"=="stop" CALL :stop_services
IF "%COMMAND%"=="restart" CALL :restart_services
IF "%COMMAND%"=="build" CALL :build_services
IF "%COMMAND%"=="logs" CALL :view_logs
IF "%COMMAND%"=="migrate" CALL :run_migrations
IF "%COMMAND%"=="health" CALL :health_check
IF "%COMMAND%"=="clean" CALL :cleanup
IF "%COMMAND%"=="shell" CALL :open_shell
IF "%COMMAND%"=="help" CALL :show_help

EXIT /B 0

:start_services
    echo [INFO] Starting services...
    IF "%ARG%"=="dev" (
        docker-compose -f docker-compose.dev.yml up -d
    ) ELSE (
        docker-compose up -d
    )
    echo [SUCCESS] Services started successfully
    timeout /t 5 /nobreak >nul
    docker-compose ps
    EXIT /B 0

:stop_services
    echo [INFO] Stopping services...
    docker-compose down
    echo [SUCCESS] Services stopped successfully
    EXIT /B 0

:restart_services
    CALL :stop_services
    CALL :start_services
    EXIT /B 0

:build_services
    echo [INFO] Building services...
    docker-compose build
    echo [SUCCESS] Services built successfully
    EXIT /B 0

:view_logs
    IF "%ARG%"=="" (
        SET SERVICE=payment
    ) ELSE (
        SET SERVICE=%ARG%
    )
    echo [INFO] Showing logs for !SERVICE!...
    docker-compose logs -f !SERVICE!
    EXIT /B 0

:run_migrations
    echo [INFO] Running database migrations...
    docker exec -it payment-api dotnet ef database update
    IF %ERRORLEVEL% NEQ 0 (
        echo [INFO] Failed to run migrations in container. Trying from host...
        dotnet ef database update --project .\Infrastructure\Infrastructure.csproj --startup-project .\payment\Presentaion.csproj
    )
    echo [SUCCESS] Migrations completed successfully
    EXIT /B 0

:health_check
    echo [INFO] Checking service health...
    
    REM Check payment API
    curl -f http://localhost:4005/health >nul 2>&1
    IF %ERRORLEVEL% EQU 0 (
        echo [SUCCESS] Payment API is healthy
    ) ELSE (
        echo [ERROR] Payment API is not responding
    )
    
    REM Check SQL Server
    docker exec payment-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P YourPassword123! -C -Q "SELECT 1" >nul 2>&1
    IF %ERRORLEVEL% EQU 0 (
        echo [SUCCESS] SQL Server is healthy
    ) ELSE (
        echo [ERROR] SQL Server is not responding
    )
    EXIT /B 0

:cleanup
    echo [WARNING] This will remove all containers, volumes, and data.
    SET /P CONFIRM=Continue? (y/N): 
    IF /I "!CONFIRM!"=="y" (
        docker-compose down -v
        docker system prune -f
        echo [SUCCESS] Cleanup completed
    ) ELSE (
        echo [INFO] Cleanup cancelled
    )
    EXIT /B 0

:open_shell
    IF "%ARG%"=="" (
        SET SERVICE=payment-api
    ) ELSE (
        SET SERVICE=%ARG%
    )
    echo [INFO] Opening shell in !SERVICE!...
    docker exec -it !SERVICE! bash
    EXIT /B 0

:show_help
    echo Payment Service Docker Management Script
    echo.
    echo Usage: docker.bat [COMMAND] [OPTIONS]
    echo.
    echo Commands:
    echo     start [dev]      Start services (use 'dev' for development mode)
    echo     stop             Stop services
    echo     restart [dev]    Restart services
    echo     build            Build Docker images
    echo     logs [service]   View logs (default: payment service)
    echo     migrate          Run database migrations
    echo     health           Check service health
    echo     clean            Clean up all Docker resources
    echo     shell [service]  Open shell in container (default: payment-api)
    echo     help             Show this help message
    echo.
    echo Examples:
    echo     docker.bat start           # Start in production mode
    echo     docker.bat start dev       # Start in development mode
    echo     docker.bat logs payment    # View payment service logs
    echo     docker.bat logs sqlserver  # View SQL Server logs
    echo     docker.bat migrate         # Run database migrations
    echo     docker.bat shell           # Open shell in payment-api container
    echo.
    EXIT /B 0
