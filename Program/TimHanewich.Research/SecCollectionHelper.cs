using System;
using SecuritiesExchangeCommission.Edgar;
using System.Threading.Tasks;
using Aletheia.Engine;
using System.Collections.Generic;
using TimHanewich.Investing;
using Newtonsoft.Json;
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
            TryPrintStatus(filings3.Length.ToString("#,##0") + " form 3's found");
            await Task.Delay(500);
            TryPrintStatus("Getting all form 4 filings...");
            EdgarFiling[] filings4 = await AletheiaToolkit.CollectAllFilingsOfTypeAsync(symbol_or_cik, "4", false);
            TryPrintStatus(filings4.Length.ToString("#,##0") + " form 4's found");
            await Task.Delay(500);
            TryPrintStatus("Getting all form 5 filings...");
            EdgarFiling[] filings5 = await AletheiaToolkit.CollectAllFilingsOfTypeAsync(symbol_or_cik, "5", false);
            TryPrintStatus(filings5.Length.ToString("#,##0") + " form 5's found");
            await Task.Delay(500);

            //Add to a basket
            List<EdgarFiling> AllFilings = new List<EdgarFiling>();
            AllFilings.AddRange(filings3);
            AllFilings.AddRange(filings4);
            AllFilings.AddRange(filings5);
            TryPrintStatus("Total of " + AllFilings.Count.ToString("#,##0") + " found!");
            await Task.Delay(500);

            //Convert all of these
            int counter = 1;
            foreach (EdgarFiling ef in AllFilings)
            {
                string prefix = "# " + counter.ToString("#,##0") + " / " + AllFilings.Count.ToString("#,##0") + ": ";
                TryPrintStatus("Starting collection process for filing # " + counter.ToString("#,##0") + " / " + AllFilings.Count.ToString("#,##0") + " (" + ef.DocumentsUrl + ")");

                TryPrintStatus(prefix + "Getting filing details");
                EdgarFilingDetails efd = await ef.GetFilingDetailsAsync();
                await Task.Delay(100);

                //Finding file
                if (efd.DocumentFormatFiles != null)
                {
                    TryPrintStatus(prefix + "Finding data file.");
                    foreach (FilingDocument fd in efd.DocumentFormatFiles)
                    {
                        if (fd.DocumentType != null)
                        {
                            if (fd.DocumentType == "4" || fd.DocumentType == "3" || fd.DocumentType == "5")
                            {
                                if (fd.DocumentName != null)
                                {
                                    if (fd.DocumentName.ToLower().Contains(".xml"))
                                    {
                                        try
                                        {
                                            //Process the document
                                            TryPrintStatus(prefix + "Downloading and parsing data file.");
                                            StatementOfBeneficialOwnership thisform = await StatementOfBeneficialOwnership.ParseXmlFromWebUrlAsync(fd.Url);
                                            await Task.Delay(100);
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
                        } 
                    }
                }
                else
                {
                    TryPrintStatus("There were not any document format files in this filing. Moving on.");
                }
                
                
                counter = counter + 1;
            }
            
            TryPrintStatus("Complete! " + ToReturn.Count.ToString("#,##0") + " non-derivative transactions found.");
            return ToReturn.ToArray();
        }

        public async Task StoreSP500NonDerivateTransactionsInFolderAsync(string folder_path)
        {
            Console.WriteLine("Getting S&P500 list...");
            string[] sp500 = await TimHanewich.Investing.InvestingToolkit.GetEquityGroupAsync(EquityGroup.SP500);
            Console.WriteLine(sp500.Length.ToString() + " stocks found.");

            foreach (string s in sp500)
            {
                ConsoleColor oc = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("[ " + s.ToString() + " ]");
                Console.ForegroundColor = oc;

                //Check if there is a file with this already
                Console.WriteLine("Checking if should collect this one.");
                bool ShouldCollect = true;
                if (System.IO.File.Exists(folder_path + "\\" + s.ToUpper().Trim() + ".json"))
                {
                    Console.WriteLine("File already exists. Checking if it has content.");
                    //Try to pick it up and see how many are in there
                    NonDerivativeTransaction[] transactions = JsonConvert.DeserializeObject<NonDerivativeTransaction[]>(System.IO.File.ReadAllText(folder_path + "\\" + s.ToUpper().Trim() + ".json"));
                    if (transactions.Length == 0)
                    {
                        Console.WriteLine("It does not have content. Collecting!");
                        ShouldCollect = true;
                    }
                    else
                    {
                        Console.WriteLine("It does have " + transactions.Length.ToString("#,##0") + " transactions, skipping this one!");
                        ShouldCollect = false;
                    }
                }
                else
                {
                    Console.WriteLine("File does not already exist. Will collect!");
                }

                if (ShouldCollect)
                {
                    try
                    {
                        Console.WriteLine("Collecting...");
                        NonDerivativeTransaction[] transactions = await GetAllNonDerivativeTransactionsAsync(s, true);
                        Console.WriteLine(transactions.Length.ToString() + " transactions found.");
                        if (transactions.Length > 0)
                        {
                            Console.WriteLine("Writing to file...");
                            System.IO.File.WriteAllText(folder_path + "\\" + s + ".json", JsonConvert.SerializeObject(transactions));
                        }
                        else
                        {
                            Console.WriteLine("0 transactions found so will not write to file.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Fatal failre on stock " + s + ": " + ex.Message);
                    }
                }

                Console.ForegroundColor = oc;                            
            }
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