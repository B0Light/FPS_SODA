using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting;

public class PlayerHealthBar : MonoBehaviour
{
    [Tooltip("Image component dispplaying current health")]
    public Image HealthFillImage;

    public PlayerController m_playerController;
    public Health m_PlayerHealth;

    void Update()
    {
        HealthFillImage.fillAmount = m_PlayerHealth.m_health.Value / m_PlayerHealth.m_MaxHealth;
    }
}
