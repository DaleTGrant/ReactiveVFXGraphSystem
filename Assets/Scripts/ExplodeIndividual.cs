using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.VFX;

public class ExplodeIndividual : MonoBehaviour
{
    private enum explodeTypes {Single, Average, Burst};
    [SerializeField] private explodeTypes explodeType = explodeTypes.Single;
    
    [SerializeField] private List<ParticleCollisionEvent> collisionEvents;
    [SerializeField] private LayerMask layermask;
    public VisualEffect _visualEffect;
    
    private Vector3 COLVEL = new Vector3(0.01f,0f,0f);
    private Vector3 COLPOS = new Vector3(100f,100f,100f);
    [SerializeField] private float COLRADIUS = 0.2f;

    private int COLCOUNT = 5;
    private int PROPCOUNT = 2;

    private bool hasExplode = false;

    private Matrix4x4 worldToLocal;
    private Vector3 position;
    private Vector3 velocity;
    private float radius;

    private List<Vector3> posTest = new List<Vector3>(5);
    private List<Vector3> velTest = new List<Vector3>(5);

    // Texture to write to on collision, x: collision index, y: 0 - position, 1 - velocity
    [SerializeField] private Texture2D collisionTexture;
    

    private void Awake()
    {
        worldToLocal = transform.worldToLocalMatrix;
        collisionEvents = new List<ParticleCollisionEvent>();
        if(!_visualEffect) _visualEffect = GetComponent<VisualEffect>();
        InitializeTexture();
    }

    private void InitializeTexture()
    {
        collisionTexture = new Texture2D(COLCOUNT,PROPCOUNT,TextureFormat.RGBAHalf,false,false);
        ResetTexture();
        _visualEffect.SetTexture("Collision Texture",collisionTexture);
    }
    
    

    private bool ExistsInLayerMask(int layer)
    {
        return (layermask.value >> layer) % 2 == 1;
    }

    private void Explode(Vector3 contactPosition, Vector3 relativeVelocity, float collisionRadius)
    {
        if (!_visualEffect.HasTexture("Collision Texture"))
        {
            _visualEffect.SetVector3("Contact Position",contactPosition);
            _visualEffect.SetVector3("Collider Velocity",relativeVelocity);
            
        }
        else
        {
            _visualEffect.SetTexture("Collision Texture",collisionTexture);
        }
        _visualEffect.SetFloat("Collision Radius",collisionRadius);
        hasExplode = true;
        
        
        
        Invoke("Reset",1*VFXManager.maxDeltaTime);
    }

    private void Reset()
    {
        if(hasExplode)
        {
            if (!_visualEffect.HasTexture("Collision Texture"))
            {
                _visualEffect.SetVector3("Contact Position",COLPOS);
                _visualEffect.SetVector3("Collider Velocity",COLVEL);
            
            }
            else
            {
                ResetTexture();
                _visualEffect.SetTexture("Collision Texture",collisionTexture);

                for (int i = 0; i < COLCOUNT; i++)
                {
                    posTest[i] = COLPOS;
                    velTest[i] = COLVEL;
                }
            }
            _visualEffect.SetFloat("Collision Radius",COLRADIUS);
            hasExplode = false;
        }
    }

    private void GenerateTexture(List<Vector3> positions,List<Vector3> velocities)
    {
        for (int y = 0; y < PROPCOUNT; y++)
        {
            var vectorList = y == 0 ? positions : velocities;
            
            for (int x = 0; x < COLCOUNT; x++)
            {
                var color = vectorList[x];
                collisionTexture.SetPixel(x,y,new Color(color.x,color.y,color.z,1));
            }
        }
        collisionTexture.Apply();
    }

    private void ResetTexture()
    {
        for (int y = 0; y < PROPCOUNT; y++)
        {
            var color = y == 0 ? COLPOS : COLVEL;
            
            for (int x = 0; x < COLCOUNT; x++)
            {
                collisionTexture.SetPixel(x,y,new Color(color.x,color.y,color.z,1));
            }
        }
        collisionTexture.Apply();
        
        
    }

    // private void OnCollisionEnter(Collision other)
    // {
    //     if (ExistsInLayerMask(other.gameObject.layer))
    //     {
    //         Explode(other.GetContact(0).point, other.relativeVelocity,COLRADIUS);
    //     }
    // }

    private void OnParticleCollision(GameObject other)
    {
        int count = other.GetComponent<ParticleSystem>().GetCollisionEvents(gameObject, collisionEvents);
        
        position = Vector3.zero;
        velocity = Vector3.zero;
        radius = 0f;
        
        // Update the World to Local matrix each time it is needed
        // Needed to change to Visual Effect transform due to movement/vfx separation
        // worldToLocal = transform.worldToLocalMatrix;
        worldToLocal = _visualEffect.transform.worldToLocalMatrix;

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
                radius = COLRADIUS;
                List<Vector3> positions = new List<Vector3>(COLCOUNT);
                List<Vector3> velocities = new List<Vector3>(COLCOUNT);
                for (int i = 0; i < COLCOUNT; i++)
                {
                    var localPosition = COLPOS;
                    var localVelocity = COLVEL;
                    if (i < count)
                    {
                        var collision = collisionEvents[i];
                        localPosition = worldToLocal.MultiplyPoint3x4(collision.intersection);
                        localVelocity = worldToLocal.MultiplyVector(collision.velocity);
                        
                        //  // Use to disable collider that is hit
                        // Collider col = collision.colliderComponent as Collider;
                        // if (col!=null)
                        // {
                        //     col.enabled = false;
                        // }
                        // ///
                        
                    }
                    
                    positions.Insert(i,localPosition);
                    velocities.Insert(i,localVelocity);
                }
                
                GenerateTexture(positions,velocities);

                // For Testing
                posTest = positions;
                velTest = velocities;
                //
                break;
        }
        Explode(position, velocity, radius);
    }

    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < posTest.Count; i++)
        {
            var pos = posTest[i];
            var vel = velTest[i];
            
            Gizmos.color = new Color(1.0f, 0.92f, 0.016f, 0.5f);
            Gizmos.DrawSphere(transform.localToWorldMatrix.MultiplyPoint3x4(pos),radius);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.localToWorldMatrix.MultiplyPoint3x4(pos),transform.localToWorldMatrix.MultiplyVector(vel.normalized));
        }
        // Gizmos.color = new Color(1.0f, 0.92f, 0.016f, 0.5f);
        // Gizmos.DrawSphere(transform.localToWorldMatrix.MultiplyPoint3x4(position),radius);
        // Gizmos.color = Color.red;
        // Gizmos.DrawRay(transform.localToWorldMatrix.MultiplyPoint3x4(position),transform.localToWorldMatrix.MultiplyVector(velocity.normalized));
    }
}
