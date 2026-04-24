using UnityEngine;

namespace Terrain
{
    public interface IWindAffected
    {
        Vector3 WindVelocity { get; set; }
        Transform TransformRoot { get; }
    }
}
