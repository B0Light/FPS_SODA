using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(Health))]
public class EnemyController : MonoBehaviour
{
    [Header("Health")]
    public float m_maxHealth;
    Health _health;

    [Header("Movement")]
    public float m_moveSpeed;
    public Transform m_target = null;
    public PatrolPath _path;

    [Header("Animator")]
    Animator _anim;
    public bool m_isChase = false;
    public bool m_isAtk = false;
    public bool m_isDead = false;

    public GameObject[] rewards;

    private void Awake()
    {
        _anim = GetComponentInChildren<Animator>();
        _health = GetComponent<Health>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (m_isDead)
        {
            StopAllCoroutines();
            return;
        }
        //animation
        _anim.SetBool("isWalk", m_isChase);

        if (_health.m_target != null)
        {
            m_target = _health.m_target.transform;
            _path.SetTarget(m_target);
        }
        else
        {
            m_target = null;
        }
    }

    void Targeting()
    {
        if (!m_isDead)
        {
            float targetRadius = 1.5f;
            float targetRange = 1f;

            RaycastHit[] rayHits = Physics.SphereCastAll(transform.position,
                targetRadius, transform.forward, targetRange, LayerMask.GetMask("Player"));
            if (rayHits.Length > 0 && !m_isAtk)
            {

            }
        }
    }
}
