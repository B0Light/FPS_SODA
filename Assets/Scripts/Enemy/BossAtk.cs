using System.Collections;
using UnityEngine;
using UnityEngine.AI;
public class BossAtk : EnemyAtk
{
    NavMeshAgent NavMeshAgent;
    Health health;

    public float BulletSpreadAngle = 0f;
    public GameObject Rock;
    public ProjectileStandard missile;
    public Transform missilePort;

    private GameObject m_target;
    public bool isLook;

    protected override void Awake()
    {
        base.Awake();
        NavMeshAgent = GetComponent<NavMeshAgent>();    
        health = GetComponent<Health>();
        NavMeshAgent.isStopped = true;
    }
    void Start()
    {
        m_meleeRadius = 10f;
        m_meleeRange = 0f;

        StartCoroutine(Think());
    }

    // Update is called once per frame
    void Update()
    {
        if (health)
        {
            m_target = health.m_target;    
            if(m_target)
                transform.LookAt(m_target.transform);
        }
            
    }

    IEnumerator Think()
    {
        if (m_target)
        {
            yield return new WaitForSeconds(0.1f);
            int ranAction = Random.Range(0, 5);
            switch (ranAction)
            {
                //missile
                case 0:
                case 1:
                    Debug.Log("Missile");
                    StartCoroutine(MissileShot());
                    break;
                //Rock
                case 2:
                case 3:
                    Debug.Log("Rock");
                    StartCoroutine(RockShot());
                    break;
                case 4:
                    Debug.Log("Taunt");
                    StartCoroutine(Taunt());
                    break;
            }
        }
        else
        {
            yield return new WaitForSeconds(1f);
            StartCoroutine(Think());
        } 
    }

    IEnumerator MissileShot()
    {
        _anim.SetTrigger("doShot");
        yield return new WaitForSeconds(0.2f);
        if (_weaponController != null)
        {
            _weaponController.Owner = gameObject;
            _weaponController.HandleShootInputs(false, true);
        }
        yield return new WaitForSeconds(0.5f);
        missilePort.localPosition += new Vector3(-3.4f,0,0);
        if (_weaponController != null)
        {
            _weaponController.Owner = gameObject;
            _weaponController.HandleShootInputs(false, true);
        }
        missilePort.localPosition += new Vector3(3.4f, 0, 0);
        yield return new WaitForSeconds(2f);
        StartCoroutine(Think());
    }

    IEnumerator RockShot()
    {
        isLook = false;
        _anim.SetTrigger("doBigShot");
        yield return new WaitForSeconds(1f);
        Instantiate(Rock, transform.position + Vector3.up * 5 + transform.forward * 20, transform.rotation);
        isLook = true;
        yield return new WaitForSeconds(2f);
        StartCoroutine(Think());
    }
    IEnumerator Taunt()
    {
        NavMeshAgent.isStopped = false;
        _anim.SetTrigger("doTaunt");
        yield return new WaitForSeconds(0.5f);
        MeleeAtk();
        yield return new WaitForSeconds(1f);
        NavMeshAgent.isStopped = true;
        yield return new WaitForSeconds(2f);
        StartCoroutine(Think());
    }
    public Vector3 GetShotDirectionWithinSpread(Transform shootTransform)
    {
        float spreadAngleRatio = BulletSpreadAngle / 180f;
        Vector3 spreadWorldDirection = Vector3.Slerp(shootTransform.forward, UnityEngine.Random.insideUnitSphere,
            spreadAngleRatio);

        return spreadWorldDirection;
    }
}
