using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Tests
{
    public class Test_Ability
    {
        private GameObject player;
        private Player pl;
        private GameObject mockBoost;
        private Boost boost;
        private GameObject mockIron;
        private Iron iron;


        [SetUp]
        public void Setup()
        {
            GameObject planet = new GameObject();
            planet.AddComponent<SpriteRenderer>();
            planet.AddComponent<Shadow>();
            planet.AddComponent<Planet>();
            planet.transform.position = new Vector2(0, 0);

            player = new GameObject();
            player.AddComponent<SpriteRenderer>();
            player.AddComponent<Rigidbody2D>();
            player.AddComponent<CircleCollider2D>();
            player.AddComponent<Shadow>();
            player.AddComponent<LineRenderer>();
            player.AddComponent<Player>();
            pl = player.GetComponent<Player>();
            pl.Body.position = new Vector2(100, 0);
            pl.Body.velocity = new Vector2(0, -pl.DefaultSpeed);
            pl.Speed = pl.Body.velocity.magnitude;
            pl.PlayerNumber = 1;

            InputAssign.players = new Player[1];
            InputAssign.players[0] = pl;

            GameObject cooldownslider = new GameObject();
            cooldownslider.AddComponent<Slider>();

            mockBoost = new GameObject();
            mockBoost.AddComponent<Boost>();
            boost = mockBoost.GetComponent<Boost>();
            boost.PlayerNumber = 1;
            boost.Cooldown = 6;
            boost.CooldownSlider = cooldownslider.GetComponent<Slider>();

            mockIron = new GameObject();
            mockIron.AddComponent<Iron>();
            mockIron.AddComponent<SpriteRenderer>();
            iron = mockIron.GetComponent<Iron>();
            iron.PlayerNumber = 1;
            iron.Cooldown = 4;
            iron.CooldownSlider = cooldownslider.GetComponent<Slider>();
        }

        [UnityTest]
        public IEnumerator IronTimers()
        {
            iron.StartAbility();
            Assert.IsTrue(iron.AbilityOnCooldown);

            yield return new WaitForSeconds(0.75f);
            Assert.IsTrue(iron.AbilityOnCooldown);
            Assert.AreEqual(iron.player.Body.mass, iron.IncreasedMass);

            yield return new WaitForSeconds(0.75f);
            Assert.IsTrue(iron.AbilityOnCooldown);
            Assert.AreEqual(iron.player.Body.mass, 1);

            yield return new WaitForSeconds(3.0f);
            Assert.IsFalse(iron.AbilityOnCooldown);
        }

        [UnityTest]
        public IEnumerator BoostTimer()
        {
            boost.StartAbility();
            Assert.IsTrue(boost.AbilityOnCooldown);

            yield return new WaitForSeconds(3.0f);
            Assert.IsTrue(boost.AbilityOnCooldown);
            Assert.AreEqual(boost.player.Speed, boost.player.DefaultSpeed, 1);

            yield return new WaitForSeconds(3.5f);
            Assert.IsFalse(boost.AbilityOnCooldown);
        }
    }
}
