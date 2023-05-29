using Photon.Pun;
using System.Collections;
using UnityEngine;

public class TBM : EnemyBullet
{
    public GameObject m_target = null;

    [SerializeField] float m_speed = 0f;

    float m_currSpeed = 0f;


    IEnumerator LaunchDelay()
    {
        yield return new WaitUntil(() => m_rigidbody.velocity.y < 0f);
        yield return new WaitForSeconds(0.1f);
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

            Vector3 _dir = (m_target.transform.position - transform.position).normalized;
            transform.up = Vector3.Lerp(transform.up, _dir, 0.5f);
        }
    }
}
