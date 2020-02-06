using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using skinchanger_loader.SDK.Api;
using skinchanger_loader.SDK.Api.Structs;
using skinchanger_loader.SDK.Device;
using skinchanger_loader.SDK.Win32;
using WebSocketSharp;
using xNet;


namespace skinchanger_loader
{
    public partial class MainWindow : Window
    {
        private bool _isDrag;

        private int _csgoPid;

        private string _hwid;
        private byte[] _dll;

        private bool _status;

        private DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();
            _timer = new DispatcherTimer(DispatcherPriority.Background);
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

        private void MainForm_Loaded(object sender, RoutedEventArgs e)
        {
            TVersion.Content += ClientData.VERSION;
            _hwid = HWID.GetSign();
            WebSocket ws = new WebSocket("wss://skinchanger.cc/api/software_v2/");
            ServerRequest<AuthData> sr = new ServerRequest<AuthData>()
            {
                data = new AuthData { hwid = _hwid },
                type = "demoConnect"
            };


            ws.OnError += (o, argc) =>
            {
                startStatus(false, "Ошибка");
            };

            ws.OnMessage += (o, args) =>
            {
                JObject obj = JObject.Parse(args.Data);
                switch (obj["type"].ToString())
                {
                    case "demoConnect":
                        switch (obj["status"].ToObject<ServerCodes>())
                        {
                            case ServerCodes.API_CODE_OK:
                                if (obj["data"]["version"].ToObject<int>() > ClientData.LAST_UPDATE)
                                {
                                    showMessage("Вышло новое обновление! Загрузите новую версию с нашего сайта.", "Скачать", "open_url", ClientData.SKINCHANGER_CC);
                                    startStatus(false, "Вышло обновление");
                                    return;
                                }

                                Dispatcher?.Invoke(() =>
                                {
                                    TOnline.Content = obj["data"]["online"].ToString();
                                    BStartButton.IsEnabled = true;
                                    checkSkinChangerStarted();
                                });
                               

                                break;

                            case ServerCodes.API_CODE_PROHIBDED:
                                checkSkinChangerStarted();
                                break;
                        }
                        break;

                    case "online":
                        Dispatcher?.Invoke(() => { TOnline.Content = obj["data"]["online"].ToString(); });
                        break;

                    case "update":
                        showMessage("Вышло новое обновление! Загрузите новую версию с нашего сайта.", "Скачать", "open_url", ClientData.SKINCHANGER_CC);
                        startStatus(false, "Вышло обновление");
                        break;
                }
            };

            ws.Connect();
            ws.Send(JsonConvert.SerializeObject(sr));

            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (o, args) =>
            {
                IntPtr hWnd = NativeMethods.FindWindowA(IntPtr.Zero, "Counter-Strike: Global Offensive");
                if (hWnd == IntPtr.Zero)
                {
                    _status = false;
                    Dispatcher?.Invoke(() =>
                    {
                        startStatus(true, "Запустить");
                    });
                }
            };
        }

        private void cheatSign()
        {
            if(!_status)
                return;

            using (LocalProto proto = new LocalProto(1338))
            {
                LocalRequest lreq = new LocalRequest
                {
                    hwid =  _hwid
                };
                proto.SendJson(lreq);
                DllResponse rsp = proto.ReciveJson<DllResponse>();
                if (rsp.salt.Length == 0)
                {
                    _status = false;
                    showMessage("Ошибка при обмене данных. Обратитесь в поддержку.", "Обратиться", "open_url", ClientData.SUPPORT_URL);
                }
            }
        }

        void injectLibs()
        {
            if(!_status)
                return;

            try
            {
                if (!Injector.ManualMapInject(_dll.Length, _dll, _csgoPid, 0, false))
                {
                    throw new Exception();
                }

            }
            catch
            {
                _status = false;
                showMessage("Ошибка запуска. Обратитесь в поддержку.", "Обратиться", "open_url", ClientData.SUPPORT_URL);
            }
            finally
            {
                Array.Clear(_dll, 0, _dll.Length);
                _dll = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

        }

        void downloadDll()
        {
            if(!_status)
                return;

            using (HttpRequest request = new HttpRequest())
            {
                RequestParams data = new RequestParams();
                data["hwid"] = _hwid;

                _dll = request.Post("https://skinchanger.cc/api/software_v2/request_dll", data).ToBytes();

                if (_dll.Length == 0)
                {
                    _status = false;
                    showMessage("Ошибка при загрузке библиотеки. Обратитесь в поддержку.", "Обратиться", "open_url", ClientData.SUPPORT_URL);
                }
            }
        }

        void runCsGo()
        {
            try
            {
                if (Process.GetProcessesByName("csgo").Length == 0)
                {
                    Process.Start("steam://rungameid/730");
                    while (Process.GetProcessesByName("csgo").Length == 0)
                    {
                        Thread.Sleep(6000);
                    }

                    Thread.Sleep(6000);
                }
                _csgoPid = Process.GetProcessesByName("csgo")[0].Id;
                _status = true;
            }
            catch
            {
                //
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


        private void showMessage(string message, string actionText, string actionType, string url = "")
        {
            Dispatcher?.Invoke(() =>
            {
                InfoMessage.Tag = url;
                InfoMessage_Text.Tag = actionType;
                InfoMessage_Text.ActionContent = actionText;
                InfoMessage_Text.Content = message;
                InfoMessage.IsActive = true;
            });
        }

        private async void BStartButton_Click(object sender, RoutedEventArgs e)
        {
            showMessage("Происходит запуск, подождите...", "Закрыть", "close");
            BStartButton.IsEnabled = false;
            await Task.Run(runCsGo);
            await Task.Run(downloadDll);
            await Task.Run(injectLibs);
            await Task.Run(cheatSign);
            startStatus(false, "Запущено");
            _timer.Start();
        }


        private void startStatus(bool status, string message)
        {
            BStartButton.IsEnabled = status;
            BStartButton.Content = message;
            BStartButton.Background = status ? new SolidColorBrush(Color.FromRgb(255, 204, 64))  : new SolidColorBrush(Color.FromRgb(45, 50, 90));
        }

        private void checkSkinChangerStarted()
        {
            Dispatcher?.Invoke(() =>
            {
                IntPtr hWnd = NativeMethods.FindWindowA(IntPtr.Zero, "Counter-Strike: Global Offensive");
                if (hWnd != IntPtr.Zero)
                {
                    IntPtr result = NativeMethods.SendMessage(hWnd, 0, IntPtr.Zero, (IntPtr)0x103);
                    bool bResult = (result == (IntPtr) 0x505 || result == (IntPtr) 0x504);
                    startStatus(!bResult, bResult ? "Запущено" : "Запустить");
                }
            });
        }

        private void TMainLink_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start(ClientData.SKINCHANGER_CC);
        }

        private void BFaqButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(ClientData.SKINCHANGER_CC);
        }
    }
}