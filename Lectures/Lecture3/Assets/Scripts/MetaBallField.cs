using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class MetaBallField
{
    public Transform[] Balls = new Transform[0];
    public float BallRadius = 1;

    private Vector3[] _ballPositions;

    // since I don't want to define constants to 'slice' the area into cubes, 
    // I'll be using a bounding box calculated here
    public Bounds BoundingBox;
    
    /// <summary>
    /// Call Field.Update to react to ball position and parameters in run-time.
    /// </summary>
    public void Update()
    {
        _ballPositions = Balls.Select(x => x.position).ToArray();

        BoundingBox = new Bounds();
        var radius1 = new Vector3(BallRadius, BallRadius, BallRadius);
        for (var i = 0; i < 3; i++)
        {
            // expand in both directions
            BoundingBox.Encapsulate(_ballPositions[i] + radius1 + Vector3.one * 0.1f);
            BoundingBox.Encapsulate(_ballPositions[i] - radius1 - Vector3.one * 0.1f);
        }
    }
    
    /// <summary>
    /// Calculate scalar field value at point
    /// </summary>
    public float F(Vector3 position)
    {
        float f = 0;
        // Naive implementation, just runs for all balls regardless the distance.
        // A better option would be to construct a sparse grid specifically around 
        foreach (var center in _ballPositions)
        {
            f += 1 / Vector3.SqrMagnitude(center - position);
        }

        f *= BallRadius * BallRadius;

        return f - 1;
    }
}