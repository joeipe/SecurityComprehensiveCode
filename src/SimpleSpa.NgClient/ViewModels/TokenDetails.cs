using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleSpa.NgClient.ViewModels
{
    public class TokenDetails
    {
        public string IdToken { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string Roles { get; set; }
    }
}
