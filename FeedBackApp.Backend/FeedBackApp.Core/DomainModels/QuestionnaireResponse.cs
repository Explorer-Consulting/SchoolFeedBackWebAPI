using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModels;

public sealed class QuestionnaireResponse : IAggregateRoot
{
    public required string QuestionnaireResponseStorageID { get; set; } = default!; // Storage ID for Cosmos DB
    public required string QuestionnaireResponseBusinessID { get; set; } = default!; // Business ID for application
    public required string QuestionnaireTemplateBusinessID { get; set; } = default!;// Associated template ID
    public required string AssigneeID { get; set; } = default!;// ID of the user who submitted the response
    public required ICollection<string> Tags { get; set; } = default!; // Tags for categorization
    public required ICollection<QuestionResponse> QuestionResponses { get; set; } = default!; // User responses
    public required ResponseStatus Status { get; set; } = default!; // Current status of the response
}
