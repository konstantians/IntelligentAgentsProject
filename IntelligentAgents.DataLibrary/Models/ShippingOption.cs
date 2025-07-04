﻿using System.Text.Json.Serialization;

namespace IntelligentAgents.DataLibrary.Models;
public class ShippingOption
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? ExtraCost { get; set; }
    public bool? ContainsDelivery { get; set; }
    public DateTime CreatedAt { get; set; }


    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Embedding { get; set; } // this will be omitted if null
}

