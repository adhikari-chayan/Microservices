TITLE GloboTicket.Gateway
color 2e
@ECHO OFF
cd C:\Users\Rag\Documents\GitHub\ps-microservices-path\GloboTicket\GloboTicket.Gateway.WebBff\
cls
ECHO Build GloboTicket.Gateway...
dotnet build GloboTicket.Gateway
cls
ECHO ===================
ECHO GloboTicket.Gateway
ECHO ===================
dotnet run GloboTicket.Gateway
cls