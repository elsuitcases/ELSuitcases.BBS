using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ELSuitcases.BBS.WpfClient.Messages;

namespace ELSuitcases.BBS.WpfClient
{
    public partial class MainShellViewModel : ViewModelBase
    {
        [ObservableProperty]
        private bool _IsBusy = false;

        [ObservableProperty]
        private string _BusyMessage = "요청을 처리하는 중 ...";
        
        [ObservableProperty]
        private string _Title = string.Empty;

        [ObservableProperty]
        private ViewModelBase? _ViewModel;

        #region [COMMAND]
        [ObservableProperty]
        private ICommand? _ShellWindowClosingCommand;

        [ObservableProperty]
        private ICommand? _ShellWindowLoadedCommand;
        #endregion



        public MainShellViewModel() : base()
        {
            _ShellWindowClosingCommand = new RelayCommand<CancelEventArgs>(ShellWindowClosingCommandAction);
            _ShellWindowLoadedCommand = new RelayCommand<RoutedEventArgs>(ShellWindowLoadedCommandAction);
            
            if (!WeakReferenceMessenger.Default.IsRegistered<BusyMessage>(this))
            {
                WeakReferenceMessenger.Default.Register<BusyMessage>(this, (sender, message) =>
                {
                    if (message == null) 
                        return;
                    else if (message.Sender is WriterViewModel vmWriter)
                    {
                        vmWriter.IsSaving = message.IsBusy;
                        return;
                    }

                    BusyMessage = message.Message;
                    IsBusy = message.IsBusy;
                });
            }
        }



        private void ShellWindowClosingCommandAction(CancelEventArgs? args)
        {

        }

        private void ShellWindowLoadedCommandAction(RoutedEventArgs? args)
        {

        }
    }
}
