using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAtk : MonoBehaviour
{

    public enum Type
    {
        A, B, C, D,
    };

    EnemyController _EnemyController;
    public Type enemyType;
    public float m_dmg;
    public float m_targetRadius = 1.5f;
    public float m_targetRange = 1f;
    public GameObject bullet;
    
    private void Awake() {
        _EnemyController = GetComponent<EnemyController>();
    }

    void Targeting(){
        RaycastHit[] rayHits = Physics.SphereCastAll(transform.position,
                m_targetRadius, transform.forward, m_targetRange, LayerMask.GetMask("Player"));
            if (rayHits.Length > 0 && !_EnemyController.m_isAtk)
            {
                StartCoroutine(Atk());
            }
    }

    IEnumerator Atk()
    {
        _EnemyController.m_isChase = false;
        _EnemyController.m_isAtk = true;

        switch (enemyType)
        {
            case Type.A:
                yield return new WaitForSeconds(0.2f);
                MeleeAtk();
                break;
            case Type.B:
                yield return new WaitForSeconds(0.1f);
                Rigidbody _rbody = GetComponent<Rigidbody>();
                if (_rbody)
                {
                    _rbody.AddForce(transform.forward * 40, ForceMode.Impulse);
                }
                MeleeAtk();
                yield return new WaitForSeconds(0.5f);
                _rbody.velocity = Vector3.zero;
                yield return new WaitForSeconds(2f);
                break;
            case Type.C:
                yield return new WaitForSeconds(0.5f);
                
                GameObject instantBullet = Instantiate(bullet, transform.position, transform.rotation);
                Rigidbody rbodyBullet = instantBullet.GetComponent<Rigidbody>();
                rbodyBullet.velocity = transform.forward * Random.Range(20f, 60f);
                yield return new WaitForSeconds(2f);
                break;
            case Type.D:
                yield return new WaitForSeconds(0.5f);
                
                Vector3 GrenadePos = Vector3.zero;
                GrenadePos.y = 15;
                GameObject instantGrenade = Instantiate(bullet, transform.position, transform.rotation);
                Rigidbody rbodyGrenade = instantGrenade.GetComponent<Rigidbody>();
                rbodyGrenade.velocity = transform.forward * 20;
                
                rbodyGrenade.AddForce(GrenadePos, ForceMode.Impulse);
                rbodyGrenade.AddTorque(Vector3.back * 15, ForceMode.Impulse);

                yield return new WaitForSeconds(2f);
                
                break;
        }

        _EnemyController.m_isChase = true;
        _EnemyController.m_isAtk = false;
    }

    void MeleeAtk()
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position,
                m_targetRadius, transform.forward, m_targetRange, LayerMask.GetMask("Player"));
        if (hits.Length > 0)
        {
            foreach (var hit in hits)
            {
                Health _health = hit.collider.GetComponent<Health>();
                if (_health)
                {
                    _health.TakeDamage(m_dmg, this.gameObject);
                }
            }
        }
    }
}
