using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinformsExample
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
            
        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            CaptureForm.path_scalingtable = textBox2.Text;
            CaptureForm.path_configxml = textBox1.Text;
            CaptureForm.path_savetsv = textBox5.Text;
            CaptureForm.path_instrumentsetting = textBox3.Text;
            CaptureForm.port = textBox6.Text;
            CaptureForm.path_savecap = textBox4.Text;
            CaptureForm.IP = textBox7.Text;
            CaptureForm.aircraftname = textBox8.Text;
            string dumpTextPath = "actual_settings\\settings.txt";
            using (StreamWriter sw = File.CreateText(dumpTextPath))
            {
                sw.WriteLine(textBox1.Text);
                sw.WriteLine(textBox2.Text);
                sw.WriteLine(textBox3.Text);
                sw.WriteLine(textBox4.Text);
                sw.WriteLine(textBox5.Text);
                sw.WriteLine(textBox6.Text);
                sw.WriteLine(textBox7.Text);
                sw.WriteLine(textBox8.Text);
            }
            Close();
        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            

        }

        private void textBox2_Click(object sender, EventArgs e)
        {

           
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

          
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
        
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
        
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_Click(object sender, EventArgs e)
        {
            
        }

        private void textBox4_Click(object sender, EventArgs e)
        {
      
        }

        private void textBox5_Click(object sender, EventArgs e)
        {
           
        }

        private void Settings_Load(object sender, EventArgs e)
        {
              // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader("actual_settings\\settings.txt"))
                {
                    // Read the stream to a string, and write the string to the console.
                    textBox1.Text= sr.ReadLine();
                    textBox2.Text = sr.ReadLine();
                    textBox3.Text = sr.ReadLine();
                    textBox4.Text = sr.ReadLine();
                    textBox5.Text = sr.ReadLine();
                    textBox6.Text = sr.ReadLine();
                    textBox7.Text = sr.ReadLine();
                    textBox8.Text = sr.ReadLine();

            }
           

        }

        private void Settings_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (textBox1.Text == "" || textBox2.Text == "" || textBox3.Text == "" || textBox4.Text == "" || textBox5.Text == "")
            { MessageBox.Show("Choose all paths");
                e.Cancel = true;
            }
            
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.ShowDialog();

            textBox1.Text = openFileDialog1.FileName;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.ShowDialog();

            textBox2.Text = openFileDialog1.FileName;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.ShowDialog();

            textBox3.Text = openFileDialog1.FileName;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "All Files (*.*)|*.*";

            saveFileDialog1.ShowDialog();




            textBox4.Text = saveFileDialog1.FileName;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            FolderBrowserDialog katalog = new FolderBrowserDialog();
            katalog.SelectedPath = textBox5.Text;
            //saveFileDialog1.Filter = "All Files (*.*)|*.*";

            katalog.ShowDialog();




            textBox5.Text = katalog.SelectedPath.ToString();
        }
    }
}
