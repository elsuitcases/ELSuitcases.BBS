using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELSuitcases.BBS.Library.Server
{
    public sealed class UploadTask
    {
        public string TransferID { get; set; } = string.Empty;
        public UploadRequest Request { get; private set; }
        public bool IsPrepared { get; private set; } = false;
        public Action<object?> PrepareTaskAction { get; set; }



        public UploadTask(string transferId, UploadRequest request, Action<object?> prepareAction)
        {
            TransferID = transferId;
            Request = request;
            PrepareTaskAction = prepareAction;
        }



        public override string ToString()
        {
            return System.Text.Json.JsonSerializer.Serialize<UploadTask>(this);
        }

        public Task Prepare()
        {
            Task task;

            if (PrepareTaskAction != null)
                task = Task.Factory.StartNew(PrepareTaskAction, this);
            else
                task = Task.CompletedTask;

            IsPrepared = true;

            return task;
        }
    }
}
