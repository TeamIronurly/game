using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : MonoBehaviour
{
    public GameObject Character;
    Rigidbody rb;

    public float speed;
    private float k;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        k = Input.GetAxisRaw("Horizontal");
    }

    void FixedUpdate()
    {
        if (Math.Abs(k) == 1)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, new Vector3(k * speed, 0f, 0f), 0.1f);
        }
    }
}
