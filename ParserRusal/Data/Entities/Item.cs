using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ParserRusal.Data.Entities
{
    public class Item
    {
        public Guid Id { get; set; }

        private string _entityNumber;
        public string EntityNumber
        {
            get { return _entityNumber; }
            set { _entityNumber = value.Substring(0, value.Length - 2); }
        }

        public string OrganizerName { get; set; }

        private string _tenderViewUrl;
        public string TenderViewUrl
        {
            get { return _tenderViewUrl; }
            set { _tenderViewUrl = "https://tender.rusal.ru" + value; }
        }

        public List<DocumentInfo> DocumentInfos { get; set; } = new List<DocumentInfo>();

        public string StartApplicationDate { get; set; }

        public override string ToString()
        {
            string documentInfos = "";
            if (DocumentInfos.Count == 0)
            {
                documentInfos = "Отсутствуют";
            }
            else
            {
                foreach (var documentInfo in DocumentInfos)
                {
                    documentInfos += documentInfo.ToString();
                }
            }

            return $"Номер лота: {EntityNumber}\n" +
                $"Организатор: {OrganizerName}\n" +
                $"Страница процедуры: {TenderViewUrl}\n" +
                $"Дата начала подачи заявок: {StartApplicationDate}\n" +
                $"Документы: {documentInfos}\n";
        }
    }
}
