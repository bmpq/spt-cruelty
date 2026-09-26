using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.SceneManagement;

namespace tarkin.cruelty.grappendix
{
    public static class LevelBorders
    {
        public static void Disable()
        {
            DisableRootGameObjectOnScene("Factory_Rework_Areas", "Factory_rework_LevelBorders");
            DisableRootGameObjectOnScene("custom_Scripts", "Custom_LevelBorders");
            DisableRootGameObjectOnScene("Reserve_Base_Scripts", "Reserve_Base_LevelBorders");
            DisableRootGameObjectOnScene("Reserve_Base_Platz", "BLOCKERS");
            DisableRootGameObjectOnScene("woods_Scripts", "woods_LevelBorders");
            DisableRootGameObjectOnScene("shoreline_scripts", "Shoreline_LevelBorders");
            DisableRootGameObjectOnScene("Shopping_Mall_Scripts", "Shopping_Mall_LevelBorders");
            DisableRootGameObjectOnScene("Lighthouse_Scripts", "Lighthouse_LevelBorders");
            DisableRootGameObjectOnScene("City_LevelBorders", "City_LevelBorders_3ST");

            DisableRootGameObjectOnScene("Shopping_Mall_Scripts_MI", "Shopping_Mall_LevelBorders");
        }

        private static void DisableRootGameObjectOnScene(string sceneName, string goName)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (scene.isLoaded)
            {
                foreach (var rootGameObject in scene.GetRootGameObjects())
                {
                    if (rootGameObject.name == goName)
                        rootGameObject.gameObject.SetActive(false);
                }
            }
        }
    }
}
