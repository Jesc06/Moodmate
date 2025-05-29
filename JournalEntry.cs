using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escarez_Finals_Mental_Health_Tracker
{
    public class JournalEntry
    {

        public int Id { get; set; }
        public string Date { get; set; }
        public string Content { get; set; }

        public JournalEntry(int id, string date, string content)
        {
            Id = id;
            Date = date;
            Content = content;
        }

    }
}
