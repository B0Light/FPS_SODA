using System.Collections;
using UnityEngine;

public class EnemyAtk : MonoBehaviour
{

    public enum Type
    {
        A, B, C, Boss
    };

    protected EnemyController _EnemyController;
    protected WeaponController _weaponController;
    protected Animator _anim;

    public Type enemyType;
    public float m_dmg;
    protected float m_meleeRadius = 4f;
    protected float m_meleeRange = 2f;

    protected virtual void Awake() {
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
                _anim.SetTrigger("doAtk");
                yield return new WaitForSeconds(1f);
                break;

            case Type.B:
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

        _EnemyController.m_isAtk = false;
    }

    protected void MeleeAtk()
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
