using System;
using Yahoo.Finance;
using System.Threading.Tasks;

namespace TimHanewich.Reserch
{
    public class StockPerformanceCalculator
    {
        public async Task<float> CalculateStockPerformanceAsync(string symbol, DateTime d1, DateTime d2)
        {
            HistoricalDataProvider hdp1 = new HistoricalDataProvider();
            await hdp1.DownloadHistoricalDataAsync(symbol, d1, d1); //Just for day 1
            
            HistoricalDataProvider hdp2 = new HistoricalDataProvider();
            await hdp2.DownloadHistoricalDataAsync(symbol, d2, d2); //Just for day 2

            float diff = hdp2.HistoricalData[0].AdjustedClose - hdp1.HistoricalData[0].AdjustedClose;
            float aspercent = diff / hdp1.HistoricalData[0].AdjustedClose;
            return aspercent;            
        }
    }
}