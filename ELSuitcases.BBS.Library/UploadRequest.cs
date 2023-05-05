using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;

namespace ELSuitcases.BBS.Library
{
    [Serializable]
    public sealed class UploadRequest : ISerializable
    {
        [Required]
        public string TransferID { get; set; } = string.Empty;
        [Required]
        public List<FilePacket> Files { get; set; } = new List<FilePacket>();
        public string UserState { get; set; } = string.Empty;



        public UploadRequest() 
        {

        }
        


        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info != null)
            {
                info.AddValue(nameof(TransferID), TransferID);
                info.AddValue(nameof(Files), Files);
                info.AddValue(nameof(UserState), UserState);
            }
        }

        public override string ToString()
        {
            return System.Text.Json.JsonSerializer.Serialize<UploadRequest>(this);
        }
    }
}
