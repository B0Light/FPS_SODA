using UnityEngine;
using Photon.Pun;

public class ItemPickup : Pickup
{
    public enum Type
    {
        coin, Grenade, heart
    };

    public Type itemType;
    [SerializeField] int Value;
    protected override void Start()
    {
        base.Start();
    }

    protected override void OnPicked(PlayerController byPlayer)
    {
        Debug.Log("PICKUP : " + byPlayer.photonView.ViewID);
        PlayerInventory playerInven = byPlayer.GetComponent<PlayerInventory>();
        if (playerInven)
        {
            switch (itemType)
            {
                case Type.coin:
                    playerInven.Coin += Value;
                    break;
                case Type.Grenade:
                    break;
                case Type.heart:
                    Health health = byPlayer.GetComponent<Health>();
                    if (health != null)
                    {
                        if(ItemId == 0)
                            health.m_health.Value += Value;
                        if (ItemId == 1)
                            health.m_health.Value = Value;
                    }
                    break;
            }
        }
        photonView.RPC("destroyThisObj", RpcTarget.AllBuffered, null);
    }
}
