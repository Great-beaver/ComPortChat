using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO.Ports;
using System.Collections;

namespace ComPortClient
{
    public partial class Form1 : Form
    {
        private ComPort cp;
        

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
             cp = new ComPort("COM2");

            this.DataRecived += new EventHandler<DataRecivedEventArgs>(Form1_DataRecived);

        }

        void Form1_DataRecived(object sender, Form1.DataRecivedEventArgs e)
        {
            this.BeginInvoke(new label_set_delegate(label_set), e.Message);

        }


        public delegate  void label_set_delegate (string message);


        public  void label_set(string message)
        {
           // label1.Text = message;
        }

        public event EventHandler<DataRecivedEventArgs> DataRecived;



        

        private void OnDataRecived(string message)
        {
            if (DataRecived!=null)
            {
                DataRecived(this, new DataRecivedEventArgs(message));
            }



        }

        public class DataRecivedEventArgs: EventArgs
        {
            public DataRecivedEventArgs(string message)
            {
                Message = message;
            }


            public string Message { get; private set; }

        }


        private void button1_Click(object sender, EventArgs e)
        {
                             
            //cp.SendMessage(textBox1.Text);
            //cp.SendString(StringCompressor.CompressString(textBox1.Text));
            cp.SendMessage("TextMessage",0,textBox1.Text);
            //richTextBox1.AppendText(String.Format("<{0}>: {1}", cp.Name, textBox1.Text + "\n"));
            richTextBox1.AppendText(textBox1.Text + "\n");
            
                textBox1.Clear();

            
        }
        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            // Обработчик принятых сообещний, вынести в класс

            while (cp.MessagesQueue.Count > 0)

            {
                // cp.MessagesQueue.Dequeue();
                //string[] message;
                object obj;
                obj = cp.MessagesQueue.Dequeue();
                string[] arr = ((IEnumerable)obj).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();
                for (int i = 0;i<arr.Length; i++)
                {
                    richTextBox1.AppendText(arr[i]+'\n');
                }

                


            //   richTextBox1.AppendText(Convert.ToString(cp.MessagesQueue.Dequeue()));
 
            }


            if (cp.FileReciveComplete)
            {
                cp.FileReciveComplete = false;
                cp.ReciveFileDialog();
                
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //cp.Name = textBox2.Text;

            //MessageBox.Show(Convert.ToString(textBox2.Text.GetHashCode()));

           // string MessageType = textBox2.Text.Substring(0, textBox2.Text.IndexOf('|'));
            
           // MessageBox.Show(MessageType);
            /*

            SaveFileDialog sd = new SaveFileDialog();

            if (sd.ShowDialog() == DialogResult.OK)
            {
                cp.ByteArrayToFile(sd.FileName, StringCompressor.Zip(textBox2.Text));
            }*/

            cp.SendMultiLineMessage(0,richTextBox2.Lines);


        }

        private void button3_Click(object sender, EventArgs e)
        {

                richTextBox1.AppendText(String.Format("<{0}>: {1}", cp.Name, "Sending File" + "\n"));
                cp.SendFileDialog();
            
        }


    }
}
