using UnityEngine;

public class DanceAreaActivator : MonoBehaviour
{
    public GameObject danceAreaUI;

    public void ShowDanceArea()
    {
        danceAreaUI.SetActive(true);
        Debug.Log("[Dance Area Activated]");
    }
}