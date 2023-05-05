using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace ELSuitcases.BBS.Library
{
    public class ProgressStatePercentChangedEventArgs : EventArgs
    {
        public double OldPercent { get; private set; }
        public double NewPercent { get; private set; }
        
        public ProgressStatePercentChangedEventArgs(double oldPercent, double newPercent) : base()
        {
            OldPercent = oldPercent;
            NewPercent = newPercent;
        }
    }


    
    public class ProgressState : INotifyPropertyChanged
    {
        private string _ID = string.Empty;
        [Required]
        public string ID
        {
            get => _ID;
            private set
            {
                _ID = value;
                OnPropertyChanged(nameof(ID));
            }
        }
        
        private string _Name = string.Empty;
        [Required]
        public string Name
        {
            get => _Name;
            set
            {
                _Name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        private double _Percent = 0.0;
        public double Percent
        {
            get => _Percent;
            set
            {
                if (value > 100) value = 100.0;
                else if (value < 0) value = 0.0;

                PercentChanged?.Invoke(this, new ProgressStatePercentChangedEventArgs(_Percent, value));

                _Percent = value;
                OnPropertyChanged(nameof(Percent));
            }
        }

        private CancellationTokenSource _CancelTokenSource = null;
        public CancellationTokenSource CancelTokenSource
        {
            get => _CancelTokenSource;
            set
            {
                _CancelTokenSource = value;
                OnPropertyChanged(nameof(CancelTokenSource));
            }
        }

        private object _UserState = null;
        public object UserState
        {
            get => _UserState;
            set
            {
                _UserState = value;
                OnPropertyChanged(nameof(UserState));
            }
        }



        public delegate void PercentChangedEventHandler(object sender, ProgressStatePercentChangedEventArgs e);
        public event PercentChangedEventHandler PercentChanged;
        public event PropertyChangedEventHandler PropertyChanged;



        private ProgressState()
        {
            ID = Guid.NewGuid().ToString();
        }

        public ProgressState(string name) : this()
        {
            Name = name;
        }



        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (string.IsNullOrEmpty(propertyName))
                return;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
