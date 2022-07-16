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
            var parser = new Parser();

            while (true)
            {
                Console.WriteLine("\n1. Парсить и выводить на экран\n" +
                                  "2. Спарсить данные\n" +
                                  "3. Вывод на экран\n" +
                                  "4. Выход");

                switch (Console.ReadLine())
                {
                    case "1":
                        try
                        {
                            parser.IsOutput = true;
                            await parser.StartParsing();
                            Printer.WriteGreen("\nУспешно.");
                        }
                        catch (Exception)
                        {
                            parser.AddExtraHeaders();
                        }
                        break;

                    case "2":
                        try
                        {
                            parser.IsOutput = false;
                            await parser.StartParsing();
                            Printer.WriteGreen("\nУспешно.");
                        }
                        catch (Exception)
                        {
                            parser.AddExtraHeaders();
                        }
                        break;

                    case "3":
                        Printer.PrintItems();
                        break;

                    case "4":
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