using System.Collections.Generic;
using UnityEngine;

namespace UIModule
{
    [CreateAssetMenu(fileName = "UISettings", menuName = "UIModule/Create UISettings Assets")]
    public class UISettings : ScriptableObject
    {
        public const string PATH_SETTINGS = "UISettings";


        public string prefabPath = "Assets";
        public string scriptPath = "Assets";
        
        public string uiRootName = "UIRoot";

        public bool skipAnimation = false;
        public bool resident = true;

        public List<UIGroupInfo> groups = new() {
            new UIGroupInfo() { name = "Default", depth = 0 }
        };

      
        public static UISettings Load() {
            var settings = Resources.Load<UISettings>(PATH_SETTINGS);
            return settings;
        }

        public void Reset()
        {
            resident = true;
            prefabPath = "Assets";
            scriptPath = "Assets";
            uiRootName = "UIRoot";
            skipAnimation = false;
            groups = new() { new UIGroupInfo() { name = "Default", depth = 0 } };
        }
    }
}