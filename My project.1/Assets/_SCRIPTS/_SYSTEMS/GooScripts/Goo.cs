using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Goo : MonoBehaviour
{
    public int gooValue = 1;
    public float tossForce = 4f;
    public float fallMultiplier = 1.5f; // extra gravity on top of the normal pull
    public float maxFallSpeed = 6f; // clamp so it settles into a steady fall instead of accelerating forever
    public float spinSpeed = 5f; // random tumble on spawn

    [Header("Player Attraction")]
    public float attractRadius = 2f;
    public float attractSpeed = 8f;

    public AudioClip collectSound1;
    public AudioClip collectSound2;
    public AudioClip collectSound3;

    private GameStats gameStats;
    private Rigidbody rb;
    private Transform player;

    void Start()
    {
        gameStats = GameObject.FindWithTag("GameStats").GetComponent<GameStats>();
        player = GameObject.FindWithTag("Player").transform;
        rb = GetComponent<Rigidbody>();

        float angle = Random.Range(20f, 160f) * Mathf.Deg2Rad; // mostly upward, random lean left/right
        Vector3 tossDirection = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
        rb.linearVelocity = tossDirection * tossForce;
        rb.angularVelocity = Random.insideUnitSphere * spinSpeed;
    }

    void FixedUpdate()
    {
        if (player != null) // the player object is destroyed on death, so stop chasing it once that happens
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= attractRadius)
            {
                Vector3 direction = (player.position - transform.position).normalized;
                rb.linearVelocity = direction * attractSpeed;
                return;
            }
        }

        rb.AddForce(Physics.gravity * (fallMultiplier - 1f), ForceMode.Acceleration);

        if (rb.linearVelocity.y < -maxFallSpeed)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, -maxFallSpeed, rb.linearVelocity.z);
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Destroy")
        {
            Destroy(this.gameObject);
        }
        if (col.gameObject.tag == "Player")
        {
            gameStats.CollectGoo(gooValue);
            AudioClip[] collectSounds = { collectSound1, collectSound2, collectSound3 };
            AudioClip randomCollectSound = collectSounds[Random.Range(0, collectSounds.Length)];
            AudioSource.PlayClipAtPoint(randomCollectSound, transform.position, 1.0f);
            Destroy(this.gameObject);
        }
    }
}
