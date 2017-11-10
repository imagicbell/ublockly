using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UBlockly.UGUI
{
    public static class BlocklyUI
    {
        public static WorkspaceView WorkspaceView;
        public static Canvas UICanvas;

        public static void NewWorkspace()
        {
            if (WorkspaceView != null)
                throw new Exception("BlocklyUI.NewWorkspace- there is already a workspace");
            
            Workspace workspace = new Workspace(new Workspace.WorkspaceOptions());
            WorkspaceView = Object.FindObjectOfType<WorkspaceView>();
            WorkspaceView.BindModel(workspace);

            UICanvas = WorkspaceView.GetComponentInParent<Canvas>();
        }

        public static void DestroyWorkspace()
        {
            if (WorkspaceView == null)
                return;
            
            WorkspaceView.Dispose();
            GameObject.Destroy(WorkspaceView.gameObject);
        }
    }
}
