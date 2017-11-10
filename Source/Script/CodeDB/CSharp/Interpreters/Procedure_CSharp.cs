/****************************************************************************
 * Copyright (c) 2017 maoling@putao.com
 * 
 * Functions for interpreting c# code for blocks.
****************************************************************************/

using System.Collections;

namespace UBlockly
{
    [CodeInterpreter(BlockType = "procedures_callreturn")]
    public class Procedure_CallReturn_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            string procedureName = block.GetFieldValue("NAME");
            Block defBlock = block.Workspace.ProcedureDB.GetDefinitionBlock(procedureName);
            yield return CSharp.Interpreter.StatementRun(defBlock, "STACK");
            
            CustomEnumerator ctor = CSharp.Interpreter.ValueReturn(defBlock, "RETURN");
            yield return ctor;
            ReturnData(ctor.Data);
        }
    }

    [CodeInterpreter(BlockType = "procedures_callnoreturn")]
    public class Procedure_CallNoReturn_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            string procedureName = block.GetFieldValue("NAME");
            Block defBlock = block.Workspace.ProcedureDB.GetDefinitionBlock(procedureName);
            yield return CSharp.Interpreter.StatementRun(defBlock, "STACK");
        }
    }

    [CodeInterpreter(BlockType = "procedures_ifreturn")]
    public class Proceudre_IfReturn_Cmdtor : EnumeratorCmdtor
    {
        protected override IEnumerator Execute(Block block)
        {
            yield return 0;
        }
    }
}
