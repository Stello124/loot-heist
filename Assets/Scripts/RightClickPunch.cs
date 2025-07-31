using UnityEngine;

public class RightClickPunch : MonoBehaviour, IRightClickAction
{
    public GameObject punchPrefab;
    public Transform spawnPoint;

    public void OnRightClick()
    {
        Instantiate(punchPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}
