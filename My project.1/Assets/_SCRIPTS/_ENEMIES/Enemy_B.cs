using System.Collections;
using UnityEngine;

public class Enemy_B : MonoBehaviour
{
    private GameStats gameStats;
    public GameObject projectile;
    public GameObject floatingPoints;
    public AudioClip death;
    public AudioClip shoot;
    public GameObject Explosion;
    public ParticleSystem LaserCharge;
    public ParticleSystem LaserFire;
    private Transform player;
    private int points = 80;
    private float speed = .5f;
    private float duration = 2f;
    private float timeElapsed = 0f;
    private bool moveRight = true;
    public string explosionType = "EnemyA";
    private bool canFire = true;

    void Start()
    {
        Instantiate(Explosion, transform.position, Quaternion.identity).GetComponent<Explosion_Effect>().explosionType = explosionType;
        gameStats = GameObject.FindWithTag("GameStats").GetComponent<GameStats>();
        player = GameObject.FindWithTag("Player").transform;
        timeElapsed += duration / 2;
    }

    void Update()
    {
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
        yield return new WaitForSeconds(1.3f);

        Vector3 targetPosition = player.position; // snapshot taken once, before any shots


        LaserFire.Play();

        for (int i = 0; i < 5; i++)
        {
            AudioSource.PlayClipAtPoint(shoot, transform.position, 1.0f);
            Instantiate(projectile, transform.position, transform.rotation).GetComponent<Projectile_B>().target = targetPosition;

            if (i < 4) yield return new WaitForSeconds(.1f);//seconds between shots
        }

        canFire = true;
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
