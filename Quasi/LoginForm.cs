using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Quasi
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            RegisterInfo.Login = "";
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex >= 0)
            {
                RegisterInfo.Login = Global.Operators[comboBox1.SelectedIndex].login;
                RegisterInfo.Password = Global.Operators[comboBox1.SelectedIndex].password;
                RegisterInfo.DisplayName = Global.Operators[comboBox1.SelectedIndex].DisplayName;
                RegisterInfo.ADLogin = Global.Operators[comboBox1.SelectedIndex].ADLogin;
            }
            else
                RegisterInfo.Login = "";

            Close();
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            for(int i = 0; i < Global.Operators.Count; i++)
                comboBox1.Items.Add(Global.Operators[i].DisplayName + " (" + Global.Operators[i].login + ")");
        }
    }
}
