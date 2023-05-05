using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ELSuitcases.BBS.Library;

namespace ELSuitcases.BBS.WpfClient
{
    internal partial class MainTopViewModel : ViewModelBase
    {
        #region [COMMAND]
        [ObservableProperty]
        private ICommand _ViewLoadedCommand;
        #endregion



        public MainTopViewModel() : base()
        {
            _ViewLoadedCommand = new RelayCommand<RoutedEventArgs>(ViewLoadedCommandAction);
        }



        private void ViewLoadedCommandAction(RoutedEventArgs? args)
        {
            
        }
    }
}
