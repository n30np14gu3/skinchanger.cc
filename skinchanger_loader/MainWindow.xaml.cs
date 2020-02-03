using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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

        public MainWindow()
        {
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

        private void MainForm_Loaded(object sender, RoutedEventArgs e)
        {
            checkSkinChangerStarted();
            _hwid = HWID.GetSign();
            WebSocket ws = new WebSocket("wss://skinchanger.cc/api/software_v2/");
            ServerRequest<AuthData> sr = new ServerRequest<AuthData>()
            {
                data = new AuthData { hwid = _hwid },
                type = "demoConnect"
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
                                    return;
                                }

                                Dispatcher?.Invoke(() =>
                                {
                                    TOnline.Content = obj["data"]["online"].ToString();
                                    BStartButton.IsEnabled = true;
                                });
                               

                                break;

                            case ServerCodes.API_CODE_PROHIBDED:
                                break;
                        }
                        break;

                    case "online":
                        Dispatcher?.Invoke(() => { TOnline.Content = obj["data"]["online"].ToString(); });
                        break;

                    case "update":
                        showMessage("Вышло новое обновление! Загрузите новую версию с нашего сайта.", "Скачать", "open_url", ClientData.SKINCHANGER_CC);
                        break;
                }
            };

            ws.Connect();
            ws.Send(JsonConvert.SerializeObject(sr));
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
                if (rsp.result == 0)
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
            await Task.Run(runCsGo);
            await Task.Run(downloadDll);
            await Task.Run(injectLibs);
            await Task.Run(cheatSign);
            if (!_status)
            {
                await Task.Delay(3000);
                showMessage("Не удалось запустить skinchanger", "Понятно", "message");
            }
            else
                startStatus(false);
        }


        private void startStatus(bool status)
        {
            BStartButton.IsEnabled = status;
            BStartButton.Content = status ? "Запустить Skinchanger" : "Skinchanger запущен";
        }

        private void checkSkinChangerStarted()
        {
            Dispatcher?.Invoke(() =>
            {
                IntPtr hWnd = NativeMethods.FindWindowA(IntPtr.Zero, "Counter-Strike: Global Offensive");
                if (hWnd != IntPtr.Zero)
                {
                    IntPtr result = NativeMethods.SendMessage(hWnd, 0, IntPtr.Zero, (IntPtr)0x103);
                    startStatus(!(result == (IntPtr)0x505 || result == (IntPtr)0x504));
                }
            });
        }

        private void TMainLink_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start(ClientData.SKINCHANGER_CC);
        }

        private void TFaq_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start(ClientData.SKINCHANGER_CC);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(ClientData.SKINCHANGER_CC);
        }
    }
}