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
                        await parser.StartParsing();
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