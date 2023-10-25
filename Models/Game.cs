using ChessLogic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Models;

public class Game
{
    [Key]
    public long Id { get; set; }
    [AllowNull]
    public string? WhitePlayerId { get; set; }
    [ForeignKey("WhitePlayerId"), ValidateNever]
    public ApplicationUser WhitePlayer { get; set; }
    [AllowNull]
    public string? BlackPlayerId { get; set; }
    [ForeignKey("BlackPlayerId"), ValidateNever]
    public ApplicationUser BlackPlayer { get; set; }
    [AllowNull]
    public string? WinnerId { get; set; }
    [ForeignKey("WinnerId"), ValidateNever]
    public ApplicationUser Winner { get; set; }
    [Required, Range(1, 30)]
    public int TimeControl { get; set; }
    [Required, Range(0, 30)]
    public int Increment { get; set; } = 0;
    [Required]
    public bool IsFinished { get; set; } = false;
    [NotMapped]
    public int Minutes
    {
        get
        {
            return TimeControl / 60;
        }
    }
    [NotMapped]
    public GameManager GameManager { get; set; }
}
