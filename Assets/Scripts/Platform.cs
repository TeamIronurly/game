using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform: MonoBehaviour
{
    public GameObject spot;
    public GameObject Plat;
    private float x;
    Vector3 whereSpawn;
    public float Delay;
    private float NextSpawn=0.0f;

    void Update()
    {
       if (Time.time>NextSpawn)
       {
        NextSpawn = Time.time+ Delay;
        x = Random.Range(-10,10);
        whereSpawn = new Vector3 (x, transform.position.y, transform.position.z);
         Plat = Instantiate (spot,whereSpawn,Quaternion.identity);
         
       }
    }
private void OnTriggerEnter(Collider collision)
    {
        if (collision.tag == "Lava") 
        {
            Destroy(Plat);
        }      
    }
}
