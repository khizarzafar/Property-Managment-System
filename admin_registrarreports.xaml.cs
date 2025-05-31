using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using MySql.Data.MySqlClient; // Add this for MySQL connectivity

namespace SocietyManagmentSystem
{
    /// <summary>
    /// Interaction logic for admin_registrarreports.xaml
    /// </summary>
    public partial class admin_registrarreports : Window
    {
        // Replace with your actual connection string
        private string connectionString = "Server=localhost;Database=zephyr;Uid=root;Pwd=root;";

        public ObservableCollection<Resident> Residents { get; set; }

        public admin_registrarreports()
        {
            InitializeComponent();
            Residents = new ObservableCollection<Resident>();
            ResidentDataGrid.ItemsSource = Residents;
            LoadResidentData();
        }

        private void LoadResidentData()
        {
            try
            {
                Residents.Clear();

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT resident_id, name, email, phone, house_address, status, move_in_date 
                                   FROM residents 
                                   ORDER BY resident_id";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Resident resident = new Resident
                                {
                                    ResidentId = reader.GetInt32("resident_id"),
                                    Name = reader.IsDBNull("name") ? "" : reader.GetString("name"),
                                    Email = reader.IsDBNull("email") ? "" : reader.GetString("email"),
                                    Phone = reader.IsDBNull("phone") ? "" : reader.GetString("phone"),
                                    HouseAddress = reader.IsDBNull("house_address") ? "" : reader.GetString("house_address"),
                                    Status = reader.IsDBNull("status") ? "" : reader.GetString("status"),
                                    MoveInDate = reader.IsDBNull("move_in_date") ? (DateTime?)null : reader.GetDateTime("move_in_date")
                                };

                                Residents.Add(resident);
                            }
                        }
                    }
                }

                // Update total count
                TotalCountTextBlock.Text = Residents.Count.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading resident data: {ex.Message}", "Database Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadResidentData();
            MessageBox.Show("Data refreshed successfully!", "Success",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Dashboard button - navigate to dashboard
            var dashboard = new admin(); // Assuming you have an admin dashboard window
            dashboard.Show();
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // Resident Reports button - we're already here, so just refresh
            LoadResidentData();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var finance = new admin_financereports();
            finance.Show();
            this.Close();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWin = new login();
            loginWin.Show();
            this.Close();
        }
    }

    // Resident model class
    public class Resident
    {
        public int ResidentId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string HouseAddress { get; set; }
        public string Status { get; set; }
        public DateTime? MoveInDate { get; set; }
    }
}