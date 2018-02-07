/****************************************************************************

Functions for interpreting c# code for blocks.

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


using System.Collections;
using UnityEngine;

namespace UBlockly
{
    [CodeInterpreter(BlockType = "coroutine_wait_time")]
    public class Coroutine_WaitTime_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            Debug.Log(">>>>>> block wait_time start: " + Time.time);

            CustomEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "TIME", new DataStruct(0));
            yield return ctor;
            DataStruct time = ctor.Data;
            
            string unit = block.GetFieldValue("UNIT");
            switch (unit)
            {
                case "MILLISECOND":
                    yield return new WaitForSeconds(time.NumberValue.Value * 0.001f);
                    break;
                case "SECONDS":
                    yield return new WaitForSeconds(time.NumberValue.Value);
                    break;
                case "MINUTES":
                    yield return new WaitForSeconds(time.NumberValue.Value * 60f);
                    break;
                case "TOOHIGH":
                    Debug.Log("wait time too long");
                    break;
            }
            Debug.Log(">>>>>> block wait_time end: " + Time.time);
        }
    }
    
    [CodeInterpreter(BlockType = "coroutine_wait_frame")]
    public class Coroutine_WaitFrame_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            Debug.Log(">>>>>> block wait_frame start: " + Time.time);

            CustomEnumerator ctor = CSharp.Interpreter.ValueReturn(block, "TIME", new DataStruct(0));
            yield return ctor;
            DataStruct time = ctor.Data;

            for (int i = 0; i < time.NumberValue.Value; i++)
            {
                yield return null;
            }
            
            Debug.Log(">>>>>> block wait_frame end: " + Time.time);
        }
    }
}
