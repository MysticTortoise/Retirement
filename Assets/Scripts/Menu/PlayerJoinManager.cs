using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.SceneManagement;

public class PlayerJoinManager : MonoBehaviour
{
    [SerializeField] private List<PlayerJoinBox> PlayerUIBoxes = new();

    public static InputDevice[] inputUsers = new InputDevice[4];

    [SerializeField] private InputActionReference StartAction;

    [SerializeField] private TextMeshProUGUI CountdownText;
    [SerializeField] private int CountdownSeconds;
    private bool starting;
    private float countdownTimer;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        #if UNITY_EDITOR
        inputUsers = new InputDevice[4];
        #endif

        for(int i = 0; i < inputUsers.Length; i++)
        {
            PlayerUIBoxes[i].UpdateUser(inputUsers[i]);
        }
        StartAction.action.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        if (!starting)
        {
            CheckPlayerStatuses();
        }
        else
        {
            countdownTimer -= Time.deltaTime;
            CountdownText.text = Mathf.CeilToInt(countdownTimer).ToString();

            if (countdownTimer <= 0)
            {
                SceneManager.LoadScene("Game");
            }
        }

        if (StartAction.action.WasPressedThisFrame())
        {
            if (!starting)
            {
                BeginStartGame();
            }
            else
            {
                CancelStartGame();
            }
        }
    }

    private void CheckPlayerStatuses()
    {
        foreach (InputDevice device in inputUsers.Where(u => u != null).ToArray())
        {
            switch (device)
            {
                case Gamepad gamepad when gamepad.buttonEast.wasPressedThisFrame:
                case Joystick joystick when joystick.allControls[2].IsPressed():
                case Keyboard keyboard when keyboard.qKey.wasPressedThisFrame:
                    RemovePlayer(device);
                    break;
            }
        }

        foreach (InputDevice inputDevice in InputSystem.devices.Where(inputDevice => !IsDeviceAssignedToUser(inputDevice)))
        {
            switch (inputDevice)
            {
                case Gamepad gamepad when gamepad.buttonSouth.wasPressedThisFrame:
                case Joystick joystick when joystick.trigger.wasPressedThisFrame:
                case Keyboard keyboard when keyboard.spaceKey.wasPressedThisFrame:
                    AddPlayer(inputDevice);
                    break;
            }
        }
    }

    private void BeginStartGame()
    {
        if (inputUsers[0] == null)
        {
            return;
        }

        if (inputUsers.Count(d => d != null) < 2)
        {
            return;
        }
        
        starting = true;
        countdownTimer = CountdownSeconds;
        CountdownText.gameObject.SetActive(true);
    }

    private void CancelStartGame()
    {
        starting = false;
        CountdownText.gameObject.SetActive(false);
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void AddPlayer(InputDevice device)
    {
        for (int i = 0; i < inputUsers.Length; i++)
        {
            if (inputUsers[i] == null)
            {
                inputUsers[i] = device;
                PlayerUIBoxes[i].UpdateUser(device);
                return;
            }
        }
        Debug.Log("TOO MANY");
    }

    private void RemovePlayer(InputDevice device)
    {
        if (!inputUsers.Contains(device))
            return;

        int pID = Array.IndexOf(inputUsers, device);

        inputUsers.SetValue(null, pID);
        PlayerUIBoxes[pID].UpdateUser(null);
    }

    private bool IsDeviceAssignedToUser(InputDevice device)
    {
        return inputUsers.Contains(device);
    }
    
}
