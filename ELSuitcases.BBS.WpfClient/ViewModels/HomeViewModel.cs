using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ELSuitcases.BBS.WpfClient
{
    internal partial class HomeViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ViewModelBase? _ViewModelOfTop;
        
        [ObservableProperty]
        private ViewModelBase? _ViewModelOfMain;

        [ObservableProperty]
        private ViewModelBase? _ViewModelOfLeft;

        [ObservableProperty]
        private SettingViewModel? _ViewModelOfSetting;

        #region [COMMAND]
        [ObservableProperty]
        private ICommand? _ViewLoadedCommand;
        #endregion



        public HomeViewModel() : base()
        {
            _ViewModelOfTop = App.IocServices.GetRequiredService<MainTopViewModel>();
            _ViewModelOfLeft = App.IocServices.GetRequiredService<BoardDetailViewModel>();
            _ViewModelOfMain = App.IocServices.GetRequiredService<BoardListViewModel>();
            _ViewLoadedCommand = new RelayCommand<RoutedEventArgs>(ViewLoadedCommandAction);
        }



        private void ViewLoadedCommandAction(RoutedEventArgs? args)
        {
            
        }
    }
}
