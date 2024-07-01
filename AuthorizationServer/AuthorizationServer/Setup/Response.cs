using System.ComponentModel.DataAnnotations;

namespace AuthorizationServer.Setup
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="status"></param>        
    public class Response(int status)
    {
        /// <summary>
        /// Código de resultado HTTP 
        /// </summary>

        [Required(ErrorMessage = "Campo obrigatório não fornecido")]
        public int Status { get; set; } = status;
    }
}
