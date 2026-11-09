using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LightBetweenCultures
{
    public sealed class ExplorationSurface : MonoBehaviour, IPointerClickHandler
    {
        public Action<Vector2> OnClicked;

        public void OnPointerClick(PointerEventData eventData)
        {
            var rect = (RectTransform)transform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rect, eventData.position, eventData.pressEventCamera, out var local))
                return;
            var normalized = new Vector2(
                (local.x / rect.rect.width) + .5f,
                (local.y / rect.rect.height) + .5f);
            OnClicked?.Invoke(normalized);
        }
    }
}
