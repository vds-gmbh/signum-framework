﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34003
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Signum.Web.Extensions.ControlPanel.Views
{
    using System;
    using System.Collections.Generic;
    
    #line 1 "..\..\ControlPanel\Views\UserChartPart.cshtml"
    using System.Configuration;
    
    #line default
    #line hidden
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Web;
    using System.Web.Helpers;
    using System.Web.Mvc;
    using System.Web.Mvc.Ajax;
    using System.Web.Mvc.Html;
    using System.Web.Routing;
    using System.Web.Security;
    using System.Web.UI;
    using System.Web.WebPages;
    
    #line 7 "..\..\ControlPanel\Views\UserChartPart.cshtml"
    using Signum.Engine.Chart;
    
    #line default
    #line hidden
    
    #line 8 "..\..\ControlPanel\Views\UserChartPart.cshtml"
    using Signum.Engine.DynamicQuery;
    
    #line default
    #line hidden
    using Signum.Entities;
    
    #line 5 "..\..\ControlPanel\Views\UserChartPart.cshtml"
    using Signum.Entities.Chart;
    
    #line default
    #line hidden
    
    #line 2 "..\..\ControlPanel\Views\UserChartPart.cshtml"
    using Signum.Entities.ControlPanel;
    
    #line default
    #line hidden
    
    #line 4 "..\..\ControlPanel\Views\UserChartPart.cshtml"
    using Signum.Entities.DynamicQuery;
    
    #line default
    #line hidden
    using Signum.Utilities;
    using Signum.Web;
    
    #line 6 "..\..\ControlPanel\Views\UserChartPart.cshtml"
    using Signum.Web.Chart;
    
    #line default
    #line hidden
    
    #line 3 "..\..\ControlPanel\Views\UserChartPart.cshtml"
    using Signum.Web.ControlPanel;
    
    #line default
    #line hidden
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/ControlPanel/Views/UserChartPart.cshtml")]
    public partial class UserChartPart : System.Web.Mvc.WebViewPage<PanelPartDN>
    {
        public UserChartPart()
        {
        }
        public override void Execute()
        {
            
            #line 10 "..\..\ControlPanel\Views\UserChartPart.cshtml"
Write(Html.ScriptsJs("~/Chart/Scripts/SF_Chart.js",
                "~/Chart/Scripts/SF_Chart_Utils.js",
                "~/scripts/d3.v3.min.js",
                "~/scripts/colorbrewer.js"));

            
            #line default
            #line hidden
WriteLiteral("\r\n");

            
            #line 14 "..\..\ControlPanel\Views\UserChartPart.cshtml"
Write(Html.ScriptCss("~/Chart/Content/SF_Chart.css"));

            
            #line default
            #line hidden
WriteLiteral("\r\n");

            
            #line 15 "..\..\ControlPanel\Views\UserChartPart.cshtml"
   
    var ucPart = (UserChartPartDN)Model.Content;
    UserChartDN uc = ucPart.UserChart;
    ChartRequest request = uc.ToRequest();
    
    var containerId = crc.Compose("sfChartBuilderContainer");

    using (var crc = new TypeContext<ChartRequest>(request, "r{0}c{1}".Formato(Model.Row, Model.Column)))
    {
        ResultTable resultTable = ChartLogic.ExecuteChart(request);



            
            #line default
            #line hidden
WriteLiteral("    <div");

WriteAttribute("id", Tuple.Create(" id=\"", 917), Tuple.Create("\"", 952)
            
            #line 27 "..\..\ControlPanel\Views\UserChartPart.cshtml"
, Tuple.Create(Tuple.Create("", 922), Tuple.Create<System.Object, System.Int32>(crc.Compose("sfChartControl")
            
            #line default
            #line hidden
, 922), false)
);

WriteLiteral(" class=\"sf-search-control sf-chart-control\"");

WriteLiteral(" data-prefix=\"");

            
            #line 27 "..\..\ControlPanel\Views\UserChartPart.cshtml"
                                                                                                Write(crc.ControlID);

            
            #line default
            #line hidden
WriteLiteral("\"");

WriteLiteral(">\r\n        <div");

WriteLiteral(" style=\"display: none\"");

WriteLiteral(">\r\n");

WriteLiteral("            ");

            
            #line 29 "..\..\ControlPanel\Views\UserChartPart.cshtml"
       Write(Html.HiddenRuntimeInfo(crc));

            
            #line default
            #line hidden
WriteLiteral("\r\n");

WriteLiteral("            ");

            
            #line 30 "..\..\ControlPanel\Views\UserChartPart.cshtml"
       Write(Html.Hidden(crc.Compose("sfOrders"), request.Orders.IsNullOrEmpty() ? "" :
                    (request.Orders.ToString(oo => (oo.OrderType == OrderType.Ascending ? "" : "-") + oo.Token.FullKey(), ";") + ";")));

            
            #line default
            #line hidden
WriteLiteral("\r\n");

            
            #line 32 "..\..\ControlPanel\Views\UserChartPart.cshtml"
            
            
            #line default
            #line hidden
            
            #line 32 "..\..\ControlPanel\Views\UserChartPart.cshtml"
              
        ViewData[ViewDataKeys.QueryDescription] = DynamicQueryManager.Current.QueryDescription(request.QueryName);
        ViewData[ViewDataKeys.FilterOptions] = request.Filters.Select(f => new FilterOption { Token = f.Token, Operation = f.Operation, Value = f.Value }).ToList();
            
            
            #line default
            #line hidden
WriteLiteral("\r\n");

WriteLiteral("            ");

            
            #line 36 "..\..\ControlPanel\Views\UserChartPart.cshtml"
       Write(Html.Partial(Navigator.Manager.FilterBuilderView, crc));

            
            #line default
            #line hidden
WriteLiteral("\r\n            <div");

WriteAttribute("id", Tuple.Create(" id=\"", 1730), Tuple.Create("\"", 1747)
            
            #line 37 "..\..\ControlPanel\Views\UserChartPart.cshtml"
, Tuple.Create(Tuple.Create("", 1735), Tuple.Create<System.Object, System.Int32>(containerId
            
            #line default
            #line hidden
, 1735), false)
);

WriteLiteral(">\r\n");

WriteLiteral("                ");

            
            #line 38 "..\..\ControlPanel\Views\UserChartPart.cshtml"
           Write(Html.Partial(ChartClient.ChartBuilderView, crc));

            
            #line default
            #line hidden
WriteLiteral("\r\n            </div>\r\n            <script");

WriteLiteral(" type=\"text/javascript\"");

WriteLiteral(">\r\n                var findOptions = ");

            
            #line 41 "..\..\ControlPanel\Views\UserChartPart.cshtml"
                              Write(MvcHtmlString.Create(uc.ToJS().ToString()));

            
            #line default
            #line hidden
WriteLiteral("\r\n                 require([\"");

            
            #line 42 "..\..\ControlPanel\Views\UserChartPart.cshtml"
                      Write(ChartClient.Module);

            
            #line default
            #line hidden
WriteLiteral("\"], function (Chart) { new Chart.ChartBuilder($(\'#");

            
            #line 42 "..\..\ControlPanel\Views\UserChartPart.cshtml"
                                                                                           Write(containerId);

            
            #line default
            #line hidden
WriteLiteral("\'), $.extend({ prefix: \'");

            
            #line 42 "..\..\ControlPanel\Views\UserChartPart.cshtml"
                                                                                                                               Write(crc.ControlID);

            
            #line default
            #line hidden
WriteLiteral("\' }, findOptions)); }); \r\n            </script>\r\n        </div>\r\n        <div");

WriteAttribute("id", Tuple.Create(" id=\"", 2186), Tuple.Create("\"", 2223)
            
            #line 45 "..\..\ControlPanel\Views\UserChartPart.cshtml"
, Tuple.Create(Tuple.Create("", 2191), Tuple.Create<System.Object, System.Int32>(crc.Compose("sfChartContainer")
            
            #line default
            #line hidden
, 2191), false)
);

WriteLiteral(">\r\n            <div");

WriteLiteral(" class=\"sf-chart-container\"");

WriteAttribute("style", Tuple.Create(" style=\"", 2270), Tuple.Create("\"", 2323)
, Tuple.Create(Tuple.Create("", 2278), Tuple.Create("display:", 2278), true)
            
            #line 46 "..\..\ControlPanel\Views\UserChartPart.cshtml"
, Tuple.Create(Tuple.Create("", 2286), Tuple.Create<System.Object, System.Int32>(ucPart.ShowData ? "none" : "block"
            
            #line default
            #line hidden
, 2286), false)
);

WriteLiteral(" \r\n                    data-open-url=\"");

            
            #line 47 "..\..\ControlPanel\Views\UserChartPart.cshtml"
                               Write(Url.Action<ChartController>(cc => cc.OpenSubgroup(crc.ControlID)));

            
            #line default
            #line hidden
WriteLiteral("\"");

WriteLiteral(" \r\n                    data-fullscreen-url=\"");

            
            #line 48 "..\..\ControlPanel\Views\UserChartPart.cshtml"
                                     Write(Url.Action<ChartController>(cc => cc.FullScreen(crc.ControlID)));

            
            #line default
            #line hidden
WriteLiteral("\"");

WriteLiteral("\r\n                    data-json=\"");

            
            #line 49 "..\..\ControlPanel\Views\UserChartPart.cshtml"
                          Write(Html.Json(ChartUtils.DataJson(crc.Value, resultTable)).ToString());

            
            #line default
            #line hidden
WriteLiteral("\"");

WriteLiteral(">\r\n            </div>\r\n        </div>\r\n    </div>\r\n");

            
            #line 53 "..\..\ControlPanel\Views\UserChartPart.cshtml"
    
        if (ucPart.ShowData)
        {
            ViewData[ViewDataKeys.Results] = resultTable;
            ViewData[ViewDataKeys.Navigate] = false;

            QuerySettings settings = Navigator.QuerySettings(request.QueryName);
            ViewData[ViewDataKeys.Formatters] = resultTable.Columns.Select((c, i) => new { c, i }).ToDictionary(c => c.i, c => settings.GetFormatter(c.c.Column));

            
            
            #line default
            #line hidden
            
            #line 62 "..\..\ControlPanel\Views\UserChartPart.cshtml"
       Write(Html.Partial(ChartClient.ChartResultsTableView, new TypeContext<ChartRequest>(request, "r{0}c{1}".Formato(Model.Row, Model.Column))));

            
            #line default
            #line hidden
            
            #line 62 "..\..\ControlPanel\Views\UserChartPart.cshtml"
                                                                                                                                                 
        }
        else
        {
            MvcHtmlString divSelector = MvcHtmlString.Create("#" + crc.Compose("sfChartContainer") + " > .sf-chart-container");
   

            
            #line default
            #line hidden
WriteLiteral("            <script");

WriteLiteral(" type=\"text/javascript\"");

WriteLiteral(">\r\n                (function () {\r\n                    $(\"#");

            
            #line 70 "..\..\ControlPanel\Views\UserChartPart.cshtml"
                   Write(containerId);

            
            #line default
            #line hidden
WriteLiteral("\").SFControl().reDraw();\r\n                })();\r\n            </script>\r\n");

            
            #line 73 "..\..\ControlPanel\Views\UserChartPart.cshtml"
        }

            
            #line default
            #line hidden
WriteLiteral("        <script");

WriteLiteral(" type=\"text/javascript\"");

WriteLiteral(">\r\n            (function () {\r\n                $(\"#\" + SF.compose(\"");

            
            #line 76 "..\..\ControlPanel\Views\UserChartPart.cshtml"
                               Write(crc.ControlID);

            
            #line default
            #line hidden
WriteLiteral("\", \"sfFullScreen\")).on(\"mousedown\", function (e) {\r\n                    $(\"#");

            
            #line 77 "..\..\ControlPanel\Views\UserChartPart.cshtml"
                   Write(containerId);

            
            #line default
            #line hidden
WriteLiteral("\").SFControl().fullScreen();\r\n                });\r\n            })();\r\n        </s" +
"cript>\r\n");

            
            #line 81 "..\..\ControlPanel\Views\UserChartPart.cshtml"
    }

            
            #line default
            #line hidden
WriteLiteral("\r\n");

        }
    }
}
#pragma warning restore 1591
