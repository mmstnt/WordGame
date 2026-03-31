using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BattleButton : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IDeselectHandler
{
    public Graphic[] childGraphics;
    public Color normalColor;
    public Color pressedColor;
    public Color selectedColor;

    [Header("廣播")]
    public VoidEventSO battleButtonUpdataEvent;

    private void OnEnable()
    {
        ChangeColor(normalColor);
        StartCoroutine(InitStatusAtEndOfFrame());
    }

    private IEnumerator InitStatusAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();

        //判斷是否選中，選中則變亮，並觸發廣播
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == this.gameObject)
        {
            ChangeColor(selectedColor);
            battleButtonUpdataEvent?.raiseEvent();
        }
        else
        {
            ChangeColor(normalColor);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        ChangeColor(selectedColor);
        battleButtonUpdataEvent?.raiseEvent();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        ChangeColor(normalColor);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (EventSystem.current != null) 
        {
            EventSystem.current.SetSelectedGameObject(this.gameObject);
        }
    }

    public void OnPointerExit(PointerEventData eventData) 
    {
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != this.gameObject)
        {
             ChangeColor(normalColor);
        }
    }

    public void OnPointerDown(PointerEventData eventData) => ChangeColor(pressedColor);

    public void OnPointerUp(PointerEventData eventData)
    {
        //放開時檢查是否仍被選中
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == this.gameObject)
        {
            ChangeColor(selectedColor);
        }
        else
        {
            ChangeColor(normalColor);
        }
    }

    private void ChangeColor(Color targetColor)
    {
        if (childGraphics == null) return;

        foreach (var g in childGraphics)
        {
            if (g != null) g.color = targetColor;
        }
    }
}
