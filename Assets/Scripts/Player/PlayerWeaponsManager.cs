using Cinemachine;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerWeaponsManager : MonoBehaviourPun
{
    public List<int> StartingWeapons = new List<int>();

    [Header("References")]
    public Transform WeaponParentSocket;
    public Transform DefaultWeaponPosition;
    public Transform AimingWeaponPosition;
    public Animator animator;

    [Header("Weapon Bob")]
    public float BobFrequency = 10f;
    public float BobSharpness = 10f;
    public float DefaultBobAmount = 0.05f;
    public float AimingBobAmount = 0.02f;

    [Header("Weapon Recoil")]
    public float RecoilSharpness = 50f;
    public float MaxRecoilDistance = 0.5f;
    public float RecoilRestitutionSharpness = 10f;

    [Header("Misc")]
    public float AimingAnimationSpeed = 10f;
    public float DefaultFov = 60f;
    public float WeaponFovMultiplier = 1f;
    public float WeaponSwitchDelay = 1f;
    public LayerMask FpsWeaponLayer;

    [Header("Cinemachine")]
    public CinemachineVirtualCamera WeaponVCamera;
    public bool IsAiming { get; private set; }

    public int ActiveWeaponIndex = -1;

    public UnityAction<WeaponController, int> OnAddedWeapon;
    public UnityAction<WeaponController, int> OnRemovedWeapon;

    public WeaponController[] m_WeaponSlots = new WeaponController[9];
    public int[] ActiveWeaponLV = new int[9];


    float m_WeaponBobFactor;
    Vector3 m_WeaponBobLocalPosition;
    Vector3 m_LastCharacterPosition;

    PlayerController m_PlayerController;
    InputSystem m_InputSystem;

    public Vector3 m_WeaponMainLocalPosition;
    public Vector3 m_WeaponRecoilLocalPosition;
    Vector3 m_AccumulatedRecoil;

    void Start()
    {
        ActiveWeaponIndex = -1;

        m_PlayerController = GetComponent<PlayerController>();
        m_InputSystem = GetComponent<InputSystem>();

        foreach (var weapon in StartingWeapons)
        {
            AddWeapon(weapon);
        }
        
        m_WeaponMainLocalPosition = new Vector3(1.2f, -0.8f, 1.6f);
        IsAiming = false;
    }

    void Update()
    {
        WeaponController activeWeapon = GetActiveWeapon();
        if (photonView.IsMine)
        {

            if (activeWeapon != null && activeWeapon.IsReloading)
                return;

            if (activeWeapon != null )
            {
                if (!activeWeapon.AutomaticReload && m_InputSystem.reload && activeWeapon.CurrentAmmoRatio < 1.0f)
                {
                    IsAiming = false;
                    return;
                }
                // Aiming
                IsAiming = m_InputSystem.aiming;
            
                // Shooting
                bool hasFired = activeWeapon.HandleShootInputs(m_InputSystem.fire, m_InputSystem.fireCharge);
                
                if (hasFired)
                {
                    m_AccumulatedRecoil += Vector3.back * activeWeapon.RecoilForce;
                    m_AccumulatedRecoil = Vector3.ClampMagnitude(m_AccumulatedRecoil, MaxRecoilDistance);
                    if(m_PlayerController.inputDirection == Vector3.zero)
                        animator.SetTrigger("doShot");
                }
                
            }
        }
        // weapon switch
        if (!IsAiming && (activeWeapon == null || !activeWeapon.IsCharging))
        {
            int switchWeaponInput = m_InputSystem.GetSwitchWeaponInput();
            if (switchWeaponInput != 0)
            {
                bool switchUp = switchWeaponInput > 0;
                animator.SetTrigger("doSwap");
                SwitchWeapon(switchUp);
            }
            else
            {
                switchWeaponInput = m_InputSystem.GetSelectWeaponInput();
                if (switchWeaponInput > 0)
                {
                    SwitchToWeaponIndex(switchWeaponInput - 1);
                }
            }
        }
    }


    // Update various animated features in LateUpdate because it needs to override the animated arm position
    void LateUpdate()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        UpdateWeaponAiming();
        UpdateWeaponBob();
        UpdateWeaponRecoil();

        WeaponParentSocket.localPosition = m_WeaponMainLocalPosition + m_WeaponBobLocalPosition + m_WeaponRecoilLocalPosition;
    }
    public void SwitchWeapon(bool scroll)
    {
        if(ActiveWeaponIndex == -1) return;
        bool chk = false;
        int newWeaponIndex = -1;
        if (scroll)
        {
            for (int i = ActiveWeaponIndex+1; i < ActiveWeaponIndex + m_WeaponSlots.Length; i++)
            {
                if (i >= m_WeaponSlots.Length && chk == false)
                {
                    chk = true;
                    i -= m_WeaponSlots.Length;
                }
                if (ActiveWeaponLV[i] > 0)
                {
                    newWeaponIndex = i;
                    break;
                }
            }
        }  
        else
        {
            for (int i = ActiveWeaponIndex-1; i > ActiveWeaponIndex - m_WeaponSlots.Length; i--)
            {
                if (i < 0 && chk == false)
                {
                    chk = true;
                    i += m_WeaponSlots.Length;
                }  
                if (ActiveWeaponLV[i] > 0)
                {
                    newWeaponIndex = i;
                    break;
                }
            }
        }
        SwitchToWeaponIndex(newWeaponIndex);
    }

    [PunRPC]
    public void SwitchToWeaponIndex(int newWeaponIndex)
    {
        if (ActiveWeaponIndex == -1) return;
        if (ActiveWeaponIndex == newWeaponIndex || newWeaponIndex == -1) return;
        photonView.RPC("SwitchToWeaponIndex", RpcTarget.Others, newWeaponIndex);
        SetAcitveWeapon(newWeaponIndex);
    }
        
    void UpdateWeaponAiming()
    {
        WeaponController activeWeapon = GetActiveWeapon();  
        if (IsAiming && activeWeapon)
        {
            m_WeaponMainLocalPosition = Vector3.Lerp(m_WeaponMainLocalPosition,
                AimingWeaponPosition.localPosition + activeWeapon.AimOffset,
                AimingAnimationSpeed * Time.deltaTime);
            SetFov(Mathf.Lerp(m_PlayerController.VirtualCamera.m_Lens.FieldOfView,
                        activeWeapon.AimZoomRatio * DefaultFov, AimingAnimationSpeed * Time.deltaTime));

        }
        else
        {
            m_WeaponMainLocalPosition = Vector3.Lerp(m_WeaponMainLocalPosition,
                DefaultWeaponPosition.localPosition, AimingAnimationSpeed * Time.deltaTime);
            SetFov(Mathf.Lerp(m_PlayerController.VirtualCamera.m_Lens.FieldOfView, DefaultFov,
                        AimingAnimationSpeed * Time.deltaTime));
        }
    }

    void UpdateWeaponBob()
    {
        if (Time.deltaTime > 0f)
        {
            Vector3 playerCharacterVelocity =
                (m_PlayerController.transform.position - m_LastCharacterPosition) / Time.deltaTime;

            // calculate a smoothed weapon bob amount based on how close to our max grounded movement velocity we are
            float characterMovementFactor = 0f;
            if (m_PlayerController.Grounded)
            {
                characterMovementFactor =
                    Mathf.Clamp01(playerCharacterVelocity.magnitude /
                                  (m_PlayerController.MoveSpeed *
                                   m_PlayerController.SprintSpeed));
            }

            m_WeaponBobFactor =
                Mathf.Lerp(m_WeaponBobFactor, characterMovementFactor, BobSharpness * Time.deltaTime);

            // Calculate vertical and horizontal weapon bob values based on a sine function
            float bobAmount = IsAiming ? AimingBobAmount : DefaultBobAmount;
            float frequency = BobFrequency;
            float hBobValue = Mathf.Sin(Time.time * frequency) * bobAmount * m_WeaponBobFactor;
            float vBobValue = ((Mathf.Sin(Time.time * frequency * 2f) * 0.5f) + 0.5f) * bobAmount *
                              m_WeaponBobFactor;

            // Apply weapon bob
            m_WeaponBobLocalPosition.x = hBobValue;
            m_WeaponBobLocalPosition.y = Mathf.Abs(vBobValue);

            m_LastCharacterPosition = m_PlayerController.transform.position;
        }
    }


    void UpdateWeaponRecoil()
    {
        if (m_WeaponRecoilLocalPosition.z >= m_AccumulatedRecoil.z * 0.99f)
        {
            m_WeaponRecoilLocalPosition = Vector3.Lerp(m_WeaponRecoilLocalPosition, m_AccumulatedRecoil,
                RecoilSharpness * Time.deltaTime);
        }
        else
        {
            m_WeaponRecoilLocalPosition = Vector3.Lerp(m_WeaponRecoilLocalPosition, Vector3.zero,
                RecoilRestitutionSharpness * Time.deltaTime);
            m_AccumulatedRecoil = m_WeaponRecoilLocalPosition;
        }
    }

    // Updates the animated transition of switching weapons

    [PunRPC]
    public bool AddWeapon(int WeaponId)
    {
        photonView.RPC("AddWeapon", RpcTarget.Others, WeaponId);
        ActiveWeaponLV[WeaponId]++;

        if (GetActiveWeapon() == null)
        {
            ActiveWeaponIndex = WeaponId;
            SetAcitveWeapon(ActiveWeaponIndex);
        }

        return false;
    }

    public WeaponController GetActiveWeapon()
    {
        if(ActiveWeaponIndex < 0) return null;
        return m_WeaponSlots[ActiveWeaponIndex];
    }

    void SetAcitveWeapon(int weaponIdx)
    {
        m_WeaponSlots[ActiveWeaponIndex].gameObject.SetActive(false);
        m_WeaponSlots[weaponIdx].gameObject.SetActive(true);
        ActiveWeaponIndex = weaponIdx;
    }

    public void SetFov(float fov)
    {
        m_PlayerController.VirtualCamera.m_Lens.FieldOfView = fov;
    }
}
