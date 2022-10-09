using UnityEngine;

namespace ClientServerPrediction
{
    public struct Input
    {
        public Vector2 movement;
    }

    public struct State
    {
        public Vector2 position;
        public Vector2 velocity;
        public float rotation;
        public float angularVelocity;
    }

    public struct InputMessage
    {

    }

    public struct StateMessage
    {

    }
}
