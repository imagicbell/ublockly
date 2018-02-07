/****************************************************************************

Functions for generating c# code for blocks.

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
