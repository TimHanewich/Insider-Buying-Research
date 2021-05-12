using System;
using SecuritiesExchangeCommission.Edgar;
using Aletheia.Engine;
using TimHanewich.Reserch;
using TimHanewich.Reserch.Trailing;

namespace Insider_Buying_Research
{
    class Program
    {
        static void Main(string[] args)
        {
            SecCollectionHelper colhelp = new SecCollectionHelper();
            colhelp.StoreSP500NonDerivateTransactionsInFolderAsync(args[0]).Wait();
        }
    }
}
