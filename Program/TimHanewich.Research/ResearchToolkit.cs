using System;
using System.IO;
using SecuritiesExchangeCommission.Edgar;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
    }
}