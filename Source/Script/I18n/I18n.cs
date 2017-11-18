using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace UBlockly
{
    public class I18n
    {
        public const string EN = "en";
        public const string CN = "cn";

        private static Dictionary<string, string> mMsg = null;
        public static Dictionary<string, string> Msg
        {
            get
            {
                if (mMsg == null)
                    BlockResMgr.Get().LoadI18n(I18n.EN);
                return mMsg;
            }
        }

        public static void AddI18nFile(string text)
        {
            if (mMsg == null)
                mMsg = new Dictionary<string, string>();
            
            JObject jobj = JObject.Parse(text);
            foreach (KeyValuePair<string, JToken> pair in jobj)
            {
                mMsg[pair.Key] = pair.Value.ToString();
            }
        }

        public static void Dispose()
        {
            if (mMsg != null)
                mMsg.Clear();
            mMsg = null;
        }
    }
}
