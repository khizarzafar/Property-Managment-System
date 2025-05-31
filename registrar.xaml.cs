using System;
using System.Windows;

namespace SocietyManagmentSystem
{
    /// <summary>
    /// Interaction logic for registrar.xaml
    /// </summary>
    public partial class registrar : Window
    {
        public registrar()
        {
            InitializeComponent();
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            // Already on dashboard, so maybe refresh or do nothing
            MessageBox.Show("You are already on the Dashboard.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ManageResidentsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var manageResidentsWindow = new registrar_manageresidents();
                manageResidentsWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Manage Residents window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HandleComplaintsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var handleComplaintsWindow = new registrar_handlecomplaints();
                handleComplaintsWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Handle Complaints window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    var loginWindow = new login();
                    loginWindow.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during logout: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
