namespace UBlockly.UGUI
{
    public abstract class FieldDialog : BaseDialog
    {
        protected Field mField;
        public Field Field { get { return mField; } }
        
        public void Init(Field field)
        {
            mField = field;
            mBlock = field.SourceBlock;
            Init();
        }
    }
}
