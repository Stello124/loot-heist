using UnityEngine;
using UnityEngine.EventSystems;

public class DanceAreaClickHandler : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private PlayerDanceController playerDanceController;
    [SerializeField] private string danceToTrigger = "Dance_Twerk"; // panelden seçilebilir

    public void OnPointerClick(PointerEventData eventData)
    {
        // Týklanan ekran pozisyonunu world-space'e çevir
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 clickWorldPos = hit.point;

            // Dans tetiklemesini RPC ile yap
            playerDanceController.TriggerDanceAtServerRpc(clickWorldPos, danceToTrigger);
        }
    }
}