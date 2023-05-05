using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ELSuitcases.BBS.Library;

namespace ELSuitcases.BBS.WpfClient
{
    internal partial class LoginViewModel : ViewModelBase
    {
        private TextBox? _txtUserName;



        public string ApiServerURI
        {
            get => App.APIServerBaseURL?.ToString() ?? string.Empty;
        }

        [ObservableProperty]
        private bool _IsLoggingIn = false;

        [ObservableProperty]
        private string _UserName = string.Empty;

        #region [COMMAND]
        [ObservableProperty]
        private ICommand _ViewLoadedCommand;

        [ObservableProperty]
        private ICommand _LoginCommand;
        #endregion



        public LoginViewModel() : base()
        {
            _ViewLoadedCommand = new RelayCommand<RoutedEventArgs>(ViewLoadedCommandAction);
            _LoginCommand = new AsyncRelayCommand<RoutedEventArgs>(LoginCommandAction);
        }



        private void ViewLoadedCommandAction(RoutedEventArgs? args)
        {
            if ((args is null) || (args.Source is not LoginView))
                return;

            var view = (LoginView)args.Source;

            OnPropertyChanged(nameof(ApiServerURI));

            UserName = "GUEST";
            _txtUserName = view.FindName("txtUserName") as TextBox;
            _txtUserName?.Focus();
        }

        private async Task LoginCommandAction(RoutedEventArgs? args)
        {
            if (string.IsNullOrEmpty(UserName))
            {
                InvokeOnUIThread(() => 
                { 
                    MessageBox.Show("사용자 이름을 입력하십시오.", "로그인", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    _txtUserName?.Focus();
                });
                return;
            }

            bool result = await Task.Factory.StartNew(new Func<bool>(delegate ()
            {
                Common.PrintDebugInfo(string.Format("{0} 로그인 시작", UserName));
                IsLoggingIn = true;

                Task.Delay(2000).Wait();

                Common.PrintDebugInfo(string.Format("{0} 로그인 완료", UserName));
                return true;
            }))
            .ContinueWith<bool>((t) =>
            {
                if (t.IsFaulted)
                {
                    Common.PrintDebugFail(t.Exception ?? new Exception());
                }
                
                IsLoggingIn = false;
                return t.Result;
            });

            if (result)
            {
                App.SetCurrentUser(UserName, UserName + "_" + Common.Generate16IdentityCode(DateTime.Now, new Random()));
                App.ShowFirstHome();
                return;
            }
            else
            {
                App.SetCurrentUser(string.Empty);
                InvokeOnUIThread(() =>
                {
                    MessageBox.Show("사용자 로그인에 실패하였습니다.", "로그인", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    _txtUserName?.Focus();
                });
            }
        }
    }
}
