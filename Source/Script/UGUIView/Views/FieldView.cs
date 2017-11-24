using UnityEngine;

namespace UBlockly.UGUI
{
    public abstract class FieldView : BaseView
    {
        public override ViewType Type
        {
            get { return ViewType.Field; }
        }

        protected Field mField;
        public Field Field { get { return mField; } }
        
        protected BlockView mSourceBlockView;
        public BlockView SourceBlockView
        {
            get { return mSourceBlockView; }
        }

        private MemorySafeFieldObserver mFieldObserver;
        
        public void BindModel(Field field)
        {
            if (mField == field) return;
            if (mField != null) UnBindModel();
            
            mField = field;
            OnBindModel();

            if (Application.isPlaying)
            {
                if (mField.SourceBlock != null)
                    mSourceBlockView = BlocklyUI.WorkspaceView.GetBlockView(mField.SourceBlock);
                
                mFieldObserver = new MemorySafeFieldObserver(this);
                mField.AddObserver(mFieldObserver);

                //register touch event after model is set and data is ready
                RegisterTouchEvent();
            }
        }

        public void UnBindModel()
        {
            if (mField == null) return;
            
            mField.RemoveObserver(mFieldObserver);
            OnUnBindModel();
            mField = null;
        }

        public sealed override void InitComponents()
        {
            base.InitComponents();
            SetComponents();
        }

        /*protected Vector2 ValidateSize(Vector2 size)
        {
            size.x = Mathf.Max(BlockViewSettings.Get().MinUnitWidth, size.x);
            return size;
        }*/

        protected abstract void SetComponents();
        protected abstract void OnBindModel();
        protected abstract void OnUnBindModel();
        
        /// <summary>
        /// Register ui touch event on this field, subclasses must override this.
        /// </summary>
        protected abstract void RegisterTouchEvent();

        /// <summary>
        /// Called after the underlying field's value changed
        /// Subclasses override this to update UI content and layout
        /// </summary>
        protected abstract void OnValueChanged(string newValue);
        
        private class MemorySafeFieldObserver : IObserver<string>
        {
            private FieldView mViewRef;

            public MemorySafeFieldObserver(FieldView viewRef)
            {
                mViewRef = viewRef;
            }

            public void OnUpdated(object field, string newValue)
            {
                if (mViewRef == null || mViewRef.ViewTransform == null || mViewRef.Field != field)
                    ((Field) field).RemoveObserver(this);
                else
                    mViewRef.OnValueChanged(newValue);
            }
        }
    }
}
