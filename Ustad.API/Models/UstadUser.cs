using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ustad.API.Models
{
    public class UstadUser
    {
        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserFirstName { get; set; } = string.Empty;
        public string UserLastName { get; set; } = string.Empty;
        public string UserTcNo { get; set; } = string.Empty;
        public string UserMobileNo { get; set; } = string.Empty;
        public string UserEMail { get; set; } = string.Empty;
        public string UserFirmGUID { get; set; } = string.Empty;
        public string UserKey { get; set; } = string.Empty;
        public string UserGUID { get; set; } = string.Empty;
    }
}
