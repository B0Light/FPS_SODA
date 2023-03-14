using System.Collections;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    protected Rigidbody m_rigidbody;
    public float m_dmg = 30f;
    protected float m_lifeTime = 10f;
    protected float m_Radius = 5f;
    [SerializeField] 
    public LayerMask m_layerMask = 0;
    public GameObject ImpactVfx;
    protected float ImpactVfxLifetime = 5f;

    public GameObject Owner;
    // Start is called before the first frame update
    protected void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
    }

    protected virtual void Start()
    {
        StartCoroutine(LifeTime());
    }
    

    protected IEnumerator LifeTime()
    {
        yield return new WaitForSeconds(m_lifeTime);
        Impact();
        Destroy(gameObject);
    }

    protected void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Floor"))
        {
            Collider[] cols = Physics.OverlapSphere(transform.position, m_Radius, m_layerMask);
            foreach (var col in cols)
            {
                Health health = col.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(m_dmg, Owner);
                }
            }

            Impact();
            Destroy(gameObject);
        }
    }

    protected void Impact()
    {
        if (ImpactVfx)
        {
            GameObject impactVfxInstance = Instantiate(ImpactVfx, transform.position,
                Quaternion.LookRotation(transform.position));
            if (ImpactVfxLifetime > 0)
            {
                Destroy(impactVfxInstance.gameObject, ImpactVfxLifetime);
            }
        }
    }
}
