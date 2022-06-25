TITLE Discount
color 08
@ECHO OFF
cd C:\Users\Rag\Documents\GitHub\ps-microservices-path\GloboTicket\GloboTicket.Services.Discount\
cls
ECHO Build Discount...
dotnet GloboTicket.Services.Discount.csproj
cls
color 0E
ECHO ===================
ECHO Discount
ECHO ===================
color 08
dotnet run GloboTicket.Services.Discount.csproj
cls
