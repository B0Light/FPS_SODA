using System.Collections;
using UnityEngine;

public class BossRock : EnemyBullet
{
    
    private float angularPower = 2;
    private float scaleValue = 0.1f;

    private bool isShoot;

    protected override void Start()
    {
        StartCoroutine(GainPowerTimer());
        StartCoroutine(GainPower());
        base.Start();
    }
    IEnumerator GainPowerTimer()
    {
        yield return new WaitForSeconds(2.2f);
        isShoot = true;
    }

    IEnumerator GainPower()
    {
        while (!isShoot)
        {
            angularPower += 0.04f;
            scaleValue += 0.002f;
            transform.localScale = Vector3.one * scaleValue;
            m_rigidbody.AddTorque(transform.right * angularPower, ForceMode.Acceleration);
            yield return null;
        }
    }
}
