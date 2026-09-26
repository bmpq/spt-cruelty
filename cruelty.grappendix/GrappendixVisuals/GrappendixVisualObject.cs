using System.Collections.Generic;
using tarkin.cruelty.grappendix.GrappendixVisuals;
using UnityEngine;

namespace tarkin.cruelty.grappendix.visuals
{
    [DefaultExecutionOrder(110)]
    public class GrappendixVisualObject : MonoBehaviour
    {
        public GrappendixVisualConfig config = new GrappendixVisualConfig();

        private readonly List<Transform> lumps = new();

        private float currentLength;
        private int requiredCount;

        [SerializeField] private Vector3 _pointGrapple;
        [SerializeField] private Vector3 _pointPlayer;

        private float _currentSlackScroll;
        private float _currentSlackAmplitude;
        private GrappleState _state = GrappleState.Idle;

        public void SetPoints(Vector3 pointGrapple, Vector3 pointPlayer)
        {
            _pointGrapple = pointGrapple;
            _pointPlayer = pointPlayer;
        }

        public void SetState(GrappleState state)
        {
            _state = state;

            gameObject.SetActive(state != GrappleState.Idle);
        }

        void EnsurePoolSize(int count)
        {
            while (lumps.Count < count && lumps.Count < 1000)
            {
                GameObject newLump = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Component.Destroy(newLump.GetComponent<Collider>());
                newLump.transform.SetParent(this.transform);
                lumps.Add(newLump.transform);
            }
        }

        void LateUpdate()
        {
            switch (_state)
            {
                case GrappleState.Seeking:
                    _currentSlackAmplitude = Mathf.MoveTowards(_currentSlackAmplitude, config.slackMaxAmplitude, config.slackFactorRate * Time.deltaTime);
                    _currentSlackScroll -= Time.deltaTime * config.slackScrollSpeed;
                    break;
                case GrappleState.Taut:
                    _currentSlackAmplitude = Mathf.MoveTowards(_currentSlackAmplitude, 0f, config.slackFactorRate * 2f * Time.deltaTime);
                    break;
                case GrappleState.Retracting:
                    _currentSlackAmplitude = Mathf.MoveTowards(_currentSlackAmplitude, config.slackMaxAmplitude, config.slackFactorRate * Time.deltaTime);
                    _currentSlackScroll += Time.deltaTime * config.slackScrollSpeed;
                    break;
            }

            Vector3 start = _pointGrapple;
            Vector3 end = _pointPlayer;

            Vector3 direction = (end - start).normalized;
            currentLength = Vector3.Distance(start, end);

            requiredCount = Mathf.CeilToInt(currentLength / config.segmentSpacing);

            EnsurePoolSize(requiredCount);

            for (int i = 0; i < lumps.Count; i++)
            {
                Transform lump = lumps[i];

                if (i >= requiredCount)
                {
                    lump.gameObject.SetActive(false);
                    continue;
                }

                lump.gameObject.SetActive(true);

                // Position travels FROM grapple point TO player
                float distanceAlongLine = Mathf.Repeat((i * config.segmentSpacing), currentLength);
                Vector3 linearPos = start + direction * distanceAlongLine;

                // -- looseness when not taut
                float t = currentLength > 0f ? distanceAlongLine / currentLength : 0f; // 1.0 = player
                float positionSlackModifier = 1f;
                switch (_state)
                {
                    case GrappleState.Seeking:
                        positionSlackModifier = t;
                        break;
                    case GrappleState.Retracting:
                        positionSlackModifier = 1f - t;
                        break;
                    case GrappleState.Taut:
                        positionSlackModifier = 1f;
                        break;
                }

                if (t > 0.9f)
                    positionSlackModifier *= Mathf.Lerp(1f, 0f ,Mathf.InverseLerp(0.9f, 1f, t));
                float sineValue = Mathf.Sin(t * config.slackFrequency * Mathf.PI * 2f * currentLength + _currentSlackScroll) * _currentSlackAmplitude * positionSlackModifier;
                Vector3 orthogonalDirection = Vector3.Cross(direction, Vector3.up).normalized;
                Vector3 sineWavePoint = linearPos + orthogonalDirection * sineValue;
                lump.position = sineWavePoint;

                // -- scale variation (pulsating)
                float wave =
                    Mathf.Sin(
                        (distanceAlongLine * config.scaleFrequency) -
                        (Time.time * config.scaleScrollSpeed)
                    );

                float scale =
                    config.baseScale +
                    (wave * config.scaleVariation);

                lump.localScale = Vector3.one * scale;

                if (config.faceDirection)
                {
                    lump.rotation =
                        Quaternion.LookRotation(direction) *
                        Quaternion.Euler(config.rotationOffset);
                }
            }
        }

        void OnDisable()
        {
            for (int i = 0; i < lumps.Count; i++)
            {
                lumps[i].gameObject.SetActive(false);
            }
        }
    }
}