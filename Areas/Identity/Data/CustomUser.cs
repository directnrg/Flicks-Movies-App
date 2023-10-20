using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace _301153142_301137955_Soto_Ko_Lab3.Areas.Identity.Data;

// Add profile data for application users by adding properties to the CustomUser class
public class CustomUser : IdentityUser
{
    [PersonalData]
    public string FullName { get; set; }
}

