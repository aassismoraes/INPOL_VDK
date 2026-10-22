namespace WebApp.Pages.Apoiadores
{
    public class ApoiadorItemDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Apelido { get; set; }
        public string? Telefone { get; set; }
        public string? Email { get; set; }
        public string? Cpf { get; set; }
        public string? Bairro { get; set; }
        public string? Cidade { get; set; }
        public string? Uf { get; set; }
        public string StatusTexto { get; set; } = "ATIVO";
        public DateTime? CriadoEm { get; set; }
        public string? PadrinhoNome { get; set; }
    }

}