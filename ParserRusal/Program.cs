using AngleSharp;
using Newtonsoft.Json.Linq;
using System.Net;
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
                var items = JObject.Parse(responseString)["Rows"].ToObject<Item[]>();

                foreach (var item in items)
                {
                    var html = await GetAsync(item.TenderViewUrl);
                    item.StartApplicationDate = ParseStartApplicationDate(html);

                    Console.WriteLine(item);
                }


                Console.ReadLine();
            }
        }

        public static async Task<string> GetAsync(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Headers.Add("x-csrf-token", "Fetch");
            request.Headers.Add("x-requested-with", "XMLHttpRequest");

            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public static string ParseStartApplicationDate(string html)
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

    }

    public class Item
    {
        public string TenderUid { get; set; }
        public string EntityNumber { get; set; }
        public string OrganizerName { get; set; }

        private string _tenderViewUrl;
        public string TenderViewUrl
        {
            get { return _tenderViewUrl; }
            set { _tenderViewUrl = "https://tender.rusal.ru" + value; }
        }

        [JsonIgnore]
        public string StartApplicationDate { get; set; }

        [JsonIgnore]
        public string Documents { get; set; }

        public override string ToString()
        {
            return $"{EntityNumber}\n{OrganizerName}\n{TenderViewUrl}\n{StartApplicationDate}\n";
        }
    }
}