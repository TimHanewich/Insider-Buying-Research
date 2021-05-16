using System;
using SecuritiesExchangeCommission.Edgar;
using Aletheia.Engine;
using TimHanewich.Reserch;
using TimHanewich.Reserch.Trailing;
using TimHanewich.Reserch.Core;
using Newtonsoft.Json;

namespace Insider_Buying_Research
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Path of folder containing S&P500 equity transactions: ");
            string path1 = Console.ReadLine();
            Console.Write("Path of full analysis export: ");
            string path2 = Console.ReadLine();

            //Clean paths
            path1 = path1.Replace("\"", "");
            path2 = path2.Replace("\"", "");
            
            PerformFulAnalysis(path1, path2);
        }

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
                    
                    //Write it
                    if (ResearchFailed == false)
                    {
                        AdminPrint("Writing to file...");
                        System.IO.File.WriteAllText(place_analyses_in + "\\" + file_name, JsonConvert.SerializeObject(frs));
                        AdminPrint("Successfully written!");
                    }  
                }
                else
                {
                    AdminPrint("This has already been researched. skipping.");
                }
            }
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
