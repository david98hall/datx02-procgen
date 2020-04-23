using System;
using Cities.Roads;
using Interfaces;
using Services;
using Terrain;
using UnityEditor;
using UnityEngine;

namespace App.ViewModels.Cities.Roads
{
    /// <summary>
    /// The view model for road network related generators.
    /// </summary>
    [Serializable]
    public class RoadViewModel : ViewModelStrategy<TerrainInfo, RoadNetwork>
    {
        #region Road Generation Fields
        
        // Visibility of the road network strategy editor.
        private bool _roadNetworkStrategyVisible;

        // Serialized view-model for <see cref="AStarStrategy"/> view model.
        [SerializeField]
        private AStarStrategy aStarStrategy;

        // Serialized view-model for <see cref="LSystemStrategy"/> view model.
        [SerializeField] 
        private LSystemStrategy lSystemStrategy;
        
        [SerializeField]
        private bool aStarEnabled;
        
        [SerializeField]
        private bool lSystemEnabled;
        #endregion
        
        #region Road Appearance fields

        /// <summary>
        /// Visibility of the plot strategy editor.
        /// </summary>
        private bool _roadAppearanceVisible;
        
        /// <summary>
        /// Toad width of the generated road network.
        /// </summary>
        [SerializeField]
        private float roadWidth = 0.3f;
        
        /// <summary>
        /// Getter for the road width of the generated road network.
        /// </summary>
        public float RoadWidth => roadWidth;

        /// <summary>
        /// Toad curvature of the generated road network.
        /// </summary>
        [SerializeField]
        private float roadCurvature = 0.1f;

        /// <summary>
        /// Getter for the road curvature of the generated road network.
        /// </summary>
        public float RoadCurvature => roadCurvature;

        /// <summary>
        /// Road smoothing iterations of the generated road network.
        /// </summary>
        [SerializeField]
        private int roadSmoothingIterations = 1;
        
        /// <summary>
        /// Getter for the road smoothing iterations of the generated road network.
        /// </summary>
        public int RoadSmoothingIterations => roadSmoothingIterations;

        [SerializeField]
        private Material roadMaterial;
        
        /// <summary>
        /// Road material of the generated road network.
        /// </summary>
        public Material RoadMaterial => roadMaterial;

        /// <summary>
        /// Getter for the road material of the generated road network.
        /// </summary>
        public float RoadTerrainOffsetY => roadTerrainOffsetY;

        [SerializeField]
        private float roadTerrainOffsetY = 0.075f;
        
        #endregion
        
        internal override IInjector<TerrainInfo> Injector
        {
            get => base.Injector;
            set
            {
                base.Injector = value;
                try
                {   
                    aStarStrategy.Injector = new Injector<float[,]>(() => value.Get().HeightMap);
                    lSystemStrategy.Injector = value;
                }
                catch (NullReferenceException)
                {
                    // Ignore
                }
            }
        }

        public override EventBus<AppEvent> EventBus
        {
            get => base.EventBus;
            set
            {
                base.EventBus = value;
                try
                {   
                    aStarStrategy.EventBus = value;
                    lSystemStrategy.EventBus = value;
                }
                catch (NullReferenceException)
                {
                    // Ignore
                }
            }
        }

        public override void Display()
        {
            _roadNetworkStrategyVisible 
                = EditorGUILayout.Foldout(_roadNetworkStrategyVisible, "Road Network Generation");
            if (!_roadNetworkStrategyVisible) return;
            EditorGUI.indentLevel++;

            // A* Strategy
            aStarEnabled = EditorGUILayout.Toggle("A*", aStarEnabled);
            if (aStarEnabled)
            {
                EditorGUI.indentLevel++;
                aStarStrategy.Display();
                EditorGUI.indentLevel--;
            }

            // L-system Strategy
            lSystemEnabled = EditorGUILayout.Toggle("L-system", lSystemEnabled);
            if (lSystemEnabled)
            {
                EditorGUI.indentLevel++;
                lSystemStrategy.Display();
                EditorGUI.indentLevel--;
            }
            
            DisplayRoadAppearance();
            EditorGUI.indentLevel--;
        }

        private void DisplayRoadAppearance()
        {
            _roadAppearanceVisible 
                = EditorGUILayout.Foldout(_roadAppearanceVisible, "Road Appearance");
            if (!_roadAppearanceVisible) return;
            EditorGUI.indentLevel++;
                
            // Road material
            roadMaterial = (Material) EditorGUILayout.ObjectField(
                "Material", roadMaterial, typeof(Material), true);
                
            // Road width
            roadWidth = EditorGUILayout.Slider("Width", roadWidth, 0.1f, 10);
                
            // Road/Terrain y-offset
            roadTerrainOffsetY = EditorGUILayout.FloatField("Y-Offset", roadTerrainOffsetY);

            // Road curvature and smoothing
            roadCurvature = EditorGUILayout.Slider("Curvature", roadCurvature, 0, 0.5f);
            if (roadCurvature > 0)
            {
                roadSmoothingIterations = EditorGUILayout.IntSlider(
                    "Smoothing Iterations", roadSmoothingIterations, 1, 10);   
            }

            EditorGUI.indentLevel--;
        }
        
        public override RoadNetwork Generate()
        {
            var aStarRoadNetwork = aStarEnabled ? aStarStrategy.Generate() : null;
            var lSystemRoadNetwork = lSystemEnabled ? lSystemStrategy.Generate() : null;

            RoadNetwork mergedRoadNetwork;
            if (aStarRoadNetwork == null)
            {
                mergedRoadNetwork = lSystemRoadNetwork;
            } 
            else if (lSystemRoadNetwork == null)
            {
                mergedRoadNetwork = aStarRoadNetwork;
            }
            else
            {
                aStarRoadNetwork.Merge(lSystemRoadNetwork);
                mergedRoadNetwork = aStarRoadNetwork;
            }

            return mergedRoadNetwork;
        }
    }
}