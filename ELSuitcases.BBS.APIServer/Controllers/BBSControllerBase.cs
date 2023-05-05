using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Components;
using ELSuitcases.BBS.Library;

namespace ELSuitcases.BBS.APIServer.Controllers
{
    public abstract class BBSControllerBase : ControllerBase
    {
        protected const int BUFFER_SIZE = 4096;
        protected const string PATH_ATTACHED_FILE_FOLDER = "C:\\Temp";

        public Dispatcher CurrentDispatcher { get; private set; }



        public BBSControllerBase() : base()
        {
            CurrentDispatcher = Dispatcher.CreateDefault();
        }



        protected void OkFormattedDetailMessage(string apiFunctionName, string message)
        {
            string msgDetail = string.Format("[{0}][OK] {1}", apiFunctionName.ToUpper(), message);
            Common.PrintDebugInfo(msgDetail);

            Ok();
        }

        protected ObjectResult ProblemFormattedDetailMessage(
            string apiFunctionName, string message, Exception? error, string? instance = null, int? statusCode = null, string? title = null, string? type = null)
        {
            string msgDetail = string.Format("[{0}][Problem] {1}", apiFunctionName.ToUpper(), message);
            Common.PrintDebugFail(error);

            return Problem(msgDetail, instance, statusCode, title, type);
        }
    }
}
