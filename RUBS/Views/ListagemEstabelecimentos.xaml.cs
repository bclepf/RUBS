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
        private async Task CarregarEstabelecimentosDoBanco()
        {
            var estabelecimentos = await _estabelecimentoService.ObterEstabelecimentosSalvosAsync();
            if (estabelecimentos != null)
            {
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
                    codigo_tipo_unidade = estabelecimentoDb.codigo_tipo_unidade,
                    descricao_natureza_juridica_estabelecimento = estabelecimentoDb.descricao_natureza_juridica_estabelecimento
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
