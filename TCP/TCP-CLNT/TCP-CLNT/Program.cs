using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;

namespace TCP_CLNT
{
    class Program
    {
        const int ECHO_PORT = 8086; //port to send
        const String ip = "127.0.0.1"; //server address
        static void Main(string[] args) {
            StreamReader fr; //to read message file
            Byte[] data; //to encode sending data
            if (args.Count() < 1) //check arguments count
            {
                Console.WriteLine("No message file");
                Console.WriteLine("tcp-clnt filename");
                Console.ReadLine();
                return;
            }
            try
            {
                fr = new StreamReader(args[0], System.Text.Encoding.Default); //open file to read
            }
            catch (Exception exp) {
                Console.WriteLine("Exception: " + exp);
                Console.ReadLine();
                return;
            }
            try {
                TcpClient eClient = new TcpClient(ip, ECHO_PORT); //create TCP-client
                NetworkStream writerStream = eClient.GetStream(); //create stream to send messages
                String dataToSend= args[0];
                data = Encoding.ASCII.GetBytes(dataToSend+"\n"); 
                writerStream.Write(data, 0, data.Length); //send file name as client name
                while ((dataToSend = fr.ReadLine())!= null){ //read file until the end line by line
                    data = Encoding.ASCII.GetBytes(dataToSend+"\n");
                    writerStream.Write(data, 0, data.Length); //send readed line
                }
                data = Encoding.ASCII.GetBytes("QUIT"+"\n");
                writerStream.Write(data, 0, data.Length); //send "QUIT" message to say that transmission is over
                eClient.Close(); //close TCP-client
            }
            catch (Exception exp) {
                Console.WriteLine("Exception: " + exp);
            }
            Console.ReadLine();
        }
    }
}
