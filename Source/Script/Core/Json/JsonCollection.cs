using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UBlockly
{
    [CreateAssetMenu(menuName = "PTBlockly/BlockJsonCollection", fileName = "BlockJsonCollection")]
    public class JsonCollection : ScriptableObject
    {
        [SerializeField] List<TextAsset> BlockJsonFiles;

        public void LoadAll()
        {
            if (BlockJsonFiles == null || BlockJsonFiles.Count == 0)
                return;

            foreach (TextAsset json in BlockJsonFiles)
            {
                BlockFactory.Instance.AddJsonDefinitions(JArray.Parse(json.text));
            }
        }
    }
}
