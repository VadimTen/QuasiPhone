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
    public partial class Form5 : Form
    {
        public Form5()
        {
            InitializeComponent();
            MessageBox.Show("Будьте аккуратны при введении данных!!!", "Мать КЦ");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //string phnum = numphon.Text;
            //string namec = namecont.Text;
            //string postc = post.Text;
            //int id;
            //id = null;
              string phone;
              phone = numphon.Text;
              string name;
              name = namecont.Text;
              string comment;
              comment = post.Text;
            if (phone!="" && name!="" && comment!="")
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
                    MySqlCommand cmd2 = new MySqlCommand("INSERT INTO phonebook (phone, DisplayName, comment) VALUES ('" + phone + "', '" + name + "', '" + comment + "')", con);
                    //----MySqlCommand cmd = new MySqlCommand("INSERT INTO phonebook (id, phone, DisplayName, comment) VALUES ('null', '" + phone + "', '" + name + "', '" + comment + "'", con);
                    cmd2.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    con.Close();
                    con.Dispose();
                }
                this.Close();
            }
            
        }

    }   
    
}
