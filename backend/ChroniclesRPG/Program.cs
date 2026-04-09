using ChroniclesRPG.Entidades;

Console.WriteLine("--- Chronicles RPG: Teste de Sistema ---");

// 1. Criando Classe e Personagem
IClasseRPG guerreiro = new Guerreiro();
FichaPersonagem heroi = new FichaPersonagem("Arthur", guerreiro);

Console.WriteLine($"Personagem criado: {heroi.Nome}");
Console.WriteLine($"Classe: {heroi.Classe.NomeDaClasse}");
Console.WriteLine($"HP Atual / Máximo: {heroi.HpAtual} / {heroi.HpMaximo}");
Console.WriteLine($"Força: {heroi.Forca} (Modificador: {heroi.ModificadorForca})");
Console.WriteLine($"Constituição: {heroi.Constituicao} (Modificador: {heroi.ModificadorConstituicao})");

// 2. Criando Itens
Armadura cotaDeMalha = new Armadura("Cota de Malha", "Armadura pesada de anéis entrelaçados.", 50, 6);
Arma espadaLonga = new Arma("Espada Longa", "Uma espada de lâmina reta afiada.", 15, "1d8");
Consumivel pocaoCura = new Consumivel("Poção de Cura", "Um líquido vermelho borbulhante.", 10, 5);

Console.WriteLine("\n--- Adicionando Itens e Equipando ---");
heroi.Inventario.Add(cotaDeMalha);
heroi.Inventario.Add(espadaLonga);
heroi.Inventario.Add(pocaoCura);
heroi.ArmaduraVestida = cotaDeMalha;
heroi.ArmaEquipada = espadaLonga;
heroi.ClasseArmadura = 10 + heroi.ModificadorDestreza + heroi.ArmaduraVestida.BonusDefesa;

Console.WriteLine($"Armadura equipada: {heroi.ArmaduraVestida.Nome} (CA Bônus: +{heroi.ArmaduraVestida.BonusDefesa})");
Console.WriteLine($"Classe de Armadura total: {heroi.ClasseArmadura}");
Console.WriteLine($"Arma equipada: {heroi.ArmaEquipada.Nome} (Dano: {heroi.ArmaEquipada.DadoDeDano})");

// 3. Testando Consumível (Simulando Dano)
Console.WriteLine("\n--- Testando Cura ---");
Console.WriteLine("O herói tomou 8 de dano!");
heroi.HpAtual -= 8;
Console.WriteLine($"HP Atual: {heroi.HpAtual} / {heroi.HpMaximo}");

Console.WriteLine("Bebeu poção...");
pocaoCura.Usar(heroi);
Console.WriteLine($"HP Atual: {heroi.HpAtual} / {heroi.HpMaximo}");

Console.WriteLine("\n> Teste finalizado com sucesso!");
