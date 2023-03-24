using UnityEngine;
using Photon.Pun;

 public class WeaponPickup : Pickup
{
    [Tooltip("The prefab for the weapon that will be added to the player on pickup")]
    public WeaponController WeaponPrefab;

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
        PlayerWeaponsManager playerWeaponsManager = byPlayer.GetComponent<PlayerWeaponsManager>();
        if (playerWeaponsManager)
        {
            if (playerWeaponsManager.AddWeapon(WeaponPrefab))
            {
                if (playerWeaponsManager.GetActiveWeapon() == null)
                {
                    playerWeaponsManager.SwitchWeapon(true);
                }

                PlayPickupFeedback();
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
}
