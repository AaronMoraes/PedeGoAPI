using GestaodePedidosAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GestaodePedidosAPI.Data;

/// <summary>
/// Cardápio de demonstração. É inserido na inicialização SOMENTE se a tabela de produtos estiver vazia,
/// então nunca conflita com dados existentes nem sobrescreve o que o admin cadastrou.
/// Para começar com o cardápio vazio, defina CARDAPIO_DEMO=false.
/// </summary>
public static class CardapioInicial
{
    public static async Task PopularSeVazio(AppDbContext db)
    {
        if (await db.Produtos.AnyAsync())
            return;

        db.Produtos.AddRange(Itens());
        await db.SaveChangesAsync();
    }

    private static Produto[] Itens() => new[]
    {
            new Produto { Nome = "X-Burguer", Preco = 10.00m, Categoria = "lanche", Descricao = "Carne artesanal\n2x Mussarela\nCebola\nPicles\nKetchup\nMostarda", ImagemUrl = "imagens/lanches/x-burger.jpg", Selo = "Mais Pedido", NaHome = false, Ativo = true },
            new Produto { Nome = "X-Salada", Preco = 15.00m, Categoria = "lanche", Descricao = "Carne artesanal\nMussarela\nAlface\nTomate\nMolho especial\nBatata palha", ImagemUrl = "imagens/lanches/x-salada.jpg", Selo = "", NaHome = false, Ativo = true },
            new Produto { Nome = "X-Egg", Preco = 17.00m, Categoria = "lanche", Descricao = "Carne artesanal\n2 Ovos\nMussarela\nAlface\nTomate\nMolho especial", ImagemUrl = "imagens/lanches/x-egg.jpg", Selo = "Novo", NaHome = false, Ativo = true },
            new Produto { Nome = "X-Bacon", Preco = 18.00m, Categoria = "lanche", Descricao = "Carne artesanal\n100g de bacon\nMussarela\nAlface\nTomate\nMolho especial", ImagemUrl = "imagens/lanches/x-bacon.jpg", Selo = "", NaHome = false, Ativo = true },
            new Produto { Nome = "X-Frango", Preco = 20.00m, Categoria = "lanche", Descricao = "200g Filé de frango\nMilho\nMussarela\nAlface\nTomate\nMolho especial", ImagemUrl = "imagens/lanches/x-frango.jpg", Selo = "", NaHome = false, Ativo = true },
            new Produto { Nome = "X-Tudo", Preco = 25.00m, Categoria = "lanche", Descricao = "Carne artesanal\nMussarela\nAlface\nTomate\nMolho especial\nBatata palha", ImagemUrl = "imagens/lanches/x-tudo.jpg", Selo = "Recomendado", NaHome = false, Ativo = true },
            new Produto { Nome = "X-Tudo Duplo", Preco = 25.00m, Categoria = "lanche", Descricao = "2x Carne artesanal\nMussarela\nPresunto\nCalabresa\nBacon\n2 Ovos", ImagemUrl = "imagens/lanches/x-tudo duplo.jpg", Selo = "", NaHome = false, Ativo = true },
            new Produto { Nome = "X-Contra Filé", Preco = 30.00m, Categoria = "lanche", Descricao = "Contra filé 180g\n100g de bacon\nMussarela\nAlface\nTomate\nMolho especial", ImagemUrl = "imagens/lanches/x-contra file.jpg", Selo = "Premium", NaHome = false, Ativo = true },
            new Produto { Nome = "Coca-Cola 350mL", Preco = 6.00m, Categoria = "bebida", Descricao = "", ImagemUrl = "imagens/bebidas/coca-cola 350ml.jpg", Selo = "POPULAR", NaHome = false, Ativo = true },
            new Produto { Nome = "Coca-Cola 1L", Preco = 10.00m, Categoria = "bebida", Descricao = "", ImagemUrl = "imagens/bebidas/coca-cola 1L.jpg", Selo = "", NaHome = false, Ativo = true },
            new Produto { Nome = "Coca-Cola 2L", Preco = 15.00m, Categoria = "bebida", Descricao = "", ImagemUrl = "imagens/bebidas/coca-cola 2L.jpg", Selo = "", NaHome = false, Ativo = true },
            new Produto { Nome = "Guaraná Antarctica 350mL", Preco = 5.00m, Categoria = "bebida", Descricao = "", ImagemUrl = "imagens/bebidas/guarana 350ml.jpg", Selo = "", NaHome = false, Ativo = true },
            new Produto { Nome = "Sprite 350mL", Preco = 5.00m, Categoria = "bebida", Descricao = "", ImagemUrl = "imagens/bebidas/sprite 350ml.jpg", Selo = "", NaHome = false, Ativo = true },
            new Produto { Nome = "Fanta Laranja 350ml", Preco = 5.00m, Categoria = "bebida", Descricao = "", ImagemUrl = "imagens/bebidas/fanta laranja 350ml.jpg", Selo = "", NaHome = false, Ativo = true },
            new Produto { Nome = "Fanta Uva 350ml", Preco = 5.00m, Categoria = "bebida", Descricao = "", ImagemUrl = "imagens/bebidas/fanta uva 350ml.jpg", Selo = "", NaHome = false, Ativo = true },
            new Produto { Nome = "Suco de Laranja 500ml", Preco = 10.00m, Categoria = "bebida", Descricao = "", ImagemUrl = "imagens/bebidas/suco de laranja 500ml.jpg", Selo = "NATURAL", NaHome = false, Ativo = true },
            new Produto { Nome = "Suco de Uva 500ml", Preco = 12.00m, Categoria = "bebida", Descricao = "", ImagemUrl = "imagens/bebidas/suco de uva 500ml.jpg", Selo = "NATURAL", NaHome = false, Ativo = true },
            new Produto { Nome = "Suco de Maracujá 500ml", Preco = 15.00m, Categoria = "bebida", Descricao = "", ImagemUrl = "imagens/bebidas/suco de maracuja 500ml.jpg", Selo = "NATURAL", NaHome = false, Ativo = true },
            new Produto { Nome = "Heineken Long Neck", Preco = 10.00m, Categoria = "bebida", Descricao = "", ImagemUrl = "imagens/bebidas/heineken.jpg", Selo = "", NaHome = false, Ativo = true },
            new Produto { Nome = "Corona Long Neck", Preco = 8.00m, Categoria = "bebida", Descricao = "", ImagemUrl = "imagens/bebidas/corona.jpg", Selo = "", NaHome = false, Ativo = true },
            new Produto { Nome = "Budweiser Long Neck", Preco = 9.00m, Categoria = "bebida", Descricao = "", ImagemUrl = "imagens/bebidas/budweiser.jpg", Selo = "", NaHome = false, Ativo = true },
            new Produto { Nome = "Combo Família", Preco = 80.00m, Categoria = "combo", Descricao = "4 X-Salada\n1 Coca-Cola 2L\n1 Porção de Fritas 500g\nIdeal para compartilhar! Um combo completo com lanches, porções e bebidas para reunir a família e aproveitar uma refeição caprichada.", ImagemUrl = "imagens/combos/combo familia.jpg", Selo = "POPULAR", NaHome = false, Ativo = true },
            new Produto { Nome = "Dogão no Prato", Preco = 35.00m, Categoria = "combo", Descricao = "2 Hot Dog Aberto\n1 Coca-Cola 600ml\n1 Porção de Fritas 300g\nUm dogão completo servido no prato, acompanhado de batata frita e bebida. Uma opção reforçada para matar a fome.", ImagemUrl = "imagens/combos/combo dogao.jpg", Selo = "", NaHome = false, Ativo = true },
            new Produto { Nome = "Combo Casal", Preco = 45.00m, Categoria = "combo", Descricao = "1 X-Tudo\n1 X-Salada\n1 Coca-Cola 600ml\nPerfeito para dividir a dois! Dois lanches acompanhados de batata frita e bebidas para deixar a refeição ainda mais completa.", ImagemUrl = "imagens/combos/combo casal.jpg", Selo = "NOVO", NaHome = false, Ativo = true },
            new Produto { Nome = "Combo Kids", Preco = 35.00m, Categoria = "combo", Descricao = "1 X-Kids\n1 Coca-Cola Lata\n1 Porção de Fritas 300g\nUma opção especial para os pequenos, com lanche, acompanhamento e bebida em uma combinação simples e gostosa.", ImagemUrl = "imagens/combos/combo kids.jpg", Selo = "", NaHome = false, Ativo = true },
            new Produto { Nome = "Combo Bacon", Preco = 55.00m, Categoria = "combo", Descricao = "2 X-BACON\n1 Coca-Cola 600ml\n1 Porção de Fritas 300g\nPara quem não dispensa bacon! Um combo completo com lanche de bacon, acompanhamento e bebida para uma refeição cheia de sabor.", ImagemUrl = "imagens/combos/combo bacon.jpg", Selo = "POPULAR", NaHome = false, Ativo = true },
            new Produto { Nome = "X-Salada + Coca-Cola 350ml", Preco = 19.90m, Categoria = "combo", Descricao = "X-Salada acompanhado de Coca-Cola 350ml.", ImagemUrl = "imagens/combos/combo mais pedidos-1.jpg", Selo = "MAIS VENDIDO", NaHome = true, Ativo = true },
            new Produto { Nome = "X-Bacon + Sprite 350ml", Preco = 21.90m, Categoria = "combo", Descricao = "X-Bacon acompanhado de Sprite 350ml.", ImagemUrl = "imagens/combos/combo mais pedidos-2.jpg", Selo = "", NaHome = true, Ativo = true },
            new Produto { Nome = "X-Tudo + Guaraná Antarctica 350ml", Preco = 27.90m, Categoria = "combo", Descricao = "X-Tudo acompanhado de Guaraná Antarctica 350ml.", ImagemUrl = "imagens/combos/combo mais pedidos-3.jpg", Selo = "DESTAQUE", NaHome = true, Ativo = true }
    };
}
