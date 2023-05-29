using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerImgManager : MonoBehaviour
{
    [SerializeField] GameManager GM;
    protected GameObject player;
    private Vector3 targetPosition;
    private void Start()
    {
        GM = FindObjectOfType<GameManager>();
        
    }
    void Update()
    {
        if (GM != null)
        {
            player = GM.player;
            if (player != null)
            {
                targetPosition = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
                transform.LookAt(targetPosition);
            }
        } 
    }
}
