TITLE EventCatalog
color 9e
@ECHO OFF
cd C:\Users\Rag\Documents\GitHub\ps-microservices-path\GloboTicket\GloboTicket.Services.EventCatalog\
cls
ECHO Build EventCatalog...
dotnet build GloboTicket.Services.EventCatalog.csproj
cls
ECHO ======================
ECHO EventCatalog
ECHO ======================
dotnet run GloboTicket.Services.EventCatalog.csproj
cls