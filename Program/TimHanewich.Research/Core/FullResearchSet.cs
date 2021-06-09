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
            //Get the name of the stock
            PrintStatus("Getting symbol from path.");
            Symbol = System.IO.Path.GetFileName(path).Replace(".json", "").ToUpper().Trim();
            Console.WriteLine("Symbol: " + Symbol);

            //Get the transactions
            ResearchToolkit rt = new ResearchToolkit();
            PrintStatus("Loading transactions from file.");
            NonDerivativeTransaction[] alltransactions = await rt.OpenNonDerivativeTransactionsAsync(path);
            PrintStatus(alltransactions.Length.ToString() + " transactions found.");
            PrintStatus("Filtering to focus... ");
            NonDerivativeTransaction[] focus = rt.FilterToTransactionsOfInterest(alltransactions);
            PrintStatus(focus.Length.ToString() + " transactions to focus on.");

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
            List<StockPerformanceSet> PerformancesFollowingBuys = new List<StockPerformanceSet>();
            PrintStatus("Starting the performance calculation following each insider buy...");
            int PCount = 1;
            foreach (NonDerivativeTransaction ndt in focus)
            {
                StockPerformanceSet sps = new StockPerformanceSet();
                PrintStatus("Calculating performance since buy on " + ndt.TransactionDate.Value.ToShortDateString() + " (" + PCount.ToString() + "/" + focus.Length.ToString() + ")...");
                try
                {
                    await sps.CalculateReturnsAsync(Symbol, ndt.TransactionDate.Value);
                    Console.WriteLine("Performance calculated!");
                    PerformancesFollowingBuys.Add(sps);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failure while processing performance since buy on " + ndt.TransactionDate.Value.ToShortDateString() + ": " + ex.Message);
                }
                PCount = PCount + 1;
            }

            //Add them
            PerformancesFollowingInsiderBuys = PerformancesFollowingBuys.ToArray();

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