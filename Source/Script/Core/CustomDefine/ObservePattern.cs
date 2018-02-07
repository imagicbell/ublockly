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

namespace UBlockly
{
    /// <summary>
    /// Customed Observable/Observer pattern
    /// </summary>
    public interface IObserver<in TArgs>
    {
        void OnUpdated(object subject, TArgs args);
    }
    
    /// <summary>
    /// Customed Observable/Observer pattern
    /// </summary>
    public abstract class Observable<TArgs>
    {
        private readonly List<IObserver<TArgs>> mObservers = new List<IObserver<TArgs>>();
        
        public void AddObserver(IObserver<TArgs> observer)
        {
            if (!mObservers.Contains(observer))
                mObservers.Add(observer);
        }

        public void RemoveObserver(IObserver<TArgs> observer)
        {
            mObservers.Remove(observer);
        }

        public void FireUpdate(TArgs args)
        {
            for (int i = mObservers.Count - 1; i >= 0; i--)
            {
                mObservers[i].OnUpdated(this, args);
            }
        }
    }
}
