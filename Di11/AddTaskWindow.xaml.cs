using System;
using System.Data.SqlClient;
using System.Windows;

namespace Di11
{
    public partial class AddTaskWindow : Window
    {
        private SqlConnection connection;
        private int goalId;

        public AddTaskWindow(SqlConnection sqlConnection, int goalId)
        {
            InitializeComponent();
            connection = sqlConnection;
            this.goalId = goalId;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TaskTitleText.Text))
            {
                MessageBox.Show("Название задачи не может быть пустым.");
                return;
            }

            if (!TaskDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Пожалуйста, выберите дату выполнения задачи.");
                return;
            }

            SqlCommand cmd = new SqlCommand("INSERT INTO DailyProgress (GoalID, ProgressDate, Title, Status) VALUES (@GoalID, @ProgressDate, @Title, @Status)", connection);
            cmd.Parameters.AddWithValue("@GoalID", goalId);
            cmd.Parameters.AddWithValue("@ProgressDate", TaskDatePicker.SelectedDate.Value);
            cmd.Parameters.AddWithValue("@Title", TaskTitleText.Text);
            cmd.Parameters.AddWithValue("@Status", false);
            cmd.ExecuteNonQuery();
            this.Close();
        }
    }
}
