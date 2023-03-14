using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileLauncher : MonoBehaviour
{
    [SerializeField] GameObject m_missile;
    [SerializeField] Transform m_missileSpawn;
    void Start()
    {
        StartCoroutine(Launch());
    }

    IEnumerator Launch()
    {
        yield return new WaitForSeconds(3f);
        GameObject i_missile = Instantiate(m_missile, m_missileSpawn.position, Quaternion.identity);
        i_missile.GetComponent<Rigidbody>().velocity = Vector3.up * 15f;     
    }
}
