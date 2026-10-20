public struct Genome
{
    
    #region Variables
    
    public readonly float ViewDensity;             // how dense the view field of a mouse is
    public readonly float ViewWidth;               // the width of the view area
    public readonly int ViewRange;               // a mouses' view range
    public readonly int Tenacity;                // how long the mouse tries to get another mouses attention
    public readonly float Appeal;                // how likely the mouse is chosen as a partner
    public readonly float MoveSpeed;             // how fast the mice move
    public readonly float MoveSpeedMultiplyer;   // how much faster the mouse gets when having a specific objective
    public readonly float ZigZag;                // how much the mouse zig zags
    
    #endregion
    
    #region Constructor
    
    public Genome(float viewDensity, float viewWidth, int viewRange, int tenacity, float appeal, float moveSpeed, float moveSpeedMultiplyer, float zigZag)
    {
        ViewDensity = viewDensity;
        ViewWidth = viewWidth;
        ViewRange = viewRange;
        Tenacity = tenacity;
        Appeal = appeal;
        MoveSpeed = moveSpeed;
        MoveSpeedMultiplyer = moveSpeedMultiplyer;
        ZigZag = zigZag;
    }
    
    #endregion
    
}
