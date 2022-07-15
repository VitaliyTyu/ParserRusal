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
    class Program
    {
        static async Task Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("\n1. Спарсить данные\n" +
                                  "2. Вывод на экран\n" +
                                  "3. Выход");

                switch (Console.ReadLine())
                {
                    case "1":
                        var parser = new Parser();
                        try
                        {
                            await parser.StartParsing();
                            Printer.WriteGreen("\nУспешно.");
                        }
                        catch (Exception)
                        {
                            parser.IsNeedExtraHeaders = true;
                            Console.WriteLine("Произошла ошибка при парсинге, скорее всего это из-за того, что сайту потребовались куки и заголовок user-agent. Эту проблему решить автоматически я не смог, поэтому необходимо взять эти значения из заголовка любого запроса.");
                            Console.WriteLine("Введите значение куки, а именно то, что идет после cookie: ");
                            parser.CookieValue = Console.ReadLine().Trim();
                            Console.WriteLine("Введите значение user-agent, а именно то, что идет после user-agent: ");
                            parser.UserAgentValue = Console.ReadLine().Trim();
                        }
                        break;

                    case "2":
                        Printer.PrintItems();
                        break;

                    case "3":
                        return;
                        break;

                    default:
                        Printer.WriteRed("\nТакой опции нет.");
                        break;
                }
            }
        }
    }
}