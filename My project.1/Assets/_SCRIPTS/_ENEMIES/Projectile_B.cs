using UnityEngine;
using System;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine.UIElements;


public class Projectile_B : MonoBehaviour
{

    private float projSpeed = 7; //speed of the bullet
    public Vector3 target;
    void Start() //when the bullet is spawned point at player
    {

        transform.LookAt(target);
        transform.SetParent(null);

    }
    void Update() //go forward forever till hit
    {

        transform.Translate(transform.forward * projSpeed * Time.deltaTime, Space.World);
        transform.Rotate(transform.forward, Space.World);
    }
    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Destroy")
        {
            Destroy(this.gameObject);
        }
        if (col.gameObject.tag == "Player")
        {
            Destroy(this.gameObject);
        }
    }

}