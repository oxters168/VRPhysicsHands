using UnityEngine;
using UnityHelpers;

namespace VRPhysicsHands
{
    public class HandEmulatorToGrabber : MonoBehaviour
    {
        public Grabber grabber;
        public HandEmulator currentHand;
        public OVRHand.Hand handType;
        BoneId[] fingerTipIds = new BoneId[] { BoneId.Hand_IndexTip, BoneId.Hand_MiddleTip, BoneId.Hand_RingTip, BoneId.Hand_PinkyTip };
        string[] grabSpotNames = new string[] { "IndexThumbPinch", "MiddleThumbPinch", "RingThumbPinch", "PinkyThumbPinch" };

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
            var thumbPosition = currentHand.bones[(int)BoneId.Hand_ThumbTip].mesh.position;
            for (int i = 0; i < fingerTipIds.Length; i++)
            {
                var currentFingerPosition = currentHand.bones[(int)fingerTipIds[i]].mesh.position;
                var difference = currentFingerPosition - thumbPosition;
                float distance = difference.magnitude;
                Vector3 direction = difference.normalized;
                grabber.SetGrabSpotDimensions(grabSpotNames[i], new RaycastInfo()
                {
                    position = thumbPosition,
                    direction = direction,
                    distance = distance,
                });
            }
        }
    }
}