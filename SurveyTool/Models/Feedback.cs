using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SurveyTool.Models
{
    public class Feedback
    {
        [Key, ForeignKey("Response")]
        public int ResponseId { get; set; }

        public string Value { get; set; }

        public string Comment { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime ModifiedOn { get; set; }

        public Response Response { get; set; }
    }
}