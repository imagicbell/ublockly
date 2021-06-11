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

        private Stack<IEnumerator> callstack = new Stack<IEnumerator>();

        private void PushCall(IEnumerator call)
        {
            callstack.Push(call);
            if (call is CmdEnumerator)
            {
                Debug.Log(">>>>>enter + " + ((CmdEnumerator) call).Block.Type);
                CSharp.Runner.FireUpdate(new RunnerUpdateState(RunnerUpdateState.RunBlock, ((CmdEnumerator) call).Block));
            }
        }

        private void PopCall()
        {
            var call = callstack.Pop();
            if (call is CmdEnumerator)
            {
                Debug.Log(">>>>>exit + " + ((CmdEnumerator) call).Block.Type);
                CSharp.Runner.FireUpdate(new RunnerUpdateState(RunnerUpdateState.FinishBlock, ((CmdEnumerator) call).Block));
            }
        }

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

            callstack.Clear();
            PushCall(entryCall);

            Debug.LogFormat("<color=green>[CodeRunner - {0}]: begin - time: {1}.</color>", gameObject.name, Time.time);

            //step mode: wait until Step() calls
            if (RunMode != Runner.Mode.Step)
                StartCoroutine(Run());
        }

        /// <summary>
        /// api - step over to next block in debug mode
        /// </summary>
        public void Step()
        {
            if (RunMode == Runner.Mode.Step)
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
                callstack.Clear();
                curStatus = Runner.Status.Stop;
            }
        }

        /// <summary>
        /// Simulate coroutine execution, replacing Unity's,
        /// in case that nestes IEnumerator call brings one more frame delay.
        /// </summary>
        IEnumerator Run()
        {
            while (callstack.Count > 0)
            {
                IEnumerator itor = callstack.Peek();

                bool finished = true;
                while (itor.MoveNext())
                {
                    if (itor.Current is IEnumerator)
                    {
                        IEnumerator current = itor.Current as IEnumerator;
                        PushCall(current);
                        if (RunMode == Runner.Mode.Step && (current is CmdEnumerator))
                        {
                            yield break;
                        }

                        finished = false;
                        break;
                    }

                    yield return itor.Current;
                }

                if (!finished) continue;
                PopCall();

                if (itor is CmdEnumerator)
                {
                    //exit point of block

                    //push next block
                    CmdEnumerator next = ((CmdEnumerator) itor).GetNextCmd();
                    if (next != null)
                    {
                        PushCall(next);
                    }

                    if (RunMode == Runner.Mode.Step)
                    {
                        break;
                    }
                }

                if (curStatus == Runner.Status.Pause || curStatus == Runner.Status.Stop)
                    break;
            }

            if (curStatus == Runner.Status.Stop)
            {
                callstack.Clear();
            }

            if (callstack.Count == 0)
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
            IEnumerator[] calls = callstack.ToArray();
            for (int i = calls.Length - 1; i >= 0; i--)
            {
                if (calls[i] is CmdEnumerator)
                    blocks.Add(((CmdEnumerator) calls[i]).Block.Type);
            }
            return blocks;
        }
    }
}