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
    public Transform RockPort;
    private GameObject m_target;
    public bool isLook;

    public GameObject ImpactVfx;
    public float ImpactVfxLifetime = 5f;

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
        m_meleeRange = 1f;
        isLook = true;
        StartCoroutine(Think());
    }

    // Update is called once per frame
    void Update()
    {
        if (health)
        {
            m_target = health.m_target;    
            if(m_target && isLook)
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
        yield return new WaitForSeconds(0.3f);
        missilePort.localPosition += new Vector3(-3.4f,0,0);
        if (_weaponController != null)
        {
            _weaponController.Owner = gameObject;
            _weaponController.HandleShootInputs(false, true);
        }
        missilePort.localPosition += new Vector3(3.4f, 0, 0);
        yield return new WaitForSeconds(2.5f);
        StartCoroutine(Think());
    }

    IEnumerator RockShot()
    {
        isLook = false;
        _anim.SetTrigger("doBigShot");
        GameObject I_Rock = Instantiate(Rock, RockPort.position, transform.rotation);
        I_Rock.GetComponent<BossRock>().Owner = gameObject;
        yield return new WaitForSeconds(3f);
        isLook = true;
        StartCoroutine(Think());
    }
    IEnumerator Taunt()
    {
        _anim.SetTrigger("doTaunt");
        yield return new WaitForSeconds(2f);
        MeleeAtk();
        yield return new WaitForSeconds(0.1f);
        if (ImpactVfx)
        {
            GameObject impactVfxInstance = Instantiate(ImpactVfx, transform.position,
                Quaternion.LookRotation(transform.position));
            if (ImpactVfxLifetime > 0)
            {
                Destroy(impactVfxInstance.gameObject, ImpactVfxLifetime);
            }
        }
        yield return new WaitForSeconds(0.5f);
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
