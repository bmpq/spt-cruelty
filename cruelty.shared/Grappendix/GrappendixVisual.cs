using System.Collections.Generic;
using UnityEngine;

namespace tarkin.cruelty.player
{
    public class GrappendixVisual : MonoBehaviour
    {
        [SerializeField] private GameObject lumpPrefab;

        [SerializeField] private float segmentSpacing = 0.5f;
        [SerializeField] private float scrollSpeed = 8f;

        [SerializeField] private float baseScale = 1f;
        [SerializeField] private float scaleVariation = 0.25f;
        [SerializeField] private float scaleFrequency = 4f;
        [SerializeField] private float scaleScrollSpeed = 6f;

        [SerializeField] private bool faceDirection = true;
        [SerializeField] private Vector3 rotationOffset;

        private readonly List<Transform> lumps = new();

        private float currentLength;
        private int requiredCount;

        [SerializeField] private Vector3 _pointGrapple;
        [SerializeField] private Vector3 _pointPlayer;

        public void SetPoints(Vector3 pointGrapple, Vector3 pointPlayer)
        {
            _pointGrapple = pointGrapple;
            _pointPlayer = pointPlayer;
        }

        void Update()
        {
            if (lumpPrefab == null)
                return;

            Vector3 start = _pointGrapple;
            Vector3 end = _pointPlayer;

            Vector3 direction = (end - start).normalized;
            currentLength = Vector3.Distance(start, end);

            requiredCount = Mathf.CeilToInt(currentLength / segmentSpacing) + 1;

            EnsurePoolSize(requiredCount);

            float scrollOffset = Time.time * scrollSpeed;

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
                float distanceAlongLine =
                    Mathf.Repeat((i * segmentSpacing) + scrollOffset, currentLength);

                Vector3 pos = start + direction * distanceAlongLine;

                lump.position = pos;

                if (faceDirection)
                {
                    lump.rotation =
                        Quaternion.LookRotation(direction) *
                        Quaternion.Euler(rotationOffset);
                }

                // Sinusoidal scale variation
                float wave =
                    Mathf.Sin(
                        (distanceAlongLine * scaleFrequency) -
                        (Time.time * scaleScrollSpeed)
                    );

                float scale =
                    baseScale +
                    (wave * scaleVariation);

                lump.localScale = Vector3.one * scale;
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