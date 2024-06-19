using System;
using System.Data.SqlClient;
using System.Windows;

namespace Di11
{
    public partial class CreateGoalWindow : Window
    {
        private SqlConnection connection;
        private string accountId;

        public CreateGoalWindow(SqlConnection sqlConnection, string accountId)
        {
            InitializeComponent();
            connection = sqlConnection;
            this.accountId = accountId;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            string title = TitleText.Text.Trim();
            string description = DescriptionText.Text.Trim();

            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show("Название цели не должно быть пустым.");
                return;
            }

            if (string.IsNullOrEmpty(description))
            {
                MessageBox.Show("Описание цели не должно быть пустым.");
                return;
            }

            SqlCommand cmd = new SqlCommand("INSERT INTO Goals (AccountID, Title, Description, StartDate, CreatedAt) VALUES (@AccountID, @Title, @Description, @StartDate, @CreatedAt)", connection);
            cmd.Parameters.AddWithValue("@AccountID", accountId);
            cmd.Parameters.AddWithValue("@Title", title);
            cmd.Parameters.AddWithValue("@Description", description);
            cmd.Parameters.AddWithValue("@StartDate", DateTime.Now);
            cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

            cmd.ExecuteNonQuery();
            MessageBox.Show("Цель успешно создана!");
            this.Close();
        }
    }
}
