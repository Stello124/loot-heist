using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    public Transform spike;           // Yukarı-aşağı hareket edecek diken modeli
    public float downY = 0f;          // İçerdeki (başlangıç) pozisyon
    public float upY = 1f;            // Yüzeye çıktığı pozisyon
    public float speed = 2f;          // Hareket hızı
    public float stayUpTime = 1f;     // Yukarıda ne kadar kalacak
    public float stayDownTime = 2f;   // Aşağıda ne kadar kalacak

    private bool goingUp = true;
    private float timer;

    private void Start()
    {
        Vector3 pos = spike.localPosition;
        pos.y = downY;
        spike.localPosition = pos;
        timer = stayDownTime;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            goingUp = !goingUp;
            timer = goingUp ? stayUpTime : stayDownTime;
        }

        // Hareketi uygula
        Vector3 targetPos = spike.localPosition;
        float targetY = goingUp ? upY : downY;
        targetPos.y = Mathf.MoveTowards(spike.localPosition.y, targetY, speed * Time.deltaTime);
        spike.localPosition = targetPos;
    }
}
