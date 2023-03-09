using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyController))]
public class PatrolPath : MonoBehaviour
{
    NavMeshAgent m_enemy = null;
    EnemyController _enemyController;

    [SerializeField] float m_distance = 0f;
    [SerializeField] LayerMask m_layerMask = 0;
    [SerializeField] Transform[] m_patrolNode = null;

    int m_count = 0;
    Transform m_target = null;

    private void Awake()
    {
        _enemyController = GetComponent<EnemyController>();
    }

    void Start()
    {
        m_enemy = GetComponent<NavMeshAgent>();
        InvokeRepeating("MoveToNextWayNode", 0f, 2f);
    }

    void Update()
    {
        if (m_enemy.velocity == Vector3.zero)
        {
            _enemyController.m_isChase = false;
            MoveToNextWayNode();
        }            
        if(_enemyController.m_target == null)
            Sight();
        if (m_target != null)
        {
            m_enemy.SetDestination(m_target.position);
            this.gameObject.transform.LookAt(m_target.position);
        }
    }

    public void SetTarget(Transform p_target)
    {
        CancelInvoke();
        m_target = p_target;
    }

    public void RemoveTarget()
    {
        m_target = null;
        MoveToNextWayNode();
    }

    void MoveToNextWayNode()
    {
        _enemyController.m_isChase = true;
        if (m_target != null) return;
        if(m_enemy.velocity == Vector3.zero)
        {
            m_count++;
            if (m_count >= m_patrolNode.Length) m_count = 0;
            m_enemy.SetDestination(m_patrolNode[m_count].position);
            this.gameObject.transform.LookAt(m_patrolNode[m_count].position);
            
        }
    }

    void Sight()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, m_distance, m_layerMask);

        if (cols.Length > 0)
        {
            Transform playerPos = cols[0].transform;
            SetTarget(playerPos);
        }
        else
        {
            RemoveTarget();
        }
    }
}
