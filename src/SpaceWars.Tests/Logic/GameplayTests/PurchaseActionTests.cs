﻿using SpaceWars.Logic.Weapons;
namespace SpaceWars.Tests.Logic.GameplayTests;

public class PurchaseActionTests
{
    [Fact]
    public void CanPurchaseItem()
    {
        var p1 = new Player("Player 1", new Ship(new Location(0, 0))
        {
            Heading = 0,
        });
        int startingCreditBalance = 300;
        p1.Ship.UpgradeCreditBalance = startingCreditBalance;

        var purchaseAction = new PurchaseAction(new BasicCannon());
        var map = new GameMap([p1], new List<IPurchaseable> { new BasicCannon()});

        var res = purchaseAction.Execute(p1, map);
        p1.Ship.Weapons.Should().HaveCount(2);
        p1.Ship.UpgradeCreditBalance.Should().Be(Math.Abs(new BasicCannon().Cost - startingCreditBalance));
        res.Success.Should().BeTrue();
        res.Message.Should().Be("Basic Cannon purchased");
    }

    [Fact]
    public void CannotPurchaseItem_TooPoor()
    {
        var p1 = new Player("Player 1", new Ship(new Location(0, 0))
        {
            Heading = 0,
        });
        int startingCreditBalance = 0;
        p1.Ship.UpgradeCreditBalance = startingCreditBalance;

        var purchaseAction = new PurchaseAction(new BasicCannon());
        var map = new GameMap([p1], new List<IPurchaseable> { new BasicCannon() });

        var res = purchaseAction.Execute(p1, map);
        res.Success.Should().BeFalse();
        res.Message.Should().Be("Not enough credit to purchase item Basic Cannon");
    }

    [Fact]
    public void CannotPurchaseItem_NotInShop()
    {
        var p1 = new Player("Player 1", new Ship(new Location(0, 0))
        {
            Heading = 0,
        });
        int startingCreditBalance = 300;
        p1.Ship.UpgradeCreditBalance = startingCreditBalance;

        var purchaseAction = new PurchaseAction(new BasicCannon());
        var map = new GameMap([p1], new List<IPurchaseable> { });

        var res = purchaseAction.Execute(p1, map);
        res.Success.Should().BeFalse();
        res.Message.Should().Be("Basic Cannon is not in shop");
    }

    [Fact]
    public void CannotPurchaseItem_DoesntMeetPrereqs()
    {
        var p1 = new Player("Player 1", new Ship(new Location(0, 0))
        {
            Heading = 0,
        });
        int startingCreditBalance = int.MaxValue;
        p1.Ship.UpgradeCreditBalance = startingCreditBalance;
        var fancyLaserMock = new Mock<IWeapon>();
        fancyLaserMock.Setup(m => m.Name).Returns("Fancy Laser");
        fancyLaserMock.Setup(m => m.PurchasePrerequisites).Returns(new List<string>() { "Fake Weapon"});
        fancyLaserMock.Setup(m => m.Cost).Returns(0);

        var purchaseAction = new PurchaseAction(fancyLaserMock.Object);
        var map = new GameMap([p1], new List<IPurchaseable> { new BasicCannon() });

        var res = purchaseAction.Execute(p1, map);
        res.Success.Should().BeFalse();
        res.Message.Should().Be("Player does not have prerequisites");
    }

    [Fact]
    public void CanPurchaseItem_MeetsPrereqs()
    {
        var p1 = new Player("Player 1", new Ship(new Location(0, 0))
        {
            Heading = 0,
        });
        int startingCreditBalance = int.MaxValue;
        p1.Ship.UpgradeCreditBalance = startingCreditBalance;
        var fancyLaserMock = new Mock<IWeapon>();
        fancyLaserMock.Setup(m => m.Name).Returns("Fancy Laser");
        fancyLaserMock.Setup(m => m.PurchasePrerequisites).Returns(new List<string>() { "Basic Cannon" });
        fancyLaserMock.Setup(m => m.Cost).Returns(0);

        var purchaseAction = new PurchaseAction(fancyLaserMock.Object);
        var map = new GameMap([p1], new List<IPurchaseable> { new BasicCannon() });

        var res = purchaseAction.Execute(p1, map);
        res.Success.Should().BeTrue();
        res.Message.Should().Be("Fancy Laser purchased");
    }
}
