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
using System.Collections.Generic;
using UnityEngine;

namespace UBlockly.UGUI
{
    /// <summary>
    /// factory to create block view
    /// developers can change this to static class and customnly load prefabs from Assetbundles or Resources. 
    /// </summary>
    public static class BlockViewFactory
    {
        public static BlockView CreateView(Block block)
        {
            BlockView blockView;
            
            GameObject blockPrefab = BlockResMgr.Get().LoadBlockViewPrefab(block.Type);
            if (blockPrefab != null)
            {
                // has been builded beforehand
                GameObject blockObj = GameObject.Instantiate(blockPrefab);
                blockObj.name = blockPrefab.name;
                
                blockView = blockObj.GetComponent<BlockView>();
                blockView.BindModel(block);
                
                // rebuild inputs for mutation blocks
                if (block.Mutator != null)
                    BlockViewBuilder.BuildInputViews(block, blockView);
                
                blockView.BuildLayout();
            }
            else
            {
                blockPrefab = BlockViewBuilder.BuildBlockView(block);
                
                blockView = blockPrefab.GetComponent<BlockView>();
                blockView.BindModel(block);
                
                // BlockViewBuilder.BuildBlockView will do both "BuildInputViews" and "BuildLayout"
            }

            blockView.ChangeBgColor(BlocklyUI.WorkspaceView.Toolbox.GetColorOfBlockView(blockView));
            
            return blockView;
        }
    }
}
