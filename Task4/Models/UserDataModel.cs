using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Task4.Models
{
    public class UserDataModel
    {
        public int Id { get; set; }
        public string SocialUserName { get; set; }
        public string SocialNetwork { get; set; }
        public DateTime FirstEnter { get; set; }
        public DateTime LastEnter { get; set; }
        public string State { get; set; }
        public string Email { get; set; }
    }
}
