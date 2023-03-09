
using UnityEngine;

public class ChargedProjectileEffectsHandler : MonoBehaviour
{
    public GameObject ChargingObject;
    public MinMaxVector3 Scale;
    public MinMaxColor Color;

    MeshRenderer[] m_AffectedRenderers;
    ProjectileBase m_ProjectileBase;

    void OnEnable()
    {
        m_ProjectileBase = GetComponent<ProjectileBase>();
        m_ProjectileBase.OnShoot += OnShoot;

        m_AffectedRenderers = ChargingObject.GetComponentsInChildren<MeshRenderer>();
        foreach (var ren in m_AffectedRenderers)
        {
            ren.sharedMaterial = Instantiate(ren.sharedMaterial);
        }
    }

    void OnShoot()
    {
        ChargingObject.transform.localScale = Scale.GetValueFromRatio(m_ProjectileBase.InitialCharge);

        foreach (var ren in m_AffectedRenderers)
        {
            ren.sharedMaterial.SetColor("_Color", Color.GetValueFromRatio(m_ProjectileBase.InitialCharge));
        }
    }
    
}