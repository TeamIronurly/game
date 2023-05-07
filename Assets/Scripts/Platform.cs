using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public GameObject platform;
    private float pos_x;
    public float Delay;
    private float NextSpawn = 0f;

    void Start(){
            Random.seed = 123;
    }

    void Update()
    {
        if (Time.time > NextSpawn)
        {
            NextSpawn = Time.time + Delay;
            pos_x = Random.Range(-15, -1);

            Vector3 left = new (pos_x, transform.position.y, transform.position.z);
            Vector3 right = new (-pos_x, transform.position.y, transform.position.z);

            Instantiate(platform, left, Quaternion.identity);
            Instantiate(platform, right, Quaternion.identity);
        }
    }
    
}
