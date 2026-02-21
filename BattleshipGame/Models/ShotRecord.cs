namespace BattleshipGame.Models;

/// <summary>A record of a shot taken during the battle phase.</summary>
public record ShotRecord(
    string ShooterName,
    int Row,
    int Col,
    ShotResult Result,
    string? SunkShipName
);
