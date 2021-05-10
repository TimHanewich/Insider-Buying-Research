using System;
using SecuritiesExchangeCommission.Edgar;
using System.Threading.Tasks;
using Aletheia.Engine;
using System.Collections.Generic;

namespace TimHanewich.Reserch
{
    public class SecCollectionHelper
    {
        private bool PrintStatus;

        public async Task<NonDerivativeTransaction[]> GetAllNonDerivativeTransactionsAsync(string symbol_or_cik, bool print_status = false)
        {
            List<NonDerivativeTransaction> ToReturn = new List<NonDerivativeTransaction>();
            PrintStatus = print_status;

            TryPrintStatus("Getting all form 3 filings...");
            EdgarFiling[] filings3 = await AletheiaToolkit.CollectAllFilingsOfTypeAsync(symbol_or_cik, "3", false);
            TryPrintStatus("Getting all form 4 filings...");
            EdgarFiling[] filings4 = await AletheiaToolkit.CollectAllFilingsOfTypeAsync(symbol_or_cik, "4", false);
            TryPrintStatus("Getting all form 5 filings...");
            EdgarFiling[] filings5 = await AletheiaToolkit.CollectAllFilingsOfTypeAsync(symbol_or_cik, "5", false);

            //Add to a basket
            List<EdgarFiling> AllFilings = new List<EdgarFiling>();
            AllFilings.AddRange(filings3);
            AllFilings.AddRange(filings4);
            AllFilings.AddRange(filings5);

            //Convert all of these
            int counter = 1;
            foreach (EdgarFiling ef in AllFilings)
            {
                string prefix = "# " + counter.ToString("#,##0") + " / " + AllFilings.Count.ToString("#,##0") + ": ";
                TryPrintStatus("Starting collection process for filing # " + counter.ToString("#,##0") + " / " + AllFilings.Count.ToString("#,##0"));

                TryPrintStatus(prefix + "Getting filing details");
                EdgarFilingDetails efd = await ef.GetFilingDetailsAsync();

                //Finding file
                TryPrintStatus(prefix + "Finding data file.");
                foreach (FilingDocument fd in efd.DocumentFormatFiles)
                {
                    if (fd.DocumentType == "4")
                    {
                        if (fd.DocumentName.ToLower().Contains(".xml"))
                        {
                            try
                            {
                                //Process the document
                                TryPrintStatus(prefix + "Downloading and parsing data file.");
                                StatementOfBeneficialOwnership thisform = await StatementOfBeneficialOwnership.ParseXmlFromWebUrlAsync(fd.Url);
                                TryPrintStatus(prefix + thisform.NonDerivativeTransactions.Length.ToString("#,##0") + " transactions found.");
                                ToReturn.AddRange(thisform.NonDerivativeTransactions);
                            }
                            catch
                            {
                                TryPrintStatus(prefix + "FAILURE! Moving on......");
                            }
                            


                        }
                    }
                }
                
                counter = counter + 1;
            }
            
            TryPrintStatus("Complete! " + ToReturn.Count.ToString("#,##0") + " non-derivative transactions found.");
            return ToReturn.ToArray();
        }

        private void TryPrintStatus(string status)
        {
            if (PrintStatus)
            {
                Console.WriteLine(status);
            }
        }
    }
}