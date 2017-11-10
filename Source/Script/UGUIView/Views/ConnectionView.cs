using System;
using UnityEngine;

namespace UBlockly.UGUI
{
    /// <summary>
    /// Node to connect output, previous, next statement to other blocks
    /// </summary>
    public class ConnectionView : BaseView
    {
        public enum ConType
        {
            NONE = 0,
            INPUT_VALUE = Blockly.INPUT_VALUE,
            OUTPUT_VALUE = Blockly.OUTPUT_VALUE,
            NEXT_STATEMENT = Blockly.NEXT_STATEMENT,
            PREVIOUS_STATEMENT = Blockly.PREVIOUS_STATEMENT,
            DUMMY_INPUT = Blockly.DUMMY_INPUT
        }

        [SerializeField] protected ConType m_ConnectionType;
        
        public override ViewType Type
        {
            get { return ViewType.Connection; }
        }

        public virtual ConType ConnectionType
        {
            get { return m_ConnectionType; }
            set { m_ConnectionType = value; }
        }
        
        public override Vector2 ChildStartXY
        {
            get
            {
                if (m_ConnectionType == ConType.NEXT_STATEMENT)
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

            if (connection.Type != (int)m_ConnectionType)
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
            Vector2 relative = XYInCodingArea;
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
                    mTargetBlockView = BlocklyUI.WorkspaceView.GetBlockView(mConnection.TargetBlock);
                    OnAttached();
                    break;
                }
                case Connection.UpdateState.Disconnected:
                {
                    if (!mConnection.IsSuperior)
                        throw new Exception("ConnectionView- OnConnectStateUpdated: Only Superior can accept the \"Disconnected\" message.");
                    
                    // this superior connection is disconnected to a child connection
                    OnDetached();
                    mTargetBlockView = null;
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
            // attach a child block view
            AddChild(mTargetBlockView); 
            mTargetBlockView.XY = ChildStartXY;
            UpdateLayout(XY);
        }

        protected virtual void OnDetached()
        {
            // detach a child block view
            RemoveChild(mTargetBlockView);
            UpdateLayout(XY);
            mTargetBlockView.SetOrphan();
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

                if (ConnectionType == ConType.INPUT_VALUE)
                {
                    highlightTrans.localRotation = Quaternion.Euler(0, 0, -90);
                    highlightTrans.pivot = new Vector2(0, 0);
                    highlightTrans.anchorMin = highlightTrans.anchorMax = new Vector2(0, 1);
                    highlightTrans.anchoredPosition3D = new Vector2(0, -17);
                }
                else if (ConnectionType == ConType.OUTPUT_VALUE)
                {
                    highlightTrans.localRotation = Quaternion.Euler(0, 0, -90);
                    highlightTrans.anchoredPosition3D = Vector3.zero;
                }
                else if (ConnectionType == ConType.NEXT_STATEMENT && Type == ViewType.ConnectionInput)
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
