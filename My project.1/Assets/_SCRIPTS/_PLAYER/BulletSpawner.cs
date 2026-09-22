using System;
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    public GameObject bullet;
    public GameObject player;
    Boolean canFire = true;
    public float cooldown =1f;
    
    public AudioSource sound_fire;


    void Start()
    {
        sound_fire = GetComponent<AudioSource>();
    }



    void Update()
    {
        if (canFire)
        {
            if (GameInput.Fire) //space, left mouse, or the on-screen fire button
            {
                Instantiate(bullet, transform.position, transform.rotation);
                sound_fire.Play();
                canFire = false;
                cooldown =1f;
            }
        }
        else if (!canFire)
        {
            if (cooldown <= 0)
            {
                canFire = true;
            }
            cooldown -= Time.deltaTime;
        }
    }

}
