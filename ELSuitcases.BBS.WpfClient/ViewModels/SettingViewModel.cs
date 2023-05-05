using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Configuration;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ELSuitcases.BBS.Library;

namespace ELSuitcases.BBS.WpfClient
{
    internal partial class SettingViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _ApiServerHostName = string.Empty;

        [ObservableProperty]
        private int _ApiServerPort = 80;

        [ObservableProperty]
        private string _ApiServerSubPath = string.Empty;

        [ObservableProperty]
        private bool _IsLoading = false;

        [ObservableProperty]
        private bool _IsSaving = false;

        public string ApiServerURI
        {
            get => string.Format("http://{0}:{1}{2}", ApiServerHostName, ApiServerPort, ApiServerSubPath);
        }

        #region [COMMAND]
        [ObservableProperty]
        private ICommand? _ViewLoadedCommand;

        [ObservableProperty]
        private ICommand? _SaveCommand;
        #endregion



        public SettingViewModel() : base()
        {
            _ViewLoadedCommand = new AsyncRelayCommand<RoutedEventArgs>(ViewLoadedCommandAction);
            _SaveCommand = new AsyncRelayCommand<RoutedEventArgs>(SaveCommandAction);
        }



        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (!string.IsNullOrEmpty(e.PropertyName))
            {
                switch (e.PropertyName)
                {
                    case nameof(ApiServerHostName):
                    case nameof(ApiServerPort):
                    case nameof(ApiServerSubPath):
                        OnPropertyChanged(nameof(ApiServerURI));
                        break;

                    case nameof(IsLoading):
                        //WeakReferenceMessenger.Default.Send(new Messages.BusyMessage(this, IsLoading, "설정을 불러오는 중 ..."));
                        break;

                    case nameof(IsSaving):
                        WeakReferenceMessenger.Default.Send(new Messages.BusyMessage(this, IsSaving, "설정을 저장하는 중 ..."));
                        break;
                }
            }
        }

        private async Task ViewLoadedCommandAction(RoutedEventArgs? args)
        {
            IsLoading = true;

            await App.LoadConfig()
                        .ContinueWith((t) =>
                        {
                            if (App.APIServerBaseURL != null)
                            {
                                ApiServerHostName = App.APIServerBaseURL.Host;
                                ApiServerPort = App.APIServerBaseURL.Port;
                                ApiServerSubPath = App.APIServerBaseURL.LocalPath;
                            }

                            IsLoading = false;
                        });
        }

        private async Task SaveCommandAction(RoutedEventArgs? args)
        {
            await Task.Factory.StartNew(() =>
            {
                IsSaving = true;

                DTO dtoConfig = new DTO
                {
                    { Constants.CONFIG_KEY_NAME_API_PROTOCOL, "http" },
                    { Constants.CONFIG_KEY_NAME_API_HOSTNAME, ApiServerHostName },
                    { Constants.CONFIG_KEY_NAME_API_PORT, ApiServerPort.ToString() },
                    { Constants.CONFIG_KEY_NAME_API_SUB_PATH, ((!string.IsNullOrEmpty(ApiServerSubPath)) && (!ApiServerSubPath.StartsWith("/"))) ? "/" + ApiServerSubPath : ApiServerSubPath }
                };

                try
                {
                    var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                    if (config == null)
                    {
                        App.ShowMessageBoxError("환경설정 파일이 존재하지 않습니다.", null);
                    }
                    else
                    {
                        foreach (string key in dtoConfig.Keys)
                        {
                            if (config.AppSettings.Settings.AllKeys.Contains(key))
                                config.AppSettings.Settings.Remove(key);

                            config.AppSettings.Settings.Add(key, dtoConfig[key]?.ToString() ?? "");
                        }
                        
                        config.Save(ConfigurationSaveMode.Modified);

                        App.LoadConfig();
                        App.ShowMessageBoxInformation("환경설정 파일을 저장하였습니다.");
                    }
                }
                catch (Exception ex)
                {
                    App.ShowMessageBoxError("환경설정 파일 저장 오류가 발생하였습니다.", ex);
                }
            })
            .ContinueWith((t) =>
            {
                IsSaving = false;
            });
        }
    }
}
