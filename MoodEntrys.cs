using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escarez_Finals_Mental_Health_Tracker
{
    public class MoodEntrys
    {
        // para sa emotional trends na code sa home dashboard
        public DateTime? Date { get; set; }
        public string Emotion { get; set; }
        public string Notes { get; set; }
    }
}
