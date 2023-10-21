using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Models;

public class Game
{
    [Key]
    public long Id { get; set; }
    [Required, AllowNull]
    public string? WhitePlayerId { get; set; }
    [ForeignKey("WhitePlayerId"), ValidateNever]
    public ApplicationUser WhitePlayer { get; set; }
    [Required, AllowNull]
    public string? BlackPlayerId { get; set; }
    [ForeignKey("BlackPlayerId"), ValidateNever]
    public ApplicationUser BlackPlayer { get; set; }
    [Required, AllowNull]
    public string? WinnerId { get; set; }
    [ForeignKey("WinnerId"), ValidateNever]
    public ApplicationUser Winner { get; set; }
    [Required]
    public int TimeControl { get; set; }
    [Required]
    public bool IsFinished { get; set; }
}
