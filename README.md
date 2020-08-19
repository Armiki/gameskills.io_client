# GameSkills client api

Main registration page - https://gameskills.io

Download latest release - <a href="https://github.com/Armiki/gameskills.io_client/releases/download/v1.0/gameskills-api-1.0.unitypackage">gameskills-api-1.0.unitypackage</a>


## Instrustions
1. Insert you <b>game_id</b> in GameSkills/Resources/Config
2. Write init code:
   GameSkillsApi.Init("player_guid", "public_player_name");
3. Add button with open page logic
   GameSkillsApi.OpenDefaultWebPage();
   
   
   
## API
### GameSkillsApi.OnLoginCallback 
  Invoke event when success init and login
  
### GameSkillsApi.OnUpdateCallback
  Invoke event when changed tournament model
  
### GameSkillsApi.OnCoinCallback
  Invoke event when player success purchase in api money
  
### GameSkillsApi.CoinCollect(_rewardToken); 
  Send to api success collect purchase
  
### Battle mode
  <b>Start:</b>
  GameSkillsApi.SetMyBattleStatus(_battleRoomId, ApiBattleStatus.start.ToString());<p>
  <b>Win\Lose:</b>
  GameSkillsApi.SetMyBattleStatus(_battleRoomId, ApiBattleStatus.win.ToString());
  GameSkillsApi.SetMyBattleStatus(_battleRoomId, ApiBattleStatus.lose.ToString());
  
  
If need more information, please, open ExampleScript.cs  
