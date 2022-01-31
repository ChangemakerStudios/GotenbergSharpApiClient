using System.Reflection;

namespace Gotenberg.Sharp.API.Client.Infrastructure;

public class MultiFormPropertyItem
{
    public PropertyInfo Property { get; set; }
    public MultiFormHeaderAttribute Attribute { get; set; }
}