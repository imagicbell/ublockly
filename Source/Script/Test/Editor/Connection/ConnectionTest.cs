using System;
using NUnit.Framework;
using UnityEngine;

namespace PTGame.Blockly.Test
{
    public class ConnectionTest
    {
        private Workspace mWorkspace;
        private Connection mInput;
        private Connection mOutput;
        private Connection mPrevious;
        private Connection mNext;

        void Setup()
        {
            mWorkspace = new Workspace();

            Func<Block> createBlock = () =>
            {
                Block block = new Block();
                block.Workspace = mWorkspace;
                return block;
            };

            mInput = new Connection(createBlock(), Blockly.INPUT_VALUE);
            mOutput = new Connection(createBlock(), Blockly.OUTPUT_VALUE);
            mPrevious = new Connection(createBlock(), Blockly.PREVIOUS_STATEMENT);
            mNext = new Connection(createBlock(), Blockly.NEXT_STATEMENT);
        }

        void TearDown()
        {
            mInput = null;
            mOutput = null;
            mPrevious = null;
            mNext = null;
            mWorkspace = null;
        }

        private Func<bool> mIsMovableFn = () => true;

        private Connection CreateConnection(Block sourceBlock, Vector2<int> location, int type)
        {
            return new Connection(sourceBlock, type)
            {
                Location = location
            };
        }

        private Block MakeSourceBlock()
        {
            return new Block()
            {
                Workspace = mWorkspace,
                Movable = true,
                IsShadow = false
            };
        }

        [Test]
        public void TestCanConnectWithReason_TargetNull()
        {
            Setup();
            Assert.AreEqual(Connection.REASON_TARGET_NULL, mInput.CanConnectWithReason(null));
            TearDown();
        }

        [Test]
        public void TestCanConnectWithReason_Disconnect()
        {
            Setup();

            var tempConnection = new Connection(new Block() {Workspace = mWorkspace}, Blockly.OUTPUT_VALUE);
            Connection.ConnectReciprocally(mInput, tempConnection);
            Assert.AreEqual(Connection.CAN_CONNECT, mInput.CanConnectWithReason(mOutput));
            
            TearDown();
        }

        [Test]
        public void TestCanConnnectWithReason_DifferentWorkspace()
        {
            Setup();

            mInput = new Connection(new Block() {Workspace = new Workspace()}, Blockly.INPUT_VALUE);
            Assert.AreEqual(Connection.REASON_DIFFERENT_WORKSPACES, mInput.CanConnectWithReason(mOutput));
            
            TearDown();
        }

        [Test]
        public void TestCanConnectWithReason_Self()
        {
            Setup();
            Assert.AreEqual(Connection.REASON_SELF_CONNECTION, mInput.CanConnectWithReason(mInput));
            TearDown();
        }

        [Test]
        public void TestCanConnectWithReason_Type()
        {
            Setup();

            Assert.AreEqual(Connection.REASON_WRONG_TYPE, mInput.CanConnectWithReason(mPrevious));
            Assert.AreEqual(Connection.REASON_WRONG_TYPE, mInput.CanConnectWithReason(mNext));
            
            Assert.AreEqual(Connection.REASON_WRONG_TYPE, mOutput.CanConnectWithReason(mPrevious));
            Assert.AreEqual(Connection.REASON_WRONG_TYPE, mOutput.CanConnectWithReason(mNext));
            
            Assert.AreEqual(Connection.REASON_WRONG_TYPE, mPrevious.CanConnectWithReason(mInput));
            Assert.AreEqual(Connection.REASON_WRONG_TYPE, mPrevious.CanConnectWithReason(mOutput));
            
            Assert.AreEqual(Connection.REASON_WRONG_TYPE, mNext.CanConnectWithReason(mInput));
            Assert.AreEqual(Connection.REASON_WRONG_TYPE, mNext.CanConnectWithReason(mOutput));
            
            TearDown();
        }

        [Test]
        public void TestCanConnectWithReason_CanConnect()
        {
            Setup();
            
            Assert.AreEqual(Connection.CAN_CONNECT, mPrevious.CanConnectWithReason(mNext));
            Assert.AreEqual(Connection.CAN_CONNECT, mNext.CanConnectWithReason(mPrevious));
            Assert.AreEqual(Connection.CAN_CONNECT, mInput.CanConnectWithReason(mOutput));
            Assert.AreEqual(Connection.CAN_CONNECT, mOutput.CanConnectWithReason(mInput));
            
            TearDown();
        }

        [Test]
        public void TestCheckConnection_Self()
        {
            Setup();
            //mInput = new Connection(new Block() {Type = "test block"}, Blockly.INPUT_VALUE);
            try
            {
                mInput.CheckConnection(mInput);
                Assert.Fail();
            }
            catch (Exception e)
            {
                //expected
            }
            
            TearDown();
        }

        [Test]
        public void TestCheckConnection_TypeInputPrev()
        {
            Setup();

            try
            {
                mInput.CheckConnection(mPrevious);
            }
            catch (Exception e)
            {
                //expected
            }
            
            TearDown();
        }

        [Test]
        public void TestCheckConnection_TypeOutputPrev()
        {
            Setup();
            try
            {
                mOutput.CheckConnection(mPrevious);
            }
            catch (Exception e)
            {
                //expected
            }
            TearDown();
        }

        [Test]
        public void TestCheckConnection_TypePrevInput()
        {
            Setup();

            try
            {
                mPrevious.CheckConnection(mInput);
            }
            catch (Exception e)
            {
                //expected
            }
            TearDown();
        }

        [Test]
        public void TestCheckConnection_TypePrevOutput()
        {
            Setup();

            try
            {
                mPrevious.CheckConnection(mOutput);
            }
            catch (Exception e)
            {
                //expected
            }
            TearDown();
        }

        [Test]
        public void TestCheckConnection_TypeNextInput()
        {
            Setup();

            try
            {
                mNext.CheckConnection(mInput);
            }
            catch (Exception e)
            {
                //expected
            }
            TearDown();
        }
        
        [Test]
        public void TestCheckConnection_TypeNextOutput()
        {
            Setup();

            try
            {
                mNext.CheckConnection(mOutput);
            }
            catch (Exception e)
            {
                //expected
            }
            TearDown();
        }

        [Test]
        public void TestIsConnectionAllowed_Distance()
        {
            Setup();
            
            Block sourceBlock = MakeSourceBlock();
            Connection one = CreateConnection(sourceBlock, new Vector2<int>(5, 10), Blockly.INPUT_VALUE);

            sourceBlock = MakeSourceBlock();
            Connection two = CreateConnection(sourceBlock, new Vector2<int>(10, 15), Blockly.OUTPUT_VALUE);

            Assert.True(one.IsConnectionAllowed(two, 20));

            two.Location = new Vector2<int>(100, 100);
            Assert.False(one.IsConnectionAllowed(two, 20));
            
            TearDown();
        }
        
        [Test]
        public void TestIsConnectionAllowed_Unrendered()
        {
            Setup();
            
            Block sourceBlock = MakeSourceBlock();
            Connection one = CreateConnection(sourceBlock, new Vector2<int>(5, 10), Blockly.INPUT_VALUE);
            
            sourceBlock = MakeSourceBlock();
            Connection two = CreateConnection(sourceBlock, new Vector2<int>(0, 0), Blockly.OUTPUT_VALUE);
            
            Assert.True(one.IsConnectionAllowed(two));
            
            sourceBlock = MakeSourceBlock();
            Connection three = CreateConnection(sourceBlock, new Vector2<int>(0, 0), Blockly.INPUT_VALUE);

            Connection.ConnectReciprocally(two, three);
            Assert.False(one.IsConnectionAllowed(two));

            two = CreateConnection(one.SourceBlock, new Vector2<int>(0, 0), Blockly.OUTPUT_VALUE);
            Assert.False(one.IsConnectionAllowed(two));
            
            TearDown();
        }

        [Test]
        public void TestIsConnectionAllowed_NoNext()
        {
            Setup();
            
            Block sourceBlock = MakeSourceBlock();
            Connection one = CreateConnection(sourceBlock, new Vector2<int>(0, 0), Blockly.NEXT_STATEMENT);
            one.SourceBlock.NextConnection = one;
            
            sourceBlock = MakeSourceBlock();
            Connection two = CreateConnection(sourceBlock, new Vector2<int>(0, 0), Blockly.PREVIOUS_STATEMENT);
            
            Assert.True(two.IsConnectionAllowed(one));
            
            sourceBlock = MakeSourceBlock();
            Connection three = CreateConnection(sourceBlock, new Vector2<int>(0, 0), Blockly.PREVIOUS_STATEMENT);
            three.SourceBlock.PreviousConnection = three;
            Connection.ConnectReciprocally(one, three);
            
            Assert.True(two.IsConnectionAllowed(one));
            
            TearDown();
        }

        [Test]
        public void TestCheckConnectionOkay()
        {
            Setup();
            
            mPrevious.CheckConnection(mNext);
            mNext.CheckConnection(mPrevious);
            mInput.CheckConnection(mOutput);
            mOutput.CheckConnection(mInput);
            
            TearDown();
        }
    }
}