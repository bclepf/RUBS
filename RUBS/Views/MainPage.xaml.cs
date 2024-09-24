namespace RUBS.Views;
public partial class MainPage : ContentPage
{
    private readonly ApiService _apiService;

    public MainPage()
    {
        InitializeComponent();
        _apiService = new ApiService();
        CarregarEstabelecimentos();
    }

    private async void CarregarEstabelecimentos()
    {
        try
        {
            var estabelecimentos = await _apiService.GetEstabelecimentosAsync();

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
}