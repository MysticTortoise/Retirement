
using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class PlayerJoinBox : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI PlayerText;

    public void UpdateUser(InputDevice device)
    {
        if (device != null)
        {
            PlayerText.text = device.displayName;
        }
        else
        {
            PlayerText.text = "NO PLAYER";
        }
        
    }
}
