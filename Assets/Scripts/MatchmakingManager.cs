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


public class MatchmakingManager : MonoBehaviour
{
    const int playerPerTeamCount = 5;
    const int playerPerMatchCount = playerPerTeamCount * 2;

    public RandomizeMatchMakingData randomizePlayers;
    public UIManager uIManager; 

    public Player[] _allPlayersInMM;

    private List<Player> _team1 = new List<Player>();
    private List<Player> _team2 = new List<Player>();

    private int _matchsMaked;

    //called from UI : button start matchmaking
    public void StartMatchMaking()
    {
        _matchsMaked = 0;
        _allPlayersInMM =  randomizePlayers.CreatePlayers(int.Parse(uIManager.playersCount.text), int.Parse(uIManager.groupsCount.text));

        TeamGroupingtest(_allPlayersInMM);
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
    private void TeamGroupingtest(Player[] allPlayersInMM)
    {
        _team1.Clear();
        _team2.Clear();

        //if all players < 10 ,return without any calcul
        if (allPlayersInMM.Length < playerPerMatchCount)
        {
            uIManager.SetMatchMakingResults(_matchsMaked, _allPlayersInMM.Length);
            return;
        }

        //get a image of all players in List
        List<Player> allPlayers = new List<Player>(allPlayersInMM);

        //list of not grouped players
        List<Player> nonGroupedPlayers = allPlayers.FindAll(x => x.GroupId == 0);

        //remove all not grouped players from all players
        allPlayers.RemoveAll(r => r.GroupId == 0);


        GroupInfo[] groupInfos = new GroupInfo[0];

        //splitting players with GroupeID
        foreach (Player _player in allPlayers)
        {
            var index = Array.FindIndex(groupInfos, id => id.GroupId == _player.GroupId);

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
            if (nonGroupedPlayers.Count < 10)
            {
                Debug.Log("Not enough Players !");
                return;
            }
            //set 2 teams from not grouped players
            SetFromNonGroupedPlayers(nonGroupedPlayers);
        }

        //we Have some groups
        else
        {
            OrderGroupInfoArray(ref groupInfos);

            SetTeams(ref groupInfos);

            //if we don't have 2 complete teams,and we have enough not grouped players
            if (nonGroupedPlayers.Count >= playerPerMatchCount - (_team1.Count + _team2.Count))
            {
                SetFromNonGroupedPlayers(nonGroupedPlayers);
            }
            //we don't have enough not grouped players
            else
            {
                uIManager.SetMatchMakingResults(_matchsMaked, _allPlayersInMM.Length);
                return;
            }
        }

        RemoveMatchedPlayers();

        _matchsMaked++;

        TeamGroupingtest(_allPlayersInMM);

    }
    private void SetTeams(ref GroupInfo[] groupInfos)
    {
        List<int> usedGroupsIndex = new List<int>();

        foreach (var GroupInfo in groupInfos)
        {
            if (GetNextTeam().Count + GroupInfo.GroupPlayers.Length <= playerPerTeamCount)
            {
                GetNextTeam().AddRange(GroupInfo.GroupPlayers);
                usedGroupsIndex.Add(GroupInfo.GroupId);
            }
            //we have already 2 teams
            if ((_team1.Count + _team2.Count).Equals(playerPerMatchCount))
            {
                return;
            }
        }

        if (usedGroupsIndex.Count > 0)
        {
            List<GroupInfo> groupInfoList = new List<GroupInfo>(groupInfos);

            foreach (var usedGroupId in usedGroupsIndex)
            {
                groupInfoList.RemoveAll(r => r.GroupId == usedGroupId);
            }
            groupInfos = groupInfos.ToArray();
        }
    }

    private void SetFromNonGroupedPlayers(List<Player> _NonGroupedPlayers)
    {
        int team1Index = playerPerTeamCount - _team1.Count;

        _team1.AddRange(_NonGroupedPlayers.GetRange(0, playerPerTeamCount - _team1.Count));

        _team2.AddRange(_NonGroupedPlayers.GetRange(team1Index, playerPerTeamCount - _team2.Count));
    }

    /// <summary>
    /// return the smallest team
    /// </summary>
    /// <param name="_Team1"></param>
    /// <param name="_Team2"></param>
    /// <returns></returns>
    private List<Player> GetNextTeam()
    {
        return _team1.Count > _team2.Count ? _team1 : _team2;
    }
    private void RemoveMatchedPlayers()
    {
        var allPlayers = _allPlayersInMM.ToList();
        var MatchedPlayers = _team1;
        MatchedPlayers.AddRange(_team2);

        allPlayers.RemoveAll(x => MatchedPlayers.Any(y => y.PlayerId == x.PlayerId));
        _allPlayersInMM = allPlayers.ToArray();
    }

    private void OrderGroupInfoArray(ref GroupInfo[] _groupInfos)
    {
        Array.Sort(_groupInfos);
    }
}
