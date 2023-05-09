using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class EnemySpawner : MonoBehaviour
{

    public GameObject[] enemies;
    public Transform[] enemySpawn;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void SpawnEnemy(int type)
    {
        GameObject enemy = PhotonNetwork.Instantiate(
            enemies[type].name, enemySpawn[UnityEngine.Random.Range(0, enemySpawn.Length)].position,
                               enemySpawn[UnityEngine.Random.Range(0, enemySpawn.Length)].rotation);

    }
}
