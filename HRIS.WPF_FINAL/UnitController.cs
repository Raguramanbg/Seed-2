using HRIS.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HRIS.Teaching;

namespace HRIS.Control
{
    class UnitController
    {

        private List<Unit> units;
        public List<UnitClass> UnitClasses;
        private ObservableCollection<Unit> viewableUnits;
        public ObservableCollection<Unit> VisibleUnits { get { return viewableUnits; } set { } }

        public UnitController()
        {
            units = DatabaseAdapter.LoadUnits();
            viewableUnits = new ObservableCollection<Unit>(units);
        }

        public ObservableCollection<Unit> GetViewableList()
        {
            return VisibleUnits;
        }

        public List<UnitClass> GetTimetable(Unit e)
        {
            e.UnitClassTimes = DatabaseAdapter.loadUnitClasses(e.Code);
            UnitClasses = e.UnitClassTimes;
            return UnitClasses;
        }

        public void UnitSearch(String name)
        {
            var selected = from Unit e in units
                           where e.Code.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0 || e.UnitTitle.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0
                           select e;
            viewableUnits.Clear();
            selected.ToList().ForEach(viewableUnits.Add);
        }

        //For getting unit coordinator
        public void GetUnitCoord(Unit e)
        {
            DatabaseAdapter.LoadUnitCoordinator(e);
        }

        public int GetCoordByCode(string code)
        {
            var selected = from Unit e in units
                           where e.Code == code
                           select e;
            int CoorID = selected.ToList().First().CoordinatorID;
            return CoorID;
        }

        public List<UnitClass> Filter(Campus camp, List<UnitClass> Unitclasses)
        {
            var selectedClasses = from UnitClass e in Unitclasses
                                  where e.UnitCampus == camp || camp == Campus.All
                                  select e;
            List<UnitClass> unitClasses = new List<UnitClass>();
            selectedClasses.ToList().ForEach(unitClasses.Add);
            return unitClasses;
        }
    }
}
