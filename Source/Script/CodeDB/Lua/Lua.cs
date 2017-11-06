/****************************************************************************
 * Copyright (c) 2017 maoling@putao.com
 *
 * Lua code generating and interpreting collection
****************************************************************************/

namespace PTGame.Blockly
{
    public static class Lua
    {
        private static LuaGenerator mGenerator = null;
        public static LuaGenerator Generator
        {
            get { return mGenerator ?? (mGenerator = new LuaGenerator(VariableNames)); }
        }
        
        private static LuaInterpreter mInterpreter = null;
        public static LuaInterpreter Interpreter
        {
            get { return mInterpreter ?? (mInterpreter = new LuaInterpreter()); }
        }
        
        private static Names mVariableNames = null;
        public static Names VariableNames
        {
            get
            {
                return mVariableNames ?? (mVariableNames = new Names( // Special character
                           "_," +
                           // From theoriginalbit"s script:
                           // https://github.com/espertus/blockly-lua/issues/6
                           "__inext,assert,bit,colors,colours,coroutine,disk,dofile,error,fs," +
                           "fetfenv,getmetatable,gps,help,io,ipairs,keys,loadfile,loadstring,math," +
                           "native,next,os,paintutils,pairs,parallel,pcall,peripheral,print," +
                           "printError,rawequal,rawget,rawset,read,rednet,redstone,rs,select," +
                           "setfenv,setmetatable,sleep,string,table,term,textutils,tonumber," +
                           "tostring,turtle,type,unpack,vector,write,xpcall,_VERSION,__indext," +
                           // Not included in the script, probably because it wasn"t enabled:
                           "HTTP," +
                           // Keywords (http://www.lua.org/pil/1.3.html).
                           "and,break,do,else,elseif,end,false,for,function,if,in,local,nil,not,or," +
                           "repeat,return,then,true,until,while," +
                           // Metamethods (http://www.lua.org/manual/5.2/manual.html).
                           "add,sub,mul,div,mod,pow,unm,concat,len,eq,lt,le,index,newindex,call," +
                           // Basic functions (http://www.lua.org/manual/5.2/manual.html, section 6.1).
                           "assert,collectgarbage,dofile,error,_G,getmetatable,inpairs,load," +
                           "loadfile,next,pairs,pcall,print,rawequal,rawget,rawlen,rawset,select," +
                           "setmetatable,tonumber,tostring,type,_VERSION,xpcall," +
                           // Modules (http://www.lua.org/manual/5.2/manual.html, section 6.3).
                           "require,package,string,table,math,bit32,io,file,os,debug"));
            }
        }

        public const int ORDER_ATOMIC = 0;          // literals
        // The next level was not explicit in documentation and inferred by Ellen.
        public const int ORDER_HIGH = 1;            // Function calls, tables[]
        public const int ORDER_EXPONENTIATION = 2;  // ^
        public const int ORDER_UNARY = 3;           // not # - ~
        public const int ORDER_MULTIPLICATIVE = 4;  // * / %
        public const int ORDER_ADDITIVE = 5;        // + -
        public const int ORDER_CONCATENATION = 6;   // ..
        public const int ORDER_RELATIONAL = 7;      // < > <=  >= ~= ==
        public const int ORDER_AND = 8;             // and
        public const int ORDER_OR = 9;              // or
        public const int ORDER_NONE = 99;

        
        public static void Dispose()
        {
            mGenerator = null;
            mInterpreter = null;
            mVariableNames = null;
        }
    }
}