using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.VFX;

public class ExplodeCollector : MonoBehaviour
{
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
    private List<Vector3> finalPositions;
    private List<Vector3> finalVelocities;
    [SerializeField] private Texture2D collisionTexture;
    

    private void Awake()
    {
        if(!_visualEffect) _visualEffect = GetComponent<VisualEffect>();
        worldToLocal = _visualEffect.transform.worldToLocalMatrix;
        InitializeTexture();
        finalPositions = new List<Vector3>();
        finalVelocities = new List<Vector3>();
    }

    private void LateUpdate()
    {
        
        if (!hasExplode && finalPositions.Count > 0)
        {
            ApplyAllCollisions();
        }
    }
    
    private void InitializeTexture()
    {
        collisionTexture = new Texture2D(COLCOUNT,PROPCOUNT,TextureFormat.RGBAHalf,false,false);
        ResetTexture();
        _visualEffect.SetTexture("Collision Texture",collisionTexture);
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

    // called by each bones OnParticleCollision and populates the list of positions/velocities for the VAT
    public void CollectCollision(List<Vector3> positionsPerBone, List<Vector3> velocitiesPerBone)
    {
        worldToLocal = _visualEffect.transform.worldToLocalMatrix;
        
        int count = positionsPerBone.Count;

        for (var i = 0; i < count; i++)
        {
            var pos = positionsPerBone[i];
            var vel = velocitiesPerBone[i];

            if (finalPositions.Count < COLCOUNT)
            {
                finalPositions.Add(worldToLocal.MultiplyPoint3x4(pos));
                finalVelocities.Add(worldToLocal.MultiplyVector(vel));
            }
            else
            {
                break;
            }
        }
    }

    // At the end of the frame, generate the texture to pass to the VFX graph
    private void ApplyAllCollisions()
    {
        while(finalPositions.Count < COLCOUNT)
        {
            finalPositions.Add(COLPOS);
            finalVelocities.Add(COLVEL);
        }
        radius = COLRADIUS;
        
        GenerateTexture(finalPositions,finalVelocities);

        Explode(position, velocity, radius);
        
        finalPositions.Clear();
        finalVelocities.Clear();
    }
    
}
