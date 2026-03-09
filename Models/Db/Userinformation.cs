using System;
using System.Collections.Generic;

namespace EventPack.Models.Db;

public partial class Userinformation
{
    public string UserId { get; set; } = null!;

    public string? UserPhone { get; set; }

    public string? FullName { get; set; }

    public string? UserAddress { get; set; }

    public string? UserPassword { get; set; }
}
