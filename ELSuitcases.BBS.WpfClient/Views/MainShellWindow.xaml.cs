using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ELSuitcases.BBS.WpfClient
{
    /// <summary>
    /// Interaction logic for MainShellWindow.xaml
    /// </summary>
    public partial class MainShellWindow : Window
    {
        public MainShellWindow()
        {
            InitializeComponent();
        }

        public new MainShellViewModel? DataContext
        {
            get => GetValue(Window.DataContextProperty) as MainShellViewModel;
            set
            {
                SetValue(Window.DataContextProperty, value);
            }
        }
    }
}
