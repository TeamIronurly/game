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
    [Range(0, 3)]
    public int airJump = 0;

    public bool isGrounded;
    private bool jump = false;
    private int jumpCnt;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        jump = jump || Input.GetButtonDown("Jump");
    }

    void FixedUpdate()
    {
        // TODO: Tune for better comfort

        isGrounded = Physics.CheckSphere(GroundChecker.position, 0.2f, groundMask);

        if (isGrounded) // Plr is touching ground => jumps resets
        {
            jumpCnt = airJump;
        }

        Vector3 v = rb.velocity;

        float k = Input.GetAxisRaw("Horizontal");
        if (k != 0)
        {
            v.x += (k * speed - v.x) * 0.1f;
        }
        else
        {
            // Horizontal drag
            v.x *= 0.9f;
        }

        if (jump)
        {
            jump = false;
            if (isGrounded || jumpCnt > 0)
            {
                v.y = jumpBoost;
            }
            jumpCnt--;
        }

        // Gravity
        v -= Vector3.up * 9.81f * 0.02f;

        // Apply recalculated velocity
        rb.velocity = v;
    }
}
