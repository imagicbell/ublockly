/****************************************************************************

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


using System;
using UnityEngine;

namespace UBlockly.UGUI
{
    [CreateAssetMenu(menuName = "UBlockly/BlockViewSettings", fileName = "BlockViewSettings")]
    public class BlockViewSettings : ScriptableObject
    {
        [Tooltip("Basic block height, without margin.")]
        [SerializeField] public int BlockHeight = 60;
        [SerializeField] public int MinUnitWidth = 40;
        [SerializeField] public RectOffset ContentMargin;
        [SerializeField] public Vector2 ContentSpace;
        [SerializeField] public int ColorFieldWidth;

        [Tooltip("Whether block pattern views in toolbox have masks and are unable to receive input.")]
        [SerializeField] public bool MaskedInToolbox = true;
        [Tooltip("Maximum misalignment between connections for them to snap together.")]
        [SerializeField] public int ConnectSearchRange = 100;
        [Tooltip("The offset for bumpping away disconnected blocks ")]
        [SerializeField] public Vector2 BumpAwayOffset;
        [SerializeField] public Rect ValueConnectPointRect;
        [SerializeField] public Rect StatementConnectPointRect;

        [SerializeField] public GameObject PrefabRoot;
        [SerializeField] public GameObject PrefabRootOutput;
        [SerializeField] public GameObject PrefabRootPrev;
        [SerializeField] public GameObject PrefabRootNext;
        [SerializeField] public GameObject PrefabRootPrevNext;

        [SerializeField] public GameObject PrefabInputValue;
        [SerializeField] public GameObject PrefabInputValueSlot;
        [SerializeField] public GameObject PrefabInputStatement;

        [SerializeField] public GameObject PrefabFieldLabel;
        [SerializeField] public GameObject PrefabFieldInput;
        [SerializeField] public GameObject PrefabFieldImage;
        [SerializeField] public GameObject PrefabFieldButton;
        [SerializeField] public GameObject PrefabFieldVariable;
        [SerializeField] public GameObject PrefabFieldCheckbox;

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
