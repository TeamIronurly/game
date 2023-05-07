using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public GameObject platform;
    private float pos_x;
    public float Delay;
    private float NextSpawn = 0f;

    void Start()
    {
        // temporary fix for platforms not syncronized between players
        //
        // TODO:
        // Send random seed to other players over network instead of keeping it always the same
        // Отправлять random seed другим игрокам по сети вмето того чтобы оставлять его всегда одинаковым
        //
        // Or send all platform positions over network, this will use more bandwidth, but
        // platform positions will be the same for all players.
        // Otherwise, if someone has slow internet their platforms will move to different places comapred to other players
        // Или отправлять положение всех платформ по сети, так будет использоваться больше трафика, но
        // положение платформ будет точно одинаковое у всех игроков.
        // Иначе, если у кого-то медленный инет, их платформы могут сдвинуться в другие места по сравнению с другими игроками

        Random.seed = 123;
    }

    void Update()
    {
        if (Time.time > NextSpawn)
        {
            NextSpawn = Time.time + Delay;
            pos_x = Random.Range(-15, -1);

            Vector3 left = new(pos_x, transform.position.y, transform.position.z);
            Vector3 right = new(-pos_x, transform.position.y, transform.position.z);

            Instantiate(platform, left, Quaternion.identity);
            Instantiate(platform, right, Quaternion.identity);
        }
    }

}
