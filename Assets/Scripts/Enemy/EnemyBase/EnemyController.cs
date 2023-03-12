using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(EnemyAtk))]
public class EnemyController : MonoBehaviour
{
    Health _health;
    EnemyAtk _enemyAtk;

    [Header("Targeting")]
    public Transform m_target = null;
    public EnemyPath _path;
    public float m_rotationSpeed = 1.0f;
    public float m_atkRadius = 2f;
    public float m_atkRange = 4f;

    [Header("Animator")]
    
    public bool m_isChase = false;
    public bool m_isAtk = false;
    public bool m_isDead = false;

    [Header("Game")]
    public GameObject[] rewards;

    private void Awake()
    {
        _health = GetComponent<Health>();
        _enemyAtk = GetComponent<EnemyAtk>();
        _path = GetComponent<EnemyPath>();
    }

    void Update()
    {
        if (m_isDead)
        {
            StopAllCoroutines();
            return;
        }

        //target
        if (_health.m_target)
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
            RaycastHit[] rayHits = Physics.SphereCastAll(transform.position,
                m_atkRadius, transform.forward, m_atkRange, LayerMask.GetMask("Player"));
            if (rayHits.Length > 0 && m_isAtk == false)
            {
                m_isAtk = true;
                //this.gameObject.transform.LookAt(rayHits[0].transform.position);
                Vector3 dir = rayHits[0].transform.position - this.transform.position;
                this.transform.rotation = Quaternion.Lerp(this.transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * m_rotationSpeed);
                _path.AttackNavSetting();
                StartCoroutine(_enemyAtk.Atk());
            }
        }
    }

    private void FixedUpdate()
    {
        Targeting();
    }
}
