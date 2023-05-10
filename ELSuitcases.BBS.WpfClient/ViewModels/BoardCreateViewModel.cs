using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ELSuitcases.BBS.Library;
using ELSuitcases.BBS.WpfClient.Messages;

namespace ELSuitcases.BBS.WpfClient
{
    internal partial class BoardCreateViewModel : ViewModelBase
    {
        private const string MESSAGE_SAVING = "저장하는 중 ...";

        private TextBox? txtBoardID = null;
        private TextBox? txtBoardName = null;



        [ObservableProperty]
        private string _BusyMessage = MESSAGE_SAVING;

        [ObservableProperty]
        private bool _IsQuering = false;

        [ObservableProperty]
        private bool _IsSaving = false;

        [ObservableProperty]
        private bool _IsUpdateMode = false;

        [ObservableProperty]
        private string _Title = "게시판 추가";

        [ObservableProperty]
        private string _BoardID = string.Empty;

        [ObservableProperty]
        private string _BoardName = string.Empty;

        [ObservableProperty]
        private BoardDTO? _UpdateSourceBoard = null;

        #region [COMMAND]
        [ObservableProperty]
        private ICommand _DeleteCommand;

        [ObservableProperty]
        private ICommand _SaveCommand;

        [ObservableProperty]
        private ICommand _ViewLoadedCommand;
        #endregion



        public BoardCreateViewModel() : base()
        {
            _ViewLoadedCommand = new RelayCommand<RoutedEventArgs>(ViewLoadedCommandAction);
            _DeleteCommand = new AsyncRelayCommand<RoutedEventArgs>(DeleteCommandAction);
            _SaveCommand = new AsyncRelayCommand<RoutedEventArgs>(SaveCommandAction);
            IsUpdateMode = false;
        }



        protected override async void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if ((e == null) || (string.IsNullOrEmpty(e.PropertyName)))
                return;

            base.OnPropertyChanged(e);

            switch (e.PropertyName)
            {
                case nameof(BoardID):
                    if ((UpdateSourceBoard = await Query(BoardID)) != null)
                    {
                        IsUpdateMode = true;
                        BoardName = UpdateSourceBoard.GetString(Constants.PROPERTY_KEY_NAME_BBS_NAME);
                    }
                    else
                        IsUpdateMode = false;
                    break;

                case nameof(IsUpdateMode):
                    if (IsUpdateMode)
                    {
                        Title = "게시판 수정";
                    }
                    else
                        Title = "게시판 추가";
                    break;

                default:
                    break;
            }
        }

        private void ViewLoadedCommandAction(RoutedEventArgs? args)
        {
            if (args?.Source is not BoardCreateView view) return;

            txtBoardID = view.FindName("txtBoardID") as TextBox;
            txtBoardName = view.FindName("txtBoardName") as TextBox;

            txtBoardID?.Focus();
        }

        private async Task DeleteCommandAction(RoutedEventArgs? args)
        {
            await Task.Delay(1);

            if (string.IsNullOrEmpty(BoardID))
                return;

            if (App.ShowMessageBoxQuestion(string.Format("{0} 게시판을 삭제 할까요?", BoardID), "게시판 삭제") != MessageBoxResult.Yes)
                return;

            if (await DeleteBoard() == true)
            {
                App.ShowMessageBoxInformation("게시판을 삭제 하였습니다.", BoardID);
                await Close();
            }
        }

        private async Task SaveCommandAction(RoutedEventArgs? args)
        {
            if (!(await ValidateForms()))
                return;

            if ((await SaveBoard()) is BoardDTO board)
            {
                if (IsUpdateMode)
                    App.ShowMessageBoxInformation("게시판을 수정 하였습니다.", BoardID);
                else
                    App.ShowMessageBoxInformation("게시판을 등록 하였습니다.", BoardID);

                await Close();
            }
            else
            {
                if (IsUpdateMode)
                    App.ShowMessageBoxInformation("게시판 수정을 실패 하였습니다.", BoardID);
                else
                    App.ShowMessageBoxInformation("게시판 등록을 실패 하였습니다.", BoardID);
            }
        }



        private async Task Close()
        {
            if (App.APIServerURL_Board != null)
                await WeakReferenceMessenger.Default.Send(new BoardListQueryMessage(
                                                                new BoardListQueryRequest(
                                                                        App.APIServerURL_Board, BoardID)));
            Window.GetWindow(txtBoardID)?.Close();
        }

        private async Task<bool> CheckIsExisted(string bbsId)
        {
            if ((await Query(bbsId)) is BoardDTO)
                return true;
            else
                return false;
        }

        private async Task<BoardDTO?> Query(string bbsId)
        {
            if (string.IsNullOrEmpty(bbsId)) 
                return null;

            IsQuering = true;

            BoardDTO? board = null;
            Uri uriApi = new Uri(string.Format("{0}/{1}", App.APIServerURL_Board?.ToString(), bbsId));

            using (HttpClient client = APIClientHelper.GenerateClient(uriApi, Constants.DEFAULT_VALUE_API_CLIENT_TIMEOUT))
            {
                using (HttpResponseMessage response = await client.GetAsync(uriApi))
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        board = JsonSerializer.Deserialize<BoardDTO>(await response.Content.ReadAsStringAsync());
                    else
                        board = null;
                }
            }

            IsQuering = false;

            return board;
        }

        private async Task<bool> DeleteBoard()
        {
            IsSaving = true;

            bool result = false;
            Uri uriApi = new Uri(string.Format("{0}/{1}", App.APIServerURL_Board?.ToString(), BoardID), UriKind.Absolute);

            using (HttpClient client = APIClientHelper.GenerateClient(uriApi, Constants.DEFAULT_VALUE_API_CLIENT_TIMEOUT))
            {
                HttpResponseMessage? response = null;

                response = await APIClientHelper.Delete(client, uriApi);

                await Task.Delay(250);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    result = true;
                else
                    result = false;
            }

            IsSaving = false;

            return result;
        }

        private async Task LoadBoard()
        {
            var board = await Query(BoardID);

            if (board == null)
                return;

            UpdateSourceBoard = board;
            BoardName = board.GetString(Constants.PROPERTY_KEY_NAME_BBS_NAME);
            IsUpdateMode = true;
        }

        private async Task<BoardDTO?> SaveBoard()
        {
            IsSaving = true;

            BoardDTO? board = new BoardDTO();
            board.SetValue(Constants.PROPERTY_KEY_NAME_BBS_ID, BoardID);
            board.SetValue(Constants.PROPERTY_KEY_NAME_BBS_NAME, BoardName);
            board.SetValue(Constants.PROPERTY_KEY_NAME_BBS_TYPE, Constants.BBSBoardType.General);

            using (HttpClient client = APIClientHelper.GenerateClient(App.APIServerURL_Board, Constants.DEFAULT_VALUE_API_CLIENT_TIMEOUT))
            {
                HttpResponseMessage? response = null;

                if (IsUpdateMode)
                    response = await APIClientHelper.Put(client, App.APIServerURL_Board, board);
                else
                    response = await APIClientHelper.Post(client, App.APIServerURL_Board, board);

                await Task.Delay(250);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    board = JsonSerializer.Deserialize<BoardDTO>(await response.Content.ReadAsStringAsync());
                else
                    board = null;
            }

            IsSaving = false;

            return board;
        }

        private async Task<bool> ValidateForms()
        {
            if (string.IsNullOrEmpty(BoardID))
            {
                App.ShowMessageBoxExclamation("게시판 ID를 입력하십시오.", Title);
                txtBoardID?.Focus();
                return false;
            }
            else if ((!IsUpdateMode) && 
                     (!BoardID.Equals(UpdateSourceBoard?.GetString(Constants.PROPERTY_KEY_NAME_BBS_ID))) && 
                     ((await CheckIsExisted(BoardID))))
            {
                App.ShowMessageBoxExclamation(string.Format("이미 등록 되어있는 게시판 입니다.\r\n\r\n- 게시판 ID : {0}", BoardID), Title);
                BoardID = string.Empty;
                txtBoardID?.Focus();
                return false;
            }
            else if (string.IsNullOrEmpty(BoardName))
            {
                App.ShowMessageBoxExclamation("게시판 이름을 입력하십시오.", Title);
                txtBoardName?.Focus();
                return false;
            }

            return true;
        }
    }
}
