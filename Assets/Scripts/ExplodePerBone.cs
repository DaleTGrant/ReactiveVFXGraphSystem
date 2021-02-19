using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

public class ExplodePerBone : MonoBehaviour
{
    private List<ParticleCollisionEvent> collisionEvents;
    private ExplodeCollector explodeCollector;
    private int COLCOUNT = 5;

    private void Awake()
    {
        collisionEvents = new List<ParticleCollisionEvent>();
        explodeCollector = GetComponentInParent<ExplodeCollector>();
    }

    private void Start()
    {
        if (!explodeCollector)
        {
            explodeCollector = GetComponentInParent<ExplodeCollector>();
        }
    }
    
    
    
    private void OnParticleCollision(GameObject other)
    {
        int count = other.GetComponent<ParticleSystem>().GetCollisionEvents(gameObject, collisionEvents);
        
        List<Vector3> positions = new List<Vector3>();
        List<Vector3> velocities = new List<Vector3>();
        for (int i = 0; i < count; i++)
        {
            var collision = collisionEvents[i];
            // // Use to disable collider that is hit
            Collider col = collision.colliderComponent as Collider;
            // if (col!=null)
            // {
            //     col.enabled = false;
            // }
            // /////
            var position = collision.intersection;
            var velocity = collision.velocity;

            if (i < COLCOUNT)
            {
                positions.Add(position);
                velocities.Add(velocity); 
            }
            
        }
        
        // Send information to the collection script
        if (explodeCollector)
        {
            explodeCollector.CollectCollision(positions,velocities);
        }
    }

    // private void OnDrawGizmosSelected()
    // {
    //     for (int i = 0; i < posTest.Count; i++)
    //     {
    //         var pos = posTest[i];
    //         var vel = velTest[i];
    //         
    //         Gizmos.color = new Color(1.0f, 0.92f, 0.016f, 0.5f);
    //         Gizmos.DrawSphere(transform.localToWorldMatrix.MultiplyPoint3x4(pos),radius);
    //         Gizmos.color = Color.red;
    //         Gizmos.DrawRay(transform.localToWorldMatrix.MultiplyPoint3x4(pos),transform.localToWorldMatrix.MultiplyVector(vel.normalized));
    //     }
    //     // Gizmos.color = new Color(1.0f, 0.92f, 0.016f, 0.5f);
    //     // Gizmos.DrawSphere(transform.localToWorldMatrix.MultiplyPoint3x4(position),radius);
    //     // Gizmos.color = Color.red;
    //     // Gizmos.DrawRay(transform.localToWorldMatrix.MultiplyPoint3x4(position),transform.localToWorldMatrix.MultiplyVector(velocity.normalized));
    // }
}
