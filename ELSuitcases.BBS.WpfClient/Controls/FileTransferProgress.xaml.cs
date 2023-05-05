using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using ELSuitcases.BBS.Library;

namespace ELSuitcases.BBS.WpfClient
{
    /// <summary>
    /// Interaction logic for FileTransferProgress.xaml
    /// </summary>
    public partial class FileTransferProgress : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ClickCommandProperty = DependencyProperty.Register(
            nameof(ClickCommand), typeof(ICommand), typeof(FileTransferProgress), new FrameworkPropertyMetadata(null));
        
        public static readonly DependencyProperty FileNameProperty = DependencyProperty.Register(
            nameof(FileName), typeof(string), typeof(FileTransferProgress), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty StateProperty = DependencyProperty.Register(
            nameof(State), typeof(ProgressState), typeof(FileTransferProgress), new PropertyMetadata(null));
        


        public ICommand ClickCommand
        {
            get => (ICommand)GetValue(ClickCommandProperty);
            set
            {
                SetValue(ClickCommandProperty, value);
                OnPropertyChanged(nameof(ClickCommand));
            }
        }

        public string FileName
        {
            get => (string)GetValue(FileNameProperty);
            set
            {
                SetValue(FileNameProperty, value);
                OnPropertyChanged(nameof(FileName));
            }
        }

        public ProgressState State
        {
            get => (ProgressState)GetValue(StateProperty);
            set
            {
                SetValue(StateProperty, value);
                OnPropertyChanged(nameof(State));
            }
        }



        public event PropertyChangedEventHandler? PropertyChanged;



        public FileTransferProgress()
        {
            InitializeComponent();
        }



        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (string.IsNullOrEmpty(propertyName))
                return;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
