using UnityEngine;

// Enemy A: the basic grunt. It sways slowly from side to side around its formation slot and every
// so often takes a single, slightly inaccurate shot at the player. One hit kills it.
public class Enemy_A : MonoBehaviour
{
    private GameStats gameStats;
    public GameObject projectile;
    public GameObject floatingPoints; //the score number that pops up where it died
    public AudioClip death;
    public AudioClip shoot;
    public GameObject Explosion;
    public GameObject gooPrefab;

    private int points = 50;
    private float speed = 0.5f;    // how fast it sways
    private float duration = 3f;   // seconds per swing before it turns around, so it sways about 0.75 either side of its slot
    private float timeElapsed = 0f;
    private bool moveRight = true;
    public string explosionType = "EnemyA"; //which shrapnel effect Explosion_Effect plays when it dies

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
        timeElapsed += duration / 2; //half a swing's head start, so it sways evenly either side of its slot instead of off to one side

        nextFireTime = Random.Range(minFireInterval, maxFireInterval);

        //It spawns in its slot, then gets moved up above the screen so it can fly down into it
        spawnPosition = transform.position;
        entryStartPosition = spawnPosition + Vector3.up * entryHeight + Vector3.right * (spawnPosition.x * entryAngleFactor);
        transform.position = entryStartPosition;
        entryDuration = Mathf.Max(0.2f, baseEntryDuration + entryDurationPerY * spawnPosition.y + Random.Range(-entryDurationVariance, entryDurationVariance));
    }

    void Update()
    {
        if (isEntering) //still flying in - it doesn't sway or shoot until it has landed
        {
            entryElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(entryElapsed / entryDuration);
            float eased = t * t * (3f - 2f * t); // smoothstep: eases in, then decelerates into a smooth landing
            transform.position = Vector3.Lerp(entryStartPosition, spawnPosition, eased);
            if (t >= 1f) isEntering = false;
            return;
        }

        if (gameStats.playerAlive) //everything freezes when the player dies
        {
            timeElapsed += Time.deltaTime;
            fireTimer += Time.deltaTime;

            //Sway: slide one way until the swing time is up, then turn around
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
                Instantiate(projectile, this.transform); //spawned as a child so it starts right here - Projectile_A unparents itself and aims at the player
                AudioSource.PlayClipAtPoint(shoot, transform.position, 1.0f);
            }
        }
    }

    //One hit from the player kills it: score popup, explosion, points, and a goo drop
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
