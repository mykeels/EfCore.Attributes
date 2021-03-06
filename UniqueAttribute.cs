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

        /// <summary>
        /// Only For Primary Keys
        /// </summary>
        /// <param name="model"></param>
        public UniqueAttribute(Type model)
            : base("A {0} field with this value already exists in the database.")
        {
            _model = model;
        }

        /// <summary>
        /// Specify table and column name
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName">If null, the name of the field were this attribute is applied to, is used</param>
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
