using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ELSuitcases.BBS.Library;
using ELSuitcases.BBS.WpfClient.Messages;

namespace ELSuitcases.BBS.WpfClient
{
    internal partial class BoardListViewModel : ViewModelBase
    {
        [ObservableProperty]
        private bool _IsQuering = false;

        [ObservableProperty]
        private string _BoardID = string.Empty;

        [ObservableProperty]
        private int _PageNo = 1;

        [ObservableProperty]
        private int _PageSize = 10;
        
        [ObservableProperty]
        private DTO? _CurrentBoard = null;

        [ObservableProperty]
        private DTO? _CurrentArticle = null;

        [ObservableProperty]
        private List<ArticleDTO>? _ListSource = null;

        [ObservableProperty]
        private Uri _QueryURL = App.APIServerURL_Article ?? new Uri("http://127.0.0.1/");

        #region [COMMAND]
        [ObservableProperty]
        private ICommand _ViewLoadedCommand;
        #endregion



        public BoardListViewModel() : base()
        {
            _ViewLoadedCommand = new AsyncRelayCommand<RoutedEventArgs>(ViewLoadedCommandAction);

            if (!WeakReferenceMessenger.Default.IsRegistered<ArticleListQueryMessage>(this))
            {
                WeakReferenceMessenger.Default.Register<ArticleListQueryMessage>(this, (sender, message) =>
                {
                    message.Reply(OnReceiveQueryRequest(message));
                });
            }
        }



        private async Task<ArticleListQueryResponse> OnReceiveQueryRequest(ArticleListQueryMessage msgQuery)
        {
            ArticleListQueryRequest request = msgQuery.Query;
            ArticleListQueryResponse response;
            Exception? error = null;

            bool result;
            try
            {
                result = await Query(request.RequestURL, request.BoardID);
            }
            catch (Exception ex)
            {
                result = false;
                error = ex;
                Common.PrintDebugFail(ex, GetType().Name, string.Format("게시물 목록 조회 쿼리 오류 : BBS_ID = {0}", request.BoardID));
            }

            if (!result)
                response = new ArticleListQueryResponse(request, 0, false, null, error);
            else
            {
                int countTotal = ((ListSource == null) || (ListSource.Count < 1)) ? 
                                    0 : 
                                    (ListSource?.ElementAt(0)?.GetInt32(Constants.PROPERTY_KEY_NAME_ARTICLES_TOTAL_COUNT) ?? 0);
                response = new ArticleListQueryResponse(request, countTotal, true, ListSource);
            } 

            if (response.IsCompletedSuccessfully)
                Common.PrintDebugInfo(string.Format("게시물 목록 조회 쿼리 성공 : BBS_ID = {0}", request.BoardID), GetType().Name);
            else
                Common.PrintDebugInfo(string.Format("게시물 목록 조회 쿼리 실패 : BBS_ID = {0}", request.BoardID), GetType().Name);

            return response;
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if ((e == null) || (string.IsNullOrEmpty(e.PropertyName)))
                return;

            base.OnPropertyChanged(e);

            switch (e.PropertyName)
            {
                case nameof(IsQuering):
                    App.ChangeBusyIndicatorState(this, IsQuering, "데이터를 조회하는 중 ...");
                    break;

                default:
                    break;
            }
        }

        private async Task ViewLoadedCommandAction(RoutedEventArgs? args)
        {
            await Task.Delay(1);
        }



        private async Task<bool> Query(Uri urlApiRequest, string bbsId)
        {
            if (string.IsNullOrEmpty(bbsId))
                return false;

            bool result = false;
            IsQuering = true;
            QueryURL = urlApiRequest;

            using (var client = APIClientHelper.GenerateClient(QueryURL, Constants.DEFAULT_VALUE_API_CLIENT_TIMEOUT))
            {
                try
                {
                    Common.PrintDebugInfo(QueryURL.ToString(), GetType().Name);

                    string data = await APIClientHelper.Get_String(client, QueryURL);
                    if (!string.IsNullOrEmpty(data))
                        ListSource = JsonSerializer.Deserialize<List<ArticleDTO>>(data);
                    else
                        ListSource = null;

                    result = true;
                }
                catch (Exception)
                {
                    result = false;
                }
            }

            if ((result) && ((CurrentBoard == null) || (bbsId != CurrentBoard.GetString(Constants.PROPERTY_KEY_NAME_BBS_ID))))
            {
                BoardDTO? board = null;
                Uri uriApiBoard = new Uri(string.Format("{0}/{1}", App.APIServerURL_Board, bbsId), UriKind.Absolute);

                using (var clientBoard = APIClientHelper.GenerateClient(uriApiBoard, Constants.DEFAULT_VALUE_API_CLIENT_TIMEOUT))
                {
                    Common.PrintDebugInfo(uriApiBoard.ToString(), GetType().Name);
                    board = JsonSerializer.Deserialize<BoardDTO?>(await APIClientHelper.Get_String(clientBoard, uriApiBoard));
                }

                CurrentBoard = board;
            }

            await Task.Delay(250);
            IsQuering = false;

            return result;
        }
    }
}
