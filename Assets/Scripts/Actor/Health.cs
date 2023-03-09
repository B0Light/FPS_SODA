using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public Gauge<float> m_health;
    public float m_MaxHealth = 100f;
    public float m_recover = 1f;
    public GameObject m_target = null;

    protected MeshRenderer[] m_meshs;

    public bool Invincible { get; set; }
    public bool CanPickup() => m_health.Value < m_health.GetMaxValue();
    public float GetRatio() => m_health.Value / m_health.GetMaxValue();
 

    private void Awake()
    {
        m_meshs = GetComponentsInChildren<MeshRenderer>();
    }
    void Start()
    {
        m_health = new Gauge<float>(m_MaxHealth);
        StartCoroutine("RecoverHealth", 2f);
    }

    private void Update()
    {
        if (m_health.Value == m_health.GetMaxValue())
        {
            m_target = null;
        }
    }

    public void Heal(float healAmount)
    {
        m_health.Value += healAmount;
    }

    IEnumerator RecoverHealth(float delay) 
    {
        if (m_health.Value < m_MaxHealth)
            m_health.Value += m_recover;
        yield return new WaitForSeconds(delay);
        StartCoroutine("RecoverHealth", delay);
    }

    public void TakeDamage(float damage, GameObject damageSource)
    {
        if (Invincible)
            return;
        m_target = damageSource;
        StartCoroutine(OnDmg(damage));
    }

    IEnumerator OnDmg(float damage)
    {
        foreach (MeshRenderer mesh in m_meshs)
            mesh.material.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        m_health.Value -= damage;
        if (m_health.Value > 0)
        {
            foreach (MeshRenderer mesh in m_meshs)
                mesh.material.color = Color.white;
        }
        else
        {
            foreach (MeshRenderer mesh in m_meshs)
                mesh.material.color = Color.gray;
            m_health.Value = 0f;
            Destroy(gameObject);
        }
    }
}
