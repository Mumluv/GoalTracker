using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace Di11
{
    public partial class TargetsWindow : Window
    {
        private SqlConnection connection;
        private string accountId;

        public TargetsWindow(SqlConnection sqlConnection, string accountId)
        {
            InitializeComponent();
            connection = sqlConnection;
            this.accountId = accountId;
            LoadGoals();
        }

        private void LoadGoals()
        {
            GoalsListBox.Items.Clear();
            SqlCommand cmd = new SqlCommand("SELECT GoalID, Title FROM Goals WHERE AccountID = @AccountID", connection);
            cmd.Parameters.AddWithValue("@AccountID", accountId);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                GoalsListBox.Items.Add(new { GoalID = reader["GoalID"], Title = reader["Title"] });
            }
            reader.Close();
        }

        private void CreateGoalButton_Click(object sender, RoutedEventArgs e)
        {
            CreateGoalWindow createGoalWindow = new CreateGoalWindow(connection, accountId);
            createGoalWindow.ShowDialog();
            LoadGoals();
        }

        private void GoalsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GoalsListBox.SelectedItem != null)
            {
                var selectedGoal = GoalsListBox.SelectedItem;
                int goalId = (int)selectedGoal.GetType().GetProperty("GoalID").GetValue(selectedGoal, null);
                if (CheckForMissedDays(goalId))
                {
                    CancelAllTasks(goalId);
                    MessageBox.Show("Цель отменена, так как был пропущен один день.");
                }
                LoadGoalDetails(goalId);
            }
        }

        private bool CheckForMissedDays(int goalId)
        {
            SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM DailyProgress WHERE GoalID = @GoalID AND ProgressDate < @Today AND Status = 0", connection);
            cmd.Parameters.AddWithValue("@GoalID", goalId);
            cmd.Parameters.AddWithValue("@Today", DateTime.Today);
            int missedDays = (int)cmd.ExecuteScalar();
            return missedDays > 0;
        }

        private void CancelAllTasks(int goalId)
        {
            SqlCommand cmd = new SqlCommand("UPDATE DailyProgress SET Status = 0 WHERE GoalID = @GoalID", connection);
            cmd.Parameters.AddWithValue("@GoalID", goalId);
            cmd.ExecuteNonQuery();
        }

        private void LoadGoalDetails(int goalId)
        {
            SqlCommand cmd = new SqlCommand("SELECT Title, Description FROM Goals WHERE GoalID = @GoalID", connection);
            cmd.Parameters.AddWithValue("@GoalID", goalId);
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                GoalTitleText.Text = reader["Title"].ToString();
                GoalDescriptionText.Text = reader["Description"].ToString();
            }
            reader.Close();
            LoadGoalProgress(goalId);
            LoadTasks(goalId);
        }

        private void LoadGoalProgress(int goalId)
        {
            SqlCommand cmd = new SqlCommand("SELECT COUNT(*) AS TotalDays, SUM(CASE WHEN Status = 1 THEN 1 ELSE 0 END) AS CompletedDays FROM DailyProgress WHERE GoalID = @GoalID", connection);
            cmd.Parameters.AddWithValue("@GoalID", goalId);
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                int totalDays = reader.GetInt32(reader.GetOrdinal("TotalDays"));
                int completedDays = reader.IsDBNull(reader.GetOrdinal("CompletedDays")) ? 0 : reader.GetInt32(reader.GetOrdinal("CompletedDays"));
                GoalProgressText.Text = $"Прогресс: {completedDays}/{totalDays} дней выполнено";
            }
            reader.Close();
        }

        private void LoadTasks(int goalId)
        {
            TasksListBox.Items.Clear();
            SqlCommand cmd = new SqlCommand("SELECT ProgressID, ProgressDate, Title, Status FROM DailyProgress WHERE GoalID = @GoalID ORDER BY ProgressDate ASC", connection);
            cmd.Parameters.AddWithValue("@GoalID", goalId);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                TasksListBox.Items.Add(new TaskItem
                {
                    ProgressID = reader.GetInt32(reader.GetOrdinal("ProgressID")),
                    ProgressDate = reader.GetDateTime(reader.GetOrdinal("ProgressDate")).ToString("yyyy-MM-dd"),
                    Title = reader["Title"].ToString(),
                    Status = reader.GetBoolean(reader.GetOrdinal("Status"))
                });
            }
            reader.Close();
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (GoalsListBox.SelectedItem != null)
            {
                var selectedGoal = GoalsListBox.SelectedItem;
                int goalId = (int)selectedGoal.GetType().GetProperty("GoalID").GetValue(selectedGoal, null);
                AddTaskWindow addTaskWindow = new AddTaskWindow(connection, goalId);
                addTaskWindow.ShowDialog();
                LoadTasks(goalId);
                LoadGoalProgress(goalId);
            }
        }

        private void TaskStatus_Checked(object sender, RoutedEventArgs e)
        {
            UpdateTaskStatus(sender, true);
        }

        private void TaskStatus_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateTaskStatus(sender, false);
        }

        private void UpdateTaskStatus(object sender, bool status)
        {
            CheckBox checkBox = sender as CheckBox;
            TaskItem task = checkBox.DataContext as TaskItem;
            int progressId = task.ProgressID;

            SqlCommand cmd = new SqlCommand("UPDATE DailyProgress SET Status = @Status WHERE ProgressID = @ProgressID", connection);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@ProgressID", progressId);
            cmd.ExecuteNonQuery();

            var selectedGoal = GoalsListBox.SelectedItem;
            int goalId = (int)selectedGoal.GetType().GetProperty("GoalID").GetValue(selectedGoal, null);
            LoadGoalProgress(goalId);
        }

        private void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (TasksListBox.SelectedItem != null)
            {
                TaskItem selectedTask = TasksListBox.SelectedItem as TaskItem;
                int progressId = selectedTask.ProgressID;

                MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить эту задачу?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    SqlCommand cmd = new SqlCommand("DELETE FROM DailyProgress WHERE ProgressID = @ProgressID", connection);
                    cmd.Parameters.AddWithValue("@ProgressID", progressId);
                    cmd.ExecuteNonQuery();

                    // Обновляем список задач и прогресс после удаления
                    var selectedGoal = GoalsListBox.SelectedItem;
                    int goalId = (int)selectedGoal.GetType().GetProperty("GoalID").GetValue(selectedGoal, null);
                    LoadTasks(goalId);
                    LoadGoalProgress(goalId);
                }
            }
        }

        private void DeleteGoalButton_Click(object sender, RoutedEventArgs e)
        {
            if (GoalsListBox.SelectedItem != null)
            {
                var selectedGoal = GoalsListBox.SelectedItem;
                int goalId = (int)selectedGoal.GetType().GetProperty("GoalID").GetValue(selectedGoal, null);

                MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить эту цель вместе со всеми задачами?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    // Удаление всех задач, связанных с целью
                    SqlCommand deleteTasksCmd = new SqlCommand("DELETE FROM DailyProgress WHERE GoalID = @GoalID", connection);
                    deleteTasksCmd.Parameters.AddWithValue("@GoalID", goalId);
                    deleteTasksCmd.ExecuteNonQuery();

                    // Удаление цели
                    SqlCommand deleteGoalCmd = new SqlCommand("DELETE FROM Goals WHERE GoalID = @GoalID", connection);
                    deleteGoalCmd.Parameters.AddWithValue("@GoalID", goalId);
                    deleteGoalCmd.ExecuteNonQuery();

                    // Перезагрузка списка целей
                    LoadGoals();
                    // Очистка информации о цели и задачах
                    GoalTitleText.Text = "";
                    GoalDescriptionText.Text = "";
                    GoalProgressText.Text = "";
                    TasksListBox.Items.Clear();
                }
            }
        }
    }
}
