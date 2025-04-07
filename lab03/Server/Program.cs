using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// Сервер
class EchoServer
{
    public static async Task Main()
    {
        // Устанавливаем IP-адрес и порт
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        int port = 8000;
        // Создаем TCP listener
        TcpListener server = new TcpListener(ipAddress, port);
        // Запускаем сервер
        server.Start();
        Console.WriteLine("Сервер запущен на {0}:{1}", ipAddress, port);
        Console.WriteLine("Ожидание подключения...");

        int clientCount = 0;
        try
        {
            while (true)
            {
                // Принимаем клиента
                TcpClient client = server.AcceptTcpClient();

                Thread thread = new Thread(() => HandleClient(client));
                thread.Name = $"Клиент №{++clientCount}";
                thread.Start();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка сервера: {0}", ex.Message);
        }
        finally
        {
            server.Stop();
        }
    }

    public static void HandleClient(TcpClient client)
    {
        Console.WriteLine($"{Thread.CurrentThread.Name} подключен!");
        
        // Получаем поток для чтения и записи   
        NetworkStream stream = client.GetStream();
        try
        {
            byte[] inputBuffer = new byte[3 * sizeof(int)];
            int bytesRead = stream.Read(inputBuffer, 0, inputBuffer.Length);

            // Читаем данные от клиента
            int a = BitConverter.ToInt32(inputBuffer, 0);
            int b = BitConverter.ToInt32(inputBuffer, sizeof(int));
            int c = BitConverter.ToInt32(inputBuffer, 2 * sizeof(int));
            Console.WriteLine($"Получены данные: {a}, {b} и {c}\nРешаем уравнение ({a})*x^2 + ({b})*x + ({c}) = 0\n\t  ...");

            // Проверка на линейное уравнение
            if (a == 0)
            {
                byte[] type = BitConverter.GetBytes(-1);
                stream.Write(type, 0, type.Length);
                return;
            }

            int d = b * b - 4 * a * c;
            if (d < 0)
            {
                byte[] type = BitConverter.GetBytes(0);
                stream.Write(type, 0, type.Length);
            }
            else if (d == 0)
            {
                byte[] type = BitConverter.GetBytes(1);
                stream.Write(type, 0, type.Length);

                double x = -b / (2.0 * a);
                byte[] answer = BitConverter.GetBytes(x);
                stream.Write(answer, 0, answer.Length);
            }
            else
            {
                byte[] type = BitConverter.GetBytes(2);
                stream.Write(type, 0, type.Length);

                double x1 = (-b + Math.Sqrt(d)) / (2.0 * a);
                double x2 = (-b - Math.Sqrt(d)) / (2.0 * a);

                byte[] answers = new byte[2 * sizeof(double)];
                Buffer.BlockCopy(BitConverter.GetBytes(x1), 0, answers, 0, sizeof(double));
                Buffer.BlockCopy(BitConverter.GetBytes(x2), 0, answers, sizeof(double), sizeof(double));
                stream.Write(answers, 0, answers.Length);
            }

            // Отправляем эхо обратно клиенту
            Console.WriteLine($"({Thread.CurrentThread.Name}) Ответ отправлен!\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"({Thread.CurrentThread.Name}) Ошибка при обработке клиента: {ex.Message}");
        }
        finally
        {
            // Закрываем соединение с клиентом
            stream.Close();
            client.Close();
            Console.WriteLine($"{Thread.CurrentThread.Name} отключен");
        }
    }
}