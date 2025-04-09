using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
class EchoClient
{
    public static void Main(string[] args)
    {
        try
        {
            // Подключаемся к серверу
            TcpClient client = new TcpClient("127.0.0.1", 8000);
            NetworkStream stream = client.GetStream();

            // Бесконечный цикл для ввода сообщений
            while (true)
            {
                Console.Write("Введите аргументы для уравнения (ax^2 + bx + c = 0) (или 'exit' для выхода): ");

                // считываем 3 числа из терминала
                string input = Console.ReadLine();
                // Проверяем условие выхода
                if (input.ToLower() == "exit")
                    break;

                string[] str_mas = input.Split(' ');
                int[] numbers = new int[3];
                for (int i = 0; i < 3; i++)
                {
                    numbers[i] = int.Parse(str_mas[i]);
                }

                // Отправляем сообщение
                byte[] data = new byte[3 * sizeof(int)];
                Buffer.BlockCopy(numbers, 0, data, 0, data.Length);  // Копируем числа в байтовый массив
                stream.Write(data, 0, data.Length);
                Console.WriteLine($"Отправлено: {input}\n");

                // Читаем ответ от сервера
                byte[] typeBuffer = new byte[sizeof(int)];
                int bytesRead = stream.Read(typeBuffer, 0, typeBuffer.Length);

                int type = BitConverter.ToInt32(typeBuffer, 0);
                if (type == -1)
                {
                    Console.WriteLine("Ошибка: уравнение линейное (a=0)");
                }
                else if (type == 0)
                {
                    Console.WriteLine("Нет действительных корней :(");
                }
                else if (type == 1)
                {
                    byte[] answerBuffer = new byte[sizeof(double)];
                    stream.Read(answerBuffer, 0, answerBuffer.Length);
                    double answer = BitConverter.ToDouble(answerBuffer, 0);
                    Console.WriteLine($"Один корень: {answer:F2}");
                }
                else
                {
                    byte[] answersBuffer = new byte[2 * sizeof(double)];
                    stream.Read(answersBuffer, 0, answersBuffer.Length);

                    double x1 = BitConverter.ToDouble(answersBuffer, 0);
                    double x2 = BitConverter.ToDouble(answersBuffer, sizeof(double));
                    Console.WriteLine($"Два корня: {x1}, {x2}");
                }
                break;
            }
            // Закрываем соединение
            stream.Close();
            client.Close();
            Console.WriteLine("Соединение закрыто");
            Console.WriteLine("###########################");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка клиента: {ex.Message}");
        }
    }
}