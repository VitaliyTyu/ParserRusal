using AngleSharp;
using AngleSharp.Dom;
using Newtonsoft.Json.Linq;
using ParserRusal.Data;
using ParserRusal.Data.Entities;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ParserRusal
{
    public class Parser
    {
        public bool IsOutput { get; set; } = false;
        public bool IsNeedExtraHeaders { get; set; } = false;
        public string CookieValue { get; set; } = "";
        public string UserAgentValue { get; set; } = "";

        public async Task StartParsing()
        {
            Console.WriteLine("\nОжидайте\n");

            using (DataContext db = new DataContext())
            {
                await db.Database.EnsureDeletedAsync();
                await db.Database.EnsureCreatedAsync();

                int totalCount = await GetTotalCountAsync();
                Console.WriteLine($"Общее количество элементов: {totalCount}\n");

                int num = 1;
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

                    var responseString = await PostAsync("https://tender.rusal.ru/Tenders/Load", values);
                    var items = JObject.Parse(responseString)["Rows"].ToObject<Item[]>();

                    foreach (var item in items)
                    {
                        // Получение html кода со страницы процедуры
                        var html = await PostAsync(item.TenderViewUrl, new Dictionary<string, string>());
                        item.StartApplicationDate = GetStartApplicationDate(html);
                        item.DocumentInfos = await GetDocuments(html);

                        // добавление итема и связанных с ним документов в БД
                        await db.Items.AddAsync(item);

                        if (IsOutput)
                        {
                            Printer.WriteGreen($"\nЭлемент {num}");
                            Console.WriteLine(item);
                            num++;
                        }
                    }

                    await db.SaveChangesAsync();
                    Printer.WriteGreen($"\nКоличество полученных элементов: {db.Items.Count()}");
                }
            }
        }

        /// <summary>
        /// Настройка метода POST
        /// </summary>
        public async Task<string> PostAsync(string uri, Dictionary<string, string> values)
        {
            using (var httpClient = new HttpClient())
            {
                // добавление заголовков
                httpClient.DefaultRequestHeaders.Add("x-csrf-token", "Fetch");
                httpClient.DefaultRequestHeaders.Add("x-requested-with", "XMLHttpRequest");
                httpClient.DefaultRequestHeaders.Add("X-Content-Requested-For", "Tab");

                if (IsNeedExtraHeaders)
                {
                    // опциональные, иногда без них не работает
                    httpClient.DefaultRequestHeaders.Add("cookie", CookieValue);
                    httpClient.DefaultRequestHeaders.Add("user-agent", UserAgentValue);
                }

                // отправка запроса и обработка ответа
                var content = new FormUrlEncodedContent(values);
                var response = await httpClient.PostAsync(uri, content);
                var responseString = await response.Content.ReadAsStringAsync();
                return responseString;
            }
        }

        /// <summary>
        /// Получение общего еол-ва элементов
        /// </summary>
        public async Task<int> GetTotalCountAsync()
        {
            var values = new Dictionary<string, string>
                {
                    { "limit", "10" },
                    { "offset", "0" },
                    { "sortAsc", "false" },
                    { "sortColumn", "EntityNumber" },
                    { "ClassifiersFieldData.SiteSectionType", "bef4c544-ba45-49b9-8e91-85d9483ff2f6" },
                };

            var responseString = await PostAsync("https://tender.rusal.ru/Tenders/Load", values);
            var TotalCount = JObject.Parse(responseString)["Paging"]["Total"].ToString();

            return int.Parse(TotalCount);
        }

        /// <summary>
        /// Получение даты начала подачи заявок
        /// </summary>
        public string GetStartApplicationDate(string html)
        {
            // Парсинг Html при помощи регулярок
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

        /// <summary>
        /// Получение информации о доккументах
        /// </summary>
        public async Task<List<DocumentInfo>> GetDocuments(string html)
        {
            // Парсинг Html при помощи библиотеки AngleSharp
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


        public void AddExtraHeaders()
        {
            IsNeedExtraHeaders = true;
            Console.WriteLine("Произошла ошибка при парсинге, скорее всего это из-за того, что сайту потребовались куки и заголовок user-agent. Эту проблему решить автоматически я не смог, поэтому необходимо взять эти значения из заголовка любого запроса.");
            Console.WriteLine("Введите значение куки, а именно то, что идет после cookie: ");
            CookieValue = Console.ReadLine().Trim();
            Console.WriteLine("Введите значение user-agent, а именно то, что идет после user-agent: ");
            UserAgentValue = Console.ReadLine().Trim();
        }
    }
}
