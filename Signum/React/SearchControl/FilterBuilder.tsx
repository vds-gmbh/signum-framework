import * as React from 'react'
import { DateTime } from 'luxon'
import { areEqual, classes, isNumber, KeyGenerator } from '../Globals'
import {
  FilterOptionParsed, QueryDescription, QueryToken, SubTokensOptions, getFilterOperations, isList, FilterOperation, FilterConditionOptionParsed, FilterGroupOptionParsed,
  hasAnyOrAll, getTokenParents, isPrefix, isCheckBox, canSplitValue, isFilterGroup, isFilterCondition
} from '../FindOptions'
import { SearchMessage, Lite, EntityControlMessage } from '../Signum.Entities'
import { StyleContext } from '../Lines'
import { Binding, IsByAll, getTypeInfos, toNumberFormat } from '../Reflection'
import { TypeContext } from '../TypeContext'
import QueryTokenBuilder from './QueryTokenBuilder'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { DashboardBehaviour, FilterGroupOperation, PinnedFilterActive } from '../Signum.DynamicQuery';
import "./FilterBuilder.css"
import { useForceUpdate, useForceUpdatePromise } from '../Hooks'
import { Dropdown } from 'react-bootstrap'
import PinnedFilterBuilder from './PinnedFilterBuilder'
import { renderFilterValue } from '../Finder'
import { trimDateToFormat } from '../Lines/DateTimeLine'
import { NumericTextBox } from '../Lines/NumberLine'

interface FilterBuilderProps {
  filterOptions: FilterOptionParsed[];
  subTokensOptions: SubTokensOptions;
  queryDescription: QueryDescription;
  onTokenChanged?: (token: QueryToken | undefined) => void;
  lastToken?: QueryToken;
  onFiltersChanged?: (filters: FilterOptionParsed[]) => void;
  onHeightChanged?: () => void;
  readOnly?: boolean;
  title?: React.ReactNode;
  renderValue?: (rvc: RenderValueContext) => React.ReactElement<any> | undefined;
  showPinnedFiltersOptions?: boolean;
  showPinnedFiltersOptionsButton?: boolean;
  showDashboardBehaviour?: boolean;
  avoidPreview?: boolean;
}

export default function FilterBuilder(p: FilterBuilderProps) {

  const [showPinnedFiltersOptionsState, setShowPinnedFiltersOptions] = React.useState<boolean>(p.showPinnedFiltersOptions ?? false)

  const showPinnedFiltersOptions = p.showPinnedFiltersOptionsButton ? showPinnedFiltersOptionsState : (p.showPinnedFiltersOptions ?? false);

  const forceUpdate = useForceUpdatePromise();

  function handlerNewFilter(e: React.MouseEvent<any>, isGroup: boolean) {
    e.preventDefault();
    var lastToken = p.lastToken;

    p.filterOptions.push(isGroup ?
      {
        groupOperation: "Or",
        token: lastToken && hasAnyOrAll(lastToken) ? getTokenParents(lastToken).filter(a => a.queryTokenType == "AnyOrAll").lastOrNull() : undefined,
        filters: [],
        frozen: false,
        expanded: true,
      } as FilterGroupOptionParsed :
      {
        token: p.lastToken,
        operation: (lastToken && (getFilterOperations(lastToken) ?? []).firstOrNull()) ?? undefined,
        value: undefined,
        frozen: false
      } as FilterConditionOptionParsed);

    if (p.onFiltersChanged)
      p.onFiltersChanged(p.filterOptions);

    forceUpdate().then(handleHeightChanged);
  };

  function handlerDeleteFilter(filter: FilterOptionParsed) {
    p.filterOptions.remove(filter);
    if (p.onFiltersChanged)
      p.onFiltersChanged(p.filterOptions);
    forceUpdate().then(handleHeightChanged);
  };

  function handleDeleteAllFilters(e: React.MouseEvent) {
    e.preventDefault();

    var filtersCount = p.filterOptions.length;
    p.filterOptions.filter(fo => !fo.frozen).forEach(fo => p.filterOptions.remove(fo));
    if (p.filterOptions.length == filtersCount)
      return;

    if (p.onFiltersChanged)
      p.onFiltersChanged(p.filterOptions);
    forceUpdate().then(handleHeightChanged);
  };

  function handleFilterChanged() {
    if (p.onFiltersChanged)
      p.onFiltersChanged(p.filterOptions);

      forceUpdate();
  };

  function handleHeightChanged() {
    if (p.onHeightChanged)
      p.onHeightChanged();
  }


  var keyGenerator = React.useMemo(() => new KeyGenerator(), []);
  var showDashboardBehaviour = showPinnedFiltersOptions && (p.showDashboardBehaviour ?? true);
  return (
    <fieldset className="form-xs">
      {showPinnedFiltersOptions && !p.avoidPreview && <div className="mb-3 border-bottom">
        <h4 className="lead">Preview</h4>
        <PinnedFilterBuilder filterOptions={p.filterOptions} onFiltersChanged={handleFilterChanged} showGrid={true} />
      </div>
      }
      {p.title && <legend>{p.title}</legend>}
      <div className="sf-filters-list table-responsive" style={{ overflowX: "visible" }}>
        <table className="table-sm">
          <thead>
            <tr>
              <th className="ps-0">
                <div className="d-flex">
                  {!p.readOnly && p.filterOptions.length > 0 &&
                    <a href="#" title={StyleContext.default.titleLabels ? SearchMessage.DeleteAllFilter.niceToString() : undefined}
                      className="sf-line-button sf-remove sf-remove-filter-icon"
                      onClick={handleDeleteAllFilters}>
                      <FontAwesomeIcon icon="xmark" />
                    </a>}
                  {SearchMessage.Field.niceToString()}
                </div>
              </th>
              <th>{SearchMessage.Operation.niceToString()}</th>
              <th style={{ paddingRight: "20px" }}>{SearchMessage.Value.niceToString()}</th>
              {showPinnedFiltersOptions && <th></th>}
              {showDashboardBehaviour && <th></th>}
              {showPinnedFiltersOptions && <th>{SearchMessage.Label.niceToString()}</th>}
              {showPinnedFiltersOptions && <th>{SearchMessage.Column.niceToString()}</th>}
              {showPinnedFiltersOptions && <th>{SearchMessage.ColSpan.niceToString()}</th>}
              {showPinnedFiltersOptions && <th>{SearchMessage.Row.niceToString()}</th>}
              {showPinnedFiltersOptions && <th>{SearchMessage.IsActive.niceToString()}</th>}
              {showPinnedFiltersOptions && <th>{SearchMessage.Split.niceToString()}</th>}
            </tr>
          </thead>
          <tbody>
            {p.filterOptions.map((f) => isFilterGroup(f) ?
              <FilterGroupComponent key={keyGenerator.getKey(f)} filterGroup={f} readOnly={Boolean(p.readOnly)} onDeleteFilter={handlerDeleteFilter}
                allFilterOptions={p.filterOptions}
                prefixToken={undefined}
                subTokensOptions={p.subTokensOptions} queryDescription={p.queryDescription}
                onTokenChanged={p.onTokenChanged} onFilterChanged={handleFilterChanged}
                lastToken={p.lastToken} onHeightChanged={handleHeightChanged} renderValue={p.renderValue}
                showPinnedFiltersOptions={showPinnedFiltersOptions}
                showDashboardBehaviour={showDashboardBehaviour}
                disableValue={false}
                level={0}
              /> :
              <FilterConditionComponent key={keyGenerator.getKey(f)} filter={f} readOnly={Boolean(p.readOnly)} onDeleteFilter={handlerDeleteFilter}
                allFilterOptions={p.filterOptions}
                prefixToken={undefined}
                subTokensOptions={p.subTokensOptions} queryDescription={p.queryDescription}
                onTokenChanged={p.onTokenChanged} onFilterChanged={handleFilterChanged} renderValue={p.renderValue}
                showPinnedFiltersOptions={showPinnedFiltersOptions}
                showDashboardBehaviour={showDashboardBehaviour}
                disableValue={false}
                level={0} />
            )}
            {!p.readOnly &&
              <tr className="sf-filter-create">
                <td colSpan={4}>
                  <a href="#" title={StyleContext.default.titleLabels ? SearchMessage.AddFilter.niceToString() : undefined}
                    className="sf-line-button sf-create sf-create-condition"
                    onClick={e => handlerNewFilter(e, false)}>
                    <FontAwesomeIcon icon="plus" className="sf-create me-1" />{SearchMessage.AddFilter.niceToString()}
                  </a>
                  <a href="#" title={StyleContext.default.titleLabels ? SearchMessage.AddOrGroup.niceToString() : undefined}
                    className="sf-line-button sf-create sf-create-group ms-3"
                    onClick={e => handlerNewFilter(e, true)}>
                    <FontAwesomeIcon icon="plus" className="sf-create me-1" />{SearchMessage.AddOrGroup.niceToString()}
                  </a>

                  {p.showPinnedFiltersOptionsButton && <a href="#" title={StyleContext.default.titleLabels ? SearchMessage.EditPinnedFilters.niceToString() : undefined}
                    className="sf-line-button ms-3"
                    onClick={e => { e.preventDefault(); setShowPinnedFiltersOptions(!showPinnedFiltersOptions); }}>
                    <FontAwesomeIcon color="orange" icon={[showPinnedFiltersOptions ? "fas" : "far", "pen-to-square"]} className="me-1" />{SearchMessage.EditPinnedFilters.niceToString()}
                  </a>
                  }
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>
    </fieldset>
  );
}


export interface RenderValueContext {
  filter: FilterConditionOptionParsed | FilterGroupOptionParsed;
  readonly: boolean;
  handleValueChange: () => void;
}

export interface FilterGroupComponentsProps {
  prefixToken: QueryToken | undefined;
  allFilterOptions: FilterOptionParsed[];
  filterGroup: FilterGroupOptionParsed;
  readOnly: boolean;
  onDeleteFilter: (fo: FilterGroupOptionParsed) => void;
  queryDescription: QueryDescription;
  subTokensOptions: SubTokensOptions;
  onTokenChanged?: (token: QueryToken | undefined) => void;
  onFilterChanged: () => void;
  onHeightChanged: () => void;
  lastToken: QueryToken | undefined;
  renderValue?: (rvc: RenderValueContext) => React.ReactElement<any> | undefined;
  showPinnedFiltersOptions: boolean;
  showDashboardBehaviour: boolean;
  disableValue: boolean;
  level: number;
}

export function FilterGroupComponent(p: FilterGroupComponentsProps) {

  const forceUpdate = useForceUpdate();
  const forceUpdatePromise = useForceUpdatePromise();

  function handleDeleteFilter(e: React.MouseEvent<any>) {
    e.preventDefault();
    p.onDeleteFilter(p.filterGroup);
  }

  function handleTokenChanged(newToken: QueryToken | null | undefined) {

    const f = p.filterGroup;
    f.token = newToken ?? undefined;
    if (p.onTokenChanged)
      p.onTokenChanged(newToken ?? undefined);
    p.onFilterChanged();
    forceUpdate();
  }

  function handleChangeOperation(e: React.FormEvent<HTMLSelectElement>) {
    const operation = (e.currentTarget as HTMLSelectElement).value as FilterGroupOperation;

    p.filterGroup.groupOperation = operation;

    p.onFilterChanged();

    forceUpdate();
  }

  function handlerDeleteFilter(filter: FilterOptionParsed) {
    p.filterGroup.filters.remove(filter);
    if (p.onFilterChanged)
      p.onFilterChanged();
    forceUpdatePromise().then(() => p.onHeightChanged());
  };



  function handlerNewFilter(e: React.MouseEvent<any>, isGroup: boolean) {

    e.preventDefault();

    let lastToken = p.lastToken;
    if (!lastToken || p.filterGroup.token && !isPrefix(p.filterGroup.token, lastToken))
      lastToken = p.filterGroup.token;

    p.filterGroup.filters.push(isGroup ?
      {
        groupOperation: p.filterGroup.groupOperation == "And" ? "Or" : "And",
        token: lastToken && hasAnyOrAll(lastToken) ? getTokenParents(lastToken).filter(a => a.queryTokenType == "AnyOrAll").lastOrNull() : p.prefixToken,
        filters: [],
        frozen: false,
        expanded: true,
      } as FilterGroupOptionParsed :
      {
        token: lastToken,
        operation: lastToken && getFilterOperations(lastToken).firstOrNull() || undefined,
        value: undefined,
        frozen: false
      } as FilterConditionOptionParsed);


    p.onFilterChanged();

    forceUpdatePromise().then(() => p.onHeightChanged());
  };

  function handleExpandCollapse(e: React.MouseEvent<any>) {
    e.preventDefault();
    const fg = p.filterGroup;
    fg.expanded = !fg.expanded;

    forceUpdatePromise().then(() => p.onHeightChanged());
  }

  const fg = p.filterGroup;

  var keyGenerator = React.useMemo(() => new KeyGenerator(), []);

  if (!p.showPinnedFiltersOptions && !isFilterActive(fg))
    return null;

  const readOnly = fg.frozen || p.readOnly;

  var paddingLeft = (25 * p.level);
  var paddingLeftNext = (25 * (p.level + 1)) + 5;
  return (
    <>
      <tr className="sf-filter-group" style={{ backgroundColor: "#eee" }}>
        <td style={{ paddingLeft: paddingLeft }} colSpan={2}>
          <div className="d-flex">
            {!readOnly &&
              <a href="#"
                className="sf-line-button sf-remove sf-remove-filter-icon"
                onClick={handleDeleteFilter}>
                <FontAwesomeIcon icon="xmark" title={StyleContext.default.titleLabels ? SearchMessage.DeleteFilter.niceToString() : undefined} />
              </a>}


            <div className="align-items-center d-flex">
              <a href="#"
                onClick={handleExpandCollapse}
                className={classes(fg.expanded ? "sf-hide-group-button" : "sf-show-group-button", "ms-2")} >
                <FontAwesomeIcon icon={fg.expanded ? ["far", "square-minus"] : ["far", "square-plus"]} className="me-2"
                  title={(fg.expanded ? EntityControlMessage.Collapse : EntityControlMessage.Expand).niceToString()} />
              </a>
              <strong className="me-2">{p.filterGroup.groupOperation == "Or" ? SearchMessage.OrGroup.niceToString() : SearchMessage.AndGroup.niceToString()} </strong>
              <small style={{ whiteSpace: "nowrap" }}>
                Prefix:
              </small>
              <div className={classes("rw-widget-xs mx-2", fg.token == null ? "hidden" : undefined)}>
                <QueryTokenBuilder
                  prefixQueryToken={p.prefixToken}
                  queryToken={fg.token}
                  onTokenChange={handleTokenChanged}
                  queryKey={p.queryDescription.queryKey}
                  subTokenOptions={p.subTokensOptions}
                  readOnly={readOnly} />
              </div>
            </div>
          </div>

        </td>
        <td>
          {fg.pinned &&
            <div>
              {renderValue()}
            </div>
          }
        </td>
        <td>
          {p.showPinnedFiltersOptions &&
            <button className={classes("btn", "btn-link", "btn-sm", "sf-user-filter", fg.pinned && "active")}
              title={fg.pinned ? SearchMessage.UnpinFilter.niceToString() : SearchMessage.PinFilter.niceToString()}
              onClick={e => {
                fg.pinned = fg.pinned ? undefined : {};
                fixDashboardBehaviour(fg);
                changeFilter();
              }} disabled={p.readOnly}>
              <FontAwesomeIcon color="orange" icon={"thumbtack"} rotation={fg.pinned ? undefined : 90} style={{ minWidth: 15 }} />
            </button>
          }
        </td>

        {p.showPinnedFiltersOptions && p.showDashboardBehaviour && <td>
          <DashboardBehaviourComponent filter={fg} readonly={readOnly} onChange={() => changeFilter()} />
        </td>}
        {p.showPinnedFiltersOptions && fg.pinned && <PinnedFilterEditor fo={fg} onChange={() => changeFilter()} readonly={readOnly} />}
      </tr >

      {
        fg.expanded ?
          <>
            {fg.filters.map((f) => isFilterGroup(f) ?

              <FilterGroupComponent key={keyGenerator.getKey(f)} filterGroup={f} readOnly={Boolean(p.readOnly)} onDeleteFilter={handlerDeleteFilter}
                allFilterOptions={p.allFilterOptions}
                prefixToken={fg.token}
                subTokensOptions={p.subTokensOptions} queryDescription={p.queryDescription}
                onTokenChanged={p.onTokenChanged} onFilterChanged={p.onFilterChanged}
                lastToken={p.lastToken} onHeightChanged={p.onHeightChanged} renderValue={p.renderValue}
                showPinnedFiltersOptions={p.showPinnedFiltersOptions}
                showDashboardBehaviour={p.showDashboardBehaviour}
                disableValue={p.disableValue || fg.pinned != null && !isCheckBox(fg.pinned.active)}
                level={p.level + 1}
              /> :

              <FilterConditionComponent key={keyGenerator.getKey(f)} filter={f} readOnly={Boolean(p.readOnly)} onDeleteFilter={handlerDeleteFilter}
                allFilterOptions={p.allFilterOptions}
                prefixToken={fg.token}
                subTokensOptions={p.subTokensOptions} queryDescription={p.queryDescription}
                onTokenChanged={p.onTokenChanged} onFilterChanged={p.onFilterChanged} renderValue={p.renderValue}
                showPinnedFiltersOptions={p.showPinnedFiltersOptions}
                showDashboardBehaviour={p.showDashboardBehaviour}
                disableValue={p.disableValue || fg.pinned != null && !isCheckBox(fg.pinned.active)}
                level={p.level + 1}
              />
            )}
            {!p.readOnly &&
              <tr className="sf-filter-create">
                <td colSpan={4} style={{ paddingLeft: paddingLeftNext }}>
                  <a href="#" title={StyleContext.default.titleLabels ? SearchMessage.AddFilter.niceToString() : undefined}
                    className="sf-line-button sf-create"
                    onClick={e => handlerNewFilter(e, false)}>
                    <FontAwesomeIcon icon="plus" className="sf-create" />&nbsp;{SearchMessage.AddFilter.niceToString()}
                  </a>

                  <a href="#" title={StyleContext.default.titleLabels ? (p.filterGroup.groupOperation == "And" ? SearchMessage.AddOrGroup.niceToString() : SearchMessage.AddAndGroup.niceToString()) : undefined}
                    className="sf-line-button sf-create ms-3"
                    onClick={e => handlerNewFilter(e, true)}>
                    <FontAwesomeIcon icon="plus" className="sf-create" />&nbsp;{p.filterGroup.groupOperation == "And" ? SearchMessage.AddOrGroup.niceToString() : SearchMessage.AddAndGroup.niceToString()}
                  </a>
                </td>
              </tr>
            }
          </> :
          <>
            <tr>
              <td colSpan={4} style={{ color: "#aaa", textAlign: "center", fontSize: "smaller", paddingLeft: paddingLeftNext }}>
                {SearchMessage._0FiltersCollapsed.niceToString(fg.filters.length)}
              </td>
            </tr>
          </>
      }
    </>
  );

  function renderValue() {

    if (p.renderValue)
      return p.renderValue({ filter: p.filterGroup, handleValueChange, readonly: p.readOnly });

    const f = p.filterGroup;

    const readOnly = p.readOnly || f.frozen;

    const ctx = new TypeContext<any>(undefined, { formGroupStyle: "None", readOnly: readOnly, formSize: "xs" }, undefined, Binding.create(f, a => a.value));

    return renderFilterValue(f, { ctx, filterOptions: p.allFilterOptions, handleValueChange: handleValueChange });
  }

  function handleValueChange() {
    forceUpdate();
    p.onFilterChanged();
  }

  function changeFilter() {
    forceUpdate();
    p.onFilterChanged();
  }
}


function isFilterActive(fo: FilterOptionParsed) {
  if (fo.pinned == null)
    return true;

  if (fo.pinned.splitValue && (fo.value == null || fo.value == ""))
    return false;

  return fo.pinned.active == null /*Always*/ ||
    fo.pinned.active == "Always" ||
    fo.pinned.active == "Checkbox_Checked" ||
    fo.pinned.active == "NotCheckbox_Unchecked" ||
    fo.pinned.active == "WhenHasValue" && !(fo.value == null || fo.value === "");
}

export interface FilterConditionComponentProps {
  filter: FilterConditionOptionParsed;
  allFilterOptions: FilterOptionParsed[];
  prefixToken: QueryToken | undefined;
  readOnly: boolean;
  onDeleteFilter: (fo: FilterConditionOptionParsed) => void;
  queryDescription: QueryDescription;
  subTokensOptions: SubTokensOptions;
  onTokenChanged?: (token: QueryToken | undefined) => void;
  onFilterChanged: () => void;
  renderValue?: (rvc: RenderValueContext) => React.ReactElement<any> | undefined;
  showPinnedFiltersOptions: boolean;
  showDashboardBehaviour: boolean;
  disableValue: boolean;
  level: number;
}

export function FilterConditionComponent(p: FilterConditionComponentProps) {

  const forceUpdate = useForceUpdate();

  function handleDeleteFilter(e: React.MouseEvent<any>) {
    e.preventDefault();
    p.onDeleteFilter(p.filter);
  }

  function handleTokenChanged(newToken: QueryToken | null | undefined) {

    const f = p.filter;

    if (newToken == undefined) {
      f.operation = undefined;
      f.value = undefined;
    }
    else {

      if (!areEqual(f.token, newToken, a => a.filterType) ||
        !areEqual(f.token, newToken, a => a.preferEquals) ||
        newToken.filterType == "Lite" && f.value != null && newToken.type.name != IsByAll && !getTypeInfos(newToken.type.name).some(t => t.name == (f.value as Lite<any>).EntityType)) {
        f.operation = newToken.preferEquals ? "EqualTo" : newToken.filterType && getFilterOperations(newToken).first();
        f.value = f.operation && isList(f.operation) ? [undefined] : undefined;
      }
      else if (f.token && f.token.filterType == "DateTime" && newToken.filterType == "DateTime") {
        if (f.value) {
          const type = newToken.type.name as "DateOnly" | "DateTime";

          function convertDateToNewFormat(val: string) {
            var date = DateTime.fromISO(val);
            if (!date.isValid)
              return val;

            const trimmed = trimDateToFormat(date, type, newToken!.format);
            return type == "DateOnly" ? trimmed.toISODate() : trimmed.toISO()!;
          }

          if (f.operation && isList(f.operation)) {
            f.value = (f.value as string[]).map(v => convertDateToNewFormat(v));
          } else {
            f.value = convertDateToNewFormat(f.value);
          }
        }
      }
    }

    f.token = newToken ?? undefined;

    if (p.filter.pinned?.splitValue && !canSplitValue(p.filter))
      p.filter.pinned.splitValue = undefined;

    if (p.onTokenChanged)
      p.onTokenChanged(newToken ?? undefined);

    p.onFilterChanged();

    forceUpdate();
  }

  function handleChangeOperation(event: React.FormEvent<HTMLSelectElement>) {
    const operation = (event.currentTarget as HTMLSelectElement).value as FilterOperation;
    if (isList(operation) != isList(p.filter.operation!))
      p.filter.value = isList(operation) && p.filter.token?.filterType == "Lite" ? [p.filter.value].notNull() :
        isList(operation) ? [p.filter.value] :
        p.filter.value[0];

    p.filter.operation = operation;
    if (p.filter.pinned?.splitValue && !canSplitValue(p.filter))
      p.filter.pinned.splitValue = undefined;

    p.onFilterChanged();

    forceUpdate();
  }

  const f = p.filter;

  const readOnly = f.frozen || p.readOnly;

  if (!p.showPinnedFiltersOptions && !isFilterActive(f))
    return null;

  return (
    <>
      <tr className="sf-filter-condition">
        <td style={{ paddingLeft: (25 * p.level) }}>
          <div className="d-flex">
            {!readOnly &&
              <a href="#" title={StyleContext.default.titleLabels ? SearchMessage.DeleteFilter.niceToString() : undefined}
                className="sf-line-button sf-remove sf-remove-filter-icon"
                onClick={handleDeleteFilter}>
                <FontAwesomeIcon icon="xmark" />
              </a>}
            <div className="rw-widget-xs">
              <QueryTokenBuilder
                prefixQueryToken={p.prefixToken}
                queryToken={f.token}
                onTokenChange={handleTokenChanged}
                queryKey={p.queryDescription.queryKey}
                subTokenOptions={p.subTokensOptions}
                readOnly={readOnly} />
            </div>
          </div>
        </td>
        <td className="sf-filter-operation">
          {f.token && f.token.filterType && f.operation &&
            <select className="form-select form-select-xs" value={f.operation} disabled={readOnly} onChange={handleChangeOperation}>
              {f.token.filterType && getFilterOperations(f.token)
                .map((ft, i) => <option key={i} value={ft as any} title={FilterOperation.niceToString(ft)}>{niceNameOrSymbol(ft)}</option>)}
            </select>}
        </td>

        <td className="sf-filter-value">
          {p.disableValue ? <small className="text-muted">{SearchMessage.ParentValue.niceToString()}</small> :
            f.token && f.token.filterType && f.operation && renderValue()}
        </td>
        {p.showPinnedFiltersOptions &&
          <td>
            {f.token && f.token.filterType && f.operation && !p.disableValue && <button className={classes("btn", "btn-link", "btn-sm", "sf-user-filter", f.pinned && "active")} onClick={e => {
              f.pinned = f.pinned ? undefined : {};
              fixDashboardBehaviour(f);
              changeFilter();
            }}
              disabled={p.readOnly}>
              <FontAwesomeIcon color="orange" icon={"thumbtack"} rotation={f.pinned ? undefined : 90} title={(f.pinned ? SearchMessage.PinFilter : SearchMessage.UnpinFilter).niceToString()} style={{ minWidth: 15 }} />
            </button>
            }
          </td>
        }
        {p.showDashboardBehaviour && <td>
          <DashboardBehaviourComponent filter={f} readonly={readOnly} onChange={() => changeFilter()} />
        </td>}

        {p.showPinnedFiltersOptions && f.pinned && <PinnedFilterEditor fo={f} onChange={() => changeFilter()} readonly={readOnly} />}

      </tr>
    </>
  );

  function changeFilter() {
    forceUpdate();
    p.onFilterChanged();
  }

  function renderValue() {

    if (p.renderValue)
      return p.renderValue({ filter: p.filter, handleValueChange, readonly: p.readOnly });

    const f = p.filter;

    const readOnly = p.readOnly || f.frozen;

    const ctx = new TypeContext<any>(undefined, { formGroupStyle: "None", readOnly: readOnly, formSize: "xs" }, undefined, Binding.create(f, a => a.value));

    return renderFilterValue(f, { ctx: ctx, filterOptions: p.allFilterOptions, handleValueChange });
  }

  function handleValueChange() {
    forceUpdate();
    p.onFilterChanged();
  }
}


interface PinnedFilterEditorProps {
  fo: FilterOptionParsed;
  readonly: boolean;
  onChange: () => void;
}


export function PinnedFilterEditor(p: PinnedFilterEditorProps) {

  var pinned = p.fo.pinned!;

  return (
    <>
      <td className="sf-pinned-filter-cell">
        <div>
          <input type="text" className="form-control form-control-xs" placeholder={SearchMessage.Label.niceToString()} readOnly={p.readonly}
            value={pinned.label ?? ""}
            onChange={e => { pinned.label = e.currentTarget.value; p.onChange(); }} />
        </div>
      </td>

      <td className="sf-pinned-filter-cell">
        {numericTextBox(Binding.create(pinned, _ => _.column), SearchMessage.Column.niceToString(), 0)}
      </td>

      <td className="sf-pinned-filter-cell">
        {numericTextBox(Binding.create(pinned, _ => _.colSpan), SearchMessage.ColSpan.niceToString(), 1)}
      </td>

      <td className="sf-pinned-filter-cell">
        {numericTextBox(Binding.create(pinned, _ => _.row), SearchMessage.Row.niceToString(), 0)}
      </td>

      <td className="sf-pinned-filter-cell">
        {renderActiveDropdown(Binding.create(pinned, a => a.active), "Select when the filter will take effect")}
      </td>
      <td className="sf-pinned-filter-cell">
        {canSplitValue(p.fo) &&
          <input type="checkbox" checked={pinned.splitValue ?? false}
            readOnly={p.readonly}
            className="form-check-input"
            onChange={e => { pinned.splitValue = e.currentTarget.checked; p.onChange() }}
            title={
              !canSplitValue(p.fo) ? undefined :
                isFilterCondition(p.fo) && isList(p.fo.operation!) ? SearchMessage.SplitsTheValuesAndSearchesEachOneIndependentlyInAnANDGroup.niceToString() :
                  SearchMessage.SplitsTheStringValueBySpaceAndSearchesEachPartIndependentlyInAnANDGroup.niceToString()
            } />
        }
      </td>
    </>
  );

  function numericTextBox(binding: Binding<number | undefined>, title: string, placeholder: number) {

    var val = binding.getValue();
    var numberFormat = toNumberFormat("0");

    return (
      <NumericTextBox readonly={p.readonly} value={val == undefined ? null : val}
        format={numberFormat}
        
        onChange={n => { binding.setValue(n == null ? undefined : n); p.onChange(); }}
        validateKey={isNumber} formControlClass="form-control form-control-xs" htmlAttributes={{ placeholder: placeholder.toString(), title: title, style: { width: "60px" } }} />
    );
  }

  //function renderButton(binding: Binding<boolean | undefined>, label: string, title: string) {
  //  return (
  //    <button type="button" className={classes("px-1 btn btn-light", binding.getValue() && "active")} disabled={p.readonly}
  //      onClick={e => { binding.setValue(binding.getValue() ? undefined : true); p.onChange(); }}
  //      title={StyleContext.default.titleLabels ? title : undefined}>
  //      {label}
  //    </button>
  //  );
  //}

  function renderActiveDropdown(binding: Binding<PinnedFilterActive | undefined>, title: string) {
    var value = binding.getValue() ?? "Always";
    return (
      <Dropdown>
        <Dropdown.Toggle variant="light" id="dropdown-basic" disabled={p.readonly} size={"xs" as any} className="px-1"
          title={StyleContext.default.titleLabels ? title : undefined}>
          {PinnedFilterActive.niceToString(value)}
        </Dropdown.Toggle>

        <Dropdown.Menu>
          {PinnedFilterActive.values().map(v =>
            <Dropdown.Item key={v} active={v == value} onClick={() => { binding.setValue(v == "Always" ? undefined : v); p.onChange(); }}>
              {PinnedFilterActive.niceToString(v)}
            </Dropdown.Item>)
          }
        </Dropdown.Menu>
      </Dropdown>
    );
  }
}

function DashboardBehaviourComponent(p: { filter: FilterOptionParsed, readonly: boolean, onChange: () => void }) {
  return (
    <Dropdown>
      <Dropdown.Toggle variant={p.filter.dashboardBehaviour ? "info" : "light"} id="dropdown-basic" disabled={p.readonly} size={"xs" as any} className={classes("px-1", p.filter.dashboardBehaviour ? "text-light" : "text-info")}
        title={StyleContext.default.titleLabels ? "Behaviour of the filter when used inside of a Dashboard" : undefined}>
        {<FontAwesomeIcon icon="gauge" className={classes("icon", p.filter.dashboardBehaviour ? "text-light" : "text-info")} />}{p.filter.dashboardBehaviour ? " " + DashboardBehaviour.niceToString(p.filter.dashboardBehaviour) : ""}
      </Dropdown.Toggle>

      <Dropdown.Menu>
        {[undefined, ...DashboardBehaviour.values()].map(v =>
          <Dropdown.Item key={v ?? "-"} active={v == p.filter.dashboardBehaviour} onClick={() => {

            p.filter.dashboardBehaviour = v;
            if (v == "PromoteToDasboardPinnedFilter" && p.filter.pinned == null)
              p.filter.pinned = {};
            else if ((v == "UseAsInitialSelection" || v == "UseWhenNoFilters") && p.filter.pinned != null)
              p.filter.pinned = undefined;

            p.onChange();
          }}>
            {v == null ? " - " : DashboardBehaviour.niceToString(v)}
          </Dropdown.Item>)
        }
      </Dropdown.Menu>
    </Dropdown>
  );
}





function fixDashboardBehaviour(fop: FilterOptionParsed) {
  if (fop.dashboardBehaviour == "PromoteToDasboardPinnedFilter" && fop.pinned == null)
    fop.dashboardBehaviour = undefined;

  if ((fop.dashboardBehaviour == "UseWhenNoFilters" || fop.dashboardBehaviour == "UseAsInitialSelection") && fop.pinned != null)
    fop.dashboardBehaviour = undefined;
}




function niceNameOrSymbol(fo: FilterOperation) {
  switch (fo) {
    case "EqualTo": return "=";
    case "DistinctTo": return "≠";
    case "GreaterThan": return ">";
    case "GreaterThanOrEqual": return "≥";
    case "LessThan": return "<";
    case "LessThanOrEqual": return "≤";
    default: return FilterOperation.niceToString(fo);
  }
}
