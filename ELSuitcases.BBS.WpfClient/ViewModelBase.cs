using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ELSuitcases.BBS.WpfClient
{
    public abstract class ViewModelBase : ObservableObject
    {
        private string _ID = string.Empty;
        public string ID
        {
            get => _ID;
            private set => SetProperty(ref _ID, value);
        }
        


        public ViewModelBase() : base()
        {
            ID = Guid.NewGuid().ToString();
        }



        public void InvokeOnUIThread(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            Application.Current.Dispatcher.Invoke(action, priority);
        }

        public TResult InvokeOnUIThread<TResult>(Func<TResult> callback, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            return Application.Current.Dispatcher.Invoke(callback, priority);
        }

        public void InvokeAsyncOnUIThread(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            if (action == null) return;
            Application.Current.Dispatcher.BeginInvoke(action, priority);
        }

        public DispatcherOperation<TResult> InvokeAsyncOnUIThread<TResult>(Func<TResult> callback, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            return Application.Current.Dispatcher.InvokeAsync(callback, priority);
        }
    }
}
