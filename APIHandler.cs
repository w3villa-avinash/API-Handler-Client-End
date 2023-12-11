using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperOne.Utils;
using UnityEngine;

namespace SuperOne.Network
{
    public class APIHandler : SingletonMonoBehaviour<APIHandler>
    {
        private ApiClient _apiClient;
        private JsonSerializationOption _options;

        private bool _isInitialized, _initializing;
        private Dictionary<string, object> _postDict = new Dictionary<string, object>();
        //private string baseUrl = "https://quickdev4.super.one/qms";


        #region Move this to a separate category data handler

        private Dictionary<int, Texture2D> _categoryImages;
        private Dictionary<int, Texture2D> _subCatIcon;

        public Dictionary<int, Texture2D> CategoryImages { get => _categoryImages; private set => _categoryImages = value; }
        public Dictionary<int, Texture2D> SubCatIcons { get => _subCatIcon; private set => _subCatIcon = value; }
        private List<int> _parentTagsWhichHaveIcons = new List<int>();

        #endregion


        public LeaderBoardEndEvent LeaderBoardEndEventData;

        public ApiClient Client => _apiClient;
        public bool IsInitialized => _isInitialized;
        //game-reader: https://fcwkyf7ggu.super.one/api/
        //game-writer: https://xgudprbn3k.super.one/api/
        //socket: wss://c3abda5vlj.super.one/ws/

#if UNITY_EDITOR
        private void Start()
        {
            //Init();
        }
#endif

        public async Task Init()
        {
            Debug.Log("Init API Client -- Start");
            if (_isInitialized) return;

            while (_initializing || UniversalConstants.UserIdentifier == null)
            {
                Debug.Log($"Init API Client -- Initializing : {_initializing} | UserIdentifier : {UniversalConstants.UserIdentifier != null}");
                await Task.Delay(200);
                if (_isInitialized) return;
            }

            _initializing = true;


            if (UniversalConstants.UserIdentifier != null && UniversalConstants.UserIdentifier.token != null)
            {
                _options = new JsonSerializationOption(UniversalConstants.UserIdentifier);
                _apiClient = new ApiClient(_options);
                _categoryImages = new Dictionary<int, Texture2D>();
                _subCatIcon = new Dictionary<int, Texture2D>();
                _parentTagsWhichHaveIcons = new List<int>();
                _isInitialized = true;

            }
            else
            {
                bool isUserIdentifierNull = UniversalConstants.UserIdentifier == null;
                Debug.Log($"isUserIdentifierNull: {isUserIdentifierNull}");
                //if (!isUserIdentifierNull)
                //Debug.Log($"Has Token: {UniversalConstants.UserIdentifier.token != null}  |  {UniversalConstants.UserIdentifier.playerId}  | {UniversalConstants.PlayerId}");
            }
            //Debug.Log("REFERENCE::::: 111 " + UniversalConstants.UserIdentifier.referenceId);
            _postDict = new Dictionary<string, object>();
            _initializing = false;
            Debug.Log("Init API Client -- Done");
        }

        public async void JoinLobby(int parentTagItemId, int subCategoryId, UInt64 playerId, string uName, string lobbyId, string lang, Action<bool, LobbyDataPOJO, bool> actionOnResponse)
        {
            if (!_isInitialized)
                await Init();

            _postDict.Clear();
            _postDict.Add("objectId", parentTagItemId);
            _postDict.Add("subCategoryId", subCategoryId);
            _postDict.Add("memberId", playerId);
            _postDict.Add("uName", uName);
            _postDict.Add("lobbyId", lobbyId);
            _postDict.Add("lang", lang);
            var postData = MySerializer.Serialize(_postDict);
            var url = _options.UserIdentifier.gameWriter + UniversalConstants.join;

            ////Debug.Log(postData);
            var res = await _apiClient.Post<APIResponse<LobbyDataPOJO>>(url, postData);
            if (res != null && res.success)
            {
                actionOnResponse.Invoke(true, res.data, false);
            }
            else if (res != null && res.success == false && res.message.Contains("User already joined this lobby."))
            {
                GameEvents.Current.ShowMessageDialog(res.message);
                actionOnResponse?.Invoke(false, null, true);
            }
            else
            {
                if (res == null)
                    GameEvents.Current.ShowMessageDialog("Unable to join lobby. Please try again!\n(SOG-001)");
                else if (res.display == true)
                    GameEvents.Current.ShowMessageDialog(res.message);
                actionOnResponse?.Invoke(false, null, false);
            }
        }

        [ContextMenu("TestCall")]
        public void Test()
        {
            Toast.Show("Sfdaskdjfhakfgwaiufgwejchvbsdcjysdcb\nvjhsdbverwuy");
        }

        private int _countdownImageLoadAttempt = 0;
        public async void LoadCountdownImage(int parentTagId, int subCategoryId, Action<bool, CountdownImages> actionOnResponse)
        {
            if (!_isInitialized)
                await Init();

            //const tagId = categoryId;
            var url = $"{_options.UserIdentifier.gameReader}{UniversalConstants.countdownImages}{parentTagId}&subCategoryId={subCategoryId}";
            Debug.Log("CDI 111: " + url);

            _countdownImageLoadAttempt++;
            var res = await _apiClient.Get<APIResponse<CountdownImages>>(url);
            if (res != null && res.success)
            {
                _countdownImageLoadAttempt = 0;
                actionOnResponse.Invoke(true, res.data);
            }
            else
            {
                if (res == null)
                {

                    if (_countdownImageLoadAttempt < 2)
                        LoadCountdownImage(parentTagId, subCategoryId, actionOnResponse);
                    else
                    {

                        _countdownImageLoadAttempt = 0;
                        actionOnResponse?.Invoke(false, null);
                    }
                }
                else if (res.display == true)
                {
                    _countdownImageLoadAttempt = 0;
                    if (!UniversalConstants.IsGameStart)
                        GameEvents.Current.ShowMessageDialog(res.message);
                    actionOnResponse?.Invoke(false, null);
                }
            }
        }

        public async void ChangeCategory(int categoryId, string lobbyId, UInt64 playerId, Action<bool> actionOnResponse)
        {
            if (!_isInitialized)
                await Init();

            _postDict.Clear();
            _postDict.Add("lobbyId", lobbyId);
            _postDict.Add("memeberId", playerId);
            _postDict.Add("objectId", categoryId);
            var postData = MySerializer.Serialize(_postDict);
            var url = _options.UserIdentifier.gameWriter + UniversalConstants.changeCategory;

            var res = await _apiClient.Patch<APIResponse<object>>(url, postData);
            if (res != null && res.success)
            {
                actionOnResponse?.Invoke(true);
            }
            else
            {
                if (res == null)
                    GameEvents.Current.ShowMessageDialog("Unable to change category. Please try again!");
                else if (res.display == true)
                    GameEvents.Current.ShowMessageDialog(res.message);
                actionOnResponse?.Invoke(false);
            }
        }

        public void ClearImages()
        {
            CategoryImages.Clear();
            _categoryImages.Clear();
            _subCatIcon.Clear();
            _parentTagsWhichHaveIcons.Clear();
        }

        public async void ExitLobby(string lobbyId, UInt64 playerId, Action<bool> actionOnResponse, int remainingTimeInGame)
        {
            if (!_isInitialized)
                await Init();

            _postDict.Clear();
            _postDict.Add("lobbyId", lobbyId);
            _postDict.Add("memberId", playerId);
            //_postDict.Add("memeberId", playerId);
            //_postDict.Add("objectId", categoryId);
            var postData = MySerializer.Serialize(_postDict);
            var url = _options.UserIdentifier.gameWriter + UniversalConstants.exitLobby;

            var res = await _apiClient.Patch<APIResponse<object>>(url, postData, remainingTimeInGame);
            if (res != null && res.success)
            {
                actionOnResponse?.Invoke(true);
            }
            else
            {
                LoadingIndicatorLogo.Instance.Hide(3);
                if (res == null)
                    GameEvents.Current.ShowMessageDialog("Unable to Exit lobby. Please try again!\n(SOG-002)");
                else if (res.display == true)
                    GameEvents.Current.ShowMessageDialog(res.message);
                actionOnResponse?.Invoke(false);
            }
        }

        public async void GetCategories(string currentLanguage, Action<bool, CategoryPOJO, int> actionOnResponse)
        {
            if (!_isInitialized)
                await Init();

            var url = $"{_options.UserIdentifier.gameReader}{UniversalConstants.publishedTags}{currentLanguage}";

            var res = await _apiClient.Get<APIResponse<CategoryPOJO>>(url);

            //string path = "Assets/updatedParentTagsJson.json";
            //StreamReader reader = new StreamReader(path);
            //string tempData = reader.ReadToEnd();
            //Debug.Log("Dummy json " + tempData);
            //var Dummy_res = MySerializer.Deserialize<CategoryPOJO>(tempData);
            //actionOnResponse?.Invoke(true, Dummy_res);
            //return;

            if (res != null && res.success)
            {
                actionOnResponse?.Invoke(true, res.data, 200);
            }
            else
            {
                if (res != null && res.display == true)
                {
                    GameEvents.Current.ShowMessageDialog(res.message);
                    actionOnResponse.Invoke(false, null, 404);
                }
                else
                    actionOnResponse?.Invoke(false, null, 400);
            }
        }

        public async void LoadCategoryImages(int id, string url)
        {
            if (!_isInitialized)
                await Init();

            var res = await _apiClient.GetTexture(url);

            if (res != null)
            {
                ////Debug.Log($"Loaded: {id}");
                _categoryImages.Add(id, res);
            }
        }

        public async void LoadSubCatIcons(int id, string url)
        {
            if (!_isInitialized)
                await Init();

            Debug.Log($"Loading: {id}   {url}");
            _parentTagsWhichHaveIcons.Add(id);
            var res = await _apiClient.GetTexture(url);

            if (res != null)
            {
                _subCatIcon.Add(id, res);
            }
        }


        public async void UpdateUsername(string userName, Action<bool> actionOnResponse)
        {
            if (!_isInitialized)
                await Init();

            var url = $"{_options.UserIdentifier.affiliateWriter}{UniversalConstants.updateUserName}";// "https://xgudprbn3k.super.one/api/user/updateUserName";// $"https://quickdev4.super.one/writer/user/updateUserName";

            _postDict.Clear();
            _postDict.Add("userName", userName);
            var postData = MySerializer.Serialize(_postDict);
            var res = await _apiClient.Patch<APIResponse<object>>(url, postData);
            //Debug.Log(url + "  :  " + MySerializer.Serialize(res));
            if (res != null && res.success == true)
            {
                actionOnResponse?.Invoke(true);
            }
            else
            {
                if (res == null)
                    GameEvents.Current.ShowMessageDialog("Something went wrong. Please try again.\n(SOG-003)");
                else if (res.display == true)
                    GameEvents.Current.ShowMessageDialog(res.message);
                actionOnResponse?.Invoke(false);
            }
        }

        public async void GetRules(string currentLanguage, bool isOnclickGameRuleBTN, Action<bool, OfficialRulesPOJO> actinOnResponse)
        {
            if (!_isInitialized)
                await Init();

            var url = $"{_options.UserIdentifier.affiliateReader}{UniversalConstants.gameRules}?lang={currentLanguage}";

            var res = await _apiClient.Get<APIResponse<OfficialRulesPOJO>>(url);

            if (res != null && res.success == true)
            {
                actinOnResponse?.Invoke(true, res.data);
            }
            else
            {
                //LoadingIndicatorLogo.Instance.Hide(7);
                if (isOnclickGameRuleBTN)
                {
                    if (res == null)
                        Toast.Show("Something went wrong. (SOG-005)");
                    else if (res.display == true)
                        Toast.Show(res.message);

                }
                actinOnResponse?.Invoke(false, null);
            }
        }

        public async void GetGameplayImages(Action<bool, GameplayImagesBufferPOJO> actionOnResponse)
        {
            if (!_isInitialized)
                await Init();

            var url = $"{_options.UserIdentifier.gameReader}{UniversalConstants.gameplayImages}?memberId={UniversalConstants.PlayerId}&lobbyId={UniversalConstants.Instance.GameStatus.LobbyId}";
            //Debug.Log(url);
            ////Debug.Log(postData);
            var res = await _apiClient.Get<APIResponse<GameplayImagesBufferPOJO>>(url);
            if (res != null && res.success)
            {
                actionOnResponse?.Invoke(true, res.data);
            }
            else
            {
                if (!UniversalConstants.IsGameStart)
                {

                    if (res == null)
                        Toast.Show("Something went wrong. Unable to fetch gameplay images.\n(SOG-006)");
                    else if (res.display == true)
                        Toast.Show(res.message);
                }
                actionOnResponse?.Invoke(false, null);
            }
        }



        public async void GetAlltypebalance(Action<bool, AlltypebalancePOJO> actionOnResponse)
        {
            if (!_isInitialized)
                await Init();

            _postDict.Clear();
            var url = $"{_options.UserIdentifier.affiliateReader}{UniversalConstants.getAlltypebalance}";
            var res = await _apiClient.Get<APIResponse<AlltypebalancePOJO>>(url);
            if (res != null && res.success)
            {
                actionOnResponse?.Invoke(true, res.data);
            }
            else
            {
                if (res == null)
                    Toast.Show("Something went wrong. (SOG-007)");
                else if (res.display == true)
                    Toast.Show(res.message);
                actionOnResponse?.Invoke(false, null);
            }

        }

        public async void GetFallBackImage(Action<bool, CategoryFallBackImagesPOJO> actionResponse)
        {
            if (!_isInitialized)
                await Init();


            var url = $"{_options.UserIdentifier.gameReader}{ UniversalConstants.fallbackImages}";
            var res = await _apiClient.Get<APIResponse<CategoryFallBackImagesPOJO>>(url);


            if (res != null && res.success)
            {
                actionResponse?.Invoke(true, res.data);
            }
            else
            {
                if (!UniversalConstants.IsGameStart)
                {

                    if (res == null)
                        Toast.Show("Something went wrong. (SOG-008)");
                    else if (res.display == true)
                        Toast.Show(res.message);
                }
                actionResponse?.Invoke(false, null);
            }
        }


        public async void GetTextureFromUrl(string url, Action<Texture2D> textureCallback)
        {
            textureCallback?.Invoke(await _apiClient.GetTexture(url));
        }


        #region MonitoringAPIS

        public async void PushMonitoringData(object data, Action<bool> actionOnResponse)
        {
            if (!_isInitialized)
                await Init();

            _postDict.Clear();
            _postDict.Add("data", data);
            var postData = MySerializer.Serialize(_postDict);
            var url = $"{_options.UserIdentifier.gameWriter}api/game/api-logs";
            try
            {
                var res = await _apiClient.Post<APIResponse<object>>(url, postData);
                if (res.success == true)
                    actionOnResponse?.Invoke(true);
                else actionOnResponse?.Invoke(false);
            }
            catch (Exception e)
            {
                Debug.Log("Data push exception : " + e);
            }
        }

        #endregion

        public async void GetLanguageData(string langCode, Action<bool, Dictionary<string, string>> actionResponse)
        {
            if (!_isInitialized)
                await Init();

            var url = $"{_options.UserIdentifier.affiliateReader}translate/{langCode}/applanguage?device=UNITY";
            var res = await _apiClient.Get<APIResponse<Dictionary<string, string>>>(url);


            if (res != null && res.success)
            {
                actionResponse?.Invoke(true, res.data);
            }
            else
            {
                actionResponse?.Invoke(false, null);
            }
        }

        public async void GetLanguageList(Action<bool, object> actionResponse)
        {
            if (!_isInitialized)
                await Init();

            var url = $"{_options.UserIdentifier.affiliateReader}translate/languages?device=UNITY";
            var res = await _apiClient.Get<APIResponse<object>>(url);


            if (res != null && res.success)
            {
                actionResponse?.Invoke(true, res.data);
            }
            else
            {
                actionResponse?.Invoke(false, null);
            }
        }

        // Move this to a separate category data handler
        public bool TryGetClubIcons(int id, out Texture2D texture)
        {
            if (_parentTagsWhichHaveIcons.Contains(id))
            {
                texture = SubCatIcons.GetValueOrDefault(id);
                return true;
            }
            else
            {
                texture = null;
                return false;
            }
        }


        #region Store

        public async void GetToolsInfo(Action<bool, CreditAndToolCooldown, bool> actionOnResponse)
        {
            if (!_isInitialized)
                await Init();

            var url = _options.UserIdentifier.gameReader + UniversalConstants.getTools;

            ////Debug.Log(postData);
            var res = await _apiClient.Get<APIResponse<CreditAndToolCooldown>>(url);
            if (res != null && res.success)
            {
                actionOnResponse.Invoke(true, res.data, false);
            }
            else
            {
                if (res == null)
                    Toast.Show("Something went wrong. \n(SOG-009)");
                else if (res.display == true)
                    Toast.Show(res.message);
                actionOnResponse?.Invoke(false, null, false);
            }
        }

        public async void PurchaseCredits(string productId, string transactionId, string orderId, string isoCurrencyCode, string localizedPrice, object rawData, Action<bool, UserCredits> actionOnResponse)
        {
            if (!_isInitialized)
                await Init();

            _postDict.Clear();
            _postDict.Add("productId", productId);
            _postDict.Add("transactionId", transactionId);
            _postDict.Add("isoCurrencyCode", isoCurrencyCode);
            _postDict.Add("localizedPrice", localizedPrice);
            _postDict.Add("rawData", rawData);

            var url = _options.UserIdentifier.gameWriter + UniversalConstants.createPurchase;

            var postData = MySerializer.Serialize(_postDict);
            Debug.Log(postData);
            var res = await _apiClient.Post<APIResponse<UserCredits>>(url, _postDict);
            if (res != null && res.success)
            {
                actionOnResponse.Invoke(true, res.data);
            }
            else
            {
                //LoadingIndicatorLogo.Instance.Hide(20);
                if (res == null)
                {
                    //GameEvents.Current.ShowMessageDialog("Something went wrong. Unable to purchase credits.");
                    Toast.Show("Unable to purchase credits! Please try again.\n(SOG-010)");
                    actionOnResponse?.Invoke(false, null);
                }
                else if (res.display == true)
                {
                    GameEvents.Current.ShowMessageDialog(res.message);
                    actionOnResponse?.Invoke(false, res.data);
                    //Toast.Show(res.message);
                }
                else
                {
                    actionOnResponse?.Invoke(false, res.data);
                }
            }
        }

        public async void RefreshTools(List<string> toolIds, Action<bool, CreditAndToolCooldown, bool> actionOnResponse)
        {
            if (!_isInitialized)
                await Init();

            bool toolIdsAvailable = toolIds != null && toolIds.Count > 0;

            _postDict.Clear();
            _postDict.Add("refreshTool", toolIdsAvailable ? "individual" : "all");
            if (toolIdsAvailable)
                _postDict.Add("toolIds", toolIds);
            var postData = MySerializer.Serialize(_postDict);
            var url = _options.UserIdentifier.gameWriter + UniversalConstants.refreshTools;

            ////Debug.Log(postData);
            var res = await _apiClient.Post<APIResponse<CreditAndToolCooldown>>(url, postData);
            if (res != null && res.success)
            {
                actionOnResponse.Invoke(true, res.data, false);
            }
            else
            {
                if (res == null)
                {
                    //GameEvents.Current.ShowMessageDialog("Something went wrong. Unable to fetch tools.");
                    Toast.Show("Unable to refresh tools! Please try again.\n(SOG-011)");
                }
                else if (res.display == true)
                {
                    Toast.Show(res.message);
                    //Toast.Show(res.message);
                }
                actionOnResponse?.Invoke(false, null, false);
            }
        }



        #endregion

        #region ProfileSetting

        public async void LogoutUser(Action<bool, LogOutUser> actionOnResponse)
        {
            Debug.Log("call done");
            if (!_isInitialized)
                await Init();

            _postDict.Clear();
            _postDict.Add("deviceToken", UniversalConstants.UserIdentifier.deviceToken);
            _postDict.Add("deviceType", UniversalConstants.UserIdentifier.DeviceType);
            _postDict.Add("lastLogout", "GAME");

            var postData = MySerializer.Serialize(_postDict);


            var url = _options.UserIdentifier.affiliateWriter + UniversalConstants.logOutUser + UniversalConstants.PlayerId + "/logout";
            var res = await _apiClient.Patch<APIResponse<LogOutUser>>(url, postData);
            if (res != null && res.success)
            {
                actionOnResponse.Invoke(true, res.data);
            }
            else
            {
                if (res == null)
                    Toast.Show("Something went wrong. Please try again.\n(SOG-012)");
                else if (res.display == true)
                    Toast.Show(res.message);
                actionOnResponse?.Invoke(false, null);
            }
        }


        public async void DeleteUserAccount(Action<bool, DeleteUserAccount> actionOnResponse)
        {
            if (!_isInitialized)
                await Init();

            _postDict.Clear();
            _postDict.Add("deviceType", UniversalConstants.UserIdentifier.DeviceType);
            _postDict.Add("deviceToken", UniversalConstants.UserIdentifier.deviceToken);
            var postData = MySerializer.Serialize(_postDict);


            var url = _options.UserIdentifier.affiliateWriter + UniversalConstants.deleteUserAccount;
            var res = await _apiClient.Patch<APIResponse<DeleteUserAccount>>(url, postData);
            if (res != null && res.success)
            {
                actionOnResponse.Invoke(true, res.data);
            }
            else
            {
                if (res == null)
                    Toast.Show("Something went wrong. Please try again.\n(SOG-013)");
                else if (res.display == true)
                    Toast.Show(res.message);
                actionOnResponse?.Invoke(false, null);
            }

        }

        #endregion
    }
}
