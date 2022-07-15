using Microsoft.EntityFrameworkCore;
using ParserRusal.Data.Entities;

namespace ParserRusal.Data
{
    /// <summary>
    /// Класс для объектного представления БД
    /// </summary>
    public class DataContext : DbContext
    {
        public DbSet<Item> Items { get; set; }
        public DbSet<DocumentInfo> DocumentInfos { get; set; }

        public DataContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(
                "server=localhost;user=root;password=root1234;database=parserrusalTest;", 
                new MySqlServerVersion(new Version(8, 0, 11))
                );
        }
    }
}
