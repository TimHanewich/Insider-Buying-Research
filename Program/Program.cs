using System;
using SecuritiesExchangeCommission.Edgar;
using Aletheia.Engine;
using TimHanewich.Reserch;
using TimHanewich.Reserch.Trailing;
using TimHanewich.Reserch.Core;
using Newtonsoft.Json;
using TimHanewich.Csv;
using System.Collections.Generic;
using System.IO;

namespace Insider_Buying_Research
{
    class Program
    {
        static void Main(string[] args)
        {
            CountAllTransactions();
        }

        #region "Utility/research"

        public static void CountAllTransactions()
        {
            Console.Write("Folder with transactions: ");
            string folder = Console.ReadLine().Replace("\"", "");

            Console.Write("Output CSV to what folder?");
            string outputto = Console.ReadLine().Replace("\"", "");

            CsvFile csv = new CsvFile();
            ResearchToolkit rt = new ResearchToolkit();
            

            string[] files = System.IO.Directory.GetFiles(folder);
            foreach (string s in files)
            {
                NonDerivativeTransaction[] transactions = JsonConvert.DeserializeObject<NonDerivativeTransaction[]>(System.IO.File.ReadAllText(s));
                
                //Count those between 2010 and 2019
                int count = 0;
                NonDerivativeTransaction[] filt = rt.FilterToTransactionsOfInterest(transactions);
                count = filt.Length;

                DataRow dr = csv.AddNewRow();
                dr.Values.Add(System.IO.Path.GetFileName(s));
                dr.Values.Add(count.ToString());
                Console.WriteLine(System.IO.Path.GetFileName(s) + "-" + count.ToString());
            }

            Stream ss = System.IO.File.Create(outputto + "\\output.csv");
            StreamWriter sw = new StreamWriter(ss);
            sw.Write(csv.GenerateAsCsvFileContent());
            sw.Close();
            ss.Close();
            Console.WriteLine("Written!");
        }

        #endregion
        
        //Step 1: Analyze stock performance
        public static void PerformFullAnalysis()
        {
            Console.Write("Path of folder containing S&P500 equity transactions (or single file): ");
            string path1 = Console.ReadLine();
            Console.Write("Path of full analysis export: ");
            string path2 = Console.ReadLine();

            //Clean paths
            path1 = path1.Replace("\"", "");
            path2 = path2.Replace("\"", "");

            //Get the files
            string[] files = null;
            if (System.IO.Directory.Exists(path1))
            {
                files = System.IO.Directory.GetFiles(path1);
            }
            else if (System.IO.File.Exists(path1))
            {
                files = new string[] {path1};
            }
            
            //Process each
            foreach (string s in files)
            {
                string file_name = System.IO.Path.GetFileName(s);
                AdminPrint("Checking " + file_name + "...");
                if (System.IO.File.Exists(path2 + "\\" + file_name) == false)
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
                                System.IO.File.WriteAllText(path2 + "\\" + file_name, JsonConvert.SerializeObject(frs));
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
            List<string> BlankFiles = new List<string>();
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
                    Console.WriteLine(s);
                    BlankFiles.Add(s);
                }
            }
            AdminPrint("Delete the ones that did not contain useful data? 'yes' to confirm", ConsoleColor.Yellow);
            string ip = Console.ReadLine();
            if (ip == "yes")
            {
                foreach (string s in BlankFiles)
                {
                    AdminPrint("Deleting " + s + "...", ConsoleColor.Red);
                    System.IO.File.Delete(s);
                }
            }
            else
            {
                Console.WriteLine("Will not delete.");
            }
        }

        //Step 3: Assemble results into a CSV file
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
            FileStream fs = System.IO.File.Create(path);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(csv.GenerateAsCsvFileContent());
            fs.Close();
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
