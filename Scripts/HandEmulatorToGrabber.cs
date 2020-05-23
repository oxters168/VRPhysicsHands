using UnityEngine;
using UnityHelpers;

namespace VRPhysicsHands
{
    public class HandEmulatorToGrabber : MonoBehaviour
    {
        public Grabber grabber;
        public HandEmulator currentHand;
        public OVRHand.Hand handType;
        BoneId[] fingerTipIds = new BoneId[] { BoneId.Hand_ThumbTip, BoneId.Hand_IndexTip, BoneId.Hand_MiddleTip, BoneId.Hand_RingTip, BoneId.Hand_PinkyTip };
        string[] grabSpotNames = new string[] { "Palm", "IndexThumbPinch", "MiddleThumbPinch", "RingThumbPinch", "PinkyThumbPinch" };

        void Start()
        {
            foreach (var grabSpotName in grabSpotNames)
            {
                grabber.AddGrabSpot(grabSpotName, currentHand.bones[(int)BoneId.Hand_WristRoot].physics.transform);
            }
            //grabber.AddGrabSpot("Palm");
        }
        void Update()
        {
            var thumbPosition = currentHand.bones[(int)fingerTipIds[0]].mesh.position;
            for (int i = 1; i < fingerTipIds.Length; i++)
            {
                var currentFingerPosition = currentHand.bones[(int)fingerTipIds[i]].mesh.position;
                var difference = currentFingerPosition - thumbPosition;
                float distance = difference.magnitude;
                //Vector3 midpoint = difference / 2;
                Vector3 direction = difference.normalized;
                grabber.SetGrabSpotDimensions(grabSpotNames[i], new SpherecastInfo()
                {
                    position = thumbPosition,
                    direction = direction,
                    distance = distance / 2f,
                    radius = distance < 0.1f ? distance / 2f : 0,
                    castMask = ~0
                });
            }
            /*grabber.SetGrabSpotDimensions("Palm", new SpherecastInfo()
            {
                position = currentHand.palm.position,
                direction = currentHand.palm.forward,
                distance = 0.1f,
                radius = 0.05f,
                castMask = ~0
            });
            grabber.SetGrabSpotGrabbing("Palm", OVRInput.Get(handType == OVRHand.Hand.HandRight ? OVRInput.RawButton.RHandTrigger : OVRInput.RawButton.LHandTrigger, OVRInput.Controller.All));*/
        }
    }
}