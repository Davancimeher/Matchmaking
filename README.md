# Matchmaking Solution
<img src="https://github.com/Davancimeher/MatchmakingTest/blob/main/README.Assets/Matchmaking_Logic.PNG" width="1000">

## Create a simple Matchmaking solution based on Groups
# Generate array of Random players using RandomizeMatchMakingData class

__( " JUST TO TEST THE DESIGN ",in real case,we get only the leader of the group in the list of all players )__

  From UI inputs, we get players count and groups count,to create an array of players and assign them to the random 
  groups.
  
  Example :
  
  Player 1 : Id = 1, groupId = 3  
  
  Player 2 : Id = 2, groupId = 3  
  
  Player 3 : Id = 3, groupId = 0
  
  player 4 : Id = 4, groupId = 0
  
  In this example,Player 1 and Player 2 are in the same group
 
  Player 3 and Player 4 are SOLO players (group ID = 0)
  
# MatchmakingManager class

1- Start the match making with creation of an array of groupInfo (Hold the groupid and the list of players)

2- Sort the groupInfos array descending with players count

3- Looping in Groupinfos Array and set AS POSSIBLE groups in team1 and team 2

4- if we have a match (team1 + team 2 =  PlayerPerMatchCount)

   - Remove matched Players
   
   - Match maked ++
   
   else 
   
   Add the rest of the teams from SOLO Players
   
## Result

<img src="https://github.com/Davancimeher/MatchmakingTest/blob/main/README.Assets/Matchmaking_Output" width="1000">

 
