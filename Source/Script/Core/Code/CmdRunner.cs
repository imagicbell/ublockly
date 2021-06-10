using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UBlockly
{
    public class CmdRunner : MonoBehaviour
    {
        public static CmdRunner Create(string runnerName, bool dontDestroyOnLoad = false)
        {
            GameObject parentObj = GameObject.Find("CodeRunners");
            if (parentObj == null)
            {
                parentObj = new GameObject("CodeRunners");
                GameObject.DontDestroyOnLoad(parentObj);
            } 
            GameObject runnerObj = new GameObject(runnerName);
            if (dontDestroyOnLoad)
            {
                GameObject.DontDestroyOnLoad(runnerObj);
            }
            runnerObj.transform.parent = parentObj.transform;
            return runnerObj.AddComponent<CmdRunner>();
        }
        
        public Runner.Mode RunMode = Runner.Mode.Normal;

        private Runner.Status curStatus = Runner.Status.Stop;
        public Runner.Status CurStatus { get { return curStatus; } } 

        private Stack<CmdEnumerator> callStack = new Stack<CmdEnumerator>();
        private Stack<IEnumerator> itorStack = new Stack<IEnumerator>();

        private Action finishCb = null;

        public void SetFinishCallback(Action callback)
        {
            finishCb = callback;
        }

        /// <summary>
        /// api - start running code
        /// </summary>
        public void StartRun(CmdEnumerator entryCall)
        {
            curStatus = Runner.Status.Running;
            
            itorStack.Clear();
            callStack.Clear();
            itorStack.Push(entryCall);

            Debug.LogFormat("<color=green>[CodeRunner - {0}]: begin - time: {1}.</color>", gameObject.name, Time.time);

            StartCoroutine(Run());
        }

        /// <summary>
        /// api - step over to next block in debug mode
        /// </summary>
        public void Step()
        {
            if (RunMode != Runner.Mode.Step)
                return;

            StartCoroutine(Run());
        }

        /// <summary>
        /// api - pause running code
        /// </summary>
        public void Pause()
        {
            if (RunMode == Runner.Mode.Step)
                return;

            curStatus = Runner.Status.Pause;
        }

        /// <summary>
        /// api - resume running code
        /// </summary>
        public void Resume()
        {
            if (RunMode == Runner.Mode.Step)
                return;

            curStatus = Runner.Status.Running;
            StartCoroutine(Run());
        }

        /// <summary>
        /// api - stop running code
        /// </summary>
        public void Stop()
        {
            if (curStatus == Runner.Status.Running)
            {
                curStatus = Runner.Status.Stop;
            }
            else if (curStatus == Runner.Status.Pause)
            {
                itorStack.Clear();
                callStack.Clear();
                curStatus = Runner.Status.Stop;
            }
        }

        /// <summary>
        /// Simulate coroutine execution, replacing Unity's,
        /// in case that nestes IEnumerator call brings one more frame delay.
        /// </summary>
        IEnumerator Run()
        {
            while (itorStack.Count > 0)
            {
                IEnumerator itor = itorStack.Peek();

                if (itor is CmdEnumerator)
                {
                    //entry point of running a block (RunBlock)
                    callStack.Push((CmdEnumerator) itor);
                }

                bool finished = true;
                while (itor.MoveNext())
                {
                    if (itor.Current is IEnumerator)
                    {
                        itorStack.Push((IEnumerator) itor.Current);
                        finished = false;
                        break;
                    }

                    yield return itor.Current;
                }

                if (!finished) continue;
                itorStack.Pop();

                if (itor is CmdEnumerator)
                {
                    //exit point of running a block
                    callStack.Pop();

                    //push next block
                    CmdEnumerator next = ((CmdEnumerator) itor).GetNextCmd();
                    if (next != null)
                    {
                        itorStack.Push(next);
                    }
                    
                    if (RunMode == Runner.Mode.Step || curStatus == Runner.Status.Pause || curStatus == Runner.Status.Stop)
                    {
                        break;
                    }
                }
            }

            if (curStatus == Runner.Status.Stop)
            {
                itorStack.Clear();
                callStack.Clear();
            }

            if (itorStack.Count == 0)
            {
                Debug.LogFormat("<color=green>[CodeRunner - {0}]: end - time: {1}.</color>", gameObject.name, Time.time);
                if (curStatus != Runner.Status.Stop)
                {
                    finishCb?.Invoke();
                }
                curStatus = Runner.Status.Stop;
            }
        }

        /// <summary>
        /// get current callstack
        /// </summary>
        public List<string> GetCallStack()
        {
            List<string> blocks = new List<string>();
            foreach (CmdEnumerator itor in callStack)
            {
                blocks.Add(itor.Block.Type);
            }
            blocks.Reverse();
            return blocks;
        }
    }
}