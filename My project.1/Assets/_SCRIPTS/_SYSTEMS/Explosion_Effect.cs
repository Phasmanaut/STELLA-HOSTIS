using UnityEngine;

public class Explosion_Effect : MonoBehaviour
{
    public ParticleSystem ShrapnelA;
    public ParticleSystem Fire;
    public ParticleSystem ShrapnelPlayer;
    private float timer = 0;
    public string explosionType;

    void Start()
    {
        ShrapnelA = transform.Find("ShrapnelA").GetComponent<ParticleSystem>();
        Fire = transform.Find("Fire").GetComponent<ParticleSystem>();
        ShrapnelPlayer = transform.Find("ShrapnelPlayer").GetComponent<ParticleSystem>();

        TriggerExplosion();
    }

    public void TriggerExplosion()
    {
        if (explosionType.ToLower() == "player")
        {
            ShrapnelPlayer.Play();
            Fire.Play();
        }
        if (explosionType.ToLower() == "enemya")
        {
            ShrapnelA.Play();
            Fire.Play();
        }
        if (explosionType.ToLower() == "enemyab")
        {
            ShrapnelA.Play();
            Fire.Play();
        }
        if (explosionType.ToLower() == "enemyb")
        {
            ShrapnelA.Play();
            Fire.Play();
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > 1.5f)
        {
            Destroy(this.gameObject);
        }
    }
}
