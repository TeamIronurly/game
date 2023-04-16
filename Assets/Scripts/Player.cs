using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;


public class Player : MonoBehaviour
{
    
    Rigidbody rb;

    [Header("Movement")]
    public float speed;
    public LayerMask groundMask;

    [Range(1, 3)]
    public int Jumps;
    public float jumpHeight;
    private int jumpCnt;
    private bool jumpTrigger = false;
    private float g = 9.81f;
    
    void Start()
    {
        Time.timeScale = 1f;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        jumpTrigger = jumpTrigger || Input.GetButtonDown("Jump");
    }

    void FixedUpdate()
    {
        Vector3 v = rb.velocity;

        if (Physics.CheckSphere(transform.position - Vector3.up, 0.1f, groundMask)) // Plr is touching ground => jumps resets
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
    private void OnTriggerExit(Collider collision) // if touches "Lava" = dies
    {
        if (collision.CompareTag("Lava"))
        {
            Time.timeScale = 0.3f;
            Invoke("Dead", 1f); // Dont kno how to make it more optimized...
        }

    }
    private void Dead()
    {
        Destroy(gameObject);
        SceneManager.LoadScene("GameOver");
    }
}
