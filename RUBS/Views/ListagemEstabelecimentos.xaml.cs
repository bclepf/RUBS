using RUBS.Services;
using Microsoft.Maui.Controls;

namespace RUBS.Views
{
    public partial class ListagemEstabelecimentos : ContentPage
    {
        private readonly ApiService _apiService;

        public ListagemEstabelecimentos()
        {
            InitializeComponent();
            _apiService = new ApiService();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CarregarEstabelecimentosDoBanco();
            await BuscarEstabelecimentos();
        }

        private async Task CarregarEstabelecimentosDoBanco()
        {
            // Carrega e exibe os estabelecimentos que já estão salvos no banco de dados
            var estabelecimentos = await _apiService._databaseService.GetEstabelecimentosAsync();
            if (estabelecimentos != null)
            {
                ListaEstabelecimentos.ItemsSource = estabelecimentos;
            }
        }

        private async Task BuscarEstabelecimentos()
        {
            // Obtém o código do município selecionado do serviço
            var codigoMunicipio = MunicipioService.Instance.CodigoMunicipioSelecionado;

            if (!string.IsNullOrEmpty(codigoMunicipio))
            {
                try
                {
                    // Realiza a busca paginada e salva os dados novos no banco
                    await _apiService.GetEstabelecimentosPaginadosAsync(codigoMunicipio);

                    // Atualiza a lista com os dados mais recentes
                    await CarregarEstabelecimentosDoBanco();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Erro", ex.Message, "OK");
                }
            }
            else
            {
                await DisplayAlert("Erro", "Código do município não pode ser vazio.", "OK");
            }
        }
    }
}
