using UnityEngine;
using UnityEngine.VFX;

public class GunToggle : MonoBehaviour
{
    [Header("Fire Animation")]
    [Tooltip("Plays a 140-frame @ 60fps animation on fire. Firing is locked out until it finishes.")]
    [SerializeField] Animator gunAnimator;
    [SerializeField] string fireAnimationTrigger = "Fire";
    [SerializeField] float fireAnimationFrameCount = 140f;
    [SerializeField] float fireAnimationFrameRate = 60f;

    [Header("Muzzle Flash")]
    [SerializeField] ParticleSystem muzzleParticles;
    [Tooltip("Delay after firing before the muzzle flash particle effect plays.")]
    [SerializeField] float muzzleEffectDelay = 1f;
    [SerializeField] float muzzleParticleDuration = 1f;

    [Header("Muzzle Light")]
    [SerializeField] Light muzzleLight;
    [Tooltip("Intensity the light snaps to on fire.")]
    [SerializeField] float muzzleLightIntensity = 13f;
    [Tooltip("How long it takes the light to ease back down to 0 after firing - independent of the fire animation's lockout duration.")]
    [SerializeField] float muzzleLightFadeDuration = 0.15f;

    [Header("Muzzle VFX Graph")]
    [Tooltip("Instantiated fresh on every shot and left to play out on its own - shots fired in quick succession will simply stack multiple instances, no need to worry about matching its length to anything.")]
    [SerializeField] VisualEffect muzzleVFXPrefab;
    [SerializeField] float muzzleVFXLifetime = 2f;

    [Header("Bullet Tracer")]
    [Tooltip("Where the tracer visually starts from (the barrel tip).")]
    [SerializeField] Transform muzzlePoint;
    [Tooltip("Shader graph material used for the tracer line.")]
    [SerializeField] Material tracerMaterial;
    [SerializeField] float tracerLineWidth = 0.03f;
    [SerializeField] float tracerLineDuration = 0.05f;

    [Header("Hitscan Spread")]
    [Tooltip("Fires from the middle of this camera's view. Defaults to Camera.main if left empty.")]
    [SerializeField] Camera fireCamera;
    [SerializeField] int pelletCount = 12;
    [Tooltip("Max random spread angle (degrees) each pellet can deviate from dead-center.")]
    [SerializeField] float spreadAngle = 4f;
    [SerializeField] float hitscanRange = 200f;
    [SerializeField] LayerMask hitscanMask = ~0;
    [Tooltip("Damage dealt by each individual pellet (not divided across pellets). Enemies take this via IDamageable - see Enemies.cs.")]
    [SerializeField] float damagePerPellet = 8f;
    [Tooltip("Physics force each pellet applies to shatterable props (via IShotImpactReceiver - see ShatterOnShot.cs).")]
    [SerializeField] float shotImpactForce = 8f;

    [Header("Impact VFX")]
    [Tooltip("Played when a pellet hits a roughly horizontal surface (floor/ceiling).")]
    [SerializeField] VisualEffect groundImpactVFXPrefab;
    [Tooltip("Played when a pellet hits a roughly vertical surface (wall).")]
    [SerializeField] VisualEffect wallImpactVFXPrefab;
    [Tooltip("If the hit surface normal is within this many degrees of straight up/down, it's treated as ground; otherwise it's treated as a wall.")]
    [SerializeField] float groundNormalMaxAngle = 45f;
    [SerializeField] float impactVFXLifetime = 2f;
    [Tooltip("Hit colliders with any of these tags will NOT spawn an impact VFX (e.g. Player, Enemies, Bullet).")]
    [SerializeField] string[] impactExcludeTags = { "Player", "Enemies", "Bullet" };

    [Header("Recoil / Knockback")]
    [Tooltip("The player's movement script - recoil pushes it opposite the fireCamera's aim direction (shoot the ground to launch upward, shoot forward to bhop backward, etc).")]
    [SerializeField] PlayerMovementdiddy playerMovement;
    [SerializeField] float recoilForce = 6f;

    [Header("Debug")]
    [Tooltip("Draw a gizmo for every pellet's path from the last shot fired (Scene view only).")]
    [SerializeField] bool showSpreadGizmos = true;
    [SerializeField] Color spreadGizmoColor = Color.yellow;
    [SerializeField] Color spreadGizmoHitColor = Color.red;

    bool canFire = true;
    readonly System.Collections.Generic.List<(Vector3 start, Vector3 end, bool hit)> lastShotPellets = new System.Collections.Generic.List<(Vector3, Vector3, bool)>();

    void Update()
    {
        // Skip the very first frame - the click that starts Play (or focuses the
        // Game view) registers as GetMouseButtonDown(0) and would fire immediately.
        if (Time.frameCount <= 1) return;

        if (canFire && Input.GetMouseButtonDown(0))
        {
            Fire();
        }
    }

    void Fire()
    {
        canFire = false;

        if (gunAnimator != null) gunAnimator.SetTrigger(fireAnimationTrigger);
        if (muzzleLight != null) muzzleLight.intensity = muzzleLightIntensity;

        FireHitscan();
        SpawnMuzzleVFX();
        ApplyFireRecoil();
        StartCoroutine(PlayMuzzleFlashDelayed());
        StartCoroutine(FadeMuzzleLight());
        StartCoroutine(FireAnimationLock());
    }

    void ApplyFireRecoil()
    {
        if (playerMovement == null) return;

        Camera cam = fireCamera != null ? fireCamera : Camera.main;
        if (cam == null) return;

        playerMovement.ApplyRecoil(-cam.transform.forward * recoilForce);
    }

    void SpawnMuzzleVFX()
    {
        if (muzzleVFXPrefab == null) return;

        Transform origin = muzzlePoint != null ? muzzlePoint : transform;
        VisualEffect vfx = Instantiate(muzzleVFXPrefab, origin.position, origin.rotation);
        vfx.Play();
        Destroy(vfx.gameObject, muzzleVFXLifetime);
    }

    System.Collections.IEnumerator FadeMuzzleLight()
    {
        if (muzzleLight == null) yield break;

        float duration = Mathf.Max(0.0001f, muzzleLightFadeDuration);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            muzzleLight.intensity = Mathf.Lerp(muzzleLightIntensity, 0f, elapsed / duration);
            yield return null;
        }
        muzzleLight.intensity = 0f;
    }

    System.Collections.IEnumerator FireAnimationLock()
    {
        float duration = fireAnimationFrameRate > 0f ? fireAnimationFrameCount / fireAnimationFrameRate : 0f;
        yield return new WaitForSeconds(duration);
        canFire = true;
    }

    System.Collections.IEnumerator PlayMuzzleFlashDelayed()
    {
        yield return new WaitForSeconds(muzzleEffectDelay);

        if (muzzleParticles != null)
        {
            muzzleParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            muzzleParticles.Play();
        }

        yield return new WaitForSeconds(muzzleParticleDuration);
    }

    void FireHitscan()
    {
        Camera cam = fireCamera != null ? fireCamera : Camera.main;
        if (cam == null) return;

        Vector3 origin = cam.transform.position;
        lastShotPellets.Clear();

        for (int i = 0; i < pelletCount; i++)
        {
            Vector3 direction = RandomSpreadDirection(cam.transform, spreadAngle);
            Vector3 hitPoint = origin + direction * hitscanRange;
            bool didHit = false;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, hitscanRange, hitscanMask))
            {
                hitPoint = hit.point;
                didHit = true;

                IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
                damageable?.TakeDamage(damagePerPellet);

                IShotImpactReceiver impactReceiver = hit.collider.GetComponentInParent<IShotImpactReceiver>();
                impactReceiver?.ReceiveShotImpact(hit.point, direction, shotImpactForce);

                if (!IsImpactExcluded(hit.collider.tag)) SpawnImpactVFX(hit.point, hit.normal);
            }

            lastShotPellets.Add((origin, hitPoint, didHit));
            SpawnTracerLine(muzzlePoint != null ? muzzlePoint.position : origin, hitPoint);
        }
    }

    void SpawnTracerLine(Vector3 start, Vector3 end)
    {
        if (tracerMaterial == null) return;

        GameObject lineObj = new GameObject("TracerLine");
        lineObj.transform.position = start;

        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        line.material = tracerMaterial;
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        line.startWidth = tracerLineWidth;
        line.endWidth = tracerLineWidth * 0.2f;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;

        Destroy(lineObj, tracerLineDuration);
    }

    void OnDrawGizmos()
    {
        if (!showSpreadGizmos) return;

        foreach (var pellet in lastShotPellets)
        {
            Gizmos.color = pellet.hit ? spreadGizmoHitColor : spreadGizmoColor;
            Gizmos.DrawLine(pellet.start, pellet.end);
            if (pellet.hit) Gizmos.DrawWireSphere(pellet.end, 0.1f);
        }
    }

    static Vector3 RandomSpreadDirection(Transform from, float maxAngle)
    {
        Vector2 randomCircle = Random.insideUnitCircle * Mathf.Tan(maxAngle * Mathf.Deg2Rad);
        Vector3 direction = from.forward + from.right * randomCircle.x + from.up * randomCircle.y;
        return direction.normalized;
    }

    bool IsImpactExcluded(string colliderTag)
    {
        foreach (string tag in impactExcludeTags)
        {
            if (tag == colliderTag) return true;
        }
        return false;
    }

    void SpawnImpactVFX(Vector3 point, Vector3 normal)
    {
        // Ground = normal close to straight up or down (floor/ceiling), otherwise treated as a wall.
        float angleFromUp = Vector3.Angle(normal, Vector3.up);
        bool isGround = angleFromUp <= groundNormalMaxAngle || angleFromUp >= 180f - groundNormalMaxAngle;

        VisualEffect prefab = isGround ? groundImpactVFXPrefab : wallImpactVFXPrefab;
        if (prefab == null) return;

        VisualEffect impact = Instantiate(prefab, point, Quaternion.identity);
        impact.Play();
        Destroy(impact.gameObject, impactVFXLifetime);
    }
}
