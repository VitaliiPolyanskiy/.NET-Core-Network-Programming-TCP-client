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
                // З'єднуємося з віддаленим пристроєм
                try
                {
                    IPAddress ipAddr = IPAddress.Parse(ip_address.Text);
                    // Встановлюємо віддалену кінцеву точку для сокета
                    // Унікальна адреса для обслуговування TCP/IP визначається комбінацією IP-адреси хоста з номером порту обслуговування
                    IPEndPoint ipEndPoint = new IPEndPoint(ipAddr /* IP-адреса */, 49152 /* Порт */);

                    // Створюємо потоковий сокет
                    sock = new Socket(AddressFamily.InterNetwork /*Схема адресації*/, SocketType.Stream /*Тип сокета*/, ProtocolType.Tcp /*Протокол*/);
                    /* Значення InterNetwork вказує на те, що при підключенні об'єкта Socket до кінцевої точки передбачається використання IPv4-адреси.
                       SocketType.Stream підтримує надійні двосторонні байтові потоки в режимі із встановленням підключення, без дублювання даних і 
                       без збереження меж даних. Об'єкт Socket цього типу взаємодіє з одним вузлом і потребує попереднього встановлення підключення 
                       до віддаленого вузла перед початком обміну даними. Тип Stream використовує протокол Tcp і схему адресації AddressFamily.
                    */

                    // З'єднуємо сокет із віддаленою кінцевою точкою
                    sock.Connect(ipEndPoint);
                    byte[] msg = Encoding.Default.GetBytes(Dns.GetHostName() /* Ім'я вузла локального комп'ютера */); // Конвертуємо рядок, що містить ім'я хоста, у масив байтів
                    int bytesSent = sock.Send(msg); // Відправляємо серверу повідомлення через сокет
                    MessageBox.Show("Клієнт " + Dns.GetHostName() + " встановив з'єднання з " + sock.RemoteEndPoint.ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Клієнт: " + ex.Message);
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
                    string theMessage = textBox1.Text; // Отримаємо текст повідомлення, введений у текстове поле
                    byte[] msg = Encoding.Default.GetBytes(theMessage); // Конвертуємо рядок, що містить повідомлення, у масив байтів
                    int bytesSent = sock.Send(msg); // Відправляємо серверу повідомлення через сокет
                    if (theMessage.IndexOf("<end>") > -1) // Якщо клієнт відправив цю команду, то приймаємо повідомлення від сервера
                    {
                        byte[] bytes = new byte[1024];
                        int bytesRec = sock.Receive(bytes); // Приймаємо дані, передані сервером. Якщо даних немає, потік блокується
                        MessageBox.Show("Сервер (" + sock.RemoteEndPoint.ToString() + ") відповів: " + Encoding.Default.GetString(bytes, 0, bytesRec) /*Конвертуємо масив байтів у рядок*/);
                        sock.Shutdown(SocketShutdown.Both); // Блокируємо передачу та отримання даних для об'єкта Socket.
                        sock.Close(); // Закриваємо сокет
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Клієнт: " + ex.Message);
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
                if (sock != null)
                {
                    sock.Shutdown(SocketShutdown.Both); // Блокируємо передачу та отримання даних для об'єкта Socket.
                    sock.Close(); // Закриваємо сокет
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Клієнт: " + ex.Message);
            }
        }
    }
}