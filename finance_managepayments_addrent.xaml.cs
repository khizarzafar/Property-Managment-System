using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace SocietyManagmentSystem
{
    public partial class finance_managepayments_addrent : Window
    {
        // Database connection string - adjust according to your setup
        private string connectionString = "Server=localhost;Database=zephyr;Uid=root;Pwd=root;";

        public finance_managepayments_addrent()
        {
            InitializeComponent();
            LoadResidents();
            SetDefaultValues();
        }

        private void LoadResidents()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"SELECT resident_id, 
                        CONCAT(name, ' - ', house_address) as DisplayText
                   FROM residents 
                   WHERE status = 'Active'
                   ORDER BY house_address, name";


                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    ResidentComboBox.ItemsSource = dataTable.DefaultView;
                    // Ensure DisplayMemberPath="DisplayText" and SelectedValuePath="resident_id" are set in XAML

                    if (dataTable.Rows.Count == 0)
                    {
                        MessageBox.Show("No active residents found. Please add residents first.",
                                        "No Residents", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading residents: {ex.Message}",
                                "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetDefaultValues()
        {
            // Set default due date to 5th of current month
            DateTime defaultDueDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5);

            // If we're past the 5th, set it to next month's 5th
            if (DateTime.Now.Day > 5)
            {
                defaultDueDate = defaultDueDate.AddMonths(1);
            }

            DueDatePicker.SelectedDate = defaultDueDate;
            // Set MonthYearTextBox to align with the defaultDueDate's month and year
            MonthYearTextBox.Text = defaultDueDate.ToString("yyyy-MM");
            AmountTextBox.Text = "500.00"; // Default amount
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInputs())
                {
                    return;
                }

                int residentId = (int)ResidentComboBox.SelectedValue;
                string monthYear = MonthYearTextBox.Text.Trim();
                decimal amount = decimal.Parse(AmountTextBox.Text.Trim(), CultureInfo.InvariantCulture);
                DateTime dueDate = DueDatePicker.SelectedDate.Value;

                if (CheckIfRentExists(residentId, monthYear))
                {
                    MessageBox.Show("Rent record already exists for this resident and month/year.",
                                    "Duplicate Record", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    // Query aligns with the 'rent' table columns: rent_id (AI), resident_id, month_year, amount, due_date, status
                    string query = @"INSERT INTO rent (resident_id, month_year, amount, due_date, status) 
                                     VALUES (@residentId, @monthYear, @amount, @dueDate, 'Pending')";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@residentId", residentId);
                    command.Parameters.AddWithValue("@monthYear", monthYear);
                    command.Parameters.AddWithValue("@amount", amount);
                    command.Parameters.AddWithValue("@dueDate", dueDate);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Rent record added successfully!",
                                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        Button clickedButton = sender as Button;
                        // Check the Tag of the button to decide whether to close or clear
                        if (clickedButton != null && "close".Equals(clickedButton.Tag?.ToString()))
                        {
                            this.DialogResult = true; // Indicate success
                            this.Close();
                        }
                        else // "Save and Add Another" or if Tag is not "close"
                        {
                            ClearForm();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Failed to add rent record.",
                                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving rent record: {ex.Message}",
                                "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInputs()
        {
            // Validate resident selection
            if (ResidentComboBox.SelectedValue == null)
            {
                MessageBox.Show("Please select a resident.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                ResidentComboBox.Focus();
                return false;
            }

            // Validate month/year format
            string monthYear = MonthYearTextBox.Text.Trim();
            if (string.IsNullOrEmpty(monthYear))
            {
                MessageBox.Show("Please enter month/year.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                MonthYearTextBox.Focus();
                return false;
            }

            if (!Regex.IsMatch(monthYear, @"^\d{4}-\d{2}$"))
            {
                MessageBox.Show("Month/Year must be in YYYY-MM format (e.g., 2025-05).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                MonthYearTextBox.Focus();
                return false;
            }

            try
            {
                string[] parts = monthYear.Split('-');
                int year = int.Parse(parts[0]);
                int month = int.Parse(parts[1]);

                if (year < 2020 || year > 2030) // Adjust range as needed
                {
                    MessageBox.Show("Year must be between 2020 and 2030.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    MonthYearTextBox.Focus();
                    return false;
                }
                if (month < 1 || month > 12)
                {
                    MessageBox.Show("Month must be between 01 and 12.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    MonthYearTextBox.Focus();
                    return false;
                }
            }
            catch
            {
                MessageBox.Show("Invalid month/year format.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                MonthYearTextBox.Focus();
                return false;
            }

            // Validate amount
            string amountText = AmountTextBox.Text.Trim();
            if (string.IsNullOrEmpty(amountText))
            {
                MessageBox.Show("Please enter an amount.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                AmountTextBox.Focus();
                return false;
            }
            if (!decimal.TryParse(amountText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amountValue))
            {
                MessageBox.Show("Please enter a valid amount.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                AmountTextBox.Focus();
                return false;
            }
            if (amountValue <= 0)
            {
                MessageBox.Show("Amount must be greater than zero.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                AmountTextBox.Focus();
                return false;
            }
            if (amountValue > 999999.99m) // Max amount check as per rent table decimal(10,2) implies 8 digits before decimal.
                                          // If it means total 10 digits, then max is 99,999,999.99.
                                          // Your original check was 999,999.99, which is decimal(8,2).
                                          // Sticking to original validation:
            {
                MessageBox.Show("Amount cannot exceed 999,999.99.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                AmountTextBox.Focus();
                return false;
            }


            // Validate due date
            if (DueDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Please select a due date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                DueDatePicker.Focus();
                return false;
            }
            DateTime dueDate = DueDatePicker.SelectedDate.Value;
            if (dueDate < DateTime.Now.Date.AddDays(-30)) // Due date cannot be too far in the past
            {
                MessageBox.Show("Due date cannot be more than 30 days in the past.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                DueDatePicker.Focus();
                return false;
            }

            return true;
        }

        private bool CheckIfRentExists(int residentId, string monthYear)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    // Query is compatible with 'rent' table structure
                    string query = "SELECT COUNT(*) FROM rent WHERE resident_id = @residentId AND month_year = @monthYear";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@residentId", residentId);
                    command.Parameters.AddWithValue("@monthYear", monthYear);

                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking for existing rent: {ex.Message}",
                                "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return true; // Prevent duplicate insertion on error
            }
        }

        private void ClearForm()
        {
            ResidentComboBox.SelectedIndex = -1;

            DateTime nextMonthForForm = DateTime.Now.AddMonths(1);
            // If current day is early in the month (e.g., before 5th),
            // and default due date was for current month, next entry might still be for current month.
            // For simplicity and consistency with SetDefaultValues for "next entry":
            DateTime nextDefaultDueDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5);
            if (DateTime.Now.Day > 5)
            { // If past the 5th, the "next" logically starts next month.
                nextDefaultDueDate = nextDefaultDueDate.AddMonths(1);
            }
            else
            { // If it's the 1st-5th, the next entry might still be for this month if manually clearing.
              // However, to always advance:
                nextDefaultDueDate = nextDefaultDueDate.AddMonths(1); // Always advance to next month for "add another"
            }


            MonthYearTextBox.Text = nextDefaultDueDate.ToString("yyyy-MM");
            DueDatePicker.SelectedDate = nextDefaultDueDate;
            AmountTextBox.Text = "500.00"; // Reset to default amount

            ResidentComboBox.Focus();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void GenerateBulkRent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show("Do you want to generate rent for all active residents for the upcoming rent cycle?",
                                             "Generate Bulk Rent", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    GenerateBulkRentForAllResidents();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initiating bulk rent generation: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateBulkRentForAllResidents()
        {
            try
            {
                // Determine the month/year for bulk generation (typically current or next month)
                DateTime targetDateForBulk = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5);
                if (DateTime.Now.Day > 5) // If we're past the 5th, generate for next month
                {
                    targetDateForBulk = targetDateForBulk.AddMonths(1);
                }
                string targetMonthYear = targetDateForBulk.ToString("yyyy-MM");
                DateTime dueDateForBulk = targetDateForBulk; // Due date is 5th of the target month
                decimal defaultAmount = 500.00m;

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Get all active residents who don't have rent for the target month/year
                    string selectQuery = @"SELECT r.resident_id 
                                           FROM residents r 
                                           WHERE r.status = 'Active' 
                                           AND r.resident_id NOT IN (
                                               SELECT rent.resident_id 
                                               FROM rent 
                                               WHERE rent.month_year = @monthYear
                                           )";
                    MySqlCommand selectCommand = new MySqlCommand(selectQuery, connection);
                    selectCommand.Parameters.AddWithValue("@monthYear", targetMonthYear);

                    List<int> residentIdsToProcess = new List<int>();
                    using (var reader = selectCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            residentIdsToProcess.Add(reader.GetInt32("resident_id"));
                        }
                    }

                    if (residentIdsToProcess.Count == 0)
                    {
                        MessageBox.Show($"All active residents already have rent records for {targetMonthYear}, or no active residents found.",
                                        "No Records to Generate", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    string insertQuery = @"INSERT INTO rent (resident_id, month_year, amount, due_date, status) 
                                           VALUES (@residentId, @monthYear, @amount, @dueDate, 'Pending')";
                    int recordsAdded = 0;
                    foreach (int residentId in residentIdsToProcess)
                    {
                        MySqlCommand insertCommand = new MySqlCommand(insertQuery, connection);
                        insertCommand.Parameters.AddWithValue("@residentId", residentId);
                        insertCommand.Parameters.AddWithValue("@monthYear", targetMonthYear);
                        insertCommand.Parameters.AddWithValue("@amount", defaultAmount);
                        insertCommand.Parameters.AddWithValue("@dueDate", dueDateForBulk);
                        recordsAdded += insertCommand.ExecuteNonQuery();
                    }

                    MessageBox.Show($"Successfully generated {recordsAdded} rent records for {targetMonthYear}.",
                                    "Bulk Generation Complete", MessageBoxButton.OK, MessageBoxImage.Information);

                    this.DialogResult = true; // Indicate success and close if needed
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating bulk rent: {ex.Message}",
                                "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}