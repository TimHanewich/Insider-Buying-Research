using System;
using SecuritiesExchangeCommission.Edgar;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TimHanewich.Reserch.Core
{
    public class FullResearchSet
    {
        //Settings
        private bool PrintStatusChanges;

        public string Symbol {get; set;}
        public StockPerformanceSet AveragePerformance {get; set;}
        public StockPerformanceSet[] PerformancesFollowingInsiderBuys {get; set;}

        public async Task GenerateFromTransactionsFileAsync(string path)    
        {
            //Start with vars
            List<NonDerivativeTransaction> ta_AnalyzedTransactions = new List<NonDerivativeTransaction>();
            List<StockPerformanceSet> ta_PerformancesFollowingInsiderBuys = new List<StockPerformanceSet>();

            //Get the name of the stock
            PrintStatus("Getting symbol from path.");
            Symbol = System.IO.Path.GetFileName(path).Replace(".json", "").ToUpper().Trim();
            Console.WriteLine("Symbol: " + Symbol);

            //Get the transactions
            ResearchToolkit rt = new ResearchToolkit();
            PrintStatus("Loading transactions from file.");
            NonDerivativeTransaction[] alltransactions = await rt.OpenNonDerivativeTransactionsAsync(path);
            Console.WriteLine(alltransactions.Length.ToString() + " transactions found.");
            Console.Write("Filtering to focus... ");
            NonDerivativeTransaction[] focus = rt.FilterToTransactionsOfInterest(alltransactions);
            Console.WriteLine(focus.Length.ToString() + " transactions to focus on.");

            //Get the average performance
            StockPerformanceSet sps_avg = new StockPerformanceSet();
            PrintStatus("Getting average stock performance for this period.");
            try
            {
                await sps_avg.CalculateAverageReturnsAsync(Symbol, new DateTime(2010, 1, 1), new DateTime(2019, 12, 31));
            }
            catch (Exception ex)
            {
                throw new Exception("Fatal failure while trying to download average returns for the period: " + ex.Message);
            }
            PrintStatus("Average perforamance downloaded!");
            AveragePerformance = sps_avg;

            //Get the performances after each buy
            PrintStatus("Starting the performance calculation following each insider buy...");
            foreach (NonDerivativeTransaction ndt in ta_AnalyzedTransactions)
            {
                StockPerformanceSet sps = new StockPerformanceSet();
                PrintStatus("Calculating performance since buy on " + ndt.TransactionDate.Value.ToShortDateString() + "...");
                try
                {
                    await sps.CalculateReturnsAsync(Symbol, ndt.TransactionDate.Value);
                    Console.WriteLine("Performance calculated!");
                    ta_PerformancesFollowingInsiderBuys.Add(sps);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failure while processing performance since buy on " + ndt.TransactionDate.Value.ToShortDateString() + ": " + ex.Message);
                }
            }

            //Add them
            PerformancesFollowingInsiderBuys = ta_PerformancesFollowingInsiderBuys.ToArray();

            PrintStatus("Generation complete!");
        }

        private void PrintStatus(string s)
        {
            if(PrintStatusChanges)
            {
                Console.WriteLine(s);
            }
        }
    
        public void StatusPrintingOn()
        {
            PrintStatusChanges = true;
        }

        public void StatusPrintingOff()
        {
            PrintStatusChanges = false;
        }
    }
}