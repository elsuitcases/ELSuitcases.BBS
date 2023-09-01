using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ELSuitcases.BBS.Library
{
    public static class APIClientHelper
    {
        public static HttpClient GenerateClient(Uri baseAddress, TimeSpan timeOut)
        {
            HttpClient client = new HttpClient()
            {
                BaseAddress = baseAddress,
                Timeout = timeOut
            };

            return client;
        }

        public static async Task<HttpResponseMessage> Delete(HttpClient client, Uri uriApi)
        {
            string apiFuncName = "ApiClientHelper.DELETE";

            if ((client == null) || (uriApi == null))
                Common.ThrowException(new ArgumentNullException(), true, apiFuncName, Constants.MESSAGE_ERROR_ARGUMENTS_NULL);

            HttpResponseMessage response = null;

            try
            {
                response = await client.DeleteAsync(uriApi);
                string resData = await response.Content.ReadAsStringAsync();

                Common.PrintDebugInfo(string.Format("{0} [{1}] {2}", apiFuncName, response.StatusCode, resData));
            }
            catch (Exception ex)
            {
                Common.ThrowException(ex, false, apiFuncName, Constants.MESSAGE_ERROR_FUNCTION_ON_RUNNING);
            }

            return response;
        }

        public static async Task<HttpResponseMessage> Get(HttpClient client, Uri uriApi)
        {
            string apiFuncName = "ApiClientHelper.GET";

            if ((client == null) || (uriApi == null))
                Common.ThrowException(new ArgumentNullException(), true, apiFuncName, Constants.MESSAGE_ERROR_ARGUMENTS_NULL);

            HttpResponseMessage response = null;

            try
            {
                response = await client.GetAsync(uriApi);
                Common.PrintDebugInfo(string.Format("ApiClientHelper.GET [{0}]", response.StatusCode));
            }
            catch (Exception ex)
            {
                Common.ThrowException(ex, false, apiFuncName, Constants.MESSAGE_ERROR_FUNCTION_ON_RUNNING);
            }

            return response;
        }

        public static async Task<long> Get_DownloadAttachedFile(
            HttpClient client, Uri uriApi, FileInfo fileToSave, ProgressState state)
        {
            string apiFuncName = "ApiClientHelper.GET_DownloadAttachedFile";

            if ((client == null) || (uriApi == null) || (fileToSave == null) || (state == null))
                Common.ThrowException(new ArgumentNullException(), true, apiFuncName, Constants.MESSAGE_ERROR_ARGUMENTS_NULL);

            long totalBytesToRead = -1;

            using (var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, uriApi)))
            {
                await Task.Delay(1);
                totalBytesToRead = Convert.ToInt64(
                                            (response.Headers.GetValues(Constants.PROPERTY_KEY_NAME_ATTACHED_FILE_SIZE).Count() > 0) ?
                                                (response.Headers.GetValues(Constants.PROPERTY_KEY_NAME_ATTACHED_FILE_SIZE).ElementAt(0) ?? string.Empty) :
                                                "-1");
            }

            int readBytes = 0;
            long readTotalBytes = 0;
            state.Percent = 0.0;

            if ((fileToSave.Directory != null) && (!Directory.Exists(fileToSave.Directory.FullName)))
                Directory.CreateDirectory(fileToSave.Directory.FullName);

            if (File.Exists(fileToSave.FullName))
                File.Delete(fileToSave.FullName);

            using (FileStream fs = new FileStream(fileToSave.FullName, FileMode.CreateNew, FileAccess.Write, FileShare.Write))
            using (Stream s = await client.GetStreamAsync(uriApi))
            {
                try
                {
                    byte[] buffer = new byte[Constants.DEFAULT_VALUE_BUFFER_SIZE];

                    while ((readBytes = await s.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fs.WriteAsync(buffer, 0, readBytes);
                        await fs.FlushAsync();

                        readTotalBytes += readBytes;

                        state.Percent = ((double)readTotalBytes / (double)totalBytesToRead) * 100;
                        if (state.Percent < 0) state.Percent = 0;
                        else if (state.Percent > 100) state.Percent = 100;

                        Common.PrintDebugInfo(string.Format("[첨부파일 버퍼 다운로드 - {0}%] FILENAME = {1} / TOTAL_RECEIVED = {2}",
                                                        state.Percent.ToString("N0"), fileToSave.Name, readTotalBytes),
                                                        apiFuncName);

                        await Task.Delay(1);
                    }
                }
                catch (Exception ex)
                {
                    state.CancelTokenSource?.Cancel(true);
                    Common.ThrowException(ex, false, apiFuncName, Constants.MESSAGE_ERROR_FUNCTION_ON_RUNNING);
                }
            }

            return readTotalBytes;
        }

        public static async Task<string> Get_String(HttpClient client, Uri uriApi)
        {
            string apiFuncName = "ApiClientHelper.GET_String";

            if ((client == null) || (uriApi == null))
                Common.ThrowException(new ArgumentNullException(), true, apiFuncName, Constants.MESSAGE_ERROR_ARGUMENTS_NULL);

            string result = string.Empty;
            HttpResponseMessage response;

            try
            {
                response = await Get(client, uriApi);

                if (response != null)
                    result = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Common.ThrowException(ex, false, apiFuncName, Constants.MESSAGE_ERROR_FUNCTION_ON_RUNNING);
            }

            return result;
        }

        public static async Task<HttpResponseMessage> Post(HttpClient client, Uri uriApi, DTO dto)
        {
            string apiFuncName = "ApiClientHelper.POST";

            if ((client == null) || (uriApi == null) || (dto == null))
                Common.ThrowException(new ArgumentNullException(), true, apiFuncName, Constants.MESSAGE_ERROR_ARGUMENTS_NULL);

            HttpResponseMessage response = null;

            try
            {
                using (var c = new StringContent(dto.ToString(), Encoding.UTF8, "application/json"))
                {
                    response = await client.PostAsync(uriApi, c);
                    Common.PrintDebugInfo(string.Format("{0} [{1}]", apiFuncName, response.StatusCode));
                }
            }
            catch (Exception ex)
            {
                Common.ThrowException(ex, false, apiFuncName, Constants.MESSAGE_ERROR_FUNCTION_ON_RUNNING);
            }

            return response;
        }

        public static async Task<HttpResponseMessage> Put(HttpClient client, Uri uriApi, DTO dto)
        {
            string apiFuncName = "ApiClientHelper.PUT";

            if ((client == null) || (uriApi == null) || (dto == null))
                Common.ThrowException(new ArgumentNullException(), true, apiFuncName, Constants.MESSAGE_ERROR_ARGUMENTS_NULL);

            HttpResponseMessage response = null;

            try
            {
                using (var c = new StringContent(dto.ToString(), Encoding.UTF8, "application/json"))
                {
                    response = await client.PutAsync(uriApi, c);
                    Common.PrintDebugInfo(string.Format("{0} [{1}]", apiFuncName, response.StatusCode));
                }
            }
            catch (Exception ex)
            {
                Common.ThrowException(ex, false, apiFuncName, Constants.MESSAGE_ERROR_FUNCTION_ON_RUNNING);
            }

            return response;
        }

        private static async Task<long> Post_UploadAttachedFile(
            HttpClient client, 
            Uri uriApi, 
            string transferId, 
            string fileId, 
            string filePath,
            string saveFileName,
            bool isLastFile = true,
            ProgressState state = null,
            int bufferSize = Constants.DEFAULT_VALUE_BUFFER_SIZE)
        {
            string apiFuncName = "ApiClientHelper.POST_UploadAttachedFile";

            if ((client == null) || (uriApi == null) || (!File.Exists(filePath)) ||
                (string.IsNullOrEmpty(transferId)) || (string.IsNullOrEmpty(fileId)))
                Common.ThrowException(new ArgumentNullException(), true, apiFuncName, Constants.MESSAGE_ERROR_ARGUMENTS_NULL);

            FileInfo fi = new FileInfo(filePath);
            FilePacket itemFile = new FilePacket(transferId, fileId, fi.Name)
            {
                IsEOF = false,
                IsLastFileInPackage = isLastFile,
                UserState = saveFileName
            };
            byte[] buffer = new byte[bufferSize];
            int readBytes = 0;
            long readTotalBytes = 0;
            double percent = 0.0;

            using (FileStream fs = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                try
                {
                    Common.PrintDebugInfo(string.Format("{0} [FILE_SENDING] PID = {1} / FileID = {2} / FileName = {3} / Size = {4}", 
                                                    apiFuncName,
                                                    itemFile.PackageID, 
                                                    itemFile.FileID, 
                                                    itemFile.FileName, 
                                                    fi.Length.ToString("N0")));
                    fs.Position = 0;

                    while ((readBytes = await fs.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        itemFile.WriteFileSlice(buffer, 0, readBytes);
                        itemFile.IsEOF = ((fs.Length - (readTotalBytes + readBytes)) < 1);

                        await client.PostAsync(
                                        uriApi, 
                                        new StringContent(itemFile.ToString(), Encoding.UTF8, "application/json"),
                                        ((state?.CancelTokenSource.Token is CancellationToken token) ? token : CancellationToken.None))
                                        .ContinueWith((t) =>
                                        {
                                            if ((t.IsCompleted) && (!t.IsCanceled) && (!t.IsFaulted))
                                            {
                                                percent = ((double)(readTotalBytes + readBytes) / (double)fs.Length) * 100;
                                                if (percent > 100) percent = 100;
                                                else if (percent < 0) percent = 0;

                                                if (state != null)
                                                    state.UserState = readBytes;
                                            }

                                            if (t.Exception != null)
                                                Common.ThrowException(
                                                            t.Exception, 
                                                            false, 
                                                            apiFuncName, 
                                                            string.Format("{0} / FileName = {1}", Constants.MESSAGE_ERROR_FUNCTION_ON_RUNNING, itemFile.FileName));
                                        });

                        readTotalBytes += readBytes;
                        readBytes = 0;

                        if (readTotalBytes == fi.Length)
                            break;

                        await Task.Delay(1);
                    }

                    Common.PrintDebugInfo(string.Format("{0} [FILE_SENT] PID = {1} / FileID = {2} / FileName = {3} / Size = {4} / SentBytes = {5}",
                                                    apiFuncName,
                                                    itemFile.PackageID,
                                                    itemFile.FileID,
                                                    itemFile.FileName,
                                                    fi.Length.ToString("N0"),
                                                    readTotalBytes.ToString("N0")));
                }
                catch (Exception ex)
                {
                    Common.ThrowException(
                                ex, 
                                false, 
                                apiFuncName, 
                                string.Format("{0} / PID = {1} / FileID = {2} / FileName = {3} / Size = {4} / SentBytes = {5}", 
                                        Constants.MESSAGE_ERROR_FUNCTION_ON_RUNNING, 
                                        itemFile.PackageID, 
                                        itemFile.FileID, 
                                        itemFile.FileName, 
                                        fi.Length.ToString("N0"), 
                                        readTotalBytes.ToString("N0")));
                }
            }

            return readTotalBytes;
        }

        public static async Task<long> Post_UploadAttachedFiles(
            string bbsId,
            string articleId,
            int replyId,
            string packageId,
            IEnumerable<FilePacket> files,
            HttpClient client,
            Uri uriApiQueue,
            Uri uriApiDequeue,
            Uri uriApiUploadFiles,
            ProgressState state = null,
            int bufferSize = Constants.DEFAULT_VALUE_BUFFER_SIZE)
        {
            if ((files == null) || (files.Count() < 1))
                return 0;

            string apiFuncName = "ApiClientHelper.Post_UploadAttachedFiles";
            UploadRequest request = new UploadRequest()
            {
                TransferID = packageId,
                Files = files.ToList(),
                UserState = string.Format("{0}{3}{1}{3}{2}", bbsId, articleId, replyId, Constants.SEPARATOR_COMMA)
            };

            using (HttpResponseMessage response = await client.PostAsync(uriApiQueue, new StringContent(request.ToString(), Encoding.UTF8, "application/json")))
            {
                bool result = JsonSerializer.Deserialize<bool>(await response.Content.ReadAsStringAsync());
                if (!result)
                {
                    Common.PrintDebugInfo(string.Format("{0} [UPLOAD_QUEUE] PID = {1} - 업로드 큐를 서버에 등록하지 못하였습니다.",
                                                    apiFuncName,
                                                    request.TransferID));
                    return 0;
                }
            }

            long bytesUpload = 0, totalBytesToUpload = 0;
            double percent = 0.0;

            foreach (var f in files)
            {
                if ((!f.IsPendingDelete) && (f.IsAddedNew))
                    totalBytesToUpload += new FileInfo(f.FileName).Length;
            }

            foreach (var f in files)
            {
                if ((!f.IsPendingDelete) && (f.IsAddedNew))
                {
                    ProgressState psEntry = new ProgressState(f.FileID);
                    psEntry.CancelTokenSource = new CancellationTokenSource();
                    psEntry.PropertyChanged += (s, e) =>
                    {
                        ProgressState ps = (ProgressState)s;
                        
                        if ((e.PropertyName == nameof(ps.UserState)) && (ps.UserState != null))
                        {
                            bytesUpload += long.Parse(ps.UserState.ToString());
                            percent = ((double)bytesUpload / (double)totalBytesToUpload) * 100;
                            ps.Percent = percent;

                            if (state != null)
                            {
                                state.UserState = f.UserState;
                                state.Percent = percent;
                            }
                        }
                    };

                    await Post_UploadAttachedFile(client, uriApiUploadFiles, f.PackageID, f.FileID, f.FileName, f.UserState, f.IsLastFileInPackage, psEntry, bufferSize);
                    await Task.Delay(1);
                }
            }

            using (HttpResponseMessage response = await client.PostAsync(uriApiDequeue, new StringContent(request.ToString(), Encoding.UTF8, "application/json")))
            {
                bool result = JsonSerializer.Deserialize<bool>(await response.Content.ReadAsStringAsync());
                if (!result)
                {
                    Common.PrintDebugInfo(string.Format("{0} [UPLOAD_DEQUEUE] PID = {1} - 업로드 큐를 정상적으로 종료하지 못하였습니다.",
                                                    apiFuncName,
                                                    request.TransferID));
                    return 0;
                }
            }

            return bytesUpload;
        }
    }
}
