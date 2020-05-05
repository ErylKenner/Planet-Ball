using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class Test_PlayerInput
    {
        private PlayerInput input;

        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void PlayerInput_InvalidControllerIndex()
        {
            Assert.Throws<System.ArgumentException>(() => input = new PlayerInput(0));
        }

        [Test]
        public void PlayerInput_ValidControllerIndex()
        {
            input = new PlayerInput(1);
            Assert.AreEqual(1, input.ControllerIndex);
        }


        [Test]
        public void PlayerInput_ButtonString()
        {
            input = new PlayerInput(1);
            Assert.AreEqual("R_C1", input.Button("R"));
        }

        [Test]
        public void PlayerInput_ButtonInt()
        {
            input = new PlayerInput(1);
            Assert.AreEqual("A_C1", input.Button(1));
        }

        [Test]
        public void PlayerInput_Axis()
        {
            input = new PlayerInput(1);
            Assert.AreEqual("R_C1", input.Axis("R"));
        }

    }
}
