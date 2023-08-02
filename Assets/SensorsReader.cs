using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

using Gyroscope = UnityEngine.InputSystem.Gyroscope;

[RequireComponent(typeof(TextMeshProUGUI))]
public class SensorsReader : MonoBehaviour
{
    private TextMeshProUGUI text;
    Vector3 angularVelocity;
    Vector3 acceleration;
    Vector3 attitude;
    Vector3 gravity;

    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();

        InputSystem.EnableDevice(Gyroscope.current);
        InputSystem.EnableDevice(Accelerometer.current);
        InputSystem.EnableDevice(AttitudeSensor.current);
        InputSystem.EnableDevice(GravitySensor.current);
    }

    void Update()
    {
        angularVelocity = Gyroscope.current.angularVelocity.ReadValue();
        acceleration = Accelerometer.current.acceleration.ReadValue();
        attitude = AttitudeSensor.current.attitude.ReadValue().eulerAngles; // ReadValue() returns a Quaternion
        gravity = GravitySensor.current.gravity.ReadValue();

        text.text = $"Angular Velocity\nX={angularVelocity.x:#0.00} Y={angularVelocity.y:#0.00} Z={angularVelocity.z:#0.00}\n\n" +
                        $"Acceleration\nX={acceleration.x:#0.00} Y={acceleration.y:#0.00} Z={acceleration.z:#0.00}\n\n" +
                            $"Attitude\nX={attitude.x:#0.00} Y={attitude.y:#0.00} Z={attitude.z:#0.00}\n\n" +
                             $"Gravity\nX={gravity.x:#0.00} Y={gravity.y:#0.00} Z={gravity.z:#0.00}";
    }
}