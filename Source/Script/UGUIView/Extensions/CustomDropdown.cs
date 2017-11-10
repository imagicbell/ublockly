using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UBlockly.UGUI
{
    public class CustomDropdown : Dropdown
    {
        private UnityEvent mShowOptionsEvent = null;

        public void AddShowOptionsListener(UnityAction listener)
        {
            if (mShowOptionsEvent == null)
                mShowOptionsEvent = new Button.ButtonClickedEvent();
            mShowOptionsEvent.AddListener(listener);
        }

        public void RemoveShowOptionsListener(UnityAction listener)
        {
            if (mShowOptionsEvent != null)
                mShowOptionsEvent.RemoveListener(listener);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            if (mShowOptionsEvent != null)
                mShowOptionsEvent.Invoke();
        }
    }
}
