# Hyzen Auto Clicker

Hyzen Auto Clicker é uma ferramenta leve e personalizável para automação de cliques. Desenvolvido em .NET 8 utilizando Windows Forms, este projeto oferece uma interface intuitiva e recursos como configuração de CPS (cliques por segundo), jitter (variação aleatória) e alteração dinâmica da hotkey.

## Recursos

- **Interface Simples e Intuitiva:** Configure facilmente os parâmetros de cliques e veja o status da ferramenta.
- **CPS Personalizável:** Defina a quantidade de cliques por segundo, com valores entre 1 e 25 CPS.
- **Jitter Configurável:** Introduza variações aleatórias para emular cliques humanos, com ajustes entre 0% e 50%.
- **Hotkey Customizável:** Alterne a funcionalidade de cliques com uma tecla configurável.
- **Execução em Background:** Funciona mesmo quando a janela do programa não está em foco.
- **Lógica Antideteção:** Inclui pausas aleatórias para maior naturalidade.

## Tecnologias Utilizadas

- **Linguagem:** C#
- **Framework:** .NET 8
- **Interface Gráfica:** Windows Forms

## Instalação e Uso

1. **Baixe a Versão mais Recente:**
   Acesse a página de [releases](https://github.com/rafael-rm/hyzen-auto-clicker/releases) e baixe o executável mais recente.

2. **Execute o Programa:**
   Execute o arquivo baixado para abrir o Hyzen Auto Clicker.

3. **Configurações:**

   - Ajuste o **CPS** utilizando o controle deslizante correspondente.
   - Configure o **Jitter** para adicionar variações nos cliques.
   - Clique no botão "Alterar Hotkey" para definir uma nova tecla para ativação.

4. **Inicie os Cliques:**
   Pressione a tecla configurada para ativar/desativar o auto clicker.

## Requisitos do Sistema

- Windows 10 ou superior

## Estrutura do Código

O projeto é dividido em dois componentes principais:

1. **HyzenAutoClicker.Core:** Lógica de automação, incluindo a implementação do loop de cliques, jitter e pausa aleatória.
2. **HyzenAutoClicker.WFA:** Interface gráfica do usuário, incluindo controles para ajuste de CPS, jitter e hotkey.
