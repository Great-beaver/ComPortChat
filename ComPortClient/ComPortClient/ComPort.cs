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
        public int ClientId = 0;
        public bool MultiLineMessageStarted = false;
        public bool _fileToRecive = false;
        public bool FileReciveComplete = false;
        StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
        public bool NewMessage { get; private set; }
        public string Name= "User";
        private bool _continue = false;
        public byte[] _bytes;
        public SerialPort sp;
        private Thread readThread;
        public string Message="";
        public Queue MessagesQueue;
        public string[] MultiLineMessageBuffer;
        public int countOfRecivedLines;


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
                        string[] message = ParseMessage(sp.ReadLine());

                       if (message != null)
                       {
                           MessagesQueue.Enqueue(message);
                          
                       }
                           
                           
                       }

                }
                catch (TimeoutException) { }
            }
        }



        public string[] ParseMessage(string s)
        {
           // if (MessagesQueue.Count <= 0) return null;

           // string s = Convert.ToString(MessagesQueue.Dequeue());

            string[] outputmessage;

            string[] message = s.Split('\0');

            switch (message[0])
            {
                case "TextMessage":
                    {

                        if ((message[3]).GetHashCode() == Convert.ToInt32(message[4]))
                        {
                            SendMessage("SystemMessage", Convert.ToInt32(message[2]), "Message delivered");
                            outputmessage = new string[1];
                            outputmessage[0] = message[3] + "\n";
                            return outputmessage;
                             
                        }
                    }
                    break;


                case "SystemMessage":
                    {
                        if ((message[3]).GetHashCode() == Convert.ToInt32(message[4]))
                        {
                            outputmessage = new string[1];
                            outputmessage[0] = message[3] + "\n";
                            return outputmessage;
                        }
                    }
                    break;


                case "MultiLineMessageStart":
                    {
                        if (ClientId == Convert.ToInt32(message[1]))
                        {
                            countOfRecivedLines = 0;
                            MultiLineMessageBuffer = new string[Convert.ToInt32(message[3])];
                            MultiLineMessageStarted = true;
                            //outputmessage = new string[1];
                           // outputmessage[0] = ("Мультистрочное сообщение от ID: " + message[1] + "\n");
                           // return outputmessage;
                        }
                    }
                    break;

                case "Line":
                    {
                        if (((message[3]).GetHashCode() == Convert.ToInt32(message[4])) && MultiLineMessageStarted)
                        {
                            MultiLineMessageBuffer[countOfRecivedLines] = message[3];
                            countOfRecivedLines++;
                        }
                    }
                    break;

                case "MultiLineMessageFinish":
                    {
                        if (ClientId == Convert.ToInt32(message[1]))
                        {
                            if (countOfRecivedLines == MultiLineMessageBuffer.Length)
                            {
                                SendMessage("SystemMessage", Convert.ToInt32(message[2]), "Message delivered");   
                            }


                            MultiLineMessageStarted = false;

                            return MultiLineMessageBuffer;

                        }
                    }
                    break;


                case "FileTransferRequest":
                    {
                        if (Convert.ToInt16(message[1]) == ClientId)
                        {
                            _bytes = new byte[Convert.ToInt32(message[4])];
                            _fileToRecive = true;

                            outputmessage = new string[1];
                            outputmessage[0] = ("Reciving file" + "\n");
                            return outputmessage;
                        }
                    }
                    break;

            }

            return null;

            //  for (int i = 0; i < message.Length; i++)
            //  {
            //      MessageBox.Show(message[i]); 
            //  }



            // richTextBox1.AppendText(cp.MessagesQueue.Dequeue() + "\n"); 

            // richTextBox1.AppendText(StringCompressor.DecompressString(s) + "\n"); 

        }

        public void SendMessage (string type, int toId, string message)     
        {
            // sp.WriteLine(String.Format("<{0}>: {1}", Name, message));   

            // Формат тестового сообщения [Тип сообщения] [ID получателя] [ID отправителя] [тело сообщения] [Контрольнная сумма]

           // string s = ("TextMessage" + '\0' + toId + '\0' + ClientId + '\0' + message);

           // MessageBox.Show(s + '\0' + Convert.ToString(s.GetHashCode()));

            sp.WriteLine(type + '\0' + toId + '\0' + ClientId + '\0' + message + '\0' + Convert.ToString(message.GetHashCode()));
        }

        public void SendFileTransferRequest(int toId, string filePath, byte[] file)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            sp.WriteLine("FileTransferRequest" + '\0' + toId + '\0' + ClientId + '\0' + fileInfo.Name + '\0' + file.Length + '\0' + file.GetHashCode());
        }



        public void SendMultiLineMessage (int toId, string[] message)
        {
            // Заголовок сообщения из нескольких строк [Тип сообщения] [ID получателя] [ID отправителя] [количество строк]
            sp.WriteLine("MultiLineMessageStart" + '\0' + toId + '\0' + ClientId + '\0' + message.Length);



            for (int i = 0; i < message.Length; i++)
            {
                sp.WriteLine("Line" + '\0' + toId + '\0' + ClientId + '\0' + message[i] + '\0' + Convert.ToString(message[i].GetHashCode()));
            }

            sp.WriteLine("MultiLineMessageFinish" + '\0' + toId + '\0' + ClientId);


        }


        //    public void SendSystemMessage(string message, int toId)
 //    {
 //        // sp.WriteLine(String.Format("<{0}>: {1}", Name, message));
 //
 //        // Формат системного сообщения [Тип сообщения] [ID получателя] [ID отправителя] [тело сообщения] [Контрольнная сумма]
 //
 //       // string s = "SystemMessage" + '\0' + toId + '\0' + ClientId + '\0' + message;
 //
 //        sp.WriteLine("SystemMessage" + '\0' + toId + '\0' + ClientId + '\0' + message + '\0' + Convert.ToString(message.GetHashCode()));
 //        
 //    }


        public void SendString (string s)
        {
            sp.WriteLine(s);
        }


        public void SendFile(string filename)
        {
            
           // MessageBox.Show(_bytes.Length.ToString());
            sp.Write(_bytes,0,_bytes.Length); 
        }

        public void SendFileDialog()
        {
            OpenFileDialog od = new OpenFileDialog();

            if (od.ShowDialog() == DialogResult.OK)
            {
                _bytes = File.ReadAllBytes(od.FileName);
                SendFileTransferRequest(0, od.FileName, _bytes);
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
