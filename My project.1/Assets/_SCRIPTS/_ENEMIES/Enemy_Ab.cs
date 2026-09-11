using UnityEngine;
public class Enemy_Ab : MonoBehaviour
{

    private GameStats gameStats;public GameObject projectile;public GameObject floatingPoints;public AudioClip death;public AudioClip shoot;public GameObject Explosion;public GameObject gooPrefab;

    private int points = 75;//more points
    private float speed = 1.5f; // faster than Enemy_A's 0.5f
    private float duration = 1.5f; // shorter than Enemy_A's 3f = fires more often

    private float timeElapsed = 0f;
    private bool moveRight = true;
    public string explosionType = "EnemyAb";

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
        timeElapsed += duration / 2; //head start to keep enemies centered

        spawnPosition = transform.position;
        entryStartPosition = spawnPosition + Vector3.up * entryHeight + Vector3.right * (spawnPosition.x * entryAngleFactor);
        transform.position = entryStartPosition;
        entryDuration = Mathf.Max(0.2f, baseEntryDuration + entryDurationPerY * spawnPosition.y + Random.Range(-entryDurationVariance, entryDurationVariance));
    }
    void Update() //idle movemets
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

        if (gameStats.playerAlive) //stops all if player is dead
        {
            timeElapsed += Time.deltaTime;
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
                moveRight = !moveRight; // Swap direction
                timeElapsed = 0f; // Reset timer     
                Instantiate(projectile, this.transform);
                AudioSource.PlayClipAtPoint(shoot, transform.position, 1.0f);
            }
        }
    }
    void OnTriggerEnter(Collider col)  //killed on hit
    {
        if (col.gameObject.tag == "Player Projectile")
        {
            Instantiate(floatingPoints, transform.position, Quaternion.identity).GetComponent<FloatingPoints>().pointWorth = points; //spawn points graphic
            Instantiate(Explosion, transform.position, Quaternion.identity).GetComponent<Explosion_Effect>().explosionType = explosionType; //spawn Explosion
            AudioSource.PlayClipAtPoint(death, transform.position, 1.0f);
            gameStats.EnemyDown(points);// pass points to gamestats
            GooSpawner.SpawnGoo(gooPrefab, transform.position, points);
            Destroy(this.gameObject);
        }
    }
}
