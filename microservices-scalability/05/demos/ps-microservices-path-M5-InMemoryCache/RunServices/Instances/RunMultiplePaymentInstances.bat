start cmd /k 5b_FakePaymentGateway.bat
TIMEOUT /T 5
start cmd /k 5a_GloboTicket.Services.PaymentInstanceOne.bat
start cmd /k 5a_GloboTicket.Services.PaymentInstanceTwo.bat
start cmd /k 5a_GloboTicket.Services.PaymentInstanceThree.bat
start TileWindows.vbs


