public class Timer
{
        
    #region Variables
        
    private readonly int maxValue;
    private int currentValue;
    private bool active;
        
    #endregion
        
    #region Contructor
    
    public Timer(int maxValue)
    {
        this.maxValue = maxValue;
        currentValue = 0;
        active = true;
    }

    public Timer(int currentValue, int maxValue)
    {
        this.maxValue = maxValue;
        this.currentValue = currentValue;
        active = true;
    }
        
    #endregion

    #region Functions
        
    public void Activate()
    {
        active = true;
    }
        
    public void Deactivate()
    {
        active = false;
    }

    public void Inc()
    {
        if (active && !Evaluate())
            currentValue++;
    }
        
    public void Inc(int amount)
    {
        if (active && !Evaluate())
            currentValue += amount;
    }
    
    public void Dec()
    {
        if (active && currentValue > 0)
            currentValue--;
    }
        
    public void Dec(int amount)
    {
        if (active && currentValue > 0)
            currentValue += amount;
    }
        
    public void Reset()
    {
        if (active)
            currentValue = 0;
    }

    public bool Evaluate()
    {
        return currentValue >= maxValue;
    }
        
    public bool EvaluateEquals()
    {
        return currentValue == maxValue;
    }

    #endregion
        
    #region Getter
        
    public int GetTimeLeft()
    {
        return maxValue - currentValue;
    }
        
    public int GetMaxValue()
    {
        return currentValue;
    }
        
    public int GetCurrentValue()
    {
        return currentValue;
    }
        
    #endregion
        
}
