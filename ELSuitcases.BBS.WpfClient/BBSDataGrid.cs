using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ELSuitcases.BBS.Library;

namespace ELSuitcases.BBS.WpfClient
{
    public sealed class BBSDataGrid : DataGrid
    {
        public BBSDataGrid() : base()
        {
            this.MouseDoubleClick += BBSDataGrid_MouseDoubleClick;
        }



        private void BBSDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if ((SelectedItems != null) && (SelectedItems.Count > 0))
            {
                if (SelectedItems[0] is not DTO dto)
                    return;

                var vmReader = App.IocServices.GetRequiredService<ReaderViewModel>();
                vmReader.Article = dto as ArticleDTO;

                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Window winPopup = new Window()
                    {
                        Name = string.Format("winReader_{0}", Common.Generate16IdentityCode(DateTime.Now, new Random())),
                        Content = vmReader,
                        MinWidth = 640,
                        MinHeight = 480,
                        Width = 800,
                        Height = 600,
                        Icon = Application.Current.MainWindow.Icon,
                        Owner = Application.Current.MainWindow,
                        ResizeMode = ResizeMode.CanResizeWithGrip,
                        ShowActivated = true,
                        ShowInTaskbar = true,
                        Title = dto.GetString(Constants.PROPERTY_KEY_NAME_TITLE) ?? "게시물 조회",
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        WindowState = WindowState.Normal,
                        WindowStyle = WindowStyle.SingleBorderWindow
                    };
                    winPopup.Closed += (s, e) =>
                    {
                        if (s is Window win)
                            App.ReaderWindows.Remove(win);

                        if (App.ReaderWindows.Count == 0)
                            Application.Current.MainWindow.Activate();
                    };

                    App.ReaderWindows.Add(winPopup);
                    winPopup.Show();
                    winPopup.Activate();
                }));
            }
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);

            Dispatcher.Invoke(() =>
            {
                if (VisualTreeHelper.GetChild(this, 0) is Decorator dec)
                {
                    var scrollViewer = dec.Child as ScrollViewer;
                    scrollViewer?.ScrollToTop();
                }
            });
        }
    }
}
