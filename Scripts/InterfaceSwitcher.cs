using UnityEngine;
using UnityHelpers;

namespace VRPhysicsHands
{
    public class InterfaceSwitcher : MonoBehaviour
    {
        public HandEmulator hand;

        [RequireInterface(typeof(IHandBoneManipulator))]
        public GameObject _touchControllersInterfaceObject;
        private IHandBoneManipulator touchControllersInterface;

        [RequireInterface(typeof(IHandBoneManipulator))]
        public GameObject _handTrackingInterfaceObject;
        private IHandBoneManipulator handTrackingInterface;

        void Awake()
        {
            touchControllersInterface = _touchControllersInterfaceObject.GetComponent<IHandBoneManipulator>();
            handTrackingInterface = _handTrackingInterfaceObject.GetComponent<IHandBoneManipulator>();
        }
        void Update()
        {
            if (OVRInput.GetConnectedControllers() == OVRInput.Controller.Hands)
                hand.handInterface = handTrackingInterface;
            else
                hand.handInterface = touchControllersInterface;
        }
    }
}