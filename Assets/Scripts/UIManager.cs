using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TMP_InputField playersCount;//players count input field 
    public TMP_InputField groupsCount;//groups count input flield 
    public TextMeshProUGUI matchMakedText;//nombre of match maked
    public TextMeshProUGUI playersInLobbyText;//the rest of players in lobby after matchmaking finished 

    /// <summary>
    /// set matchmaking outputs on UI
    /// </summary>
    /// <param name="matchMaked"></param>
    /// <param name="playersInlobby"></param>
    public void SetMatchMakingResults(int matchMaked,int playersInlobby)
    {
        matchMakedText.text = matchMaked.ToString();
        playersInLobbyText.text = playersInlobby.ToString();
    } 
}
