using System;
using System.ComponentModel.DataAnnotations;

namespace EfCore.Attributes 
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class UniqueAttribute : ValidationAttribute
    {
        public readonly Type _model;
        public readonly string _tableName;
        public readonly string _columnName;
        private bool _valid = true;

        public UniqueAttribute(Type model)
            : base("A {0} field with this value already exists in the database.")
        {
            _model = model;
        }

        public UniqueAttribute(string tableName, string columnName = null)
            : base("A {0} field with this value already exists in the database.")
        {
            _tableName = tableName;
            _columnName = columnName;
        }

        public override bool IsValid(object value)
        {
            return _valid;
        }

        public UniqueAttribute SetValid(bool valid) {
            _valid = valid;
            return this;
        }
    }
}
