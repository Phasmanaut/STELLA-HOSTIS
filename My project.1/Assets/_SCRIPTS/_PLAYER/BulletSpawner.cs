using System;
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    public GameObject bullet;
    public GameObject player;
    Boolean canFire = true;
    float inputSpace;
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
            inputSpace = Input.GetAxis("Fire1");
            if (inputSpace > 0)
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
