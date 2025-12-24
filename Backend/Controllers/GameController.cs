using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("")]
[EnableCors]
public class GameController(GameService gameService) : ControllerBase
{
	[HttpGet("rooms")]
	public IActionResult Rooms()
		=> Ok(gameService.GetRooms());

	[HttpGet("create/{ante}")]
	public async Task<IActionResult> Create(int ante)
		=> Ok(await gameService.CreateGame(ante));

	[HttpPost("{gameId}/join")]
	public async Task<IActionResult> Join(string gameId, [FromBody] Player player)
	{
		var success = await gameService.JoinGame(gameId, player);

		if (!success)
			return BadRequest("Cannot join game.");

		return Ok(new PlayerCredentials(player));
	}

	[HttpGet("{gameId}/{playerId}/state")]
	public IActionResult GetPlayerView(string gameId, string playerId)
	{
		var view = gameService.GetPlayerView(gameId, playerId);

		if (view == null)
			return NotFound();

		return Ok(view);
	}

	[HttpGet("{gameId}/{playerId}/ready")]
	public IActionResult PlayerReady(string gameId, string playerId)
		=> Ok(gameService.ConfirmPlayerReady(gameId, playerId));

	[HttpPost("{gameId}/{playerId}/action")]
	public IActionResult Action(string gameId, string playerId, [FromBody] ActionRequest action)
	{
		var success = gameService.SubmitAction(gameId, playerId, action.Type, action.Amount);

		if (!success)
			return BadRequest("Invalid action.");

		return Ok();
	}
}

