using GameSkills;
using UnityEngine;
using UnityEngine.UI;

public class DebugScript : MonoBehaviour
{
    [SerializeField] private Text _loginText;
    [SerializeField] private Text _tournamentText;
    [SerializeField] private Text _rewardText;

    private void Awake()
    {
        GameSkillsApi.OnLoginCallback += OnApiLogin;
        GameSkillsApi.OnUpdateCallback += OnApiUpdate;
        GameSkillsApi.OnCoinCallback += OnCoinUpdate;
    }

    private void OnApiLogin()
    {
        _loginText.text = "You are online!";
    }

    private void OnApiUpdate(ApiTournamentModel model)
    {
        if (model != null)
            _tournamentText.text = $"scene: {model.roomid}:{model.status}";
        else
            _tournamentText.text = string.Empty;
    }
    
    private void OnCoinUpdate(string token, string reward)
    {
        _rewardText.text = $"reward {reward} with private token {token}";
    }
}
