TITLE Payment
color 8e
@ECHO OFF
cd C:\Users\Rag\Documents\GitHub\ps-microservices-path\GloboTicket\GloboTicket.Services.Payment\
cls
ECHO Build Payment...
dotnet build GloboTicket.Services.Payment.csproj
cls
ECHO ======================
ECHO Payment
ECHO ======================
dotnet run GloboTicket.Services.Payment.csproj
cls