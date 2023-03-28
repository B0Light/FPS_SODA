using UnityEngine;
using Photon.Pun;

 public class WeaponPickup : Pickup
{
    protected override void Start()
    {
        base.Start();

        // Set all children layers to default (to prefent seeing weapons through meshes)
        foreach (Transform t in GetComponentsInChildren<Transform>())
        {
            if (t != transform)
                t.gameObject.layer = 0;
        }
    }

    protected override void OnPicked(PlayerController byPlayer)
    {
        Debug.Log("PICKUP : "+byPlayer.photonView.ViewID);
        PlayerWeaponsManager playerWeaponsManager = byPlayer.GetComponent<PlayerWeaponsManager>();
        if (playerWeaponsManager)
        {
            Debug.Log("PICKUP : " + byPlayer.photonView.ViewID);
            if (playerWeaponsManager.AddWeapon(ItemId))
            {
                PhotonNetwork.Destroy(this.gameObject);
                PlayPickupFeedback();
            }
        }
    }


}
