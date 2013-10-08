﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Reflection;
using Signum.Entities;
using Signum.Engine.DynamicQuery;
using Signum.Engine.Operations;
using Signum.Entities.Reflection;
using Signum.Utilities.DataStructures;
using Signum.Utilities;
using System.Globalization;
using Signum.Engine.Maps;
using System.Linq.Expressions;
using Signum.Engine.Linq;
using System.IO;
using System.Xml;
using System.Resources;
using Signum.Utilities.Reflection;
using System.Diagnostics;
using Signum.Entities.DynamicQuery;
using Signum.Engine.Basics;
using Signum.Entities.Basics;
using System.Text.RegularExpressions;


namespace Signum.Engine.Help
{
    public static class HelpLogic
    {
        public static string EntitiesDirectory = "Entity";
        public static string QueriesDirectory = "Query";
        public static string NamespacesDirectory = "Namespace";
        public static string AppendicesDirectory = "Appendix";
        public static string HelpDirectory = "HelpXml";
        public static string BaseUrl = "Help";

        public class HelpState
        {
            public Dictionary<Type, EntityHelp> Types;

            public Dictionary<string, NamespaceHelp> Namespaces;
            public Dictionary<string, AppendixHelp> Appendices;

            public Dictionary<Type, List<object>> TypeToQuery;
            public Dictionary<object, QueryHelp> Queries;

            public List<QueryHelp> GetQueryHelps(Type type)
            {
                var list = TypeToQuery.TryGetC(type);

                if(list == null)
                    return new List<QueryHelp>();

                return list.Select(o => Queries[o]).ToList();
            }
        }

        public static Lazy<HelpState> State = new Lazy<HelpState>(Schema_Initialize, System.Threading.LazyThreadSafetyMode.PublicationOnly);

      
        public static NamespaceHelp GetNamespace(string @namespace)
        {
            return State.Value.Namespaces.TryGetC(@namespace);
        }

        public static List<NamespaceHelp> GetNamespaces()
        {
            return State.Value.Namespaces.Select(kvp => kvp.Value).ToList();
        }

        public static List<AppendixHelp> GetAppendices()
        {
            return State.Value.Appendices.Select(kvp => kvp.Value).ToList();
        }

        public static AppendixHelp GetAppendix(string appendix)
        {
            return State.Value.Appendices.TryGetC(appendix);
        }

        public static Type[] AllTypes()
        {
            return State.Value.Types.Keys.ToArray();
        }

        public static string EntityUrl(Type entityType)
        {
            return BaseUrl + "/" + TypeLogic.GetCleanName(entityType);
        }

        public static string OperationUrl(Type entityType, Enum operation)
        {
            return HelpLogic.EntityUrl(entityType) + "#" + "o-" + OperationDN.UniqueKey(operation).Replace('.', '_');
        }

        public static string PropertyUrl(PropertyRoute route)
        {
            return HelpLogic.EntityUrl(route.RootType) + "#" + "p-" + route.PropertyString();
        }

        public static string QueryUrl(Type entityType)
        {
            return HelpLogic.EntityUrl(entityType) + "#" + "q-" + entityType.FullName.Replace(".", "_");
        }
        
        public static string QueryUrl(Enum query)
        {
            return HelpLogic.EntityUrl(GetQueryType(query))
                + "#" + "q-" + QueryUtils.GetQueryUniqueKey(query).ToString().Replace(".", "_");
        }

        public static EntityHelp GetEntityHelp(Type entityType)
        {
            return State.Value.Types[entityType];
        }

        public static List<KeyValuePair<Type, EntityHelp>> GetEntitiesHelp()
        {
            return State.Value.Types.ToList();
        }

        public static QueryHelp GetQueryHelp(string query)
        {
            return State.Value.Queries[QueryLogic.TryToQueryName(query)];
        }

        public static void Start(SchemaBuilder sb)
        {
            if (sb.NotDefined(MethodInfo.GetCurrentMethod()))
            {
            }
        }

        public static void ReloadDocumentEntity(EntityHelp entityHelp)
        {
            State.Value.Types[entityHelp.Type] = EntityHelp.Create(entityHelp.Type).Load();
        }

        public static void ReloadDocumentQuery(QueryHelp queryHelp)
        {
            State.Value.Queries[queryHelp.Key] = QueryHelp.Create(queryHelp.Key).Load();
        }

        public static void ReloadDocumentNamespace(NamespaceHelp namespaceHelp)
        {
            State.Value.Namespaces[namespaceHelp.Name] = NamespaceHelp.Create(namespaceHelp.Name).Load();
        }

        public static void ReloadDocumentAppendix(AppendixHelp appendixHelp)
        {
            State.Value.Appendices[appendixHelp.Name] = AppendixHelp.Load(XDocument.Load(appendixHelp.FileName), appendixHelp.FileName);
        }

        static HelpState Schema_Initialize()
        {
            if (!Directory.Exists(HelpDirectory))
                throw new InvalidOperationException("Help directory does not exist ('{0}')".Formato(HelpDirectory));

            HelpState result = new HelpState();

            Type[] types = Schema.Current.Tables.Keys.Where(t => !t.IsEnumEntity()).ToArray();

            result.Types = types.Select(t => EntityHelp.Create(t).Load()).ToDictionary(a => a.Type);

            var dqm = DynamicQueryManager.Current;
            result.TypeToQuery = types.ToDictionary(t => t, t => dqm.GetTypeQueries(t).Keys.ToList());
            result.Queries = result.TypeToQuery.SelectMany(kvp => kvp.Value).Distinct().Select(qn => QueryHelp.Create(qn).Load()).ToDictionary(a => a.Key);

            result.Namespaces = types.Select(t => t.Namespace).Distinct().Select(ns => NamespaceHelp.Create(ns).Load()).ToDictionary(a => a.Name);

            result.Appendices = FileNames(AppendicesDirectory).Select(fn => AppendixHelp.Load(LoadAndValidate(fn), fn)).ToDictionary(a => a.Name);
         
            return result;
        }

        public static Type GetQueryType(object query)
        {
            return DynamicQueryManager.Current.GetQuery(query).Core.Value.EntityColumn().Implementations.Value.Types.FirstEx();
        }

        static Lazy<XmlSchemaSet> Schemas = new Lazy<XmlSchemaSet>(() =>
        {
            XmlSchemaSet schemas = new XmlSchemaSet();
            Stream str = typeof(HelpLogic).Assembly.GetManifestResourceStream("Signum.Engine.Extensions.Help.SignumFrameworkHelp.xsd");
            schemas.Add("", XmlReader.Create(str));
            return schemas;
        });

        static List<string> FileNames(string subdirectory)
        {
            return Directory.GetFiles(Path.Combine(HelpDirectory, subdirectory), "*.help").ToList();
        }

        internal static XDocument LoadAndValidate(string fileName)
        {
            var document = XDocument.Load(fileName); 

            List<Tuple<XmlSchemaException, string>> exceptions = new List<Tuple<XmlSchemaException, string>>();

            document.Document.Validate(Schemas.Value, (s, e) => exceptions.Add(Tuple.Create(e.Exception, fileName)));

            if (exceptions.Any())
                throw new InvalidOperationException("Error Parsing XML Help Files: " + exceptions.ToString(e => "{0} ({1}:{2}): {3}".Formato(
                 e.Item2, e.Item1.LineNumber, e.Item1.LinePosition, e.Item1.Message), "\r\n").Indent(3));

            return document;
        }

        public static void SyncronizeAll()
        {
            if (!Directory.Exists(HelpDirectory))
            {
                Directory.CreateDirectory(HelpDirectory);
            }

            Type[] types = Schema.Current.Tables.Keys.Where(t => !t.IsEnumEntity()).ToArray();

            Replacements r = new Replacements();

            StringDistance sd = new StringDistance();

            var namespaces = types.Select(type => type.Namespace).ToHashSet();

            //Namespaces
            {
                var namespacesDocuments = FileNames(NamespacesDirectory)
                    .Select(fn => new { FileName = fn, XDocument = LoadAndValidate(fn) })
                    .ToDictionary(p => NamespaceHelp.GetNamespaceName(p.XDocument, p.FileName), "Namespaces in HelpFiles");


                HelpTools.SynchronizeReplacing(r, "Namespace", namespacesDocuments, namespaces.ToDictionary(a => a),
                 (nameSpace, pair) =>
                 {
                     File.Delete(pair.FileName);
                     Console.WriteLine("Deleted {0}".Formato(pair.FileName));
                 },
                 (nameSpace, _) =>
                 {
                 },
                 (nameSpace, pair, _) =>
                 {
                     NamespaceHelp.Synchronize(pair.FileName, pair.XDocument, nameSpace, s => SyncronizeContent(s, r, sd, namespaces));
                 });
            }

            //Types
            {
                var should = types.ToDictionary(type => type.FullName);

                var current = FileNames(EntitiesDirectory)
                    .Select(fn => new { FileName = fn, XDocument = LoadAndValidate(fn) })
                    .ToDictionary(a => EntityHelp.GetEntityFullName(a.XDocument, a.FileName), "Types in HelpFiles");


                HelpTools.SynchronizeReplacing(r, "Type", current, should,
                    (fullName, pair) =>
                    {
                        File.Delete(pair.FileName);
                        Console.WriteLine("Deleted {0}".Formato(pair.FileName));
                    },
                    (fullName, type) =>
                    {
                    },
                    (fullName, pair, type) =>
                    {
                        EntityHelp.Synchronize(pair.FileName, pair.XDocument, type, s => SyncronizeContent(s, r, sd, namespaces));
                    });
            }

            //Queries
            {
                var should = (from type in types
                              from key in DynamicQueryManager.Current.GetTypeQueries(type).Keys
                              select key).Distinct().ToDictionary(q => QueryUtils.GetQueryUniqueKey(q));

                var current = FileNames(QueriesDirectory)
                    .Select(fn => new { FileName = fn, XDocument = LoadAndValidate(fn) })
                    .ToDictionary(p => QueryHelp.GetQueryFullName(p.XDocument, p.FileName), "Queries in HelpFiles");

                HelpTools.SynchronizeReplacing(r, "Query", current, should,
                    (fullName, pair) =>
                    {
                        File.Delete(pair.FileName);
                        Console.WriteLine("Deleted {0}".Formato(pair.FileName));
                    },
                    (fullName, query) => { },
                    (fullName, oldFile, query) =>
                    {
                        QueryHelp.Synchronize(oldFile.FileName, oldFile.XDocument, query, s => SyncronizeContent(s, r, sd, namespaces));
                    });
            }
        }

        public static readonly Regex HelpLinkRegex = new Regex(@"^(?<letter>[^:]+):(?<link>[^\|]*)(\|(?<text>.*))?$");

        static string SyncronizeContent(string content, Replacements r, StringDistance sd, HashSet<string> namespaces)
        {
            return WikiMarkup.WikiParserExtensions.TokenRegex.Replace(content, m =>
            {
                var m2 = HelpLinkRegex.Match(m.Groups["content"].Value);

                if (!m2.Success)
                    return m.Value;

                string letter = m2.Groups["letter"].Value;
                string link = m2.Groups["link"].Value;
                string text = m2.Groups["text"].Value;

                switch (letter)
                {
                    case WikiFormat.EntityLink:
                        {
                            string type = ParseReplaceType(r, sd, link);

                            if (type == null)
                                return m.Value;

                            return Link(letter, type, text);
                        }
                    case WikiFormat.PropertyLink:
                        {
                            string type = ParseReplaceType(r, sd, link.Before("."));

                            if (type == null)
                                return m.Value;

                            string pr = ParseReplacePropertyRoute(r, sd, TypeLogic.GetType(type), link.After('.'));

                            if (pr == null)
                                return m.Value;

                            return Link(letter, type + "." + pr, text);
                        }
                    case WikiFormat.QueryLink:
                        {
                            string query = ParseReplaceQuery(r, sd, link);

                            if (query == null)
                                return m.Value;

                            return Link(letter, query, text);
                        }
                    case WikiFormat.OperationLink:
                        {
                            string operation = ParseReplaceOperation(r, sd, link);

                            if (operation == null)
                                return m.Value;

                            return Link(letter, operation, text);
                        }
                    case WikiFormat.Hyperlink: return m.Value;
                    case WikiFormat.NamespaceLink:
                        {
                            string @namespace = ParseReplaceNamespace(r, sd, namespaces, link);

                            if (@namespace == null)
                                return m.Value;

                            return Link(letter, @namespace, text);
                        }
                    default:
                        break;
                }

                return m.Value;
            });
        }

        static string ParseReplaceNamespace(Replacements r, StringDistance sd, HashSet<string> namespaces, string @namespace)
        {
            if (namespaces.Contains(@namespace) != null)
                return @namespace;

            @namespace = r.Apply("Namespace", @namespace);

            if (namespaces.Contains(@namespace) != null)
                return @namespace;

            @namespace = r.SelectInteractive(@namespace, namespaces, "Namespace", sd);

            return @namespace;
        }

        static string ParseReplaceOperation(Replacements r, StringDistance sd, string operation)
        {
            if (MultiEnumLogic<OperationDN>.TryToEntity(operation) != null)
                return operation;

            operation = r.Apply("Operation", operation);

            if (MultiEnumLogic<OperationDN>.TryToEntity(operation) != null)
                return operation;

            operation = r.SelectInteractive(operation, MultiEnumLogic<OperationDN>.AllUniqueKeys(), "Operation", sd);

            return operation;
        }

        static string ParseReplaceQuery(Replacements r, StringDistance sd, string query)
        {
            if (QueryLogic.QueryNames.ContainsKey(query))
                return query;

            query = r.Apply("Query", query);

            if (QueryLogic.QueryNames.ContainsKey(query))
                return query;

            query = r.SelectInteractive(query, QueryLogic.QueryNames.Keys, "Query", sd);

            return query;
        }

        private static string ParseReplaceType(Replacements r, StringDistance sd, string type)
        {
            if(TypeLogic.TryGetType(type) != null)
                return type;

            type = r.Apply("Type", type); 

            if(TypeLogic.TryGetType(type) != null)
                return type;

            type = r.SelectInteractive(type, TypeLogic.NameToType.Keys, "Type", sd);

            return type;
        }

        static string ParseReplacePropertyRoute(Replacements r, StringDistance sd, Type type, string propertyRoute)
        {
            var key = "Properties-" + TypeLogic.GetCleanName(type);

            PropertyRoute pr;
            try
            {
                pr = PropertyRoute.Parse(type, r.Apply(key, propertyRoute)); //Try parse needed

                return pr.PropertyString();
            }
            catch
            {
                var routes = PropertyRouteLogic.GenerateProperties(type, type.ToTypeDN()).Select(a => a.ToString()).ToList();

                string str = r.SelectInteractive(propertyRoute, routes, key, sd);

                return str;
            }
        }


        static string Link(string letter, string link, string text)
        {
            if (text.HasText())
                return "[{0}:{1}|{2}]".Formato(letter, link, text);
            else
                return "[{0}:{1}]".Formato(letter, link); 
        }
    }

    public static class WikiFormat
    {
        public const string EntityLink = "e";
        public const string PropertyLink = "p";
        public const string QueryLink = "q";
        public const string OperationLink = "o";
        public const string Hyperlink = "h";
        public const string WikiLink = "w";
        public const string NamespaceLink = "n";

        public const string Separator = ":";
    }
}