﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
//подключение библиотек для работы с сетью и потоками
using System.Net;
using System.Net.Sockets;
using System.Threading;
//?
using System.Xml.Linq;
using System.Windows.Interop;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //номер порта для обмена сообщениями
        int port = 8888;
        //ip адрес сервера
        string address = "127.0.0.1";
        //объявление TCP клиента
        TcpClient client = null;
        //объявление канала соединения с сервером
        NetworkStream stream = null;
        //Main поток клиента
        Thread MainListenThread = null;
        //имя пользователя
        string userName = "";

        public MainWindow()
        {
            InitializeComponent();
            ClientLog.Items.Add("Подключение отсутствует");
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            //получение имени пользователя
            userName = uName.Text;
            try //если возникнет ошибка - переход в catch
            {
                //создание клиента
                client = new TcpClient(address, port);
                //получение канала для обмена сообщениями
                stream = client.GetStream();

                ClientLog.Items.Add("Есть подключение");

                //получение сообщения
                string message = userName;
                //преобразование сообщение в массив байтов
                byte[] data = Encoding.Unicode.GetBytes(message);
                //отправка сообщения
                stream.Write(data, 0, data.Length);

                //создание нового потока для ожидания и подключения клиентов
                MainListenThread = new Thread(() => listen());
                MainListenThread.Start();
            }
            catch (Exception ex)
            {
                ClientLog.Items.Add(ex.Message);
            }
        }
        //функция ожидания сообщений от сервера
        void listen()
        {
            try //в случае возникновения ошибки - переход к catch
            {
                //цикл ожидания сообщениями
                while (true)
                {
                    //буфер для получаемых данных
                    byte[] data = new byte[64];
                    //объект для построения строк
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    //до тех пор, пока есть данные в потоке
                    do
                    {
                        //получение 64 байт
                        bytes = stream.Read(data, 0, data.Length);
                        //формирование строки
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    //пока есть доступные данные в потоке
                    while (stream.DataAvailable);
                    //получить строку
                    string message = builder.ToString();
                    //вывод сообщения в лог клиента
                    Dispatcher.BeginInvoke(new Action(() => ClientLog.Items.Add(message)));
                }
            }
            catch (Exception ex)
            {
                //вывести сообщение об ошибке
                Dispatcher.BeginInvoke(new Action(() => ClientLog.Items.Add(ex.Message)));
            }
            finally
            {
                //закрыть канал связи и завершить работу клиента
                stream.Close();
                client.Close();
            }
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //получение сообщения
                string message = msg.Text;
                //добавление имени пользователя к сообщению
                message = String.Format("{0}: {1}", userName, message);
                //преобразование сообщение в массив байтов
                byte[] data = Encoding.Unicode.GetBytes(message);
                //отправка сообщения
                stream.Write(data, 0, data.Length);
            }
            catch(Exception ex)
            {
                //вывести сообщение об ошибке
                Dispatcher.BeginInvoke(new Action(() => ClientLog.Items.Add(ex.Message)));
            }
            
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            //Условие чтобы не вылетала ошибка
            if (client != null)
            {
                MainListenThread.Abort();
                ClientLog.Items.Add("Подключение отсутствует");
                //int jopa = userName.GetHashCode();
                //ClientLog.Items.Add(jopa);
            }
        }
    }
}
