using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ELSuitcases.BBS.Library;
using ELSuitcases.BBS.WpfClient.Messages;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace ELSuitcases.BBS.WpfClient
{
    internal partial class BoardDetailViewModel : ViewModelBase
    {
        private ComboBox? cboPageNumbers;
        private Window? winCreateBoardWindow = null;
        private readonly ObservableCollection<Window?> lstWriters = new ObservableCollection<Window?>();



        [ObservableProperty]
        private bool _IsQuering = false;

        [ObservableProperty]
        private int _PageNo = Constants.DEFAULT_VALUE_PAGER_PAGE_NO;

        [ObservableProperty]
        private int _PageSize = Constants.DEFAULT_VALUE_PAGER_PAGE_SIZE;

        public int[] PageSizes
        {
            get
            {
                return new int[7] { Constants.DEFAULT_VALUE_PAGER_PAGE_SIZE, 15, 20, 25, 30, 50, 100 };
            }
        }

        [ObservableProperty]
        private ObservableCollection<int> _PageNumbers = new ObservableCollection<int>();

        [ObservableProperty]
        private string? _SearchKeyword_Title = "";

        [ObservableProperty]
        private List<BoardDTO>? _Boards = null;

        [ObservableProperty]
        private BoardDTO? _CurrentBoard = null;

        #region [COMMAND]
        [ObservableProperty]
        private ICommand _AddBoardCommand;

        [ObservableProperty]
        private ICommand _RefreshBoardsCommand;

        [ObservableProperty]
        private ICommand _SearchTitleCommand;

        [ObservableProperty]
        private ICommand _WriteNewCommand;

        [ObservableProperty]
        private ICommand _ViewLoadedCommand;
        #endregion



        public BoardDetailViewModel() : base()
        {
            _AddBoardCommand = new RelayCommand<RoutedEventArgs>(AddBoardCommandAction);
            _RefreshBoardsCommand = new AsyncRelayCommand(RefreshBoardsCommandAction);
            _SearchTitleCommand = new AsyncRelayCommand(SearchTitleCommandAction);
            _WriteNewCommand = new RelayCommand<RoutedEventArgs>(WriteNewCommandAction);
            _ViewLoadedCommand = new AsyncRelayCommand<RoutedEventArgs>(ViewLoadedCommandAction);

            if (!WeakReferenceMessenger.Default.IsRegistered<BoardListQueryMessage>(this))
            {
                WeakReferenceMessenger.Default.Register<BoardListQueryMessage>(this, (sender, message) =>
                {
                    message.Reply(OnReceiveQueryRequest(message));
                });
            }
        }


        
        protected override async void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if ((e == null) || (string.IsNullOrEmpty(e.PropertyName)))
                return;

            base.OnPropertyChanged(e);

            switch (e.PropertyName)
            {
                case nameof(Boards):
                    if ((Boards != null) && (Boards.Count > 0))
                        CurrentBoard = Boards[0];
                    else
                        CurrentBoard = null;
                    break;
                
                case nameof(CurrentBoard):
                    await QueryListArticles(CurrentBoard?.GetString(Constants.PROPERTY_KEY_NAME_BBS_ID) ?? string.Empty);
                    break;

                case nameof(IsQuering):
                    App.ChangeBusyIndicatorState(this, IsQuering, "데이터를 조회하는 중 ...");
                    break;

                case nameof(PageNo):
                    await QueryListArticles(CurrentBoard?.GetString(Constants.PROPERTY_KEY_NAME_BBS_ID) ?? string.Empty, false);
                    break;

                case nameof(PageSize):
                    await QueryListArticles(CurrentBoard?.GetString(Constants.PROPERTY_KEY_NAME_BBS_ID) ?? string.Empty);
                    break;

                default:
                    break;
            }
        }

        private async Task<BoardListQueryResponse> OnReceiveQueryRequest(BoardListQueryMessage msgQuery)
        {
            BoardListQueryRequest request = msgQuery.Query;
            BoardListQueryResponse response;
            Exception? error = null;

            bool result;
            try
            {
                result = await QueryListBoards(request.RequestURL, true);
            }
            catch (Exception ex)
            {
                result = false;
                error = ex;
                Common.PrintDebugFail(ex, GetType().Name, "게시판 전체 목록 조회 쿼리 오류");
            }

            if (!result)
                response = new BoardListQueryResponse(request, 0, false, null, error);
            else
            {
                int countTotal = ((Boards == null) || (Boards.Count < 1)) ?
                                    0 :
                                    Boards.Count;
                response = new BoardListQueryResponse(request, countTotal, true, Boards);
            }

            if (response.IsCompletedSuccessfully)
                Common.PrintDebugInfo("게시판 전체 목록 조회 쿼리 성공", GetType().Name);
            else
                Common.PrintDebugInfo("게시판 전체 목록 조회 쿼리 실패", GetType().Name);

            return response;
        }

        private void AddBoardCommandAction(RoutedEventArgs? args)
        {
            if (winCreateBoardWindow != null)
            {
                winCreateBoardWindow.Activate();
                return;
            }
            
            BoardCreateViewModel vmBoardCreate = Ioc.Default.GetRequiredService<BoardCreateViewModel>();
            vmBoardCreate.IsUpdateMode = false;

            Window win = new Window()
            {
                Name = string.Format("winAddBoard_{0}", Common.Generate16IdentityCode(DateTime.Now, new Random())),
                Content = vmBoardCreate,
                MinWidth = 420,
                MinHeight = 240,
                Width = 420,
                Height = 240,
                Icon = Application.Current.MainWindow.Icon,
                Owner = Application.Current.MainWindow,
                ResizeMode = ResizeMode.NoResize,
                ShowActivated = true,
                ShowInTaskbar = false,
                Title = vmBoardCreate.Title,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                WindowState = WindowState.Normal,
                WindowStyle = WindowStyle.ToolWindow
            };
            win.Closed += (s, e) =>
            {
                winCreateBoardWindow = null;
                Application.Current.MainWindow.Activate();
            };

            winCreateBoardWindow = win;
            win.Show();
            win.Activate();
        }

        private async Task RefreshBoardsCommandAction()
        {
            await WeakReferenceMessenger.Default.Send(
                        new BoardListQueryMessage(
                                new BoardListQueryRequest(
                                        new Uri(string.Format("{0}/{1}", App.APIServerURL_Board?.ToString(), ""), UriKind.Absolute), 
                                        "")));
        }

        private async Task SearchTitleCommandAction()
        {
            if (CurrentBoard == null)
            {
                InvokeOnUIThread(() =>
                {
                    MessageBox.Show("게시판이 선택되지 않았습니다.\r\n게시판을 먼저 선택한 후, 검색 하십시오.",
                                    "검색",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Exclamation);
                });
                return;
            }

            await Task.Delay(1);

            CurrentBoard.SetValue(Constants.PROPERTY_KEY_NAME_SEARCH_KEYWORD_TITLE, SearchKeyword_Title);
            PageNo = 1;

            await QueryListArticles(CurrentBoard.GetString(Constants.PROPERTY_KEY_NAME_BBS_ID));
        }

        private void WriteNewCommandAction(RoutedEventArgs? args)
        {
            string bbsID = CurrentBoard?.GetValueOrDefault(Constants.PROPERTY_KEY_NAME_BBS_ID, string.Empty).ToString() ?? string.Empty;
            var vmWriter = App.IocServices.GetRequiredService<WriterViewModel>();
            vmWriter.BoardID = bbsID;
            vmWriter.IsUpdateMode = false;
            vmWriter.PageSize = PageSize;

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Window winPopup = new Window()
                {
                    Name = string.Format("winWriter_{0}", Common.Generate16IdentityCode(DateTime.Now, new Random())),
                    Content = vmWriter,
                    MinWidth = 640,
                    MinHeight = 480,
                    Width = 640,
                    Height = 480,
                    Icon = Application.Current.MainWindow.Icon,
                    //Owner = Application.Current.MainWindow,
                    ResizeMode = ResizeMode.CanResizeWithGrip,
                    ShowActivated = true,
                    ShowInTaskbar = true,
                    Title = string.Format("게시글 작성 - {0}", bbsID),
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    WindowState = WindowState.Normal,
                    WindowStyle = WindowStyle.SingleBorderWindow
                };
                winPopup.Closed += (s, e) =>
                {
                    lstWriters.Remove(s as Window ?? null);
                };
                winPopup.Show();
                winPopup.Activate();
            }));
        }

        private async Task ViewLoadedCommandAction(RoutedEventArgs? args)
        {
            var view = args?.Source as BoardDetailView;
            cboPageNumbers = view?.FindName("cboPageNumbers") as ComboBox;

            await RefreshBoardsCommandAction();
        }



        private void CalculatePagesCount(ArticleListQueryResponse response)
        {
            if (PageNumbers == null)
                PageNumbers = new ObservableCollection<int>();
            else
                PageNumbers.Clear();
            PageNumbers.Add(Constants.DEFAULT_VALUE_PAGER_PAGE_NO);

            if (response.TotalQueriedCount > 0)
            {
                int PageCount = response.TotalQueriedCount / response.Request.PageSize;

                if (PageCount > 0)
                {
                    if ((response.TotalQueriedCount % response.Request.PageSize) > 0)
                        PageCount++;

                    PageNumbers.Clear();
                    for (int i = 1; i < PageCount + 1; i++)
                    {
                        PageNumbers.Add(i);
                    }
                }

                PageNo = 1;
            }
            else
                PageNo = 1;

            cboPageNumbers?.Dispatcher.Invoke(() =>
            {
                cboPageNumbers.SelectedIndex = 0;
            });
        }

        private async Task<bool> QueryListArticles(string bbsId, bool isDoCalculatePagesCount = true)
        {
            IsQuering = true;
            bool result = false;

            var msgQuery = new ArticleListQueryMessage(
                            new ArticleListQueryRequest(
                                new Uri(string.Format("{0}/{1}/{2}/{3}/{4}",
                                            App.APIServerURL_Article?.ToString(),
                                            bbsId,
                                            PageSize,
                                            PageNo,
                                            System.Web.HttpUtility.UrlEncode(SearchKeyword_Title ?? string.Empty)), UriKind.Absolute),
                                bbsId,
                                PageSize,
                                PageNo,
                                SearchKeyword_Title ?? string.Empty));
            var msgResult = await WeakReferenceMessenger.Default.Send(msgQuery);

            IsQuering = false;

            if (msgResult == null)
                return false;

            result = msgResult.IsCompletedSuccessfully;

            if (!result)
            {
                InvokeOnUIThread(() =>
                {
                    MessageBox.Show(string.Format("게시물 조회 쿼리 오류가 발생하였습니다.\r\n- 조회 URL : {0}\r\n- 오류 : {1}",
                                            msgResult.Request.RequestURL,
                                            msgResult.Error?.Message),
                                    "조회 쿼리 오류",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);

                });
            }

            if (isDoCalculatePagesCount)
            {
                CalculatePagesCount(msgResult);
            }

            return result;
        }

        private async Task<bool> QueryListBoards(Uri uriApiBoard, bool isOrderyByName = true)
        {
            IsQuering = true;
            bool result = false;

            if (App.APIServerURL_Board != null)
            {
                List<BoardDTO>? qb = null;
                
                using (var client = APIClientHelper.GenerateClient(uriApiBoard, Constants.DEFAULT_VALUE_API_CLIENT_TIMEOUT))
                {
                    string data = await APIClientHelper.Get_String(client, uriApiBoard);
                    if (!string.IsNullOrEmpty(data))
                        qb = JsonSerializer.Deserialize<List<BoardDTO>?>(data);

                }

                if (isOrderyByName)
                {
                    Boards = qb?.OrderBy(new Func<BoardDTO, string>((b) =>
                                    {
                                        return b.GetString(Constants.PROPERTY_KEY_NAME_BBS_NAME);
                                    })).ToList() 
                                    ?? null;
                }
                else
                {
                    Boards = qb?.OrderBy(new Func<BoardDTO, string>((b) =>
                                    {
                                        return b.GetString(Constants.PROPERTY_KEY_NAME_BBS_ID);
                                    })).ToList() 
                                    ?? null;
                }
                
                SearchKeyword_Title = string.Empty;
                result = true;
            }

            IsQuering = false;

            return result;
        }
    }
}
