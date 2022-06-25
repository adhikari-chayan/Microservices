start cmd /k 1_GloboTicket.Services.Discount.bat
start cmd /k 2_GloboTicket.Services.EventCatalog.bat
start cmd /k 3_GloboTicket.Services.Marketing.bat
start cmd /k 4_GloboTicket.Services.Ordering.bat
start cmd /k 5_GloboTicket.Services.Payment.bat
start cmd /k 5b_FakePaymentGateway.bat
start cmd /k 6_GloboTicket.Services.ShoppingBasket.bat
start cmd /k 7_GloboTicket.Gateway.bat
start cmd /k 8_GloboTicket.Web.bat
start TileWindows.vbs
TIMEOUT /T 20
start https://localhost:5010

