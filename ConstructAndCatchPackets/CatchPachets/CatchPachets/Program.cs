using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//Для работы программы необходимо подключить SharpPcap к проекту
//Выберете в меню Средства->Диспетчер пакетов NuGet->Консоль диспетчера пакетов...
//В консоли диспетчера напишите Install-Package SharpPcap -Version 5.1.0
//После завершения установки добавьте следующие 2 строчки
using PacketDotNet;
using SharpPcap;
//Данная библиотека нужна для работы с файлами
using System.IO;

namespace CatchPachets
{
	class Program
	{
		//Задаем имя файла
		static string path = @"analyse.txt";
		//Создаем объект для работы с файлом
		static StreamWriter sw;
		//Обработчик события "Приход пакета"
		static void device_OnPacketArrival (object sender, CaptureEventArgs e)
		{
			//Извлекаем из пришедшего пакета e время
			DateTime time = e.Packet.Timeval.Date;
			//Извлекаем из пришедшего пакета e длину
			int len = e.Packet.Data.Length;
			//Объект Console связан с командной строкой, объект sw - с файлом
			//Весь вывод будем делать в командную строку и в файл одновременно
			Console.WriteLine();
			sw.WriteLine();
			//Выводим время и длину
			Console.WriteLine("{0}:{1}:{2},{3} Len={4}", time.Hour, time.Minute, time.Second, time.Millisecond, len);
			sw.WriteLine("{0}:{1}:{2},{3} Len={4}", time.Hour, time.Minute, time.Second, time.Millisecond, len);
			//Преобразуем пришедший пакет e в объект PacketDotNet
			var packet = Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
			//Извлекаем из пакета PacketDotNet кадр Ethernet
			var eth = ((PacketDotNet.EthernetPacket)packet);
			//Сохраеяем MAC-адрес источника пакета
			var mac = eth.SourceHardwareAddress;
			//Выводим MAC-адрес источника пакета
			Console.WriteLine("MAC address: {0}", mac);
			sw.WriteLine("MAC address: {0}", mac);
			//Пытаемся извлечь из пакета PacketDotNet IP-пакет
			var ip = packet.Extract<IPPacket>();
			//Если это удалось 
			if (ip != null) {
				//Выводим IP-адрес источника пакета
				Console.WriteLine("IP address: {0}", ip.SourceAddress);
				sw.WriteLine("IP address: {0}", ip.SourceAddress);
			}
			//Пытаемся извлечь из пакета PacketDotNet TCP-сегмент
			var tcp = packet.Extract<TcpPacket>();
			//Если это удалось 
			if (tcp != null) {
				//Выводим порты источника и назначения
				Console.WriteLine("TCP source port: {0}", tcp.SourcePort);
				sw.WriteLine("TCP source port: {0}", tcp.SourcePort);
				Console.WriteLine("TCP destination port: {0}", tcp.DestinationPort);
				sw.WriteLine("TCP destination port: {0}", tcp.DestinationPort);
				string data = Encoding.UTF8.GetString(tcp.PayloadData);
				if (data.Contains("Got it!"))
				{
					Console.WriteLine("TCP data: {0}", data);
					sw.WriteLine("TCP data: {0}", data);
				}
			}
			var udp = packet.Extract<UdpPacket>();
			if (udp != null)
			{
				//Выводим порты источника и назначения
				Console.WriteLine("UDP source port: {0}", udp.SourcePort);
				sw.WriteLine("UDP source port: {0}", udp.SourcePort);
				Console.WriteLine("UDP destination port: {0}", udp.DestinationPort);
				sw.WriteLine("UDP destination port: {0}", udp.DestinationPort);
			}
		}

		static void Main(string[] args)
		{
			{
				//Связываем объект файла с путем
				sw = new StreamWriter(path);
				// Получаем информацию о сетевых адаптерах компьютера 
				CaptureDeviceList devices = CaptureDeviceList.Instance;
				// Если сетевых адаптеров нет, выводим ошибку и закрываем программу 
				if (devices.Count < 1)
				{
					Console.WriteLine("No devices were found on this machine");
					Console.Write("Hit 'Enter' to exit...");
					Console.ReadLine();
					return;
				}
				Console.WriteLine("\nThe following devices are available:");
				Console.WriteLine("------------------------------------\n");
				// Выводим на экран список всех сетевых адаптеров
				for(int i=0; i<devices.Count;i++)
				{
					Console.Write("{0}. ", i + 1);
					Console.WriteLine("{0}", devices[i].ToString());
				}
				//Предлагаем пользователю выбрать адаптер для захвата пакетов
				Console.WriteLine("Choose device number to capture packets:");
				int num = 0;
				//Пытаемся преобразовать выбор пользователя в число
				if ((!Int32.TryParse(Console.ReadLine(), out num)) ||(num>devices.Count)) {
					//Если пользователь ввел неправильные данные - выводим ошибку и закрываем программу
					Console.WriteLine("Incorrect device number");
					Console.Write("Hit 'Enter' to exit...");
					Console.ReadLine();
					return;
				}
				//Извлкаем выбранный адаптер из списка адаптеров
				ICaptureDevice device = devices[--num];
				//Регистрируем обработчик события "Приход пакета"
				device.OnPacketArrival += new PacketArrivalEventHandler(device_OnPacketArrival);
				//Открываем адаптер в "смешанном режиме" с интервалом захвата 1000 мс
				device.Open(DeviceMode.Promiscuous, 1000); 
				Console.WriteLine("Listening on {0}, hit 'Enter' to stop...",device.Description);
				// Начинаем зазват пакетов
				device.StartCapture();
				// По нажатию 'Enter' захват останавливается 
				Console.ReadLine();
				// Останавливаем захват пакетов 
				device.StopCapture();
				// Закрываем адаптер 
				device.Close();
				//Закрываем файл
				sw.Close();
				Console.Write("Hit 'Enter' to exit...");
				Console.ReadLine();
				return;

			}
		}
	}
}
