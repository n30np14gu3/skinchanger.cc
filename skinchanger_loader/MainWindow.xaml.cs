using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Newtonsoft.Json;
using skinchanger_loader.SDK.Api.Structs;
using skinchanger_loader.SDK.Device;
using skinchanger_loader.SDK.extensions;
using skinchanger_loader.SDK.Win32;
using xNet;


namespace skinchanger_loader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isDrag;

        private bool _injected;
        private bool _isEnabled;
        private bool _status;

        private string _hwid;

        private byte[] _dll;
        private int _csgoPid;

        private Thread _pingThread;
        private Thread _validThread;


        public MainWindow()
        {
            _hwid = HWID.GetSign();
            InitializeComponent();
        }

        private void DragHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDrag = true;
            DragMove();
        }

        private void DragHeader_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDrag = false;
        }

        private void DragHeader_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrag && WindowState == WindowState.Maximized)
            {
                _isDrag = false;
                WindowState = WindowState.Normal;
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void MinBtn_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        private void MainForm_Loaded(object sender, RoutedEventArgs e)
        {
            LogoImage.Source = ((System.Drawing.Image) Properties.Resources.header_image).ToBitmapSource();
            BStartButton.IsEnabled = false;
            try
            {
                using (HttpRequest request = new HttpRequest {IgnoreProtocolErrors = true})
                {
                    Tonline.Text = "0";
                    RequestParams data = new RequestParams();
                    data["hwid"] = _hwid;
                    string rsp = request.Post("https://skinchanger.cc/api/software/auth", data).ToString();
                    ServerResponse updateInfo = JsonConvert.DeserializeObject<ServerResponse>(rsp);
                    switch (updateInfo.status)
                    {
                        case ServerCodes.API_CODE_PROHIBDED:
                            throw new NullReferenceException("Какие-то проблемы. Обратитесь в техническую поддержку.");

                        case ServerCodes.API_CODE_OK:
                            if (UnixTimeStampToDateTime(updateInfo.last_update) > ClientData.LastUpdate)
                                throw new Exception("Вышло новое обновление! Загрузите новую версию с нашего сайта.");

                            ClientData.Info = updateInfo;
                            Tonline.Text = ClientData.Info.online.ToString();
                            _pingThread = new Thread(ping) {IsBackground = true, Priority = ThreadPriority.Lowest};
                            _pingThread.Start();
                            BStartButton.IsEnabled = true;

                            IntPtr hwnd = NativeMethods.FindWindowA(IntPtr.Zero, "Counter-Strike: Global Offensive");
                            if (hwnd != IntPtr.Zero)
                            {
                                IntPtr lResult = NativeMethods.SendMessage(hwnd, 0, IntPtr.Zero, (IntPtr) 0x103);
                                if (lResult == (IntPtr) 0x505)
                                {
                                    _injected = true;
                                    _isEnabled = true;
                                    BStartButton.Content = "Выключить";
                                    return;
                                }

                                if (lResult == (IntPtr) 0x504)
                                {
                                    _injected = true;
                                    _isEnabled = false;
                                    BStartButton.Content = "Включить";
                                }
                            }
                            Tonline.Text = updateInfo.ToString();

                            break;
                    }

                }
            }
            catch (NullReferenceException ex)
            {
                showMessage(ex.Message, Color.FromArgb(255, 221, 28, 28), "Обратиться", "open_url", "https://vk.me/skinchangercc");
            }
            catch (JsonException)
            {
                showMessage("Не удалось проверить обновления. Попробуйте ещё раз...", Color.FromArgb(255, 221, 28, 28), "Закрыть", "action_close");
            }
            catch (HttpException)
            {
                showMessage("Не удалось проверить обновления. Попробуйте ещё раз...", Color.FromArgb(255, 221, 28, 28), "Закрыть", "action_close");
            }
            
            catch (Exception ex)
            {
                showMessage(ex.Message, Color.FromArgb(255, 221, 28, 28), "Загрузить", "open_url", "https://skinchanger.cc");
            }
        }

        private void WebLink_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://skinchanger.cc");
        }

        private void BPreorder_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://skinchanger.cc");
        }

        private void cheatSign()
        {
            _status = false;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            int port = 1358;
            TcpListener server = new TcpListener(localAddr, port);
            server.Start();

            Socket socket = server.AcceptSocket();

            IntPtr pBuf;

            byte[] enc = new byte[72];
            SERVER_RESPONSE response = new SERVER_RESPONSE();
            response.access_token = Encoding.ASCII.GetBytes(_hwid + "\0");
            pBuf = Marshal.AllocHGlobal(72);
            Marshal.StructureToPtr(response, pBuf, true);
            Marshal.Copy(pBuf, enc, 0, 72);
            Marshal.FreeHGlobal(pBuf);

            for (int i = 0; i < enc.Length; i++)
                enc[i] ^= 0xAC;

            socket.Send(BitConverter.GetBytes(enc.Length));
            socket.Send(enc);
            Thread.Sleep(2000);
            socket.Close();
            server.Stop();
            _status = true;
        }

        void injectLibs()
        {
            try
            {
                if (!Injector.ManualMapInject(_csgoPid, _dll))
                    throw new Exception("Не удалось запустить Skinchanger! Попробуйте ещё раз...");
            }
            catch (Exception ex)
            {
                showMessage(ex.Message, Color.FromArgb(255, 221, 28, 28), "Закрыть", "action_close");
                _status = false;
            }
            finally
            {
                Array.Clear(_dll, 0, _dll.Length);
            }
        }

        void runCsGo()
        {
            repeat:
            try
            {
                if (Process.GetProcessesByName("csgo").Length == 0)
                {
                    Process.Start("steam://rungameid/730");
                    while (Process.GetProcessesByName("csgo").Length == 0)
                    {
                    }
                }

                Thread.Sleep(6000);
                kek:
                int loadedModules = 0;
                foreach (ProcessModule module in Process.GetProcessesByName("csgo")[0].Modules)
                {
                    if (module.ModuleName == "client_panorama.dll")
                        loadedModules++;

                    if (module.ModuleName == "engine.dll")
                        loadedModules++;

                    if (module.ModuleName == "server.dll")
                        loadedModules++;
                }

                if (loadedModules != 3)
                    goto kek;

                Thread.Sleep(10000);
                _csgoPid = Process.GetProcessesByName("csgo")[0].Id;
            }
            catch
            {
                goto repeat;
            }
        }

        private async void BStartButton_Click(object sender, RoutedEventArgs e)
        {
            _validThread?.Abort();

            if (!_injected)
            {
                BStartButton.IsEnabled = false;
                showMessage("Происходит запуск, подождите...", Color.FromArgb(255, 40, 167, 69), "Закрыть", "");
                await Task.Run(() => runCsGo());
#if !DEBUG
                await Task.Run(() => downloadLibs());
                if(!_status)
                    return;
#else
                _status = true;
                _dll = File.ReadAllBytes("test.dll");
#endif
                await Task.Run(() => injectLibs());
                if (!_status)
                    return;
                await Task.Run(() => cheatSign());
                if (!_status)
                    return;

                BStartButton.IsEnabled = false;
                _injected = true;
                BStartButton.IsEnabled = true;
                _isEnabled = true;
                BStartButton.Content = "Выключить";
                _validThread = new Thread(validation) {IsBackground = true, Priority = ThreadPriority.Lowest};
                _validThread.Start();
                showMessage("Skinchanger успешно запущен!", Color.FromArgb(255, 40, 167, 69), "Закрыть", "");
            }
            else
            {
                _isEnabled = !_isEnabled;

                
                IntPtr hwnd = NativeMethods.FindWindowA(IntPtr.Zero, "Counter-Strike: Global Offensive");
                if (hwnd != IntPtr.Zero)
                {
                    BStartButton.Content = _isEnabled ? "Выключить" : "Включить";
                    IntPtr comand = _isEnabled ? (IntPtr) 0x102 : (IntPtr) 0x101;
                    NativeMethods.SendMessage(hwnd, 0, IntPtr.Zero, comand);
                    showMessage($"Вы {(_isEnabled ? "включили" : "выключили")} Skinchanger", _isEnabled ? Color.FromArgb(255, 40, 167, 69) : Color.FromArgb(255, 221, 28, 28), "Закрыть", "");
                }
                else
                {
                    _isEnabled = false;
                    _injected = false;
                }
            }

            
        }

        void downloadLibs()
        {
            try
            {
                using (HttpRequest request = new HttpRequest { IgnoreProtocolErrors = true })
                {
                    RequestParams data = new RequestParams();
                    data["hwid"] = _hwid;

                    _dll = request.Post("https://skinchanger.cc/api/software/request_dll", data).ToBytes();
                    if (_dll.Length == 0)
                        throw new Exception("");

                    _status = true;
                }
            }
            catch
            {
                showMessage("Ошибка подключения. Попробуйте ещё раз...", Color.FromArgb(255, 221, 28, 28), "Закрыть", "action_close");
                _status = false;
            }
        }

        private void ping()
        {
            while (true)
            {
                try
                {
                    using (HttpRequest request = new HttpRequest())
                    {
                        RequestParams data = new RequestParams();
                        data["hwid"] = _hwid;
                        ServerResponse responce = JsonConvert.DeserializeObject<ServerResponse>(request.Post("https://skinchanger.cc/api/software/ping", data).ToString());
                        if (responce.status == ServerCodes.API_CODE_OK_PING)
                            Dispatcher.Invoke(() => { Tonline.Text = responce.online.ToString(); });
                    }
                }
                catch
                {
                    //
                }
                Thread.Sleep(30000);
            }
        }

        private void InfoMessage_Text_ActionClick(object sender, RoutedEventArgs e)
        {
            if (InfoMessage_Text.Tag.ToString() == "open_url")
            {
                Process.Start(InfoMessage.Tag.ToString());
                return;
            }

            InfoMessage.IsActive = false;


        }

        void validation()
        {
            try
            {
                while (true)
                {
                    IntPtr check = NativeMethods.FindWindowA(IntPtr.Zero, "Counter-Strike: Global Offensive");
                    if (check == IntPtr.Zero)
                    {
                        _isEnabled = false;
                        _injected = false;
                        _status = false;
                        Dispatcher.Invoke(() => { BStartButton.Content = "Запустить Skinchanger"; });
                        _validThread?.Abort();
                        _validThread = null;
                        break;
                    }

                    Thread.Sleep(500);
                }
            }
            catch
            {
                //
            }
        }

        private void showMessage(string message, Color backColor, string actionText, string actionType, string url = "")
        {
            InfoMessage.Background = new SolidColorBrush(backColor);
            InfoMessage.Tag = url;
            InfoMessage_Text.Tag = actionType;
            InfoMessage_Text.ActionContent = actionText;
            InfoMessage_Text.Content = message;
            InfoMessage.IsActive = true;
        }
    }
}