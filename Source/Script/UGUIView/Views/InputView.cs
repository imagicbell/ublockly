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
using UnityEngine;

namespace UBlockly.UGUI
{
    public class InputView : BaseView
    {
        [SerializeField] private bool m_AlignRight = false;
        
        public bool AlignRight
        {
            get { return m_AlignRight; }
            set { m_AlignRight = value; }
        }
        
        public override ViewType Type
        {
            get { return ViewType.Input; }
        }

        private Input mInput;
        public Input Input { get { return mInput; } }

        /// <summary>
        /// Check if this input group has a connection
        /// return false when it is a dummy input
        /// </summary>
        public bool HasConnection
        {
            get { return Childs[Childs.Count - 1] is ConnectionInputView; }
        }

        /// <summary>
        /// Get the connection point view for this input group
        /// maybe null if it is a dummy input
        /// </summary>
        public ConnectionInputView GetConnectionView()
        {
            return Childs.Count > 0 ? Childs[Childs.Count - 1] as ConnectionInputView : null;
        }

        public void BindModel(Input input)
        {
            if (mInput == input) return;
            if (mInput != null) UnBindModel();

            mInput = input;

            for (int i = 0; i < Childs.Count; i++)
            {
                var view = Childs[i];
                if (view.Type == ViewType.Field)
                    ((FieldView) view).BindModel(mInput.FieldRow[i]);
                else if (view.Type == ViewType.ConnectionInput)
                    ((ConnectionInputView) view).BindModel(mInput.Connection);
            }
        }

        public void UnBindModel()
        {
            for (int i = 0; i < Childs.Count; i++)
            {
                var view = Childs[i];
                if (view.Type == ViewType.Field)
                    ((FieldView) view).UnBindModel();
                else if (view.Type == ViewType.ConnectionInput)
                    ((ConnectionInputView) view).UnBindModel();
            }
            mInput = null;
        }

        protected override Vector2 CalculateSize()
        {
            //accumulate size of all fields and input connection slot
            Vector2 size = Vector2.zero;
            for (int i = 0; i < Childs.Count; i++)
            {
                //calculate x: get the last child's right
                if (i == Childs.Count - 1)
                {
                    size.x = Childs[i].XY.x + Childs[i].Width;
                }
                size.y = Mathf.Max(size.y, Childs[i].Height);
            }
            return size;
        }
    }
}
