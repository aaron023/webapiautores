using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;

namespace WebApiAutores.DTOs
{
    public class CredencialesUsuario
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
