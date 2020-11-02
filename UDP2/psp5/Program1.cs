using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace udp2
{
    class Program
    {
        const string helpstring = "udp2 <mode> [file] [address]\n" +
                    "\tmode:\n" +
                    "\t\t-s - sender mode\n" +
                    "\t\t-r - receiver mode\n" +
                    "\tfile - file name to send/reseive\n" +
                    "\taddress - host to send. In receiver mode no need address parameter";
        const int packetSize = 8192; //network packet size restriction
        const int sendport = 20600;
        const int recieveport = 20601;

        static UdpClient udpSender; 
        static UdpClient udpReciever;
        static void Main(string[] args)
        {
            udpSender=new UdpClient(); //UDPClient to send data
            udpReciever = new UdpClient(); //UDPClient to recieve data
            if ((args.Length < 1) || (args[0] != "-s" && args[0] != "-r"))
                Console.WriteLine(helpstring);
            else if (args[0] == "-s")
            {
                if (args.Length < 3)
                {
                    Console.WriteLine("Not enough parameters");
                    Console.WriteLine(helpstring);
                }
                else Sender(args[1], args[2]); 
            }
            else if (args[0] == "-r")
            {
                if(args.Length<2) Receiver(); //if no filename data will be stored with original filename
                else Receiver(args[1]);
            }
            else Console.WriteLine(helpstring);
            Console.ReadLine();
            udpSender.Close();
            udpReciever.Close();
        }
        static void Sender(string path, string address)
        {
            IPAddress ipAddr;
            try
            {
                IPHostEntry entry = Dns.GetHostEntry(address); //resolving address
                ipAddr = entry.AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork); //getting first IPv4 address
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                return;
            }
            IPEndPoint sendEndPoint = new IPEndPoint(ipAddr, sendport); //create EndPoint to send
            IPEndPoint recvEndPoint = null; //recieve EndPoint is empty as we don't know what host will connect
            try
            {
                udpReciever = new UdpClient(recieveport); //bind recieve UDPClient with resieve port
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return;
            }
            using (FileStream fsSource = new FileStream(path, FileMode.Open, FileAccess.Read)) //open file to read
            {
                fsSource.Position = 0;
                int numBytesToRead = (int)fsSource.Length; //whole ammount of sending data is file size
                int numBytesRead = 0; //nubber of bytes which already send
                String name = Path.GetFileName(path);
                byte[] packetSend; //byte array for sending data
                byte[] packetRec; //byte array for recieving data
                udpReciever.Client.ReceiveTimeout = 5000; //time to wait answer from reciever
                packetSend = Encoding.Unicode.GetBytes(name); //prepare data (filename) to send
                udpSender.Send(packetSend, packetSend.Length, sendEndPoint); //sending packet
                packetRec = udpReciever.Receive(ref recvEndPoint); //waiting for reply
                int parts = (int)fsSource.Length / packetSize; //counting number of packets to send whole file
                if ((int)fsSource.Length % packetSize != 0) parts++;
                packetSend = BitConverter.GetBytes(parts); 
                udpSender.Send(packetSend, packetSend.Length, sendEndPoint); //sending number of parts
                packetRec = udpReciever.Receive(ref recvEndPoint);
                packetSend = new byte[packetSize]; 
                int n = 0;
                for (int i = 0; i < parts - 1; i++){
                    n = fsSource.Read(packetSend, 0, packetSize); //read another piece of file
                    if (n == 0) break; //if nothing were readed - break
                    numBytesRead += n; //nubmer of bytes where readed
                    numBytesToRead -= n; //number of bytes left to read
                    udpSender.Send(packetSend, packetSend.Length, sendEndPoint); //sending piece of file
                    packetRec = udpReciever.Receive(ref recvEndPoint);
                }
                packetSend = new byte[numBytesToRead]; //if there is left piece less then packetSize
                n = fsSource.Read(packetSend, 0, numBytesToRead); //read this piece
                udpSender.Send(packetSend, packetSend.Length, sendEndPoint); //and send
                packetRec = udpReciever.Receive(ref recvEndPoint);
            }
            Console.WriteLine("file sent");
        }
        static void Receiver(string path="")
        {
            IPEndPoint recvEndPoint = null;
            try
            {
                udpReciever = new UdpClient(sendport); //binding recieve UDPClient with sendport
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return;
            }
            byte[] packetSend = new byte[1]; //reply packet
            byte[] packetRec;
            packetSend[0] = 1;
            packetRec = udpReciever.Receive(ref recvEndPoint); //start to listen port
            IPEndPoint sendEndPoint = new IPEndPoint(recvEndPoint.Address, recieveport); //if there was connection - create send EndPoint with connection address  
            String name = Encoding.Unicode.GetString(packetRec);//first recieved packet is a file name
            udpSender.Send(packetSend, packetSend.Length, sendEndPoint); //send reply
            udpReciever.Client.ReceiveTimeout = 5000; //change awaiting time
            packetRec = udpReciever.Receive(ref recvEndPoint); //wait for number of file pieces
            int parts = BitConverter.ToInt32(packetRec, 0);
            udpSender.Send(packetSend, packetSend.Length, sendEndPoint);
            if (path != "") name = path; //if no file name was given save data with recieved file name 
            using (FileStream fsSource = new FileStream(name, FileMode.Create, FileAccess.Write)) //open file to write
            {
                for (int i = 0; i < parts; i++)
                {
                    packetRec = udpReciever.Receive(ref recvEndPoint); //recieve file piece
                    fsSource.Write(packetRec, 0, packetRec.Length); //write it to file
                    udpSender.Send(packetSend, packetSend.Length, sendEndPoint);
                }
            }
            Console.WriteLine("file received");
        }
    }
}
