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
using UnityEngine.UI;

namespace UBlockly.UGUI
{
    public class FieldAngleDialog : FieldDialog
    {
        [SerializeField] private Image m_ImgPointer;
        [SerializeField] private Text m_LabelAngle;

        private FieldAngle mFieldAngle
        {
            get { return mField as FieldAngle; }
        }
        
        private Camera mCamera;
        private RectTransform mRotateAnchor;
        private int mOriAngle;
        private int mGap;

        protected override void OnInit()
        {
            string angleStr = mField.GetValue();
            if (!int.TryParse(angleStr, out mOriAngle))
                mOriAngle = Mathf.RoundToInt(float.Parse(angleStr));

            mOriAngle = ValidateAngle(mOriAngle);
            Rotate(mOriAngle);

            mGap = Mathf.RoundToInt(mFieldAngle.Gap.Value);
            
            AddCloseEvent(() =>
            {
                int imgAngle = Mathf.RoundToInt(m_ImgPointer.rectTransform.localRotation.eulerAngles.z);
                imgAngle = ValidateAngle(imgAngle);
                mField.SetValue(imgAngle.ToString());
            });

            UIEventListener.Get(m_ImgPointer.gameObject).onBeginDrag = OnDragPointer;
            UIEventListener.Get(m_ImgPointer.gameObject).onDrag = OnDragPointer;
            UIEventListener.Get(m_ImgPointer.gameObject).onEndDrag = OnDragPointer;

            mCamera = GetComponentInParent<Canvas>().worldCamera;
            mRotateAnchor = m_ImgPointer.transform.parent.transform as RectTransform;
        }

        /// <summary>
        /// Unity is based on Anticlockwise/(1,0,0)
        /// </summary>
        private int ValidateAngle(int angle)
        {
            if (Define.FIELD_ANGLE_CLOCKWISE)
            {
                angle = 90 - angle;
                if (angle < 0)
                    angle += 360;
            }
            return angle;
        }

        private void Rotate(int angle)
        {
            m_ImgPointer.rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
            m_LabelAngle.text = ValidateAngle(angle).ToString();
        }

        private void OnDragPointer(PointerEventData evtData)
        {
            Vector2 localPoint;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(mRotateAnchor,
                UnityEngine.Input.mousePosition, mCamera, out localPoint))
                return;
            
            localPoint.Normalize();
            Vector3 dir = new Vector3(localPoint.x, localPoint.y, 0);
            Vector3 offset = Vector3.right;
            float angle = Mathf.Acos(Vector3.Dot(offset, dir));
            Vector3 normal = Vector3.Cross(offset, dir);
            if (normal.z < 0)
                angle = -angle;

            //don't want to call mField.CallValidator(), as it need to transfer to string first
            int angleDegree = Mathf.RoundToInt(angle * Mathf.Rad2Deg) % 360;
            if (angleDegree < 0)
                angleDegree += 360;
            
            //consider gap
            if (mFieldAngle.Gap.Value > 0)
            {
                int interval = (angleDegree - mOriAngle) / mGap;
                angleDegree = mOriAngle + interval * mGap;
            }

            Rotate(angleDegree);
        }
    }
}
