﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace AuraGenie.Data.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string? AzureId { get; set; }

    public virtual ICollection<AuraPointsLog> AuraPointsLogs { get; set; } = new List<AuraPointsLog>();
}