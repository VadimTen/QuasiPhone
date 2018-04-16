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
    public partial class Form3 : Form
    {
        public string number = "";
        public string reason = "Заказ (оформление)";
        public bool isIncoming = true;
        public int duration = 0;
        public Form3()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (number[0] == '8')
                number = number.Substring(1);

            if(number[0] == '+')
                number = number.Substring(2);

            DB.SetReason(isIncoming, reason, number, duration);
            Close();
        }

        private void Form3_Shown(object sender, EventArgs e)
        {
            label3.Text = number;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            isIncoming = true;
            reason = radioButton1.Text;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            isIncoming = true;
            reason = radioButton2.Text;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            isIncoming = true;
            reason = radioButton3.Text;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            isIncoming = true;
            reason = radioButton4.Text;
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            isIncoming = true;
            reason = radioButton5.Text;
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            isIncoming = false;
            reason = radioButton6.Text;
        }

        private void radioButton7_CheckedChanged(object sender, EventArgs e)
        {
            isIncoming = false;
            reason = radioButton7.Text;
        }
    }
}
