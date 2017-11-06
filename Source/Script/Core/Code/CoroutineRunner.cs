using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PTGame.Framework;

namespace PTGame.Blockly
{
    /// <summary>
    /// responsible for simulating coroutines for code commands
    /// </summary>
    public class CoroutineRunner : PTMonoSingleton<CoroutineRunner>
    {
        internal struct CoroutineStruct
        {
            internal Coroutine coroutine;
            internal bool paused;

            public CoroutineStruct(Coroutine coroutine, bool paused)
            {
                this.coroutine = coroutine;
                this.paused = paused;
            }
        }

        private Dictionary<IEnumerator, CoroutineStruct> mCoroutineDict = new Dictionary<IEnumerator, CoroutineStruct>();

        protected override void OnDestroy()
        {
            //coroutines will be stopped on destroy by Unity
            mCoroutineDict.Clear();
            base.OnDestroy();
        }

        /// <summary>
        /// Start a code coroutine process
        /// </summary>
        /// <param name="itorFunc"></param>
        public bool StartProcess(IEnumerator itorFunc)
        {
            CoroutineStruct value;
            if (mCoroutineDict.TryGetValue(itorFunc, out value))
            {
                Debug.LogWarningFormat("<color=magenta>[CodeRunner]Process {0} has already started. Need to stop before restart.</color>", itorFunc);
                StopCoroutine(value.coroutine);
            }

            Debug.LogFormat("<color=green>[CodeRunner]Start process {0}.</color>", itorFunc);
            
            value = new CoroutineStruct(StartCoroutine(SimulateCoroutine(itorFunc)), false);
            mCoroutineDict[itorFunc] = value;
            return true;
        }

        /// <summary>
        /// Stop a code coroutine process
        /// </summary>
        /// <param name="itorFunc"></param>
        public bool StopProcess(IEnumerator itorFunc)
        {
            CoroutineStruct value;
            if (mCoroutineDict.TryGetValue(itorFunc, out value))
            {
                StopCoroutine(value.coroutine);
                mCoroutineDict.Remove(itorFunc);

                Debug.LogFormat("<color=green>[CodeRunner]Stop process {0}.</color>", itorFunc);
                return true;
            }

            Debug.LogWarningFormat("<color=magenta>[CodeRunner]Stop process {0}, but it was not started.</color>", itorFunc);
            return false;
        }

        /// <summary>
        /// Pause a code coroutine process
        /// </summary>
        /// <param name="itorFunc"></param>
        public bool PauseProcess(IEnumerator itorFunc)
        {
            if (!mCoroutineDict.ContainsKey(itorFunc))
            {
                Debug.LogWarningFormat("<color=magenta>[CodeRunner]Pause process {0}, but it was not started.</color>", itorFunc);
                return false;
            }

            Debug.LogFormat("<color=green>[CodeRunner]Pause process {0}.</color>", itorFunc);
            
            CoroutineStruct value = mCoroutineDict[itorFunc];
            value.paused = true;
            mCoroutineDict[itorFunc] = value;
            return true;
        }

        /// <summary>
        /// Resume a code coroutine process
        /// </summary>
        /// <param name="itorFunc"></param>
        /// <returns></returns>
        public bool ResumeProcess(IEnumerator itorFunc)
        {
            if (!mCoroutineDict.ContainsKey(itorFunc))
            {
                Debug.LogWarningFormat("<color=magenta>[CodeRunner]Resume process {0}, but it was not started.</color>", itorFunc);
                return false;
            }

            Debug.LogFormat("<color=green>[CodeRunner]Resume process {0}.</color>", itorFunc);
            
            CoroutineStruct value = mCoroutineDict[itorFunc];
            value.paused = false;
            mCoroutineDict[itorFunc] = value;
            return true;
        }

        /// <summary>
        /// Simulate coroutine execution, replacing Unity's,
        /// in case that nestes IEnumerator call brings one more frame delay.
        /// </summary>
        /// <param name="itorFunc"></param>
        /// <returns></returns>
        IEnumerator SimulateCoroutine(IEnumerator itorFunc)
        {
            Debug.LogFormat("<color=green>[CodeRunner]SimulateCoroutine: begin - time: {0}.</color>", Time.time);

            Stack<IEnumerator> stack = new Stack<IEnumerator>();
            stack.Push(itorFunc);

            while (stack.Count > 0)
            {
                IEnumerator itor = stack.Peek();
                bool finished = true;
                while (itor.MoveNext())
                {
                    //Debug.LogFormat("<color=green>[CodeRunner]SimulateCoroutine: current - {0}, time: {1}</color>", itor.Current, Time.time);
                    if (itor.Current is IEnumerator)
                    {
                        stack.Push((IEnumerator) itor.Current);
                        finished = false;
                        break;
                    }
                    
                    yield return itor.Current;

                    //pause
                    while (mCoroutineDict[itorFunc].paused)
                        yield return null;
                }

                if (finished)
                {
                    stack.Pop();
                }
            }

            mCoroutineDict.Remove(itorFunc);
            Debug.LogFormat("<color=green>[CodeRunner]SimulateCoroutine: end - time: {0}.</color>", Time.time);
        }
    }
}