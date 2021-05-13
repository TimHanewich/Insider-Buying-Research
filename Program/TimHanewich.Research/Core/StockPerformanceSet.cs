using System;
using TimHanewich.Reserch;
using System.Threading.Tasks;

namespace TimHanewich.Reserch.Core
{
    public class StockPerformanceSet
    {
        public float Return14 {get; set;}
        public float Return30 {get; set;}
        public float Return90 {get; set;}
        public float Return180 {get; set;}
        public float Return360 {get; set;}

        public async Task CalculateReturnsAsync(string stock, DateTime from)
        {
            StockPerformanceCalculator calc = new StockPerformanceCalculator();
            Return14 = await calc.CalculateStockPerformanceAsync(stock, from, from.AddDays(14));
            await Task.Delay(500);
            Return30 = await calc.CalculateStockPerformanceAsync(stock, from, from.AddDays(30));
            await Task.Delay(500);
            Return90 = await calc.CalculateStockPerformanceAsync(stock, from, from.AddDays(90));
            await Task.Delay(500);
            Return180 = await calc.CalculateStockPerformanceAsync(stock, from, from.AddDays(180));
            await Task.Delay(500);
            Return360 = await calc.CalculateStockPerformanceAsync(stock, from, from.AddDays(360));
        }
    }
}