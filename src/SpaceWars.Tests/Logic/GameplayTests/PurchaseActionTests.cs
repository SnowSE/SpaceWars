using SpaceWars.Logic.Weapons;
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

        var purchaseAction = new PurchaseAction("Basic Cannon");
        var map = new GameMap([p1], new List<IPurchasable> { new BasicCannon() });

        var res = purchaseAction.Execute(p1, map);
        p1.Ship.Weapons.Should().HaveCount(2);
        p1.Ship.UpgradeCreditBalance.Should().Be(Math.Abs(new BasicCannon().PurchaseCost - startingCreditBalance));
        res.Success.Should().BeTrue();
        res.Message.Should().Be("Basic Cannon");
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

        var purchaseAction = new PurchaseAction("Basic Cannon");
        var map = new GameMap([p1], new List<IPurchasable> { new BasicCannon() });

        var res = purchaseAction.Execute(p1, map);
        res.Success.Should().BeFalse();
        res.Message.Should().Be("Upgrade Credit Balance insufficient to purchase Basic Cannon");
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

        var purchaseAction = new PurchaseAction("Basic Cannon");
        var map = new GameMap([p1], new List<IPurchasable> { });

        var res = purchaseAction.Execute(p1, map);
        res.Success.Should().BeFalse();
        res.Message.Should().Be("Basic Cannon is not in shop");
    }

    [Fact]
    public void CanPurchaseBasicCannonButInsufficientFundsForPowerFist()
    {
        var p1 = new Player("Player 1", new Ship(new Location(0, 0))
        {
            Heading = 0,
        });
        p1.Ship.UpgradeCreditBalance = 15;
        var map = new GameMap([p1], [new BasicCannon(), new PowerFist()]);

        var res = new PurchaseAction("Basic Cannon").Execute(p1, map);
        res.Success.Should().BeTrue();
        res.Message.Should().Be("Basic Cannon");

        var res2 = new PurchaseAction("Power Fist").Execute(p1, map);
        res2.Success.Should().BeFalse();
        res2.Message.Should().Be("Upgrade Credit Balance insufficient to purchase Power Fist");
    }

    [Fact]
    public void CanPurchaseBasicCannonAndPowerFist()
    {
        var p1 = new Player("Player 1", new Ship(new Location(0, 0))
        {
            Heading = 0,
        });
        p1.Ship.UpgradeCreditBalance = 1500;
        var map = new GameMap([p1], [new BasicCannon(), new PowerFist()]);

        var res = new PurchaseAction("Basic Cannon").Execute(p1, map);
        res.Success.Should().BeTrue();
        res.Message.Should().Be("Basic Cannon");

        var res2 = new PurchaseAction("Power Fist").Execute(p1, map);
        res2.Success.Should().BeTrue();
        res2.Message.Should().Be("Power Fist");
    }
}
