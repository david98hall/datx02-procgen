using UnityEngine;
using Utils.Parallelism;

namespace Cities.Buildings
{
    /// <summary>
    /// Generated buildings. Each building is defined by its vertices, bounds, position and facing.
    /// </summary>
    public class Building
    {
        private float width;

        private float height;

        private Vector3 position;

        private Vector2 facing;

        public readonly Mesh mesh;

        /// <summary>
        /// All of the vertices, triangles and uvs that make up the building. Each side is its own polygon, 
        /// to ensure that no vertices are shared and normals/textures are displayed properly.
        /// </summary>
        //IEnumerable<(IEnumerable<Vector3>, IEnumerable<int>, IEnumerable<Vector2>)> sides;


        /// <summary>
        /// Construct a single building with dimensions, position, front direction and mesh.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="facing"></param>
        /// <param name="mesh"></param>
        public Building(Vector3 position, Vector2 facing, Mesh mesh)
        {
            Dispatcher.Instance.EnqueueAction(() =>
            {
                width = mesh.bounds.size.x;
                height = mesh.bounds.size.z;
            });

            this.position = position;
            this.facing = facing;

            //this.sideVerts = sideVerts;
            //this.sideTris = sideTris;
            //this.sideUVs = sideUVs;

            this.mesh = mesh;
        }

    }
}
