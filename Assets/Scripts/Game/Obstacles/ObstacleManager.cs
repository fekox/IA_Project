using UnityEngine;
using System.Collections.Generic;

public class ObstacleManager : MonoBehaviour
{
    const float DISTANCE_BETWEEN_OBSTACLES = 6f;
    const float HEIGHT_RANDOM = 3f;
    const int MIN_COUNT = 3;
    public GameObject prefab;
    public GameObject coinPrefab;
    Vector3 pos = new Vector3(DISTANCE_BETWEEN_OBSTACLES, 0, 0);

    List<Obstacle> obstacles = new List<Obstacle>();
    List<Obstacle> coins = new List<Obstacle>();

    private static ObstacleManager instance = null;

    public static ObstacleManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<ObstacleManager>();

            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    public void Reset()
    {
        for (int i = 0; i < obstacles.Count; i++)
            Destroy(obstacles[i].gameObject);

        obstacles.Clear();

        pos.x = 0;

        InstantiateObstacle();
        InstantiateObstacle();
        InstatiateCoin();
        InstatiateCoin();
    }

    private void InstatiateCoin()
    {
        
        pos.x += DISTANCE_BETWEEN_OBSTACLES/2;
        pos.y = Random.Range(-HEIGHT_RANDOM, HEIGHT_RANDOM);
        GameObject go = GameObject.Instantiate(coinPrefab, pos, Quaternion.identity);
        go.transform.SetParent(this.transform, false);
        Obstacle obstacle = go.GetComponent<Coin>();
        obstacle.OnDestroy += OnCoinDestroy;
        coins.Add(obstacle);;
    }

    public Obstacle GetNextObstacle(Vector3 pos)
    {
        for (int i = 0; i < obstacles.Count; i++)
        {
            if (pos.x < obstacles[i].transform.position.x + 2f)
                return obstacles[i];
        }

        return null;
    }

    public bool IsCollidingObstacle(Vector3 pos)
    {
        Collider2D collider = Physics2D.OverlapBox(pos, new Vector2(0.3f, 0.3f), 0);

        if (collider != null && collider.tag == "Obstacle")
            return true;

        return false;
    } 
    public bool IsCollidingCoin(Vector3 pos, out Obstacle obstacle)
    {
        Collider2D collider = Physics2D.OverlapBox(pos, new Vector2(0.3f, 0.3f), 0);

        if (collider != null && collider.tag == "Coin")
        {
            obstacle = collider.GetComponent<Obstacle>(); 
            return true;
        }

        obstacle = null;
        return false;
    }

    public void CheckAndInstatiate()
    {
        for (int i = 0; i < obstacles.Count; i++)
        {
            obstacles[i].CheckToDestroy();
        }

        for (int i = 0; i < coins.Count; i++)
        {
            coins[i].CheckToDestroy();
        }

        while (obstacles.Count < MIN_COUNT)
            InstantiateObstacle();
        while (coins.Count < MIN_COUNT)
            InstatiateCoin();
    }

    void InstantiateObstacle()
    {
        pos.x += DISTANCE_BETWEEN_OBSTACLES;
        pos.y = Random.Range(-HEIGHT_RANDOM, HEIGHT_RANDOM);
        GameObject go = GameObject.Instantiate(prefab, pos, Quaternion.identity);
        go.transform.SetParent(this.transform, false);
        Obstacle obstacle = go.GetComponent<Obstacle>();
        obstacle.OnDestroy += OnObstacleDestroy;
        obstacles.Add(obstacle);
    }

    void OnObstacleDestroy(Obstacle obstacle)
    {
        obstacle.OnDestroy -= OnObstacleDestroy;
        obstacles.Remove(obstacle);
    }
    
    void OnCoinDestroy(Obstacle obstacle)
    {
        obstacle.OnDestroy -= OnCoinDestroy;
        coins.Remove(obstacle);
    }

    public Obstacle GetNextCoin(Vector3 transformPosition)
    {
        for (int i = 0; i < coins.Count; i++)
        {
            if (transformPosition.x < coins[i].transform.position.x + 2f)
                return coins[i];
        }

        return null;
    }
}
