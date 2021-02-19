using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.VFX;

public class ExplodeOnContact : MonoBehaviour
{
    private enum explodeTypes {Single, Average, Burst};
    [SerializeField] private explodeTypes explodeType = explodeTypes.Single;
    
    [SerializeField] private List<ParticleCollisionEvent> collisionEvents;
    [SerializeField] private LayerMask layermask;
    private VisualEffect _visualEffect;
    
    private Vector3 COLVEL = new Vector3(0.01f,0f,0f);
    private Vector3 COLPOS = new Vector3(100f,100f,100f);
    private float COLRADIUS = 0.2f;

    private bool hasExplode = false;

    private Matrix4x4 worldToLocal;
    private Vector3 position;
    private Vector3 velocity;
    private float radius;
    

    private void Awake()
    {
        worldToLocal = transform.worldToLocalMatrix;
        collisionEvents = new List<ParticleCollisionEvent>();
        _visualEffect = GetComponent<VisualEffect>();
    }

    private bool ExistsInLayerMask(int layer)
    {
        return (layermask.value >> layer) % 2 == 1;
    }

    private void Explode(Vector3 contactPosition, Vector3 relativeVelocity, float collisionRadius)
    {
        _visualEffect.SetVector3("Contact Position",contactPosition);
        _visualEffect.SetVector3("Collider Velocity",relativeVelocity);
        _visualEffect.SetFloat("Collision Radius",collisionRadius);
        hasExplode = true;
        
        Invoke("Reset",1*VFXManager.maxDeltaTime);
    }

    private void Reset()
    {
        if(hasExplode)
        {
            _visualEffect.SetVector3("Contact Position", COLPOS);
            _visualEffect.SetVector3("Collider Velocity", COLVEL);
            _visualEffect.SetFloat("Collision Radius", COLRADIUS);
            hasExplode = false;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (ExistsInLayerMask(other.gameObject.layer))
        {
            Explode(other.GetContact(0).point, other.relativeVelocity,COLRADIUS);
        }
    }

    private void OnParticleCollision(GameObject other)
    {
        int count = other.GetComponent<ParticleSystem>().GetCollisionEvents(gameObject, collisionEvents);
        
        position = Vector3.zero;
        velocity = Vector3.zero;
        radius = 0f;
        
        switch (explodeType)
        {
            // Only consider the first point of contact
            case explodeTypes.Single:
                position = worldToLocal.MultiplyPoint3x4(collisionEvents[0].intersection);
                velocity = worldToLocal.MultiplyVector(collisionEvents[0].velocity);
                radius = COLRADIUS;
                break;
            // Find the average position, velocity, and radius around average position for all collision events
            case explodeTypes.Average:
                
                List<Vector3> positionList = new List<Vector3>();
                foreach (var collision in collisionEvents)
                {
                    var localPosition = worldToLocal.MultiplyPoint3x4(collision.intersection);
                    var localVelocity = worldToLocal.MultiplyVector(collision.velocity);
                    
                    positionList.Add(localPosition);

                    position += localPosition / count;
                    velocity += localVelocity / count;
                }
                
                foreach (var pos in positionList)
                {
                    radius += Vector3.Distance(pos, position) / positionList.Count;
                }
                break;
            // Somehow consider all collision events independently
            // Look into vfx binders, point cache, and sub-graph operators
            case explodeTypes.Burst:
                break;
        }
        
        Explode(position, velocity, radius);

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1.0f, 0.92f, 0.016f, 0.5f);
        Gizmos.DrawSphere(transform.localToWorldMatrix.MultiplyPoint3x4(position),radius);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.localToWorldMatrix.MultiplyPoint3x4(position),transform.localToWorldMatrix.MultiplyVector(velocity.normalized));
    }
}
