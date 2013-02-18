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
             cp = new ComPort("COM3");

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
            cp.SendMessage(textBox1.Text);
            //richTextBox1.AppendText(String.Format("<{0}>: {1}", cp.Name, textBox1.Text + "\n"));
            richTextBox1.AppendText(textBox1.Text + "\n");
            
                textBox1.Clear();

            
        }
        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
          // if (cp.NewMessage)
          // {
          //     richTextBox1.AppendText(cp.ReciveMessage()+"\n");
          // }

            while (cp.MessagesQueue.Count > 0)
            {
                string s = Convert.ToString(cp.MessagesQueue.Dequeue());

              //  string[] message;

                string[] message = s.Split('\0');


                // string message = s.Substring(s.IndexOf('|'), s.Length);


                    //  string MessageType = s.Substring(0, s.IndexOf('|'));


                    switch (message[0])
                    {
                        case "TextMessage":
                            {



                                if (message[1].GetHashCode() == Convert.ToInt32(message[2]))
                                {
                                    cp.SendSystemMessage("Message delivered");
                                    richTextBox1.AppendText(message[1] + "\n");
                                }
                                else
                                {
                                    richTextBox1.AppendText(s + "\n");
                                    cp.SendSystemMessage("Message not delivered");
                                }
                            }
                            break;


                        case "SystemMessage":
                            {
                                if (message[1].GetHashCode() == Convert.ToInt32(message[2]))
                                {
                                    richTextBox1.AppendText(message[1] + "\n");
                                }
                            }
                            break;






                    }



                //  for (int i = 0; i < message.Length; i++)
               //  {
               //      MessageBox.Show(message[i]); 
               //  }


                
               // richTextBox1.AppendText(cp.MessagesQueue.Dequeue() + "\n"); 

               // richTextBox1.AppendText(StringCompressor.DecompressString(s) + "\n"); 

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

            string MessageType = textBox2.Text.Substring(0, textBox2.Text.IndexOf('|'));
            
            MessageBox.Show(MessageType);
            /*

            SaveFileDialog sd = new SaveFileDialog();

            if (sd.ShowDialog() == DialogResult.OK)
            {
                cp.ByteArrayToFile(sd.FileName, StringCompressor.Zip(textBox2.Text));
            }*/

        }

        private void button3_Click(object sender, EventArgs e)
        {
  
                richTextBox1.AppendText(String.Format("<{0}>: {1}", cp.Name, "Sending File" + "\n"));
                cp.SendFileDialog();
            
        }


    }
}
