using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MissileLauncher : MonoBehaviourPun
{
    [SerializeField] GameObject m_missile;
    [SerializeField] Transform m_missileSpawn;
    public BossAtk BossAtk;
    private TBM TBM;
    void Start()
    {
        BossAtk = FindObjectOfType<BossAtk>();
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(Launch());
    }

    IEnumerator Launch()
    {
        yield return new WaitForSeconds(2f);
        if (BossAtk != null && BossAtk.m_target != null)
        {
            GameObject i_tbm = PhotonNetwork.Instantiate(m_missile.name, m_missileSpawn.position, Quaternion.identity);
            TBM = i_tbm.GetComponent<TBM>();
            if (TBM != null)
            {
                TBM.m_target = BossAtk.m_target;
            }
        }
            
        yield return new WaitForSeconds(1f);
        StartCoroutine(Launch());
    }
}
