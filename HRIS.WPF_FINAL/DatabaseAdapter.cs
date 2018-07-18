using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using HRIS.Teaching;
using System.Windows; //For generating a MessageBox upon encountering an error

namespace HRIS.Database
{
    abstract class DatabaseAdapter
    {

        //Connect to database
        private const string db = "kit206";
        private const string user = "kit206";
        private const string pass = "kit206";
        private const string server = "alacritas.cis.utas.edu.au";
        private static MySqlConnection conn = null;
        private static bool reportingErrors;

        //James supplied this because .NET's approach to converting strings to enums is 'so horribly broken'.
        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value);
        }

        /// <summary>
        /// Creates and returns (but does not open) the connection to the database.
        /// </summary>
        private static MySqlConnection GetConnection()
        {
            if (conn == null)
            {
                //Note: This approach is not thread-safe
                string connectionString = String.Format("Database={0};Data Source={1};User Id={2};Password={3}", db, server, user, pass);
                conn = new MySqlConnection(connectionString);
            }
            return conn;
        }

        //Loads staff (id, given name, family name, title and category)
        public static List<Staff> LoadAll()
        {
            List<Staff> staff = new List<Staff>();
            MySqlConnection conn = GetConnection();
            MySqlDataReader rdr = null;
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select id, given_name, family_name, title, category from staff order by family_name", conn);
                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    staff.Add(new Staff { ID = rdr.GetInt32(0), GivenName = rdr.GetString(1), FamilyName = rdr.GetString(2), Title = rdr.GetString(3), category = ParseEnum<Category>(rdr.GetString(4)) });
                }
            }
            catch (MySqlException e)
            {
                ReportError("loading staff", e);
            }
            finally
            {
                if (rdr != null)
                {
                    rdr.Close();
                }
                if (conn != null)
                {
                    conn.Close();
                }
            }
            return staff;
        }

        //TO LOAD STAFF DETAILS (id, campus, phone, room, email, photo url)
        public static void LoadStaffDetails(Staff e)
        {
            MySqlConnection conn = GetConnection();
            MySqlDataReader rdr = null;
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select campus, phone, room, email, photo from staff where id=?id", conn);
                cmd.Parameters.AddWithValue("id", e.ID);
                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    e.campus = ParseEnum<Campus>(rdr.GetString(0));
                    e.Phone = rdr.GetString(1);
                    e.Room = rdr.GetString(2);
                    e.Email = rdr.GetString(3);
                    e.Photo = rdr.GetString(4);
                }
            }
            catch (MySqlException ex)
            {
                ReportError("loading staff details", ex);
            }
            finally
            {
                if (rdr != null)
                {
                    rdr.Close();
                }
                if (conn != null)
                {
                    conn.Close();
                }
            }
        }

        //SELECTS UNITS THAT STAFF INVOLVED WITH (teaching AND coordinating)
        public static List<Unit> LoadStaffUnits(int id)
        {
            List<Unit> StaffUnits = new List<Unit>();
            MySqlConnection conn = GetConnection();
            MySqlDataReader rdr = null;
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("(select distinct class.unit_code, unit.title from class inner join unit on class.unit_code=unit.code where staff=?id) union (select distinct code, title from unit where coordinator=?id)", conn);
                cmd.Parameters.AddWithValue("id", id);
                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    StaffUnits.Add(new Unit
                    {

                        Code = rdr.GetString(0),
                        UnitTitle = rdr.GetString(1)
                    });
                }
            }
            catch (MySqlException e)
            {
                ReportError("loading class times", e);
            }
            finally
            {
                if (rdr != null)
                {
                    rdr.Close();
                }
                if (conn != null)
                {
                    conn.Close();
                }
            }
            return StaffUnits;
        }


        //Loads staff consultation times
        public static List<Event> LoadEvents(int id)
        {
            List<Event> work = new List<Event>();
            MySqlConnection conn = GetConnection();
            MySqlDataReader rdr = null;
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select day, start, end from consultation where staff_id=?id", conn);
                cmd.Parameters.AddWithValue("id", id);
                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    work.Add(new Event
                    {
                        Day = ParseEnum<DayOfWeek>(rdr.GetString(0)),
                        Start = rdr.GetTimeSpan(1),
                        End = rdr.GetTimeSpan(2)
                    });
                }
            }
            catch (MySqlException e)
            {
                ReportError("loading consultation times", e);
            }
            finally
            {
                if (rdr != null)
                {
                    rdr.Close();
                }
                if (conn != null)
                {
                    conn.Close();
                }
            }
            return work;
        }

        //Loads unit classes that staff teach (for use in 'Current availability' status and also eventually in activity grid)
        public static List<UnitClass> LoadStaffClasses(int id)
        {
            List<UnitClass> classes = new List<UnitClass>();
            MySqlConnection conn = GetConnection();
            MySqlDataReader rdr = null;
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select day, start, end from class where staff=?id order by day, start", conn);
                cmd.Parameters.AddWithValue("id", id);
                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    classes.Add(new UnitClass
                    {
                        Day = ParseEnum<DayOfWeek>(rdr.GetString(0)),
                        Start = rdr.GetTimeSpan(1),
                        End = rdr.GetTimeSpan(2),

                    });
                }
            }
            catch (MySqlException e)
            {
                ReportError("loading class times", e);
            }
            finally
            {
                if (rdr != null)
                {
                    rdr.Close();
                }
                if (conn != null)
                {
                    conn.Close();
                }
            }
            return classes;
        }



        //For getting consultation times that overlap with 'now'
        //This is useful if the necessary data has not been loaded into memory yet.
        public static bool EmployeeConsultingNow(Staff e)
        {
            MySqlConnection conn = GetConnection();
            int overlapping = 0;
            try
            {
                conn.Open();
                DateTime now = DateTime.Now;
                MySqlCommand cmd = new MySqlCommand("select count(*) from consultation where staff_id=?id and day=?day and start <= ?now and end > ?now", conn);
                cmd.Parameters.AddWithValue("id", e.ID);
                cmd.Parameters.AddWithValue("day", now.DayOfWeek.ToString());
                cmd.Parameters.AddWithValue("now", now.TimeOfDay.ToString());
                overlapping = Int32.Parse(cmd.ExecuteScalar().ToString());
            }
            catch (MySqlException ex)
            {
                ReportError("Error connecting to database", ex);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
            return overlapping > 0;
        }

        //Database query for 'teaching now' status
        //Modified from the above code

        public static bool EmployeeTeachingNow(Staff e)
        {
            MySqlConnection conn = GetConnection();
            int overlapping = 0;
            try
            {
                conn.Open();
                DateTime now = DateTime.Now;
                MySqlCommand cmd = new MySqlCommand("select count(*) from class where staff=?id and day=?day and start <= ?now and end > ?now", conn);
                cmd.Parameters.AddWithValue("id", e.ID);
                cmd.Parameters.AddWithValue("day", now.DayOfWeek.ToString());
                cmd.Parameters.AddWithValue("now", now.TimeOfDay.ToString());
                overlapping = Int32.Parse(cmd.ExecuteScalar().ToString());
            }
            catch (MySqlException ex)
            {
                ReportError("Error connecting to database", ex);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
            return overlapping > 0;
        }

        
        //For loading teaching room and unit (for'teaching now' status)
       
        public static void LoadCurrentTeachingDetails(Staff e)
        {
            MySqlConnection conn = GetConnection();
            MySqlDataReader rdr = null;
            try
            {
                conn.Open();
                DateTime now = DateTime.Now;
                MySqlCommand cmd = new MySqlCommand("select unit_code, room from class where staff=?id and day=?day and start <= ?now and end > ?now", conn);
                cmd.Parameters.AddWithValue("id", e.ID);
                cmd.Parameters.AddWithValue("day", now.DayOfWeek.ToString());
                cmd.Parameters.AddWithValue("now", now.TimeOfDay.ToString());
                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    e.CurrentTeachingDetails = "Unit: " + rdr.GetString(0) + ", Room: " + rdr.GetString(1);

                }
            }
            catch (MySqlException ex)
            {
                ReportError("loading staff details", ex);
            }
            finally
            {
                if (rdr != null)
                {
                    rdr.Close();
                }
                if (conn != null)
                {
                    conn.Close();
                }
            }
        }


        //CODE BELOW IS FOR UNIT TAB

        //Loads unit list
        public static List<Unit> LoadUnits()
        {
            List<Unit> units = new List<Unit>();
            MySqlConnection conn = GetConnection();
            MySqlDataReader rdr = null;
            try
            {
                conn.Open();

                MySqlCommand cmd = new MySqlCommand("select code, title, coordinator from unit order by code", conn);
                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    units.Add(new Unit { Code = rdr.GetString(0), UnitTitle = rdr.GetString(1), CoordinatorID = rdr.GetInt32(2) });
                }
            }
            catch (MySqlException e)
            {
                ReportError("Loading units", e);
            }
            finally
            {
                if (rdr != null)
                {
                    rdr.Close();
                }
                if (conn != null)
                {
                    conn.Close();
                }
            }
            return units;
        }

        //Loads unit coordinator's name for a unit
        public static void LoadUnitCoordinator(Unit e)
        {
            MySqlConnection conn = GetConnection();
            MySqlDataReader rdr = null;
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select unit.coordinator, staff.given_name, staff.family_name from unit inner join staff on unit.coordinator=staff.id where code=?code", conn);
                cmd.Parameters.AddWithValue("code", e.Code);

                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    e.UnitCoordinator = rdr.GetString(1) + " " + rdr.GetString(2);
                }
            }
            catch (MySqlException ex)
            {
                ReportError("loading staff details", ex);
            }
            finally
            {
                if (rdr != null)
                {
                    rdr.Close();
                }
                if (conn != null)
                {
                    conn.Close();
                }
            }

        }



        //Loads unit classes for a unit 
        public static List<UnitClass> loadUnitClasses(string code)
        {
            List<UnitClass> unitClasses = new List<UnitClass>();
            MySqlConnection conn = GetConnection();
            MySqlDataReader rdr = null;

            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select class.campus, class.day, class.start, class.end, class.type, class.room, class.staff, staff.family_name, staff.given_name from class inner join staff on class.staff=staff.id where class.unit_code=?unit.code order by day, start", conn);
                cmd.Parameters.AddWithValue("unit.code", code);
                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    unitClasses.Add(new UnitClass
                    {
                        UnitCampus = ParseEnum<Campus>(rdr.GetString(0)),
                        Day = ParseEnum<DayOfWeek>(rdr.GetString(1)),
                        Start = rdr.GetTimeSpan(2),
                        End = rdr.GetTimeSpan(3),
                        classType = ParseEnum<ClassType>(rdr.GetString(4)),
                        Room = rdr.GetString(5),
                        TaughtBy = rdr.GetInt32(6),
                        Teacher = rdr.GetString(8) + " " + rdr.GetString(7)

                    });
                }
            }
            catch (MySqlException e)
            {
                ReportError("loading class times", e);
            }
            finally
            {
                if (rdr != null)
                {
                    rdr.Close();
                }
                if (conn != null)
                {
                    conn.Close();
                }
            }

            return unitClasses;
        }

        //HEAT MAPS QUERIES BELOW (heat maps are for future version)
        //For unit classes heat maps (heat maps not implemented in this version)
        
            /*public static List<UnitClass> LoadHeatMapClasses()
        {
            List<UnitClass> HeatMapClasses = new List<UnitClass>();
            MySqlConnection conn = GetConnection();
            MySqlDataReader rdr = null;
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select day, start, end, campus from class", conn);
                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    HeatMapClasses.Add(new UnitClass { Day = ParseEnum<DayOfWeek>(rdr.GetString(0)), Start = rdr.GetTimeSpan(1), End = rdr.GetTimeSpan(2), UnitCampus = ParseEnum<Campus>(rdr.GetString(3)) });
                }
            }
            catch (MySqlException e)
            {
                ReportError("loading heat map unit classes", e);
            }
            finally
            {
                if (rdr != null)
                {
                    rdr.Close();
                }
                if (conn != null)
                {
                    conn.Close();
                }
            }
            return HeatMapClasses;
        }
        */


        //For consultation heatmaps (not implemented in this version)
        //Add future code here 


        //CODE FOR REPORTING ERROR
        private static void ReportError(string msg, Exception e)
        {
            if (reportingErrors)
            {
                MessageBox.Show("An error occurred while " + msg + ". Try again later.\n\nError Details:\n" + e,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}


