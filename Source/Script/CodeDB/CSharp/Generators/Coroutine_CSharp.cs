/****************************************************************************
 * Copyright (c) 2017 maoling@putao.com
 *
 * Functions for generating c# code for blocks.
****************************************************************************/

namespace UBlockly
{
    public partial class CSharpGenerator 
    {
        [CodeGenerator(BlockType = "coroutine_wait_time")]
        private string Time_Wait(Block block)
        {
            string time = CSharp.Generator.ValueToCode(block, "TIME", CSharp.ORDER_NONE, "0");
            Number timeNumber = new Number(time);
            string op = block.GetFieldValue("UNIT");
            switch (op)
            {
                case "MILLISECOND":
                    time = (timeNumber.Value * 0.001f).ToString();
                    break;
                case "SECONDS":
                    break;
                case "MINUTES":
                    time = (timeNumber.Value * 60f).ToString();
                    break;
                case "TOOHIGH":
                    time = null;
                    break;
            }
            if (string.IsNullOrEmpty(time)) return null;
            return string.Format("yield return new WaitForSeconds({0});\n", time);
        }
    }
}
