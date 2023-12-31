﻿namespace SpaceWars.Logic;

public interface IPurchasable
{
    int Cost { get; init; }
    string Name { get; }
    IEnumerable<string> PurchasePrerequisites { get; set; }
}
