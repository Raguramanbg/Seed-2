using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace HRIS.Teaching
{
    public class Unit
    {
        public string Code { get; set; }
        public string UnitTitle { get; set; }
        public int CoordinatorID { get; set; }
        public string UnitCoordinator { get; set; }
        public List<UnitClass> UnitClassTimes { get; set; }

        public override string ToString()
        {
            return String.Format("{0} {1}", Code, UnitTitle);
        }

    }
}
