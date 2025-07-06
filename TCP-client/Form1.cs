using System;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TCP_client
{
    public partial class Form1 : Form
    {
        Socket sock;
        public Form1()
        {
            InitializeComponent();
        }

        private async void Connect()
        {
            await Task.Run(() =>
            {
                // соединяемся с удаленным устройством
                try
                {

                    IPAddress ipAddr = IPAddress.Parse(ip_address.Text);
                    // устанавливаем удаленную конечную точку для сокета
                    // уникальный адрес для обслуживания TCP/IP определяется комбинацией IP-адреса хоста с номером порта обслуживания
                    IPEndPoint ipEndPoint = new IPEndPoint(ipAddr /* IP-адрес */, 49152 /* порт */);

                    // создаем потоковый сокет
                    sock = new Socket(AddressFamily.InterNetwork /*схема адресации*/, SocketType.Stream /*тип сокета*/, ProtocolType.Tcp /*протокол*/);
                    /* Значение InterNetwork указывает на то, что при подключении объекта Socket к конечной точке предполагается использование IPv4-адреса.
                      SocketType.Stream поддерживает надежные двусторонние байтовые потоки в режиме с установлением подключения, без дублирования данных и 
                      без сохранения границ данных. Объект Socket этого типа взаимодействует с одним узлом и требует предварительного установления подключения 
                      к удаленному узлу перед началом обмена данными. Тип Stream использует протокол Tcp и схему адресации AddressFamily.
                    */

                    // соединяем сокет с удаленной конечной точкой
                    sock.Connect(ipEndPoint);
                    byte[] msg = Encoding.Default.GetBytes(Dns.GetHostName() /* имя узла локального компьютера */);// конвертируем строку, содержащую имя хоста, в массив байтов
                    int bytesSent = sock.Send(msg); // отправляем серверу сообщение через сокет
                    MessageBox.Show("Клиент " + Dns.GetHostName() + " установил соединение с " + sock.RemoteEndPoint.ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Клиент: " + ex.Message);
                }
            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Connect();
        }

        private async void Exchange()
        {
            await Task.Run(() =>
            {
                try
                {
                    string theMessage = textBox1.Text; // получим текст сообщения, введенный в текстовое поле
                    byte[] msg = Encoding.Default.GetBytes(theMessage); // конвертируем строку, содержащую сообщение, в массив байтов
                    int bytesSent = sock.Send(msg); // отправляем серверу сообщение через сокет
                    if (theMessage.IndexOf("<end>") > -1) // если клиент отправил эту команду, то принимаем сообщение от сервера
                    {
                        byte[] bytes = new byte[1024];
                        int bytesRec = sock.Receive(bytes); // принимаем данные, переданные сервером. Если данных нет, поток блокируется
                        MessageBox.Show("Сервер (" + sock.RemoteEndPoint.ToString() + ") ответил: " + Encoding.Default.GetString(bytes, 0, bytesRec) /*конвертируем массив байтов в строку*/);
                        sock.Shutdown(SocketShutdown.Both); // Блокируем передачу и получение данных для объекта Socket.
                        sock.Close(); // закрываем сокет
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Клиент: " + ex.Message);
                }
            });
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Exchange();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                sock.Shutdown(SocketShutdown.Both); // Блокируем передачу и получение данных для объекта Socket.
                sock.Close(); // закрываем сокет
            }
            catch (Exception ex)
            {
                MessageBox.Show("Клиент: " + ex.Message);
            }
        }

    }
}
