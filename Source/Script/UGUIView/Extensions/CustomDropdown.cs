/****************************************************************************

Copyright 2016 sophieml1989@gmail.com

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

****************************************************************************/

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
