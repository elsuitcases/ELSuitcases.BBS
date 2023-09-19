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
        private PasswordBox? _txtUserPassword;



        public string ApiServerURI
        {
            get => App.APIServerBaseURL?.ToString() ?? string.Empty;
        }

        [ObservableProperty]
        private bool _IsLoggingIn = false;

        [ObservableProperty]
        private string _UserName = string.Empty;

        [ObservableProperty]
        private string _UserPassword = string.Empty;

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

            _txtUserName = view.FindName("txtUserName") as TextBox;
            _txtUserPassword = view.FindName("txtUserPassword") as PasswordBox;
            if (_txtUserPassword != null)
                _txtUserPassword.PasswordChanged += (s, e) =>
                {
                    if (e.Source is PasswordBox pb)
                    {
                        e.Handled = true;
                        UserPassword = pb.Password;
                    }
                };

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

            UserName = UserName.ToUpper();
            IsLoggingIn = true;

            UserPassword = Convert.ToBase64String(await Common.AESEncrypt(
                                                                Encoding.UTF8.GetBytes(UserPassword),
                                                                Encoding.UTF8.GetBytes(Constants.ENCRYPT_KEY),
                                                                Encoding.UTF8.GetBytes(Constants.ENCRYPT_IV)));
            bool result = await App.Login(UserName, UserPassword);

            IsLoggingIn = false;

            if (result)
            {
                App.ShowFirstHome();
                return;
            }
            else
            {
                InvokeOnUIThread(() =>
                {
                    MessageBox.Show("사용자 로그인에 실패하였습니다.", "로그인", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    _txtUserName?.SelectAll();
                    _txtUserName?.Focus();
                });
            }
        }
    }
}
