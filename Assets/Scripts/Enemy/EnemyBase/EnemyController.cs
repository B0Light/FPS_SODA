using Photon.Pun;
using UnityEngine;
using System.Collections;
using UnityEngine.Windows;
using Unity.VisualScripting;

public class EnemyController : MonoBehaviourPun
{
    protected Health _health;
    EnemyAtk _enemyAtk;

    public EnemyManager EM;
    [Header("Targeting")]
    public GameObject m_target = null;
    public EnemyPath _path;
    public Animator _anim;
    public float m_rotationSpeed = 1.0f;
    public float m_atkRadius = 2f;
    public float m_atkRange = 4f;

    public bool isBoss = false;
    public bool m_isAtk = false;
    public bool m_isDead = false;

    [Header("Game")]
    public GameObject[] rewards;

    private void Awake()
    {
        _health = GetComponent<Health>();
        _enemyAtk = GetComponent<EnemyAtk>();
        _path = GetComponent<EnemyPath>();
        _anim = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        EM = FindObjectOfType<EnemyManager>();
    }
    void Update()
    {
        if (m_isDead)
        {
            StopAllCoroutines();
            return;
        }
        if (PhotonNetwork.IsMasterClient)
        {
            //target
            if (_health.m_target)
            {
                m_target = _health.m_target;
                _path.SetTarget(m_target.transform);
            }
            else
            {
                m_target = null;
            }
        }
        
    }

    private void Targeting()
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
        if (PhotonNetwork.IsMasterClient)
            Targeting();
    }

    public void Die()
    {
        m_isDead = true;
        if (!isBoss)
            photonView.RPC("EnemyDown", RpcTarget.All, null);
        _anim.SetTrigger("doDie");
        if (rewards.Length > 0 && PhotonNetwork.IsMasterClient)
        {
            int rewardIdx = Random.Range(0, rewards.Length);
            PhotonNetwork.Instantiate(rewards[rewardIdx].name, transform.position, Quaternion.identity);
            if (isBoss)
            {
                for (int i = 0; i < 5; i++)
                {
                    rewardIdx = Random.Range(0, rewards.Length);
                    PhotonNetwork.Instantiate(rewards[rewardIdx].name, transform.position, Quaternion.identity);
                }

            }
            photonView.RPC("destroyThisObj", RpcTarget.All, null);
        }
    }

    [PunRPC]
    public void EnemyDown()
    {
        EM.currEnemy -= 1;
    }

    [PunRPC]
    public void destroyThisObj()
    {
        Destroy(this.gameObject, 2f);
    }

}
