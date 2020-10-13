using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.IO;

namespace TCP_Serv
{
    public class ClientHandler {
        public TcpClient clientSocket;
        public void RunClient() { //client proceed
            StreamReader readStream = new StreamReader(clientSocket.GetStream()); //reader for client messages
            String returnData = readStream.ReadLine(); //read client name
            String name = returnData;
            Console.WriteLine("Client: " + name+" connected.");
            StreamWriter file = new StreamWriter(name); //file to write client messages
            while (true) {
                returnData = readStream.ReadLine(); //read new messge
                if (returnData.IndexOf("QUIT") > -1) { //if the message is "QUIT"
                    Console.WriteLine("Client " + name + " disconnected."); 
                    break; //stop client proceed
                }
                file.WriteLine(returnData); //if message is not "QUIT" write the message to file
            }
            file.Close();
            clientSocket.Close();
        }
    }
    public class Serv
    {
        const int ECHO_PORT=8086; //port to listen
        static void Main(string[] args)
        {
            try
            {
                TcpListener ClientListener = new TcpListener(ECHO_PORT); //create listener
                ClientListener.Start(); //start to listen
                Console.WriteLine("Server starts...");
                while(true)
                {
                    TcpClient client = ClientListener.AcceptTcpClient(); //accept client connection when it comes
                    ClientHandler cHandler = new ClientHandler(); //create client handler (runs inside client thread)
                    cHandler.clientSocket = client; //set socket property
                    Thread clientThread = new Thread(new ThreadStart(cHandler.RunClient)); //create new thread for new client
                    clientThread.Start(); //start client thread
                }
            }
            catch (Exception exp) { Console.WriteLine("Exception: " + exp); }
        }
    }
}
