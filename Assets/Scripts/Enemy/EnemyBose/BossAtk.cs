using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
public class BossAtk : EnemyAtk
{
    NavMeshAgent NavMeshAgent;
    Health health;

    public float BulletSpreadAngle = 0f;
    public GameObject Rock;
    public ProjectileStandard missile;
    public Transform missilePort;
    public Transform RockPort;
    public GameObject m_target;
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
        _anim = GetComponentInChildren<Animator>();
        m_meleeRadius = 20f;
        m_meleeRange = 1f;
        isLook = true;
        if(PhotonNetwork.IsMasterClient)
            StartCoroutine(Think());
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (health.m_health.Value > 0)
            {
                m_target = health.m_target;
                if (m_target && isLook)
                    transform.LookAt(m_target.transform);
            }
            else
            {
                StopAllCoroutines();
            }
        }
    }

    IEnumerator Think()
    {
        if(!PhotonNetwork.IsMasterClient) yield return null;
        if (m_target)
        {
            yield return new WaitForSeconds(0.1f);
            int ranAction = Random.Range(0, 5);
            switch (ranAction)
            {
                //missile
                case 0:
                case 1:
                    photonView.RPC("setAni1", RpcTarget.All, null);
                    StartCoroutine(MissileShot());
                    break;
                //Rock
                case 2:
                case 3:
                    photonView.RPC("setAni2", RpcTarget.All, null);
                    StartCoroutine(RockShot());
                    break;
                case 4:
                    photonView.RPC("setAni3", RpcTarget.All, null);
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
        yield return new WaitForSeconds(0.2f);
        if (_weaponController != null)
        {
            _weaponController.Owner = gameObject;
            _weaponController.HandleShootInputs(false, true);
        }
        yield return new WaitForSeconds(0.3f);
        photonView.RPC("setMissilePortA", RpcTarget.All, null);
        if (_weaponController != null)
        {
            _weaponController.Owner = gameObject;
            _weaponController.HandleShootInputs(false, true);
        }
        photonView.RPC("setMissilePortB", RpcTarget.All, null);
        yield return new WaitForSeconds(2.5f);
        StartCoroutine(Think());
    }

    IEnumerator RockShot()
    {
        isLook = false;
        GameObject I_Rock = PhotonNetwork.Instantiate(Rock.name, RockPort.position, transform.rotation);
        I_Rock.GetComponent<BossRock>().Owner = gameObject;
        yield return new WaitForSeconds(3f);
        isLook = true;
        StartCoroutine(Think());
    }
    public IEnumerator Taunt()
    {
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

    [PunRPC] public void setAni1() { _anim.SetTrigger("doShot"); }
    [PunRPC] public void setAni2() { _anim.SetTrigger("doBigShot"); }
    [PunRPC] public void setAni3() { _anim.SetTrigger("doTaunt"); }
    [PunRPC] public void setMissilePortA() { missilePort.localPosition += new Vector3(-3.4f, 0, 0); }
    [PunRPC] public void setMissilePortB() { missilePort.localPosition += new Vector3(3.4f, 0, 0); }


    public Vector3 GetShotDirectionWithinSpread(Transform shootTransform)
    {
        float spreadAngleRatio = BulletSpreadAngle / 180f;
        Vector3 spreadWorldDirection = Vector3.Slerp(shootTransform.forward, UnityEngine.Random.insideUnitSphere,
            spreadAngleRatio);

        return spreadWorldDirection;
    }
}
