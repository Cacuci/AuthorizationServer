using System.ComponentModel.DataAnnotations;

namespace AuthorizationServer.DTOs
{
    public class AuthRequest
    {
        /// <summary>
        /// Login de acesso
        /// </summary>
        [Required(ErrorMessage = "Campo obrigatório não fornecido")]
        [MaxLength(40, ErrorMessage = "Valor não deve ser maior que 40 caracteres")]
        [Display(Name = "Código do depósito/loja")]
        public string Login { get; set; }

        /// <summary>
        /// Senha de acesso
        /// </summary>
        [Required(ErrorMessage = "Campo obrigatório não fornecido")]
        [MaxLength(20, ErrorMessage = "Valor não deve ser maior que 20 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; }
    }
}
