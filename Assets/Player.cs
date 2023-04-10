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
        rb.AddForce(new Vector3(k * speed, 0f, 0f), ForceMode.Impulse);
    }



    // Смерть при соприкосновении с DIe коллайдером
    //private void OnTriggerEnter(Collider collision)
    //{
    //    if (collision.CompareTag("Die"))
    //    {

    //        Dead1 = true;
    //        Destroy(gameObject);

    //    }

    //}
}
