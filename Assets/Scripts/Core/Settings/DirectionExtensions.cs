using UnityEngine;

public enum Direction
{
    North,
    NorthEast,
    East,
    SouthEast,
    South,
    SouthWest,
    West,
    NorthWest
}
public enum DirectionChange 
{ 
    None, 
    TurnRight, 
    TurnLeft, 
    TurnAround 
}

public static class DirectionExtensions
{
    public static Quaternion GetRotation(this Direction direction)
    {
        // to calculate the angle mathematically based on the enum index (0 to 7 multiplied by 45 degrees)
        float angle = (int)direction * 45f;

        return Quaternion.Euler(0f, angle, 0f);
    }

    public static Direction VectorToDirection(Vector3 direction)
    {
        // normalise the vector so that the values are always between -1 and 1
        Vector3 dir = direction.normalized;

        // round X and Z to integers (-1, 0 or 1), as we are moving along a grid
        int x = Mathf.RoundToInt(dir.x);
        int z = Mathf.RoundToInt(dir.z);

        if (x == 0)
        {
            if (z > 0)
            {
                return Direction.North;
            }

            if (z < 0)
            {
                return Direction.South;
            }
        }
        else if (x > 0)
        {
            if (z > 0)
            {
                return Direction.NorthEast;
            }

            if (z == 0)
            {
                return Direction.East;
            }

            if (z < 0)
            {
                return Direction.SouthEast;
            }
        }
        else if (x < 0)
        {
            if (z > 0)
            {
                return Direction.NorthWest;
            }

            if (z == 0)
            {
                return Direction.West;
            }

            if (z < 0)
            {
                return Direction.SouthWest;
            }
        }

        return Direction.North;
    }
}