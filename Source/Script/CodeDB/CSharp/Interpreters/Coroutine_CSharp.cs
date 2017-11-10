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
