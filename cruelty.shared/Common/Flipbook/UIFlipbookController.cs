using UnityEngine;
using UnityEngine.UI;

namespace tarkin.cruelty.shared
{
    [RequireComponent(typeof(RawImage))]
    public class UIFlipbookController : MonoBehaviour
    {
        [Header("Atlas Settings")]
        public int columns = 4;
        public int rows = 4;

        [Tooltip("Leave at 0 to automatically use (Columns * Rows)")]
        public int totalFrames = 0;

        [Header("Playback Settings")]
        public float framesPerSecond = 12f;
        public bool playOnAwake = true;
        public bool loop = true;

        private RawImage targetImage;
        private Material runtimeMaterial;

        private bool isPlaying;
        private float timer;
        private int currentFrame = 0;

        private readonly int colId = Shader.PropertyToID("_Columns");
        private readonly int rowId = Shader.PropertyToID("_Rows");
        private readonly int frameId = Shader.PropertyToID("_Frame");

        void Awake()
        {
            targetImage = GetComponent<RawImage>();

            if (targetImage.material != null)
            {
                runtimeMaterial = new Material(targetImage.material);
                targetImage.material = runtimeMaterial;
            }
            else
            {
                Debug.LogError("UIFlipbookController requires a Material assigned to the RawImage!");
                return;
            }

            runtimeMaterial.SetFloat(colId, columns);
            runtimeMaterial.SetFloat(rowId, rows);
            UpdateShaderFrame();

            if (totalFrames <= 0)
                totalFrames = columns * rows;

            if (playOnAwake)
                Play();
        }

        void Update()
        {
            if (!isPlaying) return;

            timer += Time.deltaTime;
            float secondsPerFrame = 1f / framesPerSecond;

            if (timer >= secondsPerFrame)
            {
                timer -= secondsPerFrame;
                currentFrame++;

                if (currentFrame >= totalFrames)
                {
                    if (loop)
                    {
                        currentFrame = 0;
                    }
                    else
                    {
                        currentFrame = totalFrames - 1;
                        Stop();
                        return;
                    }
                }

                UpdateShaderFrame();
            }
        }

        private void UpdateShaderFrame()
        {
            if (runtimeMaterial != null)
            {
                runtimeMaterial.SetFloat(frameId, currentFrame);
            }
        }

        public void Play()
        {
            isPlaying = true;
        }

        public void Stop()
        {
            isPlaying = false;
        }

        public void Restart()
        {
            currentFrame = 0;
            timer = 0;
            UpdateShaderFrame();
            Play();
        }

        void OnDestroy()
        {
            if (runtimeMaterial != null)
            {
                Destroy(runtimeMaterial);
            }
        }
    }
}