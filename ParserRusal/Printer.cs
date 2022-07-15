using Microsoft.EntityFrameworkCore;
using ParserRusal.Data;


namespace ParserRusal
{
    public class Printer
    {
        public static void PrintItems()
        {
            using (DataContext db = new DataContext())
            {
                WriteGreen($"\nКоличество элементов: {db.Items.Count()}");

                int i = 1;
                foreach (var item in db.Items.Include(i => i.DocumentInfos))
                {
                    WriteGreen($"\nЭлемент {i}");
                    Console.WriteLine(item);
                    i++;
                }
            }
        }

        public static void WriteRed(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void WriteGreen(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
