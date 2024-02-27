using System.Numerics;

namespace SpaceWars.Logic;

public class GameMap
{
    public GameMap(IEnumerable<Player> players, IEnumerable<IPurchasable>? NewShop, int boardWidth = 2000, int boardHeight = 2000)
    {
        foreach (var p in players)
        {
            playerLocations.Add(p.Ship.Location, p);
        }
        if (NewShop is not null)
        {
            this.CurrentShop = NewShop;
        }
        else
        {
            this.CurrentShop = new List<IPurchasable>() { };
        }
        this.Players = players;
        this.BoardWidth = boardWidth;
        this.BoardHeight = boardHeight;
    }

    private Dictionary<Location, Player> playerLocations = new Dictionary<Location, Player>();
    public IEnumerable<Player> Players { get; private set; }
    public IEnumerable<IPurchasable>? CurrentShop { get; private set; }

    public readonly int BoardWidth;
    public readonly int BoardHeight;



    public IEnumerable<Player> GetPlayersInWeaponRange(Player player, int maxDistance)
    {
        var playerLocation = new Vector2(player.Ship.Location.X, player.Ship.Location.Y);

        foreach (var otherPlayer in Players)
        {
            if (otherPlayer == player || !otherPlayer.IsAlive)
                continue;

            var otherLocation = new Vector2(otherPlayer.Ship.Location.X, otherPlayer.Ship.Location.Y);
            var distance = (int)Vector2.Distance(playerLocation, otherLocation);
            if (distance <= maxDistance)
                yield return otherPlayer;
        }

        //return from otherPlayer in players
        //       let otherPlayerLocation = new Vector2(otherPlayer.Ship.Location.X, otherPlayer.Ship.Location.Y)
        //       let distance = (int)Vector2.Distance(playerLocation, otherPlayerLocation)
        //       where distance <= maxDistance
        //       select otherPlayer;
    }

}