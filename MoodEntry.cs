using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escarez_Finals_Mental_Health_Tracker
{
    public class MoodEntry
    {
        public int Id { get; set; }
        public string Mood { get; set; }
        public string Date { get; set; }
        public string Content { get; set; }

        public MoodEntry(int id, string mood, string date, string content)
        {
            Id = id;
            Mood = mood;
            Date = date;
            Content = content;
        }

    }
}
