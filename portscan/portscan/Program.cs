using System;
using System.Net;
using System.Net.Sockets;
using System.IO; //for StreamReader
using System.Collections; //for ArrayList
namespace Lab1
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("portscan IPs_file.txt ports_file.txt");
                return;
            }
            String ipfn = args[0]; //IPs file name
            String pfn = args[1]; //ports file name
            ArrayList ips = new ArrayList();
            ArrayList ports = new ArrayList();
            try
            {
                StreamReader ipf = new StreamReader(ipfn); //open IPs file
                StreamReader pf = new StreamReader(pfn); //open ports file
                String line;
                while ((line = ipf.ReadLine()) != null) ips.Add(line); //read line from file and put it to ArrayList
                while ((line = pf.ReadLine()) != null) ports.Add(line);
            }
            catch (Exception e)
            { //catch file proceed exceptions
                Console.WriteLine("Incorrect filename");
                return;
            }
            foreach (String ipAddress in ips) //iterating over all IPs
            {
                try
                {
                    IPAddress ipAddr = IPAddress.Parse(ipAddress); // trying to parse IP
                    foreach (String p in ports) //iterating over all ports
                    {
                        int i = 0;
                        Int32.TryParse(p, out i); //trying to parse port
                        if (i == 0) continue; //if port parsing error - get next port
                        IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, i); //create ipendpoint 
                        try
                        {
                            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //create socket
                            IAsyncResult result = s.BeginConnect(ipEndPoint, null, s); //trying to connect to pendpoint
                            bool success = result.AsyncWaitHandle.WaitOne(100, true); //time to wait connection
                            if (s.Connected)
                            {
                                Console.WriteLine("Port {0} on address {1} is opened", ipEndPoint.Port, ipEndPoint.Address);
                                s.Shutdown(SocketShutdown.Both);
                                s.EndConnect(result);
                                s.Close();
                            }
                            else
                            {
                                Console.WriteLine("Port {0} on address {1} is closed", ipEndPoint.Port, ipEndPoint.Address);
                                s.Shutdown(SocketShutdown.Both);
                                s.Close();
                            }

                        }
                        catch (Exception e) { } //socket connection error
                    }
                }
                catch (Exception e) //IP parse error
                {
                    Console.WriteLine("No access to ip address {0}", ipAddress);
                    continue;
                }

            }
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
