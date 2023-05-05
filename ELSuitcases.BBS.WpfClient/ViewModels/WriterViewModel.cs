using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using ELSuitcases.BBS.Library;
using ELSuitcases.BBS.WpfClient.Messages;

namespace ELSuitcases.BBS.WpfClient
{
    internal partial class WriterViewModel : ViewModelBase
    {
        private const string MESSAGE_SAVING = "저장하는 중 ...";

        private CancellationTokenSource? ctsSave;
        private RichTextBox? txtContents;
        private PasswordBox? txtPassword;
        private TextBox? txtTitle;



        [ObservableProperty]
        private string _BusyMessage = MESSAGE_SAVING;

        [ObservableProperty]
        private bool _IsSaving = false;

        [ObservableProperty]
        private bool _IsUpdateMode = false;

        [ObservableProperty]
        private bool _IsUploading = false;

        [ObservableProperty]
        private ProgressState _UploadState = new ProgressState("");

        [ObservableProperty]
        private int _MaxLengthOfContents = Constants.DEFAULT_VALUE_ARTICLE_CONTENTS_MAX_LENGTH;

        [ObservableProperty]
        private int _PageSize = Constants.DEFAULT_VALUE_PAGER_PAGE_SIZE;

        [ObservableProperty]
        private ArticleDTO? _UpdateSourceArticle = null;

        [ObservableProperty]
        private string _ViewTitle = "게시글 작성";

        #region [ARTICLE - Writing]
        [ObservableProperty]
        private string _BoardID = string.Empty;

        [ObservableProperty]
        private string _Title = string.Empty;

        [ObservableProperty]
        private string _Contents = string.Empty;

        [ObservableProperty]
        private ObservableCollection<FilePacket> _AttachedFiles = new ObservableCollection<FilePacket>();
        #endregion

        #region [COMMAND]
        [ObservableProperty]
        private ICommand? _ViewLoadedCommand;

        [ObservableProperty]
        private ICommand? _AddFileCommand;

        [ObservableProperty]
        private ICommand? _AttachedFileItemMouseDoubleClickCommand;

        [ObservableProperty]
        private ICommand? _CancelCommand;

        [ObservableProperty]
        private ICommand? _SaveCommand;
        #endregion



        public WriterViewModel() : base() 
        {
            _ViewLoadedCommand = new AsyncRelayCommand<RoutedEventArgs>(ViewLoadedCommandAction);
            _AddFileCommand = new RelayCommand<RoutedEventArgs>(AddFileCommandAction);
            _AttachedFileItemMouseDoubleClickCommand = new RelayCommand<MouseButtonEventArgs>(AttachedFileItemMouseDoubleClickCommandAction);
            _CancelCommand = new AsyncRelayCommand<RoutedEventArgs>(CancelCommandAction);
            _SaveCommand = new AsyncRelayCommand<RoutedEventArgs>(SaveCommandAction);
        }



        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if ((e == null) || (string.IsNullOrEmpty(e.PropertyName)))
                return;

            base.OnPropertyChanged(e);

            switch (e.PropertyName)
            {
                case nameof(Contents):
                    if ((!string.IsNullOrEmpty(Contents)) && (Contents.Length > MaxLengthOfContents))
                    {
                        Contents = Contents.Remove(MaxLengthOfContents);
                    }
                    break;

                case nameof(IsUpdateMode):
                    if (IsUpdateMode)
                        ViewTitle = "게시글 수정";
                    else
                        ViewTitle = "게시글 작성";
                    break;

                case nameof(IsUploading):
                    if (IsUploading)
                        BusyMessage = "업로드 할 첨부파일 확인 중 ...";
                    else
                        BusyMessage = MESSAGE_SAVING;
                    break;

                case nameof(MaxLengthOfContents):
                    if (MaxLengthOfContents < 1)
                        MaxLengthOfContents = Constants.DEFAULT_VALUE_ARTICLE_CONTENTS_MAX_LENGTH;
                    break;

                default:
                    break;
            }
        }

        private async Task ViewLoadedCommandAction(RoutedEventArgs? args)
        {
            if (args?.Source is not WriterView view) return;

            txtPassword = view.FindName("txtPassword") as PasswordBox;
            txtTitle = view.FindName("txtTitle") as TextBox;
            txtContents = view.FindName("txtContents") as RichTextBox;
            MaxLengthOfContents = Int32.MaxValue;

            txtTitle?.Focus();

            if ((IsUpdateMode) && (UpdateSourceArticle != null))
                await LoadArticle(UpdateSourceArticle);
        }

        private void AddFileCommandAction(RoutedEventArgs? args)
        {
            FileDialog dlgFile = new OpenFileDialog()
            {
                CheckFileExists = true,
                Multiselect = true,
                ShowReadOnly = true,
                Title = "첨부파일 선택"
            };

            if (!(dlgFile.ShowDialog() ?? false))
                return;

            AttachedFiles ??= new ObservableCollection<FilePacket>();

            foreach (string f in dlgFile.FileNames)
            {
                FileInfo fi = new FileInfo(f);
                if (fi.Length > Constants.DEFAULT_VALUE_ATTACHED_FILE_ITEM_MAX_LENGTH)
                {
                    string message = string.Format("파일의 크기가 제한된 크기보다 큽니다.\r\n- 파일 : {0}\r\n- 파일 크기 : {1} (바이트)\r\n- 제한된 크기 : {2} (바이트)\r\n\r\n이 파일은 첨부되지 않습니다.",
                                                fi.Name,
                                                fi.Length.ToString("N0"),
                                                Constants.DEFAULT_VALUE_ATTACHED_FILE_ITEM_MAX_LENGTH.ToString("N0"));
                    App.ShowMessageBoxExclamation(message, "첨부파일 크기 제한");
                    continue;
                }

                FilePacket filePack = new FilePacket(this.ID, fi.Name, fi.FullName)
                {
                    IsAddedNew = true,
                    UserState = fi.Name
                };

                if (AttachedFiles.Any(e => e.FileName == filePack.FileName))
                    continue;

                // 수정 모드
                if ((IsUpdateMode) && (AttachedFiles.Any(e => e.FileName == filePack.UserState)))
                    filePack.UserState = fi.Name.Split('.')[0] + "_1" + fi.Extension;

                AttachedFiles.Add(filePack);
            }
        }

        private void AttachedFileItemMouseDoubleClickCommandAction(MouseButtonEventArgs? args)
        {
            if ((args?.Source is not ListBox ctl) ||
                (ctl.SelectedValue is not FilePacket itemFile))
                return;

            if (IsUpdateMode)
            {
                // 수정 모드
                if (itemFile.IsAddedNew)
                    AttachedFiles.Remove(itemFile);
                else
                    itemFile.IsPendingDelete = !itemFile.IsPendingDelete;

            }
            else
                // 신규 모드
                AttachedFiles.Remove(itemFile);
        }

        private async Task CancelCommandAction(RoutedEventArgs? args)
        {
            if (ctsSave != null)
            {
                ctsSave.Cancel();
                ctsSave = null;
            }

            await Task.Delay(100);
            IsSaving = false;
        }

        private async Task SaveCommandAction(RoutedEventArgs? args)
        {
            if ((App.CurrentUser == null) || (App.APIServerBaseURL == null) || (!ValidateForms()))
                return;

            await SaveArticle();
        }



        private async Task LoadArticle(ArticleDTO? article)
        {
            if (article == null) return;

            BoardID = article.GetString(Constants.PROPERTY_KEY_NAME_BBS_ID);
            Title = article.GetString(Constants.PROPERTY_KEY_NAME_TITLE);

            FlowDocument doc = new FlowDocument();
            TextRange range = new TextRange(doc.ContentStart, doc.ContentEnd);

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    byte[] data;

                    try
                    {
                        data = Encoding.UTF8.GetBytes(HttpUtility.HtmlDecode(article.GetString(Constants.PROPERTY_KEY_NAME_CONTENTS)));
                    }
                    catch (FormatException)
                    {
                        data = Encoding.UTF8.GetBytes(article.GetString(Constants.PROPERTY_KEY_NAME_CONTENTS));
                    }

                    await ms.WriteAsync(data, 0, data.Length);
                    ms.Flush();
                    ms.Position = 0;

                    try
                    {
                        if (range.CanLoad(DataFormats.Xaml))
                            range.Load(ms, DataFormats.Xaml);
                        else
                            range.Load(ms, DataFormats.Text);
                    }
                    catch (XamlParseException)
                    {
                        range.Load(ms, DataFormats.Text);
                    }
                }

                txtContents?.Dispatcher.Invoke((() =>
                {
                    txtContents.Document = doc;
                }));

                AttachedFiles.Clear();

                if ((!string.IsNullOrEmpty(article.GetString(Constants.PROPERTY_KEY_NAME_ATTACHED_FILE_NAME))) &&
                    (article.GetString(Constants.PROPERTY_KEY_NAME_ATTACHED_FILE_NAME).Split(Constants.DEFAULT_VALUE_SEPARATOR) is string[] files) &&
                    (files.Length > 0))
                {
                    string packageID = string.Format("PID_{0}_{1}_{2}",
                                                article.GetString(Constants.PROPERTY_KEY_NAME_BBS_ID),
                                                article.GetString(Constants.PROPERTY_KEY_NAME_ARTICLE_ID),
                                                article.GetString(Constants.PROPERTY_KEY_NAME_REPLY_ID));
                    
                    for (int i = 0; i < files.Length; i++)
                    {
                        FilePacket filePack = new FilePacket(packageID, (i + 1).ToString(), files[i])
                        {
                            IsAddedNew = false,
                            IsPendingDelete = false
                        };
                        filePack.UserState = filePack.FileName;

                        AttachedFiles.Add(filePack);
                    }
                }
            }
            catch (IOException exIO)
            {
                App.ShowMessageBoxError("게시물 조회 오류 (IO)가 발생하였습니다.", exIO);
            }
            catch (Exception ex)
            {
                App.ShowMessageBoxError("게시물 조회 오류가 발생하였습니다.", ex);
            }
        }

        private async Task SaveArticle()
        {
            IsSaving = true;

            MessageBoxResult userAnswer = App.ShowMessageBoxQuestion("작성된 내용으로 저장하고 게시합니다.", ViewTitle);
            if (userAnswer == MessageBoxResult.No)
            {
                IsSaving = false;
                return;
            }

            using (MemoryStream ms = new MemoryStream())
            {
                TextRange range = new TextRange(txtContents?.Document.ContentStart, txtContents?.Document.ContentEnd);
                if (range.CanSave(DataFormats.Xaml))
                    range.Save(ms, DataFormats.Xaml);
                else
                    range.Save(ms, DataFormats.Text);

                await ms.FlushAsync();

                using (StreamReader reader = new StreamReader(ms))
                {
                    ms.Position = 0;
                    Contents = HttpUtility.HtmlEncode(reader.ReadToEnd());
                }
            }

            ArticleDTO article;

            if (IsUpdateMode)
            {
                // 수정 모드
                article = new ArticleDTO();
                UpdateSourceArticle?.CopyTo(article);
                article.SetValue(Constants.PROPERTY_KEY_NAME_TITLE, Title);
                article.SetValue(Constants.PROPERTY_KEY_NAME_CONTENTS, Contents);
                if (!string.IsNullOrEmpty(txtPassword?.Password))
                    article.SetValue(Constants.PROPERTY_KEY_NAME_ARTICLE_PASSWORD, txtPassword.Password);
            }
            else
            {
                // 신규 모드
                article = new ArticleDTO(
                                BoardID,
                                Constants.BBSArticleType.General,
                                Title,
                                Contents,
                                txtPassword?.Password ?? string.Empty,
                                App.GetCurrentUserID());
            }

            if (ctsSave == null)
            {
                ctsSave = new CancellationTokenSource();
                ctsSave.Token.Register(() =>
                {
                    App.ShowMessageBoxInformation("게시물 저장이 사용자에 의해 취소 되었습니다.", ViewTitle);
                    IsSaving = false;
                });
            }

            await Task.Run(async () =>
            {
                using (HttpClient client = APIClientHelper.GenerateClient(App.APIServerURL_Article, Constants.DEFAULT_VALUE_API_CLIENT_TIMEOUT))
                {
                    HttpResponseMessage? response = null;

                    if (IsUpdateMode)
                        response = await APIClientHelper.Put(client, App.APIServerURL_Article, article);
                    else
                        response = await APIClientHelper.Post(client, App.APIServerURL_Article, article);

                    await Task.Delay(250);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var dtoPosted = JsonSerializer.Deserialize<ArticleDTO>(await response.Content.ReadAsStringAsync());
                        if (dtoPosted != null)
                        {
                            article.SetValue(Constants.PROPERTY_KEY_NAME_BBS_ID, dtoPosted.GetString(Constants.PROPERTY_KEY_NAME_BBS_ID));
                            article.SetValue(Constants.PROPERTY_KEY_NAME_ARTICLE_ID, dtoPosted.GetString(Constants.PROPERTY_KEY_NAME_ARTICLE_ID));
                            article.SetValue(Constants.PROPERTY_KEY_NAME_REPLY_ID, dtoPosted.GetInt32(Constants.PROPERTY_KEY_NAME_REPLY_ID, 0));
                        }

                        await PostAttachedFiles(
                                article.GetString(Constants.PROPERTY_KEY_NAME_BBS_ID),
                                article.GetString(Constants.PROPERTY_KEY_NAME_ARTICLE_ID),
                                article.GetInt32(Constants.PROPERTY_KEY_NAME_REPLY_ID));
                    }
                }
            }, ctsSave.Token)
            .ContinueWith(async (t) =>
            {
                if (ctsSave != null)
                {
                    ctsSave.Dispose();
                    ctsSave = null;
                }
                if (t.IsCanceled)
                {
                    this.IsSaving = false;
                    return;
                }

                if (t.IsCompletedSuccessfully)
                {
                    App.ShowMessageBoxInformation(
                            "저장 하였습니다.\r\n- 게시물 ID : " + article.GetString(Constants.PROPERTY_KEY_NAME_ARTICLE_ID),
                            (string)this.ViewTitle);

                    var msgQuery = new ArticleListQueryMessage(
                                        new ArticleListQueryRequest(
                                            new Uri(string.Format("{0}/{1}/{2}/{3}/{4}",
                                                        App.APIServerURL_Article?.ToString(),
                                                        (object)this.BoardID,
                                                        (object)this.PageSize,
                                                        1,
                                                        HttpUtility.UrlEncode(string.Empty)), UriKind.Absolute),
                                            (string)this.BoardID,
                                            (int)this.PageSize,
                                            1,
                                            string.Empty));
                    await WeakReferenceMessenger.Default.Send<ArticleListQueryMessage>(msgQuery);

                    InvokeOnUIThread(() => { Window.GetWindow(txtContents)?.Close(); });
                }
                else
                {
                    if (t.Exception != null)
                        App.ShowMessageBoxError("저장 오류가 발생하였습니다.", t.Exception, (string)this.ViewTitle);
                    else
                        App.ShowMessageBoxExclamation("저장을 실패하였습니다.", (string)this.ViewTitle);
                }

                this.IsSaving = false;
            }, ctsSave.Token);
        }

        private async Task<bool> PostAttachedFiles(string bbsId, string articleId, int replyId = 0)
        {
            if ((AttachedFiles == null) || (AttachedFiles.Count < 1))
                return true;

            string packageID = string.Format("PID_{0}_{1}_{2}", bbsId, articleId, replyId);
            bool isSuccess = false;
            Uri uriQueue = new Uri(string.Format("{0}/{1}", App.APIServerURL_Article?.ToString(), Constants.API_ARTICLE_SUBPATH_UPLOAD_QUEUE), UriKind.Absolute);
            Uri uriDequeue = new Uri(string.Format("{0}/{1}", App.APIServerURL_Article?.ToString(), Constants.API_ARTICLE_SUBPATH_UPLOAD_DEQUEUE), UriKind.Absolute);
            Uri uriUpload = new Uri(string.Format("{0}/{1}/{2}/{3}/{4}",
                                            App.APIServerURL_Article?.ToString(),
                                            Constants.API_ARTICLE_SUBPATH_UPLOAD_FILE,
                                            bbsId,
                                            articleId,
                                            replyId),
                                    UriKind.Absolute);
            UploadState = new ProgressState(packageID)
            {
                Percent = 0,
                CancelTokenSource = new CancellationTokenSource()
            };
            UploadState.PercentChanged += (s, e) =>
            {
                Common.PrintDebugInfo(string.Format("첨부파일(들)을 업로드 하는 중 ... ({0} %)", e.NewPercent.ToString("N0")));
                BusyMessage = "첨부파일 업로드 중 ...";
            };

            IsUploading = true;

            using (HttpClient client = APIClientHelper.GenerateClient(uriUpload, Constants.DEFAULT_VALUE_API_CLIENT_TIMEOUT))
            {
                for (int i = 0; i < AttachedFiles.Count; i++)
                {
                    if (IsUpdateMode)
                    {
                        if (AttachedFiles[i].IsPendingDelete)
                        {
                            AttachedFiles[i].IsLastFileInPackage = false;
                            continue;
                        }
                    }

                    AttachedFiles[i].PackageID = packageID;
                    AttachedFiles[i].FileID = (i +  1).ToString();
                    AttachedFiles[i].IsLastFileInPackage = (i == (AttachedFiles.Count - 1));
                }

                await APIClientHelper.Post_UploadAttachedFiles(
                                                                bbsId, 
                                                                articleId, 
                                                                replyId, 
                                                                packageID, 
                                                                AttachedFiles, 
                                                                client, 
                                                                uriQueue, 
                                                                uriDequeue,
                                                                uriUpload, 
                                                                UploadState, 
                                                                Constants.DEFAULT_VALUE_BUFFER_SIZE)
                                        .ContinueWith((t) =>
                                        {
                                            isSuccess = (t.IsCompletedSuccessfully && (t.Exception is null));
                                        });
            }

            IsUploading = false;

            return isSuccess;
        }

        private bool ValidateForms()
        {
            if (string.IsNullOrEmpty(Title))
            {
                App.ShowMessageBoxExclamation("제목을 입력하십시오.", ViewTitle);
                txtTitle?.Focus();
                return false;
            }
            else if ((!IsUpdateMode) && (string.IsNullOrEmpty(txtPassword?.Password ?? string.Empty)))
            {
                App.ShowMessageBoxExclamation("암호를 입력하십시오.", ViewTitle);
                txtPassword?.Focus();
                return false;
            }

            return true;
        }
    }
}
