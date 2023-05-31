using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyController))]
public class EnemyPath : MonoBehaviourPun
{
    public NavMeshAgent m_navMesh = null;
    EnemyController _enemyController;
    Rigidbody rbody;
    Animator _anim;
    EnemyManager EM;

    public float m_distance = 0f;
    [SerializeField] LayerMask m_layerMask = 0;

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
        if (m_navMesh == null) return;
        m_navMesh.isStopped = false;
        m_navMesh.updatePosition = true;
        m_navMesh.updateRotation = true;
    }

    public void AttackNavSetting()
    {
        if (m_navMesh == null) return;
        m_navMesh.isStopped = true;
        m_navMesh.updatePosition = false;
        m_navMesh.updateRotation = false;
        m_navMesh.velocity = Vector3.zero;
        
    }

    void Start()
    {
        rbody = GetComponent<Rigidbody>();
        EM = FindObjectOfType<EnemyManager>();
        TraceNavSetting();
        InvokeRepeating("MoveToNextWayNode", 0f, 2f);
    }

    void Update()
    {
        if (m_navMesh == null) return;
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

    private void FixedUpdate()
    {
        Freeze();
    }

    public void SetTarget(Transform p_target)
    {
        if (chkDead) return;
        CancelInvoke();
        m_target = p_target;
        TraceNavSetting();
        if (m_navMesh == null) return;
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
            photonView.RPC("SetDestination", RpcTarget.All, null);
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
    public void SetDestination()
    {
        if (m_navMesh == null) return;
        if (EM != null)
        {
            if (m_count == EM.EnemyPath.Length) m_count = 0;
            m_navMesh.SetDestination(EM.EnemyPath[m_count].position);
            this.gameObject.transform.LookAt(EM.EnemyPath[m_count++].position);
        }
    }

    [PunRPC]
    public void RemoveTarget()
    {
        if (chkDead) return;
        m_target = null;
        MoveToNextWayNode();
    }

    public void Freeze()
    {
        if (rbody != null)
        {
            rbody.velocity = Vector3.zero;
            rbody.angularVelocity = Vector3.zero;
        }
    }
}
