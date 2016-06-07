﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using OpenQA.Selenium.Remote;
using Signum.Engine;
using Signum.Engine.Basics;
using Signum.Entities;
using Signum.Entities.Reflection;
using Signum.Entities.UserQueries;
using Signum.Utilities;
using System.Reflection;
using Signum.Entities.DynamicQuery;
using Signum.Entities.UserAssets;
using OpenQA.Selenium;
using Signum.React.Selenium;

namespace Signum.React.Selenium
{
    public interface ILineContainer<T> : ILineContainer where T : ModifiableEntity
    {
    }

    public interface ILineContainer
    {
        IWebElement Element { get; }

        PropertyRoute Route { get; }
    }

    public  class LineLocator<T>
    {
        public WebElementLocator ElementLocator { get; set; }

        public PropertyRoute Route { get; set; }
    }

    public static class LineContainerExtensions
    {
        public static bool HasError(this RemoteWebDriver selenium, string elementId)
        {
            return selenium.IsElementPresent(By.CssSelector("#{0}.input-validation-error".FormatWith(elementId)));
        }

        public static LineLocator<S> LineLocator<T, S>(this ILineContainer<T> lineContainer, Expression<Func<T, S>> property) where T : ModifiableEntity
        {
            PropertyRoute route = lineContainer.Route ?? PropertyRoute.Root(typeof(T));

            var element = lineContainer.Element;

            foreach (var mi in Reflector.GetMemberList(property))
            {
                if (mi is MethodInfo && ((MethodInfo)mi).IsInstantiationOf(MixinDeclarations.miMixin))
                {
                    route = route.Add(((MethodInfo)mi).GetGenericArguments()[0]);
                }
                else
                {
                    var newRoute = route.Add(mi);

                    if (newRoute.Parent != route)
                        element = element.FindElement(By.CssSelector("[data-propertypath=" + route.PropertyString() + "]"));

                    route = newRoute;
                }
            }

            return new LineLocator<S>
            {
                Route = route,
                ElementLocator = element.WithLocator(By.CssSelector("[data-propertypath=" + route.PropertyString() + "]"))
            };
        }


        public static bool IsVisible<T, S>(this ILineContainer<T> lineContainer, Expression<Func<T, S>> property)
            where T : ModifiableEntity
        {
            return lineContainer.LineLocator(property).ElementLocator.IsVisible();
        }

        public static bool IsPresent<T, S>(this ILineContainer<T> lineContainer, Expression<Func<T, S>> property)
            where T : ModifiableEntity
        {
            return lineContainer.LineLocator(property).ElementLocator.IsPresent();
        }

        public static void WaitVisible<T, S>(this ILineContainer<T> lineContainer, Expression<Func<T, S>> property)
            where T : ModifiableEntity
        {
            lineContainer.LineLocator(property).ElementLocator.WaitVisible();
        }

        public static void WaitPresent<T, S>(this ILineContainer<T> lineContainer, Expression<Func<T, S>> property)
            where T : ModifiableEntity
        {
            lineContainer.LineLocator(property).ElementLocator.WaitPresent();
        }

        public static void WaitNoVisible<T, S>(this ILineContainer<T> lineContainer, Expression<Func<T, S>> property)
       where T : ModifiableEntity
        {
            lineContainer.LineLocator(property).ElementLocator.WaitNoVisible();
        }

        public static void WaitNoPresent<T, S>(this ILineContainer<T> lineContainer, Expression<Func<T, S>> property)
            where T : ModifiableEntity
        {
            lineContainer.LineLocator(property).ElementLocator.WaitNoPresent();
        }

        public static LineContainer<S> SubContainer<T, S>(this ILineContainer<T> lineContainer, Expression<Func<T, S>> property) 
            where T : ModifiableEntity
            where S : ModifiableEntity
        {
            var lineLocator = lineContainer.LineLocator(property);

            return new LineContainer<S>(lineLocator.ElementLocator.Find(), lineLocator.Route);
        }

        public static ValueLineProxy ValueLine<T, V>(this ILineContainer<T> lineContainer, Expression<Func<T, V>> property)
            where T : ModifiableEntity
        {
            var lineLocator = lineContainer.LineLocator(property);

            return new ValueLineProxy(lineLocator.ElementLocator.Find(), lineLocator.Route);
        }

        public static void ValueLineValue<T, V>(this ILineContainer<T> lineContainer, Expression<Func<T, V>> property, V value, bool loseFocus = false)
            where T : ModifiableEntity
        {
            var valueLine = lineContainer.ValueLine(property);

            valueLine.Value = value;

            if (loseFocus)
                valueLine.MainElement.Find().LoseFocus();
        }

        public static FileLineProxy FileLine<T, V>(this ILineContainer<T> lineContainer, Expression<Func<T, V>> property)
        where T : ModifiableEntity
        {
            var lineLocator = lineContainer.LineLocator(property);

            return new FileLineProxy(lineLocator.ElementLocator.Find(), lineLocator.Route);
        }

        public static V ValueLineValue<T, V>(this ILineContainer<T> lineContainer, Expression<Func<T, V>> property)
            where T : ModifiableEntity
        {
            return (V)lineContainer.ValueLine(property).Value;
        }

        public static EntityLineProxy EntityLine<T, V>(this ILineContainer<T> lineContainer, Expression<Func<T, V>> property)
          where T : ModifiableEntity
        {
            var lineLocator = lineContainer.LineLocator(property);

            return new EntityLineProxy(lineLocator.ElementLocator.Find(), lineLocator.Route);
        }

        public static V EntityLineValue<T, V>(this ILineContainer<T> lineContainer, Expression<Func<T, V>> property)
        where T : ModifiableEntity
        {
            var lite = lineContainer.EntityLine(property).LiteValue;

            return lite is V ? (V)lite : (V)(object)lite.Retrieve();
        }

        public static void EntityLineValue<T, V>(this ILineContainer<T> lineContainer, Expression<Func<T, V>> property, V value)
            where T : ModifiableEntity
        {
            lineContainer.EntityLine(property).LiteValue = value as Lite<IEntity> ?? ((IEntity)value)?.ToLite();
        }

        public static EntityComboProxy EntityCombo<T, V>(this ILineContainer<T> lineContainer, Expression<Func<T, V>> property)
          where T : ModifiableEntity
        {
            var lineLocator = lineContainer.LineLocator(property);

            return new EntityComboProxy(lineLocator.ElementLocator.Find(), lineLocator.Route);
        }

        public static V EntityComboValue<T, V>(this ILineContainer<T> lineContainer, Expression<Func<T, V>> property)
        where T : ModifiableEntity
        {
            var lite = lineContainer.EntityCombo(property).LiteValue;

            return lite is V ? (V)lite : (V)(object)lite.Retrieve();
        }

        public static void EntityComboValue<T, V>(this ILineContainer<T> lineContainer, Expression<Func<T, V>> property, V value, bool loseFocus = false)
            where T : ModifiableEntity
        {
            var combo = lineContainer.EntityCombo(property);

            combo.LiteValue = value as Lite<IEntity> ?? ((IEntity)value)?.ToLite();

            if (loseFocus)
                combo.ComboElement.WrappedElement.LoseFocus();
        }

        public static EntityDetailProxy EntityDetail<T, V>(this ILineContainer<T> lineContainer, Expression<Func<T, V>> property)
          where T : ModifiableEntity
        {
            var lineLocator = lineContainer.LineLocator(property);

            return new EntityDetailProxy(lineLocator.ElementLocator.Find(), lineLocator.Route);
        }

        public static EntityRepeaterProxy EntityRepeater<T, V>(this ILineContainer<T> lineContainer, Expression<Func<T, V>> property)
          where T : ModifiableEntity
        {
            var lineLocator = lineContainer.LineLocator(property);

            return new EntityRepeaterProxy(lineLocator.ElementLocator.Find(), lineLocator.Route);
        }

        public static EntityTabRepeaterProxy EntityTabRepeater<T, V>(this ILineContainer<T> lineContainer, Expression<Func<T, V>> property)
           where T : ModifiableEntity
        {
            var lineLocator = lineContainer.LineLocator(property);

            return new EntityTabRepeaterProxy(lineLocator.ElementLocator.Find(), lineLocator.Route);
        }

        public static EntityStripProxy EntityStrip<T, V>(this ILineContainer<T> lineContainer, Expression<Func<T, V>> property)
            where T : ModifiableEntity
        {
            var lineLocator = lineContainer.LineLocator(property);

            return new EntityStripProxy(lineLocator.ElementLocator.Find(), lineLocator.Route);
        }

        public static EntityListProxy EntityList<T, V>(this ILineContainer<T> lineContainer, Expression<Func<T, V>> property)
          where T : ModifiableEntity
        {
            var lineLocator = lineContainer.LineLocator(property);

            return new EntityListProxy(lineLocator.ElementLocator.Find(), lineLocator.Route);
        }

        public static EntityListCheckBoxProxy EntityListCheckBox<T, V>(this ILineContainer<T> lineContainer, Expression<Func<T, V>> property)
            where T : ModifiableEntity
        {
            var lineLocator = lineContainer.LineLocator(property);

            return new EntityListCheckBoxProxy(lineLocator.ElementLocator.Find(), lineLocator.Route);
        }

        public static bool IsImplementation(this PropertyRoute route, Type type)
        {
            if (!typeof(Entity).IsAssignableFrom(type))
                return false;

            var routeType = route.Type.CleanType();

            return routeType.IsAssignableFrom(type);
        }

        public static QueryTokenBuilderProxy QueryTokenBuilder<T>(this ILineContainer<T> lineContainer, Expression<Func<T, QueryTokenEntity>> property)
            where T : ModifiableEntity
        {
            var lineLocator = lineContainer.LineLocator(property);

            return new QueryTokenBuilderProxy(lineLocator.ElementLocator.Find());
        }

        public static void SelectTab(this ILineContainer lineContainer, string title)
        {
            var tabs = lineContainer.Element.FindElement(By.CssSelector("ul[role=tablist]"));

            var tab = tabs.FindElements(By.CssSelector("a[role=tab]")).Single(a => a.Text.Contains(title));

        }

        public static SearchControlProxy GetSearchControl(this ILineContainer lineContainer, object queryName)
        {
            string queryKey = QueryUtils.GetKey(queryName);
            
            var element = lineContainer.Element.FindElement(By.CssSelector("div.sf-search-control[data-query-key={0}]".FormatWith(queryKey)));

            return new SearchControlProxy(element);
        }
    }

    public class LineContainer<T> :ILineContainer<T> where T:ModifiableEntity
    {
        public IWebElement Element { get; private set; }

        public PropertyRoute Route { get; private set; }

        public LineContainer(IWebElement element, PropertyRoute route = null)
        {
            this.Element = element;
            this.Route = route == null || route.IsImplementation(typeof(T)) ? PropertyRoute.Root(typeof(T)) : route;
        }
    }

    public class NormalPage<T> : ILineContainer<T>, IEntityButtonContainer<T>, IWidgetContainer, IValidationSummaryContainer, IDisposable where T : ModifiableEntity
    {
        public RemoteWebDriver Selenium { get; private set; }

        public IWebElement Element { get; private set; }

        public PropertyRoute Route { get; private set; }

        public NormalPage(RemoteWebDriver selenium)
        {
            this.Selenium = selenium;
            this.Element = selenium.WaitElementPresent(By.CssSelector(".normal-control"));
            this.Route = PropertyRoute.Root(typeof(T));
        }

        public IWebElement ContainerElement()
        {
            return this.Element;
        }

        public void Dispose()
        {
        }

        public NormalPage<T> WaitLoadedAndId()
        {
            this.Selenium.Wait(() => {var ri = this.EntityInfo(); return ri != null && ri.EntityType == typeof(T) && ri.IdOrNull.HasValue;});

            return this;
        }

        public string Title()
        {
            return (string)Selenium.ExecuteScript("return $('#divMainPage > h3 > .sf-entity-title').html()");
        }

        public EntityInfoProxy EntityInfo()
        {
            return EntityInfoProxy.Parse(this.Element.FindElement(By.CssSelector("sf-main-control")).GetAttribute("data-main-entity"));
        }

        public T RetrieveEntity()
        {
            var lite = this.EntityInfo().ToLite();
            return (T)(IEntity)lite.Retrieve();
        }

        public NormalPage<T> WaitLoaded()
        {
            this.Element.GetDriver().Wait(() => this.EntityInfo() != null);
            return this;
        }
    }
}
