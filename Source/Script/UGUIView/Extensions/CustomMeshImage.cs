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

using System.Collections.Generic;
using UnityEngine.Sprites;
using UnityEngine;
using UnityEngine.UI;

namespace UBlockly.UGUI
{
    public class CustomMeshImage : Image
    {
        /// <summary>
        /// local draw dimensions
        /// </summary>
        [SerializeField] private Vector4[] m_DrawDimensions;

        private float[] mVert_X = new float[4];
        private float[] mVert_Y = new float[4];
        private float[] mUV_X = new float[4];
        private float[] mUV_Y = new float[4];

        /// <summary>
        /// Set local draw dimensions
        /// </summary>
        public void SetDrawDimensions(Vector4[] dimensions)
        {
            if (dimensions == null) 
                return;
            
            if (m_DrawDimensions == null)
            {
                m_DrawDimensions = dimensions;
                SetVerticesDirty();
                return;
            }

            if (m_DrawDimensions.Length == dimensions.Length)
            {
                bool same = true;
                for (int i = 0; i < dimensions.Length; i++)
                {
                    if (m_DrawDimensions[i] != dimensions[i])
                    {
                        same = false;
                        break;
                    }
                }
                if (same)
                    return;
            }
                
            m_DrawDimensions = dimensions;
            SetVerticesDirty();
        }
        
        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            if (m_DrawDimensions == null || m_DrawDimensions.Length == 0)
            {
                base.OnPopulateMesh(toFill);
                return;
            }
            
            toFill.Clear();

            Vector4 outerUV;
            Vector4 innerUV;
            Vector4 border;
            if ((UnityEngine.Object) this.overrideSprite != (UnityEngine.Object) null)
            {
                outerUV = DataUtility.GetOuterUV(this.overrideSprite);
                innerUV = DataUtility.GetInnerUV(this.overrideSprite);
                border = this.overrideSprite.border / this.pixelsPerUnit;
            }
            else
            {
                outerUV = Vector4.zero;
                innerUV = Vector4.zero;
                border = Vector4.zero;
            }

            Rect pixelAdjustedRect = this.GetPixelAdjustedRect();
            //Vector4 adjustedBorders = this.GetAdjustedBorders(border, pixelAdjustedRect);

//                Debug.Log(">>>>>>  this.GetPixelAdjustedRect():  " + pixelAdjustedRect);
//                Debug.Log(">>>>>>  DataUtility.GetOuterUV(this.overrideSprite): " + outerUV);
//                Debug.Log(">>>>>>  DataUtility.GetInnerUV(this.overrideSprite): " + innerUV);
//                Debug.Log(">>>>>>  border: " + border);
//                for (int i = 0; i < m_DrawDimensions.Length; i++)
//                {
//                    Debug.Log(">>>>>>  m_DrawDimensions " + i + ": " + m_DrawDimensions[i]);
//                }

            RectTransform rectTrans = GetComponent<RectTransform>();
            float xFactor = pixelAdjustedRect.width / rectTrans.rect.width;
            float yFactor = pixelAdjustedRect.height / rectTrans.rect.height;
            Vector4 factor = new Vector4(xFactor, yFactor, xFactor, yFactor);

            for (int i = 0; i < m_DrawDimensions.Length; i++)
            {
                Vector4 dimension = m_DrawDimensions[i];
                dimension.Scale(factor);

                int xCount = 3;
                int yCount;

                mVert_X[0] = dimension.x;
                mVert_X[1] = dimension.x + border.x;
                mVert_X[2] = dimension.z - border.z;
                mVert_X[3] = dimension.z;

                mUV_X[0] = outerUV.x;
                mUV_X[1] = innerUV.x;
                mUV_X[2] = innerUV.z;
                mUV_X[3] = outerUV.z;

                if (m_DrawDimensions.Length == 1)
                {
                    //9 quads
                    mVert_Y[0] = dimension.y;
                    mVert_Y[1] = dimension.y + border.y;
                    mVert_Y[2] = dimension.w - border.w;
                    mVert_Y[3] = dimension.w;

                    mUV_Y[0] = outerUV.y;
                    mUV_Y[1] = innerUV.y;
                    mUV_Y[2] = innerUV.w;
                    mUV_Y[3] = outerUV.w;

                    yCount = 3;
                }
                else
                {
                    if (i == 0)
                    {
                        //6 quads
                        mVert_Y[0] = dimension.y;
                        mVert_Y[1] = dimension.w - border.w;
                        mVert_Y[2] = dimension.w;

                        mUV_Y[0] = innerUV.y;
                        mUV_Y[1] = innerUV.w;
                        mUV_Y[2] = outerUV.w;

                        yCount = 2;
                    }
                    else if (i == m_DrawDimensions.Length - 1)
                    {
                        //6 quads
                        mVert_Y[0] = dimension.y;
                        mVert_Y[1] = dimension.y + border.y;
                        mVert_Y[2] = dimension.w;

                        mUV_Y[0] = outerUV.y;
                        mUV_Y[1] = innerUV.y;
                        mUV_Y[2] = innerUV.w;

                        yCount = 2;
                    }
                    else
                    {
                        //3 quads
                        mVert_Y[0] = dimension.y;
                        mVert_Y[1] = dimension.w;

                        mUV_Y[0] = innerUV.y;
                        mUV_Y[1] = innerUV.w;

                        yCount = 1;
                    }
                }

                Vector4 dim, uv;
                for (int yMin = 0; yMin < yCount; yMin++)
                {
                    int yMax = yMin + 1;
                    for (int xMin = 0; xMin < xCount; xMin++)
                    {
                        int xMax = xMin + 1;
                        dim = new Vector4(mVert_X[xMin], mVert_Y[yMin], mVert_X[xMax], mVert_Y[yMax]);
                        uv = new Vector4(mUV_X[xMin], mUV_Y[yMin], mUV_X[xMax], mUV_Y[yMax]);
                        AddQuad(toFill, dim, uv);
                    }
                }
            }
        }
        
        private void AddQuad(VertexHelper vertexHelper, Vector4 pos, Vector4 uv)
        {
            int currentVertCount = vertexHelper.currentVertCount;
            Color32 c = (Color32) this.color;
            vertexHelper.AddVert(new Vector3(pos.x, pos.y, 0.0f), c, new Vector2(uv.x, uv.y));
            vertexHelper.AddVert(new Vector3(pos.x, pos.w, 0.0f), c, new Vector2(uv.x, uv.w));
            vertexHelper.AddVert(new Vector3(pos.z, pos.w, 0.0f), c, new Vector2(uv.z, uv.w));
            vertexHelper.AddVert(new Vector3(pos.z, pos.y, 0.0f), c, new Vector2(uv.z, uv.y));
            vertexHelper.AddTriangle(currentVertCount, currentVertCount + 1, currentVertCount + 2);
            vertexHelper.AddTriangle(currentVertCount + 2, currentVertCount + 3, currentVertCount);
        }
        
        private Vector4 GetAdjustedBorders(Vector4 border, Rect rect)
        {
            for (int index = 0; index <= 1; ++index)
            {
                float num1 = border[index] + border[index + 2];
                if ((double) rect.size[index] < (double) num1 && (double) num1 != 0.0)
                {
                    float num2 = rect.size[index] / num1;
                    border[index] *= num2;
                    border[index + 2] *= num2;
                }
            }
            return border;
        }
    }
}
