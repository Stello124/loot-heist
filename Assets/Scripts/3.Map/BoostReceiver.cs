using System.Collections;
using UnityEngine;

public class BoostReceiver : MonoBehaviour
{
    private Controller.CharacterMover mover;
    private AudioSource audioSource;

    public float boostDuration = 4f;
    public float chestRespawnTime = 3f;

    public float boostedJump = 7f;
    public float boostedSpeed = 7f;

    public AudioClip jumpBoostSound;
    public AudioClip speedBoostSound;

    private float originalJump;
    private float originalSpeed;

    private void Start()
    {
        mover = GetComponent<Controller.CharacterMover>();
        audioSource = GetComponent<AudioSource>();
        originalJump = mover.GetJumpHeight();
        originalSpeed = mover.GetRunSpeed();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("JumpChest"))
        {
            Debug.Log("Jump boost alýndý!");
            PlaySound(jumpBoostSound);
            StartCoroutine(ApplyJumpBoost());
            StartCoroutine(RespawnChest(other.gameObject));
        }
        else if (other.CompareTag("SpeedChest"))
        {
            Debug.Log("Speed boost alýndý!");
            PlaySound(speedBoostSound);
            StartCoroutine(ApplySpeedBoost());
            StartCoroutine(RespawnChest(other.gameObject));
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    private IEnumerator ApplyJumpBoost()
    {
        mover.SetJumpHeight(boostedJump);
        yield return new WaitForSeconds(boostDuration);
        mover.SetJumpHeight(originalJump);
    }

    private IEnumerator ApplySpeedBoost()
    {
        mover.SetRunSpeed(boostedSpeed);
        yield return new WaitForSeconds(boostDuration);
        mover.SetRunSpeed(originalSpeed);
    }

    private IEnumerator RespawnChest(GameObject chest)
    {
        chest.SetActive(false);
        yield return new WaitForSeconds(chestRespawnTime);
        chest.SetActive(true);
    }
}

