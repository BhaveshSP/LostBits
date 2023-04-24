using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace MoodMe
{
    [CustomEditor(typeof(CameraManager))]
    [CanEditMultipleObjects]
    public class CameraManager_Editor : Editor
    {
        Dropdown m_Dropdown;
        int selGridInt = 0;
        public override void OnInspectorGUI()
        {

            GUILayout.Label("Available Video Sources");
            WebCamDevice[] devices = WebCamTexture.devices;
            List<string> deviceNames = new List<string>();
            for (int cameraIndex = 0; cameraIndex < devices.Length; cameraIndex++)
            {
                GUILayout.Label(cameraIndex + " - " + devices[cameraIndex].name + (WebCamTexture.devices[cameraIndex].isFrontFacing ? " (Front facing)" : " (Back facing)"));
            }

            DrawDefaultInspector();

        }
    }
}