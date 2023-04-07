using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Photon.Pun;

public class WeaponSpawner : MonoBehaviour
{

    [SerializeField] int Price = 1000;
    [SerializeField] int GetCoin = 0;
    [SerializeField] GameObject[] SpawnItem;
    [SerializeField] int SpawnID;
    [SerializeField] Transform spawnPos;
    [SerializeField] Image fillImg;
    [SerializeField] int setToken = 50;
    [SerializeField] float GetCoinPerSec = 0;

    private void Update()
    {
        fillImg.fillAmount = (float)GetCoin / (float)Price;
    }

    private void OnTriggerStay(Collider other)
    {
        if(GetCoin < Price)
        {
            PlayerInventory inventory = other.GetComponent<PlayerInventory>();
            if (inventory)
            {
                if (inventory.Coin > setToken)
                {
                    GetCoinPerSec += (setToken) * Time.deltaTime;
                    if(GetCoinPerSec >= setToken)
                    {
                        inventory.Coin -= 50;
                        GetCoin += 50;
                        GetCoinPerSec = 0;
                    }
                    
                }
            }
            
        }
        else if(GetCoin >= Price)
        {
            GameObject player = other.gameObject;
            GetCoin -= Price;
            Spawning(player);
        }
    }

    public void Spawning(GameObject player)
    {
        PhotonNetwork.Instantiate(SpawnItem[SpawnID].name, player.transform.forward, Quaternion.identity);
    }
}
