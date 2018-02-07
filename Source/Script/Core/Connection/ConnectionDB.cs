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

namespace UBlockly
{
    public class ConnectionDB : List<Connection>
    {
        /// <summary>
        /// Database of connections. 
        /// Connections are stored in order of their vertical component.  
        /// This way connections in an area may be looked up quickly using a binary search.
        /// </summary>
        public ConnectionDB()
        {
        }

        /// <summary>
        /// Add a connection to the database.  Must not already exist in DB.
        /// </summary>
        public void AddConnection(Connection connection)
        {
            if (connection.InDB)
            {
                throw new Exception("Connection already in database.");
            }

            var position = this.FindPositionForConnection(connection);
            this.Insert(position, connection);
            connection.InDB = true;
        }

        /// <summary>
        /// Find the given connection.
        /// Starts by doing a binary search to find the approximate location, 
        /// then linearly searches nearby for the exact connection.
        /// </summary>
        /// <returns>The index of the connection, or -1 if the connection was not found.</returns>
        public int FindConnection(Connection connection)
        {
            if (this.Count == 0)
                return -1;

            var bestGuess = this.FindPositionForConnection(connection);
            if (bestGuess >= this.Count)
            {
                return -1;
            }

            var yPos = connection.Location.y;
            var pointerMin = bestGuess;
            var pointerMax = bestGuess;
            while (pointerMin >= 0 && this[pointerMin].Location.y == yPos)
            {
                if (this[pointerMin] == connection)
                    return pointerMin;
                pointerMin--;
            }

            while (pointerMax < this.Count && this[pointerMax].Location.y == yPos)
            {
                if (this[pointerMax] == connection)
                    return pointerMax;
                pointerMax++;
            }
            
            return -1;
        }

        /// <summary>
        /// Finds a candidate position for inserting this connection into the list.
        /// This will be in the correct y order but makes no guarantees about ordering in the x axis.
        /// </summary>
        /// <returns>The candidate index.</returns>
        public int FindPositionForConnection(Connection connection)
        {
            if (this.Count == 0)
            {
                return 0;
            }

            var pointerMin = 0;
            var pointerMax = this.Count;
            while (pointerMin < pointerMax)
            {
                int pointerMid = (pointerMin + pointerMax) / 2;
                if (this[pointerMid].Location.y < connection.Location.y)
                {
                    pointerMin = pointerMid + 1;
                }
                else if (this[pointerMid].Location.y > connection.Location.y)
                {
                    pointerMax = pointerMid;
                }
                else
                {
                    pointerMin = pointerMid;
                    break;
                }
            }
            return pointerMin;
        }

        /// <summary>
        /// Remove a connection from the database.  Must already exist in DB.
        /// </summary>
        public void RemoveConnection(Connection connection)
        {
            if (!connection.InDB) return;

            var removeIndex = FindConnection(connection);
            if (removeIndex == -1)
                throw new Exception("Unable to find connection in connectionDB, but the connection's property \"InDB\" is true");

            connection.InDB = false;
            this.RemoveAt(removeIndex);
        }

        /// <summary>
        /// Find all nearby connections to the given connection.
        /// Type checking does not apply, since this function is used for bumping.
        /// </summary>
        /// <param name="maxRadius">The maximum radius to another connection.</param>
        public List<Connection> GetNeighbours(Connection connection, int maxRadius)
        {
            var currentX = connection.Location.x;
            var currentY = connection.Location.y;
            
            // Binary search to find the closest y location.
            int pointerMin = 0;
            int pointerMax = this.Count - 2;
            int pointerMid = pointerMax;
            while (pointerMin < pointerMid)
            {
                if (this[pointerMid].Location.y < currentY)
                    pointerMin = pointerMid;
                else
                    pointerMax = pointerMid;
                pointerMid = (pointerMin + pointerMax) / 2;
            }

            List<Connection> neighbours = new List<Connection>();

            //Computes if the current connection is within the allowed radius of another connection.
            Func<int, bool> checkConnection = (yIndex) =>
            {
                var c = this[yIndex];
                var dx = currentX - c.Location.x;
                var dy = currentY - c.Location.y;
                var r = Math.Sqrt(dx * dx + dy * dy);
                if (r <= maxRadius)
                    neighbours.Add(c);
                
                return dy < maxRadius;
            };

            // Walk forward and back on the y axis looking for the closest x,y point.
            pointerMin = pointerMid;
            pointerMax = pointerMid + 1;
            if (this.Count > 0)
            {
                while (pointerMin >= 0 && checkConnection(pointerMin))
                    pointerMin--;
                
                while (pointerMax < this.Count && checkConnection(pointerMax))
                    pointerMax++;
            }
            
            return neighbours;
        }

        /// <summary>
        /// Find the closest compatible connection to this connection.
        /// </summary>
        /// <param name="connection">The connection searching for a compatible mate.</param>
        /// <param name="maxRadius">The maximum radius to another connection.</param>
        /// <param name="dxy">Offset between this connection's location in the database and the current location (as a result of dragging).</param>
        /// <param name="closestConnection"></param>
        /// <param name="closestRadius">the distance to closestConnection found.</param>
        public void SearchForClosest(Connection connection, int maxRadius, Vector2<int> dxy, 
                                     out Connection closestConnection, out int closestRadius)
        {
            closestConnection = null;
            closestRadius = maxRadius;

            if (this.Count == 0) return;

            var baseX = connection.X;
            var baseY = connection.Y;

            connection.X = baseX + dxy.x;
            connection.Y = baseY + dxy.y;
            
            // findPositionForConnection finds an index for insertion, which is always
            // after any block with the same y index.  We want to search both forward
            // and back, so search on both sides of the index.
            var closestIndex = FindPositionForConnection(connection);

            Connection temp;

            //Is the candidate connection close to the reference connection.
            Func<int, int, int, bool> isInYRange = (idx, refY, refRadius) => Math.Abs(this[idx].Y - refY) <= refRadius;
            
            // Walk forward and back on the y axis looking for the closest x,y point.
            var pointerMin = closestIndex - 1;
            while (pointerMin >= 0 && isInYRange(pointerMin, connection.Y, maxRadius))
            {
                temp = this[pointerMin];
                if (connection.IsConnectionAllowed(temp, closestRadius))
                {
					closestConnection = temp;
                    closestRadius = temp.DistanceFrom(connection);
                }
                pointerMin--;
            }

            var pointerMax = closestIndex;
            while (pointerMax < this.Count && isInYRange(pointerMax, connection.Y, maxRadius))
            {
                temp = this[pointerMax];
                if (connection.IsConnectionAllowed(temp, closestRadius))
                {
					closestConnection = temp;
                    closestRadius = temp.DistanceFrom(connection);
                }
                pointerMax++;
            }

            connection.X = baseX;
            connection.Y = baseY;
        }

        /// <summary>
        /// Build a set of connection DBs
        /// </summary>
        public static Dictionary<Define.EConnection, ConnectionDB> Build()
        {
            // Create for databases,one for each connection type.
            var dbList = new Dictionary<Define.EConnection, ConnectionDB>();
            dbList.Add(Define.EConnection.InputValue, new ConnectionDB());
            dbList.Add(Define.EConnection.OutputValue, new ConnectionDB());
            dbList.Add(Define.EConnection.NextStatement, new ConnectionDB());
            dbList.Add(Define.EConnection.PrevStatement, new ConnectionDB());
            return dbList;
        }
    }
}
