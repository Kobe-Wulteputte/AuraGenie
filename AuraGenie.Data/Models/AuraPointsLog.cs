﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace AuraGenie.Data.Models;

public partial class AuraPointsLog
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int Points { get; set; }

    public double CreatedOn { get; set; }

    public int? SourceMessageId { get; set; }

    public virtual Message? SourceMessage { get; set; }

    public virtual User User { get; set; } = null!;
}