using System;
using UnityEngine;

namespace UBlockly.UGUI
{
    [CreateAssetMenu(menuName = "UBlockly/BlockViewSettings", fileName = "BlockViewSettings")]
    public class BlockViewSettings : ScriptableObject
    {
        [SerializeField] public string BlockPrefabPath;
        
        [Tooltip("Basic block height, without margin.")]
        [SerializeField] public int BlockHeight = 60;        
        [SerializeField] public RectOffset ContentMargin;
        [SerializeField] public Vector2 ContentSpace;
        [SerializeField] public int EmptyInputSlotWidth;
        [SerializeField] public int ColorFieldWidth;
        
        [Tooltip("Maximum misalignment between connections for them to snap together.")]
        [SerializeField] public int ConnectSearchRange = 100;
        [Tooltip("The offset for bumpping away disconnected blocks ")]
        [SerializeField] public Vector2 BumpAwayOffset;
        [SerializeField] public Rect ValueConnectPointRect;
        [SerializeField] public Rect StatementConnectPointRect;

        [SerializeField] public GameObject PrefabRoot;
        [SerializeField] public GameObject PrefabRootOutput;
        [SerializeField] public GameObject PrefabRootPrev;
        [SerializeField] public GameObject PrefabRootPrevNext;

        [SerializeField] public GameObject PrefabInputValue;
        [SerializeField] public GameObject PrefabInputStatement;

        [SerializeField] public GameObject PrefabFieldLabel;
        [SerializeField] public GameObject PrefabFieldInput;
        [SerializeField] public GameObject PrefabFieldButton;
        [SerializeField] public GameObject PrefabFieldVariable;

        [SerializeField] public GameObject PrefabBtnCreateVar;
        [SerializeField] public GameObject PrefabConnectHighlight;
        [SerializeField] public GameObject PrefabStatusLight;

        public float ContentHeight
        {
            get { return BlockHeight; }
        }
        
        private static BlockViewSettings mInstance = null; 
        public static BlockViewSettings Get()
        {
            if (mInstance == null)
                mInstance = Resources.Load<BlockViewSettings>("BlockViewSettings");
            if (mInstance == null)
                throw new Exception("There is no \"BlockViewSettings\" ScriptObject under Resources folder");
                
            return mInstance;
        }

        public static void Dispose()
        {
            mInstance = null;
        }
    }
}
