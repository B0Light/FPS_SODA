using System.Collections;
using UnityEngine;

public class EnemyBoom : MonoBehaviour
{
    Rigidbody _rbody;

    private void Awake()
    {
        _rbody = GetComponent<Rigidbody>();
    }
    void Start()
    {
        Vector3 GrenadePos = Vector3.zero;
        GrenadePos.y = 15;
        _rbody.AddForce(GrenadePos, ForceMode.Impulse);
    }
}
