using UnityEngine;

public class Projectile_A : MonoBehaviour
{

    public float projSpeed; //speed of the bullet
    public float spreadAngle = 10f; //how far off the aim it can land, in degrees. Whoever fires it can widen this for a sloppier shot

    void Start() //when the bullet is spawned point at player
    {
        GameObject player = GameObject.FindWithTag("Player");
        transform.SetParent(null);

        if (player == null) //the player died in the gap between this being fired and Start running, so there's nothing to aim at
        {
            Destroy(this.gameObject);
            return;
        }

        transform.LookAt(player.transform);
        transform.Rotate(UnityEngine.Random.Range(-spreadAngle, spreadAngle), 0, 0);// adds a bit of randomness to aim
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
