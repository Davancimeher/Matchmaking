using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using System.Threading.Tasks;
using System.Threading;
using System;

///ready to implement in unity project
public class MatchmakingManager : MonoBehaviour
{
    public TMP_InputField PlayersCount;
    public TMP_InputField GroupsCount;
    public TextMeshProUGUI MatchFoundTxt;
    public TextMeshProUGUI PlayersInLobbyTxt;

    private Player[] AllPlayersInMM;
    private Player[] Team1;
    private Player[] Team2;

    private int Matchfound;
    public Task< Dictionary<int,int>> RandomizeGroups(int groupsCount)
    {
        Dictionary<int, int> GroupsCountDict = new Dictionary<int, int>();

        for (int i = 1; i < groupsCount; i++)
        {
            GroupsCountDict.Add(i,UnityEngine.Random.Range(2, 6));
        }
        return Task.FromResult < Dictionary<int, int> > (GroupsCountDict);
    }
    public int GetGroup(Dictionary<int, int> GroupsCountDict)
    {
        if (GroupsCountDict.Count <= 0) return 0;

        int groupId = GroupsCountDict.First().Key;

        GroupsCountDict[groupId] = GroupsCountDict[groupId] - 1;

        if (GroupsCountDict.First().Value <= 0)
        {
            GroupsCountDict.Remove(groupId);
        }
        return groupId;
    }
    public async Task< Player[]> RandomizePlayers(int playersCount,int groupsCount)
    {
        List<Player> PlayersRandomized = new List<Player>();

        Dictionary<int, int> GroupsCountDict = await  RandomizeGroups(groupsCount);

        for (int i = 0; i < playersCount; i++)
        {
            Player player = new Player(i, GetGroup(GroupsCountDict));
            PlayersRandomized.Add(player);
            Task.Delay(1).Wait();
        }

        return PlayersRandomized.ToArray(); 
    }

    public async void StartMatchMaking()
    {
        Matchfound = 0;
        AllPlayersInMM = await RandomizePlayers(int.Parse(PlayersCount.text), int.Parse(GroupsCount.text));
        TeamGroupingtest(AllPlayersInMM, Team1, Team2);
    }
    /// create a clone from AllplayersinMM as a liste (easy to manipulate)
    /// create a dictionary<int,list<player> to split our player with groups
    /// create a dictionary<int,int> to handle groupe ID with size
    /// create a list of players containt not grouped players and get them from allplayer list and deleted from all players
    /// get all groupes in dictionary<ID,player list> and dictionary <ID,count of players>
    ///team setting :
    ///if we don't have 10 players,we return without any calcul
    ///if we have 10 players and we don't have any groupe,set 2 team from not grouped players
    ///else set teams,set all possible groupe in teams
    ///if we have 2 teams,return 2 teams,
    ///else if we don't have 2 teams and have enough not grouped players,complete teams from not grouped players
    ///else we don't have enough players to create 2 teams.
    ///  Thank you for your time  
    /// </summary>
    /// <param name="allPlayersInMM"></param>
    /// <param name="Team1"></param>
    /// <param name="Team2"></param>
    private void TeamGroupingtest(Player[] allPlayersInMM,  Player[] Team1, Player[] Team2)
    {
        //if all players < 10 ,return without any calcul
        if (allPlayersInMM.Length < 10)
        {
            MatchFoundTxt.text = Matchfound.ToString();
            PlayersInLobbyTxt.text = AllPlayersInMM.Length.ToString();
            return;
        }
        //get a image of all players in List
        List<Player> AllPlayers = new List<Player>(allPlayersInMM);
        //list of 2 teams

        List<Player> team1 = new List<Player>();
        List<Player> team2 = new List<Player>();

        //list of not grouped players
        List<Player> NonGroupedPlayers = new List<Player>();
        //fill the list of not groupped players
        NonGroupedPlayers = AllPlayers.FindAll(x => x.GroupId == 0);
        //remove all not grouped players from all players
        AllPlayers.RemoveAll(r => r.GroupId == 0);
        //dicionary contient ID of the groupe and list of players


        Dictionary<int, List<Player>> Groupe = new Dictionary<int, List<Player>>();
        //dicionary contient ID of the groupe and numbre of players
        Dictionary<int, int> GroupsInfoDict = new Dictionary<int, int>();
        //splitting players with GroupeID
        foreach (Player _player in AllPlayers)
        {
            if (!Groupe.ContainsKey(_player.GroupId))
            {
                //add the key and value into groupe dictionary(groupeID,list of players)
                Groupe.Add(_player.GroupId, new List<Player> { _player });
                //add the key and value into groupe informations dictionary(groupeID,number of players)
                GroupsInfoDict.Add(_player.GroupId, 1);
            }
            else
            {
                //add the player to his list
                Groupe[_player.GroupId].Add(_player);
                //increase the value of list counts 
                GroupsInfoDict[_player.GroupId]++;
            }
        }



        //set 2 teams from non-grouped players if we don't have any groupe
        if (GroupsInfoDict.Count <= 0)
        {
            if(NonGroupedPlayers.Count < 10)
            {
                // Debug.Log("Not enough Players !");
                return;
            }

          //  Debug.Log("set 2 teams from not grouped players");
            //set 2 teams from not grouped players
            SetFromNonGroupedPlayers(NonGroupedPlayers, team1, team2, 5);
        }

        //we Have some groups
        else
        {
            SetTeams(team1, team2, GroupsInfoDict, Groupe, 5);
            //if we don't have 2 complete teams,and we have enough not grouped players
            if (NonGroupedPlayers.Count >= 10 - (team1.Count + team2.Count))
            {
                SetFromNonGroupedPlayers(NonGroupedPlayers, team1, team2, 5);
            }
            //we don't have enough not grouped players
            else
            {
                MatchFoundTxt.text = Matchfound.ToString();
                PlayersInLobbyTxt.text = AllPlayersInMM.Length.ToString();
                return;
            }
        }
        Team1 = team1.ToArray();
        Team2 = team2.ToArray();

        RemoveMatchedPlayers(team1, team2);

         //Debug.Log("we have " + team1.Count + " players in team 1, and we have " + team2.Count + " players in team 2");
        Matchfound++;
        TeamGroupingtest(AllPlayersInMM, Team1, Team2);
    }
    /// <summary>
    /// set teams from groupe players into team 1 and team 2 referenced by groupe informations and team count
    /// </summary>
    /// <param name="_Team1"></param>
    /// <param name="_Team2"></param>
    /// <param name="_GroupeInformations"></param>
    /// <param name="_Groupes"></param>
    /// <param name="TeamCount"></param>
    public void SetTeams(List<Player> _Team1, List<Player> _Team2, Dictionary<int, int> _GroupeInformations, Dictionary<int, List<Player>> _Groupes, int TeamCount)
    {
        //ordering the dictionary "groupsInfo" descending by groups count
        var OderedGroupInfoDict = from pair in _GroupeInformations orderby pair.Value descending select pair;

        foreach (var GroupInfo in OderedGroupInfoDict)
        {
            if (GetSmallestTeamCount(_Team1, _Team2) + GroupInfo.Value <= TeamCount)
            {
                GetSmallestTeam(_Team1, _Team2).AddRange(_Groupes[GroupInfo.Key]);

               //Debug.Log("we added " + GroupInfo.Value + " players from " + GroupInfo.Key);
            }
            //we have already 2 teams
            if ((_Team1.Count + _Team2.Count).Equals(TeamCount * 2))
            {
                //Debug.Log("we have 2 teams");
                Team1 = _Team1.ToArray();
                Team2 = _Team2.ToArray();
                return;
            }
        }

    }

    public void SetFromNonGroupedPlayers(List<Player> _NonGroupedPlayers, List<Player> _Team1, List<Player> _Team2, int teamcount)
    {
        int team1Index = teamcount - _Team1.Count;

        _Team1.AddRange(_NonGroupedPlayers.GetRange(0,teamcount - _Team1.Count));

        _Team2.AddRange(_NonGroupedPlayers.GetRange(team1Index,teamcount - _Team2.Count));
    }

    /// <summary>
    /// return the smallest team
    /// </summary>
    /// <param name="_Team1"></param>
    /// <param name="_Team2"></param>
    /// <returns></returns>
    public List<Player> GetSmallestTeam(List<Player> _Team1, List<Player> _Team2)
    {
        if (_Team1.Count > _Team2.Count)
        {
            return _Team2;
        }
        else
        {
            return _Team1;
        }
    }
    public int GetSmallestTeamCount(List<Player> _Team1, List<Player> _Team2)
    {
        if (_Team1.Count > _Team2.Count)
        {
            return _Team2.Count;
        }
        else
        {
            return _Team1.Count;
        }
    }

    public void RemoveMatchedPlayers(List<Player> _Team1, List<Player> _Team2)
    {
        var clone= AllPlayersInMM.ToList();
        var  MatchedPlayers = _Team1.Concat(_Team2).ToList();
        clone.RemoveAll(x => MatchedPlayers.Any(y => y.PlayerId == x.PlayerId));
        AllPlayersInMM = clone.ToArray();
    }
}

[System.Serializable]
public struct Player
{
    public int PlayerId;
    //Player with GroupId = 0 => not grouped
    public int GroupId;

    public Player(int playerId, int groupId)
    {
        PlayerId = playerId;
        GroupId = groupId;
    }
}