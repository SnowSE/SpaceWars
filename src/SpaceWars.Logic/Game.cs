using SpaceWars.Logic.Actions;
using SpaceWars.Logic.Exceptions;
using SpaceWars.Logic.Weapons;

namespace SpaceWars.Logic;

public class Game
{
    public const int MaxPlayerCount = 100;

    private readonly List<IPurchasable> Shop = [
        new BasicCannon(),
        new PowerFist(),
        new RailGun()
    ];

    private readonly Dictionary<PlayerToken, Player> players;
    private readonly IInitialLocationProvider locationProvider;
    private ITimer? timer;

    public Game(IInitialLocationProvider locationProvider, ITimer gameTimer = null, int boardWidth = 500, int boardHeight = 500)
    {
        this.players = new();
        this.locationProvider = locationProvider;
        BoardWidth = boardWidth;
        BoardHeight = boardHeight;
        state = GameState.Joining;
        if (gameTimer is not null)
        {
            timer = gameTimer;
            timer.RegisterAction(() => Tick());
        }
        this.Map = new GameMap([], Shop, BoardWidth, BoardHeight);
    }

    public event EventHandler Ticked;

    public GameJoinResult Join(string playerName)
    {
        if (players.Count > MaxPlayerCount)
        {
            throw new TooManyPlayersException();
        }

        var newPlayer = new Player(playerName, new Ship(locationProvider.GetNewInitialLocation(BoardWidth, BoardHeight)));
        players.Add(newPlayer.Token, newPlayer);

        return new GameJoinResult(newPlayer.Token, newPlayer.Ship.Location, newPlayer.Ship.Heading, Shop);
    }

    public void Start()
    {
        if (State != GameState.Joining)
        {
            throw new InvalidGameStateException();
        }

        state = GameState.Playing;
        timer?.Start();
    }

    public void Stop()
    {
        if (State != GameState.Playing)
        {
            throw new InvalidGameStateException();
        }

        state = GameState.GameOver;
        timer?.Stop();
    }

    public void Reset()
    {
        state = GameState.Joining;
        Map = new GameMap([], null, BoardWidth, BoardHeight);
        players.Clear();
        timer?.Stop();
    }

    private GameState state;

    public GameState State => state;

    public IEnumerable<Location> PlayerLocations => players.Values
        .Where(p => p.IsAlive)
        .Select(p => p.Ship.Location);

    public void Tick()
    {
        var playerActions = players.Values.Select(p => new PlayerAction(p, p.DequeueAction()))
            .Where(playerAction => playerAction.Action != null && playerAction.Player.IsAlive)
            .OrderBy(playerAction => playerAction.Action!.Priority)
            .ToList();

        foreach (var gamePlayAction in playerActions.Where(a => a.Action.Priority == 1))
        {
            gamePlayAction.Action.Execute(gamePlayAction.Player, Map);
        }

        //Check for collisions here
        checkCollision();

        Map = new GameMap(players.Values, Shop, BoardWidth, BoardHeight);//initialize that here

        foreach (var gamePlayAction in playerActions.Where(a => a.Action.Priority != 1))
        {
            gamePlayAction.Action.Execute(gamePlayAction.Player, Map);
        }

        foreach (var player in Map.Players)
        {
            if (player.IsAlive)
            {
                player.Score += 1;
                player.Ship.RepairCreditBalance += 1;
                player.Ship.UpgradeCreditBalance += 1;
            }
        }

        Ticked?.Invoke(this, EventArgs.Empty);
    }

    public Player GetPlayerByToken(PlayerToken token) => players[token];
    public int BoardWidth { get; }
    public int BoardHeight { get; }

    public IEnumerable<GamePlayAction> GetActionsForPlayer(PlayerToken playerToken) => players[playerToken].GetActions();
    public IEnumerable<PlayerMessage> GetPlayerMessages(PlayerToken playerToken) => players[playerToken].ReadMessages();
    public GameMap Map { get; private set; }

    public void EnqueueAction(PlayerToken token, GamePlayAction action) => players[token].EnqueueAction(action);
    public void ClearActions(PlayerToken token) => players[token].ClearActions();

    public IEnumerable<Player> GetPlayersInRange(Player player, int maxDistance) => Map.GetPlayersInWeaponRange(player, maxDistance);
    public IEnumerable<Player> GetOtherPlayers(Player player) => players.Values.Where(p => p != player);
    public IEnumerable<TargetedLocation> GetPotentialTargets(Player player, Weapon weapon) => weapon.GetPotentialTargets(player, Map);
    private void checkCollision()
    {
        List<Player> collidedPlayers = new();

        for (int i = 0; i < players.Count; i++)
        {
            var player1 = players.Values.ElementAt(i);
            if (collidedPlayers.Contains(player1)) { continue; }

            for (int j = i + 1; j < players.Count; j++)
            {
                var player2 = players.Values.ElementAt(j);
                if (collidedPlayers.Contains(player2)) { continue; }

                var ship1Location = player1.Ship.Location;
                var ship2Location = player2.Ship.Location;

                if (player1 != player2 && ship1Location == ship2Location)
                {
                    if (!collidedPlayers.Contains(player1)) { collidedPlayers.Add(player1); }
                    if (!collidedPlayers.Contains(player2)) { collidedPlayers.Add(player2); }
                }
            }
        }

        foreach (var collidedPlayer in collidedPlayers)
        {
            collidedPlayer.Ship.Location = bounceShip(collidedPlayer.Ship.Location);

            collidedPlayer.Ship.TakeDamage(10);
        }
    }

    private bool isSpotOpen(Location location)
    {
        foreach (var player in players)
        {
            if (player.Value.Ship.Location == location)
            {
                return false;
            }
        }
        return true;
    }

    private Location bounceShip(Location shipLocation)
    {
        Location[] directions = [new(shipLocation.X - 1, shipLocation.Y),
            new(shipLocation.X + 1, shipLocation.Y),
            new(shipLocation.X, shipLocation.Y + 1),
            new(shipLocation.X, shipLocation.Y - 1),
            new(shipLocation.X - 1, shipLocation.Y + 1),
            new(shipLocation.X + 1, shipLocation.Y + 1),
            new(shipLocation.X - 1, shipLocation.Y - 1),
            new(shipLocation.X + 1, shipLocation.Y - 1)];

        foreach (var direction in directions)
        {
            if (isSpotOpen(direction))
            {
                return direction;
            }
        }

        return shipLocation;
    }

    public void UpdateFrequency(int newFrequency)
    {
        timer.Frequency = TimeSpan.FromMilliseconds(newFrequency);
    }
}

record PlayerAction(Player Player, GamePlayAction? Action);
public record GameJoinResult(PlayerToken Token, Location Location, int heading, List<IPurchasable> Shop);
