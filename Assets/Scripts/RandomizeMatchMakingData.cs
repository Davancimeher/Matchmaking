using System;
public class RandomizeMatchMakingData
{

    private Player[] _players = new Player[0];
    private int[] _groups = new int[0];

    private int _lastGroupId;

    /// <summary>
    /// create (playersCount) players and set them in (groupsCount) groups
    /// </summary>
    /// <param name="playersCount"></param>
    /// <param name="groupsCount"></param>
    /// <returns></returns>
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
    /// <summary>
    /// create random groups based on groupscount,and set a random count of players on it
    /// </summary>
    /// <param name="groupsCount"></param>
    private void RandomizeGroups(int groupsCount)
    {
        Array.Resize(ref _groups, groupsCount);

        Random random = new Random();

        for (int i = 0; i < groupsCount; i++)
        {
            var memberCount = random.Next(2, 6);
            _groups[i] = memberCount;
        }
        _lastGroupId = 0;
    }
    /// <summary>
    /// retunr the next group
    /// </summary>
    /// <returns></returns>
    private int GetGroup()
    {
        if (_lastGroupId > _groups.Length - 1) return 0;//if the last group id more than the possible groups id,set the player as a SOLO Player (group id = 0) 

        var idGroup = _lastGroupId;

        _groups[_lastGroupId]--;

        if (_groups[_lastGroupId] <= 0)
        {
            _lastGroupId++;
        }
        return idGroup+1;
    }   
}
