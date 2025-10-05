using System.Collections.Generic;
using UnityEngine;

public static class PathValidator
{

    //validates pos of lily pads and creates a solvable path
    public static bool ValidateLilyPadPath(List<int> lilyPadPositions, float maxJumpDistance = 2.0f, int laneWidth = 10)
    {
        if(lilyPadPositions == null || lilyPadPositions.Count == 0) return false;
        if(lilyPadPositions.Count < 2 ) return false;

        List<int> sortedPos = new List<int>(lilyPadPositions); //sorting pos to check for continuity
        sortedPos.Sort();

        for(int i = 0; i < sortedPos.Count - 1; i++)
        {
            float distance= Mathf.Abs(sortedPos[i + 1] - sortedPos[i]);
            if (distance > maxJumpDistance) 
            {
                Debug.LogWarning($"the path validation has failed, the gap is too large at {sortedPos[i]} and {sortedPos[i + 1]} (distance: {distance})");
                return false;
            }
        }

        //checking if the path covers the lane correctly
        int minX = sortedPos[0];
        int maxX = sortedPos[sortedPos.Count - 1];

        if (maxX - minX < laneWidth * 0.6f)
        {
            Debug.LogWarning($"Path validation failed: Insufficient lane coverage (min: {minX}, max: {maxX})");
            return false;
        }
        return true;
    }

    //generates a path so that there is always a way for the player to move
    public static List<int> GenerateSolvableLilyPadPath(int laneWidth = 10, float minSpacing = 2.0f, float maxSpacing = 4.0f, float maxJumpDistance = 4.0f)
    {
        List<int> path = new List<int>();

        int startX = -laneWidth/2;
        int endX = laneWidth/2;
        int lilypadCount = Random.Range(2, 4);

        if (lilypadCount == 2)
        {
            // Two lily pads: start and end with some variation
            path.Add(startX + Random.Range(1, 3));
            path.Add(endX - Random.Range(1, 3));
        }
        else if (lilypadCount == 3)
        {
            // Three lily pads: start, middle, end
            path.Add(startX + Random.Range(1, 2));
            path.Add(Random.Range(-1, 2)); // Middle area
            path.Add(endX - Random.Range(1, 2));
        }

        // Sort to ensure proper order
        path.Sort();

        //ensuring we have atleast 2 lily pads
        if (path.Count < 2)
        {
            Debug.LogWarning("Generated path has insufficient lily pads, adding more...");
            int secondX = Mathf.Clamp(startX + Mathf.RoundToInt(minSpacing), -laneWidth / 2, laneWidth / 2);
            if (!path.Contains(secondX))
            {
                path.Add(secondX);
            }
        }
        return path;
    }


    //this checks if the decorations are blocking the lily pad or not
    public static bool WouldBlockLilyPadPath(int decorationX, List<int> lilyPadPos, float clearRad = 1.0f)
    {
        if(lilyPadPos == null|| lilyPadPos.Count == 0)
        {
            return false;
        }
        foreach(int lily in lilyPadPos)
        {
            if (Mathf.Abs(decorationX - lily) < clearRad)
            {
                return true;
            }
        }
        return false;
    }

    //finds safe pos for decorations that don't block the lily pads
    public static List<int> FindSafeDecoPos(List<int> lilyPadPos, int laneWidth = 10, float clearRadius = 1.0f, int maxAttempts = 50) 
    {
        List<int> safePos = new List<int>();

        for(int x = -laneWidth / 2; x <= laneWidth / 2; x++)
        {
            if(!WouldBlockLilyPadPath(x, lilyPadPos, clearRadius))
            {
                safePos.Add(x);
            }
        }
        return safePos;
    }
}
