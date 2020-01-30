using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using skinchanger_loader.SDK.Api;
using skinchanger_loader.SDK.Api.Structs;
using skinchanger_loader.SDK.Device;
using WebSocketSharp;


namespace skinchanger_loader
{
    public partial class MainWindow : Window
    {
        private bool _isDrag;

        private int _csgoPid;

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
            WebSocket ws = new WebSocket("wss://skinchanger.cc/api/software_v2/");
            ServerRequest<AuthData> sr = new ServerRequest<AuthData>()
            {
                data = new AuthData { hwid = HWID.GetSign() },
                type = "demoConnect"
            };

            ws.OnError += (o, args) =>
            {
                showMessage($"WS Error: {args.Message}", Color.FromRgb(255, 0, 0), "Закрыть", "message");
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
                                    Dispatcher?.Invoke(() =>
                                    {
                                        showMessage("Вышло новое обновление!\r\nВышло новое обновление!", Color.FromRgb(255, 0, 0), "Закрыть",
                                            "message");
                                    });
                                    return;
                                }

                                Dispatcher?.Invoke(() =>
                                {
                                    TOnline.Content = obj["data"]["online"].ToString();
                                    BStartButton.IsEnabled = true;
                                });
                               

                                break;

                            case ServerCodes.API_CODE_PROHIBDED:
                                Dispatcher?.Invoke(() =>
                                {
                                    showMessage("Отказ от авторизации!", Color.FromRgb(255, 0, 0), "Закрыть", "message");
                                });
                                break;
                        }
                        break;

                    case "online":
                        Dispatcher?.Invoke(() => { TOnline.Content = obj["data"]["online"].ToString(); });
                        break;

                    case "update":
                        showMessage("Вышло новое обновление!\r\nВышло новое обновление!", Color.FromRgb(255, 0, 0), "Закрыть",
                            "message");
                        break;
                }
            };

            ws.Connect();
            ws.Send(JsonConvert.SerializeObject(sr));
        }

        private void cheatSign()
        {
            using (LocalProto proto = new LocalProto(1358))
            {


            }
        }

        void injectLibs()
        {

        }

        void downloadDll()
        {

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


        private void InfoMessage_Text_ActionClick(object sender, RoutedEventArgs e)
        {
            if (InfoMessage_Text.Tag.ToString() == "open_url")
            {
                Process.Start(InfoMessage.Tag.ToString());
                return;
            }

            InfoMessage.IsActive = false;

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

        private async void BStartButton_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(downloadDll);
        }
    }
}