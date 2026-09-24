using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ScoreZone : MonoBehaviour
{
    private bool scored;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (scored) return;
        if (other.GetComponentInParent<BirdController>() == null) return;

        scored = true;
        GameManager.Instance?.AddScore();
    }
}
