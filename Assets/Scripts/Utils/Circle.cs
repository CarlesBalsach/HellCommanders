using System.Collections.Generic;
using UnityEngine;

public class Circle
{
    public Vector2 Position { get; private set; }
    public float Radius { get; private set; }

    public Circle(Vector2 position, float radius)
    {
        Position = position;
        Radius = radius;
    }

    public List<Vector2> IntersectionPoints(Circle other)
    {
        List<Vector2> intersectionPoints = new List<Vector2>();

        float d = Vector2.Distance(this.Position, other.Position);

        // Check if circles are coincident
        if (d == 0 && this.Radius == other.Radius)
        {
            return intersectionPoints; // Return an empty list
        }

        // Check if there are no intersection points
        if (d > this.Radius + other.Radius || d < Mathf.Abs(this.Radius - other.Radius))
        {
            return intersectionPoints; // Return an empty list
        }

        // Calculate intersection points
        float a = (this.Radius * this.Radius - other.Radius * other.Radius + d * d) / (2 * d);
        float h = Mathf.Sqrt(this.Radius * this.Radius - a * a);
        Vector2 P0 = this.Position + a * (other.Position - this.Position) / d;
        Vector2 P1 = new Vector2(P0.x + h * (other.Position.y - this.Position.y) / d, P0.y - h * (other.Position.x - this.Position.x) / d);
        Vector2 P2 = new Vector2(P0.x - h * (other.Position.y - this.Position.y) / d, P0.y + h * (other.Position.x - this.Position.x) / d);

        intersectionPoints.Add(P1);
        if (P1 != P2) // Check if both points are the same
        {
            intersectionPoints.Add(P2);
        }

        return intersectionPoints;
    }
}
