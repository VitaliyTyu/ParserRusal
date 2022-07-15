using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserRusal
{
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
        public List<DocumentInfo> DocumentInfos { get; set; } = new List<DocumentInfo>();

        [JsonIgnore]
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
