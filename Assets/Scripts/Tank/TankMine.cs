using UnityEngine;

public class TankMine : MonoBehaviour, IMinable
{
    private bool isGoodMine = false;

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public bool IsGoodMine()
    {
        return isGoodMine;
    }

    public void SetMine(bool value)
    {
        isGoodMine = value;
        GetComponent<MeshRenderer>().material.color = value ? Color.green : Color.red;
    }

    public void SetPosition(Vector3 getRandomPos)
    {
        transform.position = getRandomPos;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
}