using UnityEngine;
using UnityEditor;

namespace LMK.CameraController2D.Editor
{
    [CustomEditor(typeof(CameraBoundsTrigger))]
    public class CameraBoundsTriggerEditor : UnityEditor.Editor
    {
        private CameraBoundsTrigger asTarget;



        private void OnEnable()
        {
            asTarget = (CameraBoundsTrigger)target;
        }

        private void OnSceneGUI()
        {
            if (!asTarget.enabled) return;

            // Constant handle size since we only drawing in 2D.
            float scaleHandleSize = HandleUtility.GetHandleSize(Vector3.zero) * 0.05f;

            Undo.RecordObject(asTarget, "Change bounds data of CameraBoundsTrigger");
            asTarget.boundsData.WorldBounds = HandleUtils.DrawRectHandles(
                asTarget.boundsData.WorldBounds, 
                Color.yellow, 
                scaleHandleSize, 
                "(Visible Bounds)");
            EditorUtility.SetDirty(asTarget);
        }
    }
}
