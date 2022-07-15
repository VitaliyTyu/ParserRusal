using AngleSharp;
using AngleSharp.Dom;
using Newtonsoft.Json.Linq;
using ParserRusal.Data;
using ParserRusal.Data.Entities;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace ParserRusal
{
    public class Parser
    {
        public async Task StartParsing()
        {
            Console.WriteLine("\nОжидайте\n");
            using (var httpClient = new HttpClient())
            using (DataContext db = new DataContext())
            {
                await db.Database.EnsureDeletedAsync();
                await db.Database.EnsureCreatedAsync();

                int totalCount = await GetTotalCountAsync();

                for (int i = 0; i < totalCount / 25 + 1; i++)
                {

                    var values = new Dictionary<string, string>
                    {
                        { "limit", "25" },
                        { "offset", $"{i * 25}" },
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
                        await db.Items.AddAsync(item);
                    }

                    await db.SaveChangesAsync();
                    Printer.WriteGreen($"Количество элементов: {db.Items.Count()}");
                }
            }
        }

        public async Task<string> PostAsync(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Headers.Add("x-csrf-token", "Fetch");
            request.Headers.Add("x-requested-with", "XMLHttpRequest");
            request.Headers.Add("X-Content-Requested-For", "Tab");
            request.Method = "POST";

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public async Task<int> GetTotalCountAsync()
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

        public string GetStartApplicationDate(string html)
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

        public async Task<List<DocumentInfo>> GetDocuments(string html)
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
