using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
public abstract class ProjectileBase : MonoBehaviourPun
{
    public GameObject Owner;
    public Vector3 InitialPosition { get; private set; }
    public Vector3 InitialDirection { get; private set; }
    public Vector3 InheritedMuzzleVelocity { get; private set; }
    public float InitialCharge { get; private set; }

    public UnityAction OnShoot;

    public int projectileLV { get; set; }

    public void Shoot(WeaponController controller)
    {
        Owner = controller.Owner;
        InitialPosition = transform.position;
        InitialDirection = transform.forward;
        InheritedMuzzleVelocity = controller.MuzzleWorldVelocity;
        InitialCharge = controller.CurrentCharge;

        OnShoot?.Invoke();
    }
}
