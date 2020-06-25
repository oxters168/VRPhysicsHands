using UnityEngine;

namespace VRPhysicsHands
{
    public class OculusTouchInput : MonoBehaviour, IHandBoneManipulator
    {
        private readonly float[] THUMB_CLOSE = new float[] { 0.4f, 0.8f, 0.6f };
        private readonly float[] THUMB_PINCH = new float[] { 0.1f, 0.4f, 0.6f };
        private readonly float[] INDEX_PINCH = new float[] { 0.3f, 0.8f, 0.7f };

        public OVRHand.Hand handType;
        public Vector3 wristEuler;

        [Space(10), Tooltip("If set to true will close and open the index finger based on near touch values or else from the index trigger value")]
        public bool indexFromNearTouch;
        [Tooltip("If set to true then the the hand when closed will have the thumb and index fingers pinching or else they'll be fully closed to make a fist")]
        public bool pinchOnClose;

        [Space(10)]
        public bool lerpThumb = true;
        public float thumbLerp = 5;
        public bool lerpIndex = true;
        public float indexLerp = 5;
        public bool lerpGrip = true;
        public float gripLerp = 5;
        private float previousThumbValue;
        private float previousIndexValue;
        private float previousGripValue;

        public HandBoneValues GetValues()
        {
            HandBoneValues providedData = default;

            float thumbValue = 0;
            float indexValue = 0;
            float gripValue = 0;

            #region Get values from controllers
            if (handType == OVRHand.Hand.HandLeft)
            {
                thumbValue = OVRInput.Get(OVRInput.RawNearTouch.LThumbButtons, OVRInput.Controller.All) ? 1 : 0;

                if (indexFromNearTouch)
                    indexValue = OVRInput.Get(OVRInput.RawNearTouch.LIndexTrigger, OVRInput.Controller.All) ? 1 : 0;
                else
                    indexValue = OVRInput.Get(OVRInput.RawAxis1D.LIndexTrigger, OVRInput.Controller.All);

                gripValue = OVRInput.Get(OVRInput.RawAxis1D.LHandTrigger, OVRInput.Controller.All);
            }
            else if (handType == OVRHand.Hand.HandRight)
            {
                thumbValue = OVRInput.Get(OVRInput.RawNearTouch.RThumbButtons, OVRInput.Controller.All) ? 1 : 0;

                if (indexFromNearTouch)
                    indexValue = OVRInput.Get(OVRInput.RawNearTouch.RIndexTrigger, OVRInput.Controller.All) ? 1 : 0;
                else
                    indexValue = OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger, OVRInput.Controller.All);

                gripValue = OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger, OVRInput.Controller.All);
            }
            #endregion

            #region Lerp values
            if (lerpThumb)
            {
                thumbValue = Mathf.Lerp(previousThumbValue, thumbValue, Time.deltaTime * thumbLerp);
                previousThumbValue = thumbValue;
            }
            if (lerpIndex)
            {
                indexValue = Mathf.Lerp(previousIndexValue, indexValue, Time.deltaTime * indexLerp);
                previousIndexValue = indexValue;
            }
            if (lerpGrip)
            {
                gripValue = Mathf.Lerp(previousGripValue, gripValue, Time.deltaTime * gripLerp);
                previousGripValue = gripValue;
            }
            #endregion
            
            #region Spread values across knuckles
            var thumbKnuckleValues = SpreadValue(thumbValue, 3);
            var indexKnuckleValues = SpreadValue(indexValue, 3);
            var gripKnuckleValues = SpreadValue(gripValue, 3);
            #endregion

            #region Create returned data
            providedData.bones = new HandBoneValues.BoneRotValue[]
            {
                new HandBoneValues.BoneRotValue() { id = BoneId.Hand_WristRoot, localRotation = Quaternion.Euler(wristEuler) },

                new HandBoneValues.BoneRotValue() { id = BoneId.Hand_Thumb1, value = thumbKnuckleValues[0] * (pinchOnClose ? THUMB_PINCH[0] : THUMB_CLOSE[0]) },
                new HandBoneValues.BoneRotValue() { id = BoneId.Hand_Thumb2, value = thumbKnuckleValues[1] * (pinchOnClose ? THUMB_PINCH[1] : THUMB_CLOSE[1]) },
                new HandBoneValues.BoneRotValue() { id = BoneId.Hand_Thumb3, value = thumbKnuckleValues[2] * (pinchOnClose ? THUMB_PINCH[2] : THUMB_CLOSE[2]) },

                new HandBoneValues.BoneRotValue() { id = BoneId.Hand_Index1, value = indexKnuckleValues[0] * (pinchOnClose ? INDEX_PINCH[0] : 1) },
                new HandBoneValues.BoneRotValue() { id = BoneId.Hand_Index2, value = indexKnuckleValues[1] * (pinchOnClose ? INDEX_PINCH[1] : 1) },
                new HandBoneValues.BoneRotValue() { id = BoneId.Hand_Index3, value = indexKnuckleValues[2] * (pinchOnClose ? INDEX_PINCH[2] : 1) },

                new HandBoneValues.BoneRotValue() { id = BoneId.Hand_Middle1, value = gripKnuckleValues[0] },
                new HandBoneValues.BoneRotValue() { id = BoneId.Hand_Middle2, value = gripKnuckleValues[1] },
                new HandBoneValues.BoneRotValue() { id = BoneId.Hand_Middle3, value = gripKnuckleValues[2] },

                new HandBoneValues.BoneRotValue() { id = BoneId.Hand_Ring1, value = gripKnuckleValues[0] },
                new HandBoneValues.BoneRotValue() { id = BoneId.Hand_Ring2, value = gripKnuckleValues[1] },
                new HandBoneValues.BoneRotValue() { id = BoneId.Hand_Ring3, value = gripKnuckleValues[2] },

                new HandBoneValues.BoneRotValue() { id = BoneId.Hand_Pinky1, value = gripKnuckleValues[0] },
                new HandBoneValues.BoneRotValue() { id = BoneId.Hand_Pinky2, value = gripKnuckleValues[1] },
                new HandBoneValues.BoneRotValue() { id = BoneId.Hand_Pinky3, value = gripKnuckleValues[2] },
            };
            #endregion
            
            return providedData;
        }

        public bool ShowHand()
        {
            return true;
        }

        private float[] SpreadValue(float value, int spreadAmount, float minValue = 0, float maxValue = 1)
        {
            float[] splitValues = new float[spreadAmount];
            float offset = (maxValue - minValue) / spreadAmount;
            for (int i = 0; i < spreadAmount; i++)
            {
                float currentMin = minValue + offset * i;
                float currentMax = minValue + offset * (i + 1);
                splitValues[i] = (Mathf.Clamp(value, currentMin, currentMax) - currentMin) / offset;
            }
            return splitValues;
        }
    }
}