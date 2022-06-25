TITLE Ordering
color 4e
@ECHO OFF
cd C:\Users\Rag\Documents\GitHub\ps-microservices-path\GloboTicket\GloboTicket.Services.Order\
cls
ECHO Build Ordering...
dotnet build GloboTicket.Services.Ordering.csproj
cls
ECHO ======================
ECHO Ordering
ECHO ======================
dotnet run GloboTicket.Services.Ordering.csproj
cls