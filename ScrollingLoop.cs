using UnityEngine;

public class ScrollingLoop : MonoBehaviour
{
    [Tooltip("Width of ONE tile in world units. Leave 0 to measure it from the first child's SpriteRenderer.")]
    [SerializeField] private float tileWidth = 0f;
    [Tooltip("1 = same speed as the columns. Use < 1 for a background (parallax).")]
    [SerializeField] private float speedMultiplier = 1f;

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
        if (tileWidth <= 0f)
        {
            var sr = GetComponentInChildren<SpriteRenderer>();
            if (sr) tileWidth = sr.bounds.size.x;
        }
    }

    private void Update()
    {
        if (tileWidth <= 0f) return;

        float speed = GameManager.Instance ? GameManager.Instance.CurrentSpeed : 0f;
        if (GameManager.Instance && GameManager.Instance.State == GameState.Ready) speed = 3f;

        transform.position += Vector3.left * speed * speedMultiplier * Time.deltaTime;

        if (startPos.x - transform.position.x >= tileWidth)
            transform.position += Vector3.right * tileWidth;
    }
}
