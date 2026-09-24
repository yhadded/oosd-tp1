using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private float duration = 0.25f;
    [SerializeField] private float magnitude = 0.2f;

    private Vector3 originalPos;
    private Coroutine routine;

    private void Awake()
    {
        originalPos = transform.localPosition;
    }

    public void Shake()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(DoShake());
    }

    private IEnumerator DoShake()
    {
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            float damper = 1f - t / duration;
            Vector2 offset = Random.insideUnitCircle * magnitude * damper;
            transform.localPosition = originalPos + (Vector3)offset;
            yield return null;
        }
        transform.localPosition = originalPos;
    }
}
