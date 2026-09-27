
using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.UI;

public class PlayerJoinBox : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI PlayerText;
    [SerializeField] private Image CharacterImage;

    [SerializeField] private Color EnabledColor;
    [SerializeField] private Color DisabledColor;

    [SerializeField] private float BobAmount;
    [SerializeField] private float BobSpeed;
    [SerializeField] private float BobOffset;

    public void UpdateUser(InputDevice device)
    {
        if (device != null)
        {
            PlayerText.text = device.displayName;
            CharacterImage.color = EnabledColor;
        }
        else
        {
            PlayerText.text = "NO PLAYER";
            CharacterImage.color = DisabledColor;
        }
    }

    private void Update()
    {
        CharacterImage.rectTransform.pivot = new Vector2(0.5f, 0.5f + (Mathf.Sin(Time.time * BobSpeed + BobOffset) * BobAmount));
    }
}
