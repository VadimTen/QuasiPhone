using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace Quasi
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                RegisterInfo.SIPServerIP = System.Configuration.ConfigurationSettings.AppSettings["server"];
                RegisterInfo.SIPServerPort = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["port"]);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте корректность заполнения конфигурационного файла! " + ex.Message);
                Application.Exit();
            }

            try
            {
                Global.ReadSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте корректность заполнения конфигурационного файла! " + ex.Message);
                Application.Exit();
            }

            try
            {
                DB.GetOperators();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении списка операторов: " + ex.Message);
                Application.Exit();
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if(Global.Skin == 0)
                Application.Run(new Form1());
            else
                Application.Run(new Form2());
        }
    }
    public static class DB
    {
        public static string connectionString;
        #region Operators
        public static void GetOperators()
        {
            MySqlConnection con = new MySqlConnection(connectionString);

            try
            {
                con.Open();
                MySqlCommand e = new MySqlCommand("SET NAMES utf8", con);
                e.ExecuteNonQuery();
                e.Dispose();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM operator WHERE Active = '1' ORDER BY DisplayName", con);
                MySqlDataReader rdr = cmd.ExecuteReader();

                if (rdr.HasRows)
                    foreach (DbDataRecord record in rdr)
                    {
                        Operator op = new Operator();

                        if (record["ADLogin"] != DBNull.Value)
                            op.ADLogin = record["ADLogin"].ToString();

                        op.login = record["login"].ToString();
                        op.password = record["password"].ToString();
                        op.DisplayName = record["DisplayName"].ToString();
                        op.DeliveryLogin = record["DeliveryLogin"].ToString();
                        op.DeliveryPassword = record["DeliveryPassword"].ToString();
                        Global.Operators.Add(op);
                    }

                rdr.Close();
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
        }
        #endregion
        public static void SetStatus(string status)
        {
            MySqlConnection con = new MySqlConnection(connectionString);

            try
            {
                con.Open();
                MySqlCommand e = new MySqlCommand("SET NAMES utf8", con);
                e.ExecuteNonQuery();
                e.Dispose();
                MySqlCommand cmd = new MySqlCommand("INSERT INTO status_log (operator, status, dt) VALUES('" + RegisterInfo.Login + "', '" + status + "', NOW())", con);
                cmd.ExecuteNonQuery();
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
        }
        //получение списка контактов из справочника
        public static void GetContacts()
        {
            Global.Contacts.Clear();
            MySqlConnection con = new MySqlConnection(connectionString);

            try
            {
                con.Open();
                MySqlCommand e = new MySqlCommand("SET NAMES utf8", con);
                e.ExecuteNonQuery();
                e.Dispose();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM phonebook ORDER BY DisplayName", con);
                MySqlDataReader rdr = cmd.ExecuteReader();

                if (rdr.HasRows)
                    foreach (DbDataRecord record in rdr)
                    {
                        Contact c = new Contact();
                        c.id = Convert.ToInt32(record["id"]);
                        c.phone = record["phone"].ToString();
                        c.DisplayName = record["DisplayName"].ToString();
                        c.comment = record["comment"].ToString();
                        Global.Contacts.Add(c);
                    }

                rdr.Close();
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
        }

        //создание новой записи в справочнике
        //public static void AddNewContacts()
        //{
        //    MySqlConnection con = new MySqlConnection(connectionString);
        //    try
        //    {
        //        //int id;
        //        //id = null;
        //        Form5 EnterFrm = new Form5();
        //        string phone;
        //        //phone = EnterFrm;
        //        string name;
        //        name= null;
        //        string comment;
        //        comment = null;
        //        con.Open();
        //        MySqlCommand e = new MySqlCommand("SET NAMES utf8", con);
        //        e.ExecuteNonQuery();
        //        e.Dispose();
        //        MySqlCommand cmd = new MySqlCommand("INSERT INTO phonebook (phone, DisplayName, comment) VALUES ('" + phone + "', '" + name + "', '" + comment + "'", con);
        //        //MySqlCommand cmd = new MySqlCommand("INSERT INTO phonebook (id, phone, DisplayName, comment) VALUES ('null', '" + phone + "', '" + name + "', '" + comment + "'", con);
        //        cmd.ExecuteNonQuery();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    finally
        //    {
        //        con.Close();
        //        con.Dispose();
        //    }
        //}

        public static void SetReason(bool isIncoming, string reason, string guestNumber, int duration)
        {
            MySqlConnection con = new MySqlConnection(connectionString);

            try
            {
                con.Open();
                MySqlCommand e = new MySqlCommand("SET NAMES utf8", con);
                e.ExecuteNonQuery();
                e.Dispose();
                MySqlCommand cmd = new MySqlCommand("INSERT INTO callresult (isIncoming, reason, guestNumber, agentNumber, duration, calldatetime) VALUES(" + isIncoming.ToString() + ", '" + reason  + "', '" + guestNumber + "', '" + RegisterInfo.Login + "', " + duration.ToString() + ", NOW())", con);
                cmd.ExecuteNonQuery();
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
        }
    }
    public static class RegisterInfo
    {
        public static string SIPServerIP;
        public static int SIPServerPort;
        public static string NatAdress;
        public static string Login;
        public static string Password;
        public static string DisplayName;
        public static string ADLogin;
    }
    public static class DeliveryRI
    {
        public static string Login;
    }
    public class CallInfo
    {
        public bool isActive;
        public bool isIncoming;
        public string Phone;
        public string DisplayName;
        public DateTime CallDT;
        public DateTime dt;
        public TimeSpan Duration;
        public int SessionID;
        public string result;
        public bool Answered;
    }
    public class Operator
    {
        public string login;
        public string password;
        public string DisplayName;
        public string DeliveryLogin;
        public string DeliveryPassword;
        public string ADLogin;
    }
    public class Contact
    {
        public int id;
        public string phone;
        public string DisplayName;
        public string comment;
    }

    public static class Global
    {
        public static List<Operator> Operators = new List<Operator>();
        public static List<Contact> Contacts = new List<Contact>();
        public static bool ConnectSound;
        public static bool DisconnectSound;
        public static byte Skin;

        public static void ReadSettings()
        {
            try
            {
                DB.connectionString = System.Configuration.ConfigurationSettings.AppSettings["ConnectionString"];
                Global.ConnectSound = Convert.ToBoolean(System.Configuration.ConfigurationSettings.AppSettings["ConnectSound"]);
                Global.DisconnectSound = Convert.ToBoolean(System.Configuration.ConfigurationSettings.AppSettings["DisconnectSound"]);
                Global.Skin = Convert.ToByte(System.Configuration.ConfigurationSettings.AppSettings["Skin"]);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void SaveSettings()
        {
            try
            {
                System.Configuration.Configuration configuration = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
                configuration.AppSettings.Settings["ConnectSound"].Value = Global.ConnectSound.ToString();
                configuration.AppSettings.Settings["DisconnectSound"].Value = Global.DisconnectSound.ToString();
                configuration.AppSettings.Settings["Skin"].Value = Global.Skin.ToString();
                configuration.Save();
                System.Configuration.ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
