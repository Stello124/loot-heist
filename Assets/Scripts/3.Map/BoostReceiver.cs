using System.Collections;
using UnityEngine;

public class BoostReceiver : MonoBehaviour
{
    private Controller.CharacterMover mover;

    public float boostDuration = 4f;
    public float chestRespawnTime = 3f;

    public float boostedJump = 7f;
    public float boostedSpeed = 7f;

    private float originalJump;
    private float originalSpeed;

    private void Start()
    {
        mover = GetComponent<Controller.CharacterMover>();
        originalJump = mover.GetJumpHeight();
        originalSpeed = mover.GetRunSpeed();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("JumpChest"))
        {
            Debug.Log("Jump boost alýndý!");
            StartCoroutine(ApplyJumpBoost());
            StartCoroutine(RespawnChest(other.gameObject));
        }
        else if (other.CompareTag("SpeedChest"))
        {
            Debug.Log("Speed boost alýndý!");
            StartCoroutine(ApplySpeedBoost());
            StartCoroutine(RespawnChest(other.gameObject));
        }
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
