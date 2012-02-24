﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.239
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ASP
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Helpers;
    using System.Web.Security;
    using System.Web.UI;
    using System.Web.WebPages;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel.DataAnnotations;
    using System.Configuration;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web.Caching;
    using System.Web.DynamicData;
    using System.Web.SessionState;
    using System.Web.Profile;
    using System.Web.UI.WebControls;
    using System.Web.UI.WebControls.WebParts;
    using System.Web.UI.HtmlControls;
    using System.Xml.Linq;
    using System.Web.Mvc;
    using System.Web.Mvc.Ajax;
    using System.Web.Mvc.Html;
    using System.Web.Routing;
    using Signum.Utilities;
    using Signum.Entities;
    using Signum.Web;
    using Signum.Web.Extensions.Properties;
    using Signum.Entities.DynamicQuery;
    using Signum.Engine.DynamicQuery;
    using Signum.Entities.Reflection;
    using Signum.Entities.Chart;
    using Signum.Web.Chart;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("MvcRazorClassGenerator", "1.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Chart/Views/ChartToken.cshtml")]
    public class _Page_Chart_Views_ChartToken_cshtml : System.Web.Mvc.WebViewPage<dynamic>
    {


        public _Page_Chart_Views_ChartToken_cshtml()
        {
        }
        protected System.Web.HttpApplication ApplicationInstance
        {
            get
            {
                return ((System.Web.HttpApplication)(Context.ApplicationInstance));
            }
        }
        public override void Execute()
        {









WriteLiteral("\r\n");


 using (var tc = Html.TypeContext<ChartTokenDN>())
{
    if (tc.Value == null)
    {
        tc.Value = new ChartTokenDN();
    }
    ChartBase chart = ((TypeContext<ChartBase>)tc.Parent).Value;
    

WriteLiteral("    <tr class=\"sf-chart-token\" data-token=\"");


                                      Write(chart.GetTokenName(tc.Value));

WriteLiteral("\">\r\n        <td>");


       Write(tc.Value.PropertyLabel);

WriteLiteral("</td>\r\n        <td>\r\n");


             if (tc.Value.GroupByVisible)
            { 
                var groupCheck = new HtmlTag("input").IdName(tc.Compose("group")).Attr("type", "checkbox").Attr("value", "True").Class("sf-chart-group-trigger");
                bool groupResults = chart.GroupResults;
                if (groupResults)
                {
                    groupCheck.Attr("checked", "checked");
                }
                
           Write(groupCheck.ToHtmlSelf());

                                        
                
           Write(Html.Hidden(tc.Compose("group"), groupResults));

                                                               
            }

WriteLiteral("        </td>\r\n        <td>\r\n            <div class=\"sf-query-token\">\r\n          " +
"      ");


           Write(Html.ChartTokenCombo(tc.Value, chart, ViewData[ViewDataKeys.QueryName], tc));

WriteLiteral("\r\n            </div>\r\n            <a class=\"sf-chart-token-config-trigger\">");


                                                Write(Resources.Chart_ToggleInfo);

WriteLiteral("</a>\r\n        </td>\r\n    </tr>\r\n");



WriteLiteral("    <tr class=\"sf-chart-token-config\" style=\"display:none\">\r\n        <td></td>\r\n " +
"       <td colspan=\"2\">\r\n");


             using (Html.FieldInline()) 
            { 
                
           Write(Html.ValueLine(tc, ct => ct.DisplayName));

                                                         
            }

WriteLiteral("        </td>\r\n    </tr>\r\n");


}

        }
    }
}
