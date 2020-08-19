using UnityEngine;

namespace GameSkills
{
    [CreateAssetMenu(fileName = "Config", menuName = "GameSkills/Config", order = 1)]
    public class Config : ScriptableObject
    {
        public string GameId = "com.company.gamename";
    }
}