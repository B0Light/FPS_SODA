using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerWeaponsManager : MonoBehaviour
{
    public enum WeaponSwitchState
    {
        Up,
        Down,
        PutDownPrevious,
        PutUpNew,
    }
    public List<WeaponController> StartingWeapons = new List<WeaponController>();

    [Header("References")]
    public Camera WeaponCamera;
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

    public bool IsAiming { get; private set; }
    public bool IsPointingAtEnemy { get; private set; }
    public int ActiveWeaponIndex { get; private set; }

    public UnityAction<WeaponController> OnSwitchedToWeapon;
    public UnityAction<WeaponController, int> OnAddedWeapon;
    public UnityAction<WeaponController, int> OnRemovedWeapon;

    WeaponController[] m_WeaponSlots = new WeaponController[9]; // 9 available weapon slots

    float m_WeaponBobFactor;
    Vector3 m_WeaponBobLocalPosition;
    Vector3 m_LastCharacterPosition;

    PlayerController m_PlayerController;
    InputSystem m_InputSystem;

    public Vector3 m_WeaponMainLocalPosition;
    public Vector3 m_WeaponRecoilLocalPosition;
    Vector3 m_AccumulatedRecoil;

    float m_TimeStartedWeaponSwitch;
    WeaponSwitchState m_WeaponSwitchState;
    int m_WeaponSwitchNewWeaponIndex;

    void Start()
    {
        ActiveWeaponIndex = -1;
        m_WeaponSwitchState = WeaponSwitchState.Down;

        m_PlayerController = GetComponent<PlayerController>();
        m_InputSystem = GetComponent<InputSystem>();

        SetFov(DefaultFov);

        OnSwitchedToWeapon += OnWeaponSwitched;

        foreach (var weapon in StartingWeapons)
        {
            AddWeapon(weapon);
        }
        SwitchWeapon(true);

        m_WeaponMainLocalPosition = new Vector3(1.2f, -0.8f, 1.6f);
        IsAiming = false;
    }

    void Update()
    {
        WeaponController activeWeapon = GetActiveWeapon();

        if (activeWeapon != null && activeWeapon.IsReloading)
            return;

        if (activeWeapon != null )
        {
            if (!activeWeapon.AutomaticReload && m_InputSystem.reload && activeWeapon.CurrentAmmoRatio < 1.0f)
            {
                IsAiming = false;
                activeWeapon.StartReloadAnimation();
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
            
        // weapon switch
        if (!IsAiming &&
            (activeWeapon == null || !activeWeapon.IsCharging) &&
            (m_WeaponSwitchState == WeaponSwitchState.Up || m_WeaponSwitchState == WeaponSwitchState.Down))
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
                if (switchWeaponInput != 0)
                {
                    if (GetWeaponAtSlotIndex(switchWeaponInput - 1) != null)
                        SwitchToWeaponIndex(switchWeaponInput - 1);
                }
            }
        }
        IsPointingAtEnemy = false;
        if (activeWeapon)
        {
            if (Physics.Raycast(WeaponCamera.transform.position, WeaponCamera.transform.forward, out RaycastHit hit,
                1000, -1, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.GetComponentInParent<Health>() != null)
                {
                    IsPointingAtEnemy = true;
                }
            }
        }
    }


    // Update various animated features in LateUpdate because it needs to override the animated arm position
    void LateUpdate()
    {
        UpdateWeaponAiming();
        UpdateWeaponBob();
        UpdateWeaponRecoil();
        UpdateWeaponSwitching();

        WeaponParentSocket.localPosition = m_WeaponMainLocalPosition + m_WeaponBobLocalPosition + m_WeaponRecoilLocalPosition;
    }

    public void SetFov(float fov)
    {
        m_PlayerController.PlayerCamera.fieldOfView = fov;
        WeaponCamera.fieldOfView = fov * WeaponFovMultiplier;
    }

    public void SwitchWeapon(bool ascendingOrder)
    {
        int newWeaponIndex = -1;
        int closestSlotDistance = m_WeaponSlots.Length;
        for (int i = 0; i < m_WeaponSlots.Length; i++)
        {
            if (i != ActiveWeaponIndex && GetWeaponAtSlotIndex(i) != null)
            {
                int distanceToActiveIndex = GetDistanceBetweenWeaponSlots(ActiveWeaponIndex, i, ascendingOrder);

                if (distanceToActiveIndex < closestSlotDistance)
                {
                    closestSlotDistance = distanceToActiveIndex;
                    newWeaponIndex = i;
                }
            }
        }

        SwitchToWeaponIndex(newWeaponIndex);
    }
        
    public void SwitchToWeaponIndex(int newWeaponIndex, bool force = false)
    {
        if (force || (newWeaponIndex != ActiveWeaponIndex && newWeaponIndex >= 0))
        {
            m_WeaponSwitchNewWeaponIndex = newWeaponIndex;
            m_TimeStartedWeaponSwitch = Time.time;

            if (GetActiveWeapon() == null)
            {
                m_WeaponSwitchState = WeaponSwitchState.PutUpNew;
                ActiveWeaponIndex = m_WeaponSwitchNewWeaponIndex;

                WeaponController newWeapon = GetWeaponAtSlotIndex(m_WeaponSwitchNewWeaponIndex);
                if (OnSwitchedToWeapon != null)
                {
                    OnSwitchedToWeapon.Invoke(newWeapon);
                }
            }
            else
            {
                m_WeaponSwitchState = WeaponSwitchState.PutDownPrevious;
            }
        }
    }
        
    public WeaponController HasWeapon(WeaponController weaponPrefab)
    {
        // Checks if we already have a weapon coming from the specified prefab
        for (var index = 0; index < m_WeaponSlots.Length; index++)
        {
            var w = m_WeaponSlots[index];
            if (w != null && w.SourcePrefab == weaponPrefab.gameObject)
            {
                return w;
            }
        }

        return null;
    }
    void UpdateWeaponAiming()
    {
        if (m_WeaponSwitchState == WeaponSwitchState.Up)
        {
            WeaponController activeWeapon = GetActiveWeapon();  
            if (IsAiming && activeWeapon)
            {
                m_WeaponMainLocalPosition = Vector3.Lerp(m_WeaponMainLocalPosition,
                    AimingWeaponPosition.localPosition + activeWeapon.AimOffset,
                    AimingAnimationSpeed * Time.deltaTime);
                SetFov(Mathf.Lerp(m_PlayerController.PlayerCamera.fieldOfView,
                    activeWeapon.AimZoomRatio * DefaultFov, AimingAnimationSpeed * Time.deltaTime));
            }
            else
            {
                m_WeaponMainLocalPosition = Vector3.Lerp(m_WeaponMainLocalPosition,
                    DefaultWeaponPosition.localPosition, AimingAnimationSpeed * Time.deltaTime);
                SetFov(Mathf.Lerp(m_PlayerController.PlayerCamera.fieldOfView, DefaultFov,
                    AimingAnimationSpeed * Time.deltaTime));
            }
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
    void UpdateWeaponSwitching()
    {
        float switchingTimeFactor = 0f;
        if (WeaponSwitchDelay == 0f)
        {
            switchingTimeFactor = 1f;
        }
        else
        {
            switchingTimeFactor = Mathf.Clamp01((Time.time - m_TimeStartedWeaponSwitch) / WeaponSwitchDelay);
        }

        // Handle transiting to new switch state
        if (switchingTimeFactor >= 1f)
        {
            if (m_WeaponSwitchState == WeaponSwitchState.PutDownPrevious)
            {
                // Deactivate old weapon
                WeaponController oldWeapon = GetWeaponAtSlotIndex(ActiveWeaponIndex);
                if (oldWeapon != null)
                {
                    oldWeapon.ShowWeapon(false);
                }

                ActiveWeaponIndex = m_WeaponSwitchNewWeaponIndex;
                switchingTimeFactor = 0f;

                // Activate new weapon
                WeaponController newWeapon = GetWeaponAtSlotIndex(ActiveWeaponIndex);
                if (OnSwitchedToWeapon != null)
                {
                    OnSwitchedToWeapon.Invoke(newWeapon);
                }

                if (newWeapon)
                {
                    m_TimeStartedWeaponSwitch = Time.time;
                    m_WeaponSwitchState = WeaponSwitchState.PutUpNew;
                }
                else
                {
                    // if new weapon is null, don't follow through with putting weapon back up
                    m_WeaponSwitchState = WeaponSwitchState.Down;
                }
            }
            else if (m_WeaponSwitchState == WeaponSwitchState.PutUpNew)
            {
                m_WeaponSwitchState = WeaponSwitchState.Up;
            }
        }

    }
        

    // Adds a weapon to our inventory
    public bool AddWeapon(WeaponController weaponPrefab)
    {
        if (HasWeapon(weaponPrefab) != null)
        {
            return false;
        }

        // search our weapon slots for the first free one, assign the weapon to it, and return true if we found one. Return false otherwise
        for (int i = 0; i < m_WeaponSlots.Length; i++)
        {
            // only add the weapon if the slot is free
            if (m_WeaponSlots[i] == null)
            {
                // spawn the weapon prefab as child of the weapon socket
                WeaponController weaponInstance = Instantiate(weaponPrefab, WeaponParentSocket);
                weaponInstance.transform.localPosition = Vector3.zero;
                weaponInstance.transform.localRotation = Quaternion.identity;
                // Set owner to this gameObject so the weapon can alter projectile/damage logic accordingly
                weaponInstance.Owner = gameObject;
                weaponInstance.SourcePrefab = weaponPrefab.gameObject;
                weaponInstance.ShowWeapon(false);

                // Assign the first person layer to the weapon
                int layerIndex = Mathf.RoundToInt(Mathf.Log(FpsWeaponLayer.value, 2)); // This function converts a layermask to a layer index
                foreach (Transform t in weaponInstance.gameObject.GetComponentsInChildren<Transform>(true))
                {
                    t.gameObject.layer = layerIndex;
                }

                m_WeaponSlots[i] = weaponInstance;

                if (OnAddedWeapon != null)
                {
                    OnAddedWeapon.Invoke(weaponInstance, i);
                }

                return true;
            }
        }

        // Handle auto-switching to weapon if no weapons currently
        if (GetActiveWeapon() == null)
        {
            SwitchWeapon(true);
        }

        return false;
    }

    public bool RemoveWeapon(WeaponController weaponInstance)
    {
        // Look through our slots for that weapon
        for (int i = 0; i < m_WeaponSlots.Length; i++)
        {
            // when weapon found, remove it
            if (m_WeaponSlots[i] == weaponInstance)
            {
                m_WeaponSlots[i] = null;

                if (OnRemovedWeapon != null)
                {
                    OnRemovedWeapon.Invoke(weaponInstance, i);
                }

                Destroy(weaponInstance.gameObject);

                // Handle case of removing active weapon (switch to next weapon)
                if (i == ActiveWeaponIndex)
                {
                    SwitchWeapon(true);
                }

                return true;
            }
        }

        return false;
    }

    public WeaponController GetActiveWeapon()
    {
        return GetWeaponAtSlotIndex(ActiveWeaponIndex);
    }

    public WeaponController GetWeaponAtSlotIndex(int index)
    {
        // find the active weapon in our weapon slots based on our active weapon index
        if (index >= 0 &&
            index < m_WeaponSlots.Length)
        {
            return m_WeaponSlots[index];
        }
        return null;
    }

    int GetDistanceBetweenWeaponSlots(int fromSlotIndex, int toSlotIndex, bool ascendingOrder)
    {
        int distanceBetweenSlots = 0;

        if (ascendingOrder)
            distanceBetweenSlots = toSlotIndex - fromSlotIndex;
        else
            distanceBetweenSlots = -1 * (toSlotIndex - fromSlotIndex);

        if (distanceBetweenSlots < 0)
            distanceBetweenSlots = m_WeaponSlots.Length + distanceBetweenSlots;

        return distanceBetweenSlots;
    }

    void OnWeaponSwitched(WeaponController newWeapon)
    {
        if (newWeapon != null)
            newWeapon.ShowWeapon(true);
    }
}
