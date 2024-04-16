using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ButtonHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public class ButtonHoverEvent : UnityEvent { }
    public ButtonHoverEvent OnHover = new ButtonHoverEvent();
    public ButtonHoverEvent OnUnhover = new ButtonHoverEvent();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnHover?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnUnhover?.Invoke();
    }
}
