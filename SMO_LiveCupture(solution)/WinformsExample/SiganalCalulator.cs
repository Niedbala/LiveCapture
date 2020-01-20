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
            if (!((rdbtnsignal.Checked && comboBox1.SelectedItem == null) || (textBox1.Text == "" && radioButton1.Checked)))
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
            else
            {
                MessageBox.Show("Set signal or constant");
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
            comboBox2.SelectedItem = comboBox2.Items[0];


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
                    if( (type == operation.type) && (signal == operation.signal))
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
            var keys3 = new List<string>(Chart_form.extractedSamples.Keys);
            chart.dodawanie_checklistbox2(keys3, chart.checkedListBox1, Chart_form.extractedSamples);
            //trzeba wyciagnac z interpolate occure
           var occure =  Chart_form.interpolate_samples.Keys.ToList();
            var occurence2 = new List<int>();
            Chart_form.extractedSamples.Keys.ToList().ForEach(x => occurence2.Add(Chart_form.extractedSamples[x].Count));
            var dic3 = keys3.Zip(occurence2, (k, v) => new { k, v })
                         .ToDictionary(x => x.k, x => x.v);

            var occure2 = dic3.Values.ToList().Distinct().ToList();
            //var occure2 = ocure.Select(x => x).ToList();
            occure2.Sort();
            var occure3 = new List<int>(occure2);
            occure2.Remove(occure2.Last());
            occure3.Remove(occure3.First());
            // dla kazdego occurance wywolac interpolatre
            int j = 0;
            foreach (var num in occure2)
            {
                var new_interpolate_signal = interpolate(new_signal, occure3[j]);
               Chart_form.interpolate_samples[num][name] = new List<ValueType>(new_interpolate_signal);
                j++;
            }

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

        public List<ValueType> interpolate(List<ValueType> Y, double SampleCount)
        {   //moge tu policzyc count z Y
            //mam ile musi byc probek na kocu

            var ilosc = Y.Count();
            var SampleRate = SampleCount / ilosc;

            List<ValueType> new_signal = new List<ValueType>();
            if (SampleRate == 2)
            {

                for (int a = 0; a < Y.Count() - 1; a++)
                {
                    new_signal.Add(Y[a]);
                    if (Y[a] == Y[a + 1])
                    {
                        for (int i = 1; i < SampleCount; i++)
                        { new_signal.Add(Y[a]); }
                    }
                    else if (Y[a] != Y[a + 1])
                    {
                        var dis = (Convert.ToDouble(Y[a + 1]) - Convert.ToDouble(Y[a])) / SampleCount;
                        for (int i = 1; i < SampleRate; i++)
                        { new_signal.Add(Convert.ToUInt32(Convert.ToDouble(Y[a]) + (dis * i))); }

                    }
                }
                new_signal.Add(Y[Y.Count() - 1]);
                new_signal.Add(Y[Y.Count() - 1]);
            }
            else if (SampleRate < 2)
            {

                for (int a = 0; a < Y.Count() - 1; a++)
                {
                    new_signal.Add(Y[a]);

                }
                new_signal.Add(Y[Y.Count() - 1]);

                for (int i = new_signal.Count; i < SampleCount; i++)
                { new_signal.Add(0); }
            }
            else if (SampleRate > 2 && SampleRate < 5)
            {

                for (int a = 0; a < Y.Count() - 1; a++)
                {
                    new_signal.Add(Y[a]);
                    if (Y[a] == Y[a + 1])
                    {
                        for (int i = 1; i < (int)SampleCount; i++)
                        { new_signal.Add(Y[a]); }
                    }
                    else if (Y[a] != Y[a + 1])
                    {
                        var dis = (Convert.ToDouble(Y[a + 1]) - Convert.ToDouble(Y[a])) / SampleCount;
                        for (int i = 1; i < (int)SampleRate; i++)
                        { new_signal.Add(Convert.ToUInt32(Convert.ToDouble(Y[a]) + (dis * i))); }

                    }
                }
                new_signal.Add(Y[Y.Count() - 1]);
                new_signal.Add(Y[Y.Count() - 1]);
                for (int i = new_signal.Count; i < SampleCount; i++)
                { new_signal.Add(0); }
            }
            else if (SampleRate > 768 && SampleRate < 775)
            {

                for (int i = 1; i < 96; i++)
                { new_signal.Add(Y[0]); }
                for (int a = 0; a < Y.Count() - 1; a++)
                {
                    new_signal.Add(Y[a]);
                    if (Y[a] == Y[a + 1])
                    {
                        for (int i = 1; i < 768; i++)
                        { new_signal.Add(Y[a]); }
                    }
                    else if (Y[a] != Y[a + 1])
                    {
                        var dis = (Convert.ToDouble(Y[a + 1]) - Convert.ToDouble(Y[a])) / SampleCount;
                        for (int i = 1; i < 768; i++)
                        { new_signal.Add(Convert.ToUInt32(Convert.ToDouble(Y[a]) + (dis * i))); }

                    }
                }
                for (int i = 1; i < 96; i++)
                { new_signal.Add(Y[Y.Count() - 1]); }
                for (int i = new_signal.Count; i < SampleCount; i++)
                { new_signal.Add(0); }
            }
            else if (SampleRate > 755 && SampleRate < 768)
            {

                for (int i = 1; i < 96; i++)
                { new_signal.Add(Y[0]); }
                for (int a = 0; a < Y.Count() - 1; a++)
                {
                    new_signal.Add(Y[a]);
                    if (Y[a] == Y[a + 1])
                    {
                        for (int i = 1; i < 768; i++)
                        { new_signal.Add(Y[a]); }
                    }
                    else if (Y[a] != Y[a + 1])
                    {
                        var dis = (Convert.ToDouble(Y[a + 1]) - Convert.ToDouble(Y[a])) / SampleCount;
                        for (int i = 1; i < 768; i++)
                        { new_signal.Add(Convert.ToUInt32(Convert.ToDouble(Y[a]) + (dis * i))); }

                    }
                }
                for (int i = 1; i < 96; i++)
                { new_signal.Add(Y[Y.Count() - 1]); }
                for (int i = new_signal.Count; i < SampleCount; i++)
                { new_signal.Add(0); }
            }
            else if (SampleRate > 370 && SampleRate < 390)
            {
                for (int i = 1; i < 48; i++)
                { new_signal.Add(Y[0]); }
                for (int a = 0; a < Y.Count() - 1; a++)
                {
                    new_signal.Add(Y[a]);
                    if (Y[a] == Y[a + 1])
                    {
                        for (int i = 1; i < 384; i++)
                        { new_signal.Add(Y[a]); }
                    }
                    else if (Y[a] != Y[a + 1])
                    {
                        var dis = (Convert.ToDouble(Y[a + 1]) - Convert.ToDouble(Y[a])) / SampleCount;
                        for (int i = 1; i < 384; i++)
                        { new_signal.Add(Convert.ToUInt32(Convert.ToDouble(Y[a]) + (dis * i))); }

                    }
                }
                for (int i = 1; i < 48; i++)
                { new_signal.Add(Y[Y.Count() - 1]); }
                for (int i = new_signal.Count; i < SampleCount; i++)
                { new_signal.Add(0); }
            }

            return new_signal;
        }
    }
}
