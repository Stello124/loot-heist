using UnityEngine;

public class MouseLocker : MonoBehaviour
{
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;  // Mouse ekranýn ortasýna sabitlenir
        Cursor.visible = false;                    // Mouse görünmez olur
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None; // ESC ile serbest býrak
            Cursor.visible = true;                  // ESC ile mouse görünür olur
        }
    }
}
