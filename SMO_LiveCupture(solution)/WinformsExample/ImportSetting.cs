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
    public partial class ImportSetting : Form
    {
        private int Clic = 1;
        public ImportSetting(int clic)
        {
            InitializeComponent();
            //Clic = clic;
        }

        private void ImportSetting_Load(object sender, EventArgs e)
        {
            object[] resamplingi = new object[] { "100", "50", "20", "10", "5", "2", "1" };
            comboBox4.Items.AddRange(resamplingi);
            if (Clic == 1)
            {
                comboBox4.SelectedItem = Chart_form.resamplingFile;
                radioButton1.Checked = Chart_form.everyFile;
                radioButton2.Checked = Chart_form.dyFile;
            }
            else
            {
                comboBox4.SelectedItem = Chart_form.resamplingFile2;
                radioButton1.Checked = Chart_form.everyFile2;
                radioButton2.Checked = Chart_form.dyFile2;

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Clic == 1)
            {
                Chart_form.resamplingFile = comboBox4.SelectedItem.ToString();
                Chart_form.everyFile = radioButton1.Checked;
                Chart_form.dyFile = radioButton2.Checked;
            }
            else
            {
                Chart_form.resamplingFile2 = comboBox4.SelectedItem.ToString();
                Chart_form.everyFile2 = radioButton1.Checked;
                Chart_form.dyFile2 = radioButton2.Checked;
            }



            //Chart_form
            Close();
        }
    }
}
