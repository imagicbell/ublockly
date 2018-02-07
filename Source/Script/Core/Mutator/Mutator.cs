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


using System.Xml;

namespace UBlockly
{
    public abstract class Mutator
    {
        protected Block mBlock;
        public Block Block { get { return mBlock; } }

        private string mMutatorId;
        public string MutatorId
        {
            get { return mMutatorId; }
            set
            {
                // only set once
                if (string.IsNullOrEmpty(mMutatorId))
                    mMutatorId = value;
            }
        }

        /// <summary>
        /// If this mutator need a editor to set mutation parameters
        /// </summary>
        public abstract bool NeedEditor { get; }

        /// <summary>
        /// This is called when a mutator is attached to a block.
        /// </summary>
        public void AttachToBlock(Block block)
        {
            mBlock = block;
            OnAttached();
        }

        /// <summary>
        ///  This is called when a mutator is detached from a block. 
        /// </summary>
        public void DetachFromBlock(Block block)
        {
            OnDetached();
            mBlock = null;
        }
        
        /// <summary>
        /// Called immediately after the mutator is attached to the block. Can be used to perform
        /// additional block initialization related to this mutator.
        /// </summary>
        protected virtual void OnAttached() {}
        
        /// <summary>
        /// Called immediately after the mutator is detached from a block, usually as a result of destroying the block.
        /// </summary>
        protected virtual void OnDetached() {}

        /// <summary>
        /// Serializes the Mutator's state to an XML mutation element.
        /// </summary>
        public abstract XmlElement ToXml();

        /// <summary>
        /// Updates the mutator state from the provided mutation XML.
        /// </summary>
        public abstract void FromXml(XmlElement xmlElement);
        
        
        /// <summary>
        /// mutate on block's fields update
        /// </summary>
        protected virtual void MutateInternal() {}
        
        protected class MemorySafeMutatorObserver : IObserver<string>
        {
            private Mutator mMutatorRef;

            public MemorySafeMutatorObserver(Mutator mutatorRef)
            {
                mMutatorRef = mutatorRef;
            }

            public void OnUpdated(object field, string newValue)
            {
                if (mMutatorRef == null || mMutatorRef.Block == null)
                    ((Observable<string>) field).RemoveObserver(this);
                else
                    mMutatorRef.MutateInternal();
            }
        }
    }
}
