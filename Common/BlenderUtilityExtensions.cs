using UnityEngine;

namespace StudioManette
{
    namespace Common
    {
        public static class BlenderUtilityExtensions
        {
            public static Vector3 FromUnityToBlender(this Vector3 positionVector)
            {
                return new Vector3(-positionVector.x, -positionVector.z, positionVector.y);
            }

            public static Vector3 FromBlenderToUnity(this Vector3 positionVector)
            {
                return new Vector3(-positionVector.x, positionVector.z, -positionVector.y);
            }

            // Apply the input PRS data to the given transform
            public static void FromBlenderToUnity(
                this Transform existingTransform,
                Vector3 blenderPosition,
                Vector3 blenderRotation,
                Vector3 blenderScale)
            {
                existingTransform.localPosition = blenderPosition.FromBlenderToUnity();
                if (true)//master.rotation.eulerAngles != rot)
                {
                    Vector3 convertedRot = -1.0f * blenderRotation.FromBlenderToUnity();
                    existingTransform.localEulerAngles = new Vector3(existingTransform.localEulerAngles.x,
                                                                        0.0f,
                                                                        existingTransform.localEulerAngles.z);
                    existingTransform.localEulerAngles += convertedRot;
                }
                existingTransform.localScale = blenderScale;
            }
        }
    }
}
