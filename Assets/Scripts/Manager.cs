using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Manager : MonoBehaviour
{
    public Transform[] spawn_points;
    public string player_prefab;

    private void Start() {
        Spawn();
    }

    public void Spawn() {
        Transform t_spawn = spawn_points[Random.Range(0, spawn_points.Length)];
        PhotonNetwork.Instantiate(player_prefab, t_spawn.position, t_spawn.rotation);
    }
}
