using System;
using System.Threading;
using System.Threading.Tasks;
using BIAS.PCG.Cities.Roads;
using BIAS.Utils.Interfaces;
using BIAS.Utils.Services;
using BIAS.PCG.Terrain;
using UnityEditor;
using UnityEngine;

namespace BIAS.App.ViewModels.Cities.Roads
{
    /// <summary>
    /// The view model for road network related generators.
    /// </summary>
    [Serializable]
    public class RoadViewModel : ViewModel<TerrainInfo, RoadNetwork>
    {
        #region Road Generation Fields
        
        // Visibility of the road network strategy editor.
        private bool _roadNetworkStrategyVisible;

        // Serialized view-model for <see cref="AStarStrategy"/> view model.
        [SerializeField]
        private AStar aStar = null;

        // Serialized view-model for <see cref="LSystemStrategy"/> view model.
        [SerializeField] 
        private LSystem lSystem = null;
        
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
                    aStar.Injector = new Injector<float[,]>(() => value.Get().HeightMap);
                    lSystem.Injector = value;
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
                    aStar.EventBus = value;
                    lSystem.EventBus = value;
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
                    aStar.CancelToken = value;
                    lSystem.CancelToken = value;
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
                aStar.Display();
                EditorGUI.indentLevel--;
            }

            // L-system Strategy
            lSystemEnabled = EditorGUILayout.Toggle("L-system", lSystemEnabled);
            if (lSystemEnabled)
            {
                EditorGUI.indentLevel++;
                lSystem.Display();
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
            EventBus.CreateEvent(AppEvent.GenerationStart, "Generating Road Network", this);
            
            // One task per road network generation strategy
            var tasks = new Task[]
            {
                Task.Run(() => aStarEnabled ? aStar.Generate() : null, CancelToken),
                Task.Run(() => lSystemEnabled ? lSystem.Generate() : null, CancelToken)
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
            var resultingNetwork = lSystemRoadNetwork;
            if (aStarRoadNetwork != null)
            {
                resultingNetwork = lSystemRoadNetwork == null 
                    ? aStarRoadNetwork 
                    : aStarRoadNetwork.Merge(lSystemRoadNetwork);
            }

            EventBus.CreateEvent(AppEvent.GenerationEnd, "Generated Road Network", this);
            return resultingNetwork;
        }
    }
}