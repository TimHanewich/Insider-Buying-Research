using System;
using Aletheia.Service;
using Aletheia.Service.InsiderTrading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Aletheia.InsiderTrading;
using SecuritiesExchangeCommission.Edgar;
namespace TimHanewich.Reserch
{
    public class AletheiaServiceHelper
    {
        public event StatusUpdate StatusChanged;
        private string AletheiaApiKey;

        public AletheiaServiceHelper(string api_key)
        {
            AletheiaApiKey = api_key;
        }

        public async Task<SecurityTransactionHolding[]> GetAllSecurityTransactionHoldingsAsync(string company_cik_or_symbol)
        {
            AletheiaService service = new AletheiaService(AletheiaApiKey);
            UpdateStatus("Getting starter transactions...");
            SecurityTransactionHolding[] sths = await service.LatestTransactionsAsync(company_cik_or_symbol, null, 100, null, SecurityType.NonDerivative, TransactionType.OpenMarketOrPrivatePurchase);

            //Collect
            List<SecurityTransactionHolding> ToReturn = new List<SecurityTransactionHolding>();
            while (sths.Length > 0)
            {
                UpdateStatus("Adding " + sths.Length.ToString() + " transactions...");

                foreach (SecurityTransactionHolding sth in sths)
                {
                    ToReturn.Add(sth);
                }
                
                //Find the oldest transaction in the group
                UpdateStatus("Finding oldest transaction...");
                DateTime oldest = DateTime.Now;
                foreach (SecurityTransactionHolding sth in sths)
                {
                    if (sth.TransactionDate.HasValue)
                    {
                        if (sth.TransactionDate < oldest)
                        {
                            oldest = sth.TransactionDate.Value;
                        }
                    }
                }
                UpdateStatus("Oldest transaction in this group: " + oldest.ToShortDateString());

                //Search again
                UpdateStatus("Querying again...");
                sths = await service.LatestTransactionsAsync(company_cik_or_symbol, null, 100, oldest, SecurityType.NonDerivative, TransactionType.OpenMarketOrPrivatePurchase);
            }

            UpdateStatus("Collection complete! " + ToReturn.Count.ToString("#,##0") + " found.");
            return ToReturn.ToArray();
        }

        private void UpdateStatus(string msg)
        {
            if (StatusChanged != null)
            {
                StatusChanged.Invoke(msg);
            }
        }
    }
    
    public delegate void StatusUpdate(string msg);
}