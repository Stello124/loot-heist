using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    public Transform doorTransform;     // Kapý objesi
    public float openAngle = 90f;       // Açýlma açýsý
    public float openSpeed = 3f;        // Açýlma hýzý

    private bool isPlayerNearby = false;
    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private BoxCollider doorCollider;

    void Start()
    {
        // Baþlangýç ve hedef rotasyonlarý ayarla
        closedRotation = doorTransform.rotation;
        openRotation = Quaternion.Euler(doorTransform.eulerAngles + new Vector3(0f, -openAngle, 0f)); // Ýçeri doðru
        doorCollider = doorTransform.GetComponent<BoxCollider>();
    }

    void Update()
    {
        // E tuþuna basýnca kapýyý aç/kapat
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            isOpen = !isOpen;
        }

        // Kapýnýn yumuþak dönüþü
        if (isOpen)
            doorTransform.rotation = Quaternion.Lerp(doorTransform.rotation, openRotation, Time.deltaTime * openSpeed);
        else
            doorTransform.rotation = Quaternion.Lerp(doorTransform.rotation, closedRotation, Time.deltaTime * openSpeed);

        // Kapý neredeyse tamamen açýkken collider'ý devre dýþý býrak
        if (Quaternion.Angle(doorTransform.rotation, openRotation) < 5f)
        {
            if (doorCollider != null)
                doorCollider.enabled = false;
        }
        else
        {
            if (doorCollider != null)
                doorCollider.enabled = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            isPlayerNearby = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            isPlayerNearby = false;
    }
}
