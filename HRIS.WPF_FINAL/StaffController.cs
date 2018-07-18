using HRIS.Teaching;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HRIS.Database;

namespace HRIS.Control
{
    class StaffController
    {
        private List<Staff> staff;
        public List<Staff> Workers { get { return staff; } set { } }
        private ObservableCollection<Staff> viewableStaff;
        public ObservableCollection<Staff> VisibleWorkers { get { return viewableStaff; } set { } }

        public StaffController()
        {
            staff = DatabaseAdapter.LoadAll();
            viewableStaff = new ObservableCollection<Staff>(staff);
        }

        public ObservableCollection<Staff> GetViewableList()
        {
            return VisibleWorkers;
        }
        
        //Sorting staff list by category (using drop-down list on main screen)
        public void Filter(Category cat)
        {
            var selected = from Staff e in staff
                           where e.category == cat || cat == Category.All
                           select e;
            viewableStaff.Clear();
            selected.ToList().ForEach(viewableStaff.Add);
        }

        //Sorting staff list by name filter text (linked to category filter)
        public void StaffSearch(String name, Category cat)
        {
            var selected = from Staff e in staff
                           where e.FamilyName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0
                           && (e.category == cat || cat == Category.All)
                           || e.GivenName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0
                           && (e.category == cat || cat == Category.All)
                           select e;
            viewableStaff.Clear();
            selected.ToList().ForEach(viewableStaff.Add);
         }

        //For calling staff details queries
        public void GetStaffDetail(Staff e)
        {
            DatabaseAdapter.LoadStaffDetails(e);
            e.ConsultationTimes = DatabaseAdapter.LoadEvents(e.ID);
            e.ClassTimes = DatabaseAdapter.LoadStaffClasses(e.ID);
            e.UnitsInvolvedWith = DatabaseAdapter.LoadStaffUnits(e.ID);
            
            //For loading teaching room and unit (if Availablility is 'teaching now')
            if (e.Availability == AvailabilityStatus.Teaching)
            {
                DatabaseAdapter.LoadCurrentTeachingDetails(e);

            }
                   
         }

        //For getting staff detail by clicking on unit coordinator or teacher in timetable view
        public Staff GetStaffDetailByID(int ID)
        {
            var selected = from Staff e in staff
                           where e.ID == ID
                           select e;
            return selected.ToList().First();
        }

     }
}
