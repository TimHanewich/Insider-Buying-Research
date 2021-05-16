using System;
using TimHanewich.Reserch;
using System.Threading.Tasks;

namespace TimHanewich.Reserch.Core
{
    public class StockPerformanceSet
    {
        //Time frame context
        public DateTime Beginning {get; set;}
        public DateTime End {get; set;}

        //Day count returns
        public float Return14 {get; set;}
        public float Return30 {get; set;}
        public float Return90 {get; set;}
        public float Return180 {get; set;}
        public float Return360 {get; set;}

        public async Task CalculateReturnsAsync(string stock, DateTime from)
        {
            //Set context
            Beginning = from;
            End = from.AddYears(1);

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

        public async Task CalculateAverageReturnsAsync(string stock, DateTime beginning, DateTime end)
        {
            //Set context
            Beginning = beginning;
            End = end;

            //Get the performance since that time
            StockPerformanceCalculator calc = new StockPerformanceCalculator();
            float FullReturn = await calc.CalculateStockPerformanceAsync(stock, beginning, end);
            TimeSpan ts = end - beginning;
            float days = Convert.ToSingle(ts.Days);

            //Percent
            Return14 = Convert.ToSingle(Math.Pow(1f + FullReturn, Convert.ToSingle(14) / days) - 1);
            Return30 = Convert.ToSingle(Math.Pow(1f + FullReturn, Convert.ToSingle(30) / days) - 1);
            Return90 = Convert.ToSingle(Math.Pow(1f + FullReturn, Convert.ToSingle(90) / days) - 1);
            Return180 = Convert.ToSingle(Math.Pow(1f + FullReturn, Convert.ToSingle(180) / days) - 1);
            Return360 = Convert.ToSingle(Math.Pow(1f + FullReturn, Convert.ToSingle(360) / days) - 1);
        }
    }
}