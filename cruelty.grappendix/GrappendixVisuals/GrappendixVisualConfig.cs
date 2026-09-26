using UnityEngine;

namespace tarkin.cruelty.grappendix.GrappendixVisuals
{
    public struct GrappendixVisualConfig
    {
        public float segmentSpacing;
        public float baseScale;
        public float scaleVariation;
        public float scaleFrequency;
        public float scaleScrollSpeed;

        public float slackFactorRate;
        public float slackMaxAmplitude;
        public float slackFrequency;
        public float slackScrollSpeed;

        public bool faceDirection;
        public Vector3 rotationOffset;

        public GrappendixVisualConfig()
        {
            segmentSpacing = 0.14f;

            baseScale = 0.2f;
            scaleVariation = 0.05f;
            scaleFrequency = 2f;
            scaleScrollSpeed = 6f;

            slackFactorRate = 2.5f;
            slackMaxAmplitude = 0.25f;
            slackFrequency = 0.17f;
            slackScrollSpeed = 22f;

            faceDirection = true;
            rotationOffset = Vector3.zero;
        }
    }
}
