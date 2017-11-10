/****************************************************************************
 * Copyright (c) 2017 maoling@putao.com
 * 
 * Utility functions for interpreting Blockly code to implementations
****************************************************************************/

using System;
using System.Collections.Generic;
using System.Reflection;

namespace UBlockly
{
    public abstract class Interpreter : Observable<InterpreterUpdateState>
    {
        public abstract CodeName Name { get; }
        
        /// <summary>
        /// instances for interpreting code
        /// </summary>
        protected Dictionary<string, Cmdtor> mCmdMap;

        protected Interpreter()
        {
            InitCodeDB();
        }
        
        /// <summary>
        /// collect all code generation/interpretion methods
        /// </summary>
        protected void InitCodeDB()
        {
            mCmdMap = new Dictionary<string, Cmdtor>();
            Assembly assem = Assembly.GetAssembly(this.GetType());
            foreach (Type type in assem.GetTypes())
            {
                if (type.IsSubclassOf(typeof(Cmdtor)))
                {
                    var attrs = type.GetCustomAttributes(typeof(CodeInterpreterAttribute), false);
                    if (attrs.Length > 0)
                    {
                        mCmdMap[((CodeInterpreterAttribute) attrs[0]).BlockType] = Activator.CreateInstance(type) as Cmdtor;
                    }
                }
            }
        }

        /// <summary>
        /// run code for all blocks in the workspace to the specified language.
        /// </summary>
        public virtual void Run(Workspace workspace) {}

        /// <summary>
        /// Pause the current running interpreting process
        /// </summary>
        public virtual void Pause() {} 

        /// <summary>
        /// Resume the paused the interpreting process
        /// </summary>
        public virtual void Resume() {}

        /// <summary>
        /// stop the current running interpreting process 
        /// </summary>
        public virtual void Stop() {}
    }
    
    public class InterpreterUpdateState
    {
        public const int RunBlock = 1;
        public const int FinishBlock = 2;
        public const int Pause = 3;
        public const int Resume = 4;
        public const int Stop = 5;

        public readonly int Type;
        public readonly Block RunningBlock;

        public InterpreterUpdateState(int type)
        {
            Type = type;
        }

        public InterpreterUpdateState(int type, Block runBlock) : this(type)
        {
            RunningBlock = runBlock;
        }
    }
}
