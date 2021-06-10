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

using System.Collections;

namespace UBlockly
{
    /// <summary>
    /// IEnumerator wrapper for running Cmdtor, block code
    /// </summary>
    public class CmdEnumerator : IEnumerator
    {
        private readonly Block mBlock;
        private readonly Cmdtor mCmdtor;
        private IEnumerator mItor;

        public Block Block
        {
            get { return mBlock; }
        }

        public Cmdtor Cmdtor
        {
            get { return mCmdtor; }
        }

        public DataStruct Data
        {
            get { return mCmdtor.Data; }
        }

        public CmdEnumerator(Block block)
        {
            mBlock = block;
            mCmdtor = CSharp.Interpreter.GetBlockInterpreter(block);
            mItor = mCmdtor.Run(block);
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

        /// <summary>
        /// get the next block's running code, connected with previous - next connection
        /// </summary>
        public CmdEnumerator GetNextCmd()
        {
            var nextblock = mBlock.NextBlock;
            if (nextblock == null || nextblock.Disabled)
                return null;

            //parent loop was break or continue, move out. 
            if (LoopCmdtor.SkipRunByControlFlow(nextblock))
                return null;

            return new CmdEnumerator(nextblock);
        }
    }
}
