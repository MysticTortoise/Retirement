
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.Users;

public class MultiPlayerManager : MonoBehaviour
{
    private InputDevice[] playerControllers;

    private PlayerInputManager playerInputManager;

    [SerializeField] private GameObject MonsterPrefab;
    [SerializeField] private GameObject RatPrefab;

    
    
    private void Start()
    {
        playerInputManager = GetComponent<PlayerInputManager>();
        SpawnPlayers();
    }

    private void SpawnPlayers()
    {
        if (playerControllers == null)
        {
            playerControllers = InputSystem.devices
                .Where(d => d is Keyboard or Joystick or Gamepad)
                .ToArray();
            
        }

        var spawns = FindObjectsByType<PlayerSpawn>();
        PlayerSpawn monsterSpawn = spawns.First(s => s.IsMonster);
        PlayerInput monster = PlayerInput.Instantiate(
            MonsterPrefab,
            pairWithDevice: playerControllers[0]
        );
        monster.transform.position = monsterSpawn.transform.position;

        var ratSpawns = spawns.Where(s => !s.IsMonster);
        int pCount = 1;
        foreach (PlayerSpawn ratSpawn in ratSpawns)
        {
             PlayerInput rat = PlayerInput.Instantiate(
                RatPrefab,
                pairWithDevice: playerControllers[pCount]
            );
            rat.transform.position = ratSpawn.transform.position;
            pCount++;

            if (pCount >= playerControllers.Length)
            {
                break;
            }
        }
    }
}
