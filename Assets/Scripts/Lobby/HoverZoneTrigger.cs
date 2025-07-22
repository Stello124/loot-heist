using UnityEngine;

public class HoverZoneTrigger : MonoBehaviour
{
    private Renderer characterRenderer;
    private Color baseColor;
    private Color hoverColor;

    private bool isMouseInside = false;

    [SerializeField] private GameObject customizationPanel;
    [SerializeField] private GameObject mainPanel;

    void Start()
    {
        characterRenderer = transform.parent.GetComponent<Renderer>();
        baseColor = characterRenderer.material.GetColor("_BaseColor");
        hoverColor = new Color(
            Mathf.Clamp01(baseColor.r + 0.2f),
            Mathf.Clamp01(baseColor.g + 0.2f),
            Mathf.Clamp01(baseColor.b + 0.2f),
            0.7f
        );
    }

    void OnMouseEnter()
    {
        characterRenderer.material.SetColor("_BaseColor", hoverColor);
        isMouseInside = true;
    }

    void OnMouseExit()
    {
        characterRenderer.material.SetColor("_BaseColor", baseColor);
        isMouseInside = false;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && isMouseInside)
        {
            bool isActive = customizationPanel.activeSelf;
            customizationPanel.SetActive(!isActive);
        }
    }
}