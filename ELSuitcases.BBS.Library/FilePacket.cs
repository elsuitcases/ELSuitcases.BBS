using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;

namespace ELSuitcases.BBS.Library
{
    [Serializable]
    public class FilePacket : INotifyPropertyChanged, ISerializable
    {
        private string _PackageID = string.Empty;
        [Required]
        public string PackageID
        {
            get => _PackageID;
            set
            {
                _PackageID = value;
                OnPropertyChanged(nameof(PackageID));
            }
        }

        private string _FileID = string.Empty;
        [Required]
        public string FileID
        {
            get => _FileID;
            set
            {
                _FileID = value;
                OnPropertyChanged(nameof(FileID));
            }
        }

        private string _FileName = string.Empty;
        [Required]
        public string FileName
        {
            get => _FileName;
            set
            {
                _FileName = value;
                OnPropertyChanged(nameof(FileName));
            }
        }

        private bool _IsAddedNew = false;
        [Required]
        public bool IsAddedNew
        {
            get => _IsAddedNew;
            set
            {
                _IsAddedNew = value;
                OnPropertyChanged(nameof(IsAddedNew));
            }
        }

        private bool _IsEOF = false;
        [Required]
        public bool IsEOF
        {
            get => _IsEOF;
            set
            {
                _IsEOF = value;
                OnPropertyChanged(nameof(IsEOF));
            }
        }

        private bool _IsLastFileInPackage = false;
        [Required]
        public bool IsLastFileInPackage
        {
            get => _IsLastFileInPackage;
            set
            {
                _IsLastFileInPackage = value;
                OnPropertyChanged(nameof(IsLastFileInPackage));
            }
        }

        private bool _IsPendingDelete = false;
        [Required]
        public bool IsPendingDelete
        {
            get => _IsPendingDelete;
            set
            {
                _IsPendingDelete = value;
                OnPropertyChanged(nameof(IsPendingDelete));
            }
        }

        private string _FileSliceAsBase64String = string.Empty;
        public string FileSliceAsBase64String
        {
            get => _FileSliceAsBase64String;
            set
            {
                _FileSliceAsBase64String = value;
                OnPropertyChanged(nameof(FileSliceAsBase64String));
            }
        }

        public string _UserState = string.Empty;
        public string UserState
        {
            get => _UserState;
            set
            {
                _UserState = value;
                OnPropertyChanged(nameof(UserState));
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;



        [System.Text.Json.Serialization.JsonConstructor]
        public FilePacket()
        {
            
        }

        public FilePacket(string packageId, string fileId, string fileName) : this()
        {
            PackageID = packageId;
            FileID = fileId;
            FileName = fileName;
        }

        

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (string.IsNullOrEmpty(propertyName))
                return;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info != null)
            {
                info.AddValue(nameof(PackageID), PackageID);
                info.AddValue(nameof(FileID), FileID);
                info.AddValue(nameof(FileName), FileName);
                info.AddValue(nameof(IsAddedNew), IsAddedNew);
                info.AddValue(nameof(IsEOF), IsEOF);
                info.AddValue(nameof(IsLastFileInPackage), IsLastFileInPackage);
                info.AddValue(nameof(IsPendingDelete), IsPendingDelete);
                info.AddValue(nameof(FileSliceAsBase64String), FileSliceAsBase64String);
                info.AddValue(nameof(UserState), UserState);
            }
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize<FilePacket>(this);
        }



        public byte[] ReadFileSlice()
        {
            if (string.IsNullOrEmpty(FileSliceAsBase64String))
                return new byte[0];

            byte[] bytes;

            try
            {
                bytes = Convert.FromBase64String(FileSliceAsBase64String);
            }
            catch (Exception)
            {
                bytes = new byte[0];
            }

            return bytes;
        }

        public string WriteFileSlice(byte[] buffer, int offset, int count)
        {
            string data = Convert.ToBase64String(new byte[0], Base64FormattingOptions.None);

            if ((buffer != null) && (buffer.Length > 0))
                data = Convert.ToBase64String(buffer, offset, count, Base64FormattingOptions.None);

            FileSliceAsBase64String = data;

            return data;
        }
    }
}
