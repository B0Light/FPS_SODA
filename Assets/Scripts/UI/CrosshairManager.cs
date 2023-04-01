using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairManager : MonoBehaviour
{
    public Image CrosshairImage;
    public Sprite NullCrosshairSprite;
    public float CrosshairUpdateshrpness = 5f;

    public PlayerWeaponsManager m_WeaponsManager;
    WeaponController currWeapon;
    RectTransform m_CrosshairRectTransform;
    CrosshairData m_CrosshairDataDefault;
    CrosshairData m_CurrentCrosshair;

    public void setPlayer()
    {
        OnWeaponChanged(m_WeaponsManager.ActiveWeaponIndex);
        m_WeaponsManager.OnSwitchedToWeapon += OnWeaponChanged;
    }

    void OnWeaponChanged(int newWeaponIdx)
    {
        if (newWeaponIdx > -1)
        {
            currWeapon = m_WeaponsManager.m_WeaponSlots[newWeaponIdx];
            CrosshairImage.enabled = true;
            m_CrosshairDataDefault = currWeapon.CrosshairDataDefault;
            m_CrosshairRectTransform = CrosshairImage.GetComponent<RectTransform>();
        }
        else
        {
            if (NullCrosshairSprite)
            {
                CrosshairImage.sprite = NullCrosshairSprite;
            }
            else
            {
                CrosshairImage.enabled = false;
            }
        }
        UpdateCrosshair(true);
    }

    void UpdateCrosshair(bool force)
    {
        if (m_CrosshairDataDefault.CrosshairSprite == null)
            return;

        if (force)
        {
            m_CurrentCrosshair = m_CrosshairDataDefault;
            CrosshairImage.sprite = m_CurrentCrosshair.CrosshairSprite;
            m_CrosshairRectTransform.sizeDelta = m_CurrentCrosshair.CrosshairSize * Vector2.one;
        }
        CrosshairImage.color = Color.Lerp(CrosshairImage.color, m_CurrentCrosshair.CrosshairColor,
                Time.deltaTime * CrosshairUpdateshrpness);
    }
}