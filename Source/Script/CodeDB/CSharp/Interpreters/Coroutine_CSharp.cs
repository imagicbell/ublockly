using System.Collections;
using UnityEngine;

namespace PTGame.Blockly
{
    [CodeInterpreter(BlockType = "coroutine_time_wait")]
    public class Coroutine_TimeWait_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            Debug.Log(">>>>>> block time_wait start: " + Time.time);

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
            Debug.Log(">>>>>> block time_wait end: " + Time.time);
        }
    }
}