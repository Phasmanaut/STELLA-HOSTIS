using UnityEngine;

public class Enemy_A : MonoBehaviour
{
    private GameStats gameStats;
    public GameObject projectile;
    public GameObject floatingPoints;
    public AudioClip death;
    public AudioClip shoot;
    public GameObject Explosion;

    private int points = 50;
    private float speed = 0.5f;
    private float duration = 3f;
    private float timeElapsed = 0f;
    private bool moveRight = true;
    public string explosionType = "EnemyA";

    [Header("Firing")]
    public float minFireInterval = 1.5f; // shortest possible time between shots
    public float maxFireInterval = 4f;   // longest possible time between shots
    private float fireTimer = 0f;
    private float nextFireTime;

    void Start()
    {
        Instantiate(Explosion, transform.position, Quaternion.identity).GetComponent<Explosion_Effect>().explosionType = explosionType;
        gameStats = GameObject.FindWithTag("GameStats").GetComponent<GameStats>();
        timeElapsed += duration / 2;

        nextFireTime = Random.Range(minFireInterval, maxFireInterval);
    }

    void Update()
    {
        if (gameStats.playerAlive)
        {
            timeElapsed += Time.deltaTime;
            fireTimer += Time.deltaTime;

            if (moveRight)
            {
                transform.Translate(speed * Time.deltaTime, 0, 0);
            }
            else
            {
                transform.Translate(-speed * Time.deltaTime, 0, 0);
            }

            if (timeElapsed >= duration)
            {
                moveRight = !moveRight;
                timeElapsed = 0f;
            }

            if (fireTimer >= nextFireTime)
            {
                fireTimer = 0f;
                nextFireTime = Random.Range(minFireInterval, maxFireInterval); // rolls a new random interval
                Instantiate(projectile, this.transform);
                AudioSource.PlayClipAtPoint(shoot, transform.position, 1.0f);
            }
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player Projectile")
        {
            Instantiate(floatingPoints, transform.position, Quaternion.identity).GetComponent<FloatingPoints>().pointWorth = points;
            Instantiate(Explosion, transform.position, Quaternion.identity).GetComponent<Explosion_Effect>().explosionType = explosionType;
            AudioSource.PlayClipAtPoint(death, transform.position, 1.0f);

            gameStats.EnemyDown(points);
            Destroy(this.gameObject);
        }
    }
}
