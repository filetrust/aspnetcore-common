using NUnit.Framework.Constraints;

namespace Glasswall.Common.Test.NUnit
{
    public static class NunitExtensions
    {
        public static EqualConstraint WithPropEqual(this ConstraintExpression expr, string propName, object expectedVal)
        {
            return expr.With.Property(propName).EqualTo(expectedVal);
        }
    }
}