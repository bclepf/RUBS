using Microsoft.Maui.Controls;
using RUBS.Views;
using RUBS.Services;

namespace RUBS.Views
{
    public partial class DetalhesEstabelecimento : ContentPage
    {
        public DetalhesEstabelecimento(Estabelecimento estabelecimento)
        {
            InitializeComponent();
            BindingContext = estabelecimento;

            if (estabelecimento != null)
            {
                System.Diagnostics.Debug.WriteLine($"Nome: {estabelecimento.nome_fantasia}");
            }
        }

    }
}
