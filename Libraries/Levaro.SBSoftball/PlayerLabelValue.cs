namespace Levaro.SBSoftball
{
    /// <summary>
    /// When player information is gathered from the Web site in the 
    /// <see cref="Game.ConstructTeams(HtmlAgilityPack.HtmlDocument)"/> static method, the player stats labels (HR, 1B, etc) are
    /// different from the <see cref="Player"/> property names (one reason is that property names cannot start with
    /// numerics). This record is used to provide a vehicle to pass information to the 
    /// <see cref="Player.ConstructPlayer"/> static method.
    /// </summary>
    /// <param name="Label">Sets the value of the automatic <c>Label</c> property.</param>
    /// <param name="Value">Sets the value of the automatic <c>Value</c> property.</param>
    /// <remarks>
    /// Instances are only created by the <c>ConstructTeams</c> methods and only consumed the <c>ConstructPlayer</c>
    /// </remarks>
    public record PlayerLabelValue(string Label, string Value);
    //{
    //    /// <summary>
    //    /// Gets and initializes the property label
    //    /// </summary>
    //    public string Label
    //    {
    //        get;
    //        init;
    //    }

    //    /// <summary>
    //    /// Gets and initializes the property value
    //    /// </summary>
    //    public string Value
    //    {
    //        get;
    //        init;
    //    }
    //}
}
