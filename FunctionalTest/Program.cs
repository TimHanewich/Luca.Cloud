using System;
using Luca.Cloud;
using Xbrl.FinancialStatement;
using Newtonsoft.Json;
using Luca;

namespace FunctionalTest
{
    class Program
    {
        static void Main(string[] args)
        {
            TestHistoric10ks();
        }

        static void SingleTest10K()
        {
            do
            {
                Console.WriteLine("What stock?");
                string stock = Console.ReadLine();
                LucaCloudHelper lch = new LucaCloudHelper();
                LucaDataPackage ldp = lch.GetFinancialsAsync(stock, "10-K", DateTime.Now, false).Result;
                string json = JsonConvert.SerializeObject(ldp);
                Console.WriteLine(json);
            } while (true);
        }


        static void TestHistoric10ks()
        {
            do
            {

                Console.WriteLine("What stock?");
                string stock = Console.ReadLine();
                LucaCloudHelper lch = new LucaCloudHelper();
                LucaDataPackage[] data = lch.GetHistoricalFinancialsAsync(stock, "10-K", false).Result;
                foreach (LucaDataPackage ldp in data)
                {
                    string rev = "?";
                    if (ldp.FinancialStatementContent.Revenue.HasValue)
                    {
                        rev = ldp.FinancialStatementContent.Revenue.Value.ToString("#,##0");
                    }
                    Console.WriteLine(ldp.FilingDate.ToShortDateString() + " - " + rev);
                }
                

            } while (true);
        }

        static void TestHistoric10qs()
        {
            do
            {

                Console.WriteLine("What stock?");
                string stock = Console.ReadLine();
                LucaCloudHelper lch = new LucaCloudHelper();
                LucaDataPackage[] data = lch.GetHistoricalFinancialsAsync(stock, "10-Q", false).Result;
                foreach (LucaDataPackage ldp in data)
                {
                    string rev = "?";
                    if (ldp.FinancialStatementContent.Revenue.HasValue)
                    {
                        rev = ldp.FinancialStatementContent.Revenue.Value.ToString("#,##0");
                    }
                    Console.WriteLine(ldp.FilingDate.ToShortDateString() + " - " + rev);
                }
                

            } while (true);
        }

       
    
    
    }
}
