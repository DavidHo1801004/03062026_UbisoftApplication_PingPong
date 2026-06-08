using UnityEngine;
using Developer.MotorControl;
using LMK.Attribute;

public class GeneralTestScript : MonoBehaviour
{
    [SerializeField]
    private RigidbodyMotor2D motor;

    [SerializeReference, InlineReference]
    private MotorControlModule module;

    private void Start()
    {
        motor.AddControlModule(module);
    }
}
