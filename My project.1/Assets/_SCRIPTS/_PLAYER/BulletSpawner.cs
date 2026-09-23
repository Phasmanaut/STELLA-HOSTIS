using System;
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    public GameObject bullet;
    public GameObject player;
    Boolean canFire = true;
    public float fireCooldown = 0.67f; //seconds between shots
    private float cooldown; //counts down after each shot

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
                cooldown = fireCooldown;
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
