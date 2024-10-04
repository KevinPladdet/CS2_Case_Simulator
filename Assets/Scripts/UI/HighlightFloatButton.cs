using UnityEngine;
using UnityEngine.EventSystems;

public class HighlightFloatButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField] private GameObject floatMenu;

    public void OnPointerEnter(PointerEventData eventData)
    {
        floatMenu.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        floatMenu.SetActive(false);
    }
}
