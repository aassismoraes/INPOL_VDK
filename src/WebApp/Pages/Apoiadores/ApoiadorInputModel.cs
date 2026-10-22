using System.ComponentModel.DataAnnotations;

namespace WebApp.Pages.Apoiadores
{
    public class ApoiadorInputModel
    {
        [Required(ErrorMessage = "O campo {0} é de preenchimento obrigatório.")]
        [StringLength(150, ErrorMessage = "O campo {0} deve ter no máximo {1} caracteres.")]
        [Display(Name = "Nome Completo")]
        public string Nome { get; set; } = string.Empty;

        [Display(Name = "CPF")]
        public string? Cpf { get; set; }

        [Display(Name = "Apelido")]
        public string? Apelido { get; set; }

        [Required(ErrorMessage = "O campo {0} é de preenchimento obrigatório.")]
        [Display(Name = "Celular/WhatsApp")]
        public string Telefone { get; set; } = string.Empty;

        [Display(Name = "Telefone Fixo")]
        public string? TelefoneFixo { get; set; }

        [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
        [Display(Name = "E-mail")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "O campo {0} é de preenchimento obrigatório.")]
        [Display(Name = "Data de Nascimento")]
        public DateTime? DataNascimento { get; set; }

        [Required(ErrorMessage = "O campo {0} é de preenchimento obrigatório.")]
        [Display(Name = "Gênero")]
        public string? Genero { get; set; }

        [Display(Name = "Profissão")]
        public string? Profissao { get; set; }

        [Display(Name = "CEP")]
        public string? Cep { get; set; }

        [Display(Name = "Logradouro")]
        public string? Logradouro { get; set; }

        [Display(Name = "Número")]
        public string? Numero { get; set; }

        [Required(ErrorMessage = "O campo {0} é de preenchimento obrigatório.")]
        [Display(Name = "Bairro")]
        public string Bairro { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo {0} é de preenchimento obrigatório.")]
        [Display(Name = "Cidade")]
        public string Cidade { get; set; } = string.Empty;

        [Display(Name = "UF")]
        [StringLength(2, ErrorMessage = "O campo {0} deve ter no máximo 2 caracteres.")]
        public string? Uf { get; set; }
               
        [Required(ErrorMessage = "O campo {0} é de preenchimento obrigatório.")]
        [Display(Name = "Cabo Eleitoral (Padrinho)")]
        public Guid? PadrinhoId { get; set; }

        [Display(Name = "Observações")]
        public string? Observacoes { get; set; }
    }
}