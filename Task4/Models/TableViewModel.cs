using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Task4.Models
{
    public class TableViewModel
    {
        public IEnumerable<UserDataModel> Users { get; set; }
        public SelectList SocialNetworksList { get; set; }     
        public SelectList BlockedList { get; set; }
    }
}
