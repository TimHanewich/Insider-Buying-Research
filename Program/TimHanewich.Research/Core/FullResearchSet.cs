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
            PrintStatus(alltransactions.Length.ToString("#,##0") + " loaded.");
            PrintStatus("Filtering to only buys...");
            NonDerivativeTransaction[] buys = rt.FilterToBuys(alltransactions);
            PrintStatus(buys.Length.ToString() + " buys found.");
            PrintStatus("Sorting...");
            NonDerivativeTransaction[] sorted = rt.SortByTransactionDate(buys);
            PrintStatus(sorted.Length.ToString() + " sorted!");
            
            //Filter to just the particular date range
            //2010 - 2019, including 2019. So 10 years: 2010, 2011, 2012, 2013, 2014, 2015, 2016, 2017, 2018, 2019
            PrintStatus("Filtering to only in range.");
            List<NonDerivativeTransaction> InDateRange = new List<NonDerivativeTransaction>();
            foreach (NonDerivativeTransaction ndt in sorted)
            {
                if (ndt.TransactionDate.HasValue)
                {
                    if (ndt.TransactionDate.Value >= new DateTime(2010, 1, 1)) //Older than (or equal to) 2010
                    {
                        if (ndt.TransactionDate.Value < new DateTime(2020, 1, 1)) //Before 2020 (2019 or earlier)
                        {
                            InDateRange.Add(ndt);
                        }
                    }
                }
            }
            PrintStatus(InDateRange.Count.ToString() + " found in date range.");

            //Check if there are any. If not, cancel
            if (InDateRange.Count == 0)
            {
                PrintStatus("Nothing to analyze here! Cancelling.");
                AveragePerformance = null;
                PerformancesFollowingInsiderBuys = ta_PerformancesFollowingInsiderBuys.ToArray();
                return;
            }

            //Now that we have all of the non derivative transactions that we are going to do (filtered heavily), add them to the focus
            ta_AnalyzedTransactions.AddRange(InDateRange);

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