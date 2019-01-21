using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IAP
{
    public partial class Form1 : Form
    {
        SerialPort myport = new SerialPort();
        string filename = null;

 
        static byte[] download = { 0x44, 0x4F, 0x57, 0x4E, 0x4C, 0x4F, 0x41, 0x44, 0x0D };
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            myport.DataReceived += Port_DataReceived;

            string[] ports = SerialPort.GetPortNames();//获取当前计算机的串行端口名称数组  ports = {COM1,COM2.....}
            if (ports == null || ports.Length <= 0)
            {
                comboBox_port.Items.Add("无端口");
            }
            else
            {
                comboBox_port.Items.AddRange(ports);
                comboBox_port.SelectedIndex = 0;
            }
            comboBox5_baud.Items.AddRange(new object[]{
            "1200","2400","4800","9600","19200","38400","115200"
            });

            comboBox1.Items.AddRange(new object[]{
            "1200","2400","4800","9600","19200","38400","115200"
            });

            comboBox1.SelectedIndex = 3;
            comboBox5_baud.SelectedIndex = 3;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;
            textBox1.Enabled = false;
        }


        public void openPort(SerialPort SP,int flag)
        { 
            SP.PortName = comboBox_port.Text;
            if (flag == 1)
            {
                SP.BaudRate = Convert.ToInt32(comboBox1.Text);
                textBox2.AppendText("当前波特率：" + comboBox1.Text + "\n");
            }
            else
            {
                SP.BaudRate = Convert.ToInt32(comboBox5_baud.Text);
                textBox2.AppendText("当前波特率：" + comboBox5_baud.Text + "\n");
            }
            SP.Parity = Parity.None;
            SP.StopBits = StopBits.One;
            SP.DataBits = 8;

            try
            {
                SP.Open();
                button1_Open.Text = "关闭串口";
                if (flag == 1)
                {
                    textBox2.AppendText("串口打开成功，可以开始升级！\n");
                }
                else
                {
                    textBox2.AppendText("串口打开成功，请选择升级包！\n");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button1_Open_Click(object sender, EventArgs e)
        {
            if (button1_Open.Text == "打开串口")
            {
                openPort(myport, 0);
            }
            else
            {
                if (myport.IsOpen)
                {
                    myport.Close();
                    button1_Open.Text = "打开串口";
                    textBox2.AppendText("串口已关闭\n");
                   
                }
            }
        }

        public static byte[] ConvertToBinary(string Path)
        {
            FileStream stream = new FileInfo(Path).OpenRead();
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, Convert.ToInt32(stream.Length));
            return buffer;
        }

        private void button2_Click(object sender, EventArgs e)
        {

            Task task = new Task(senddata);
            task.Start();
            
        }

       public void senddata()
        {
            textBox2.AppendText("正在升级程序...\n");
            int cout = 0, len = 0;
            int length = 0, i = 0, m;
            long kk;
            float n = 0;

            byte[] sendbuf = ConvertToBinary(filename);

            len = sendbuf.Length;

            cout = len / 1024;
            m = len - 1024 * cout;

            for (i = 0; i < cout + 1;i++ )
            {
                if (i == cout)
                {
                    myport.Write(sendbuf, length, m);
                    length += m;
                    progressBar1.Value = 100;
                }
                else
                {
                    myport.Write(sendbuf, length, 1024);
                    length += 1024;

                    n = (float)length / (float)len;
                    n *= 100;
                    progressBar1.Value = (int)n;
                }
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = @"升级包|*.bin";
            file.ShowDialog();
            if (file.FileName.Length > 0)
            {
                this.textBox1.Text = file.FileName;
                filename = file.FileName;
                textBox2.AppendText("已加载升级包！\n");
                button1.Enabled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox2.AppendText("正在连接设备.....\n");
            myport.Write(download, 0, download.Length);
        }

        public  void  State(string s)
        {
            switch (s)
            {
                case "OK":
                    textBox2.AppendText("设备连接成功！\n");
                    myport.Close();
                    openPort(myport, 1);
                    button2.Enabled = true;
                    break;
                case "FALSE":
                    textBox2.AppendText("设备连接失败！\n");
                    break;
                case "ERROR":
                    textBox2.AppendText("升级失败！\n");
                    break;
                case "OVER":
                    textBox2.AppendText("升级成功！\n");
                    break;
            }
        }

        void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string s = null;
       
            try
            {
                do  //确保数据接收完整----用myport.ReadExisting(s)会出现中文乱码现象
                {
                    int count = myport.BytesToRead;
                    if (count <= 0)
                        break;
                    byte[] readBuffer = new byte[count];

                    Application.DoEvents();
                    myport.Read(readBuffer, 0, count);
                    s += System.Text.Encoding.Default.GetString(readBuffer);

                } while (myport.BytesToRead > 0);
                textBox2.AppendText(s + "\n");
                State(s);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void comboBox_port_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
