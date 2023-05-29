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
    [SerializeField] MissileLauncher _missileLauncher1;
    [SerializeField] MissileLauncher _missileLauncher2;
    private void Start()
    {
        _weaponController = GetComponent<WeaponController>();
        if (PhotonNetwork.IsMasterClient)
        {
            CurrBoss = PhotonNetwork.Instantiate(Phase1.name, BossTransform.position, BossTransform.rotation);
            Phase1Health = CurrBoss.GetComponent<Health>();
            BossAtk = CurrBoss.GetComponent<BossAtk>();
            _missileLauncher1.BossAtk = BossAtk;
            _missileLauncher2.BossAtk = BossAtk;
        } 
    }

    private void Update()
    {
        if(PhotonNetwork.IsMasterClient)
            if (Phase1Health.isDead && CurrPhase == 1)
                photonView.RPC("NextPhase",RpcTarget.All, null);
    }

    [PunRPC]
    void NextPhase()
    {
        CurrPhase++;
        _weaponController.Owner = gameObject;
        Impact();
        _weaponController.HandleShootInputs(false, true);
        StartCoroutine(ChangePhase());
    }

    IEnumerator ChangePhase()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            CurrBoss.SetActive(false);
            PhotonNetwork.Destroy(CurrBoss);
            yield return new WaitForSeconds(0.5f);
            var newBoss = PhotonNetwork.Instantiate(Phase2.name, BossTransform.position, BossTransform.rotation);
            BossAtk = newBoss.GetComponent<BossAtk>();
            _missileLauncher1.BossAtk = BossAtk;
            _missileLauncher2.BossAtk = BossAtk;
            BossAtk.setAni3();
            StartCoroutine(BossAtk.Taunt());
        }
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
