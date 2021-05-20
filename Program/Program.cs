using System;
using SecuritiesExchangeCommission.Edgar;
using Aletheia.Engine;
using TimHanewich.Reserch;
using TimHanewich.Reserch.Trailing;
using TimHanewich.Reserch.Core;
using Newtonsoft.Json;
using TimHanewich.Csv;

namespace Insider_Buying_Research
{
    class Program
    {
        static void Main(string[] args)
        {
            CheckData();
        }

        
        //Step 1
        public static void PerformFulAnalysis(string non_derivative_transactions_folder, string place_analyses_in)
        {
            string[] files = System.IO.Directory.GetFiles(non_derivative_transactions_folder);
            foreach (string s in files)
            {
                string file_name = System.IO.Path.GetFileName(s);
                AdminPrint("Checking " + file_name + "...");
                if (System.IO.File.Exists(place_analyses_in + "\\" + file_name) == false)
                {
                    AdminPrint("This has not been research yet. Going!");

                    //Reseaerch
                    bool ResearchFailed = false;
                    FullResearchSet frs = new FullResearchSet();
                    frs.StatusPrintingOn();
                    try
                    {
                        frs.GenerateFromTransactionsFileAsync(s).Wait();
                    }
                    catch (Exception ex)
                    {
                        AdminPrint("Generation for " + file_name + " failed. Msg: " + ex.Message, ConsoleColor.Red);
                        ResearchFailed = true;
                    }
                    
                    //Write it (if it has analyses)
                    if (frs.PerformancesFollowingInsiderBuys != null)
                    {
                        if (frs.PerformancesFollowingInsiderBuys.Length > 0)
                        {
                            if (ResearchFailed == false)
                            {
                                AdminPrint("Writing to file...");
                                System.IO.File.WriteAllText(place_analyses_in + "\\" + file_name, JsonConvert.SerializeObject(frs));
                                AdminPrint("Successfully written!");
                            }  
                        }
                        else
                        {
                            AdminPrint("Not going to write this one to file... It had no performance analyses following transctions!", ConsoleColor.Yellow);
                        }
                    }
                    
                }
                else
                {
                    AdminPrint("This has already been researched. skipping.");
                }
            }
        }
    
        //Step 2: Make sure all full analyses have data
        public static void CheckData()
        {
            Console.Write("Path to the folder containing the analyses: ");
            string path = Console.ReadLine();
            path = path.Replace("\"", "");
            string[] files = System.IO.Directory.GetFiles(path);
            foreach (string s in files)
            {
                FullResearchSet frs = JsonConvert.DeserializeObject<FullResearchSet>(System.IO.File.ReadAllText(s));
                bool flagthis = false;
                if (frs.AveragePerformance == null)
                {
                    flagthis = true;
                }
                if (frs.PerformancesFollowingInsiderBuys == null)
                {
                    flagthis = true;
                }
                if (frs.PerformancesFollowingInsiderBuys.Length == 0)
                {
                    flagthis = true;
                }
                if (flagthis)
                {
                    Console.WriteLine(frs.Symbol);
                }
            }
        }

        public static void AssembleResultsCsv()
        {
            Console.Write("Folder with the full analyses >");
            string analysis_folder = Console.ReadLine().Replace("\"", "");

            Console.Write("Output CSV file to folder:");
            string output_folder = Console.ReadLine().Replace("\"", "");

            string[] files = System.IO.Directory.GetFiles(analysis_folder);
            AdminPrint(files.Length.ToString() + " files found");

            CsvFile csv = new CsvFile();
            DataRow headerrow = csv.AddNewRow();
            headerrow.Values.Add("Symbol");

            headerrow.Values.Add("Insider Buys");

            //Average
            headerrow.Values.Add("14 Days");
            headerrow.Values.Add("30 Days");
            headerrow.Values.Add("90 Days");
            headerrow.Values.Add("180 Days");
            headerrow.Values.Add("360 Days");

            //Following buys
            headerrow.Values.Add("14 Days");
            headerrow.Values.Add("30 Days");
            headerrow.Values.Add("90 Days");
            headerrow.Values.Add("180 Days");
            headerrow.Values.Add("360 Days");

            foreach (string s in files)
            {
                DataRow dr = csv.AddNewRow();

                AdminPrint("Opening " + System.IO.Path.GetFileName(s) + "...");
                FullResearchSet frs = JsonConvert.DeserializeObject<FullResearchSet>(System.IO.File.ReadAllText(s));
                AdminPrint("Writing " + System.IO.Path.GetFileName(s) + "...");

                dr.Values.Add(frs.Symbol.ToUpper().Trim());
                dr.Values.Add(frs.PerformancesFollowingInsiderBuys.Length.ToString());

                //Write the average over that period
                dr.Values.Add(frs.AveragePerformance.Return14.ToString());
                dr.Values.Add(frs.AveragePerformance.Return30.ToString());
                dr.Values.Add(frs.AveragePerformance.Return90.ToString());
                dr.Values.Add(frs.AveragePerformance.Return180.ToString());
                dr.Values.Add(frs.AveragePerformance.Return360.ToString());

                //Get the averages
                StockPerformanceSet avg = StockPerformanceSet.Average(frs.PerformancesFollowingInsiderBuys);
                dr.Values.Add(avg.Return14.ToString());
                dr.Values.Add(avg.Return30.ToString());
                dr.Values.Add(avg.Return90.ToString());
                dr.Values.Add(avg.Return180.ToString());
                dr.Values.Add(avg.Return360.ToString()); 
            }

            //Write to the file
            AdminPrint("Writing to file...");
            string path = output_folder + "\\InsiderBuyPerformance.csv";
            System.IO.File.Create(path);
            System.IO.File.WriteAllText(path, csv.GenerateAsCsvFileContent());
            AdminPrint("Successfully wrote to " + path, ConsoleColor.Green);
        }


        private static void AdminPrint(string msg, ConsoleColor cc = ConsoleColor.Blue)
        {
            ConsoleColor oc = Console.ForegroundColor;
            Console.ForegroundColor = cc;
            Console.WriteLine(msg);
            Console.ForegroundColor = oc;
        }
    }
}
