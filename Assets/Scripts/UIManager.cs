using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TMP_InputField playersCount;
    public TMP_InputField groupsCount;
    public TextMeshProUGUI matchMakedText;
    public TextMeshProUGUI playersInLobbyText;


    public void SetMatchMakingResults(int matchMaked,int playersInlobby)
    {
        matchMakedText.text = matchMaked.ToString();
        playersInLobbyText.text = playersInlobby.ToString();
    } 
}
