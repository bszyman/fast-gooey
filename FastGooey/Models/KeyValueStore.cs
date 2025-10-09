using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Models;

[Index(nameof(Key), IsUnique = true)]
public class KeyValueStore
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    public required string Key { get; set; }
    
    [MaxLength(2000)]
    public string Value { get; set; } = string.Empty;
}