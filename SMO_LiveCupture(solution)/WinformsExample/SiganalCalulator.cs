using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;


namespace WinformsExample
{
    public partial class SiganalCalulator : Form
    {
        public SiganalCalulator(Chart_form chart1)
        {
            InitializeComponent();
            chart = chart1;
        }
        public Chart_form chart = new Chart_form();
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private double kon = 0;
        private string name = "New signal";
        private int count = 0;
        private List<Operation> operations =new  List<Operation>();
        private Dictionary<int, List<Operation>> SaveOperation = new Dictionary<int, List<Operation>>();
        private Dictionary<int, int> SaveSamples= new Dictionary<int, int>();
        private Dictionary<string, string> SeriesMap = new Dictionary<string, string>();
        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox2.SelectedItem.ToString() != "")
            {
                comboBox2.Enabled = false;
                   var signal = ""; var oper = ""; var Type = "";
                if (radioButton1.Checked) { signal = textBox1.Text; Type = "Con"; }
                if (rdbtnsignal.Checked) { signal = comboBox1.SelectedItem.ToString(); Type = "Signal"; }
                //var znak = comboBox3.SelectedItem.ToString();
                if (count == 0)
                {
                    dataGridView1.Rows.Add("", Type, signal); comboBox3.Show(); label2.Show();
                    operations.Add(new WinformsExample.Operation() { signal = signal, oper = "", type = Type });
                }
                else
                {
                    if (comboBox3.SelectedItem == null)
                    {
                        MessageBox.Show("Give me opertaion");
                    }
                    else
                    {
                        dataGridView1.Rows.Add(comboBox3.SelectedItem.ToString(), Type, signal);
                        operations.Add(new WinformsExample.Operation() { signal = signal, oper = comboBox3.SelectedItem.ToString(), type = Type });
                    }

                }
                count++;
            }
            
        }
        IEnumerable<string> keys { set; get; }

        private void SiganalCalulator_Load(object sender, EventArgs e)
        {
            textBox2.Text = name;
            object[] opers = new object[] { '+' , '-', 'x' , '/'};
            object[] nums = new object[] { '1', '2', '3', '4', '5', '6' };
            comboBox3.Items.AddRange(opers);
            comboBox3.Hide();
            label2.Hide();
            keys = Chart_form.dic1.Values.Distinct();
            comboBox5.Items.AddRange(nums);

            // object[] Samplings = new object[] { keys.ToArray()};
            comboBox2.Items.AddRange(keys.ToArray());
        
            radioButton1.Checked = true;
            foreach (var seria in Chart_form.dic1.Keys.Distinct())
            {
                SeriesMap.Add(seria.Map_Series(), seria);
            }

            try { readfile(SaveOperation); }
            catch { };
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            var chosen = comboBox2.SelectedItem.ToString();
            var matches = Chart_form.dic1.Where(kvp => kvp.Value ==chosen).ToDictionary(x => x.Key.Map_Series(), x => x.Value);

            comboBox1.Items.AddRange(matches.Keys.ToArray());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                var oper = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();
                var type = dataGridView1.SelectedRows[0].Cells[1].Value.ToString();
                var signal = dataGridView1.SelectedRows[0].Cells[2].Value.ToString();
                var count = 0;
                foreach (var operation in operations)
                {
                    if((oper == operation.oper) && (type == operation.type) && (signal == operation.signal))
                    {
                        operations.Remove(operation);
                        dataGridView1.Rows.RemoveAt(dataGridView1.SelectedRows[0].Index);
                        if (count == 0) { dataGridView1.Rows[0].Cells[0].Value = ""; }
                        //return;
                    }
                    count++;
                }
                
                
            }
            catch {
                
            }
            try { var log = dataGridView1.Rows[1].Cells[2].Value == null;  }
            catch{
                comboBox2.Enabled = true;
                count = 0;
                comboBox3.Hide();
                label2.Hide();
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            var num = Int32.Parse(comboBox5.SelectedItem.ToString());
            if (SaveOperation.Keys.Contains(num))
            {
                SaveOperation[num] = new List<Operation>(operations);
            }
            else
            {
                SaveOperation.Add(num, new List<Operation>(operations));
            }

            if (SaveSamples.Keys.Contains(num))
            {
                SaveSamples[num] = Int32.Parse(comboBox2.SelectedItem.ToString());
            }
            else
            {
                SaveSamples.Add(num, Int32.Parse(comboBox2.SelectedItem.ToString()));
            }


        }

        private void savefile (Dictionary<int, List<Operation>> SaveOperation)
        {
            string dumpTextPath = "actual_settings\\SaveOperation.txt";
            using (StreamWriter sw = File.CreateText(dumpTextPath))
            {
                foreach(var key in SaveOperation.Keys)
                {
                    var num = SaveOperation[key].Count();
                    sw.WriteLine("$#" + key.ToString() + "#" + num.ToString() + "#" + SaveSamples[key]);
                    foreach (var operation in SaveOperation[key])
                    {
                        var line = operation.oper + "#";
                        line += operation.type + "#";
                        line += operation.signal;
                        sw.WriteLine(line);
                    }

                }



              
            }
        }

        private void readfile(Dictionary<int, List<Operation>> SaveOperation)
        {
            using (StreamReader sr = new StreamReader("actual_settings\\SaveOperation.txt"))
            {
                // Read the stream to a string, and write the string to the console.
                int num = 0;
                int count = 0;
                for (int i = 0; i<6; i++)
                {
                    var line = sr.ReadLine();


                    if (line != null)
                    {
                        if (line[0] == '$')
                        {
                            var nums = line.Split('#');
                            num = Int32.Parse(nums[1]);
                            count = Int32.Parse(nums[2]);
                            var samples = Int32.Parse(nums[3]);
                            var operations = new List<Operation>();
                            for (int j = 0; j < count; j++)
                            {
                                var line2 = sr.ReadLine();

                                var operationstings = line2.Split('#');
                                //var num = Int32.Parse(operationstings[0]);
                                var operation = new Operation() { oper = operationstings[0], type = operationstings[1], signal = operationstings[2] };
                                operations.Add(operation);
                            }

                            if (SaveOperation.Keys.Contains(num))
                            {
                                SaveOperation[num] = new List<Operation>(operations);
                            }
                            else
                            {
                                SaveOperation.Add(num, new List<Operation>(operations));
                            }

                            if (SaveSamples.Keys.Contains(num))
                            {
                                SaveSamples[num] = samples;
                            }
                            else
                            {
                                SaveSamples.Add(num, samples);
                            }
                        }
                    }
                }
                

            }
        }
        private bool AddSignal()
        {
            bool first = true;
            List<ValueType> new_signal = new List<ValueType>();
            var sample = comboBox2.SelectedItem.ToString();
            var count = Chart_form.time_dictionatyf[sample].Count();
            foreach (var operation in operations)
            {
                if (first)
                {
                    if(operation.type == "Con")
                    {
                        kon = Double.Parse(operation.signal);
                        new_signal = Enumerable.Range(1, count).Select(i => (ValueType)kon).ToList();
                    }
                    else
                    {
                        var SignalName = SeriesMap[operation.signal];
                        new_signal =new List<ValueType>( Chart_form.extractedSamples[SignalName]);
                    }
                    first = false;
                }
                else
                {
                    if(operation.type == "Con")
                    {
                        kon = Double.Parse(operation.signal);
                        new_signal = new_signal.Select((x, index) => Mat.Oper(x, kon,operation.oper)).ToList();
                    }
                    else
                    {
                        var SignalName = SeriesMap[operation.signal];
                        new_signal = new_signal.Select((x, index) => Mat.Oper(x, Chart_form.extractedSamples[SignalName][index], operation.oper)).ToList();
                    }
                 
                }

            }

            name = textBox2.Text;
            try { Chart_form.extractedSamples.Add(name, new List<ValueType>(new_signal)); }
            catch { MessageBox.Show("Error. Try other signal name"); return false; }

                Chart_form.dic1.Add(name, sample);
            var keys2 = new List<string>(Chart_form.extractedSamples.Keys);
            chart.dodawanie_checklistbox2(keys2, chart.checkedListBox1, Chart_form.extractedSamples);

            return true;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            savefile(SaveOperation);
            var valid = AddSignal();
            if (valid) Close();

        }

        private void DGV_change(int num)
        {
            try
            {
                dataGridView1.Rows.Clear();
                operations = SaveOperation[num];
                comboBox2.Enabled = true;
                comboBox2.SelectedItem = SaveSamples[num].ToString();
                comboBox2.Enabled = false;
                foreach (var operation in operations)
                {
                    dataGridView1.Rows.Add(operation.oper, operation.type, operation.signal);
                }
            }
            catch { MessageBox.Show("Not exist in memory"); }
            
        }
        private void ChangeButtonsColor(Button button)
        {
            button3.BackColor = SystemColors.Control;
            button4.BackColor = SystemColors.Control;
            button5.BackColor = SystemColors.Control;
            button7.BackColor = SystemColors.Control;
            button8.BackColor = SystemColors.Control;
            button9.BackColor = SystemColors.Control;
            button.BackColor = Color.Azure;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            DGV_change(1);
            ChangeButtonsColor(button3);
   

        }

        private void button4_Click(object sender, EventArgs e)
        {
            DGV_change(2);
            ChangeButtonsColor(button4);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            DGV_change(3);
            ChangeButtonsColor(button5);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            DGV_change(4);
            ChangeButtonsColor(button7);

        }

        private void button8_Click(object sender, EventArgs e)
        {
            DGV_change(5);
            ChangeButtonsColor(button8);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            DGV_change(6);
            ChangeButtonsColor(button9);
        }
    }
}
