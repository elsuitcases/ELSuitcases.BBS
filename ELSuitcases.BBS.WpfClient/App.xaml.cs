using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using ELSuitcases.BBS.Library;
using ELSuitcases.BBS.WpfClient.Messages;

namespace ELSuitcases.BBS.WpfClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static readonly ObservableCollection<Window> ReaderWindows = new ObservableCollection<Window>();
        
        internal static Uri? APIServerBaseURL { get; set; }

        internal static Uri? APIServerURL_Article
        {
            get
            {
                if (APIServerBaseURL != null)
                    return new Uri(APIServerBaseURL.ToString() + "/article");
                else 
                    return null;
            }
        }

        internal static Uri? APIServerURL_Board
        {
            get
            {
                if (APIServerBaseURL != null)
                    return new Uri(APIServerBaseURL.ToString() + "/board");
                else
                    return null;
            }
        }

        internal static UserDTO? CurrentUser { get; private set; }
        internal static Ioc IocServices => Ioc.Default;



        public App()
        {
            Process.GetCurrentProcess().Exited += App_Process_Exited;
            this.Startup += App_Startup;
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            this.Exit += App_Exit;
        }

        private void App_Process_Exited(object? sender, EventArgs e)
        {
            Common.PrintDebugInfo("앱 프로세스가 종료되었습니다.");
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            Common.PrintDebugInfo("앱이 종료됩니다.");
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Common.PrintDebugInfo("앱 오류가 발생하였습니다 : ");
            Common.PrintDebugFail(e.Exception);

            App.ShowMessageBoxError("오류!\r\n\r\n불편을 드려 대단히 죄송합니다.\r\n앱 오류가 발생하였습니다.", e.Exception);
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            Common.PrintDebugInfo("앱이 시작되었습니다.");

            LoadConfig().Wait();
            BuildIocServices();

            var vmLogin = IocServices.GetRequiredService<LoginViewModel>();
            SetMainWindowShell(vmLogin, "사용자 로그인").Show();
        }



        #region [MESSAGE BOX]
        private static MessageBoxResult ShowMessageBox(
            MessageBoxImage messageBoxImage, string message, string title = Constants.APPLICATION_NAME, MessageBoxButton messageBoxButton = MessageBoxButton.OK)
        {
            MessageBoxResult result = MessageBoxResult.None;
            Application.Current.Dispatcher.Invoke(() =>
            {
                result = MessageBox.Show(message, title, messageBoxButton, messageBoxImage);
            });
            return result;
        }

        internal static MessageBoxResult ShowMessageBoxError(string message, Exception? error, string title = Constants.APPLICATION_NAME)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(message);
            if (error != null)
            {
                sb.AppendLine(string.Format("- 오류 : {0}", @error.GetType().Name));
                sb.AppendLine(string.Format("- 오류 내용 : {0}", @error.Message));
            }

            return ShowMessageBox(MessageBoxImage.Error, sb.ToString(), title);
        }

        internal static MessageBoxResult ShowMessageBoxExclamation(string message, string title = Constants.APPLICATION_NAME)
        {
            return ShowMessageBox(MessageBoxImage.Exclamation, message, title);
        }

        internal static MessageBoxResult ShowMessageBoxInformation(string message, string title = Constants.APPLICATION_NAME)
        {
            return ShowMessageBox(MessageBoxImage.Information, message, title);
        }

        internal static MessageBoxResult ShowMessageBoxQuestion(string message, string title = Constants.APPLICATION_NAME)
        {
            return ShowMessageBox(MessageBoxImage.Question, message, title, MessageBoxButton.YesNo);
        }
        #endregion

        private static void BuildIocServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton<MainShellWindow>();
            services.AddSingleton<MainShellViewModel>();
            services.AddSingleton<HomeViewModel>();
            services.AddSingleton<MainTopViewModel>();
            services.AddSingleton<SettingViewModel>();

            services.AddTransient<AttachedFileItem>();
            services.AddTransient<BoardCreateViewModel>();
            services.AddTransient<BoardDetailViewModel>();
            services.AddTransient<BoardListViewModel>();
            services.AddTransient<FileTransferProgress>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<ReaderViewModel>();
            services.AddTransient<WriterViewModel>();

            Ioc.Default.ConfigureServices(services.BuildServiceProvider());
        }

        internal static void ChangeBusyIndicatorState(object? sender, bool isBusy, string busyMessage)
        {
            Application.Current.Dispatcher.Invoke((() =>
            {
                WeakReferenceMessenger.Default.Send(new BusyMessage(sender, isBusy, busyMessage));
            }), System.Windows.Threading.DispatcherPriority.Render);
        }

        internal static Task LoadConfig()
        {
            return Task.Factory.StartNew(() =>
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if ((config == null) || (config.AppSettings.Settings.Count < 1))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("환경설정 파일이 존재하지 않습니다.\r\n홈 화면에서 설정 후 저장 하십시오.", Constants.APPLICATION_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
                else
                {
                    string hostname = "", port = "", proto = "", subpath = "";

                    hostname = config.AppSettings.Settings[Constants.CONFIG_KEY_NAME_API_HOSTNAME].Value ?? string.Empty;
                    port = config.AppSettings.Settings[Constants.CONFIG_KEY_NAME_API_PORT].Value ?? "80";
                    proto = config.AppSettings.Settings[Constants.CONFIG_KEY_NAME_API_PROTOCOL].Value ?? string.Empty;
                    subpath = ((!string.IsNullOrEmpty(config.AppSettings.Settings[Constants.CONFIG_KEY_NAME_API_SUB_PATH].Value)) && 
                               (!config.AppSettings.Settings[Constants.CONFIG_KEY_NAME_API_SUB_PATH].Value.StartsWith("/"))) ? 
                                    "/" + config.AppSettings.Settings[Constants.CONFIG_KEY_NAME_API_SUB_PATH].Value : 
                                    config.AppSettings.Settings[Constants.CONFIG_KEY_NAME_API_SUB_PATH].Value;

                    APIServerBaseURL = new Uri(string.Format("{0}://{1}:{2}{3}", proto, hostname, port, subpath), UriKind.Absolute);
                }
            });
        }

        internal static void ShowFirstHome()
        {
            var vmHome = IocServices.GetRequiredService<HomeViewModel>();
            vmHome.ViewModelOfSetting = IocServices.GetRequiredService<SettingViewModel>();

            var winMain = SetMainWindowShell(vmHome, Constants.APPLICATION_NAME);
            if (winMain == null) return;
            winMain.ShowInTaskbar = true;
            winMain.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            winMain.WindowState = WindowState.Maximized;
            winMain.BorderThickness = new Thickness(2);
            winMain.BorderBrush = Brushes.LightGray;
            winMain.MinWidth = 1024;
            winMain.MinHeight = 600;
        }

        internal static string GetCurrentUserID()
        {
            return CurrentUser?.GetString(Constants.PROPERTY_KEY_NAME_CURRENT_USER_ACCOUNT_ID, null) ?? "GUEST" + Common.Generate16IdentityCode(DateTime.Now, new Random());
        }

        internal static DTO? SetCurrentUser(string accountId, string fullName = "")
        {
            if (string.IsNullOrWhiteSpace(accountId))
                CurrentUser = null;
            else
                CurrentUser = new UserDTO(accountId, (!string.IsNullOrEmpty(fullName)) ? fullName : accountId);

            return CurrentUser;
        }

        internal static Window SetMainWindowShell(ViewModelBase? viewModel, string title = "")
        {
            var shell = IocServices.GetRequiredService<MainShellWindow>();
            shell.DataContext = IocServices.GetRequiredService<MainShellViewModel>();
            shell.DataContext.Title = (!string.IsNullOrEmpty(title)) ? title : Constants.APPLICATION_NAME;
            shell.DataContext.ViewModel = viewModel;

            Application.Current.MainWindow = shell;

            return Application.Current.MainWindow;
        }
    }
}
