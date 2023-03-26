using Photon.Pun;
using System.Collections;
using UnityEngine;

public class TBM : EnemyBullet
{
    Transform m_target = null;

    [SerializeField] float m_speed = 0f;

    float m_currSpeed = 0f;

    void SearchEnemy()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, 100f, m_layerMask);
        if (cols.Length > 0)
        {
            m_target = cols[Random.Range(0, cols.Length)].transform;
        }
    }   

    IEnumerator LaunchDelay()
    {
        yield return new WaitUntil(() => m_rigidbody.velocity.y < 0f);
        yield return new WaitForSeconds(0.1f);
        SearchEnemy();
    }
    protected override void Start()
    {
        GetComponent<Rigidbody>().velocity = Vector3.up * 15f;
        StartCoroutine(LaunchDelay());
        base.Start();
    }

    void Update()
    {
        if(m_target != null)
        {
            if(m_currSpeed <= m_speed)
                m_currSpeed += m_speed * Time.deltaTime;

            transform.position += transform.up * m_currSpeed * Time.deltaTime;

            Vector3 _dir = (m_target.position - transform.position).normalized;
            transform.up = Vector3.Lerp(transform.up, _dir, 0.25f);
        }
    }
}
