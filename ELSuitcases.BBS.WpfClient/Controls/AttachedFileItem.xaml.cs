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

namespace ELSuitcases.BBS.WpfClient
{
    /// <summary>
    /// Interaction logic for AttachedFileItem.xaml
    /// </summary>
    public partial class AttachedFileItem : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ClickCommandProperty = DependencyProperty.Register(
            nameof(ClickCommand), typeof(ICommand), typeof(AttachedFileItem), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty FileIDProperty = DependencyProperty.Register(
            nameof(FileID), typeof(string), typeof(AttachedFileItem), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty FileNameProperty = DependencyProperty.Register(
            nameof(FileName), typeof(string), typeof(AttachedFileItem), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register(
            nameof(FilePath), typeof(string), typeof(AttachedFileItem), new PropertyMetadata(string.Empty));



        public ICommand ClickCommand
        {
            get => (ICommand)GetValue(ClickCommandProperty);
            set
            {
                SetValue(ClickCommandProperty, value);
                OnPropertyChanged(nameof(ClickCommand));
            }
        }

        public string FileID
        {
            get => (string)GetValue(FileIDProperty);
            set => SetValue(FileIDProperty, value);
        }

        public string FileName
        {
            get => (string)GetValue(FileNameProperty);
            set => SetValue(FileNameProperty, value);
        }

        public string FilePath
        {
            get => (string)GetValue(FilePathProperty);
            set => SetValue(FilePathProperty, value);
        }



        public event PropertyChangedEventHandler? PropertyChanged;



        public AttachedFileItem()
        {
            InitializeComponent();
        }



        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (string.IsNullOrEmpty(propertyName))
                return;

            switch (propertyName)
            {
                case nameof(FilePath):
                    if (string.IsNullOrEmpty(FilePath))
                        FilePath = FileName;
                    break;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
