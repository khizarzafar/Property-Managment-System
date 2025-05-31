using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace SocietyManagmentSystem
{
    /// <summary>
    /// Interaction logic for finance.xaml
    /// </summary>
    public partial class finance : Window
    {
        public finance()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Dashboard button - you might want to refresh the current page or navigate to a different dashboard
            // Avoid creating infinite navigation loop
            // NavigationService?.Navigate(new admin()); // This creates a loop

            // Instead, you could refresh the page or do nothing since we're already on the dashboard
            // Or navigate to a specific dashboard content page
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                var managePaymentsWin = new finance_managepayments();
                managePaymentsWin.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Manage Payments: {ex.Message}");
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                var paymentReportsWin = new finance_paymentreports();
                paymentReportsWin.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Payment Reports: {ex.Message}");
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var loginWin = new login();
                loginWin.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error logging out: {ex.Message}");
            }
        }

        
    }
}
