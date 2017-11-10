/****************************************************************************
 * Copyright (c) 2017 maoling@putao.com
 * 
 * Helper functions for interpreting lua for blocks.
****************************************************************************/

#if ULUA_SUPPORT
using LuaInterface;
#endif

namespace UBlockly
{
    public partial class LuaInterpreter : Interpreter
    {
        public override CodeName Name
        {
            get { return CodeName.Lua; }
        }

        public override void Run(Workspace workspace)
        {
            string code = Lua.Generator.WorkspaceToCode(workspace);
            
            #if ULUA_SUPPORT
            LuaState lua = new LuaState();
            lua.Start();
            lua.DoString(code);
            lua = null;
            #endif
        }
    }
}
