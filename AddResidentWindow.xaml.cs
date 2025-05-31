using System;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient; // Ensure you have this using statement and the MySql.Data NuGet package

namespace SocietyManagmentSystem
{
    public partial class AddResidentWindow : Window
    {
        private string connectionString = "Server=localhost;Database=zephyr;Uid=root;Pwd=root;"; // Your MySQL connection string
        private ResidentModel _editingResident; // To store the resident being edited

        // Constructor for adding a NEW resident
        public AddResidentWindow()
        {
            InitializeComponent();
            this.Title = "Add New Resident";
            // Set default value for ComboBox and DatePicker if needed
            StatusComboBox.SelectedIndex = 0; // Default to "Active"
            MoveInDatePicker.SelectedDate = DateTime.Today; // Default to today's date
        }

        // Constructor for EDITING an existing resident
        public AddResidentWindow(ResidentModel residentToEdit) : this() // Calls the default constructor first
        {
            InitializeComponent(); // Already called by this(), but no harm if XAML elements are accessed after
            this.Title = "Edit Resident";
            _editingResident = residentToEdit;
            PopulateFieldsForEditing();
        }

        private void PopulateFieldsForEditing()
        {
            if (_editingResident != null)
            {
                NameTextBox.Text = _editingResident.name;
                EmailTextBox.Text = _editingResident.email;
                PhoneTextBox.Text = _editingResident.phone;
                HouseAddressTextBox.Text = _editingResident.house_address;
                MoveInDatePicker.SelectedDate = _editingResident.move_in_date;

                // Set ComboBox status
                foreach (ComboBoxItem item in StatusComboBox.Items)
                {
                    if (item.Content.ToString() == _editingResident.status)
                    {
                        StatusComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // --- 1. Retrieve data from UI elements ---
            string name = NameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string phone = PhoneTextBox.Text.Trim();
            string houseAddress = HouseAddressTextBox.Text.Trim();
            string status = ((ComboBoxItem)StatusComboBox.SelectedItem)?.Content.ToString();
            DateTime? moveInDate = MoveInDatePicker.SelectedDate;

            // --- 2. Basic Validation (remains the same) ---
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                NameTextBox.Focus();
                return;
            }
            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Email is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                EmailTextBox.Focus();
                return;
            }
            if (!email.Contains("@") || !email.Contains("."))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                EmailTextBox.Focus();
                return;
            }
            if (string.IsNullOrEmpty(phone))
            {
                MessageBox.Show("Phone number is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                PhoneTextBox.Focus();
                return;
            }
            if (string.IsNullOrEmpty(houseAddress))
            {
                MessageBox.Show("House address is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                HouseAddressTextBox.Focus();
                return;
            }
            if (status == null)
            {
                MessageBox.Show("Status is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                StatusComboBox.Focus();
                return;
            }
            if (!moveInDate.HasValue)
            {
                MessageBox.Show("Move-in date is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                MoveInDatePicker.Focus();
                return;
            }

            // --- 3. Construct SQL statement (INSERT or UPDATE) ---
            string query;
            bool isEditing = _editingResident != null;

            if (isEditing)
            {
                query = "UPDATE residents SET name = @Name, email = @Email, phone = @Phone, " +
                        "house_address = @HouseAddress, status = @Status, move_in_date = @MoveInDate " +
                        "WHERE resident_id = @ResidentId";
            }
            else
            {
                query = "INSERT INTO residents (name, email, phone, house_address, status, move_in_date) " +
                        "VALUES (@Name, @Email, @Phone, @HouseAddress, @Status, @MoveInDate)";
            }

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", name);
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Phone", phone);
                        command.Parameters.AddWithValue("@HouseAddress", houseAddress);
                        command.Parameters.AddWithValue("@Status", status);
                        command.Parameters.AddWithValue("@MoveInDate", moveInDate.Value);

                        if (isEditing)
                        {
                            command.Parameters.AddWithValue("@ResidentId", _editingResident.resident_id);
                        }

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show(isEditing ? "Resident updated successfully!" : "Resident added successfully!",
                                            "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            this.DialogResult = true;
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show(isEditing ? "Failed to update resident." : "Failed to add resident.",
                                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}\nError Number: {ex.Number}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (ex.Number == 1062) // Duplicate entry
                {
                    MessageBox.Show("A resident with this email or phone number might already exist.", "Duplicate Entry", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Indicate cancellation
            this.Close();
        }
    }
}