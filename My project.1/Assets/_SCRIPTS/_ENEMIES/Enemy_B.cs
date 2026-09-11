using System.Collections;
using UnityEngine;

public class Enemy_B : MonoBehaviour
{
    private GameStats gameStats;
    public GameObject projectile;
    public GameObject floatingPoints;
    public AudioClip death;
    public AudioClip shoot;
    public AudioClip shootCharge;
    public AudioClip shootChargeFull;
    public GameObject Explosion;
    public GameObject gooPrefab;
    public GameObject energySphere;
    public ParticleSystem LaserCharge;
    public ParticleSystem LaserFire;
    private Transform player;
    private int points = 80;
    private float speed = .5f;
    private float duration = 2f;
    private float timeElapsed = 0f;
    private bool moveRight = true;
    public string explosionType = "EnemyB";
    private bool canFire = true;

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
        player = GameObject.FindWithTag("Player").transform;
        energySphere.SetActive(false);
        timeElapsed += duration / 2;

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

        LaserCharge.transform.Rotate(0,0,30*Time.deltaTime,Space.Self);

        if (!gameStats.playerAlive) return;

        timeElapsed += Time.deltaTime;
        transform.Translate((moveRight ? speed : -speed) * Time.deltaTime, 0, 0);

        if (timeElapsed >= duration)
        {
            moveRight = !moveRight;
            timeElapsed = 0f;
            if (canFire) StartCoroutine(FireRoutine());
        }
    }

    IEnumerator FireRoutine()
    {
        canFire = false;
        LaserCharge.Play();
        yield return StartCoroutine(ChargeUpSphere());
       // yield return new WaitForSeconds(1.3f);

        if (player == null) // the player object is destroyed on death, so cancel the shot instead of firing at nothing
        {
            canFire = true;
            yield break;
        }

        Vector3 targetPosition = player.position; // snapshot taken once, before any shots for one stream

        LaserFire.Play();

        for (int i = 0; i < 5; i++)
        {

            AudioSource.PlayClipAtPoint(shoot, transform.position, 1.0f);
            Instantiate(projectile, transform.position + Vector3.down * 0.3f, transform.rotation).GetComponent<Projectile_B>().target = targetPosition;

            if (i < 4) yield return new WaitForSeconds(.1f);//seconds between shots
        }

        canFire = true;
    }

    IEnumerator ChargeUpSphere()
    {
        AudioSource.PlayClipAtPoint(shootCharge,transform.position, 1.0f);
        Vector3 startScale = energySphere.transform.localScale;
        Vector3 maxScale = startScale * 15f;
        float chargeDuration = 2f;
        float t = 0f;

        energySphere.SetActive(true);

        while (t < chargeDuration)
        {
            t += Time.deltaTime;
            energySphere.transform.localScale = Vector3.Lerp(startScale, maxScale, t / chargeDuration);
            yield return null;
        }
        AudioSource.PlayClipAtPoint(shootChargeFull, transform.position, 1.0f);
        energySphere.SetActive(false);
        energySphere.transform.localScale = startScale;
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
