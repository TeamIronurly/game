using UnityEngine;
using System;


public class Player : MonoBehaviour
{
    Rigidbody rb;

    [Header("Movement")]
    public float speed;
    public LayerMask groundMask;

    [Range(1, 3)]
    public int Jumps;
    public float jumpHeight; // How high can the player jump in units
    private int jumpCnt;
    private bool jumpTrigger = false;
    private float g = 9.81f;

    [Header("Mobile Input")]
    public TouchInput leftButton;
    public TouchInput rightButton;
    public TouchInput jumpButton;


    void Start()
    {
        rb = GetComponent<Rigidbody>();


    }

    void Update()
    {
        jumpTrigger = jumpTrigger || Input.GetButtonDown("Jump") || jumpButton.pressed;
        jumpButton.pressed = false;
    }

    void FixedUpdate()
    {
        Vector3 v = rb.velocity;

        if (Physics.CheckSphere(transform.position - Vector3.up, 0.1f, groundMask)) // Plr is touching ground => jumps resets
        {
            jumpCnt = Jumps;
        }

        float k = Input.GetAxisRaw("Horizontal") + (leftButton.pressed ? -1 : 0)+ (rightButton.pressed ? 1 : 0);
        if (k != 0)
        {
            v.x += (k * speed - v.x) * 0.1f;
        }
        else
        {
            v.x *= 0.95f; // Horizontal drag
        }

        if (jumpTrigger && jumpCnt > 0)
        {
            v.y = Mathf.Sqrt(2 * g * jumpHeight);
            jumpCnt--;
        }

        v -= Vector3.up * g * 0.02f; // Gravity
        rb.velocity = v; // Apply recalculated velocity

        jumpTrigger = false;
    }
}
