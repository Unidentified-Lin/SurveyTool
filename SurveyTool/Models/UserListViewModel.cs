using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SurveyTool.Models
{
    public class UserListViewModel
    {
        public List<UserViewModel> Users { get; set; }
    }

    public class UserViewModel
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Account { get; set; }
        public string Role { get; set; }
    }
}