using System.Collections;
using UnityEngine;

// Enemy B: the heavy gunner. It drifts slowly around its slot, and each time it turns around it
// starts charging - an energy sphere swells up for two seconds - then fires a stream of five shots
// at the spot the player was standing on when the charge finished. The shots fly to that spot
// rather than following the player, so moving once it fires dodges the whole stream. One hit kills it.
public class Enemy_B : MonoBehaviour
{
    private GameStats gameStats;
    public GameObject projectile;
    public GameObject floatingPoints; //the score number that pops up where it died
    public AudioClip death;
    public AudioClip shoot;
    public AudioClip shootCharge;     //played as the charge starts
    public AudioClip shootChargeFull; //played the moment it's fully charged, right before the stream
    public GameObject Explosion;
    public GameObject gooPrefab;
    public GameObject energySphere;   //the ball that swells up while it charges
    public ParticleSystem LaserCharge; //plays while it charges
    public ParticleSystem LaserFire;   //plays as the stream fires
    private Transform player;         //looked up once, so it can aim at the player when the charge finishes
    private int points = 80;
    private float speed = .5f;    // slow, same as A
    private float duration = 2f;  // seconds per swing before it turns around, so it drifts about 0.5 either side of its slot
    private float timeElapsed = 0f;
    private bool moveRight = true;
    public string explosionType = "EnemyB"; //which shrapnel effect Explosion_Effect plays when it dies
    private bool canFire = true; //false from the start of a charge until the stream is done, so it never starts a second one on top

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
        energySphere.SetActive(false); //hidden until it starts charging
        timeElapsed += duration / 2; //half a swing's head start, so it drifts evenly either side of its slot instead of off to one side

        //It spawns in its slot, then gets moved up above the screen so it can fly down into it
        spawnPosition = transform.position;
        entryStartPosition = spawnPosition + Vector3.up * entryHeight + Vector3.right * (spawnPosition.x * entryAngleFactor);
        transform.position = entryStartPosition;
        entryDuration = Mathf.Max(0.2f, baseEntryDuration + entryDurationPerY * spawnPosition.y + Random.Range(-entryDurationVariance, entryDurationVariance));
    }

    void Update()
    {
        if (isEntering) //still flying in - it doesn't move or charge until it has landed
        {
            entryElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(entryElapsed / entryDuration);
            float eased = t * t * (3f - 2f * t); // smoothstep: eases in, then decelerates into a smooth landing
            transform.position = Vector3.Lerp(entryStartPosition, spawnPosition, eased);
            if (t >= 1f) isEntering = false;
            return;
        }

        LaserCharge.transform.Rotate(0,0,30*Time.deltaTime,Space.Self); //keeps the charge effect slowly spinning

        if (!gameStats.playerAlive) return; //everything freezes when the player dies, like the other enemies

        //Drift: slide one way until the swing time is up, then turn around
        timeElapsed += Time.deltaTime;
        transform.Translate((moveRight ? speed : -speed) * Time.deltaTime, 0, 0);

        if (timeElapsed >= duration)
        {
            moveRight = !moveRight;
            timeElapsed = 0f;
            if (canFire) StartCoroutine(FireRoutine()); //every turn starts a charge, unless one is still going
        }
    }

    //Charges up, then fires five shots in quick succession at where the player was when the charge finished.
    //It keeps drifting the whole time, so the stream leaves from wherever it has moved to
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

    //Swells the energy sphere up to 15x its size over two seconds, then hides it and shrinks it back for next time
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





    //One hit from the player kills it, even mid-charge: score popup, explosion, points, and a goo drop
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
