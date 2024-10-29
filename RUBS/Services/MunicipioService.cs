using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUBS.Services
{
    internal class MunicipioService
    {
        private static MunicipioService _instance;
        public static MunicipioService Instance => _instance ??= new MunicipioService();

        public string CodigoMunicipioSelecionado { get; set; }
    }
}
