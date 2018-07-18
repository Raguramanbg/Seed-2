using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRIS.Teaching
{
    public class Event
    {
        
        public DayOfWeek Day { get; set; }
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }

        public bool Overlaps(DateTime sometime)

        {
            //TimeSpan objects overload the comparison operators, so treated as if they were plain numbers in the checks below
            //A longer version of the second test would be sometime.TimeOfDay.CompareTo(Start) >= 0 
            return sometime.DayOfWeek == Day &&
                sometime.TimeOfDay >= Start &&
                sometime.TimeOfDay < End;
        }

        public override string ToString()
        {
            //return Day + " " + Start + "--" + End;
            return string.Format("{0} {1:hh':'mm}-{2:hh':'mm}", Day, Start, End);

        }

    }
}
