using System.Collections.Generic;

namespace PTGame.Blockly
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