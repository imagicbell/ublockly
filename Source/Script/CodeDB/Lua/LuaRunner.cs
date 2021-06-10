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
    public class LuaRunner : Runner
    {
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