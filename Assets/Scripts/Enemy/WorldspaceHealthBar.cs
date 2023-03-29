using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldspaceHealthBar : MonoBehaviour
{
    public Health Health;
    public Image HealthBarImage;
    public Transform HealthBarPivot;

    [Tooltip("Whether the health bar is visible when at full health or not")]
    public bool HideFullHealthBar = true;

    void Update()
    {
        HealthBarImage.fillAmount = Health.m_health.Value / Health.m_MaxHealth;

        HealthBarPivot.LookAt(Camera.main.transform.position);

        if (HideFullHealthBar)
            HealthBarPivot.gameObject.SetActive(HealthBarImage.fillAmount != 1);
    }
}
