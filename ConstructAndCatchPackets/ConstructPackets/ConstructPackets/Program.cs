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

namespace ConstructPackets
{
	class Program
	{
		static int getDeviceID() {
			// Получаем информацию о сетевых адаптерах компьютера 
			CaptureDeviceList devices = CaptureDeviceList.Instance;
			// Если сетевых адаптеров нет, выводим ошибку и закрываем программу 
			if (devices.Count < 1)
			{
				Console.WriteLine("No devices were found on this machine");
				Console.Write("Hit 'Enter' to exit...");
				Console.ReadLine();
				return -1;
			}
			Console.WriteLine("\nThe following devices are available:");
			Console.WriteLine("------------------------------------\n");
			// Выводим на экран список всех сетевых адаптеров
			for (int i = 0; i < devices.Count; i++)
			{
				Console.Write("{0}. ", i + 1);
				Console.WriteLine("{0}", devices[i].ToString());
			}
			//Предлагаем пользователю выбрать адаптер для захвата пакетов
			Console.WriteLine("Choose device number to capture packets:");
			int num = 0;
			//Пытаемся преобразовать выбор пользователя в число
			if ((!Int32.TryParse(Console.ReadLine(), out num)) || (num > devices.Count))
			{
				//Если пользователь ввел неправильные данные - выводим ошибку и закрываем программу
				Console.WriteLine("Incorrect device number");
				Console.Write("Hit 'Enter' to exit...");
				Console.ReadLine();
				return -1;
			}
			return --num;
		}
		static void Main(string[] args)
		{
			//Задаем TCP-порт источника
			ushort tcpSourcePort = 123;
			//Задаем TCP-порт назначения
			ushort tcpDestinationPort = 321;
			//Создаем новый TCP-сегмент передавая конструктору порт источника и назначения
			var tcpPacket = new TcpPacket(tcpSourcePort, tcpDestinationPort);
			//Подготавливаем данные для встраивания в TCP-сегмент
			//Данные должны быть представлены массивом байтов, поэтому преобразуем строку в массив байтов
			byte[] data = Encoding.ASCII.GetBytes("Got it!");
			//Встраиваем данные
			tcpPacket.PayloadData = data;
			//Задаем IP-адрес источника пакета
			var ipSourceAddress = System.Net.IPAddress.Parse("192.169.1.2");
			//Задаем IP-адрес назначения
			var ipDestinationAddress = System.Net.IPAddress.Parse("192.169.1.2");
			//127.0.0.1 - это виртуальный адрес компьютера, он применяется если клиент-серверное приложение запускается на одном узле
			//Если нужно отправлять пакеты на другой узел, то необходимо задать правильные адреса источника и приемника
			//Создаем IP-пакет
			var ipPacket = new IPv4Packet(ipSourceAddress,ipDestinationAddress);
			//Задаем MAC-адрес источника пакета
			var sourceHwAddress = "90-90-90-90-90-90";
			var ethernetSourceHwAddress =System.Net.NetworkInformation.PhysicalAddress.Parse(sourceHwAddress);
			//Задаем MAC-адрес источника пакета
			var destinationHwAddress = "80-80-80-80-80-80";
			var ethernetDestinationHwAddress =System.Net.NetworkInformation.PhysicalAddress.Parse(destinationHwAddress);
			//Для корректной работы в сети MAC-адреса также должны быть реальными а не вымышленными
			//Создаем Ethernet-кадр
			var ethernetPacket = new EthernetPacket(ethernetSourceHwAddress,ethernetDestinationHwAddress,EthernetType.None);
			// Собираем пакеты вместе
			//Встраиваем TCP-сегмент в IP-пакет
			ipPacket.PayloadPacket = tcpPacket;
			//Встраиваем IP-пакет в Ethernet-кадр
			ethernetPacket.PayloadPacket = ipPacket;
			//Преобразуем итоговый Ethernet-кадр в последовательность байтов
			byte[] packetBytes = ethernetPacket.Bytes;
			//Вызываем функцию выбора сетевого адаптера
			int devNum = getDeviceID();
			//Если функция вернула отрицательное число - заканчиваем программу
			if (devNum < 0) return;
			//Выбираем адаптер с нужним номером
			CaptureDeviceList devices = CaptureDeviceList.Instance;
			//Открываем адаптер
			devices[devNum].Open();
			//Пытаемся отправить пакет
			try
			{
				devices[devNum].SendPacket(packetBytes);
				Console.WriteLine("-- Packet sent successfuly.");
			}
			catch (Exception e)
			{
				//Если отправка не удалась выводим сообщение об ошибке
				Console.WriteLine("-- " + e.Message);
			}
			// Закрываем адаптер
			devices[devNum].Close();
			Console.WriteLine("-- Device closed.");
			Console.ReadLine();
		}
	}
}
