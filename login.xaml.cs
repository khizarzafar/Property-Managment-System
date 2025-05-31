using System;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;

namespace SocietyManagmentSystem
{
    /// <summary>
    /// Interaction logic for login.xaml
    /// </summary>
    public partial class login : Window
    {
        // Update this connection string with your actual database details
        private string connectionString = "Server=localhost;Database=zephyr;Uid=root;Pwd=root;";

        private TextBox emailTextBox;
        private PasswordBox passwordBox;

        public login()
        {
            InitializeComponent();
            GetControlReferences();
        }

        private void GetControlReferences()
        {
            // Get references to the TextBox and PasswordBox from your XAML
            // You'll need to add x:Name attributes to these controls in your XAML
            emailTextBox = this.FindName("EmailTextBox") as TextBox;
            passwordBox = this.FindName("PasswordBox") as PasswordBox;

            // If the controls don't have names, find them by type
            if (emailTextBox == null || passwordBox == null)
            {
                FindControlsByType(this);
            }
        }

        private void FindControlsByType(DependencyObject parent)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is TextBox textBox && emailTextBox == null)
                {
                    emailTextBox = textBox;
                }
                else if (child is PasswordBox pBox && passwordBox == null)
                {
                    passwordBox = pBox;
                }

                FindControlsByType(child);
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Optional: Add email validation here
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Not needed for login functionality
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string email = emailTextBox?.Text?.Trim();
                string password = passwordBox?.Password;

                // Validate input
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Please enter both email and password.", "Validation Error",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validate email format
                if (!IsValidEmail(email))
                {
                    MessageBox.Show("Please enter a valid email address.", "Validation Error",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Authenticate user
                string userRole = AuthenticateUser(email, password);

                if (!string.IsNullOrEmpty(userRole))
                {
                    // Login successful - navigate based on role
                    NavigateBasedOnRole(userRole);
                }
                else
                {
                    MessageBox.Show("Invalid email or password.", "Login Failed",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during login: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string AuthenticateUser(string email, string password)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT password, role FROM users WHERE email = @email";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@email", email);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedPassword = reader["password"].ToString();
                                string role = reader["role"].ToString();

                                // Verify password (assuming you're storing hashed passwords)
                                if (VerifyPassword(password, storedPassword))
                                {
                                    return role;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database connection error: {ex.Message}", "Database Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return null;
        }

        private bool VerifyPassword(string enteredPassword, string storedPassword)
        {
            // Use plain text comparison since your DB stores plain passwords like 'ceo123'
            return enteredPassword == storedPassword;
        }


       

        private void NavigateBasedOnRole(string role)
        {
            try
            {
                Window dashboardWindow = null;

                switch (role.ToUpper())
                {
                    case "CEO":
                        
                      dashboardWindow = new admin();
                        break;

                    case "REGISTRAR":
                        dashboardWindow = new registrar(); // You'll need to create RegistrarWindow
                        // dashboardWindow = new RegistrarWindow();
                        break;

                    case "FINANCE":
                        dashboardWindow = new finance(); // You'll need to create FinanceWindow
                        // dashboardWindow = new FinanceWindow();
                        break;

                    default:
                        MessageBox.Show("Unknown user role.", "Error",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                }

                if (dashboardWindow != null)
                {
                    // Show the appropriate dashboard
                    dashboardWindow.Show();

                    // Close the login window
                    this.Close();

                    MessageBox.Show($"Welcome! Logged in as {role}.", "Login Successful",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error navigating to dashboard: {ex.Message}", "Navigation Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // Optional: Handle Enter key press for login
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Button_Click(sender, new RoutedEventArgs());
            }
        }
    }
}