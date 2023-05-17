using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ELSuitcases.BBS.Library;

namespace ELSuitcases.BBS.WpfClient
{
    internal partial class ReaderViewModel : ViewModelBase
    {
        private ReaderView? _view = null;



        private readonly ObservableCollection<FilePacket> _AttachedFiles = new ObservableCollection<FilePacket>();
        public ObservableCollection<FilePacket> AttachedFiles
        {
            get => _AttachedFiles;
        }

        [ObservableProperty]
        private ArticleDTO? _Article = null;

        [ObservableProperty]
        private string _BusyMessage = "게시물을 불러오는 중 ...";

        [ObservableProperty]
        private bool _IsDownloading = false;

        [ObservableProperty]
        private bool _IsQuering = false;

        private bool _IsOwnedArticle = false;
        public bool IsOwnedArticle
        {
            get => _IsOwnedArticle;
            private set => SetProperty(ref _IsOwnedArticle, value);
        }

        [ObservableProperty]
        private ProgressState _DownloadState = new ProgressState("");

        #region [COMMAND]
        [ObservableProperty]
        private ICommand? _ViewLoadedCommand;

        [ObservableProperty]
        private ICommand? _AttachedFileClickCommand;

        [ObservableProperty]
        private ICommand? _CancelCommand;

        [ObservableProperty]
        private ICommand? _DeleteCommand;

        [ObservableProperty]
        private ICommand? _RefreshCommand;

        [ObservableProperty]
        private ICommand? _UpdateCommand;
        #endregion



        public ReaderViewModel() : base()
        {
            _ViewLoadedCommand = new RelayCommand<RoutedEventArgs>(ViewLoadedCommandAction);
            _AttachedFileClickCommand = new AsyncRelayCommand<MouseButtonEventArgs>(AttachedFileClickCommandAction);
            _CancelCommand = new AsyncRelayCommand<RoutedEventArgs>(CancelCommandAction);
            _DeleteCommand = new AsyncRelayCommand<RoutedEventArgs>(DeleteCommandAction);
            _RefreshCommand = new AsyncRelayCommand(RefreshCommandAction);
            _UpdateCommand = new RelayCommand<RoutedEventArgs>(UpdateCommandAction);

            _AttachedFiles.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(AttachedFiles));
            };
        }



        private void ViewLoadedCommandAction(RoutedEventArgs? args)
        {
            if (args?.Source is not ReaderView view) return;

            _view = view;

            RefreshCommand?.Execute(null);
        }

        private async Task AttachedFileClickCommandAction(MouseButtonEventArgs? args)
        {
            if (args?.Source is not AttachedFileItem ctlFileItem)
                return;
            
            if ((string.IsNullOrEmpty(ctlFileItem.FileID) ||
                (AttachedFiles.SingleOrDefault(f => f.FileID == ctlFileItem.FileID) is not FilePacket file))) 
                return;
            
            if (App.ShowMessageBoxQuestion(string.Format("첨부파일을 다운로드 하시겠습니까?\r\n\r\n{0}", file.FileName), "첨부파일") == MessageBoxResult.Yes)
            {
                await Task.Delay(1);
                await DownloadFile(
                        Article?.GetString(Constants.PROPERTY_KEY_NAME_BBS_ID) ?? string.Empty,
                        Article?.GetString(Constants.PROPERTY_KEY_NAME_ARTICLE_ID) ?? string.Empty,
                        Article?.GetInt32(Constants.PROPERTY_KEY_NAME_REPLY_ID, -1) ?? -1,
                        file.FileName);
            }
        }

        private async Task CancelCommandAction(RoutedEventArgs? args)
        {
            await Task.Delay(1);

            if (!IsQuering)
            {
                if (Window.GetWindow(_view) is Window win)
                    win.Close();
            }
        }

        private async Task DeleteCommandAction(RoutedEventArgs? args)
        {
            if (Article == null) return;

            MessageBoxResult userAnswer = App.ShowMessageBoxQuestion("게시물을 삭제 하시겠습니까?", "게시물 삭제");
            if (userAnswer == MessageBoxResult.No) return;

            await Task.Delay(1);

            bool result = false;
            Uri uriApi = new Uri(string.Format("{0}/{1}/{2}/{3}",
                                                App.APIServerURL_Article?.ToString() ?? string.Empty,
                                                Article.GetString(Constants.PROPERTY_KEY_NAME_BBS_ID),
                                                Article.GetString(Constants.PROPERTY_KEY_NAME_ARTICLE_ID),
                                                Article.GetInt32(Constants.PROPERTY_KEY_NAME_REPLY_ID)));

            using (HttpClient client = APIClientHelper.GenerateClient(uriApi, Constants.DEFAULT_VALUE_API_CLIENT_TIMEOUT))
            {
                using (HttpResponseMessage response = await APIClientHelper.Delete(client, uriApi))
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        result = true;
                    else
                        result = false;
                }
            }

            if (result)
            {
                App.ShowMessageBoxInformation("게시물을 삭제 하였습니다.", "게시물 삭제");

                if (Window.GetWindow(_view) is Window win)
                    win.Close();
            }
            else
            {
                App.ShowMessageBoxExclamation("게시물 삭제를 실패 하였습니다.", "게시물 삭제");
            }
        }

        private async Task RefreshCommandAction()
        {
            if (Article != null)
                await Query();
            else
                return;
        }

        private void UpdateCommandAction(RoutedEventArgs? args)
        {
            if (Article == null) return;

            WriterViewModel vmWriter = App.IocServices.GetRequiredService<WriterViewModel>();
            vmWriter.IsUpdateMode = true;
            vmWriter.UpdateSourceArticle = Article;

            if (Window.GetWindow(_view) is Window winCurrent)
            {
                winCurrent.BeginInit();
                winCurrent.Content = vmWriter;
                winCurrent.Title = string.Format("게시물 수정 - {0}", Article.GetString(Constants.PROPERTY_KEY_NAME_TITLE));
                winCurrent.EndInit();
            }
        }



        private async Task DownloadFile(string bbsId, string articleId, int replyId, string entryFileName = "")
        {
            if ((string.IsNullOrEmpty(bbsId)) || (string.IsNullOrEmpty(articleId)) || (replyId < 0))
                return;
            
            string fileDownName = (!string.IsNullOrEmpty(entryFileName)) ? entryFileName : string.Format("{0}_{1}_{2}.zip", bbsId, articleId, replyId);
            
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                FileName = fileDownName,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer),
                OverwritePrompt = true,
                Title = "첨부파일 저장 경로 선택"
            };

            if (!saveFileDialog.ShowDialog(Window.GetWindow(_view)).GetValueOrDefault())
                return;

            BusyMessage = "첨부파일 다운로드 중 ...";
            IsQuering = true;
            IsDownloading = true;

            long downloadedSize = 0;
            FileInfo pathDownload = new FileInfo(saveFileDialog.FileName);
            Uri uriApiGetFile = new Uri(string.Format("{0}/{1}/{2}/{3}/{4}{5}", 
                                                App.APIServerURL_Article?.ToString(), 
                                                (!string.IsNullOrEmpty(entryFileName)) ? Constants.API_ARTICLE_SUBPATH_DOWNLOAD_ENTRY_FILE : Constants.API_ARTICLE_SUBPATH_DOWNLOAD_PACKAGE_FILE, 
                                                bbsId, 
                                                articleId, 
                                                replyId,
                                                (!string.IsNullOrEmpty(entryFileName)) ? "/" + HttpUtility.UrlEncode(entryFileName, Encoding.UTF8) : ""), 
                                        UriKind.Absolute);
            CancellationTokenSource ctsFile = new CancellationTokenSource();
            ctsFile.Token.Register(() =>
            {
                IsQuering = false;
                IsDownloading = false;
                App.ShowMessageBoxExclamation("다운로드가 취소되었습니다.", "첨부파일");
            });
            DownloadState = new ProgressState(pathDownload.Name);
            DownloadState.UserState = fileDownName;
            DownloadState.PercentChanged += (s, e) =>
            {
                BusyMessage = "첨부파일 다운로드 중 ...";
            };

            using (HttpClient client = APIClientHelper.GenerateClient(uriApiGetFile, Constants.DEFAULT_VALUE_API_CLIENT_TIMEOUT))
            {
                downloadedSize = await APIClientHelper.Get_DownloadAttachedFile(client, uriApiGetFile, pathDownload, DownloadState);
            }

            App.ShowMessageBoxInformation(
                    string.Format("첨부파일을 다운로드 하였습니다.\r\n- 파일 : {0}\r\n- 다운로드 크기 : {1} (바이트)", pathDownload.Name, downloadedSize.ToString("N0")),
                    "다운로드 완료");

            IsDownloading = false;
            IsQuering = false;
        }

        private async Task Query()
        {
            if (Article == null) return;

            BusyMessage = "게시물을 불러오는 중 ...";
            IsQuering = true;

            string bbsID = Article.GetString(Constants.PROPERTY_KEY_NAME_BBS_ID);
            string articleID = Article.GetString(Constants.PROPERTY_KEY_NAME_ARTICLE_ID);
            int replyID = Article.GetInt32(Constants.PROPERTY_KEY_NAME_REPLY_ID);
            Uri uriApi = new Uri(string.Format("{0}/read/{1}/{2}/{3}", App.APIServerURL_Article, bbsID, articleID, replyID), UriKind.Absolute);
            ArticleDTO? result = null;

            using (HttpClient client = APIClientHelper.GenerateClient(uriApi, Constants.DEFAULT_VALUE_API_CLIENT_TIMEOUT))
            {
                string data = await client.GetStringAsync(uriApi);
                if (!string.IsNullOrEmpty(data))
                {
                    result = JsonSerializer.Deserialize<ArticleDTO?>(data);
                }
            }

            if (result != null)
            {
                Article = result;
                FlowDocument doc = new FlowDocument();
                TextRange range = new TextRange(doc.ContentStart, doc.ContentEnd);
                
                try
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        byte[] data;

                        try
                        {
                            data = Encoding.UTF8.GetBytes(HttpUtility.HtmlDecode(Article.GetString(Constants.PROPERTY_KEY_NAME_CONTENTS)));
                        }
                        catch (FormatException)
                        {
                            data = Encoding.UTF8.GetBytes(Article.GetString(Constants.PROPERTY_KEY_NAME_CONTENTS));
                        }
                        
                        await stream.WriteAsync(data, 0, data.Length);
                        await stream.FlushAsync();

                        try
                        {
                            stream.Position = 0;
                            range.Load(stream, DataFormats.Xaml);
                        }
                        catch (XamlParseException)
                        {
                            range.Load(stream, DataFormats.Text);
                        }
                        finally
                        {
                            stream.Close();
                        }
                    }

                    if (_view?.FindName("docViewer") is FlowDocumentScrollViewer ctlViewer)
                    {
                        ctlViewer.Dispatcher.Invoke(() =>
                        {
                            ctlViewer.Document = doc;
                        });
                    }

                    AttachedFiles.Clear();

                    if ((!string.IsNullOrEmpty(Article.GetString(Constants.PROPERTY_KEY_NAME_ATTACHED_FILE_NAME))) &&
                        (Article.GetString(Constants.PROPERTY_KEY_NAME_ATTACHED_FILE_NAME).Split(Constants.DEFAULT_VALUE_SEPARATOR) is string[] files) &&
                        (files.Length > 0))
                    {
                        for (int i = 0; i < files.Length; i++)
                        {
                            AttachedFiles.Add(new FilePacket()
                            {
                                FileID = (i + 1).ToString(),
                                FileName = files[i],
                                IsAddedNew = false
                            });
                        }
                    }

                    if ((App.CurrentUser != null) && 
                        (App.CurrentUser.GetString(Constants.PROPERTY_KEY_NAME_CURRENT_USER_ACCOUNT_ID) == (Article.GetString(Constants.PROPERTY_KEY_NAME_WRITER))))
                    {
                        IsOwnedArticle = true;
                    }
                    else
                    {
                        IsOwnedArticle = false;
                    }
                }
                catch (IOException exIO)
                {
                    App.ShowMessageBoxError("게시물 조회 오류 (IO)가 발생하였습니다.", exIO);
                }
                catch (XamlParseException exParse)
                {
                    App.ShowMessageBoxError("게시물 조회 오류 (잘못된 문서 포맷)가 발생하였습니다.", exParse);
                }
                catch (Exception ex)
                {
                    App.ShowMessageBoxError("게시물 조회 오류가 발생하였습니다.", ex);
                }
            }

            await Task.Delay(250);
            IsQuering = false;
        }
    }
}
