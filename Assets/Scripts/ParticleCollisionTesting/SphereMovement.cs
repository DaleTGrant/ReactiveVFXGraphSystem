using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereMovement : MonoBehaviour
{

    [SerializeField] private Vector3 startPosition;
    [SerializeField] private Vector3 newPosition;

    public float distanceOffset = 1.0f;
    public float frequency = 1.0f;
    public Vector3 movementDirection;

    private void Awake()
    {
        startPosition = transform.position;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        newPosition = startPosition + distanceOffset * Mathf.Sin(Time.time*frequency) * movementDirection;
        transform.position = newPosition;

    }
}
