using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quasi
{
    public partial class Form4 : Form
    {
        public Form4()
        {
            InitializeComponent();
            textBox1.PasswordChar = '*';
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string ControllPas = "mothercc";
            bool PasStat = false;
            if (textBox1.Text==ControllPas)
            {
                PasStat = true;
                MessageBox.Show("Пароль введён верно!", "Проверка пароля");
                MessageBox.Show("Добро пожаловать, Татьяна!", "Мать КЦ");
                
                string status = status_action.Text;

                if (status == "1")
                {
                    Form5 FAdd = new Form5();
                    FAdd.Show();
                }
                if (status == "2")
                {
                    Form7 FEdit = new Form7();
                    FEdit.Show();
                }
                if (status == "3")
                {
                    Form6 FDel = new Form6();
                    FDel.Show();
                }
                textBox1.Text = "";
                this.Close();
            }
            else
            {
                PasStat = false;
                MessageBox.Show("Пароль введён не верно!", "Проверка пароля");
            }
        }
    }
}
