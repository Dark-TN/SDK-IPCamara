using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.Respuesta
{
    public class ORespuesta
    {
        public List<object> Data { get; set; }
        public String Mensaje { get; set; }
        public uint Error { get; set; }
        public bool Exitoso { get; set; }

        public ORespuesta()
        {
            this.Data = new List<object>();
            this.Mensaje = String.Empty;
            this.Error = 0;
            this.Exitoso = false;
        }
    }
}
