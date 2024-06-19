using System;
using System.Data.SqlClient;
using System.Windows;

namespace Di11
{
    public partial class RegWindow : Window
    {
        private SqlConnection connection;

        public RegWindow(SqlConnection sqlConnection)
        {
            InitializeComponent();
            connection = sqlConnection;
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка на пустоту полей
            if (string.IsNullOrEmpty(EmailText.Text) || string.IsNullOrEmpty(PasswordText.Text) || string.IsNullOrEmpty(NameText.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }

            SqlCommand checkUserCmd = new SqlCommand("SELECT COUNT(*) FROM Accounts WHERE Email = @Email", connection);
            checkUserCmd.Parameters.AddWithValue("@Email", EmailText.Text);
            int userCount = (int)checkUserCmd.ExecuteScalar();

            if (userCount > 0)
            {
                MessageBox.Show("Пользователь с таким login уже существует");
                return;
            }

            SqlCommand cmd = new SqlCommand("INSERT INTO Accounts (Email, PasswordHash, Username, CreatedAt) VALUES (@Email, @PasswordHash, @Username, @CreatedAt)", connection);
            cmd.Parameters.AddWithValue("@Email", EmailText.Text);
            cmd.Parameters.AddWithValue("@PasswordHash", PasswordText.Text);
            cmd.Parameters.AddWithValue("@Username", NameText.Text);
            cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

            cmd.ExecuteNonQuery();
            MessageBox.Show("Регистрация успешна!");

            this.Close();
        }
    }
}
