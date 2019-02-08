using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using SharpPcap;
using SharpPcap.LibPcap;
using SharpPcap.AirPcap;
using SharpPcap.WinPcap;
using PacketDotNet;
using Smo.Startup;
using Smo.Common.Entities;
using Smo.Common.Infrastructure;
using Smo.Common.Public.Repositories;
using SmoReader.Entities;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;



namespace WinformsExample
{
    public partial class CaptureForm : Form
    {
        public static string path_scalingtable = "";
        public static string path_configxml = "";
        public static string path_savetsv = "";
        public static string path_instrumentsetting = "";
        public static string port = "";
        public static string path_savecap = "";
        public static string IP = "";
        public static string aircraftname = "";
        private Settings settings = new Settings();
        private Chart_form chartform = new Chart_form();
        public static Dictionary<string, List<ValueType>> extractedSamples = new Dictionary<string, List<ValueType>>();
        public static Dictionary<string, List<DateTime>> time_dictionaty = new Dictionary<string, List<DateTime>>();
        public static Dictionary<string, List<ValueType>> temp_exSam= new Dictionary<string, List<ValueType>>();
        public static bool start_chart = false;
        public static List<DateTime> timearray = new List<DateTime>();
        public static List<DateTime> timearray_128 = new List<DateTime>();
        public static List<DateTime> timearray_256 = new List<DateTime>();
        public static int data_delay = 100;
        public static Dictionary<int, List<int>> occurance = new Dictionary<int, List<int>>();
        public static List<ParameterDefinition> parametr_definition = new List<ParameterDefinition>();
        public static List<int> repeat_occurance = new List<int>();
        /// <summary>
        /// When true the background thread will terminate
        /// </summary>
        /// <param name="args">
        /// A <see cref="System.String"/>
        /// </param>
        private bool BackgroundThreadStop;

        /// <summary>
        /// Object that is used to prevent two threads from accessing
        /// PacketQueue at the same time
        /// </summary>
        /// <param name="args">
        /// A <see cref="System.String"/>
        /// </param>
        private object QueueLock = new object();

        /// <summary>
        /// The queue that the callback thread puts packets in. Accessed by
        /// the background thread when QueueLock is held
        /// </summary>
        private List<RawCapture> PacketQueue = new List<RawCapture>();

        /// <summary>
        /// The last time PcapDevice.Statistics() was called on the active device.
        /// Allow periodic display of device statistics
        /// </summary>
        /// <param name="args">
        /// A <see cref="System.String"/>
        /// </param>
        private DateTime LastStatisticsOutput;

        /// <summary>
        /// Interval between PcapDevice.Statistics() output
        /// </summary>
        /// <param name="args">
        /// A <see cref="System.String"/>
        /// </param>
        private TimeSpan LastStatisticsInterval = new TimeSpan(0, 0, 2);

        private System.Threading.Thread backgroundThread;

        private DeviceListForm deviceListForm;
        private static ICaptureDevice device;
        private static CaptureFileWriterDevice captureFileWriter;
        private static bool save_file = false;
        private static bool save_tsv = false;
        public CaptureForm()
        {
            InitializeComponent();
            Application.ApplicationExit += new EventHandler(Application_ApplicationExit);
        }

        void Application_ApplicationExit(object sender, EventArgs e)
        {
        }

        private void CaptureForm_Load(object sender, EventArgs e)
        {
            settings = new Settings();
            settings.ShowDialog();
            
            deviceListForm = new DeviceListForm();
            deviceListForm.OnItemSelected += new DeviceListForm.OnItemSelectedDelegate(deviceListForm_OnItemSelected);
            deviceListForm.OnCancel += new DeviceListForm.OnCancelDelegate(deviceListForm_OnCancel);
           // timearray.Add(DateTime.Now);
           
            using (StreamReader sr = new StreamReader("actual_settings\\settings.txt"))
            {
                   
                path_configxml  = sr.ReadLine();
                path_scalingtable = sr.ReadLine();
                path_instrumentsetting = sr.ReadLine();
                path_savecap = sr.ReadLine();
                path_savetsv = sr.ReadLine();
                port = sr.ReadLine();
                IP= sr.ReadLine();
                aircraftname = sr.ReadLine();

            }

        }

        void deviceListForm_OnItemSelected(int itemIndex)
        {
            // close the device list form
            deviceListForm.Hide();

            StartCapture(itemIndex);
        }

        void deviceListForm_OnCancel()
        {
            Application.Exit();
        }

        public class PacketWrapper
        {
            public RawCapture p;

            public int Count { get; private set; }
            //public PosixTimeval Timeval { get { return p.Timeval; } }
            // LinkLayers LinkLayerType { get { return p.LinkLayerType; } }
            public int Length { get { return p.Data.Length; } }
            //public int ip_dest { get { return p.; } }
            public int dataKeys{ get; private set; }

            public PacketWrapper(int count, RawCapture p,int dataKeys)
            {
                this.Count = count;
                this.dataKeys = dataKeys;
                this.p = p;
            }
        }

        private PacketArrivalEventHandler arrivalEventHandler;
        private CaptureStoppedEventHandler captureStoppedEventHandler;

        
        private void Shutdown()
        {
            if (device != null)
            {
                device.StopCapture();
                device.Close();
                device.OnPacketArrival -= arrivalEventHandler;
                device.OnCaptureStopped -= captureStoppedEventHandler;
                device = null;

                // ask the background thread to shut down
                BackgroundThreadStop = true;

                // wait for the background thread to terminate
                backgroundThread.Join();

                // switch the icon back to the play icon
                startStopToolStripButton.Image = global::WinformsExample.Properties.Resources.play_icon_enabled;
                startStopToolStripButton.ToolTipText = "Select device to capture from";
            }
        }


        


        private void StartCapture(int itemIndex)
        {
            packetCount = 0;
            device = CaptureDeviceList.Instance[itemIndex];
            packetStrings = new Queue<PacketWrapper>();
            bs = new BindingSource();
            dataGridView.DataSource = bs;
            LastStatisticsOutput = DateTime.Now;
            
            var converter = Converter.BuildLiveConverter(aircraftname, path_configxml, path_scalingtable, path_instrumentsetting);
            // start the background thread
            BackgroundThreadStop = false;
            backgroundThread = new System.Threading.Thread(() =>BackgroundThread(converter));
            backgroundThread.Start();

            // setup background capture
            
            arrivalEventHandler = new PacketArrivalEventHandler((sender, e) => device_OnPacketArrival(this, e, converter, extractedSamples));
            device.OnPacketArrival += arrivalEventHandler;
            captureStoppedEventHandler = new CaptureStoppedEventHandler(device_OnCaptureStopped);
            device.OnCaptureStopped += captureStoppedEventHandler;
            device.Open();
            //string filter = "udp port 1027 and host 192.168.28.1";
            string part1 = "";
            string part2 = "";
            string filter = "";
            if (port != "") { part1 = String.Concat("udp port ", port); }
            else { part1 = ""; }
            if (IP != "") { part2 = String.Concat("host ", IP); }
            else { part1 = ""; }
            if (part1 == "") {  filter = part2; }
            else if (part2 == "") {filter = part1; }
            else { filter = String.Concat(part1, " and ", part2); }
            //device.Filter = "host 192.168.0.179";
            device.Filter = filter;

            // force an initial statistics update
            captureStatistics = device.Statistics;
            UpdateCaptureStatistics("0");

            // start the background capture
            device.StartCapture();

            // disable the stop icon since the capture has stopped
            startStopToolStripButton.Image = global::WinformsExample.Properties.Resources.stop_icon_enabled;
            startStopToolStripButton.ToolTipText = "Stop capture";
        }

        void device_OnCaptureStopped(object sender, CaptureStoppedEventStatus status)
        {
            if (status != CaptureStoppedEventStatus.CompletedWithoutError)
            {
                MessageBox.Show("Error stopping capture", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Queue<PacketWrapper> packetStrings;

        private int packetCount;
        private BindingSource bs;
        private ICaptureStatistics captureStatistics;
        private bool statisticsUiNeedsUpdate = false;

        void device_OnPacketArrival(object sender, CaptureEventArgs e, LiveConverter converter, Dictionary<string, List<ValueType>> extractedSamples)
        {
            // print out periodic statistics about this device
            var Now = DateTime.Now; // cache 'DateTime.Now' for minor reduction in cpu overhead
            var interval = Now - LastStatisticsOutput;
            if (interval > LastStatisticsInterval)
            {
                Console.WriteLine("device_OnPacketArrival: " + e.Device.Statistics);
                captureStatistics = e.Device.Statistics;
                statisticsUiNeedsUpdate = true;
                LastStatisticsOutput = Now;
               
            }
           
            // lock QueueLock to prevent multiple threads accessing PacketQueue at
            // the same time
            lock (QueueLock)
            {
                PacketQueue.Add(e.Packet);
            }
        }

        private void CaptureForm_Shown(object sender, EventArgs e)
        {
            deviceListForm.Show();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (device == null)
            {
                deviceListForm.Show();
            }
            else
            {
                Shutdown();
            }
        }

        /// <summary>
        /// Checks for queued packets. If any exist it locks the QueueLock, saves a
        /// reference of the current queue for itself, puts a new queue back into
        /// place into PacketQueue and unlocks QueueLock. This is a minimal amount of
        /// work done while the queue is locked.
        ///
        /// The background thread can then process queue that it saved without holding
        /// the queue lock.
        /// </summary>
        private void BackgroundThread(LiveConverter converter)
        {
            while (!BackgroundThreadStop)
            {
                bool shouldSleep = true;

                lock (QueueLock)
                {
                    if (PacketQueue.Count != 0)
                    {
                        shouldSleep = false;
                    }
                }

                if (shouldSleep)
                {
                    System.Threading.Thread.Sleep(data_delay - 99);
                }
                else // should process the queue
                {
                    List<RawCapture> ourQueue;
                    lock (QueueLock)
                    {
                        // swap queues, giving the capture callback a new one
                        ourQueue = PacketQueue;
                        PacketQueue = new List<RawCapture>();
                    }

                    Console.WriteLine("BackgroundThread: ourQueue.Count is {0}", ourQueue.Count);

                     temp_exSam = new Dictionary<string, List<ValueType>>();
                    var st = new Stopwatch();
                    st.Start();
                    foreach (var packet in ourQueue)
                    //Parallel.ForEach(ourQueue, (packet) =>
                    {
                        // Here is where we can process our packets freely without
                        // holding off packet capture.
                        //
                        // NOTE: If the incoming packet rate is greater than
                        //       the packet processing rate these queues will grow
                        //       to enormous sizes. Packets should be dropped in these
                        //       cases
                        var result = converter.DecodePacket(packet);
                        

                        if (packetCount == 0)
                        {
                            packetCount++;
                            timearray.Add(result.AcraTime.Value);

                            //result.Samples.ForEach(s =>
                            //{
                            //    if (temp_exSam.ContainsKey(s.Key))
                            //        temp_exSam?[s.Key].Add(s.Value);
                            //    else
                            //        temp_exSam[s.Key] = new List<ValueType>() { s.Value };
                            //});

            
                            parametr_definition = converter.parameterDefinitions;
                            parametr_definition.ForEach(x => 
                            {
                                if (repeat_occurance.Any(b => b == x.Occurrences) == false)
                                {
                                    if (occurance.ContainsKey(x.StreamID))
                                    {
                                        occurance?[x.StreamID].Add(x.Occurrences);
                                        repeat_occurance.Add(x.Occurrences);
                                    }




                                    else
                                    {
                                        occurance[x.StreamID] = new List<int>() { x.Occurrences };
                                        repeat_occurance.Add(x.Occurrences);
                                    }

                                }


                                
                            });
                            //foreach(var item in occurance.Values)
                            //{
                            //    item.Distinct().ToList();
                            //}
                            

                            break;
                        }
                        if (save_file == true)
                        {
                            captureFileWriter.Write(packet);
                            save_tsv = true;

                        }


                        temp_exSam = new Dictionary<string, List<ValueType>>();

                        result.Samples.ForEach(s =>
                        {
                            if (extractedSamples.ContainsKey(s.Key))
                                extractedSamples?[s.Key].Add(s.Value);


                            else
                                extractedSamples[s.Key] = new List<ValueType>() { s.Value };


                            if (temp_exSam.ContainsKey(s.Key))
                                temp_exSam?[s.Key].Add(s.Value);
                            else
                                temp_exSam[s.Key] = new List<ValueType>() { s.Value };
                        });

                        timearray.Add(result.AcraTime.Value);
                        //var timeat = result.AcraTime.Value.Ticks;
                        // var timetdt = result.TimeDeltaTicks;

                        if (timearray.Count() > 1)
                        {
                            foreach (var time_divide in occurance[result.streamID])
                            {
                                TimeSpan delta = TimeSpan.FromTicks((timearray[timearray.Count() - 1].Subtract(timearray[timearray.Count() - 2]).Ticks) / time_divide);

                                if (time_dictionaty.ContainsKey(time_divide.ToString()))
                                {
                                    time_dictionaty?[time_divide.ToString()].Add(timearray[timearray.Count() - 2] + delta);
                                    for (int i = 0; i < time_divide; i++)
                                    {
                                        time_dictionaty[time_divide.ToString()].Add(time_dictionaty[time_divide.ToString()].Last() + delta);

                                    }
                                }


                                else
                                {
                                    time_dictionaty[time_divide.ToString()] = new List<DateTime>();
                                    time_dictionaty[time_divide.ToString()].Add(timearray[timearray.Count() - 2] + delta);
                                    for (int i = 0; i < time_divide - 1; i++)
                                    {
                                        time_dictionaty[time_divide.ToString()].Add(time_dictionaty[time_divide.ToString()].Last() + delta);

                                    }


                                }
                            }
                        }
                        /*
                        if (timearray.Count() > 1)
                        {
                            TimeSpan delta = TimeSpan.FromTicks((timearray[timearray.Count() - 1].Subtract(timearray[timearray.Count() - 2]).Ticks) / 256);
                            timearray_256.Add(timearray[timearray.Count() - 2] + delta);
                            for (int i = 0; i < 255; i++)
                            {
                                timearray_256.Add(timearray_256.Last() + delta);
                            }
                        }
                        */

                        int dataKeys = extractedSamples.Keys.Count();
                        var packetWrapper = new PacketWrapper(packetCount, packet, dataKeys);
                        this.BeginInvoke(new MethodInvoker(delegate
                        {
                            packetStrings.Enqueue(packetWrapper);
                        }
                        ));



                        packetCount++;
                        //zmien_label1(packetCount.ToString());
                        var time = packet.Timeval.Date;
                        var len = packet.Data.Length;
                        Console.WriteLine("BackgroundThread: {0}:{1}:{2},{3} Len={4}",
                            time.Hour, time.Minute, time.Second, time.Millisecond, len);
                    };
                    

                    if (save_tsv == true && save_file == false)
                    {
                        string headerLine = "";
                        foreach (string key in extractedSamples.Keys)
                        {
                            headerLine = headerLine + key + "\t";
                        }
                        var keys_test = new List<string>(extractedSamples.Keys);
                        var test = keys_test[0];
                        int len = extractedSamples[test].Count;
                        //string dumpTextPath = "C:\\Users\\pniedbala\\Desktop\\test_data\\509_valid_file1\\data_read2.tsv";
                        using (StreamWriter sw = File.CreateText(path_savetsv))
                        {
                            sw.WriteLine(headerLine);
                            for (int i = 0; i < len; i++)
                            {
                                string valuesLine = "";
                                foreach (string key in extractedSamples.Keys)
                                {
                                    valuesLine = valuesLine + extractedSamples[key][i] + "\t";
                                }
                                sw.WriteLine(valuesLine);
                            }
                        }
                    }

                    this.BeginInvoke(new MethodInvoker(delegate
                    {
                        bs.DataSource = packetStrings.Reverse();
                    }
                    ));

                    if (statisticsUiNeedsUpdate)
                    {
                        UpdateCaptureStatistics(ourQueue.Count.ToString());
                        statisticsUiNeedsUpdate = false;
                    }
                    if (start_chart)
                    {
                        try
                        {
                            this.Invoke((MethodInvoker)delegate { chartform.UpdateChart(); });
                            //UpdateChart();
                        }
                        catch
                        {
                        }

                    }
                    st.Stop();
                    var time_em = st.ElapsedMilliseconds;
                }
            }
        }
       

        private void UpdateCaptureStatistics(string QueueCount)
        {
            captureStatisticsToolStripStatusLabel.Text = string.Format("Received packets: {0}, Dropped packets: {1}, Interface dropped packets: {2}, QueueCount: {3} ",
                                                       captureStatistics.ReceivedPackets,
                                                       captureStatistics.DroppedPackets,
                                                       captureStatistics.InterfaceDroppedPackets,
                                                       QueueCount);
        }

        private void CaptureForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Shutdown();
        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView.SelectedCells.Count == 0)
                return;

            var packetWrapper = (PacketWrapper)dataGridView.Rows[dataGridView.SelectedCells[0].RowIndex].DataBoundItem;
            var packet = Packet.ParsePacket(packetWrapper.p.LinkLayerType, packetWrapper.p.Data);
            packetInfoTextbox.Text = packet.ToString(StringOutputType.VerboseColored);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
           
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void textBox1_Click(object sender, EventArgs e)
        {
    
        }

        private void button1_Click(object sender, EventArgs e)
        {
        
        }

        private void toolStripButton1_Click_1(object sender, EventArgs e)
        {
            if (save_file == false)
            {
                string capFile = path_savecap;
                captureFileWriter = new CaptureFileWriterDevice(capFile);
                save_file = true;
                System.Drawing.Image img = System.Drawing.Image.FromFile("C:\\Users\\pniedbala\\Pictures\\icons\\rec5.png");
                toolStripButton1.Image = img;
            }
            else
            {
                save_file = false;
                captureFileWriter.Close();
                System.Drawing.Image img2 = System.Drawing.Image.FromFile("C:\\Users\\pniedbala\\Pictures\\icons\\sav2.jpg");
               toolStripButton1.Image = img2;
            }

        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            settings = new Settings();
            settings.Show();

        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            chartform = new Chart_form();
            chartform.Show();
        }
    }
}
