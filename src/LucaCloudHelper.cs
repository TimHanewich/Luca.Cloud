using System;
using Xbrl;
using Xbrl.FinancialStatement;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using SecuritiesExchangeCommission.Edgar;
using Luca;

namespace Luca.Cloud
{
    public class LucaCloudHelper
    {
        public async Task<LucaDataPackage> GetFinancialsAsync(string symbol, string filing, DateTime before, bool force_calculation)
        {
            
            if (filing.ToLower() != "10-k" && filing.ToLower() != "10-q")
            {
                throw new Exception("Did not recognize filing '" + filing + "'. Use only '10-K' or '10-Q'.");
            }
            
            string filing_for_api = "";
            if (filing.ToLower() == "10-k")
            {
                filing_for_api = "10k";
            }
            else if (filing.ToLower() == "10-q")
            {
                filing_for_api = "10q";
            }
            
            string url = "https://projectluca.azurewebsites.net/api/GetFinancials?";
            url = url + "symbol=" + symbol.Trim().ToLower();
            url = url + "&filing=" + filing_for_api.Trim().ToLower();
            url = url + "&before=" + before.Month.ToString("00") + before.Day.ToString("00") + before.Year.ToString("0000");
            url = url + "&forcecalculation=" + force_calculation.ToString();
            
            
            HttpClient hc = new HttpClient();
            hc.Timeout = new TimeSpan(0, 10, 0);
            HttpResponseMessage hrm = await hc.GetAsync(url);
            string resp = await hrm.Content.ReadAsStringAsync();
            if (hrm.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("API Call Failed. Message: " + resp);
            }


            LucaDataPackage ldp;
            try
            {
                ldp = JsonConvert.DeserializeObject<LucaDataPackage>(resp);
            }
            catch
            {
                throw new Exception("Fatal error while parsing JSON.");
            }

            return ldp;
        }
    
        public async Task<LucaDataPackage[]> GetHistoricalFinancialsAsync(string symbol, string filing, bool force_calculation)
        {
            if (filing.ToLower() != "10-k" && filing.ToLower() != "10-q")
            {
                throw new Exception("Did not recognize filing '" + filing + "'. Use only '10-K' or '10-Q'.");
            }


            EdgarSearch es = await EdgarSearch.CreateAsync(symbol, filing, null);

            //Get a list of dates to pull (right before each 10-? filing date)
            List<DateTime> BeforeDates = new List<DateTime>();
            foreach (EdgarSearchResult esr in es.Results)
            {
                if (esr.Filing == filing)
                {
                    if (esr.InteractiveDataUrl != null & esr.InteractiveDataUrl != "")
                    {
                        BeforeDates.Add(esr.FilingDate.AddDays(5));
                    }
                }
            }

            //Get a filing to call for the api
            string filing_for_api = "";
            if (filing.ToLower() == "10-k")
            {
                filing_for_api = "10k";
            }
            else if (filing.ToLower() == "10-q")
            {
                filing_for_api = "10q";
            }

            //Assemble a list of calls
            HttpClient hc = new HttpClient();
            hc.Timeout = new TimeSpan(0,10,0);
            List<Task<HttpResponseMessage>> responses = new List<Task<HttpResponseMessage>>();
            foreach (DateTime dt in BeforeDates)
            {
                string url = "https://projectluca.azurewebsites.net/api/GetFinancials?";
                url = url + "symbol=" + symbol.Trim().ToLower();
                url = url + "&filing=" + filing_for_api.Trim().ToLower();
                url = url + "&before=" + dt.Month.ToString("00") + dt.Day.ToString("00") + dt.Year.ToString("0000");
                url = url + "&forcecalculation=" + force_calculation.ToString();
                responses.Add(hc.GetAsync(url));
            }



            //Call them
            //Console.WriteLine(responses.Count.ToString() + " requests");
            HttpResponseMessage[] resps = await Task.WhenAll(responses);
            //Console.WriteLine(resps.Length.ToString() + " responses");

            //Parse
            List<LucaDataPackage> statements = new List<LucaDataPackage>();
            foreach (HttpResponseMessage hrm in resps)
            {
                if (hrm.StatusCode == HttpStatusCode.OK)
                {
                    string content = await hrm.Content.ReadAsStringAsync();
                    LucaDataPackage fs = JsonConvert.DeserializeObject<LucaDataPackage>(content);
                    statements.Add(fs);
                }
            }

            return statements.ToArray();

        }
    }
}