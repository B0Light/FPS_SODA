using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

public enum WeaponShootType
{
    Manual,
    Automatic,
    Charge,
}

[System.Serializable]
public struct CrosshairData
{
    public Sprite CrosshairSprite;
    public int CrosshairSize;
    public Color CrosshairColor;
}

[RequireComponent(typeof(AudioSource))]
public class WeaponController : MonoBehaviourPun
{
    public int WeaponId;
    private PhotonView m_PhotonView;
    [Space(10)]
    [Header("Information")]
    public string WeaponName;
    public Sprite WeaponIcon;
    public CrosshairData CrosshairDataDefault;

    [Header("Internal References")]
    public GameObject WeaponRoot;
    public Transform WeaponMuzzle;

    [Header("Shoot Parameters")]
    public WeaponShootType ShootType;
    public ProjectileBase[] ProjectilePrefab;
    public int ProjectileID = 0;


    public float DelayBetweenShots = 0.5f;
    public float BulletSpreadAngle = 0f;
    public int BulletsPerShot = 1;

    [Range(0f, 2f)]
    public float RecoilForce = 1;

    [Range(0f, 1f)]
    public float AimZoomRatio = 1f;

    public Vector3 AimOffset;

    [Header("Ammo Parameters")]
    public bool AutomaticReload = true;
    public bool HasPhysicalBullets = false;
    public int ClipSize = 30;
    public GameObject ShellCasing;
    public Transform EjectionPort;
    [Range(0.0f, 5.0f)] public float ShellCasingEjectionForce = 2.0f;
    [Range(1, 30)] public int ShellPoolSize = 1;
    public float AmmoReloadRate = 1f;
    public float AmmoReloadDelay = 2f;
    public int MaxAmmo = 8;

    [Header("Charging parameters (charging weapons only)")]
    public bool AutomaticReleaseOnCharged;
    public float MaxChargeDuration = 2f;
    public float AmmoUsedOnStartCharge = 1f;
    public float AmmoUsageRateWhileCharging = 1f;

    [Header("Audio & Visual")]
    public GameObject[] MuzzleFlashPrefab;
    public bool UnparentMuzzleFlash;
    public AudioClip ShootSfx;
    public AudioClip ChangeWeaponSfx;
    public bool UseContinuousShootSound = false;
    public AudioClip ContinuousShootStartSfx;
    public AudioClip ContinuousShootLoopSfx;
    public AudioClip ContinuousShootEndSfx;
    AudioSource m_ContinuousShootAudioSource = null;
    bool m_WantsToShoot = false;

    public UnityAction OnShoot;
    public event Action OnShootProcessed;

    int m_CarriedPhysicalBullets;
    float m_CurrentAmmo;
    float m_LastTimeShot = Mathf.NegativeInfinity;
    public float LastChargeTriggerTimestamp { get; private set; }
    Vector3 m_LastMuzzlePosition;

    public GameObject Owner;
    public GameObject SourcePrefab { get; set; }
    public bool IsCharging { get; private set; }
    public float CurrentAmmoRatio { get; private set; }
    public bool IsWeaponActive { get; private set; }
    public bool IsCooling { get; private set; }
    public float CurrentCharge { get; private set; }
    public Vector3 MuzzleWorldVelocity { get; private set; }

    public float GetAmmoNeededToShoot() =>
        (ShootType != WeaponShootType.Charge ? 1f : Mathf.Max(1f, AmmoUsedOnStartCharge)) /
        (MaxAmmo * BulletsPerShot);

    public int GetCarriedPhysicalBullets() => m_CarriedPhysicalBullets;
    public int GetCurrentAmmo() => Mathf.FloorToInt(m_CurrentAmmo);

    AudioSource m_ShootAudioSource;

    public bool IsReloading { get; private set; }

    private Queue<Rigidbody> m_PhysicalAmmoPool;
    

    void Awake()
    {
        m_CurrentAmmo = MaxAmmo;
        m_CarriedPhysicalBullets = HasPhysicalBullets ? ClipSize : 0;

        if (HasPhysicalBullets)
        {
            m_PhysicalAmmoPool = new Queue<Rigidbody>(ShellPoolSize);

            for (int i = 0; i < ShellPoolSize; i++)
            {
                GameObject shell = Instantiate(ShellCasing, transform);
                shell.SetActive(false);
                m_PhysicalAmmoPool.Enqueue(shell.GetComponent<Rigidbody>());
            }
        }
    }

    private void Start()
    {
        m_PhotonView = GetComponent<PhotonView>();
        m_ShootAudioSource = GetComponent<AudioSource>();
        if (UseContinuousShootSound)
        {
            m_ContinuousShootAudioSource = gameObject.AddComponent<AudioSource>();
            m_ContinuousShootAudioSource.playOnAwake = false;
            m_ContinuousShootAudioSource.clip = ContinuousShootLoopSfx;
            m_ContinuousShootAudioSource.loop = true;
        }
    }

    public void AddCarriablePhysicalBullets(int count) => m_CarriedPhysicalBullets = Mathf.Max(m_CarriedPhysicalBullets + count, MaxAmmo);

    void ShootShell()
    {
        Rigidbody nextShell = m_PhysicalAmmoPool.Dequeue();

        nextShell.transform.position = EjectionPort.transform.position;
        nextShell.transform.rotation = EjectionPort.transform.rotation;
        nextShell.gameObject.SetActive(true);
        nextShell.transform.SetParent(null);
        nextShell.collisionDetectionMode = CollisionDetectionMode.Continuous;
        nextShell.AddForce(nextShell.transform.up * ShellCasingEjectionForce, ForceMode.Impulse);

        m_PhysicalAmmoPool.Enqueue(nextShell);
    }

    void Reload()
    {
        if (m_CarriedPhysicalBullets > 0)
        {
            m_CurrentAmmo = Mathf.Min(m_CarriedPhysicalBullets, ClipSize);
        }

        IsReloading = false;
    }

    void Update()
    {
        if(photonView.IsMine)
        {
            UpdateAmmo();
            UpdateCharge();
            UpdateContinuousShootSound();
        }
       
        if (Time.deltaTime > 0)
        {
            MuzzleWorldVelocity = (WeaponMuzzle.position - m_LastMuzzlePosition) / Time.deltaTime;
            m_LastMuzzlePosition = WeaponMuzzle.position;
        }
    }

    void UpdateAmmo()
    {
        if (AutomaticReload && m_LastTimeShot + AmmoReloadDelay < Time.time && m_CurrentAmmo < MaxAmmo && !IsCharging)
        {
            // reloads weapon over time
            m_CurrentAmmo += AmmoReloadRate * Time.deltaTime;

            // limits ammo to max value
            m_CurrentAmmo = Mathf.Clamp(m_CurrentAmmo, 0, MaxAmmo);

            IsCooling = true;
        }
        else
        {
            IsCooling = false;
        }

        if (MaxAmmo == Mathf.Infinity)
        {
            CurrentAmmoRatio = 1f;
        }
        else
        {
            CurrentAmmoRatio = m_CurrentAmmo / MaxAmmo;
        }
    }

    void UpdateCharge()
    {
        if (IsCharging)
        {
            if (CurrentCharge < 1f)
            {
                float chargeLeft = 1f - CurrentCharge;

                float chargeAdded = 0f;
                if (MaxChargeDuration <= 0f)
                {
                    chargeAdded = chargeLeft;
                }
                else
                {
                    chargeAdded = (1f / MaxChargeDuration) * Time.deltaTime;
                }

                chargeAdded = Mathf.Clamp(chargeAdded, 0f, chargeLeft);

                // See if we can actually add this charge
                float ammoThisChargeWouldRequire = chargeAdded * AmmoUsageRateWhileCharging;
                if (ammoThisChargeWouldRequire <= m_CurrentAmmo)
                {
                    // Use ammo based on charge added
                    UseAmmo(ammoThisChargeWouldRequire);

                    // set current charge ratio
                    CurrentCharge = Mathf.Clamp01(CurrentCharge + chargeAdded);
                }
            }
        }
    }

    void UpdateContinuousShootSound()
    {
        if (UseContinuousShootSound)
        {
            if (m_WantsToShoot && m_CurrentAmmo >= 1f)
            {
                if (!m_ContinuousShootAudioSource.isPlaying)
                {
                    m_ShootAudioSource.PlayOneShot(ShootSfx);
                    m_ShootAudioSource.PlayOneShot(ContinuousShootStartSfx);
                    m_ContinuousShootAudioSource.Play();
                }
            }
            else if (m_ContinuousShootAudioSource.isPlaying)
            {
                m_ShootAudioSource.PlayOneShot(ContinuousShootEndSfx);
                m_ContinuousShootAudioSource.Stop();
            }
        }
    }

    public void UseAmmo(float amount)
    {
        m_CurrentAmmo = Mathf.Clamp(m_CurrentAmmo - amount, 0f, MaxAmmo);
        m_CarriedPhysicalBullets -= Mathf.RoundToInt(amount);
        m_CarriedPhysicalBullets = Mathf.Clamp(m_CarriedPhysicalBullets, 0, MaxAmmo);
        m_LastTimeShot = Time.time;
    }

    public bool HandleShootInputs(bool inputDown, bool inputHeld)
    {
        m_WantsToShoot = inputDown || inputHeld;
        switch (ShootType)
        {
            case WeaponShootType.Manual:
                if (inputDown)
                {
                    return TryShoot();
                }

                return false;

            case WeaponShootType.Automatic:
                if (inputHeld)
                {
                    return TryShoot();
                }

                return false;

            case WeaponShootType.Charge:
                if (inputHeld)
                {
                    TryBeginCharge();
                }

                // Check if we released charge or if the weapon shoot autmatically when it's fully charged
                if (!inputHeld || (AutomaticReleaseOnCharged && CurrentCharge >= 1f))
                {
                    return TryReleaseCharge();
                }

                return false;

            default:
                return false;
        }
    }

    [PunRPC]
    bool TryShoot()
    {
        if (m_CurrentAmmo >= 1f
            && m_LastTimeShot + DelayBetweenShots < Time.time)
        {
            m_PhotonView.RPC("HandleShoot", RpcTarget.All, null);
            m_CurrentAmmo -= 1f;
            return true;
        }
        return false;
    }

    [PunRPC]
    void TryBeginCharge()
    {
        if (!IsCharging
            && m_CurrentAmmo >= AmmoUsedOnStartCharge
            && Mathf.FloorToInt((m_CurrentAmmo - AmmoUsedOnStartCharge) * BulletsPerShot) > 0
            && m_LastTimeShot + DelayBetweenShots < Time.time)
        {
            UseAmmo(AmmoUsedOnStartCharge);

            LastChargeTriggerTimestamp = Time.time;
            IsCharging = true;
        }
    }

    [PunRPC]
    bool TryReleaseCharge()
    {
        if (IsCharging)
        {
            m_PhotonView.RPC("HandleShoot", RpcTarget.All, null);
            CurrentCharge = 0f;
            IsCharging = false;
            return true;
        }

        return false;
    }

    [PunRPC]
    void HandleShoot()
    {
        if (!m_PhotonView.IsMine)
        {
            CurrentCharge = 1;
        }   
        int bulletsPerShotFinal = (ShootType == WeaponShootType.Charge)
            ? Mathf.CeilToInt(CurrentCharge * BulletsPerShot)
            : BulletsPerShot;

        // spawn all bullets with random direction
        for (int i = 0; i < bulletsPerShotFinal; i++)
        {
            Vector3 shotDirection = GetShotDirectionWithinSpread(WeaponMuzzle);
            ProjectileBase newProjectile = Instantiate(ProjectilePrefab[ProjectileID], WeaponMuzzle.position,
                Quaternion.LookRotation(shotDirection));
            newProjectile.Shoot(this);
        }

        // muzzle flash
        if (MuzzleFlashPrefab[ProjectileID] != null)
        {
            GameObject muzzleFlashInstance = Instantiate(MuzzleFlashPrefab[ProjectileID], WeaponMuzzle.position,
                WeaponMuzzle.rotation, WeaponMuzzle.transform);
            // Unparent the muzzleFlashInstance
            if (UnparentMuzzleFlash)
            {
                muzzleFlashInstance.transform.SetParent(null);
            }

            Destroy(muzzleFlashInstance, 1f);
        }

        if (HasPhysicalBullets)
        {
            ShootShell();
            m_CarriedPhysicalBullets--;
        }

        m_LastTimeShot = Time.time;

        // play shoot SFX
        if (ShootSfx && !UseContinuousShootSound)
        {
            m_ShootAudioSource.PlayOneShot(ShootSfx);
        }

        OnShoot?.Invoke();
        OnShootProcessed?.Invoke();
    }

    public Vector3 GetShotDirectionWithinSpread(Transform shootTransform)
    {
        float spreadAngleRatio = BulletSpreadAngle / 180f;
        Vector3 spreadWorldDirection = Vector3.Slerp(shootTransform.forward, UnityEngine.Random.insideUnitSphere,
            spreadAngleRatio);

        return spreadWorldDirection;
    }

    [PunRPC]
    public void Upgrade()
    {
        if (ProjectilePrefab[ProjectileID+1] != null)
        {
            ProjectileID++;
            m_PhotonView.RPC("Upgrade", RpcTarget.OthersBuffered, null);
        }
            
    }
}