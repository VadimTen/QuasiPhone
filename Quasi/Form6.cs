using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace Quasi
{
    public partial class Form6 : Form
    {
        public Form6()
        {
            InitializeComponent();
        }

        private void cont_del_button_Click(object sender, EventArgs e)
        {
            string del = num_del.Text;
            if (del != "")
            {
                string connectionString;
                connectionString = System.Configuration.ConfigurationSettings.AppSettings["ConnectionString"];
                MySqlConnection con = new MySqlConnection(connectionString);
                try
                {

                    con.Open();
                    MySqlCommand e2 = new MySqlCommand("SET NAMES utf8", con);
                    e2.ExecuteNonQuery();
                    e2.Dispose();
                    MySqlCommand cmd1 = new MySqlCommand("DELETE FROM phonebook WHERE phone='" + del + "'", con);

                    cmd1.ExecuteNonQuery();

                    
                }
                catch
                {
                    MessageBox.Show("Данный номер отсутствует в справочнике!", "Ну ё-моё! Не получилось((");
                }
                finally
                {
                    con.Close();
                    con.Dispose();
                    MessageBox.Show("Контакт удалён из справочника", "Всё ок!");
                    this.Close();
                }
            }
        }
    }
}
