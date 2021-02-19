using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooting : MonoBehaviour
{

    [SerializeField] private VFXAnimationTransition vfxAnimationTransition;
    private Animator animator;

    [SerializeField] private ParticleSystem shootSystem;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        vfxAnimationTransition = GetComponent<VFXAnimationTransition>();
    }

    private void Start()
    {
        Shoot();
    }

    private void Shoot()
    {
        vfxAnimationTransition.ChangeAnimation(VFXAnimationTransition.AnimationName.Shoot,0f,false);

        if (animator) animator.SetBool("IsShooting",true);
    }

    // Spawn a particle system to shoot when called.
    private void FireProjectile()
    {
        shootSystem.Stop();
        shootSystem.Clear();
        shootSystem.Play();

    }

}
