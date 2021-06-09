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

        private readonly Names mVariableNames;
        private readonly Datas mVariableDatas;
        private List<KeyValuePair<Block, CodeRunner>> mCodeRunners;

        public CSharpInterpreter(Names variableNames, Datas variableDatas)
        {
            mVariableNames = variableNames;
            mVariableDatas = variableDatas;
            mCodeRunners = new List<KeyValuePair<Block, CodeRunner>>();
        }

        public override void Run(Workspace workspace)
        {
            mVariableNames.Reset();
            mVariableDatas.Reset();
            
            //start runner from the topmost blocks
            List<Block> blocks = workspace.GetTopBlocks(true);
            foreach (Block block in blocks)
            {
                //exclude the procedure definition blocks
                if (ProcedureDB.IsDefinition(block))
                    continue;

                CodeRunner runner = CodeRunner.Create(block.Type);
                mCodeRunners.Add(new KeyValuePair<Block, CodeRunner>(block, runner));
            }

            if (workspace.Options.Synchronous)
            {
                //run synchronously
                foreach (KeyValuePair<Block, CodeRunner> pair in mCodeRunners)
                {
                    pair.Value.StartRun(new CmdEnumerator(pair.Key));
                }
            }
            else
            {
                //run one after another
                for (int i = 0; i < mCodeRunners.Count - 1; i++)
                {
                    var next = mCodeRunners[i + 1];
                    mCodeRunners[i].Value.SetFinishCallback(() =>
                    {
                        next.Value.StartRun(new CmdEnumerator(next.Key));
                    });
                }
                mCodeRunners[0].Value.StartRun(new CmdEnumerator(mCodeRunners[0].Key));
            }
        }

        public override void Pause()
        {
            foreach (KeyValuePair<Block, CodeRunner> pair in mCodeRunners)
            {
                var runner = pair.Value;
                if (runner.CurStatus == CodeRunner.Status.Running)
                    runner.Pause();
            }
            CSharp.Interpreter.FireUpdate(new InterpreterUpdateState(InterpreterUpdateState.Pause));
        }

        public override void Resume()
        {
            foreach (KeyValuePair<Block, CodeRunner> pair in mCodeRunners)
            {
                var runner = pair.Value;
                if (runner.CurStatus == CodeRunner.Status.Pause)
                    runner.Resume();
            }
            CSharp.Interpreter.FireUpdate(new InterpreterUpdateState(InterpreterUpdateState.Resume));
        }

        public override void Stop()
        {
            foreach (KeyValuePair<Block, CodeRunner> pair in mCodeRunners)
            {
                var runner = pair.Value;
                runner.Stop();
            }
            CSharp.Interpreter.FireUpdate(new InterpreterUpdateState(InterpreterUpdateState.Stop));
        }

        public override void Error(string msg)
        {
            foreach (KeyValuePair<Block, CodeRunner> pair in mCodeRunners)
            {
                var runner = pair.Value;
                runner.Stop();
            }
            CSharp.Interpreter.FireUpdate(new InterpreterUpdateState(InterpreterUpdateState.Error, msg));
        }


        /// <summary>
        /// run code representing the specified value input.
        /// should return a DataStruct
        /// </summary>
        public CmdEnumerator ValueReturn(Block block, string name)
        {
            var targetBlock = block.GetInputTargetBlock(name);
            if (targetBlock == null)
            {
                Debug.Log(string.Format("Value input block of {0} is null", block.Type));
                return null;
            }
            if (targetBlock.OutputConnection == null)
            {
                Debug.Log(string.Format("Value input block of {0} must have an output connection", block.Type));
                return null;
            }
            return new CmdEnumerator(targetBlock);
        }

        /// <summary>
        /// run code representing the specified value input. WITH a default DataStruct
        /// </summary>
        public CmdEnumerator ValueReturn(Block block, string name, DataStruct defaultData)
        {
            CmdEnumerator etor = ValueReturn(block, name);
            etor.Cmdtor.DefaultData = defaultData;
            return etor;
        }

        /// <summary>
        /// Run code representing the statement.
        /// </summary>
        public CmdEnumerator StatementRun(Block block, string name)
        {
            var targetBlock = block.GetInputTargetBlock(name);
            if (targetBlock == null)
            {
                Debug.Log(string.Format("Statement input block of {0} is null", block.Type));
                return null;
            }
            if (targetBlock.PreviousConnection == null)
            {
                Debug.Log(string.Format("Statement input block of {0} must have a previous connection", block.Type));
                return null;
            }

            return new CmdEnumerator(targetBlock);
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
