using AngleSharp;
using AngleSharp.Dom;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ParserRusal
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await StartParsing();
        }

        public static async Task StartSeleniumParsing()
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("headless");

            IWebDriver driver = new ChromeDriver(chromeOptions);
            driver.Url = @"https://tender.rusal.ru/Tenders";

            driver.FindElement(By.XPath("//span[contains(.,'Дополнительные параметры')]")).Click();

            driver.FindElement(By.XPath("//button[@class='btn-default btn-accept-cookie']")).Click();

            driver.FindElement(By.XPath("(//i[@class='icon-arrow-down icon-large'])[7]")).Click();
            await Task.Delay(100);

            driver.FindElement(By.XPath("(//span[contains(.,'ЖД, авиа, авто, контейнерные перевозки')])[2]")).Click();

            driver.FindElement(By.XPath("//span[contains(.,'Искать')]")).Click();
            await Task.Delay(300);

            new SelectElement(driver.FindElement(By.XPath("//select[@data-skip-form-reset='true']"))).SelectByValue("100");
            await Task.Delay(300);

            var htmlItems = driver.FindElements(By.XPath("(//a[contains(@class,'ui-link')])"));
            foreach (var item in htmlItems)
            {
                Console.WriteLine(item.Text);
            }

        }

        public static async Task StartParsing()
        {
            using (var httpClient = new HttpClient())
            {

                int totalCount = await GetTotalCountAsync();
                Console.WriteLine(totalCount);

                var values = new Dictionary<string, string>
                {
                    { "limit", "10" },
                    { "offset", "0" },
                    { "sortAsc", "false" },
                    { "sortColumn", "EntityNumber" },
                    { "ClassifiersFieldData.SiteSectionType", "bef4c544-ba45-49b9-8e91-85d9483ff2f6" },
                };

                var content = new FormUrlEncodedContent(values);
                var response = await httpClient.PostAsync("https://tender.rusal.ru/Tenders/Load", content);
                var responseString = await response.Content.ReadAsStringAsync();
                var items = JObject.Parse(responseString)["Rows"].ToObject<Item[]>();

                foreach (var item in items)
                {
                    var html = await PostAsync(item.TenderViewUrl);
                    item.StartApplicationDate = GetStartApplicationDate(html);
                    item.DocumentInfos = await GetDocuments(html);
                    Console.WriteLine(item);
                }
            }
        }

        public static async Task<string> PostAsync(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Headers.Add("x-csrf-token", "Fetch");
            request.Headers.Add("x-requested-with", "XMLHttpRequest");
            request.Headers.Add("X-Content-Requested-For", "Tab");
            request.Method = "POST";

            //request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public static async Task<int> GetTotalCountAsync()
        {
            using (var httpClient = new HttpClient())
            {
                var values = new Dictionary<string, string>
                {
                    { "limit", "10" },
                    { "offset", "0" },
                    { "sortAsc", "false" },
                    { "sortColumn", "EntityNumber" },
                    { "ClassifiersFieldData.SiteSectionType", "bef4c544-ba45-49b9-8e91-85d9483ff2f6" },
                };

                var content = new FormUrlEncodedContent(values);

                var response = await httpClient.PostAsync("https://tender.rusal.ru/Tenders/Load", content);

                var responseString = await response.Content.ReadAsStringAsync();

                var TotalCount = JObject.Parse(responseString)["Paging"]["Total"].ToString();
                return int.Parse(TotalCount);
            }
        }

        public static string GetStartApplicationDate(string html)
        {
            string startDatePattern = @"<div class=""control-readonly"" data-field-name=""Fields.RequestReceivingBeginDate"">(?<val>.*?)<\/div>";
            RegexOptions options = RegexOptions.Compiled | RegexOptions.Singleline;
            Regex startDateRegex = new Regex(startDatePattern, options);
            Match match = startDateRegex.Match(html);
            string Result = "";

            while (match.Success)
            {
                Result += match.Groups["val"].Value;
                match = match.NextMatch();
            }
            return Result.Trim();
        }

        public static async Task<List<DocumentInfo>> GetDocuments(string html)
        {
            IConfiguration config = Configuration.Default;
            IBrowsingContext context = BrowsingContext.New(config);
            IDocument document = await context.OpenAsync(req => req.Content(html));
            var cells = document.GetElementsByClassName("file-download-link ");
            var titles = cells.Select(m => m.TextContent);
            var documentInfos = new List<DocumentInfo>();
            foreach (var item in cells)
            {
                var documentInfo = new DocumentInfo();
                documentInfo.Name = item.Text().Trim();
                documentInfo.DocRef = "https://tender.rusal.ru" + item.GetAttribute("href");
                documentInfos.Add(documentInfo);
            }
            return documentInfos;
        }
    }
}