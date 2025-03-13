﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace AuraGenie.Data.Models;

public partial class Message
{
    public int Id { get; set; }

    public string MessageContent { get; set; } = null!;

    public double CreatedOn { get; set; }

    public string SenderId { get; set; } = null!;

    public int RoomId { get; set; }

    public int? ReplyMessageId { get; set; }

    public virtual ICollection<AuraPointsLog> AuraPointsLogs { get; set; } = new List<AuraPointsLog>();

    public virtual ICollection<Message> InverseReplyMessage { get; set; } = new List<Message>();

    public virtual Message? ReplyMessage { get; set; }
}