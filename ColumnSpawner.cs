using UnityEngine;

public class ColumnSpawner : MonoBehaviour
{
    [SerializeField] private ColumnPair columnPairPrefab;

    [Header("Position")]
    [Tooltip("X where pairs appear (just outside the right edge of the camera).")]
    [SerializeField] private float spawnX = 12f;
    [SerializeField] private float minCenterY = -1.5f;
    [SerializeField] private float maxCenterY = 2.5f;

    [Header("Gap (space between top and bottom column)")]
    [SerializeField] private float minGap = 2.8f;
    [SerializeField] private float maxGap = 3.6f;
    [Tooltip("Bonus: gap reduction per point scored.")]
    [SerializeField] private float gapShrinkPerPoint = 0.03f;
    [Tooltip("Never go below this, whatever the score (bird is ~1 unit tall).")]
    [SerializeField] private float absoluteMinGap = 2.2f;

    [Header("Rhythm")]
    [Tooltip("Horizontal distance between two pairs, in world units.")]
    [SerializeField] private float distanceBetweenPairs = 5f;

    private float timer;

    private void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null || gm.State != GameState.Playing) return;

        float interval = distanceBetweenPairs / Mathf.Max(0.01f, gm.CurrentSpeed);
        timer += Time.deltaTime;
        if (timer >= interval)
        {
            timer -= interval;
            Spawn(gm.Score);
        }
    }

    private void Spawn(int score)
    {
        float centerY = Random.Range(minCenterY, maxCenterY);

        float shrink = score * gapShrinkPerPoint;
        float gap = Random.Range(minGap, maxGap) - shrink;
        gap = Mathf.Max(gap, absoluteMinGap);

        ColumnPair pair = Instantiate(columnPairPrefab, new Vector3(spawnX, centerY, 0f), Quaternion.identity);
        pair.Layout(gap);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(spawnX, minCenterY), new Vector3(spawnX, maxCenterY));
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(new Vector3(spawnX, maxCenterY), new Vector3(1f, maxGap, 0f));
    }
#endif
}
