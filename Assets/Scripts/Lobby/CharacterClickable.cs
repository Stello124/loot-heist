using UnityEngine;

public class CharacterClickable : MonoBehaviour
{
    void OnMouseDown()
    {
        
        if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            CustomizationUIController.Instance.OpenCustomizationPanel();
        }
    }
}