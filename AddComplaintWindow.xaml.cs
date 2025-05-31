// AddComplaintWindow.xaml.cs
using MySql.Data.MySqlClient; // Make sure to add this using statement
using System.Windows;

namespace SocietyManagmentSystem
{
    public partial class AddComplaintWindow : Window
    {
        // Replace with your actual connection string or get it from a config file
        private string connectionString = "server=your_server;user=your_user;database=your_database;port=3306;password=your_password;";

        public Complaint NewComplaint { get; private set; }

        public AddComplaintWindow()
        {
            InitializeComponent();
            NewComplaint = new Complaint(); // Initialize with default status "Open"
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ResidentIdTextBox.Text) ||
                string.IsNullOrWhiteSpace(SubjectTextBox.Text) ||
                string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                MessageBox.Show("Resident ID, Subject, and Description cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(ResidentIdTextBox.Text, out int residentId))
            {
                MessageBox.Show("Resident ID must be a valid number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            NewComplaint.resident_id = residentId;
            NewComplaint.subject = SubjectTextBox.Text;
            NewComplaint.description = DescriptionTextBox.Text;
            // NewComplaint.admin_notes = AdminNotesTextBox.Text; // If you add AdminNotesTextBox

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    // complaint_id is auto-increment, status defaults to 'Open' in DB or class
                    string query = "INSERT INTO complaints (resident_id, subject, description, status, admin_notes) VALUES (@resident_id, @subject, @description, @status, @admin_notes)";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@resident_id", NewComplaint.resident_id);
                    command.Parameters.AddWithValue("@subject", NewComplaint.subject);
                    command.Parameters.AddWithValue("@description", NewComplaint.description);
                    command.Parameters.AddWithValue("@status", NewComplaint.status); // Will be "Open" by default
                    command.Parameters.AddWithValue("@admin_notes", NewComplaint.admin_notes ?? (object)DBNull.Value); // Handle null admin notes

                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Complaint added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.DialogResult = true; // Indicate success
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Failed to add complaint.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show($"Database error: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}