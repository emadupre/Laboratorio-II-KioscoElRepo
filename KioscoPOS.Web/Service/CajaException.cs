using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KioscoPOS.Web.Service
{
    public class CajaException : Exception
    {
        public CajaException(string mensaje) : base(mensaje) { }
    }
}