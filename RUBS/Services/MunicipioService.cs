using System;

namespace RUBS.Services
{
    public class MunicipioService
    {
        private static MunicipioService _instance;
        public static MunicipioService Instance => _instance ??= new MunicipioService();

        private string _codigoMunicipioSelecionado;

        public string CodigoMunicipioSelecionado
        {
            get => _codigoMunicipioSelecionado;
            set
            {
                if (_codigoMunicipioSelecionado != value)
                {
                    _codigoMunicipioSelecionado = value;
                    OnMunicipioSelecionadoChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        // Evento que pode ser utilizado para notificar quando o município selecionado muda
        public event EventHandler OnMunicipioSelecionadoChanged;

        // Construtor privado para evitar instância externa
        private MunicipioService() { }
    }
}