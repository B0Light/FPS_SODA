using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyController))]
public class EnemyPath : MonoBehaviourPun
{
    public NavMeshAgent m_navMesh = null;
    EnemyController _enemyController;
    Animator _anim;

    public float m_distance = 0f;
    [SerializeField] LayerMask m_layerMask = 0;
    public Transform[] m_patrolNode = null;

    public Transform m_target = null;

    int m_count = 0;
    bool chkDead = false;
    private void Awake()
    {
        _enemyController = GetComponent<EnemyController>();
        _anim = GetComponentInChildren<Animator>();
        m_navMesh = GetComponent<NavMeshAgent>();
    }

    public void TraceNavSetting()
    {
        m_navMesh.isStopped = false;
        m_navMesh.updatePosition = true;
        m_navMesh.updateRotation = true;
    }

    public void AttackNavSetting()
    {
        m_navMesh.isStopped = true;
        m_navMesh.updatePosition = false;
        m_navMesh.updateRotation = false;
        m_navMesh.velocity = Vector3.zero;
        
    }

    void Start()
    {
        TraceNavSetting();
        InvokeRepeating("MoveToNextWayNode", 0f, 2f);
    }

    void Update()
    {
        _anim.SetBool("isWalk", m_navMesh.velocity.magnitude > 0);
         
        if (_enemyController.m_isDead == true && chkDead == false)
        {
            chkDead = true;
            m_navMesh.isStopped = true;
            m_navMesh.enabled = false;
        }

        if (_enemyController.m_isAtk == true || chkDead) return;

        if (m_navMesh.velocity == Vector3.zero)
        {
            MoveToNextWayNode();
        }
        else
        {
            Sight();
        }
    }

    public void SetTarget(Transform p_target)
    {
        if (chkDead) return;
        CancelInvoke();
        m_target = p_target;
        TraceNavSetting();
        m_navMesh.SetDestination(m_target.position);
        gameObject.transform.LookAt(m_target.position);
    }

    void MoveToNextWayNode()
    {
        if (chkDead) return;
        TraceNavSetting();
        if (m_target != null ) return;

        if(m_navMesh.velocity == Vector3.zero)
        {
            m_count = Random.Range(0, m_patrolNode.Length);
            m_navMesh.SetDestination(m_patrolNode[m_count].position);
            this.gameObject.transform.LookAt(m_patrolNode[m_count].position);
        }
    }
    void Sight()
    {
        if (chkDead) return;
        Collider[] cols = Physics.OverlapSphere(transform.position, m_distance, m_layerMask);

        if (cols.Length > 0)
        {
            Transform playerPos = cols[0].transform;

            if(m_target == null) 
                AttackNavSetting(); 
            SetTarget(playerPos);
        }
        else
        {
            if (_enemyController.m_target == null)
                photonView.RPC("RemoveTarget", RpcTarget.All,null);
        }
    }

    [PunRPC]
    public void RemoveTarget()
    {
        if (chkDead) return;
        m_target = null;
        MoveToNextWayNode();
    }
}
