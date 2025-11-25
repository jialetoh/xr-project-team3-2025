using Oculus.Interaction.Input;
using UnityEngine;

public class ControllerOriginalTransform : MonoBehaviour
{
    // Adapted from Assignment2 GrabPoint.cs
    // Tracks a controller's transform
    [SerializeField] private Controller controller;
    public Vector3 Position
    {
        get
        {
            controller.TryGetPose(out Pose controllerPose);
            return controllerPose.position;
        }
    }

    private void Update()
    {
        this.transform.position = Position;
    }
}
