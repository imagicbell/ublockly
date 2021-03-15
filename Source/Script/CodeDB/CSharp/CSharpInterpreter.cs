/****************************************************************************

Helper functions for interpreting C# for blocks.

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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UBlockly
{
    public partial class CSharpInterpreter : Interpreter
    {
        public override CodeName Name
        {
            get { return CodeName.CSharp; }
        }

        private readonly CoroutineRunner mRunner;
        private readonly Names mVariableNames;
        private readonly Datas mVariableDatas;

        private IEnumerator mRunningProcess;

        public CSharpInterpreter(Names variableNames, Datas variableDatas)
        {
            mVariableNames = variableNames;
            mVariableDatas = variableDatas;
            mRunner = CoroutineRunner.Create("CodeRunner", true);
        }

        public override void Run(Workspace workspace)
        {
            mVariableNames.Reset();
            mVariableDatas.Reset();

            mRunningProcess = RunWorkspace(workspace);
            mRunner.StartProcess(mRunningProcess);
        }

        public override void Pause()
        {
            if (mRunningProcess != null)
            {
                mRunner.PauseProcess(mRunningProcess);
                CSharp.Interpreter.FireUpdate(new InterpreterUpdateState(InterpreterUpdateState.Pause));
            }
        }

        public override void Resume()
        {
            if (mRunningProcess != null)
            {
                mRunner.ResumeProcess(mRunningProcess);
                CSharp.Interpreter.FireUpdate(new InterpreterUpdateState(InterpreterUpdateState.Resume));
            }
        }

        public override void Stop()
        {
            if (mRunningProcess != null)
            {
                mRunner.StopProcess(mRunningProcess);
                mRunningProcess = null;
                CSharp.Interpreter.FireUpdate(new InterpreterUpdateState(InterpreterUpdateState.Stop));
            }
        }

        public override void Error(string msg)
        {
            if (mRunningProcess != null)
            {
                mRunner.StopProcess(mRunningProcess);
                mRunningProcess = null;
                CSharp.Interpreter.FireUpdate(new InterpreterUpdateState(InterpreterUpdateState.Error, msg));
            }
        }

        /// <summary>
        /// coroutine run code for workspace
        /// todo: execute topblocks in order or synchronously
        /// </summary>
        IEnumerator RunWorkspace(Workspace workspace)
        {
            yield return null;
            
            //traverse all blocks in the workspace and run code for the blocks
            List<Block> blocks = workspace.GetTopBlocks(true);
            foreach (Block block in blocks)
            {
                //exclude the procedure definition blocks
                if (ProcedureDB.IsDefinition(block))
                    continue;
                
                yield return RunBlock(block);
            }
            
            mRunningProcess = null;
            CSharp.Interpreter.FireUpdate(new InterpreterUpdateState(InterpreterUpdateState.Stop));
        }
        
        /// <summary>
        /// run the block in a coroutine way
        /// </summary>
        IEnumerator RunBlock(Block block)
        {
            //check if stopped
            if (mRunningProcess == null)
            {
                yield break;
            }
            
            //check flow 
            if (ControlCmdtor.SkipRunByControlFlow(block))
            {
                yield break;
            }

            if (!block.Disabled)
            {
                CSharp.Interpreter.FireUpdate(new InterpreterUpdateState(InterpreterUpdateState.RunBlock, block));
                yield return GetBlockInterpreter(block).Run(block);
                CSharp.Interpreter.FireUpdate(new InterpreterUpdateState(InterpreterUpdateState.FinishBlock, block));
            }
            
            if (block.NextBlock != null)
                yield return RunBlock(block.NextBlock);
        }
        
        /// <summary>
        /// run code representing the specified value input.
        /// should return a DataStruct
        /// </summary>
        public CustomEnumerator ValueReturn(Block block, string name)
        {
            var targetBlock = block.GetInputTargetBlock(name);
            if (targetBlock == null)
            {
                Debug.Log(string.Format("Value input block of {0} is null", block.Type));
                return new CustomEnumerator(null);
            }
            if (targetBlock.OutputConnection == null)
            {
                Debug.Log(string.Format("Value input block of {0} must have an output connection", block.Type));
                return new CustomEnumerator(null);
            }

            CustomEnumerator etor = new CustomEnumerator(RunBlock(targetBlock));
            etor.Cmdtor = GetBlockInterpreter(targetBlock);
            return etor;
        }

        /// <summary>
        /// run code representing the specified value input. WITH a default DataStruct
        /// </summary>
        public CustomEnumerator ValueReturn(Block block, string name, DataStruct defaultData)
        {
            CustomEnumerator etor = ValueReturn(block, name);
            etor.Cmdtor.DefaultData = defaultData;
            return etor;
        }

        /// <summary>
        /// Run code representing the statement.
        /// </summary>
        public IEnumerator StatementRun(Block block, string name)
        {
            var targetBlock = block.GetInputTargetBlock(name);
            if (targetBlock == null)
            {
                Debug.Log(string.Format("Statement input block of {0} is null", block.Type));
                yield break;
            }
            if (targetBlock.PreviousConnection == null)
            {
                Debug.Log(string.Format("Statement input block of {0} must have a previous connection", block.Type));
                yield break;
            }

            yield return RunBlock(targetBlock);
        }

        public Cmdtor GetBlockInterpreter(Block block)
        {
            Cmdtor cmdtor;
            if (!mCmdMap.TryGetValue(block.Type, out cmdtor))
                throw new Exception(string.Format("Language {0} does not know how to interprete code for block type {1}.", Name, block.Type));
            return cmdtor;
        }
    }
}
