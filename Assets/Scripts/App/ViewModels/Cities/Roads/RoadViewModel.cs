using System;
using System.Threading;
using System.Threading.Tasks;
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
                {}
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
                {}
            }
        }

        public override CancellationToken CancelToken
        {
            get => base.CancelToken;
            set
            {
                base.CancelToken = value;
                try
                {   
                    aStarStrategy.CancelToken = value;
                    lSystemStrategy.CancelToken = value;
                }
                catch (NullReferenceException)
                {}
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

            EditorGUI.indentLevel--;
        }
        
        public override RoadNetwork Generate()
        {
            // One task per road network generation strategy
            var tasks = new Task[]
            {
                Task.Run(() => aStarEnabled ? aStarStrategy.Generate() : null, CancelToken),
                Task.Run(() => lSystemEnabled ? lSystemStrategy.Generate() : null, CancelToken)
            };

            try
            {
                // Wait for all strategies to finish generating
                Task.WaitAll(tasks);
            }
            catch (Exception)
            {
                // Task canceled, abort
                return null;
            }
            
            // Extract the results of the different road network generation tasks
            var aStarRoadNetwork = ((Task<RoadNetwork>) tasks[0]).Result;
            var lSystemRoadNetwork = ((Task<RoadNetwork>) tasks[1]).Result;
            
            // Merge any combinations of road networks and return the result
            if (aStarRoadNetwork == null) return lSystemRoadNetwork;
            return lSystemRoadNetwork == null ? aStarRoadNetwork : aStarRoadNetwork.Merge(lSystemRoadNetwork);
        }
    }
}