using System;
using SecuritiesExchangeCommission.Edgar;
using Aletheia.Engine;
using TimHanewich.Reserch;

namespace Insider_Buying_Research
{
    class Program
    {
        static void Main(string[] args)
        {
            SecCollectionHelper colhelp = new SecCollectionHelper();
            colhelp.StoreSP500NonDerivateTransactionsInFolderAsync(@"C:\Users\tahan\Downloads\Sp500Transactions").Wait();
        }
    }
}
