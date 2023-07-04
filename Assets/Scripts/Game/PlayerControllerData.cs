using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerControllerData", menuName = "Player/PlayerControllerData", order = 1)]
public class PlayerControllerData : ScriptableObject
{
    [Header("Wind/Unwind")]
    [MinMaxSlider(0f, 20f)] public Vector2 RADIUS;
    [Range(0f, 1f)] public float WIND_TETHER_RATIO;
    [Range(0f, 1f)] public float UNWIND_TETHER_RATIO;

    [Header("Speed Boost")]
    [MinMaxSlider(0f, 50f)] public Vector2 SPEED;
    [Range(0f, 1f)] public float SPEED_FALLOFF;
    [Range(0f, 1f)] public float SPEED_BOOST_RAMP;
    [Range(0f, 10f)] public float SPEED_BOOST_COOLDOWN;
    [Range(0f, 50f)] public float GAS_INCREASE_TIME;
    [Range(0f, 30f)] public float GAS_DRAIN_TIME;
    [Range(0f, 50f)] public float BOOST_SPEED_MINIMUM;
    [Range(0f, 1f)] public float SPEED_MASS_MULTIPLIER;

    [Header("Heavy")]
    [Range(0f, 10f)] public float HEAVY_COOLDOWN;
    [Range(0f, 10f)] public float HEAVY_DURATION;
    [Range(0f, 20f)] public float HEAVY_MASS;

    [Header("Tethering")]
    [Range(1f, 10f)] public float STEER_RATE;

    [Header("Collision")]
    [Range(0f, 5f)] public float COLLISION_TETHER_DISABLED_DURATION;

    private void Reset()
    {
        RADIUS = new Vector2(1.25f, 7.25f);
        WIND_TETHER_RATIO = 0.11f;
        UNWIND_TETHER_RATIO = 0.22f;
        SPEED = new Vector2(12f, 35f);
        SPEED_FALLOFF = 0.85f;
        SPEED_BOOST_RAMP = 0.4f;
        SPEED_BOOST_COOLDOWN = 2f;
        GAS_INCREASE_TIME = 20f;
        GAS_DRAIN_TIME = 3f;
        BOOST_SPEED_MINIMUM = 20f;
        SPEED_MASS_MULTIPLIER = 0.5f;
        HEAVY_COOLDOWN = 0.5f;
        HEAVY_DURATION = 1f;
        HEAVY_MASS = 10f;
        STEER_RATE = 2f;
        COLLISION_TETHER_DISABLED_DURATION = 0.7f;
    }
}
