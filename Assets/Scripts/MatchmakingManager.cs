using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


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
/// <summary>
/// holding group information : group ID and players
/// </summary>
public struct GroupInfo : IComparable<GroupInfo>
{
    public int GroupId;
    public Player[] GroupPlayers;

    public GroupInfo(int groupId)
    {
        GroupId = groupId;
        GroupPlayers = new Player[1];
    }
    public int CompareTo(GroupInfo other)
    {
        if (this.GroupPlayers.Length < other.GroupPlayers.Length)
        {
            return 1;
        }
        else if (this.GroupPlayers.Length > other.GroupPlayers.Length)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }
}

/// <summary>
/// Matchmaking class 
/// </summary>
public class MatchmakingManager : MonoBehaviour
{
    const int PlayerPerTeamCount = 5;
    const int PlayerPerMatchCount = PlayerPerTeamCount * 2;

    public RandomizeMatchMakingData randomizePlayers = new RandomizeMatchMakingData();
    public UIManager uIManager; 

    public  Player[] _allPlayersInMM;

    private readonly List<Player> _team1 = new List<Player>();
    private readonly List<Player> _team2 = new List<Player>();

    private  List<Player> _allPlayersList = new List<Player>();
    //list of not grouped players
    private  List<Player> _nonGroupedPlayers = new List<Player>();

    private  GroupInfo[] groupInfos = new GroupInfo[0];

    private int _matchsMaked;

    //called from UI : button start matchmaking
    public void StartMatchMaking()
    {
        _matchsMaked = 0;
        _allPlayersInMM =  randomizePlayers.CreatePlayers(int.Parse(uIManager.playersCount.text), int.Parse(uIManager.groupsCount.text));

        TeamGrouping(_allPlayersInMM);
    }

    /// <summary>
    /// Get array of players and create as possible matchs (depended on PlayerPerMatchCount)
    /// </summary>
    /// <param name="allPlayersInMM">Players Array input</param>
    private void TeamGrouping(Player[] allPlayersInMM)
    {
        ClearLists();

        //if all players < 10 ,return without any calcul
        if (allPlayersInMM.Length < PlayerPerMatchCount)
        {
            uIManager.SetMatchMakingResults(_matchsMaked, _allPlayersInMM.Length);
            return;
        }

        _allPlayersList = new List<Player>(allPlayersInMM);

        //get all solo players from _allplayersList
        _nonGroupedPlayers = _allPlayersList.FindAll(x => x.GroupId == 0);

        //remove all not grouped players from all players
        _allPlayersList.RemoveAll(r => r.GroupId == 0);

        //splitting players with GroupeID
        foreach (Player _player in _allPlayersList)
        {
            int index = Array.FindIndex(groupInfos, id => id.GroupId == _player.GroupId);

            if (index >= 0)
            {
                GroupInfo groupInfo = groupInfos[index];

                Array.Resize(ref groupInfo.GroupPlayers, groupInfo.GroupPlayers.Length + 1);

                groupInfo.GroupPlayers[groupInfo.GroupPlayers.Length - 1] = _player;

                groupInfos[index] = groupInfo;
            }
            else
            {
                Player[] players = { _player };
                GroupInfo groupInfo = new GroupInfo(_player.GroupId);
                groupInfo.GroupPlayers = players;

                Array.Resize(ref groupInfos, groupInfos.Length + 1);
                groupInfos[groupInfos.Length - 1] = groupInfo;
            }
        }

        //set 2 teams from non-grouped players if we don't have any groupe
        if (groupInfos.Length <= 0)
        {
            if (_nonGroupedPlayers.Count < 10)
            {
                Debug.Log("Not enough Players !");
                return;
            }
            //set 2 teams from not grouped players
            SetFromNonGroupedPlayers();
        }

        //we Have some groups
        else
        {
            //order descending  groupinfo with players count
            OrderGroupInfoArray();
            //create teams 
            SetTeams();

            //if we don't have 2 complete teams,and we have enough not grouped players
            if (_nonGroupedPlayers.Count >= PlayerPerMatchCount - (_team1.Count + _team2.Count))
            {
                SetFromNonGroupedPlayers();
            }
            //we don't have enough not grouped players
            else
            {
                uIManager.SetMatchMakingResults(_matchsMaked, _allPlayersInMM.Length);
                return;
            }
        }
        //remove matched players from all players 
        RemoveMatchedPlayers();

        _matchsMaked++;
        //recursive call 
        TeamGrouping(_allPlayersInMM);

    }
    /// <summary>
    /// clear all list used in matchmaking
    /// </summary>
    private void ClearLists()
    {
        _team1.Clear();
        _team2.Clear();
        _allPlayersList.Clear();
        _nonGroupedPlayers.Clear();
        Array.Resize(ref groupInfos, 0);    
    }
    /// <summary>
    /// set teams based on groupinfo (the first priority to groups)
    /// </summary>
    private void SetTeams()
    {
        List<int> usedGroupsIndex = new List<int>();

        foreach (GroupInfo groupInfo in groupInfos)
        {
            //we get the next team or the smalleset team and check if the next group can be added in it 
            if (GetNextTeam().Count + groupInfo.GroupPlayers.Length <= PlayerPerTeamCount)
            {
                GetNextTeam().AddRange(groupInfo.GroupPlayers);
                usedGroupsIndex.Add(groupInfo.GroupId);
            }
            //we have already 2 teams
            if ((_team1.Count + _team2.Count).Equals(PlayerPerMatchCount))
            {
                return;
            }
        }
    }
    /// <summary>
    /// set players from non grouped players or solo players 
    /// </summary>
    private void SetFromNonGroupedPlayers()
    {
        int team1Index = PlayerPerTeamCount - _team1.Count;

        _team1.AddRange(_nonGroupedPlayers.GetRange(0, PlayerPerTeamCount - _team1.Count));

        _team2.AddRange(_nonGroupedPlayers.GetRange(team1Index, PlayerPerTeamCount - _team2.Count));
    }

    /// <summary>
    /// return the smallest team
    /// </summary>
    /// <returns></returns>
    private List<Player> GetNextTeam()
    {
        return _team1.Count > _team2.Count ? _team1 : _team2;
    }
    /// <summary>
    /// remove matched players
    /// </summary>
    private void RemoveMatchedPlayers()
    {
        List<Player> allPlayers = _allPlayersInMM.ToList();
        List<Player> matchedPlayers = _team1;
        matchedPlayers.AddRange(_team2);

        allPlayers.RemoveAll(x => matchedPlayers.Any(y => y.PlayerId == x.PlayerId));
        _allPlayersInMM = allPlayers.ToArray();
    }
    /// <summary>
    /// sort all group info array descending using players count
    /// </summary>
    private void OrderGroupInfoArray()
    {
        Array.Sort(groupInfos);
    }
}
