using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class WeaponSpawner : MonoBehaviour, IPunObservable
{
    [SerializeField] Sprite[] itemSprite;
    [SerializeField] Image image;
    [SerializeField] int Price = 1000;
    [SerializeField] int GetCoin = 0;
    [SerializeField] GameObject[] SpawnItem;
    [SerializeField] int SpawnID;
    [SerializeField] Transform spawnPos;
    [SerializeField] Image fillImg;
    [SerializeField] int setToken = 50;
    [SerializeField] float GetCoinPerSec = 0;

    private void Start()
    {
        image.sprite = itemSprite[SpawnID];
    }

    private void Update()
    {
        fillImg.fillAmount = (float)GetCoin / (float)Price;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(GetCoin);
        }
        else
        {
            this.GetCoin = (int)stream.ReceiveNext();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(GetCoin < Price)
        {
            PlayerInventory inventory = other.GetComponent<PlayerInventory>();
            if (inventory)
            {
                if (inventory.Coin >= setToken)
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
            GetCoin -= Price;
            Spawning();
        }
    }

    public void Spawning()
    {
        PhotonNetwork.Instantiate(SpawnItem[SpawnID].name, spawnPos.position, Quaternion.identity); ;
    }
}
