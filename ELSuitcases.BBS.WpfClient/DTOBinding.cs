using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Data;
using ELSuitcases.BBS.Library;

namespace ELSuitcases.BBS.WpfClient
{
    public sealed class DTOBinding : Binding
    {
        private string? _Keys = string.Empty;
        public string? Keys
        {
            get => (string.IsNullOrEmpty(_Keys)) ? string.Empty : _Keys;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    string[] keys = value.Split(',');

                    if ((keys != null) && (keys.Length > 0))
                    {
                        if (keys.Length == 1)
                        {
                            this.Path = new PropertyPath(string.Format("[{0}]", keys[0]));
                        }
                        else
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append('[');
                            for (int i = 0; i < keys.Length; i++)
                            {
                                sb.Append(keys[i]);
                                if (i < keys.Length - 1)
                                {
                                    sb.Append(',');
                                }
                            }
                            sb.Append(']');
                            this.Path = new PropertyPath(sb.ToString());
                        }
                    }
                }

                _Keys = value;
            }
        }

        private DTO? _Source = null;
        public new DTO? Source
        {
            get => _Source;
            set => _Source = value;
        }



        public DTOBinding() : base()
        {
            
        }
    }
}
