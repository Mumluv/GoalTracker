using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

namespace Di11
{
    /// <summary>
    /// Логика взаимодействия для EntryWindow.xaml
    /// </summary>
    public partial class EntryWindow : Window
    {
        private SqlConnection connection;
        public EntryWindow()
        {
            InitializeComponent();
            connection = new SqlConnection(@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = GoalTracker; Integrated Security = True;");
            connection.Open();
        }

        private void LoginButton(object sender, RoutedEventArgs e)
        {
            SqlCommand sql = new SqlCommand("select AccountID from Accounts where Email = '" + LoginText.Text + "' and PasswordHash = '" + PasswordText.Text + "';", connection);
            SqlDataReader reader = sql.ExecuteReader();
            if (reader.Read())
            {
                var id = reader[0].ToString();
                reader.Close();
                TargetsWindow targetsWindow = new TargetsWindow(connection, id);
                targetsWindow.Show();
                this.Close();

            }
            else
            {
                MessageBox.Show("Неверный логин или пароль");
            }
        }

        private void RegButton(object sender, RoutedEventArgs e)
        {
            RegWindow regWindow = new RegWindow(connection);
            regWindow.Show();
        }
    }
}
