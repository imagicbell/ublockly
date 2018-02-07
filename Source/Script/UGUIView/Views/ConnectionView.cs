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
    /// <summary>
    /// Node to connect output, previous, next statement to other blocks
    /// </summary>
    public class ConnectionView : BaseView
    {
        [SerializeField] protected Define.EConnection m_ConnectionType;
        
        public override ViewType Type
        {
            get { return ViewType.Connection; }
        }

        public virtual Define.EConnection ConnectionType
        {
            get { return m_ConnectionType; }
            set { m_ConnectionType = value; }
        }
        
        public override Vector2 ChildStartXY
        {
            get
            {
                if (m_ConnectionType == Define.EConnection.NextStatement)
                    return -BlockViewSettings.Get().StatementConnectPointRect.position;
                return base.ChildStartXY;
            }
        }
        
        protected Connection mConnection;
        public Connection Connection { get { return mConnection; } }

        protected BlockView mSourceBlockView;
        public BlockView SourceBlockView
        {
            get { return mSourceBlockView; }
        }

        protected BlockView mTargetBlockView;
        public BlockView TargetBlockView
        {
            get { return mTargetBlockView; }
        }

        protected GameObject mHighlightObj;
        private MemorySafeConnectionObserver mObserver;

        public virtual void BindModel(Connection connection)
        {
            if (mConnection == connection) return;
            if (mConnection != null) UnBindModel();

            if (connection.Type != m_ConnectionType)
                throw new Exception("ConnectionView must be bound to connection with the same connection type");
            
            mConnection = connection;
            mSourceBlockView = BlocklyUI.WorkspaceView.GetBlockView(mConnection.SourceBlock);
            
            mObserver = new MemorySafeConnectionObserver(this);
            mConnection.AddObserver(mObserver);
        }

        public virtual void UnBindModel()
        {
            if (mConnection == null) return;
            
            mConnection.RemoveObserver(mObserver);
            mConnection = null;
        }
        
        protected override Vector2 CalculateSize()
        {
            //do nothing
            return Size;
        }

        protected internal override void OnXYUpdated()
        {
            if (mConnection == null)
                return;
            
            // Remove it from its old location in the database (if already present)
            if (mConnection.InDB)
                mConnection.DB.RemoveConnection(mConnection);
            
            //update Connection's location for connection search
            Vector2 relative = BlocklyUI.WorkspaceView.CodingArea.InverseTransformPoint(ViewTransform.position);
            mConnection.Location.x = (int) relative.x;
            mConnection.Location.y = (int) relative.y;

            // Insert it into its new location in the database.
            if (!mConnection.Hidden)
                mConnection.DB.AddConnection(mConnection);
            
            // update target block view's
            if (mTargetBlockView != null)
                mTargetBlockView.OnXYUpdated();
        }

        protected void OnConnectStateUpdated(Connection.UpdateState updateState)
        {
            switch (updateState)
            {
                case Connection.UpdateState.Connected:
                {
                    if (!mConnection.IsSuperior)
                        throw new Exception("ConnectionView- OnConnectStateUpdated: Only Superior can accept the \"Connected\" message.");
                    
                    // this superior connection is connected to a child connection
                    OnAttached();
                    break;
                }
                case Connection.UpdateState.Disconnected:
                {
                    if (!mConnection.IsSuperior)
                        throw new Exception("ConnectionView- OnConnectStateUpdated: Only Superior can accept the \"Disconnected\" message.");
                    
                    // this superior connection is disconnected to a child connection
                    OnDetached();
                    break;
                }
                case Connection.UpdateState.BumpedAway:
                {
                    if (mConnection.IsSuperior)
                        throw new Exception("ConnectionView- OnConnectStateUpdated: Only Inferior can accept the \"BumpedAway\" message.");
                    
                    // this inferior connection is bumped away after disconnecting
                    mSourceBlockView.XY += BlockViewSettings.Get().BumpAwayOffset;
                    break;
                }
                case Connection.UpdateState.Highlight:
                {
                    Highlight(true);
                    break;
                }
                case Connection.UpdateState.UnHighlight:
                {
                    Highlight(false);
                    break;
                }
            }
        }

        protected virtual void OnAttached()
        {
            mTargetBlockView = BlocklyUI.WorkspaceView.GetBlockView(mConnection.TargetBlock);
            
            // attach a child block view
            AddChild(mTargetBlockView); 
            mTargetBlockView.XY = ChildStartXY;
            UpdateLayout(XY);
        }

        protected virtual void OnDetached()
        {
            // detach a child block view
            RemoveChild(mTargetBlockView);
            BlockView detachedView = mTargetBlockView;
            mTargetBlockView = null;
            UpdateLayout(XY);
            detachedView.SetOrphan();
        }

        public bool SearchClosest(int searchLimit, ref Connection closest, ref int closestRadius)
        {
            Connection closestFound;
            int radius;
            mConnection.DBOpposite.SearchForClosest(mConnection, searchLimit, new Vector2<int>(0, 0), out closestFound, out radius);
            if (closestFound != null)
            {
                closest = closestFound;
                closestRadius = radius;
                return true;
            }
            return false;
        }

        protected void Highlight(bool active)
        {
            if (mHighlightObj == null)
            {
                mHighlightObj = GameObject.Instantiate(BlockViewSettings.Get().PrefabConnectHighlight);
                RectTransform highlightTrans = mHighlightObj.GetComponent<RectTransform>();
                highlightTrans.SetParent(ViewTransform, false);

                if (ConnectionType == Define.EConnection.InputValue)
                {
                    highlightTrans.localRotation = Quaternion.Euler(0, 0, -90);
                    highlightTrans.pivot = new Vector2(0, 0);
                    highlightTrans.anchorMin = highlightTrans.anchorMax = new Vector2(0, 1);
                    highlightTrans.anchoredPosition3D = new Vector2(0, -17);
                }
                else if (ConnectionType == Define.EConnection.OutputValue)
                {
                    highlightTrans.localRotation = Quaternion.Euler(0, 0, -90);
                    highlightTrans.anchoredPosition3D = Vector3.zero;
                }
                else if (ConnectionType == Define.EConnection.NextStatement && Type == ViewType.ConnectionInput)
                {
                    highlightTrans.pivot = new Vector2(0, 1);
                    highlightTrans.anchorMin = highlightTrans.anchorMax = new Vector2(0, 1);
                    highlightTrans.anchoredPosition3D = new Vector2(18, 0);
                }
                else
                {
                    highlightTrans.anchoredPosition3D = Vector3.zero;
                }
            }
            mHighlightObj.SetActive(active);
        }

        private class MemorySafeConnectionObserver : IObserver<Connection.UpdateState>
        {
            private ConnectionView mViewRef;

            public MemorySafeConnectionObserver(ConnectionView viewRef)
            {
                mViewRef = viewRef;
            }

            public void OnUpdated(object connection, Connection.UpdateState newValue)
            {
                if (mViewRef == null || mViewRef.ViewTransform == null || mViewRef.Connection != connection)
                    ((Connection) connection).RemoveObserver(this);
                else
                    mViewRef.OnConnectStateUpdated(newValue);
            }
        }
    }
}
