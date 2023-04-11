using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : MonoBehaviour
{
    Rigidbody rb;

    public float speed;
    public float jumpBoost;
    [Range(0, 3)]
    public int airJump = 0;
    private bool jump = false;
    private int jumpCnt;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update() {
        jump = jump || Input.GetButtonDown("Jump");
    }

    void FixedUpdate()
    {
        // TODO: Tune for better comfort
        // TODO: Better ground detection
        if (rb.velocity.y == 0) // Plr is not jumping or falling -> Recharge jumps
        {
            jumpCnt = airJump + 1;
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
            if (v.y == 0 || jumpCnt > 0)
            {
                v.y += jumpBoost;
            }
            jumpCnt--;
        }

        // Gravity
        v -= Vector3.up * 9.81f * 0.02f;

        // Apply recalculated velocity
        rb.velocity = v;
    }
}
