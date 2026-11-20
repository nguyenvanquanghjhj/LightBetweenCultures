using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LightBetweenCultures
{
    public sealed class ExplorationItem : MonoBehaviour, IPointerClickHandler
    {
        public Action<ExplorationItem> OnSelected;

        public void OnPointerClick(PointerEventData eventData)
        {
            OnSelected?.Invoke(this);
        }
    }
}
