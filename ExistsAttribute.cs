using System;
using System.ComponentModel.DataAnnotations;

namespace EfCore.Attributes 
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class ExistsAttribute : ValidationAttribute
    {
        public readonly Type _model;
        private bool _valid = true;

        public ExistsAttribute(Type model)
            : base("No {0} field with this value was found in the database.")
        {
            _model = model;
        }

        public override bool IsValid(object value)
        {
            return _valid;
        }

        public ExistsAttribute SetValid(bool valid) {
            _valid = valid;
            return this;
        }
    }
}
