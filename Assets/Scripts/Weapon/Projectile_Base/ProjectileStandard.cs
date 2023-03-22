using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using Photon.Pun;

[RequireComponent (typeof(Rigidbody))]
[RequireComponent (typeof(SphereCollider))]
public class ProjectileStandard : ProjectileBase
{
    [Header("General")]
    public float Radius = 0.01f; // chargeParameter / Boom
    public Transform Root;
    public LayerMask m_layerMask = 0;
    public bool Boom = false;

    public float MaxLifeTime = 5f;
    public GameObject ImpactVfx;
    public float ImpactVfxLifetime = 5f;
    public float ImpactVfxSpawnOffset = 0.1f;

    public AudioClip ImpactSfxClip;
    public LayerMask HittableLayers = -1;

    [Header("Movement")]
    public float Speed = 20f;
    public float GravityDownAcceleration = 0f;
    public float TrajectoryCorrectionDistance = -1;

    [Header("Damage")]
    public float Damage = 40f;


    ProjectileBase m_ProjectileBase;
    Vector3 m_Velocity;
    Vector3 m_TrajectoryCorrectionVector;
    List<Collider> m_IgnoredColliders;

    void OnEnable()
    {
        m_ProjectileBase = GetComponent<ProjectileBase>();
        m_ProjectileBase.OnShoot += OnShoot;

        Destroy(gameObject, MaxLifeTime);
    }

    new void OnShoot()
    {
        m_Velocity = transform.forward * Speed;
        m_IgnoredColliders = new List<Collider>();
        transform.position += m_ProjectileBase.InheritedMuzzleVelocity * Time.deltaTime;
    }

    void Update()
    {
        transform.position += m_Velocity * Time.deltaTime;
        transform.forward = m_Velocity.normalized;

        // Gravity
        if (GravityDownAcceleration > 0)
        {
            m_Velocity += Vector3.down * GravityDownAcceleration * Time.deltaTime;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Health health = collision.gameObject.GetComponent<Health>();
        if (PhotonNetwork.IsMasterClient)
        {
            if (health)
                health.TakeDamage(Damage, m_ProjectileBase.Owner);

        
            if (Boom)
            {
                RaycastHit[] hits = Physics.SphereCastAll(transform.position,
                    Radius, transform.up, 0f, m_layerMask);
                foreach (var hit in hits)
                {
                    Health blasted = collision.gameObject.GetComponent<Health>();
                    if (blasted)
                        blasted.TakeDamage(Damage, m_ProjectileBase.Owner);
                }
            }
        }
        
        // impact vfx
        if (ImpactVfx)
        {
            GameObject impactVfxInstance = Instantiate(ImpactVfx, transform.position,
                Quaternion.LookRotation(transform.position));
            if (ImpactVfxLifetime > 0)
            {
                Destroy(impactVfxInstance.gameObject, ImpactVfxLifetime);
            }
        }
        Destroy(this.gameObject);
    }
}
