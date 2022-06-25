TITLE External Payment Gateway
color 8e
@ECHO OFF
cd C:\Users\Rag\Documents\GitHub\ps-microservices-path\GloboTicket\External.PaymentGateway\
cls
ECHO External Payment Gateway...
dotnet build External.PaymentGateway.csproj
cls
ECHO ======================
ECHO External.PaymentGateway.csproj
ECHO ======================
dotnet run External.PaymentGateway.csproj
cls