using UnityEngine;
using UnityEngine.EventSystems;

public class PanelCloseOnClick : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        gameObject.SetActive(false);
    }
}
