// registrar_handlecomplaints.xaml.cs
using MySql.Data.MySqlClient; // Add this for MySQL
using System;
using System.Collections.Generic; // For List
using System.Windows;
using System.Windows.Controls; // For DataGridRowEditEndingEventArgs

namespace SocietyManagmentSystem
{
    public partial class registrar_handlecomplaints : Window
    {
        // IMPORTANT: Replace with your actual connection string
        private string connectionString = "Server=localhost;Database=zephyr;Uid=root;Pwd=root;";

        public registrar_handlecomplaints()
        {
            InitializeComponent();
            LoadComplaintsData();
        }

        private void LoadComplaintsData()
        {
            List<Complaint> complaintsList = new List<Complaint>();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT complaint_id, resident_id, subject, description, status, admin_notes FROM complaints ORDER BY complaint_id DESC";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        complaintsList.Add(new Complaint
                        {
                            complaint_id = reader.GetInt32("complaint_id"),
                            resident_id = reader.GetInt32("resident_id"),
                            subject = reader.GetString("subject"),
                            description = reader.GetString("description"),
                            status = reader.GetString("status"),
                            admin_notes = reader.IsDBNull(reader.GetOrdinal("admin_notes")) ? string.Empty : reader.GetString("admin_notes")
                        });
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show($"Database error while loading complaints: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while loading complaints: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            ComplaintsDataGrid.ItemsSource = complaintsList;
        }

        private void AddNewComplaint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open AddComplaintWindow as modal dialog
                var addComplaintWindow = new AddComplaintWindow();
                addComplaintWindow.Owner = this; // Optional: sets owner for centering and focus
                bool? dialogResult = addComplaintWindow.ShowDialog();

                // If dialogResult is true, refresh the complaints list
                if (dialogResult == true)
                {
                    LoadComplaintsData(); // Replace with your actual refresh method
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Add Complaint Form: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void ComplaintsDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit) // Ensure the edit was committed
            {
                if (e.Row.Item is Complaint editedComplaint)
                {
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        try
                        {
                            connection.Open();
                            string query = "UPDATE complaints SET sta   tus = @status, admin_notes = @admin_notes WHERE complaint_id = @complaint_id";
                            MySqlCommand command = new MySqlCommand(query, connection);
                            // The 'editedComplaint.status' will have the new value from the ComboBox
                            // The 'editedComplaint.admin_notes' will have the new text
                            command.Parameters.AddWithValue("@status", editedComplaint.status);
                            command.Parameters.AddWithValue("@admin_notes", editedComplaint.admin_notes ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@complaint_id", editedComplaint.complaint_id);

                            int result = command.ExecuteNonQuery();
                            // ... rest of the error handling and success/failure messages ...
                        }
                        catch (MySqlException ex)
                        {
                            MessageBox.Show($"Database error while updating complaint: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            e.Cancel = true; // Cancel the edit if DB update fails
                            LoadComplaintsData(); // Revert to original data
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"An unexpected error occurred while updating: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            e.Cancel = true; // Cancel the edit
                            LoadComplaintsData(); // Revert
                        }
                    }
                }
            }
        }

        private void DeleteComplaint_Click(object sender, RoutedEventArgs e)
        {
            if (ComplaintsDataGrid.SelectedItem is Complaint selectedComplaint)
            {
                MessageBoxResult messageBoxResult = MessageBox.Show($"Are you sure you want to delete complaint ID: {selectedComplaint.complaint_id} ({selectedComplaint.subject})?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        try
                        {
                            connection.Open();
                            string query = "DELETE FROM complaints WHERE complaint_id = @complaint_id";
                            MySqlCommand command = new MySqlCommand(query, connection);
                            command.Parameters.AddWithValue("@complaint_id", selectedComplaint.complaint_id);

                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Complaint deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                LoadComplaintsData(); // Refresh the DataGrid
                            }
                            else
                            {
                                MessageBox.Show("Could not delete complaint. It might have been already deleted.", "Deletion Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        catch (MySqlException ex)
                        {
                            MessageBox.Show($"Database error while deleting: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"An error occurred while deleting: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a complaint to delete.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // --- Placeholder methods for other navigation buttons ---
        // (These methods from your provided code are kept as is)
        private void Button_Click(object sender, RoutedEventArgs e) // Dashboard
        {
            SocietyManagmentSystem.registrar registrarWindow = new SocietyManagmentSystem.registrar();
            registrarWindow.Show();

            Window currentWindow = Window.GetWindow(this);
            if (currentWindow != null)
                currentWindow.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) // Manage Residents
        {
            try
            {
                var manageResidentsWindow = new registrar_manageresidents(); // Assuming this window exists
                manageResidentsWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Manage Residents: {ex.Message}");
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) // Handle Complaints (Current Window Refresh)
        {
            try
            {
                LoadComplaintsData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing complaints: {ex.Message}");
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var loginWindow = new login(); // Assuming this window exists
                loginWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Login: {ex.Message}");
            }
        }
    }
}