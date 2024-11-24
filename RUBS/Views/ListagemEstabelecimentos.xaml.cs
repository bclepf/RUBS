using RUBS.Models;
using RUBS.Services;
using RUBS.Views;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace RUBS.Views
{
    public partial class ListagemEstabelecimentos : ContentPage
    {
        private readonly ApiService _apiService;
        private readonly EstabelecimentoService _estabelecimentoService;
        private List<EstabelecimentosDB> estabelecimentos;

        public ListagemEstabelecimentos()
        {
            InitializeComponent();
            _apiService = new ApiService();
            _estabelecimentoService = new EstabelecimentoService();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CarregarEstabelecimentosDoBanco();
            await BuscarEstabelecimentos();
        }
        private async Task TravarTela()
        {
            FundoBlur.IsVisible = true;
            Shell.SetTabBarIsVisible(this, false);
        }

        private async Task DestravarTela()
        {
            FundoBlur.IsVisible = false;
            Shell.SetTabBarIsVisible(this, true);
        }

        private async void Filtrar_Clicked(object sender, EventArgs e)
        {
            // Defina a rela��o de c�digos como long
            var tiposUnidade = new (long codigo, string descricao)[]
            {
        (1, "Posto de Sa�de"),
        (2, "Centro de Sa�de/Unidade B�sica"),
        (3, "Policl�nica"),
        (5, "Hospital Geral"),
        (7, "Hospital Especializado"),
        (15, "Unidade Mista"),
        (20, "Pronto Socorro Geral"),
        (21, "Pronto Socorro Especializado"),
        (22, "Consult�rio Isolado"),
        (23, "Cl�nica/Centro de Especialidade"),
        (39, "Unidade de Apoio Diagnose e Terapia (SADT Isolado)"),
        (40, "Unidade M�vel Terrestre"),
        (42, "Unidade M�vel de N�vel Pr�-Hospitalar na �rea de Urg�ncia"),
        (43, "Farm�cia"),
        (50, "Unidade de Vigil�ncia em Sa�de"),
        (56, "Cooperativa ou Empresa de Cess�o de Trabalhadores na Sa�de"),
        (61, "Centro de Parto Normal - Isolado"),
        (68, "Hospital/Dia - Isolado"),
        (69, "Central de Gest�o em Sa�de"),
        (70, "Centro de Aten��o Hemoterapia e/ou Hematol�gica"),
        (71, "Centro de Aten��o Psicossocial"),
        (72, "Centro de Apoio � Sa�de da Fam�lia"),
        (73, "Unidade de Aten��o � Sa�de Ind�gena"),
        (74, "Pronto Atendimento"),
        (75, "Polo Academia da Sa�de"),
        (76, "Telessa�de"),
        (78, "Central de Regula��o M�dica das Urg�ncias"),
        (80, "Servi�o de Aten��o Domiciliar Isolado (Home Care)"),
        (81, "Unidade de Aten��o em Regime Residencial"),
        (82, "Oficina Ortop�dica"),
        (83, "Laborat�rio de Sa�de P�blica"),
        (84, "Central de Regula��o do Acesso"),
        (85, "Central de Notifica��o, Capta��o e Distribui��o de �rg�os Estadual"),
        (86, "Polo de Preven��o de Doen�as e Agravos e Promo��o da Sa�de"),
        (87, "Central de Abastecimento"),
        (88, "Centro de Imuniza��o"),
            };

            // Exibe a sele��o de tipos
            var tipoSelecionado = await DisplayActionSheet("Selecione o Tipo de Estabelecimento", "Cancelar", null, tiposUnidade.Select(t => t.descricao).ToArray());

            if (tipoSelecionado != null && tipoSelecionado != "Cancelar")
            {
                // Obtem o c�digo como long
                var codigoSelecionado = tiposUnidade.First(t => t.descricao == tipoSelecionado).codigo;
                TipoSelecionadoLabel.Text = tipoSelecionado;
                FiltrarEstabelecimentos(codigoSelecionado); // Passando como long
            }
        }

        private void FiltrarEstabelecimentos(long codigoTipoUnidade)
        {
            if (estabelecimentos != null)
            {
                var estabelecimentosFiltrados = estabelecimentos
                    .Where(e => e.codigo_tipo_unidade == codigoTipoUnidade)
                    .ToList();

                // Atualiza a lista exibida
                ListaEstabelecimentos.ItemsSource = estabelecimentosFiltrados;

                // Verifica se h� estabelecimentos filtrados e atualiza a mensagem
                if (estabelecimentosFiltrados.Count == 0)
                {
                    MensagemSemEstabelecimentos.Text = "N�o h� estabelecimentos deste tipo no munic�pio selecionado.";
                    MensagemSemEstabelecimentos.IsVisible = true;
                }
                else
                {
                    MensagemSemEstabelecimentos.IsVisible = false; // Oculta a mensagem
                }
            }
            else
            {
                ListaEstabelecimentos.ItemsSource = new List<EstabelecimentosDB>(); // Limpa a lista
            }
        }

        private void RemoverFiltros()
        {
            // Redefine a lista para mostrar todos os estabelecimentos
            ListaEstabelecimentos.ItemsSource = estabelecimentos;
            // Limpa o texto da etiqueta de filtro, se existir
            TipoSelecionadoLabel.Text = string.Empty;
        }

        private void RemoverFiltros_Clicked(object sender, EventArgs e)
        {
            RemoverFiltros();
            MensagemSemEstabelecimentos.IsVisible=false;
            TipoSelecionadoLabel.Text = "Nenhum filtro";
            TipoSelecionadoLabel.IsVisible = true;
        }

        private readonly List<string> naturezasPermitidas = new List<string>
{
            "1015", "1023", "1031", "1040", "1058", "1066", "1074", "1082",
            "1104", "1112", "1120", "1139", "1147", "1155", "1163", "1171",
            "1180", "1198", "1210", "1228", "1236", "1244", "1252", "1260",
            "1279", "1287", "1295", "1309", "1317", "1325", "1333", "1341"
         };

        private void OnApenasSusClicked(object sender, EventArgs e)
        {
            if (estabelecimentos != null)
            {
                var estabelecimentosFiltrados = estabelecimentos
                    .Where(e => naturezasPermitidas.Contains(e.descricao_natureza_juridica_estabelecimento))
                    .ToList();

                ListaEstabelecimentos.ItemsSource = estabelecimentosFiltrados;

                if (estabelecimentosFiltrados.Count == 0)
                {
                    MensagemSemEstabelecimentos.Text = "N�o h� estabelecimentos com as naturezas jur�dicas permitidas.";
                    MensagemSemEstabelecimentos.IsVisible = true;
                }
                else
                {
                    MensagemSemEstabelecimentos.IsVisible = false;
                }
                TipoSelecionadoLabel.Text = "Apenas estabelecimentos SUS";


            }
            else
            {
                ListaEstabelecimentos.ItemsSource = new List<EstabelecimentosDB>();
            }
        }

        private async Task CarregarEstabelecimentosDoBanco()
        {
            estabelecimentos = await _estabelecimentoService.ObterEstabelecimentosSalvosAsync(); // Inicializa a lista de estabelecimentos

            if (estabelecimentos != null)
            {
                // Atualiza a imagem do pin com base no tipo de unidade
                foreach (var estabelecimento in estabelecimentos)
                {
                    if (estabelecimento.codigo_tipo_unidade == 5 || estabelecimento.codigo_tipo_unidade == 7 || estabelecimento.codigo_tipo_unidade == 62 || 
                        estabelecimento.codigo_tipo_unidade == 20 || estabelecimento.codigo_tipo_unidade == 21 || estabelecimento.codigo_tipo_unidade == 73)
                    {
                        estabelecimento.TipoImagem = "hospital.svg"; // Imagem de hospital
                    }
                    else if (estabelecimento.codigo_tipo_unidade == 1 || estabelecimento.codigo_tipo_unidade == 2 || estabelecimento.codigo_tipo_unidade == 4 || estabelecimento.codigo_tipo_unidade == 15 ||
                             estabelecimento.codigo_tipo_unidade == 43 || estabelecimento.codigo_tipo_unidade == 73 || estabelecimento.codigo_tipo_unidade == 80 ||
                             estabelecimento.codigo_tipo_unidade == 85)
                    {
                        estabelecimento.TipoImagem = "clinica.svg"; // Imagem de cl�nica
                    }
                    else
                    {
                        estabelecimento.TipoImagem = "medico.svg"; // Imagem padr�o
                    }
                }

                ListaEstabelecimentos.ItemsSource = estabelecimentos;
            }
        }

        private async void ListaEstabelecimentos_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is EstabelecimentosDB estabelecimentoDb)
            {
                var estabelecimentoSelecionado = new Estabelecimento
                {
                    codigo_cnes = estabelecimentoDb.codigo_cnes,
                    nome_fantasia = estabelecimentoDb.nome_fantasia,
                    endereco_estabelecimento = estabelecimentoDb.endereco_estabelecimento,
                    numero_estabelecimento = estabelecimentoDb.numero_estabelecimento,
                    bairro_estabelecimento = estabelecimentoDb.bairro_estabelecimento,
                    descricao_turno_atendimento = estabelecimentoDb.descricao_turno_atendimento,
                    numero_telefone_estabelecimento = estabelecimentoDb.numero_telefone_estabelecimento,
                };

                await Navigation.PushAsync(new DetalhesEstabelecimento(estabelecimentoSelecionado));
            }
            else
            {
                await DisplayAlert("Erro", "Estabelecimento n�o encontrado.", "OK");
            }

            ListaEstabelecimentos.SelectedItem = null;
        }




        private async Task BuscarEstabelecimentos()
        {
            var codigoMunicipio = MunicipioService.Instance.CodigoMunicipioSelecionado;

            if (!string.IsNullOrEmpty(codigoMunicipio))
            {
                await TravarTela();
                try
                {
                    // Executa a busca paginada e salva os dados novos no banco de dados
                    await _apiService.GetEstabelecimentosPaginadosAsync(codigoMunicipio);

                    // Atualiza a lista com os dados mais recentes do banco de dados
                    await CarregarEstabelecimentosDoBanco();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Erro", ex.Message, "OK");
                }
                await DestravarTela();
            }
        }
    }
}
