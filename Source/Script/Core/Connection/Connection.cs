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
using System.Text;
using System.Xml;
using UnityEngine;
using UnityEngine.Assertions;

namespace UBlockly
{
    /// <summary>
    /// The connection model
    /// inherited from Observable
    /// </summary>
    public class Connection : Observable<Connection.UpdateState>
    {
        public enum UpdateState
        {
            Connected,
            Disconnected,
            BumpedAway,    //force disconnected and bump the block away from connection
            Highlight,     //highlight the connection
            UnHighlight,   //unhighlight the connection
        }
        
        private Block mSourceBlock;
        public Block SourceBlock
        {
            get { return mSourceBlock; }
            set
            {
                if (mSourceBlock == value) return;
                if (mSourceBlock != null && value != null)
                    throw new Exception("Connection is already a member of another block.");
                mSourceBlock = value;
                if (mSourceBlock != null && mSourceBlock.Workspace.ConnectionDBList != null)
                {
                    ConnectionDB db;
                    mSourceBlock.Workspace.ConnectionDBList.TryGetValue(Type, out db);
                    DB = db;
                    Hidden = DB == null;

                    ConnectionDB dbOpposite;
                    mSourceBlock.Workspace.ConnectionDBList.TryGetValue(Define.OppositeConnection(Type), out dbOpposite);
                    DBOpposite = dbOpposite;
                }
            }
        }

        /// <summary>
        /// The type of the connection.
        /// </summary>
        public Define.EConnection Type { get; private set; }

        /// <summary>
        /// Does the connection belong to a superior block (higher in the source stack)?
        /// </summary>
        public bool IsSuperior
        {
            get { return this.Type == Define.EConnection.InputValue || this.Type == Define.EConnection.NextStatement; }
        }

        /// <summary>
        /// Class for a connection between blocks.
        /// </summary>
        /// <param name="source">The block establishing this connection.</param>
        /// <param name="type">The type of the connection.</param>
        public Connection(Block source, Define.EConnection type)
        {
            Type = type;
            SourceBlock = source;
        }
        
        /// <summary>
        /// Class for a connection between blocks.
        /// </summary>
        /// <param name="type"></param>
        public Connection(Define.EConnection type) : this(null, type)
        {
        }

        //Constants for checking whether two connections are compatible.
        public const int CAN_CONNECT = 0;
        public const int REASON_SELF_CONNECTION = 1;
        public const int REASON_WRONG_TYPE = 2;
        public const int REASON_TARGET_NULL = 3;
        public const int REASON_CHECKS_FAILED = 4;
        public const int REASON_DIFFERENT_WORKSPACES = 5;
        public const int REASON_SHADOW_PARENT = 6;
        
        /// <summary>
        /// Connection this connection connects to.  Null if not connected.
        /// </summary>
        public Connection TargetConnection;

        /// <summary>
        /// Is the connection connected?
        /// </summary>
        public bool IsConnected
        {
            get { return TargetConnection != null; }
        }
        
        /// <summary>
        /// Returns the block that this connection connects to.
        /// </summary>
        public Block TargetBlock
        {
            get
            {
                if (this.IsConnected)
                {
                    return this.TargetConnection.SourceBlock;
                }
                return null;
            }
        }

        /// <summary>
        /// List of compatible value types.  Null if all types are compatible.
        /// </summary>
        public List<string> Check { get; protected set; }

        /// <summary>
        /// Horizontal and Vertical location of this connection.
        /// </summary>
        public Vector2<int> Location;
        public int X
        {
            get { return Location.x; }
            set { Location.x = value; }
        }
        public int Y
        {
            get { return Location.y; }
            set { Location.y = value; }
        }

        /// <summary>
        /// Has this connection been added to the connection database?
        /// </summary>
        public bool InDB { get; set; }

        /// <summary>
        /// Connection database for connections of this type on the current workspace.
        /// </summary>
        public ConnectionDB DB { get; private set; }
        
        /// <summary>
        /// Connection database for connections compatible with this type on the current workspace.
        /// </summary>
        public ConnectionDB DBOpposite { get; private set; }
        
        /// <summary>
        /// Whether this connections is hidden (not tracked in a database) or not.
        /// </summary>
        public bool Hidden { get; private set; }
        
        /// <summary>
        /// DOM representation of a block or null.
        /// </summary>
        public XmlNode ShadowDom { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="otherConnection"></param>
        public void Connect(Connection otherConnection)
        {
            if (this.TargetConnection == otherConnection)
            {
                return;
            }

            this.CheckConnection(otherConnection);
            if (this.IsSuperior)
                this.ConnectInternal(otherConnection);
            else
                otherConnection.ConnectInternal(this);
        }

        /// <summary>
        /// Connect two connections together.  This is the connection on the superior block.
        /// </summary>
        /// <param name="childConnection"></param>
        private void ConnectInternal(Connection childConnection)
        {
            var parentConnection = this;
            var parentBlock = parentConnection.SourceBlock;
            var childBlock = childConnection.SourceBlock;
            
            // Disconnect any existing parent on the child connection.
            if (childConnection.IsConnected)
                childConnection.Disconnect();

            // Other connection is already connected to something.
            // Disconnect it and reattach it or bump it as needed.
            if (parentConnection.IsConnected)
            {
                Block orphanBlock = parentConnection.TargetBlock;
                XmlNode shadowDom = parentConnection.ShadowDom;
                parentConnection.ShadowDom = null;

                if (orphanBlock.IsShadow)
                {
                    // Save the shadow block so that field values are preserved.
                    shadowDom = Xml.BlockToDom(orphanBlock);
                    orphanBlock.Dispose();
                    orphanBlock = null;
                }
                else if (parentConnection.Type == Define.EConnection.InputValue)
                {
                    //value connnections
                    if (orphanBlock.OutputConnection == null)
                        throw new Exception("Orphan block does not have an output connection.");
                    
                    // Attempt to reattach the orphan at the end of the newly inserted
                    // block.  Since this block may be a row, walk down to the end
                    // or to the first (and only) shadow block.
                    var connection = Connection.LastConnectionInRow(childBlock, orphanBlock);
                    if (connection != null)
                    {
                        orphanBlock.OutputConnection.Connect(connection);
                        orphanBlock = null;
                    }
                }
                else if (parentConnection.Type == Define.EConnection.NextStatement)
                {
                    // Statement connections.
                    // Statement blocks may be inserted into the middle of a stack.
                    // Split the stack.
                    if (orphanBlock.PreviousConnection == null)
                        throw new Exception("Orphan block does not have a previous connection.");
                    
                    // Attempt to reattach the orphan at the bottom of the newly inserted
                    // block.  Since this block may be a stack, walk down to the end.
                    var newBlock = childBlock;
                    while (newBlock.NextConnection != null)
                    {
                        var nextBlock = newBlock.NextBlock;
                        if (nextBlock != null && !nextBlock.IsShadow)
                        {
                            newBlock = nextBlock;
                        }
                        else
                        {
                            if (orphanBlock.PreviousConnection.CheckType(newBlock.NextConnection))
                            {
                                newBlock.NextConnection.Connect(orphanBlock.PreviousConnection);
                                orphanBlock = null;
                            }
                            break;
                        }
                    }
                }

                if (orphanBlock != null)
                {
                    // Unable to reattach orphan.
                    parentConnection.Disconnect();
                    Connection orphanBlockCon = orphanBlock.OutputConnection != null ? orphanBlock.OutputConnection : orphanBlock.PreviousConnection;
                    orphanBlockCon.FireUpdate(UpdateState.BumpedAway);
                }
                
                // Restore the shadow DOM.
                parentConnection.ShadowDom = shadowDom;
            }
            
            // Establish the connections.
            Connection.ConnectReciprocally(parentConnection, childConnection);
            // Demote the inferior block so that one is a child of the superior one.
            childBlock.SetParent(parentBlock);

            FireUpdate(UpdateState.Connected);
        }

        /// <summary>
        /// Sever all links to this connection (not including from the source object).
        /// </summary>
        public void Dispose()
        {
            if (this.IsConnected)
            {
                throw new Exception("Disconnect connection before disposing of it.");
            }
            if (this.InDB)
            {
                this.DB.RemoveConnection(this);
            }

            this.DB = null;
            this.DBOpposite = null;
        }

        /// <summary>
        /// Checks whether the current connection can connect with the target connection.
        /// </summary>
        public int CanConnectWithReason(Connection target)
        {
            if (target == null)
                return Connection.REASON_TARGET_NULL;

            var blockA = this.IsSuperior ? this.mSourceBlock : target.SourceBlock;
            var blockB = this.IsSuperior ? target.SourceBlock : this.mSourceBlock;

            if (blockA != null && blockA == blockB)
            {
                return Connection.REASON_SELF_CONNECTION;
            }
            if (target.Type != Define.OppositeConnection(this.Type))
            {
                return Connection.REASON_WRONG_TYPE;
            }
            if (blockA != null && blockB != null && blockA.Workspace != blockB.Workspace)
            {
                return Connection.REASON_DIFFERENT_WORKSPACES;
            }
            if (!this.CheckType(target))
            {
                return Connection.REASON_CHECKS_FAILED;
            }
            if (blockA != null && blockB != null && blockA.IsShadow && !blockB.IsShadow)
            {
                return Connection.REASON_SHADOW_PARENT;
            }
            return Connection.CAN_CONNECT;
        }

        /// <summary>
        /// Checks whether the current connection and target connection are compatible and throws an exception if they are not.
        /// </summary>
        public void CheckConnection(Connection target)
        {
            switch (CanConnectWithReason(target))
            {
                case Connection.CAN_CONNECT:
                    break;
                case Connection.REASON_SELF_CONNECTION:
                    throw new Exception("Attempted to connect a block to itself.");
                case Connection.REASON_DIFFERENT_WORKSPACES:
                    // Usually this means one block has been deleted.
                    throw new Exception("Blocks not on same workspace.");
                case Connection.REASON_WRONG_TYPE:
                    throw new Exception("Attempt to connect incompatible types.");
                case Connection.REASON_TARGET_NULL:
                    throw new Exception("Target connection is null.");
                case Connection.REASON_CHECKS_FAILED:
                    StringBuilder thisCheckStr = new StringBuilder();
                    foreach (var c in Check)
                        thisCheckStr.Append(c + ", ");
                    StringBuilder targetCheckStr = new StringBuilder();
                    foreach (var c in target.Check)
                        targetCheckStr.Append(c + ", ");
                    throw new Exception(string.Format("Connection checks failed. {0} expected {1}, found {2}", this.ToString(), thisCheckStr, targetCheckStr));
                case Connection.REASON_SHADOW_PARENT:
                    throw new Exception("Connecting non-shadow to shadow block.");
                default:
                    throw new Exception("Unknown connection failure: this should never happen!");   
            }
        }

        /// <summary>
        /// Check if the two connections can be dragged to connect to each other.
        /// </summary>
        public bool IsConnectionAllowed(Connection candidate, int maxRadius = 0)
        {
            int canConnect = this.CanConnectWithReason(candidate);
            if (canConnect != Connection.CAN_CONNECT)
                return false;
            
            // Don't offer to connect an already connected left (male) value plug to
            // an available right (female) value plug.  Don't offer to connect the
            // bottom of a statement block to one that's already connected.
            if (candidate.Type == Define.EConnection.OutputValue|| candidate.Type == Define.EConnection.PrevStatement)
            {
                if (candidate.IsConnected || this.IsConnected)
                    return false;
            }

            // Offering to connect the left (male) of a value block to an already
            // connected value pair is ok, we'll splice it in.
            // However, don't offer to splice into an immovable block.
            if (candidate.Type == Define.EConnection.InputValue && candidate.IsConnected
                && !candidate.TargetBlock.Movable && !candidate.TargetBlock.IsShadow)
            {
                return false;
            }

            // Don't let a block with no next connection bump other blocks out of the
            // stack.  But covering up a shadow block or stack of shadow blocks is fine.
            // Similarly, replacing a terminal statement with another terminal statement
            // is allowed.
            if (this.Type == Define.EConnection.PrevStatement && candidate.IsConnected
                && this.SourceBlock.NextConnection == null && !candidate.TargetBlock.IsShadow
                && candidate.TargetBlock.NextConnection != null)
            {
                return false;
            }

            if (maxRadius > 0 && this.DistanceFrom(candidate) > maxRadius)
                return false;
            
            return true;
        }

        public static void ConnectReciprocally(Connection first, Connection second)
        {
            if (first == null || second == null)
                throw new Exception("Cannot connect null connections.");
            first.TargetConnection = second;
            second.TargetConnection = first;
        }

        /// <summary>
        /// Does the given block have one and only one connection point that will accept an orphaned block?
        /// </summary>
        /// <returns>The suitable connection point on 'block', or null.</returns>
        public static Connection SingleConnection(Block block, Block orphanBlock)
        {
            Connection connection = null;
            foreach (var input in block.InputList)
            {
                var thisConnection = input.Connection;
                if (thisConnection != null && thisConnection.Type == Define.EConnection.InputValue
                    && orphanBlock.OutputConnection.CheckType(thisConnection))
                {
                    if (connection != null)
                    {
                        //more than one connection
                        return null;
                    }
                    connection = thisConnection;
                }
            }
            return connection;
        }
        
        /// <summary>
        /// Walks down a row a blocks, at each stage checking if there are any connections that will accept the orphaned block.  
        /// If at any point there are zero or multiple eligible connections, returns null.  
        /// Otherwise returns the only input on the last block in the chain.
        /// Terminates early for shadow blocks.
        /// </summary>
        /// <param name="startBlock">The block on which to start the search</param>
        /// <param name="orphanBlock">The block that is looking for a home</param>
        /// <returns>The suitable connection point on the chain of blocks, or null.</returns>
        public static Connection LastConnectionInRow(Block startBlock, Block orphanBlock)
        {
            var newBlock = startBlock;
            Connection connection = Connection.SingleConnection(newBlock, orphanBlock);
            while (connection != null)
            {
                newBlock = connection.TargetBlock;
                if (newBlock == null || newBlock.IsShadow)
                    return connection;

                connection = Connection.SingleConnection(newBlock, orphanBlock);
            }
            return null;
        }

        /// <summary>
        /// Disconnect this connection.
        /// </summary>
        public void Disconnect()
        {
            if (!IsConnected) return;
            
            var otherConnection = TargetConnection;
            if (otherConnection.TargetConnection != this)
            {
                Debug.LogWarning("Target connection not connected to source connection.");
                return;
            }

            if (this.IsSuperior)
            {
                this.DisconnectInternal(otherConnection);
                this.RespawnShadow();
            }
            else
            {
                otherConnection.DisconnectInternal(this);
                otherConnection.RespawnShadow();
            }
        }

        private void DisconnectInternal(Connection childConnection)
        {
            var otherConnection = this.TargetConnection;
            otherConnection.TargetConnection = null;
            this.TargetConnection= null;
            childConnection.SourceBlock.SetParent(null);
            FireUpdate(UpdateState.Disconnected);
        }

        /// <summary>
        /// Respawn the shadow block if there was one connected to this connection.
        /// </summary>
        public void RespawnShadow()
        {
            var parentBlock = this.SourceBlock;
            var shadow = this.ShadowDom;
            if (parentBlock.Workspace != null && shadow != null /*&& Events.recordUndo*/)
            {
                var blockShadow = Xml.DomToBlock(shadow, parentBlock.Workspace);
                if (blockShadow.OutputConnection != null)
                {
                    this.Connect(blockShadow.OutputConnection);
                }
                else if (blockShadow.PreviousConnection != null)
                {
                    this.Connect(blockShadow.PreviousConnection);
                }
                else
                {
                    throw new Exception("Child block does not have output or previous statement.");
                }
            }
        }

        /// <summary>
        /// Is this connection compatible with another connection with respect to the
        /// value type system.  E.g. square_root("Hello") is not compatible.
        /// </summary>
        /// <param name="otherConnection"></param>
        /// <returns></returns>
        public bool CheckType(Connection otherConnection)
        {
            if (this.Check == null || otherConnection.Check == null)
                return true;

            foreach (var i in this.Check)
            {
                if (otherConnection.Check.Contains(i))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Function to be called when this connection's compatible types have changed.
        /// </summary>
        protected virtual void OnCheckChanged()
        {
            // The new value type may not be compatible with the existing connection.
            if (this.IsConnected && !this.CheckType(this.TargetConnection))
            {
                var child = this.IsSuperior ? this.TargetBlock : this.SourceBlock;
                child.UnPlug();
            }
        }

        /// <summary>
        /// Change a connection's compatibility.
        /// </summary>
        /// <param name="check"></param>
        public void SetCheck(List<string> check)
        {
            if (check == null || check.Count == 0)
            {
                this.Check = null;
                return;
            }
            
            this.Check = check;
            this.OnCheckChanged();
        }

        /// <summary>
        ///  Find all nearby compatible connections to this connection.
        /// Type checking does not apply, since this function is used for bumping.
        /// </summary>
        /// <returns>List of connections</returns>
        public virtual List<Connection> Neighbours(int maxLimit)
        {
            return DBOpposite.GetNeighbours(this, maxLimit);
        }

        /// <summary>
        /// Returns the distance between this connection and another connection in workspace units.
        /// </summary>
        public int DistanceFrom(Connection otherConnection)
        {
            var dx = this.X - otherConnection.X;
            var dy = this.Y - otherConnection.Y;
            return (int) Math.Sqrt(dx * dx + dy * dy);
        }

        public override string ToString()
        {
            string msg = null;
            if (mSourceBlock == null || mSourceBlock.InputList == null)
                return "Orphan Connection";
            if (mSourceBlock.OutputConnection == this)
                msg = "Output Connection of ";
            else if (mSourceBlock.PreviousConnection == this)
                msg = "Previous Connection of ";
            else if (mSourceBlock.NextConnection == this)
                msg = "Next Connection of ";
            else
            {
                Input parentInput = mSourceBlock.InputList.Find(i => i.Connection == this);
                if (parentInput == null)
                {
                    return "Orphan Connection";
                }
                msg = string.Format("Input \"{0}\" Connection on", parentInput.Name);
            }
            return msg + mSourceBlock.ToDevString();
        }
    }
}
