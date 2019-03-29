using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinformsExample
{
    public partial class Set_chart_form : Form
    {
        public Set_chart_form()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Set_chart_form_Load(object sender, EventArgs e)
        {
            comboBox1.Items.Add("1000");
            comboBox1.Items.Add("2000");
            comboBox1.Items.Add("3000");
            comboBox1.Items.Add("5000");
            comboBox1.Items.Add("8000");
            comboBox1.Items.Add("10000");
            comboBox1.Items.Add("12000");
            comboBox1.Items.Add("15000");
            comboBox1.Items.Add("20000");
            comboBox1.Items.Add("25000");
            comboBox1.Items.Add("30000");
            comboBox1.Items.Add("40000");
            comboBox1.Items.Add("50000");
            comboBox1.Items.Add("60000");
            comboBox1.Items.Add("80000");
            comboBox1.Items.Add("100000");

            comboBox2.Items.Add("100");
            comboBox2.Items.Add("200");
            comboBox2.Items.Add("300");
            comboBox2.Items.Add("500");
            comboBox2.Items.Add("800");
            comboBox2.Items.Add("1000");
            comboBox2.Items.Add("1200");
            comboBox2.Items.Add("1500");

            comboBox1.SelectedItem = Chart_form.chart_width.ToString();
            comboBox2.SelectedItem = CaptureForm.data_delay.ToString();

            comboBox3.Items.Add("Auto");
            comboBox3.Items.Add("None");
            comboBox3.Items.Add("500");
            comboBox3.Items.Add("1000");
            comboBox3.Items.Add("3000");
            comboBox3.Items.Add("5000");
            comboBox3.Items.Add("10000");
            comboBox3.Items.Add("15000");
            comboBox3.Items.Add("20000");
            comboBox3.Items.Add("30000");
            comboBox3.Items.Add("50000");
            comboBox3.Items.Add("80000");
            comboBox3.Items.Add("100000");
            comboBox3.SelectedItem = Chart_form.grid.ToString();

            object[] resamplingi = new object[] { "100", "50", "20", "10", "5", "2", "1" };
            comboBox4.Items.AddRange(resamplingi);
            comboBox4.SelectedItem = Chart_form.resampling;
            radioButton1.Checked = Chart_form.every;
            radioButton2.Checked = Chart_form.dy;


        }

        private void Set_chart_form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (comboBox2.SelectedItem.ToString() == "" || comboBox1.SelectedItem.ToString() == "" || comboBox3.SelectedItem.ToString() == "")
            {
                MessageBox.Show("Choose all paths");
                e.Cancel = true;
            }
            else
            {
                CaptureForm.data_delay = Int32.Parse(comboBox2.SelectedItem.ToString());
                Chart_form.chart_width = Int32.Parse(comboBox1.SelectedItem.ToString());
               // if (checkBox1.Checked) { Chart_form.show_last_value = true; }
               // else { Chart_form.show_last_value = true; }
               // if (checkBox2.Checked) { Chart_form.show_legend = true; }
               // else { Chart_form.show_legend = true; }
                Chart_form.grid = comboBox3.SelectedItem.ToString();
                Chart_form.resampling = comboBox4.SelectedItem.ToString();
                Chart_form.every = radioButton1.Checked;
                Chart_form.dy = radioButton2.Checked;
                


            }

          
           
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            if (comboBox2.SelectedItem.ToString() == "" || comboBox1.SelectedItem.ToString() == "" || comboBox3.SelectedItem.ToString() == "")
            {
                MessageBox.Show("Choose all paths");
                return;
            }
            else
            {
                CaptureForm.data_delay = Int32.Parse(comboBox2.SelectedItem.ToString());
                Chart_form.chart_width = Int32.Parse(comboBox1.SelectedItem.ToString());
                // if (checkBox1.Checked) { Chart_form.show_last_value = true; }
                // else { Chart_form.show_last_value = true; }
                // if (checkBox2.Checked) { Chart_form.show_legend = true; }
                // else { Chart_form.show_legend = true; }
                Chart_form.grid = comboBox3.SelectedItem.ToString();
                Chart_form.resampling = comboBox4.SelectedItem.ToString();
                Chart_form.every = radioButton1.Checked;
                Chart_form.dy = radioButton2.Checked;



            }
            
            //Chart_form
            Close();
        }
    }
}
