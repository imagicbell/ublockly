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
using UnityEngine;

namespace UBlockly
{
    /// <summary>
    /// code interpreter base class
    /// interpreters for blocks should implement 3 subclasses of this
    /// 1. ValueCmdtor
    /// 2. VoidCmdtor
    /// 3. EnumeratorCmdtor
    /// </summary>
    public abstract class Cmdtor
    {
        protected DataStruct mData;
        public DataStruct Data { get { return mData; } }

        private DataStruct mDefaultData;
        public DataStruct DefaultData { set { mDefaultData = value; } }

        public IEnumerator Run(Block block)
        {
            Reset();
            yield return OnRun(block);

            if (mData.IsUndefined && !mDefaultData.IsUndefined)
                mData = mDefaultData;
        }

        public void Reset()
        {
            if (!mData.IsUndefined)
                mData = DataStruct.Undefined;
        }
        
        protected abstract IEnumerator OnRun(Block block);
    }

    /// <summary>
    /// execution of block's intepreter returns value
    /// </summary>
    public abstract class ValueCmdtor : Cmdtor
    {
        protected sealed override IEnumerator OnRun(Block block)
        {
            // never reached code, just for passing compile
            if (false) yield break;
            
            mData = Execute(block);
        }

        protected abstract DataStruct Execute(Block block);
    }
    
    /// <summary>
    /// execution of block's interpreter returns void 
    /// </summary>
    public abstract class VoidCmdtor : Cmdtor
    {
        protected sealed override IEnumerator OnRun(Block block)
        {
            // never reached code, just for passing compile
            if (false) yield break;
            
            Execute(block);
        }

        protected abstract void Execute(Block block);
    }
    
    /// <summary>
    /// execution of block's interpreter returns IEnumerator 
    /// </summary>
    public abstract class EnumeratorCmdtor : Cmdtor
    {
        protected sealed override IEnumerator OnRun(Block block)
        {
            yield return Execute(block);
        }

        /// <summary>
        /// set the data to return after execution
        /// </summary>
        protected void ReturnData(DataStruct data)
        {
            mData = data;
        }

        protected abstract IEnumerator Execute(Block block);
    }
}
