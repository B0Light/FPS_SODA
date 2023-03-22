using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MissileLauncher : MonoBehaviourPun
{
    [SerializeField] GameObject m_missile;
    [SerializeField] Transform m_missileSpawn;
    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(Launch());
    }

    IEnumerator Launch()
    {
        yield return new WaitForSeconds(3f);
        GameObject i_missile = PhotonNetwork.Instantiate(m_missile.name, m_missileSpawn.position, Quaternion.identity);
        i_missile.GetComponent<Rigidbody>().velocity = Vector3.up * 15f;     
    }
}