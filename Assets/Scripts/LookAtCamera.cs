using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    ///     Script for orientating the game object towards the user.
    /// </summary>
    public class LookAtCamera : MonoBehaviour
    {
        private void Update()
        {
            this.gameObject.transform.LookAt(Camera.main.transform);
            this.gameObject.transform.Rotate(Vector3.up, 180f);
        }
    }
}