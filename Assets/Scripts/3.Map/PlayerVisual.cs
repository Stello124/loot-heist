using System.Collections;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    private Renderer characterRenderer;

    private Color normalColor = Color.white;
    private Color bombColor = Color.red;

    private Coroutine flashRoutine;

    void Awake()
    {
        // Karakterin içindeki ilk Renderer'ý otomatik bul
        characterRenderer = GetComponentInChildren<Renderer>();
        if (characterRenderer == null)
        {
            Debug.LogError("Renderer bulunamadý! Prefab içinde Renderer var mý kontrol et.");
        }
    }

    public void SetBomb(bool isBomber)
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        if (isBomber)
            flashRoutine = StartCoroutine(BlinkRed());
        else if (characterRenderer != null)
            characterRenderer.material.color = normalColor;
    }

    IEnumerator BlinkRed()
    {
        while (true)
        {
            if (characterRenderer != null)
                characterRenderer.material.color = bombColor;
            yield return new WaitForSeconds(0.3f);

            if (characterRenderer != null)
                characterRenderer.material.color = normalColor;
            yield return new WaitForSeconds(0.3f);
        }
    }
}


