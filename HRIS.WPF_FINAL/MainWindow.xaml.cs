using HRIS.Control;
using HRIS.Teaching;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace HRIS.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string STAFF_LIST_KEY = "staffList";
        private StaffController boss;
        static List<UnitClass> UnitClassTimes;
        static bool flag = false;

        //Initialize the main window
        public MainWindow()
        {
            InitializeComponent();
            StaffRightFrame.Visibility = Visibility.Hidden;
            UnitRightFrame.Visibility = Visibility.Hidden;
            CategorySorter.SelectedIndex = 0;
            boss = (StaffController)(Application.Current.FindResource(STAFF_LIST_KEY) as ObjectDataProvider).ObjectInstance;
        }

        //Search staff list by staff name
        private void BtnStaffSearch_Click(object sender, RoutedEventArgs e)
        {
            String name = (String)StaffSearch.Text;
            Category cat = (Category)CategorySorter.SelectedItem;
            StaffController boss = (StaffController)Application.Current.FindResource("boss");
            boss.StaffSearch(name, cat);
        }

        //Search unit list by unit code or name
        private void BtnUnitSearch_Click(object sender, RoutedEventArgs e)
        {
            String name = (String)UnitSearch.Text;
            UnitController UnitBoss = (UnitController)Application.Current.FindResource("UnitBoss");
            UnitBoss.UnitSearch(name);
        }

        //Select one staff name from staff list to show staff detail
        private void StaffListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StaffWelcome.Visibility = Visibility.Hidden;
            if (e.AddedItems.Count > 0)
            {
                Staff selectedStaff = (Staff)e.AddedItems[0];
                StaffController boss = (StaffController)Application.Current.FindResource("boss");
                boss.GetStaffDetail(selectedStaff);
                StaffDetailsPanel.DataContext = selectedStaff;
                if (selectedStaff.category == Category.Academic || selectedStaff.category == Category.Casual)
                {
                    AcademicOnly.Visibility = Visibility.Visible;
                }
                else
                {
                    AcademicOnly.Visibility = Visibility.Hidden;
                }
            }
            StaffRightFrame.Visibility = Visibility.Visible;
        }

        //Select one unit from unit list to show unit detail
        private void UnitListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UnitWelcome.Visibility = Visibility.Hidden;
            CampusSorter.SelectedIndex = 0;
            if (e.AddedItems.Count > 0)
            {
                UnitClassGrid.Items.Clear();
                Unit selectedUnit = (Unit)e.AddedItems[0];
                UnitController UnitBoss = (UnitController)Application.Current.FindResource("UnitBoss");
                UnitClassTimes = UnitBoss.GetTimetable(selectedUnit);
                for (int i = 0; i < UnitClassTimes.Count(); i++)
                {
                    UnitClassGrid.Items.Add(UnitClassTimes[i]);
                }
                UnitBoss.GetUnitCoord(selectedUnit);
                UnitDetailsPanel.DataContext = selectedUnit;
            }
            UnitRightFrame.Visibility = Visibility.Visible;
        }

        //Select one unit from 'units involved with' list in staff detail to show unit detail
        private void UnitInvolvedList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UnitWelcome.Visibility = Visibility.Hidden;
            CampusSorter.SelectedIndex = 0;
            UnitClassGrid.Items.Clear();
            if (e.AddedItems.Count > 0)
            {
                Unit selectedUnit = (Unit)e.AddedItems[0];
                UnitController UnitBoss = (UnitController)Application.Current.FindResource("UnitBoss");
                UnitClassTimes = UnitBoss.GetTimetable(selectedUnit);
                for (int i = 0; i < UnitClassTimes.Count(); i++)
                {
                    UnitClassGrid.Items.Add(UnitClassTimes[i]);
                }
                UnitBoss.GetUnitCoord(selectedUnit);
                selectedUnit.CoordinatorID = UnitBoss.GetCoordByCode(selectedUnit.Code);
                UnitDetailsPanel.DataContext = selectedUnit;
                tab.SelectedItem = tab.Items[1];
           }
            UnitRightFrame.Visibility = Visibility.Visible;
        }

        //Click unit coordinator from unit detail to show staff detail
        private void UnitCoordinator_Click(object sender, RoutedEventArgs e)
        {
            StaffWelcome.Visibility = Visibility.Hidden;
            int staffID = Convert.ToInt32(CoorID.Content.ToString());
            StaffController boss = (StaffController)Application.Current.FindResource("boss");
            Staff selectedStaff = boss.GetStaffDetailByID(staffID);
            boss.GetStaffDetail(selectedStaff);
            StaffDetailsPanel.DataContext = selectedStaff;
            tab.SelectedItem = tab.Items[0];
            StaffRightFrame.Visibility = Visibility.Visible;
        }

        //Filter staff list by category
        private void CategorySorter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Category cat = (Category)e.AddedItems[0];
            StaffController boss = (StaffController)Application.Current.FindResource("boss");
            boss.Filter(cat);
        }

        //Filter unit timetable by campus
        private void CampusSorter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UnitClassTimes != null)
            {
                UnitController UnitBoss = (UnitController)Application.Current.FindResource("UnitBoss");
                if (UnitClassTimes.Count != 0)
                {
                    UnitClassGrid.Items.Clear();
                    Campus cam = (Campus)e.AddedItems[0];
                    List<UnitClass> SortedUnitClassTimes = UnitBoss.Filter(cam, UnitClassTimes);
                    foreach (UnitClass Classes in SortedUnitClassTimes)
                    {
                        UnitClassGrid.Items.Add(Classes);
                    }
                }
            }
        }

        //Click staff name on unit timetable to show staff detail
        private void UnitClassGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StaffWelcome.Visibility = Visibility.Hidden;
            if (e.AddedItems.Count > 0)
            {
                UnitClass uniClass = (UnitClass)UnitClassGrid.SelectedValue;
                int staffID = Convert.ToInt32(uniClass.TaughtBy);
                StaffController boss = (StaffController)Application.Current.FindResource("boss");
                Staff selectedStaff = boss.GetStaffDetailByID(staffID);
                boss.GetStaffDetail(selectedStaff);
                StaffDetailsPanel.DataContext = selectedStaff;
                tab.SelectedItem = tab.Items[0];
                StaffRightFrame.Visibility = Visibility.Visible;
                AcademicOnly.Visibility = Visibility.Visible;
            }
        }

        private void Activity_Click(object sender, RoutedEventArgs e)
        {


            if (flag)
            {
                ActivityUserControl.Visibility = Visibility.Hidden;
                flag = false;
            }
            else {
                ActivityUserControl.Visibility = Visibility.Visible;
                flag = true;
            }
        }


        private void StaffSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            StaffSearch.Text = "";
        }

        private void UnitSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            UnitSearch.Text = "";
        }

        
    }
}
