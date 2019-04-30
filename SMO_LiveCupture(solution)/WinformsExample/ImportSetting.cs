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
        public ImportSetting()
        {
            InitializeComponent();
        }

        private void ImportSetting_Load(object sender, EventArgs e)
        {
            object[] resamplingi = new object[] { "100", "50", "20", "10", "5", "2", "1" };
            comboBox4.Items.AddRange(resamplingi);
            comboBox4.SelectedItem = Chart_form.resamplingFile;
            radioButton1.Checked = Chart_form.everyFile;
            radioButton2.Checked = Chart_form.dyFile;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
                Chart_form.resamplingFile = comboBox4.SelectedItem.ToString();
                Chart_form.everyFile = radioButton1.Checked;
                Chart_form.dyFile = radioButton2.Checked;




            //Chart_form
            Close();
        }
    }
}
