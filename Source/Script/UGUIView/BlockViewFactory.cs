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
