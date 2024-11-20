using System;
using UnityEngine;

[Serializable]
public class SegmentLimit
{
    [SerializeField] private Transform origin;
    [SerializeField] private Transform final;
    [SerializeField] private DirectionLimit directionLimit = DirectionLimit.None;
    private Vector2 opositePosition;

    public Vector2 Origin => origin.position;  // Transform.position returns a Vector3, so it needs to be converted to Vector2
    public Vector2 Final => final.position;    // Same as above

    public Vector2 GetOpositePosition(Vector2 pos)
    {
        Vector2 newPos = Vector2.zero;
        
        float distanceX = Mathf.Abs(Mathf.Abs(pos.x) - Mathf.Abs(origin.position.x)) * 2;
        float distanceY = Mathf.Abs(Mathf.Abs(pos.y) - Mathf.Abs(origin.position.y)) * 2; 

        switch (directionLimit)
        {
            case DirectionLimit.None:
                Debug.LogWarning("Está en None el Limite.");
                break;
            case DirectionLimit.Left:
                newPos.x = pos.x - distanceX;
                newPos.y = pos.y; // y remains the same
                break;
            case DirectionLimit.Up:
                newPos.x = pos.x; // x remains the same
                newPos.y = pos.y + distanceY; // Adjusting y for upward movement
                break;
            case DirectionLimit.Right:
                newPos.x = pos.x + distanceX;
                newPos.y = pos.y; // y remains the same
                break;
            case DirectionLimit.Down:
                newPos.x = pos.x; // x remains the same
                newPos.y = pos.y - distanceY; // Adjusting y for downward movement
                break;
            default:
                Debug.LogWarning("Default el Limite.");
                break;
        }

        opositePosition = newPos;
        return newPos;
    }
}
enum DirectionLimit
{
    None,
    Left,
    Up,
    Right,
    Down
}