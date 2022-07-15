using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserRusal.Data.Entities
{
    public class DocumentInfo
    {
        public Guid Id { get; set; }

        public string DocRef { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return $"\n\tДокумент: {Name}\n\tСсылка: {DocRef}";
        }

        public Item Item { get; set; }
    }
}
