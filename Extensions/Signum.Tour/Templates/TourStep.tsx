import * as React from 'react'
import { CssStepType, TourEntity, TourMessage, TourStepEntity } from '../Signum.Tour'
import { AutoLine, CheckboxLine, EntityLine, EntityTable, EnumLine, FormGroup, MarkdownLine, TextAreaLine, TextBoxLine, TypeContext } from '@framework/Lines'
import HtmlCodeMirror from "@extensions/Signum.CodeMirror/HtmlCodeMirror"
import { getTypeInfos, getTypeInfo } from '@framework/Reflection';
import { PropertyRouteEntity, QueryEntity, TypeEntity } from '@framework/Signum.Basics';
import { useForceUpdate } from '@framework/Hooks';
import PropertyRouteCombo from '@framework/Components/PropertyRouteCombo';
import { classes } from '@framework/Globals';
import { getToString, liteKey } from '@framework/Signum.Entities';
import { DashboardEntity } from '../../Signum.Dashboard/Signum.Dashboard';
import { JSX } from 'react/jsx-runtime';

export default function TourStep(p: {
  ctx: TypeContext<TourStepEntity>,
  invalidate: () => void;
  type?: TypeEntity | null;
  dashboard?: DashboardEntity | null;
}): JSX.Element {
  const ctx = p.ctx;
  const sc = ctx.subCtx({ labelColumns: 2 });
  const sc4 = ctx.subCtx({ labelColumns: 4 });
  const forceUpdate = useForceUpdate();

  const handleSideChange = () => {
    const side = ctx.value.side;
    if (side === "Top" || side === "Bottom") {
      ctx.value.align = "Center";
    } else if (side === "Left" || side === "Right") {
      ctx.value.align = "Start";
    }
    forceUpdate();
    p.invalidate();
  };

  const getAvailableCssStepTypes = (): CssStepType[] => {
    return CssStepType.values().filter(t =>
      (t !== "Property" || p.type != null) &&
      (t !== "DashboardPart" || p.dashboard != null));
  };

  const dashboardPartOptions = React.useMemo(() => {
    if (!p.dashboard)
      return [];
    return p.dashboard.parts
      .map(pp => pp.element)
      .map(pp => ({
        key: pp.guid,
        label: pp.title || getToString(pp.content),
      }));
  }, [p.dashboard]);

  return (
    <div>
      <AutoLine ctx={sc.subCtx(a => a.title)} onChange={p.invalidate} />
      <EntityTable ctx={sc.subCtx(a => a.cssSteps)} avoidFieldSet onChange={forceUpdate} columns={[
        {
          property: a => a.type,
          template: (ctx, row) => <EnumLine ctx={ctx.subCtx(a => a.type)} onChange={() => {
            ctx.value.cssSelector = null;
            ctx.value.property = null;
            ctx.value.toolbarContent = null;
            ctx.value.dashboardPart = null;
            row.forceUpdate();
          }} optionItems = { getAvailableCssStepTypes() } />,
          headerHtmlAttributes: { style: { width: "20%" } }
        },
        {
          header: "CSS Step",
          template: (ctx) => {
            const type = ctx.value.type;
            if (type === "CSSSelector") {
              return <TextBoxLine ctx={ctx.subCtx(a => a.cssSelector)} onChange={forceUpdate} valueHtmlAttributes={{ className: "font-monospace", placeholder: "#someId div.some-class" }} />;
            } else if (type === "Property") {
              return p.type && <PropertyRouteCombo ctx={ctx.subCtx(a => a.property)} type={p.type!} onChange={forceUpdate}/>;
            } else if (type === "ToolbarContent") {
              return <EntityLine ctx={ctx.subCtx(a => a.toolbarContent)} onChange={forceUpdate} create={false}  />;
            } else if (type === "DashboardPart") {
              return (
                <select className="form-select form-select-sm"
                  value={ctx.value.dashboardPart ?? ""}
                  onChange={e => {
                    ctx.value.dashboardPart = e.currentTarget.value || null;
                    ctx.value.modified = true;
                    forceUpdate();
                  }}>
                  <option value="">-</option>
                  {dashboardPartOptions.map(o => <option key={o.key} value={o.key}>{o.label}</option>)}
                </select>
              );
            }
            return null;
          },
          headerHtmlAttributes: { style: { width: "80%" } }
        }
      ]} />
      <div className="mt-3">
      <FormGroup ctx={ctx} label="Final CSS Selector" >
      {id => <code id={id}>{ctx.value.cssSteps.map(a => a.element).map(s =>
        s.type == "CSSSelector" ? s.cssSelector :
        s.type == "Property" ? `[data-property-path='${s.property?.path}']` :
        s.type == "ToolbarContent" && s.toolbarContent ? `[data-toolbar-content='${(QueryEntity.isLite(s.toolbarContent) ? getToString(s.toolbarContent) : liteKey(s.toolbarContent))}']` :
        s.type == "DashboardPart" && s.dashboardPart ? `[data-part-content='${s.dashboardPart}']` :
        null).join(" ")
        }</code> }
      </FormGroup>
      </div>
       
      <div className="row">
        <div className="col-sm-4">
          <AutoLine ctx={sc4.subCtx(a => a.side)} onChange={handleSideChange} mandatory />
        </div>
        <div className="col-sm-4">
          <AutoLine ctx={sc4.subCtx(a => a.align)} />
        </div>
         <div className="col-sm-4 d-flex justify-content-center align-items-center">
          <EnumLine ctx={sc4.subCtx(a => a.click)} onChange={forceUpdate} />
        </div>
      </div>
      <MarkdownLine ctx={sc.subCtx(a => a.description)} onChange={forceUpdate} />
    </div>
  );
}
