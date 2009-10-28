﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Signum.Engine.Maps;
using Signum.Entities.Authorization;
using Signum.Entities.Basics;
using Signum.Engine.DynamicQuery;
using Signum.Engine.Basics;
using Signum.Utilities;
using Signum.Utilities.DataStructures;
using System.Threading;
using Signum.Entities;
using System.Reflection;

namespace Signum.Engine.Authorization
{

    public static class QueryAuthLogic
    {
        static Dictionary<RoleDN, Dictionary<string, bool>> _runtimeRules;
        static Dictionary<RoleDN, Dictionary<string, bool>> RuntimeRules
        {
            get { return Sync.Initialize(ref _runtimeRules, () => NewCache()); }
        }

        public static void Start(SchemaBuilder sb, params DynamicQueryManager[] queryManagers)
        {
            if (sb.NotDefined(MethodInfo.GetCurrentMethod()))
            {
                AuthLogic.AssertIsStarted(sb);
                QueryLogic.Start(sb, queryManagers);
               
                sb.Include<RuleQueryDN>();
                sb.Schema.Initializing(InitLevel.Level1SimpleEntities, Schema_Initializing);
                sb.Schema.EntityEvents<RuleQueryDN>().Saved += Rule_Saved;
                AuthLogic.RolesModified += UserAndRoleLogic_RolesModified;
            }
        }

        static void Schema_Initializing(Schema sender)
        {
            _runtimeRules = NewCache();
        }

        static void Rule_Saved(RuleQueryDN rule)
        {
            Transaction.RealCommit += () => _runtimeRules = null;
        }

        static void UserAndRoleLogic_RolesModified()
        {
            Transaction.RealCommit += () => _runtimeRules = null;
        }

        public static HashSet<object> AuthorizedQueryNames(DynamicQueryManager dqm)
        {
            RoleDN role = RoleDN.Current;

            return dqm.GetQueryNames().Where(q => GetAllowed(role, q.ToString())).ToHashSet();
        }

        public static void AuthorizeQuery(object queryName)
        {
            if (!GetAllowed(RoleDN.Current, queryName.ToString()))
                throw new UnauthorizedAccessException("Access to Query '{0}' is not allowed".Formato(queryName));
        }

        static bool GetAllowed(RoleDN role, string queryName)
        {
            return RuntimeRules.TryGetC(role).TryGetS(queryName) ?? true;
        }

        static bool GetBaseAllowed(RoleDN role, string queryName)
        {
            return role.Roles.Count == 0 ? true :
                  role.Roles.Select(r => GetAllowed(r, queryName)).MaxAllowed();
        }

        public static bool GetQueryAllowed(object queryName)
        {
            if (!AuthLogic.IsEnabled)
                return true;

            return GetAllowed(RoleDN.Current, queryName.ToString());
        }

        public static List<AllowedRule> GetAllowedRule(Lite<RoleDN> roleLite)
        {
            var role = roleLite.Retrieve();

            var queries = QueryLogic.RetrieveOrGenerateQueries();
            return queries.Select(q => new AllowedRule(GetBaseAllowed(role, q.Name))
                   {
                       Resource = q,
                       Allowed = GetAllowed(role, q.Name),
                   }).ToList();    
        }

        public static void SetAllowedRule(List<AllowedRule> rules, Lite<RoleDN> roleLite)
        {
            var role = roleLite.Retrieve();

            var current = Database.Query<RuleQueryDN>().Where(r => r.Role == role).ToDictionary(a => a.Query);
            var should = rules.Where(a => a.Overriden).ToDictionary(r => (QueryDN)r.Resource);

            Synchronizer.Syncronize(current, should,
                (q,qr)=>qr.Delete(),
                (q,ar)=>new RuleQueryDN{ Query = q, Allowed = ar.Allowed, Role = role}.Save(),
                (q, qr, ar) => { qr.Allowed = ar.Allowed; qr.Save(); });

            _runtimeRules = null; 
        }

        public static Dictionary<RoleDN, Dictionary<string, bool>> NewCache()
        {
            using (AuthLogic.Disable())
            using (new EntityCache(true))
            {
                List<RoleDN> roles = AuthLogic.RolesInOrder().ToList();

                Dictionary<RoleDN, Dictionary<string, bool>> realRules = Database.RetrieveAll<RuleQueryDN>()
                    .AgGroupToDictionary(ru => ru.Role, gr => gr.ToDictionary(a => a.Query.Name, a => a.Allowed));

                Dictionary<RoleDN, Dictionary<string, bool>> newRules = new Dictionary<RoleDN, Dictionary<string, bool>>();
                foreach (var role in roles)
                {
                    var permissions = (role.Roles.Count == 0 ?
                         null :
                         role.Roles.Select(r => newRules.TryGetC(r)).OuterCollapseDictionariesS(vals => vals.MaxAllowed()));

                    permissions = permissions.Override(realRules.TryGetC(role)).Simplify(a => a);

                    if (permissions != null)
                        newRules.Add(role, permissions);
                }

                return newRules;
            }
        }
    }
}
