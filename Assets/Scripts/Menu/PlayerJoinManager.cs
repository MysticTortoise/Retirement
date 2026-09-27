using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class PlayerJoinManager : MonoBehaviour
{
    [SerializeField] private List<PlayerJoinBox> PlayerUIBoxes = new();

    private static InputUser[] inputUsers = new InputUser[4];
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        #if UNITY_EDITOR
        inputUsers = new InputUser[4];
        #endif

        for(int i = 0; i < inputUsers.Length; i++)
        {
            PlayerUIBoxes[i].UpdateUser(inputUsers[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (InputUser user in inputUsers.Where(u => u.valid).ToArray())
        {
            foreach (InputDevice device in user.pairedDevices.ToArray())
            {
                switch (device)
                {
                    case Gamepad gamepad when gamepad.buttonEast.wasPressedThisFrame:
                    case Joystick joystick when joystick.allControls[2].IsPressed():
                    case Keyboard keyboard when keyboard.qKey.wasPressedThisFrame:
                        RemovePlayer(user);
                        return;
                }
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

    // ReSharper disable Unity.PerformanceAnalysis
    private void AddPlayer(InputDevice device)
    {
        for (int i = 0; i < inputUsers.Length; i++)
        {
            if (!inputUsers[i].valid)
            {
                InputUser user = InputUser.PerformPairingWithDevice(device);
                inputUsers[i] = user;
                PlayerUIBoxes[i].UpdateUser(user);
                return;
            }
        }
        Debug.Log("TOO MANY");
    }

    private void RemovePlayer(InputUser user)
    {
        if (!inputUsers.Contains(user))
            return;

        int pID = Array.IndexOf(inputUsers, user);

        inputUsers.SetValue(null, pID);
        user.UnpairDevicesAndRemoveUser();
        PlayerUIBoxes[pID].UpdateUser(user);
    }

    private bool IsDeviceAssignedToUser(InputDevice device)
    {
        return inputUsers.Any(u => u.valid && u.pairedDevices.Contains(device));
    }
    
}
