using UnityEngine;
using UnityEngine.Rendering;

namespace tarkin.cruelty.world
{
    public class ReplacerEditorBootstrap : MonoBehaviour
    {
        [SerializeField] MaterialTextureMapping mapping;

        void Start()
        {
            Process(mapping);
        }
        public static void Process(MaterialTextureMapping mapping)
        {
            var allRends = Component.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            Shader diffuseShader = Shader.Find("Diffuse");

            foreach (var rend in allRends)
            {
                var mats = rend.materials;

                if (rend.shadowCastingMode == ShadowCastingMode.ShadowsOnly)
                {
                    Component.Destroy(rend);
                    continue;
                }

                foreach (var mat in mats)
                {
                    mat.name = mat.name.Replace(" (Instance)", "");

                    if (mat.name.Contains("puddle"))
                    {
                        Component.Destroy(rend);
                        break;
                    }

                    if (mat.shader.name.Contains("Transparent"))
                    {
                        mat.mainTexture = null;
                        mat.color = new Color(0.7f, 0f, 0.7f, 0.5f);
                        continue;
                    }

                    mat.shader = diffuseShader;

                    mat.mainTexture = mapping.GetMainTexture(mat);
                }

                rend.materials = mats;
            }
        }
        void Update()
        {
        
        }
    }
}
