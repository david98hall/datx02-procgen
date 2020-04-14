using System;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace Demo
{
    [CustomEditor(typeof(Demo))]
    [CanEditMultipleObjects]
    public class DemoEditor : Editor
    {
        private Demo _demo;

        private void OnEnable()
        {
            _demo = (Demo) target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
        }
    }
}