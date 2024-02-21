using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SpaceWars.Logic;
using SpaceWars.Logic.Actions;
using SpaceWars.Logic.Exceptions;
using SpaceWars.Web.Types;

namespace SpaceWars.Web.Controllers;

[ApiController]
[Route("[controller]")]
public partial class GameController(ILogger<GameController> logger, Game game, IOptions<GameConfig> gameConfig) : ControllerBase
{
    [LoggerMessage(6, LogLevel.Warning, "Player {name} failed to join game. Too many players")] partial void LogTooManyPlayers(string name, ILogger<GameController> logger);

    [HttpGet("join")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(JoinGameResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<JoinGameResponse>> JoinGame(string name)
    {
        try
        {
            var joinResult = game.Join(name);
            var shopItems = joinResult.Shop.Select(item => new PurchasableItem(item.Name, item.MaxDamage, item.PurchaseCost, item.Ranges)).ToList();

            return new JoinGameResponse(
                joinResult.Token.ToString(),
                joinResult.Location.ToApiLocation(),
                game.State.ToString(),
                joinResult.heading,
                game.BoardHeight,
                game.BoardWidth,
                shopItems
            );
        }
        catch (TooManyPlayersException)
        {
            LogTooManyPlayers(name, logger);
            return Problem("Cannot join game, too many players.", statusCode: 400, title: "Too many players");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error joining game");
            return Problem("Error joining game", statusCode: 500, title: "Join Game Error");
        }
    }

    [HttpGet("start")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> StartGame(string password)
    {
        if (gameConfig.Value.Password == password)
        {
            game.Start();
            return Ok();
        }
        return BadRequest();
    }

    [HttpGet("state")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GameStateResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<GameStateResponse> GameStateResponse()
    {
        return new GameStateResponse(
            game.State.ToString(),
            game.PlayerLocations.Select(l => l.ToApiLocation())
        );
    }

    [HttpGet("playermessages")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PlayerMessageResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IEnumerable<PlayerMessageResponse>> GetPlayerMessagesAsync(string token)
    {
        var player = game.GetPlayerByToken(new PlayerToken(token));
        return player.DequeueMessages().Select(m => new PlayerMessageResponse(m.Type.ToString(), m.Message));
    }


    //POST /game/{token}/queue/[{type=move,request=250}]
    //POST /game/{token}/queue/[{type=fire,request=BasicCannon}]
    //POST /game/{token}/queue/[{type=repair,request=Null}]
    //POST /game/{token}/queue/[{type=purchase,request=item}]
    //POST /game/{token}/queue/[{type=name,request=item},{type=name,request=item}]
    [HttpPost("{token}/queue")]
    public async Task<QueueActionResponse> QueuePlayerAction(string token, IEnumerable<QueueActionRequest> actions)
    {
        var player = game.GetPlayerByToken(new PlayerToken(token));

        if (player == null) { return new QueueActionResponse("Player token invalid"); }

        foreach (var action in actions)
        {
            switch (action.Type)
            {
                case "move":
                    if (action.Request == null) { return new QueueActionResponse("Failed to queue action"); }

                    MoveForwardAction moveAction = new(Int32.Parse(action.Request));
                    player.EnqueueAction(moveAction);

                    break;
                case "fire":
                    if (action.Request == null) { return new QueueActionResponse("Failed to queue action"); }

                    FireWeaponAction fireAction = new(action.Request);
                    player.EnqueueAction(fireAction);

                    break;
                case "repair":
                    RepairAction repairAction = new();
                    player.EnqueueAction(repairAction);
                    break;
                case "purchase":
                    if (action.Request == null) { return new QueueActionResponse("Failed to queue action"); }

                    PurchaseAction purchaseAction = new(action.Request);
                    player.EnqueueAction(purchaseAction);

                    break;
                case "changeHeading":
                    if (action.Request == null) { return new QueueActionResponse("Failed to queue action"); }

                    ChangeHeadingAction changeHeadingAction = new(Int32.Parse(action.Request));
                    player.EnqueueAction(changeHeadingAction);

                    break;
                case "clear":
                    player.ClearActions();
                    return new QueueActionResponse("Actions cleared");
                default:
                    return new QueueActionResponse("Failed to queue action");

            }
        }
        return new QueueActionResponse("Action queued");

    }


    [HttpDelete("{token}/queue/clear")]
    public async Task<QueueActionResponse> ClearPlayerQueue(string token)
    {
        var player = game.GetPlayerByToken(new PlayerToken(token));

        if (player == null) { return new QueueActionResponse("Player token invalid"); }
        player.ClearActions();
        return new QueueActionResponse("Actions cleared");
    }
}

public static class Extensions
{

    public static Types.Location ToApiLocation(this Logic.Location gameLocation) => new Types.Location(gameLocation.X, gameLocation.Y);

}