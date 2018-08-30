﻿using AuthApp.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AuthApp.Security
{
    public class RequiresPermissionAttribute : TypeFilterAttribute {


        public RequiresPermissionAttribute(params string[] permissions)
             : base(typeof(RequiresPermissionAttributeExecutor)) {
            Arguments = new[] { new PermissionAuthorizationRequirement(permissions) };
        }

        private class RequiresPermissionAttributeExecutor : Attribute, IAsyncResourceFilter {
            private readonly ILogger _logger;
            private readonly PermissionAuthorizationRequirement _requiredPermissions;
            private readonly IPermissionProvider _PermissionProvider;

            public RequiresPermissionAttributeExecutor(ILogger<RequiresPermissionAttribute> logger,
                                            PermissionAuthorizationRequirement requiredPermissions,
                                            IPermissionProvider permissionProvider) {
                _logger = logger;
                _requiredPermissions = requiredPermissions;
                _PermissionProvider = permissionProvider;
            }

            public async Task OnResourceExecutionAsync(ResourceExecutingContext context,
                                                       ResourceExecutionDelegate next) {
                var principal = new AppPrincipal(_PermissionProvider, context.HttpContext.User.Identity);
                bool isInOneOfThisRole = false;
                foreach (var item in _requiredPermissions.RequiredPermissions) {
                    if (principal.IsInRole(item)) {
                        isInOneOfThisRole = true;
                    }
                }

                if (isInOneOfThisRole == false) {
                    context.Result = new UnauthorizedResult();
                    await context.Result.ExecuteResultAsync(context);
                }
                else {
                    await next();
                }
            }
        }
    }
}
