using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace GameSkills
{
    public class GameSkillsApi : MonoBehaviour
    {
        private readonly string _uri = "https://gameskills.io/game/api";
        private readonly string _defaultPage = "https://gameskills.io/login/";
        private readonly string _version = "1.0.0";
        
        private string _webviewBackBtn;
        private string _gameId;
        private string _playerId;
        private string _playerName;
        private string _socialId;
        private string _pushId;
        private string _platformToken = "";

        private int _updateTime = 5;
        private float _prevTime = 0;
        
        private bool _isLogin = false;
        private static bool _isInit = false;
        private static GameSkillsApi _instance;

        public static string TournamentBannerUrl { get; private set; }
        public static int Notification { get; private set; }
        public static Action OnLoginCallback;
        public static Action<ApiTournamentModel> OnUpdateCallback;
        public static Action<string, string> OnCoinCallback;

        public static void Init(
            string playerId, 
            string playerName = "User", 
            string localeBackBtnName = "Close", 
            string socialId = "none", 
            string pushId = "none")
        {
            if(_isInit) return;
            GameObject go = new GameObject("GameSkillsApi");
            var comp = go.AddComponent<GameSkillsApi>();
            _instance = comp;
            _isInit = true;
            DontDestroyOnLoad(comp);

            _instance._playerId = playerId;
            _instance._playerName = playerName;
            _instance._webviewBackBtn = localeBackBtnName;
            _instance._socialId = socialId;
            _instance._pushId = pushId;

            Config config = Resources.Load<Config>("Config");
            if (config != null)
            {
                _instance._gameId = config.GameId;
                _instance.Login(socialId, pushId);
            }
        }

        private void OnDestroy()
        {
            _isInit = false;
            _isLogin = false;
        }
        
        private void Update()
        {
            if(Time.time - _prevTime < _updateTime) return;
            _prevTime = Time.time;

            try
            {
                if (_isLogin)
                {
                    Dictionary<string, string> dict = new Dictionary<string, string>
                    {
                        {"sid", _playerId},
                        {"platform_token", _platformToken}
                    };

                    StartCoroutine(ApiGet("update", dict, model =>
                    {
                        ApiUpdateModel data = JsonUtility.FromJson<ApiUpdateModel>(model);
                        
                        if (!string.IsNullOrEmpty(data.bill_token))
                        {
                            OnCoinCallback?.Invoke(data.bill_token, data.bill_value);
                        }

                        TournamentBannerUrl = data.platform_banner;
                        _updateTime = data.update_time;
                        Notification = data.notif;
                        _platformToken = data.platform_token;
                        
                        OnUpdateCallback?.Invoke(data.tournaments);
                        if (!string.IsNullOrEmpty(data.platform_need_open_url)) OpenApi(data.platform_need_open_url);
                    }));
                }

                if (_isInit && !_isLogin)
                {
                    Login(_socialId, _pushId);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[GameSkillsApi] " + e.Message);
            }
        }

        private static bool IsInit
        {
            get { 
                if(_isInit == false) 
                    Debug.LogError("[GameSkillsApi] GameSkillsApi not inited, please, use GameSkillsApi.Init() ");
                return _isInit;
            }
        }

        public static void OpenDefaultWebPage()
        {
            if(!IsInit) return;
            _instance.OpenApi(_instance._defaultPage);
        }
        
        private void Login(string social, string push)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>
            {
                {"gameId",  _gameId},
                {"version", _version},
                {"device", SystemInfo.deviceUniqueIdentifier},
                {"social", social},
                {"push", push},
                {"sid", _playerId},
                {"name", _playerName},
            };

            StartCoroutine(ApiGet("login", dict, model =>
            {
                ApiLoginModel data = JsonUtility.FromJson<ApiLoginModel>(model);
                _isLogin = true;
                _platformToken = data.platform_token;
                TournamentBannerUrl = data.platform_banner;
                Notification = data.notif;
                _updateTime = data.update_time;
                
                OnLoginCallback?.Invoke();
                if(data.tournaments != null && !string.IsNullOrEmpty(data.tournaments.roomid))
                    OnUpdateCallback?.Invoke(data.tournaments);
                if(!string.IsNullOrEmpty(data.platform_need_open_url)) 
                    OpenApi(data.platform_need_open_url);
            }));
        }
        
        public static void CoinCollect(string token)
        {
            if(!IsInit) return;
            Dictionary<string, string> dict = new Dictionary<string, string>
            {
                {"sid", _instance._playerId},
                {"platform_token", _instance._platformToken},
                {"bill_token", token}
            };

            _instance.StartCoroutine(_instance.ApiGet("coin_result", dict, model => { }));
        }

        public static void SetMyBattleStatus(string roomId, string status)
        {
            if(!IsInit) return;
            Dictionary<string, string> dict = new Dictionary<string, string>
            {
                {"platform_token", _instance._platformToken},
                {"roomid", roomId},
                {"sid", _instance._playerId},
                {"status", status}
            };
            
            _instance.StartCoroutine(_instance.ApiGet("battle", dict, model =>
            {
                ApiBattleResult data = JsonUtility.FromJson<ApiBattleResult>(model);
                if(!string.IsNullOrEmpty(data.platform_need_open_url)) _instance.OpenApi(data.platform_need_open_url);
            }));
        }
        
        IEnumerator ApiGet(string eventName, Dictionary<string, string> dict, Action<string> resultJson)
        {
            UnityWebRequest www = UnityWebRequest.Post(_uri + "/" + eventName, dict);
            yield return www.SendWebRequest();

            try
            {
                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.LogError("[GameSkillsApi] api get error");
                }
                else
                {
                    string resultText = www.downloadHandler.text;
                    Debug.Log("[GameSkillsApi] " + resultText);
                    resultJson.Invoke(resultText);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[GameSkillsApi] " + e.Message);
            }
        }
        
        private void OpenApi(string url)
        {
            UniWebView webView = Instantiate(Resources.Load<UniWebView>("UniWebView"));

            webView.SetUserAgent($"gid:{_gameId};sid:{_playerId};os:{Application.platform};tkn:{_platformToken}");
        
            webView.SetToolbarDoneButtonText(_webviewBackBtn);
            webView.OnMessageReceived += (view, message) =>
            {
                Debug.Log($"[GameSkillsApi] {message.RawMessage}");
            };

            webView.OnPageStarted += (view, s) =>
            {
                Debug.Log($"[GameSkillsApi] start {s}");
                if (s.Contains("#closewebview"))
                {
                    Destroy(webView.gameObject);
                }
            };

            webView.OnKeyCodeReceived += (view, code) =>
            {
                if (code == 4) {
                    Destroy(webView.gameObject);
                }
            };

            webView.OnPageFinished += (view, code, s) =>
            {
                Debug.Log($"[GameSkillsApi] finish {s}");
            };
            
            webView.Load(url);
            webView.Show();
        }
    }
    
    [Serializable]
    public class ApiLoginModel
    {
        public string platform_token = "";
        public int update_time = 10;
        public ApiTournamentModel tournaments;
        public string platform_banner = "";
        public string platform_need_open_url = "";
        public int notif = 0;
    }

    [Serializable]
    public class ApiUpdateModel
    {
        public string platform_token = "";
        public int update_time = 10;
        public ApiTournamentModel tournaments;
        public string platform_banner = "";
        public string platform_need_open_url = "";
        public string bill_token = "";
        public string bill_value = "";
        public int notif = 0;
    }

    [Serializable]
    public class ApiTournamentModel
    {
        public string status;
        public string roomid;
        public int next_update;
    }

    [Serializable]
    public class ApiBattleResult
    {
        public string platform_need_open_url = "";
    }

    public enum ApiBattleStatus
    {
        none,
        start,
        win,
        lose
    }
}