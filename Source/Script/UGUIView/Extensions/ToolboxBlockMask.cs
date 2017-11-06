using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PTGame.Blockly.UGUI
{
    public class ToolboxBlockMask : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private BlockView mBlockView;
        public BlockView BlockView
        {
            set { mBlockView = value; }
        }

        private BlockView mDraggedBlockView;
        
        public static void AddMask(BlockView blockView)
        {
            ToolboxBlockMask mask = blockView.GetComponentInChildren<ToolboxBlockMask>();
            if (mask != null)
            {
                mask.BlockView = blockView;
                return;
            }

            GameObject maskObj = new GameObject("ToolboxMask");
            maskObj.transform.SetParent(blockView.transform, false);
            RectTransform maskTrans = maskObj.AddComponent<RectTransform>();
            maskTrans.sizeDelta = blockView.Size;
            Image maskImage = maskObj.AddComponent<Image>();
            maskImage.color = new Color(1, 1, 1, 0);
            
            mask = maskObj.AddComponent<ToolboxBlockMask>();
            mask.BlockView = blockView;
        }

        public static void RemoveMask(BlockView blockView)
        {
            ToolboxBlockMask mask = blockView.GetComponentInChildren<ToolboxBlockMask>();
            if (mask != null)
                GameObject.Destroy(mask.gameObject);
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (mDraggedBlockView != null)
            {
                Debug.LogError("ToolboxBlockMask-OnBeginDrag: Already dragged a block");
                return;
            }
            
            // compute the local position of the block view in coding area
            Vector3 localPos = BlocklyUI.WorkspaceView.CodingArea.InverseTransformPoint(mBlockView.ViewTransform.position);
            
            // clone a new block view for coding area
            mDraggedBlockView = BlocklyUI.WorkspaceView.CloneBlockView(mBlockView, new Vector2(localPos.x, localPos.y));
            mDraggedBlockView.OnBeginDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (mDraggedBlockView == null)
            {
                Debug.LogError("ToolboxBlockMask-OnDrag: No dragging block now");
                return;
            }
            
            mDraggedBlockView.OnDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (mDraggedBlockView == null)
            {
                Debug.LogError("ToolboxBlockMask-OnEndDrag: No dragging block now");
                return;
            }
            
            mDraggedBlockView.OnEndDrag(eventData);
            mDraggedBlockView = null;
            
            BlocklyUI.WorkspaceView.Toolbox.HideBlockMenu();
        }
    }
}