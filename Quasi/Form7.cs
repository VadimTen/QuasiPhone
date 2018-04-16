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
using System.Text.RegularExpressions;

namespace Quasi
{
    public partial class Form7 : Form
    {
        public Form7()
        {
            InitializeComponent();
        }

        private void Form7_Shown(object sender, EventArgs e)
        {
            edit_name_label.Visible = false;
            edit_namecont.Visible = false;
            edit_num_label.Visible = false;
            edit_numphon.Visible = false;
            edit_post.Visible = false;
            edit_post_label.Visible = false;
            edit_button.Visible = false;

            this.Height = 126;
        }

        private void search_button_Click(object sender, EventArgs e)
        {
            string search = num_search.Text;

            Regex rex = new Regex(@"[\d+]");
            Match match = rex.Match(search);
            if (search != "" && match.Success)
            {
                string connectionString;
                connectionString = System.Configuration.ConfigurationSettings.AppSettings["ConnectionString"];
                MySqlConnection con = new MySqlConnection(connectionString);
                try
                {

                    con.Open();
                   /* MySqlCommand e2 = new MySqlCommand("SET NAMES utf8", con);
                    e2.ExecuteNonQuery();
                    e2.Dispose();*/
                    MySqlCommand cmd1 = new MySqlCommand("DELETE FROM phonebook WHERE phone='" + search + "'", con);
                    
                    cmd1.ExecuteNonQuery();

                    search_button.Visible = false;
                    num_search_label.Visible = false;
                    num_search.Visible = false;

                    edit_name_label.Visible = true;
                    edit_namecont.Visible = true;
                    edit_num_label.Visible = true;
                    edit_numphon.Visible = true;
                    edit_post.Visible = true;
                    edit_post_label.Visible = true;
                    edit_button.Visible = true;
                    this.Width = 338;
                    this.Height = 233;
                    MessageBox.Show("Введите новые данные контакта", "Обновление данных");
                }
                catch
                {
                    MessageBox.Show("Данный номер отсутствует в справочнике!", "Ошибка");
                }
                finally
                {
                    con.Close();
                    con.Dispose();
                }
            }
            else
            {
                MessageBox.Show("Поле не заполнено, либо содержит что-то кроме цифр", "Ну ё-моё! Не получилось((");
            }
            
        }

        private void edit_button_Click(object sender, EventArgs e)
        {
            
            string numphone = edit_numphon.Text;
            string name = edit_namecont.Text;
            string post = edit_post.Text;

            if (numphone != "" && name != "" && post != "")
            {
                string connectionString;
                connectionString = System.Configuration.ConfigurationSettings.AppSettings["ConnectionString"];
                MySqlConnection con2 = new MySqlConnection(connectionString);
                try
                {

                    con2.Open();
                    MySqlCommand e3 = new MySqlCommand("SET NAMES utf8", con2);
                    e3.ExecuteNonQuery();
                    e3.Dispose();
                    MySqlCommand cmd2 = new MySqlCommand("INSERT INTO phonebook (phone, DisplayName, comment) VALUES ('" + numphone + "', '" + name + "', '" + post + "')", con2);
                    //----MySqlCommand cmd = new MySqlCommand("INSERT INTO phonebook (id, phone, DisplayName, comment) VALUES ('null', '" + phone + "', '" + name + "', '" + comment + "'", con);
                    cmd2.ExecuteNonQuery();
                }
                catch
                {
                    MessageBox.Show("Вот что вы натворили?!", "Ну ё-моё! Не получилось((");
                }
                finally
                {
                    con2.Close();
                    con2.Dispose();
                    MessageBox.Show("Запись успешно обновлена!");
                    this.Close();
                }
                
            }
        }
    }
}
