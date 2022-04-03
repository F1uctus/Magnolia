namespace Magnolia.Attributes;

using System;

[AttributeUsage(
    AttributeTargets.Class
  | AttributeTargets.Struct
  | AttributeTargets.Property
  | AttributeTargets.Field
)]
public class NotTraversableAttribute : Attribute { }
