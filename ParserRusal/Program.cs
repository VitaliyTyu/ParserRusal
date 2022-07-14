using AngleSharp;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ParserRusal
{
    class Program
    {
        public static async Task<string> GetAsync(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        static async Task Main(string[] args)
        {
            HttpClient client = new HttpClient();

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

                var response = await client.PostAsync("https://tender.rusal.ru/Tenders/Load", content);

                var responseString = await response.Content.ReadAsStringAsync();
                var items = JObject.Parse(responseString)["Rows"].ToObject<Item[]>();

                foreach (var item in items)
                {
                    Console.WriteLine(item);
                }

                var res2 = await GetAsync("https://tender.rusal.ru/Tender/T-0004608/1");



                Regex rg = new Regex(@"RequestReceivingBeginDate..</div>");
                var date = rg.Matches(res2);
                foreach (var item in date)
                {
                    Console.WriteLine(item);
                }

                var config = Configuration.Default.WithDefaultLoader();
                var context = BrowsingContext.New(config);
                var document = await context.OpenAsync("https://tender.rusal.ru/Tender/T-0004608/1");
                //var cells = document.GetElementsByTagName("div").Where(div => div.ClassName == "col-md-9").ToList();
                var cells = document.GetElementsByTagName("div").Where(div => div.ClassName != null && div.ClassName.Contains("col-md-9")).ToList();

                var titles = cells.Select(m => m.TextContent);



                Console.ReadLine();
            }
        }
    }

    public class Item
    {
        //public string TenderUid { get; set; }
        //public string TenderNumber { get; set; }
        //public string TenderLotUid { get; set; }
        //public string TenderTitle { get; set; }
        //public string TenderConfigCode { get; set; }
        //public string TenderConfigCodeName { get; set; }
        //public int TenderLotNumber { get; set; }
        //public int TenderLotNoticeNumber { get; set; }
        //public int TenderLotVersion { get; set; }
        //public int TenderVersion { get; set; }
        //public int SubTenderVersion { get; set; }
        //public string EntityTypeCode { get; set; }
        //public string TenderLotTitle { get; set; }
        //public string TenderStatusCode { get; set; }
        //public string LotStatusCode { get; set; }
        //public string LotConfigCode { get; set; }
        //public string LotStatusName { get; set; }
        //public string CustomerName { get; set; }
        //public string OrganizerUid { get; set; }
        //public string CustomerUid { get; set; }
        //public string PriceCurrencyCode { get; set; }
        //public string PublishedDate { get; set; }
        //public string RequestReceivingBeginDate { get; set; }
        //public string RequestReceivingEndDate { get; set; }
        //public string EntityViewUrl { get; set; }
        //public string TenderLotViewUrl { get; set; }
        //public string OrganizerViewUrl { get; set; }
        //public string CustomerViewUrl { get; set; }
        //public string OperationName { get; set; }
        //public string OperationAction { get; set; }

        public Item()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public string EntityNumber { get; set; }
        public string OrganizerName { get; set; }
        public string TenderViewUrl { get; set; }


        public override string ToString()
        {
            return $"{EntityNumber}\n{OrganizerName}\n{TenderViewUrl}\n";
        }
    }
}