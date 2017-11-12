using System.Collections.Generic;
using NUnit.Framework;

namespace UBlockly.Test
{
    public class ConnectionDBTest
    {
        private void VerifyDB(string msg, ConnectionDB expected, ConnectionDB db)
        {
            bool equal = expected.Count == db.Count;
            if (equal)
            {
                for (int i = 0; i < expected.Count; i++)
                {
                    if (expected[i] != db[i])
                    {
                        equal = false;
                        break;
                    }
                }
            }
            if (equal)
            {
                Assert.True(true, msg);
            }
            else
            {
                Assert.AreEqual(expected, db, msg);
            }
        }

        [Test]
        public void TestDB_AddConnection()
        {
            Workspace workspace = new Workspace();
            var db = new ConnectionDB();
            
            var o2 = new Connection(new Block{Workspace = workspace}, Define.EConnection.InputValue);
            o2.Y = 2;
            db.AddConnection(o2);
            VerifyDB("Adding connection #2", new ConnectionDB() {o2}, db);

            var o4 = new Connection(new Block{Workspace = workspace}, Define.EConnection.InputValue);
            o4.Y = 4;
            db.AddConnection(o4);
            VerifyDB("Adding connection #4", new ConnectionDB() {o2, o4}, db);
            
            var o1 = new Connection(new Block{Workspace = workspace}, Define.EConnection.InputValue);
            o1.Y = 1;
            db.AddConnection(o1);
            VerifyDB("Adding connection #1", new ConnectionDB() {o1, o2, o4}, db);
            
            var o3a = new Connection(new Block{Workspace = workspace}, Define.EConnection.InputValue);
            o3a.Y = 3;
            db.AddConnection(o3a);
            VerifyDB("Adding connection #3a", new ConnectionDB() {o1, o2, o3a, o4}, db);
            
            var o3b = new Connection(new Block{Workspace = workspace}, Define.EConnection.InputValue);
            o3b.Y = 3;
            db.AddConnection(o3b);
            VerifyDB("Adding connection #3a", new ConnectionDB() {o1, o2, o3b, o3a, o4}, db);
        }

        [Test]
        public void TestDB_RemoveConnection()
        {
            Workspace workspace = new Workspace();
            var db = new ConnectionDB();
            
            var o1 = new Connection(new Block{Workspace = workspace}, Define.EConnection.InputValue);
            o1.Y = 1;
            
            var o2 = new Connection(new Block{Workspace = workspace}, Define.EConnection.InputValue);
            o2.Y = 2;
            
            var o3a = new Connection(new Block{Workspace = workspace}, Define.EConnection.InputValue);
            o3a.Y = 3;
            
            var o3b = new Connection(new Block{Workspace = workspace}, Define.EConnection.InputValue);
            o3b.Y = 3;
            
            var o3c = new Connection(new Block{Workspace = workspace}, Define.EConnection.InputValue);
            o3c.Y = 3;
            
            var o4 = new Connection(new Block{Workspace = workspace}, Define.EConnection.InputValue);
            o4.Y = 4;
            
            db.AddConnection(o1);
            db.AddConnection(o2);
            db.AddConnection(o3c);
            db.AddConnection(o3b);
            db.AddConnection(o3a);
            db.AddConnection(o4);

            VerifyDB("Adding connections 1-4", new ConnectionDB() {o1, o2, o3a, o3b, o3c, o4}, db);
            
            db.RemoveConnection(o2);
            VerifyDB("Removing connection #2", new ConnectionDB() {o1, o3a, o3b, o3c, o4}, db);
            
            db.RemoveConnection(o4);
            VerifyDB("Removing connection #4", new ConnectionDB() {o1, o3a, o3b, o3c}, db);
            
            db.RemoveConnection(o1);
            VerifyDB("Removing connection #1", new ConnectionDB() {o3a, o3b, o3c}, db);
            
            db.RemoveConnection(o3a);
            VerifyDB("Removing connection #3a", new ConnectionDB() {o3b, o3c}, db);
            
            db.RemoveConnection(o3c);
            VerifyDB("Removing connection #3c", new ConnectionDB() {o3b}, db);
            
            db.RemoveConnection(o3b);
            VerifyDB("Removing connection #3b", new ConnectionDB(), db);
        }

        [Test]
        public void TestDB_GetNeighbours()
        {
            var db = new ConnectionDB();
            Workspace workspace = new Workspace();

            Assert.AreEqual(GetNeighbours(db, new Vector2<int>(10, 10), 10).Count, 0);

            for (int i = 0; i < 10; i++)
            {
                db.AddConnection(CreateConnection(new Vector2<int>(0, i), Define.EConnection.PrevStatement, new Workspace()));
            }

            // Test block belongs at beginning.
            var result = GetNeighbours(db, new Vector2<int>(0, 0), 4);
            Assert.AreEqual(5, result.Count);
            for (int i = 0; i < result.Count; i++)
            {
                Assert.AreNotEqual(result.IndexOf(db[i]), -1);
            }
            
            // Test block belongs at middle.
            result = GetNeighbours(db, new Vector2<int>(0, 4), 2);
            Assert.AreEqual(5, result.Count);
            for (int i = 0; i < result.Count; i++)
            {
                Assert.AreNotEqual(result.IndexOf(db[i + 2]), -1);
            }
            
            // Test block belongs at end.
            result = GetNeighbours(db, new Vector2<int>(0, 9), 4);
            Assert.AreEqual(5, result.Count);
            for (int i = 0; i < result.Count; i++)
            {
                Assert.AreNotEqual(result.IndexOf(db[i + 5]), -1);
            }
            
            // Test block has no neighbours due to being out of range in the x direction.
            result = GetNeighbours(db, new Vector2<int>(10, 9), 4);
            Assert.AreEqual(0, result.Count);
            
            // Test block has no neighbours due to being out of range in the y direction.
            result = GetNeighbours(db, new Vector2<int>(0, 19), 4);
            Assert.AreEqual(0, result.Count);
            
            // Test block has no neighbours due to being out of range diagonally.
            result = GetNeighbours(db, new Vector2<int>(-2, -2), 2);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void TestDB_FindPositionForConnection()
        {
            var db = new ConnectionDB();
            var workspace = new Workspace();
            db.AddConnection(CreateConnection(new Vector2<int>(0, 0), Define.EConnection.PrevStatement, workspace));
            db.AddConnection(CreateConnection(new Vector2<int>(0, 1), Define.EConnection.PrevStatement, workspace));
            db.AddConnection(CreateConnection(new Vector2<int>(0, 2), Define.EConnection.PrevStatement, workspace));
            db.AddConnection(CreateConnection(new Vector2<int>(0, 4), Define.EConnection.PrevStatement, workspace));
            db.AddConnection(CreateConnection(new Vector2<int>(0, 5), Define.EConnection.PrevStatement, workspace));
            Assert.AreEqual(5, db.Count);

            var connection = CreateConnection(new Vector2<int>(0, 3), Define.EConnection.PrevStatement, workspace);
            Assert.AreEqual(3, db.FindPositionForConnection(connection));
        }

        [Test]
        public void TestDB_FindConnection()
        {
            var db = new ConnectionDB();
            var workspace = new Workspace();
            for (int i = 0; i < 10; i++)
            {
                db.AddConnection(CreateConnection(new Vector2<int>(i, 0), Define.EConnection.PrevStatement, workspace));
                db.AddConnection(CreateConnection(new Vector2<int>(0, i), Define.EConnection.PrevStatement, workspace));
            }
         
            var connection = CreateConnection(new Vector2<int>(3, 3), Define.EConnection.PrevStatement, workspace);
            db.AddConnection(connection);
            Assert.AreEqual(connection, db[db.FindConnection(connection)]);
            
            connection = CreateConnection(new Vector2<int>(3, 3), Define.EConnection.PrevStatement, workspace);
            Assert.AreEqual(-1, db.FindConnection(connection));
        }

        [Test]
        public void TestDB_Ordering()
        {
            var db = new ConnectionDB();
            var workspace = new Workspace();
            for (int i = 0; i < 10; i++)
            {
                db.AddConnection(CreateConnection(new Vector2<int>(0, 9-i), Define.EConnection.PrevStatement, workspace));
            }

            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(i, db[i].Y);
            }
            
            // quasi-random, low discrepancy sequence (https://en.wikipedia.org/wiki/Low-discrepancy_sequence)
            int[] xCoords =
            {
                -29, -47, -77, 2, 43, 34, -59, -52, -90, -36, -91, 38, 87, -20,
                60, 4, -57, 65, -37, -81, 57, 58, -96, 1, 67, -79, 34, 93, -90, -99, -62,
                4, 11, -36, -51, -72, 3, -50, -24, -45, -92, -38, 37, 24, -47, -73, 79,
                -20, 99, 43, -10, -87, 19, 35, -62, -36, 49, 86, -24, -47, -89, 33, -44,
                25, -73, -91, 85, 6, 0, 89, -94, 36, -35, 84, -9, 96, -21, 52, 10, -95, 7,
                -67, -70, 62, 9, -40, -95, -9, -94, 55, 57, -96, 55, 8, -48, -57, -87, 81,
                23, 65
            };

            int[] yCoords =
            {
                -81, 82, 5, 47, 30, 57, -12, 28, 38, 92, -25, -20, 23, -51, 73,
                -90, 8, 28, -51, -15, 81, -60, -6, -16, 77, -62, -42, -24, 35, 95, -46,
                -7, 61, -16, 14, 91, 57, -38, 27, -39, 92, 47, -98, 11, -33, -72, 64, 38,
                -64, -88, -35, -59, -76, -94, 45, -25, -100, -95, 63, -97, 45, 98, 99, 34,
                27, 52, -18, -45, 66, -32, -38, 70, -73, -23, 5, -2, -13, -9, 48, 74, -97,
                -11, 35, -79, -16, -77, 83, -57, -53, 35, -44, 100, -27, -15, 5, 39, 33,
                -19, -20, -95
            };

            for (int i = 0; i < xCoords.Length; i++)
            {
                db.AddConnection(CreateConnection(new Vector2<int>(xCoords[i], yCoords[i]), Define.EConnection.PrevStatement, workspace));
            }

            for (int i = 1; i < xCoords.Length; i++)
            {
                Assert.True(db[i].Y >= db[i - 1].Y);
            }
        }

        [Test]
        public void TestDB_SearchForClosest()
        {
            var db = new ConnectionDB();
            var workspace = new Workspace()
            {
                Id = "Shared workspace"
            };

            Assert.AreEqual(null, SearchDB(db, new Vector2<int>(10, 10), 100, workspace));

            db.AddConnection(CreateConnection(new Vector2<int>(100, 0), Define.EConnection.PrevStatement, workspace));
            Assert.AreEqual(null, SearchDB(db, new Vector2<int>(0, 0), 5, workspace));
            
            db = new ConnectionDB();
            for (int i = 0; i < 10; i++)
            {
                Block block = new Block()
                {
                    Workspace = workspace,
                    Movable = true,
                    IsShadow = false
                };
                var tempConnection = new Connection(block, Define.EConnection.PrevStatement);
                tempConnection.Location = new Vector2<int>(0, i);
                db.AddConnection(tempConnection);
            }
            
            //should be at 0, 9
            var last = db[db.Count - 1];
            Assert.AreEqual(last, SearchDB(db, new Vector2<int>(0, 10), 15, workspace));
            //nothing nearby
            Assert.AreEqual(null, SearchDB(db, new Vector2<int>(100, 100), 15, workspace));
            //firt in db, exact match
            Assert.AreEqual(db[0], SearchDB(db, new Vector2<int>(0,0), 0, workspace));
            
            var temp = CreateConnection(new Vector2<int>(6,6),  Define.EConnection.PrevStatement, workspace);
            db.AddConnection(temp);
            temp = CreateConnection(new Vector2<int>(5, 5), Define.EConnection.PrevStatement, workspace);
            db.AddConnection(temp);

            var result = SearchDB(db, new Vector2<int>(4, 6), 3, workspace);
            Assert.AreEqual(5, result.X);
            Assert.AreEqual(5, result.Y);
        }

        List<Connection> GetNeighbours(ConnectionDB db, Vector2<int> location, int radius)
        {
            return db.GetNeighbours(CreateConnection(location, Define.EConnection.NextStatement, new Workspace()), radius);
        }

        private Connection CreateConnection(Vector2<int> location, Define.EConnection type, Workspace workspace)
        {
            return new Connection(new Block() {Workspace = workspace}, type)
            {
                Location = location
            };
        }

        private Connection SearchDB(ConnectionDB db, Vector2<int> location, int radius, Workspace workspace)
        {
            Block block = new Block()
            {
                Workspace = workspace,
                Movable = true,
                IsShadow = false
            };
            var tempConnection = new Connection(block, Define.EConnection.NextStatement);
            tempConnection.Location = location;
            Connection closet;
            int distance;
            db.SearchForClosest(tempConnection, radius, new Vector2<int>(0, 0), out closet, out distance);
            return closet;
        }
    }
}
