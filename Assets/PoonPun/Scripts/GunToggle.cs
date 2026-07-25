using UnityEngine;
using UnityEngine.VFX;

public class GunToggle : MonoBehaviour
{
    [SerializeField] KeyCode toggleKey = KeyCode.Alpha1;
    [SerializeField] GameObject target;
    [SerializeField] float moveDistance = 15f;
    [SerializeField] float moveDuration = 0.3f;

    [Header("Muzzle Flash")]
    [SerializeField] GameObject muzzleObject;
    [SerializeField] Light muzzleLight;
    [SerializeField] VisualEffect muzzleVFX;
    [SerializeField] float muzzleLightIntensity = 13f;
    [SerializeField] float muzzleFlashDuration = 0.1f;

    bool isOn;
    Vector3 downPosition;
    Vector3 upPosition;
    Coroutine moveRoutine;
    Coroutine muzzleRoutine;

    void Start()
    {
        isOn = false;
        downPosition = target.transform.localPosition;
        Vector3 localUp = target.transform.parent != null
            ? target.transform.parent.InverseTransformDirection(Vector3.up)
            : Vector3.up;
        upPosition = downPosition + localUp * moveDistance;
        target.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (isOn) TurnOff();
            else TurnOn();
        }

        if (isOn && Input.GetMouseButtonDown(0))
        {
            Fire();
        }
    }

    void Fire()
    {
        if (muzzleRoutine != null) StopCoroutine(muzzleRoutine);
        muzzleRoutine = StartCoroutine(MuzzleFlash());
    }

    System.Collections.IEnumerator MuzzleFlash()
    {
        if (muzzleObject != null) muzzleObject.SetActive(true);
        if (muzzleVFX != null) muzzleVFX.Play();

        if (muzzleLight != null)
        {
            float half = muzzleFlashDuration * 0.5f;
            float elapsed = 0f;
            while (elapsed < half)
            {
                elapsed += Time.deltaTime;
                muzzleLight.intensity = Mathf.Lerp(0f, muzzleLightIntensity, elapsed / half);
                yield return null;
            }
            muzzleLight.intensity = muzzleLightIntensity;

            elapsed = 0f;
            while (elapsed < half)
            {
                elapsed += Time.deltaTime;
                muzzleLight.intensity = Mathf.Lerp(muzzleLightIntensity, 0f, elapsed / half);
                yield return null;
            }
            muzzleLight.intensity = 0f;
        }
        else
        {
            yield return new WaitForSeconds(muzzleFlashDuration);
        }

        if (muzzleObject != null) muzzleObject.SetActive(false);
    }

    void TurnOn()
    {
        isOn = true;
        target.SetActive(true);
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(MoveAndThen(upPosition, null));
    }

    void TurnOff()
    {
        isOn = false;
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(MoveAndThen(downPosition, () => target.SetActive(false)));
    }

    System.Collections.IEnumerator MoveAndThen(Vector3 destination, System.Action onComplete)
    {
        Vector3 start = target.transform.localPosition;
        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            target.transform.localPosition = Vector3.Lerp(start, destination, t);
            yield return null;
        }
        target.transform.localPosition = destination;
        onComplete?.Invoke();
    }
}
