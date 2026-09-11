using UnityEngine;

public class Projectile_A : MonoBehaviour
{

    public float projSpeed; //speed of the bullet
    
    void Start() //when the bullet is spawned point at player
    {
        GameObject player = GameObject.FindWithTag("Player");
        transform.LookAt(player.transform);
        transform.SetParent(null);
        transform.Rotate(UnityEngine.Random.Range(-10f, 10f), 0, 0);// adds a bit of randomness to aim
    }
    void Update() //go forward forever till hit
    {
        transform.Translate(transform.forward * projSpeed * Time.deltaTime, Space.World);
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