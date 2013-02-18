using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Ports;
using System.Collections;

using System.Windows.Forms;

namespace ComPortClient
{
    class ComPort
    {
        private bool _fileToRecive = false;
        public bool FileReciveComplete = false;
        StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
        public bool NewMessage { get; private set; }
        public string Name= "User";
        private bool _continue = false;
        private byte[] _bytes;
        private SerialPort sp;
        private Thread readThread;
        public string Message="";
        public Queue MessagesQueue;

        public ComPort (string ComPortNumber)
        {

            sp = new SerialPort();
            sp.PortName = ComPortNumber;
            sp.BaudRate = 9600;
            sp.Parity = Parity.None;
            sp.DataBits = 8;
            sp.StopBits = StopBits.One;
            sp.Handshake = Handshake.None;
            sp.ReadTimeout = 500;
            sp.WriteTimeout = 500;

            sp.WriteBufferSize=20000;
            sp.ReadBufferSize = 20000;

            MessagesQueue = new Queue(100);

            NewMessage = false;

            

            readThread = new Thread(Read);
            sp.Open();
            _continue = true;
            readThread.Start();
        }

         ~ComPort()
        {
            _continue = false;
            readThread.Join();
            sp.Close();
        }

        private void Read()
        {
            while (_continue)
            {
                try
                {

                    if (_fileToRecive)
                    {
                        ReciveFile();
                        _fileToRecive = false;
                        FileReciveComplete = true;

                    }
                    else
                       {

                           MessagesQueue.Enqueue(sp.ReadLine());

                   //       string message = sp.ReadLine();
                   //
                   //       if (Message != message)
                   //       {
                   //           NewMessage = true;
                   //           Message = message;
                   //
                   //           if (stringComparer.Equals("#SendFile#", message))
                   //           {
                   //               Message = "Start reciving file";
                   //               _fileToRecive = true;
                   //           }
                   //       }


                       }

                }
                catch (TimeoutException) { }
            }
        }



        public string ReciveMessage ()
        {
            NewMessage = false;
            return Message;
        }

        public void SendMessage (string message)
        {
           // sp.WriteLine(String.Format("<{0}>: {1}", Name, message));

            sp.WriteLine("TextMessage|"+message+"|"+Convert.ToString(message.GetHashCode()));
        }

        public void SendSystemMessage(string message)
        {
            // sp.WriteLine(String.Format("<{0}>: {1}", Name, message));

            sp.WriteLine("SystemMessage|" + message + "|" + Convert.ToString(message.GetHashCode()));
        }


        public void SendString (string s)
        {
            sp.WriteLine(s);
        }


        public void SendFile(string filename)
        {
            _bytes = File.ReadAllBytes(filename);
            SendString("#SendFile#");
           // MessageBox.Show(_bytes.Length.ToString());
            sp.Write(_bytes,0,_bytes.Length); 
        }

        public void SendFileDialog()
        {
            OpenFileDialog od = new OpenFileDialog();

            if (od.ShowDialog() == DialogResult.OK)
            {
                
                SendFile(od.FileName);
            }
 
        }


        public void ReciveFile()
        {
            // sp.Read(_bytes, 0, _bytes.Length);   //  <<<<<< TROUBLE HERE!!!  just _bytes.Length = null!
            _bytes= new byte[19737];
            sp.Read(_bytes, 0, 19737);   

        //  return  ByteArrayToFile(filename, _bytes);
        }

        public void ReciveFileDialog()
        {
            //sp.Read(_bytes, 0, _bytes.Length);

            SaveFileDialog sd = new SaveFileDialog();

            if (sd.ShowDialog() == DialogResult.OK)
            {
                 ByteArrayToFile(sd.FileName, _bytes);
            }

        }



        public bool ByteArrayToFile(string _FileName, byte[] _ByteArray)
        {
            try
            {
                // Open file for reading
                System.IO.FileStream _FileStream = new System.IO.FileStream(_FileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);

                // Writes a block of bytes to this stream using data from a byte array.
                _FileStream.Write(_ByteArray, 0, _ByteArray.Length);

                // close file stream
                _FileStream.Close();

                return true;
            }
            catch (Exception _Exception)
            {
                // Error
                MessageBox.Show("Exception caught in process: {0}", _Exception.ToString());
            }

            // error occured, return false
            return false;
        }






    }
}
