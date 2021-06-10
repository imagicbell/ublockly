/****************************************************************************

Copyright 2021 sophieml1989@gmail.com

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

namespace UBlockly
{
    public abstract class Runner : Observable<RunnerUpdateState>
    {
        public enum Mode
        {
            Normal,
            Step
        }
        
        public enum Status
        {
            Running,
            Pause,
            Stop,
        }

        public Mode RunMode { get; protected set; } = Mode.Normal;
        public Status CurStatus { get; protected set; } = Status.Stop;

        public virtual void SetMode(Mode mode)
        {
            RunMode = mode;
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
        
        /// <summary>
        /// for debug mode, step block one by one
        /// </summary>
        public virtual void Step() {}
        
        /// <summary>
        /// process overflows
        /// </summary>
        public virtual void Error(string msg) {}
    }
    
    public class RunnerUpdateState
    {
        public const int RunBlock = 1;
        public const int FinishBlock = 2;
        public const int Pause = 3;
        public const int Resume = 4;
        public const int Stop = 5;
        public const int Error = 6;

        public readonly int Type;
        public readonly Block RunningBlock;
        public readonly string Msg;

        public RunnerUpdateState(int type)
        {
            Type = type;
        }

        public RunnerUpdateState(int type, Block runBlock) : this(type)
        {
            RunningBlock = runBlock;
        }

        public RunnerUpdateState(int type, string msg) : this(type)
        {
            Msg = msg;
        }
    }
}