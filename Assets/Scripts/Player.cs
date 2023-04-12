using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : MonoBehaviour
{
    Rigidbody rb;
    [Header("Movement")]
    public float speed;
    public Transform GroundChecker;
    public LayerMask groundMask;
    public float jumpBoost;
    [Range(1, 3)]
    public int Jumps;
    [SerializeField] private int jumpCnt; //to check jumps (Like a debug.log)

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // TODO: Tune for better comfort
        Vector3 v = rb.velocity;

        if (Physics.CheckSphere(GroundChecker.position, 0.1f, groundMask)) // Plr is touching ground => jumps resets
        {
            jumpCnt = Jumps;
        }

        float k = Input.GetAxisRaw("Horizontal");
        if (k != 0)
        {
            v.x += (k * speed - v.x) * 0.1f;
        }
        else
        {
            v.x *= 0.95f;// Horizontal drag
        }

        if (jumpCnt > 0 && Input.GetButtonDown("Jump")) // sometime uses two jumps in one press (i think cause of fixedUpdate),dont kno how to fix it correctly
        {
            v.y = jumpBoost;
            jumpCnt--;
        }

        v -= Vector3.up * 9.81f * 0.02f; // Gravity
        rb.velocity = v; // Apply recalculated velocity
    }
}
