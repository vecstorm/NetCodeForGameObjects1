using TMPro;
using UnityEngine;

public class PlayerLobbyItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textName;

    public void SetPlayerName(string namePlayer)
    {
        textName.text = namePlayer;
    }
}
