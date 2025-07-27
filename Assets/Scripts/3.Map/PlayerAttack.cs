using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float attackRange = 2f;
    public LayerMask playerLayer;
    public Transform attackOrigin;
    public AudioClip punchSound;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryAttack();
        }
    }

    void TryAttack()
    {
        RaycastHit hit;
        if (Physics.Raycast(attackOrigin.position, attackOrigin.forward, out hit, attackRange, playerLayer))
        {
            GameObject hitPlayer = hit.collider.gameObject;

            if (hitPlayer.CompareTag("Player"))
            {
                if (BombManager.Instance.GetCurrentBombHolder() == this.gameObject)
                {
                    BombManager.Instance.SetBombHolder(hitPlayer);
                    Debug.Log($"{gameObject.name} passed bomb to {hitPlayer.name} with punch!");

                    // Ses efekti
                    if (punchSound != null && audioSource != null)
                        audioSource.PlayOneShot(punchSound);
                }
            }
        }
    }
}

