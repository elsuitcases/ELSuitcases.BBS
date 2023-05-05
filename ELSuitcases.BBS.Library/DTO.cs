using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.Serialization;
using System.Text.Json;

namespace ELSuitcases.BBS.Library
{
    [Serializable]
    public class DTO : SortedDictionary<string, object>, IComparable<DTO>, ISerializable
    {
        private string[] _PrimaryKeyPropertyNames = new string[0];
        public string[] PrimaryKeyPropertyNames
        {
            get => _PrimaryKeyPropertyNames;
            protected set => _PrimaryKeyPropertyNames = value;
        }



        public DTO() : base()
        {

        }



        public int CompareTo(DTO other)
        {
            if (this.Equals(other))
                return 0;
            else
                return -1;
        }

        public void CopyTo(DTO dtoTarget)
        {
            if (dtoTarget == null) return;

            foreach (var key in this.Keys)
            {
                dtoTarget.SetValue(key, this[key]);
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info != null)
            {
                foreach (var key in this.Keys)
                {
                    info.AddValue(key, this[key]);
                }
                info.AddValue(Constants.PROPERTY_KEY_NAME_PRIMARY_KEY_PROPERTY_NAMES, JsonSerializer.Serialize(PrimaryKeyPropertyNames));
            }
        }

        public override bool Equals(object obj)
        {
            if ((PrimaryKeyPropertyNames == null) ||
                (PrimaryKeyPropertyNames.Length < 1) ||
                (!(obj is DTO dtoTarget)))
                return false;

            bool result = true;

            var me = this as SortedDictionary<string, object>;
            var other = dtoTarget as SortedDictionary<string, object>;

            foreach (string p in PrimaryKeyPropertyNames)
            {
                if (!((me.ContainsKey(p)) && (other.ContainsKey(p)) && (me[p].Equals(other[p]))))
                {
                    result = false;
                    break;
                }
            }

            return result;
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }

        public int GetInt32(string key, int defaultValue = -1)
        {
            if ((!this.ContainsKey(key)) || (string.IsNullOrEmpty(GetString(key))))
                return defaultValue;
            else
                return Convert.ToInt32(Convert.ToDouble(GetString(key)));
        }

        public object GetObject(string key, object defaultValue = null)
        {
            if ((!this.ContainsKey(key)) || (this[key] == null))
                return defaultValue;
            else
                return this[key];
        }

        public string GetString(string key, string defaultValue = "")
        {
            if ((!this.ContainsKey(key)) || (string.IsNullOrEmpty(Convert.ToString(this[key]))))
                return defaultValue;
            else
                return Convert.ToString(this[key]);
        }

        public void SetValue(string key, object value)
        {
            if (this.ContainsKey(key))
                this[key] = value;
            else
                this.Add(key, value);
        }
    }
}
