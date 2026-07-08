using System.Collections.Generic;
using UnityEngine;

public enum RotationType
{
    None = 0,
    Rotate_0 = 1,
    Rotate_90 = 2,
    Rotate_180 = 3,
    Rotate_270 = 4
}

public static class RotationTypeExtensions
{
    public static float ConvertToAngle(RotationType rotationType)
    {
        if (rotationType == RotationType.Rotate_0)
        {
            return 0;
        }
        if (rotationType == RotationType.Rotate_90)
        {
            return -90;
        }
        if (rotationType == RotationType.Rotate_180)
        {
            return -180;
        }
        if (rotationType == RotationType.Rotate_270)
        {
            return -270;
        }
        return 0;
    }

    public static RotationType Rotate90(RotationType rotationType)
    {
        List<RotationType> circleRotations = new()
        {
            RotationType.Rotate_0,
            RotationType.Rotate_90,
            RotationType.Rotate_180,
            RotationType.Rotate_270
        };

        var index = circleRotations.IndexOf(rotationType);
        int rotate90Index = index + 1;
        if (rotate90Index >= circleRotations.Count)
        {
            rotate90Index = 0;
        }
        if (rotate90Index < 0)
        {
            rotate90Index = 0;
        }
        return circleRotations[rotate90Index];
    }

    public static RotationType Add(RotationType a, RotationType b)
    {
        List<RotationType> circleRotations = new()
        {
            RotationType.Rotate_0,
            RotationType.Rotate_90,
            RotationType.Rotate_180,
            RotationType.Rotate_270
        };

        var indexA = circleRotations.IndexOf(a);
        var indexB = circleRotations.IndexOf(b);
        if (indexA < 0) indexA = 0;
        if (indexB < 0) indexB = 0;

        int finalIndex = (indexA + indexB) % circleRotations.Count;
        return circleRotations[finalIndex];
    }
}