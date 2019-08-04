using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

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

            CheckShallowExists(context, parameters);

            CheckDeepExists(context, parameters);

            if (!context.ModelState.IsValid) {
                context.Result = new BadRequestObjectResult(context.ModelState);
                return;
            }

            return;
        }

        private void CheckDeepExists(ActionExecutingContext context, IEnumerable<ControllerParameterDescriptor> parameters) {
            var _context = (DbContext)context.HttpContext.RequestServices.GetService(_dbContextType);

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

                            object entry = _context.Find(existsAttribute._model, propertyValue);
                            existsAttribute.SetValid(entry != null);
                            try {
                                existsAttribute.Validate(propertyValue, property.Name);
                            }
                            catch(Exception ex) {
                                context.ModelState.AddModelError(property.Name, ex.Message);
                            }
                        }
                    }
                }
            }
        }

        private void CheckShallowExists(ActionExecutingContext context, IEnumerable<ControllerParameterDescriptor> parameters) {
            var _context = (DbContext)context.HttpContext.RequestServices.GetService(_dbContextType);

            foreach (var p in parameters) {
                var attributes = p.ParameterInfo.GetCustomAttributes(typeof(ExistsAttribute), false).Cast<ExistsAttribute>();

                foreach (var attribute in attributes) {
                    object value;
                    context.ActionArguments.TryGetValue(p.Name, out value);

                    if (value != null) {
                        object entry = _context.Find(attribute._model, value);
                        attribute.SetValid(entry != null);
                        try {
                            attribute.Validate(value, p.Name);
                        }
                        catch(Exception ex) {
                            context.ModelState.AddModelError(p.Name, ex.Message);
                        }
                    }
                }
            }
        }
    }
}
