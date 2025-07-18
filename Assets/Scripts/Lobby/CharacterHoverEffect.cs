using UnityEngine;

public class CharacterHoverEffect : MonoBehaviour
{
    private Renderer rend;
    private Color baseColor;
    private Color hoverColor;
    private bool isHovered;

    void Start()
    {
        rend = GetComponent<Renderer>();
        baseColor = rend.material.GetColor("_BaseColor");

        // Hover için daha parlak ve hafif saydam renk
        hoverColor = new Color(
            Mathf.Clamp01(baseColor.r + 0.2f),
            Mathf.Clamp01(baseColor.g + 0.2f),
            Mathf.Clamp01(baseColor.b + 0.2f),
            0.7f
        );
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject == gameObject)
            {
                if (!isHovered)
                {
                    rend.material.SetColor("_BaseColor", hoverColor);
                    isHovered = true;
                }
            }
            else if (isHovered)
            {
                rend.material.SetColor("_BaseColor", baseColor);
                isHovered = false;
            }
        }
        else if (isHovered)
        {
            rend.material.SetColor("_BaseColor", baseColor);
            isHovered = false;
        }
    }
}