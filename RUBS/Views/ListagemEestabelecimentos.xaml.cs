namespace RUBS.Views
{
    using RUBS.Services;

    public partial class ListagemEestabelecimentos : ContentPage
    {
        private readonly ApiService _apiService;
        private readonly string _codigoMunicipio;

        public ListagemEestabelecimentos(string codigoMunicipio)
        {
            InitializeComponent();
            _apiService = new ApiService();
            _codigoMunicipio = codigoMunicipio;

            // Chame a fun��o que busca os estabelecimentos ao carregar a p�gina
            BuscarEstabelecimentos();
        }

        private async void BuscarEstabelecimentos()
        {
            if (!string.IsNullOrEmpty(_codigoMunicipio))
            {
                try
                {
                    // Realiza a busca paginada
                    await _apiService.GetEstabelecimentosPaginadosAsync(_codigoMunicipio);

                    // Carregar estabelecimentos salvos no banco e exibir na lista
                    var estabelecimentos = await _apiService._databaseService.GetEstabelecimentosAsync();

                    if (estabelecimentos != null)
                    {
                        ListaEstabelecimentos.ItemsSource = estabelecimentos;
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Erro", ex.Message, "OK");
                }
            }
            else
            {
                await DisplayAlert("Erro", "C�digo do munic�pio n�o pode ser vazio.", "OK");
            }
        }
    }
}
