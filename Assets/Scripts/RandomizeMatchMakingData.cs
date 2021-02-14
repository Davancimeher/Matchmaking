using System;
using UnityEngine;

public class RandomizeMatchMakingData : MonoBehaviour
{

    private Player[] _players = new Player[0];
    private int[] _groups = new int[0];

    private int _lastGroupId;


    public Player[] CreatePlayers(int playersCount, int groupsCount)
    {
        Array.Resize(ref _players, playersCount);

        RandomizeGroups(groupsCount);

        for (int i = 0; i < playersCount; i++)
        {
            Player player = new Player(i, GetGroup());
            _players[i] = player;
        }
        return _players;
    }

    private void RandomizeGroups(int groupsCount)
    {
        Array.Resize(ref _groups, groupsCount);

        for (int i = 0; i < groupsCount; i++)
        {
            var groupCount = UnityEngine.Random.Range(2, 6);
            _groups[i] = groupCount;
        }
        _lastGroupId = 0;
    }

    private int GetGroup()
    {
        if (_lastGroupId > _groups.Length - 1) return 0;

        var idGroup = _lastGroupId;

        _groups[_lastGroupId]--;

        if (_groups[_lastGroupId] <= 0)
        {
            _lastGroupId++;
        }
        return idGroup+1;
    }   
}
