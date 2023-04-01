using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBoss : EnemyController
{
    void Start()
    {
        _health = GetComponent<Health>();
        _anim = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
