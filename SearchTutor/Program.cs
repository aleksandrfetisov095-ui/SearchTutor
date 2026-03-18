using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace SearchTutor
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine(" SEARCH TUTOR SERVER");
            Console.WriteLine("=======================");

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                int port = int.Parse(ConfigurationManager.AppSettings["ServerPort"] ?? "5555");

                
                var dbService = new DatabaseService(connectionString);
                await dbService.InitializeDatabaseAsync();

                var authService = new AuthService(connectionString);
                var handler = new CommandHandler(dbService, authService);

                
                await StartUdpServer(port, handler);
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Критическая ошибка: {ex.Message}");
                Console.WriteLine("Нажмите любую клавишу для выхода...");
                Console.ReadKey();
            }
        }

        static async Task StartUdpServer(int port, CommandHandler handler)
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));

                Console.WriteLine($"\n Сервер запущен на порту {port}");
                Console.WriteLine(" Ожидание команд...\n");

                while (true)
                {
                    try
                    {
                        byte[] buffer = new byte[8192];
                        EndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                        int received = socket.ReceiveFrom(buffer, ref clientEndPoint);
                        string message = Encoding.UTF8.GetString(buffer, 0, received);

                        string response = await handler.ProcessMessageAsync(message, clientEndPoint.ToString());

                        byte[] responseData = Encoding.UTF8.GetBytes(response);
                        socket.SendTo(responseData, clientEndPoint);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($" Ошибка: {ex.Message}");
                    }
                }
            }
        }
    }
}
