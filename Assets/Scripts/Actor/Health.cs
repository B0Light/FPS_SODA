using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[RequireComponent(typeof(PhotonView))]
public class Health : MonoBehaviourPun
{
    public Gauge<float> m_health;
    public float m_MaxHealth = 100f;
    public float m_recover = 1f;
    public GameObject m_target = null;

    protected MeshRenderer[] m_meshs;
    protected bool isDead = false;

    public bool CanPickup() => m_health.Value < m_health.GetMaxValue();
    public float GetRatio() => m_health.Value / m_health.GetMaxValue();

    [PunRPC]
    public void ApplyUpdatedHealth(float newHealth, bool newDead)
    {
        m_health.Value = newHealth;
        isDead = newDead;
    }

    private void Awake()
    {
        m_meshs = GetComponentsInChildren<MeshRenderer>();
    }
    void Start()
    {
        m_health = new Gauge<float>(m_MaxHealth);
        if(PhotonNetwork.IsMasterClient)
            StartCoroutine("RecoverHealth", 2f);
    }

    private void Update()
    {
        if (m_health.Value == m_health.GetMaxValue())
        {
            m_target = null;
        }
    }

    [PunRPC]
    public void Heal(float healAmount)
    {
        if(isDead) { return; }

        if(PhotonNetwork.IsMasterClient)
        {
            m_health.Value += healAmount;
            photonView.RPC("ApplyUpdatedHealth", RpcTarget.Others, m_health.Value, isDead);
            photonView.RPC("Heal", RpcTarget.Others, healAmount);
        }
        
    }

    IEnumerator RecoverHealth(float delay) 
    {
        if (m_health.Value < m_MaxHealth)
            m_health.Value += m_recover;
        yield return new WaitForSeconds(delay);
        photonView.RPC("ApplyUpdatedHealth", RpcTarget.Others, m_health.Value, isDead);
        StartCoroutine("RecoverHealth", delay);
    }

    [PunRPC]
    public void TakeDamage(float damage, GameObject damageSource)
    {
        m_target = damageSource;
        if (PhotonNetwork.IsMasterClient)
        {
            m_health.Value -= damage;
            photonView.RPC("ApplyUpdatedHealth", RpcTarget.Others, m_health.Value, isDead);
            //photonView.RPC("TakeDamage", RpcTarget.Others, damage, damageSource);
        }
        StartCoroutine(OnDmg(damage));
    }

    IEnumerator OnDmg(float damage)
    {
        foreach (MeshRenderer mesh in m_meshs)
            mesh.material.color = Color.red;
        yield return new WaitForSeconds(0.2f);
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
            PlayerController playerController = this.gameObject.GetComponent<PlayerController>();
            if(playerController != null && isDead == false)
            {
                isDead = true;
                playerController.Die();
            }
        }
    }
}
