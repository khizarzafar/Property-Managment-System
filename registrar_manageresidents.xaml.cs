using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient; // Ensure you have this using statement and the MySql.Data NuGet package
using System.Data; // Required for CommandType

namespace SocietyManagmentSystem
{
    public partial class registrar_manageresidents : Window
    {
        private string connectionString = "Server=localhost;Database=zephyr;Uid=root;Pwd=root;"; // Your MySQL connection string

        public registrar_manageresidents()
        {
            InitializeComponent();
            LoadResidentsData();
        }

        private void LoadResidentsData()
        {
            List<ResidentModel> residents = new List<ResidentModel>();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT resident_id, name, email, phone, house_address, status, move_in_date FROM residents ORDER BY name";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                residents.Add(new ResidentModel
                                {
                                    resident_id = reader.GetInt32("resident_id"),
                                    name = reader.GetString("name"),
                                    email = reader.IsDBNull(reader.GetOrdinal("email")) ? null : reader.GetString("email"),
                                    phone = reader.IsDBNull(reader.GetOrdinal("phone")) ? null : reader.GetString("phone"),
                                    house_address = reader.GetString("house_address"),
                                    status = reader.GetString("status"),
                                    move_in_date = reader.GetDateTime("move_in_date")
                                });
                            }
                        }
                    }
                }
                ResidentsDataGrid.ItemsSource = residents;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading residents data: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadResidentsData();
        }

        private void AddNewResident_Click(object sender, RoutedEventArgs e)
        {
            // Assuming AddResidentWindow is designed to handle both Add (no parameter)
            // and Edit (with a ResidentModel parameter or resident_id)
            AddResidentWindow addResidentWin = new AddResidentWindow(); // Pass null or no arg for new resident
            addResidentWin.Owner = this;
            bool? result = addResidentWin.ShowDialog();

            if (result == true) // If AddResidentWindow sets DialogResult to true on successful save
            {
                LoadResidentsData(); // Refresh the grid
            }
        }

        private void EditResident_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null && button.DataContext is ResidentModel selectedResident)
            {
                // You'll need to modify AddResidentWindow to accept a ResidentModel or resident_id
                // and populate its fields for editing, then perform an UPDATE operation.
                AddResidentWindow editResidentWin = new AddResidentWindow(selectedResident); // Example: Pass the selected resident
                editResidentWin.Owner = this;
                bool? result = editResidentWin.ShowDialog();

                if (result == true)
                {
                    LoadResidentsData(); // Refresh the grid
                }
            }
            else if (button != null && button.Tag is int residentId) // Fallback if DataContext is not directly ResidentModel but Tag has ID
            {
                // Fetch resident details by ID first if needed, or pass ID to edit window
                // This scenario is less direct than using DataContext
                MessageBox.Show($"Edit for Resident ID: {residentId}. Implement fetching and passing data to Edit Window.", "Information");
                // Example:
                // ResidentModel residentToEdit = GetResidentById(residentId);
                // if (residentToEdit != null) {
                //    AddResidentWindow editResidentWin = new AddResidentWindow(residentToEdit);
                //    ...
                // }
            }
        }


        private void DeleteResident_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int residentIdToDelete = -1;

            if (button?.DataContext is ResidentModel selectedResident) // Preferred way
            {
                residentIdToDelete = selectedResident.resident_id;
            }
            else if (button?.Tag is int idFromTag) // Fallback using Tag
            {
                residentIdToDelete = idFromTag;
            }

            if (residentIdToDelete == -1)
            {
                MessageBox.Show("Could not determine resident to delete.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBoxResult messageBoxResult = MessageBox.Show($"Are you sure you want to delete resident ID: {residentIdToDelete}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        // It's good practice to check for related records (e.g., in 'rent' table) before deleting
                        // or use ON DELETE SET NULL / ON DELETE CASCADE in your database schema if appropriate.
                        // For now, direct delete:
                        string query = "DELETE FROM residents WHERE resident_id = @ResidentId";
                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@ResidentId", residentIdToDelete);
                            int result = command.ExecuteNonQuery();
                            if (result > 0)
                            {
                                MessageBox.Show("Resident deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                LoadResidentsData(); // Refresh
                            }
                            else
                            {
                                MessageBox.Show("Resident not found or could not be deleted.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    // Check for foreign key constraint violations if a resident cannot be deleted due to related records
                    if (ex.Number == 1451) // MySQL error code for foreign key constraint failure
                    {
                        MessageBox.Show($"Cannot delete resident. They have related records (e.g., rent payments). Please resolve these first.\nDetails: {ex.Message}", "Deletion Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBox.Show($"Database error while deleting resident: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting resident: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        // --- Navigation Button Handlers ---
        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            // Create and show the registrar window
            SocietyManagmentSystem.registrar registrarWindow = new SocietyManagmentSystem.registrar();
            registrarWindow.Show();

            // Close the current window
            Window currentWindow = Window.GetWindow(this);
            if (currentWindow != null)
                currentWindow.Close();
        }

        private void ManageResidentsButton_Click(object sender, RoutedEventArgs e)
        {
            // Already on this page, optionally refresh
            LoadResidentsData();
            MessageBox.Show("Residents data refreshed.", "Manage Residents", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void HandleComplaintsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create and show the target window
                registrar_handlecomplaints complaintsWindow = new registrar_handlecomplaints();
                complaintsWindow.Show();

                // Close the current window (optional)
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Handle Complaints window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new login(); // Assuming your login window class is named 'login'
            loginWindow.Show();
            this.Close();
        }
    }

    // Data model for Residents
    public class ResidentModel
    {
        public int resident_id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string house_address { get; set; }
        public string status { get; set; }
        public DateTime move_in_date { get; set; }
    }
}