using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    public Transform doorTransform;     // Kap� objesi
    public float openAngle = 90f;       // A��lma a��s�
    public float openSpeed = 3f;        // A��lma h�z�

    private bool isPlayerNearby = false;
    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private BoxCollider doorCollider;

    void Start()
    {
        // Ba�lang�� ve hedef rotasyonlar� ayarla
        closedRotation = doorTransform.rotation;
        openRotation = Quaternion.Euler(doorTransform.eulerAngles + new Vector3(0f, -openAngle, 0f)); // ��eri do�ru
        doorCollider = doorTransform.GetComponent<BoxCollider>();
    }

    void Update()
    {
        // E tu�una bas�nca kap�y� a�/kapat
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            isOpen = !isOpen;
        }

        // Kap�n�n yumu�ak d�n���
        if (isOpen)
            doorTransform.rotation = Quaternion.Lerp(doorTransform.rotation, openRotation, Time.deltaTime * openSpeed);
        else
            doorTransform.rotation = Quaternion.Lerp(doorTransform.rotation, closedRotation, Time.deltaTime * openSpeed);

        // Kap� neredeyse tamamen a��kken collider'� devre d��� b�rak
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
