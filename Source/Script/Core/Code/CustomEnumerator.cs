using System.Collections;

namespace PTGame.Blockly
{
    /// <summary>
    /// custom inplementation of IEnumerator for carrying data
    /// </summary>
    public class CustomEnumerator : IEnumerator
    {
        private IEnumerator mItor;
        
        public Cmdtor Cmdtor { get; set; }
        public DataStruct Data { get { return Cmdtor.Data; } }

        public CustomEnumerator(IEnumerator itor)
        {
            mItor = itor;
        }

        public bool MoveNext()
        {
            return mItor.MoveNext();
        }

        public void Reset()
        {
            mItor = null;
        }

        public object Current
        {
            get { return mItor.Current; }
        }
    }
}