
using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Users;

public class PlayerJoinBox : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI PlayerText;

    public void UpdateUser(InputUser user)
    {
        if (user.valid)
        {
            PlayerText.text = user.pairedDevices[0].displayName;
        }
        else
        {
            PlayerText.text = "NO PLAYER";
        }
        
    }
}
