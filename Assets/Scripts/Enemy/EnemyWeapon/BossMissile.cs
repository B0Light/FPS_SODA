using UnityEngine;
public class BossMissile : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {
        transform.Rotate(Vector3.down * 2000f * Time.deltaTime);
    }
}
