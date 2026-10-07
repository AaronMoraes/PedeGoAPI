using System.ComponentModel.DataAnnotations;

namespace GestaodePedidosAPI.DTOs;

public class ProdutoDto
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 80 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Range(0.01, 9999.99, ErrorMessage = "O preço deve estar entre 0,01 e 9999,99.")]
    public decimal Preco { get; set; }

    [Required(ErrorMessage = "A categoria é obrigatória.")]
    public string Categoria { get; set; } = string.Empty;

    [StringLength(600, ErrorMessage = "A descrição deve ter no máximo 600 caracteres.")]
    public string Descricao { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Imagem inválida.")]
    public string ImagemUrl { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "O selo deve ter no máximo 20 caracteres.")]
    public string Selo { get; set; } = string.Empty;

    public bool NaHome { get; set; }

    public bool Ativo { get; set; } = true;
}
