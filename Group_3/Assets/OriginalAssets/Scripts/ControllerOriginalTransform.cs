using Oculus.Interaction.Input;
using UnityEngine;

public class ControllerOriginalTransform : MonoBehaviour
{
    // Adapted from Assignment2 GrabPoint.cs
    // Tracks a controller's transform (position and rotation)
    [SerializeField] private Controller controller;

    public Vector3 Position
    {
        get
        {
            controller.TryGetPose(out Pose controllerPose);
            return controllerPose.position;
        }
    }

    public Quaternion Rotation
    {
        get
        {
            controller.TryGetPose(out Pose controllerPose);
            return controllerPose.rotation;
        }
    }

    private void Update()
    {
        if (controller.TryGetPose(out Pose controllerPose))
        {
            transform.SetPositionAndRotation(controllerPose.position, controllerPose.rotation);
        }
    }
}
