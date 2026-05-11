using System.Collections.Generic;
using UnityEngine;

namespace tarkin.cruelty.player
{
    public enum GrappleState
    {
        Seeking,
        Taut,
        Retracting
    }

    public class GrappendixVisual : MonoBehaviour
    {
        [SerializeField] private GameObject lumpPrefab;

        [SerializeField] private float segmentSpacing = 0.5f;

        [Space(10)]
        [SerializeField] private float baseScale = 1f;
        [SerializeField] private float scaleVariation = 0.25f;
        [SerializeField] private float scaleFrequency = 4f;
        [SerializeField] private float scaleScrollSpeed = 6f;

        [Space(10)]
        [SerializeField] private float slackFactorRate = 2f;
        [SerializeField] private float slackMaxAmplitude = 0.4f;
        [SerializeField] private float slackFrequency = 0.15f;
        [SerializeField] private float slackScrollSpeed = 15f;

        [Space(10)]
        [SerializeField] private bool faceDirection = true;
        [SerializeField] private Vector3 rotationOffset;

        private readonly List<Transform> lumps = new();

        private float currentLength;
        private int requiredCount;

        [SerializeField] private Vector3 _pointGrapple;
        [SerializeField] private Vector3 _pointPlayer;

        private float _currentSlackScroll;
        private float _currentSlackAmplitude;
        private GrappleState _state;

        public void SetPoints(Vector3 pointGrapple, Vector3 pointPlayer)
        {
            _pointGrapple = pointGrapple;
            _pointPlayer = pointPlayer;
        }

        public void SetState(GrappleState state)
        {
            _state = state;
        }

        void Update()
        {
            if (lumpPrefab == null)
                return;

            switch (_state)
            {
                case GrappleState.Seeking:
                    _currentSlackAmplitude = Mathf.MoveTowards(_currentSlackAmplitude, slackMaxAmplitude, slackFactorRate * Time.deltaTime);
                    _currentSlackScroll -= Time.deltaTime * slackScrollSpeed;
                    break;
                case GrappleState.Taut:
                    _currentSlackAmplitude = Mathf.MoveTowards(_currentSlackAmplitude, 0f, slackFactorRate * 2f * Time.deltaTime);
                    break;
                case GrappleState.Retracting:
                    _currentSlackAmplitude = Mathf.MoveTowards(_currentSlackAmplitude, slackMaxAmplitude, slackFactorRate * Time.deltaTime);
                    _currentSlackScroll += Time.deltaTime * slackScrollSpeed;
                    break;
            }

            Vector3 start = _pointGrapple;
            Vector3 end = _pointPlayer;

            Vector3 direction = (end - start).normalized;
            currentLength = Vector3.Distance(start, end);

            requiredCount = Mathf.CeilToInt(currentLength / segmentSpacing);

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
                float distanceAlongLine = Mathf.Repeat((i * segmentSpacing), currentLength);
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
                float sineValue = Mathf.Sin(t * slackFrequency * Mathf.PI * 2f * currentLength + _currentSlackScroll) * _currentSlackAmplitude * positionSlackModifier;
                Vector3 orthogonalDirection = Vector3.Cross(direction, Vector3.up).normalized;
                Vector3 sineWavePoint = linearPos + orthogonalDirection * sineValue;
                lump.position = sineWavePoint;

                // -- scale variation (pulsating)
                float wave =
                    Mathf.Sin(
                        (distanceAlongLine * scaleFrequency) -
                        (Time.time * scaleScrollSpeed)
                    );

                float scale =
                    baseScale +
                    (wave * scaleVariation);

                lump.localScale = Vector3.one * scale;

                if (faceDirection)
                {
                    lump.rotation =
                        Quaternion.LookRotation(direction) *
                        Quaternion.Euler(rotationOffset);
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

        void EnsurePoolSize(int count)
        {
            while (lumps.Count < count)
            {
                GameObject obj = Instantiate(lumpPrefab, transform);
                lumps.Add(obj.transform);
            }
        }
    }
}