using System;
using Yahoo.Finance;
using System.Threading.Tasks;

namespace TimHanewich.Reserch
{
    public class StockPerformanceCalculator
    {
        public async Task<float> CalculateStockPerformanceAsync(string symbol, DateTime d1, DateTime d2)
        {
            float p1 = await StockPriceClosestToDateAsync(symbol, d1);
            float p2 = await StockPriceClosestToDateAsync(symbol, d2);
            float diff = p2 - p1;
            float aspercent = diff / p1;
            return aspercent;            
        }

        private async Task<float> StockPriceClosestToDateAsync(string symbol, DateTime target)
        {
            HistoricalDataProvider hdp = new HistoricalDataProvider();
            await hdp.DownloadHistoricalDataAsync(symbol, target.AddDays(-5), target.AddDays(5), 20);

            //Throw an error if not successful
            if (hdp.DownloadResult != HistoricalDataDownloadResult.Successful)
            {
                throw new Exception("Unable to download historical data for '" + symbol.ToUpper().Trim() + "'. Status: " + hdp.DownloadResult.ToString());
            }

            //Find the closest
            int closest_measure = int.MaxValue;
            HistoricalDataRecord winner = hdp.HistoricalData[0];
            foreach (HistoricalDataRecord hdr in hdp.HistoricalData)
            {
                TimeSpan ts = hdr.Date - target;
                if (Math.Abs(ts.Days) < closest_measure)
                {
                    closest_measure = Math.Abs(ts.Days);
                    winner = hdr;
                }
            }

            return winner.AdjustedClose;
        }
    }
}