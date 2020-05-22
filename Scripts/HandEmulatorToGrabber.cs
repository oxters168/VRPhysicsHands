using UnityEngine;
using UnityHelpers;

namespace VRPhysicsHands
{
    public class HandEmulatorToGrabber : MonoBehaviour
    {
        public Grabber grabber;
        public HandEmulator currentHand;
        public OVRHand.Hand handType;

        void Start()
        {
            grabber.AddGrabSpot("Palm");
        }
        void Update()
        {
            grabber.SetGrabSpotDimensions("Palm", new SpherecastInfo()
            {
                position = currentHand.palm.position,
                direction = currentHand.palm.forward,
                distance = 0.1f,
                radius = 0.05f,
                castMask = ~0
            });
            grabber.SetGrabSpotGrabbing("Palm", OVRInput.Get(handType == OVRHand.Hand.HandRight ? OVRInput.RawButton.RHandTrigger : OVRInput.RawButton.LHandTrigger, OVRInput.Controller.All));
        }
    }
}