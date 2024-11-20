using System;

public class Mine : IPlace
{
    private int gold = 3;
    private int food = 15;
    public Action<Mine> OnGoldEmpty;
    public Action<Mine> OnFoodEmpty;
    public bool TryGetFood()
    {
        if (food > 0)
        {
            food--;
            return true;
        }
        OnFoodEmpty?.Invoke(this);
        return false;
    }

    public bool TryGetGold()
    {
        if (gold > 0)
        {
            gold--;
            return true;
        }
        OnGoldEmpty?.Invoke(this);
        return false;
    }

    public bool hasGold => gold > 0;
    public bool hasFood => food > 0;
    

    public void SetFood(int food) => this.food = food;
    public void ActionOnPlace()
    {
        
    }
    
}