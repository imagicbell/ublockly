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



using System.Collections.Generic;
using UnityEngine;

namespace UBlockly.UGUI
{
    public class BlockStatusView : MonoBehaviour
    {
        private RunnerUpdateStateObserver mObserver;
        private GameObject mStatusObj;
        private Stack<Block> mRunningBlocks;
        private BlockView mRunBlockView;

        private void Awake()
        {
            mRunningBlocks = new Stack<Block>();
            mObserver = new RunnerUpdateStateObserver(this);
            CSharp.Runner.AddObserver(mObserver);
        }

        private void Show()
        {
            if (mStatusObj == null)
            {
                mStatusObj = GameObject.Instantiate(BlockViewSettings.Get().PrefabStatusLight, BlocklyUI.WorkspaceView.CodingArea, false);
                RectTransform statusRect = mStatusObj.GetComponent<RectTransform>();
                statusRect.anchorMin = statusRect.anchorMax = new Vector2(0, 1);
                statusRect.pivot = 0.5f * Vector2.one;
            }
            if (!mStatusObj.activeInHierarchy)
                mStatusObj.SetActive(true);
        }

        private void Hide()
        {
            if (mStatusObj != null)
            {
                mStatusObj.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            CSharp.Runner.RemoveObserver(mObserver);
        }

        public void UpdateStatus(RunnerUpdateState args)
        {
            switch (args.Type)
            {
                case RunnerUpdateState.RunBlock:
                {
                    mRunningBlocks.Push(args.RunningBlock);
                    mRunBlockView = BlocklyUI.WorkspaceView.GetBlockView(args.RunningBlock);
                    Show();
                    break;
                }
                case RunnerUpdateState.FinishBlock:
                {
                    if (mRunningBlocks.Count > 0 && mRunningBlocks.Peek() == args.RunningBlock)
                    {
                        mRunningBlocks.Pop();
                        if (mRunningBlocks.Count > 0)
                            mRunBlockView = BlocklyUI.WorkspaceView.GetBlockView(mRunningBlocks.Peek());
                        else 
                            Hide();
                    }
                    break;
                }
                case RunnerUpdateState.Stop:
                {
                    Hide();
                    mRunningBlocks.Clear();
                    mRunBlockView = null;
                    break;
                }
                case RunnerUpdateState.Error:
                {
                    if (!string.IsNullOrEmpty(args.Msg))
                    {
                        MsgDialog dialog = DialogFactory.CreateDialog("message") as MsgDialog;
                        dialog.SetMsg(args.Msg);    
                    }
                    Hide();
                    mRunningBlocks.Clear();
                    mRunBlockView = null;
                    break;
                }
            }
        }

        private void LateUpdate()
        {
            //update the status object on lateupdate, to avoid moving it multiple times in on frame
            if (mRunBlockView != null)
            {
                RectTransform statusRect = mStatusObj.GetComponent<RectTransform>();
                statusRect.SetParent(mRunBlockView.ViewTransform, false);
                statusRect.anchoredPosition = new Vector2(20, -25);
                mRunBlockView = null;
            }
        }

        private class RunnerUpdateStateObserver : IObserver<RunnerUpdateState>
        {
            private BlockStatusView mView;

            public RunnerUpdateStateObserver(BlockStatusView statusView)
            {
                mView = statusView;
            }

            public void OnUpdated(object subject, RunnerUpdateState args)
            {
                mView.UpdateStatus(args);
            }
        }
    }
}
