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


using UnityEngine;
using UnityEngine.EventSystems;

namespace UBlockly.UGUI 
{
	public class UIEventListener : EventTrigger
	{
		public System.Action onClick;
	
		public System.Action<PointerEventData> onPointerDown;
		public System.Action<PointerEventData> onPointerEnter;
		public System.Action<PointerEventData> onPointerExit;
		public System.Action<PointerEventData> onPointerUp;

		public System.Action<PointerEventData> onBeginDrag;
		public System.Action<PointerEventData> onEndDrag;
		public System.Action<PointerEventData> onDrag;

		public System.Action<bool> onValueChanged;

		public static UIEventListener Get(GameObject go)
		{
			UIEventListener listener = go.GetComponent<UIEventListener>();
			if (listener == null) listener = go.AddComponent<UIEventListener>();

			return listener;
		}

		public override void OnPointerClick(PointerEventData eventData)
		{
			if (onClick != null) onClick();
		}
		public override void OnPointerDown(PointerEventData eventData)
		{
			if (onPointerDown != null) onPointerDown(eventData);
		}
		public override void OnPointerEnter(PointerEventData eventData)
		{
			if (onPointerEnter != null) onPointerEnter(eventData);
		}
		public override void OnPointerExit(PointerEventData eventData)
		{
			if (onPointerExit != null) onPointerExit(eventData);
		}
		public override void OnPointerUp(PointerEventData eventData)
		{
			if (onPointerUp != null) onPointerUp(eventData);
		}
		public override void OnBeginDrag(PointerEventData eventData)
		{
			if (onBeginDrag != null) onBeginDrag(eventData);
		}
		public override void OnEndDrag(PointerEventData eventData)
		{
			if (onEndDrag != null) onEndDrag(eventData);
		}
		public override void OnDrag(PointerEventData eventData) 
		{
			if (onDrag != null) onDrag(eventData);
		}

	    void OnDestroy()
	    {
	        onClick = null;

	        onPointerDown = null;
	        onPointerEnter = null;
	        onPointerExit = null;
	        onPointerUp = null;

	        onBeginDrag = null;
	        onEndDrag = null;
	        onDrag = null;

	        onValueChanged = null;
	    }
	}
}
