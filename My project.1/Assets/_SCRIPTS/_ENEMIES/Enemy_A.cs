using UnityEngine;

public class Enemy_A : MonoBehaviour
{
    private GameStats gameStats;
    public GameObject projectile;
    public GameObject floatingPoints;
    public AudioClip death;
    public AudioClip shoot;
    public GameObject Explosion;
    public GameObject gooPrefab;

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

    [Header("Entrance")]
    public float entryHeight = 6f; // how far above its spawn slot it flies in from
    public float entryAngleFactor = 0.6f; // sideways offset per unit of the slot's x, so it glides in at an angle
    public float baseEntryDuration = 1f; // how long the fly-in takes
    public float entryDurationPerY = 0.08f; // longer for higher spawn slots, shorter for lower ones
    public float entryDurationVariance = 0.15f; // random +/- duration so the formation doesn't fly in perfectly uniform
    private Vector3 spawnPosition;
    private Vector3 entryStartPosition;
    private float entryDuration;
    private float entryElapsed = 0f;
    private bool isEntering = true;

    void Start()
    {
        gameStats = GameObject.FindWithTag("GameStats").GetComponent<GameStats>();
        timeElapsed += duration / 2;

        nextFireTime = Random.Range(minFireInterval, maxFireInterval);

        spawnPosition = transform.position;
        entryStartPosition = spawnPosition + Vector3.up * entryHeight + Vector3.right * (spawnPosition.x * entryAngleFactor);
        transform.position = entryStartPosition;
        entryDuration = Mathf.Max(0.2f, baseEntryDuration + entryDurationPerY * spawnPosition.y + Random.Range(-entryDurationVariance, entryDurationVariance));
    }

    void Update()
    {
        if (isEntering)
        {
            entryElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(entryElapsed / entryDuration);
            float eased = t * t * (3f - 2f * t); // smoothstep: eases in, then decelerates into a smooth landing
            transform.position = Vector3.Lerp(entryStartPosition, spawnPosition, eased);
            if (t >= 1f) isEntering = false;
            return;
        }

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
            GooSpawner.SpawnGoo(gooPrefab, transform.position, points);
            Destroy(this.gameObject);
        }
    }
}
