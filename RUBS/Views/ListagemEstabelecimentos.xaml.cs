using RUBS.Models; 
using RUBS.Services;
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
            if (e.Item != null)
            {
                var estabelecimentoSelecionado = e.Item as Estabelecimento;

                await Navigation.PushAsync(new DetalhesEstabelecimento(estabelecimentoSelecionado));

                ListaEstabelecimentos.SelectedItem = null;
            }
        }


        private async Task BuscarEstabelecimentos()
        {
            var codigoMunicipio = MunicipioService.Instance.CodigoMunicipioSelecionado;

            if (!string.IsNullOrEmpty(codigoMunicipio))
            {
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
            }
        }
    }
}
