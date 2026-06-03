using System.Linq;

using UnityEngine;

namespace LMK.Editor
{
    public static class CustomGUIStyles
    {
        private static GUISkin defaultSkin;

        static CustomGUIStyles()
        {
            defaultSkin = Resources.Load<GUISkin>("CustomGUISkin");
        }

        public static GUIStyle foldoutHeader => defaultSkin.customStyles.FirstOrDefault(m => m.name == "foldoutHeader");
        public static GUIStyle toolbarButton => defaultSkin.customStyles.FirstOrDefault(m => m.name == "toolbarButton");
    }
}
