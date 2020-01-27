using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;


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

        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        private void MainForm_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void cheatSign()
        {

        }

        void injectLibs()
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

    }
}