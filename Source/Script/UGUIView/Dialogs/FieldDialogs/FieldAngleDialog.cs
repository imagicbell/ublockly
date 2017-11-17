﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UBlockly.UGUI
{
    public class FieldAngleDialog : FieldDialog
    {
        [SerializeField] private Image m_ImgPointer;

        private Camera mCamera;
        private RectTransform mRotateAnchor;

        protected override void OnInit()
        {
            int angle;
            string angleStr = mField.GetValue();
            if (!int.TryParse(angleStr, out angle))
                angle = (int) float.Parse(angleStr);

            Rotate(ValidateAngle(angle));
            
            AddCloseEvent(() =>
            {
                int imgAngle = (int)m_ImgPointer.rectTransform.localRotation.eulerAngles.z;
                imgAngle = ValidateAngle(imgAngle);
                mField.SetValue(imgAngle.ToString());
            });

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
                angle = 90 - angle;
            return angle;
        }

        private void Rotate(int angle)
        {
            m_ImgPointer.rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
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
            
            int angleDegree = (int) (angle * Mathf.Rad2Deg);
            Rotate(angleDegree);
        }
    }
}