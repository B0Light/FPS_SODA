using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;

[RequireComponent(typeof(PhotonView))]
public class Health : MonoBehaviourPun, IPunObservable
{
    public Gauge<float> m_health;
    public float m_MaxHealth = 100f;
    public float m_recover = 1f;
    public GameObject m_target = null;

    protected MeshRenderer[] m_meshs;
    public bool isDead = false;

    public bool CanPickup() => m_health.Value < m_health.GetMaxValue();
    public float GetRatio() => m_health.Value / m_health.GetMaxValue();

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(m_health.Value);
            stream.SendNext(isDead);
        }
        else
        {
            this.m_health.Value = (float)stream.ReceiveNext();
            this.isDead = (bool)stream.ReceiveNext();
        }
    }

    [PunRPC]
    public void ApplyUpdatedHealth(float newHealth, bool newDead)
    {
        if (isDead) return;
        if(m_health.Value > newHealth) { StartCoroutine(OnDmg()); }
       

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
        if (PhotonNetwork.IsMasterClient)
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
        if (isDead) { return; }

        if (PhotonNetwork.IsMasterClient)
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

    public void TakeDamage(float damage, GameObject damageSource)
    {
        if(isDead) { return; }
        m_target = damageSource;
        photonView.RPC("TakeDmgOnServer", RpcTarget.MasterClient, damage);
    }

    [PunRPC]
    public void TakeDmgOnServer(float damage)
    {
        if (isDead) { return; }
        m_health.Value -= damage;
        StartCoroutine(OnDmg());
        photonView.RPC("ApplyUpdatedHealth", RpcTarget.Others, m_health.Value, isDead);
    }

    IEnumerator OnDmg()
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
            EnemyController enemyController = this.gameObject.GetComponent<EnemyController>();
            if(isDead == false)
            {
                isDead = true;
                if (playerController != null)
                {
                    
                    GameManager gameManager = FindObjectOfType<GameManager>();
                    Tuple<string, int> value;
                    int currSco = 0;
                    if (gameManager.killScore.TryGetValue(photonView.OwnerActorNr, out value))
                    {
                        currSco = value.Item2;
                        gameManager.killScore.Remove(photonView.OwnerActorNr); 
                        gameManager.killScore.Add(photonView.OwnerActorNr, Tuple.Create(value.Item1, currSco - 1));
                    }
                    playerController.Die();
                    if(currSco > 0)
                    {
                        gameManager.RespawnPlayer();
                    }
                }
                else if (enemyController != null)
                {
                    enemyController.Die();
                }
            }
            
        }
    }
}