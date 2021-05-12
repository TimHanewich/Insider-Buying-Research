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
            Console.WriteLine("What would you like to do?");
            Console.WriteLine("1 - Download transactions");
            Console.WriteLine("2 - Download filings");
            Console.Write(">");
            string cmd = Console.ReadLine();
            SecCollectionHelper colhelp = new SecCollectionHelper();
            if (cmd == "1")
            {
                colhelp.StoreSP500NonDerivateTransactionsInFolderAsync(args[0]).Wait();
            }
            else if (cmd == "2")
            {
                colhelp.StoreSP500StatementOfOwnershipsInFolderAsync(args[0]).Wait();
            }
            else
            {
                Console.WriteLine("Not recognized");
            }  
        }
    }
}
