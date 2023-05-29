using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemyManager : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private GameObject[] Enemy;
    [SerializeField] private Transform[] EnemySpawnPos;
    [Space(10)]
    public int currEnemy = 0;
    [SerializeField] private int setEnemy;
    [SerializeField] private Transform[] EnemyPath;
    // Update is called once per frame
    void Update()
    {
        if(PhotonNetwork.IsMasterClient)
            if(currEnemy < setEnemy)
                StartCoroutine(spawn());
    }

    IEnumerator spawn()
    {
        if(!PhotonNetwork.IsMasterClient) yield return null;

        yield return new WaitForSeconds(5f);
        if (currEnemy < setEnemy)
        {
            int rand_T = Random.Range(0, Enemy.Length);
            int rand_P = Random.Range(0, EnemySpawnPos.Length);
            GameObject NewEnemy = PhotonNetwork.Instantiate(Enemy[rand_T].name, EnemySpawnPos[rand_P].position, Quaternion.identity);
            EnemyPath enemyPath = NewEnemy.GetComponent<EnemyPath>();
            if (enemyPath != null)
            {
                enemyPath.m_patrolNode = EnemyPath;
            }
            EnemyController enemyController = NewEnemy.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.enemyManager = this;
            }
            currEnemy += 1;
            
        }
        
    }
}
