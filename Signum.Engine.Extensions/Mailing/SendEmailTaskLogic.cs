﻿using Signum.Engine;
using Signum.Engine.Basics;
using Signum.Engine.DynamicQuery;
using Signum.Engine.Maps;
using Signum.Engine.Operations;
using Signum.Engine.Scheduler;
using Signum.Entities;
using Signum.Entities.Basics;
using Signum.Entities.Mailing;
using Signum.Utilities;
using Signum.Utilities.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Signum.Entities.UserQueries;
using Signum.Engine.UserQueries;
using Signum.Entities.Processes;
using Signum.Engine.Processes;
using System.Threading;

namespace Signum.Engine.Mailing
{

    public static class SendEmailTaskLogic
    {
        public static void Start(SchemaBuilder sb, DynamicQueryManager dqm)
        {
            if (sb.NotDefined(MethodInfo.GetCurrentMethod()))
            {
                sb.Include<SendEmailTaskEntity>()
                    .WithQuery(dqm, () => e => new
                    {
                        Entity = e,
                        e.Id,
                        e.Name,
                        e.EmailTemplate,
                        e.UniqueTarget,
                    });
                
                Validator.PropertyValidator((SendEmailTaskEntity er) => er.UniqueTarget).StaticPropertyValidation += (er, pi) =>
                {
                    if (er.UniqueTarget != null && er.TargetsFromUserQuery != null)
                        return ValidationMessage._0And1CanNotBeSetAtTheSameTime.NiceToString(pi.NiceName(), ReflectionTools.GetPropertyInfo(()=> er.TargetsFromUserQuery).NiceName());

                    Implementations? implementations = er.EmailTemplate == null ? null : GetImplementations(er.EmailTemplate.InDB(a => a.Query));
                    if (implementations != null && er.UniqueTarget == null && er.TargetsFromUserQuery == null)
                        return ValidationMessage._0IsNotSet.NiceToString(pi.NiceName());

                    if (er.UniqueTarget != null)
                    {
                        if (!implementations.Value.Types.Contains(er.UniqueTarget.EntityType))
                            return ValidationMessage._0ShouldBeOfType1.NiceToString(pi.NiceName(), implementations.Value.Types.CommaOr(t => t.NiceName()));
                    }

                    return null;
                };

                Validator.PropertyValidator((SendEmailTaskEntity er) => er.TargetsFromUserQuery).StaticPropertyValidation += (SendEmailTaskEntity er, PropertyInfo pi) =>
                {
                    Implementations? implementations = er.EmailTemplate == null ? null : GetImplementations(er.EmailTemplate.InDB(a => a.Query));
                    if (implementations != null && er.TargetsFromUserQuery == null && er.UniqueTarget == null)
                        return ValidationMessage._0IsNotSet.NiceToString(pi.NiceName());

                    if (er.TargetsFromUserQuery != null)
                    {
                        var uqImplementations = GetImplementations(er.TargetsFromUserQuery.InDB(a => a.Query));
                        if (!implementations.Value.Types.Intersect(uqImplementations.Value.Types).Any())
                            return ValidationMessage._0ShouldBeOfType1.NiceToString(pi.NiceName(), implementations.Value.Types.CommaOr(t => t.NiceName()));
                    }

                    return null;
                };

                new Graph<SendEmailTaskEntity>.Execute(SendEmailTaskOperation.Save)
                {
                    AllowsNew = true,
                    Lite = false,
                    Execute = (e, _) => { }
                }.Register();

                SchedulerLogic.ExecuteTask.Register((SendEmailTaskEntity er, ScheduledTaskContext ctx) =>
                {
                    if (er.UniqueTarget != null)
                    {
                        var email = er.EmailTemplate.CreateEmailMessage(er.UniqueTarget?.Retrieve()).SingleEx();
                        email.SendMailAsync();
                        return email.ToLite();
                    }
                    else
                    {
                        var qr = er.TargetsFromUserQuery.Retrieve().ToQueryRequest();
                        qr.Columns.Clear();
                        var result = DynamicQueryManager.Current.ExecuteQuery(qr);

                        var entities = result.Rows.Select(a => a.Entity).ToList();
                        if (entities.IsEmpty())
                            return null;

                        return EmailPackageLogic.SendMultipleEmailsAsync(er.EmailTemplate, entities).Execute(ProcessOperation.Execute).ToLite();
                    }
                });
            }
        }

        public static Implementations? GetImplementations(QueryEntity query)
        {
            if (query == null)
                return null;

            var queryName = query?.ToQueryName();

            if (queryName == null)
                return null;

            var entityColumn = DynamicQueryManager.Current.QueryDescription(queryName).Columns.Single(a => a.IsEntity);
            var implementations = entityColumn.Implementations.Value;

            if (implementations.IsByAll)
                throw new InvalidOperationException("ByAll implementations not supported");

            return implementations;
        }
    }
}
