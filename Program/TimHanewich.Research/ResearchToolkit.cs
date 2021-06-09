using System;
using System.IO;
using SecuritiesExchangeCommission.Edgar;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TimHanewich.Reserch.Trailing;
using System.Collections.Generic;
using System.Linq;


namespace TimHanewich.Reserch
{
    public class ResearchToolkit
    {
        public async Task<NonDerivativeTransaction[]> OpenNonDerivativeTransactionsAsync(string path)
        {
            Stream s = System.IO.File.OpenRead(path);
            NonDerivativeTransaction[] ToReturn = await OpenNonDerivativeTransactionsAsync(s);
            return ToReturn;            
        }

        public async Task<NonDerivativeTransaction[]> OpenNonDerivativeTransactionsAsync(Stream s)
        {
            StreamReader sr = new StreamReader(s);
            string content = await sr.ReadToEndAsync();
            NonDerivativeTransaction[] ToReturn = JsonConvert.DeserializeObject<NonDerivativeTransaction[]>(content);
            return ToReturn;
        }
    
        public NonDerivativeTransaction[] FilterToBuys(NonDerivativeTransaction[] all)
        {
            List<NonDerivativeTransaction> ToReturn = new List<NonDerivativeTransaction>();
            foreach (NonDerivativeTransaction ndt in all)
            {
                if (ndt.AcquiredOrDisposed == AcquiredDisposed.Acquired)
                {
                    if (ndt.TransactionCode == TransactionType.OpenMarketOrPrivatePurchase)
                    {
                        ToReturn.Add(ndt);
                    }
                }
            }
            return ToReturn.ToArray();
        }

        public NonDerivativeTransaction[] FilterToTimePeriod(NonDerivativeTransaction[] all)
        {
            List<NonDerivativeTransaction> F2 = new List<NonDerivativeTransaction>();
            foreach (NonDerivativeTransaction ndt in all)
            {
                if (ndt.TransactionDate.HasValue)
                {
                    if (ndt.TransactionDate.Value.Year >= 2010 && ndt.TransactionDate.Value.Year <= 2019) //Year is between 2010 and 2019, including 2010 and 2019
                    {
                        F2.Add(ndt);
                    }
                }
            }
            return all.ToArray();
        }

        public NonDerivativeTransaction[] FilterToTransactionsOfInterest(NonDerivativeTransaction[] all)
        {
            //Filter to only buys
            NonDerivativeTransaction[] F1 = FilterToBuys(all);

            //Filter to occuring between 2010 and 2019 (first day of 2010 to last day of 2019)
            NonDerivativeTransaction[] F2 = FilterToTimePeriod(F1);

            return F2;
        }

        //Arrange from oldest to newest
        public NonDerivativeTransaction[] SortByTransactionDate(NonDerivativeTransaction[] all)
        {
            List<NonDerivativeTransaction> ToReturn = new List<NonDerivativeTransaction>();
            List<NonDerivativeTransaction> ToPullFrom = new List<NonDerivativeTransaction>();
            ToPullFrom.AddRange(all);
            while (ToPullFrom.Count > 0)
            {
                NonDerivativeTransaction winner = ToPullFrom[0];
                foreach (NonDerivativeTransaction ndt in ToPullFrom)
                {
                    if (ndt.TransactionDate.Value < winner.TransactionDate.Value)
                    {
                        winner = ndt;
                    }
                }
                ToReturn.Add(winner);
                ToPullFrom.Remove(winner);
            }
            return ToReturn.ToArray();
        }

        public TrailingAverage[] CalculateTrailingAverages(NonDerivativeTransaction[] transactions, int trailing_days)
        {

            //Sort by date
            NonDerivativeTransaction[] ndts = SortByTransactionDate(transactions);

            //Make a list of dates
            List<DateTime> DatesToCompile = new List<DateTime>();
            DateTime Oldest = ndts[0].TransactionDate.Value;
            DateTime Newest = ndts[ndts.Length-1].TransactionDate.Value;
            DatesToCompile.Add(Oldest.AddDays(trailing_days * -1)); //Add the starter. Start days prior so the moving average doesn't start high;
            while (DatesToCompile.Contains(Newest) == false)
            {
                DatesToCompile.Add(DatesToCompile[DatesToCompile.Count-1].AddDays(1));
            }

            //Assemble into key value pairs with the # of shares purchased as the value
            List<KeyValuePair<DateTime, float>> Kvps = new List<KeyValuePair<DateTime, float>>();
            foreach (DateTime dt in DatesToCompile)
            {
                //Find how many were purchased on this day. Assume 0 unless we have one
                float shares_purchased = 0f;
                foreach (NonDerivativeTransaction ndt in ndts)
                {
                    if (ndt.TransactionDate.Value.ToShortDateString() == dt.ToShortDateString())
                    {
                        shares_purchased = ndt.TransactionQuantity.Value;
                    }
                }

                KeyValuePair<DateTime, float> ThisKvp = new KeyValuePair<DateTime, float>(dt, shares_purchased);
                Kvps.Add(ThisKvp);
            }

            //Assemble a list to return
            List<TrailingAverage> ToReturn = new List<TrailingAverage>();
            int i = 0;
            foreach (KeyValuePair<DateTime, float> kvp in Kvps)
            {
                TrailingAverage ta = new TrailingAverage();
                ta.Date = kvp.Key;
                ta.Value = GetFlexibleTrailingAverage(Kvps.ToArray(), i, trailing_days);
                ToReturn.Add(ta);
                i = i + 1;
            }
            return ToReturn.ToArray();

        }
    
        private float GetFlexibleTrailingAverage(KeyValuePair<DateTime, float>[] kvps, int on_index, int trailing_days)
        {
            List<float> CompiledUpToIndex = new List<float>();
            int t = 0;
            while (t <= on_index)
            {
                CompiledUpToIndex.Add(kvps[t].Value);
                t = t + 1;
            }

            //Trim or just return?
            if (CompiledUpToIndex.Count < trailing_days)
            {
                return CompiledUpToIndex.Average();
            }
            else
            {
                List<float> ToReturnAvg = new List<float>();
                while (ToReturnAvg.Count < trailing_days)
                {
                    ToReturnAvg.Add(CompiledUpToIndex[CompiledUpToIndex.Count-1]); //Add the last one
                    CompiledUpToIndex.RemoveAt(CompiledUpToIndex.Count-1); //Remove the last one
                }
                return ToReturnAvg.Average();
            }
        }
    }
}