﻿using System.ComponentModel.DataAnnotations;

namespace CodedByKay.BondBridge.API.Models
{
    public class SigninModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
        public bool IsApp { get; set; } = false;
    }
}
