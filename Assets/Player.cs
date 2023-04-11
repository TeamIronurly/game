using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : MonoBehaviour
{
    Rigidbody rb;

    public float speed;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float k = Input.GetAxisRaw("Horizontal");
        if (k != 0)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, new Vector3(k * speed, 0f, 0f), 0.1f);
        }
    }
}
