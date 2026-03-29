using UnityEngine;

public class PingCue : MonoBehaviour
{
    [SerializeField] private float lifetime = 1f;

    private void Awake()
    {
        Destroy(gameObject, lifetime);
    }
}
