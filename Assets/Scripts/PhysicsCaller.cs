using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    ///     Abstraction layer for calls of <see cref="Physics" /> functions.
    /// </summary>
    public static class PhysicsCaller
    {
        private static readonly int LayerMask = UnityEngine.LayerMask.GetMask("Spatial Mesh");

        /// <summary>
        ///     Casts a sphere cast starting from the origin into the world space.
        /// </summary>
        /// <param name="origin">Start position of the cast.</param>
        /// <param name="direction">Direction of the cast.</param>
        /// <param name="hitInfo">Info about the object that has been hit.</param>
        /// <returns>Whether the cast has hit anything.</returns>
        public static bool SphereCastOnSpatialMesh(Vector3 origin, Vector3 direction, out RaycastHit hitInfo)
        {
#if UNITY_EDITOR
            hitInfo = new RaycastHit
            {
                point = direction + origin
            };
            return true;
#else
            return Physics.SphereCast(origin, Parameters.SphereCastSize, direction, out hitInfo, Parameters.MaxSphereLength, LayerMask);
#endif
        }
    }
}