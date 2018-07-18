using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRIS.Teaching
{
    public enum ClassType { Lecture, Tutorial, Practical, Workshop };

    public class UnitClass : Event
    {
        public string Room { get; set; }
        public Campus UnitCampus { get; set; }
        public int TaughtBy { get; set; }
        public string Teacher { get; set; }
        public ClassType classType { get; set; }
        public string ClassCode {get; set; }
     }
}
