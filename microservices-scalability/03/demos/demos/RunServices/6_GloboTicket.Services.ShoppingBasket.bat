TITLE ShoppingBasket
color 8e
@ECHO OFF
cd C:\Users\Rag\Documents\GitHub\ps-microservices-path\GloboTicket\GloboTicket.Services.ShoppingBasket\
cls
ECHO Build ShoppingBasket...
dotnet build GloboTicket.Services.ShoppingBasket.csproj
cls
ECHO ======================
ECHO ShoppingBasket
ECHO ======================
dotnet run GloboTicket.Services.ShoppingBasket.csproj
cls