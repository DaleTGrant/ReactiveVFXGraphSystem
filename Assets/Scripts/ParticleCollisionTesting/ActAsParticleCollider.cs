using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ActAsParticleCollider : MonoBehaviour
{

    public List<VisualEffect> visualEffects;

    private SphereCollider _collider;
    private Vector3 centre;
    private float radius;

    private void Awake()
    {
        _collider = GetComponent<SphereCollider>();
        UpdateSphere();

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSphere();
    }

    private void UpdateSphere()
    {
        if (_collider && visualEffects.Count>0)
        {
            if (!HasSphereChanged())
            {
                return;
            }
            centre = transform.position;
            radius = _collider.radius;
            
            foreach (var vfx in visualEffects)
            {
                vfx.SetVector3("Centre", centre - vfx.transform.position);
                vfx.SetFloat("Radius",radius);
            }
            
        }
    }

    private bool HasSphereChanged()
    {
        return !centre.Equals(transform.position) || !radius.Equals(_collider.radius);
    }
}
