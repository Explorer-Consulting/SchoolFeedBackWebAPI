using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces;

/// <summary>
/// Represents a property that supports aggregation operations within a data model or query context.
/// </summary>
/// <remarks>Implementations of this interface typically define properties that can be used in aggregate functions
/// such as sum, average, count, or similar operations. This interface is intended for use in scenarios where properties
/// need to be identified or processed as aggregatable within collections or queries.</remarks>
public interface IAggregateProperty { }