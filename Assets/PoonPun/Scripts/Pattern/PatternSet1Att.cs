using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternSet1Att : MonoBehaviour
{
    // ============================================================
    // Full sequence, in order:
    //   [Action 1] both bullets spawn -> Approach -> Charge Out (no shooting)
    //   ... Delay Before Second Carrier ...
    //   [Action 2] both bullets spawn AGAIN -> Approach -> Delay Before Loop Start
    //              -> Action 3 (Shooting Loop) -> Charge Out
    // "Approach" and "Charge Out" below are reused as-is for both Action 1 and
    // Action 2 - they're the same two moves every time a pair of carriers spawns.
    // ============================================================

    [Header("APPROACH - carrier eases from spawn to its own Spawn Position")]
    [Tooltip("Runs at the start of every pass (both the Action 1 charge-out pass and the Action 2 shooting pass). How long the ease-in from the enemy's position to the bullet's own Spawn/Loop Position takes.")]
    [SerializeField] private float approachDuration = 1f;

    [Header("ACTION 1 - Charge Out (no shooting)")]
    [Tooltip("The first pass: both carriers approach, then immediately charge themselves out - nothing is fired here, this is a decoy dash. Also reused at the very end of Action 2's shooting pass.")]
    [SerializeField] private float selfShotDelay = 0.3f;
    [Tooltip("Small forward windup move before the final launch (eases in, like Approach).")]
    [SerializeField] private BulletMoveStep selfShotMove = new BulletMoveStep { move = new Vector3(0f, 0f, 2f), duration = 0.3f };
    [Tooltip("The final launch, at constant speed - e.g. Z=100 with Duration=1 travels 100 units in 1 second.")]
    [SerializeField] private BulletMoveStep selfShotLaunchMove = new BulletMoveStep { move = new Vector3(0f, 0f, 100f), duration = 1f };

    [Header("ACTION 2 - Shooting Loop")]
    [Tooltip("The second pass: a fresh pair of carriers spawns, approaches, waits Delay Before Loop Start, then runs this shot loop together (one shot from each carrier per tick) before charging out via the same Charge Out settings above.")]
    [SerializeField] private int shotsPerLoop = 5;
    [SerializeField] private float shotDelay = 0.15f;
    [SerializeField] private float shotLifetime = 5f;
    [SerializeField] private int batchCount = 2;
    [Tooltip("Move between each batch of shots, at constant speed.")]
    [SerializeField] private BulletMoveStep advanceMove = new BulletMoveStep { move = new Vector3(0f, 0f, 5f), duration = 0.5f };

    [Header("TIMING BETWEEN ACTIONS")]
    [Tooltip("Pause after Action 1's charge-out finishes, before Action 2 even spawns its carriers.")]
    [SerializeField] private float delayBeforeSecondCarrier = 0f;
    [Tooltip("Pause after Action 2's carriers finish approaching, before the Shooting Loop begins.")]
    [SerializeField] private float delayBeforeLoopStart = 0f;

    public bool IsCarrierPattern(PatternSet1 patternData, BulletPattern pattern)
    {
        return pattern != null && patternData != null && patternData.Patterns.IndexOf(pattern) == 0;
    }

    public void Fire(PatternSet1 patternData, BulletPattern pattern, Transform origin, MonoBehaviour runner, System.Action onComplete = null)
    {
        if (pattern == null)
        {
            Debug.LogWarning($"{name}: Fire() called with a null pattern.");
            onComplete?.Invoke();
            return;
        }

        int patternIndex = patternData != null ? patternData.Patterns.IndexOf(pattern) : -1;
        bool isFirstPattern = patternIndex == 0;

        Debug.Log($"{name}: firing pattern id='{pattern.id}' (index {patternIndex}) - {(isFirstPattern ? "carrier loop" : "waypoint")} behavior, {pattern.bullets.Count} bullet entrie(s).");

        if (!isFirstPattern)
        {
            foreach (BulletData bulletData in pattern.bullets)
            {
                SpawnWaypointBullet(bulletData, origin, runner);
            }
            onComplete?.Invoke();
            return;
        }

        runner.StartCoroutine(RunAttackSequence(pattern.bullets, origin, runner, onComplete));
    }

    // --- Index 0 pattern: both bullet entries spawn together as a mirrored pair (e.g. +0.5/-0.5),
    // approach in parallel, then charge themselves out with no shooting ("carrier 1" pass).
    // After a delay, both spawn AGAIN as a fresh mirrored pair, approach again, run the shot
    // loop together (one shot from each per tick), then charge out ("carrier 2" pass). ---

    private IEnumerator RunAttackSequence(List<BulletData> bulletsData, Transform origin, MonoBehaviour runner, System.Action onComplete)
    {
        if (bulletsData == null || bulletsData.Count == 0)
        {
            onComplete?.Invoke();
            yield break;
        }

        // Carrier 1 pass: mirrored pair spawns, approaches, charges out - no shooting.
        yield return RunCarrierPair(bulletsData, origin, runner, shoot: false);

        if (delayBeforeSecondCarrier > 0f) yield return new WaitForSeconds(delayBeforeSecondCarrier);

        // Carrier 2 pass: a fresh mirrored pair spawns, approaches, shoots together, charges out.
        yield return RunCarrierPair(bulletsData, origin, runner, shoot: true);

        onComplete?.Invoke();
    }

    private IEnumerator RunCarrierPair(List<BulletData> bulletsData, Transform origin, MonoBehaviour runner, bool shoot)
    {
        Transform[] carriers = new Transform[bulletsData.Count];
        for (int i = 0; i < bulletsData.Count; i++)
        {
            carriers[i] = SpawnCarrierTransform(bulletsData[i], origin);
        }

        yield return RunInParallel(runner, carriers, (carrier, i) => MoveStep(carrier, ApproachMoveFor(bulletsData[i]), origin, easeIn: true));

        if (shoot)
        {
            if (delayBeforeLoopStart > 0f) yield return new WaitForSeconds(delayBeforeLoopStart);
            yield return RunShotLoopParallel(carriers, bulletsData, origin, runner);
        }

        yield return RunInParallel(runner, carriers, (carrier, i) => ChargeOut(carrier, origin));
    }

    // Starts one coroutine per non-null carrier and waits for all of them to finish.
    private IEnumerator RunInParallel(MonoBehaviour runner, Transform[] carriers, System.Func<Transform, int, IEnumerator> routine)
    {
        var running = new List<Coroutine>();
        for (int i = 0; i < carriers.Length; i++)
        {
            if (carriers[i] == null) continue;
            running.Add(runner.StartCoroutine(routine(carriers[i], i)));
        }
        foreach (Coroutine c in running) yield return c;
    }

    private Transform SpawnCarrierTransform(BulletData data, Transform origin)
    {
        if (data.bulletPrefab == null) return null;
        return Instantiate(data.bulletPrefab, origin.position, origin.rotation).transform;
    }

    private BulletMoveStep ApproachMoveFor(BulletData data)
    {
        return new BulletMoveStep { move = data.spawnPosition, duration = approachDuration };
    }

    // Fires one shot per surviving carrier each tick (e.g. 2 bullets at once for a mirrored pair).
    private IEnumerator RunShotLoopParallel(Transform[] carriers, List<BulletData> bulletsData, Transform origin, MonoBehaviour runner)
    {
        for (int batch = 0; batch < batchCount; batch++)
        {
            for (int i = 0; i < shotsPerLoop; i++)
            {
                for (int c = 0; c < carriers.Length; c++)
                {
                    if (carriers[c] == null) continue;

                    BulletData data = bulletsData[c];
                    BulletMoveStep shotMove = data.moveSteps.Count > 0 ? data.moveSteps[0] : null;

                    GameObject shot = Instantiate(data.bulletPrefab, carriers[c].position, carriers[c].rotation);
                    runner.StartCoroutine(RunShot(shot.transform, shotMove, origin));
                }

                if (shotDelay > 0f) yield return new WaitForSeconds(shotDelay);
            }

            bool isLastBatch = batch == batchCount - 1;
            if (!isLastBatch)
            {
                yield return RunInParallel(runner, carriers, (carrier, i) => MoveStep(carrier, advanceMove, origin));
            }
        }
    }

    private IEnumerator ChargeOut(Transform carrier, Transform origin)
    {
        if (carrier != null && selfShotDelay > 0f) yield return new WaitForSeconds(selfShotDelay);
        if (carrier != null) yield return MoveStep(carrier, selfShotMove, origin, easeIn: true);
        if (carrier != null) yield return MoveStep(carrier, selfShotLaunchMove, origin);
    }

    private IEnumerator RunShot(Transform shot, BulletMoveStep shotMove, Transform origin)
    {
        yield return MoveStep(shot, shotMove, origin);
        if (shot == null) yield break;

        yield return new WaitForSeconds(shotLifetime);
        if (shot != null) Destroy(shot.gameObject);
    }

    // --- Any other pattern: spawn and walk through moveSteps in order, once ---

    private void SpawnWaypointBullet(BulletData data, Transform origin, MonoBehaviour runner)
    {
        if (data.bulletPrefab == null) return;

        Vector3 spawnPos = origin.position + origin.TransformDirection(data.spawnPosition);
        GameObject bullet = Instantiate(data.bulletPrefab, spawnPos, origin.rotation);
        runner.StartCoroutine(RunMoveSteps(bullet.transform, data.moveSteps, origin));
    }

    private IEnumerator RunMoveSteps(Transform bullet, List<BulletMoveStep> moveSteps, Transform origin)
    {
        foreach (BulletMoveStep step in moveSteps)
        {
            yield return MoveStep(bullet, step, origin);
            if (bullet == null) yield break;
        }
    }

    private IEnumerator MoveStep(Transform bullet, BulletMoveStep step, Transform origin, bool easeIn = false)
    {
        if (step == null || bullet == null) yield break;

        Vector3 start = bullet.position;
        Vector3 target = start + origin.TransformDirection(step.move);
        float duration = Mathf.Max(0.0001f, step.duration);

        float elapsed = 0f;
        while (bullet != null && elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            if (easeIn) t = t * t;
            bullet.position = Vector3.Lerp(start, target, t);
            yield return null;
        }

        if (bullet != null) bullet.position = target;
    }
}
