import * as React from 'react'
import { Button } from 'react-bootstrap';
import { PropertyRouteEntity } from '@framework/Signum.Basics';
import { Operations } from '@framework/Operations'
import { TypeContext, ButtonsContext, IRenderButtons } from '@framework/TypeContext'
import { EntityLine, AutoLine } from '@framework/Lines'
import { AuthAdminClient } from '../AuthAdminClient'
import { PropertyRulePack, PropertyAllowedRule, PropertyAllowed, AuthAdminMessage, WithConditions } from './Signum.Authorization.Rules'
import { ColorRadio, GrayCheckbox } from './ColoredRadios'
import "./AuthAdmin.css"
import { useForceUpdate } from '@framework/Hooks';

export default function PropertyRulesPackControl({ ctx, innerRef }: { ctx: TypeContext<PropertyRulePack>, innerRef?: React.Ref<IRenderButtons> }): React.JSX.Element {

  function handleSaveClick(bc: ButtonsContext) {
    let pack = ctx.value;

    AuthAdminClient.API.savePropertyRulePack(pack)
      .then(() => AuthAdminClient.API.fetchPropertyRulePack(pack.type.cleanName!, pack.role.id!))
      .then(newPack => {
        Operations.notifySuccess();
        bc.frame.onReload({ entity: newPack, canExecute: {} });
      });
  }

  function renderButtons(bc: ButtonsContext) {
    return [
      { button: <Button variant="primary" disabled={ctx.readOnly} onClick={() => handleSaveClick(bc)}>{AuthAdminMessage.Save.niceToString()}</Button> }
    ];
  }

  React.useImperativeHandle(innerRef, () => ({ renderButtons }), [ctx.value]);
  const forceUpdate = useForceUpdate();

  function handleHeaderClick(e: React.MouseEvent<HTMLAnchorElement>, hc: PropertyAllowed) {

    ctx.value.rules.forEach(mle => {
      if (!mle.element.coercedValues!.some(c => c.fallback == hc)) {
        mle.element.allowed = WithConditions(PropertyAllowed).New({ fallback: hc });
        mle.element.modified = true;
      }
    });

    forceUpdate();
  }

  return (
    <div>
      <div className="form-compact">
        <EntityLine ctx={ctx.subCtx(f => f.role)} />
        <AutoLine ctx={ctx.subCtx(f => f.strategy)} />
        <EntityLine ctx={ctx.subCtx(f => f.type)} />
      </div>
      <table className="table table-sm sf-auth-rules">
        <thead>
          <tr>
            <th>
              {PropertyRouteEntity.niceName()}
            </th>
            <th style={{ textAlign: "center" }}>
              <a onClick={e => handleHeaderClick(e, "Write")}>{PropertyAllowed.niceToString("Write")}</a>
            </th>
            <th style={{ textAlign: "center" }}>
              <a onClick={e => handleHeaderClick(e, "Read")}>{PropertyAllowed.niceToString("Read")}</a>
            </th>
            <th style={{ textAlign: "center" }}>
              <a onClick={e => handleHeaderClick(e, "None")}>{PropertyAllowed.niceToString("None")}</a>
            </th>
            <th style={{ textAlign: "center" }}>
              {AuthAdminMessage.Overriden.niceToString()}
            </th>
          </tr>
        </thead>
        <tbody>
          {ctx.mlistItemCtxs(a => a.rules).map((c, i) =>
            <tr key={i}>
              <td>
                {c.value.resource.path}
              </td>
              <td style={{ textAlign: "center" }}>
                {renderRadio(c.value, "Write", "green")}
              </td>
              <td style={{ textAlign: "center" }}>
                {renderRadio(c.value, "Read", "#FFAD00")}
              </td>
              <td style={{ textAlign: "center" }}>
                {renderRadio(c.value, "None", "red")}
              </td>
              <td style={{ textAlign: "center" }}>
                <GrayCheckbox readOnly={ctx.readOnly} checked={c.value.allowed != c.value.allowedBase} onUnchecked={() => {
                  c.value.allowed = c.value.allowedBase;
                  ctx.value.modified = true;
                  forceUpdate();
                }} />
              </td>
            </tr>
          )
          }
        </tbody>
      </table>

    </div>
  );

  function renderRadio(c: PropertyAllowedRule, allowed: PropertyAllowed, color: string) {

    if (c.coercedValues!.some(a => a.fallback == allowed))
      return;

    return <ColorRadio
      readOnly={ctx.readOnly}
      checked={c.allowed.fallback == allowed}
      color={color}
      onClicked={a => { c.allowed = WithConditions(PropertyAllowed).New({ fallback: allowed }); c.modified = true; forceUpdate() }}
    />;
  }
}
