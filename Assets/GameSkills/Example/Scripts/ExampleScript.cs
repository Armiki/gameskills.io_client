using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GameSkills
{
    public class ExampleScript : MonoBehaviour
    {
        [SerializeField]private RawImage _rawImage;
        [SerializeField]private Button _joinTournamentBtn;
        [SerializeField]private Button _battleWinBtn;
        [SerializeField]private Button _battleLoseBtn;

        private string _battleRoomId = String.Empty;
        private string _rewardToken = String.Empty;
        
        private void Start()
        {
            GameSkillsApi.Init("player_0", "DemoPlayerName");
            GameSkillsApi.OnLoginCallback += OnLogin;
            GameSkillsApi.OnUpdateCallback += OnApiUpdate;
            GameSkillsApi.OnCoinCallback += OnCoinUpdate;
        }

        private void OnLogin()
        {
            StartCoroutine(LoadBannerImage(GameSkillsApi.TournamentBannerUrl));
        }

        IEnumerator LoadBannerImage(string url)
        {
            Debug.Log(url);
            WWW www = new WWW(url);
            yield return www;
            
            if (www.error == null || string.IsNullOrEmpty(www.error))
            {
                if(_rawImage.texture != null)
                    Destroy(_rawImage.texture);
            
                _rawImage.texture = www.texture;
                _rawImage.gameObject.SetActive(true);
            }
        }

        private void OnApiUpdate(ApiTournamentModel model)
        {
            if (model != null)
            {
                _joinTournamentBtn.interactable = true;
                _battleWinBtn.interactable = true;
                _battleLoseBtn.interactable = true;
                
                _battleRoomId = model.roomid;
            }
            else
            {
                _joinTournamentBtn.interactable = false;
                _battleWinBtn.interactable = false;
                _battleLoseBtn.interactable = false;
                
                _battleRoomId = String.Empty;
            }
        }
        
        private void OnCoinUpdate(string token, string reward)
        {
            _rewardToken = token;
        }
        
        public void OpenPage()
        {
            GameSkillsApi.OpenDefaultWebPage();
        }

        public void BattleStart()
        {
            if(!string.IsNullOrEmpty(_battleRoomId))
                GameSkillsApi.SetMyBattleStatus(_battleRoomId, ApiBattleStatus.start.ToString());
        }
        
        public void BattleWin()
        {
            if(!string.IsNullOrEmpty(_battleRoomId))
                GameSkillsApi.SetMyBattleStatus(_battleRoomId, ApiBattleStatus.win.ToString());
        }
        
        public void BattleLose()
        {
            if(!string.IsNullOrEmpty(_battleRoomId))
                GameSkillsApi.SetMyBattleStatus(_battleRoomId, ApiBattleStatus.lose.ToString());
        }
        
        public void CollectReward()
        {
            if(!string.IsNullOrEmpty(_rewardToken))
                GameSkillsApi.CoinCollect(_rewardToken);
        }
    }
}