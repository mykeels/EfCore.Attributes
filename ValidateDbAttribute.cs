using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace EfCore.Attributes
{
    public class ValidateDBAttribute: ActionFilterAttribute {
        private readonly Type _dbContextType;

        public ValidateDBAttribute(Type dbContextType) {
            _dbContextType = dbContextType;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {

            var parameters = context.ActionDescriptor.Parameters.Cast<ControllerParameterDescriptor>();

            bool isValid = true;

            if (isValid) {
                isValid = CheckShallowExists(context, parameters);
            }

            if (isValid) {
                isValid = CheckDeepExists(context, parameters);
            }

            if (isValid) {
                isValid = CheckShallowUnique(context, parameters);
            }

            if (isValid) {
                isValid = CheckDeepUnique(context, parameters);
            }

            if (!isValid) {
                context.Result = new BadRequestObjectResult(context.ModelState);
                return;
            }

            return;
        }

        private bool CheckDeepExists(ActionExecutingContext context, IEnumerable<ControllerParameterDescriptor> parameters) {
            var _context = (IdentityDbContext)context.HttpContext.RequestServices.GetService(_dbContextType);

            foreach (var p in parameters) {
                var attributes = p.ParameterInfo.GetCustomAttributes(typeof(FromBodyAttribute), false).Cast<FromBodyAttribute>();

                foreach (var attribute in attributes) {
                    object value;
                    context.ActionArguments.TryGetValue(p.Name, out value);

                    var properties = value.GetType().GetProperties()
                                                .Where(property => property.GetCustomAttribute(typeof(ExistsAttribute)) != null);
                    
                    foreach (var property in properties) {
                        var propertyValue = property.GetValue(value);

                        if (propertyValue != null) {
                            var existsAttribute = property.GetCustomAttribute<ExistsAttribute>();
                            string column = existsAttribute._columnName ?? property.Name;

                            if (existsAttribute._model != null) {
                                object entry = _context.Find(existsAttribute._model, propertyValue);

                                existsAttribute.SetValid(entry != null);
                            }
                            else if (!String.IsNullOrEmpty(existsAttribute._tableName)) {
                                string sql = @"select ? from ? where ? == ?";

                                int rows = _context.Database.ExecuteSqlCommand(sql, column, existsAttribute._tableName, column, propertyValue);

                                existsAttribute.SetValid(rows > 0);
                            }
                            
                            try {
                                existsAttribute.Validate(propertyValue, property.Name);
                            }
                            catch(Exception ex) {
                                context.ModelState.AddModelError(column, ex.Message);
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        private bool CheckShallowExists(ActionExecutingContext context, IEnumerable<ControllerParameterDescriptor> parameters) {
            var _context = (DbContext)context.HttpContext.RequestServices.GetService(_dbContextType);

            foreach (var p in parameters) {
                var attributes = p.ParameterInfo.GetCustomAttributes(typeof(ExistsAttribute), false).Cast<ExistsAttribute>();

                foreach (var attribute in attributes) {
                    object value;
                    context.ActionArguments.TryGetValue(p.Name, out value);

                    if (value != null) {
                        string column = attribute._columnName ?? p.Name;
                        if (attribute._model != null) {
                            object entry = _context.Find(attribute._model, value);

                            attribute.SetValid(entry != null);
                        }
                        else if (!String.IsNullOrEmpty(attribute._tableName)) {

                            string sql = @"select ? from ? where ? == ?";

                            int rows = _context.Database.ExecuteSqlCommand(sql, column, attribute._tableName, column, value);

                            attribute.SetValid(rows > 0);
                        }

                        try {
                            attribute.Validate(value, p.Name);
                        }
                        catch(Exception ex) {
                            context.ModelState.AddModelError(column, ex.Message);
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private bool CheckDeepUnique(ActionExecutingContext context, IEnumerable<ControllerParameterDescriptor> parameters) {
            var _context = (IdentityDbContext)context.HttpContext.RequestServices.GetService(_dbContextType);

            foreach (var p in parameters) {
                var attributes = p.ParameterInfo.GetCustomAttributes(typeof(FromBodyAttribute), false).Cast<FromBodyAttribute>();

                foreach (var attribute in attributes) {
                    object value;
                    context.ActionArguments.TryGetValue(p.Name, out value);

                    var properties = value.GetType().GetProperties()
                                                .Where(property => property.GetCustomAttribute(typeof(UniqueAttribute)) != null);
                    
                    foreach (var property in properties) {
                        var propertyValue = property.GetValue(value);

                        if (propertyValue != null) {
                            var uniqueAttribute = property.GetCustomAttribute<UniqueAttribute>();
                            string column = uniqueAttribute._columnName ?? property.Name;

                            if (uniqueAttribute._model != null) {
                                object entry = _context.Find(uniqueAttribute._model, propertyValue);

                                uniqueAttribute.SetValid(entry != null);
                            }
                            else if (!String.IsNullOrEmpty(uniqueAttribute._tableName)) {
                                string sql = @"select ? from ? where ? == ?";

                                int rows = _context.Database.ExecuteSqlCommand(sql, column, uniqueAttribute._tableName, column, propertyValue);

                                uniqueAttribute.SetValid(rows == 0);
                            }
                            
                            try {
                                uniqueAttribute.Validate(propertyValue, property.Name);
                            }
                            catch(Exception ex) {
                                context.ModelState.AddModelError(column, ex.Message);
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        private bool CheckShallowUnique(ActionExecutingContext context, IEnumerable<ControllerParameterDescriptor> parameters) {
            var _context = (DbContext)context.HttpContext.RequestServices.GetService(_dbContextType);

            foreach (var p in parameters) {
                var attributes = p.ParameterInfo.GetCustomAttributes(typeof(UniqueAttribute), false).Cast<UniqueAttribute>();

                foreach (var attribute in attributes) {
                    object value;
                    context.ActionArguments.TryGetValue(p.Name, out value);

                    if (value != null) {
                        string column = attribute._columnName ?? p.Name;

                        if (attribute._model != null) {
                            object entry = _context.Find(attribute._model, value);

                            attribute.SetValid(entry != null);
                        }
                        else if (!String.IsNullOrEmpty(attribute._tableName)) {

                            string sql = @"select ? from ? where ? == ?";

                            int rows = _context.Database.ExecuteSqlCommand(sql, column, attribute._tableName, column, value);

                            attribute.SetValid(rows > 0);
                        }

                        try {
                            attribute.Validate(value, p.Name);
                        }
                        catch(Exception ex) {
                            context.ModelState.AddModelError(column, ex.Message);
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}
