using UnityEngine;

namespace Developer.MotorControl.Events
{
    public readonly struct MotorCollisionCallback
    {
        public readonly RigidbodyMotor2D motor;
        public readonly CollisionPhase phase;
        public readonly Collision2D collision;

        public MotorCollisionCallback(
            RigidbodyMotor2D _Motor,
            CollisionPhase _Phase,
            Collision2D _Collision)
        {
            motor = _Motor;
            phase = _Phase;
            collision = _Collision;
        }
    }

    public readonly struct MotorTriggerCallback
    {
        public readonly RigidbodyMotor2D motor;
        public readonly CollisionPhase phase;
        public readonly Collider2D otherCollider;

        public MotorTriggerCallback(
            RigidbodyMotor2D _Motor,
            CollisionPhase _Phase,
            Collider2D _OtherCollider)
        {
            motor = _Motor;
            phase = _Phase;
            otherCollider = _OtherCollider;
        }
    }

    public enum CollisionPhase
    {
        Enter,
        Exit,
    }
}
