using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace SurveyTool.IdentityExtentions
{
    public class CustomPasswordValidator : IIdentityValidator<string>
    {

        public bool RequireDigit { get; set; }
        public int RequiredLength { get; set; }
        public bool RequireLowercase { get; set; }
        public bool RequireNonLetter { get; set; }
        public bool RequireUppercase { get; set; }
        //public CustomPasswordValidator(int length)
        //{
        //    RequiredLength = length;
        //}

        Task<IdentityResult> IIdentityValidator<string>.ValidateAsync(string item)
        {
            List<string> filedMessages = new List<string>();
            string[] filedMessage = { };
            if (String.IsNullOrEmpty(item) || item.Length < RequiredLength)
            {
                filedMessages.Add(String.Format("密碼長度至少 {0} ", RequiredLength));
                //return Task.FromResult(IdentityResult.Failed(String.Format("密碼長度至少 {0}", RequiredLength)));
            }

            if (RequireDigit)
            {
                string digitPattern = @"^(?=.*[0-9]).*$";
                if (!Regex.IsMatch(item, digitPattern))
                {
                    filedMessages.Add("密碼須包含 數字");
                    //return Task.FromResult(IdentityResult.Failed("密碼須包含 數字"));
                }
            }

            if (RequireUppercase)
            {
                string lowercasePattern = @"^(?=.*[A-Z]).*$";
                if (!Regex.IsMatch(item, lowercasePattern))
                {
                    filedMessages.Add("密碼須包含 大寫英文字母");
                    //return Task.FromResult(IdentityResult.Failed("密碼須包含 大寫英文字母"));
                }
            }

            if (RequireLowercase)
            {
                string lowercasePattern = @"^(?=.*[a-z]).*$";
                if (!Regex.IsMatch(item, lowercasePattern))
                {
                    filedMessages.Add("密碼須包含 小寫英文字母");
                    //return Task.FromResult(IdentityResult.Failed("密碼須包含 小寫英文字母"));
                }
            }

            if (RequireNonLetter)
            {
                string lowercasePattern = @"^(?=.*[!@#$%^&*;]).*$";
                if (!Regex.IsMatch(item, lowercasePattern))
                {
                    filedMessages.Add("密碼須包含 特殊字元(!@#$%^&*;)");
                    //return Task.FromResult(IdentityResult.Failed("密碼須包含 特殊字元(!@#$%^&*;)"));
                }
            }

            if (filedMessages.Count != 0)
            {
                return Task.FromResult(IdentityResult.Failed(filedMessages.ToArray()));
            }

            //string pattern = @"^(?=.*[0-9])(?=.*[!@#$%^&*])[0-9a-zA-Z!@#$%^&*0-9]{10,}$";

            //if (!Regex.IsMatch(item, pattern))
            //{
            //    return Task.FromResult(IdentityResult.Failed("密碼須包含 數字 與 特殊字元"));
            //}

            return Task.FromResult(IdentityResult.Success);
        }
    }
}