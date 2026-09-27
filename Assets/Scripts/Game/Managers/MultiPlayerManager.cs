
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.Users;

public class MultiPlayerManager : MonoBehaviour
{
    private PlayerInputManager playerInputManager;

    [SerializeField] private GameObject MonsterPrefab;
    [SerializeField] private GameObject RatPrefab;

    
    
    private void Start()
    {
        if (PlayerJoinManager.inputUsers[0] == null)
        {
            var list = InputSystem.devices.Where(d => d is Gamepad or Keyboard or Joystick).ToArray();
            for (int i = 0; i < Mathf.Min(PlayerJoinManager.inputUsers.Length, list.Length); i++)
            {
                PlayerJoinManager.inputUsers[i] = list[i];
            }
        }
        playerInputManager = GetComponent<PlayerInputManager>();
        SpawnPlayers();
    }

    private void SpawnPlayers()
    {

        var spawns = FindObjectsByType<PlayerSpawn>();
        PlayerSpawn monsterSpawn = spawns.First(s => s.IsMonster);
        PlayerInput monster = PlayerInput.Instantiate(
            MonsterPrefab,
            pairWithDevice: PlayerJoinManager.inputUsers[0],
            playerIndex: 0
        );
        monster.transform.position = monsterSpawn.transform.position;

        var ratSpawns = spawns.Where(s => !s.IsMonster).ToArray();

        for (int i = 1; i < PlayerJoinManager.inputUsers.Length; i++)
        {
            if (PlayerJoinManager.inputUsers[i] == null)
                continue;
            
            PlayerInput rat = PlayerInput.Instantiate(
                RatPrefab,
                pairWithDevice: PlayerJoinManager.inputUsers[i],
                playerIndex: i
            );
            PlayerSpawn spawn = ratSpawns[i - 1];
            rat.transform.position = spawn.transform.position;
        }
    }
}
