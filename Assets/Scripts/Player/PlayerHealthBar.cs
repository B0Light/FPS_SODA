using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthBar : MonoBehaviour
{
    public Image HealthFillImage;

    public PlayerController m_playerController;
    public Health m_PlayerHealth;
    public TMP_Text text;

    void Update()
    {
        HealthFillImage.fillAmount = m_PlayerHealth.m_health.Value / m_PlayerHealth.m_MaxHealth;
        text.text = m_PlayerHealth.m_health.Value.ToString() + " / " + m_PlayerHealth.m_MaxHealth.ToString();
    }
}
