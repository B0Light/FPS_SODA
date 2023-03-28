using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Pickup : MonoBehaviourPun
{
    public int ItemId;
    [Space(10)]
    public float VerticalBobFrequency = 1f;
    public float BobbingAmount = 1f;
    public float RotatingSpeed = 360f;
    public AudioClip PickupSfx;
    public GameObject PickupVfxPrefab;

    public Rigidbody PickupRigidbody { get; private set; }

    Collider m_Collider;
    Vector3 m_StartPosition;
    bool m_HasPlayedFeedback;

    protected virtual void Start()
    {
        PickupRigidbody = GetComponent<Rigidbody>();
        m_Collider = GetComponent<Collider>();
           

        // ensure the physics setup is a kinematic rigidbody trigger
        PickupRigidbody.isKinematic = true;
        m_Collider.isTrigger = true;

        // Remember start position for animation
        m_StartPosition = transform.position;
    }

    void Update()
    {
        // Handle bobbing
        float bobbingAnimationPhase = ((Mathf.Sin(Time.time * VerticalBobFrequency) * 0.5f) + 0.5f) * BobbingAmount;
        transform.position = m_StartPosition + Vector3.up * bobbingAnimationPhase;

        // Handle rotating
        transform.Rotate(Vector3.up, RotatingSpeed * Time.deltaTime, Space.Self);
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerController pickingPlayer = other.GetComponent<PlayerController>();

        if (pickingPlayer != null)
        {
            OnPicked(pickingPlayer);
        }
    }

    protected virtual void OnPicked(PlayerController playerController)
    {
        PlayPickupFeedback();
    }

    public void PlayPickupFeedback()
    {
        if (m_HasPlayedFeedback)
            return;

        if (PickupVfxPrefab)
        {
            var pickupVfxInstance = Instantiate(PickupVfxPrefab, transform.position, Quaternion.identity);
        }

        m_HasPlayedFeedback = true;
    }

    [PunRPC]
    public void destroyThisObj()
    {
        Destroy(gameObject);
    }
}
