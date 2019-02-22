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
    public partial class Axis_setting : Form
    {
        public Axis_setting()
        {
            InitializeComponent();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Chart_form.chart1.ChartAreas[0].AxisX.ScaleView.Position = Int32.Parse(textBox1.Text);
            Chart_form.chart1.ChartAreas[0].AxisX.ScaleView.Size = Int32.Parse(textBox3.Text) - Int32.Parse(textBox1.Text);
            Chart_form.chart1.ChartAreas[0].AxisY.ScaleView.Position = Int32.Parse(textBox6.Text);
            Chart_form.chart1.ChartAreas[0].AxisY.ScaleView.Size = Int32.Parse(textBox5.Text) - Int32.Parse(textBox6.Text);
            Chart_form.chart1.ChartAreas[0].AxisX.MajorGrid.Interval = Int32.Parse(textBox2.Text);
            Chart_form.chart1.ChartAreas[0].AxisY.MajorGrid.Interval = Int32.Parse(textBox4.Text);

            this.Close();
        }

        private void Axis_setting_Load(object sender, EventArgs e)
        {
            textBox1.Text = Chart_form.chart1.ChartAreas[0].AxisX.Minimum.ToString();
            textBox3.Text = Chart_form.chart1.ChartAreas[0].AxisX.Maximum.ToString();
            textBox6.Text = Chart_form.chart1.ChartAreas[0].AxisY.Minimum.ToString();
            textBox5.Text = Chart_form.chart1.ChartAreas[0].AxisY.Maximum.ToString();
            textBox2.Text = Chart_form.chart1.ChartAreas[0].AxisX.MajorGrid.Interval.ToString();
            textBox4.Text = Chart_form.chart1.ChartAreas[0].AxisY.MajorGrid.Interval.ToString();
        }
    }
}
