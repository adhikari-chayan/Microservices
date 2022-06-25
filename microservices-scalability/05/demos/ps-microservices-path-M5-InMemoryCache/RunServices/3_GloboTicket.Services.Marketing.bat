TITLE Marketing
color 3e
@ECHO OFF
cd C:\Users\Rag\Documents\GitHub\ps-microservices-path\GloboTicket\GloboTicket.Services.Marketing\
cls
ECHO Build Marketing...
dotnet build GloboTicket.Services.Marketing.csproj
cls
ECHO ======================
ECHO Marketing
ECHO ======================
dotnet run GloboTicket.Services.Marketing.csproj
cls