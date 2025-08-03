using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class BoostReceiver : NetworkBehaviour
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
        if (mover != null)
        {
            originalJump = mover.GetJumpHeight();
            originalSpeed = mover.GetRunSpeed();
        }
    }
    
    // NetworkChest tarafından çağrılır
    public void ApplyNetworkBoost(string boostType)
    {
        if (boostType == "jump")
        {
            PlaySound(jumpBoostSound);
            StartCoroutine(ApplyJumpBoost());
        }
        else if (boostType == "speed")
        {
            PlaySound(speedBoostSound);
            StartCoroutine(ApplySpeedBoost());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Sadece owner collision'ları işlesin
        if (!IsOwner) return;
        
        if (other.CompareTag("JumpChest"))
        {
            Debug.Log($"🦘 Jump boost alındı! (Client: {OwnerClientId})");
            RequestChestBoostServerRpc(other.transform.position, "jump");
        }
        else if (other.CompareTag("SpeedChest"))
        {
            Debug.Log($"⚡ Speed boost alındı! (Client: {OwnerClientId})");
            RequestChestBoostServerRpc(other.transform.position, "speed");
        }
    }
    
    [ServerRpc]
    private void RequestChestBoostServerRpc(Vector3 chestPosition, string boostType)
    {
        // Server'da chest'i bul ve boost uygula
        ApplyBoostToPlayerClientRpc(OwnerClientId, boostType);
        DisableNearbyChestClientRpc(chestPosition, boostType);
    }
    
    [ClientRpc]
    private void ApplyBoostToPlayerClientRpc(ulong targetClientId, string boostType)
    {
        // Sadece hedef client boost alsın
        if (NetworkManager.Singleton.LocalClientId == targetClientId)
        {
            if (boostType == "jump")
            {
                PlaySound(jumpBoostSound);
                StartCoroutine(ApplyJumpBoost());
            }
            else if (boostType == "speed")
            {
                PlaySound(speedBoostSound);
                StartCoroutine(ApplySpeedBoost());
            }
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

    [ClientRpc]
    private void DisableNearbyChestClientRpc(Vector3 chestPosition, string chestType)
    {
        // Tüm client'larda chest'i bul ve devre dışı bırak
        GameObject chest = FindNearbyChest(chestPosition, chestType);
        if (chest != null)
        {
            StartCoroutine(RespawnChest(chest));
        }
    }
    
    private GameObject FindNearbyChest(Vector3 position, string chestType)
    {
        string targetTag = chestType == "jump" ? "JumpChest" : "SpeedChest";
        GameObject[] chests = GameObject.FindGameObjectsWithTag(targetTag);
        
        foreach (GameObject chest in chests)
        {
            if (Vector3.Distance(chest.transform.position, position) < 1f)
            {
                return chest;
            }
        }
        
        return null;
    }
    
    private IEnumerator RespawnChest(GameObject chest)
    {
        chest.SetActive(false);
        Debug.Log($"📦 {chest.name} devre dışı - {chestRespawnTime}s sonra respawn");
        yield return new WaitForSeconds(chestRespawnTime);
        chest.SetActive(true);
        Debug.Log($"✨ {chest.name} respawn oldu!");
    }
}

