using UnityEngine;

public class SlimeVisual : MonoBehaviour
{
    [Header("Fill")]
    public Renderer bodyRenderer; // the cylinder's renderer, using the GooClip shader/material
    public float fullHeight = 2f; // world-space height representing 100% goo

    [Header("Surface")]
    public Transform surface; // invisible plane; its position/tilt double as the clip plane
    public float tiltAmount = 15f; // max degrees the surface tips from input
    public float springStrength = 60f; // how strongly it's pulled toward the target tilt
    public float damping = 4f; // how quickly the wobble settles; lower = more overshoot/wobblier

    [Header("Idle Wobble")]
    public float wobbleAmount = 3f; // degrees of constant ambient sloshing, even when still
    public float wobbleSpeed = 1.2f;

    [Header("Surface Sizing")]
    public float radiusMargin = 1.05f; // extra safety margin beyond the exact ellipse coverage

    private GameStats gameStats;
    private float surfaceBaseY;
    private Vector2 surfaceBaseRadiusScale; // the disc's XZ scale at zero tilt, sized to match the cylinder's radius exactly
    private Material bodyMaterial;
    private Renderer surfaceRenderer;
    private float wobbleSeedX;
    private float wobbleSeedZ;
    private float currentTiltX;
    private float currentTiltZ;
    private float tiltVelocityX;
    private float tiltVelocityZ;
    private static readonly int ClipPlanePositionID = Shader.PropertyToID("_ClipPlanePosition");
    private static readonly int ClipPlaneNormalID = Shader.PropertyToID("_ClipPlaneNormal");

    void Start()
    {
        gameStats = GameObject.FindWithTag("GameStats").GetComponent<GameStats>();
        surfaceBaseY = surface.localPosition.y;
        surfaceBaseRadiusScale = new Vector2(surface.localScale.x, surface.localScale.z);
        bodyMaterial = bodyRenderer.material;
        surfaceRenderer = surface.GetComponent<Renderer>();
        wobbleSeedX = Random.Range(0f, 1000f);
        wobbleSeedZ = Random.Range(0f, 1000f);
    }

    void Update()
    {
        float fillT = Mathf.Clamp01((float)gameStats.goo / gameStats.maxGoo);

        bool hasGoo = gameStats.goo > 0;
        bodyRenderer.enabled = hasGoo;
        surfaceRenderer.enabled = hasGoo;

        Vector3 surfacePos = surface.localPosition;
        surfacePos.y = surfaceBaseY + fullHeight * fillT;
        surface.localPosition = surfacePos;

        bool right = Input.GetKey(KeyCode.D);
        bool left = Input.GetKey(KeyCode.A);
        bool up = Input.GetKey(KeyCode.W);
        bool down = Input.GetKey(KeyCode.S);

        float targetTiltZ = 0f;
        if (right && !left) targetTiltZ = -tiltAmount; // slosh lags opposite the turn, like inertia
        else if (left && !right) targetTiltZ = tiltAmount;

        float targetTiltX = 0f;
        if (up && !down) targetTiltX = tiltAmount;
        else if (down && !up) targetTiltX = -tiltAmount;

        // constant ambient slosh so it never sits perfectly still, like real liquid
        targetTiltX += (Mathf.PerlinNoise(Time.time * wobbleSpeed, wobbleSeedX) - 0.5f) * 2f * wobbleAmount;
        targetTiltZ += (Mathf.PerlinNoise(Time.time * wobbleSpeed, wobbleSeedZ) - 0.5f) * 2f * wobbleAmount;

        // spring-damper toward the target so it overshoots and oscillates instead of gliding smoothly
        tiltVelocityX += (targetTiltX - currentTiltX) * springStrength * Time.deltaTime;
        tiltVelocityX *= Mathf.Clamp01(1f - damping * Time.deltaTime);
        currentTiltX += tiltVelocityX * Time.deltaTime;

        tiltVelocityZ += (targetTiltZ - currentTiltZ) * springStrength * Time.deltaTime;
        tiltVelocityZ *= Mathf.Clamp01(1f - damping * Time.deltaTime);
        currentTiltZ += tiltVelocityZ * Time.deltaTime;

        surface.localRotation = Quaternion.Euler(currentTiltX, surface.localEulerAngles.y, currentTiltZ);

        // grow the disc just enough to cover the current tilted (elliptical) cut, shrink back when flat
        float tiltAngle = Vector3.Angle(surface.up, Vector3.up);
        float radiusScale = radiusMargin / Mathf.Cos(Mathf.Clamp(tiltAngle, 0f, 80f) * Mathf.Deg2Rad);
        Vector3 surfaceScale = surface.localScale;
        surfaceScale.x = surfaceBaseRadiusScale.x * radiusScale;
        surfaceScale.z = surfaceBaseRadiusScale.y * radiusScale;
        surface.localScale = surfaceScale;

        bodyMaterial.SetVector(ClipPlanePositionID, surface.position);
        bodyMaterial.SetVector(ClipPlaneNormalID, surface.up);
    }
}
