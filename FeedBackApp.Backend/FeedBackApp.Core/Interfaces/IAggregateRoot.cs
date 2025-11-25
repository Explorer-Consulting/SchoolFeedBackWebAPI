using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces;

/// <summary>
/// Represents an aggregate root in a domain-driven design context.
/// </summary>
/// <remarks>An aggregate root is the entry point for accessing and modifying an aggregate, ensuring consistency
/// and encapsulation of related entities and value objects. Implement this interface to mark a domain entity as the
/// root of an aggregate, which is responsible for enforcing business invariants and transactional boundaries.</remarks>
public interface IAggregateRoot { }