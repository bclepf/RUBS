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
            // Defina a relação de códigos como long
            var tiposUnidade = new (long codigo, string descricao)[]
            {
        (1, "Posto de Saúde"),
        (2, "Centro de Saúde/Unidade Básica"),
        (3, "Policlínica"),
        (5, "Hospital Geral"),
        (7, "Hospital Especializado"),
        (15, "Unidade Mista"),
        (20, "Pronto Socorro Geral"),
        (21, "Pronto Socorro Especializado"),
        (22, "Consultório Isolado"),
        (23, "Clínica/Centro de Especialidade"),
        (39, "Unidade de Apoio Diagnose e Terapia (SADT Isolado)"),
        (40, "Unidade Móvel Terrestre"),
        (42, "Unidade Móvel de Nível Pré-Hospitalar na Área de Urgência"),
        (43, "Farmácia"),
        (50, "Unidade de Vigilância em Saúde"),
        (56, "Cooperativa ou Empresa de Cessão de Trabalhadores na Saúde"),
        (61, "Centro de Parto Normal - Isolado"),
        (68, "Hospital/Dia - Isolado"),
        (69, "Central de Gestão em Saúde"),
        (70, "Centro de Atenção Hemoterapia e/ou Hematológica"),
        (71, "Centro de Atenção Psicossocial"),
        (72, "Centro de Apoio à Saúde da Família"),
        (73, "Unidade de Atenção à Saúde Indígena"),
        (74, "Pronto Atendimento"),
        (75, "Polo Academia da Saúde"),
        (76, "Telessaúde"),
        (78, "Central de Regulação Médica das Urgências"),
        (80, "Serviço de Atenção Domiciliar Isolado (Home Care)"),
        (81, "Unidade de Atenção em Regime Residencial"),
        (82, "Oficina Ortopédica"),
        (83, "Laboratório de Saúde Pública"),
        (84, "Central de Regulação do Acesso"),
        (85, "Central de Notificação, Captação e Distribuição de Órgãos Estadual"),
        (86, "Polo de Prevenção de Doenças e Agravos e Promoção da Saúde"),
        (87, "Central de Abastecimento"),
        (88, "Centro de Imunização"),
            };

            // Exibe a seleção de tipos
            var tipoSelecionado = await DisplayActionSheet("Selecione o Tipo de Estabelecimento", "Cancelar", null, tiposUnidade.Select(t => t.descricao).ToArray());

            if (tipoSelecionado != null && tipoSelecionado != "Cancelar")
            {
                // Obtem o código como long
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

                // Verifica se há estabelecimentos filtrados e atualiza a mensagem
                if (estabelecimentosFiltrados.Count == 0)
                {
                    MensagemSemEstabelecimentos.Text = "Não há estabelecimentos deste tipo no município selecionado.";
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
                    MensagemSemEstabelecimentos.Text = "Não há estabelecimentos com as naturezas jurídicas permitidas.";
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
                        estabelecimento.TipoImagem = "clinica.svg"; // Imagem de clínica
                    }
                    else
                    {
                        estabelecimento.TipoImagem = "medico.svg"; // Imagem padrão
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
                await DisplayAlert("Erro", "Estabelecimento não encontrado.", "OK");
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
