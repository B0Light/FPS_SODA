using System.Collections;
using UnityEngine;

public class EnemyAtk : MonoBehaviour
{

    public enum Type
    {
        A, B, C
    };

    EnemyController _EnemyController;
    Animator _anim;
    WeaponController _weaponController;

    public Type enemyType;
    public float m_dmg;
    float m_meleeRadius = 1.5f;
    float m_meleeRange = 1f;
    public GameObject bullet;
    
    private void Awake() {
        _EnemyController = GetComponent<EnemyController>();
        _weaponController = GetComponentInChildren<WeaponController>();
        _anim = GetComponentInChildren<Animator>();
    }

    public IEnumerator Atk()
    {
        _anim.SetBool("isWalk", false);
        _anim.SetBool("isAtk", true);

        switch (enemyType)
        {
            case Type.A:
                yield return new WaitForSeconds(0.2f);
                MeleeAtk();
                _anim.SetBool("isAtk", true);
                yield return new WaitForSeconds(1f);
                _anim.SetBool("isAtk", false);
                break;

            case Type.B:
                yield return new WaitForSeconds(0.5f);
                Rigidbody _rbody = GetComponent<Rigidbody>();
                if (_rbody)
                {
                    _rbody.AddForce(transform.forward * 200, ForceMode.Impulse);
                    MeleeAtk();
                    _rbody.velocity = Vector3.zero;
                }
                yield return new WaitForSeconds(2f);
                break;

            case Type.C:

                _anim.SetTrigger("doAtk");
                
                yield return new WaitForSeconds(0.5f);
                
                if (_weaponController != null)
                {
                    _weaponController.Owner = gameObject;
                    _weaponController.HandleShootInputs(false, true);
                }
                yield return new WaitForSeconds(2f);
                break;
        }

        _EnemyController.m_isChase = true;
        _EnemyController.m_isAtk = false;
    }

    void MeleeAtk()
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position,
                m_meleeRadius, transform.forward, m_meleeRange, LayerMask.GetMask("Player"));
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
