using System.ComponentModel.DataAnnotations;

namespace AuthBI.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Nome de usuário é obrigatório")]
        public string Username { get; set; }

        [Required(ErrorMessage = "E-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
