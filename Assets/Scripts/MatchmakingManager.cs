using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
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
public struct GroupInfo
{
    public int GroupId;
    public int GroupCount;
    public Player[] GroupPlayers;

    public GroupInfo(int groupId)
    {
        GroupId = groupId;
        GroupCount = 1;
        GroupPlayers = new Player[1];
    }
}


public class MatchmakingManager : MonoBehaviour
{
    public TMP_InputField playersCount;
    public TMP_InputField groupsCount;
    public TextMeshProUGUI matchFoundText;
    public TextMeshProUGUI playersInLobbyText;

    public Player[] _allPlayersInMM;
    private Player[] _team1;
    private Player[] _team2;

    private int matchsfound;

    private int _lastGroupId;
    private bool _groupInforSorted = false;

    public Task<int[]> RandomizeGroups(int groupsCount)
    {

        int[] groups = new int[groupsCount + 1];

        for (int i = 1; i <= groupsCount; i++)
        {
            var groupCount = UnityEngine.Random.Range(2, 6);
            groups[i] = groupCount;
        }
        _lastGroupId = 1;
        _groupInforSorted = false;

        return Task.FromResult(groups);
    }
    public int GetGroup(int[] _groupsCount)
    {
        if (_lastGroupId > _groupsCount.Length - 1) return 0;

        var idGroup = _lastGroupId;
        _groupsCount[_lastGroupId]--;

        if (_groupsCount[_lastGroupId] <= 0)
        {
            _lastGroupId++;
        }
        return idGroup;
    }
    public async Task<Player[]> RandomizePlayers(int playersCount, int groupsCount)
    {
        Player[] playersRandomized = new Player[playersCount];

        int[] groups = await RandomizeGroups(groupsCount);

        for (int i = 0; i < playersCount; i++)
        {
            Player player = new Player(i, GetGroup(groups));
            playersRandomized[i] = player;
        }
        return playersRandomized;
    }

    public async void StartMatchMaking()
    {
        matchsfound = 0;
        _allPlayersInMM = await RandomizePlayers(int.Parse(playersCount.text), int.Parse(groupsCount.text));
        TeamGroupingtest(_allPlayersInMM, _team1, _team2);
    }
    private void Update()
    {

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
    private void TeamGroupingtest(Player[] allPlayersInMM, Player[] Team1, Player[] Team2)
    {
        //if all players < 10 ,return without any calcul
        if (allPlayersInMM.Length < 10)
        {
            matchFoundText.text = matchsfound.ToString();
            playersInLobbyText.text = _allPlayersInMM.Length.ToString();
            return;
        }
        //get a image of all players in List
        List<Player> allPlayers = new List<Player>(allPlayersInMM);

        //list of 2 teams
        List<Player> team1 = new List<Player>();
        List<Player> team2 = new List<Player>();

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

                groupInfo.GroupCount = groupInfo.GroupPlayers.Length;

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
            SetFromNonGroupedPlayers(nonGroupedPlayers, team1, team2, 5);
        }

        //we Have some groups
        else
        {
            OrderGroupInfoArray(ref groupInfos);
            SetTeams(team1, team2, ref groupInfos, 5);

            //if we don't have 2 complete teams,and we have enough not grouped players
            if (nonGroupedPlayers.Count >= 10 - (team1.Count + team2.Count))
            {
                SetFromNonGroupedPlayers(nonGroupedPlayers, team1, team2, 5);
            }
            //we don't have enough not grouped players
            else
            {
                matchFoundText.text = matchsfound.ToString();
                playersInLobbyText.text = _allPlayersInMM.Length.ToString();
                return;
            }
        }

        Team1 = team1.ToArray();
        Team2 = team2.ToArray();

        RemoveMatchedPlayers(team1, team2);

        matchsfound++;

        TeamGroupingtest(_allPlayersInMM, Team1, Team2);

    }
    public void SetTeams(List<Player> _Team1, List<Player> _Team2, ref GroupInfo[] groupInfos, int TeamCount)
    {
        List<int> usedGroupsIndex = new List<int>();

        foreach (var GroupInfo in groupInfos)
        {
            if (GetSmallestTeamCount(_Team1, _Team2) + GroupInfo.GroupCount <= TeamCount)
            {
                GetSmallestTeam(_Team1, _Team2).AddRange(GroupInfo.GroupPlayers);
                usedGroupsIndex.Add(GroupInfo.GroupId);
            }
            //we have already 2 teams
            if ((_Team1.Count + _Team2.Count).Equals(TeamCount * 2))
            {
                _team1 = _Team1.ToArray();
                _team2 = _Team2.ToArray();
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

    public void SetFromNonGroupedPlayers(List<Player> _NonGroupedPlayers, List<Player> _Team1, List<Player> _Team2, int teamcount)
    {
        int team1Index = teamcount - _Team1.Count;

        _Team1.AddRange(_NonGroupedPlayers.GetRange(0, teamcount - _Team1.Count));

        _Team2.AddRange(_NonGroupedPlayers.GetRange(team1Index, teamcount - _Team2.Count));
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
        var allPlayers = _allPlayersInMM.ToList();
        var MatchedPlayers = _Team1;
        MatchedPlayers.AddRange(_Team2);

        allPlayers.RemoveAll(x => MatchedPlayers.Any(y => y.PlayerId == x.PlayerId));
        _allPlayersInMM = allPlayers.ToArray();
    }
    public void OrderGroupInfoArray(ref GroupInfo[] _groupInfos)
    {
        if (_groupInforSorted) return;
        Array.Sort(_groupInfos, new GroupInfoComparer());
        _groupInforSorted = true;
    }

    public class GroupInfoComparer : IComparer<GroupInfo>
    {
        public int Compare(GroupInfo x, GroupInfo y)
        {
            if (x.GroupCount > y.GroupCount) return -1;
            else return 0;
        }
    }
}
