import React from "react";
import { OverlayTrigger, Popover } from "react-bootstrap";
import { getQueryNiceName, isTypeEntity } from "../Reflection";
import SearchControlLoaded, { getAddFilterIcon, getEditColumnIcon, getGroupByThisColumnIcon, getInsertColumnIcon, getRemoveColumnIcon, getResotreDefaultColumnsIcon } from "./SearchControlLoaded";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { Dic } from "../Globals";
import { CollectionMessage } from "../Signum.External";
import { getAllPinned } from "./PinnedFilterBuilder";
import { faFilter } from "@fortawesome/free-solid-svg-icons";
import { AggregateFunction, CollectionAnyAllType, CollectionElementType, ColumnFieldMessage, FieldExpressionMessage, FilterFieldMessage, QueryTokenDateMessage, QueryTokenMessage } from "../Signum.DynamicQuery.Tokens";
import { JavascriptMessage, SearchMessage } from "../Signum.Entities";
import { OverlayInjectedProps } from "react-bootstrap/esm/Overlay";
import { FilterOperation, QueryDescription } from "../FindOptions";
import { getNiceTypeName } from "../Operations/MultiPropertySetter";
import SearchControl from "./SearchControl";

export function SearchHelp(p: { sc: SearchControlLoaded, injected: OverlayInjectedProps }): React.JSX.Element {
  var sc = p.sc;
  var query = getQueryNiceName(sc.props.queryDescription.queryKey);
  var type = sc.props.queryDescription.columns['Entity'].niceTypeName;
  var searchMode = getSearchMode(sc);
  const fo = sc.props.findOptions;

  const tokensObj = fo.columnOptions.map(a => a.token)
    .concat(fo.orderOptions.map(a => a.token))
    .filter(a => a != undefined && a.queryTokenType != "Aggregate")
    .toObjectDistinct(a => a!.fullKey, a => a!);

  const tokens = Dic.getValues(tokensObj);

  return (
    <Popover id="popover-basic" {...p.injected} style={{ ...p.injected.style, minWidth: 900 }}>
      <Popover.Header as="h3"><strong>{SearchMessage.SearchHelp.niceToString()}</strong></Popover.Header>
      <Popover.Body>
        <div className="my-2">{SearchMessage.The0IsVeryPowerfulButCanBeIntimidatingTakeSomeTimeToLearnHowToUseItWillBeWorthIt.niceToString().formatHtml(<strong>{SearchMessage.SearchControl.niceToString()}</strong>)}</div>
        <div className="pt-2"><strong>{SearchMessage.TheBasics.niceToString()}</strong></div>
        {searchMode == "Search" && <p className="my-2">{SearchMessage.CurrentlyWeAreInTheQuery0YouCanOpenA1ByClickingThe2IconOrDoing3InTheRowButNotInALink.niceToString().formatHtml(<strong><samp>{query}</samp></strong>, <strong><samp>{type}</samp></strong>, <strong><samp style={{ whiteSpace: 'nowrap' }}>{SearchMessage.DoubleClick.niceToString()}</samp></strong>)}</p>}
        {searchMode == "Group" && <p className="my-2">{SearchMessage.CurrentlyWeAreInTheQuery0GroupedBy1YouCanOpenAGroupByClickingThe2IconOrDoing3InTheRowButNotInALink.niceToString().formatHtml(<strong><samp>{query}</samp></strong>, tokens.map(a => <strong><samp>{a.niceName}</samp></strong>).joinCommaHtml(CollectionMessage.And.niceToString()), <FontAwesomeIcon icon="layer-group" color="#b1bac4" />, <strong><samp style={{ whiteSpace: 'nowrap' }}>{SearchMessage.DoubleClick.niceToString()}</samp></strong>)}</p>}
        {searchMode == "Find" && <p className="my-2">{SearchMessage.Doing0InTheRowWillSelectTheEntityAndCloseTheModalAutomaticallyAlternativelyYouCanSelectOneEntityAndClickOK.niceToString().formatHtml(<strong><samp style={{ whiteSpace: 'nowrap' }}>{SearchMessage.DoubleClick.niceToString()}</samp></strong>)}</p>}
        {getAllPinned(fo.filterOptions).length > 0 && <div className="my-2">{SearchMessage.YouCanUseThePreparedFiltersOnTheTopToQuicklyFindThe0YouAreLookingFor.niceToString().formatHtml(<strong><samp>{type}</samp></strong>)}</div>}
        <div className="pt-2"><strong>{SearchMessage.OrderingResults.niceToString()}</strong></div>
        <p className="my-2">{SearchMessage.YouCanOrderResultsByClickingInAColumnHeaderDefaultOrderingIs0AndByClickingAgainItChangesTo1YouCanOrderByMoreThanOneColumnIfYouKeep2DownWhenClickingOnTheColumnsHeader.niceToString().formatHtml(<span><samp>Ascending</samp> <FontAwesomeIcon icon="sort-up" /></span>, <span><samp>Descending</samp> <FontAwesomeIcon icon="sort-down" /></span>, <kbd>Shift</kbd> )}</p>
        <div className="pt-2"><strong>{SearchMessage.ChangeColumns.niceToString()}</strong></div>
        <p className="my-2">{SearchMessage.YouAreNotLimitedToTheColumnsYouSeeTheDefaultColumnsCanBeChangedBy0InAColumnHeaderAndThenSelect123.niceToString().formatHtml(<strong><samp>{SearchMessage.RightClicking.niceToString()}</samp></strong>, <span>{getInsertColumnIcon()}<em>{SearchMessage.InsertColumn.niceToString()}</em></span>, <span>{getEditColumnIcon()}<em>{SearchMessage.EditColumn.niceToString()}</em></span>, <span>{getRemoveColumnIcon()}<em>{SearchMessage.RemoveColumn.niceToString()}</em></span>)}</p>
        <p className="my-2">{SearchMessage.YouCanAlso0TheColumnsByDraggingAndDroppingThemToAnotherPosition.niceToString().formatHtml(<em>{SearchMessage.Rearrange.niceToString()}</em>)}</p>
        <p className="my-2">{SearchMessage.WhenInsertingTheNewColumnWillBeAddedBeforeOrAfterTheSelectedColumnDependingWhereYou0.niceToString().formatHtml(<strong><samp style={{ whiteSpace: 'nowrap' }}>{SearchMessage.RightClick.niceToString()}</samp></strong>)}</p>
        <div className="pt-2"><strong>{SearchMessage.AdvancedFilters.niceToString()}</strong></div>
        <p className="my-2">{SearchMessage.ClickOnThe0ButtonToOpenTheAdvancedFiltersThisWillAllowYouCreateComplexFiltersManuallyBySelectingThe1OfTheEntityOrARelatedEntitiesAComparison2AndA3ToCompare.niceToString().formatHtml(<FontAwesomeIcon icon="filter" />, <strong>field</strong>, <strong>operator</strong>, <strong>value</strong>)}</p>
        <p className="my-2">{SearchMessage.TrickYouCan0OnA1AndChoose2ToQuicklyFilterByThisColumnEvenMoreYouCan3ToFilterByThis4Directly.niceToString().formatHtml(<strong><samp style={{ whiteSpace: 'nowrap' }}>{SearchMessage.RightClick.niceToString()}</samp></strong>, <strong>{SearchMessage.ColumnHeader.niceToString()}</strong>, <span>{getAddFilterIcon()}<em>{SearchMessage.AddFilter.niceToString()}</em></span>, <strong><samp style={{ whiteSpace: 'nowrap' }}>{SearchMessage.RightClicking.niceToString()}</samp></strong>, <strong>{SearchMessage.Value.niceToString()}</strong>)}</p>
        <div className="pt-2"><strong>{SearchMessage.GroupingResultsByOneOrMoreColumn.niceToString()}</strong></div>
        <p className="my-2">{SearchMessage.YouCanGroupResultsBy0InAColumnHeaderAndSelecting1AllTheColumnsWillDisappearExceptTheSelectedOneAndAnAggregationColumnTypically2.niceToString().formatHtml(<strong><samp style={{ whiteSpace: 'nowrap' }}>{SearchMessage.RightClicking.niceToString()}</samp></strong>, <span>{getGroupByThisColumnIcon()}<em style={{ whiteSpace: 'nowrap' }}>{SearchMessage.GroupByThisColumn.niceToString()}</em></span>, <em>{QueryTokenMessage.Count.niceToString()}</em>)}</p>
      </Popover.Body>
    </Popover>
  );
}

export function GroupHelp(p: { injected: OverlayInjectedProps }): React.JSX.Element {
  return (
    <Popover id="popover-basic" {...p.injected} style={{ ...p.injected.style, minWidth: 900 }}>
      <Popover.Header as="h3"><strong>{SearchMessage.GroupHelp.niceToString()}</strong></Popover.Header>
      <Popover.Body>
        <p className="my-2">{SearchMessage.AnyNewColumnShouldEitherBeAnAggregate0OrItWillBeConsideredANewGroupKey1.niceToString().formatHtml(<span>(<samp>{AggregateFunction.niceToString("Count")}</samp>, <samp>{AggregateFunction.niceToString("Sum")}</samp>, <samp>{AggregateFunction.niceToString("Min")}</samp>...)</span>, <FontAwesomeIcon icon="key" color="gray" />)}</p>
        <p className="my-2">{SearchMessage.OnceGroupingYouCanFilterNormallyOrUsingAggregatesAsTheField0.niceToString().formatHtml(<span><code>HAVING</code> {SearchMessage.InSql.niceToString()}</span>)}</p>
        <p className="my-2">{SearchMessage.FinallyYouCanStopGroupingBy0InAColumnHeaderAndSelect1.niceToString().formatHtml(<strong><samp style={{ whiteSpace: 'nowrap' }}>{SearchMessage.RightClicking.niceToString()}</samp></strong>, <span>{getResotreDefaultColumnsIcon()}<em style={{ whiteSpace: 'nowrap' }}>{SearchMessage.RestoreDefaultColumns.niceToString()}</em></span>)}</p>
      </Popover.Body>
    </Popover>
  );
}

type SearchMode = "Search" | "Group" | "Find";

function getSearchMode(sc: SearchControlLoaded): SearchMode {
  if (sc.props.findOptions.groupResults)
    return "Group";
  if (sc.props.onDoubleClick)
    return "Find";
  return "Search";
}

export function FilterHelp(p: { queryDescription: QueryDescription, injected: OverlayInjectedProps }): React.JSX.Element {
  const [expressionExpanded, setExpressionExpanded] = React.useState(false);
  var type = p.queryDescription.columns['Entity'].niceTypeName;
  const isDefaultQuery = isTypeEntity(p.queryDescription.queryKey);
  var sampleColumns = Object.values(p.queryDescription.columns).filter(cd => cd.key != "Id" && cd.key != "Entity");
  return (
    <Popover id="popover-basic" {...p.injected} style={{ ...p.injected.style, minWidth: expressionExpanded ? 900 : 600 }}>
      <Popover.Header as="h3"><strong>{FilterFieldMessage.FiltersHelp.niceToString()}</strong></Popover.Header>
      <Popover.Body>
        <div className="my-2">{FilterFieldMessage.AFilterConsistsOfA0AComparison1AndAConstant2.niceToString()
          .formatHtml(<strong>{FilterFieldMessage.Field.niceToString()}</strong>, <strong>{FilterFieldMessage.Operator.niceToString()}</strong>, <strong>{FilterFieldMessage.Value.niceToString()}</strong>)}</div>
        <ul>
          <li>
            {isDefaultQuery && <div className="my-2"><strong>{FilterFieldMessage.Field.niceToString()}: </strong>{SearchMessage.AQueryExpressionCouldBeAnyColumnOfThe.niceToString()} <strong><samp>{type}</samp></strong> ({SearchMessage.Like.niceToString()} {sampleColumns.filter((c, i) => i < 3).map((c, i) => <strong><samp>{c.niceName}</samp></strong>).notNull().joinHtml(', ')} {SearchMessage.OrAnyOtherFieldThatYouSeeInThe.niceToString()} <strong><samp>{type}</samp></strong> {SearchMessage.WhenYouClick.niceToString()} <FontAwesomeIcon icon="arrow-right" color="#b1bac4" /> {SearchMessage.IconOrAnyRelatedEntity.niceToString()}</div>}
            {!isDefaultQuery && <div className="my-2"><strong>{FilterFieldMessage.Field.niceToString()}: </strong>{SearchMessage.AQueryExpressionCouldBeAnyColumnOfThe.niceToString()} <strong><samp>{getQueryNiceName(p.queryDescription.queryKey)}</samp></strong> (like {Object.values(p.queryDescription.columns).map((c, i) => i < 3 && <strong><samp>{c.niceName}</samp></strong>).joinCommaHtml(',')}, {SearchMessage.OrAnyOtherFieldThatYouSeeInTheProjectWhenYouClick.niceToString()} <FontAwesomeIcon icon="arrow-right" color="#b1bac4" /> {SearchMessage.IconOrAnyRelatedEntity.niceToString()}</div>}
            <LearnMoreAboutFieldExpressions expanded={expressionExpanded} onSetExpanded={setExpressionExpanded} showAny={true} />
          </li>
          <li><div className="my-2"><strong>{FilterFieldMessage.Operator.niceToString()}: </strong>{SearchMessage.TheOperationThatWillBeUsedToCompareThe.niceToString()} <strong><samp>field</samp></strong><samp></samp> {SearchMessage.WithThe.niceToString()} <strong><samp>value</samp></strong>, {SearchMessage.Like.niceToString()} <samp>{SearchMessage.EqualsDistinctGreaterThan.niceToString()}</samp>, {SearchMessage.Etc.niceToString()}</div></li>
          <li><div className="my-2"><strong>{FilterFieldMessage.Value.niceToString()}: </strong>{SearchMessage.TheValueThatWillBeComparedWithThe.niceToString()} <strong><samp>field</samp></strong>, {SearchMessage.TypicallyHasTheSameTypeAsTheFieldButSomeOperatorsLike.niceToString()} <strong><samp>{FilterOperation.niceToString("IsIn")}</samp></strong> {QueryTokenMessage.And.niceToString()} <strong><samp>{FilterOperation.niceToString("IsNotIn")}</samp></strong> {SearchMessage.AllowToSelectMultipleValues.niceToString()}</div></li>
        </ul>
        {/*{isDefaultQuery && <div className="my-2">{FilterFieldMessage.FieldCanBeAnyFieldOfThe0OrAnyRelatedEntity.niceToString().formatHtml(<strong>{type}</strong>)}</div>}*/}
        {/*{!isDefaultQuery && <div className="my-2">{FilterFieldMessage.FieldCanBeAnyColumnOfTheQuery0OrAnyFieldOf1.niceToString().formatHtml(<strong>{getQueryNiceName(queryKey)}</strong>, <strong>{type}</strong>)}</div>}*/}
        <strong>{FilterFieldMessage.AndOrGroups.niceToString()}</strong>
        <div className="my-2">{FilterFieldMessage.Using0YouCanGroupAFewFiltersTogether.niceToString()
          .formatHtml(<strong><samp>[+ {SearchMessage.AddOrGroup.niceToString()}]</samp></strong>, <strong><samp>{SearchMessage.OrGroup.niceToString()}</samp></strong>, <strong><samp>{SearchMessage.AndGroup.niceToString()}</samp></strong>)}</div>
        <div className="my-2">{FilterFieldMessage.FilterGroupsCanAlsoBeUsedToCombineFiltersForTheSameElement012.niceToString()
          .formatHtml(<strong>{FilterFieldMessage.TheSameElement.niceToString()}</strong>, <strong><samp>{CollectionAnyAllType.niceToString("Any")}</samp></strong>, <strong><samp>{CollectionAnyAllType.niceToString("All")}</samp></strong>)}</div>
      </Popover.Body>
    </Popover>);
}

export function ColumnHelp(p: { queryDescription: QueryDescription, injected: OverlayInjectedProps }): React.JSX.Element {
  const [expressionExpanded, setExpressionExpanded] = React.useState(false);
  const type = getNiceTypeName(p.queryDescription.columns['Entity'].type);
  const isDefaultQuery = isTypeEntity(p.queryDescription.queryKey);
  return (
    <Popover id="popover-basic" {...p.injected} style={{ ...p.injected.style, minWidth: 800 }}>
      <Popover.Header as="h3"><strong>{ColumnFieldMessage.ColumnsHelp.niceToString()}</strong></Popover.Header>
      <Popover.Body>
        <div className="my-2">{SearchMessage.YouAreEditingAColumnLetMeExplainWhatEachFieldDoes.niceToString()}</div>
        <ul>
          <li>
            <strong>{SearchMessage.ColumnField.niceToString()}: </strong>          
            {!isDefaultQuery && ColumnFieldMessage.YouCanSelectAFieldExpressionToPointToAnyColumnOfTheQuery0OrAnyFieldOf1OrAnyRelatedEntity.niceToString().formatHtml(<strong>{getQueryNiceName(p.queryDescription.queryKey)}</strong>, <strong>{type}</strong>)}
            {isDefaultQuery && ColumnFieldMessage.YouCanSelectAFieldExpressionToPointToAnyFieldOfThe0OrAnyRelatedEntity.niceToString().formatHtml(<strong>{type}</strong>)}
            <LearnMoreAboutFieldExpressions expanded={expressionExpanded} onSetExpanded={setExpressionExpanded} showAny={false} />
          </li>
          <li><strong>{SearchMessage.DisplayName.niceToString()}: </strong>{ColumnFieldMessage.TheColumnHeaderTextIsTypicallyAutomaticallySetDependingOnTheFieldExpression.niceToString()
            .formatHtml(<em>{SearchMessage.DisplayName.niceToString()}</em>)}</li>
          <li><strong>{SearchMessage.SummaryHeader.niceToString()} (Ʃ): </strong>
            {ColumnFieldMessage.YouCanAddOneNumericValueToTheColumnHeaderLikeTheTotalSumOfTheInvoices.niceToString().formatHtml(<span><em>{AggregateFunction.niceToString("Count")}</em>, <em>{AggregateFunction.niceToString("Sum")}</em></span>)}
            <br/>
            {ColumnFieldMessage.NoteTheAggregationIncludesRowsThatMayNotBeVisibleDueToPagination.niceToString()}
          </li>
          <li><strong>{SearchMessage.CombineRowsWith.niceToString()}: </strong>{ColumnFieldMessage.WhenATableHasManyRepeatedValuesInAColumnYouCanCombineThemVertically01.niceToString()
            .formatHtml(<code>rowSpan</code>, <strong><samp>{type}</samp></strong>)}</li>
        </ul>
      </Popover.Body>
    </Popover>
  );
}



export function LearnMoreAboutFieldExpressions(p: { expanded: boolean, onSetExpanded: (a: any) => void, showAny: boolean }): React.JSX.Element {
  return (
    <div className="mb-2">
      <a href="#" onClick={e => {
        e.preventDefault();
        p.onSetExpanded(!p.expanded);
      }}><FontAwesomeIcon icon={p.expanded ? "chevron-up" : "chevron-down"} /> {FieldExpressionMessage.LearnMoreAboutFieldExpressions.niceToString()}</a>
      {p.expanded == true && <div className="ms-4">
        <div className="my-2">{FieldExpressionMessage.YouCanNavigateDatabaseRelationshipsByContinuingTheExpressionWithMoreItems.niceToString()}</div>
        <ul>
          <li><strong>{FieldExpressionMessage.SimpleValues.niceToString()}: </strong>{FieldExpressionMessage.AStringLikeHelloANumberLike.niceToString().formatHtml(<em>{QueryTokenMessage.Length.niceToString()}</em>, <em>{QueryTokenMessage.Modulo0.niceToString("")}</em>, <em>{QueryTokenMessage.Step0.niceToString("")}</em>)}</li>
          <li><strong style={{ color: '#5100a1' }}>{FieldExpressionMessage.Dates.niceToString()}: </strong>{FieldExpressionMessage._0And1YouCanExtractsPartsOfTheDateByContinuingTheExpressionWith2ReturnANumberOr3ReturnADate.niceToString()
            .formatHtml(<em>{QueryTokenMessage.Date.niceToString()}</em>, <em>{QueryTokenMessage.DateTime.niceToString()}</em>, <span><em>{QueryTokenDateMessage.Month.niceToString()}</em>, <em>{QueryTokenDateMessage.WeekNumber.niceToString()}</em>, <em>{QueryTokenDateMessage.Day.niceToString()}</em></span>,
              <span><em>{QueryTokenDateMessage.MonthStart.niceToString()}</em>, <em>{QueryTokenDateMessage.WeekStart.niceToString()}</em>, <em>{QueryTokenDateMessage.Date.niceToString()}</em></span>)}
          </li>
          <li><strong style={{ color: '#2b91af' }}>{FieldExpressionMessage.EntityRelationships.niceToString()}: </strong> {FieldExpressionMessage.EntityRelationshipsAllowYouToNavigateToOtherTablesToGetFields.niceToString()} (<code>LEFT JOIN</code> {FieldExpressionMessage.InSql.niceToString()})</li>
          <li><strong style={{ color: '#ce6700' }}>{FieldExpressionMessage.Collections.niceToString()}: </strong> {FieldExpressionMessage.CollectionOfEntitiesOrRelationships.niceToString()}</li>
          <li><strong style={{ color: 'blue' }}>{FieldExpressionMessage.CollectionOperators.niceToString()}:</strong>
            <ul>
              <li><strong>{CollectionElementType.niceToString("Element")}: </strong> {FieldExpressionMessage.MultipliesTheNumberOfRowsByAllTheElementsInTheCollection012.niceToString()
                .formatHtml(<code>OUTER APPLY</code>, <code>LEFT JOIN LATERAL</code>, FieldExpressionMessage.InSql.niceToString(), <em>{CollectionElementType.niceToString("Element")}</em>, <em>{CollectionElementType.niceToString("Element2")}</em>, <em>{CollectionElementType.niceToString("Element3")}</em>)}
              </li>
              {p.showAny && <li><strong>{CollectionAnyAllType.niceToString("Any")} / {CollectionAnyAllType.niceToString("NotAny")} / {CollectionAnyAllType.niceToString("All")} / {CollectionAnyAllType.niceToString("NotAll")}:</strong> {FieldExpressionMessage.AllowsToAddFiltersThatUseConditionsOnTheCollectionElemens.niceToString()
                .formatHtml(<code>EXISTS</code>, FieldExpressionMessage.InSql.niceToString(), <code>AND</code>, <code>OR</code>)}
              </li>}
            </ul>
          </li>
          <li><strong style={{ color: 'green' }}>{FieldExpressionMessage.Aggregates.niceToString()}:</strong> {FieldExpressionMessage.WhenGroupingAllowsToCollapseManyValuesInOneValue.niceToString()}
            <ul>
              <li><strong>{AggregateFunction.niceToString("Count")}:</strong> {SearchMessage.CanBeUsedAsTheFirstItemCountsTheNumberOfRowsOnEachGroup.niceToString()}</li>
              <li><strong>{AggregateFunction.niceToString("Min")}, {AggregateFunction.niceToString("Max")}, {AggregateFunction.niceToString("Average")}, {FieldExpressionMessage.CountNotNull.niceToString()}, {FieldExpressionMessage.CountDistinct.niceToString()} ..:</strong> {FieldExpressionMessage.CanOnlyBeUsedAfterAnotherField.niceToString()}</li>
            </ul>
          </li>
        </ul>
        <div>{FieldExpressionMessage.FinallyRememberThatYouCan01FullFieldExpression.niceToString().formatHtml(<code>COPY</code>, <code>PASTE</code>, <kbd>Ctrl+C</kbd>, <kbd>Ctrl+V</kbd>)}</div>
      </div>}
    </div>
  );
}
