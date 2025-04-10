using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Dev1.Module.GoogleAdmin.Models
{
    public enum eFileSource
    {
        Oqtane,
        Disk
    }


    public enum eCalandarType
    {
        [Display(Name = "Organisation")]
        Organisation,
        [Display(Name = "Logged in User")]
        LoggedInUser,
        [Display(Name = "Custom")]
        Custom

    }

    public enum eDefaultFileName
    {
        [Display(Name = "Original")]
        Original,
        [Display(Name = "Append Username")]
        AppendUserName,
        [Display(Name = "Append Email")]
        AppendEmail
    }

}

