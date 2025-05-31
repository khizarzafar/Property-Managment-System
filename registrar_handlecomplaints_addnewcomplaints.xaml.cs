using System;
using System.Windows;
using System.Windows.Controls;

namespace SocietyManagmentSystem
{
    public partial class registrar_handlecomplaints_addnewcomplaints : Window
    {
        public registrar_handlecomplaints_addnewcomplaints()
        {
            InitializeComponent();
        }

        private void AddNewComplaint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                registrar_handlecomplaints_addnewcomplaints addComplaintWindow = new registrar_handlecomplaints_addnewcomplaints();
                addComplaintWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Add New Complaint: {ex.Message}");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                registrar_manageresidents residentsWindow = new registrar_manageresidents();
                residentsWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Manage Residents: {ex.Message}");
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                registrar_handlecomplaints complaintsWindow = new registrar_handlecomplaints();
                complaintsWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Handle Complaints: {ex.Message}");
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                login loginWindow = new login();
                loginWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Login: {ex.Message}");
            }
        }

        private void SaveComplaint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string residentId = ResidentIdTextBox.Text;
                string subject = SubjectTextBox.Text;
                string description = DescriptionTextBox.Text;
                string status = (StatusComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                string adminNotes = AdminNotesTextBox.Text;

                // TODO: Save the complaint to the database or wherever needed

                MessageBox.Show(
                    $"Complaint Saved:\nResident ID: {residentId}\nSubject: {subject}\nDescription: {description}\nStatus: {status}\nAdmin Notes: {adminNotes}",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                // Navigate back or close window if needed
                registrar_handlecomplaints complaintsWindow = new registrar_handlecomplaints();
                complaintsWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving complaint: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Add handlers for EditComplaint_Click and DeleteComplaint_Click here
    }
}
    