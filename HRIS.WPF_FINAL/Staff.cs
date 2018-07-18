using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRIS.Teaching
{
    public enum Category { All, Academic, Technical, Admin, Casual };


    public class Staff
    {

        public int ID { get; set; }
        public string FamilyName { get; set; }
        public string GivenName { get; set; }
        public string Title { get; set; }
        public Campus campus { get; set; }
        public string Room { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Photo { get; set; }
        public Category category { get; set; }
        public List<Event> ConsultationTimes { get; set; }
        public List<UnitClass> ClassTimes { get; set; }
        public List<Unit> UnitsInvolvedWith { get; set; }
        public string CurrentTeachingDetails { get; set; }
      
        
        //The code below for working out 'Availability' for staff details screen.
        //The 'CurrentTeachingDetails' attribute above holds unit and room info if staff status is 'Teaching'
              
        public bool ConsultingNow()
        {
            if (ConsultationTimes != null)
            {
                DateTime now = DateTime.Now;
                var overlapping = from Event work in ConsultationTimes
                                  where work.Overlaps(now)
                                  select work;
                return overlapping.Count() > 0;

            }
            return false;

        }

        //The 'TeachingNow' code has been modified from above
       
        public bool TeachingNow()
        {
            if (ClassTimes != null)
            {
                DateTime now = DateTime.Now;
                var overlapping = from UnitClass classes in ClassTimes
                                  where classes.Overlaps(now)
                                  select classes;
                return overlapping.Count() > 0;

            }
            return false;
        }

                  
  
        public AvailabilityStatus Availability
        {
            get
            {
                if (TeachingNow() == false && ConsultingNow() == false)
                {
                    return AvailabilityStatus.Free;
                }
                else if (TeachingNow() == true && ConsultingNow() == false)
                {
                    return AvailabilityStatus.Teaching;
                }
                else if (ConsultingNow() == true && TeachingNow() == false)
                {
                    return AvailabilityStatus.Consulting;
                }
                else
                {
                    return AvailabilityStatus.Free;
                }
            ;}
        }
  
    }
    }

