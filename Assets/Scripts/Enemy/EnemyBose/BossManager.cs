using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Processors;
using Photon.Pun;

public class BossManager : MonoBehaviourPun
{
    [Header("BossObj")]
    [SerializeField] private GameObject Phase1;
    [SerializeField] private GameObject Phase2;
    [Space(10)]
    [SerializeField] private Transform BossTransform;
    [Space(10)]
    [SerializeField] private int CurrPhase = 1;

    [Header("Impact")]
    [SerializeField] private GameObject ImpactVfx;
    [SerializeField] private float ImpactVfxLifetime = 5f;

    private Health Phase1Health;
    private GameObject CurrBoss;
    private BossAtk BossAtk;
    private WeaponController _weaponController;

    private void Start()
    {
        _weaponController = GetComponent<WeaponController>();
        CurrBoss = PhotonNetwork.Instantiate(Phase1.name, BossTransform.position, BossTransform.rotation);
        Phase1Health = CurrBoss.GetComponent<Health>();
        BossAtk = CurrBoss.GetComponent<BossAtk>();
    }

    private void Update()
    {
        if (Phase1Health.isDead && CurrPhase == 1)
            photonView.RPC("NextPhase",RpcTarget.All, null);
    }

    [PunRPC]
    void NextPhase()
    {
        CurrPhase++;
        StartCoroutine(ChangePhase());
    }

    IEnumerator ChangePhase()
    {
        CurrBoss.SetActive(false);
        _weaponController.Owner = gameObject;
        Impact();
        _weaponController.HandleShootInputs(false, true);
        yield return new WaitForSeconds(0.5f); // ADD EFFECT
        var newBoss = PhotonNetwork.Instantiate(Phase2.name, BossTransform.position, BossTransform.rotation);
        BossAtk = newBoss.GetComponent<BossAtk>();
        StartCoroutine(BossAtk.Taunt());
    }

    private void Impact()
    {
        if (ImpactVfx)
        {
            GameObject impactVfxInstance = Instantiate(ImpactVfx, transform.position,
                Quaternion.LookRotation(transform.position));
            if (ImpactVfxLifetime > 0)
            {
                Destroy(impactVfxInstance.gameObject, ImpactVfxLifetime);
            }
        }
    }
}
