
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

    [SerializeField] private AudioSource JoinSound;
    [SerializeField] private AudioSource LeaveSound;

    public void UpdateUser(InputDevice device, bool playFX)
    {
        if (device != null)
        {
            PlayerText.text = device.displayName;
            CharacterImage.color = EnabledColor;
            if (playFX)
            {
                JoinSound.Play();
            }
        }
        else
        {
            PlayerText.text = "NO PLAYER";
            CharacterImage.color = DisabledColor;
            if (playFX)
            {
                LeaveSound.Play();
            }
        }
    }

    private void Update()
    {
        CharacterImage.rectTransform.pivot = new Vector2(0.5f, 0.5f + (Mathf.Sin(Time.time * BobSpeed + BobOffset) * BobAmount));
    }
}
